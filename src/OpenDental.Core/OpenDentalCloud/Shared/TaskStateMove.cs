namespace OpenDental.Cloud.Shared
{
    public abstract class TaskStateMove : TaskState
    {
        private int _countMoveFailed;
        private int _countMoveSuccess;
        private int _countMoveTotal;
        private string _fromPath;
        private string _toPath;

        /// <summary>
        /// The folder of the corresponding file to be downloaded
        /// </summary>
        public string FromPath
        {
            get
            {
                string fromPath;

                lock (Lock)
                {
                    fromPath = _fromPath;
                }

                return fromPath;
            }
            set
            {
                lock (Lock)
                {
                    _fromPath = value;
                }
            }
        }

        /// <summary>
        /// The folder of the corresponding file to be downloaded
        /// </summary>
        public string ToPath
        {
            get
            {
                string toPath;

                lock (Lock)
                {
                    toPath = _toPath;
                }

                return toPath;
            }
            set
            {
                lock (Lock)
                {
                    _toPath = value;
                }
            }
        }

        /// <summary>
        /// Number of move attempts that failed and are still in the original folder.
        /// </summary>
        public int CountFailed
        {
            get
            {
                int countMoveFailed;

                lock (Lock)
                {
                    countMoveFailed = _countMoveFailed;
                }

                return countMoveFailed;
            }
            set
            {
                lock (Lock)
                {
                    _countMoveFailed = value;
                }
            }
        }

        /// <summary>
        /// Number of move attempts that succeeded and have been removed from the original folder.
        /// </summary>
        public int CountSuccess
        {
            get
            {
                int countMoveSuccess;

                lock (Lock)
                {
                    countMoveSuccess = _countMoveSuccess;
                }

                return countMoveSuccess;
            }
            set
            {
                lock (Lock)
                {
                    _countMoveSuccess = value;
                }
            }
        }

        /// <summary>
        /// Number of total files to move from the original folder.
        /// </summary>
        public int CountTotal
        {
            get
            {
                int countMoveTotal;

                lock (Lock)
                {
                    countMoveTotal = _countMoveTotal;
                }

                return countMoveTotal;
            }
            set
            {
                lock (Lock)
                {
                    _countMoveTotal = value;
                }
            }
        }
    }
}