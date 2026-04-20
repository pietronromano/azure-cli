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

- Open same folder twice in VS Code, one for sender and one for receiver, to enable debugging both at the same time.
- In the first VS Code window, dotnet-service-bus folder 
- For the second VS Code Window: Vs Code -> File -> Open Workspace from File -> select the dotnet-service-bus-1st.code-workspace file

---

## SB.Utils

```bash
dotnet new classlib -n SB.Utils -f net10.0
cd SB.Utils

# Add Packages
dotnet add package Azure.Identity
dotnet add package Azure.Messaging.ServiceBus

# Build
dotnet build

```
---





