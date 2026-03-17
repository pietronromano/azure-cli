# Create a .Net Web App, with local build and run
## VS Code: for local build and debug, open the src folder in VS Code: 

# Move to the service-bus directory
cd apps/dotnet-service-bus

# List of available templates: https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new-list  
dotnet new list

# Initial creation of the app
app="ServiceBus.Sender"

# ASP.NET Core Web API (most common for container apps). Minimal APIs work great for lightweight services
dotnet new webapi -n $app --framework net10.0
cd $app

# Add Packages
dotnet add reference ../ServiceBus.Utils/ServiceBus.Utils.csproj

## dotnet commands
dotnet clean
dotnet restore
dotnet build

## Creates folder bin/publish
dotnet publish

## Run the app
dotnet run


## New Terminal: using preconfigured port 5141
curl http://localhost:5141/send?message=my-message?num=10

