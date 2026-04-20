/**
  Service Bus Processor example 
 Using SB.* naming to avoid confusion with Azure.Messaging.ServiceBus types.
References:
- Sessions: 
    - Important: When you enable sessions on a queue or a subscription, client applications can no longer send or receive regular messages. 
      Sessions can only be enabled on Creating the queue or subscription, they cannot be enabled on an existing queue or subscription.
      Clients MUST send messages as part of a session by setting the session ID and receive messages by accepting the session. 
      Clients can still peek a queue or subscription that has sessions enabled. For more information, see Message browsing.
    - https://learn.microsoft.com/en-us/azure/service-bus-messaging/message-sessions
    - https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/samples/Sample03_SendReceiveSessions.md
    - https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/samples/Sample05_SessionProcessor.md
- Long Running Processing: https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/samples/Sample18_LongRunningProcessing.md
*/

namespace SB.Utils;

// VS Code shows this as an unused using, but it's actually used in the code. We can ignore the warning.
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

public class SBUtilProcessor: SBUtilBase
{
    private  ServiceBusProcessor _processor;
    private  ServiceBusSessionProcessor _sessionProcessor;

    private DateTime _lastMessageTime;
    private CancellationTokenSource _shutdownCts;
    private int _consecutiveErrors = 3;
    private int _maxConsecutiveErrors = 5;
    private int _instanceMessageCount = 0; // Counter to track number of messages processed for this instance, for logging purposes

    private readonly int _idleTimeoutMinutes = 5; // in minutes, default to 5 minutes of idle time before shutdown. Adjust based on expected message frequency and processing time. Set it to at least the longest expected time between messages, plus some margin for safety.
    
    //Long Running Processing:
    // The client automatically renews the lock in the background for up to this duration.
    // Set it to at least the longest expected processing time (including any retries or
    // delays in your handler), plus ~25% margin for safety.
    private int _maxAutoLockRenewalMinutes = 120; // in minutes, default to 2 hours which is the max allowed by Service Bus. Set it to at least the longest expected processing time (including any retries or delays in your handler), plus ~25% margin for safety.
    
    // Disable auto-complete so we settle messages explicitly after processing succeeds.          
    private bool _autoCompleteMessages = false;

    // Process one message at a time. Increase for higher throughput if the processing is I/O-bound rather than CPU-bound.
    // Note: MaxConcurrentCalls = 1 limits parallelism but does not guarantee processing ordering. 
    // Use sessions if message processing ordering is required.   
    private int _maxConcurrentCalls = 1;
    private bool _enableSessions = false; // Set to true if processing a session-enabled queue or subscription. Note that enabling sessions will limit you to processing one message at a time per session, even if MaxConcurrentCalls is greater than 1, since messages with the same SessionId must be processed in order.                

    public SBUtilProcessor(ILogger<SBUtilBase> logger): base(logger)
    {
        // Configure Environment variables
        _maxConsecutiveErrors = GetIntFromEnv("MAX_CONSECUTIVE_ERRORS", _maxConsecutiveErrors);
        _idleTimeoutMinutes = GetIntFromEnv("IDLE_TIMEOUT_MINUTES", _idleTimeoutMinutes);
        _maxAutoLockRenewalMinutes = GetIntFromEnv("MAX_AUTO_LOCK_RENEWAL_MINUTES", _maxAutoLockRenewalMinutes);
        _maxConcurrentCalls = GetIntFromEnv("MAX_CONCURRENT_CALLS", _maxConcurrentCalls);
        _autoCompleteMessages = GetBoolFromEnv("AUTO_COMPLETE_MESSAGES", _autoCompleteMessages);
        _enableSessions = GetBoolFromEnv("ENABLE_SESSIONS", _enableSessions);
    }


