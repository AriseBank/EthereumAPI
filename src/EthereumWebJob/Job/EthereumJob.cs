using System;
using System.Threading.Tasks;
using EthereumCore.ContractEvents;
using EthereumCore.Log;
using EthereumCore.Settings;
using EthereumServices;
using Microsoft.Azure.WebJobs;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;

namespace EthereumWebJob.Job
{
	public class EthereumJob
	{
		private const int ContractsPerRequest = 50;

		private readonly IContractService _contractService;
		private readonly IPaymentService _paymentService;
		private readonly IEthereumQueueOutService _queueOutService;
		private readonly IContractQueueService _contractQueueService;
		private readonly IEmailNotifierService _emailNotifier;
		private readonly IBaseSettings _settings;
		private readonly ILog _logger;

		private bool _isProcessing;
		private HexBigInteger _filter;

		public EthereumJob(IContractService contractService,
									IPaymentService paymentService,
									IEthereumQueueOutService queueOutService,
									IContractQueueService contractQueueService,
									IEmailNotifierService emailNotifier,
									IBaseSettings settings,
									ILog logger)
		{
			_contractService = contractService;
			_paymentService = paymentService;
			_logger = logger;
			_queueOutService = queueOutService;
			_contractQueueService = contractQueueService;
			_emailNotifier = emailNotifier;
			_settings = settings;
		}

		/// <summary>
		/// Check ethereum main account balance
		/// </summary>
		/// <param name="timer"></param>
		/// <returns></returns>
		public async Task CheckMainAccountBalance([TimerTrigger("00:30:00", RunOnStartup = true)] TimerInfo timer)
		{
			await InternalBalanceCheck();
		}

		/// <summary>
		/// Refreshe contracts queue every 12 hours
		/// </summary>
		/// <param name="timer"></param>
		/// <returns></returns>
		public async Task RefreshQueue([TimerTrigger("12:00:00")] TimerInfo timer)
		{
			for (var i = 0; i < _contractQueueService.Count(); i++)
			{
				try
				{
					var contract = await _contractQueueService.GetContract();
					await _contractQueueService.PushContract(contract);
				}
				catch (Exception e)
				{
					await _logger.WriteError("EthereumWebJob", "RefreshQueue", "", e);
				}
			}
		}

		/// <summary>
		/// Increase user contract pool length
		/// </summary>
		/// <param name="timer"></param>
		/// <returns></returns>
		public async Task CheckContractQueueCount([TimerTrigger("00:00:05", RunOnStartup = true)] TimerInfo timer)
		{
			try
			{
				if (_contractQueueService.Count() < _settings.MinContractPoolLength)
				{
					for (var i = 0; i < (_settings.MaxContractPoolLength - _settings.MinContractPoolLength) / ContractsPerRequest; i++)
					{
						await InternalBalanceCheck();

						var contracts = await _contractService.GenerateUserContracts(ContractsPerRequest);
						foreach (var contract in contracts)
							await _contractQueueService.PushContract(contract);
					}
				}
			}
			catch (Exception e)
			{
				await _logger.WriteError("EthereumWebJob", "CheckContractQueueCount", "", e);
			}
		}

		/// <summary>
		/// Check user payment event
		/// </summary>
		/// <param name="timer"></param>
		/// <returns></returns>
		public async Task CheckPaymentToUserContracts([TimerTrigger("00:00:05", RunOnStartup = true)] TimerInfo timer)
		{
			if (_isProcessing)
				return;

			_isProcessing = true;

			try
			{
				if (_filter == null)
					_filter = await _contractService.CreateFilterEventForUserContractPayment();


				var logs = await _contractService.GetNewPaymentEvents(_filter);

				if (logs == null)
					return;

				foreach (var item in logs)
				{
					try
					{
						await ProcessLogItem(item);
					}
					catch (Exception e)
					{
						Console.WriteLine($"Unhandled exception if thrown in CheckPaymentToUserContract: {e.Message}");
					}
				}

			}
			catch (Exception e)
			{
				await _logger.WriteError("EthereumWebJob", "CheckPaymentToUserContracts", "", e);
			}
			finally
			{
				_isProcessing = false;
			}
		}

		/// <summary>
		/// Process one payment event. Try to transfer from contract to main account (if failed, then it is duplicated event)
		/// </summary>
		/// <param name="log"></param>
		/// <returns></returns>
		private async Task<bool> ProcessLogItem(UserPaymentEvent log)
		{
			try
			{
				await _logger.WriteInfo("EthereumWebJob", "ProcessLogItem", "", $"Start proces: event from {log.Address} for {log.Amount} WEI.");

				var transaction = await _paymentService.TransferFromUserContract(log.Address, log.Amount);

				await _logger.WriteInfo("EthereumWebJob", "ProcessLogItem", "", $"Finish process: Event from {log.Address} for {log.Amount} WEI. Transaction: {transaction}");

				await _queueOutService.FirePaymentEvent(log.Address, UnitConversion.Convert.FromWei(log.Amount));

				await _logger.WriteInfo("EthereumWebJob", "ProcessLogItem", "", $"Message sended to queue: Event from {log.Address}");

				Console.WriteLine($"Event from {log.Address} for {log.Amount} WEI processed! Transaction: {transaction}");

				return true;
			}
			catch (Exception e)
			{
				await _logger.WriteError("EthereumWebJob", "ProcessLogItem", "Failed to process item", e);
			}

			return false;
		}

		private async Task InternalBalanceCheck()
		{
			try
			{
				var balance = await _paymentService.GetMainAccountBalance();
				if (balance < _settings.MainAccountMinBalance)
					_emailNotifier.Warning("Ethereum worker",
						$"Main account {_settings.EthereumMainAccount} balance is less that {_settings.MainAccountMinBalance} ETH !");
			}
			catch (Exception e)
			{
				await _logger.WriteError("EthereumWebJob", "InternalBalanceCheck", "", e);
			}
		}
	}
}
