using Azure.Messaging.ServiceBus;
using System.Text.Json;

// Define your message payload
var payload = new
{
  IdId = "aaa2c3d4-5e6f-7890-abcd-ef1234567890",
  companyId = "fff5a96e6-92a3-4c00-a58c-68236cf4cb8a",
  amountCents = 100000,
  currency = "EUR",
  operationDate = "2026-06-15T10:00:00+00:00",
  traceId = "test-001"
};

// Serialize to JSON
string json = JsonSerializer.Serialize(payload);

//Environment.GetEnvironmentVariable("SERVICE_BUS_CONNECTION_STRING")!;
string connectionString = "Endpoint=sb:";

string queueName = "...queue"; // Environment.GetEnvironmentVariable("SERVICE_BUS_QUEUE") ?? "session-queue";

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

// create a receiver that we can use to receive the message
ServiceBusReceiver receiver = client.CreateReceiver(queueName);

// the received message is a different type as it contains some service set properties
ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();

// get the message body as a string
string body = receivedMessage.Body.ToString();
Console.WriteLine(body);