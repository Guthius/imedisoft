using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CodeBase;
using DataConnectionBase;
using OpenDental.Thinfinity;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

///<summary>Handles documents and images for the Images module</summary>
public class Documents
{
    #region Insert

    ///<summary>Inserts the Document and retrieves it immediately from the database.</summary>
    public static Document InsertAndGet(Document document, Patient patient)
    {
        Insert(document, patient);
        return GetByNum(document.DocNum);
    }

    #endregion

    
    public static Document[] GetAllWithPat(long patNum)
    {
        var command = "SELECT * FROM document WHERE PatNum=" + SOut.Long(patNum) + " ORDER BY DateCreated";
        var table = DataCore.GetTable(command);
        return DocumentCrud.TableToList(table).ToArray();
    }

    
    public static List<Document> GetPatientData(long patNum)
    {
        var command = "SELECT * FROM document WHERE PatNum=" + SOut.Long(patNum) + " ORDER BY DateCreated";
        var table = DataCore.GetTable(command);
        return DocumentCrud.TableToList(table);
    }

    /// <summary>
    ///     Returns all Documents with an image capture type that is not Miscellaneous, in descending order by
    ///     dateCreated.
    /// </summary>
    public static List<Document> GetOcrDocumentsForPat(long patNum)
    {
        var command = "SELECT * FROM document WHERE PatNum=" + SOut.Long(patNum) + " AND ImageCaptureType > 0 ORDER BY DateCreated DESC";
        return DocumentCrud.SelectMany(command);
    }

    /// <summary>Gets the document with the specified document number.</summary>
    /// <param name="doReturnNullIfNotFound">If false and there is no document with that docNum, will return a new Document.</param>
    public static Document GetByNum(long docNum, bool doReturnNullIfNotFound = false)
    {
        Document document = null;
        if (docNum != 0) document = DocumentCrud.SelectOne(docNum);
        if (document == null && !doReturnNullIfNotFound) return new Document();
        return document;
    }

    public static List<Document> GetByNums(List<long> listDocNums)
    {
        if (listDocNums.Count < 1) return new List<Document>();
        var command = "SELECT * FROM document WHERE DocNum IN(" + string.Join(",", listDocNums) + ")";
        return DocumentCrud.SelectMany(command);
    }

    public static Document[] Fill(DataTable table)
    {
        if (table == null) return new Document[0];
        var listDocuments = DocumentCrud.TableToList(table);
        return listDocuments.ToArray();
    }
    
    /// <summary>
    ///     Returns a unique filename for a previously inserted doc based on the pat's first and last name and docNum with
    ///     the given extension.
    /// </summary>
    public static string GetUniqueFileNameForPatient(Patient patient, long docNum, string fileExtension)
    {
        var fileName = new string((patient.LName + patient.FName).Where(x => char.IsLetter(x)).ToArray()) + docNum + fileExtension; //ensures unique name
        //there is still a slight chance that someone manually added a file with this name, so quick fix:
        var listUsedNames = GetAllWithPat(patient.PatNum).Select(x => x.FileName).ToList();
        while (listUsedNames.Contains(fileName)) fileName = "x" + fileName;
        return fileName;
    }

    /// <summary>
    ///     Usually, set just the extension before passing in the doc.  Inserts a new document into db, creates a filename
    ///     based on Cur.DocNum, and then updates the db with this filename.  Should always refresh the document after calling
    ///     this method in order to get the correct filename for RemotingRole.ClientWeb.
    /// </summary>
    public static long Insert(Document document, Patient patient)
    {
        document.DocNum = DocumentCrud.Insert(document);
        if (document.FileName != Path.GetExtension(document.FileName)) return document.DocNum;
        //If the current filename is just an extension, then assign it a unique name.
        document.FileName = GenerateUniqueFileName(document.FileName, patient, document.DocNum.ToString());
        //there is still a slight chance that someone manually added a file with this name, so quick fix:
        var command = "SELECT FileName FROM document WHERE PatNum=" + SOut.Long(document.PatNum);
        var table = DataCore.GetTable(command);
        var listUsedNames = new List<string>();
        for (var i = 0; i < table.Rows.Count; i++) listUsedNames.Add(SIn.String(table.Rows[i][0].ToString()));
        while (true)
        {
            var hasMatch = false;
            for (var i = 0; i < listUsedNames.Count; i++)
                if (listUsedNames[i] == document.FileName)
                {
                    hasMatch = true;
                    break;
                }

            if (!hasMatch) break;
            document.FileName = "x" + document.FileName;
        }

        /*Document[] documentArray=GetAllWithPat(document.PatNum);
        while(IsFileNameInList(document.FileName,documentArray)) {
            document.FileName="x"+document.FileName;
        }*/
        Update(document);
        return document.DocNum;
    }

