# SEE https://github.com/Azure-Samples/containerapps-albumapi-csharp/blob/main/src/Dockerfile

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Copy SB.Utils project files and restore as distinct layers
COPY SB.Utils/*.csproj ./SB.Utils/
COPY SB.ExampleSender/*.csproj ./SB.ExampleSender/
WORKDIR /app/SB.ExampleSender
RUN dotnet restore 

# Copy everything else and build
WORKDIR /app
COPY SB.Utils/. ./SB.Utils/
COPY SB.ExampleSender/. ./SB.ExampleSender/
WORKDIR /app/SB.ExampleSender
RUN dotnet publish -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .

# NOTE: Azure Container Apps uses the EXPOSE instruction to determine which ports to open on the container.
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SB.ExampleSender.dll"]