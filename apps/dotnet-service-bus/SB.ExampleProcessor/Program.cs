/*
    Example Worker Service application that processes messages from an Azure Service Bus Queue. 
    The Worker class is a BackgroundService that continuously listens for messages on the queue and processes them using the SBUtilProcessor class.
    The SBUtilProcessor class is responsible for handling the logic of processing messages from the Service Bus
*/

using SB.ExampleProcessor;
using SB.Utils;

var builder = Host.CreateApplicationBuilder(args);

// Register SBUtilProcessor with factory to provide correct logger type
builder.Services.AddSingleton<SBUtilProcessor>(sp => 
{
    var logger = sp.GetRequiredService<ILogger<SBUtilBase>>();
    return new SBUtilProcessor(logger);
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

/*
    When host.Run() is called, the hosting framework:
    Creates an instance of Worker (injecting ILogger<Worker> via the constructor)
    Calls StartAsync() on the BackgroundService base class
    StartAsync() internally calls your ExecuteAsync() method in a background thread
*/
host.Run();
