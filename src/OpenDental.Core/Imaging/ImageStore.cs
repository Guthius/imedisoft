using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using ODCrypt;
using OpenDentBusiness.FileIO;
using Encoder = System.Drawing.Imaging.Encoder;

namespace OpenDentBusiness;

public class ImageStore
{
    public static string GetDataFolder()
    {
        return FileAtoZ.GetPreferredAtoZpath();
    }

    public static string GetPatientFolder(Patient patient, string path)
    {
        var patientCopy = patient.Copy();
        if (string.IsNullOrEmpty(patient.ImageFolder))
        {
            patient.ImageFolder = GetImageFolderName(patient);
        }

        var patientFolder = Path.Combine(path, patient.ImageFolder.Substring(0, 1).ToUpper(), patient.ImageFolder);

        try
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ApplicationException("AtoZpath was null or empty");
            }

            if (!Directory.Exists(patientFolder))
            {
                Directory.CreateDirectory(patientFolder);
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Error. Could not create folder for patient: " + patientFolder, ex);
        }

        if (string.IsNullOrEmpty(patientCopy.ImageFolder))
        {
            Patients.Update(patient, patientCopy);
        }

        return patientFolder;
    }

    public static string GetImageFolderName(Patient patient)
    {
        var patientName = patient.LName + patient.FName;

        var folder = "";

        for (var i = 0; i < patientName.Length; i++)
        {
            if (char.IsLetter(patientName, i))
            {
                folder += patientName.Substring(i, 1);
            }
        }

        folder += patient.PatNum.ToString();

        return folder;
    }

    private static string GetFolder(string folderName)
    {
        var dataFolder = GetDataFolder();
        if (string.IsNullOrEmpty(dataFolder))
        {
            throw new ApplicationException("Could not find the path for the data folder.");
        }

        var path = Path.Combine(dataFolder, folderName);
        if (Directory.Exists(path))
        {
            return path;
        }

        Directory.CreateDirectory(path);

        return path;
    }

    public static string GetEobFolder()
    {
        return GetFolder("EOBs");
    }

    public static string GetAmdFolder()
    {
        return GetFolder("Amendments");
    }

    public static string GetProviderImagesFolder()
    {
        return GetFolder("ProviderImages");
    }

    public static string GetEmailImagePath()
    {
        return GetFolder("EmailImages");
    }

    public static void AddMissingFilesToDatabase(Patient patient)
    {
        var patientFolder = GetPatientFolder(patient, GetDataFolder());

        var directoryInfo = new DirectoryInfo(patientFolder);

        var fileNames = directoryInfo
            .GetFiles()
            .Where(x => !x.Attributes.HasFlag(FileAttributes.Hidden))
            .Select(x => x.FullName)
            .ToList();

        Documents.InsertMissing(patient, fileNames);
    }

    public static string GetHashString(Document document, string patientFolder, bool includeFileInHash = true)
    {
        var bytes = new byte[1];
        if (includeFileInHash)
        {
            bytes = GetBytes(document, patientFolder);
        }

        var noteBytes = Encoding.UTF8.GetBytes(document.Note ?? "");

        var length = bytes.Length;
        var buffer = new byte[noteBytes.Length + bytes.Length];

        Array.Copy(bytes, 0, buffer, 0, length);
        Array.Copy(noteBytes, 0, buffer, length, noteBytes.Length);

        return Encoding.ASCII.GetString(MD5.Hash(buffer));
    }

    public static BitmapDicom OpenBitmapDicom(Document document, string patientFolder)
    {
        if (!document.FileName.EndsWith(".dcm"))
        {
            return null;
        }

        var path = Path.Combine(patientFolder, document.FileName);

        return DicomHelper.GetFromFile(path);
    }

    public static Collection<Bitmap> OpenImages(IList<Document> documents, string patientFolder)
    {
        var bitmaps = new Collection<Bitmap>();

        foreach (var document in documents)
        {
            bitmaps.Add(document is null ? null : OpenImage(document, patientFolder));
        }

        return bitmaps;
    }

    public static Bitmap[] OpenImages(Document[] documents, string patientFolder)
    {
        return OpenImages(new Collection<Document>(documents), patientFolder).ToArray();
    }

    public static Bitmap OpenImage(Document document, string patientFolder)
    {
        var path = Path.Combine(patientFolder, document.FileName);

        if (!HasImageExtension(path))
        {
            return null;
        }

        try
        {
            return new Bitmap(path);
        }
        catch
        {
            return null;
        }
    }

    public static Bitmap[] OpenImagesEob(EobAttach eob)
    {
        var values = new Bitmap[1];
        var path = Path.Combine(GetEobFolder(), eob.FileName);

        if (HasImageExtension(path))
        {
            if (File.Exists(path))
            {
                try
                {
                    values[0] = new Bitmap(path);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("File found but could not be opened: " + path, ex);
                }
            }
            else
            {
                throw new ApplicationException("File not found: " + path);
            }
        }
        else
        {
            values[0] = null;
        }

        return values;
    }

    public static byte[] GetBytes(Document doc, string patFolder)
    {
        var path = Path.Combine(patFolder, doc.FileName);
        if (!File.Exists(path))
        {
            return [];
        }

        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        var length = (int) fileStream.Length;
        var bytes = new byte[length];

        _ = fileStream.Read(bytes, 0, length);

        return bytes;
    }

    public static Document Import(string path, long docCategory, Patient patient)
    {
        var patientFolder = GetPatientFolder(patient, GetDataFolder());

        var document = new Document();

        if (Path.GetExtension(path) == "")
        {
            try
            {
                using var bitmap = new Bitmap(path);

                document.FileName = ".jpg";
            }
            catch
            {
                document.FileName = ".txt";
            }
        }
        else
        {
            document.FileName = Path.GetExtension(path);
        }

        document.DateCreated = File.GetLastWriteTime(path);
        document.PatNum = patient.PatNum;

        if (HasImageExtension(document.FileName))
        {
            document.ImgType = ImageType.Photo;
            if (path.ToLower().EndsWith("jpg") || path.ToLower().EndsWith("jpeg"))
            {
                var image = Image.FromFile(path);

                var propertyItem = image.PropertyItems.FirstOrDefault(x => x.Id == 0x0112);
                if (propertyItem is not null && propertyItem.Value.Length > 0)
                {
                    document.DegreesRotated = propertyItem.Value[0] switch
                    {
                        6 => 90,
                        3 => 180,
                        8 => 270,
                        _ => document.DegreesRotated
                    };
                }

                image.Dispose();
            }
        }
        else if (document.FileName.ToLower().EndsWith(".dcm"))
        {
            document.ImgType = ImageType.Radiograph;

            var bitmapDicom = DicomHelper.GetFromFile(path);

            DicomHelper.CalculateWindowingOnImport(bitmapDicom);

            document.PrintHeading = true;
            document.WindowingMin = bitmapDicom.WindowingMin;
            document.WindowingMax = bitmapDicom.WindowingMax;
        }
        else
        {
            document.ImgType = ImageType.Document;
        }

        document.DocCategory = docCategory;
        document = Documents.InsertAndGet(document, patient);

        try
        {
            SaveDocument(document, path, patientFolder);
        }
        catch
        {
            Documents.Delete(document);

            throw;
        }

        return document;
    }

    public static Document Import(Bitmap image, long docCategory, ImageType imageType, Patient patient, string mimeType = "image/jpeg", bool printHeading = false)
    {
        var patientFolder = GetPatientFolder(patient, GetDataFolder());

        var document = new Document
        {
            ImgType = imageType,
            FileName = GetImageFileExtensionByMimeType(mimeType),
            DateCreated = DateTime.Now,
            PatNum = patient.PatNum,
            DocCategory = docCategory
        };

        if (printHeading)
        {
            document.PrintHeading = true;
        }

        Documents.Insert(document, patient);

        document = Documents.GetByNum(document.DocNum);

        var quality = imageType is ImageType.Radiograph or ImageType.Photo or ImageType.Attachment ? 100L : ComputerPrefs.LocalComputer.ScanDocQuality;
        var imageCodecInfos = ImageCodecInfo.GetImageEncoders();

        ImageCodecInfo imageCodecInfo = null;

        foreach (var codecInfo in imageCodecInfos)
        {
            if (codecInfo.MimeType == mimeType)
            {
                imageCodecInfo = codecInfo;
            }
        }

        var encoderParameters = new EncoderParameters(1);
        var encoderParameter = new EncoderParameter(Encoder.Quality, quality);

        encoderParameters.Param[0] = encoderParameter;

        try
        {
            SaveDocument(document, image, imageCodecInfo, encoderParameters, patientFolder);
        }
        catch
        {
            Documents.Delete(document);

            throw;
        }

        return document;
    }

    public static Document Import(byte[] bytes, long docCategory, ImageType imageType, Patient patient, string mimeType = "image/jpeg", string fileExtension = null)
    {
        var patientFolder = GetPatientFolder(patient, GetDataFolder());

        var document = new Document
        {
            ImgType = imageType,
            FileName = fileExtension ?? GetImageFileExtensionByMimeType(mimeType),
            DateCreated = DateTime.Now,
            PatNum = patient.PatNum,
            DocCategory = docCategory
        };

        Documents.Insert(document, patient);

        document = Documents.GetByNum(document.DocNum);

        try
        {
            SaveDocument(document, bytes, patientFolder);
        }
        catch
        {
            Documents.Delete(document);
            throw;
        }

        return document;
    }

    private static string GetImageFileExtensionByMimeType(string mimeType)
    {
        return mimeType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/tiff" => ".tif",
            _ => throw new NotSupportedException()
        };
    }

    public static Document ImportForm(string form, long docCategory, Patient patient)
    {
        var patientFolder = GetPatientFolder(patient, GetDataFolder());
        var sourceFileName = Path.Combine(GetDataFolder(), "Forms", form);

        if (!File.Exists(sourceFileName))
        {
            throw new Exception("Could not find file: " + sourceFileName);
        }

        var document = new Document
        {
            FileName = Path.GetExtension(sourceFileName),
            DateCreated = DateTime.Now,
            DocCategory = docCategory,
            PatNum = patient.PatNum,
            ImgType = ImageType.Document
        };

        Documents.Insert(document, patient);

        document = Documents.GetByNum(document.DocNum);

        try
        {
            SaveDocument(document, sourceFileName, patientFolder);
        }
        catch
        {
            Documents.Delete(document);
            throw;
        }

        return document;
    }

    public static Document ImportImageToMount(Bitmap image, short rotationAngle, long mountItemNum, long docCategory, Patient patient)
    {
        var patientFolder = GetPatientFolder(patient, GetDataFolder());

        var document = new Document
        {
            MountItemNum = mountItemNum,
            DegreesRotated = rotationAngle,
            ImgType = ImageType.Radiograph,
            FileName = ".bmp",
            DateCreated = DateTime.Now,
            PatNum = patient.PatNum,
            DocCategory = docCategory,
            PrintHeading = true,
            WindowingMin = PrefC.GetInt(PrefName.ImageWindowingMin),
            WindowingMax = PrefC.GetInt(PrefName.ImageWindowingMax)
        };

        Documents.Insert(document, patient);

        document = Documents.GetByNum(document.DocNum);
        try
        {
            SaveDocument(document, image, patientFolder);
        }
        catch
        {
            Documents.Delete(document);

            throw;
        }

        return document;
    }

    public static EobAttach ImportEobAttach(Bitmap image, long claimPaymentNum)
    {
        var eobFolder = GetEobFolder();

        var eob = new EobAttach
        {
            FileName = ".jpg",
            DateTCreated = DateTime.Now,
            ClaimPaymentNum = claimPaymentNum
        };

        EobAttaches.Insert(eob);

        eob = EobAttaches.GetOne(eob.EobAttachNum);

        var quality = (long) ComputerPrefs.LocalComputer.ScanDocQuality;
        var imageCodecInfos = ImageCodecInfo.GetImageEncoders();

        ImageCodecInfo myImageCodecInfo = null;

        foreach (var imageCodecInfo in imageCodecInfos)
        {
            if (imageCodecInfo.MimeType == "image/jpeg")
            {
                myImageCodecInfo = imageCodecInfo;
            }
        }

        var encoderParameters = new EncoderParameters(1);
        var encoderParameter = new EncoderParameter(Encoder.Quality, quality);

        encoderParameters.Param[0] = encoderParameter;

        try
        {
            SaveEobAttach(eob, image, myImageCodecInfo, encoderParameters, eobFolder);
        }
        catch
        {
            EobAttaches.Delete(eob.EobAttachNum);

            throw;
        }

        return eob;
    }

    public static EobAttach ImportEobAttach(string path, long claimPaymentNum)
    {
        var eobFolder = GetEobFolder();

        var eob = new EobAttach
        {
            FileName = Path.GetExtension(path) == "" ? ".jpg" : Path.GetExtension(path),
            DateTCreated = File.GetLastWriteTime(path),
            ClaimPaymentNum = claimPaymentNum
        };

        EobAttaches.Insert(eob);

        eob = EobAttaches.GetOne(eob.EobAttachNum);

        try
        {
            SaveEobAttach(eob, path, eobFolder);
        }
        catch
        {
            EobAttaches.Delete(eob.EobAttachNum);

            throw;
        }

        return eob;
    }

    public static void Export(string destFileName, Document document, Patient patient)
    {
        var sourceFileName = Path.Combine(GetPatientFolder(patient, GetDataFolder()), document.FileName);

        File.Copy(sourceFileName, destFileName, true);
    }

    public static void ExportEobAttach(string destFileName, EobAttach eob)
    {
        var path = Path.Combine(GetEobFolder(), eob.FileName);

        File.Copy(path, destFileName);
    }

    public static void SaveBitmapJpg(Bitmap bitmap, string fileName, long quality)
    {
        if (bitmap is null)
        {
            return;
        }

        var encoderParameter = new EncoderParameter(Encoder.Quality, quality);
        var encoderParameters = new EncoderParameters(1);

        encoderParameters.Param[0] = encoderParameter;

        var imageCodecInfo = ImageCodecInfo.GetImageEncoders().First(x => x.MimeType == "image/jpeg");

        bitmap.Save(fileName, imageCodecInfo, encoderParameters);
    }

    public static void SaveBitmap(Bitmap bitmap, string fileName, long quality = 90)
    {
        if (bitmap is null)
        {
            return;
        }

        var extension = Path.GetExtension(fileName);

        var encoderParameter = new EncoderParameter(Encoder.Quality, quality);
        var encoderParameters = new EncoderParameters(1);

        encoderParameters.Param[0] = encoderParameter;

        var imageCodecInfo = extension switch
        {
            ".tiff" or ".tif" => ImageCodecInfo.GetImageEncoders().First(x => x.MimeType == "image/tiff"),
            ".bmp" => ImageCodecInfo.GetImageEncoders().First(x => x.MimeType == "image/bmp"),
            ".png" => ImageCodecInfo.GetImageEncoders().First(x => x.MimeType == "image/png"),
            ".gif" => ImageCodecInfo.GetImageEncoders().First(x => x.MimeType == "image/gif"),
            _ => ImageCodecInfo.GetImageEncoders().First(x => x.MimeType == "image/jpeg")
        };

        bitmap.Save(fileName, imageCodecInfo, encoderParameters);
    }

    public static void SaveDocument(Document document, Bitmap image, string patientFolder)
    {
        using (var bitmap = new Bitmap(image))
        {
            var pathFileOut = Path.Combine(patientFolder, document.FileName);

            bitmap.Save(pathFileOut);
        }

        LogDocument("Document Created: ", EnumPermType.ImageEdit, document, DateTime.MinValue);
    }

    public static void SaveDocument(Document document, Bitmap image, ImageCodecInfo imageCodecInfo, EncoderParameters encoderParameters, string patientFolder)
    {
        using (var bitmap = new Bitmap(image))
        {
            bitmap.Save(Path.Combine(patientFolder, document.FileName), imageCodecInfo, encoderParameters);
        }

        LogDocument(document.ImgType + " Created: ", EnumPermType.ImageCreate, document, DateTime.MinValue);
    }

    public static void SaveDocument(Document document, string sourceFileName, string patientFolder)
    {
        File.Copy(sourceFileName, Path.Combine(patientFolder, document.FileName));

        LogDocument(document.ImgType + " Created: ", EnumPermType.ImageCreate, document, DateTime.MinValue);
    }

    public static void SaveDocument(Document document, byte[] bytes, string patientFolder)
    {
        File.WriteAllBytes(Path.Combine(patientFolder, document.FileName), bytes);

        LogDocument(document.ImgType + " Created: ", EnumPermType.ImageCreate, document, DateTime.MinValue);
    }

    public static void SaveEobAttach(EobAttach eob, Bitmap image, ImageCodecInfo codec, EncoderParameters encoderParameters, string eobFolder)
    {
        using var bitmap = new Bitmap(image);

        bitmap.Save(Path.Combine(eobFolder, eob.FileName), codec, encoderParameters);
    }

    public static void SaveEobAttach(EobAttach eob, string sourceFileName, string eobFolder)
    {
        File.Copy(sourceFileName, Path.Combine(eobFolder, eob.FileName));
    }

    public static void DeleteDocuments(IList<Document> documents, string patFolder)
    {
        foreach (var document in documents)
        {
            if (document is null)
            {
                continue;
            }

            var sheets = Sheets.GetForDocument(document.DocNum);
            if (sheets.Count != 0)
            {
                var message = "Cannot delete image, it is referenced by sheets with the following dates:";

                foreach (var sheet in sheets)
                {
                    message += "\r\n" + sheet.DateTimeSheet.ToShortDateString();
                }

                throw new Exception(message);
            }

            try
            {
                var path = Path.Combine(patFolder, document.FileName);

                if (File.Exists(path))
                {
                    File.Delete(path);

                    LogDocument("Document Deleted: ", EnumPermType.ImageDelete, document, document.DateTStamp);
                }
            }
            catch
            {
                throw new Exception(
                    "Could not delete file. " +
                    "It may be in use by another program, flagged as read-only, or you might not have sufficient permissions.");
            }

            Documents.Delete(document);
            ImageDraws.DeleteByDocNum(document.DocNum);
            PearlRequests.DeleteByDocNum(document.DocNum);
        }
    }

    public static void DeleteEobAttach(EobAttach eob)
    {
        var path = Path.Combine(GetEobFolder(), eob.FileName);

        if (File.Exists(path))
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
                // ignored
            }
        }

        EobAttaches.Delete(eob.EobAttachNum);
    }

    public static void TryDeleteFile(string filePath, Action<string> actInUseException = null)
    {
        try
        {
            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            if (!ex.Message.ToLower().Contains("being used by another process"))
            {
                throw;
            }

            actInUseException?.Invoke(ex.Message);
        }
    }

    public static void DeleteThumbnailImage(Document document, string patientFolder)
    {
        var path = Path.Combine(patientFolder, "Thumbnails", document.FileName);

        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch
        {
            // ignored
        }
    }

    public static string GetExtension(Document document)
    {
        return Path.GetExtension(document.FileName).ToLower();
    }

    public static string GetFilePath(Document document, string patientFolder)
    {
        return Path.Combine(patientFolder, document.FileName);
    }

    public static bool HasImageExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();

        return extension is ".jpg" or ".jpeg" or ".tga" or ".bmp" or ".tif" or ".tiff" or ".gif" or ".emf" or ".exif" or ".ico" or ".png" or ".wmf" or ".tig";
    }

    public static void LogDocument(string messagePrefix, EnumPermType perm, Document doc, DateTime secDatePrevious, long userNum = 0)
    {
        var message = messagePrefix + doc.FileName;

        if (doc.Description != "")
        {
            var description = doc.Description;
            if (description.Length > 50)
            {
                description = description.Substring(0, 50);
            }

            message += " with description " + description;
        }

        if (doc.DocCategory != 0)
        {
            var docCat = Defs.GetDef(DefCat.ImageCats, doc.DocCategory);

            message += " with category " + docCat.ItemName;
        }

        if (userNum == 0)
        {
            SecurityLogs.MakeLogEntry(perm, doc.PatNum, message, doc.DocNum, secDatePrevious);

            return;
        }

        SecurityLogs.MakeLogEntry(perm, doc.PatNum, message, doc.DocNum, LogSources.None, secDatePrevious, userNum);
    }
}