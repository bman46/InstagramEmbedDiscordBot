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
using Instagram_Reels_Bot.DataTables;

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

			//Embeds:
			//Account Name:
			var account = new EmbedAuthorBuilder();
			account.IconUrl = response.iconURL.ToString();
			account.Name = response.accountName;
			account.Url = response.accountUrl.ToString();

			//Instagram Footer:
			EmbedFooterBuilder footer = new EmbedFooterBuilder();
			footer.IconUrl = "https://upload.wikimedia.org/wikipedia/commons/a/a5/Instagram_icon.png";
			footer.Text = "Instagram";

			var embed = new EmbedBuilder();
			embed.Author = account;
			embed.Title = "Content from " + Context.User.Username + "'s linked post.";
			embed.Footer = footer;
			embed.Timestamp = new DateTimeOffset(response.postDate);
			embed.Url = response.postURL.ToString();
			embed.Description = (response.caption != null) ? (DiscordTools.Truncate(response.caption)) : ("");
			embed.WithColor(new Color(131, 58, 180));

			if (response.isVideo)
			{
				if (response.stream != null)
                {
					//Response with stream:
					using (Stream stream = new MemoryStream(response.stream))
					{
						FileAttachment attachment = new FileAttachment(stream, "IGMedia.mp4", "An Instagram Video.");

						await Context.Interaction.FollowupWithFileAsync(attachment, embed: embed.Build());
					}
				}
                else
                {
					//Response without stream:
					await FollowupAsync(response.contentURL.ToString(), embed: embed.Build());
                }

			}
            else
            {
				embed.ImageUrl = "attachment://IGMedia.jpg";
				if (response.stream != null)
				{
					using (Stream stream = new MemoryStream(response.stream))
					{
						FileAttachment attachment = new FileAttachment(stream, "IGMedia.jpg", "An Instagram Image.");
						await Context.Interaction.FollowupWithFileAsync(attachment, embed: embed.Build(), allowedMentions: AllowedMentions.None);
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
			embed.Description = "This bot uploads videos and images from an Instagram post provided via a link. The bot also allows for subscribing to new posts from accounts using the `/subscribe` command.  For more help and to view the status of the bot, please join our support server: https://discord.gg/6K3tdsYd6J";
			embed.AddField("Embedding Posts", "To embed the contents of an Instagram url, simply paste the link into the chat and the bot will do the rest (as long as it has permission to).\nYou can also use the `/link` along with a URL.\nFor posts with multiple slides, use the `/link` command along with the optional `Index:` parameter to select the specific slide.");
			embed.AddField("Subscriptions", "Note: The subscriptions module is currently under beta testing.\nTo subscribe to an account, use `/subscribe` and the users Instagram account to get new posts from that account delivered to the channel where the command is executed.\nTo unsubscribe from an account, use `/unsubscribe` and the username of the Instagram account in the channel that is subscribed to the account. You can also use `/unsubscribeall` to unsubscribe from all Instagram accounts.\nUse `/subscribed` to list all of the Instagram accounts that the guild is subscribed to.");
			embed.AddField("Roles", "Only users with the role `InstagramBotSubscribe` (case sensitive) or guild administrator permission are allowed to unsubscribe and subscribe to accounts.");
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
		[RequireBotPermission(ChannelPermission.SendMessages)]
		[RequireBotPermission(ChannelPermission.AttachFiles)]
		[RequireContext(ContextType.Guild)]
		public async Task Subscribe([Summary("username", "The username of the Instagram user.")]string username)
		{
			//Ensure subscriptions are enabled:
			if (!_subscriptions.ModuleEnabled)
			{
				await RespondAsync("Subscriptions module is currently disabled.", ephemeral: true);
				return;
			}

			//Check role:
			var role = (Context.User as SocketGuildUser).Roles.FirstOrDefault(role => role.Name == "InstagramBotSubscribe");
			if (role == null && !(Context.User as SocketGuildUser).GuildPermissions.Administrator)
            {
				await RespondAsync("You need guild Administrator permission or the role `InstagramBotSubscribe` assigned to your account to perform this action.", ephemeral: true);
				return;
			}

			//Buy more time to process posts:
			await DeferAsync(true);

			// Account limits:
			int subcount = _subscriptions.GuildSubscriptionCount(Context.Guild.Id);
			int maxcount = _subscriptions.MaxSubscriptionsCountForGuild(Context.Guild.Id);
			if (subcount >= maxcount)
            {
                if (maxcount == 0)
                {
					await FollowupAsync("The subscription module is currently in beta testing to a limited number of guilds. Please join the support server https://discord.gg/6K3tdsYd6J for future updates.", ephemeral: false);
					return;
				}
				await FollowupAsync("You are already subscribed to "+ subcount +" Instagram accounts which is greater than or equal to your limit of "+maxcount+" accounts. use `/unsubscribe` to remove these accounts.");
				return;
			}

			long IGID;
            try
            {
				IGID = await InstagramProcessor.GetUserIDFromUsername(username);
            }
			catch(Exception e)
            {
				//Possibly incorrect username:
				Console.WriteLine("Get username failure: " + e);
				await FollowupAsync("Failed to get Instagram ID. Is the account name correct?");
				return;
            }
            if (!await InstagramProcessor.AccountIsPublic(IGID))
            {
				await FollowupAsync("The account appears to be private and cannot be viewed by the bot.");
				return;
			}
			//Subscribe:
			try
			{
				await _subscriptions.SubscribeToAccount(IGID, Context.Channel.Id, Context.Guild.Id);
			}catch(ArgumentException e) when (e.Message.Contains("Already subscribed"))
            {
				await FollowupAsync("You are already subscribed to this account.");
				return;
			}
			//Notify:
			await Context.Channel.SendMessageAsync("This channel has been subscribed to " + username + " on Instagram by " + Context.User.Mention, allowedMentions: AllowedMentions.None);
			await FollowupAsync("Success! You will receive new posts to this channel. They will not be instant and accounts are checked on a time interval.");
		}
		[SlashCommand("unsubscribe", "Stop getting updates when a user posts a new post on Instagram.", runMode: RunMode.Async)]
		[RequireContext(ContextType.Guild)]
		public async Task Unsubscribe([Summary("username","The username of the Instagram user.")] string username)
		{
			//Ensure subscriptions are enabled:
			if (!_subscriptions.ModuleEnabled)
			{
				await RespondAsync("Subscriptions module is currently disabled.", ephemeral: true);
				return;
			}

			//Check role:
			var role = (Context.User as SocketGuildUser).Roles.FirstOrDefault(role => role.Name == "InstagramBotSubscribe");
			if (role == null && !(Context.User as SocketGuildUser).GuildPermissions.Administrator)
			{
				await RespondAsync("You need guild Administrator permission or the role `InstagramBotSubscribe` assigned to your account to perform this action.", ephemeral: true);
				return;
			}

			//Buy more time to process posts:
			await DeferAsync(true);

			long IGID;
			try
			{
				IGID = await InstagramProcessor.GetUserIDFromUsername(username);
			}
			catch (Exception e)
			{
				//Possibly incorrect username:
				Console.WriteLine("Get username failure: " + e);
				await FollowupAsync("Failed to get Instagram ID. Is the account name correct?");
				return;
			}
			try
			{
				//Subscribe:
				await _subscriptions.UnsubscribeToAccount(IGID, Context.Channel.Id, Context.Guild.Id);
			}
			catch(ArgumentException e) when (e.Message.Contains("Cannot find user."))
            {
				await FollowupAsync("You are not subscribed to that user.");
				return;
            }
			//Notify:
			await Context.Channel.SendMessageAsync("This channel has been unsubscribed to " + username + " on Instagram by " + Context.User.Mention, allowedMentions: AllowedMentions.None);
			await FollowupAsync("Success! You will no longer receive new posts to this channel.");
		}
		[SlashCommand("unsubscribeall", "Unsubscribe from all Instagram accounts.", runMode: RunMode.Async)]
		[RequireContext(ContextType.Guild)]
		public async Task UnsubscribeAll()
        {
			//Ensure subscriptions are enabled:
			if (!_subscriptions.ModuleEnabled)
			{
				await RespondAsync("Subscriptions module is currently disabled.", ephemeral: true);
				return;
			}

			//Check role:
			var role = (Context.User as SocketGuildUser).Roles.FirstOrDefault(role => role.Name == "InstagramBotSubscribe");
			if (role == null && !(Context.User as SocketGuildUser).GuildPermissions.Administrator)
			{
				await RespondAsync("You need guild Administrator permission or the role `InstagramBotSubscribe` assigned to your account to perform this action.", ephemeral: true);
				return;
			}

			//Buy more time to process posts:
			await DeferAsync(false);

			var subs = _subscriptions.GuildSubscriptions(Context.Guild.Id);
			int errorCount = 0;
			foreach (FollowedIGUser user in subs)
			{
				foreach (RespondChannel chan in user.SubscribedChannels)
				{
					if (chan.GuildID.Equals(Context.Guild.Id.ToString()))
					{
						try
						{
							await _subscriptions.UnsubscribeToAccount(long.Parse(user.InstagramID), ulong.Parse(chan.ChannelID), Context.Guild.Id);
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
							errorCount++;
						}
					}
				}
			}
            if (errorCount > 0)
            {
				await FollowupAsync("Failed to unsubscribe " + errorCount + " account(s).");
            }
            else
            {
                if (subs.Length == 0)
                {
					await FollowupAsync("This guild is not subscribed to any accounts.");
                }
                else
                {
					await FollowupAsync("Success! Unsubscribed from all accounts.");
				}
			}
		}
		[SlashCommand("subscribed", "List of accounts that the guild is subscribed to.", runMode: RunMode.Async)]
		[RequireContext(ContextType.Guild)]
		public async Task Subscribed()
        {
			// buy time:
			await DeferAsync(false);

			var embed = new Discord.EmbedBuilder();
			embed.Title = "Guild Subscriptions";
			embed.WithColor(new Color(131, 58, 180));

			var subs = _subscriptions.GuildSubscriptions(Context.Guild.Id);

			string accountOutput = "";
			string channelOutput = "";
			foreach(FollowedIGUser user in subs)
            {
				foreach(RespondChannel chan in user.SubscribedChannels)
                {
                    if (chan.GuildID.Equals(Context.Guild.Id.ToString()))
                    {
						string chanMention = "Missing channel.";
                        try
                        {
							chanMention = "<#"+Context.Guild.GetChannel(ulong.Parse(chan.ChannelID)).Id+">";
						}catch(Exception e)
                        {
							Console.WriteLine(e);
                        }
						accountOutput += "- [" + await InstagramProcessor.GetIGUsername(user.InstagramID) + "](https://www.instagram.com/" + await InstagramProcessor.GetIGUsername(user.InstagramID) + ")\n";
						channelOutput += chanMention + "\n";
					}
                }
			}
			embed.Description = subs.Count()+" of "+_subscriptions.MaxSubscriptionsCountForGuild(Context.Guild.Id)+" subscribes used.\n**Instagram Accounts:**";
			if (subs.Length == 0)
            {
				embed.Description = "No accounts followed. Get started by using `/subscribe`";
            }
            else
            {
				embed.AddField("Account", accountOutput, true);
				embed.AddField("Channel", channelOutput, true);
			}
			await FollowupAsync(embed: embed.Build());
		}
	}
}