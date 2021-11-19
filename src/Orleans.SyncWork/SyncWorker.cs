using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.SyncWork.Enums;
using Orleans.SyncWork.Exceptions;

namespace Orleans.SyncWork;

public abstract class SyncWorker<TRequest, TResult> : Grain, ISyncWorker<TRequest, TResult>, IGrain
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

    public Task<SyncWorkStatus> GetWorkStatus()
    {
        return Task.FromResult(_status);
    }

    public Task<Exception> GetException()
    {
        if (_status != SyncWorkStatus.Faulted)
        {
            _logger.LogError($"{nameof(this.GetException)}: Attempting to retrieve exception from grain when grain not in a faulted state ({_status}).");
            throw new InvalidStateException(_status, SyncWorkStatus.Faulted);
        }

        return Task.FromResult(_exception);
    }

    public Task<TResult> GetResult()
    {
        if (_status != SyncWorkStatus.Completed)
        {
            _logger.LogError($"{nameof(this.GetResult)}: Attempting to retrieve result from grain when grain not in a completed state ({_status}).");
            throw new InvalidStateException(_status, SyncWorkStatus.Completed);
        }

        _task = null;

        return Task.FromResult(_result);
    }

    protected abstract Task<TResult> PerformWork(TRequest request);

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
