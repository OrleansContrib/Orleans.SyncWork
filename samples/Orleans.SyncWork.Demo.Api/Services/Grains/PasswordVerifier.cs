using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Orleans.SyncWork.Demo.Api.Services.Grains;

public class PasswordVerifier : SyncWorker<PasswordVerifierRequest, PasswordVerifierResult>, IGrain
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
    public string Password { get; set; }
    public string PasswordHash { get; set; }
}

public class PasswordVerifierResult
{
    public bool IsValid { get; set; }
}
