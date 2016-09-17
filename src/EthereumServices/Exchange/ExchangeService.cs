using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using EthereumCore.Settings;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace EthereumServices.Exchange
{
	public class ExchangeService : BaseTransactionService
	{
		private const int SwapGas = 200000;

		private readonly IBaseSettings _settings;

		public ExchangeService(IBaseSettings settings)
			: base(settings)
		{
			_settings = settings;
		}

		public async Task<string> Swap(long id, string clientA, string clientB, string coinA, string coinB, BigInteger amountA,
			BigInteger amountB)
		{
			var web3 = new Web3(_settings.EthereumUrl);
			var mainContract = web3.Eth.GetContract(_settings.ExchangeContract.Abi, _settings.ExchangeContract.Address);

			var transactionHash = await mainContract.GetFunction("swap").SendTransactionAsync(_settings.EthereumMainAccount, new HexBigInteger(SwapGas), new HexBigInteger(0),
				coinA, coinB, coinA, coinB, amountA, amountB);

			TransactionReceipt receipt;

			while ((receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash)) == null)
			{
				await Task.Delay(100);
			}

			return await GetTransactionStatus(SwapGas, receipt);
		}

		 
	}
}
