using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure;

namespace EthereumCore.Azure
{
	public static class CloudConfigurationLoader
	{
		public static T ReadCloudConfiguration<T>() where T : new()
		{
			var properties = typeof(T).GetProperties();
			T result = new T();

			foreach (var property in properties)
			{
				if (property.GetSetMethod() != null)
				{
					var type = property.PropertyType;
					property.SetValue(result, Convert.ChangeType(CloudConfigurationManager.GetSetting(property.Name), type));
				}
			}

			return result;
		}
	}
}
