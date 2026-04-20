# SB.ExampleProcessor

# Create a new Worker project and add the necessary packages for Azure Service Bus and Azure Identity. 

# Move to the service-bus directory
cd apps/dotnet-service-bus

# List of available templates: https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new-list  
dotnet new list
 
# Initial creation of the app
app="SB.ExampleProcessor"

# Worker project (most common for background services). 
dotnet new worker -n $app --framework net10.0
cd $app

# Add Packages
dotnet add reference ../SB.Utils/SB.Utils.csproj

## dotnet commands
dotnet clean
dotnet restore
dotnet build

## Creates folder bin/publish
dotnet publish

## Run the app
dotnet run
