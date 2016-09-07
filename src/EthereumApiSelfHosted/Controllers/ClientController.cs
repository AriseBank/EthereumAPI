using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using EthereumApiSelfHosted.Models;
using EthereumServices;

namespace EthereumApiSelfHosted.Controllers
{
	[RoutePrefix("api/client")]
	public class ClientController : ApiController
	{
		private readonly IContractQueueService _contractQueueService;

		public ClientController(IContractQueueService contractQueueService)
		{
			_contractQueueService = contractQueueService;
		}

		[Route("register")]
		[HttpPost]
		[ResponseType(typeof(RegisterResponse))]
		public async Task<IHttpActionResult> NewClient()
		{
			var contract = await _contractQueueService.GetContract();

			var response = new RegisterResponse
			{
				Contract = contract
			};

			return Ok(response);
		}
	}
}
