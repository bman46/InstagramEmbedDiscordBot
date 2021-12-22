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
			Uri link;
            try
            {
				link = new Uri(url);
            }
			catch(System.UriFormatException)
            {
				return new InstagramProcessorResponse("Malformed URL.");
            }
            if (isStory(link))
            {
				//TODO: Story processor:
				return new InstagramProcessorResponse("Stories not implemented yet. Check back later.");
            }
            else
            {
				return await PostProcessorAsync(url, postIndex, ((int)guild.PremiumTier));
            }
        }
		private static bool isStory(Uri url)
        {
            //URL Starts with stories
            //https://instagram.com/stories/google/2733780123514124411?utm_source=ig_story_item_share&utm_medium=copy_link
            return url.Segments[1].StartsWith("stories");
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
			catch (Exception e)
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
							using (var stream = new MemoryStream(wc.DownloadData(videourl)))
							{
								if (stream.Length < maxUploadSize)
								{
									//upload video:
									return new InstagramProcessorResponse(true, media.Value.Caption.Text, videourl, url, stream);
								}
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
						using (var stream = new MemoryStream(wc.DownloadData(imageUrl)))
						{
							if (stream.Length < maxUploadSize)
							{
								//upload video:
								return new InstagramProcessorResponse(false, media.Value.Caption.Text, imageUrl.ToString(), url, stream);
							}
						}
					}
					return new InstagramProcessorResponse(false, media.Value.Caption.Text, imageUrl.ToString(), url, null);
				}
			}
		}
		
	}
}

