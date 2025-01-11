using System.Collections.Generic;

namespace OpenDental.Cloud.Shared
{
    public abstract class TaskStateListFolders : TaskState
    {
        private List<string> _listFolderPathDisplay = new();
        private string _folderPath;

        /// <summary>
        /// The folder of the corresponding file to be downloaded
        /// </summary>
        public string FolderPath
        {
            get
            {
                string folderPath;

                lock (Lock)
                {
                    folderPath = _folderPath;
                }

                return folderPath;
            }
            set
            {
                lock (Lock)
                {
                    _folderPath = value;
                }
            }
        }

        /// <summary>
        /// List of cased paths that were found in
        /// </summary>
        public List<string> ListFolderPathsDisplay
        {
            get
            {
                var listFolderPaths = new List<string>();

                lock (Lock)
                {
                    foreach (var path in _listFolderPathDisplay)
                    {
                        listFolderPaths.Add(path);
                    }
                }

                return listFolderPaths;
            }
            protected set
            {
                lock (Lock)
                {
                    _listFolderPathDisplay = value;
                }
            }
        }
    }
}