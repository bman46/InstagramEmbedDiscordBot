# Configuring The Subscribe Module
This module allows you to use the `/subscribe` command and get automatic updates from accounts.
This only works in Discord server channels. Only administrators and users with the role "InstagramBotSubscribe" (case-sensitive) can use commands related to the subscription of Instagram accounts. The subscriptions are independent of the accounts that the throwaway Instagram accounts follow.

The bot executable needs access to a MongoDB database for this module. You can host the database yourself, for example with Docker Compose as described in the [Docker guide](./docker/docker.md), or use an online provider. This tutorial uses the online provider [MongoDB Atlas](https://www.mongodb.com/atlas/database), please sign up for an account before proceeding.
If you choose to self-host the database, you still might want to take a look at the configuration file settings explained in step three.

> :warning: **Note**: [MongoDB Atlas](https://www.mongodb.com/atlas/database) usage may be [billed](https://www.mongodb.com/pricing), but there is a free tier as of writing this.
> 
> Alternatively, you may self host MongoDB for free. There are plenty of tutorials online for this.

## Step 1: Security Quickstart
1. When asked `How would you like to authenticate your connection?` select `Username and Password`.
2. Type in credentials and click `Create User`.
   - Dont forget the password you created. It will be needed later.
   - Use a unique password. Not the same one that you signed up with or a password that you frequently use.
3. When asked `Where would you like to connect from?` you can either type in your IP (more secure) or use `0.0.0.0` to allow all IPs (less secure)
   - Using your IP is suggested, but keep in mind that **IPs may change from time to time**.
4. Click `Finish and Close`

## Step 2: Create a connection string 
1. Navigate to `Database` in the left pane
2. Next to `Cluster0` (or whatever your cluster name is) click on the `Connect` button
3. Select `Connect Your Application`
4. Under the driver dropdown, Select `C# / .NET`
5. Under version, select `2.13` or `2.13 or later`
6. Copy the connection string from the box and replace `<password>` with the password you created earlier. Make sure that your remove the `<` and `>` as well.
   - If you forgot this password, you can re-create it again under the `Database Access` tab.

## Step 3: Edit the bot configuration file 
1. Paste the connection string into the `MongoDBUrl` parameter in the `config.json` file.
2. Set `AllowSubscriptions` to `true` in the `config.json` file.
You can also edit the following Values:
    - `DefaultSubscriptionsPerGuildMax` Limits the number of subscriptions each discord server can have at the same time.
	- `HoursToCheckForNewContent` Sets the interval at which the bot will check subscribed accounts for new content.
	- `SubscribeCheckDelayTime` Sets the wait time in seconds that needs to be between each check.
3. Start the bot and test the commands! 