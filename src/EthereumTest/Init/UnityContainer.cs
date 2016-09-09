using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EthereumCore;
using EthereumCore.Azure;
using EthereumCore.Log;
using EthereumCore.Settings;
using EthereumServices;
using Microsoft.Practices.Unity;

namespace EthereumTest.Init
{
	public class UnityConfig
	{
		#region Unity Container
		private static readonly Lazy<IUnityContainer> Container = new Lazy<IUnityContainer>(() =>
		{
			var container = new UnityContainer();
			RegisterTypes(container);
			return container;
		});

		/// <summary>
		/// Gets the configured Unity container.
		/// </summary>
		public static IUnityContainer GetConfiguredContainer()
		{
			return Container.Value;
		}
		#endregion

		public static void RegisterTypes(IUnityContainer container)
		{
			var file = File.ReadAllText(@"..\..\..\..\settings\generalsettings.json");
			var settings = GeneralSettingsReader.ReadSettingsFromData<BaseSettings>(file);

			container.RegisterInstance<IBaseSettings>(settings);

			var log = new LogToTable(
				new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, "TestError", null),
				new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, "TestWarning", null),
				new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, "TestInfo", null));

			container.RegisterInstance<ILog>(log);
			
			container.RegisterType<IContractService, ContractService>();
			container.RegisterType<IPaymentService, PaymentService>();
			container.RegisterType<IEthereumQueueOutService, EthereumQueueOutService>();

		}
	}
}
