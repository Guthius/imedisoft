namespace OpenDental.Cloud.Shared
{
    public abstract class TaskStateFile : TaskState
    {
        /// <summary>
        /// If a file is greater than 2MB in size, we will break it up into chunks when uploading it to Dropbox.
        /// </summary>
        protected const int MaxFileSizeBytes = 2000000;

        private string _folder;
        private string _fileName;
        private byte[] _fileContent = new byte[1];

        /// <summary>
        /// The folder of the corresponding file to be downloaded
        /// </summary>
        public string Folder
        {
            get
            {
                string folder;

                lock (Lock)
                {
                    folder = _folder;
                }

                return folder;
            }
            set
            {
                lock (Lock)
                {
                    _folder = value;
                }
            }
        }

        /// <summary>
        /// The file name of the file to be downloaded.
        /// </summary>
        public string FileName
        {
            get
            {
                string fileName;

                lock (Lock)
                {
                    fileName = _fileName;
                }

                return fileName;
            }
            set
            {
                lock (Lock)
                {
                    _fileName = value;
                }
            }
        }

        /// <summary>
        /// The file stored in bytes.
        /// This value will grow while the download is still in progress.
        /// </summary>
        public byte[] ByteArray
        {
            get
            {
                byte[] fileContent;

                lock (Lock)
                {
                    fileContent = _fileContent;
                }

                return fileContent;
            }
            set
            {
                lock (Lock)
                {
                    _fileContent = value;
                }
            }
        }
    }
}