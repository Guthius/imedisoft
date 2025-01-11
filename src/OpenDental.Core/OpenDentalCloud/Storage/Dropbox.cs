using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeBase;
using Dropbox.Api;
using Dropbox.Api.Files;
using OpenDental.Cloud.Shared;

namespace OpenDental.Cloud.Storage
{
    public class Dropbox
    {
        /// <summary>
        /// Called by OAuth web app to display this URL in their browser.
        /// </summary>
        public static string GetDropboxAuthorizationUrl(string appkey)
        {
            try
            {
                var ret = DropboxOAuth2Helper.GetAuthorizeUri(appkey).ToString();
                if (string.IsNullOrEmpty(ret))
                {
                    throw new Exception("Invalid URL returned by Dropbox");
                }

                return ret;
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message, e);
            }
        }

        /// <summary>
        /// Called by Open Dental Proper to get the real access code form the code given by Dropbox.
        /// Returns empty string if something went wrong.
        /// </summary>
        public static string GetDropboxAccessToken(string code, string appkey, string appsecret)
        {
            var ret = "";
            ApplicationException ae = null;
            var wait = new ManualResetEvent(false);

            new Task(async () =>
            {
                try
                {
                    var resp = await DropboxOAuth2Helper.ProcessCodeFlowAsync(code, appkey, appsecret);
                    if (string.IsNullOrEmpty(resp.AccessToken))
                    {
                        throw new Exception("Empty token returned by Dropbox.");
                    }

                    ret = resp.AccessToken;
                }
                catch (Exception ex)
                {
                    ae = new ApplicationException(ex.Message, ex);
                }

                wait.Set();
            }).Start();

            wait.WaitOne(10000);

            if (ae != null)
            {
                throw ae;
            }

            return ret;
        }

        public class Upload : TaskStateUpload
        {
            private readonly DropboxClient _client;

            /// <summary>
            /// Upload only supports WriteMode.Overwrite as of right now.
            /// </summary>
            public Upload(string accessToken)
            {
                _client = new DropboxClient(accessToken);
            }

            protected override async Task PerformIO()
            {
                var numOfChunks = Math.Max(ByteArray.Length / MaxFileSizeBytes + 1, 2); // Add 1 so that we are under the max file size limit, since an integer will truncate remainders.
                var chunkSize = ByteArray.Length / numOfChunks;
                string sessionId = null;
                var index = 0;
                for (var i = 1; i <= numOfChunks; i++)
                {
                    if (DoCancel)
                    {
                        throw new Exception("Operation cancelled by user");
                    }

                    var lastChunk = i == numOfChunks;
                    var curChunkSize = chunkSize;
                    if (lastChunk)
                    {
                        curChunkSize = ByteArray.Length - index;
                    }

                    using var memoryStream = new MemoryStream(ByteArray, index, curChunkSize);

                    if (i == 1)
                    {
                        var result = await _client.Files.UploadSessionStartAsync(false, memoryStream);
                        sessionId = result.SessionId;
                    }
                    else
                    {
                        var cursor = new UploadSessionCursor(sessionId, (ulong) index);
                        if (lastChunk)
                        {
                            //Always forcing Dropbox to overwrite any conflicting files.  
                            //Otherwise a Dropbox.Api.Files.UploadSessionFinishError.Path error will be returned by Dropbox if there is a "path/conflict/file/..."
                            await _client.Files.UploadSessionFinishAsync(cursor
                                , new CommitInfo(ODFileUtils.CombinePaths(Folder, FileName, '/'), WriteMode.Overwrite.Instance)
                                , memoryStream);
                        }
                        else
                        {
                            await _client.Files.UploadSessionAppendV2Async(cursor, false, memoryStream);
                        }
                    }

                    index += curChunkSize;
                }
            }
        }

        public class Download : TaskStateDownload
        {
            private readonly DropboxClient _client;

            public Download(string accessToken)
            {
                _client = new DropboxClient(accessToken);
            }

            protected override async Task PerformIO()
            {
                if (!FileExists(_client, ODFileUtils.CombinePaths(Folder, FileName, '/')))
                {
                    throw new Exception("File not found.");
                }

                var response = await _client.Files.DownloadAsync(ODFileUtils.CombinePaths(Folder, FileName, '/'));
                DownloadFileSize = response.Response.Size;
                var numChunks = DownloadFileSize / MaxFileSizeBytes + 1;
                var chunkSize = (int) DownloadFileSize / (int) numChunks;
                var buffer = new byte[chunkSize];
                var finalBuffer = new byte[DownloadFileSize];
                var index = 0;

                using (var stream = await response.GetContentAsStreamAsync())
                {
                    int length;
                    do
                    {
                        if (DoCancel)
                        {
                            throw new Exception("Operation cancelled by user");
                        }

                        length = await stream.ReadAsync(buffer, 0, chunkSize);

                        // Convert each chunk to a MemoryStream. This plays nicely with garbage collection.
                        using var memoryStream = new MemoryStream();

                        memoryStream.Write(buffer, 0, length);
                        Array.Copy(memoryStream.ToArray(), 0, finalBuffer, index, length);
                        index += length;

                        // Null is ok
                    } while (length > 0);
                }

                ByteArray = finalBuffer;
            }
        }

