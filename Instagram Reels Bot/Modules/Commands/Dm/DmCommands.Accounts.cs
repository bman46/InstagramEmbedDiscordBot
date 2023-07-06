using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using Instagram_Reels_Bot.Helpers;
using Instagram_Reels_Bot.Helpers.Instagram;
using System;

namespace Instagram_Reels_Bot.Modules.Commands.Dm;
public partial class DmCommands {
    private static async Task CommandAccounts(SocketUserMessage message) {
        foreach (IGAccount user in InstagramProcessor.AccountFinder.Accounts) {
            if (user.OTPSecret != null) {
                try {
                    var code = Security.GetTwoFactorAuthCode(user.OTPSecret);
                    await message.ReplyAsync("Username: " + user.UserName + "\n2FA Code: " + code + "\nLast Failed: " + user.Blacklist);
                } catch (Exception e) {
                    await message.ReplyAsync("Failed to get 2FA code.");
                    Console.WriteLine("2FA Code error: " + e);
                }
            } else {
                await message.ReplyAsync("Username: " + user.UserName + "\nLast Failed: " + user.Blacklist);
            }
        }
    }

}
