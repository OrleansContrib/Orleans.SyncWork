using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.SyncWork.Enums;
using Orleans.SyncWork.Exceptions;

namespace Orleans.SyncWork;

/// <summary>
/// This class should be used as the base class for extending for the creation of long running, cpu bound, synchronous work.
/// 
/// It relies on a configured <see cref="LimitedConcurrencyLevelTaskScheduler"/> that limits concurrent work to some level
/// below the CPU being "fully engaged with work", as to leave enough resources for the Orleans async messaging to get through.
/// </summary>
/// <typeparam name="TRequest">The request type (arguments/parameters) for a long running piece of work.</typeparam>
/// <typeparam name="TResult">The result/response for a long running piece of work.</typeparam>
public abstract class SyncWorker<TRequest, TResult> : Grain, ISyncWorker<TRequest, TResult>
{
    /// <summary>
    /// The logger for the instance.
    /// </summary>
    protected readonly ILogger Logger;
    private readonly LimitedConcurrencyLevelTaskScheduler _limitedConcurrencyScheduler;

    private SyncWorkStatus _status = SyncWorkStatus.NotStarted;
    private TResult? _result;
    private Exception? _exception;
    private Task? _task;

    /// <summary>
    /// Constructs an instance of the <see cref="SyncWorker{TRequest,TResult}"/>.
    /// </summary>
    /// <param name="logger">The logger for the instance</param>
    /// <param name="limitedConcurrencyScheduler">The task scheduler that will be used for the long running work.</param>
    protected SyncWorker(ILogger logger, LimitedConcurrencyLevelTaskScheduler limitedConcurrencyScheduler)
    {
        Logger = logger;
        _limitedConcurrencyScheduler = limitedConcurrencyScheduler;
    }

    /// <inheritdoc />
    public Task<bool> Start(TRequest request)
    {
        if (_task != null)
        {
            Logger.LogDebug("{Method}: Task already initialized upon call.", nameof(Start));
            return Task.FromResult(false);
        }

        Logger.LogDebug("{Method}: Starting task, set status to running.", nameof(Start));
        _status = SyncWorkStatus.Running;
        _task = CreateTask(request);

        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<SyncWorkStatus> GetWorkStatus()
    {
        if (_status == SyncWorkStatus.NotStarted)
        {
            Logger.LogError("{Method} was in a status of {WorkStatus}", nameof(GetWorkStatus), SyncWorkStatus.NotStarted);
            DeactivateOnIdle();
            throw new InvalidStateException(_status);
        }

        return Task.FromResult(_status);
    }

    /// <inheritdoc />
    public Task<Exception?> GetException()
    {
        if (_status != SyncWorkStatus.Faulted)
        {
            Logger.LogError("{Method}: Attempting to retrieve exception from grain when grain not in a faulted state ({_status}).", nameof(GetException), _status);
            DeactivateOnIdle();
            throw new InvalidStateException(_status, SyncWorkStatus.Faulted);
        }

        _task = null;
        this.DeactivateOnIdle();

        return Task.FromResult(_exception);
    }

    /// <inheritdoc />
    public Task<TResult?> GetResult()
    {
        if (_status != SyncWorkStatus.Completed)
        {
            Logger.LogError("{Method}: Attempting to retrieve result from grain when grain not in a completed state ({Status}).", nameof(GetResult), _status);
            DeactivateOnIdle();
            throw new InvalidStateException(_status, SyncWorkStatus.Completed);
        }

        _task = null;
        DeactivateOnIdle();

        return Task.FromResult(_result);
    }

    /// <summary>
    /// The method that actually performs the long running work.
    /// </summary>
    /// <param name="request">The request/parameters used for the execution of the method.</param>
    /// <returns></returns>
    protected abstract Task<TResult> PerformWork(TRequest request);

    /// <summary>
    /// The task creation that fires off the long running work to the <see cref="LimitedConcurrencyLevelTaskScheduler"/>.
    /// </summary>
    /// <param name="request">The request to use for the invoke of the long running work.</param>
    /// <returns>a <see cref="Task"/> representing the fact that the work has been dispatched.</returns>
    private Task CreateTask(TRequest request)
    {
        return Task.Factory.StartNew(async () =>
        {
            try
            {
                Logger.LogInformation("{Method}: Beginning work for task.", nameof(CreateTask));
                _result = await PerformWork(request);
                _exception = default;
                _status = SyncWorkStatus.Completed;
                Logger.LogInformation("{Method}: Completed work for task.", nameof(CreateTask));
            }
            catch (Exception e)
            {
                Logger.LogError(e, "{Method)}: Exception during task.", nameof(CreateTask));
                _result = default;
                _exception = e;
                _status = SyncWorkStatus.Faulted;
            }
        }, CancellationToken.None, TaskCreationOptions.LongRunning, _limitedConcurrencyScheduler);
    }
}
