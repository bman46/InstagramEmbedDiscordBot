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

    public static bool Has<T>(this IConfiguration configuration, string key, T expectedValue, bool defaultValue = false) {
        string value = configuration[key];
        if (string.IsNullOrEmpty(value)) {
            return defaultValue;
        }

        return value.ToLower() == expectedValue.ToString().ToLower();
    }

    public static bool IsBotOwner(this IUser user, IConfiguration configuration) 
        => configuration.Has("OwnerID", user.Id);

    public static T Parse<T>(this IConfiguration configuration, string key, T defaultValue = default) {
        if(TryParse(configuration, key, out T result, defaultValue)) {
            return result;
        }

        throw new FormatException($"The '{key}' value in the configuration is in the wrong format, expected '{typeof(T).Name}'");
    }

    public static bool TryParse<T>(this IConfiguration configuration, string key, out T result, T defaultValue = default) {
        string stringValue = configuration[key];
        if (string.IsNullOrEmpty(stringValue)) {
            result = defaultValue;
            return false;
        }

        try {
            result = (T)Convert.ChangeType(stringValue, typeof(T));
            return true;
        } catch {
            result = defaultValue;
            return false;
        }
    }
}
