using Azure.Messaging.ServiceBus;
using System.Text.Json;

// Define your message payload
var payload = new
{
  Id = "ddddb2c3d4-5e6f-7890-abcd-ef1234567890",
  companyId = "essss96e6-92a3-4c00-a58c-68236cf4cb8a",
  amountCents = 100,
  currency = "USD"
};

// Serialize to JSON
string json = JsonSerializer.Serialize(payload);

string connectionString = Environment.GetEnvironmentVariable("SERVICE_BUS_CONNECTION_STRING")!;
string queueName = Environment.GetEnvironmentVariable("SERVICE_BUS_QUEUE") ?? "session-queue";

await using var client = new ServiceBusClient(connectionString);
ServiceBusSender sender = client.CreateSender(queueName);

var message = new ServiceBusMessage(json)
{
    ContentType = "application/json",
    // Optional: set a session ID if using session-enabled queues
    // SessionId = "session-001"
};

await sender.SendMessageAsync(message);
Console.WriteLine($"Sent JSON message to queue '{queueName}'");

await sender.DisposeAsync();