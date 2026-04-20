/*
  Simplified .NET HTTP Server for testing purposes.
  Provides endpoints to retrieve environment info, environment variables, request info, and to post JSON data.
*/
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHealthChecks();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

// Health check endpoint, returns 200 OK if the app is running
app.MapHealthChecks("/health");

// GET endpoint: returns system info as JSON
app.MapGet("/system-info", () =>
{
  EnvironmentInfo info = new EnvironmentInfo();

  string json = JsonSerializer.Serialize(info);
  EnvironmentInfo.LogInfo("/system-info",json); 

  return Results.Ok(info);
});

// GET endpoint: returns environment variables as a pipe ("|") separated string
app.MapGet("/env-vars", () =>
{
  string variables = EnvironmentInfo.GetEnvironmentVariables();

  EnvironmentInfo.LogInfo("/env-vars",variables); 

  return Results.Ok(variables);
});

// GET endpoint: returns a hard codedversion number, independent of environment variables
// Used to verify that the deployment of new version has succeeded after updates
app.MapGet("/version", () =>
{
  return Results.Ok("v1.0.0");
});

// GET endpoint: returns request info as JSON
app.MapGet("/request-info", (HttpRequest request) =>
{
    RequestInfo info = new RequestInfo(request.HttpContext);
    string json = JsonSerializer.Serialize(info);
    EnvironmentInfo.LogInfo("/request-info", json);
    return Results.Ok(info);
});

// POST endpoint: receives any JSON data and returns it, wrapped in a JSON object
app.MapPost("/echo", (HttpRequest request, HttpResponse response) =>
{
  try
  {
    //response.CompleteAsync();
    if (request.BodyReader.TryRead(out ReadResult result))
    {
        var resultString = Encoding.UTF8.GetString(result.Buffer);
        return Results.Ok(new { requestReceived = resultString });
    }
    else
    {
        return Results.BadRequest(new { Error = "(Couldn't read Body)" });
    }

  }
  catch (System.Exception exc)
  {
    string msg = "/postjson exception: " + exc.Message;
    Console.WriteLine(msg);
    return Results.Problem(msg);
  }

});

app.MapGet("/log", (HttpRequest request, string message = "") =>
{
  //Create a new log file for each day
  string logFile = "log_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
  string logPath = Path.Join(Environment.CurrentDirectory, "logs");
  if (!Path.Exists(logPath))
      System.IO.Directory.CreateDirectory(logPath);

  logPath = Path.Join(logPath, logFile);
  
  // The StreamWriter is wrapped in a using declaration, which ensures proper disposal when it goes out of scope. 
  // No need for a manual Close() call since the using statement handles disposal automatically
  using StreamWriter logWriter = File.AppendText(logPath);
  logWriter.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
  logWriter.Flush();

  return Results.Ok(new { success = true, message = $"Log entry written: {message}", logFile = logFile });

});

//Configure the HTTP request pipeline:
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();


