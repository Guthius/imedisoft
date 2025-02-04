using System;
using System.IO;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;

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

        var path = PrefC.GetString(PrefName.DocPath);
        if (!string.IsNullOrEmpty(path))
        {
            return path;
        }

        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Imedisoft", "Data");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }

    public static string GetValidPathFromString(string documentPaths)
    {
        return documentPaths.Split(';')
            .Select(path => new {path, tryPath = Path.Combine(path, "A")})
            .Where(t => Directory.Exists(t.tryPath))
            .Select(t => t.path)
            .FirstOrDefault();
    }

    public static void CreateDirectoryRelative(string folder)
    {
        Directory.CreateDirectory(Path.Combine(GetPreferredAtoZpath(), folder));
    }

    public static void WriteAllTextRelative(string folder, string fileName, string contents)
    {
        File.WriteAllText(Path.Combine(GetPreferredAtoZpath(), folder, fileName), contents);
    }

    public static bool DirectoryExistsRelative(string folder)
    {
        return Directory.Exists(Path.Combine(GetPreferredAtoZpath(), folder));
    }
}