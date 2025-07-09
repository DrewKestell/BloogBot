FROM mcr.microsoft.com/dotnet/aspnet:8.0-windowsservercore-ltsc2022 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-windowsservercore-ltsc2022 AS build
WORKDIR /src

# Install VC++ Redistributable manually
ADD https://aka.ms/vs/17/release/vc_redist.x64.exe C:/vc_redist.x64.exe
RUN C:\vc_redist.x64.exe /install /quiet /norestart || exit 1

COPY Services/PathfindingService/*.csproj ./Services/PathfindingService/
COPY Exports/BotCommLayer/BotCommLayer.csproj ./Exports/BotCommLayer/
COPY Exports/GameData.Core/GameData.Core.csproj ./Exports/GameData.Core/
COPY Exports/WinImports/WinProcessImports.csproj ./Exports/WinImports/
RUN dotnet restore ./Services/PathfindingService/PathfindingService.csproj

COPY . .
WORKDIR /src/Services/PathfindingService
RUN dotnet publish PathfindingService.csproj -c Release -o /app/publish

COPY Services/StateManager/appsettings.json /app/publish/appsettings.json

# Copy the data file and its contents
COPY Bot/Debug/net8.0/Data /C:/app/publish/Data/

# Copy Navigation.dll (native dependency)
COPY Bot/Debug/net8.0/Navigation.dll /C:/app/publish/Navigation.dll

# Copy the maps file and its contents
COPY Bot/Debug/net8.0/Maps /C:/app/publish/Maps/

#copy the mmaps file and its contents
COPY Bot/Debug/net8.0/mmaps /C:/app/publish/mmaps/

#copy the vmaps file and its contents
COPY Bot/Debug/net8.0/vmaps /C:/app/publish/vmaps/

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PathfindingService.dll"]
