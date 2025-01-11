namespace OpenDental.Cloud.Shared;

public abstract class TaskStateFile : TaskState
{
    private string _folder;
    private string _fileName;
    private byte[] _fileContent = new byte[1];
    
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