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

        var embed = new EmbedBuilder {
            Title = "Guild Subscriptions"
        };
        embed.WithColor(new Color(131, 58, 180));

        FollowedIGUser[] subs = await _subscriptions.GuildSubscriptionsAsync(Context.Guild.Id);
        embed.Description = $"{subs.Length} of {await _subscriptions.MaxSubscriptionsCountForGuildAsync(Context.Guild.Id)} subscribes used.\n**Instagram Accounts:**";

        var accountOutput = new StringBuilder();
        var channelOutput = new StringBuilder();
        foreach (FollowedIGUser user in subs) {
            foreach (RespondChannel channel in user.SubscribedChannels) {
                if (!channel.GuildID.Equals(Context.Guild.Id.ToString())) {
                    continue;
                }
                
                string chanMention = "Missing channel.\n";
                if (Context.Guild.GetChannel(ulong.Parse(channel.ChannelID)) is not null) {
                    chanMention = "<#" + Context.Guild.GetChannel(ulong.Parse(channel.ChannelID)).Id + ">\n";
                }

                string username = await instagram.GetIGUsername(user.InstagramID);
                string accountMention = username is null 
                                        ? "- Deleted Account\n" 
                                        : $"- [{username}](https://www.instagram.com/{username})\n";

                if (accountOutput.Length + accountMention.Length <= 1024 && channelOutput.Length + chanMention.Length <= 1024) {
                    accountOutput.Append(accountMention);
                    channelOutput.Append( chanMention);
                } else {
                    embed.AddField("Account", accountOutput.ToString(), true);
                    embed.AddField("Channel", channelOutput.ToString(), true);
                    embeds.Add(embed.Build());

                    //Restart new embed:
                    embed = new EmbedBuilder();
                    embed.WithColor(new Color(131, 58, 180));
                    accountOutput.Clear();
                    channelOutput.Clear();

                    accountOutput.Append(accountMention);
                    channelOutput.Append(chanMention);
                }
            }
        }

        if (subs.Length == 0) {
            embed.Description = "No accounts followed. Get started by using `/subscribe`";
        } else {
            embed.AddField("Account", accountOutput.ToString(), true);
            embed.AddField("Channel", channelOutput.ToString(), true);
        }

        embeds.Add(embed.Build());
        await FollowupAsync(embeds: embeds.ToArray());
    }
}