    private void InitializeProcessor(string[] sessionIds = null)
    {

        try
        {
            if (_processor != null)
            {
                return; // Processor is already initialized
            }
            base.InitializeClient(); // Ensure client is initialized (will do nothing if already initialized)
           

            if (_enableSessions)
            {
                var sessionProcessorOptions = new ServiceBusSessionProcessorOptions
                {
                    MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(_maxAutoLockRenewalMinutes),
                    AutoCompleteMessages = _autoCompleteMessages,
                    MaxConcurrentSessions = _maxConcurrentCalls
                };
                
                // Add session IDs if provided
                if (sessionIds != null)
                {
                    foreach (var sessionId in sessionIds)
                    {
                        sessionProcessorOptions.SessionIds.Add(sessionId);
                    }
                }
                
                _sessionProcessor = _client.CreateSessionProcessor(_serviceBusQueue, sessionProcessorOptions);
                _sessionProcessor.SessionInitializingAsync += SessionInitializingHandler;
                _sessionProcessor.SessionClosingAsync += SessionClosingHandler;
                _logger.LogInformation("Created Session Processor for Service Bus queue {ServiceBusQueue}", _serviceBusQueue);
            }
            else
            {
                _processor = _client.CreateProcessor(_serviceBusQueue, 
                    new ServiceBusProcessorOptions{
                        MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(_maxAutoLockRenewalMinutes),
                        AutoCompleteMessages = _autoCompleteMessages,
                        MaxConcurrentCalls = _maxConcurrentCalls
                    }
                );
                _logger.LogInformation("Created Processor for Service Bus queue {ServiceBusQueue}", _serviceBusQueue);
            }
        }
        catch (System.Exception exc)
        {
            _logger.LogError(exc, "An error occurred while initializing the Service Bus client. Shutting down");
            throw; // Re-throw to propagate the error to the caller
        }
    }

    public async Task ProcessMessages(CancellationToken hostToken, string[] sessionIds = null)
    {
        try
        {
            // Initialize processor if not already done - will throw on error
            InitializeProcessor(sessionIds);
            
            string sessionInfo = (sessionIds != null && sessionIds.Length > 0) 
                    ? string.Join(",", sessionIds) : "None.";

            _logger.LogInformation(
                $"Started processing messages. | " +
                $"Host Process ID: {_envInfo.HostProcessId}. | " + 
                $"Info GUID: {_envInfo.InfoGuid}. | " +
                $"SessionIds: {sessionInfo}. | " +
                $"Will shut down after {_idleTimeoutMinutes} minutes of inactivity. |");

            // Initialize last message time
            _lastMessageTime = DateTime.UtcNow;
            // Create a linked cancellation token source that will be cancelled when either the host signals shutdown or when we want to shut down due to idle timeout
            _shutdownCts = CancellationTokenSource.CreateLinkedTokenSource(hostToken);
            
            if (_enableSessions)
            {
                // add handler to process session messages
                _sessionProcessor.ProcessMessageAsync += SessionMessageHandler;
                // add handler to process any errors
                _sessionProcessor.ProcessErrorAsync += ErrorHandler;
                // start processing 
                await _sessionProcessor.StartProcessingAsync();
            }
            else
            {
                // add handler to process messages
                _processor.ProcessMessageAsync += MessageHandler;
                // add handler to process any errors
                _processor.ProcessErrorAsync += ErrorHandler;
                // start processing 
                await _processor.StartProcessingAsync();
            }
           
            // Monitor for idle timeout
            await MonitorIdleTimeout(_shutdownCts.Token);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing messages");      
            // try Clean shutdown
            if (_processor != null && _processor.IsProcessing)
            {
                await _processor.StopProcessingAsync();
                _logger.LogInformation("Processor stopped");
            }
            if (_sessionProcessor != null && _sessionProcessor.IsProcessing)
            {
                await _sessionProcessor.StopProcessingAsync();
                _logger.LogInformation("Session Processor stopped");
            }
            // If an error occurs, we want to ensure the host is signaled to shut down, since the processor may not be in a healthy state to continue processing messages.
            throw new Exception("An error occurred while processing messages. See inner exception for details.", ex);
        }
    }

    private async Task MonitorIdleTimeout(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                
                var idleTime = DateTime.UtcNow - _lastMessageTime;
                if (idleTime >= TimeSpan.FromMinutes(_idleTimeoutMinutes))
                {
                     _logger.LogInformation(
                    $"No messages received for {idleTime.TotalMinutes:F1} minutes. Shutting down.... | " +
                    $"FinalInstanceMessageCount: {_instanceMessageCount}. | " +
                    $"HostProcessID: {_envInfo.HostProcessId}. | " + 
                    $"InfoGuid: {_envInfo.InfoGuid}. | ");
                
                    _shutdownCts?.Cancel();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while monitoring idle timeout");    
        }
    }

