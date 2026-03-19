namespace ServiceBus.Utils;

using Azure.Messaging.ServiceBus;
using Azure.Identity;

public class ServiceBusUtil
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ServiceBusProcessor _processor;
    private readonly Logger _logger;
    private DateTime _lastMessageTime;
    private readonly TimeSpan _idleTimeout;
    private CancellationTokenSource? _shutdownCts;
   

    public ServiceBusUtil()
    {
        _logger = new Logger();

        // Configure idle timeout (default 5 minutes, configurable via environment variable)
        string? idleTimeoutMinutes = Environment.GetEnvironmentVariable("IDLE_TIMEOUT_MINUTES");


        if(String.IsNullOrEmpty(idleTimeoutMinutes))
        {
            _logger.Write("Missing environment variables: IDLE_TIMEOUT_MINUTES is not set. Please check configuration. Exiting message processing..");
            return;
        }

        _idleTimeout = int.TryParse(idleTimeoutMinutes, out int timeout) 
            ? TimeSpan.FromMinutes(timeout) 
            : TimeSpan.FromMinutes(5);

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
                return;
            }
            
            // Initialize last message time
            _lastMessageTime = DateTime.UtcNow;
            _shutdownCts = new CancellationTokenSource();
            
            // add handler to process messages
            _processor.ProcessMessageAsync += MessageHandler;

            // add handler to process any errors
            _processor.ProcessErrorAsync += ErrorHandler;

            // start processing 
            await _processor.StartProcessingAsync();
            _logger.Write($"Started processing messages. Will shut down after {_idleTimeout.TotalMinutes} minutes of inactivity.");

            // Monitor for idle timeout
            await MonitorIdleTimeout(_shutdownCts.Token);

        }
        catch (Exception ex)
        {
            string errorMessage = $"An error occurred while processing messages: {ex.Message}";
            _logger.Write(errorMessage);      
        }
        finally
        {
            // Clean shutdown
            if (_processor != null && _processor.IsProcessing)
            {
                await _processor.StopProcessingAsync();
                _logger.Write("Processor stopped.");
            }
        }
    }

    private async Task MonitorIdleTimeout(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            
            var idleTime = DateTime.UtcNow - _lastMessageTime;
            if (idleTime >= _idleTimeout)
            {
                _logger.Write($"No messages received for {idleTime.TotalMinutes:F1} minutes. Shutting down...");
                _shutdownCts?.Cancel();
                break;
            }
        }
    }

    // handle received messages
    async Task MessageHandler(ProcessMessageEventArgs args)
    {
        string body = args.Message.Body.ToString();
        _logger.Write($"ProcessMessages: {body}");

        // Update last message time
        _lastMessageTime = DateTime.UtcNow;

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