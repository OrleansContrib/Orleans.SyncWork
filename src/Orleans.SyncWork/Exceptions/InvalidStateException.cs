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
    SyncWorkStatus ActualStatus { get; }
    SyncWorkStatus ExpectedStatus { get; }

    public InvalidStateException(SyncWorkStatus actualStatus, SyncWorkStatus expectedStatus) : base()
    {
        ActualStatus = actualStatus;
        ExpectedStatus = expectedStatus;
    }
}
