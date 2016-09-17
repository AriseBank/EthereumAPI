using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage.Azure;
using EthereumCore;
using EthereumCore.Exceptions;

namespace EthereumServices
{
	public interface IContractQueueService
	{
		Task<string> GetContract();
		Task PushContract(string contract);
		int Count();
	}

	public class ContractQueueService : IContractQueueService
	{
		private readonly IEmailNotifierService _emailNotifier;
		private readonly IQueueExt _queue;

		public ContractQueueService(Func<string, IQueueExt> queueFactory, IEmailNotifierService emailNotifier)
		{
			_emailNotifier = emailNotifier;
			_queue = queueFactory(Constants.EthereumContractQueue);
		}

		public async Task<string> GetContract()
		{
			var contract = await _queue.Pop();

			if (string.IsNullOrWhiteSpace(contract))
			{
				_emailNotifier.Warning("Ethereum", "User contract pool is empty!");
				throw new BackendException(BackendExceptionType.ContractPoolEmpty);
			}

			return contract;
		}

		public async Task PushContract(string contract)
		{
			await _queue.PutRawMessage(contract);
		}

		public int Count()
		{
			return _queue.Count() ?? 0;
		}
	}
}
