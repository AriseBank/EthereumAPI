using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using EthereumCore.Log;

namespace EthereumApiSelfHosted
{
	public class ApiExceptionAttribute : ExceptionFilterAttribute
	{
		private readonly ILog _logger;

		public ApiExceptionAttribute(ILog logger)
		{
			_logger = logger;
		}

		public override void OnException(HttpActionExecutedContext context)
		{
			var controller = context.ActionContext?.ControllerContext?.ControllerDescriptor?.ControllerName;
			var action = context.ActionContext?.ActionDescriptor?.ActionName;

			_logger.WriteError("ApiException", "EthereumApi", $"Controller: {controller}, action: {action}", context.Exception);

			var apiError = new ApiException()
			{
				Message = "Internal server error. Try again."
			};

			var errorResponse =
			   context.Request.CreateResponse(HttpStatusCode.InternalServerError, apiError);
			context.Response = errorResponse;

			base.OnException(context);
		}
	}

	public class ApiException
	{
		public string Message { get; set; }
	}
}
