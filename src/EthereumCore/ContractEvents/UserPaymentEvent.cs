using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace EthereumCore.ContractEvents
{
	public class UserPaymentEvent
	{
		[Parameter("address", "userAddress", 1, true)]
		public string Address { get; set; }

		[Parameter("uint", "amount", 2, false)]
		public BigInteger Amount { get; set; }
	}
}
