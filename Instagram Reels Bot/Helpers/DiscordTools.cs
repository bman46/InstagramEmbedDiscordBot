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
        /// Cuts the size of the string down.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="length"></param>
        /// <param name="atWord"></param>
        /// <param name="addEllipsis"></param>
        /// <returns></returns>
        public static string Truncate(string s, int length = 45, bool atWord = true, bool addEllipsis = true)
        {
            // Return if the string is less than or equal to the truncation length
            if (s == null || s.Length <= length)
            {
                return s;
            }
            //cut description at new line
            else if (s.Contains("\n"))
            {
                //cut string at newline:
                s = s.Substring(0, s.IndexOf("\n")) + "...";
                //recheck size after cut:
                if (s == null || s.Length <= length)
                {
                    return s;
                }
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
        /// <param name="context"></param>
        /// <returns>Upload size in bytes</returns>
        public static long MaxUploadSize(ICommandContext context)
        {
            return MaxUploadSize(context.Guild);
        }
        public static long MaxUploadSize(SocketGuild guild)
        {
            return MaxUploadSize(((int)guild.PremiumTier));
        }
        public static long MaxUploadSize(IGuild guild)
        {
            return MaxUploadSize(((int)guild.PremiumTier));
        }
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

