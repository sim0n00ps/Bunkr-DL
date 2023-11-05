using Bunkr_DL.Entities;
using Bunkr_DL.Helpers;
using Spectre.Console;
using System;
using System.Text.RegularExpressions;

namespace Bunkr_DL
{
	public class Program
	{
		private static readonly IAPIHelper aPIHelper;
		private static readonly IDownloadHelper downloadHelper;
		static Program()
		{
			aPIHelper = new APIHelper();
			downloadHelper = new DownloadHelper();
		}
		public static async Task Main()
		{
			do
			{
				var mainMenuOptions = GetMainMenuOptions();

				var mainMenuSelection = AnsiConsole.Prompt(
					new SelectionPrompt<string>()
						.Title("[red]What would you like to do? | Download From Album - Download content from an album | Download Single File - Download a single file e.g a video or zip file[/]")
						.AddChoices(mainMenuOptions)
				);

				switch (mainMenuSelection)
				{
					case "[red]Download From Album[/]":
						await AlbumDownload();
						break;
					case "[red]Download Single File[/]":
						await SingleDownload();
						break;
					case "[red]Exit[/]":
						Environment.Exit(0);
						break;
				}
			} while (true);
		}

		public static async Task AlbumDownload()
		{
			try
			{
				string albumUrl = AnsiConsole.Prompt(
					new TextPrompt<string>("[red]Please enter a Bunkr Album URL: [/]")
						.ValidationErrorMessage("[red]Please enter a valid Bunkr Album URL[/]")
						.Validate(url =>
						{
							Regex regex = new Regex("https://bunkrr\\.su/a/[A-Za-z0-9]+", RegexOptions.IgnoreCase);
							if (regex.IsMatch(url))
							{
								return ValidationResult.Success();
							}
							return ValidationResult.Error("[red]Please enter a valid Bunkr Album URL[/]");
						}));

				Album album = await aPIHelper.GetAlbum(albumUrl);

				if(album != null && album.Files.Count > 0)
				{
					string folder = $"Downloads/{album.Title}";
					if (!Path.Exists(folder))
					{
						Directory.CreateDirectory(folder);
					}
					foreach (Entities.File file in album.Files)
					{
						await AnsiConsole.Progress()
						.Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new DownloadedColumn(), new RemainingTimeColumn())
						.StartAsync(async ctx =>
						{
							var downloadTask = ctx.AddTask($"[red]{Markup.Escape(folder)}/{Markup.Escape(file.FileName)}[/]");
							downloadTask.MaxValue = file.Size;

							bool downloadSuccessful = false;

							do
							{
								await downloadHelper.DownloadFile(folder, file.FileName, file.CDNUrl, new HttpClient(), downloadTask);

								downloadSuccessful = true;

							} while (!downloadSuccessful);
						});
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
		public static async Task SingleDownload()
		{
			try
			{
				string fileUrl = AnsiConsole.Prompt(
					new TextPrompt<string>("[red]Please enter a Bunkr URL: [/]")
						.ValidationErrorMessage("[red]Please enter a valid Bunkr URL[/]")
						.Validate(url =>
						{
							Regex regex = new Regex("https://bunkrr\\.su/[a-z]/[A-Za-z0-9]+", RegexOptions.IgnoreCase);
							if (regex.IsMatch(url))
							{
								return ValidationResult.Success();
							}
							return ValidationResult.Error("[red]Please enter a valid Bunkr URL[/]");
						}));

				Uri uri = new Uri(fileUrl);

				if (uri.Segments.Contains("v/"))
				{
					string cdnURL = await aPIHelper.GetVideoCDNURL(fileUrl);
					string filename = System.IO.Path.GetFileName(new Uri(cdnURL).LocalPath);
					await AnsiConsole.Progress()
						.Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new DownloadedColumn(), new RemainingTimeColumn())
						.StartAsync(async ctx =>
						{
							var downloadTask = ctx.AddTask($"[red]{Markup.Escape(filename)}[/]");
							downloadTask.MaxValue = await aPIHelper.GetSize(cdnURL);

							bool downloadSuccessful = false;

							do
							{
								await downloadHelper.DownloadFile("Downloads", filename, cdnURL, new HttpClient(), downloadTask);

								downloadSuccessful = true;

							} while (!downloadSuccessful);
						});
				}
				else
				{
					string cdnURL = await aPIHelper.GetOtherCDNURL(fileUrl);
					string filename = System.IO.Path.GetFileName(uri.LocalPath);
					await AnsiConsole.Progress()
						.Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new DownloadedColumn(), new RemainingTimeColumn())
						.StartAsync(async ctx =>
						{
							var downloadTask = ctx.AddTask($"[red]{Markup.Escape(filename)}[/]");
							downloadTask.MaxValue = await aPIHelper.GetSize(cdnURL);

							bool downloadSuccessful = false;

							do
							{
								await downloadHelper.DownloadFile("Downloads", filename, cdnURL, new HttpClient(), downloadTask);

								downloadSuccessful = true;

							} while (!downloadSuccessful);
						});
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

		public static List<string> GetMainMenuOptions()
		{
			return new List<string>
			{
				"[red]Download From Album[/]",
				"[red]Download Single File[/]",
				"[red]Exit[/]"
			};
		}
	}
}