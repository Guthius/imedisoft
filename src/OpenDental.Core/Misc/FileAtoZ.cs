using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Caching;

namespace OpenDentBusiness.FileIO;

public class FileAtoZ
{
    public static string LocalAtoZpath;

    public static string GetPreferredAtoZpath()
    {
        if (LocalAtoZpath == null)
        {
            try
            {
                LocalAtoZpath = ComputerPrefs.LocalComputer.AtoZpath;
            }
            catch
            {
                LocalAtoZpath = "";
            }
        }
            
        if (!string.IsNullOrEmpty(LocalAtoZpath))
        {
            return LocalAtoZpath.Trim();
        }

        var replicationAtoZ = ReplicationServers.GetAtoZpath();
        return !string.IsNullOrEmpty(replicationAtoZ) ? GetValidPathFromString(replicationAtoZ)?.Trim() : GetValidPathFromString(PrefC.GetString(PrefName.DocPath))?.Trim();
    }

    public static string GetValidPathFromString(string documentPaths)
    {
        return documentPaths.Split(';')
            .Select(path => new {path, tryPath = ODFileUtils.CombinePaths(path, "A")})
            .Where(t => Directory.Exists(t.tryPath))
            .Select(t => t.path)
            .FirstOrDefault();
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