FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder
MAINTAINER notarock <roch.damour@gmail.com>

LABEL org.opencontainers.image.source "https://github.com/bman46/InstagramEmbedDiscordBot"
LABEL org.opencontainers.image.description "Usage: `docker run -v $(pwd)/config.json:/app/config.json ghcr.io/notarock/instagramembeddiscordbot`"

WORKDIR /usr/src

ARG src="Instagram Reels Bot"
ENV APP_ENV="Docker"

COPY ${src}/*.csproj ./
RUN dotnet restore

COPY . .

RUN dotnet publish --framework net6.0 --runtime "linux-x64" --self-contained true -c Release -o app

FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app

COPY --from=builder /usr/src/app .

CMD ["dotnet", "Instagram Reels Bot.dll", "-error"]
