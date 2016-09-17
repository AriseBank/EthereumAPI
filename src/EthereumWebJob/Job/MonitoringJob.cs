using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EthereumCore.Log;
using EthereumCore.Monitoring;
using EthereumCore.Timers;
using EthereumServices;

namespace EthereumWebJob.Job
{
	public class MonitoringJob : TimerPeriod
	{
		private const int TimerPeriodSeconds = 30;

		private readonly IMonitoringRepository _repository;

		public MonitoringJob(IMonitoringRepository repository, ILog logger)
			: this("MonitoringJob", TimerPeriodSeconds * 1000, logger)
		{
			_repository = repository;
		}

		private MonitoringJob(string componentName, int periodMs, ILog log) : base(componentName, periodMs, log)
		{
		}

		public override async Task Execute()
		{
			await _repository.SaveAsync(new Monitoring { DateTime = DateTime.UtcNow, ServiceName = "EthereumJobService" });
		}
	}
}
