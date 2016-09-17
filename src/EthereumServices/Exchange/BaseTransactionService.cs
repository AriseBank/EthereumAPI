using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EthereumCore.Settings;
using EthereumServices.Exceptions;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace EthereumServices.Exchange
{
	public class BaseTransactionService
	{
		private readonly IBaseSettings _settings;

		public BaseTransactionService(IBaseSettings settings)
		{
			_settings = settings;
		}

		public async Task<string> GetTransactionStatus(int gasSended, TransactionReceipt receipt)
		{
			if (receipt.GasUsed == new Nethereum.Hex.HexTypes.HexBigInteger(gasSended))
				return receipt.TransactionHash;

			var web3 = new Web3(_settings.EthereumUrl);
			var logs =
				await
					web3.DebugGeth.TraceTransaction.SendRequestAsync(receipt.TransactionHash,
						new Nethereum.RPC.DebugGeth.DTOs.TraceTransactionOptions());

			var obj = logs.ToObject<TansactionTrace>();
			if (obj.StructLogs?.Length > 0 && !string.IsNullOrWhiteSpace(obj.StructLogs[obj.StructLogs.Length - 1].Error))
				throw new EthTransactionException(obj.StructLogs[obj.StructLogs.Length - 1].Error, receipt.TransactionHash);

			return receipt.TransactionHash;
		}
	}

	public class TansactionTrace
	{
		public int Gas { get; set; }
		public string ReturnValue { get; set; }
		public TransactionStructLog[] StructLogs { get; set; }
	}

	public class TransactionStructLog
	{
		public string Error { get; set; }
	}
}
