FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ../Follower.sln ./
COPY ../Followers/Follower.csproj ./Followers/
RUN dotnet restore ./Followers/Follower.csproj
COPY .. .
WORKDIR "/src/Followers"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
ENTRYPOINT [ "dotnet", "Follower.dll" ]