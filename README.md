# Instagram Embed Discord Bot
Delivers new posts from Instagram accounts to a Discord channel.
Embeds linked videos and images from users linked Instagram posts, videos, and reels into a Discord chat. 
Used in over 1,000 Discord servers for a total of over 250,000 users.

[Invite To Discord Server](https://link.mcshane.systems/reelsbotinvite)

[Support Discord Server](https://discord.gg/6K3tdsYd6J)

[Rate and Upvote on Top.gg](https://top.gg/bot/815695225678463017)

[Rate and Upvote on DiscordBotList.com](https://discord.ly/instagram-embed)

## Features:
- No prefixes needed
- Videos are downloadable
- Adjusted upload sizes for Nitro Boosted servers
- Supports Discord slash commands
- Supports subscribing to new posts from Instagram users

## Example: 
![Example of reels bot on discord](https://github.com/bman46/Instagram-Reels-Bot/raw/master/Example.png)

### Config.json format:
```
{
  "Token": "TokenHere",
  "Prefix": [ "https://www.instagram.com/", "https://instagram.com/", "http://www.instagram.com/", "http://instagram.com/" ],
  "OwnerID": "OwnerID",
  "ProxyURL": "",
  "IGUserName": "IGUsername",
  "IGPassword": "IGPassword",
  "2FASecret": "2FA OTP secret (optional)",

  "AllowSubscriptions": true/false,
  "EndpointUrl": "CosmosDB Endpoint (optional if AllowSubscriptions is false)",
  "PrimaryKey": "CosmosDB key (optional if AllowSubscriptions is false)",
  "DefaultSubscriptionsPerGuildMax": 0,
  "HoursToCheckForNewContent": 3
}
```
