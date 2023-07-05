using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;
using Instagram_Reels_Bot.DataTables;
using Instagram_Reels_Bot.Helpers;
using System;

namespace Instagram_Reels_Bot.Modules;
public partial class SlashCommands {
    [SlashCommand("unsubscribe", "Unsubscribe to updates from selectable Instagram accounts.", runMode: RunMode.Async)]
    [RequireUserPermission(GuildPermission.Administrator, Group = "UserPerm")]
    [RequireRole("InstagramBotSubscribe", Group = "UserPerm")]
    [RequireContext(ContextType.Guild)]
    public async Task Unsubscribe() {
        //Ensure subscriptions are enabled:
        if (!await EnsureSubscription()) {
            return;
        }

        //Buy more time to process:
        await DeferAsync(false);

        // Get Accounts:
        var subs = await _subscriptions.GuildSubscriptionsAsync(Context.Guild.Id);

        // Create Dropdown with channels:
        var menuBuilder = new SelectMenuBuilder()
            .WithCustomId("unsubscribe")
            .WithPlaceholder("Select accounts to remove.")
            .WithMinValues(0);

        // Get IG account:
        InstagramProcessor instagram = new InstagramProcessor(InstagramProcessor.AccountFinder.GetIGAccount());

        // Add users to dropdown:
        foreach (FollowedIGUser user in subs) {
            foreach (RespondChannel chan in user.SubscribedChannels) {
                // Get username:
                string username = await instagram.GetIGUsername(user.InstagramID);
                if (username == null) {
                    username = "Deleted Account";
                }

                string channelName = Context.Guild.GetChannel(ulong.Parse(chan.ChannelID))?.Name;
                // Channel null check:
                if (channelName is null) {
                    channelName = "Unknown Channel";
                }

                // Add account option to menu:
                SelectMenuOptionBuilder optBuilder = new SelectMenuOptionBuilder()
                    .WithLabel(username)
                    .WithValue(user.InstagramID + "-" + chan.ChannelID)
                    .WithDescription(username + " in channel " + channelName);
                menuBuilder.AddOption(optBuilder);
            }
        }

        // Check for subs:
        if (subs.Length < 1) {
            await FollowupAsync("No accounts subscribed.");
            return;
        }

        // Make embed:
        var embed = new EmbedBuilder();
        embed.Title = "Unsubscribe";
        embed.Description = "Select accounts that you would like to unsubscribe from in the dropdown below.";
        embed.WithColor(new Color(131, 58, 180));

        // Set max count:
        menuBuilder.WithMaxValues(menuBuilder.Options.Count);
        // Component Builder:
        var builder = new ComponentBuilder()
            .WithSelectMenu(menuBuilder)
            .WithButton("Delete Message", $"delete-message-{Context.User.Id}", style: ButtonStyle.Danger);

        // Send message
        await FollowupAsync(embed: embed.Build(), components: builder.Build());
    }
}
