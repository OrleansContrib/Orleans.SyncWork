namespace Orleans.SyncWork.Demo.Services.TestGrains;

public interface
    IGrainThatWaitsSetTimePriorToExceptionResultBecomingAvailable :
    ISyncWorker<TestDelayExceptionRequest, TestDelayExceptionResult>, IGrainWithGuidKey;
