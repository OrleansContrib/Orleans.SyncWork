namespace Orleans.SyncWork.Demo.Services.Grains;

public interface IPasswordVerifierGrain
    : ISyncWorker<PasswordVerifierRequest, PasswordVerifierResult>, IGrainWithGuidKey;
