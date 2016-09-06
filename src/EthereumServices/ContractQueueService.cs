using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EthereumCore;
using EthereumCore.Azure;

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
		private readonly IQueueExt _queue;

		public ContractQueueService(Func<string, IQueueExt> queueFactory)
		{
			_queue = queueFactory(Constants.EthereumContractQueue);
		}

		public async Task<string> GetContract()
		{
			return await _queue.Pop();
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
