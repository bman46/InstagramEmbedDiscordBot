using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.WebSocket;
using InstagramApiSharp.Classes;
using Microsoft.Extensions.Configuration;

namespace Instagram_Reels_Bot.Helpers
{
	public class InstagramProcessor
	{
		public static InstagramApiSharp.API.IInstaApi instaApi;

		/// <summary>
		/// Routes the url to the desired method.
		/// </summary>
		/// <param name="url">Link to the post.</param>
		/// <param name="guild">The guild that the message originated from. Used to determine max upload size.</param>
		/// <param name="postIndex">Post number in carousel.</param>
		/// <returns>Instagram processor response with related information.</returns>
		public static async Task<InstagramProcessorResponse> PostRouter(string url, SocketGuild guild, int postIndex = 1)
        {
			return await PostRouter(url, ((int)guild.PremiumTier), postIndex);
        }
		/// <summary>
		/// Routes the url to the desired method.
		/// </summary>
		/// <param name="url">Link to the post.</param>
		/// <param name="tier">Discord nitro tier</param>
		/// <param name="postIndex">Post number in carousel.</param>
		/// <returns>Instagram processor response with related information.</returns>
		public static async Task<InstagramProcessorResponse> PostRouter(string url, int tier, int postIndex = 1)
		{
			Uri link;
			try
			{
				link = new Uri(url);
			}
			catch (System.UriFormatException)
			{
				return new InstagramProcessorResponse("Malformed URL.");
			}
			//Process story
			if (isStory(link))
			{
				return await StoryProcessor(url, tier);
			}
			//TODO Highlights:
			else if (false)
			{
				return new InstagramProcessorResponse("Highlights not implemented.");
			}
			//all others:
			else
			{
				return await PostProcessorAsync(url, postIndex, tier);
			}
		}
		/// <summary>
        /// Decides if a link is a story or not.
        /// </summary>
        /// <param name="url">Link to the content.</param>
        /// <returns>True if the post is a story.</returns>
		private static bool isStory(Uri url)
        {
			//URL Starts with stories
			//https://instagram.com/stories/google/2733780123514124411?utm_source=ig_story_item_share&utm_medium=copy_link
			return url.Segments[1].StartsWith("stories");
        }
		/// <summary>
		/// Processes an Instagram story.
        /// Doesnt work with highlights.
		/// </summary>
		/// <param name="url">Link to the story.</param>
		/// <param name="premiumTier">Discord Nitro tier. For max file upload size.</param>
		/// <returns>Instagram processor response with related information.</returns>
		public static async Task<InstagramProcessorResponse> StoryProcessor(string url, int premiumTier)
		{
			//ensure login:
			InstagramLogin();
			Uri link = new Uri(url);
			string userName = link.Segments[2].Replace("/", "");
			string storyID = link.Segments[3].Replace("/", "");

			//get user:
			var user = await instaApi.UserProcessor.GetUserAsync(userName);
			long userId = user.Value.Pk;
			var stories = await instaApi.StoryProcessor.GetUserStoryAsync(userId);
			if (stories.Value.Items.Count == 0)
			{
				return new InstagramProcessorResponse("No stories exist for that user. (Is the account private?)");
			}
			foreach (var story in stories.Value.Items)
			{
				//find story:
				if (story.Id.Contains(storyID))
				{
					long maxUploadSize = DiscordTools.MaxUploadSize(premiumTier);
					if (story.VideoList.Count > 0)
					{
						//process video:
						string videourl = story.VideoList[0].Uri;
						try
						{
							using (System.Net.WebClient wc = new System.Net.WebClient())
							{
								wc.OpenRead(videourl);
								if (Convert.ToInt64(wc.ResponseHeaders["Content-Length"]) < maxUploadSize)
								{
									byte[] data = wc.DownloadData(videourl);
									if (data.Length < maxUploadSize)
									{
										return new InstagramProcessorResponse(true, "", videourl, url, data);
									}
								}
							}
						}
						catch (Exception e)
						{
							//failback to link to video:
							Console.WriteLine(e);
						}
						return new InstagramProcessorResponse(true, "", videourl, url, null);

					}
					else if (story.ImageList.Count > 0)
					{
						//Image:
						Uri imageUrl = new Uri(story.ImageList[0].Uri);
						using (System.Net.WebClient wc = new System.Net.WebClient())
						{
							wc.OpenRead(imageUrl);
							if (Convert.ToInt64(wc.ResponseHeaders["Content-Length"]) < maxUploadSize)
							{
								byte[] data = wc.DownloadData(imageUrl);
								if (data.Length < maxUploadSize)
								{
									//upload video:
									return new InstagramProcessorResponse(false, "", imageUrl.ToString(), url, data);
								}
								
							}
							return new InstagramProcessorResponse(false, "", imageUrl.ToString(), url, null);
						}
					}

				}
			}
			return new InstagramProcessorResponse("Could not find story.");
		}
		/// <summary>
		/// Processes an Instagram post.
		/// </summary>
		/// <param name="url">Link to the post</param>
		/// <param name="index">Post number in carousel</param>
		/// <param name="premiumTier">Discord Nitro tier. For max file upload size.</param>
		/// <returns>Instagram processor response with related information.</returns>
		public static async Task<InstagramProcessorResponse> PostProcessorAsync(string url, int index, int premiumTier)
        {
			//ensure login:
			InstagramLogin();

			//Arrays start at zero:
			index--;

			//parse url:
			InstagramApiSharp.Classes.IResult<string> mediaId;
			InstagramApiSharp.Classes.IResult<InstagramApiSharp.Classes.Models.InstaMedia> media;
			try
			{
				//parse URL:
				mediaId = await instaApi.MediaProcessor.GetMediaIdFromUrlAsync(new Uri(url));

				//Parse for url:
				media = await instaApi.MediaProcessor.GetMediaByIdAsync(mediaId.Value);
			}
			catch (Exception)
			{
				//Error loading
				return new InstagramProcessorResponse("Error Loading Post.");
			}

			//check for private account:
			if (media.Value == null)
			{
				return new InstagramProcessorResponse("Private Account.");
			}

			string caption = "";
			//check caption value (ensure not null)
			if (media.Value.Caption!=null)
            {
				caption = media.Value.Caption.Text;
			}

			//inject image from carousel:
			if (media.Value.Carousel != null && media.Value.Carousel.Count > 0)
			{
				if (media.Value.Carousel.Count <= index)
				{
					return new InstagramProcessorResponse("Index out of bounds. There is only " + media.Value.Carousel.Count + " Posts.");
				}
				if (media.Value.Carousel[index].Videos.Count > 0)
				{
					var video = media.Value.Carousel[index].Videos[0];
					media.Value.Videos.Add(video);
				}
				else
				{
					var image = media.Value.Carousel[index].Images[0];
					media.Value.Images.Add(image);
				}
			}
			//get upload tier:
			long maxUploadSize = DiscordTools.MaxUploadSize(premiumTier);

			bool isVideo = media.Value.Videos.Count > 0;
			string downloadUrl = "";

			//Video or image:
			if (isVideo)
			{
				//video:
				downloadUrl = media.Value.Videos[0].Uri;
			}
			else
			{
				//Image:
				downloadUrl = media.Value.Images[0].Uri;
			}
			
			//Return downloaded content (if possible):
			try
			{
				using (HttpClient client = new HttpClient())
				{
					client.MaxResponseContentBufferSize = maxUploadSize;
					var response = await client.GetAsync(downloadUrl);
					byte[] data = await response.Content.ReadAsByteArrayAsync();
					//If statement to double check size.
					if (data.Length < maxUploadSize)
					{
						return new InstagramProcessorResponse(isVideo, caption, downloadUrl, url, data);
					}

				}
			}
			catch (HttpRequestException e)
			{
				if (e.Message.Contains("Cannot write more bytes to the buffer than the configured maximum buffer size"))
				{
					//File too big to upload to discord. Just ignore the error.
				}
				else
				{
					//Unexpected error:
					Console.WriteLine("HttpRequestException Error:\n" + e);
				}
			}
			catch (Exception e)
			{
				//Log Error:
				Console.WriteLine(e);
			}
			//Fallback to URL:
			return new InstagramProcessorResponse(true, caption, downloadUrl, url, null);
		}
		/// <summary>
		/// Logs the bot into Instagram if logged out.
		/// </summary>
		public static void InstagramLogin()
		{
			if (instaApi.IsUserAuthenticated)
			{
				//Skip. Already Authenticated.
				return;
			}
			// create the configuration
			var _builder = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile(path: "config.json");

			// build the configuration and assign to _config          
			var config = _builder.Build();
			//set user session
			var userSession = new UserSessionData
			{
				UserName = config["IGUserName"],
				Password = config["IGPassword"]
			};
			instaApi.SetUser(userSession);
			string stateFile;
			if (config["StateFile"] != null && config["StateFile"] != "")
			{
				stateFile = config["StateFile"];
			}
			else
			{
				stateFile = "state.bin";
			}
			try
			{
				// load session file if exists
				if (File.Exists(stateFile))
				{
					Console.WriteLine("Loading state from file");
					using (var fs = File.OpenRead(stateFile))
					{
						// Load state data from file:
						instaApi.LoadStateDataFromStream(fs);
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			// login
			Console.WriteLine($"Logging in as {userSession.UserName}");
			var logInResult = instaApi.LoginAsync().GetAwaiter().GetResult();
			if (!logInResult.Succeeded)
			{
				Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
				return;
			}
			var state = instaApi.GetStateDataAsStream();
			// in .net core or uwp apps don't use GetStateDataAsStream.
			// use this one:
			// var state = _instaApi.GetStateDataAsString ();
			// this returns you session as json string.
			try
			{
				using (var fileStream = File.Create(stateFile))
				{
					state.Seek(0, SeekOrigin.Begin);
					state.CopyTo(fileStream);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Error writing state file. Error: " + e);
			}

		}

	}
}

