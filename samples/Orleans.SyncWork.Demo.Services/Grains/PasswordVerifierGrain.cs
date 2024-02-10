using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Orleans.SyncWork.Demo.Services.Grains;

public class PasswordVerifierGrain : SyncWorker<PasswordVerifierRequest, PasswordVerifierResult>, IPasswordVerifierGrain
{
    private readonly IPasswordVerifier _passwordVerifier;

    public PasswordVerifierGrain(
        ILogger<PasswordVerifierGrain> logger,
        LimitedConcurrencyLevelTaskScheduler limitedConcurrencyLevelTaskScheduler,
        IPasswordVerifier passwordVerifier) : base(logger, limitedConcurrencyLevelTaskScheduler)
    {
        _passwordVerifier = passwordVerifier;
    }

    protected override async Task<PasswordVerifierResult> PerformWork(
        PasswordVerifierRequest request, GrainCancellationToken grainCancellationToken)
    {
        var verifyResult = await _passwordVerifier.VerifyPassword(request.PasswordHash, request.Password);

        return new PasswordVerifierResult()
        {
            IsValid = verifyResult
        };
    }
}

[GenerateSerializer]
public class PasswordVerifierRequest
{
    [Id(0)]
    public string? Password { get; set; }
    [Id(1)]
    public string? PasswordHash { get; set; }
}

[GenerateSerializer]
public class PasswordVerifierResult
{
    [Id(0)]
    public bool IsValid { get; set; }
}
