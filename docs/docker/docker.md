# Running The Bot With Docker
This tutorial will cover how to use [Docker](https://www.docker.com/) to run the bot application. Docker is a program that allows you to run applications isolated from one another and from the host system in so-called containers. Docker is designed to make managing different applications easy and secure. See the [Docker overview](https://docs.docker.com/get-started/overview/) for more information about Docker.

Currently, there is no premade Docker image, however, the Linux 64 bit release contains the Dockerfile that's needed to easily build the image yourself.

> **Note**: You can create the image from the Linux files and run it in Docker Windows without problems.

There are two main ways you can use Docker to run the bot. Either with the `docker run`command or with a Docker compose file `compose.yml`. Research what is best for you and take a look at the sections below when you have chosen. The compose example contains a self-host database.

In both cases, have Docker installed before further following this tutorial. You can select between Docker desktop and command line Docker. Docker desktop offers you a graphical user interface, the commands presented here will still work with it. On Windows, make sure to [add the Docker executable to the Windows path](https://stackoverflow.com/questions/49478343/windows-doesnt-recognize-docker-command) if the console fails to recognize Docker commands.

If you are on Linux or macOS, you might need to run all Docker commands as the administrator. Do that by typing the prefix `sudo` in front of every command that starts with `docker`.

## Without Compose

### Creating the Docker image
Start by downloading the latest release of the linux-x64 version of the bot from our [GitHub Releases Page](https://github.com/bman46/InstagramEmbedDiscordBot/releases). Extract the folder using a tool of your choice (Windows File Explorer can open .tar.gz archives). Save the uncompressed folder to a path without spaces.

Now open a console and switch to the directory containing the Dockerfile and the main executable by using the `cd` command followed by the path to that folder.

Next, tell Docker to create the Image with the command `docker image build --tag igreelsbot --pull .`. This will create a Docker image named "igreelsbot" from which you can create a container. If you want to, you can now delete the folder. Check that your image was created by executing `docker image ls`.

### Running the container
Now create and edit the configuration file `config.json` as described in the [installation guide](../Install.md#step-3). Save it to another folder without spaces in its path.

You are now ready to create and start the container. Use the following command to create and start the container with the name "botContainer". Adjust the path `C:\path\to\config.json` to point to your configuration file.

`docker container run --name botContainer --volume botVolume:/app/stateFile --volume C:\path\to\config.json:/app/config.json:ro --detach igreelsbot`

If everything worked, you should see a unique container ID and the container should have the status `Up` in the output of the command `docker container ls --all`.

> **Note**: If you get the message `Error response from daemon: Conflict. The container name "/botContainer" is already in use by container`, that means that another container with the same name already exits. Take a look at the output of `docker container ls --all`. Either remove the contianer with `docker container rm botContainer` or start it back up when it's stoppet with `docker container start botContainer`. As long as you use the same volume name in the `run` command, no data will be lost if you choose to remove the container.


> **Note**: If the container is shown as "Exited", that means that the executable encounterd an error. Check the logs of the container as described below.

### Further operations
To see the console output of your container named "botContainer", execute this command: `docker container logs --follow botContainer`. Press `Ctrl` + `c` to return to your normal command line.

Docker containers and volumes persist when the Docker host (the operating system you installed Docker on) restarts. Make sure to start your container back up as well.

You can see the current status ("Up" or "Exited") by executing `docker container ls --all`. Stop and start your container with `docker container stop botContainer` and `docker container start botContainer`.

You can remove your container with `docker container rm botContainer`. The state files of the throwaway Instagram accounts get stored in the Docker volume `botVolume` and won't get lost. Use `docker volume ls` to see a list of all Docker volumes. The next time you use the `run` command, be sure to use the same volume name.
If you've failed to mount a volume, these files will still get stored in so-called anonymous volumes, which have long IDs instead of names. You can delete any volume with `docker volume rm` followed by the name/ ID of the volume.

To update to a new release of the bot, stop *and remove* the container as described, then redo the steps to create a new Docker image and a new container. You can keep the same image name, it will get overwritten with new content. Optionally, remove old images with `docker image prune --all` after recreating the container.

## With Compose
Compose gives you the ability to manage multiple containers with just one command. This example includes a self-hosted MongoDB database.

### Creating a project directory
Create a Folder where the compose project should live. The path of this folder can't contain any spaces.

Now create a text file named `compose.yml` and paste in the contents of the [sample compose file](./compose.yml). Make sure to adjust the database password and replace `YOURPASSWORDHERE`. Do not change this password after the first run.

Next, create the configuration file `config.json` in the same folder as the compose file. Edit it as is described in the [installation guide](../Install.md#step-3). Enable the subscribe module, and set the `MongoDBUrl` to `"mongodb://root:YOURPASSWORDHERE@botDatabaseContainer:27017/?authSource=admin"`. Replace `YOURPASSWORDHERE` with the password you set in the compose file.

### Inserting release files
Download the latest release of the linux-x64 version of the bot from our [GitHub Releases Page](https://github.com/bman46/InstagramEmbedDiscordBot/releases). Extract the folder using a tool of your choice (Windows File Explorer can open .tar.gz archives) and save the uncompressed folder to the same directory as the compose file. Rename the folder you just extracted to `IGReelsBot`. In contrast to the method described above without compose, this folder should not get deleted.

### Using the compose file
When everything is set up as described, you are ready to create and start the containers defined in the compose file. Open up a console and switch to the directory containing the compose file by using the `cd` command followed by the path to that folder.

Next, run the command `docker compose up --detach`. Startup might take a moment, the container "botContainer" only gets started after the database has passed the health check defined in the compose file. This command will consider already existing containers with the defined names and continue to use or recreate them.

If everything worked, you should see checkmarks next to all the container names and both containers should have the status `Up` in the output of the command `docker container ls --all`.

> **Note**: If the container "botContainer" is shown as "Exited", that means that the executable encounterd an error. Check the logs of the container. Simulary, check the logs of the container "botDatabaseContainer" when it gets reported as "unhealthy" or otherwise fails.

The compose up command basically issues the `run` command for each of the containers defined in the compose file. These Containers are regular containers just like the one created in [Without Compose](./docker.md#without-compose) above. That means they appear in the output of `docker container ls --all` and could be started, stopped and removed individually if so desired.
The up command also creates and mounts Docker volumes and Docker networks that are defined in a compose file. A Docker network is what's used by the containers to communicate with each other. Docker networks can be listed with `docker network ls` and can be created and removed independently, just like volumes and containers.

Take a look at section [Further Operations](./docker.md#further-operations) to learn, for example, how to view the command line output of the containers.

Use the command `docker compose down` to stop the bot. Make sure to execute this command in the folder that contains the compose file. Unlike the stop command from the section about running the bot without compose from above, this will not only stop, but also remove containers and networks defined in the Dockerfile. Volumes won't get removed, so no data is lost. The compose down command will even work if one of the containers was stopped or removed manually before.

The volume `botVolume` contains the state files for the throwaway Instagram accounts. The volumes `botDatabaseVolume` and `botConfigDatabaseVolume` contain the database of the database container. The database contains information about which discord server channel subscribed to witch Instagram profile. Make sure to reuse the `/subscribe` command if the database should get deleted.

To update to a new release of the bot, stop *and remove* both containers with `docker compose down`. Make sure to execute this and the following commands in the same folder as the compose file. Download the new Linux 64 bit release and extract it into the folder containing the compose file. Delete the old folder `IGReelsBot` and rename the new folder to `IGReelsBot`. Now run the command `docker compose pull` and `docker compose build --pull`. Optionally, remove old images with `docker image prune --all` after starting the containers back up. You might also want to take a look at the [sample compose file](./compose.yml) to check if any changes occurred.
