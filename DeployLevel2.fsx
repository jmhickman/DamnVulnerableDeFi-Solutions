#i """nuget: C:\Users\jon_h\source\repos\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.3" 

open web3.fs

// Values and partial applications
let rinEnv = createWeb3Environment "http://127.0.0.1:1248" "2.0" "0x2268b96e204379ee8366505c344ebe5cc34d3a46"

let ganEnv = createWeb3Environment "http://127.0.0.1:1248" "2.0" "0xffcf8fdee72ac11b5c542428b35eef5769c409f0"

let callRin = makeEthCall rinEnv
let txnRin = makeEthTxn rinEnv

let callGan = makeEthCall ganEnv
let txnGan = makeEthTxn ganEnv

let logIt = ganEnv.log Log
let bindIt (a: Result<CallResponses, Web3Error>) = bindDeployedContract >> List.head

// deploy contracts to local ganache

let naiveLenderABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\NaiveReceiverLenderPool.abi"""
let naiveReceiverABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\FlashLoanReceiver.abi"""

let deployLender () = 
    let naiveLenderBytecode = returnBytecodeFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\NaiveReceiverLenderPool.json"""
    prepareUndeployedContract ganEnv naiveLenderBytecode None GANACHE naiveLenderABI |> Result.bind (deployEthContract ganEnv ZEROV) |> logIt

let deployReceiver () = 
    let naiveReceiverBytecode = returnBytecodeFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\FlashLoanReceiver.json"""
    prepareUndeployedContract ganEnv naiveReceiverBytecode (Some [Address "0xf282cf23b03f001dc05c35e927c99dd01c463450"]) GANACHE naiveReceiverABI |> Result.bind (deployEthContract ganEnv ZEROV) |> logIt
