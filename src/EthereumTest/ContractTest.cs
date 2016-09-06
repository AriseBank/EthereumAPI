using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EthereumCore;
using EthereumCore.Azure;
using EthereumCore.ContractEvents;
using EthereumCore.Settings;
using EthereumServices;
using EthereumTest.Init;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Filters;
using Nethereum.Web3;
using Newtonsoft.Json;

namespace EthereumTest
{
	[TestClass]
	public class ContractTest
	{
		private readonly IContractService _contractService;
		private readonly IPaymentService _paymentService;
		private readonly IBaseSettings _settings;

		public ContractTest()
		{
			_paymentService = UnityConfig.GetConfiguredContainer().Resolve<IPaymentService>();
			_contractService = UnityConfig.GetConfiguredContainer().Resolve<IContractService>();
			_settings = UnityConfig.GetConfiguredContainer().Resolve<IBaseSettings>();
		}

		[Ignore]
		[TestMethod]
		public async Task GetMoneyFromContract()
		{
			var web3 = new Web3();

			await web3.Personal.UnlockAccount.SendRequestAsync(_settings.EthereumMainAccount, _settings.EthereumMainAccountPassword, new HexBigInteger(120));

			var userContractAddress = await _contractService.GenerateUserContract();

			var code = await web3.Eth.GetCode.SendRequestAsync(userContractAddress);

			if (code.Length < 5)
			{
				throw new Exception("Code was not deployed correctly, verify bytecode or enough gas was uto deploy the contract");
			}


			var contract = web3.Eth.GetContract(_settings.UserContract.Abi, userContractAddress);
			

			int amount = 900;

			// transfer from main account to second account (imagine that second account is external user account)
			await TransferFunds(web3, _settings.EthereumMainAccount, userContractAddress, amount + 10, _settings.EthereumMainAccountPassword);

			var function = contract.GetFunction("transferMoney");

			var transaction = await function.SendTransactionAsync(_settings.EthereumMainAccount, UnitConversion.Convert.ToWei(amount));

			TransactionReceipt receiptTransaction = null;

			while (receiptTransaction == null)
			{
				await Task.Delay(100);
				receiptTransaction = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transaction);
			}
		}

		[Ignore]
		[TestMethod]
		public async Task CreateMainContract()
		{
			string contractAddress = await _contractService.GenerateMainContract();

			var web3 = new Web3();
			var contract = web3.Eth.GetContract(_settings.MainContract.Abi, contractAddress);

			var paymentEvent = contract.GetEvent("PaymentFromUser");
			var filterAll = await paymentEvent.CreateFilterAsync();

			var func = contract.GetFunction("gotPayment");
			var result = await func.SendTransactionAsync(_settings.EthereumMainAccount, _settings.EthereumMainAccount, 1234);


			while (await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(result) == null)
			{
				await Task.Delay(100);
			}

			await web3.Miner.Stop.SendRequestAsync();

			var eventLogsAll = await paymentEvent.GetFilterChanges<UserPaymentEvent>(filterAll);
			var add = eventLogsAll[0].Event.Address;
			var amount = eventLogsAll[0].Event.Amount;


			Assert.IsTrue(string.Compare(_settings.EthereumMainAccount, add, StringComparison.OrdinalIgnoreCase) == 0);
			Assert.AreEqual(1234, amount);
		}

		[Ignore]
		[TestMethod]
		public async Task CreateUserContract()
		{
			var secondAccount = "0x13F022d72158410433CBd66f5DD8bF6D2D129924";

			var web3 = new Web3();

			await web3.Personal.UnlockAccount.SendRequestAsync(_settings.EthereumMainAccount, _settings.EthereumMainAccountPassword, new HexBigInteger(120));

			var userContractAddress = await _contractService.GenerateUserContract();

			var code = await web3.Eth.GetCode.SendRequestAsync(userContractAddress);

			if (code.Length < 5)
			{
				throw new Exception("Code was not deployed correctly, verify bytecode or enough gas was uto deploy the contract");
			}


			var contract = web3.Eth.GetContract(_settings.MainContract.Abi, _settings.EthereumMainContractAddress);
			var ev = contract.GetEvent("PaymentFromUser");
			
			var filter = await contract.CreateFilterAsync();

			int amount = 900;

			// transfer from main account to second account (imagine that second account is external user account)
			await TransferFunds(web3, _settings.EthereumMainAccount, secondAccount, amount + 10, _settings.EthereumMainAccountPassword);

			// user transfer ethers from his external account to our contract)
			await TransferFunds(web3, secondAccount, userContractAddress, 900, _settings.EthereumMainAccountPassword);

			// we receive event from main contract about payment (contract address and amount)

			var logs = await web3.Eth.Filters.GetFilterChangesForEthNewFilter.SendRequestAsync(filter);

			Assert.IsNotNull(logs);

			var eventLogs = ev.DecodeAllEvents<UserPaymentEvent>(logs);

			Assert.AreEqual(userContractAddress, eventLogs[0].Event.Address);
			Assert.AreEqual(amount, UnitConversion.Convert.FromWei(eventLogs[0].Event.Amount));

			logs = await web3.Eth.Filters.GetFilterChangesForEthNewFilter.SendRequestAsync(filter);
			
			Assert.IsNull(logs);
		}

		private async Task TransferFunds(Web3 web3, string from, string to, int amount, string password)
		{
			var unlockAccountResult =
				await web3.Personal.UnlockAccount.SendRequestAsync(from, password, new HexBigInteger(120));
			Assert.IsTrue(unlockAccountResult);

			var unitConversion = new UnitConversion();

			var transactionHash = await web3.Eth.Transactions.SendTransaction.SendRequestAsync(new TransactionInput()
			{
				From = from,
				To = to,
				Value = new HexBigInteger(unitConversion.ToWei(amount))
			});

			TransactionReceipt receipt;

			while ((receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash)) == null)
			{
				await Task.Delay(100);
			}
		}
	}
}
