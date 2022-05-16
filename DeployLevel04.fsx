#i """nuget: C:\Users\jon_h\source\repos\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.7" 
#load "../Common.fsx"

#load "../DamnValuableTokenBindings.fsx"

open web3.fs
open Common
open DamnValuableTokenBindings

let lenderABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\SideEntranceLenderPool.abi"""

let deploySideEntranceLender () =
    let lenderBytecode = returnBytecodeFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\SideEntranceLenderPool.json"""
    prepareUndeployedContract devEnvironment1 lenderBytecode None GANACHE lenderABI 
    |> Result.bind (deployEthContract devEnvironment1 ZEROV)
    |> logIt

deploySideEntranceLender ()

let attackABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\Level04Attack.abi"""

let deployAttack () = 
    let attackBytecode = returnBytecodeFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\Level04Attack.json"""
    prepareUndeployedContract devEnvironment1 attackBytecode None GANACHE attackABI
    |> Result.bind (deployEthContract devEnvironment1 ZEROV)
    |> logIt

deployAttack () 
