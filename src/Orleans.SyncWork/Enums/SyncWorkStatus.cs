namespace Orleans.SyncWork.Enums;

/// <summary>
/// The working status of the <see cref="ISyncWorker{TRequest, TResult}"/> grain.
/// </summary>
public enum SyncWorkStatus
{
    /// <summary>
    /// Work has not yet been started.
    /// </summary>
    NotStarted,
    /// <summary>
    /// Work is actively running or is in queue to be run.
    /// </summary>
    Running,
    /// <summary>
    /// The work has been completed, and a result is available.
    /// </summary>
    Completed,
    /// <summary>
    /// The work has been completed, though an exception was thrown.
    /// </summary>
    Faulted,
}
