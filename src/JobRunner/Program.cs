using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EthereumCore;
using EthereumCore.Settings;
using EthereumWebJob.Job;
using Microsoft.Azure.WebJobs;

namespace JobRunner
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Clear();
			Console.Title = "Ethereum Web Job - Ver. " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

			var settings = GetSettings();
			if (settings == null)
			{
				Console.WriteLine("Press any key to exit...");
				Console.ReadKey();
				return;
			}

			var container = new UnityConfig();
			container.InitContainer(settings);

			var config = new JobHostConfiguration
			{
				JobActivator = container,
				StorageConnectionString = settings.Db.JobStorageConnString,
				DashboardConnectionString = settings.Db.JobStorageConnString,
				Tracing = { ConsoleLevel = TraceLevel.Error }
			};

#if DEBUG
			config.HostId = "developmenthost";
			config.UseDevelopmentSettings();
#endif
			config.UseTimers();

			Console.WriteLine("Web job started");

			var host = new JobHost(config);
			host.RunAndBlock();
		}

		static BaseSettings GetSettings()
		{
			var settingsData = ReadSettingsFile();

			if (string.IsNullOrWhiteSpace(settingsData))
			{
				Console.WriteLine("Please, provide generalsettings.json file");
				return null;
			}

			BaseSettings settings = GeneralSettingsReader.ReadSettingsFromData<BaseSettings>(settingsData);
			
			return settings;
		}

		static string ReadSettingsFile()
		{
			try
			{
#if DEBUG
				return File.ReadAllText(@"..\..\..\..\settings\generalsettings.json");
#else
				return File.ReadAllText("generalsettings.json");
#endif
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
