using System;
using System.Threading.Tasks;
using Orleans.SyncWork.Enums;

namespace Orleans.SyncWork;

/// <summary>
/// Provides a means of Starting long running work, polling said work, and retrieving an eventual result/exception.
/// </summary>
/// <typeparam name="TRequest">The type of request to dispatch.</typeparam>
/// <typeparam name="TResult">The type of result to receive.</typeparam>
public interface ISyncWorker<in TRequest, TResult> : IGrainWithGuidKey
{
    /// <summary>
    /// Start long running work with the provided parameter.
    /// </summary>
    /// <param name="request">The parameter containing all necessary information to start the workload.</param>
    /// <returns>true if work is started, false if it was already started.</returns>
    Task<bool> Start(TRequest request);
    /// <summary>
    /// Gets the long running work status.
    /// </summary>
    /// <returns>The status of the long running work.</returns>
    Task<SyncWorkStatus> GetWorkStatus();
    /// <summary>
    /// The result of the long running work.
    /// </summary>
    /// <returns>The result of the work done through the SyncWorker.</returns>
    Task<TResult> GetResult();
    /// <summary>
    /// Gets the exception information when the long running work faulted.
    /// </summary>
    /// <returns>The exception information as it relates to the failure.</returns>
    Task<Exception> GetException();
}
