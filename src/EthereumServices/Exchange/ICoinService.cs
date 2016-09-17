using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EthereumServices.Exchange
{
	public interface ICoinService
	{
		Task<string> Cashin(string address, BigInteger amount);

		Task<string> Cashout(string addressFrom, BigInteger amount, string addressTo);
	}
}