    //Returns a unique file name with the given extension for the specified patient. If uniqueNum is not set, will generate a random number to ensure the filename is unique.
    public static string GenerateUniqueFileName(string extension, Patient patient, string uniqueIdentifier = null)
    {
        if (string.IsNullOrWhiteSpace(extension) || patient == null) return "";
        if (uniqueIdentifier.IsNullOrEmpty()) //The current date/time stamp, to the 100,000th of a second
            uniqueIdentifier = DateTime.Now.ToString("yyMMddhhmmssfffff");
        var patientNameLastFirst = patient.LName + patient.FName;
        var fileName = new string(patientNameLastFirst.Where(x => char.IsLetter(x)).Select(x => x).ToArray());
        fileName += uniqueIdentifier + extension;
        return fileName;
    }

    ///<summary>This is a generic insert statement used to insert documents with custom file names.</summary>
    public static long Insert(Document document)
    {
        return DocumentCrud.Insert(document);
    }

    
    public static void Update(Document document)
    {
        DocumentCrud.Update(document);
    }

    
    public static bool Update(Document document, Document documentOld)
    {
        return DocumentCrud.Update(document, documentOld);
    }

    ///<summary>Updates all of the mount's Document.DocCategory information when moving a mount.</summary>
    public static void UpdateDocCategoryForMountItems(long mountNum, long docCategory)
    {
        var listMountItems = MountItems.GetItemsForMount(mountNum);
        var listDocumentsInMountDb = GetDocumentsForMountItems(listMountItems).Where(x => x != null).ToList();
        if (listDocumentsInMountDb.Count <= 0) return;
        for (var i = 0; i < listDocumentsInMountDb.Count; i++)
        {
            var documentOld = listDocumentsInMountDb[i].Copy();
            listDocumentsInMountDb[i].DocCategory = docCategory;
            Update(listDocumentsInMountDb[i], documentOld);
        }
    }

    
    public static void Delete(Document document)
    {
        DocumentCrud.Delete(document.DocNum);
    }

    /// <summary>
    ///     This is used by FormImageViewer to get a list of paths based on supplied list of DocNums. The reason is that
    ///     later we will allow sharing of documents, so the paths may not be in the current patient folder.
    /// </summary>
    public static List<string> GetPaths(List<long> listDocNums, string atoZPath)
    {
        if (listDocNums.Count == 0) return new List<string>();
        var command = "SELECT document.DocNum,document.FileName,patient.ImageFolder "
                      + "FROM document "
                      + "LEFT JOIN patient ON patient.PatNum=document.PatNum "
                      + "WHERE document.DocNum = '" + listDocNums[0] + "'";
        for (var i = 1; i < listDocNums.Count; i++) command += " OR document.DocNum = '" + listDocNums[i] + "'";
        //remember, they will not be in the correct order.
        var table = DataCore.GetTable(command);
        var hashtable = new Hashtable(); //key=docNum, value=path
        //one row for each document, but in the wrong order
        for (var i = 0; i < table.Rows.Count; i++)
            //We do not need to check if A to Z folders are being used here, because
            //thumbnails are not visible from the chart module when A to Z are disabled,
            //making it impossible to launch the form image viewer (the only place this
            //function is called from).
            hashtable.Add(SIn.Long(table.Rows[i][0].ToString()),
                ODFileUtils.CombinePaths(new[]
                {
                    atoZPath,
                    SIn.String(table.Rows[i][2].ToString()).Substring(0, 1).ToUpper(),
                    SIn.String(table.Rows[i][2].ToString()),
                    SIn.String(table.Rows[i][1].ToString())
                }));
        var listStrings = new List<string>();
        for (var i = 0; i < listDocNums.Count; i++) listStrings.Add((string) hashtable[listDocNums[i]]);
        return listStrings;
    }

