using Spectre.Console;

namespace Bunkr_DL.Helpers
{
	public interface IDownloadHelper
	{
		Task DownloadFile(string path, string filename, string url, HttpClient client, ProgressTask progressTask);
	}
}