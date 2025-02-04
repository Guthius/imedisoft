using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using CodeBase;
using Imedisoft.Core.Caching;

namespace OpenDental;

public class FileAtoZ
{
    public static void OpenFile(string actualFilePath, string displayedFileName = "")
    {
        try
        {
            var tempFile = Path.Combine(PrefC.GetTempFolderPath(), displayedFileName == "" ? Path.GetFileName(actualFilePath) : displayedFileName);

            File.Copy(actualFilePath, tempFile, true);

            Process.Start(tempFile);
        }
        catch (Exception ex)
        {
            MsgBox.Show(ex.Message);
        }
    }

    public static string AppendSuffix(string filePath, string suffix)
    {
        return ODFileUtils.AppendSuffix(filePath, suffix);
    }

    public static Bitmap GetImage(string path)
    {
        return new Bitmap(path);
    }

    public static void StartProcessRelative(string folder, string fileName)
    {
        Process.Start(Path.Combine(OpenDentBusiness.FileIO.FileAtoZ.GetPreferredAtoZpath(), folder, fileName));
    }

    public static void Copy(string sourceFileName, string destinationFileName, bool doOverwrite = false)
    {
        File.Copy(sourceFileName, destinationFileName, doOverwrite);
    }
}