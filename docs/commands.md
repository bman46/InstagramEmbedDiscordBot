# Supported Commands
This is an overview of the commands this bot supports. Check out the [Troubleshooting Guide](troubleshooting.md) if you have any issues while running the bot.

## Basic Commands
These commands can mostly be executed by anyone in any channel where the bot has the required permissions. They also work when send as direct messages.

### `/help` Print Help Text
This command will print a short help message.

### `/link [url] [index] [has-spoilers]` Embed Instagram Content
Processes an Instagram link. Use this command to along with an Instagram post URL to embed the image or video. `[index]` and `[has-spoilers]` are optional. Use `[index]` to embed specific images from posts with multiple images.

### `/profile [username]` Embed Instagram Profiles
Use this command to display information about an Instagram profile.

### `/github` Link This Repository
Use this command to link to this Repository.

## Subscribe Module Commands
These commands relate to the automatic posting of new Instagram content. The [subscribe module](subscribe.md) needs to be enabled for them to work.
They only work in Discord server channels and can be executed only by administrators and users with the role "InstagramBotSubscribe" (case-sensitive).

### `/subscribe [username]` Set Up Automatic Updates
Get updates when a user posts a new post on Instagram. The bot will check for new content at set time intervals, which can be defined in the configuration file `config.json`. The bot will post new content in the channel this command was executed in.

### `/subscribed` List Subscriptions
Lists the Instagram accounts that the Discord server is subscribed to.

### `/unsubscribe` Unsubscribe From Accounts
This command will open a dropdown where you can choose which Instagram accounts you no longer wish to receive updates from.

### `/unsubscribeall` Unsubscribe From All Accounts
This will remove updates for all Instagram accounts in the Discord server.

## Special Owner Commands
The Discord account that is set with the `OwnerID` in the configuration file `config.json` can DM the bot special phrases (without a slash).

If the bot does not reply to any of these texts, ensure that the `OwnerID` setting is set to your Discord ID in the `config.json` file.

### `accounts` Get Instagram Account Information
The bot will respond with information about the throwaway Instagram accounts that were set in the `config.json` file. This includes if each account is flagged as broken and will be skipped due to previous errors, and a 2FA OTP code for each account.

### `clear state` Reset Instagram Account State
This command will reset the state of all throwaway Instagram accounts that were set in the `config.json` file. The bot will use all the defined accounts again afterwards. The bot will also generate a new virtual Android device for each Instagram account and use it to log into Instagram the next time that account gets used. The bot should reply with `State files removed.`.

This command will not wipe subscriptions made with the subscribe module commands.

### `sync` Update Subscriptions
If the executable has not been running for a longer period of time, the DM command `sync` can be used to update all subscriptions that were made in Discord channels with the subscribe command.
After a short wait, the bot will respond with `Working on it.`, however, errors might occur during this process, so keep an eye on the console output of the executable.

### `overwrite` Reload Slash Commands
This will cause all Discord slash commands to get recreated. The bot should reply with `Slash commands resynced.`.

### `toggle error` Overwrite DM Errors
Use this command to change if the owner (you) that was set in the configuration file will receive certain errors as Discord direct messages.

### `debug`, `guilds`, `users` Debugging Information
Each of these commands will result in the bot answering with different information.

### `generatedevice`, `accountdevice` Android Device Information
The command `accountdevice` will cause the bot to reply with information about the virtual Android devices which are used to log into Instagram with the defined throwaway accounts. These are the devices Instagram displays when you look up which devices are currently logged into the given account on the Instagram website.

The command `generatedevice` will test the generation of virtual devices, however the displayed device will not be used by the bot in any way.