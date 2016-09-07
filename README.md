# README #

How to setup build and run Ethereum Api.

### Before running ###

* Install .NET 4.6
* Build solution (release configuration will outputs to ${SolutionDir}/builds
* Build [solidity contracts](src/EthereumContractBuilder/contracts)

### How to create configuration file? ###

* Create generalsettings.json file [template](src/EthereumContractBuilder/generalsettings_template.json)
* Fill contract data from .bin and .abi contract files
* Fill fields:
    * EthereumMainAccount - main account in ethereum (to create contracts and transactions)
    * EthereumMainAccountPassword - main account password (for unlocking)
    * EthereumUrl - ethereum node URL
    * EthereumPrivateAccount - ethereum account to hold ethers
    * MinContractPoolLength - min length of user contract queue (default: 100)
    * MaxContractPoolLength - max length of user contract queue (default: 200)
    * MainAccountMinBalance - main account minimum balance in ETH (default: 1.0)
* Fill Db configuration
* Create Ethereum main contract
    * Save generalsettings.json and put it into EthereumContractBuilder release folder.
    * Run EthereumContractBuiler.exe and choose "Deploy main contract from local json file" (this will override generaltsettings.json and fill EthereumMainContractAddress field)
* Your generalsettings.json file is ready.

### How to run api ###

* Copy generalsettings.json to ApiRunner release folder
* Run ApiRunner with params:
    * -port=<port> - web server port
* Check web server on http://localhost:<port>/swagger

### How to run job? ###

* Copy generalsettings.json to JobRunner release folder
* Run JobRunner.exe
* 

https://ci.appveyor.com/api/projects/status/q3eafe8yjc5x36qt?svg=true
