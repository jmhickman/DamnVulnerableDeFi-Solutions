#i """nuget: C:\Users\jon_h\source\repos\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.7" 
#load "../Common.fsx"

#load "../DamnValuableTokenBindings.fsx"

open web3.fs
open Common
open DamnValuableTokenBindings

let trusterLenderPoolABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\TrusterLenderPool.abi"""

let deployTrusterLenderPool () = 
    let trusterLenderPoolBytecode = returnBytecodeFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\TrusterLenderPool.json"""
    prepareUndeployedContract devEnvironment1 trusterLenderPoolBytecode (Some [Address DVT.address]) GANACHE trusterLenderPoolABI 
    |> Result.bind (deployEthContract devEnvironment1 ZEROV) 
    |> logIt

deployTrusterLenderPool ()

// Deployed to: 0x5e0ba83149fe051a9d365f0b725ad0964b45fb0b

// fund pool
transferDVT "0x5e0ba83149fe051a9d365f0b725ad0964b45fb0b" "1000000000000000000000000"

let drainPoolABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\DrainPool.abi"""

let deployAttacker () =
    let drainPoolBytecode = returnBytecodeFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\DrainPool.json"""
    prepareUndeployedContract devEnvironment2 drainPoolBytecode None GANACHE drainPoolABI 
    |> Result.bind (deployEthContract devEnvironment2 ZEROV) 
    |> logIt

deployAttacker ()

