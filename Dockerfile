FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build

COPY ScribdDownloader /ScribdDownloader
WORKDIR /ScribdDownloader
RUN dotnet publish -o /app

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 as final
COPY --from=build /app /app
WORKDIR /app

ENTRYPOINT ["dotnet", "ScribdDownloader.dll"]