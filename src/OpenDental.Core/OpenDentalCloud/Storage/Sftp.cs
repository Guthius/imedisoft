using System;
using System.IO;
using System.Threading.Tasks;
using CodeBase;
using OpenDental.Cloud.Shared;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace OpenDental.Cloud.Storage;

public static class Sftp
{
    private static SftpClient Init(string host, string user, string pass, int port = 22)
    {
        return new SftpClient(new ConnectionInfo(host, port, user, new PasswordAuthenticationMethod(user, pass)));
    }

    public class Upload(string host, string user, string pass, int port = 22) : TaskStateUpload
    {
        private readonly SftpClient _client = Init(host, user, pass, port);

        protected override async Task PerformIO()
        {
            var hadToConnect = _client.ConnectIfNeeded();

            _client.CreateDirectoriesIfNeeded(Folder);

            var fullFilePath = Folder + '/' + FileName;

            using (var uploadStream = new MemoryStream(ByteArray))
            {
                var res = (SftpUploadAsyncResult) _client.BeginUploadFile(uploadStream, fullFilePath);
                while (!res.AsyncWaitHandle.WaitOne(100))
                {
                    if (DoCancel)
                    {
                        res.IsUploadCanceled = true;
                    }
                }

                _client.EndUploadFile(res);
                if (res.IsUploadCanceled)
                {
                    TaskStateDelete state = new Delete
                    {
                        Client = _client,
                        Path = fullFilePath
                    };
                    state.Execute();
                }
            }

            _client.DisconnectIfNeeded(hadToConnect);
            await Task.Run(() => { }); // Gets rid of a compiler warning and does nothing.
        }
    }

    public class Delete : TaskStateDelete
    {
        internal SftpClient Client;

        internal Delete()
        {
        }

        protected override async Task PerformIO()
        {
            var hadToConnect = Client.ConnectIfNeeded();

            await Task.Run(() => { Client.Delete(Path); });

            Client.DisconnectIfNeeded(hadToConnect);
        }
    }

    public static bool IsConnectionValid(string host, string user, string pass, int port = 22)
    {
        try
        {
            var client = Init(host, user, pass, port);

            client.Connect();

            if (client.IsConnected)
            {
                client.Disconnect();

                return true;
            }
        }
        catch
        {
            // ignored
        }

        return false;
    }
}

public static class SftpExtension
{
    public static void CreateDirectoriesIfNeeded(this SftpClient client, string path)
    {
        var hadToConnect = client.ConnectIfNeeded();
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        var currentDir = "";
        var directories = path.Split("/", StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < directories.Length; i++)
        {
            if (i > 0 || path[0] == '/')
            {
                currentDir += "/";
            }

            currentDir += directories[i];
            try
            {
                // This will throw an exception of SftpPathNotFoundException if the directory does not exist
                var attributes = client.GetAttributes(currentDir);

                // Check to see if it's a directory.  This will not throw an exception of SftpPathNotFoundException, so we want to break out if it's a file path.
                // This would be a weird permission issue or implementation error, but it doesn't hurt anything.
                if (!attributes.IsDirectory)
                {
                    break;
                }
            }
            catch (SftpPathNotFoundException)
            {
                client.CreateDirectory(currentDir);
            }
        }

        client.DisconnectIfNeeded(hadToConnect);
    }

    public static bool ConnectIfNeeded(this SftpClient client)
    {
        if (client.IsConnected)
        {
            return false;
        }

        try
        {
            client.Connect();
        }
        catch (Exception e)
        {
            throw new Exception("Connecting to " + client.ConnectionInfo.Host + " has failed with user: " + client.ConnectionInfo.Username + "\r\n" + e.Message, e);
        }

        return true;
    }

    public static void DisconnectIfNeeded(this SftpClient client, bool hadToConnect)
    {
        if (hadToConnect && client.IsConnected)
        {
            client.Disconnect();
        }
    }
}