using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzureStorage.Azure
{
	public interface IQueueExt
	{
		Task PutRawMessage(string message);
		Task<string> Pop();
		int? Count();
	}

	public class AzureQueueExt : IQueueExt
	{
		private readonly CloudQueue _queue;

		public AzureQueueExt(string conectionString, string queueName)
		{
			queueName = queueName.ToLower();
			var storageAccount = CloudStorageAccount.Parse(conectionString);
			var queueClient = storageAccount.CreateCloudQueueClient();

			_queue = queueClient.GetQueueReference(queueName);
			_queue.CreateIfNotExists();
		}


		public Task PutRawMessage(string message)
		{
			return _queue.AddMessageAsync(new CloudQueueMessage(message));
		}

		public async Task<string> Pop()
		{
			var mesage = await _queue.GetMessageAsync();

			if (mesage == null)
				return null;

			await _queue.DeleteMessageAsync(mesage);

			return mesage.AsString;
		}

		public int? Count()
		{
			_queue.FetchAttributes();
			return _queue.ApproximateMessageCount;
		}
	}
}
