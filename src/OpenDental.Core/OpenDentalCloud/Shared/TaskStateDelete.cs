namespace OpenDental.Cloud.Shared;

public abstract class TaskStateDelete : TaskState
{
    private string _path;
    
    public string Path
    {
        get
        {
            string path;
                
            lock (Lock)
            {
                path = _path;
            }

            return path;
        }
        set
        {
            lock (Lock)
            {
                _path = value;
            }
        }
    }
}