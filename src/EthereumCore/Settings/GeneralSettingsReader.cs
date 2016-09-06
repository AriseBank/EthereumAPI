using System.Text;
using EthereumCore.Azure;

namespace EthereumCore.Settings
{
	public static class GeneralSettingsReader
	{
		public static T ReadGeneralSettings<T>(string connectionString)
		{
			var settingsStorage = new AzureBlobStorage(connectionString);
			var settingsData = settingsStorage.GetAsync("settings", "generalsettings.json").Result.ToBytes();
			var str = Encoding.UTF8.GetString(settingsData);

			return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
		}

		public static T ReadSettingsFromData<T>(string jsonData)
		{
			return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonData);
		}
	}
}
