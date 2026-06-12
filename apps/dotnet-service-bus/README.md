# Service Bus with Managed Identity 
Sample code (NOT PRODUCTION QUALITY) based on the Azure SDK for .NET, demonstrating how to use Azure Service Bus with Managed Identity for authentication. This sample includes both a sender and a receiver application.

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

## Send a REST request to the API endpoint (for testing purposes, using Postman or curl)

```bash
curl -X POST "http://localhost:5141/send?num=1&sessionId=" \
-H "Content-Type: application/json" \
-d '{
  "Id": "a1b2c3d4-5e6f-7890-abcd-ef1234567890",
  "companyId": "ss5a96e6-92a3-4c00-a58c-68236cf4cb8a",
  "amount": 1000,
  "currency": "USD",
}'
```

---
