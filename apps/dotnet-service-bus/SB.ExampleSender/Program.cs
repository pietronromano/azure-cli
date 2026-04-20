/*
    Example ASP.NET Core Web API application that sends messages to an Azure Service Bus Queue and processes messages from the queue. 
*/
using SB.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// Register SBUtilSender with factory to provide correct logger type
builder.Services.AddSingleton<SBUtilSender>(sp => 
{
    var logger = sp.GetRequiredService<ILogger<SBUtilSender>>();
    return new SBUtilSender(logger);
});

var app = builder.Build();

//Create an instance of the ServiceBusUtil class from DI to use for sending and processing messages. The ServiceBusUtil class handles authentication and interaction with the Azure Service Bus Queue.
var sbSender = app.Services.GetRequiredService<SBUtilSender>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Uncomment the following line if you want to use HTTPS redirection. Make sure to configure HTTPS in your development environment if you enable this.
//app.UseHttpsRedirection();

// Health check endpoint, returns 200 OK if the app is running

app.MapHealthChecks("/health");

//Send a message to the Service Bus Queue with the message and number of messages to send as query parameters
app.MapGet("/send", async (string message, int num, string sessionId) =>
{
    try
    {
        if (string.IsNullOrEmpty(message) || num <= 0)
        {
            return "Please provide a valid message and a positive number of messages to send as query parameters.\nExample: /send?message=Hello&num=5\n";
        }
        await sbSender.TestSendMessageBatch(message, num, sessionId);
        return $"A batch of {num} messages with body '{message}' has been published to the queue.\n";
    }
    catch (Exception ex)
    {
        return $"Error sending messages: {ex.Message}";
    }

})
.WithName("Send");


// Start the application
app.Run();

