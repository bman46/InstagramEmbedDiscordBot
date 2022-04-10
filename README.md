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

## Example: 
![Example of reels bot on discord](/docs/Content/ReadMe/Example.png)

### Config.json format:
Create a new file named `config.json`, copy and paste the contents below into it, fill it out, and then save it in the same directory as the Instagram Embed executable file.
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

  "ProxyURL": "(optional)",
  "ProxyUsername": "username (optional)",
  "ProxyPassword": "password (optional)",

  "AllowSubscriptions": true/false,
  "MongoDBUrl": "MongoDB Connection String (Required for subscriptions)",
  "DefaultSubscriptionsPerGuildMax": 1,
  "HoursToCheckForNewContent": 3
}
```