    ///<summary>Will return null if no picture for this patient.</summary>
    public static Document GetPatPictFromDb(long patNum)
    {
        //first establish which category pat pics are in
        long defNumPicts = 0;
        var listDefs = Defs.GetDefsForCategory(DefCat.ImageCats, true);
        for (var i = 0; i < listDefs.Count; i++)
            if (Regex.IsMatch(listDefs[i].ItemValue, @"P"))
            {
                defNumPicts = listDefs[i].DefNum;
                break;
            }

        if (defNumPicts == 0) //no category set for picts
            return null;
        //then find, limit 1 to get the most recent
        var command = "SELECT * FROM document "
                      + "WHERE document.PatNum=" + SOut.Long(patNum)
                      + " AND document.DocCategory=" + SOut.Long(defNumPicts)
                      + " ORDER BY DateCreated DESC";
        command = DbHelper.LimitOrderBy(command, 1);
        var table = DataCore.GetTable(command);
        var listDocuments = Fill(table).ToList();
        if (listDocuments == null || listDocuments.Count < 1) //no pictures
            return null;
        return listDocuments[0];
    }

    /// <summary>
    ///     Makes one call to the database to retrieve the document of the patient for the given patNum, then uses that
    ///     document and the patFolder to load and process the patient picture so it appears the same way it did in the image
    ///     module.  It first creates a 100x100 thumbnail if needed, then it uses the thumbnail. Can return null. Assumes
    ///     WithPat will always be same as patnum.
    /// </summary>
    public static Bitmap GetPatPict(long patNum, string patFolder)
    {
        var document = GetPatPictFromDb(patNum);
        var bitmap = GetPatPict(patNum, patFolder, document);
        return bitmap;
    }

    /// <summary>
    ///     Uses the passed-in document and the patFolder to load and process the patient picture so it appears the same
    ///     way it did in the image module.  It first creates a 100x100 thumbnail if needed, then it uses the thumbnail. Can
    ///     return null. Assumes WithPat will always be same as patnum.
    /// </summary>
    public static Bitmap GetPatPict(long patNum, string patFolder, Document document)
    {
        if (document == null) return null;
        var bitmap = GetThumbnail(document, patFolder);
        return bitmap;
    }

    /// <summary>
    ///     Gets the thumbnail image for the given document. The thumbnail for every document is in a subfolder named
    ///     'Thumbnails' within each patient's images folder.  Always 100x100.
    /// </summary>
    public static Bitmap GetThumbnail(Document document, string patFolder)
    {
        var fileName = document.FileName;
        //If no file name associated with the document, then there cannot be a thumbnail,
        //because thumbnails have the same name as the original image document.
        if (fileName.Length < 1) return NoAvailablePhoto();
        var fullName = ODFileUtils.CombinePaths(patFolder, fileName);
        //If the document no longer exists, then there is no corresponding thumbnail image.
        if (true && !File.Exists(fullName)) return NoAvailablePhoto();
        //If the specified document is not an image return 'not available'.
        if (!ImageHelper.HasImageExtension(fullName)) return NoAvailablePhoto();
        //Create Thumbnails folder if it does not already exist for this patient folder.
        var thumbPath = ODFileUtils.CombinePaths(patFolder, "Thumbnails");
        if (true && !Directory.Exists(thumbPath))
            try
            {
                Directory.CreateDirectory(thumbPath);
            }
            catch (Exception ex)
            {
                return NoAvailablePhoto();
            }

        var fileNameThumb = ODFileUtils.CombinePaths(patFolder, "Thumbnails", fileName);
        //Use the existing thumbnail if it already exists and it was created after the last document modification.
        if (true && File.Exists(fileNameThumb))
            try
            {
                var dateTimeThumbMod = File.GetLastWriteTime(fileNameThumb);
                if (dateTimeThumbMod > document.DateTStamp //thumbnail file is old
                    && !document.IsCropOld) //Prevents using the existing thumbnail file if the crop data has not yet been converted to the new style. New thumbnail will be created below.
                {
                    var bitmap = (Bitmap) Image.FromFile(fileNameThumb);
                    var bitmap2 = new Bitmap(bitmap);
                    bitmap?.Dispose(); //releases the file lock
                    return bitmap2;
                }
            }
            catch
            {
                try
                {
                    File.Delete(fileNameThumb); //File may be invalid, corrupted, or unavailable. This was a bug in previous versions.
                }
                catch
                {
                    //we tried our best, and it just wasn't good enough
                }
            }

        #region Create New Thumbnail

        //Get the cropped/flipped/rotated image with any color filtering applied.
        Bitmap bitmapFullSize;
        try
        {
            bitmapFullSize = ImageHelper.GetImageCropped(document, patFolder);
        }
        catch (Exception ex)
        {
            return NoAvailablePhoto();
        }

        if (bitmapFullSize is null) return NoAvailablePhoto();
        var bitmapThumb = ImageHelper.GetBitmapSquare(bitmapFullSize, 100); //Thumbnails saved in the thumbnails folder are always 100x100
        bitmapFullSize?.Dispose();
        if (true) //Only save thumbnail to local directory if using local AtoZ
            try
            {
                bitmapThumb.Save(fileNameThumb);
            }
            catch (Exception ex)
            {
                //Oh well, we can regenerate it next time if we have to!
            }

        return bitmapThumb;

        #endregion Create New Thumbnail
    }

