#this docker file for debug

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app
WORKDIR /app/mnt

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TelegramBotDownloader.UI/TelegramBotDownloader.UI.csproj", "TelegramBotDownloader.UI/"]
COPY ["TelegramBotDownloader.Core/TelegramBotDownloader.Core.csproj", "TelegramBotDownloader.Core/"]
RUN dotnet restore "TelegramBotDownloader.UI/TelegramBotDownloader.UI.csproj"
COPY . .
WORKDIR "/src/TelegramBotDownloader.UI"
RUN dotnet build "TelegramBotDownloader.UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TelegramBotDownloader.UI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["sudo" "dotnet", "TelegramBotDownloader.UI.dll"]