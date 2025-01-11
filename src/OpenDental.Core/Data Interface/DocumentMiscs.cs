using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class DocumentMiscs
{
    public static DocumentMisc GetByTypeAndFileName(string fileName, DocumentMiscType documentMiscType)
    {
        var command = "SELECT * FROM documentmisc "
                      + "WHERE DocMiscType=" + SOut.Int((int) documentMiscType) + " "
                      + "AND FileName='" + SOut.String(fileName) + "'";
        return DocumentMiscCrud.SelectOne(command);
    }

    public static List<DocumentMisc> GetByType(DocumentMiscType documentMiscType, bool canIncludeRawBase64 = false, string nameStartWith = "")
    {
        var command = "SELECT ";
        if (canIncludeRawBase64)
            command += "* ";
        else
            command += "DocMiscNum,DateCreated,FileName,DocMiscType,'' RawBase64 ";
        command += "FROM documentmisc WHERE DocMiscType=" + SOut.Int((int) documentMiscType);
        if (!string.IsNullOrEmpty(nameStartWith)) command += " AND FileName LIKE '" + SOut.String(nameStartWith) + "%'";
        return DocumentMiscCrud.SelectMany(command);
    }

    
    public static long Insert(DocumentMisc documentMisc)
    {
        return DocumentMiscCrud.Insert(documentMisc);
    }

    ///<summary>Appends the passed in rawBase64 string to the RawBase64 column in the db for the UpdateFiles DocMiscType row.</summary>
    public static void AppendRawBase64ForUpdateFiles(string rawBase64)
    {
        var command = "UPDATE documentmisc SET RawBase64=CONCAT(" + DbHelper.IfNull("RawBase64", "") + "," + DbHelper.ParamChar + "paramRawBase64) "
                      + "WHERE DocMiscType=" + SOut.Int((int) DocumentMiscType.UpdateFiles);
        var sqlParameterRawBase64 = new OdSqlParameter("paramRawBase64", OdDbType.Text, rawBase64);
        Db.NonQ(command, sqlParameterRawBase64);
    }

    
    public static void DeleteAllForType(DocumentMiscType documentMiscType)
    {
        var command = "DELETE FROM documentmisc WHERE DocMiscType=" + SOut.Int((int) documentMiscType);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDocMiscNums)
    {
        DocumentMiscCrud.DeleteMany(listDocMiscNums);
    }

    ///<summary>Returns a non-empty and translated error message on failure. Returns null on success.</summary>
    public static string LaunchShareScreen()
    {
        var shareScreenUriBase = "https://opendental.com/resources/OpenDentalShareScreen";
        var shareScreenVersion = "";
        var xmlDocument = new XmlDocument();
        try
        {
            xmlDocument.Load(shareScreenUriBase + ".xml");
        }
        catch (Exception ex)
        {
            return Lans.g("DocumentMiscs", "Could not download share screen configuration file") + ": " + ex.Message;
        }

        if (xmlDocument.DocumentElement != null && xmlDocument.ChildNodes.Count > 0)
        {
            //Shouldn't fail, unless the xml doc got corrupted in transit or the resource was modified.
            var xmlNodeRoot = xmlDocument.ChildNodes[0];
            var listXmlNodes = xmlNodeRoot.ChildNodes.AsEnumerable<XmlNode>().ToList();
            var xmlNodeVersion = listXmlNodes.Find(x => x.Name.ToLower() == "version");
            if (xmlNodeVersion != null) shareScreenVersion = xmlNodeVersion.InnerText;
        }

        if (shareScreenVersion == "") //If shareScreenVersion is still empty by this point, something went wrong while reading xmlDocument.
            return Lans.g("DocumentMiscs", "Could not read share screen configuration file. Try again or contact support.");
        var exeUri = shareScreenUriBase + ".exe";
        var exeName = Path.GetFileName(exeUri);
        var exeFilePath = ODFileUtils.CombinePaths(ODFileUtils.GetProgramDirectory(), exeName);
        var isDownloadNeeded = true;
        if (File.Exists(exeFilePath))
        {
            FileVersionInfo fileVersionInfo = null;
            try
            {
                fileVersionInfo = FileVersionInfo.GetVersionInfo(exeFilePath);
            }
            catch (Exception ex)
            {
            }

            if (fileVersionInfo != null && fileVersionInfo.FileVersion == shareScreenVersion) //FileVersion can be null if exe is corrupt or empty from failed previous download.
                isDownloadNeeded = false;
        }

        if (isDownloadNeeded)
        {
            //local exe is missing, corrupt, or an old version.
            var documentMiscExisting = GetByType(DocumentMiscType.ShareScreenExeSegment, nameStartWith: shareScreenVersion)
                .Find(x => x.FileName.EndsWith("}")); //Empty segment indicating a complete exe ends with }
            if (documentMiscExisting == null)
            {
                //A complete OpenDentalShareScreen.exe of this version does not exist in the database yet. Download from website.
                var webClient = new WebClient();
                try
                {
                    webClient.DownloadFile(exeUri, exeFilePath);
                }
                catch (Exception ex)
                {
                    webClient?.Dispose();
                    return Lans.g("DocumentMiscs", "Could not download " + exeName + " from website") + ":\r\n"
                                                                                                      + ex.Message + "\r\n"
                                                                                                      + Lans.g("DocumentMiscs", "Close any existing " + exeName + " instances, check permissions, and try again.");
                }

                webClient?.Dispose();
                try
                {
                    //Make the exe available for the local network to prevent additional web downloads.
                    var charsPerPayload = MiscData.GetMaxAllowedPacket() - 8192; //Arbitrarily subtracted 8KB from max allowed bytes for MySQL "header" information.
                    charsPerPayload = Math.Min(charsPerPayload, 1048575); //1048575 is divisible by 3 which is important for Base64 "appending" logic.
                    charsPerPayload = 3 * charsPerPayload / 4; //Scale down to account for bloating when converting to base64.
                    charsPerPayload -= charsPerPayload % 3; //Use the closest amount of bytes divisible by 3 for compatibility with base64 enconding.
                    var byteArrayBuffer = new byte[charsPerPayload];
                    var fileStream = File.OpenRead(exeFilePath);
                    var segmentCount = 0;
                    var dateCreated = DateTime.Today;
                    var guid = Guid.NewGuid(); //We need this guid to separate segments from a broader set of segments if multiple workstaions inserted at the same time.
                    var countRead = fileStream.Read(byteArrayBuffer, 0, byteArrayBuffer.Length);
                    while (countRead > 0)
                    {
                        var documentMiscSegment = new DocumentMisc();
                        documentMiscSegment.DateCreated = dateCreated;
                        var segmentCountStr = segmentCount.ToString().PadLeft(5, '0');
                        documentMiscSegment.FileName = shareScreenVersion + "{" + guid + "}" + segmentCountStr;
                        segmentCount++;
                        documentMiscSegment.DocMiscType = DocumentMiscType.ShareScreenExeSegment;
                        documentMiscSegment.RawBase64 = Convert.ToBase64String(byteArrayBuffer, 0, countRead);
                        Insert(documentMiscSegment);
                        countRead = fileStream.Read(byteArrayBuffer, 0, byteArrayBuffer.Length);
                    }

                    fileStream.Close();
                    var documentMiscLastSegment = new DocumentMisc(); //This empty segment indicates that the exe has been completely inserted into the database.
                    documentMiscLastSegment.DateCreated = dateCreated;
                    documentMiscLastSegment.FileName = shareScreenVersion + "{" + guid + "}";
                    documentMiscLastSegment.DocMiscType = DocumentMiscType.ShareScreenExeSegment;
                    documentMiscLastSegment.RawBase64 = "";
                    Insert(documentMiscLastSegment);
                    var listDocumentMiscsSegmentsOld = GetByType(DocumentMiscType.ShareScreenExeSegment)
                        .FindAll(x => !x.FileName.StartsWith(shareScreenVersion));
                    var listDocMiscNums = listDocumentMiscsSegmentsOld.Select(x => x.DocMiscNum).ToList();
                    DeleteMany(listDocMiscNums);
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                //The exe is pre-existing in the database. Unpack from database instead of downloading from web.
                try
                {
                    var listDocumentMiscsSegments =
                        GetByType(DocumentMiscType.ShareScreenExeSegment, true, documentMiscExisting.FileName)
                            .OrderBy(x => x.FileName).ToList();
                    var fileStream = File.Open(exeFilePath, FileMode.Create); //Creates or overwrites.
                    for (var i = 0; i < listDocumentMiscsSegments.Count; i++)
                    {
                        var byteArraySegments = Convert.FromBase64String(listDocumentMiscsSegments[i].RawBase64);
                        fileStream.Write(byteArraySegments, 0, byteArraySegments.Length);
                    }

                    fileStream.Close();
                }
                catch (Exception ex)
                {
                    return Lans.g("DocumentMiscs", "Could not unpack " + exeName + " from database") + ":\r\n"
                                                                                                     + ex.Message + "\r\n"
                                                                                                     + Lans.g("DocumentMiscs", "Close any existing " + exeName + " instances, check permissions, and try again.");
                }
            }
        }

        try
        {
            Process.Start(exeFilePath);
        }
        catch (Exception ex)
        {
            return Lans.g("DocumentMiscs", "Failed to launch " + exeName) + ": " + ex.Message;
        }

        return null;
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<DocumentMisc> Refresh(long patNum){

        string command="SELECT * FROM documentmisc WHERE PatNum = "+POut.Long(patNum);
        return Crud.DocumentMiscCrud.SelectMany(command);
    }

    
    public static long Insert(DocumentMisc documentMisc){

        return Crud.DocumentMiscCrud.Insert(documentMisc);
    }

    
    public static void Update(DocumentMisc documentMisc){

        Crud.DocumentMiscCrud.Update(documentMisc);
    }

    
    public static void Delete(long docMiscNum) {

        string command= "DELETE FROM documentmisc WHERE DocMiscNum = "+POut.Long(docMiscNum);
        Db.NonQ(command);
    }
    */
}