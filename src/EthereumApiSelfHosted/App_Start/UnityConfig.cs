using System;
using AzureStorage;
using AzureStorage.Azure;
using AzureStorage.Log;
using EthereumCore;
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

			AzureDependencyRegister.RegisterLogs(container, settings, "Api");
			AzureDependencyRegister.RegisterStorage(container, settings);
			AzureDependencyRegister.RegisterQueues(container, settings);

			ServiceDependencyRegister.Register(container);

			return container;
		}
	}
}