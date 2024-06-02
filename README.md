# Instagram Embed Discord Bot
[![CodeQL](https://github.com/bman46/InstagramEmbedDiscordBot/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/bman46/InstagramEmbedDiscordBot/actions/workflows/codeql-analysis.yml)
[![Publish](https://github.com/bman46/InstagramEmbedDiscordBot/actions/workflows/AutoRelease.yml/badge.svg)](https://github.com/bman46/InstagramEmbedDiscordBot/actions/workflows/AutoRelease.yml)

Delivers new posts from Instagram accounts to a Discord channel.
Embeds linked videos and images from users linked Instagram posts, videos, and reels into a Discord chat. Public bot had over 500k users across over 2k servers before being shut down by the owner due to resource restrictions.

[Support Discord Server](https://discord.gg/6K3tdsYd6J)

## Features:
- No prefixes needed
- Videos are downloadable
- Adjusted upload sizes for Nitro Boosted servers
- Supports Discord slash commands
- Supports subscribing to new posts from Instagram users
- Multiple IG accounts for failover

See the list of [commands](docs/commands.md) to get an impression of how users can interact with the bot.
## Example: 
![Example of reels bot on discord](/docs/Content/ReadMe/Example.png)

## Setup
See the [installation guide](docs/Install.md) for steps to setup the bot. For troubleshooting help, see the [troubleshooting guide](docs/troubleshooting.md).

### Config.json format:
Create a new file named `config.json`, copy and paste the contents below into it, fill it out, and then save it in the same directory as the Instagram Embed executable file. Replace any fields that are optional and not filled in with `""`. Example: `"OTPSecret": "",`. If you get an error with your JSON formatting, you can check your JSON syntax on [jsonlint.com](https://jsonlint.com/).
```
{
  "Token": "Token",
  "Prefix": [ "https://www.instagram.com/", "https://instagram.com/", "http://www.instagram.com/", "http://instagram.com/" ],
  "OwnerID": "ID",
  "TestGuildID": "ID",
  "DMErrors": true/false,

  "IGAccounts": [
    {
      "username": "IG Username",
      "password": "IG Password",
      "OTPSecret": "IG OTP Secret (optional)",
      "UsageTimes": [
        {
          "StartHour": optional (int; 0-23),
          "EndHour": optional (int; 0-23)
        }
      ]
    }
  ],

  "ProxyURL": "",
  "ProxyUsername": "username (optional)",
  "ProxyPassword": "password (optional)",

  "DisableTitle": false,
  "EnableDeleteButton": true,

  "AllowSubscriptions": true/false,
  "MongoDBUrl": "MongoDB Connection String (Required for subscriptions)",
  "DefaultSubscriptionsPerGuildMax": 1,
  "HoursToCheckForNewContent": 3,
  "SubscribeCheckDelayTime": 10
}
```
## How does it work?
The executable file connects to a Discord application (the “Discord bot”) with a bot token, processes its commands and controls its actions. As long as the application is running, the bot can be prompted for Instagram content with commands. The executable then uses throwaway Instagram accounts to access Instagram content.

A Discord application can be created via the Discord Developer Portal and then invited to multiple servers afterwards. See [installation guide](docs/Install.md).

To automatically share new posts from Instagram accounts, the subscribe module must be activated. The executable then requires access to a MongoDB database. See [subscribe module](docs/subscribe.md).