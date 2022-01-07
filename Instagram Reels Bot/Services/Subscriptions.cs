using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;
using Instagram_Reels_Bot.DataTables;
using Instagram_Reels_Bot.Helpers;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Instagram_Reels_Bot.Services
{
    /// <summary>
    /// Allows users to get the lastest posts from an account
    /// </summary>
	public class Subscriptions
    {
        private readonly IConfiguration _config;
        private readonly IServiceProvider services;
        private System.Timers.Timer UpdateTimer;
        private readonly DiscordShardedClient _client;
        //CosmosDB:
        private static string EndpointUri;
        private static string PrimaryKey;
        // The Cosmos db client instance
        private CosmosClient CosmosClient;
        // Add the Database:
        private Database Database;
        // Followed Accounts Container
        private Container FollowedAccountsContainer;
        private Container PremiumGuildsContainer;
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

            //Dont set database locations unless AllowSubscriptions is true:
            if (config["AllowSubscriptions"].ToLower() != "true")
            {
                //Disable the module:
                ModuleEnabled = false;
                Console.WriteLine("Subscriptions not allowed.");
                return;
            }
            //Enable the module:
            ModuleEnabled = true;

            //Set cosmos DB info:
            EndpointUri = config["EndpointUrl"];
            PrimaryKey = config["PrimaryKey"];
        }
        /// <summary>
        /// Starts the subscription tasks.
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            Console.WriteLine("Starting the subscription task...");
            if (string.IsNullOrEmpty(PrimaryKey) || string.IsNullOrEmpty(EndpointUri))
            {
                Console.WriteLine("Databases not setup.");
                return;
            }
            //Connect to Database:
            this.CosmosClient = new CosmosClient(EndpointUri, PrimaryKey);

            //link and create the database if it is missing:
            this.Database = await this.CosmosClient.CreateDatabaseIfNotExistsAsync("InstagramEmbedDatabase");
            this.FollowedAccountsContainer = await this.Database.CreateContainerIfNotExistsAsync("FollowedAccounts", "/id");
            this.PremiumGuildsContainer = await this.Database.CreateContainerIfNotExistsAsync("PremiumGuilds", "/id");

            // Timer:
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
            try
            {
                IQueryable<FollowedIGUser> queryable = FollowedAccountsContainer.GetItemLinqQueryable<FollowedIGUser>(true);
                queryable = queryable.Where<FollowedIGUser>(item => item.InstagramID.Equals(instagramID.ToString()));
                databaseValue = queryable.ToArray().FirstOrDefault(defaultValue: null);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                databaseValue = null;
            }
            //Create new Entry:
            if (databaseValue == null)
            {
                List<RespondChannel> chans = new List<RespondChannel>();
                chans.Add(new RespondChannel(guildID, channelID));
                databaseValue = new FollowedIGUser
                {
                    InstagramID = instagramID.ToString(),
                    SubscribedChannels = chans
                };
                //Create the Item:
                await this.FollowedAccountsContainer.CreateItemAsync<FollowedIGUser>(databaseValue, new PartitionKey(databaseValue.InstagramID));
            }
            else
            {
                foreach(RespondChannel chan in databaseValue.SubscribedChannels)
                {
                    if(ulong.Parse(chan.ChannelID) == channelID)
                    {
                        //Already subscribed:
                        throw new ArgumentException("Already subscribed");
                    }
                }
                databaseValue.SubscribedChannels.Add(new RespondChannel(guildID, channelID));
                await this.FollowedAccountsContainer.UpsertItemAsync<FollowedIGUser>(databaseValue, new PartitionKey(databaseValue.InstagramID));
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
                IQueryable<FollowedIGUser> queryable = FollowedAccountsContainer.GetItemLinqQueryable<FollowedIGUser>(true);
                queryable = queryable.Where<FollowedIGUser>(item => item.InstagramID.Equals(instagramID.ToString()));
                databaseValue = queryable.ToArray().FirstOrDefault(defaultValue: null);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception("Cannot find user.");
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
                await this.FollowedAccountsContainer.UpsertItemAsync<FollowedIGUser>(databaseValue, new PartitionKey(databaseValue.InstagramID));
            }
            else
            {
                //Delete if empty
                await this.FollowedAccountsContainer.DeleteItemAsync<FollowedIGUser>(databaseValue.InstagramID, new PartitionKey(databaseValue.InstagramID));
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
                using (FeedIterator<FollowedIGUser> dbfeed = FollowedAccountsContainer.GetItemQueryIterator<FollowedIGUser>())
                {
                    while (dbfeed.HasMoreResults)
                    {
                        foreach (var igAccount in await dbfeed.ReadNextAsync())
                        {
                            Console.WriteLine("Checking " + igAccount.InstagramID);

                            //Check to see if their is any subscribed accounts:
                            if (igAccount.SubscribedChannels.Count == 0)
                            {
                                //If not, delete.
                                await this.FollowedAccountsContainer.DeleteItemAsync<FollowedIGUser>(igAccount.InstagramID, new PartitionKey(igAccount.InstagramID));
                            }
                            else //Otherwise proceed:
                            {

                                //Set last check as now:
                                igAccount.LastCheckTime = DateTime.Now;
                                var newIGPosts = await InstagramProcessor.PostsSinceDate(long.Parse(igAccount.InstagramID), igAccount.LastPostDate);
                                if (newIGPosts.Length > 0 && newIGPosts[newIGPosts.Length - 1].success)
                                {
                                    //Set the most recent posts date:
                                    igAccount.LastPostDate = newIGPosts[newIGPosts.Length - 1].postDate;
                                }
                                foreach (InstagramProcessorResponse response in newIGPosts)
                                {
                                    List<RespondChannel> invalidChannels = new List<RespondChannel>();
                                    foreach (RespondChannel subbedGuild in igAccount.SubscribedChannels)
                                    {
                                        if (response.success)
                                        {
                                            //Account Name:
                                            var account = new EmbedAuthorBuilder();
                                            account.IconUrl = response.iconURL.ToString();
                                            account.Name = response.accountName;
                                            account.Url = response.postURL.ToString();

                                            //Instagram Footer:
                                            EmbedFooterBuilder footer = new EmbedFooterBuilder();
                                            footer.IconUrl = "https://upload.wikimedia.org/wikipedia/commons/a/a5/Instagram_icon.png";
                                            footer.Text = "Instagram";

                                            var embed = new EmbedBuilder();
                                            embed.Author = account;
                                            embed.Footer = footer;
                                            embed.Timestamp = new DateTimeOffset(response.postDate);
                                            embed.Url = response.postURL.ToString();
                                            embed.Description = (response.caption != null) ? (DiscordTools.Truncate(response.caption)) : ("");
                                            embed.WithColor(new Color(131, 58, 180));

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
                                                        catch (Exception e)
                                                        {
                                                            Console.WriteLine("Cannot find channel. Removing from DB.");
                                                            invalidChannels.Add(subbedGuild);
                                                        }
                                                        if (chan != null)
                                                        {
                                                            //send message
                                                            await chan.SendFileAsync(attachment, embed: embed.Build());
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
                                                    catch (Exception e)
                                                    {
                                                        Console.WriteLine("Cannot find channel. Removing from DB.");
                                                        invalidChannels.Add(subbedGuild);
                                                    }
                                                    if (chan != null)
                                                    {
                                                        //send message
                                                        await chan.SendMessageAsync(response.contentURL.ToString(), embed: embed.Build());
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
                                                embed.ImageUrl = "attachment://IGMedia.jpg";
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
                                                        catch (Exception e)
                                                        {
                                                            Console.WriteLine("Cannot find channel. Removing from DB.");
                                                            invalidChannels.Add(subbedGuild);
                                                        }
                                                        if (chan != null)
                                                        {
                                                            //send message
                                                            await chan.SendFileAsync(attachment, embed: embed.Build());
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
                                                    embed.ImageUrl = response.contentURL.ToString();
                                                    // get channel:
                                                    IMessageChannel chan = null;
                                                    try
                                                    {
                                                        chan = _client.GetChannel(ulong.Parse(subbedGuild.ChannelID)) as IMessageChannel;
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Console.WriteLine("Cannot find channel. Removing from DB.");
                                                        invalidChannels.Add(subbedGuild);
                                                    }
                                                    if (chan != null)
                                                    {
                                                        //send message
                                                        await chan.SendMessageAsync(embed: embed.Build());
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Cannot find channel. Removing from DB.");
                                                        invalidChannels.Add(subbedGuild);
                                                    }

                                                }
                                            }
                                        }
                                        else
                                        {
                                            //TODO: Decide if the user should be informed or not. May create spam.
                                            Console.WriteLine("Failed auto post. ID: " + igAccount.InstagramID);
                                            var chan = _client.GetChannel(ulong.Parse(subbedGuild.ChannelID)) as IMessageChannel;
                                            string igUsername = await InstagramProcessor.GetIGUsername(igAccount.InstagramID);
                                            await chan.SendMessageAsync("Failed to get latest posts for " + igUsername + ". Use `/unsubscribe " + igUsername + "` to remove the inaccessible account.");
                                        }
                                    }

                                    //Remove all invalid channels:
                                    invalidChannels.ForEach(item => igAccount.SubscribedChannels.RemoveAll(c => c.ChannelID.Equals(item.ChannelID)));
                                }
                                //Update database:
                                await this.FollowedAccountsContainer.UpsertItemAsync<FollowedIGUser>(igAccount, new PartitionKey(igAccount.InstagramID));
                                //Wait to prevent spamming IG api:
                                Thread.Sleep(4000);
                            }
                        }
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
        public int GuildSubscriptionCount(ulong guildID)
        {
            try
            {
                IQueryable<FollowedIGUser> queryable = FollowedAccountsContainer.GetItemLinqQueryable<FollowedIGUser>(true);
                queryable = queryable.Where<FollowedIGUser>(item => item.SubscribedChannels.Any<RespondChannel>(n=>n.GuildID.Equals(guildID.ToString())));
                return queryable.Count();
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception("Cannot find user.");
            }
        }
        /// <summary>
        /// The maximum amount of instagram users that a guild can be subscribed to.
        /// </summary>
        /// <param name="guildID"></param>
        /// <returns></returns>
        public int MaxSubscriptionsCountForGuild(ulong guildID)
        {
            int max = int.Parse(_config["DefaultSubscriptionsPerGuildMax"]);

            try
            {
                IQueryable<PremiumGuild> queryable = PremiumGuildsContainer.GetItemLinqQueryable<PremiumGuild>(true);
                queryable = queryable.Where<PremiumGuild>(item=>item.GuildID.Equals(guildID.ToString()));
                max += int.Parse(queryable.ToArray().FirstOrDefault(defaultValue: null).AdditionalAccounts.ToString());
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                //Not premium
            }
            catch (NullReferenceException)
            {
                //Not premium
            }

            return max;
        }
        /// <summary>
        /// Checks any account with the RecheckSubscribedAccounts bool set to true. Unsubscribes if applicable.
        /// </summary>
        /// <returns></returns>
        public async Task UnsubscribeOverSubscriptions()
        {
            IQueryable<PremiumGuild> queryable = PremiumGuildsContainer.GetItemLinqQueryable<PremiumGuild>(true);
            queryable = queryable.Where<PremiumGuild>(item => item.RecheckSubscribedAccounts);
            foreach(PremiumGuild pguild in queryable.ToArray())
            {
                int maxAccounts = MaxSubscriptionsCountForGuild(ulong.Parse(pguild.GuildID));
                int currentAccounts = GuildSubscriptionCount(ulong.Parse(pguild.GuildID));
                if (currentAccounts > maxAccounts)
                {
                    Console.WriteLine("Guild over limit.");

                    int NumberOfAccountsToRemove = currentAccounts - maxAccounts;

                    IQueryable<FollowedIGUser> queryableIG = FollowedAccountsContainer.GetItemLinqQueryable<FollowedIGUser>(true);
                    queryableIG = queryableIG.Where<FollowedIGUser>(item => item.SubscribedChannels.Any<RespondChannel>(n => n.GuildID == pguild.GuildID));
                    foreach(FollowedIGUser igAccount in queryableIG.ToArray())
                    {
                        if (NumberOfAccountsToRemove <= 0)
                        {
                            break;
                        }
                        //Get all to be removed:
                        RespondChannel[] chans = igAccount.SubscribedChannels.FindAll(item => item.GuildID.Equals(pguild.GuildID)).ToArray();
                        foreach(RespondChannel chan in chans)
                        {
                            //Remove:
                            igAccount.SubscribedChannels.Remove(chan);

                            //Notify:
                            var discordChan = _client.GetChannel(ulong.Parse(chan.ChannelID)) as IMessageChannel;
                            await discordChan.SendMessageAsync("This channel has been automatically unsubscribed to " + (await InstagramProcessor.GetIGUsername(igAccount.InstagramID)) + " as it exceeded the guild's maximum subscription limit.");
                        }
                        //Update Database:
                        await this.FollowedAccountsContainer.UpsertItemAsync<FollowedIGUser>(igAccount, new PartitionKey(igAccount.InstagramID));

                        NumberOfAccountsToRemove--;
                    }
                }
                pguild.RecheckSubscribedAccounts = false;
                await this.PremiumGuildsContainer.UpsertItemAsync<PremiumGuild>(pguild, new PartitionKey(pguild.GuildID));
            }
        }
    }
}