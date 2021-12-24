using System;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Instagram_Reels_Bot.Helpers
{
	public class InstagramProcessor
	{
		/// <summary>
        /// Routes the url to the desired method.
        /// </summary>
        /// <returns>
        /// The Instagram post output.
        /// </returns>
		public static async Task<InstagramProcessorResponse> PostRouter(string url, SocketGuild guild, int postIndex = 1)
        {
			return await PostRouter(url, ((int)guild.PremiumTier), postIndex);
        }
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
		private static bool isStory(Uri url)
        {
			//URL Starts with stories
			//https://instagram.com/stories/google/2733780123514124411?utm_source=ig_story_item_share&utm_medium=copy_link
			return url.Segments[1].StartsWith("stories");
        }
		public static async Task<InstagramProcessorResponse> StoryProcessor(string url, int premiumTier)
		{
			//ensure login:
			Program.InstagramLogin();
			Uri link = new Uri(url);
			string userName = link.Segments[2].Replace("/", "");
			string storyID = link.Segments[3].Replace("/", "");

			//get user:
			var user = await Program.instaApi.UserProcessor.GetUserAsync(userName);
			long userId = user.Value.Pk;
			var stories = await Program.instaApi.StoryProcessor.GetUserStoryAsync(userId);
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
							//TODO: support nitro uploads:
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
			public static async Task<InstagramProcessorResponse> PostProcessorAsync(string url, int index, int premiumTier)
        {
			//ensure login:
			Program.InstagramLogin();

			//Arrays start at zero:
			index--;

			//parse url:
			InstagramApiSharp.Classes.IResult<string> mediaId;
			InstagramApiSharp.Classes.IResult<InstagramApiSharp.Classes.Models.InstaMedia> media;
			try
			{
				//parse URL:
				mediaId = await Program.instaApi.MediaProcessor.GetMediaIdFromUrlAsync(new Uri(url));

				//Parse for url:
				media = await Program.instaApi.MediaProcessor.GetMediaByIdAsync(mediaId.Value);
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
			//check video:
			if (media.Value.Videos.Count > 0)
			{
				string videourl = media.Value.Videos[0].Uri;
				try
				{
					using (System.Net.WebClient wc = new System.Net.WebClient())
					{
						wc.OpenRead(videourl);
						//TODO: support nitro uploads:
						if (Convert.ToInt64(wc.ResponseHeaders["Content-Length"]) < maxUploadSize)
						{
							byte[] data = wc.DownloadData(videourl);
							if (data.Length < maxUploadSize)
							{
								//upload video:
								return new InstagramProcessorResponse(true, media.Value.Caption.Text, videourl, url, data);
							}
							
						}
					}
				}
				catch (Exception e)
				{
					//Log Error:
					Console.WriteLine(e);
				}
				return new InstagramProcessorResponse(true, media.Value.Caption.Text, videourl, url, null);
			}
			else
			{
				//Image:
				Uri imageUrl = new Uri(media.Value.Images[0].Uri);
				using (System.Net.WebClient wc = new System.Net.WebClient())
				{
					wc.OpenRead(imageUrl);
					//TODO: support nitro uploads:
					if (Convert.ToInt64(wc.ResponseHeaders["Content-Length"]) < maxUploadSize)
					{
						byte[] data = wc.DownloadData(imageUrl);
						if (data.Length < maxUploadSize)
						{
							//upload video:
							return new InstagramProcessorResponse(false, media.Value.Caption.Text, imageUrl.ToString(), url, data);
						}
						
					}
					return new InstagramProcessorResponse(false, media.Value.Caption.Text, imageUrl.ToString(), url, null);
				}
			}
		}
		
	}
}

