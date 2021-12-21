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
    public InvalidStateException(string message) : base(message) { }
    
    public InvalidStateException(SyncWorkStatus unexpectedStatus) : base(
        $"Grain was in an invalid state of {unexpectedStatus}.") { }
    
    public InvalidStateException(SyncWorkStatus actualStatus, SyncWorkStatus expectedStatus) : base(
        $"Grain was in an invalid state for the requested grain method.  Expected status {expectedStatus}, got {actualStatus}.") { }
}
