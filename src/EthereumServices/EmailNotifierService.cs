using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EthereumCore;
using EthereumCore.Azure;
using Newtonsoft.Json;

namespace EthereumServices
{
	public interface IEmailNotifierService
	{
		void Warning(string title, string message);
	}

	public class EmailNotifierService : IEmailNotifierService
	{
		private readonly IQueueExt _queue;

		public EmailNotifierService(Func<string, IQueueExt> queueFactory)
		{
			_queue = queueFactory(Constants.EmailNotifierQueue);
		}

		public void Warning(string title, string message)
		{
			var obj = new
			{
				Data = new
				{
					BroadcastGroup = 100,
					MessageData = new
					{
						Subject = title,
						Text = message
					}
				}
			};

			var str = "PlainTextBroadcast:" + JsonConvert.SerializeObject(obj);

			_queue.PutRawMessage(str);
		}
	}
}
