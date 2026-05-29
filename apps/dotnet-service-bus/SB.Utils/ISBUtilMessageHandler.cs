namespace SB.Utils;

public interface ISBUtilMessageHandler
{
    /// <summary>
    /// Handles a message received from the Service Bus queue or topic subscription.
    /// This method is invoked by the SBUtilProcessor when a message is received and needs to be processed.
    /// </summary>
    /// <param name="body">The message body content as a string.</param>
    /// <param name="sequenceNumber">The unique sequence number assigned to the message by Service Bus.</param>
    /// <param name="messageId">The unique identifier of the message.</param>
    /// <param name="deliveryCount">The number of times this message has been delivered. Useful for detecting poison messages.</param>
    /// <param name="sessionId">The session identifier for session-enabled queues or subscriptions. Null for non-session messages.</param>
    /// <returns>A task that represents the asynchronous operation. Returns true if the message was processed successfully; otherwise, false.</returns>
    Task<bool> ProcessMessage(string body, long sequenceNumber, string messageId, 
            int deliveryCount, string sessionId = null);
    
}
