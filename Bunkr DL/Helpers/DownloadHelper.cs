using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkr_DL.Helpers
{
	public class DownloadHelper : IDownloadHelper
	{
		public async Task DownloadFile(string path, string filename, string url, HttpClient client, ProgressTask progressTask)
		{
			try
			{
				using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
				{
					response.EnsureSuccessStatusCode();

					using (var contentStream = await response.Content.ReadAsStreamAsync())
					using (var fileStream = new FileStream(path + "/" + filename, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
					{
						var buffer = new byte[8192];
						while (true)
						{
							var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
							if (read == 0)
							{
								break;
							}

							progressTask.Increment(read);

							await fileStream.WriteAsync(buffer, 0, read);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception caught: {0}\n\nStackTrace: {1}", ex.Message, ex.StackTrace);

				if (ex.InnerException != null)
				{
					Console.WriteLine("\nInner Exception:");
					Console.WriteLine("Exception caught: {0}\n\nStackTrace: {1}", ex.InnerException.Message, ex.InnerException.StackTrace);
				}
			}
		}
	}
}
