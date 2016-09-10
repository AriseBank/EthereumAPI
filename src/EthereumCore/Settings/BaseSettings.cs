﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EthereumCore.Settings
{
	public interface IBaseSettings
	{
		EthereumContract MainContract { get; set; }
		EthereumContract UserContract { get; set; }

		string EthereumPrivateAccount { get; set; }
		string EthereumMainAccount { get; set; }
		string EthereumMainAccountPassword { get; set; }

		string EthereumMainContractAddress { get; set; }

		string EthereumUrl { get; set; }

		DbSettings Db { get; set; }

		int MinContractPoolLength { get; set; }
		int MaxContractPoolLength { get; set; }
		decimal MainAccountMinBalance { get; set; }
	}

	public class BaseSettings : IBaseSettings
	{
		public EthereumContract MainContract { get; set; }
		public EthereumContract UserContract { get; set; }

		public string EthereumPrivateAccount { get; set; }

		public string EthereumMainAccount { get; set; }
		public string EthereumMainAccountPassword { get; set; }

		/// <summary>
		/// Ethereum main contract (which fires event) address
		/// </summary>
		public string EthereumMainContractAddress { get; set; }

		/// <summary>
		/// Ethereum geth URL
		/// </summary>
		public string EthereumUrl { get; set; }

		public DbSettings Db { get; set; }

		public int MinContractPoolLength { get; set; } = 100;
		public int MaxContractPoolLength { get; set; } = 200;
		public decimal MainAccountMinBalance { get; set; } = 1.0m;
	}

	public class EthereumContract
	{
		public string Abi { get; set; }
		public string ByteCode { get; set; }
	}

	public class DbSettings
	{
		public string DataConnString { get; set; }
		public string LogsConnString { get; set; }

		public string ExchangeQueueConnString { get; set; }
	}
}
