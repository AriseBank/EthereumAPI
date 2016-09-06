using System.Web.Http;
using Swashbuckle.Application;

namespace EthereumApiSelfHosted
{
	public class SwaggerConfig
	{
		public static void Register(HttpConfiguration config)
		{
			config.EnableSwagger(c =>
			{
				c.SingleApiVersion("v1", "Ethereum.API");
			})
			.EnableSwaggerUi(c =>
			{
			});
		}
	}
}