using System.Web.Http;
using EthereumCore.Log;

namespace EthereumApiSelfHosted
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
			// Web API configuration and services
	        var log = (ILog) config.DependencyResolver.GetService(typeof (ILog));
			config.Filters.Add(new ApiExceptionAttribute(log));

			// Web API routes
			config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
