using System;
using AzureStorage;
using AzureStorage.Azure;
using AzureStorage.Log;
using EthereumCore;
using EthereumCore.Log;
using EthereumCore.Settings;
using EthereumServices;
using EthereumWebJob.Job;
using Microsoft.Practices.Unity;

namespace EthereumWebJob.Config
{
	public class UnityConfig
	{
		public static IUnityContainer InitContainer(IBaseSettings settings)
		{
			var container = new UnityContainer();

			container.RegisterInstance(settings);

			AzureDependencyRegister.RegisterLogs(container, settings, "Job");
			AzureDependencyRegister.RegisterStorage(container, settings);
			AzureDependencyRegister.RegisterQueues(container, settings);

			ServiceDependencyRegister.Register(container);

			RegisterJobs(container);

			return container;
		}

		private static void RegisterJobs(IUnityContainer container)
		{
			container.RegisterType<CheckContractQueueCountJob>(new ContainerControlledLifetimeManager());
			container.RegisterType<CheckPaymentsToUserContractsJob>(new ContainerControlledLifetimeManager());
			container.RegisterType<RefreshContractQueueJob>(new ContainerControlledLifetimeManager());
			container.RegisterType<MonitoringJob>(new ContainerControlledLifetimeManager());
		}
	}
}
