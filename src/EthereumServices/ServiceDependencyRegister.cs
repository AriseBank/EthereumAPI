using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;

namespace EthereumServices
{
	public class ServiceDependencyRegister
	{
		public static IUnityContainer Register(IUnityContainer container)
		{
			container.RegisterType<IContractService, ContractService>();
			container.RegisterType<IPaymentService, PaymentService>();
			container.RegisterType<IEthereumQueueOutService, EthereumQueueOutService>();
			container.RegisterType<IContractQueueService, ContractQueueService>();
			container.RegisterType<IEmailNotifierService, EmailNotifierService>();

			return container;
		}
	}
}
