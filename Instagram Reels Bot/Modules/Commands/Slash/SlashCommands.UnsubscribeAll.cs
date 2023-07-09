using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;
using Instagram_Reels_Bot.DataTables;
using System;

namespace Instagram_Reels_Bot.Modules;
public partial class SlashCommands {
    [SlashCommand("unsubscribeall", "Unsubscribe from all Instagram accounts.", runMode: RunMode.Async)]
    [RequireUserPermission(GuildPermission.Administrator, Group = "UserPerm")]
    [RequireRole("InstagramBotSubscribe", Group = "UserPerm")]
    [RequireContext(ContextType.Guild)]
    public async Task UnsubscribeAll() {
        //Ensure subscriptions are enabled:
        if (!await EnsureSubscription()) {
            return;
        }

        //Buy more time to process posts:
        await DeferAsync(false);

        FollowedIGUser[] subs = await _subscriptions.GuildSubscriptionsAsync(Context.Guild.Id);
        int errorCount = 0;
        foreach (FollowedIGUser user in subs) {
            foreach (RespondChannel chan in user.SubscribedChannels) {
                if (!chan.GuildID.Equals(Context.Guild.Id.ToString())) {
                    continue;
                }

                try {
                    await _subscriptions.UnsubscribeToAccount(long.Parse(user.InstagramID), ulong.Parse(chan.ChannelID), Context.Guild.Id);
                } catch (Exception e) {
                    Console.WriteLine(e);
                    errorCount++;
                }
            }
        }

        if (errorCount > 0) {
            await FollowupAsync($"Failed to unsubscribe {errorCount} account(s).");
            return;
        }

        if (subs.Length == 0) {
            await FollowupAsync("This guild is not subscribed to any accounts.");
            return;
        }

        await FollowupAsync("Success! Unsubscribed from all accounts.");
    }
}
