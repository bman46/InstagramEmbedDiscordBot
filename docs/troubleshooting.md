# Troubleshooting Guide
This guide covers how to troubleshoot the bot and common issues that arise during the operation of the bot. This file does currently not contain troubleshooting steps relating to Docker.

## How to check that the bot is working
To check that the bot is working, you can take a look at the command line output of the executable. Send the bot a basic command (like `/link`) as a direct message on Discord, and observe what happens in the output of the executable.
If the retrieval of Instagram content fails for any reason, the executable will flag that account and try to use other Instagram accounts. You can check which accounts are flagged by sending the bot a Discord direct message with the text `accounts` and looking at the `Last failed` part of the answer.

If the bot does not reply, ensure that the `OwnerID` setting is set to your Discord ID in the `config.json` file.

If the error message mentions an ID, it might refer to an Instagram profile user ID. There are tools online to look up which ID belongs to which Instagram profile.

## How to get 2FA codes
If the Instagram accounts you added to your `config.json` file use two-factor authentication (2FA) and you want to log in manually to one of them, you'll need a one time password-code (OTP). Either add the secret key from the `config.json` file to an authentication app on your phone, or send the bot a Discord direct message with the text `accounts` and use the code it replies with. These codes are only valid for a few seconds.

If the bot does not reply, ensure that the `OwnerID` setting is set to your Discord ID in the `config.json` file.

## Failed Login Error
This error is usually caused by Instagram blocking the bot from logging in as it flags it as suspicious activity. In most cases, logging into the Instagram account on your phone/browser and completing any required steps to confirm that the account is owned by a human will allow the accounts to become usable again. After logging into the account successfully, DM the Discord bot with the phrase `clearstate`. It should reply with `State files removed`. If it does not reply, ensure that the `OwnerID` setting is set to your Discord ID in the `config.json` file.

Alternatively, this error can also be caused by using Two-Factor Authentication (2FA) on the Instagram account. This bot supports using One Time Password (OTP) 2FA codes by placing the secret in the `OTPSecret` field for the account in the `config.json` file.
The secret key is usually in the format `XXXX XXXX XXXX XXXX XXXX XXXX XXXX XXXX`.

## No Available Accounts Error
This error is caused by all of the accounts on the bot being locked out due to previous failures. In most cases, logging into the Instagram account on your phone/browser and completing any required steps to confirm that the account is owned by a human will allow the accounts to become usable again. After logging into the account successfully, DM the Discord bot with the phrase `clearstate`. It should reply with `State files removed`.

If it does not reply, ensure that the `OwnerID` setting is set to your Discord ID in the `config.json` file.

## Relogin Required
This error is caused by the Discord bot's login token becoming expired. In order to fix this, DM the Discord bot with the phrase `clearstate`. It should reply with `State files removed`.

If it does not reply, ensure that the `OwnerID` setting is set to your Discord ID in the `config.json` file.

## Long Links
Longs links instead of a video embed is usually because the video requested is larger than the maximum upload size allowed by Discord. Note that the long link that the bot returns is not a permanent link, as it will expire shortly after posting.

## No Slash Commands
First, kick the bot from your server and then reinvite it with the invite URL listed in the [install guide](https://github.com/bman46/InstagramEmbedDiscordBot/blob/master/docs/Install.md#step-6).
If that fails to add slash commands, check your Discord server permissions to ensure that slash commands are allowed. 
Next, send the bot a direct message with the contents `overwrite`. It should reply with `Slash commands resynced`.

If it does not reply, ensure that the `OwnerID` setting is set to your Discord ID in the `config.json` file.

## Other Common Errors
The message `Error retrieving the content. The account may be private. Please report this to the admin if the account is public or if this is unexpected. (unknown error)` indicates that you should take a look at the command line output of the executable. More details will be shown there.

Both `WebSocket.GatewayReconnectException` and `System.Exception: WebSocket connection was closed` are caused by Discords servers and can safely be ignored.

The message `You'll need to update Instagram to the latest version before you can use the app` often indicates that the Instagram account was flagged for suspicious activity. Try to use that account on your phone/browser and wait a day or two, before using it with the bot again. This error will cause the executable to flag the account as faulty and skip it. Use the `clearstate`command as described for the other errors when you want the bot to use that account again.

The error `RequestsLimit: Please wait a few minutes before you try again..` is similar to the previous error, however, it won't cause the executable to flag the account as faulty. Remove the affected account from the configuration file manually and restart the bot to stop it from using that account.