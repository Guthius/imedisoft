#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DocumentMiscCrud
{
    public static DocumentMisc SelectOne(long docMiscNum)
    {
        var command = "SELECT * FROM documentmisc "
                      + "WHERE DocMiscNum = " + SOut.Long(docMiscNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static DocumentMisc SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<DocumentMisc> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DocumentMisc> TableToList(DataTable table)
    {
        var retVal = new List<DocumentMisc>();
        DocumentMisc documentMisc;
        foreach (DataRow row in table.Rows)
        {
            documentMisc = new DocumentMisc();
            documentMisc.DocMiscNum = SIn.Long(row["DocMiscNum"].ToString());
            documentMisc.DateCreated = SIn.Date(row["DateCreated"].ToString());
            documentMisc.FileName = SIn.String(row["FileName"].ToString());
            documentMisc.DocMiscType = (DocumentMiscType) SIn.Int(row["DocMiscType"].ToString());
            documentMisc.RawBase64 = SIn.String(row["RawBase64"].ToString());
            retVal.Add(documentMisc);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<DocumentMisc> listDocumentMiscs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "DocumentMisc";
        var table = new DataTable(tableName);
        table.Columns.Add("DocMiscNum");
        table.Columns.Add("DateCreated");
        table.Columns.Add("FileName");
        table.Columns.Add("DocMiscType");
        table.Columns.Add("RawBase64");
        foreach (var documentMisc in listDocumentMiscs)
            table.Rows.Add(SOut.Long(documentMisc.DocMiscNum), SOut.DateT(documentMisc.DateCreated, false), documentMisc.FileName, SOut.Int((int) documentMisc.DocMiscType), documentMisc.RawBase64);
        return table;
    }

    public static long Insert(DocumentMisc documentMisc)
    {
        return Insert(documentMisc, false);
    }

    public static long Insert(DocumentMisc documentMisc, bool useExistingPK)
    {
        var command = "INSERT INTO documentmisc (";

        command += "DateCreated,FileName,DocMiscType,RawBase64) VALUES(";

        command +=
            SOut.Date(documentMisc.DateCreated) + ","
                                                + "'" + SOut.String(documentMisc.FileName) + "',"
                                                + SOut.Int((int) documentMisc.DocMiscType) + ","
                                                + DbHelper.ParamChar + "paramRawBase64)";
        if (documentMisc.RawBase64 == null) documentMisc.RawBase64 = "";
        var paramRawBase64 = new OdSqlParameter("paramRawBase64", OdDbType.Text, SOut.StringParam(documentMisc.RawBase64));
        {
            documentMisc.DocMiscNum = Db.NonQ(command, true, "DocMiscNum", "documentMisc", paramRawBase64);
        }
        return documentMisc.DocMiscNum;
    }

    public static long InsertNoCache(DocumentMisc documentMisc)
    {
        return InsertNoCache(documentMisc, false);
    }

    public static long InsertNoCache(DocumentMisc documentMisc, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO documentmisc (";
        if (isRandomKeys || useExistingPK) command += "DocMiscNum,";
        command += "DateCreated,FileName,DocMiscType,RawBase64) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(documentMisc.DocMiscNum) + ",";
        command +=
            SOut.Date(documentMisc.DateCreated) + ","
                                                + "'" + SOut.String(documentMisc.FileName) + "',"
                                                + SOut.Int((int) documentMisc.DocMiscType) + ","
                                                + DbHelper.ParamChar + "paramRawBase64)";
        if (documentMisc.RawBase64 == null) documentMisc.RawBase64 = "";
        var paramRawBase64 = new OdSqlParameter("paramRawBase64", OdDbType.Text, SOut.StringParam(documentMisc.RawBase64));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramRawBase64);
        else
            documentMisc.DocMiscNum = Db.NonQ(command, true, "DocMiscNum", "documentMisc", paramRawBase64);
        return documentMisc.DocMiscNum;
    }

    public static void Update(DocumentMisc documentMisc)
    {
        var command = "UPDATE documentmisc SET "
                      + "DateCreated=  " + SOut.Date(documentMisc.DateCreated) + ", "
                      + "FileName   = '" + SOut.String(documentMisc.FileName) + "', "
                      + "DocMiscType=  " + SOut.Int((int) documentMisc.DocMiscType) + ", "
                      + "RawBase64  =  " + DbHelper.ParamChar + "paramRawBase64 "
                      + "WHERE DocMiscNum = " + SOut.Long(documentMisc.DocMiscNum);
        if (documentMisc.RawBase64 == null) documentMisc.RawBase64 = "";
        var paramRawBase64 = new OdSqlParameter("paramRawBase64", OdDbType.Text, SOut.StringParam(documentMisc.RawBase64));
        Db.NonQ(command, paramRawBase64);
    }

    public static bool Update(DocumentMisc documentMisc, DocumentMisc oldDocumentMisc)
    {
        var command = "";
        if (documentMisc.DateCreated.Date != oldDocumentMisc.DateCreated.Date)
        {
            if (command != "") command += ",";
            command += "DateCreated = " + SOut.Date(documentMisc.DateCreated) + "";
        }

        if (documentMisc.FileName != oldDocumentMisc.FileName)
        {
            if (command != "") command += ",";
            command += "FileName = '" + SOut.String(documentMisc.FileName) + "'";
        }

        if (documentMisc.DocMiscType != oldDocumentMisc.DocMiscType)
        {
            if (command != "") command += ",";
            command += "DocMiscType = " + SOut.Int((int) documentMisc.DocMiscType) + "";
        }

        if (documentMisc.RawBase64 != oldDocumentMisc.RawBase64)
        {
            if (command != "") command += ",";
            command += "RawBase64 = " + DbHelper.ParamChar + "paramRawBase64";
        }

        if (command == "") return false;
        if (documentMisc.RawBase64 == null) documentMisc.RawBase64 = "";
        var paramRawBase64 = new OdSqlParameter("paramRawBase64", OdDbType.Text, SOut.StringParam(documentMisc.RawBase64));
        command = "UPDATE documentmisc SET " + command
                                             + " WHERE DocMiscNum = " + SOut.Long(documentMisc.DocMiscNum);
        Db.NonQ(command, paramRawBase64);
        return true;
    }

    public static bool UpdateComparison(DocumentMisc documentMisc, DocumentMisc oldDocumentMisc)
    {
        if (documentMisc.DateCreated.Date != oldDocumentMisc.DateCreated.Date) return true;
        if (documentMisc.FileName != oldDocumentMisc.FileName) return true;
        if (documentMisc.DocMiscType != oldDocumentMisc.DocMiscType) return true;
        if (documentMisc.RawBase64 != oldDocumentMisc.RawBase64) return true;
        return false;
    }

    public static void Delete(long docMiscNum)
    {
        var command = "DELETE FROM documentmisc "
                      + "WHERE DocMiscNum = " + SOut.Long(docMiscNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDocMiscNums)
    {
        if (listDocMiscNums == null || listDocMiscNums.Count == 0) return;
        var command = "DELETE FROM documentmisc "
                      + "WHERE DocMiscNum IN(" + string.Join(",", listDocMiscNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}