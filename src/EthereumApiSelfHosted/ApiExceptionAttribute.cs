using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using EthereumCore.Exceptions;
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

			ApiException ex;

			var exception = context.Exception as BackendException;
			if (exception != null)
			{
				ex = new ApiException
				{
					Error = new ApiError
					{
						Code = exception.Type,
						Message = exception.Message
					}
				};
			}
			else
			{
				_logger.WriteError("ApiException", "EthereumApi", $"Controller: {controller}, action: {action}", context.Exception);
				ex = new ApiException
				{
					Error = new ApiError
					{
						Code = BackendExceptionType.None,
						Message = "Internal server error. Try again."
					}
				};
			}

			
			var errorResponse =
			   context.Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
			context.Response = errorResponse;

			base.OnException(context);
		}
	}

	public class ApiException
	{
		public ApiError Error { get; set; }
	}

	public class ApiError
	{
		public BackendExceptionType Code { get; set; }
		public string Message { get; set; }
	}
}
