using Bunkr_DL.Entities;
using HtmlAgilityPack;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using File = Bunkr_DL.Entities.File;

namespace Bunkr_DL.Helpers
{
	public class APIHelper : IAPIHelper
	{
		public async Task<Album> GetAlbum(string albumURL)
		{
			try
			{
				List<string> urls = new List<string>();
				Album album = new Album();
				Uri albumURI = new Uri(albumURL);
				HttpClient client = new HttpClient();
				HttpRequestMessage request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = albumURI
				};
				using (var response = await client.SendAsync(request))
				{
					response.EnsureSuccessStatusCode();
					string body = await response.Content.ReadAsStringAsync();

					var doc = new HtmlDocument();
					doc.LoadHtml(body);
					HtmlNode titleElement = doc.DocumentNode.SelectSingleNode("//h1[@class='text-[24px] font-bold text-dark dark:text-white']");
					if (titleElement != null)
					{
						album.Title = SanitizeAlbumName(titleElement.InnerHtml);
					}

					HtmlNodeCollection files = doc.DocumentNode.SelectNodes("//a[contains(@class, 'grid-images_box-link')]");
					foreach (HtmlNode file in files)
					{
						urls.Add($"https://{albumURI.Host}{file.GetAttributeValue("href", "")}");
					}
				}

				if(urls != null && urls.Count > 0)
				{
					foreach (string url in urls)
					{
						Uri uri = new Uri(url);
						if (uri.Segments.Contains("v/"))
						{
							string cdnUrl = await GetVideoCDNURL(url);
							if(!string.IsNullOrEmpty(cdnUrl))
							{
								album.Files.Add(new Entities.File
                                {
									CDNUrl = cdnUrl,
									FileName = GetFileName(cdnUrl),
									Size = await GetSize(cdnUrl)
								});
							}
						}
						else
						{
							string cdnUrl = await GetOtherCDNURL(url);
							if (!string.IsNullOrEmpty(cdnUrl))
							{
								album.Files.Add(new Entities.File
								{
									CDNUrl = cdnUrl,
									FileName = GetFileName(cdnUrl),
									Size = await GetSize(cdnUrl)
								});
							}
						}
					}
				}

				return album;
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
			return null;
		}

		public async Task<string> GetVideoCDNURL(string url)
		{
			try
			{
				HttpClient client = new HttpClient();
				HttpRequestMessage request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri(url)
				};
				using (var response = await client.SendAsync(request))
				{
					response.EnsureSuccessStatusCode();
					string body = await response.Content.ReadAsStringAsync();

					var doc = new HtmlDocument();
					doc.LoadHtml(body);
					HtmlNode cdnHref = doc.DocumentNode.SelectSingleNode("//a[contains(@class, 'bg-blue-500')]");
					if (cdnHref != null)
					{
						return cdnHref.GetAttributeValue("href", "");
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
			return string.Empty;
		}
		public async Task<string> GetOtherCDNURL(string url)
		{
			try
			{
				HttpClient client = new HttpClient();
				HttpRequestMessage request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri(url)
				};
				using (var response = await client.SendAsync(request))
				{
					response.EnsureSuccessStatusCode();
					string body = await response.Content.ReadAsStringAsync();

					var doc = new HtmlDocument();
					doc.LoadHtml(body);
					HtmlNode cdnHref = doc.DocumentNode.SelectSingleNode("//a[contains(@class, 'text-white') and contains(@class, 'inline-flex')]");
					if (cdnHref != null)
					{
						return cdnHref.GetAttributeValue("href", "");
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
			return string.Empty;
		}
		public static string SanitizeAlbumName(string title)
		{
			title = title.Replace("\n", "").Trim();
			title = title.Replace("\t", "").Trim();
			title = Regex.Replace(title, " +", " ");
			title = Regex.Replace(title, @"[\\*?:""<>|./]", "-");

			return title;
		}

		public static string GetFileName(string url)
		{
			return Path.GetFileName(new Uri(url).LocalPath);
		}

		public async Task<long> GetSize(string url)
		{
			long fileSize = 0;
			using HttpClient client = new();

			using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
			if (response.IsSuccessStatusCode)
			{
				fileSize = response.Content.Headers.ContentLength ?? 0;
			}

			return fileSize;
		}
	}
}
