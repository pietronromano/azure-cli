# SEE https://github.com/Azure-Samples/containerapps-albumapi-csharp/blob/main/src/Dockerfile

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Copy SB.Utils project files and restore as distinct layers
COPY SB.Utils/*.csproj ./SB.Utils/
COPY SB.ExampleProcessor/*.csproj ./SB.ExampleProcessor/
WORKDIR /app/SB.ExampleProcessor
RUN dotnet restore 

# Copy everything else and build
WORKDIR /app
COPY SB.Utils/. ./SB.Utils/
COPY SB.ExampleProcessor/. ./SB.ExampleProcessor/
WORKDIR /app/SB.ExampleProcessor
RUN dotnet publish -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .

# NOTE: NO EXPOSE AS THIS IS A JOB, NOT A SERVICE: https://learn.microsoft.com/en-us/azure/container-apps/jobs?tabs=bash-cli-azcli#expose-ports-for-jobs
# EXPOSE 8080

ENTRYPOINT ["dotnet", "SB.ExampleProcessor.dll"]