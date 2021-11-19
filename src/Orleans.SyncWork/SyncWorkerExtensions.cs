using System;
using System.Threading.Tasks;
using Orleans.SyncWork.Enums;

namespace Orleans.SyncWork;

public static class SyncWorkerExtensions
{
    /// <summary>
    /// Starts the work of a <see cref="ISyncWorker{TRequest, TResult}"/>, polls it until a result is available, then returns it.
    /// </summary>
    /// <typeparam name="TRequest">The type of request being dispatched.</typeparam>
    /// <typeparam name="TResult">The type of result expected from the work.</typeparam>
    /// <param name="worker">The <see cref="ISyncWorker{TRequest, TResult}"/> doing the work.</param>
    /// <param name="request">The request to be dispatched.</param>
    /// <returns>The result of the <see cref="ISyncWorker{TRequest, TResult}"/>.</returns>
    /// <exception cref="Exception">Thrown when the <see cref="ISyncWorker{TRequest, TResult}"/> is in a <see cref="SyncWorkStatus.Faulted"/> state.</exception>
    public static async Task<TResult> StartWorkAndPollUntilResult<TRequest, TResult>(this ISyncWorker<TRequest, TResult> worker, TRequest request)
    {
        await worker.Start(request);
        await Task.Delay(100);

        SyncWorkStatus status;
        while (true)
        {
            status = await worker.GetWorkStatus();

            switch (status)
            {
                case SyncWorkStatus.Running:
                    await Task.Delay(1000);
                    break;
                case SyncWorkStatus.Completed:
                    return await worker.GetResult();
                case SyncWorkStatus.Faulted:
                    var exception = await worker.GetException();
                    throw exception;
                case SyncWorkStatus.NotStarted:
                    throw new Exception("This should happen, but if it does, I'm assuming it means the cluster may have died, or a timeout occurred and the grain got reinstantiated without firing off the work.");
                default:
                    throw new Exception("How did we even get here...?");
            }
        }
    }
}
