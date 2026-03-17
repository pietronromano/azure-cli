# Service Bus with Managed Identity 

References:
- https://learn.microsoft.com/en-us/samples/azure/azure-sdk-for-net/azuremessagingservicebus-samples/
- https://learn.microsoft.com/en-gb/azure/service-bus-messaging/service-bus-dotnet-get-started-with-queues?tabs=passwordless
- https://learn.microsoft.com/en-gb/azure/service-bus-messaging/service-bus-managed-service-identity
- https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/samples/Sample01_SendReceive.md
- **Identity**: https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/identity/Azure.Identity/README.md#specify-a-user-assigned-managed-identity-with-defaultazurecredential


---

## VS Code: 
- for local build and debug, open the dotnet-service-bus folder in VS Code 

### Debugger types in VS Code for .NET
There are two debugger types for .NET in VS Code:
- "**dotnet**" - The newer unified debugger (C# Dev Kit extension)
  - Does not support env property in launch.json
  - Uses launchSettings.json for environment variables
  - More integrated with modern .NET tooling
- "**coreclr**" - The legacy debugger (OmniSharp C# extension)
  - Does support env property directly in launch.json
  - More straightforward for setting environment variables
  - Still widely used and supported
  - If you want to set environment variables directly in launch.json, using "**coreclr**" is the better choice.

### Duplicate Workspace (Full New Window) 
This is the most common way to get a completely separate window for the same folder, including its own Explorer sidebar and debug sessions. It creates an "Untitled Workspace" which allows you to open the same folder path twice without conflicts.
- Command Palette: Press Ctrl+Shift+P (Windows/Linux) or Cmd+Shift+P (Mac).
- Search: Type "Duplicate Workspace" and select Workspaces: Duplicate As Workspace in New Window.
- Result: A new window opens with the same folder. Note that this technically creates an "Untitled Workspace" to bypass the restriction of opening the exact same folder path twice.


---

## ServiceBus.Utils

```bash
dotnet new classlib -n ServiceBus.Utils -f net10.0
cd ServiceBus.Utils

# Add Packages
dotnet add package Azure.Identity
dotnet add package Azure.Messaging.ServiceBus

# Build
dotnet build

```
---





