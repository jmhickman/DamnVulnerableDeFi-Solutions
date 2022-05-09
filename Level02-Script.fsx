#i """nuget: C:\Users\jon_h\source\repos\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.3" 
#load "./DeployLevel2.fsx"

open web3.fs
open DeployLevel2

// Values and partial applications
let rinEnv = createWeb3Environment "http://127.0.0.1:1248" "2.0" "0x2268b96e204379ee8366505c344ebe5cc34d3a46"

let ganEnv = createWeb3Environment "http://127.0.0.1:1248" "2.0" "0xffcf8fdee72ac11b5c542428b35eef5769c409f0"

let callRin = makeEthCall rinEnv
let txnRin = makeEthTxn rinEnv

let callGan = makeEthCall ganEnv
let txnGan = makeEthTxn ganEnv

let logIt = ganEnv.log Log


// deploy contracts to local ganache

let lender = loadDeployedContract ganEnv "0xf282cf23b03f001dc05c35e927c99dd01c463450" GANACHE naiveLenderABI |> bindDeployedContract |> List.head
let naiveReceiver = loadDeployedContract ganEnv "0x7b6b01d2a669d602c87d5b453c2ed9115daddbb7" GANACHE naiveReceiverABI |> bindDeployedContract |> List.head

//txnGan lender Receive [] "49000000000000000050" |> logIt
//txnGan naiveReceiver Receive [] "9000000000000000050" |> logIt

//makeEthRPCCall ganEnv GANACHE EthMethod.GetBalance [$"{lender.address}"] |> logIt
makeEthRPCCall ganEnv GANACHE EthMethod.GetBalance [$"{naiveReceiver.address}"] |> logIt
//txnGan lender (ByString "flashLoan") [Address naiveReceiver.address; Uint256 "10000000000000000000"] ZEROV |> logIt
makeEthRPCCall ganEnv GANACHE EthMethod.GetBalance [$"{naiveReceiver.address}"] |> logIt
