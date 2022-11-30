# Installing The Bot
This setup will go over the basic installation and configuration of the bot.

## Whats Required?

1. Must have a Windows/Linux/MacOS computer that is always running
    - Optionally, you may use a cloud provider (such as [AWS](https://aws.amazon.com/), [Azure](https://azure.microsoft.com/), or [Digital Ocean](https://www.digitalocean.com/))
    - Note that some cloud providers may be blocked or highly limited by Instagram. Using a residential network is preferred for this reason
    - A [Raspberry Pi](https://www.raspberrypi.com/products/) is a great option for hosting on your home network.
      - It has a low power consumption and has a small form factor
2. An internet connection with access to Instagram and Discord
3. At least one throw away Instagram account
4. A Discord bot token [Instructions here](https://www.writebots.com/discord-bot-token/)

## Installation Steps:
### Step 1:
Download the latest version of the bot from our [GitHub Releases Page](https://github.com/bman46/InstagramEmbedDiscordBot/releases).

> **_NOTE:_**  Download the version that corresponds to your device's OS and architecture. If you are unsure, go with you OS (windows, mac, or linux) and the x64 build. Windows is usually win-x64.

### Step 2:

Unzip the downloaded file and place it in a convenient location.

### Step 3:
Download or create the `config.json` file. 

You can download the `config.json` from our [Discord page](https://cdn.discordapp.com/attachments/921848709829001236/945556370487398400/config.json). Place this file inside the unzipped folder. Alternatively, you can manually create this file using notepad or any other text editor. Contents of the file are located in the readme for this repo.

### Step 4:
Edit the `config.json` file.

Open the `config.json` file from the previous step with any text editor (such as notepad). Then, change the following values:

**Token:**

`"Token": "Your token here",` Replace `Your token here` with your Discord bot token. See [this article](https://www.writebots.com/discord-bot-token/) on instructions on how to obtain one.

**OwnerID:**

`"OwnerID": "ID",` Replace ID with your Discord ID. See [this article](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-#:~:text=On%20Android%20press%20and%20hold,name%20and%20select%20Copy%20ID.) on instructions on how to find this.

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
Replace `IG Username` with the username of the throw away Instagram account. Then, replace `IG Password` with the password to the Instagram account.

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
### Step 5:
Enable 

Go to the [Discord developer portal](https://discord.com/developers) and navigate to your bot page. Then, under `Privileged Gateway Intents` toggle the `MESSAGE CONTENT INTENT` check box to on.

### Step 6:
Invite the Discord bot to your server.

`https://discord.com/oauth2/authorize?client_id=YOURBOTID&permissions=60480&scope=applications.commands%20bot`

Replace `YOURBOTID` with the application ID from the Discord Developer Portal. Paste the URL into a web browser and navigate to the site to complete the invite.

### Step 7:
Launch the program.

On Windows, find the `Instagram Reels Bot.exe` file and open it. A command line window should open and the bot should indicate that it is online in Discord.

On Linux and MacOS, find the file named `Instagram Reels Bot` with no extensions. Launch that file and a terminal should appear and the bot should indicate that it is online in Discord. On Linux, using tmux is recommended for beginners as it will run the program in the background. See the [tmux guide](linux/tmux.md) for more information. A creating a service is a better but more advanced approach.

> **_NOTE:_**  The program and computer must be running in order for the bot to process requests. Exiting the Window or closing the SSH session (without using tmux or a service) will stop the bot from running.

### Step 8:
Test the bot by typing `/link`. Discord should acknowledge the slash command and show you parameters to enter. Then, put a link to the Instagram post in the `URL` parameter and hit enter. The bot should reply with the contents of the post.

## Further Configuration:
- See the guide on the [subscribe module](subscribe.md) for steps on setting up automatic posts from Instagram accounts.
- For custom statuses, see the article on [custom statuses](CustomStatus.md).
