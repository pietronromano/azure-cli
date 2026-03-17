namespace ServiceBus.Utils;

using Azure.Messaging.ServiceBus;
using Azure.Identity;

public class ServiceBusUtil
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ServiceBusProcessor _processor;
    private readonly Logger _logger;
   

    public ServiceBusUtil()
    {
        _logger = new Logger();

        // The Service Bus client types are safe to cache and use as a singleton for the lifetime
        // of the application, which is best practice when messages are being published or read regularly.
        // Set the transport type to AmqpWebSockets so that the ServiceBusClient uses the port 443. 
        // If you use the default AmqpTcp, ensure that ports 5671 and 5672 are open.
        var clientOptions = new ServiceBusClientOptions
        { 
            TransportType = ServiceBusTransportType.AmqpWebSockets
        };

        try
        {
            string? connectionString = Environment.GetEnvironmentVariable("SERVICE_BUS_CONNECTION_STRING");
            string? serviceBusQueue = Environment.GetEnvironmentVariable("SERVICE_BUS_QUEUE");

            if(String.IsNullOrEmpty(serviceBusQueue))
            {
                _logger.Write("Missing environment variables: Service Bus queue name is not set. Please check configuration");
                return;
            }

            // Use connection string for local development if provided
            if (!string.IsNullOrEmpty(connectionString))
            {
                _client = new ServiceBusClient(connectionString, clientOptions);
                _logger.Write("Created Client using connection string for Service Bus authentication");
            }
            else
            {
                // Use managed identity for Azure/Production (NOTE: Managed Identities only work when running in an Azure environment)
                // Requires identity to have: Azure Service Bus Data Owner | Sender
                string? clientId = Environment.GetEnvironmentVariable("MANAGED_IDENTITY_CLIENT_ID");
                var credential = new DefaultAzureCredential(
                new DefaultAzureCredentialOptions
                {
                    ManagedIdentityClientId = clientId
                });

                string? serviceBusNamespace = Environment.GetEnvironmentVariable("SERVICE_BUS_NAMESPACE");
                _client = new ServiceBusClient(
                    $"{serviceBusNamespace}.servicebus.windows.net",
                    credential,
                    clientOptions);
                _logger.Write("Created Client using DefaultAzureCredential for Service Bus authentication");

            }

            _sender = _client.CreateSender(serviceBusQueue);   
            _processor = _client.CreateProcessor(serviceBusQueue, new ServiceBusProcessorOptions());
            _logger.Write($"Created Sender and Processor for Service Bus queue {serviceBusQueue}");

        }
        catch (System.Exception exc)
        {
            string errorMessage = "An error occurred while initializing the Service Bus client: " + exc.Message;
            _logger.Write(errorMessage);
        }

    }

    // publish messages to the Service Bus queue
    public async Task<string> SendMessages(string messageBody, int numOfMessages)
    {
        if (_client == null)
        {
            string notInitializedMessage = "Service Bus client is not initialized. Please check previous log messages for errors during initialization.";
            _logger.Write(notInitializedMessage);
            return notInitializedMessage;
        }
        
        // create a batch 
        using ServiceBusMessageBatch messageBatch = await _sender.CreateMessageBatchAsync();

        for (int i = 1; i <= numOfMessages; i++)
        {
            // try adding a message to the batch
            if (!messageBatch.TryAddMessage(new ServiceBusMessage($"{messageBody} {i}")))
            {
                // if it is too large for the batch
                throw new Exception($"The message {i} is too large to fit in the batch.");
            }
        }

        try
        {
            // Use the producer client to send the batch of messages to the Service Bus queue
            await _sender.SendMessagesAsync(messageBatch);
            string result = $"A batch of {numOfMessages} messages with body '{messageBody}' has been published to the queue.";
            _logger.Write(result);
            return result;
        }
        catch (Exception ex)
        {
            string errorMessage = $"An error occurred while sending the batch of messages: {ex.Message}";
            _logger.Write(errorMessage);
            return errorMessage;        
        }
    }

    public async Task ProcessMessages()
    {

        try
        {
            if (_processor == null)
            {
                string notInitializedMessage = "Service Bus Processor is not initialized. Please check previous log messages for errors during initialization.";
                _logger.Write(notInitializedMessage);
            }
            // add handler to process messages
            _processor.ProcessMessageAsync += MessageHandler;

            // add handler to process any errors
            _processor.ProcessErrorAsync += ErrorHandler;

            // start processing 
            await _processor.StartProcessingAsync();

        }
        catch (Exception ex)
        {
            string errorMessage = $"An error occurred while processing messages: {ex.Message}";
            _logger.Write(errorMessage);      
        }
    }

    // handle received messages
    async Task MessageHandler(ProcessMessageEventArgs args)
    {
        string body = args.Message.Body.ToString();
        _logger.Write($"ProcessMessages: {body}");

        // complete the message. message is deleted from the queue. 
        await args.CompleteMessageAsync(args.Message);
    }

    // handle any errors when receiving messages
    Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.Write($"ProcessMessages Error: {args.Exception}");
        return Task.CompletedTask;
    }

}