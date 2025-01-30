using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CodeBase;

public class ODFileUtils
{
    private static readonly Random Rand = new();

    private static List<string> _listKnownFileTypes;

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
        return CombinePaths([path1, path2]);
    }

    public static string CombinePaths(string path1, string path2, char separator)
    {
        return CombinePaths([path1, path2], separator);
    }

    public static string CombinePaths(string path1, string path2, string path3)
    {
        return CombinePaths([path1, path2, path3]);
    }

    public static string CombinePaths(string path1, string path2, string path3, string path4)
    {
        return CombinePaths([path1, path2, path3, path4]);
    }

    public static string CombinePaths(string[] paths)
    {
        var finalPath = "";
        
        for (var i = 0; i < paths.Length; i++)
        {
            var path = RemoveTrailingSeparators(paths[i]);
            
            if (i < paths.Length - 1)
            {
                if (path is {Length: > 0})
                {
                    path += Path.DirectorySeparatorChar;
                }
            }

            finalPath += path;
        }

        return finalPath;
    }

    public static string CombinePaths(string[] paths, char separator)
    {
        return CombinePaths(paths).Replace(Path.DirectorySeparatorChar, separator);
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

    public static void ProcessStart(Process process)
    {
        process.Start();
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

    public static void WriteAllTextThenStart(string filePath, string fileText, string processPath, bool doStartWithoutExtraFile = false)
    {
        WriteAllTextThenStart(filePath, fileText, processPath, "");
    }

    public static Process WriteAllTextThenStart(string filePath, string fileText, string processPath, string commandLineArgs)
    {
        File.WriteAllText(filePath, fileText);
        return Process.Start(processPath, commandLineArgs);
    }

    public static void WriteAllTextThenStart(string filePath, string fileText, Encoding encoding, string processPath, string commandLineArgs)
    {
        File.WriteAllText(filePath, fileText, encoding);
        Process.Start(processPath, commandLineArgs);
    }

    public static bool IsFileInUse(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        try
        {
            var fileInfo = new FileInfo(filePath);
            
            using var fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            
            fileStream.Close();
        }
        catch
        {
            return true;
        }

        return false;
    }

    public static List<string> GetListKnownFileTypes()
    {
        if (_listKnownFileTypes is not null)
        {
            return _listKnownFileTypes;
        }
        
        try
        {
            FillListKnownFileTypes();
        }
        catch
        {
            // ignored
        }

        return _listKnownFileTypes;
    }

    private static void FillListKnownFileTypes()
    {
        _listKnownFileTypes = [..FileTypes]; //Make a shallow copy of the default list.
        //This can fail for a variety of reasons, namely if the user doesn't have access to the registry or if the computer is not a windows computer.
        //This gets all the extension filetypes inside of the registry.
        var listRegistryFileTypes = Registry.ClassesRoot.GetSubKeyNames().ToList();
        for (var i = 0; i < listRegistryFileTypes.Count; i++)
        {
            //There are files in this section of the registry that don't start with . We trim those out since we're only looking for file extensions.
            if (!listRegistryFileTypes[i].StartsWith("."))
            {
                continue;
            }

            if (_listKnownFileTypes.Contains(listRegistryFileTypes[i].ToLower()))
            {
                continue;
            }

            //Open up the registry entry. If the content type is not present, it's probably not a file so we remove it.
            if (Registry.ClassesRoot.OpenSubKey(listRegistryFileTypes[i]).GetValue("Content Type") == null)
            {
                continue;
            }

            _listKnownFileTypes.Add(listRegistryFileTypes[i].ToLower());
        }

        _listKnownFileTypes = _listKnownFileTypes.OrderBy(x => x).ToList(); //Organize the list.
    }

    public static bool IsKnownFileType(string path)
    {
        _listKnownFileTypes ??= GetListKnownFileTypes();

        try
        {
            return _listKnownFileTypes.Contains(Path.GetExtension(path.ToLower()));
        }
        catch
        {
            return false; //There is an issue with the path, so it's not a known filetype.
        }
    }

    public static void WriteAllBytesThenStart(string filePath, byte[] fileBytes, string processPath)
    {
        WriteAllBytesThenStart(filePath, fileBytes, processPath, "");
    }

    public static void WriteAllBytesThenStart(string filePath, byte[] fileBytes, string processPath, string commandLineArgs, int millisecondsToSleep = 0)
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

    public static string GetProgramDirectory()
    {
        var endPos = Application.ExecutablePath.LastIndexOf(Path.DirectorySeparatorChar);
        return Application.ExecutablePath.Substring(0, endPos + 1);
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

    private static readonly List<string> FileTypes = new()
    {
        #region Known file endings

        ".323",
        ".3g2",
        ".3gp",
        ".3gp2",
        ".3gpp",
        ".7z",
        ".aa",
        ".aac",
        ".aaf",
        ".aax",
        ".ac3",
        ".aca",
        ".accda",
        ".accdb",
        ".accdc",
        ".accde",
        ".accdr",
        ".accdt",
        ".accdw",
        ".accft",
        ".acx",
        ".addin",
        ".ade",
        ".adobebridge",
        ".adp",
        ".adt",
        ".adts",
        ".afm",
        ".ai",
        ".aif",
        ".aifc",
        ".aiff",
        ".air",
        ".amc",
        ".application",
        ".art",
        ".asa",
        ".asax",
        ".ascx",
        ".asd",
        ".asf",
        ".ashx",
        ".asi",
        ".asm",
        ".asmx",
        ".aspx",
        ".asr",
        ".asx",
        ".atom",
        ".au",
        ".avi",
        ".axs",
        ".bas",
        ".bat",
        ".bcpio",
        ".bin",
        ".bmp",
        ".c",
        ".cab",
        ".caf",
        ".calx",
        ".cat",
        ".cc",
        ".cd",
        ".cdda",
        ".cdf",
        ".cer",
        ".chm",
        ".class",
        ".clp",
        ".cmx",
        ".cnf",
        ".cod",
        ".config",
        ".contact",
        ".coverage",
        ".cpio",
        ".cpp",
        ".crd",
        ".crl",
        ".crt",
        ".cs",
        ".csdproj",
        ".csh",
        ".csproj",
        ".css",
        ".csv",
        ".cur",
        ".cxx",
        ".dat",
        ".datasource",
        ".dbproj",
        ".dcm",
        ".dcr",
        ".def",
        ".deploy",
        ".der",
        ".dgml",
        ".dib",
        ".dif",
        ".dir",
        ".disco",
        ".dll",
        ".dll.config",
        ".dlm",
        ".doc",
        ".docm",
        ".docx",
        ".dot",
        ".dotm",
        ".dotx",
        ".dsp",
        ".dsw",
        ".dtd",
        ".dtsconfig",
        ".dv",
        ".dvi",
        ".dwf",
        ".dwp",
        ".dxr",
        ".eml",
        ".emz",
        ".eot",
        ".eps",
        ".etl",
        ".etx",
        ".evy",
        ".exe",
        ".exe.config",
        ".fdf",
        ".fif",
        ".filters",
        ".fla",
        ".flr",
        ".flv",
        ".fsscript",
        ".fsx",
        ".generictest",
        ".gif",
        ".group",
        ".gsm",
        ".gtar",
        ".gz",
        ".h",
        ".hdf",
        ".hdml",
        ".hhc",
        ".hhk",
        ".hhp",
        ".hlp",
        ".hpp",
        ".hqx",
        ".hta",
        ".htc",
        ".htm",
        ".html",
        ".htt",
        ".hxa",
        ".hxc",
        ".hxd",
        ".hxe",
        ".hxf",
        ".hxh",
        ".hxi",
        ".hxk",
        ".hxq",
        ".hxr",
        ".hxs",
        ".hxt",
        ".hxv",
        ".hxw",
        ".hxx",
        ".i",
        ".ico",
        ".ics",
        ".idl",
        ".ief",
        ".iii",
        ".inc",
        ".inf",
        ".inl",
        ".ins",
        ".ipa",
        ".ipg",
        ".ipproj",
        ".ipsw",
        ".iqy",
        ".isp",
        ".ite",
        ".itlp",
        ".itms",
        ".itpc",
        ".ivf",
        ".jar",
        ".java",
        ".jck",
        ".jcz",
        ".jfif",
        ".jnlp",
        ".jpb",
        ".jpe",
        ".jpeg",
        ".jpg",
        ".js",
        ".jsx",
        ".jsxbin",
        ".latex",
        ".library-ms",
        ".lit",
        ".loadtest",
        ".lpk",
        ".lsf",
        ".lst",
        ".lsx",
        ".lzh",
        ".m13",
        ".m14",
        ".m1v",
        ".m2t",
        ".m2ts",
        ".m2v",
        ".m3u",
        ".m3u8",
        ".m4a",
        ".m4b",
        ".m4p",
        ".m4r",
        ".m4v",
        ".mac",
        ".mak",
        ".man",
        ".manifest",
        ".map",
        ".master",
        ".mda",
        ".mdb",
        ".mde",
        ".mdp",
        ".me",
        ".mfp",
        ".mht",
        ".mhtml",
        ".mid",
        ".midi",
        ".mix",
        ".mk",
        ".mmf",
        ".mno",
        ".mny",
        ".mod",
        ".mov",
        ".movie",
        ".mp2",
        ".mp2v",
        ".mp3",
        ".mp4",
        ".mp4v",
        ".mpa",
        ".mpe",
        ".mpeg",
        ".mpf",
        ".mpg",
        ".mpp",
        ".mpv2",
        ".mqv",
        ".ms",
        ".msi",
        ".mso",
        ".mts",
        ".mtx",
        ".mvb",
        ".mvc",
        ".mxp",
        ".nc",
        ".nsc",
        ".nws",
        ".ocx",
        ".oda",
        ".odc",
        ".odh",
        ".odl",
        ".odp",
        ".ods",
        ".odt",
        ".one",
        ".onea",
        ".onepkg",
        ".onetmp",
        ".onetoc",
        ".onetoc2",
        ".orderedtest",
        ".osdx",
        ".p10",
        ".p12",
        ".p7b",
        ".p7c",
        ".p7m",
        ".p7r",
        ".p7s",
        ".pbm",
        ".pcast",
        ".pct",
        ".pcx",
        ".pcz",
        ".pdf",
        ".pfb",
        ".pfm",
        ".pfx",
        ".pgm",
        ".pic",
        ".pict",
        ".pkgdef",
        ".pkgundef",
        ".pko",
        ".pls",
        ".pma",
        ".pmc",
        ".pml",
        ".pmr",
        ".pmw",
        ".png",
        ".pnm",
        ".pnt",
        ".pntg",
        ".pnz",
        ".pot",
        ".potm",
        ".potx",
        ".ppa",
        ".ppam",
        ".ppm",
        ".pps",
        ".ppsm",
        ".ppsx",
        ".ppt",
        ".pptm",
        ".pptx",
        ".prf",
        ".prm",
        ".prx",
        ".ps",
        ".psc1",
        ".psd",
        ".psess",
        ".psm",
        ".psp",
        ".pub",
        ".pwz",
        ".qht",
        ".qhtm",
        ".qt",
        ".qti",
        ".qtif",
        ".qtl",
        ".qxd",
        ".ra",
        ".ram",
        ".rar",
        ".ras",
        ".rat",
        ".rc",
        ".rc2",
        ".rct",
        ".rdlc",
        ".resx",
        ".rf",
        ".rgb",
        ".rgs",
        ".rm",
        ".rmi",
        ".rmp",
        ".roff",
        ".rpm",
        ".rqy",
        ".rtf",
        ".rtx",
        ".ruleset",
        ".s",
        ".safariextz",
        ".scd",
        ".sct",
        ".sd2",
        ".sdp",
        ".sea",
        ".searchconnector-ms",
        ".setpay",
        ".setreg",
        ".settings",
        ".sgimb",
        ".sgml",
        ".sh",
        ".shar",
        ".shtml",
        ".sit",
        ".sitemap",
        ".skin",
        ".sldm",
        ".sldx",
        ".slk",
        ".sln",
        ".slupkg-ms",
        ".smd",
        ".smi",
        ".smx",
        ".smz",
        ".snd",
        ".snippet",
        ".snp",
        ".sol",
        ".sor",
        ".spc",
        ".spl",
        ".src",
        ".srf",
        ".ssisdeploymentmanifest",
        ".ssm",
        ".sst",
        ".stl",
        ".sv4cpio",
        ".sv4crc",
        ".svc",
        ".swf",
        ".t",
        ".tar",
        ".tcl",
        ".testrunconfig",
        ".testsettings",
        ".tex",
        ".texi",
        ".texinfo",
        ".tgz",
        ".thmx",
        ".thn",
        ".tif",
        ".tiff",
        ".tlh",
        ".tli",
        ".toc",
        ".tr",
        ".trm",
        ".trx",
        ".ts",
        ".tsv",
        ".ttf",
        ".tts",
        ".txt",
        ".u32",
        ".uls",
        ".user",
        ".ustar",
        ".vb",
        ".vbdproj",
        ".vbk",
        ".vbproj",
        ".vbs",
        ".vcf",
        ".vcproj",
        ".vcs",
        ".vcxproj",
        ".vddproj",
        ".vdp",
        ".vdproj",
        ".vdx",
        ".vml",
        ".vscontent",
        ".vsct",
        ".vsd",
        ".vsi",
        ".vsix",
        ".vsixlangpack",
        ".vsixmanifest",
        ".vsmdi",
        ".vspscc",
        ".vss",
        ".vsscc",
        ".vssettings",
        ".vssscc",
        ".vst",
        ".vstemplate",
        ".vsto",
        ".vsw",
        ".vsx",
        ".vtx",
        ".wav",
        ".wave",
        ".wax",
        ".wbk",
        ".wbmp",
        ".wcm",
        ".wdb",
        ".wdp",
        ".webarchive",
        ".webtest",
        ".wiq",
        ".wiz",
        ".wks",
        ".wlmp",
        ".wlpginstall",
        ".wlpginstall3",
        ".wm",
        ".wma",
        ".wmd",
        ".wmf",
        ".wml",
        ".wmlc",
        ".wmls",
        ".wmlsc",
        ".wmp",
        ".wmv",
        ".wmx",
        ".wmz",
        ".wpl",
        ".wps",
        ".wri",
        ".wrl",
        ".wrz",
        ".wsc",
        ".wsdl",
        ".wvx",
        ".x",
        ".xaf",
        ".xaml",
        ".xap",
        ".xbap",
        ".xbm",
        ".xdr",
        ".xht",
        ".xhtml",
        ".xla",
        ".xlam",
        ".xlc",
        ".xld",
        ".xlk",
        ".xll",
        ".xlm",
        ".xls",
        ".xlsb",
        ".xlsm",
        ".xlsx",
        ".xlt",
        ".xltm",
        ".xltx",
        ".xlw",
        ".xml",
        ".xmta",
        ".xof",
        ".xoml",
        ".xpm",
        ".xps",
        ".xrm-ms",
        ".xsc",
        ".xsd",
        ".xsf",
        ".xsl",
        ".xslt",
        ".xsn",
        ".xss",
        ".xtp",
        ".xwd",
        ".z",
        ".zip",

        #endregion
    };
}