    public static Bitmap NoAvailablePhoto()
    {
        var bitmap = new Bitmap(100, 100);
        using var g = Graphics.FromImage(bitmap);
        g.InterpolationMode = InterpolationMode.High;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.FillRectangle(Brushes.WhiteSmoke, 0, 0, bitmap.Width, bitmap.Height);
        using var notAvailFormat = new StringFormat();
        notAvailFormat.Alignment = StringAlignment.Center;
        notAvailFormat.LineAlignment = StringAlignment.Center;
        using var font = new Font(FontFamily.GenericSansSerif, 8f);
        g.DrawString("Thumbnail not available", font, Brushes.Black, new RectangleF(0, 0, 100, 100), notAvailFormat);
        return bitmap;
    }

    ///<summary>Returns the documents which correspond to the given mountitems. They should already be ordered by ItemOrder.</summary>
    public static Document[] GetDocumentsForMountItems(List<MountItem> listMountItems)
    {
        if (listMountItems == null || listMountItems.Count < 1) return new Document[0];
        var strMountItemNums = string.Join(",", listMountItems.Select(x => SOut.Long(x.MountItemNum)));
        var command = "SELECT * FROM document WHERE MountItemNum IN(" + strMountItemNums + ")";
        var table = DataCore.GetTable(command);
        var listDocuments = DocumentCrud.TableToList(table);
        var documentArray = new Document[listMountItems.Count];
        for (var i = 0; i < listMountItems.Count; i++) documentArray[i] = listDocuments.Find(x => x.MountItemNum == listMountItems[i].MountItemNum); //frequently null
        return documentArray;
    }

    ///<summary>Returns the document for one mountitem. Can be null. Db call.</summary>
    public static Document GetDocumentForMountItem(long mountItemNum)
    {
        var command = "SELECT * FROM document WHERE MountItemNum='" + SOut.Long(mountItemNum) + "'";
        var document = DocumentCrud.SelectOne(command);
        return document;
    }

