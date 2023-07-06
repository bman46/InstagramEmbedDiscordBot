using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;
using System;

namespace Instagram_Reels_Bot.Modules;
public partial class SlashCommands {
    [SlashCommand("help", "For help with the bot.", runMode: RunMode.Async)]
    public async Task Help() {
        // Check whitelist:
        if (!await EnsureWhitelist()) {
            return;
        }

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
        if (Context.Client.CurrentUser.Id == 815695225678463017) {
            embed.AddField("Legal", "[Terms of Use](https://github.com/bman46/InstagramEmbedDiscordBot/blob/master/legal/TermsAndConditions.md)\n[Privacy Policy](https://github.com/bman46/InstagramEmbedDiscordBot/blob/master/legal/Privacy.md)");
        } else {
            embed.AddField("Support", "Please note that this bot is self-hosted. For any support, ask the server owner/mods.");
        }
        embed.WithColor(new Color(131, 58, 180));

        ComponentBuilder component = new ComponentBuilder();

        // Only on official bot:
        if (Context.Client.CurrentUser.Id == 815695225678463017) {
            ButtonBuilder button = new ButtonBuilder();
            button.Label = "Support Server";
            button.Style = ButtonStyle.Link;
            button.Url = "https://discord.gg/6K3tdsYd6J";
            component.WithButton(button);
        }

        await RespondAsync(embed: embed.Build(), ephemeral: false, components: component.Build());
    }
}
