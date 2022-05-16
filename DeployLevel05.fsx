#i """nuget: C:\Users\jon_h\source\repos\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.3" 
#load "../Common.fsx"

#load "../DamnValuableTokenBindings.fsx"

open web3.fs
open Common
open DamnValuableTokenBindings

let accountingABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\AccountingToken.abi"""
let flashLoanerABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\FlashLoanerPool.abi"""
let rewardTokenABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\RewardToken.abi"""
let rewarderPoolABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\TheRewarderPool.abi"""


let deployFlashloaner () = 
    prepareAndDeployContract 
        devEnvironment1 
        GANACHE 
        (returnBytecodeFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\FlashLoanerPool.json""") 
        flashLoanerABI
        (Some [Address DVT.address]) 
        ZEROV
    

let deployRewardPool () = 
    prepareAndDeployContract
        devEnvironment1
        GANACHE
        (returnBytecodeFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\TheRewarderPool.json""")
        rewarderPoolABI
        (Some [Address DVT.address])
        ZEROV
    

//deployFlashloaner ()
//deployRewardPool ()

let flashLoaner = loadDeployedContract devEnvironment1 "0x05eac33fc744d1d08b0443a88bc2723266a43e91" GANACHE flashLoanerABI |> optimisticallyBindDeployedContract
let rewardPool = loadDeployedContract devEnvironment1 "0x1ce6a2cd2a0a25db38aeb84a06a2f83eb6cfa611" GANACHE rewarderPoolABI |> optimisticallyBindDeployedContract

// each of them needs 100 tokens DVT
let signers = 
    [ "0x676f23ff5645c7587c80f3fd770780d074a61519"
      "0x837c770cb3f9f57c970c86c1f4aae8f12d925900"
      "0x9ccb8c5b0d147a96bcf1bad78fe28fac9ca64988"
      "0x21f1b2ad1616cfb6b8c9c61ccdd978619713c10c" ]

signers |> List.map(fun signer -> transferDVT signer "100")

transferDVT flashLoaner.address "1000000000000000000000000"

// token approval and deposit performed in Remix for convenience' sake.

// Wait 1 hour, call Distribute rewards
txnDevWith2 rewardPool (ByString "distributeRewards") [] ZEROV |> logIt

// check that rewards were distributed by querying the reward token balance of one of the users (remix)

let attackABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\Level05Attack.abi"""

let attack =
    prepareAndDeployContract
        devEnvironment2
        GANACHE
        (returnBytecodeFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\Level05Attack.json""")
        attackABI
        None
        ZEROV
