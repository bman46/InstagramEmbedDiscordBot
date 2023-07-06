using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;
using System;

namespace Instagram_Reels_Bot.Modules;
public partial class SlashCommands {
    [SlashCommand("github", "Visit our github page", runMode: RunMode.Async)]
    public async Task Github() {
        //response embed:
        var embed = new EmbedBuilder {
            Title = "GitHub",
            Url = "https://github.com/bman46/InstagramEmbedDiscordBot",
            Description = "View the source code, download code to host your own version, contribute to the bot, and file issues for improvements or bugs. [Github](https://github.com/bman46/InstagramEmbedDiscordBot)"
        };
        embed.WithColor(new Color(131, 58, 180));

        var buttonGithub = new ButtonBuilder {
            Label = "GitHub",
            Style = ButtonStyle.Link,
            Url = "https://github.com/bman46/InstagramEmbedDiscordBot"
        };
        ComponentBuilder component = new ComponentBuilder().WithButton(buttonGithub);

        await RespondAsync(embed: embed.Build(), ephemeral: true, components: component.Build());
    }
}
