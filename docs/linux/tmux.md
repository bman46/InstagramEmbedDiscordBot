# Using tmux
Tmux is a terminal emulator. Essentially, it will act as a seperate session that will run in the background and allow you to connect and disconnect from it as you wish. For a better explanation and more advanced usage, see [this guide](https://linuxize.com/post/getting-started-with-tmux/). We will be using it to run the bot in the background. This guide is for linux only.

> **_NOTE:_**  Tmux sessions will be closed when the computer is restarted or shut down. You will need to restart the bot when the computer starts back up.

## Install tmux
Ubuntu/Debian:
```
sudo apt-get update
sudo apt-get install tmux
```
Fedora/CentOS:
```
sudo dnf -y install tmux
```

## Setup
1. SSH to your machine
3. Start a tmux session with the following command
```
tmux new -s instagram_bot
```
4. Start the Instagram Bot
5. Exit the tmux session by pressing `ctrl-b` then `d`

## Reconnecting to the session
If you need to reconnect to the session to close the bot or restart it, follow these steps:

1. List the tmux sessions with `tmux ls`
2. Find the session ID for the session with the name `instagram_bot`
3. Connect to the session with `tmux attach-session -t ID` where `ID` is the session ID from the previous step.
4. Execute commands and then exit the tmux session by pressing `ctrl-b` then `d`