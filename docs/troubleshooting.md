# Troubleshooting Guide
This guide covers how to troubleshoot the bot and common issues that arise during the operation of the bot.

## Failed Login Error
This error is usually caused by Instagram blocking the bot from logging in as it flags it as suspicious activity. In most cases, logging into the Instagram account on your phone/browser and completing and required steps to confirm that the account is owned by a human will allow the accounts to become usable again. After logging into the account successfully, DM the Discord bot with the phrase `clearstate`. It should reply with `State files removed`. If it does not reply, ensure that the `OwnerID` setting is set to your Discord ID in the `config.json` file.

Alternatively, this error can also be caused by using Two Factor Authentication (2FA) on the Instagram account. This bot supports using One Time Password (OTP) 2FA codes by placing the secret in the `OTPSecret` field for the account in the `config.json` file.

## No Available Accounts Error
This error is caused by all of the accounts on the bot being locked out due to previous failures. In most cases, logging into the Instagram account on your phone/browser and completing and required steps to confirm that the account is owned by a human will allow the accounts to become usable again. After logging into the account successfully, DM the Discord bot with the phrase `clearstate`. It should reply with `State files removed`. If it does not reply, ensure that the `OwnerID` setting is set to your Discord ID in the `config.json` file.

## Relogin Required
This error is caused by the Discord bot's login token becoming expired. In order to fix this, DM the Discord bot with the phrase `clearstate`. It should reply with `State files removed`. If it does not reply, ensure that the `OwnerID` setting is set to your Discord ID in the `config.json` file.

## Long Links
Longs links instead of a video embed is usually because the video requested is larger than the maximum upload size allowed by Discord. Note that the long link that the bot returns is not a permanent link as it will expire shortly after posting.

## No Slash Commands
First, kick the bot from your server and then reinvite it with the invite URL listed in the [install guide](https://github.com/bman46/InstagramEmbedDiscordBot/blob/master/docs/Install.md#step-6).
If that fails to add slash commands, check your Discord server permissions to ensure that slash commands are allowed. 
Next, send the bot a direct message with the contents `overwrite`. It should reply with `Slash commands resynced`.
