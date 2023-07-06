using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;
using Instagram_Reels_Bot.Helpers;
using Instagram_Reels_Bot.Helpers.Extensions;
using System;

namespace Instagram_Reels_Bot.Modules;
public partial class SlashCommands {
    [SlashCommand("profile", "Gets information about an Instagram profile.", runMode: RunMode.Async)]
    public async Task Profile([Summary("username", "The username of the Instagram account.")] string username) {
        // Check whitelist:
        if (!await EnsureWhitelist()) {
            return;
        }

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
        if (!response.success) {
            await FollowupAsync(response.error);
            return;
        }
        // If not a profile for some reason, treat otherwise:
        if (!response.onlyAccountData) {
            await FollowupAsync("This doesn't appear to be a profile. Try using `/link` for posts.");
            return;
        }

        IGEmbedBuilder embed = _config.Is("DisableTitle", true) 
                                ? new IGEmbedBuilder(response) 
                                : new IGEmbedBuilder(response, Context.User.Username);

        IGComponentBuilder component = new IGComponentBuilder(response, Context.User.Id, _config);

        await FollowupAsync(embed: embed.AutoSelector(), allowedMentions: AllowedMentions.None, components: component.AutoSelector());
    }
}
