using System.Web.Http;
using EthereumApiSelfHosted;
using EthereumCore.Settings;
using Microsoft.Owin;
using Owin;
using Unity.WebApi;

[assembly: OwinStartup(typeof(Startup))]

namespace EthereumApiSelfHosted
{
	public class Startup
	{
		private readonly IBaseSettings _settings;

		public Startup(IBaseSettings settings)
		{
			_settings = settings;
		}

		public void Configuration(IAppBuilder app)
		{
			var configuration = new HttpConfiguration();
			
			var container = UnityConfig.RegisterComponents(_settings);
			configuration.DependencyResolver = new UnityDependencyResolver(container);

			SwaggerConfig.Register(configuration);

			WebApiConfig.Register(configuration);

			app.UseWebApi(configuration);
		}
	}
}
