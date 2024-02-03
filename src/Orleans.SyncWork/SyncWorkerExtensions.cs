using System;
using System.Threading.Tasks;
using Orleans.SyncWork.Enums;
using Orleans.SyncWork.Exceptions;

namespace Orleans.SyncWork;

/// <summary>
/// Extension method(s) for <see cref="ISyncWorker{TRequest, TResult}"/>.
/// </summary>
public static class SyncWorkerExtensions
{
    /// <summary>
    /// <para>
    /// Starts the work of a <see cref="ISyncWorker{TRequest, TResult}"/>, polls it until a result is available,
    /// then returns it.
    /// </para>
    /// <para>
    /// Polls the <see cref="ISyncWorker{TRequest, TResult}"/> every 1000ms for a result, until one is available.
    /// </para>
    /// </summary>
    /// <typeparam name="TRequest">The type of request being dispatched.</typeparam>
    /// <typeparam name="TResult">The type of result expected from the work.</typeparam>
    /// <param name="worker">The <see cref="ISyncWorker{TRequest, TResult}"/> doing the work.</param>
    /// <param name="request">The request to be dispatched.</param>
    /// <returns>The result of the <see cref="ISyncWorker{TRequest, TResult}"/>.</returns>
    /// <exception cref="Exception">Thrown when the <see cref="ISyncWorker{TRequest, TResult}"/> is in a <see cref="SyncWorkStatus.Faulted"/> state.</exception>
    public static Task<TResult> StartWorkAndPollUntilResult<TRequest, TResult>(this ISyncWorker<TRequest, TResult> worker, TRequest request)
    {
        var grainCancellationToken = new GrainCancellationTokenSource().Token;
        return worker.StartWorkAndPollUntilResult(request, 1000, grainCancellationToken);
    }

    /// <summary>
    /// <para>
    /// Starts the work of a <see cref="ISyncWorker{TRequest, TResult}"/>, polls it until a result is available,
    /// then returns it.
    /// </para>
    /// <para>
    /// Polls the <see cref="ISyncWorker{TRequest, TResult}"/> every 1000ms for a result, until one is available.
    /// </para>
    /// </summary>
    /// <typeparam name="TRequest">The type of request being dispatched.</typeparam>
    /// <typeparam name="TResult">The type of result expected from the work.</typeparam>
    /// <param name="worker">The <see cref="ISyncWorker{TRequest, TResult}"/> doing the work.</param>
    /// <param name="request">The request to be dispatched.</param>
    /// <param name="grainCancellationToken">The token for cancelling tasks.</param>
    /// <returns>The result of the <see cref="ISyncWorker{TRequest, TResult}"/>.</returns>
    /// <exception cref="Exception">Thrown when the <see cref="ISyncWorker{TRequest, TResult}"/> is in a <see cref="SyncWorkStatus.Faulted"/> state.</exception>
    public static Task<TResult> StartWorkAndPollUntilResult<TRequest, TResult>(this ISyncWorker<TRequest, TResult> worker, TRequest request, GrainCancellationToken grainCancellationToken)
    {
        return worker.StartWorkAndPollUntilResult(request, 1000, grainCancellationToken);
    }

    /// <summary>
    /// Starts the work of a <see cref="ISyncWorker{TRequest, TResult}"/>, polls it until a result is available, then returns it.
    /// </summary>
    /// <remarks>
    ///     Caution is advised when setting the msDelayPerStatusPoll "too low" - 1000 ms seems to be pretty safe, 
    ///     but if the cluster is under *enough* load, that much grain polling could overwhelm it.
    /// </remarks>
    /// <typeparam name="TRequest">The type of request being dispatched.</typeparam>
    /// <typeparam name="TResult">The type of result expected from the work.</typeparam>
    /// <param name="worker">The <see cref="ISyncWorker{TRequest, TResult}"/> doing the work.</param>
    /// <param name="request">The request to be dispatched.</param>
    /// <param name="msDelayPerStatusPoll">
    ///     The ms delay per attempt to poll for a <see cref="SyncWorkStatus.Completed"/> or <see cref="SyncWorkStatus.Faulted"/> status.
    /// </param>
    /// <returns>The result of the <see cref="ISyncWorker{TRequest, TResult}"/>.</returns>
    /// <exception cref="Exception">Thrown when the <see cref="ISyncWorker{TRequest, TResult}"/> is in a <see cref="SyncWorkStatus.Faulted"/> state.</exception>
    public static async Task<TResult> StartWorkAndPollUntilResult<TRequest, TResult>(this ISyncWorker<TRequest, TResult> worker, TRequest request, int msDelayPerStatusPoll)
    {
        var grainCancellationToken = new GrainCancellationTokenSource().Token;
        return await StartWorkAndPollUntilResult(worker, request, msDelayPerStatusPoll, grainCancellationToken);
    }

    /// <summary>
    /// Starts the work of a <see cref="ISyncWorker{TRequest, TResult}"/>, polls it until a result is available, then returns it.
    /// </summary>
    /// <remarks>
    ///     Caution is advised when setting the msDelayPerStatusPoll "too low" - 1000 ms seems to be pretty safe, 
    ///     but if the cluster is under *enough* load, that much grain polling could overwhelm it.
    /// </remarks>
    /// <typeparam name="TRequest">The type of request being dispatched.</typeparam>
    /// <typeparam name="TResult">The type of result expected from the work.</typeparam>
    /// <param name="worker">The <see cref="ISyncWorker{TRequest, TResult}"/> doing the work.</param>
    /// <param name="request">The request to be dispatched.</param>
    /// <param name="msDelayPerStatusPoll">
    ///     The ms delay per attempt to poll for a <see cref="SyncWorkStatus.Completed"/> or <see cref="SyncWorkStatus.Faulted"/> status.
    /// </param>
    /// <param name="grainCancellationToken">The token for cancelling tasks.</param>
    /// <returns>The result of the <see cref="ISyncWorker{TRequest, TResult}"/>.</returns>
    /// <exception cref="Exception">Thrown when the <see cref="ISyncWorker{TRequest, TResult}"/> is in a <see cref="SyncWorkStatus.Faulted"/> state.</exception>
    public static async Task<TResult> StartWorkAndPollUntilResult<TRequest, TResult>(this ISyncWorker<TRequest, TResult> worker, TRequest request, int msDelayPerStatusPoll, GrainCancellationToken grainCancellationToken)
    {
        await worker.Start(request);
        await Task.Delay(100);

        while (true)
        {
            var status = await worker.GetWorkStatus();

            switch (status)
            {
                case SyncWorkStatus.Running:
                    await Task.Delay(msDelayPerStatusPoll);
                    break;
                case SyncWorkStatus.Completed:
                    return (await worker.GetResult())!;
                case SyncWorkStatus.Faulted:
                    var exception = await worker.GetException();
                    throw exception!;
                case SyncWorkStatus.NotStarted:
                    throw new InvalidStateException("This shouldn't happen, but if it does, it probably means the cluster may have died and restarted, and/or a timeout occurred and the grain got reinstantiated without firing off the work.");
                default:
                    throw new InvalidStateException("How did we even get here...?");
            }
        }
    }
}
