using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EthereumCore.Timers;
using EthereumWebJob.Job;
using Microsoft.Practices.Unity;

namespace EthereumWebJob
{
	public class JobFactory
	{
		public static void RunJobs(IUnityContainer container)
		{
			container.Resolve<CheckContractQueueCountJob>().Start();
			container.Resolve<CheckPaymentsToUserContractsJob>().Start();
			container.Resolve<RefreshContractQueueJob>().Start();
			container.Resolve<MonitoringJob>().Start();
		}
	}
}
