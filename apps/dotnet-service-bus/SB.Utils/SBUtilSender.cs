/**
  Service Bus Sender example 
  Using SB.* naming to avoid confusion with Azure.Messaging.ServiceBus types.
*/

namespace SB.Utils;

// VS Code shows this as an unused using, but it's actually used in the code. We can ignore the warning.
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;


public class SBUtilSender: SBUtilBase
{
    private ServiceBusSender _sender;
   
    public SBUtilSender(ILogger<SBUtilBase> logger) : base(logger)
    {
        // Base constructor is called before this body executes
    }

    private void InitializeSender()
    {

        try
        {
            if (_sender != null )
            {
                return; // Client is already initialized
            }
            base.InitializeClient(); // Ensure client is initialized (will do nothing if already initialized)
            
            _sender = _client.CreateSender(_serviceBusQueue);   
            
            _logger.LogInformation("Created Sender  Service Bus queue {ServiceBusQueue}", _serviceBusQueue);
            
        }
        catch (System.Exception exc)
        {
            _logger.LogError(exc, "An error occurred while initializing the Service Bus sender. Shutting down");
            throw; // Re-throw to propagate the error to the caller
        }
    }

    // publish messages to the Service Bus queue
    public async Task TestSendMessageBatch(string messageBody, int numOfMessages, string sessionId = null)
    {
        // Initialize sender if not already done - will throw on error if needed
        InitializeSender();
        
        // create a batch 
        try
        {
            using ServiceBusMessageBatch messageBatch = await _sender.CreateMessageBatchAsync();
            for (int i = 1; i <= numOfMessages; i++)
            {
                // try adding a message to the batch
                var message = new ServiceBusMessage($"{messageBody} Batch#{i}");
                if (!string.IsNullOrEmpty(sessionId))
                {
                    message.SessionId = sessionId;
                }
                if (!messageBatch.TryAddMessage(message))
                {
                    // if it is too large for the batch
                    throw new Exception($"The message {i} is too large to fit in the batch.");
                }
            }
            // Use the producer client to send the batch of messages to the Service Bus queue
            await _sender.SendMessagesAsync(messageBatch);
            string result = $"A batch of {numOfMessages} messages with body '{messageBody}' has been published to the queue.";
            _logger.LogInformation(result);
        }
        catch (Exception ex)
        {
            string errorMessage = $"An error occurred while sending the batch of messages: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            //Throwing an exception here to stop processing and ensure that the caller is aware that the operation did not succeed.
            throw new Exception(errorMessage, ex);        
        }
    }


}