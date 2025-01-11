using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using CodeBase;
using OpenDentBusiness;

namespace OpenDental
{
    public class FileAtoZ
    {
        public static string ReadAllText(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        public static void WriteAllText(string fileName, string textForFile)
        {
            {
                //Not cloud
                File.WriteAllText(fileName, textForFile);
            }
        }

        public static List<string> GetFilesInDirectory(string folderFullPath)
        {
            return OpenDentBusiness.FileIO.FileAtoZ.GetFilesInDirectory(folderFullPath);
        }

        public static List<string> GetFilesInDirectoryRelative(string folder)
        {
            return OpenDentBusiness.FileIO.FileAtoZ.GetFilesInDirectoryRelative(folder);
        }

        public static void OpenFile(string actualFilePath, string displayedFileName = "")
        {
            try
            {
                string tempFile;
                if (displayedFileName == "")
                {
                    tempFile = ODFileUtils.CombinePaths(PrefC.GetTempFolderPath(), Path.GetFileName(actualFilePath));
                }
                else
                {
                    tempFile = ODFileUtils.CombinePaths(PrefC.GetTempFolderPath(), displayedFileName);
                }

                File.Copy(actualFilePath, tempFile, true);

                Process.Start(tempFile);
            }
            catch (Exception ex)
            {
                MsgBox.Show(ex.Message);
            }
        }

        public static string CombinePaths(params string[] paths)
        {
            return OpenDentBusiness.FileIO.FileAtoZ.CombinePaths(paths);
        }

        public static string AppendSuffix(string filePath, string suffix)
        {
            return OpenDentBusiness.FileIO.FileAtoZ.AppendSuffix(filePath, suffix);
        }

        public static bool Exists(string filePath)
        {
            return OpenDentBusiness.FileIO.FileAtoZ.Exists(filePath);
        }

        public static Bitmap GetImage(string imagePath)
        {
            return new Bitmap(imagePath);
        }

        public static void StartProcess(string fileFullPath)
        {
            Process.Start(fileFullPath);
        }

        public static void StartProcessRelative(string folder, string fileName)
        {
            StartProcess(CombinePaths(OpenDentBusiness.FileIO.FileAtoZ.GetPreferredAtoZpath(), folder, fileName));
        }

        public static void Copy(string sourceFileName, string destinationFileName, bool doOverwrite = false)
        {
            File.Copy(sourceFileName, destinationFileName, doOverwrite);
        }

        public static void Delete(string fileName)
        {
            OpenDentBusiness.FileIO.FileAtoZ.Delete(fileName);
        }

        public static bool DirectoryExists(string folderName)
        {
            return OpenDentBusiness.FileIO.FileAtoZ.DirectoryExists(folderName);
        }

        public static void OpenDirectory(string folderName)
        {
            Process.Start(folderName);
        }

        public static void Download(string AtoZFilePath, string localFilePath)
        {
            Copy(AtoZFilePath, localFilePath);
        }

        public static void Upload(string sourceFileName, string destinationFileName)
        {
            Copy(sourceFileName, destinationFileName);
        }
    }
}