        public class Delete : TaskStateDelete
        {
            private readonly DropboxClient _client;

            public Delete(string accessToken)
            {
                _client = new DropboxClient(accessToken);
            }

            protected override async Task PerformIO()
            {
                // If path is a folder, all contents will be deleted.
                await _client.Files.DeleteAsync(Path);
            }
        }

        public class Thumbnail : TaskStateThumbnail
        {
            private readonly DropboxClient _client;

            public Thumbnail(string accessToken)
            {
                _client = new DropboxClient(accessToken);
            }

            protected override async Task PerformIO()
            {
                var data = await _client.Files.GetThumbnailAsync(ODFileUtils.CombinePaths(Folder, FileName, '/'), size: ThumbnailSize.W128h128.Instance);
                ByteArray = await data.GetContentAsByteArrayAsync();
            }
        }

        public class ListFolders : TaskStateListFolders
        {
            // client needs to be internal so that it can be set from TaskStateMove when getting a list of files to move
            internal readonly DropboxClient Client;

            public ListFolders(string accessToken)
            {
                Client = new DropboxClient(accessToken);
            }

            protected override async Task PerformIO()
            {
                var data = await Client.Files.ListFolderAsync(FolderPath);
                ListFolderPathsDisplay = data.Entries.Select(x => x.PathDisplay).ToList();
            }
        }

        public class Move : TaskStateMove
        {
            internal DropboxClient Client;
            internal bool IsCopy;

            public Move(string accessToken)
            {
                Client = new DropboxClient(accessToken);
            }

            internal Move()
            {
            }

            protected override async Task PerformIO()
            {
                List<string> listFilePaths;

                var isFile = (await Client.Files.GetMetadataAsync(FromPath)).IsFile;
                if (isFile)
                {
                    listFilePaths = new List<string> {FromPath};
                }
                else
                {
                    // Copying a directory
                    if (DoCancel)
                    {
                        return;
                    }

                    //Only include files (not sub-directories) in the list
                    listFilePaths = (await Client.Files.ListFolderAsync(FromPath)).Entries
                        .Where(x => x.IsFile)
                        .Select(x => x.Name).ToList();
                }

                CountTotal = listFilePaths.Count;
                for (var i = 0; i < CountTotal; i++)
                {
                    if (DoCancel)
                    {
                        return;
                    }

                    var path = listFilePaths[i];
                    try
                    {
                        var fileName = Path.GetFileName(path);
                        var toPathFull = ToPath;
                        if (!isFile)
                        {
                            toPathFull = ODFileUtils.CombinePaths(ToPath, fileName, '/');
                        }

                        if (FileExists(Client, toPathFull))
                        {
                            throw new Exception(); // Throw so that we can iterate CountFailed
                        }

                        var fromPathFull = FromPath;
                        if (!isFile)
                        {
                            fromPathFull = ODFileUtils.CombinePaths(FromPath, fileName, '/');
                        }

                        // Throws if fails.
                        if (IsCopy)
                        {
                            await Client.Files.CopyAsync(fromPathFull, toPathFull);
                        }
                        else
                        {
                            await Client.Files.MoveAsync(ODFileUtils.CombinePaths(FromPath, fromPathFull, '/'), toPathFull);
                        }

                        CountSuccess++;
                    }
                    catch (Exception)
                    {
                        CountFailed++;
                    }
                }
            }
        }

        public class Copy : TaskStateCopy
        {
            protected readonly DropboxClient Client;

            public Copy(string accessToken)
            {
                Client = new DropboxClient(accessToken);
            }

            protected override async Task PerformIO()
            {
                TaskStateMove stateMove = new Move {Client = Client, IsCopy = true};

                stateMove.FromPath = FromPath;
                stateMove.ToPath = ToPath;
                stateMove.Execute(isAsync: true);

                while (!stateMove.IsDone)
                {
                    Thread.Sleep(10);
                    if (DoCancel)
                    {
                        stateMove.DoCancel = true;
                    }
                }

                await Task.Run(() => { });
            }
        }

        /// <summary>
        /// Synchronous.
        /// Returns true if a file exists in the passed in filePath
        /// </summary>
        public static bool FileExists(string accessToken, string filePath)
        {
            return FileExists(new DropboxClient(accessToken), filePath);
        }

        /// <summary>
        /// Synchronous.
        /// Returns true if a file exists in the passed in filePath
        /// </summary>
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
}