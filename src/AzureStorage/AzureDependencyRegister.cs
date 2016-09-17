using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage.Azure;
using AzureStorage.Log;
using AzureStorage.Monitoring;
using EthereumCore;
using EthereumCore.Log;
using EthereumCore.Monitoring;
using EthereumCore.Settings;
using Microsoft.Practices.Unity;

namespace AzureStorage
{
	public class AzureDependencyRegister
	{
		public static IUnityContainer RegisterStorage(IUnityContainer container, IBaseSettings settings)
		{

			container.RegisterInstance<IMonitoringRepository>(
				new MonitoringRepository(new AzureTableStorage<MonitoringEntity>(settings.Db.ExchangeQueueConnString, "Monitoring", container.Resolve<ILog>())));

			return container;
		}

		public static IUnityContainer RegisterQueues(IUnityContainer container, IBaseSettings settings)
		{
			container.RegisterType<Func<string, IQueueExt>>(new InjectionFactory(c =>
				new Func<string, IQueueExt>(x =>
				{
					switch (x)
					{
						case Constants.EthereumContractQueue:
							return new AzureQueueExt(settings.Db.DataConnString, x);
						case Constants.EthereumOutQueue:
							return new AzureQueueExt(settings.Db.EthereumNotificationsConnString, x);
						case Constants.EmailNotifierQueue:
							return new AzureQueueExt(settings.Db.ExchangeQueueConnString, x);
						default:
							throw new Exception("Queue is not registered");
					}
				})));


			return container;
		}

		public static IUnityContainer RegisterLogs(IUnityContainer container, IBaseSettings settings, string logPrefix)
		{
			var logToTable = new LogToTable(
				new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, logPrefix + "Error", null),
				new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, logPrefix + "Warning", null),
				new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, logPrefix + "Info", null));

			container.RegisterInstance(logToTable);
			container.RegisterType<LogToConsole>();

			container.RegisterType<ILog, LogToTableAndConsole>();

			return container;
		}

	}
}
