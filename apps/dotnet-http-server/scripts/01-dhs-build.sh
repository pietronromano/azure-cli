# Create a .Net Web App, with local build and run
## VS Code: for local build and debug, open the src folder in VS Code: 

# Move to the app directory
cd apps/dotnet-http-server/app

## Initial creation of the app
app="DotNetHttpServer"
dotnet new webapp -n $app --framework net10.0
cd $app

## dotnet commands
dotnet clean
dotnet restore
dotnet build

## Creates folder bin/publish
dotnet publish

## Run the app
dotnet run

## New Terminal: using preconfigured port 5087
curl http://localhost:5087/

### Send a GET request to the /request-info endpoint to see detailed information about the incoming HTTP request for debugging purposes.
curl http://localhost:5087/request-info

### Send a GET request to the /system-info endpoint to see detailed information about the system and environment for debugging purposes.
curl http://localhost:5087/system-info

### Send a GET request to the /log endpoint to log a message
curl "http://localhost:5087/log?message=HelloFromCurl"

### Send JSON via a POST request to the /echo endpoint
curl -X POST -H "Content-Type: application/json" -d '{"name":"Pluxee","role":"DevOps"}' http://localhost:5087/echo