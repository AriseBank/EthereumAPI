using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using EthereumCore.Settings;
using EthereumServices;
using EthereumTest.Init;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NBitcoin.Crypto;
using Nethereum.ABI.Encoders;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Util;
using Nethereum.Core.Signing.Crypto;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.DebugGeth.DTOs;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace EthereumTest
{
	[TestClass]
	public class MultisignedTest
	{
		const string contractAbi = "[{\"constant\":false,\"inputs\":[{\"name\":\"to\",\"type\":\"address\"},{\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"cashin\",\"outputs\":[{\"name\":\"\",\"type\":\"bool\"}],\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"addr\",\"type\":\"address\"}],\"name\":\"balanceOf\",\"outputs\":[{\"name\":\"\",\"type\":\"uint256\"}],\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"name\":\"from\",\"type\":\"address\"},{\"name\":\"amount\",\"type\":\"uint256\"},{\"name\":\"to\",\"type\":\"address\"}],\"name\":\"cashout\",\"outputs\":[{\"name\":\"\",\"type\":\"bool\"}],\"type\":\"function\"},{\"inputs\":[],\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"name\":\"who\",\"type\":\"address\"},{\"indexed\":false,\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"Payment\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"name\":\"who\",\"type\":\"address\"},{\"indexed\":false,\"name\":\"from\",\"type\":\"address\"},{\"indexed\":false,\"name\":\"amount\",\"type\":\"uint256\"},{\"indexed\":false,\"name\":\"to\",\"type\":\"address\"}],\"name\":\"Confirmation\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"name\":\"who\",\"type\":\"address\"},{\"indexed\":false,\"name\":\"requestAddress\",\"type\":\"address\"},{\"indexed\":false,\"name\":\"amount\",\"type\":\"uint256\"},{\"indexed\":false,\"name\":\"to\",\"type\":\"address\"}],\"name\":\"ConfirmationNeeded\",\"type\":\"event\"}]\n";
		const string contractByteCode = "0x60606040525b33600060006101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908302179055505b61082f8061003f6000396000f360606040523615610053576000357c010000000000000000000000000000000000000000000000000000000090048063388f3cd3146100b157806370a08231146100e6578063e417b7771461011257610053565b6100af5b7fd4f43975feb89f48dd30cabbb32011045be187d1e11c8ea9faa43efc352825193334604051808373ffffffffffffffffffffffffffffffffffffffff1681526020018281526020019250505060405180910390a15b565b005b6100d06004808035906020019091908035906020019091905050610150565b6040518082815260200191505060405180910390f35b6100fc6004808035906020019091905050610200565b6040518082815260200191505060405180910390f35b61013a6004808035906020019091908035906020019091908035906020019091905050610293565b6040518082815260200191505060405180910390f35b60007fd4f43975feb89f48dd30cabbb32011045be187d1e11c8ea9faa43efc352825193334604051808373ffffffffffffffffffffffffffffffffffffffff1681526020018281526020019250505060405180910390a160008210156101b557610002565b81600160005060008573ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020600082828250540192505081905550600190506101fa565b92915050565b60007fd4f43975feb89f48dd30cabbb32011045be187d1e11c8ea9faa43efc352825193334604051808373ffffffffffffffffffffffffffffffffffffffff1681526020018281526020019250505060405180910390a1600160005060008373ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060005054905061028e565b919050565b6000600060007fd4f43975feb89f48dd30cabbb32011045be187d1e11c8ea9faa43efc352825193334604051808373ffffffffffffffffffffffffffffffffffffffff1681526020018281526020019250505060405180910390a18573ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff16141580156103785750600060009054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff1614155b1561038257610002565b84600160005060008873ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000206000505410156103be57610002565b6000366040518083838082843782019150509250505060405180910390209150600260005060008381526020019081526020016000206000509050600060009054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff161415610519578060000160019054906101000a900460ff1615156105145760018160000160016101000a81548160ff021916908302179055507fb2a9fee4323d1fd3bd94275103abdaca2bfa415322e4bc10feb90f1496544d9533878787604051808573ffffffffffffffffffffffffffffffffffffffff1681526020018473ffffffffffffffffffffffffffffffffffffffff1681526020018381526020018273ffffffffffffffffffffffffffffffffffffffff16815260200194505050505060405180910390a15b6105df565b8060000160009054906101000a900460ff1615156105de5760018160000160006101000a81548160ff021916908302179055507fb2a9fee4323d1fd3bd94275103abdaca2bfa415322e4bc10feb90f1496544d9533878787604051808573ffffffffffffffffffffffffffffffffffffffff1681526020018473ffffffffffffffffffffffffffffffffffffffff1681526020018381526020018273ffffffffffffffffffffffffffffffffffffffff16815260200194505050505060405180910390a15b5b8060000160009054906101000a900460ff16801561060b57508060000160019054906101000a900460ff165b156106af57843073ffffffffffffffffffffffffffffffffffffffff1631101561063457610002565b6002600050600083815260200190815260200160002060006000820160006101000a81549060ff02191690556000820160016101000a81549060ff021916905550508373ffffffffffffffffffffffffffffffffffffffff16600086604051809050600060405180830381858888f193505050509250610826565b8060000160009054906101000a900460ff161515610759577f9c60b2df4b8bc59ae9c670df66acd0da8999024b23c7a3f07d50c934b18e8f8433878787604051808573ffffffffffffffffffffffffffffffffffffffff1681526020018473ffffffffffffffffffffffffffffffffffffffff1681526020018381526020018273ffffffffffffffffffffffffffffffffffffffff16815260200194505050505060405180910390a15b8060000160019054906101000a900460ff161515610825577f9c60b2df4b8bc59ae9c670df66acd0da8999024b23c7a3f07d50c934b18e8f8433600060009054906101000a900473ffffffffffffffffffffffffffffffffffffffff168787604051808573ffffffffffffffffffffffffffffffffffffffff1681526020018473ffffffffffffffffffffffffffffffffffffffff1681526020018381526020018273ffffffffffffffffffffffffffffffffffffffff16815260200194505050505060405180910390a15b5b5050939250505056";

		private readonly IContractService _contractService;
		private readonly IPaymentService _paymentService;
		private readonly IBaseSettings _settings;

		public MultisignedTest()
		{
			_paymentService = UnityConfig.GetConfiguredContainer().Resolve<IPaymentService>();
			_contractService = UnityConfig.GetConfiguredContainer().Resolve<IContractService>();
			_settings = UnityConfig.GetConfiguredContainer().Resolve<IBaseSettings>();
		}

		[TestMethod]
		public async Task TestSignature()
		{
			string account_a = "0x960336a077fB32d675405bd0A6cD0cb74aaa5062";
			string account_b = "0xb295e245eD2fdf5776c3C8a49f0403BF0242262A";

			var web3 = new Web3(_settings.EthereumUrl);
			await web3.Personal.UnlockAccount.SendRequestAsync(_settings.EthereumMainAccount, _settings.EthereumMainAccountPassword, new HexBigInteger(300));

			string privateKey_a = "4085dde01ea641a0f4fd6586ca11fc1f5df38e1bdcbef501da970cad9335b389";

			var test = "0x9cef50cb7f4cce644e45a92d533b184c7ee92d04"; //await CreateContract(web3, GetContractData("Test.abi"), GetContractData("Test.bin"));
			var contract = web3.Eth.GetContract(GetContractData("Test.abi"), test);
			//await SignMessage(contract, "hellow world", privateKey_a);

			var left = await contract.GetFunction("getHash3").CallAsync<byte[]>(account_a, account_b, 10);

			var str = account_a.HexToByteArray().ToHex() + account_b.HexToByteArray().ToHex() + new IntTypeEncoder().EncodeInt(10).ToHex();
			var right = new Sha3Keccack().CalculateHash(str.HexToByteArray());


		}

		[TestMethod]
		public async Task TestCoin()
		{
			const string DefaultAddress = "0x00000000000000000000000000000000";
			// pass: 123456789
			string privateKey_a = "4085dde01ea641a0f4fd6586ca11fc1f5df38e1bdcbef501da970cad9335b389";
			string privateKey_b = "74ed04f45c2a375a94189ef69661fa08235bb3b76be65934a0827262542e870c";
			string account_a = "0x960336a077fB32d675405bd0A6cD0cb74aaa5062";
			string account_b = "0xb295e245eD2fdf5776c3C8a49f0403BF0242262A";

			var web3 = new Web3(_settings.EthereumUrl);

			await web3.Personal.UnlockAccount.SendRequestAsync(_settings.EthereumMainAccount, _settings.EthereumMainAccountPassword, new HexBigInteger(300));

			var main = "0x3e052bb6c4ae2eeb867eb61c2e8ed51a6dfe1641"; //await CreateContract(web3, GetContractData("MainExchange.abi"), GetContractData("MainExchange.bin"));
			var coinA = "0xcb1f914327b50cb956f4f2f752ec1526727ef58c"; //await CreateContract(web3, GetContractData("ColorCoin.abi"), GetContractData("ColorCoin.bin"), main);
			var coinB = "0xecac46763bef29cf540496fb17f87c010c10751e"; //await CreateContract(web3, GetContractData("ColorCoin.abi"), GetContractData("ColorCoin.bin"), main);


			var mainContract = web3.Eth.GetContract(GetContractData("MainExchange.abi"), main);
			var coinContract_A = web3.Eth.GetContract(GetContractData("ColorCoin.abi"), coinA);
			var coinContract_B = web3.Eth.GetContract(GetContractData("ColorCoin.abi"), coinB);

			
			//await CoinCashinInternal(web3, coinContract_A, account_a, 200);
			//await CoinCashinInternal(web3, coinContract_B, account_b, 500);

			//var ev = mainContract.GetEvent("ConfirmationNeed");

			//var filter = await ev.CreateFilterAsync();

			string value = account_a + account_b + coinA + coinB + 15 + 20;
			var hash = new Sha3Keccack().CalculateHash(value).HexToByteArray();

			var client_a_sign = Sign(hash, privateKey_a);
			var client_b_sign = Sign(hash, privateKey_b);

			var result = await mainContract.GetFunction("swap").CallAsync<bool>(account_a, account_b, coinA, coinB, 15, 20, hash, client_a_sign, client_b_sign);

			
			//string trHash = await CoinCashoutInternal(web3, mainContract, coinA, account_a, 11, DefaultAddress);

			var transactionHash = await mainContract.GetFunction("swap").SendTransactionAsync(_settings.EthereumMainAccount, new HexBigInteger("200000"), new HexBigInteger(0),
				account_a, account_b, coinA, coinB, 15, 20, hash, client_a_sign, client_b_sign);


			TransactionReceipt receipt;

			while ((receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash)) == null)
			{
				await Task.Delay(100);
			}



			//var changes = await ev.GetFilterChanges<ConfirmNeededEvent>(filter);
			//var hash = changes.First().Event.Hash;


			var amount_a_contract_a = await coinContract_A.GetFunction("coinBalanceMultisig").CallAsync<BigInteger>(account_a);
			var amount_b_contract_a = await coinContract_A.GetFunction("coinBalanceMultisig").CallAsync<BigInteger>(account_b);

			var amount_a_contract_b = await coinContract_B.GetFunction("coinBalanceMultisig").CallAsync<BigInteger>(account_a);
			var amount_b_contract_b = await coinContract_B.GetFunction("coinBalanceMultisig").CallAsync<BigInteger>(account_b);


			//var amount = await coinContract.GetFunction("coinBalanceMultisig").CallAsync<BigInteger>(_settings.EthereumMainAccount);

			//var transactionHash = await mainContract.GetFunction("transferTest").SendTransactionAsync(_settings.EthereumMainAccount, new HexBigInteger("200000"), new HexBigInteger(0), coin, _settings.EthereumMainAccount, account, 100);

			//TransactionReceipt receipt;

			//while ((receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash)) == null)
			//{
			//	await Task.Delay(100);
			//}

			//var amount2 = await coinContract.GetFunction("coinBalanceMultisig").CallAsync<BigInteger>(_settings.EthereumMainAccount);
			//var amount3 = await coinContract.GetFunction("coinBalanceMultisig").CallAsync<BigInteger>(account);

			//var changes = await ev.GetFilterChanges<CoinTransferEvent>(filter);
		}

		private async Task CoinCashinInternal(Web3 web3, Contract contract, string to, int amount)
		{
			await web3.Personal.UnlockAccount.SendRequestAsync(_settings.EthereumMainAccount, _settings.EthereumMainAccountPassword, new HexBigInteger(120));

			var transactionHash = await contract.GetFunction("cashin").SendTransactionAsync(_settings.EthereumMainAccount, new HexBigInteger("200000"), new HexBigInteger(0),
				to, amount);

			TransactionReceipt receipt;

			while ((receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash)) == null)
			{
				await Task.Delay(100);
			}
		}

		private async Task<string> CoinCashoutInternal(Web3 web3, Contract contract, string coinContract, string from, int amount, string to)
		{

			await web3.Personal.UnlockAccount.SendRequestAsync(_settings.EthereumMainAccount, _settings.EthereumMainAccountPassword, new HexBigInteger(120));

			var transactionHash = await contract.GetFunction("cashout").SendTransactionAsync(_settings.EthereumMainAccount, new HexBigInteger("200000"), new HexBigInteger(0),
				coinContract, from, amount, to);

			TransactionReceipt receipt;

			while ((receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash)) == null)
			{
				await Task.Delay(100);
			}

			return transactionHash;
		}

		private async Task<string> ConfirmTransaction(string name, Web3 web3, Contract contract, string address, string privateKey, params object[] input)
		{
			var func = contract.GetFunction(name);
			var data = func.GetData(input);
			var price = await web3.Eth.GasPrice.SendRequestAsync();

			var txCount = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(address);
			
			var encoded = web3.OfflineTransactionSigning.SignTransaction(privateKey, contract.Address, 0, txCount, price.Value, 200000, data);
			
			Assert.IsTrue(web3.OfflineTransactionSigning.VerifyTransaction(encoded));

			var cashout2 = await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync("0x" + encoded);

			TransactionReceipt receipt;
			while ((receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(cashout2)) == null)
			{
				await Task.Delay(100);
			}

			return receipt.TransactionHash;
		}

		private byte[] Sign(byte[] hash, string privateKey)
		{
			var key = new ECKey(privateKey.HexToByteArray(), true);
			var signature = key.SignAndCalculateV(hash);

			var r = signature.R.ToByteArrayUnsigned().ToHexCompact();
			var s = signature.S.ToByteArrayUnsigned().ToHexCompact();
			var v = new[] { signature.V }.ToHexCompact();

			var arr = (r + s + v).HexToByteArray();
			return FixBytesOrder(arr);
		}

		private async Task<string> SignMessage(Contract contract, string message, string privateKey)
		{
			var web3 = new Web3();

			

			var hash = new Sha3Keccack().CalculateHash(message);
			var hashBytes = hash.HexToByteArray();

			var key = new ECKey(privateKey.HexToByteArray(), true);

			var signature = key.SignAndCalculateV(hashBytes);
			var test_r = signature.R.ToByteArrayUnsigned().ToHexCompact();
			var test_s = signature.S.ToByteArrayUnsigned().ToHexCompact();
			var test_v = new[] { signature.V }.ToHexCompact();

			var der = key.Sign(hashBytes).ToDER();
			var sss = Encoding.UTF8.GetString(signature.R.ToByteArrayUnsigned());
			//var hex = "0x" + der.ToHex();
			//var r = hex.Substring(0, 66).HexToByteArray();
			//var s = ("0x" + hex.Substring(66, 130 - 66)).HexToByteArray();
			//var v = ("0x" + hex.Substring(130, 132 - 130)).HexToByteArray();
			
			var arr = (test_r + test_s + test_v).HexToByteArray();
			var resArr = FixBytesOrder(arr);

			var result = await contract.GetFunction("checkSig").CallAsync<bool>("0x960336a077fB32d675405bd0A6cD0cb74aaa5062", hashBytes, resArr);
			//var result = await contract.GetFunction("calc").CallAsync<string>(hashBytes, 27, test_r.HexToByteArray(), test_s.HexToByteArray());
			
			//var t = BitConverter.TO(v, 0);

			//var r = EthECKey.RecoverFromSignature(signature, hashBytes);

			//var address = r.GetPublicAddress();

			return "";
		}

		private byte[] FixBytesOrder(byte[] source)
		{
			if (!BitConverter.IsLittleEndian)
				return source;

			return source.Reverse().ToArray();
		}

		private string GetContractData(string name)
		{
			return File.ReadAllText(@"..\..\..\EthereumContractBuilder\contracts\bin\" + name);
		}


		[TestMethod]
		public async Task SimpleTest()
		{
			const string account = "0x5912216a589cDEBc95798f2709c2D5a88c562bdB";

			var web3 = new Web3(_settings.EthereumUrl);

			await web3.Personal.UnlockAccount.SendRequestAsync(_settings.EthereumMainAccount, _settings.EthereumMainAccountPassword, new HexBigInteger(120));

			var contractAddress = await CreateContract(web3);

			var contract = web3.Eth.GetContract(contractAbi, contractAddress);

			var filter = contract.CreateFilterAsync();

			int amount = 1000000;

			var transaction = await contract.GetFunction("cashin").SendTransactionAsync(_settings.EthereumMainAccount, account, amount);

			TransactionReceipt receipt;

			while ((receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transaction)) == null)
			{
				await Task.Delay(100);
			}

			var balance = await contract.GetFunction("balanceOf").CallAsync<BigInteger>(account);

			Assert.AreEqual(amount, (int)balance);
		}

		[TestMethod]
		public async Task CashoutTest()
		{
			var privateAddress = "0x2ed8cdc3CD7D2DC510B7A14A1400ba163606e4Dc";
			var privateKey = "98ec4294a36a85464b1d8e7f237242bfb2d0497e3d2848a047c117baf0bdbba9";

			//const string accountMiddle = "0x5912216a589cDEBc95798f2709c2D5a88c562bdB";
			const string accountTo = "0x5ee16bc7bD7974Ea20C88F65172A3fFb6E82F0Bb";

			var web3 = new Web3(_settings.EthereumUrl);
			TransactionReceipt receipt;

			await web3.Personal.UnlockAccount.SendRequestAsync(_settings.EthereumMainAccount, _settings.EthereumMainAccountPassword, new HexBigInteger(600));

			var contractAddress = "0x2d65ca38369d4c2887c5add912be8813b228beec";//await CreateContract(web3);

			var contract = web3.Eth.GetContract(contractAbi, contractAddress);
			var paymentEvent = contract.GetEvent("Payment");
			//var pfilter = await paymentEvent.CreateFilterAsync();

			//var transactionHash = await web3.Eth.Transactions.SendTransaction.SendRequestAsync(new TransactionInput()
			//{
			//	From = _settings.EthereumMainAccount,
			//	To = contractAddress,
			//	Value = new HexBigInteger(UnitConversion.Convert.ToWei(2)),
			//	Gas = new HexBigInteger(300000)
			//});

			//TransactionReceipt receipt;

			//while ((receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash)) == null)
			//{
			//	await Task.Delay(100);
			//}


			var confirmEvent = contract.GetEvent("Confirmation");
			var confirmNeededEvent = contract.GetEvent("ConfirmationNeeded");

			var f1 = await confirmEvent.CreateFilterAsync();
			var f2 = await confirmNeededEvent.CreateFilterAsync();

			BigInteger amount = UnitConversion.Convert.ToWei(0.5);

			//var transaction = await contract.GetFunction("cashin").SendTransactionAsync(_settings.EthereumMainAccount, privateAddress, amount);


			//while ((receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transaction)) == null)
			//{
			//	await Task.Delay(100);
			//}

			var cashout = await contract.GetFunction("cashout").SendTransactionAsync(_settings.EthereumMainAccount, privateAddress, amount, accountTo);

			//while ((receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(cashout)) == null)
			//{
			//	await Task.Delay(100);
			//}

			//var c1 = await confirmEvent.GetFilterChanges<ConfirmEvent>(f1);
			//var c2 = await confirmNeededEvent.GetFilterChanges<ConfirmNeededEvent>(f2);


			//var x = await web3.OfflineTransactionSigning.SignTransaction(

			//await web3.Personal.UnlockAccount.SendRequestAsync(accountMiddle, "123456", new HexBigInteger(120));
			//var cashout2 = await contract.GetFunction("cashout").SendTransactionAsync(accountMiddle, accountMiddle, amount, accountTo);

			var data = contract.GetFunction("cashout").GetData(privateAddress, amount, accountTo);

			var gas = await contract.GetFunction("cashout").EstimateGasAsync(privateAddress, amount, accountTo);
			var price = await web3.Eth.GasPrice.SendRequestAsync();

			var txCount = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(privateAddress);
			var encoded = web3.OfflineTransactionSigning.SignTransaction(privateKey, contractAddress, 0, txCount, price.Value, gas.Value, data);

			Assert.IsTrue(web3.OfflineTransactionSigning.VerifyTransaction(encoded));

			var cashout2 = await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync("0x" + encoded);

			while ((receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(cashout2)) == null)
			{
				await Task.Delay(100);
			}

			//c1 = await confirmEvent.GetFilterChanges<ConfirmEvent>(f1);
			//c2 = await confirmNeededEvent.GetFilterChanges<ConfirmNeededEvent>(f2);
		}

		private async Task<string> CreateContract(Web3 web3)
		{
			// deploy contract
			var transactionHash = await web3.Eth.DeployContract.SendRequestAsync(contractAbi, contractByteCode, _settings.EthereumMainAccount, new HexBigInteger(1000000));

			// get contract transaction
			TransactionReceipt receipt;
			while ((receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash)) == null)
			{
				await Task.Delay(100);
			}

			// check if contract byte code is deployed
			var code = await web3.Eth.GetCode.SendRequestAsync(receipt.ContractAddress);

			if (string.IsNullOrWhiteSpace(code) || code == "0x")
			{
				throw new Exception("Code was not deployed correctly, verify bytecode or enough gas was to deploy the contract");
			}

			return receipt.ContractAddress;
		}

		private async Task<string> CreateContract(Web3 web3, string abi, string byteCode, params object[] values)
		{
			// deploy contract
			var transactionHash = await web3.Eth.DeployContract.SendRequestAsync(abi, "0x" + byteCode, _settings.EthereumMainAccount, new HexBigInteger(2000000), values);

			// get contract transaction
			TransactionReceipt receipt;
			while ((receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash)) == null)
			{
				await Task.Delay(100);
			}

			// check if contract byte code is deployed
			var code = await web3.Eth.GetCode.SendRequestAsync(receipt.ContractAddress);

			if (string.IsNullOrWhiteSpace(code) || code == "0x")
			{
				throw new Exception("Code was not deployed correctly, verify bytecode or enough gas was to deploy the contract");
			}

			return receipt.ContractAddress;
		}

		private async Task<string> TransferFunds(Web3 web3, string from, string to, double amount, string password)
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

			return transactionHash;
		}

		public class ConfirmEvent
		{
			[Parameter("address", "who", 1, false)]
			public string Address { get; set; }

			[Parameter("address", "from", 2, false)]
			public string From { get; set; }

			[Parameter("uint", "amount", 3, false)]
			public BigInteger Amount { get; set; }

			[Parameter("address", "to", 4, false)]
			public string To { get; set; }
		}

		public class ConfirmNeededEvent
		{
			[Parameter("address", "client", 1, false)]
			public string Address { get; set; }

			[Parameter("bytes32", "hash", 2, false)]
			public byte[] Hash { get; set; }
		}

		public class PaymentEvent
		{
			[Parameter("address", "who", 1, false)]
			public string Address { get; set; }

			[Parameter("uint", "amount", 2, false)]
			public BigInteger Amount { get; set; }
		}

		public class CoinTransferEvent
		{
			[Parameter("address", "sender", 1, false)]
			public string Address { get; set; }

			[Parameter("address", "from", 2, false)]
			public string From { get; set; }

			[Parameter("address", "to", 3, false)]
			public string To { get; set; }

			[Parameter("uint", "amount", 4, false)]
			public BigInteger Amount { get; set; }
		}

		public class DebugEvent
		{
			[Parameter("string", "msg", 1, false)]
			public string Message { get; set; }
		}
	}
}
