using ServiceBus.Processor;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

/*
    When host.Run() is called, the hosting framework:
    Creates an instance of Worker (injecting ILogger<Worker> via the constructor)
    Calls StartAsync() on the BackgroundService base class
    StartAsync() internally calls your ExecuteAsync() method in a background thread
*/
host.Run();
