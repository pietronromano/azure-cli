namespace ServiceBus.Processor;

using ServiceBus.Utils;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    //Create an instance of the ServiceBusUtil class to use for sending and processing messages. The ServiceBusUtil class handles authentication and interaction with the Azure Service Bus Queue.
    private readonly ServiceBusUtil _serviceBusUtil = new ServiceBusUtil();
    
    /*
        You never call ExecuteAsync directly - it's framework-managed
        It runs on a background thread automatically
        The CancellationToken is provided by the hosting infrastructure to signal shutdown
        The while loop keeps the service running until a shutdown signal is received
        Inside the loop, you can perform any background work you need, and use the CancellationToken to gracefully handle shutdown requests
     */
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Start processing messages from the Service Bus Queue
        await _serviceBusUtil.ProcessMessages();
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}
