using System;
using EthereumCore;
using EthereumCore.Azure;
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

			var logToTable = new LogToTable(
				new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, "JobError", null),
				new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, "JobWarning", null),
				new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, "JobInfo", null));

			container.RegisterInstance(logToTable);
			container.RegisterType<LogToConsole>();

			container.RegisterType<ILog, LogToTableAndConsole>();

			container.RegisterType<IContractService, ContractService>();
			container.RegisterType<IPaymentService, PaymentService>();
			container.RegisterType<IEthereumQueueOutService, EthereumQueueOutService>();
			container.RegisterType<IContractQueueService, ContractQueueService>();
			container.RegisterType<IEmailNotifierService, EmailNotifierService>();

			container.RegisterType<Func<string, IQueueExt>>(new InjectionFactory(c =>
				new Func<string, IQueueExt>(x =>
				{
					switch (x)
					{
						case Constants.EthereumContractQueue:
						case Constants.EthereumOutQueue:
							return new AzureQueueExt(settings.Db.EthereumOutQueueConnString, x);
						case Constants.EmailNotifierQueue:
							return new AzureQueueExt(settings.Db.ClientPersonalInfoConnString, x);
						default:
							throw new Exception("Queue is not registered");
					}
				})));

			RegisterJobs(container);

			return container;
		}

		private static void RegisterJobs(IUnityContainer container)
		{
			container.RegisterType<CheckContractQueueCountJob>(new ContainerControlledLifetimeManager());
			container.RegisterType<CheckPaymentsToUserContractsJob>(new ContainerControlledLifetimeManager());
			container.RegisterType<RefreshContractQueueJob>(new ContainerControlledLifetimeManager());
		}
	}
}
