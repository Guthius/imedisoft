using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CodeBase;
using OpenDental.Cloud.Shared;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace OpenDental.Cloud.Storage
{
    public static class Sftp
    {
        private static SftpClient Init(string host, string user, string pass, int port = 22)
        {
            return new SftpClient(new ConnectionInfo(host, port, user, new PasswordAuthenticationMethod(user, pass)));
        }

        public class Upload : TaskStateUpload
        {
            private readonly SftpClient _client;

            public Upload(string host, string user, string pass, int port = 22)
            {
                _client = Init(host, user, pass, port);
            }

            protected override async Task PerformIO()
            {
                var hadToConnect = _client.ConnectIfNeeded();

                _client.CreateDirectoriesIfNeeded(Folder);

                var fullFilePath = ODFileUtils.CombinePaths(Folder, FileName, '/');

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

        public class Download : TaskStateDownload
        {
            private static readonly string StashRootFolderPath = ODFileUtils.CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "OpenDental", "Stash");
            private static readonly string StashInstanceFolderPath = ODFileUtils.CombinePaths(StashRootFolderPath, Process.GetCurrentProcess().Id.ToString());
            internal SftpClient Client;
            public const bool DoUseStash = true;

            public Download(string host, string user, string pass, int port = 22)
            {
                Client = Init(host, user, pass, port);
            }

            internal Download()
            {
            }

            protected override async Task PerformIO()
            {
                var hadToConnect = Client.ConnectIfNeeded();
                var fullFilePath = ODFileUtils.CombinePaths(Folder, FileName, '/');
                var attribute = Client.GetAttributes(fullFilePath);
                string stashFilePath;

                if (DoUseStash)
                {
                    try
                    {
                        stashFilePath = ODFileUtils.CombinePaths(StashInstanceFolderPath, fullFilePath);

                        var stashFileParentDir = Directory.GetParent(stashFilePath)?.FullName ?? string.Empty;
                        if (!Directory.Exists(stashFileParentDir))
                        {
                            Directory.CreateDirectory(stashFileParentDir);
                        }

                        if (File.Exists(stashFilePath))
                        {
                            var stashFileInfo = new FileInfo(stashFilePath);
                            while ((DateTime.Now - stashFileInfo.CreationTime).TotalSeconds < 15)
                            {
                                // while recently created (currently being written to)
                                if (stashFileInfo.LastWriteTime == attribute.LastWriteTime)
                                {
                                    break;
                                }

                                Thread.Sleep(10);
                            }

                            if (stashFileInfo.LastWriteTime == attribute.LastWriteTime)
                            {
                                ByteArray = ProtectedData.Unprotect(File.ReadAllBytes(stashFilePath), null, DataProtectionScope.CurrentUser);
                                return;
                            }
                        }
                        else
                        {
                            // Create immediately as an empty file so that any read attempts will know that the file is in the process of being downloaded.
                            var fileStream = File.Create(stashFilePath);

                            fileStream.Dispose();
                        }
                    }
                    catch
                    {
                        // If anything goes wrong, use SFTP normally without relying on the stash.
                        stashFilePath = "";
                    }
                }

                using (var memoryStream = new MemoryStream())
                {
                    var res = (SftpDownloadAsyncResult) Client.BeginDownloadFile(fullFilePath, memoryStream);

                    while (!res.AsyncWaitHandle.WaitOne(100))
                    {
                        if (!DoCancel)
                        {
                            continue;
                        }

                        res.IsDownloadCanceled = true;

                        Client.DisconnectIfNeeded(hadToConnect);
                        return;
                    }

                    Client.EndDownloadFile(res);

                    ByteArray = memoryStream.ToArray();

                    if (DoUseStash && !stashFilePath.IsNullOrEmpty())
                    {
                        File.WriteAllBytes(stashFilePath, ProtectedData.Protect(ByteArray, null, DataProtectionScope.CurrentUser));
                        File.SetLastWriteTime(stashFilePath, attribute.LastWriteTime); // Must be last operation
                    }
                }

                Client.DisconnectIfNeeded(hadToConnect);
                await Task.Run(() => { }); // Gets rid of a compiler warning and does nothing.
            }

            public static void CleanupStashes(bool canCleanCurrentInstance)
            {
                try
                {
                    if (!Directory.Exists(StashRootFolderPath))
                    {
                        return;
                    }

                    var arrayStashInstanceFolderPaths = Directory.GetDirectories(StashRootFolderPath, "*", SearchOption.TopDirectoryOnly);
                    var listCleanupStashInstanceFolderPaths = new List<string>();
                    if (canCleanCurrentInstance)
                    {
                        listCleanupStashInstanceFolderPaths.Add(StashInstanceFolderPath); // Always cleanup current instance stash first.
                    }

                    foreach (var stashInstanceFolderPath in arrayStashInstanceFolderPaths)
                    {
                        var stashInstanceFolderPathInfo = new DirectoryInfo(stashInstanceFolderPath);
                        if (!int.TryParse(stashInstanceFolderPathInfo.Name, out var processId))
                        {
                            continue;
                        }

                        var isRunning = false;
                        try
                        {
                            var process = Process.GetProcessById(processId); // This line throws an exception if there is no such process running.
                            if (process.ProcessName.StartsWith("OpenDental"))
                            {
                                // The process for this stash instance folder is still running.
                                isRunning = true;
                            }
                        }
                        catch
                        {
                            // ignored
                        }

                        if (!isRunning)
                        {
                            listCleanupStashInstanceFolderPaths.Add(stashInstanceFolderPath);
                        }
                    }

                    foreach (var stashInstanceFolderPath in listCleanupStashInstanceFolderPaths)
                    {
                        try
                        {
                            //Could fail for two main reasons: 1) File(s) still in use. 2) Stash instance folder was created by another user.
                            Directory.Delete(stashInstanceFolderPath, true); //Recursive. Deletes subfolders.
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                catch
                {
                    // Not an issue because we will try again when closing or opening the application next.
                }
            }
        }

        public class Delete : TaskStateDelete
        {
            internal SftpClient Client;

            public Delete(string host, string user, string pass)
            {
                Client = Init(host, user, pass);
            }

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

        public class Thumbnail : TaskStateThumbnail
        {
            private readonly SftpClient _client;

            public Thumbnail(string host, string user, string pass)
            {
                _client = Init(host, user, pass);
            }

            protected override async Task PerformIO()
            {
                TaskStateDownload stateDownload = new Download
                {
                    Client = _client,
                    Folder = Folder,
                    FileName = FileName
                };

                stateDownload.Execute(); // We could get cute later and make this match isAsync to update the main thread if they are downloading a large file.

                ByteArray = stateDownload.ByteArray;

                await Task.Run(() => { }); // This gets rid of a compiler warning.
            }
        }

        public class ListFolders : TaskStateListFolders
        {
            internal readonly SftpClient Client;

            public ListFolders(string host, string user, string pass)
            {
                Client = Init(host, user, pass);
            }

            protected override async Task PerformIO()
            {
                var hadToConnect = Client.ConnectIfNeeded();

                Client.CreateDirectoriesIfNeeded(FolderPath);

                var listFiles = (await Task.Factory.FromAsync(Client.BeginListDirectory(FolderPath, null, null), Client.EndListDirectory)).ToList();
                listFiles = listFiles.FindAll(x => !x.FullName.EndsWith(".")); // Sftp has 2 "extra" files that are "." and "..".  I think it's for explorer navigation.
                ListFolderPathsDisplay = listFiles.Select(x => x.FullName).ToList();

                Client.DisconnectIfNeeded(hadToConnect);
            }
        }

        public class Move : TaskStateMove
        {
            private readonly SftpClient _client;
            internal bool IsCopy;
            private readonly string _host;
            private readonly string _user;
            private readonly string _pass;

            public Move(string host, string user, string pass)
            {
                _client = Init(host, user, pass);
                _host = host;
                _user = user;
                _pass = pass;
            }

            protected override async Task PerformIO()
            {
                var hadToConnect = _client.ConnectIfNeeded();

                List<string> listFilePaths;

                var isDirectory = _client.GetAttributes(FromPath).IsDirectory;
                if (isDirectory)
                {
                    if (DoCancel)
                    {
                        return;
                    }

                    // Only include files (not sub-directories) in the list
                    listFilePaths = (await Task.Factory.FromAsync(_client.BeginListDirectory(FromPath, null, null), _client.EndListDirectory))
                        .Where(x => !x.IsDirectory)
                        .Select(x => x.FullName).ToList();
                }
                else
                {
                    // Copying a file
                    listFilePaths = new List<string> {FromPath};
                }

                CountTotal = listFilePaths.Count;
                for (var i = 0; i < CountTotal; i++)
                {
                    if (DoCancel)
                    {
                        _client.DisconnectIfNeeded(hadToConnect);
                        return;
                    }

                    var path = listFilePaths[i];
                    try
                    {
                        var fileName = Path.GetFileName(path);

                        var toPathFull = ToPath;
                        if (isDirectory)
                        {
                            toPathFull = ODFileUtils.CombinePaths(ToPath, fileName, '/');
                        }

                        if (FileExists(_client, toPathFull))
                        {
                            throw new Exception(); // Throw so that we can iterate CountFailed
                        }

                        var fromPathFull = FromPath;
                        if (isDirectory)
                        {
                            fromPathFull = ODFileUtils.CombinePaths(FromPath, fileName, '/');
                        }

                        if (IsCopy)
                        {
                            // Throws if fails.
                            await Task.Run(() =>
                            {
                                var folder = Path.GetDirectoryName(fromPathFull) ?? string.Empty;

                                TaskStateDownload stateDown = new Download(_host, _user, _pass)
                                {
                                    Folder = folder.Replace('\\', '/'),
                                    FileName = Path.GetFileName(fromPathFull),
                                    HasExceptions = HasExceptions
                                };

                                stateDown.Execute();
                                while (!stateDown.IsDone)
                                {
                                    stateDown.DoCancel = DoCancel;
                                }

                                if (DoCancel)
                                {
                                    _client.DisconnectIfNeeded(hadToConnect);
                                    return;
                                }

                                folder = Path.GetDirectoryName(toPathFull) ?? string.Empty;

                                TaskStateUpload stateUp = new Upload(_host, _user, _pass)
                                {
                                    Folder = folder.Replace('\\', '/'),
                                    FileName = Path.GetFileName(toPathFull),
                                    ByteArray = stateDown.ByteArray,
                                    HasExceptions = HasExceptions
                                };

                                stateUp.Execute();

                                while (!stateUp.IsDone)
                                {
                                    stateUp.DoCancel = DoCancel;
                                }

                                if (!DoCancel)
                                {
                                    return;
                                }

                                _client.DisconnectIfNeeded(hadToConnect);
                            });
                        }
                        else
                        {
                            // Moving
                            await Task.Run(() =>
                            {
                                var file = _client.Get(path);

                                _client.CreateDirectoriesIfNeeded(ToPath);

                                file.MoveTo(toPathFull);
                            });
                        }

                        CountSuccess++;
                    }
                    catch
                    {
                        CountFailed++;
                    }
                }

                _client.DisconnectIfNeeded(hadToConnect);
            }
        }

        public class Copy : TaskStateCopy
        {
            private readonly TaskStateMove _stateMove;

            public Copy(string host, string user, string pass)
            {
                _stateMove = new Move(host, user, pass)
                {
                    IsCopy = true
                };
            }

            protected override async Task PerformIO()
            {
                _stateMove.FromPath = FromPath;
                _stateMove.ToPath = ToPath;
                _stateMove.Execute(isAsync: true);

                while (!_stateMove.IsDone)
                {
                    Thread.Sleep(10);

                    if (DoCancel)
                    {
                        _stateMove.DoCancel = true;
                    }
                }

                await Task.Run(() => { });
            }
        }

        /// <summary>
        /// Synchronous.
        /// Returns true if a file exists in the passed in filePath
        /// </summary>
        public static bool FileExists(string host, string user, string pass, string filePath)
        {
            var client = Init(host, user, pass);

            return FileExists(client, filePath);
        }

        /// <summary>
        /// Synchronous.
        /// Returns true if a file exists in the passed in filePath
        /// </summary>
        private static bool FileExists(SftpClient client, string filePath)
        {
            try
            {
                var hadToConnect = client.ConnectIfNeeded();

                client.Get(filePath);
                client.DisconnectIfNeeded(hadToConnect);

                return true;
            }
            catch
            {
                // ignored
            }

            return false;
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
        /// <summary>
        /// Loops through the entire path and sees if any directory along the way doesn't exist.
        /// If any don't exist, they get created.
        /// </summary>
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

        /// <summary>
        /// Returns true if it was not connected and had to connect.
        /// </summary>
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

        /// <summary>
        /// Will disconnect the client if the client was the one that determined it needed to be connected.
        /// </summary>
        public static void DisconnectIfNeeded(this SftpClient client, bool hadToConnect)
        {
            if (hadToConnect && client.IsConnected)
            {
                client.Disconnect();
            }
        }
    }
}