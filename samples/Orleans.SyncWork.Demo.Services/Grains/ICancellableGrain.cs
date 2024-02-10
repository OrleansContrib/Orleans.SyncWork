namespace Orleans.SyncWork.Demo.Services.Grains;

public interface ICancellableGrain
    : ISyncWorker<SampleCancellationRequest, SampleCancellationResult>, IGrainWithGuidKey;
