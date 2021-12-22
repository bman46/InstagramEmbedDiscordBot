using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;

namespace Instagram_Reels_Bot.Modules
{
	public class SlashCommands : InteractionModuleBase<ShardedInteractionContext>
	{
		// Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
		public InteractionService Commands { get; set; }

		private Services.CommandHandler _handler;

		// Constructor injection is also a valid way to access the dependecies
		public SlashCommands(Services.CommandHandler handler)
		{
			_handler = handler;
		}

		[SlashCommand("link","Processes an Instagram link.")]
		public async Task Link(string url, [Summary(description: "The post number for the desired post in a carousel.")][MinValue(1)] int index = 1)
        {
			Console.WriteLine(url);
			//Buy more time to process posts:
			await DeferAsync(false);

			//ensure login:
			Program.InstagramLogin();

			//Arrays start a zero:
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
				//private post:
				await FollowupAsync("Invalid URL.");
				
				return;
			}

			//check for private account:
			if (media.Value == null)
			{
				await FollowupAsync("Private account.");
				return;
			}

			//inject image from carousel:
			if (media.Value.Carousel != null && media.Value.Carousel.Count > 0)
			{
                if (media.Value.Carousel.Count <= index)
                {
					await FollowupAsync("Index out of bounds. There is only "+ media.Value.Carousel.Count +" Posts.");
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
						if (Convert.ToInt64(wc.ResponseHeaders["Content-Length"]) < 8000000)
						{
							using (var stream = new MemoryStream(wc.DownloadData(videourl)))
							{
								if (stream.Length < 8000000)
								{
									//upload video:
									//List <FileAttachment> attachments= new List<FileAttachment>();
									//attachments.Add(new FileAttachment(stream, "IGPost.mp4", "An Instagram post."));
									await Context.Interaction.FollowupWithFileAsync(stream, "IGPost.mp4", "Video from " + Context.User.Mention + "'s linked Post:", null, false, false, null, null, null, null);
									//await command.ModifyOriginalResponseAsync(x => { x.Content = "Video from " + command.User.Mention + "'s linked Post:"; x.Attachments = attachments; });
								}
								else
								{
									//Fallback to url:
									await FollowupAsync("Video from " + Context.User.Mention + "'s linked post: " + videourl);
								}
							}
						}
						else
						{
							//Fallback to url:
							await FollowupAsync("Video from " + Context.User.Mention + "'s linked post: " + videourl);
						}
					}
				}
				catch (Exception e)
				{
					//failback to link to video:
					Console.WriteLine(e);
					//Fallback to url:
					await FollowupAsync("Video from " + Context.User.Mention + "'s linked post: " + videourl);
				}
			}
			else
			{
				var embed = new EmbedBuilder();
				embed.Title = "Content from " + Context.User.Username + "'s linked post";
				embed.Url = url;
				embed.Description = (media.Value.Caption != null) ? (ReelsCommand.Truncate(media.Value.Caption.Text, 40)) : ("");
				embed.ImageUrl = media.Value.Images[0].Uri;
				embed.WithColor(new Color(131, 58, 180));
				await FollowupAsync(null,null,false,false,null,null,null,embed.Build());
			}
		}
		[SlashCommand("help", "For help with the bot.")]
		public async Task Help()
		{
			//response embed:
			var embed = new Discord.EmbedBuilder();
			embed.Title = "Help With Instagram Embed";
			embed.Url = "https://discord.gg/6K3tdsYd6J";
			embed.Description = "This bot links content from an Instagram post put in the chat from an Instagram URL. No setup is needed. For more help and to view the status of the bot, please join our support server: https://discord.gg/6K3tdsYd6J";
			embed.WithColor(new Color(131, 58, 180));
			await RespondAsync(null, null, false,true, null, null, null, embed.Build());
		}
		[SlashCommand("invite", "Invite the bot to your server!")]
		public async Task Invite()
		{
			//response embed:
			var embed = new Discord.EmbedBuilder();
			embed.Title = "Invite Instagram Embed To Your Server!";
			embed.Url = "https://top.gg/bot/815695225678463017";
			embed.Description = "Please visit our top.gg page to invite the bot to your server. https://top.gg/bot/815695225678463017";
			embed.WithColor(new Color(131, 58, 180));
			await RespondAsync(null, null, false, true, null, null, null, embed.Build());
		}
		[SlashCommand("topgg", "Visit our top.gg page.")]
		public async Task Topgg()
		{
			//response embed:
			var embed = new Discord.EmbedBuilder();
			embed.Title = "Instagram Embed Top.gg Page";
			embed.Url = "https://top.gg/bot/815695225678463017";
			embed.Description = "Please vote for us and leave a rating on Top.gg. https://top.gg/bot/815695225678463017";
			embed.WithColor(new Color(131, 58, 180));
			await RespondAsync(null, null, false, true, null, null, null, embed.Build());
		}
		[SlashCommand("github", "Visit our github page")]
		public async Task Github()
		{
			//response embed:
			var embed = new Discord.EmbedBuilder();
			embed.Title = "GitHub";
			embed.Url = "https://github.com/bman46/InstagramEmbedDiscordBot";
			embed.Description = "View the source code and file issues for improvements or bugs. https://github.com/bman46/InstagramEmbedDiscordBot";
			embed.WithColor(new Color(131, 58, 180));
			await RespondAsync(null, null, false, true, null, null,null, embed.Build());
		}
	}
}

