using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Orleans.SyncWork.Demo.Services.Grains;

public class PasswordVerifier : SyncWorker<PasswordVerifierRequest, PasswordVerifierResult>
{
    private readonly IPasswordVerifier _passwordVerifier;

    public PasswordVerifier(
        ILogger<PasswordVerifier> logger, 
        LimitedConcurrencyLevelTaskScheduler limitedConcurrencyLevelTaskScheduler, 
        IPasswordVerifier passwordVerifier) : base(logger, limitedConcurrencyLevelTaskScheduler)
    {
        _passwordVerifier = passwordVerifier;
    }

    protected override async Task<PasswordVerifierResult> PerformWork(PasswordVerifierRequest request)
    {
        var verifyResult = await _passwordVerifier.VerifyPassword(request.PasswordHash, request.Password);

        return new PasswordVerifierResult()
        {
            IsValid = verifyResult
        };
    }
}
public class PasswordVerifierRequest
{
    public string Password { get; init; }
    public string PasswordHash { get; init; }
}

public class PasswordVerifierResult
{
    public bool IsValid { get; init; }
}
