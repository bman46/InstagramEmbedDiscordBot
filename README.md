# Instagram Embed Discord Bot
Delivers new posts from Instagram accounts to a Discord channel.
Embeds linked videos and images from users linked Instagram posts, videos, and reels into a Discord chat.

[Support Discord Server](https://discord.gg/6K3tdsYd6J)

## Features:
- No prefixes needed
- Videos are downloadable
- Adjusted upload sizes for Nitro Boosted servers
- Supports Discord slash commands
- Supports subscribing to new posts from Instagram users
- Multiple IG accounts for failover

## Example: 
![Example of reels bot on discord](https://github.com/bman46/Instagram-Reels-Bot/raw/master/Example.png)

### Config.json format:
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
  "EndpointUrl": "CosmosDB URL",
  "PrimaryKey": "CosmosDB Key",
  "DefaultSubscriptionsPerGuildMax": 1,
  "HoursToCheckForNewContent": 3
}
```
