#i """nuget: C:\Users\jon_h\source\repos\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.3" 
#load "../Common.fsx"

#load "../DamnValuableTokenSnapshotBindings.fsx"

open web3.fs
open Common
open DamnValuableTokenSnapshotBindings

let simpleGovernanceABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\SimpleGovernance.abi"""
let selfiePoolABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\SelfiePool.abi"""

let deployGovernance () =
    let simpleGovernanceBytecode = returnBytecodeFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\SimpleGovernance.json"""
    prepareUndeployedContract devEnvironment1 simpleGovernanceBytecode (Some [Address DVTS.address]) GANACHE simpleGovernanceABI 
    |> Result.bind (deployEthContract devEnvironment1 ZEROV)
    |> logIt


//deployGovernance ()

let governance = loadDeployedContract devEnvironment1 "0xa4d6dce5360832875cf958af3009ae173dafe130" GANACHE simpleGovernanceABI |> bindDeployedContract |> List.head


let deploySelfiePool () =
    let selfiePoolBytecode = returnBytecodeFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\SelfiePool.json"""
    prepareUndeployedContract devEnvironment1 selfiePoolBytecode (Some [Address DVTS.address; Address governance.address ]) GANACHE selfiePoolABI
    |> Result.bind (deployEthContract devEnvironment1 ZEROV)
    |> logIt


let selfiePool = loadDeployedContract devEnvironment1 "0xbf88e4ce1eb3fd0b69a07f2be29bb78526c24183" GANACHE selfiePoolABI |> bindDeployedContract |> List.head

deploySelfiePool ()

// transfer 1.5 million to simplePool

transferDVTS selfiePool.address "1500000000000000000000000"

let attackerABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\Level06Attack.abi"""

let deployAttacker () =
    let attackerBytecode = returnBytecodeFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\Level06Attack.json"""
    prepareUndeployedContract devEnvironment2 attackerBytecode None GANACHE attackerABI
    |> Result.bind (deployEthContract devEnvironment2 ZEROV)
    |> logIt

deployAttacker ()

let attacker = loadDeployedContract devEnvironment2 "0x6b0bfb11bf7f2aae5e2ff6fea265d68f2a93fa2f" GANACHE attackerABI |> bindDeployedContract |> List.head
