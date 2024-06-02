# Installing The Bot
This setup will go over the basic installation and configuration of the bot.

## What's Required?

1. Must have a Windows/Linux/MacOS computer that is always running
    - Optionally, you may use a cloud provider (such as [AWS](https://aws.amazon.com/), [Azure](https://azure.microsoft.com/), or [Digital Ocean](https://www.digitalocean.com/))
    - Note that some cloud providers may be blocked or highly limited by Instagram. Using a residential network is preferred for this reason
    - A [Raspberry Pi](https://www.raspberrypi.com/products/) is a great option for hosting on your home network.
      - It has a low power consumption and a small form factor
2. An internet connection with access to Instagram and Discord
3. At least one throwaway Instagram account
4. A Discord bot token [Instructions here](https://www.writebots.com/discord-bot-token/)
    - The bot account must have the `message content intent` enabled

## Installation Steps:
### Step 1:
Download the latest version of the bot from our [GitHub Releases Page](https://github.com/bman46/InstagramEmbedDiscordBot/releases).

> **_NOTE:_**  Download the version that corresponds to your device's OS and architecture. If you are unsure, go with you OS (windows, mac, or linux) and the x64 build. Windows is usually win-x64.

### Step 2:

Unzip the downloaded file and place it in a convenient location.

### Step 3:
Create the `config.json` file.

You can manually create this file using notepad or any other text editor. Place this file inside the unzipped folder.
Contents of the file are located in the readme for this repo.

### Step 4:
Edit the `config.json` file.

Open the `config.json` file from the previous step with any text editor (such as notepad). Then, change the following values:

**Token:**

`"Token": "Token",` Replace `Token` with your Discord bot token. See [this article](https://www.writebots.com/discord-bot-token/) on instructions on how to obtain one. This is the backend login to the Discord application that users can invite to their Discord server.

**OwnerID:**

`"OwnerID": "ID",` Replace ID with your Discord ID. See [this article](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-) on instructions on how to find this. The account that is defined here will be able to send special maintenance commands as a direct message to the bot.

**TestGuildID:**

`"TestGuildID": "ID",` Ignore this value for now. You can delete the line from your file.

**DMErrors:**

`"DMErrors": true/false,` Set this to `true` or `false` depending on if you want the owner from `OwnerID` to receive certain errors as Discord direct messages.

**IGAccounts:**

```
  "IGAccounts": [
    {
      "username": "IG Username",
      "password": "IG Password",
      "OTPSecret": ""
    }
  ],
```
Replace `IG Username` with the username of the throwaway Instagram account. Then, replace `IG Password` with the password to the Instagram account. `OTPSecret` needs to contain the secret key that would be registered in two-factor authentication apps if the Instagram account has two-factor authentication enabled. Leave it blank otherwise. The secret key is usually 39 characters (including spaces) and not to be confused with a one time password-code (which can be generated from the key).

For multiple accounts, use the following syntax:
```
  "IGAccounts": [
    {
      "username": "IG Username 1",
      "password": "IG Password 1",
      "OTPSecret": ""
    },
    {
      "username": "IG Username 2",
      "password": "IG Password 2",
      "OTPSecret": ""
    },
    {
      "username": "IG Username 3",
      "password": "IG Password 3",
      "OTPSecret": ""
    }
  ],
```
The settings relating to usage times are optional. Leave at least one account usable at any given time.

**ProxyURL:**

`"ProxyURL": "",` Change this only if you want the application to use a network proxy. Leave it blank otherwise.

**AllowSubscriptions:**

`"AllowSubscriptions": true/false,` Set this to `true` if you want to use automatic sharing of new instagram content and have configured `MongoDBUrl`. See [subscribe module](subscribe.md) on how to do this. Set it to `false` otherwise.

### Step 5:
Assign bot intents.

Go to the [Discord developer portal](https://discord.com/developers) and navigate to your bot page. Then, under `Privileged Gateway Intents` toggle the `MESSAGE CONTENT INTENT` check box to on. This means that the bot application has access to Discord messages. It is required for the bot to work.

### Step 6:
Invite the Discord bot to your server.

`https://discord.com/oauth2/authorize?client_id=YOURBOTID&permissions=60480&scope=applications.commands%20bot`

Replace `YOURBOTID` with the application ID from the Discord Developer Portal. Paste the URL into a web browser and navigate to the site to complete the invite. You must log in as a Discord user who has the authorization to add a bot to your desired server. You can later share this URL to let other people invite the bot to their server.

### Step 7:
Configure bot permissions on your Discord server.

The bot will create its own role on the Discord server. Use that along with channel permissions to allow the bot the following:
- View Channel(s)
- Send Messages
- Embed Links
- Attach Files
- Manage Messages
- Use Application Commands

### Step 8:
Launch the program.

On Windows, find the `Instagram Reels Bot.exe` file and open it. A command line window should open and the bot should indicate that it is online in Discord.

On Linux and MacOS, find the file named `Instagram Reels Bot` with no extensions. Launch that file and a terminal should appear and the bot should indicate that it is online in Discord. On Linux, using tmux is recommended for beginners as it will run the program in the background. See the [tmux guide](linux/tmux.md) for more information. A creating a service is a better but more advanced approach.

> **_NOTE:_** The program and computer must be running in order for the bot to process requests. Exiting the Window or closing the SSH session (without using tmux or a service) will stop the bot from running.

> **_NOTE:_** On Linux, files might need to be made executable before you can run them. Run the command `sudo chmod a+x Instagram\ Reels\ Bot` in the same folder as the file, to make it executable for all users.

### Step 9:
Test the bot by typing `/link`. Discord should acknowledge the slash command and show you parameters to enter. Then, put a link to the Instagram post in the `URL` parameter and hit enter. The bot should reply with the contents of the post.

## Further Configuration:
- See the guide on the [subscribe module](subscribe.md) for steps on setting up automatic posts from Instagram accounts.
- For custom statuses, see the article on [custom statuses](CustomStatus.md).
- Check out the [Troubleshooting Guide](troubleshooting.md) if you have any issues while running the bot.