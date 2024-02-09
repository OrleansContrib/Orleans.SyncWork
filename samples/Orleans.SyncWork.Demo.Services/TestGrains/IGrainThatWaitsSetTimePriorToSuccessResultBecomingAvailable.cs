namespace Orleans.SyncWork.Demo.Services.TestGrains;

public interface
    IGrainThatWaitsSetTimePriorToSuccessResultBecomingAvailable :
    ISyncWorker<TestDelaySuccessRequest, TestDelaySuccessResult>, IGrainWithGuidKey;
