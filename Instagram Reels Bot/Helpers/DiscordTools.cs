using System;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

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
        public static string Truncate(string s, int length = 100, bool atWord = true, bool addEllipsis = true)
        {
            //cut description at new line:
            if (s.Contains("\n"))
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
            else if (s == null || s.Length <= length)
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
                    //Default 8MB Upload Limit
                    return 8000000;
            }
        }

    }
}