    /// <summary>
    ///     Any filenames mentioned in the listFiles which are not attached to the given patient are properly attached to
    ///     that patient. Returns the total number of documents that were newly attached to the patient.
    /// </summary>
    public static int InsertMissing(Patient patient, List<string> listFiles)
    {
        var countAdded = 0;
        var listDefNumsImgCat = Defs.GetDefsForCategory(DefCat.ImageCats).Select(x => x.DefNum).ToList();
        var table = GetFileNamesForPatient(patient.PatNum);
        for (var j = 0; j < listFiles.Count; j++)
        {
            var fileName = Path.GetFileName(listFiles[j]);
            if (!IsAcceptableFileName(fileName)) continue;
            var inList = false;
            for (var i = 0; i < table.Rows.Count && !inList; i++) inList = table.Rows[i]["FileName"].ToString() == fileName;
            if (inList) continue;
            //If fileName is not in listFiles
            var document = new Document();
            var strPrefixResult = "";
            var match = Regex.Match(fileName, @"^_\d*_"); //Check for specific category prefix information. Match should only be if we start with _###_ and not anywhere else.
            if (match.Success)
            {
                strPrefixResult = fileName.Substring(0, match.Length);
                fileName = fileName.Substring(strPrefixResult.Length);
                while (true)
                {
                    if (!File.Exists(Path.Combine(Path.GetDirectoryName(listFiles[j]), fileName))) break;
                    fileName = "x" + fileName;
                }

                File.Move(listFiles[j], Path.Combine(Path.GetDirectoryName(listFiles[j]), fileName));
            }

            var prefixCategoryNum = SIn.Long(strPrefixResult.Trim('_'));
            document.DocCategory = Defs.GetFirstForCategory(DefCat.ImageCats, true).DefNum;
            //Check if the category exists, if so move to that category otherwise it will go into the first one
            if (listDefNumsImgCat.Contains(prefixCategoryNum)) document.DocCategory = prefixCategoryNum;
            if (fileName.ToLower().EndsWith(".dcm"))
            {
                //DICOM images come with additional metadata we need to collect
                document.ImgType = ImageType.Radiograph;
                var bitmapDicom = DicomHelper.GetFromFile(listFiles[j]);
                DicomHelper.CalculateWindowingOnImport(bitmapDicom);
                document.PrintHeading = true;
                document.WindowingMin = bitmapDicom.WindowingMin;
                document.WindowingMax = bitmapDicom.WindowingMax;
            }

            document.DateCreated = DateTime.Now;
            var dateTPrevious = document.DateTStamp;
            document.Description = fileName;
            document.FileName = fileName;
            document.PatNum = patient.PatNum;
            Insert(document, patient);
            countAdded++;
            var strDocCategory = Defs.GetDef(DefCat.ImageCats, document.DocCategory).ItemName;
            SecurityLogs.MakeLogEntry(EnumPermType.ImageEdit, patient.PatNum, Lans.g("ContrImages", "Document Created: A file") + ", " + document.FileName + ", "
                                                                              + Lans.g("ContrImages", "placed into the patient's AtoZ images folder from outside of the program was detected and a record automatically inserted into the first image category") + ", " + strDocCategory, document.DocNum, dateTPrevious);
        }

        return countAdded;
    }

    ///<summary>Returns a datatable containing all filenames of the documents for the supplied patnum.</summary>
    public static DataTable GetFileNamesForPatient(long patNum)
    {
        var command = "SELECT FileName FROM document WHERE PatNum='" + patNum + "' ORDER BY FileName";
        return DataCore.GetTable(command);
    }

    ///<Summary>isImagingOrderDescending sorts images by DateCreated (descending). False by default.</Summary>
    public static DataSet RefreshForPatient(long patNum, bool isImagingOrderDescending = false)
    {
        var dataSet = new DataSet();
        dataSet.Tables.Add(GetTreeListTableForPatient(patNum, isImagingOrderDescending));
        return dataSet;
    }

