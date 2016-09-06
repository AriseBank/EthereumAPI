using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EthereumCore
{
	public static class Utils
	{
		public static IEnumerable<IEnumerable<T>> ToPieces<T>(this IEnumerable<T> src, int countInPicese)
		{
			var result = new List<T>();

			foreach (var itm in src)
			{
				result.Add(itm);
				if (result.Count >= countInPicese)
				{
					yield return result;
					result = new List<T>();
				}
			}

			if (result.Count > 0)
				yield return result;
		}

		public static byte[] ToBytes(this Stream src)
		{
			var memoryStream = src as MemoryStream;

			if (memoryStream != null)
				return memoryStream.ToArray();


			src.Position = 0;
			var result = new MemoryStream();

			src.CopyTo(result);
			return result.ToArray();
		}
	}
}
