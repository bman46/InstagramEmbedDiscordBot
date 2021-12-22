using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;
using Instagram_Reels_Bot.Helpers;

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

			//Process Post:
			InstagramProcessorResponse response = await InstagramProcessor.PostRouter(url, Context.Guild, index);

            if (!response.success)
            {
				await FollowupAsync(response.error);
				return;
            }
			if (response.isVideo)
			{
				if (response.stream != null)
                {
					//Response with stream:
					await Context.Interaction.FollowupWithFileAsync(new MemoryStream(response.stream),"IGVid.mp4", "Video from " + Context.User.Mention + "'s linked reel: ");
                }
                else
                {
					//Response without stream:
					await FollowupAsync("Video from " + Context.User.Mention + "'s linked reel: " + response.contentURL);
                }

			}
            else
            {
				var embed = new EmbedBuilder();
				embed.Title = "Content from " + Context.User.Username + "'s linked post";
				embed.Url = url;
				embed.Description = (response.caption != null) ? (DiscordTools.Truncate(response.caption, 40)) : ("");
				embed.ImageUrl = "attachment://IGImage.jpg";
				embed.WithColor(new Color(131, 58, 180));
				if (response.stream != null)
				{
					await Context.Interaction.FollowupWithFileAsync(new MemoryStream(response.stream), "IGImage.jpg", null, null, false, false, null, null, embed.Build());
                }
                else
                {
					embed.ImageUrl = response.contentURL.ToString();
					await FollowupAsync(null, null, false, false, null, null, null, embed.Build());
                }
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