    public static DataTable GetTreeListTableForPatient(long patNum, bool isImagingOrderDescending)
    {
        var dataConnection = new DataConnection();
        var table = new DataTable("DocumentList");
        DataRow dataRow;
        DataTable tableRaw;
        string command;
        //Rows are first added to listObjectsDataRows so they can be sorted at the end as a larger group, then
        //they are placed in the datatable to be returned.
        var listObjectsDataRows = new List<object>();
        //columns that start with lowercase are altered for display rather than being raw data.
        table.Columns.Add("DocNum");
        table.Columns.Add("MountNum");
        table.Columns.Add("DocCategory");
        table.Columns.Add("DateCreated");
        table.Columns.Add("idxCategory"); //Was previously called docFolder. The index of the category within Defs.
        table.Columns.Add("description");
        table.Columns.Add("ImgType");
        //Move all documents which are invisible to the first document category.
        //Why would a DocCategory ever be set to -1?  Where does that happen?
        //Also finds all document rows for the patient where the DocCategory is not a valid Image Category
        var listDefs = Defs.GetCatList((int) DefCat.ImageCats).ToList();
        command = "SELECT DocNum FROM document WHERE PatNum=" + SOut.Long(patNum) + " AND (DocCategory NOT IN (";
        for (var i = 0; i < listDefs.Count; i++)
        {
            if (i > 0) command += ",";
            command += SOut.Long(listDefs[i].DefNum);
        }

        command += ") OR DocCategory < 0)";
        tableRaw = dataConnection.GetTable(command);
        if (tableRaw.Rows.Count > 0)
        {
            //Are there any invisible documents?
            command = "UPDATE document SET DocCategory='" + Defs.GetFirstForCategory(DefCat.ImageCats, true).DefNum
                                                          + "' WHERE PatNum='" + SOut.Long(patNum) + "' AND (";
            for (var i = 0; i < tableRaw.Rows.Count; i++)
            {
                command += "DocNum='" + SIn.Long(tableRaw.Rows[i]["DocNum"].ToString()) + "' ";
                if (i < tableRaw.Rows.Count - 1) command += "OR ";
            }

            command += ")";
            dataConnection.NonQ(command);
        }

        //Load all documents into the result table.
        command = "SELECT DocNum,DocCategory,DateCreated,Description,ImgType,MountItemNum FROM document WHERE PatNum='" + SOut.Long(patNum) + "'";
        tableRaw = dataConnection.GetTable(command);
        for (var i = 0; i < tableRaw.Rows.Count; i++)
        {
            //Make sure hidden documents are never added (there is a small possibility that one is added after all are made visible).
            if (Defs.GetOrder(DefCat.ImageCats, SIn.Long(tableRaw.Rows[i]["DocCategory"].ToString())) < 0) continue;
            //Do not add individual documents which are part of a mount object.
            if (SIn.Long(tableRaw.Rows[i]["MountItemNum"].ToString()) != 0) continue;
            dataRow = table.NewRow();
            dataRow["DocNum"] = SIn.Long(tableRaw.Rows[i]["DocNum"].ToString());
            dataRow["MountNum"] = 0;
            dataRow["DocCategory"] = SIn.Long(tableRaw.Rows[i]["DocCategory"].ToString());
            dataRow["DateCreated"] = SIn.Date(tableRaw.Rows[i]["DateCreated"].ToString());
            dataRow["idxCategory"] = Defs.GetOrder(DefCat.ImageCats, SIn.Long(tableRaw.Rows[i]["DocCategory"].ToString()));
            dataRow["description"] = //PIn.Date(raw.Rows[i]["DateCreated"].ToString()).ToString("d")+": "+
                SIn.String(tableRaw.Rows[i]["Description"].ToString());
            dataRow["ImgType"] = SIn.Long(tableRaw.Rows[i]["ImgType"].ToString());
            listObjectsDataRows.Add(dataRow);
        }

        //Move all mounts which are invisible to the first document category.
        //Why would a DocCategory ever be set to -1?  Where does that happen?
        command = "SELECT MountNum FROM mount WHERE PatNum='" + SOut.Long(patNum) + "' AND "
                  + "DocCategory<0";
        tableRaw = dataConnection.GetTable(command);
        if (tableRaw.Rows.Count > 0)
        {
            //Are there any invisible mounts?
            command = "UPDATE mount SET DocCategory='" + Defs.GetFirstForCategory(DefCat.ImageCats, true).DefNum
                                                       + "' WHERE PatNum='" + SOut.Long(patNum) + "' AND (";
            for (var i = 0; i < tableRaw.Rows.Count; i++)
            {
                command += "MountNum='" + SIn.Long(tableRaw.Rows[i]["MountNum"].ToString()) + "' ";
                if (i < tableRaw.Rows.Count - 1) command += "OR ";
            }

            command += ")";
            dataConnection.NonQ(command);
        }

        //Load all mounts into the result table.
        command = "SELECT MountNum,DocCategory,DateCreated,Description FROM mount WHERE PatNum='" + SOut.Long(patNum) + "'";
        tableRaw = dataConnection.GetTable(command);
        for (var i = 0; i < tableRaw.Rows.Count; i++)
        {
            //Make sure hidden mounts are never added (there is a small possibility that one is added after all are made visible).
            if (Defs.GetOrder(DefCat.ImageCats, SIn.Long(tableRaw.Rows[i]["DocCategory"].ToString())) < 0) continue;
            dataRow = table.NewRow();
            dataRow["DocNum"] = 0;
            dataRow["MountNum"] = SIn.Long(tableRaw.Rows[i]["MountNum"].ToString());
            dataRow["DocCategory"] = SIn.Long(tableRaw.Rows[i]["DocCategory"].ToString());
            dataRow["DateCreated"] = SIn.Date(tableRaw.Rows[i]["DateCreated"].ToString());
            dataRow["idxCategory"] = Defs.GetOrder(DefCat.ImageCats, SIn.Long(tableRaw.Rows[i]["DocCategory"].ToString()));
            dataRow["description"] = //PIn.Date(raw.Rows[i]["DateCreated"].ToString()).ToString("d")+": "+
                SIn.String(tableRaw.Rows[i]["Description"].ToString());
            dataRow["ImgType"] = 0; //Not an image type at all.  It's a mount.
            listObjectsDataRows.Add(dataRow);
        }

        //We must sort the results after they are returned from the database, because the database software (i.e. MySQL)
        //cannot return sorted results from two or more result sets like we have here.
        listObjectsDataRows.Sort(delegate(object object1, object object2)
        {
            var dataRow1 = (DataRow) object1;
            var dataRow2 = (DataRow) object2;
            var idxCategory1 = Convert.ToInt32(dataRow1["idxCategory"].ToString());
            var idxCategory2 = Convert.ToInt32(dataRow2["idxCategory"].ToString());
            if (idxCategory1 < idxCategory2) return -1;

            if (idxCategory1 > idxCategory2) return 1;
            if (isImagingOrderDescending) return SIn.Date(dataRow2["DateCreated"].ToString()).CompareTo(SIn.Date(dataRow1["DateCreated"].ToString()));
            return SIn.Date(dataRow1["DateCreated"].ToString()).CompareTo(SIn.Date(dataRow2["DateCreated"].ToString()));
        });
        //Finally, move the results from the list into a data table.
        for (var i = 0; i < listObjectsDataRows.Count; i++) table.Rows.Add((DataRow) listObjectsDataRows[i]);
        return table;
    }

