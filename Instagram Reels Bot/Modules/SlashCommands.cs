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

			// Get IG account:
			InstagramProcessor instagram = new InstagramProcessor(InstagramProcessor.AccountFinder.GetIGAccount());

			//Process Post:
			InstagramProcessorResponse response = await instagram.PostRouter(url, Context.Guild, index);

            if (!response.success)
            {
				//Failed to process post:
				await FollowupAsync(response.error, ephemeral: true);
				return;
            }

			//Create embed builder:
			IGEmbedBuilder embed = new IGEmbedBuilder(response, Context.User.Username);

			//Create component builder:
			IGComponentBuilder component = new IGComponentBuilder(response, Context.User.Id);

			if (response.isVideo)
			{
				if (response.stream != null)
                {
					//Response with stream:
					using (Stream stream = new MemoryStream(response.stream))
					{
						FileAttachment attachment = new FileAttachment(stream, "IGMedia.mp4", "An Instagram Video.");

						await Context.Interaction.FollowupWithFileAsync(attachment, embed: embed.AutoSelector(), components: component.AutoSelector());
					}
				}
                else
                {
					//Response without stream:
					await FollowupAsync(response.contentURL.ToString(), embed: embed.AutoSelector(), components: component.AutoSelector());
                }

			}
            else
            {
				if (response.stream != null)
				{
					using (Stream stream = new MemoryStream(response.stream))
					{
						FileAttachment attachment = new FileAttachment(stream, "IGMedia.jpg", "An Instagram Image.");
						await Context.Interaction.FollowupWithFileAsync(attachment, embed: embed.AutoSelector(), allowedMentions: AllowedMentions.None, components: component.AutoSelector());
					}
				}
				else
				{
					await FollowupAsync(embed: embed.AutoSelector(), components: component.AutoSelector());
				}
			}
			
		}
		[SlashCommand("profile", "Gets information about an Instagram profile.", runMode: RunMode.Async)]
		public async Task Link([Summary("username", "The username of the Instagram account.")] string username)
        {
			//Buy more time to process posts:
			await DeferAsync(false);

			// Get IG account:
			InstagramProcessor instagram = new InstagramProcessor(InstagramProcessor.AccountFinder.GetIGAccount());

			//Create url:
			string url = username;
			if (!Uri.IsWellFormedUriString(username, UriKind.Absolute))
				url = "https://instagram.com/" + username;

			// Process profile:
			InstagramProcessorResponse response = await instagram.PostRouter(url, (int)Context.Guild.PremiumTier, 1);

			// Check for failed post:
			if (!response.success)
			{
				await FollowupAsync(response.error);
				return;
			}
			// If not a profile for some reason, treat otherwise:
			if (!response.onlyAccountData)
			{
				await FollowupAsync("This doesnt appear to be a profile. Try using `/link` for posts.");
				return;
			}

			IGEmbedBuilder embed = new IGEmbedBuilder(response, Context.User.Username);
			IGComponentBuilder component = new IGComponentBuilder(response, Context.User.Id);

			await FollowupAsync(embed: embed.AutoSelector(), allowedMentions: AllowedMentions.None, components: component.AutoSelector());
		}
		[SlashCommand("help", "For help with the bot.", runMode: RunMode.Async)]
		public async Task Help()
		{
			//response embed:
			var embed = new EmbedBuilder();
			embed.Title = "Help With Instagram Embed";
			embed.Url = "https://discord.gg/6K3tdsYd6J";
			embed.Description = "This bot uploads videos and images from an Instagram post provided via a link. The bot also allows for subscribing to new posts from accounts using the `/subscribe` command.";
			embed.AddField("Embedding Individual Posts", "To embed the contents of an Instagram url, simply paste the link into the chat and the bot will do the rest (as long as it has permission to).\nYou can also use the `/link` along with a URL.\nFor posts with multiple slides, use the `/link` command along with the optional `Index:` parameter to select the specific slide.\nTo get information about an Instagram account, use `/profile [username]` or `/link` with a link to the profile. These commands will NOT subscribe you to an account or get reoccuring updates from that account. Use `/subscribe` for that.");
			embed.AddField("Subscriptions", "Note: The subscriptions module is currently under beta testing to limited guilds.\nTo subscribe to an account, use `/subscribe` and the users Instagram account to get new posts from that account delivered to the channel where the command is executed.\nTo unsubscribe from an account, use `/unsubscribe` and the username of the Instagram account in the channel that is subscribed to the account. You can also use `/unsubscribeall` to unsubscribe from all Instagram accounts.\nUse `/subscribed` to list all of the Instagram accounts that the guild is subscribed to.");
			embed.AddField("Roles", "Only users with the role `InstagramBotSubscribe` (case sensitive) or guild administrator permission are allowed to unsubscribe and subscribe to accounts.");
			embed.AddField("Permissions", "The following channel permissions are required for the bot's operation:\n" +
				"- `Send Messages`\n" +
				"- `View Channel`\n" +
                "- `Attach Files`\n" +
                "- `Manage Messages` (optional-used to remove duplicate embeds)");
			// Only display on official bot.
			if (Context.Client.CurrentUser.Id == 815695225678463017)
			{
				embed.AddField("Legal", "[Terms of Use](https://github.com/bman46/InstagramEmbedDiscordBot/blob/master/legal/TermsAndConditions.md)\n[Privacy Policy](https://github.com/bman46/InstagramEmbedDiscordBot/blob/master/legal/Privacy.md)");
            }
            else
            {
				embed.AddField("Support", "Please note that this bot is self-hosted. For any support, ask the server owner/mods.");
			}
			embed.WithColor(new Color(131, 58, 180));

			ComponentBuilder component = new ComponentBuilder();

			// Only on official bot:
			if (Context.Client.CurrentUser.Id == 815695225678463017)
			{
				ButtonBuilder button = new ButtonBuilder();
				button.Label = "Support Server";
				button.Style = ButtonStyle.Link;
				button.Url = "https://discord.gg/6K3tdsYd6J";
				component.WithButton(button);
			}

			await RespondAsync(embed: embed.Build(), ephemeral: false, components: component.Build());
		}
		//[SlashCommand("invite", "Invite the bot to your server!", runMode: RunMode.Async)]
		//public async Task Invite()
		//{
		//	//response embed:
		//	var embed = new Discord.EmbedBuilder();
		//	embed.Title = "Invite Instagram Embed To Your Server!";
		//	embed.Url = "https://top.gg/bot/815695225678463017";
		//	embed.Description = "Please visit our [top.gg page](https://top.gg/bot/815695225678463017) to invite the bot to your server. https://top.gg/bot/815695225678463017";
		//	embed.WithColor(new Color(131, 58, 180));

		//	ButtonBuilder buttonTopgg = new ButtonBuilder();
		//	buttonTopgg.Label = "Top.gg";
		//	buttonTopgg.Style = ButtonStyle.Link;
		//	buttonTopgg.Url = "https://top.gg/bot/815695225678463017";
		//	// https://discord.com/oauth2/authorize?client_id=815695225678463017&permissions=60480&scope=applications.commands%20bot
		//	ButtonBuilder buttonInvite = new ButtonBuilder();
		//	buttonInvite.Label = "Invite";
		//	buttonInvite.Style = ButtonStyle.Link;
		//	buttonInvite.Url = "https://discord.com/oauth2/authorize?client_id=815695225678463017&permissions=60480&scope=applications.commands%20bot";
		//	ComponentBuilder component = new ComponentBuilder().WithButton(buttonTopgg).WithButton(buttonInvite);

		//	await RespondAsync(embed: embed.Build(), ephemeral: true, components: component.Build());
		//}
		//[SlashCommand("vote", "Vote our bot on Top.gg and DiscordBotList.com", runMode: RunMode.Async)]
		//public async Task Vote()
		//{
		//	//response embed:
		//	var embed = new Discord.EmbedBuilder();
		//	embed.Title = "Instagram Embed Top.gg and DiscordBotList.com Page";
		//	embed.Url = "https://top.gg/bot/815695225678463017";
		//	embed.Description = "Please vote for us and leave a rating on [Top.gg](https://top.gg/bot/815695225678463017/vote) and [DiscordBotList.com](https://discordbotlist.com/bots/instagram-embed/upvote).";
		//	embed.WithColor(new Color(131, 58, 180));

		//	// top.gg
		//	ButtonBuilder buttonTopgg = new ButtonBuilder();
		//	buttonTopgg.Label = "Top.gg";
		//	buttonTopgg.Style = ButtonStyle.Link;
		//	buttonTopgg.Url = "https://top.gg/bot/815695225678463017/vote";
		//	// dbl
		//	ButtonBuilder buttonDBL = new ButtonBuilder();
		//	buttonDBL.Label = "DBL";
		//	buttonDBL.Style = ButtonStyle.Link;
		//	buttonDBL.Url = "https://discordbotlist.com/bots/instagram-embed/upvote";
		//	ComponentBuilder component = new ComponentBuilder().WithButton(buttonTopgg).WithButton(buttonDBL);

		//	await RespondAsync(embed: embed.Build(), ephemeral: true, components: component.Build());
		//}
		[SlashCommand("github", "Visit our github page", runMode: RunMode.Async)]
		public async Task Github()
		{
			//response embed:
			var embed = new Discord.EmbedBuilder();
			embed.Title = "GitHub";
			embed.Url = "https://github.com/bman46/InstagramEmbedDiscordBot";
			embed.Description = "View the source code, download code to host your own version, contribute to the bot, and file issues for improvements or bugs. [Github](https://github.com/bman46/InstagramEmbedDiscordBot)";
			embed.WithColor(new Color(131, 58, 180));

			ButtonBuilder buttonGithub = new ButtonBuilder();
			buttonGithub.Label = "GitHub";
			buttonGithub.Style = ButtonStyle.Link;
			buttonGithub.Url = "https://github.com/bman46/InstagramEmbedDiscordBot";
			ComponentBuilder component = new ComponentBuilder().WithButton(buttonGithub);

			await RespondAsync(embed: embed.Build(), ephemeral: true, components: component.Build());
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

			// Get IG account:
			InstagramProcessor instagram = new InstagramProcessor(InstagramProcessor.AccountFinder.GetIGAccount());

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
				IGID = await instagram.GetUserIDFromUsername(username);
            }
			catch(Exception e)
            {
				//Possibly incorrect username:
				Console.WriteLine("Get username failure: " + e);
				await FollowupAsync("Failed to get Instagram ID. Is the account name correct?");
				return;
            }
            if (!await instagram.AccountIsPublic(IGID))
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

			// Get IG account:
			InstagramProcessor instagram = new InstagramProcessor(InstagramProcessor.AccountFinder.GetIGAccount());

			long IGID;
			try
			{
				IGID = await instagram.GetUserIDFromUsername(username);
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
			//Ensure subscriptions are enabled:
			if (!_subscriptions.ModuleEnabled)
			{
				await RespondAsync("Subscriptions module is currently disabled.", ephemeral: true);
				return;
			}

			// buy time:
			await DeferAsync(false);

			// Get IG account:
			InstagramProcessor instagram = new InstagramProcessor(InstagramProcessor.AccountFinder.GetIGAccount());

			List<Embed> embeds = new List<Embed>();

			var embed = new Discord.EmbedBuilder();
			embed.Title = "Guild Subscriptions";
			embed.WithColor(new Color(131, 58, 180));

			var subs = _subscriptions.GuildSubscriptions(Context.Guild.Id);
			embed.Description = subs.Count() + " of " + _subscriptions.MaxSubscriptionsCountForGuild(Context.Guild.Id) + " subscribes used.\n**Instagram Accounts:**";

			string accountOutput = "";
			string channelOutput = "";
			foreach(FollowedIGUser user in subs)
            {
				foreach(RespondChannel chan in user.SubscribedChannels)
                {
                    if (chan.GuildID.Equals(Context.Guild.Id.ToString()))
                    {
						string chanMention = "Missing channel.\n";
                        try
                        {
							chanMention = "<#"+Context.Guild.GetChannel(ulong.Parse(chan.ChannelID)).Id+">\n";
						}catch(Exception e)
                        {
							Console.WriteLine(e);
                        }
						string accountMention = "- [" + await instagram.GetIGUsername(user.InstagramID) + "](https://www.instagram.com/" + await instagram.GetIGUsername(user.InstagramID) + ")\n";
						if((accountOutput+ accountMention).Length<=1024 && (channelOutput + chanMention).Length <= 1024)
                        {
							accountOutput += accountMention;
							channelOutput += chanMention;
                        }
                        else
                        {
							embed.AddField("Account", accountOutput, true);
							embed.AddField("Channel", channelOutput, true);
							embeds.Add(embed.Build());

							//Restart new embed:
							embed = new EmbedBuilder();
							embed.WithColor(new Color(131, 58, 180));
							accountOutput = accountMention;
							accountOutput = chanMention;
						}
					}
                }
			}
			if (subs.Length == 0)
            {
				embed.Description = "No accounts followed. Get started by using `/subscribe`";
            }
            else
            {
				embed.AddField("Account", accountOutput, true);
				embed.AddField("Channel", channelOutput, true);
			}
			embeds.Add(embed.Build());
			await FollowupAsync(embeds: embeds.ToArray());
		}
	}
}