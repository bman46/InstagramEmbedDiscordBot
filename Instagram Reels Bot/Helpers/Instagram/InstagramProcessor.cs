using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using InstagramApiSharp.Logger;
using Microsoft.Extensions.Configuration;
using OtpNet;
using System.Linq;
using System.Net;

namespace Instagram_Reels_Bot.Helpers
{
    public class InstagramProcessor
	{
        /// <summary>
        /// The IG processor for the account:
        /// </summary>
        public InstagramApiSharp.API.IInstaApi instaApi;

        /// <summary>
        /// Class responsible for getting an account to use the IG Processor with
        /// </summary>
        public class AccountFinder
        {
            
        }
    }
}