using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace CodeBase;

public class ODFileUtils
{
    private static readonly Random Rand = new();

    public static string RemoveTrailingSeparators(string path)
    {
        while (path is {Length: > 0} && (path[path.Length - 1] == '\\' || path[path.Length - 1] == '/'))
        {
            path = path.Substring(0, path.Length - 1);
        }

        return path;
    }

    public static string CombinePaths(string path1, string path2)
    {
        return Path.Combine(path1, path2);
    }

    public static string CreateRandomFile(string dir, string ext, string prefix = "")
    {
        if (ext.Length > 0 && ext[0] != '.')
        {
            ext = '.' + ext;
        }

        var fileCreated = false;
        var filePath = "";
        const string randChrs = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        do
        {
            var fileName = prefix;
            for (var i = 0; i < 6; i++)
            {
                fileName += randChrs[Rand.Next(0, randChrs.Length - 1)];
            }

            fileName += DateTime.Now.ToString("yyyyMMddhhmmss");
            filePath = CombinePaths(dir, fileName + ext);
            try
            {
                var fs = File.Create(filePath);
                fs.Dispose();
                fileCreated = true;
            }
            catch
            {
                // ignored
            }
        } while (!fileCreated);

        return filePath;
    }

    public static string CreateRandomFolder(string dir)
    {
        var isFolderCreated = false;
        var folderPath = "";
        const string randChrs = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        do
        {
            var subDirName = "";
            for (var i = 0; i < 6; i++)
            {
                subDirName += randChrs[Rand.Next(0, randChrs.Length - 1)];
            }

            subDirName += DateTime.Now.ToString("yyyyMMddhhmmss");
            folderPath = CombinePaths(dir, subDirName);
            if (Directory.Exists(folderPath)) continue;
            Directory.CreateDirectory(folderPath);
            isFolderCreated = true;
        } while (!isFolderCreated);

        return folderPath;
    }

    public static string AppendSuffix(string filePath, string suffix)
    {
        var ext = Path.GetExtension(filePath);
        return CombinePaths(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + suffix + ext);
    }

    public static string CleanFileName(string fileName)
    {
        return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
    }

    public static Process ProcessStart(string path, string commandLineArgs = "", string createDirIfNeeded = "")
    {
        if (!string.IsNullOrEmpty(createDirIfNeeded) && !Directory.Exists(createDirIfNeeded))
        {
            Directory.CreateDirectory(createDirIfNeeded);
        }

        return Process.Start(path, commandLineArgs);
    }

    public static void WriteAllText(string filePath, string text, bool doOverwriteFile = true)
    {
        if (!File.Exists(filePath) || doOverwriteFile)
        {
            File.WriteAllText(filePath, text);
        }
    }

    public static void WriteAllTextThenStart(string filePath, string fileText, string processPath)
    {
        WriteAllTextThenStart(filePath, fileText, processPath, "");
    }

    public static void WriteAllTextThenStart(string filePath, string fileText, string processPath, string commandLineArgs)
    {
        File.WriteAllText(filePath, fileText);
        Process.Start(processPath, commandLineArgs);
    }

    public static void WriteAllTextThenStart(string filePath, string fileText, Encoding encoding, string processPath, string commandLineArgs)
    {
        File.WriteAllText(filePath, fileText, encoding);
        Process.Start(processPath, commandLineArgs);
    }

    public static void WriteAllBytesThenStart(string filePath, byte[] fileBytes, string processPath, string commandLineArgs = "", int millisecondsToSleep = 0)
    {
        File.WriteAllBytes(filePath, fileBytes);
        if (millisecondsToSleep > 0)
        {
            Thread.Sleep(millisecondsToSleep);
        }

        if (!string.IsNullOrEmpty(processPath))
        {
            Process.Start(processPath, commandLineArgs);
            return;
        }

        Process.Start(filePath, commandLineArgs);
    }

    public static List<string> GetFilePathsFromText(string text)
    {
        var listStringMatches = Regex.Matches(text,
                @"(\\\\\w(([\w. \\-]*?)(?=(\\)[\s,.;]|(\\)\z)|[\w. \\-]*?(\.[a-zA-Z]{1,4}(?=[\s,.;]|\z))))|([a-zA-Z]{1}\:(\\|\/)((\s|\z)|\w[\w-. \\\/]*((?=(\\|\/)[\s,.;]|(\\|\/)\z)|(\.[a-zA-Z]{1,4}(?=[\s,.;]|\z)))))")
            .OfType<Match>().Select(m => m.Groups[0].Value).Distinct().ToList();
        var folderPathsOnly = new List<string>(listStringMatches.Count);
        foreach (var match in listStringMatches)
        {
            var folderPath = match;
            try
            {
                //In regex we pick up extra white space but we don't want to open the file explorer with a file path with white space attached 
                folderPath = Regex.Replace(folderPath, @"[\s]+$", "");
                //If string has extension, assuming specific file
                if (Path.GetExtension(folderPath) != "")
                {
                    //If text is a specific file, truncate to the parent directory
                    folderPath = new FileInfo(folderPath).Directory.FullName;
                }
            }
            catch
            {
                //We don't want this method to throw any errors. If the path doesn't exist we want to preserve what was found and throw when the 
                //user clicks to navigate to the selected path. See OpenFileExplorer().
            }

            folderPathsOnly.Add(folderPath);
        }

        return folderPathsOnly;
    }
}