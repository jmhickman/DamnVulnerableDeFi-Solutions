#i """nuget: C:\Users\jon_h\source\repos\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.3.0" 
#load "../Common.fsx"
#load "../UniswapV2Bindings.fsx"
#load "../DamnValuableNFTBindings.fsx"
#load "../DamnValuableTokenBindings.fsx"

open web3.fs
open Common
open UniswapV2Bindings
open DamnValuableTokenBindings
open DamnValuableNFTBindings


///////////////////////////////////////////////////////////////////////////////////////////////////
//// Uniswap setup section                                                                     ////
///////////////////////////////////////////////////////////////////////////////////////////////////

let nftprice = (Ether "15") |> asWei
let tokenReserve = (Ether "15000") |> asWei
let ethReserve = (Ether "9000") |> asWei

let currentTimePlusSome = 
    makeEthRPCCall EthMethod.BlockNumber [] GANACHE devEnvironment1 
    |> devEnvironment1.log Emit 
    |> unwrapSimpleValue
    |> fun hexBlockNum ->
        makeEthRPCCall EthMethod.GetBlockByNumber [hexBlockNum; "false"] GANACHE devEnvironment1
    |> devEnvironment1.log Emit
    |> unwrapBlock
    |> fun b -> 
        b.timestamp 
        |> hexToBigIntP 
        |> fun i -> (i + 100000I).ToString()

//deployWETH ()
//deployUniFactory () devEnvironment1 

// Weth: 0xA72d94a231e175e5CCcCDb9131456209e85fEd57
// Factory: 0x639a0121BCAd7f4AFee10ee8DA00335c6f47D194

let weth = 
    loadDeployedContract wethABI GANACHE "0xA72d94a231e175e5CCcCDb9131456209e85fEd57" 
    |> optimisticallyBindDeployedContract
let uniFactory = 
    loadUniFactory GANACHE "0x639a0121BCAd7f4AFee10ee8DA00335c6f47D194"

let uniRouter = 
    loadUniRouter GANACHE "0x2356577B17A5e3A84F71E51A3a24194a72f2C875"

// Make sure to approve DVT on the Router, if the add liquidity fails with a 'transfer' error.
//addLiquidityEth DVT.address tokenReserve ZEROV ZEROV devEnvironment1.constants.walletAddress currentTimePlusSome ethReserve uniRouter devEnvironment1 |> logIt

let pair = 
    loadDeployedContract uniPairABI GANACHE "0x413bc17ef2256f42befc75fbcc8f881aa33844de" 
    |> optimisticallyBindDeployedContract


///////////////////////////////////////////////////////////////////////////////////////////////////
//// Remove Liquidity After Done                                                               ////
///////////////////////////////////////////////////////////////////////////////////////////////////


//let lpTokenQuantity = makeEthCall pair (ByName "balanceOf") [Address devEnvironment1.constants.walletAddress] devEnvironment1 |> devEnvironment1.log Emit |> unwrapCallResult |> List.head |> unwrapEVMValue
//printfn $"{lpTokenQuantity}"
// Let the router spend the LP token, pull out the liquidity, and swap the WETH to ETH (since I can't figure out how to make the Router do it...)
//makeEthTxn pair (ByName "approve") [Address uniRouter.address; Uint256 lpTokenQuantity] ZEROV devEnvironment1
//removeLiquidity DVT.address "0xA72d94a231e175e5CCcCDb9131456209e85fEd57" lpTokenQuantity ZEROV ZEROV devEnvironment1.constants.walletAddress currentTimePlusSome uniRouter devEnvironment1 |> logIt
//makeEthTxn weth (ByName "withdraw") [Uint256 $"""{makeEthCall weth (ByName "balanceOf") [Address devEnvironment1.constants.walletAddress] devEnvironment1 |> devEnvironment1.log Emit |> unwrapCallResult |> List.head |> unwrapEVMValue}"""] ZEROV devEnvironment1 |> logIt

// Uniswap Router is currently holding 15000 DVT and 9000 WETH in pair 0x413bc17ef2256f42befc75fbcc8f881aa33844de

///////////////////////////////////////////////////////////////////////////////////////////////////
//// Actual Challenge Setup Section                                                            ////
///////////////////////////////////////////////////////////////////////////////////////////////////


let freeRiderMarketplaceABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\FreeRiderNFTMarketplace.abi"""
let freeRiderBytecode = returnBytecodeFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\FreeRiderNFTMarketplace.json"""
let freeRiderBuyerABI = returnABIFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\FreeRiderBuyer.abi"""
let freeRiderBuyerBytecode = returnBytecodeFromFile """C:\Users\jon_h\source\repos\DamnVulnerableDeFi\bin\FreeRiderBuyer.json"""

// deploy and load marketplace with 90ETH of starting balance
let deployMarketplace () = 
    prepareAndDeployContract freeRiderBytecode freeRiderMarketplaceABI GANACHE [Uint256 "6" ] $"""{(Ether "90") |> asWei}""" devEnvironment1 

let marketplace = 
    loadDeployedContract freeRiderMarketplaceABI GANACHE "0x28AA15Bb9eAc449DEfb1c568A7f86a8Abc134FBC" 
    |> optimisticallyBindDeployedContract

// marketplace automatically creates nft contract instance and mints 6 nfts.
// Approval must be manually done. Dunno why that isn't in the constructor...
// First, get the address of the nft contract, then load it
let nftContractAddress = 
    makeEthCall marketplace (ByName "token") [] devEnvironment1 
    |> devEnvironment1.log Emit 
    |> unwrapCallResult |> List.head |> unwrapEVMValue
let nftContract = 
    loadDeployedContract DVNFTABI GANACHE nftContractAddress 
    |> optimisticallyBindDeployedContract

// Perform approval to sell
//makeEthTxn 
//    nftContract 
//    (ByName "setApprovalForAll") 
//    [Address marketplace.address; Bool true] 
//    ZEROV 
//    devEnvironment1 
//    |> logIt


// Then the offer has to be called on the 6 nfts
//let nftprice = (Ether "15") |> asWei
//makeEthTxn 
//    marketplace
//    (ByName "offerMany") 
//    [Uint256Array ["0"; "1"; "2"; "3"; "4"; "5"]; Uint256Array[nftprice; nftprice; nftprice; nftprice; nftprice; nftprice]] 
//    ZEROV 
//    devEnvironment1 
//    |> logIt

let bounty = (Ether "45") |> asWei
//prepareAndDeployContract freeRiderBuyerBytecode freeRiderBuyerABI GANACHE [Address devEnvironment2.constants.walletAddress; Address nftContract.address] bounty devEnvironment1
let buyer = 
    loadDeployedContract freeRiderBuyerABI GANACHE "0xf5b358Fa706AaF4A30eA4d7aCC16C2B9e3f2ae70" 
    |> optimisticallyBindDeployedContract


// Some diagnostics to make sure that the token pair (DVT/WETH) is alive and has sane values
//dumpAllPairs 1 uniFactory devEnvironment1
//getReserves pair devEnvironment1 |> logIt
//getToken0 pair devEnvironment1 |> logIt
//getToken1 pair devEnvironment1 |> logIt
