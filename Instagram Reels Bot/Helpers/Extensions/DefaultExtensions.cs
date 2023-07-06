using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace Instagram_Reels_Bot.Helpers.Extensions;
internal static class DefaultExtensions {
    public static string ToString(this bool value, string trueValue, string falseValue) {
        return value ? trueValue : falseValue;
    }
}
