using System;
using Orleans.SyncWork.Enums;

namespace Orleans.SyncWork.Exceptions;

/// <summary>
/// Exception used to describe when the consumer is requests data
/// from the <see cref="ISyncWorker{TRequest, TResult}"/>, when the grain
/// is not in a valid state to return the requested data.
/// </summary>
public class InvalidStateException : Exception
{
    /// <summary>
    /// Construct the exception with a specific message.
    /// </summary>
    /// <param name="message">The message that describes the invalid state.</param>
    public InvalidStateException(string message) : base(message) { }
    
    /// <summary>
    /// Construct the exception specifying the unexpectedStatus that was encountered.
    /// </summary>
    /// <param name="unexpectedStatus">The unexpected <see cref="SyncWorkStatus"/> that was encountered.</param>
    public InvalidStateException(SyncWorkStatus unexpectedStatus) : base(
        $"Grain was in an invalid state of {unexpectedStatus}.") { }
    
    /// <summary>
    /// Construct the exception that states an expected status, when the actual status was encountered.
    /// </summary>
    /// <param name="actualStatus">The actual <see cref="SyncWorkStatus"/> that occurred.</param>
    /// <param name="expectedStatus">The expected <see cref="SyncWorkStatus"/> state that should have been received.</param>
    public InvalidStateException(SyncWorkStatus actualStatus, SyncWorkStatus expectedStatus) : base(
        $"Grain was in an invalid state for the requested grain method.  Expected status {expectedStatus}, got {actualStatus}.") { }
}
