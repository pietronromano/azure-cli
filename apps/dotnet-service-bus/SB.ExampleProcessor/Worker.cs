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
    ILogger<Worker> logger) : BackgroundService
{
    
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
            await sbprocessor.ProcessMessages(stoppingToken);
            
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
}
