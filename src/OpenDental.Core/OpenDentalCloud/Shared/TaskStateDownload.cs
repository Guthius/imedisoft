namespace OpenDental.Cloud.Shared
{
    public abstract class TaskStateDownload : TaskStateFile
    {
        private ulong _downloadFileSize;

        /// <summary>
        /// The total size of the file that is being downloaded.
        /// </summary>
        protected ulong DownloadFileSize
        {
            get
            {
                ulong downloadFileSize;
                lock (Lock)
                {
                    downloadFileSize = _downloadFileSize;
                }

                return downloadFileSize;
            }
            set
            {
                lock (Lock)
                {
                    _downloadFileSize = value;
                }
            }
        }
    }
}