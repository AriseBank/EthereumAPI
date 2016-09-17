using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Azure;
using EthereumCore;
using EthereumCore.Settings;
using EthereumServices;
using Newtonsoft.Json;

namespace EhtereumContractBuilder
{
	class Program
	{
		static void Main(string[] args)
		{
			var exit = false;

			while (!exit)
			{
				Console.WriteLine("Choose number: ");
				Console.WriteLine("1. Deploy main contract from local json file");
				Console.WriteLine("0. Exit");

				var input = Console.ReadLine();

				switch (input)
				{
					case "1":
						DeployMainContractLocal().Wait();
						break;
					//case "2":
					//	CreatePaymentEvent().Wait();
					//	break;
					case "0":
						exit = true;
						break;
					default:
						Console.WriteLine("Bad input!");
						continue;
				}

				Console.WriteLine("Done!");
			}
		}
		
		static void UpdateMainContract()
		{
			var contractAbi = GetFileContent("MainContract.abi");
			var contractByteCode = GetFileContent("MainContract.bin");

			var settings = GetCurrentSettings();

			settings.MainContract.Abi = contractAbi;
			settings.MainContract.ByteCode = contractByteCode;

			SaveSettings(settings);
		}

		static void UpdateUserContract()
		{
			var contractAbi = GetFileContent("UserContract.abi");
			var contractByteCode = GetFileContent("UserContract.bin");

			var settings = GetCurrentSettings();

			settings.UserContract.Abi = contractAbi;
			settings.UserContract.ByteCode = contractByteCode;

			SaveSettings(settings);
		}

		
		static async Task DeployMainContractLocal()
		{
			Console.WriteLine("Begin contract deployment process");
			try
			{
				var json = File.ReadAllText("generalsettings.json");
				var settings = JsonConvert.DeserializeObject<BaseSettings>(json);
				string contractAddress = await new ContractService(settings).GenerateMainContract();

				settings.EthereumMainContractAddress = contractAddress;
				Console.WriteLine("New contract: " + contractAddress);

				File.WriteAllText("generalsettings.json", JsonConvert.SerializeObject(settings, Formatting.Indented));

				Console.WriteLine("Contract address stored in generalsettings.json file");
			}
			catch (Exception e)
			{
				Console.WriteLine("Action failed!");
				Console.WriteLine(e.Message);
			}
		}

		static BaseSettings GetCurrentSettings()
		{
			return GeneralSettingsReader.ReadGeneralSettings<BaseSettings>(ConfigurationManager.AppSettings["ConnectionString"]);
		}

		static void SaveSettings(BaseSettings settings)
		{
			SaveSettingsString(JsonConvert.SerializeObject(settings));
		}

		static void SaveSettingsString(string settings)
		{
			var settingsStorage = new AzureBlobStorage(ConfigurationManager.AppSettings["ConnectionString"]);

			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(settings);
			writer.Flush();
			stream.Position = 0;

			settingsStorage.SaveBlob("settings", "generalsettings.json", stream);
		}

		static string GetFileContent(string fileName)
		{
			return File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "contracts", "bin", fileName));
		}
	}
}
