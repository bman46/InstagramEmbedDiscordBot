using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;
using Instagram_Reels_Bot.Helpers;
using Instagram_Reels_Bot.Services;

namespace Instagram_Reels_Bot.Modules
{
	public class SlashCommands : InteractionModuleBase<ShardedInteractionContext>
	{
		// Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
		public InteractionService Commands { get; set; }

		private Services.CommandHandler _handler;
		private Subscriptions _subscriptions;

		// Constructor injection is also a valid way to access the dependecies
		public SlashCommands(Services.CommandHandler handler, Services.Subscriptions subs)
		{
			_handler = handler;
			_subscriptions = subs;
		}

		[SlashCommand("link","Processes an Instagram link.", runMode: RunMode.Async)]
		public async Task Link(string url, [Summary(description: "The post number for the desired post in a carousel.")][MinValue(1)] int index = 1)
        {
			//Buy more time to process posts:
			await DeferAsync(false);

			//Process Post:
			InstagramProcessorResponse response = await InstagramProcessor.PostRouter(url, Context.Guild, index);

            if (!response.success)
            {
				//Failed to process post:
				await FollowupAsync(response.error, ephemeral: true);
				return;
            }
			else if (response.isVideo)
			{
				if (response.stream != null)
                {
					//Response with stream:
					using (Stream stream = new MemoryStream(response.stream))
					{
						FileAttachment attachment = new FileAttachment(stream, "IGMedia.mp4", "An Instagram Video.");
						await Context.Interaction.FollowupWithFileAsync(attachment, "Video from " + Context.User.Mention + "'s linked reel: ");
					}
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
				embed.Description = (response.caption != null) ? (DiscordTools.Truncate(response.caption)) : ("");
				embed.ImageUrl = "attachment://IGMedia.jpg";
				embed.WithColor(new Color(131, 58, 180));
				if (response.stream != null)
				{
					using (Stream stream = new MemoryStream(response.stream))
					{
						FileAttachment attachment = new FileAttachment(stream, "IGMedia.jpg", "An Instagram Image.");
						await Context.Interaction.FollowupWithFileAsync(attachment, embed: embed.Build());
					}
				}
				else
				{
					embed.ImageUrl = response.contentURL.ToString();
					await FollowupAsync(embed: embed.Build());
				}
			}
			
		}
		[SlashCommand("help", "For help with the bot.", runMode: RunMode.Async)]
		public async Task Help()
		{
			//response embed:
			var embed = new Discord.EmbedBuilder();
			embed.Title = "Help With Instagram Embed";
			embed.Url = "https://discord.gg/6K3tdsYd6J";
			embed.Description = "This bot uploads videos and images from an Instagram post provided via a link. This bot does not require any setup besides basic Discord permission. For more help and to view the status of the bot, please join our support server: https://discord.gg/6K3tdsYd6J";
			embed.WithColor(new Color(131, 58, 180));
			await RespondAsync(embed: embed.Build(), ephemeral: false);
		}
		[SlashCommand("invite", "Invite the bot to your server!", runMode: RunMode.Async)]
		public async Task Invite()
		{
			//response embed:
			var embed = new Discord.EmbedBuilder();
			embed.Title = "Invite Instagram Embed To Your Server!";
			embed.Url = "https://top.gg/bot/815695225678463017";
			embed.Description = "Please visit our top.gg page to invite the bot to your server. https://top.gg/bot/815695225678463017";
			embed.WithColor(new Color(131, 58, 180));
			await RespondAsync(embed: embed.Build(), ephemeral: true);
		}
		[SlashCommand("topgg", "Visit our top.gg page.", runMode: RunMode.Async)]
		public async Task Topgg()
		{
			//response embed:
			var embed = new Discord.EmbedBuilder();
			embed.Title = "Instagram Embed Top.gg Page";
			embed.Url = "https://top.gg/bot/815695225678463017";
			embed.Description = "Please vote for us and leave a rating on Top.gg. https://top.gg/bot/815695225678463017";
			embed.WithColor(new Color(131, 58, 180));
			await RespondAsync(embed: embed.Build(), ephemeral: true);
		}
		[SlashCommand("github", "Visit our github page", runMode: RunMode.Async)]
		public async Task Github()
		{
			//response embed:
			var embed = new Discord.EmbedBuilder();
			embed.Title = "GitHub";
			embed.Url = "https://github.com/bman46/InstagramEmbedDiscordBot";
			embed.Description = "View the source code, contribute to the bot, and file issues for improvements or bugs. https://github.com/bman46/InstagramEmbedDiscordBot";
			embed.WithColor(new Color(131, 58, 180));
			await RespondAsync(embed: embed.Build(), ephemeral: true);
		}
		[SlashCommand("subscribe", "Get updates when a user posts a new post on Instagram.", runMode: RunMode.Async)]
		public async Task Subscribe([Summary("username", "The username of the Instagram user.")]string username)
		{
			//Buy more time to process posts:
			await DeferAsync(true);

			long IGID;
            try
            {
				IGID = await InstagramProcessor.GetUserIDFromUsername(username);
            }catch(Exception e)
            {
				//Possibly incorrect username:
				Console.WriteLine("Get username failure: " + e);
				await FollowupAsync("Failed to get Instagram ID. Is the account name correct?");
				return;
            }
			//Subscribe:
			await _subscriptions.SubscribeToAccount(IGID, Context.Channel.Id, Context.Guild.Id);
			//Notify:
			await Context.Channel.SendMessageAsync("This channel has been subscribed to " + username + " on Instagram by " + Context.User.Mention);
			await FollowupAsync("Success! You will receive new posts to this channel. They will not be instant and accounts are checked on a time interval.");
		}
		[SlashCommand("unsubscribe", "Stop getting updates when a user posts a new post on Instagram.", runMode: RunMode.Async)]
		public async Task Unsubscribe([Summary("username","The username of the Instagram user.")] string username)
		{
			//TODO: Implement.
		}
	}
}