    ///<summary>Returns false if the file is a specific short file name that is not accepted.</summary>
    public static bool IsAcceptableFileName(string fileName)
    {
        var listBadFileNames = new List<string>();
        listBadFileNames.Add("thumbs.db");
        for (var i = 0; i < listBadFileNames.Count; i++)
            if (fileName.Length >= listBadFileNames[i].Length &&
                fileName.Substring(fileName.Length - listBadFileNames[i].Length,
                    listBadFileNames[i].Length).ToLower() == listBadFileNames[i])
                return false;

        if (fileName.StartsWith(".")) //Extension-only file, ignore.
            return false;
        return true;
    }
    
    /// <summary>
    ///     Moves one document from one patient to another and updates the file name accordingly.
    ///     Only call when physically storing images in a folder share and after the physical images have been successfully
    ///     copied over to the "to patient" folder.
    /// </summary>
    public static void MergePatientDocument(long patNumFrom, long patNumTo, string fileNameOld, string fileNameNew)
    {
        var command = "UPDATE document"
                      + " SET PatNum=" + SOut.Long(patNumTo) + ","
                      + " FileName='" + SOut.String(fileNameNew) + "'"
                      + " WHERE PatNum=" + SOut.Long(patNumFrom)
                      + " AND FileName='" + SOut.String(fileNameOld) + "'";
        Db.NonQ(command);
    }

    /// <summary>
    ///     Moves all documents from one patient to another.
    ///     Only call when physically storing images in a folder share and only if every document.Filename matches a file in
    ///     patNumTo's folder.
    /// </summary>
    public static void MergePatientDocuments(long patNumFrom, long patNumTo)
    {
        var command = "UPDATE document"
                      + " SET PatNum=" + SOut.Long(patNumTo)
                      + " WHERE PatNum=" + SOut.Long(patNumFrom);
        Db.NonQ(command);
    }

    /// <summary>
    ///     Attempts to open the document using the default program. If not using AtoZfolder saves a local temp file and
    ///     opens it. Returns empty string on success, otherwise returns error message.
    /// </summary>
    public static string OpenDoc(long docNum)
    {
        var document = GetByNum(docNum);
        if (document.DocNum == 0) return "";
        var patient = Patients.GetPat(document.PatNum);
        if (patient == null) return "";
        string documentPath;
        if (true)
        {
            documentPath = ImageStore.GetFilePath(document, ImageStore.GetPatientFolder(patient, ImageStore.GetPreferredAtoZpath()));
        }

        try
        {
            Process.Start(documentPath);
        }
        catch (Exception ex)
        {
            return Lans.g("Documents", "Error occurred while attempting to open document.\r\n" +
                                       "Verify a default application has been selected to open files of type: ") +
                   Path.GetExtension(document.FileName) + "\r\n" + ex.Message;
        }

        return "";
    }

