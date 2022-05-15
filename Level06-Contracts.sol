// SPDX-License-Identifier: MIT

pragma solidity ^0.8.0;

import "../../OZ/token/ERC20/extensions/ERC20Snapshot.sol";
import "../../OZ/utils/Address.sol";
import "../../OZ/security/ReentrancyGuard.sol";
import "./DamnValuableTokenSnapshot.sol";


/**
 * @title SimpleGovernance
 * @author Damn Vulnerable DeFi (https://damnvulnerabledefi.xyz)
 */
contract SimpleGovernance {

    using Address for address;
    
    struct GovernanceAction {
        address receiver;
        bytes data;
        uint256 weiAmount;
        uint256 proposedAt;
        uint256 executedAt;
    }
    
    DamnValuableTokenSnapshot public governanceToken;

    mapping(uint256 => GovernanceAction) public actions;
    uint256 private actionCounter;
    uint256 private ACTION_DELAY_IN_SECONDS = 2 days;

    event ActionQueued(uint256 actionId, address indexed caller);
    event ActionExecuted(uint256 actionId, address indexed caller);

    constructor(address governanceTokenAddress) {
        require(governanceTokenAddress != address(0), "Governance token cannot be zero address");
        governanceToken = DamnValuableTokenSnapshot(governanceTokenAddress);
        actionCounter = 1;
    }
    
    function queueAction(address receiver, bytes calldata data, uint256 weiAmount) external returns (uint256) {
        require(_hasEnoughVotes(msg.sender), "Not enough votes to propose an action");
        require(receiver != address(this), "Cannot queue actions that affect Governance");

        uint256 actionId = actionCounter;

        GovernanceAction storage actionToQueue = actions[actionId];
        actionToQueue.receiver = receiver;
        actionToQueue.weiAmount = weiAmount;
        actionToQueue.data = data;
        actionToQueue.proposedAt = block.timestamp;

        actionCounter++;

        emit ActionQueued(actionId, msg.sender);
        return actionId;
    }

    function executeAction(uint256 actionId) external payable {
        require(_canBeExecuted(actionId), "Cannot execute this action");
        
        GovernanceAction storage actionToExecute = actions[actionId];
        actionToExecute.executedAt = block.timestamp;

        actionToExecute.receiver.functionCallWithValue(
            actionToExecute.data,
            actionToExecute.weiAmount
        );

        emit ActionExecuted(actionId, msg.sender);
    }

    function getActionDelay() public view returns (uint256) {
        return ACTION_DELAY_IN_SECONDS;
    }

    /**
     * @dev an action can only be executed if:
     * 1) it's never been executed before and
     * 2) enough time has passed since it was first proposed
     */
    function _canBeExecuted(uint256 actionId) private view returns (bool) {
        GovernanceAction memory actionToExecute = actions[actionId];
        return (
            actionToExecute.executedAt == 0 &&
            (block.timestamp - actionToExecute.proposedAt >= ACTION_DELAY_IN_SECONDS)
        );
    }
    
    function _hasEnoughVotes(address account) private view returns (bool) {
        uint256 balance = governanceToken.getBalanceAtLastSnapshot(account);
        uint256 halfTotalSupply = governanceToken.getTotalSupplyAtLastSnapshot() / 2;
        return balance > halfTotalSupply;
    }
}

///////////////////////////////////////////////////////////////////////////////////////////////////

/**
 * @title SelfiePool
 * @author Damn Vulnerable DeFi (https://damnvulnerabledefi.xyz)
 */
contract SelfiePool is ReentrancyGuard {

    using Address for address;

    ERC20Snapshot public token;
    SimpleGovernance public governance;

    event FundsDrained(address indexed receiver, uint256 amount);

    modifier onlyGovernance() {
        require(msg.sender == address(governance), "Only governance can execute this action");
        _;
    }

    constructor(address tokenAddress, address governanceAddress) {
        token = ERC20Snapshot(tokenAddress);
        governance = SimpleGovernance(governanceAddress);
    }

    function flashLoan(uint256 borrowAmount) external nonReentrant {
        uint256 balanceBefore = token.balanceOf(address(this));
        require(balanceBefore >= borrowAmount, "Not enough tokens in pool");
        
        token.transfer(msg.sender, borrowAmount);        
        
        require(msg.sender.isContract(), "Sender must be a deployed contract");
        msg.sender.functionCall(
            abi.encodeWithSignature(
                "receiveTokens(address,uint256)",
                address(token),
                borrowAmount
            )
        );
        
        uint256 balanceAfter = token.balanceOf(address(this));

        require(balanceAfter >= balanceBefore, "Flash loan hasn't been paid back");
    }

    function drainAllFunds(address receiver) external onlyGovernance {
        uint256 amount = token.balanceOf(address(this));
        token.transfer(receiver, amount);
        
        emit FundsDrained(receiver, amount);
    }
}
