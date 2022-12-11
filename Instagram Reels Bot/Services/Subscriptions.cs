using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;
using Instagram_Reels_Bot.DataTables;
using Instagram_Reels_Bot.Helpers;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Instagram_Reels_Bot.Services
{
    /// <summary>
    /// Allows users to get the lastest posts from an account
    /// </summary>
	public class Subscriptions
    {
        private readonly IConfiguration _config;
        private System.Timers.Timer UpdateTimer;
        private readonly DiscordShardedClient _client;
        //CosmosDB:
        private static string MongoDB_ConnectionString;
        // The Cosmos db client instance
        private MongoClient mongoClient;
        // Add the Database:
        private IMongoDatabase Database;
        // Followed Accounts Container
        private IMongoCollection<FollowedIGUser> FollowedAccountsContainer;
        private IMongoCollection<PremiumGuild> PremiumGuildsContainer;
        //To ensure that the loop is only run one at a time.
        private static bool InSubLoop = false;
        //Enable and disable the module
        public readonly bool ModuleEnabled;

        /// <summary>
        /// Initialize sub
        /// </summary>
        /// <param name="services"></param>
        public Subscriptions(DiscordShardedClient client, IConfiguration config)
        {
            // Dependancy injection:
            _config = config;
            _client = client;

            // Dont set database locations unless AllowSubscriptions is true:
            if (config["AllowSubscriptions"].ToLower() != "true")
            {
                //Disable the module:
                ModuleEnabled = false;
                Console.WriteLine("Subscriptions not allowed.");
                return;
            }
            // Enable the module:
            ModuleEnabled = true;

            // Set cosmos DB info:
            MongoDB_ConnectionString = config["MongoDBUrl"];
            // Check for old Cosmos Settings and warn the user:
            if (string.IsNullOrEmpty(MongoDB_ConnectionString))
            {
                if (!string.IsNullOrEmpty(config["EndpointUrl"]))
                {
                    Console.WriteLine("CosmosDB is no longer supported. Please migrate to MongoDB.");
                }
            }
        }
        /// <summary>
        /// Starts the subscription tasks.
        /// TODO: Check this initialize method
        /// </summary>
        /// <returns></returns>
        public void Initialize()
        {
            Console.WriteLine("Starting the subscription task...");
            if (string.IsNullOrEmpty(MongoDB_ConnectionString) || !ModuleEnabled)
            {
                Console.WriteLine("Databases not setup or module disabled.");
                return;
            }
            //Connect to Database:
            this.mongoClient = new MongoClient(MongoDB_ConnectionString);

            //Override database for debugging:
            string databaseName = "InstagramEmbedDatabase";
#if (DEBUG)
            databaseName = "InstagramEmbedDatabaseDev";
#endif
            //link and create the database if it is missing:
            this.Database = this.mongoClient.GetDatabase(databaseName);
            this.FollowedAccountsContainer = this.Database.GetCollection<FollowedIGUser>("FollowedAccounts");
            this.PremiumGuildsContainer = this.Database.GetCollection<PremiumGuild>("PremiumGuilds");

            UpdateTimer = new System.Timers.Timer(3600000.0 * double.Parse(_config["HoursToCheckForNewContent"])); //one hour in milliseconds
            UpdateTimer.Elapsed += new ElapsedEventHandler(GetLatestsPosts);
            UpdateTimer.Start();
        }
        /// <summary>
        /// Subscribe a channel to an Instagram account.
        /// TODO: check count per guild.
        /// </summary>
        /// <param name="instagramID"></param>
        /// <param name="channelID"></param>
        /// <param name="guildID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task SubscribeToAccount(long instagramID, ulong channelID, ulong guildID)
        {
            FollowedIGUser databaseValue;
            try // search InstagramID on database
            {
                databaseValue = await FollowedAccountsContainer.Find(followedAccount => followedAccount.InstagramID.Equals(instagramID.ToString())).FirstOrDefaultAsync();
            }
            catch (MongoException)
            {
                databaseValue = null;
            }
            //Create new Entry:
            if (databaseValue == null)
            {
                List<RespondChannel> channels = new List<RespondChannel>();
                channels.Add(new RespondChannel(guildID, channelID));
                databaseValue = new FollowedIGUser
                {
                    InstagramID = instagramID.ToString(),
                    SubscribedChannels = channels
                };
                //Create the Item:
                await this.FollowedAccountsContainer.InsertOneAsync(databaseValue);
            }
            else
            {
                foreach(RespondChannel channel in databaseValue.SubscribedChannels)
                {
                    if(channel.ChannelID == channelID.ToString())
                    {
                        //Already subscribed:
                        throw new ArgumentException("Already subscribed");
                    }
                }
                databaseValue.SubscribedChannels.Add(new RespondChannel(guildID, channelID));
                await this.FollowedAccountsContainer.ReplaceOneAsync(x => x.InstagramID == databaseValue.InstagramID, databaseValue, new ReplaceOptions { IsUpsert = true });
            }
        }
        /// <summary>
        /// Unsubscribes a channel from an Instagram Account
        /// </summary>
        /// <param name="instagramID"></param>
        /// <param name="channelID"></param>
        /// <param name="guildID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task UnsubscribeToAccount(long instagramID, ulong channelID, ulong guildID)
        {
            FollowedIGUser databaseValue;
            try
            {
                databaseValue = await FollowedAccountsContainer.Find(x => x.InstagramID.Equals(instagramID.ToString())).FirstOrDefaultAsync();
            }
            catch (MongoException) //ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new ArgumentException("Cannot find user.");
            }
            if (databaseValue == null)
            {
                throw new ArgumentException("Cannot find user.");
            }
            foreach (RespondChannel chan in databaseValue.SubscribedChannels)
            {
                if (ulong.Parse(chan.ChannelID) == channelID)
                {
                    databaseValue.SubscribedChannels.Remove(chan);
                    break;
                }
            }
            if (databaseValue.SubscribedChannels.Count > 0)
            {
                // Update item:
                await this.FollowedAccountsContainer.ReplaceOneAsync(x => x.InstagramID == databaseValue.InstagramID, databaseValue, new ReplaceOptions { IsUpsert = true });
            }
            else
            {
                //Delete if empty
                await this.FollowedAccountsContainer.DeleteOneAsync(x => x.InstagramID == databaseValue.InstagramID);
            }
        }
        /// <summary>
        /// Returns true if GetLatestsPosts() loop is running.
        /// </summary>
        /// <returns></returns>
        public bool CurrentlyCheckingAccounts()
        {
            return InSubLoop;
        }
        /// <summary>
        /// Main loop to parse through all subscribed accounts and upload their contents.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetLatestsPosts(object sender, System.Timers.ElapsedEventArgs e)
        {
            await GetLatestsPosts();
        }
        /// <summary>
        /// Gets the latests posts for all subscriptions
        /// </summary>
        /// <returns></returns>
        public async Task GetLatestsPosts()
        {
            //Ensure module is enabled.
            if (!ModuleEnabled)
            {
                Console.WriteLine("Module disabled.");
                return;
            }

            if (InSubLoop)
            {
                //Prevents multiple loops running at once which could cause an instagram block.
                Console.WriteLine("Already in loop. Skipping.");
                return;
            }
            else
            {
                InSubLoop = true;
            }
            try
            {
                //Unsubscribe oversubs:
                await UnsubscribeOverSubscriptions();

                Console.WriteLine("Getting new posts!");
                var getdbfeed = await FollowedAccountsContainer.Find(_ => true).ToListAsync();

                //Randomize the order of the IG accounts:
                Random rand = new Random();
                getdbfeed.OrderBy(item => rand.Next());

                foreach (var dbfeed in getdbfeed)
                {
                    Console.WriteLine("Checking " + dbfeed.InstagramID);
                    try
                    {
                        // Get IG account:
                        InstagramProcessor instagram = new InstagramProcessor(InstagramProcessor.AccountFinder.GetIGAccount());

                        //Check to see if there is any channel that is subscribed to IG accounts:
                        if (dbfeed.SubscribedChannels.Count == 0)
                        {
                            //If not, delete.
                            await this.FollowedAccountsContainer.DeleteOneAsync(x => x.InstagramID == dbfeed.InstagramID);
                        }
                        else //Otherwise proceed:
                        {
                            //Set last check as now:
                            dbfeed.LastCheckTime = DateTime.Now;
                            var newIGPosts = await instagram.PostsSinceDate(long.Parse(dbfeed.InstagramID), dbfeed.LastPostDate);
                            if (newIGPosts.Length > 0 && newIGPosts[newIGPosts.Length - 1].success)
                            {
                                //Set the most recent posts date:
                                dbfeed.LastPostDate = newIGPosts[newIGPosts.Length - 1].postDate;
                            }
                            foreach (InstagramProcessorResponse response in newIGPosts)
                            {
                                List<RespondChannel> invalidChannels = new List<RespondChannel>();
                                foreach (RespondChannel subbedGuild in dbfeed.SubscribedChannels)
                                {
                                    if (response.success)
                                    {
                                        //Create component builder:
                                        IGComponentBuilder component = new IGComponentBuilder(response, _config);
                                        //Create embed response:
                                        IGEmbedBuilder embed = new IGEmbedBuilder(response);

                                        if (!response.success)
                                        {
                                            //Failed to process post:
                                            Console.WriteLine("Failed to process post.");
                                            return;
                                        }
                                        else if (response.isVideo)
                                        {
                                            if (response.stream != null)
                                            {
                                                //Response with stream:
                                                using (Stream stream = new MemoryStream(response.stream))
                                                {
                                                    FileAttachment attachment = new FileAttachment(stream, "IGMedia.mp4", "An Instagram Video.");
                                                    // get channel:
                                                    IMessageChannel chan = null;
                                                    try
                                                    {
                                                        chan = _client.GetChannel(ulong.Parse(subbedGuild.ChannelID)) as IMessageChannel;
                                                    }
                                                    catch
                                                    {
                                                        Console.WriteLine("Cannot find channel. Removing from DB.");
                                                        invalidChannels.Add(subbedGuild);
                                                    }
                                                    if (chan != null)
                                                    {
                                                        //send message
                                                        await chan.SendFileAsync(attachment, embed: embed.AutoSelector(), components: component.AutoSelector());
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Cannot find channel. Removing from DB.");
                                                        invalidChannels.Add(subbedGuild);
                                                    }

                                                }
                                            }
                                            else
                                            {
                                                //Response without stream:
                                                // get channel:
                                                IMessageChannel chan = null;
                                                try
                                                {
                                                    chan = _client.GetChannel(ulong.Parse(subbedGuild.ChannelID)) as IMessageChannel;
                                                }
                                                catch
                                                {
                                                    Console.WriteLine("Cannot find channel. Removing from DB.");
                                                    invalidChannels.Add(subbedGuild);
                                                }
                                                if (chan != null)
                                                {
                                                    //send message
                                                    await chan.SendMessageAsync(response.contentURL.ToString(), embed: embed.AutoSelector(), components: component.AutoSelector());
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Cannot find channel. Removing from DB.");
                                                    invalidChannels.Add(subbedGuild);
                                                }

                                            }

                                        }
                                        else
                                        {
                                            if (response.stream != null)
                                            {
                                                using (Stream stream = new MemoryStream(response.stream))
                                                {
                                                    FileAttachment attachment = new FileAttachment(stream, "IGMedia.jpg", "An Instagram Image.");

                                                    // get channel:
                                                    IMessageChannel chan = null;
                                                    try
                                                    {
                                                        chan = _client.GetChannel(ulong.Parse(subbedGuild.ChannelID)) as IMessageChannel;
                                                    }
                                                    catch
                                                    {
                                                        Console.WriteLine("Cannot find channel.");
                                                    }
                                                    if (chan != null)
                                                    {
                                                        //send message
                                                        await chan.SendFileAsync(attachment, embed: embed.AutoSelector(), components: component.AutoSelector());
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Cannot find channel. Removing from DB.");
                                                        invalidChannels.Add(subbedGuild);
                                                    }

                                                }
                                            }
                                            else
                                            {
                                                // get channel:
                                                IMessageChannel chan = null;
                                                try
                                                {
                                                    chan = _client.GetChannel(ulong.Parse(subbedGuild.ChannelID)) as IMessageChannel;
                                                }
                                                catch
                                                {
                                                    Console.WriteLine("Cannot find channel. Removing from DB.");
                                                    invalidChannels.Add(subbedGuild);
                                                }
                                                if (chan != null)
                                                {
                                                    //send message
                                                    try
                                                    {
                                                        await chan.SendMessageAsync(embed: embed.AutoSelector(), components: component.AutoSelector());
                                                    }catch(Exception e)
                                                    {
                                                        Console.WriteLine("Error sending subscription message. Error: " + e);
                                                        invalidChannels.Add(subbedGuild);
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Cannot find channel. Removing from DB.");
                                                    invalidChannels.Add(subbedGuild);
                                                }

                                            }
                                        }
                                    }
                                    else if (response.error == "NullAccount")
                                    {
                                        Console.WriteLine("Removing null account: "+ dbfeed.InstagramID);
                                        invalidChannels.Add(subbedGuild);
                                    }
                                    else
                                    {
                                        //TODO: Decide if the user should be informed or not. May create spam.
                                        Console.WriteLine("Failed auto post. ID: " + dbfeed.InstagramID);
                                        //var chan = _client.GetChannel(ulong.Parse(subbedGuild.ChannelID)) as IMessageChannel;
                                        //string igUsername = await instagram.GetIGUsername(dbfeed.InstagramID);
                                        //await chan.SendMessageAsync("Failed to get latest posts for " + igUsername + ". Use `/unsubscribe " + igUsername + "` to remove the inaccessible account.");
                                    }
                                }
                                //Remove all invalid channels:
                                if (_config["DisableSubscribeCleanup"] is null || !bool.Parse(_config["DisableSubscribeCleanup"]))
                                {
                                    invalidChannels.ForEach(item => dbfeed.SubscribedChannels.RemoveAll(c => c.ChannelID.Equals(item.ChannelID)));
                                }
                                else
                                {
                                    Console.WriteLine("Subscribe cleanup is disabled.");
                                }
                            }
                            //Update database:
                            await this.FollowedAccountsContainer.ReplaceOneAsync(x => x.InstagramID == dbfeed.InstagramID, dbfeed, new ReplaceOptions { IsUpsert = true });

                            // Wait to prevent spamming IG api
                            // Get value from config:
                            int time;
                            _ = int.TryParse(_config["SubscribeCheckDelayTime"], out time);
                            // Enforce a minimum of 10 seconds.
                            time = Math.Max(time, 10);
                            await Task.Delay(time * 1000);
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Failed to get updates for IG account. Error: "+e);
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Error with update loop: " + e);
            }
            finally
            {
                //Always mark as done loop when exiting:
                InSubLoop = false;

                Console.WriteLine("Done.");
            }
        }
        /// <summary>
        /// Gets the number of channels that a user is susbscribed to.
        /// </summary>
        /// <returns></returns>
        public async Task<int> GuildSubscriptionCountAsync(ulong guildID)
        {
            try
            {
                List<FollowedIGUser> databaseValue = await FollowedAccountsContainer.Find(x => x.SubscribedChannels.Any(n => n.GuildID.Equals(guildID.ToString()))).ToListAsync();
                return databaseValue.Count;
            }
            catch (MongoException ex)
            {
                throw new Exception("Cannot find user in database. Error: "+ex);
            }
        }
        /// <summary>
        /// The accounts that a guild is subscribed to.
        /// </summary>
        /// <param name="guildID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<FollowedIGUser[]> GuildSubscriptionsAsync(ulong guildID)
        {
            try
            {
                List<FollowedIGUser> databaseValue = await FollowedAccountsContainer.Find(x => x.SubscribedChannels.Any(n => n.GuildID.Equals(guildID.ToString()))).ToListAsync();
                return databaseValue.ToArray();
            }
            catch (MongoException)
            {
                throw new Exception("Cannot find user.");
            }
        }
        /// <summary>
        /// The maximum amount of instagram users that a guild can be subscribed to.
        /// </summary>
        /// <param name="guildID"></param>
        /// <returns></returns>
        public async Task<int> MaxSubscriptionsCountForGuildAsync(ulong guildID)
        {
            int max = int.Parse(_config["DefaultSubscriptionsPerGuildMax"]);

            try
            {
                PremiumGuild databaseValue = await PremiumGuildsContainer.Find(x => x.GuildID.Equals(guildID.ToString())).FirstOrDefaultAsync();
                if (databaseValue != null)
                {
                    max += int.Parse(databaseValue.AdditionalAccounts.ToString());
                }
            }
            catch (MongoException ex)
            {
                //Not premium
                Console.WriteLine(max.ToString() + " MongoException\n"+ex);
            }

            return max;
        }
        /// <summary>
        /// Checks any account with the RecheckSubscribedAccounts bool set to true. Unsubscribes if applicable.
        /// </summary>
        /// <returns></returns>
        public async Task UnsubscribeOverSubscriptions()
        {
            // Get IG account:
            InstagramProcessor instagram = new InstagramProcessor(InstagramProcessor.AccountFinder.GetIGAccount());

            List<PremiumGuild> queryable = PremiumGuildsContainer.Find(x => x.RecheckSubscribedAccounts).ToListAsync().Result;

            foreach (PremiumGuild pguild in queryable.ToArray())
            {
                int maxAccounts = await MaxSubscriptionsCountForGuildAsync(ulong.Parse(pguild.GuildID));
                int currentAccounts = await GuildSubscriptionCountAsync(ulong.Parse(pguild.GuildID));
                if (currentAccounts > maxAccounts)
                {
                    Console.WriteLine("Guild over limit.");

                    int NumberOfAccountsToRemove = currentAccounts - maxAccounts;

                    List<FollowedIGUser> queryableIG = FollowedAccountsContainer.Find(x => x.SubscribedChannels.Any(n => n.GuildID == pguild.GuildID)).ToListAsync().Result;
                    foreach (FollowedIGUser igAccount in queryableIG.ToArray())
                    {
                        if (NumberOfAccountsToRemove <= 0)
                        {
                            break;
                        }
                        //Get all to be removed:
                        RespondChannel[] chans = igAccount.SubscribedChannels.FindAll(item => item.GuildID.Equals(pguild.GuildID)).ToArray();
                        foreach (RespondChannel chan in chans)
                        {
                            //Remove:
                            igAccount.SubscribedChannels.Remove(chan);

                            //Notify:
                            var discordChan = _client.GetChannel(ulong.Parse(chan.ChannelID)) as IMessageChannel;
                            await discordChan.SendMessageAsync("This channel has been automatically unsubscribed to " + (await instagram.GetIGUsername(igAccount.InstagramID)) + " as it exceeded the guild's maximum subscription limit.");
                        }
                        //Update Database:
                        await this.FollowedAccountsContainer.ReplaceOneAsync(x => x.InstagramID == igAccount.InstagramID, igAccount, new ReplaceOptions { IsUpsert = true });

                        NumberOfAccountsToRemove--;
                    }
                }
                pguild.RecheckSubscribedAccounts = false;
                await this.PremiumGuildsContainer.ReplaceOneAsync(x => x.GuildID == pguild.GuildID, pguild, new ReplaceOptions { IsUpsert = true });
            }
        }
    }
}
