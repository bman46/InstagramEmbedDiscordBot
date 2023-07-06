using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;
using Instagram_Reels_Bot.DataTables;
using Instagram_Reels_Bot.Helpers;
using System;

namespace Instagram_Reels_Bot.Modules;
public partial class SlashCommands {
    [SlashCommand("subscribed", "List of accounts that the guild is subscribed to.", runMode: RunMode.Async)]
    [RequireContext(ContextType.Guild)]
    public async Task Subscribed() {
        // Check whitelist:
        if (!await EnsureWhitelist()) {
            return;
        }

        //Ensure subscriptions are enabled:
        if (!await EnsureSubscription()) {
            return;
        }

        // buy time:
        await DeferAsync(false);

        // Get IG account:
        InstagramProcessor instagram = new InstagramProcessor(InstagramProcessor.AccountFinder.GetIGAccount());

        List<Embed> embeds = new List<Embed>();

        var embed = new EmbedBuilder();
        embed.Title = "Guild Subscriptions";
        embed.WithColor(new Color(131, 58, 180));

        var subs = await _subscriptions.GuildSubscriptionsAsync(Context.Guild.Id);
        embed.Description = subs.Count() + " of " + await _subscriptions.MaxSubscriptionsCountForGuildAsync(Context.Guild.Id) + " subscribes used.\n**Instagram Accounts:**";

        string accountOutput = "";
        string channelOutput = "";
        foreach (FollowedIGUser user in subs) {
            foreach (RespondChannel chan in user.SubscribedChannels) {
                if (chan.GuildID.Equals(Context.Guild.Id.ToString())) {
                    string chanMention = "Missing channel.\n";
                    if (Context.Guild.GetChannel(ulong.Parse(chan.ChannelID)) is not null) {
                        chanMention = "<#" + Context.Guild.GetChannel(ulong.Parse(chan.ChannelID)).Id + ">\n";
                    }

                    string username = await instagram.GetIGUsername(user.InstagramID);
                    string accountMention = "- Deleted Account\n";
                    if (username is not null) {
                        accountMention = "- [" + username + "](https://www.instagram.com/" + username + ")\n";
                    }

                    if ((accountOutput + accountMention).Length <= 1024 && (channelOutput + chanMention).Length <= 1024) {
                        accountOutput += accountMention;
                        channelOutput += chanMention;
                    } else {
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
        if (subs.Length == 0) {
            embed.Description = "No accounts followed. Get started by using `/subscribe`";
        } else {
            embed.AddField("Account", accountOutput, true);
            embed.AddField("Channel", channelOutput, true);
        }
        embeds.Add(embed.Build());
        await FollowupAsync(embeds: embeds.ToArray());
    }
}
