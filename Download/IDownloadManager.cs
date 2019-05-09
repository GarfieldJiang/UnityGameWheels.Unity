using COL.UnityGameWheels.Core;

namespace COL.UnityGameWheels.Unity
{
    /// <summary>
    /// Interface of a download manager.
    /// </summary>
    public interface IDownloadManager : IManager
    {
        /// <summary>
        /// Download module.
        /// </summary>
        IDownloadModule Module { get; }

        /// <summary>
        /// Get or set the reference pool module.
        /// </summary>
        IRefPoolModule RefPoolModule { get; set; }

        /// <summary>
        /// Start a downloading task.
        /// </summary>
        /// <param name="downloadTaskInfo">Downloading task info.</param>
        /// <returns>A unique ID of the downloading task.</returns>
        int StartDownloading(DownloadTaskInfo downloadTaskInfo);

        /// <summary>
        /// Stop a downloading task.
        /// </summary>
        /// <param name="taskId">Downloading task ID.</param>
        /// <returns>True if there is a downloading task with this ID.</returns>
        bool StopDownloading(int taskId);
    }
}
