using System;
using System.Collections.Generic;
using System.Net.Http;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Instagram_Reels_Bot.Helpers
{
	public class DiscordTools
	{
        /// <summary>
        /// Cuts the size of the string down and cuts the string at a convenient location.
        /// </summary>
        /// <param name="s">The target string.</param>
        /// <param name="length">Desired maximum length</param>
        /// <param name="atWord">Try to cutoff at the end or start of a word instead of the middle.</param>
        /// <param name="addEllipsis">Add ... to the end of a truncated string.</param>
        /// <returns></returns>
        public static string Truncate(string s, int length = 300, bool atWord = true, bool addEllipsis = true, bool cutAtNewLine = true)
        {
            // Dont process null values:
            if (s == null)
            {
                return s;
            }
            //cut description at new line, regardless of length (if enabled):
            if (s.Contains("\n")&&cutAtNewLine)
            {
                //cut string at newline:
                s = s.Substring(0, s.IndexOf("\n")) + "...";
                //recheck size after cut:
                if (s == null || s.Length <= length)
                {
                    return s;
                }
            }
            // Return if the string is less than or equal to the truncation length:
            else if (s.Length <= length)
            {
                return s;
            }
            // Do a simple tuncation at the desired length
            string s2 = s.Substring(0, length);

            // Truncate the string at the word
            if (atWord)
            {
                // List of characters that denote the start or a new word (add to or remove more as necessary)
                List<char> alternativeCutOffs = new List<char>() { ' ', ',', '.', '?', '/', ':', ';', '\'', '\"', '\'', '-', '\n' };

                // Get the index of the last space in the truncated string
                int lastSpace = s2.LastIndexOf(' ');

                // If the last space index isn't -1 and also the next character in the original
                // string isn't contained in the alternativeCutOffs List (which means the previous
                // truncation actually truncated at the end of a word),then shorten string to the last space
                if (lastSpace != -1 && (s.Length >= length + 1 && !alternativeCutOffs.Contains(s.ToCharArray()[length])))
                    s2 = s2.Remove(lastSpace);
            }
            // Add Ellipsis if desired
            if (addEllipsis)
                s2 += "...";
            return s2;
        }

        #region Nitro
        /// <summary>
        /// Calculates the max upload size of a given server.
        /// </summary>
        /// <param name="context">Message context to get guild information from</param>
        /// <returns>Upload size in bytes</returns>
        public static long MaxUploadSize(ICommandContext context)
        {
            return MaxUploadSize(context.Guild);
        }
        /// <summary>
        /// Calculates the max upload size of a given server.
        /// </summary>
        /// <param name="guild">The guild to get the max size from.</param>
        /// <returns>Upload size in bytes</returns>
        public static long MaxUploadSize(SocketGuild guild)
        {
            return MaxUploadSize(((int)guild.PremiumTier));
        }
        /// <summary>
        /// Calculates the max upload size of a given server.
        /// </summary>
        /// <param name="guild">The guild to get the max size from.</param>
        /// <returns>Upload size in bytes</returns>
        public static long MaxUploadSize(IGuild guild)
        {
            return MaxUploadSize(((int)guild.PremiumTier));
        }
        /// <summary>
        /// Calculates the max upload size of a given server.
        /// </summary>
        /// <param name="tier">The nitro tier of the server converted to an int.</param>
        /// <returns>Upload size in bytes</returns>
        public static long MaxUploadSize(int tier)
        {
            switch (tier)
            {
                case ((int)PremiumTier.Tier2):
                    //Tier 2 50MB Upload Limit
                    return 50000000;
                case ((int)PremiumTier.Tier3):
                    //Tier 3 100MB Upload Limit
                    return 100000000;
                default:
                    //Default 25MB Upload Limit
                    return 25000000;
            }
        }
        #endregion

        /// <summary>
        /// Manually suppress embeds. Preferably use discord.net for this.
        /// TODO: Remove this when Discord.Net is fixed.
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="messageId"></param>
        [Obsolete("This function should no longer be used.")]
        public static void SuppressEmbeds(ulong channelId, ulong messageId)
        {
            // create the configuration
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");
            // build the configuration and assign to _config          
            var config = _builder.Build();

            using (HttpClient client = new HttpClient()) {

                Uri apiUrl = new Uri("https://discord.com/api/v9/channels/" + channelId + "/messages/" + messageId);

                using (var request = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl)
                {
                    Content = new StringContent("{\"flags\": 4}", System.Text.Encoding.UTF8, "application/json"),
                })
                {

                    request.Headers.Add("User-Agent", "DiscordBot (https://github.com/bman46/InstagramEmbedDiscordBot, 1.0)");
                    request.Headers.Add("Authorization", "Bot " + config["Token"]);

                    //Send the request:
                    client.Send(request);
                }
            }
        }
        /// <summary>
        /// Attempts to remove the embed.
        /// </summary>
        /// <param name="context">The command context</param>
        public static async void SuppressEmbeds(ICommandContext context)
        {
            //Try to remove the embeds on the command post:
            try
            {
                await context.Message.ModifyAsync(item => { item.Flags = MessageFlags.SuppressEmbeds; });
            }
            catch
            {
                //Doesnt really matter if it fails.
            }
        }
        /// <summary>
        /// Converts a slash param array to a string.
        /// Used for error handling and reporting.
        /// </summary>
        /// <returns></returns>
        public static string SlashParamToString(IInteractionContext arg2)
        {
            string output = "";

            foreach(var param in (arg2.Interaction.Data as SocketSlashCommandData).Options)
            {
                output += param.Name + ": " + param.Value+" ";
            }
            return output;
        }
    }
}
