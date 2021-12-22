using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;

namespace Instagram_Reels_Bot.Modules
{
	public class SlashCommands
	{
		public async Task SlashCommandHandler(SocketSlashCommand command)
		{
            switch (command.Data.Name)
            {
				case "carousel":
					await CarouselPostParser(command);
					break;
				case "help":
					await help(command);
					break;
				case "invite":
					await invite(command);
					break;
				case "topgg":
					await topgg(command);
					break;
				case "github":
					await github(command);
					break;
				default:
					break;
			}
		}
		/// <summary>
        /// Help command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
		public async Task help(SocketSlashCommand command)
		{
			//response embed:
			var embed = new Discord.EmbedBuilder();
			embed.Title = "Help With Instagram Embed";
			embed.Url = "https://discord.gg/6K3tdsYd6J";
			embed.Description = "This bot links content from an Instagram post put in the chat from an Instagram URL. No setup is needed. For more help and to view the status of the bot, please join our support server: https://discord.gg/6K3tdsYd6J";
			embed.WithColor(new Color(131, 58, 180));
			await command.RespondAsync(null, null, false,true, null, null, embed.Build());
		}
		/// <summary>
        /// invite command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
		public async Task invite(SocketSlashCommand command)
		{
			//response embed:
			var embed = new Discord.EmbedBuilder();
			embed.Title = "Invite Instagram Embed To Your Server!";
			embed.Url = "https://top.gg/bot/815695225678463017";
			embed.Description = "Please visit our top.gg page to invite the bot to your server. https://top.gg/bot/815695225678463017";
			embed.WithColor(new Color(131, 58, 180));
			await command.RespondAsync(null, null, false, true, null, null, embed.Build());
		}
		/// <summary>
        /// Top.gg
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
		public async Task topgg(SocketSlashCommand command)
		{
			//response embed:
			var embed = new Discord.EmbedBuilder();
			embed.Title = "Instagram Embed Top.gg Page";
			embed.Url = "https://top.gg/bot/815695225678463017";
			embed.Description = "Please vote for us and leave a rating on Top.gg. https://top.gg/bot/815695225678463017";
			embed.WithColor(new Color(131, 58, 180));
			await command.RespondAsync(null, null, false, true, null, null, embed.Build());
		}
		/// <summary>
        /// github
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
		public async Task github(SocketSlashCommand command)
		{
			//response embed:
			var embed = new Discord.EmbedBuilder();
			embed.Title = "GitHub";
			embed.Url = "https://github.com/bman46/InstagramEmbedDiscordBot";
			embed.Description = "View the source code and file issues for improvements or bugs. https://github.com/bman46/InstagramEmbedDiscordBot";
			embed.WithColor(new Color(131, 58, 180));
			await command.RespondAsync(null, null, false, true, null, null, embed.Build());
		}
		/// <summary>
		/// /Carousel command
		/// </summary>
		public async Task CarouselPostParser(SocketSlashCommand command)
		{
			//URL of the post:
			string url = command.Data.Options.First().Value.ToString();
			// index of the post:
			int index = int.Parse(command.Data.Options.ElementAt(1).Value.ToString())-1;

			//Parse ig post:
			//ensure login:
            Program.InstagramLogin();
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
				//private post:
				await command.RespondAsync("Invalid URL.", null, false, true);
				return;
			}
            //check for private account:
            if (media.Value == null)
            {
				//private post:
				await command.RespondAsync("The post is private.", null, false, true);
                return;
            }

			if (media.Value.Carousel != null && media.Value.Carousel.Count > 0)
			{
                if (index > media.Value.Carousel.Count-1)
                {
					//catch not carousel.
					await command.RespondAsync("Post number doesnt exist.", null, false, true);
					return;
				}
				//Video
				else if (media.Value.Carousel[index].Videos.Count > 0)
				{
					//get desired video:
					var video = media.Value.Carousel[index].Videos[0];
					//get url:
					string videourl = video.Uri;
					//Upload video:
					try
					{
						using (System.Net.WebClient wc = new System.Net.WebClient())
						{
							wc.OpenRead(videourl);
							//TODO: Support nitro upload sizes:
							if (Convert.ToInt64(wc.ResponseHeaders["Content-Length"]) < 8000000)
							{
								using (var stream = new MemoryStream(wc.DownloadData(videourl)))
								{
									if (stream.Length < 8000000)
									{
										//Upload video:
										await command.RespondWithFileAsync(stream, "IGPost.mp4", "Video from " + command.User.Mention + "'s linked Post:", null, false,false,null,null,null,null);
									}
									else
									{
										//Fallback to url:
										await command.RespondAsync("Video from " + command.User.Mention + "'s linked post: " + videourl);
									}
								}
							}
							else
							{
								//Fallback to url:
								await command.RespondAsync("Video from " + command.User.Mention + "'s linked post: " + videourl);
							}
						}
					}
					catch (Exception e)
					{
						//failback to link to video:
						Console.WriteLine(e);
						//Fallback to url:
						await command.RespondAsync("Video from " + command.User.Mention + "'s linked post: " + videourl);
					}
				}
				//Image:
				else
				{
					//get desired image:
					var image = media.Value.Carousel[index].Images[0];
					media.Value.Images.Add(image);
					//build image embed:
					var embed = new Discord.EmbedBuilder();
					embed.Title = "Content from " + command.User.Username + "'s linked post";
					embed.Url = url;
					embed.Description = (media.Value.Caption != null) ? (ReelsCommand.Truncate(media.Value.Caption.Text, 40)) : ("");
					embed.ImageUrl = media.Value.Images[0].Uri;
					embed.WithColor(new Color(131, 58, 180));
					await command.RespondAsync(null,null,false,false,null,null,embed.Build());
				}
            }
            else
            {
				//catch not carousel.
				await command.RespondAsync("Not a carousel post.",null,false,true);
				return;
			}
		}
	}
}

