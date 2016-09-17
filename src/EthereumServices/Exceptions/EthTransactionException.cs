using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EthereumServices.Exceptions
{
	public class EthTransactionException : Exception
	{
		public string Error { get; private set; }
		public string Transaction { get; private set; }

		public EthTransactionException(string error, string trHash)
		{
			Error = error;
			Transaction = trHash;
		}
	}
}
