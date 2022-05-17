#i """nuget: C:\Users\jon_h\source\repos\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.3" 
#load "../Common.fsx"

#load "../DamnValuableTokenSnapshotBindings.fsx"

open web3.fs
open Common


// Exchange needs 9990 ETH
// NFT price is 999 ETH
// Each Source gets 2 ETH
// Attacker starts with .1 ETH

// Initializer Contract deploys the Oracles

let trust1 = "0xA73209FB1a42495120166736362A1DfA9F95A105"
let trust2 = "0xe92401A4d3af5E446d93D11EEc806b1462b39D15"
let trust3 = "0x81A5D6E50C214044bE44cA0CB057fe119097850c"
let symbol = String "DVNFT"
let NFTVALUE = (Ether "999") |> asWei

let oracleInitalizerABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\TrustfulOracleInitializer.abi"""
let exchangeABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\Exchange.abi"""

let deployOracleInitializer () =
    prepareAndDeployContract
        devEnvironment1
        GANACHE
        (returnBytecodeFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\TrustfulOracleInitializer.json""")
        oracleInitalizerABI
        (Some [AddressArray [trust1; trust2; trust3]; StringArray[symbol; symbol; symbol]; Uint256Array[NFTVALUE; NFTVALUE; NFTVALUE]])
        ZEROV
        
// deployOracleInitializer ()


let trustedOracle =
    loadDeployedContract
        devEnvironment1
        "0x9172e4b445b61698629cbcB61671C134Dc89B9a3"
        GANACHE
        (returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\TrustfulOracle.abi""")
        |> optimisticallyBindDeployedContract
        

// Exchange deploys the NFT itself, which I didn't expect
let deployExchange () =
    prepareAndDeployContract
        devEnvironment1
        GANACHE
        (returnBytecodeFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\Exchange.json""")
        exchangeABI
        (Some [Address trustedOracle.address])
        ZEROV
    
let exchange =
    loadDeployedContract
        devEnvironment1
        "0xff8a26a271ef7963a24cc01262bcfb3c588860d4"
        GANACHE
        exchangeABI
    |> optimisticallyBindDeployedContract    
        
txnDevWith1 exchange  Receive [] ((Ether "9990") |> asWei) |> logIt
