using System;
using EthereumCore;
using EthereumCore.Azure;
using EthereumCore.Log;
using EthereumCore.Settings;
using EthereumServices;
using Microsoft.Practices.Unity;

namespace EthereumApiSelfHosted
{
	public static class UnityConfig
	{
		public static IUnityContainer RegisterComponents(IBaseSettings settings)
		{
			var container = new UnityContainer();

			container.RegisterInstance(settings);

			var logToTable = new LogToTable(new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, "EthereumLogApi", null));
			container.RegisterInstance(logToTable);
			container.RegisterType<LogToConsole>();

			container.RegisterType<ILog, LogToTableAndConsole>();

			container.RegisterType<IContractService, ContractService>();
			container.RegisterType<IPaymentService, PaymentService>();
			container.RegisterType<IContractQueueService, ContractQueueService>();

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

			return container;
		}
	}
}