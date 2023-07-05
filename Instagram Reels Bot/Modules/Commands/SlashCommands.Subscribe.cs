using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;
using Instagram_Reels_Bot.Helpers;
using Instagram_Reels_Bot.Services;
using System;

namespace Instagram_Reels_Bot.Modules;
public partial class SlashCommands {
    [SlashCommand("subscribe", "Get updates when a user posts a new post on Instagram.", runMode: RunMode.Async)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [RequireBotPermission(ChannelPermission.AttachFiles)]
    [RequireUserPermission(GuildPermission.Administrator, Group = "UserPerm")]
    [RequireRole("InstagramBotSubscribe", Group = "UserPerm")]
    [RequireContext(ContextType.Guild)]
    public async Task Subscribe([Summary("username", "The username of the Instagram user.")] string username) {
        // Check whitelist:
        if (!await EnsureWhitelist()) {
            return;
        }

        //Ensure subscriptions are enabled:
        if (!await EnsureSubscription()) {
            return;
        }

        //Buy more time to process posts:
        await DeferAsync(true);

        // Get IG account:
        InstagramProcessor instagram = new InstagramProcessor(InstagramProcessor.AccountFinder.GetIGAccount());

        // Account limits:
        int subcount = await _subscriptions.GuildSubscriptionCountAsync(Context.Guild.Id);
        int maxcount = await _subscriptions.MaxSubscriptionsCountForGuildAsync(Context.Guild.Id);
        if (subcount >= maxcount) {
            await FollowupAsync("You are already subscribed to " + subcount + " Instagram accounts which is greater than or equal to your limit of " + maxcount + " accounts. use `/unsubscribe` to remove these accounts.");
            return;
        }

        long IGID;
        try {
            IGID = await instagram.GetUserIDFromUsername(username);
        } catch (Exception e) {
            //Possibly incorrect username:
            Console.WriteLine("Get username failure: " + e);
            await FollowupAsync("Failed to get Instagram ID. Is the account name correct?");
            return;
        }
        if (!await instagram.AccountIsPublic(IGID)) {
            await FollowupAsync("The account appears to be private and cannot be viewed by the bot.");
            return;
        }
        //Subscribe:
        try {
            await _subscriptions.SubscribeToAccount(IGID, Context.Channel.Id, Context.Guild.Id);
        } catch (ArgumentException e) when (e.Message.Contains("Already subscribed")) {
            await FollowupAsync("You are already subscribed to this account.");
            return;
        }
        //Notify:
        await Context.Channel.SendMessageAsync("This channel has been subscribed to " + username + " on Instagram by " + Context.User.Mention, allowedMentions: AllowedMentions.None);
        await FollowupAsync("Success! You will receive new posts to this channel. They will not be instant and accounts are checked on a time interval.");
    }
}
