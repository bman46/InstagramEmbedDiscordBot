# Configuring Custom Statuses
These settings allow you to set a alternative Discord status for the bot aside from the default `Watching for links` status.
## Step 1: Setting the Activity
1. Choose one of the following Discord activities. The default is `Watching`.

| **Status**       | **Description**                              |
|------------------|----------------------------------------------|
| Competing        | Sets the activity to 'Competing'             |
| ~~CustomStatus~~ | Bots are not allowed to have custom statuses |
| Listening        | Sets the activity to 'Listening'             |
| Playing          | Sets the activity to 'Playing'               |
| Streaming        | Sets the activity to 'Streaming'             |
| Watching         | Sets the activity to 'Watching'              |

2. Add the following line to the `config.json` file below `DMErrors`: `"statusActivity": "Watching",`
3. Replace the word `Watching` with one of the statuses from the table in step 1 (Case Sensitive).
4. Save the config file.
## Step 2: Setting the Status Name
1. Open `config.json` file
2. Add the following line below `DMErrors`: `"statusDesc": "Your text here",`
3. Replace `Your text here` with the message that you would like to display.
