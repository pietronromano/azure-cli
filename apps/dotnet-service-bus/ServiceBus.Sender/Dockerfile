# SEE https://github.com/Azure-Samples/containerapps-albumapi-csharp/blob/main/src/Dockerfile

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Copy ServiceBus.Utils project files and restore as distinct layers
COPY ServiceBus.Utils/*.csproj ./ServiceBus.Utils/
COPY ServiceBus.Sender/*.csproj ./ServiceBus.Sender/
WORKDIR /app/ServiceBus.Sender
RUN dotnet restore 

# Copy everything else and build
WORKDIR /app
COPY ServiceBus.Utils/. ./ServiceBus.Utils/
COPY ServiceBus.Sender/. ./ServiceBus.Sender/
WORKDIR /app/ServiceBus.Sender
RUN dotnet publish -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .

# NOTE: Azure Container Apps uses the EXPOSE instruction to determine which ports to open on the container.
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ServiceBus.Sender.dll"]