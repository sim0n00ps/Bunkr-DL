using Bunkr_DL.Entities;

namespace Bunkr_DL.Helpers
{
	public interface IAPIHelper
	{
		Task<Album> GetAlbum(string albumURL);
		Task<string> GetVideoCDNURL(string url);
		Task<string> GetOtherCDNURL(string url);
		Task<long> GetSize(string url);
	}
}