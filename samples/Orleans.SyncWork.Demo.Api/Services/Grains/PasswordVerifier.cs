using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Orleans.SyncWork.Demo.Api.Services.Grains;

public class PasswordVerifier : SyncWorker<PasswordVerifierRequest, PasswordVerifierResponse>, IGrain
{
    private readonly IPasswordVerifier _passwordVerifier;

    public PasswordVerifier(
        ILogger<PasswordVerifier> logger, 
        LimitedConcurrencyLevelTaskScheduler limitedConcurrencyLevelTaskScheduler, 
        IPasswordVerifier passwordVerifier) : base(logger, limitedConcurrencyLevelTaskScheduler)
    {
        _passwordVerifier = passwordVerifier;
    }

    protected override async Task<PasswordVerifierResponse> PerformWork(PasswordVerifierRequest request)
    {
        var verifyResult = await _passwordVerifier.VerifyPassword(request.PasswordHash, request.Password);

        return new PasswordVerifierResponse()
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

public class PasswordVerifierResponse
{
    public bool IsValid { get; set; }
}
