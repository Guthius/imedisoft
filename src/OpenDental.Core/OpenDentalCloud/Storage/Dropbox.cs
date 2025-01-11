using System;
using System.Threading;
using System.Threading.Tasks;
using Dropbox.Api;

namespace OpenDental.Cloud.Storage;

public class Dropbox
{
    public static bool FileExists(string accessToken, string filePath)
    {
        return FileExists(new DropboxClient(accessToken), filePath);
    }
        
    private static bool FileExists(DropboxClient client, string filePath)
    {
        var retVal = false;

        var wait = new ManualResetEvent(false);

        new Task(async () =>
        {
            try
            {
                await client.Files.GetMetadataAsync(filePath);
                retVal = true;
            }
            catch (Exception)
            {
                // ignored
            }

            wait.Set();
        }).Start();

        if (!wait.WaitOne(10000))
        {
            throw new Exception("Checking if file exists in Dropbox timed out.");
        }

        return retVal;
    }
}