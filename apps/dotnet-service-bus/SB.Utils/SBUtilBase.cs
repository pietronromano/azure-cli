/**
 Service Bus Base Client Utility Class
 Using SB.* naming to avoid confusion with Azure.Messaging.ServiceBus types.
 This class provides common functionality for initializing and validating a Service Bus client connection. 
*/

namespace SB.Utils;


using Azure.Messaging.ServiceBus;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public class SBUtilBase
{
    protected ServiceBusClient _client;
    protected int _clientMaxRetryAttempts = 3; // Default max retry attempts for client operations, can be overridden by environment variable
    protected int _clientDelaySeconds = 5; // Default delay for client operations, can be overridden by environment variable
    protected int _clientMaxDelaySeconds = 10; // Default delay for message processing, can be overridden by environment variable
    protected int _clientTryTimeoutSeconds = 20; // Default timeout for client operations, can be overridden by environment variable

    protected string _serviceBusQueue;
    protected readonly ILogger<SBUtilBase> _logger;
    protected EnvironmentInfo _envInfo;


    public SBUtilBase(ILogger<SBUtilBase> logger)
    {
        _logger = logger;

        _envInfo = new EnvironmentInfo();
        string envInfoJson = JsonSerializer.Serialize(_envInfo, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        _logger.LogInformation("Initializing SBUtilBase with Environment Info: {EnvironmentInfo}", envInfoJson);

        // Get configuration values from environment variables with defaults
        _clientMaxRetryAttempts = GetIntFromEnv("CLIENT_MAX_RETRY_ATTEMPTS", _clientMaxRetryAttempts);
        _clientDelaySeconds = GetIntFromEnv("CLIENT_DELAY_SECONDS", _clientDelaySeconds);
        _clientMaxDelaySeconds = GetIntFromEnv("CLIENT_MAX_DELAY_SECONDS", _clientMaxDelaySeconds);
        _clientTryTimeoutSeconds = GetIntFromEnv("CLIENT_TRY_TIMEOUT_SECONDS", _clientTryTimeoutSeconds);
     }

    protected int GetIntFromEnv(string envVarName, int defaultValue)
    {
        string envValue = Environment.GetEnvironmentVariable(envVarName);
        if (!string.IsNullOrEmpty(envValue) && int.TryParse(envValue, out int outValue))
        {
            return outValue;
        }
        else
        {
            _logger.LogWarning($"Missing or invalid environment variable: {envVarName} is not set or not an integer. Using default value of {defaultValue}.");
            return defaultValue;
        }
    }

    protected bool GetBoolFromEnv(string envVarName, bool defaultValue)
    {
        string envValue = Environment.GetEnvironmentVariable(envVarName);
        if (!string.IsNullOrEmpty(envValue) && bool.TryParse(envValue, out bool outValue))
        {
            return outValue;
        }
        else
        {
            _logger.LogWarning($"Missing or invalid environment variable: {envVarName} is not set or not a boolean. Using default value of {defaultValue}.");
            return defaultValue;
        }
    }

    protected void InitializeClient()
    {

        try
        {
            if (_client != null )
            {
                return; // Client is already initialized
            }
            // The Service Bus client types are safe to cache and use as a singleton for the lifetime
            // of the application, which is best practice when messages are being published or read regularly.
            // Set the transport type to AmqpWebSockets so that the ServiceBusClient uses the port 443. 
            // If you use the default AmqpTcp, ensure that ports 5671 and 5672 are open.
            // Configure retry policy to fail fast instead of retrying indefinitely
            var clientOptions = new ServiceBusClientOptions
            { 
                TransportType = ServiceBusTransportType.AmqpWebSockets,
                RetryOptions = new ServiceBusRetryOptions
                {
                    Mode = ServiceBusRetryMode.Fixed,
                    MaxRetries = _clientMaxRetryAttempts,
                    Delay = TimeSpan.FromSeconds(_clientDelaySeconds),
                    MaxDelay = TimeSpan.FromSeconds(_clientMaxDelaySeconds),
                    TryTimeout = TimeSpan.FromSeconds(_clientTryTimeoutSeconds)
                }
            };
            string connectionString = Environment.GetEnvironmentVariable("SERVICE_BUS_CONNECTION_STRING");
            _serviceBusQueue = Environment.GetEnvironmentVariable("SERVICE_BUS_QUEUE");

            // Use connection string for local development if provided
            if (!string.IsNullOrEmpty(connectionString))
            {
                _client = new ServiceBusClient(connectionString, clientOptions);
                _logger.LogInformation("Created Client using connection string for Service Bus authentication");
            }
            else
            {
                // Use managed identity for Azure/Production (NOTE: Managed Identities only work when running in an Azure environment)
                // Requires identity to have: Azure Service Bus Data Owner | Sender
                string clientId = Environment.GetEnvironmentVariable("MANAGED_IDENTITY_CLIENT_ID");
                var credential = new DefaultAzureCredential(
                new DefaultAzureCredentialOptions
                {
                    ManagedIdentityClientId = clientId
                });

                string serviceBusNamespace = Environment.GetEnvironmentVariable("SERVICE_BUS_NAMESPACE");
                _client = new ServiceBusClient(
                    $"{serviceBusNamespace}.servicebus.windows.net",
                    credential,
                    clientOptions);
                _logger.LogInformation("Created Client using DefaultAzureCredential for Service Bus authentication");

            }
          
            // Validate connection by attempting to peek a message with a short timeout
            // This fails fast during initialization rather than hanging in background retries
            _logger.LogInformation("Validating Service Bus connection...");
            ValidateConnectionAsync(_serviceBusQueue).GetAwaiter().GetResult();
            _logger.LogInformation("Service Bus connection validated successfully");
        }
        catch (System.Exception exc)
        {
            _logger.LogError(exc, "An error occurred while initializing the Service Bus client. Shutting down");
            throw; // Re-throw to propagate the error to the caller
        }
    }

    protected async Task ValidateConnectionAsync(string queueName)
    {
        try
        {
            // Create a receiver just for validation
            await using var receiver = _client.CreateReceiver(queueName);
            // Try to peek a message with a 10 second timeout - this validates connectivity
            // We don't care if there are messages or not, just that we can connect
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await receiver.PeekMessageAsync(cancellationToken: cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Timeout is okay - it means we couldn't peek but connection might be fine
            // Let's try one more validation
            _logger.LogWarning("Peek operation timed out, but this may be normal if queue is empty");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection validation failed");
            throw;
        }
    }

}