    //Checks to see if the document exists in the correct location, or checks DB for stored content.
    public static bool DocExists(long docNum)
    {
        var document = GetByNum(docNum);
        if (document.DocNum == 0) return false;
        var patient = Patients.GetPat(document.PatNum);
        if (patient == null) return false;
        if (true) return File.Exists(ImageStore.GetFilePath(document, ImageStore.GetPatientFolder(patient, ImageStore.GetPreferredAtoZpath())));
    }

    /// <summary>
    ///     Returns the filepath of the document if using AtoZfolder. If storing files in DB or third party storage, saves
    ///     document to local temp file and returns filepath.
    ///     Empty string if not found.
    /// </summary>
    public static string GetPath(long docNum)
    {
        var document = GetByNum(docNum);
        if (document.DocNum == 0) return "";
        var patient = Patients.GetPat(document.PatNum);
        if (patient == null) return "";
        string documentPath;
        if (true)
        {
            documentPath = ImageStore.GetFilePath(document, ImageStore.GetPatientFolder(patient, ImageStore.GetPreferredAtoZpath()));
        }

        return documentPath;
    }

    #region Xam Methods

    ///<summary>Throws exception. Creates and Saves a PDF document for the given statement.</summary>
    public static DataSet CreateAndSaveStatementPDF(Statement statement, SheetDef sheetDef, bool isLimitedCustom, bool showLName, bool excludeTxfr, List<Def> listDefsImageCat, string pdfFileName = "", Sheet sheet = null, DataSet dataSet = null, string description = "")
    {
        string tempPath;
        if (statement == null || sheetDef == null) return null;
        if (dataSet == null)
        {
            if (isLimitedCustom)
            {
                dataSet = AccountModules.GetSuperFamAccount(statement, doIncludePatLName: showLName, doShowHiddenPaySplits: statement.IsReceipt, doExcludeTxfrs: excludeTxfr);
            }
            else
            {
                var patNum = Statements.GetPatNumForGetAccount(statement);
                dataSet = AccountModules.GetAccount(patNum, statement, doIncludePatLName: showLName, doShowHiddenPaySplits: statement.IsReceipt, doExcludeTxfrs: excludeTxfr);
            }
        }

        if (pdfFileName == "")
        {
            if (sheet == null)
            {
                sheet = SheetUtil.CreateSheet(sheetDef, statement.PatNum, statement.HidePayment);
                sheet.Parameters.Add(new SheetParameter(true, "Statement") {ParamValue = statement});
                SheetFiller.FillFields(sheet, dataSet, statement);
                SheetUtil.CalculateHeights(sheet, dataSet, statement);
            }

            tempPath = ODFileUtils.CombinePaths(PrefC.GetTempFolderPath(), statement.PatNum + ".pdf");
            SheetPrinting.CreatePdf(sheet, tempPath, statement, dataSet, null);
        }
        else
        {
            tempPath = pdfFileName;
        }

        long docCategory = 0;
        for (var i = 0; i < listDefsImageCat.Count; i++)
            if (Regex.IsMatch(listDefsImageCat[i].ItemValue, @"S"))
            {
                docCategory = listDefsImageCat[i].DefNum;
                break;
            }

        if (docCategory == 0) docCategory = listDefsImageCat[0].DefNum; //put it in the first category.
        //create doc--------------------------------------------------------------------------------------
        Document document = null;
        try
        {
            document = ImageStore.Import(tempPath, docCategory, Patients.GetPat(statement.PatNum));
        }
        finally
        {
            //Delete the temp file since we don't need it anymore.
            try
            {
                if (pdfFileName == "") //If they're passing in a PDF file name, they probably have it open somewhere else.
                    File.Delete(tempPath);
            }
            catch
            {
                //Do nothing.  This file will likely get cleaned up later.
            }
        }

        document.ImgType = ImageType.Document;
        document.Description = description;
        //Some customers have wanted to sort their statements in the images module by date and time.
        //We would need to enhance DateSent to include the time portion.
        statement.DateSent = document.DateCreated;
        statement.DocNum = document.DocNum; //this signals the calling class that the pdf was created successfully.
        Statements.AttachDoc(statement.StatementNum, document);
        Statements.SyncStatementProdsForStatement(dataSet, statement.StatementNum, statement.DocNum);
        return dataSet;
    }

    #endregion Xam Methods
}

public class DocumentForApi
{
    public DateTime DateTimeServer;
    public Document DocumentCur;
}