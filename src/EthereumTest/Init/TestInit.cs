using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EthereumTest.Init;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nethereum.RPC;

namespace EthereumTest
{
	[TestClass]
	public class TestInit
	{
		[AssemblyInitialize]
		public static void SetUp(TestContext context)
		{
			UnityConfig.GetConfiguredContainer();
		}

		[AssemblyCleanup]
		public static void TearDown()
		{
			UnityConfig.GetConfiguredContainer().Dispose();
		}
	}
}
