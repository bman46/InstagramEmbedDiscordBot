# Configuring The Subscribe Module
This module allows you to use the `/subscribe` command and get automatic updates from accounts.
> :warning: **Note**: This feature requires a [Microsoft Azure](https://azure.microsoft.com/) account. Usage may be [billed](https://azure.microsoft.com/pricing/details/cosmos-db/).
> 
> [Student discounts](https://azure.microsoft.com/free/students/) are avaliable through Azure and do not require a credit card.

## Step 1: Create an Azure CosmosDB Instance
1. Log into [Azure](https://portal.azure.com/)
2. Navigate to `Create a Resource`
3. Search for `Azure Cosmos DB`
4. Click create on the `Azure Cosmos DB` page
5. Select `Core (SQL)`
6. Select a valid subscription and create a new resource group
7. Enter a unique account name
8. Change the location to one close to you
9. Optional: if you plan on pursuing the free tier option, select the provisioned throughput option. Then ensure the apply free tier option is set to apply.
10. Suggested: Enable the `Limit total account throughput` toggle
11. Optionally, you may change other settings at this point if you are an advanced user
12. Click `Review + Create`
13. Ensure all of the information is correct and there is no errors, then select `Create`
14. Wait until the deployment is complete to proceed.
15. When the deployment is complete, press the `Go to Resource` button

## Step 2: Create the Database
1. Go to the `Data Explorer` tab
2. Select the `New Database` option in the drop down next to the `New Container` button
3. Set the Database ID as `InstagramEmbedDatabase` (case sensitive)
4. Check the `Provision throughput` checkbox
5. Set `Database throughput` to manual
6. Set `Database throughput` to 400 RU/s (suggested)
7. Hit Ok
> :warning: **Note**: If you are planning on working with the source code, you should repeat this process and create a `InstagramEmbedDatabaseDev` container.

## Step 3: Configure the Bot
1. Navigate to the Keys tab
2. Copy the URI
3. Open the bot's `config.json` file
4. Replace the value of the EndpointUrl with your URL

Example:
```
"EndpointUrl": "https://test.documents.azure.com:443/",
```
5. Copy the `PRIMARY KEY`
6. Set the `PrimaryKey` value in config.json to the value copied from Azure
7. Ensure the `AllowSubscriptions` value is set to true
8. Set the `HoursToCheckForNewContent` to how often (in hours) that you would like it to check the accounts for new posts (Higher numbers are better)
9. Start the bot and test the commands!
