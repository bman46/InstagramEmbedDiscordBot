# Define the base image.
FROM mcr.microsoft.com/dotnet/runtime-deps:6.0

# Folder in the file system of the container in which the following commands get executed.
WORKDIR /app

# Copy everything from the folder in which this Dockerfile resides to the workdir. Assign correct permissions.
COPY --chmod=0755 . .

# Ensure the folder /app/stateFile and the file /app/config.json do not exist
RUN rm -rf /app/stateFile && rm -f /app/config.json

# Declare the folder /app/stateFile as a volume, so that its contents persists.
VOLUME /app/stateFile

# Set the file that is executed when the container starts.
ENTRYPOINT ./Instagram\ Reels\ Bot

# Labels for metadata
LABEL maintainer="FreSchNDE 129771532+FreSchNDE@users.noreply.github.com"
LABEL description="Docker image for Instagram Reels Bot"
LABEL dockerfileEdition="1.0"