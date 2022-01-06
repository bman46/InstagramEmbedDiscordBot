using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly IConfiguration config;
        private readonly DiscordShardedClient client;
        private System.Timers.Timer UpdateTimer;
        //CosmosDB:
        private static string EndpointUri;
        private static string PrimaryKey;
        // The Cosmos db client instance
        private CosmosClient CosmosClient;
        // Add the Database:
        private Database Database;
        // Followed Accounts Container
        private Container FollowedAccountsContainer;

        /// <summary>
        /// Initialize sub
        /// </summary>
        /// <param name="services"></param>
        public Subscriptions(IServiceProvider services)
        {
            // Dependancy injection:
            config = services.GetRequiredService<IConfiguration>();
            client = services.GetRequiredService<DiscordShardedClient>();

            //Dont set database locations unless AllowSubscriptions is true:
            if (config["AllowSubscriptions"].ToLower() != "true")
            {
                Console.WriteLine("Subscriptions not allowed.");
                return;
            }

            //Set cosmos DB info:
            EndpointUri = config["EndpointUrl"];
            PrimaryKey = config["PrimaryKey"];
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
                ItemResponse<FollowedIGUser> response = await this.FollowedAccountsContainer.ReadItemAsync<FollowedIGUser>(instagramID.ToString(), new PartitionKey(instagramID));
                databaseValue = response.Resource;
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
                    if(chan.ChannelID == channelID)
                    {
                        throw new Exception("Already subscribed");
                    }
                }
                databaseValue.SubscribedChannels.Add(new RespondChannel(guildID, channelID));
                await this.FollowedAccountsContainer.UpsertItemAsync<FollowedIGUser>(databaseValue, new PartitionKey(databaseValue.InstagramID));
            }
        }
        /// <summary>
        /// Starts the subscription tasks.
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            if(string.IsNullOrEmpty(PrimaryKey)|| string.IsNullOrEmpty(EndpointUri))
            {
                Console.WriteLine("Databases not setup.");
                return;
            }
            //Connect to Database:
            this.CosmosClient = new CosmosClient(EndpointUri, PrimaryKey);

            //link and create the database if it is missing:
            this.Database = await this.CosmosClient.CreateDatabaseIfNotExistsAsync("InstagramEmbedDatabase");
            this.FollowedAccountsContainer = await this.Database.CreateContainerIfNotExistsAsync("FollowedAccounts", "/id");

            // Timer:
            UpdateTimer = new System.Timers.Timer(3600000.0 * double.Parse(config["HoursToCheckForNewContent"])); //one hour in milliseconds
            UpdateTimer.Elapsed += new ElapsedEventHandler(GetLatestsPosts);
            UpdateTimer.Start();

            // Check latest posts:
            GetLatestsPosts();
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
        public async Task GetLatestsPosts()
        {
            Console.WriteLine("Getting new posts!");
            using (FeedIterator<FollowedIGUser> dbfeed = FollowedAccountsContainer.GetItemQueryIterator<FollowedIGUser>()) {
                while (dbfeed.HasMoreResults)
                {
                    foreach (var igAccount in await dbfeed.ReadNextAsync())
                    {
                        Console.WriteLine("Checking " + igAccount.InstagramID);
                        //Set last check as now:
                        igAccount.LastCheckTime = DateTime.Now;
                        var newIGPosts = await InstagramProcessor.PostsSinceDate(long.Parse(igAccount.InstagramID), igAccount.LastPostDate.AddSeconds(1));
                        if (newIGPosts.Length!=0&&newIGPosts[0].success)
                        {
                            //Set the most recent posts date:
                            igAccount.LastPostDate = newIGPosts[0].postDate;
                        }
                        foreach(InstagramProcessorResponse response in newIGPosts)
                        {
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
                                        foreach (RespondChannel subbedGuild in igAccount.SubscribedChannels)
                                        {
                                            // get channel:
                                            IMessageChannel chan = null;
                                            try
                                            {
                                                chan = client.GetChannel(subbedGuild.ChannelID) as IMessageChannel;
                                            }
                                            catch (Exception e)
                                            {
                                                Console.WriteLine("Cannot find channel. Removing from DB.");
                                                igAccount.SubscribedChannels.Remove(subbedGuild);
                                            }
                                            if (chan != null)
                                            {
                                                //send message
                                                await chan.SendFileAsync(attachment, "New post!");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //Response without stream:
                                    foreach (RespondChannel subbedGuild in igAccount.SubscribedChannels)
                                    {
                                        // get channel:
                                        IMessageChannel chan = null;
                                        try
                                        {
                                            chan = client.GetChannel(subbedGuild.ChannelID) as IMessageChannel;
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine("Cannot find channel. Removing from DB.");
                                            igAccount.SubscribedChannels.Remove(subbedGuild);
                                        }
                                        if (chan != null)
                                        {
                                            //send message
                                            await chan.SendMessageAsync("New post! " + response.contentURL);
                                        }
                                    }
                                }

                            }
                            else
                            {
                                var embed = new EmbedBuilder();
                                embed.Title = "New Post!";
                                embed.Url = response.postURL.ToString();
                                embed.Description = (response.caption != null) ? (DiscordTools.Truncate(response.caption)) : ("");
                                embed.ImageUrl = "attachment://IGMedia.jpg";
                                embed.WithColor(new Color(131, 58, 180));
                                if (response.stream != null)
                                {
                                    using (Stream stream = new MemoryStream(response.stream))
                                    {
                                        FileAttachment attachment = new FileAttachment(stream, "IGMedia.jpg", "An Instagram Image.");
                                        foreach (RespondChannel subbedGuild in igAccount.SubscribedChannels)
                                        {
                                            // get channel:
                                            IMessageChannel chan = null;
                                            try
                                            {
                                                chan = client.GetChannel(subbedGuild.ChannelID) as IMessageChannel;
                                            }
                                            catch (Exception e)
                                            {
                                                Console.WriteLine("Cannot find channel. Removing from DB.");
                                                igAccount.SubscribedChannels.Remove(subbedGuild);
                                            }
                                            if (chan != null)
                                            {
                                                //send message
                                                await chan.SendFileAsync(attachment, embed: embed.Build());
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    embed.ImageUrl = response.contentURL.ToString();
                                    foreach (RespondChannel subbedGuild in igAccount.SubscribedChannels)
                                    {
                                        // get channel:
                                        IMessageChannel chan = null;
                                        try
                                        {
                                            chan = client.GetChannel(subbedGuild.ChannelID) as IMessageChannel;
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine("Cannot find channel. Removing from DB.");
                                            igAccount.SubscribedChannels.Remove(subbedGuild);
                                        }
                                        if (chan != null)
                                        {
                                            //send message
                                            await chan.SendMessageAsync(embed: embed.Build());
                                        }
                                    }
                                }
                            }
                        }
                        // Delete if empty:
                        if (igAccount.SubscribedChannels.Count == 0)
                        {
                            await this.FollowedAccountsContainer.DeleteItemAsync<FollowedIGUser>(igAccount.InstagramID, new PartitionKey(igAccount.InstagramID));
                        }
                        else
                        {
                            //Update database:
                            await this.FollowedAccountsContainer.UpsertItemAsync<FollowedIGUser>(igAccount, new PartitionKey(igAccount.InstagramID));
                        }
                        //Wait to prevent spamming IG api:
                        Console.WriteLine("Complete. Onto the next post after sleep.");
                        Thread.Sleep(2000);
                    }
                }
                Console.WriteLine("Done.");
            }
        }
    }
}