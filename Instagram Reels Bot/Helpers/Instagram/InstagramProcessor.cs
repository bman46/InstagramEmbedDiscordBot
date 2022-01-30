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
using Instagram_Reels_Bot.Helpers.Instagram;

namespace Instagram_Reels_Bot.Helpers
{
    public class InstagramProcessor
	{
        #region accounts
        /// <summary>
        /// Creates an instagram processor using an account.
        /// </summary>
        /// <param name="account"></param>
        public InstagramProcessor(IGAccount account)
        {
            //Create the instaApi object:
            instaApi = InstaApiBuilder.CreateBuilder()
                .UseLogger(new DebugLogger(LogLevel.Exceptions))
                .Build();

            // Set the Android Device:
            instaApi.SetDevice(device);

            //log in
            InstagramLogin(account);
        }
        /// <summary>
        /// The IG processor for the account:
        /// </summary>
        public InstagramApiSharp.API.IInstaApi instaApi;
        /// <summary>
        /// Log into an instagram account.
        /// </summary>
        /// <param name="account">The ig account to use</param>
        public void InstagramLogin(IGAccount account)
        {

            // create the configuration
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");

            // build the configuration and assign to _config          
            var config = _builder.Build();

            //Set the user:
            instaApi.SetUser(account);

            //Get the state file
            string stateFile;
            if (config["StateFile"] != null && config["StateFile"] != "")
            {
                stateFile = Path.Combine(config["StateFile"], account.UserName+".state.bin");
            }
            else
            {
                stateFile = Path.Combine(Directory.GetCurrentDirectory()+Path.DirectorySeparatorChar+"StateFiles", account.UserName + ".state.bin");
            }
            //Try to load session file:
            try
            {
                // load session file if exists
                if (File.Exists(stateFile))
                {
                    Console.WriteLine("Loading state from file");
                    using (var fs = File.OpenRead(stateFile))
                    {
                        // Load state data from file:
                        instaApi.LoadStateDataFromStream(fs);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            //log in:
            var logInResult = instaApi.LoginAsync().GetAwaiter().GetResult();
            //check for login failure:
            if (!logInResult.Succeeded)
            {
                // 2FA
                if (logInResult.Value == InstaLoginResult.TwoFactorRequired)
                {
                    Console.WriteLine("Logging in with 2FA...");
                    //Try to log in:
                    string code = Security.GetTwoFactorAuthCode(account.OTPSecret);
                    Console.WriteLine(code);
                    var twoFAlogInResult = instaApi.TwoFactorLoginAsync(code, 0).GetAwaiter().GetResult();
                    if (!twoFAlogInResult.Succeeded)
                    {
                        Console.WriteLine("Failed to log in with 2FA.");
                        Console.WriteLine(twoFAlogInResult.Info.Message);

                        //Set failed login:
                        var user = AccountFinder.Accounts.FirstOrDefault(acc => acc.UserName == account.UserName);
                        user.FailedLogin = true;

                        //Throw failed login:
                        throw new Exception("Failed login. Error: " + twoFAlogInResult.Info.Message);
                    }
                    else
                    {
                        Console.WriteLine("Logged in with 2FA.");
                    }
                }
                else
                {
                    //Set failed login:
                    var user = AccountFinder.Accounts.FirstOrDefault(acc => acc.UserName == account.UserName);
                    user.FailedLogin = true;

                    //Throw error:
                    throw new Exception("Failed login. Error: " + logInResult.Info.Message);
                }
            }
            // Write the state file:

            var state = instaApi.GetStateDataAsStream();
            // in .net core or uwp apps don't use GetStateDataAsStream.
            // use this one:
            // var state = _instaApi.GetStateDataAsString ();
            // this returns you session as json string.
            // TODO: lock the state file to one user at a time.
            // TODO: write only if needed.
            try
            {
                using (var fileStream = File.Create(stateFile))
                {
                    state.Seek(0, SeekOrigin.Begin);
                    state.CopyTo(fileStream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error writing state file. Error: " + e);
            }
        }
        /// <summary>
        /// Returns the username of the logged in Instagram account.
        /// </summary>
        /// <returns></returns>
        public string GetIGUsername()
        {
            return instaApi.GetLoggedUser().UserName;
        }
        /// <summary>
        /// Class responsible for getting an account to use the IG Processor with
        /// </summary>
        public static class AccountFinder
        {
            /// <summary>
            /// List of accounts
            /// </summary>
            public static List<IGAccount> Accounts = new List<IGAccount>();

            /// <summary>
            /// Loads accounts from the Config file:
            /// </summary>
            public static void LoadAccounts()
            {
                // create the configuration
                var _builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile(path: "config.json");

                // build the configuration and assign to _config          
                var config = _builder.Build();

                //Add accounts to the array:
                List<IGAccount> creds = config.GetSection("IGAccounts").Get<List<IGAccount>>();
                for(int i = 0; i < creds.Count; i++)
                {
                    creds[i].UsageTimes = config.GetSection("IGAccounts:" + i + ":UsageTimes").Get<List<IGAccount.OperatingTime>>();
                }
                Accounts = creds;
            }
            /// <summary>
            /// Gets a valid IG account.
            /// </summary>
            /// <returns>An IG Account</returns>
            /// <exception cref="InvalidDataException">No accounts avaliable</exception>
            public static IGAccount GetIGAccount()
            {
                //Randomize the accounts
                Random rand = new Random();
                var shuffledAccounts = Accounts.OrderBy(x => rand.Next()).ToList();

                //Find a valid account
                foreach (IGAccount cred in shuffledAccounts)
                {
                    TimeOnly timeNow = TimeOnly.FromDateTime(DateTime.Now);
                    if (!cred.FailedLogin)
                    {
                        if (cred.UsageTimes.Count > 0)
                        {
                            // Check valid times:
                            foreach (IGAccount.OperatingTime time in cred.UsageTimes)
                            {
                                if (time.BetweenStartAndEnd(timeNow))
                                {
                                    return cred;
                                }
                            }
                        }
                        else
                        {
                            // Warn about not setting valid times:
                            Console.WriteLine("Warning: No time set on account " + cred.UserName+". Using the account.");
                            return cred;
                        }
                    }
                }
                throw new InvalidDataException("No available accounts.");
            }
        }
        /// <summary>
        /// Android device to be used
        /// </summary>
        public static AndroidDevice device = new AndroidDevice
        {
            // Device name
            AndroidBoardName = "HONOR",
            // Device brand
            DeviceBrand = "HUAWEI",
            // Hardware manufacturer
            HardwareManufacturer = "HUAWEI",
            // Device model
            DeviceModel = "PRA-LA1",
            // Device model identifier
            DeviceModelIdentifier = "PRA-LA1",
            // Firmware brand
            FirmwareBrand = "HWPRA-H",
            // Hardware model
            HardwareModel = "hi6250",
            // Device guid
            DeviceGuid = new Guid("be997499-c663-492e-a125-f4c8d3786ebf"),
            // Phone guid
            PhoneGuid = new Guid("7b92321f-dd9a-425e-b3ee-d4aaf476ec53"),
            // Device id based on Device guid
            DeviceId = ApiRequestMessage.GenerateDeviceIdFromGuid(new Guid("be997499-c663-492e-a125-f4c8d3786ebf")),
            // Resolution
            Resolution = "1080x1812",
            // Dpi
            Dpi = "480dpi"
        };
        #endregion Accounts
        #region IG Accounts
        /// <summary>
        /// The Instagram username of the user.
        /// </summary>
        /// <param name="igid"></param>
        /// <returns></returns>
        public async Task<string> GetIGUsername(string igid)
        {
            return (await instaApi.UserProcessor.GetUserInfoByIdAsync(long.Parse(igid))).Value.Username;
        }
        /// <summary>
        /// Checks to see if an account is public and accesible or not.
        /// </summary>
        /// <param name="instagramID"></param>
        /// <returns>True if the account is public. False on error or private.</returns>
        public async Task<bool> AccountIsPublic(long instagramID)
        {
            var userInfo = (await instaApi.UserProcessor.GetUserInfoByIdAsync(instagramID));
            if (!userInfo.Succeeded)
            {
                Console.WriteLine("Failed to get user. " + userInfo.Info);
                return false;
            }
            return !userInfo.Value.IsPrivate;
        }
        public async Task<long> GetUserIDFromUsername(string username)
        {
            return (await instaApi.UserProcessor.GetUserAsync(username)).Value.Pk;
        }
        /// <summary>
        /// Gets information about the account.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="premiumTier"></param>
        /// <returns>An InstagramProcessorResponse with just profile data.</returns>
        public async Task<InstagramProcessorResponse> ProcessAccountAsync(Uri url, int premiumTier)
        {
            string username = url.Segments[1].TrimEnd('/');
            var user = await instaApi.UserProcessor.GetUserInfoByUsernameAsync(username);
            if (!user.Succeeded)
            {
                //Handle the failed case:
                return HandleFailure(user);
            }
            return new InstagramProcessorResponse(user.Value.FullName, username, new Uri(user.Value.ProfilePicUrl), user.Value.FollowerCount, user.Value.FollowingCount, user.Value.MediaCount, (string.IsNullOrEmpty(user.Value.Biography)) ? ("No bio") : DiscordTools.Truncate(user.Value.Biography, 4000, cutAtNewLine: false), user.Value.ExternalUrl);
        }
        #endregion IG Accounts
        #region Media
        /// <summary>
        /// Routes the url to the desired method.
        /// </summary>
        /// <param name="url">Link to the post.</param>
        /// <param name="guild">The guild that the message originated from. Used to determine max upload size.</param>
        /// <param name="postIndex">Post number in carousel.</param>
        /// <returns>Instagram processor response with related information.</returns>
        public async Task<InstagramProcessorResponse> PostRouter(string url, SocketGuild guild, int postIndex = 1)
        {
            //No guild in DMs
            if (guild == null)
            {
                return await PostRouter(url, 0, postIndex);
            }
            return await PostRouter(url, ((int)guild.PremiumTier), postIndex);
        }
        /// <summary>
        /// Routes the url to the desired method.
        /// </summary>
        /// <param name="url">Link to the post.</param>
        /// <param name="tier">Discord nitro tier</param>
        /// <param name="postIndex">Post number in carousel.</param>
        /// <returns>Instagram processor response with related information.</returns>
        public async Task<InstagramProcessorResponse> PostRouter(string url, int tier, int postIndex = 1)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("URL is null.");
            }
            Uri link;
            //Check for valid link:
            try
            {
                link = new Uri(url);
            }
            catch (System.UriFormatException)
            {
                return new InstagramProcessorResponse("Malformed URL.");
            }
            //Ensure link is an Instagram Link:
            if (!(link.DnsSafeHost.ToLower().Equals("www.instagram.com") || link.DnsSafeHost.ToLower().Equals("instagram.com")))
            {
                return new InstagramProcessorResponse("Not a recognized Instagram link.");
            }
            if (!(link.Scheme.ToLower().Equals("https") || link.Scheme.ToLower().Equals("http")))
            {
                return new InstagramProcessorResponse("Link must be served over http or https.");
            }

            //Process the link:
            //process account
            if (isProfileLink(link))
            {
                return await ProcessAccountAsync(link, tier);
            }
            //Process story
            if (isStory(link))
            {
                return await StoryProcessorAsync(url, tier);
            }
            //TODO Highlights:
            else if (isHighlight(link))
            {
                return new InstagramProcessorResponse("Highlights are not supported yet.");
            }
            //all others:
            else
            {
                return await PostProcessorAsync(url, postIndex, tier);
            }
        }
        /// <summary>
        /// Decides if a link is a story or not.
        /// </summary>
        /// <param name="url">Link to the content.</param>
        /// <returns>True if the post is a story.</returns>
        private static bool isStory(Uri url)
        {
            //Stories URL Starts with stories
            //https://instagram.com/stories/google/2733780123514124411?utm_source=ig_story_item_share&utm_medium=copy_link
            return url.Segments[1].Equals("stories/");
        }
        /// <summary>
        /// Decides if the link is a profile link or not.
        /// </summary>
        /// <param name="url">Link to the content</param>
        /// <returns>True if it is a profile link</returns>
        public static bool isProfileLink(Uri url)
        {
            // a profile link should have two segments / and profile/
            // Ex: https://instagram.com/google
            return url.Segments.Length == 2;
        }
        /// <summary>
        /// Check to see if the URL is a IG highlight
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static bool isHighlight(Uri url)
        {
            //Highlights URL Starts with an s
            //https://www.instagram.com/s/aGlnaGxpZ2h0OjE3ODU1NDgxNjc0NjE3ODM0?story_media_id=2643763694163262580&utm_medium=copy_link
            return url.Segments[1].Equals("s/");
        }
        /// <summary>
        /// Processes an Instagram post.
        /// </summary>
        /// <param name="url">Link to the post</param>
        /// <param name="index">Post number in carousel</param>
        /// <param name="premiumTier">Discord Nitro tier. For max file upload size.</param>
        /// <returns>Instagram processor response with related information.</returns>
        public async Task<InstagramProcessorResponse> PostProcessorAsync(string url, int index, int premiumTier, InstagramApiSharp.Classes.Models.InstaMedia media = null)
        {
            //Arrays start at zero:
            index--;

            //Check for 'prefed' media
            if (media == null)
            {
                //parse url:
                InstagramApiSharp.Classes.IResult<string> mediaId;
                InstagramApiSharp.Classes.IResult<InstagramApiSharp.Classes.Models.InstaMedia> mediaSource;

                //Get the media ID:
                mediaId = await instaApi.MediaProcessor.GetMediaIdFromUrlAsync(new Uri(url));

                //Check to see if it worked:
                if (!mediaId.Succeeded)
                {
                    return HandleFailure(mediaId);
                }
                else if (mediaId.Value == null)
                {
                    return new InstagramProcessorResponse("No post information returned. Profile links are not supported (if applicable).");
                }
                else
                {
                    mediaSource = (await instaApi.MediaProcessor.GetMediaByIdAsync(mediaId.Value));
                    //Check for failure:
                    if (!mediaSource.Succeeded)
                    {
                        HandleFailure(mediaSource);
                    }
                }

                media = mediaSource.Value;
            }
            //Create URL if it isnt already sent.
            if (string.IsNullOrEmpty(url))
            {
                url = "https://www.instagram.com/p/" + media.Code;
            }

            string caption = "";
            //check caption value (ensure not null)
            if (media.Caption != null)
            {
                caption = media.Caption.Text;
            }

            //inject image from carousel:
            if (media.Carousel != null && media.Carousel.Count > 0)
            {
                if (media.Carousel.Count <= index)
                {
                    return new InstagramProcessorResponse("Index out of bounds. There is only " + media.Carousel.Count + " Posts.");
                }
                if (media.Carousel[index].Videos.Count > 0)
                {
                    var video = media.Carousel[index].Videos[0];
                    media.Videos.Add(video);
                }
                else
                {
                    var image = media.Carousel[index].Images[0];
                    media.Images.Add(image);
                }
            }
            //get upload tier:
            long maxUploadSize = DiscordTools.MaxUploadSize(premiumTier);

            bool isVideo = media.Videos.Count > 0;
            string downloadUrl = "";

            //Video or image:
            if (isVideo)
            {
                //video:
                downloadUrl = media.Videos[0].Uri;
            }
            else
            {
                //Image:
                downloadUrl = media.Images[0].Uri;
            }

            //Return downloaded content (if possible):
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.MaxResponseContentBufferSize = maxUploadSize;
                    var response = await client.GetAsync(downloadUrl);
                    byte[] data = await response.Content.ReadAsByteArrayAsync();
                    //If statement to double check size.
                    if (data.Length < maxUploadSize)
                    {
                        //No account information avaliable:
                        return new InstagramProcessorResponse(isVideo, caption, media.User.FullName, media.User.UserName, new Uri(media.User.ProfilePicture), downloadUrl, url, media.TakenAt, data);
                    }

                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("Cannot write more bytes to the buffer than the configured maximum buffer size"))
                {
                    //File too big to upload to discord. Just ignore the error.
                }
                else
                {
                    //Unexpected error:
                    Console.WriteLine("HttpRequestException Error:\n" + e);
                }
            }
            catch (Exception e)
            {
                //Log Error:
                Console.WriteLine(e);
            }
            //Fallback to URL:
            //No account information avaliable:
            return new InstagramProcessorResponse(true, caption, media.User.FullName, media.User.UserName, new Uri(media.User.ProfilePicture), downloadUrl, url, media.TakenAt, null);
        }
        /// <summary>
        /// Processes an Instagram story.
        /// Doesnt work with highlights.
        /// </summary>
        /// <param name="url">Link to the story.</param>
        /// <param name="premiumTier">Discord Nitro tier. For max file upload size.</param>
        /// <returns>Instagram processor response with related information.</returns>
        public async Task<InstagramProcessorResponse> StoryProcessorAsync(string url, int premiumTier)
        {
            Uri link = new Uri(url);
            string userName = link.Segments[2].Replace("/", "");
            string storyID = link.Segments[3].Replace("/", "");

            //get user:
            var user = await instaApi.UserProcessor.GetUserAsync(userName);

            // On failed to get user:
            if (!user.Succeeded)
            {
                return HandleFailure(user);
            }
            else if (user.Value.IsPrivate)
            {
                return new InstagramProcessorResponse("The account is private.");
            }

            //Get user data:
            long userId = user.Value.Pk;
            long followers = -1, following = -1, posts = -1;
            string bio = "";
            try
            {
                var userData = await instaApi.UserProcessor.GetUserInfoByIdAsync(userId);

                followers = userData.Value.FollowerCount;
                following = userData.Value.FollowingCount;
                posts = userData.Value.MediaCount;
                bio = (string.IsNullOrEmpty(userData.Value.Biography)) ? ("") : (userData.Value.Biography);
            }
            catch (Exception e)
            {
                //Not too important, but log anyway.
                Console.WriteLine("Error loading profile info. Error: " + e);
                bio = "Error loading profile information.";
            }

            //Get the story:
            var stories = await instaApi.StoryProcessor.GetUserStoryAsync(userId);
            if (!stories.Succeeded)
            {
                return new InstagramProcessorResponse("Failed to load stories for the user. Support Server: https://discord.gg/6K3tdsYd6J");
            }
            if (stories.Value.Items.Count == 0)
            {
                return new InstagramProcessorResponse("No stories exist for that user.");
            }
            foreach (var story in stories.Value.Items)
            {
                //find story:
                if (story.Id.Contains(storyID))
                {
                    long maxUploadSize = DiscordTools.MaxUploadSize(premiumTier);
                    bool isVideo = story.VideoList.Count > 0;
                    string downloadUrl = "";

                    if (isVideo)
                    {
                        //process video:
                        downloadUrl = story.VideoList[0].Uri;
                    }
                    else if (story.ImageList.Count > 0)
                    {
                        //Image:
                        downloadUrl = story.ImageList[0].Uri;

                    }
                    else
                    {
                        return new InstagramProcessorResponse("This story uses a format that we do not support.");
                    }
                    //Return downloaded content (if possible):
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.MaxResponseContentBufferSize = maxUploadSize;
                            var response = await client.GetAsync(downloadUrl);
                            byte[] data = await response.Content.ReadAsByteArrayAsync();
                            //If statement to double check size.
                            if (data.Length < maxUploadSize)
                            {
                                return new InstagramProcessorResponse(isVideo, "", story.User.FullName, story.User.UserName, new Uri(story.User.ProfilePicture), downloadUrl, url, story.TakenAt, data);
                            }

                        }
                    }
                    catch (HttpRequestException e)
                    {
                        if (e.Message.Contains("Cannot write more bytes to the buffer than the configured maximum buffer size"))
                        {
                            //File too big to upload to discord. Just ignore the error.
                        }
                        else
                        {
                            //Unexpected error:
                            Console.WriteLine("HttpRequestException Error:\n" + e);
                        }
                    }
                    catch (Exception e)
                    {
                        //Log Error:
                        Console.WriteLine(e);
                    }
                    //Fallback to URL:
                    return new InstagramProcessorResponse(true, "", story.User.FullName, story.User.UserName, new Uri(story.User.ProfilePicture), downloadUrl, url, story.TakenAt, null);
                }
            }
            return new InstagramProcessorResponse("Could not find story.");
        }
        /// <summary>
        /// Gets the latest instagram posts from a user account:
        /// </summary>
        /// <param name="userID">IG User ID</param>
        /// <param name="startDate">Time of the last post.</param>
        /// <returns></returns>
        public async Task<InstagramProcessorResponse[]> PostsSinceDate(long userID, DateTime startDate)
        {
            //get the IG user:
            var user = (await instaApi.UserProcessor.GetUserInfoByIdAsync(userID)).Value;
            List<InstagramProcessorResponse> responses = new List<InstagramProcessorResponse>();
            var LatestMedia = (await instaApi.UserProcessor.GetUserMediaAsync(user.Username, PaginationParameters.MaxPagesToLoad(1))).Value;

            if (LatestMedia == null)
            {
                responses.Add(new InstagramProcessorResponse("Cannot get profile."));
                return responses.ToArray();
            }
            //Only show the latest 4 for resource reasons:
            if (LatestMedia.Count > 4)
            {
                LatestMedia.RemoveRange(4, LatestMedia.Count - 4);
            }
            foreach (var media in LatestMedia)
            {
                if (media.TakenAt.ToUniversalTime() > startDate.ToUniversalTime())
                {
                    //Add post to array:
                    responses.Insert(0, await PostProcessorAsync(null, 1, 0, media));
                }
            }
            return responses.ToArray();
        }
        /// <summary>
        /// Gets the date of the last instagram post from a user.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public async Task<DateTime> GetLatestPostDate(long userID)
        {
            //get the IG user:
            var user = (await instaApi.UserProcessor.GetUserInfoByIdAsync(userID)).Value;

            var LatestMedia = (await instaApi.UserProcessor.GetUserMediaAsync(user.Username, PaginationParameters.MaxPagesToLoad(1))).Value;
            //Ensure there are posts:
            if (LatestMedia == null || LatestMedia.Count == 0)
            {
                Console.WriteLine("Cannot see profile. May be private.");
                return DateTime.UnixEpoch;
            }
            return LatestMedia[0].TakenAt;
        }
        #endregion Media
        #region Errors
        /// <summary>
        /// Handle failures
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private InstagramProcessorResponse HandleFailure(IResult<object> result)
        {
            switch (result.Info.ResponseType)
            {
                case ResponseType.ChallengeRequired:
                case ResponseType.LoginRequired:
                    throw new Exception("Relogin required");
                case ResponseType.MediaNotFound:
                    return new InstagramProcessorResponse("Could not find that post. Is the account private?");
                case ResponseType.DeletedPost:
                    return new InstagramProcessorResponse("The post was deleted from Instagram.");
                case ResponseType.NetworkProblem:
                    return new InstagramProcessorResponse("Could not connect to Instagram. Is Instagram down? https://discord.gg/6K3tdsYd6J");
                case ResponseType.UnExpectedResponse:
                    if (result.Info.Message.Contains("User not found"))
                    {
                        return new InstagramProcessorResponse("User not found.");
                    }
                    else
                    {
                        goto default;
                    }
                default:
                    Console.WriteLine("Error: " + result.Info);
                    return new InstagramProcessorResponse("Error retrieving the content. The account may be private. Please report this on our support server if the account is public or if this is unexpected. https://discord.gg/6K3tdsYd6J");
            }
        }
        #endregion Errors
    }
}