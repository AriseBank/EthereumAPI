using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EthereumCore.Log;
using EthereumCore.Timers;
using EthereumServices;

namespace EthereumWebJob.Job
{
	public class RefreshContractQueueJob : TimerPeriod
	{
		private const int TimerPeriodSeconds = 12 * 60 * 60; // 12 hours

		private readonly IContractQueueService _contractQueueService;
		private readonly ILog _logger;

		public RefreshContractQueueJob(IContractQueueService contractQueueService, ILog logger)
			: this("RefreshContractQueueJob", TimerPeriodSeconds * 1000, logger)
		{
			_contractQueueService = contractQueueService;
			_logger = logger;
		}

		private RefreshContractQueueJob(string componentName, int periodMs, ILog log) : base(componentName, periodMs, log)
		{
		}

		public override async Task Execute()
		{
			for (var i = 0; i < _contractQueueService.Count(); i++)
			{
				try
				{
					var contract = await _contractQueueService.GetContract();
					await _contractQueueService.PushContract(contract);
				}
				catch (Exception e)
				{
					await _logger.WriteError("EthereumWebJob", "RefreshQueue", "", e);
				}
			}
		}
	}
}
