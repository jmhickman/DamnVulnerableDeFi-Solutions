# DamnVulnerableDeFi-Solutions

This is a collection of source files representing work on [Damn Vulnerable DeFi](https://damnvulnerabledefi.xyz). Most of the challenges were completed using Web3.fs as the RPC library, because I didn't know Hardhat when I started on them.

Deploy* files are used to set up the scenario for a given level.
Level*-Attack.sol represent Solidity contracts used by the attacker, if required.
Level*-script.fsx represent the "code your exploit" section from the hardhat test files, implemented in Web3.fs.
Level*-Contracts.sol are usually just copies of the target contract(s) from a given level. They may contain modifications from testing/understanding the exploit for my own notes, but don't represent the contract as-deployed during exploitation.
Level*-script.js is a level completed with the Hardhat testing rig. It will contain only the modifications to the test script, not the whole script. Typically it's imports for putting at the top of the file, and then the section that represents the exploitation.
