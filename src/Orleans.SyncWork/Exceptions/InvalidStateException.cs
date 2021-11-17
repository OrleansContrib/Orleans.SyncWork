using System;
using Orleans.SyncWork.Enums;

namespace Orleans.SyncWork.Exceptions;

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
