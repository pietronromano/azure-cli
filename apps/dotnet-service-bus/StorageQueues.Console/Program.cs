/*
    -Reference: https://learn.microsoft.com/en-us/azure/storage/queues/storage-tutorial-queues?toc=/azure/storage/queues/toc.json
*/

using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
class Program
{

    static async Task Main(string[] args)
    {
        string connectionString = args[0];
        string queueName = args[1];

        // Instantiate a QueueClient which will be used to create and manipulate the queue
        QueueClient queueClient = new QueueClient(connectionString, queueName);

        // Define your message payload
        var payload = new
        {
            id = "aaa2c3d4-5e6f-7890-abcd-ef1234567890",
            companyId = "fff5a96e6-92a3-4c00-a58c-68236cf4cb8a",
            amountCents = 100000,
            currency = "EUR",
            operationDate = "2026-06-15T10:00:00+00:00",
            traceId = "test-001"
        };

        // Serialize to JSON
        string json = JsonSerializer.Serialize(payload);

        await InsertMessageAsync(queueClient, json);

        string retrievedMessage = await RetrieveNextMessageAsync(queueClient);

        Console.WriteLine($"Retrieved message: {retrievedMessage}");

        // Deserialize the message back to a strongly typed object
        //Note: Attribute names will start with captial after deserialization
        var deserializedPayload = JsonSerializer.Deserialize<QueuePayload>(retrievedMessage);
        Console.WriteLine($"Deserialized payload Id: {deserializedPayload?.Id}");
    }           

    static async Task InsertMessageAsync(QueueClient queue, string newMessage)
    {
        if (null != await queue.CreateIfNotExistsAsync())
        {
            Console.WriteLine($"The queue {queue.Name} was created.");
        }

        await queue.SendMessageAsync(newMessage);
    }

    static async Task<string> RetrieveNextMessageAsync(QueueClient theQueue)
    {
        if (await theQueue.ExistsAsync())
        {
            QueueProperties properties = await theQueue.GetPropertiesAsync();

            if (properties.ApproximateMessagesCount > 0)
            {
                QueueMessage[] retrievedMessage = await theQueue.ReceiveMessagesAsync(1);
                string theMessage = retrievedMessage[0].Body.ToString();
                await theQueue.DeleteMessageAsync(retrievedMessage[0].MessageId, retrievedMessage[0].PopReceipt);
                return theMessage;
            }

            return "";
        }

        return "";
    }           

}

public sealed record QueuePayload
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("companyId")]
    public string? CompanyId { get; init; }

    [JsonPropertyName("amountCents")]
    public int AmountCents { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    [JsonPropertyName("operationDate")]
    public string? OperationDate { get; init; }

    [JsonPropertyName("traceId")]
    public string? TraceId { get; init; }
}