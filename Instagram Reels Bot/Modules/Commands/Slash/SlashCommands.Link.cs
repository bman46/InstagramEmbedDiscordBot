using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;
using Instagram_Reels_Bot.Helpers;
using System;

namespace Instagram_Reels_Bot.Modules;
public partial class SlashCommands {

    [SlashCommand("link", "Processes an Instagram link.", runMode: RunMode.Async)]
    public async Task Link(string url, [Summary(description: "The post number for the desired post in a carousel.")][MinValue(1)] int index = 1, [Summary(description: "Set to true to mark the image/video and caption as a spoiler.")] bool HasSpoilers = false) {
        // Check whitelist:
        if (!await EnsureWhitelist()) {
            return;
        }

        //Buy more time to process posts:
        await DeferAsync(false);

        // Get IG account:
        InstagramProcessor instagram = new InstagramProcessor(InstagramProcessor.AccountFinder.GetIGAccount());

        //Process Post:
        InstagramProcessorResponse response = await instagram.PostRouter(url, Context.Guild, index);

        if (!response.success) {
            //Failed to process post:
            await FollowupAsync(response.error, ephemeral: true);
            return;
        }

        //Create embed builder:
        IGEmbedBuilder embed = (!string.IsNullOrEmpty(_config["DisableTitle"]) && _config["DisableTitle"].ToLower() == "true") ? (new IGEmbedBuilder(response)) : (new IGEmbedBuilder(response, Context.User.Username));

        //Create component builder:
        IGComponentBuilder component = new IGComponentBuilder(response, Context.User.Id, _config);

        if (response.isVideo) {
            if (response.stream != null) {
                //Response with stream:
                using (Stream stream = new MemoryStream(response.stream)) {
                    FileAttachment attachment = new FileAttachment(stream, "IGMedia.mp4", "An Instagram Video.", isSpoiler: HasSpoilers);

                    await Context.Interaction.FollowupWithFileAsync(attachment, embed: embed.AutoSelector(), components: component.AutoSelector());
                }
            } else {
                //Response without stream:
                await FollowupAsync(response.contentURL.ToString(), embed: embed.AutoSelector(), components: component.AutoSelector());
            }

        } else {
            if (response.stream != null) {
                using (Stream stream = new MemoryStream(response.stream)) {
                    FileAttachment attachment = new FileAttachment(stream, "IGMedia.jpg", "An Instagram Image.", isSpoiler: HasSpoilers);
                    await Context.Interaction.FollowupWithFileAsync(attachment, embed: embed.AutoSelector(), allowedMentions: AllowedMentions.None, components: component.AutoSelector());
                }
            } else {
                await FollowupAsync(embed: embed.AutoSelector(), components: component.AutoSelector());
            }
        }

    }
}
