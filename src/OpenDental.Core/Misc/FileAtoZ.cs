using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeBase;
using OpenDentBusiness;

namespace OpenDentBusiness.FileIO
{
    public class FileAtoZ
    {
        public static string LocalAtoZpath = null;

        public static string GetPreferredAtoZpath()
        {
            if (LocalAtoZpath == null)
            {
                //on startup
                try
                {
                    LocalAtoZpath = ComputerPrefs.LocalComputer.AtoZpath;
                }
                catch
                {
                    //fails when loading plugins after switching to version 15.1 because of schema change.
                    LocalAtoZpath = "";
                }
            }

            //Override path.  Because it overrides all other paths, we evaluate it first.
            if (!string.IsNullOrEmpty(LocalAtoZpath))
            {
                return LocalAtoZpath.Trim();
            }

            string replicationAtoZ = ReplicationServers.GetAtoZpath();
            if (!string.IsNullOrEmpty(replicationAtoZ))
            {
                return GetValidPathFromString(replicationAtoZ)?.Trim();
            }

            //use this to handle possible multiple paths separated by semicolons.
            return GetValidPathFromString(PrefC.GetString(PrefName.DocPath))?.Trim();
            //If you got here you are using a cloud storage method.
        }

        public static string GetValidPathFromString(string documentPaths)
        {
            foreach (string path in documentPaths.Split(new char[] {';'}))
            {
                string tryPath = ODFileUtils.CombinePaths(path, "A");
                if (Directory.Exists(tryPath))
                {
                    return path;
                }
            }

            return null;
        }

        public static void CreateDirectoryRelative(string folder)
        {
            Directory.CreateDirectory(CombinePaths(GetPreferredAtoZpath(), folder));
        }

        public static string ReadAllText(string fileFullPath)
        {
            return File.ReadAllText(fileFullPath);
        }

        public static byte[] ReadAllBytes(string fileFullPath)
        {
            return File.ReadAllBytes(fileFullPath);
        }

        public static void WriteAllText(string fileFullPath, string contents)
        {
            File.WriteAllText(fileFullPath, contents);
        }

        public static void WriteAllTextRelative(string folder, string fileName, string contents)
        {
            WriteAllText(CombinePaths(GetPreferredAtoZpath(), folder, fileName), contents);
        }

        public static void WriteAllBytes(string fileFullPath, byte[] byteArray)
        {
            File.WriteAllBytes(fileFullPath, byteArray);
        }

        public static List<string> GetFilesInDirectory(string folderFullPath)
        {
            return Directory.GetFiles(folderFullPath).ToList();
        }

        public static List<string> GetFilesInDirectoryRelative(string folder)
        {
            return GetFilesInDirectory(CombinePaths(GetPreferredAtoZpath(), folder));
        }

        public static string CombinePaths(params string[] paths)
        {
            return ODFileUtils.CombinePaths(paths);
        }

        public static string AppendSuffix(string fileFullPath, string suffix)
        {
            return ODFileUtils.AppendSuffix(fileFullPath, suffix);
        }

        public static bool Exists(string fileFullPath)
        {
            return File.Exists(fileFullPath);
        }

        public static bool ExistsRelative(string folder, string fileName)
        {
            return Exists(CombinePaths(GetPreferredAtoZpath(), folder, fileName));
        }

        public static void Copy(string sourceFileName, string destinationFileName, FileAtoZSourceDestination sourceDestination, bool isFolder = false, bool doOverwrite = false)
        {
            File.Copy(sourceFileName, destinationFileName, doOverwrite);
        }

        public static void Delete(string fileFullPath)
        {
            File.Delete(fileFullPath);
        }

        public static bool DirectoryExists(string folderFullPath)
        {
            return Directory.Exists(folderFullPath);
        }

        public static bool DirectoryExistsRelative(string folder)
        {
            return DirectoryExists(CombinePaths(GetPreferredAtoZpath(), folder));
        }

        public static Bitmap GetImage(string imagePath)
        {
            return new Bitmap(imagePath);
        }
    }
}

namespace OpenDentBusiness
{
    ///<summary>Used to specify where the files are coming from and going when copying.</summary>
    public enum FileAtoZSourceDestination
    {
        ///<summary>Copying a local file to AtoZ folder. Equivalent to 'upload.'</summary>
        LocalToAtoZ,

        ///<summary>Copying an AtoZ file to a local file. Equivalent to 'download'.</summary>
        AtoZToLocal,

        ///<summary>Copying an AtoZ file to another AtoZ file. Equivalent to 'download' then 'upload'.</summary>
        AtoZToAtoZ
    }
}