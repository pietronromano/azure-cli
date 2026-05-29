/**
 Worker Class example for a .NET Worker Service that processes messages from an Azure Service Bus Queue. 
 This class inherits from BackgroundService, which provides a base implementation for long-running services.
 References:
 - Workers: https://learn.microsoft.com/en-us/dotnet/core/extensions/workers
 - Logging: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging/overview
*/



namespace SB.ExampleProcessor;

using Azure.Messaging.ServiceBus;
using SB.Utils;

public sealed class Worker(
    SBUtilProcessor sbprocessor,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<Worker> logger) : BackgroundService, ISBUtilMessageHandler
   {
    private readonly EnvironmentInfo _envInfo = new();
    private int _instanceMessageCount = 0;
    private DateTime _lastMessageTime = DateTime.UtcNow;
    
    /*
        You never call ExecuteAsync directly - it's framework-managed
        It runs on a background thread automatically
        The CancellationToken is provided by the hosting infrastructure to signal shutdown
    */
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // ServiceBusUtil is injected via constructor dependency injection
            logger.LogInformation("Calling ServiceBusUtil.ProcessMessages...");
            await sbprocessor.ProcessMessages(this, stoppingToken);
            
            // If we reach here, ProcessMessages returned (idle timeout)
            logger.LogInformation("ProcessMessages completed at: {time}", DateTimeOffset.Now);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Shutdown requested");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Critical error processing messages: {Message}", ex.Message);
            // Force immediate exit with error code 1 for critical failures
            // This is necessary because StopApplication() waits for background tasks
            // (like Service Bus retry loops) which may never complete
            logger.LogCritical("Forcing application exit with code 1");
            Environment.Exit(1);
        }
        finally
        {
            // Signal the host to shut down after processing is complete (normal/idle timeout)
            logger.LogInformation("Stopping the application...");
            hostApplicationLifetime.StopApplication();      
        }
    }

    public async Task<bool> ProcessMessage(string body, long sequenceNumber, string messageId, int deliveryCount, 
                    string? sessionId = null)
    {
        if (string.IsNullOrEmpty(body))
        {
            body = "<EMPTY MESSAGE BODY>";
        }
        else if (body.Length > 30)
        {
            body = body.Substring(0, 30) + "...(truncated)";
        }
        _instanceMessageCount++;
        if (String.IsNullOrEmpty(sessionId))
        {
            sessionId = "<NO SESSION>";
        }
        string processStartTime = DateTime.UtcNow.ToString("o"); // ISO 8601 format
        string infoMessage = $"Starting Processing Message: | "
            + $"SessionId: {sessionId} | "
            + $"InstanceMessageCount: {_instanceMessageCount} | "
            + $"MessageBody: {body} | SequenceNumber: {sequenceNumber} | " 
            + $"HostProcessID: {_envInfo.HostProcessId} | InfoGuid: {_envInfo.InfoGuid} | "  
            + $"MessageId: {messageId} | DeliveryCount: {deliveryCount} |";
        logger.LogInformation(infoMessage);

        
        // Simulate message processing logic here: sleep for 10 seconds:
        await Task.Delay(10000);
        string processEndTime = DateTime.UtcNow.ToString("o"); // ISO 8601 format
        logger.LogInformation($"Finished Processing Message with MessageId: {messageId} "
        + $"at {processEndTime}");

        // Update last message time
        _lastMessageTime = DateTime.UtcNow;

        // Simulate error handling: if the message body contains "error", throw an exception to test retry logic
        if (body.Contains("error", StringComparison.OrdinalIgnoreCase))
            return false;
        else
            return true;
    }
}
