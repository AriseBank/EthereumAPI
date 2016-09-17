using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using EthereumApiSelfHosted.Models;
using EthereumCore.Log;
using EthereumServices;

namespace EthereumApiSelfHosted.Controllers
{
	[RoutePrefix("api/system")]
	public class SystemController : ApiController
	{
		private readonly IContractQueueService _contractQueueService;
		private readonly IContractService _contractService;

		public SystemController(IContractQueueService contractQueueService, IContractService contractService)
		{
			_contractQueueService = contractQueueService;
			_contractService = contractService;
		}

		[Route("amialive")]
		[HttpGet]
		public async Task<IHttpActionResult> NewClient()
		{
			// check contract queue
			var count = _contractQueueService.Count();

			// check ethereum node
			var block = await _contractService.GetCurrentBlock();

			return Ok(new { QueueCount = count, BlockNumber = block });
		}
	}
}