    private void HandleMessage(string body, long sequenceNumber, string messageId, int deliveryCount){
        if (string.IsNullOrEmpty(body))
        {
            body = "<EMPTY MESSAGE BODY>";
        }
        else if (body.Length > 30)
        {
            body = body.Substring(0, 30) + "...(truncated)";
        }
        _instanceMessageCount++;
        string infoMessage = $"InstanceMessageCount: {_instanceMessageCount} | "
            + $"MessageBody: {body} | SBSequenceNumber: {sequenceNumber} | " 
            + $"HostProcessID: {_envInfo.HostProcessId} | InfoGuid: {_envInfo.InfoGuid} | "  
            + $"MessageId: {messageId} | DeliveryCount: {deliveryCount}  |";
        _logger.LogInformation(infoMessage);

        // Update last message time
        _lastMessageTime = DateTime.UtcNow;
        // Reset consecutive error counter on successful processing
            _consecutiveErrors = 0;
    }
    // handle received messages
    async Task MessageHandler(ProcessMessageEventArgs args)
    {
        try
        {
            HandleMessage(args.Message.Body.ToString(), args.Message.SequenceNumber, 
                args.Message.MessageId, args.Message.DeliveryCount);
            
            // complete the message. message is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MessageHandler");
        }

    }

    // handle Session received messages
    async Task SessionMessageHandler(ProcessSessionMessageEventArgs args)
    {
        try
        {
            string sessionId = String.IsNullOrEmpty(args.SessionId) ? "Unknown" : args.SessionId;
            _logger.LogInformation($"Received message ID {args.Message.MessageId} for SessionId: {sessionId}");
            HandleMessage(args.Message.Body.ToString(), args.Message.SequenceNumber, 
                args.Message.MessageId, args.Message.DeliveryCount);

            // complete the message. message is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MessageHandler");
        }

    }

    // handle any errors when receiving messages
    Task ErrorHandler(ProcessErrorEventArgs args)
    {
        try
        {
            _consecutiveErrors++;
            
            _logger.LogError(
                args.Exception, 
                $"Service Bus Error #{_consecutiveErrors}. Source: {args.ErrorSource}, Entity: {args.EntityPath}, Namespace: {args.FullyQualifiedNamespace}");
            
            // Check for fatal errors that should cause immediate exit
            var exceptionMessage = args.Exception.Message.ToLower();
            bool isFatalError = 
                exceptionMessage.Contains("unauthorized") ||
                exceptionMessage.Contains("not authorized") ||
                exceptionMessage.Contains("authentication failed") ||
                exceptionMessage.Contains("connection refused") ||
                exceptionMessage.Contains("entity could not be found") ||
                args.Exception is Azure.Messaging.ServiceBus.ServiceBusException sbEx && 
                    (sbEx.Reason == ServiceBusFailureReason.MessagingEntityNotFound);
            
            if (isFatalError)
            {
                _logger.LogCritical(
                    args.Exception,
                    "Fatal Service Bus error detected. Forcing application exit with code 1");
                Environment.Exit(1);
            }
            
            // Force exit if we've had too many consecutive errors
            if (_consecutiveErrors >= _maxConsecutiveErrors)
            {
                _logger.LogCritical(
                    "Reached {MaxErrors} consecutive errors. Forcing application exit with code 1",
                    _maxConsecutiveErrors);
                Environment.Exit(1);
            }
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ErrorHandler");
            // Even if error handler fails, we should exit to avoid infinite loops
            Environment.Exit(1);
            return Task.CompletedTask;
        }

    }

    async Task SessionInitializingHandler(ProcessSessionEventArgs args)
    {
        try{
            string sessionId = String.IsNullOrEmpty(args.SessionId) ? "Unknown" : args.SessionId;
             _logger.LogInformation($"Session initializing. SessionId: {sessionId}. HostProcessID: {_envInfo.HostProcessId}. InfoGuid: {_envInfo.InfoGuid}.");
            
            //await args.SetSessionStateAsync(new BinaryData("Some state specific to this session when the session is opened for processing."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SessionInitializingHandler");
        }
    }

    async Task SessionClosingHandler(ProcessSessionEventArgs args)
    {
        try{
            string sessionId = String.IsNullOrEmpty(args.SessionId) ? "Unknown" : args.SessionId;
            _logger.LogInformation($"Session closing. SessionId: {sessionId}. HostProcessID: {_envInfo.HostProcessId}. InfoGuid: {_envInfo.InfoGuid}.");
            
            // We may want to clear the session state when no more messages are available for the session or when some known terminal message
            // has been received. This is entirely dependent on the application scenario.
            // BinaryData sessionState = await args.GetSessionStateAsync();
            // if (sessionState.ToString() ==
            //     "Some state that indicates the final message was received for the session")
            // {
            //     await args.SetSessionStateAsync(null);
            // }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SessionClosingHandler");
        }
    }
}