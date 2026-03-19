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

  return info;
});

// GET endpoint: returns environment variables as a pipe ("|") separated string
app.MapGet("/env-vars", () =>
{
  string variables = EnvironmentInfo.GetEnvironmentVariables();

  EnvironmentInfo.LogInfo("/env-vars",variables); 

  return variables;
});

// GET endpoint: returns a version number: use to verify deployment has succeeded after updates
app.MapGet("/version", () =>
{
  return "v2.0.0";
});

// GET endpoint: returns request info as JSON
app.MapGet("/request-info", (HttpRequest request) =>
{
    RequestInfo info = new RequestInfo(request.HttpContext);
    string json = JsonSerializer.Serialize(info);
    EnvironmentInfo.LogInfo("/request-info", json);
    return info;
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
        return "{\"requestReceived\":\"" + resultString + "\"}";
    }
    else
    {
        return "{\"Error\":\"(Couldn't read Body)\"}";
    }

  }
  catch (System.Exception exc)
  {
    string msg = "/postjson exception: " + exc.Message;
    Console.WriteLine(msg);
    return msg;
  }

});


//Configure the HTTP request pipeline:
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();


