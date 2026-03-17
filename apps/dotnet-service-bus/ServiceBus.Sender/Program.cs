//Simple ASP.NET Core Web API application that sends messages to an Azure Service Bus Queue and processes messages from the queue. 
//The application uses Azure.Identity for authentication and Azure.Messaging.ServiceBus for interacting with the Service Bus.

using ServiceBus.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

//Create an instance of the ServiceBusUtil class to use for sending and processing messages. The ServiceBusUtil class handles authentication and interaction with the Azure Service Bus Queue.
var serviceBusUtil = new ServiceBusUtil();
bool processingHandlerRegistered = false; // Flag to track if the message handler has been registered

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Uncomment the following line if you want to use HTTPS redirection. Make sure to configure HTTPS in your development environment if you enable this.
app.UseHttpsRedirection();

//Send a message to the Service Bus Queue with the message and number of messages to send as query parameters
app.MapGet("/send", async (string message, int num) =>
{
    string result = await serviceBusUtil.SendMessages(message, num);
    return result;
})
.WithName("Send");

//Process a message from the Service Bus Queue
app.MapGet("/process", async () =>
{
    if (processingHandlerRegistered)
    {
        return "Message processing is already in progress. Check the console output for details.";
    } 
    else
    {
        await serviceBusUtil.ProcessMessages();
        processingHandlerRegistered = true;
        return "Processing messages from the Service Bus Queue. Check the console output for details.";

    }   
 })
.WithName("Process");


app.Run();

