using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EthereumApiSelfHosted;
using EthereumCore;
using EthereumCore.Settings;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Starter;

namespace ApiRunner
{
	class Program
	{
		static void Main(string[] args)
		{
			var arguments = args.Select(t => t.Split('=')).ToDictionary(spl => spl[0].Trim('-'), spl => spl[1]);

			Console.Clear();
			Console.Title = "Ethereum Self-hosted API - Ver. " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

			var settings = GetSettings(arguments);
			if (settings == null)
			{
				Console.WriteLine("Press any key to exit...");
				Console.ReadKey();
				return;
			}

			var services = (ServiceProvider)ServicesFactory.Create();
			services.AddInstance<IBaseSettings>(settings);

			var url = $"http://*:{arguments["port"]}";

			var options = new StartOptions(url);

			var starter = services.GetService<IHostingStarter>();

			using (starter.Start(options))
			{
				Console.WriteLine($"Web Server is running - {url}");
				Console.WriteLine("Press 'q' to quit.");

				while (Console.ReadLine() != "q") continue;
			}
		}

		static BaseSettings GetSettings(Dictionary<string, string> arguments)
		{
			var settingsData = ReadSettingsFile();

			if (string.IsNullOrWhiteSpace(settingsData))
			{
				Console.WriteLine("Please, provide generalsettings.json file");
				return null;
			}


			if (!arguments.ContainsKey("port"))
			{
				Console.WriteLine("Please, specify command line parameters:");
				Console.WriteLine("-port=<port> # port for web server");
				return null;
			}

			var settings = GeneralSettingsReader.ReadSettingsFromData<BaseSettings>(settingsData);

			return settings;
		}


		static string ReadSettingsFile()
		{
#if DEBUG
			return File.ReadAllText(@"..\..\..\..\settings\generalsettings.json");
#else
			return File.ReadAllText("generalsettings.json");
#endif
		}
	}
}
