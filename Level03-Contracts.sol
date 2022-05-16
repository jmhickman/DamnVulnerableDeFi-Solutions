// SPDX-License-Identifier: MIT

pragma solidity ^0.8.0;

import "../../OZ/token/ERC20/IERC20.sol";
import "../../OZ/utils/Address.sol";
import "../../OZ/security/ReentrancyGuard.sol";

/**
 * @title TrusterLenderPool
 * @author Damn Vulnerable DeFi (https://damnvulnerabledefi.xyz)
 */
contract TrusterLenderPool is ReentrancyGuard {

    using Address for address;

    IERC20 public immutable damnValuableToken;

    constructor (address tokenAddress) {
        damnValuableToken = IERC20(tokenAddress);
    }

    function flashLoan(
        uint256 borrowAmount,
        address borrower,
        address target,
        bytes calldata data
    )
        external
        nonReentrant
    {
        uint256 balanceBefore = damnValuableToken.balanceOf(address(this));
        require(balanceBefore >= borrowAmount, "Not enough tokens in pool");
        
        damnValuableToken.transfer(borrower, borrowAmount);
        target.functionCall(data);

        uint256 balanceAfter = damnValuableToken.balanceOf(address(this));
        require(balanceAfter >= balanceBefore, "Flash loan hasn't been paid back");
    }

}

/*
This was susceptable to (what I would have called in 'trad infosec') an "impersonation attack." Setting the `target` value 
to this contract is enough to perform any action as it (since it's a call). So the actual flash loan itself is immaterial
to the exploit. All that is done is use the impersonation to approve the attacking contract as a spender of the token.
Then the attacker just transfers out the tokens after the loan resolves.

*/
