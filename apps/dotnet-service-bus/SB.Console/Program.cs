using Azure.Messaging.ServiceBus;
using System.Text.Json;

// Define your message payload
var payload = new
{
  topUpId = "a1b2c3d4-5e6f-7890-abcd-ef1234567890",
  companyId = "ee5a96e6-92a3-4c00-a58c-68236cf4cb8a",
  amountCents = 1001100,
  currency = "EUR",
  operationDate = "2026-04-15T10:00:00+00:00",
  country = "ES",
  type = "funds-load-by-card",
  concept = "RECARGA DE FONDOS PRUEBA 001",
  traceId = "test-001"
};

// Serialize to JSON
string json = JsonSerializer.Serialize(payload);

string connectionString = "Endpoint=sb:";
//Environment.GetEnvironmentVariable("SERVICE_BUS_CONNECTION_STRING")!;
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