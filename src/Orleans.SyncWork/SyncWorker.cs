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
    protected readonly ILogger _logger;
    private readonly LimitedConcurrencyLevelTaskScheduler _limitedConcurrencyScheduler;

    private SyncWorkStatus _status = SyncWorkStatus.NotStarted;
    private TResult _result;
    private Exception _exception;
    private Task _task;

    protected SyncWorker(ILogger logger, LimitedConcurrencyLevelTaskScheduler limitedConcurrencyScheduler)
    {
        _logger = logger;
        _limitedConcurrencyScheduler = limitedConcurrencyScheduler;
    }

    /// <inheritdoc />
    public Task<bool> Start(TRequest request)
    {
        if (_task != null)
        {
            _logger.LogDebug($"{nameof(Start)}: Task already initialized upon call.");
            return Task.FromResult(false);
        }

        _logger.LogDebug($"{nameof(Start)}: Starting task, set status to running.");
        _status = SyncWorkStatus.Running;
        _task = CreateTask(request);

        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<SyncWorkStatus> GetWorkStatus()
    {
        return Task.FromResult(_status);
    }

    /// <inheritdoc />
    public Task<Exception> GetException()
    {
        if (_status != SyncWorkStatus.Faulted)
        {
            _logger.LogError("{nameof(this.GetException)}: Attempting to retrieve exception from grain when grain not in a faulted state ({_status}).", nameof(this.GetException), _status);
            throw new InvalidStateException(_status, SyncWorkStatus.Faulted);
        }

        _task = null;
        this.DeactivateOnIdle();

        return Task.FromResult(_exception);
    }

    /// <inheritdoc />
    public Task<TResult> GetResult()
    {
        if (_status != SyncWorkStatus.Completed)
        {
            _logger.LogError("{nameof(this.GetResult)}: Attempting to retrieve result from grain when grain not in a completed state ({_status}).", nameof(this.GetResult), _status);
            throw new InvalidStateException(_status, SyncWorkStatus.Completed);
        }

        _task = null;
        this.DeactivateOnIdle();

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
                _logger.LogInformation($"{nameof(this.CreateTask)}: Beginning work for task.");
                _result = await PerformWork(request);
                _exception = default;
                _status = SyncWorkStatus.Completed;
                _logger.LogInformation($"{nameof(this.CreateTask)}: Completed work for task.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(this.CreateTask)}: Exception during task.");
                _result = default;
                _exception = e;
                _status = SyncWorkStatus.Faulted;
            }
        }, CancellationToken.None, TaskCreationOptions.LongRunning, _limitedConcurrencyScheduler);
    }
}
