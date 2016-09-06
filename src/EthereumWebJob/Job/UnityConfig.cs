using System;
using EthereumCore;
using EthereumCore.Azure;
using EthereumCore.Log;
using EthereumCore.Settings;
using EthereumServices;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Practices.Unity;

namespace EthereumWebJob.Job
{
	public class UnityConfig : IJobActivator
	{
		private readonly IUnityContainer _container;

		public UnityConfig()
		{
			_container = new UnityContainer();
		}

		public void InitContainer(IBaseSettings settings)
		{
			_container.RegisterInstance(settings);

			var logToTable = new LogToTable(new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, "EthereumLogApi", null));
			_container.RegisterInstance(logToTable);
			_container.RegisterType<LogToConsole>();

			_container.RegisterType<ILog, LogToTableAndConsole>();

			_container.RegisterType<IContractService, ContractService>();
			_container.RegisterType<IPaymentService, PaymentService>();
			_container.RegisterType<IEthereumQueueOutService, EthereumQueueOutService>();
			_container.RegisterType<IContractQueueService, ContractQueueService>();
			_container.RegisterType<IEmailNotifierService, EmailNotifierService>();

			_container.RegisterType<Func<string, IQueueExt>>(new InjectionFactory(c =>
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

			_container.RegisterType<EthereumJob>(new ContainerControlledLifetimeManager());
		}

		public T CreateInstance<T>()
		{
			return _container.Resolve<T>();
		}
	}
}
