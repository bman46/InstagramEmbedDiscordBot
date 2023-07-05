using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver.Search;
using static Org.BouncyCastle.Math.EC.ECCurve;
using System;

namespace Instagram_Reels_Bot.Helpers.Extensions;
internal static class ConfigExtensions {

    public static bool Contains<T>(this IConfiguration configuration, string key, T expectedValue, bool defaultValue = false) {
        string value = configuration[key];
        if (string.IsNullOrEmpty(value)) {
            return defaultValue;
        }

        return value.ToLower() == expectedValue.ToString().ToLower();
    }

    public static bool IsBotOwner(this IUser user, IConfiguration configuration) 
        => configuration.Contains("OwnerID", user.Id);
}
