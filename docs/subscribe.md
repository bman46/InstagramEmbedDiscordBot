# Configuring The Subscribe Module
This module allows you to use the `/subscribe` command and get automatic updates from accounts.
> :warning: **Note**: This tutorial requires a [MongoDB Atlas](https://www.mongodb.com/atlas/database) account. Usage may be [billed](https://www.mongodb.com/pricing) but there is a free tier as of writing this.
> 
> Alternatively, you may self host MongoDB for free. There are plenty of tutorials online for this.

Please sign up for a [MongoDB Atlas](https://www.mongodb.com/atlas/database) account before proceeding.
## Step 1: Security Quickstart
1. When asked `How would you like to authenticate your connection?` select `Username and Password`.
2. Type in credentials and click `Create User`.
   - Dont forget the password you created. It will be needed later.
   - Use a unique password. Not the same one that you signed up with or a password that you frequently use.
3. When asked `Where would you like to connect from?` you can either type in your IP (more secure) or use `0.0.0.0` to allow all IPs (less secure)
   - Using your IP is suggested, but keep in mind that **IPs may change from time to time**.
4. Click `Finish and Close`

## Step 2: Connecting the Bot to the Database
1. Navigate to `Database` in the left pane
2. Next to `Cluster0` (or whatever your cluster name is) click on the `Connect` button
3. Select `Connect Your Application`
4. Under the driver dropdown, Select `C# / .NET`
5. Under version, select `2.13` or `2.13 or later`
6. Copy the connection string from the box and replace `<password>` with the password you created earlier.
   - If you forgot this password, you can re-create it again under the `Database Access` tab.
7. Paste the connection string into the `MongoDBUrl` parameter in the `config.json` file.
8. Start the bot and test the commands! 
