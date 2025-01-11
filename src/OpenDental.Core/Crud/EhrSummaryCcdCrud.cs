#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrSummaryCcdCrud
{
    public static EhrSummaryCcd SelectOne(long ehrSummaryCcdNum)
    {
        var command = "SELECT * FROM ehrsummaryccd "
                      + "WHERE EhrSummaryCcdNum = " + SOut.Long(ehrSummaryCcdNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrSummaryCcd SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrSummaryCcd> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrSummaryCcd> TableToList(DataTable table)
    {
        var retVal = new List<EhrSummaryCcd>();
        EhrSummaryCcd ehrSummaryCcd;
        foreach (DataRow row in table.Rows)
        {
            ehrSummaryCcd = new EhrSummaryCcd();
            ehrSummaryCcd.EhrSummaryCcdNum = SIn.Long(row["EhrSummaryCcdNum"].ToString());
            ehrSummaryCcd.PatNum = SIn.Long(row["PatNum"].ToString());
            ehrSummaryCcd.DateSummary = SIn.Date(row["DateSummary"].ToString());
            ehrSummaryCcd.ContentSummary = SIn.String(row["ContentSummary"].ToString());
            ehrSummaryCcd.EmailAttachNum = SIn.Long(row["EmailAttachNum"].ToString());
            retVal.Add(ehrSummaryCcd);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrSummaryCcd> listEhrSummaryCcds, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrSummaryCcd";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrSummaryCcdNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("DateSummary");
        table.Columns.Add("ContentSummary");
        table.Columns.Add("EmailAttachNum");
        foreach (var ehrSummaryCcd in listEhrSummaryCcds)
            table.Rows.Add(SOut.Long(ehrSummaryCcd.EhrSummaryCcdNum), SOut.Long(ehrSummaryCcd.PatNum), SOut.DateT(ehrSummaryCcd.DateSummary, false), ehrSummaryCcd.ContentSummary, SOut.Long(ehrSummaryCcd.EmailAttachNum));
        return table;
    }

    public static long Insert(EhrSummaryCcd ehrSummaryCcd)
    {
        return Insert(ehrSummaryCcd, false);
    }

    public static long Insert(EhrSummaryCcd ehrSummaryCcd, bool useExistingPK)
    {
        var command = "INSERT INTO ehrsummaryccd (";

        command += "PatNum,DateSummary,ContentSummary,EmailAttachNum) VALUES(";

        command +=
            SOut.Long(ehrSummaryCcd.PatNum) + ","
                                            + SOut.Date(ehrSummaryCcd.DateSummary) + ","
                                            + DbHelper.ParamChar + "paramContentSummary,"
                                            + SOut.Long(ehrSummaryCcd.EmailAttachNum) + ")";
        if (ehrSummaryCcd.ContentSummary == null) ehrSummaryCcd.ContentSummary = "";
        var paramContentSummary = new OdSqlParameter("paramContentSummary", OdDbType.Text, SOut.StringParam(ehrSummaryCcd.ContentSummary));
        {
            ehrSummaryCcd.EhrSummaryCcdNum = Db.NonQ(command, true, "EhrSummaryCcdNum", "ehrSummaryCcd", paramContentSummary);
        }
        return ehrSummaryCcd.EhrSummaryCcdNum;
    }

    public static long InsertNoCache(EhrSummaryCcd ehrSummaryCcd)
    {
        return InsertNoCache(ehrSummaryCcd, false);
    }

    public static long InsertNoCache(EhrSummaryCcd ehrSummaryCcd, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrsummaryccd (";
        if (isRandomKeys || useExistingPK) command += "EhrSummaryCcdNum,";
        command += "PatNum,DateSummary,ContentSummary,EmailAttachNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrSummaryCcd.EhrSummaryCcdNum) + ",";
        command +=
            SOut.Long(ehrSummaryCcd.PatNum) + ","
                                            + SOut.Date(ehrSummaryCcd.DateSummary) + ","
                                            + DbHelper.ParamChar + "paramContentSummary,"
                                            + SOut.Long(ehrSummaryCcd.EmailAttachNum) + ")";
        if (ehrSummaryCcd.ContentSummary == null) ehrSummaryCcd.ContentSummary = "";
        var paramContentSummary = new OdSqlParameter("paramContentSummary", OdDbType.Text, SOut.StringParam(ehrSummaryCcd.ContentSummary));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramContentSummary);
        else
            ehrSummaryCcd.EhrSummaryCcdNum = Db.NonQ(command, true, "EhrSummaryCcdNum", "ehrSummaryCcd", paramContentSummary);
        return ehrSummaryCcd.EhrSummaryCcdNum;
    }

    public static void Update(EhrSummaryCcd ehrSummaryCcd)
    {
        var command = "UPDATE ehrsummaryccd SET "
                      + "PatNum          =  " + SOut.Long(ehrSummaryCcd.PatNum) + ", "
                      + "DateSummary     =  " + SOut.Date(ehrSummaryCcd.DateSummary) + ", "
                      + "ContentSummary  =  " + DbHelper.ParamChar + "paramContentSummary, "
                      + "EmailAttachNum  =  " + SOut.Long(ehrSummaryCcd.EmailAttachNum) + " "
                      + "WHERE EhrSummaryCcdNum = " + SOut.Long(ehrSummaryCcd.EhrSummaryCcdNum);
        if (ehrSummaryCcd.ContentSummary == null) ehrSummaryCcd.ContentSummary = "";
        var paramContentSummary = new OdSqlParameter("paramContentSummary", OdDbType.Text, SOut.StringParam(ehrSummaryCcd.ContentSummary));
        Db.NonQ(command, paramContentSummary);
    }

    public static bool Update(EhrSummaryCcd ehrSummaryCcd, EhrSummaryCcd oldEhrSummaryCcd)
    {
        var command = "";
        if (ehrSummaryCcd.PatNum != oldEhrSummaryCcd.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(ehrSummaryCcd.PatNum) + "";
        }

        if (ehrSummaryCcd.DateSummary.Date != oldEhrSummaryCcd.DateSummary.Date)
        {
            if (command != "") command += ",";
            command += "DateSummary = " + SOut.Date(ehrSummaryCcd.DateSummary) + "";
        }

        if (ehrSummaryCcd.ContentSummary != oldEhrSummaryCcd.ContentSummary)
        {
            if (command != "") command += ",";
            command += "ContentSummary = " + DbHelper.ParamChar + "paramContentSummary";
        }

        if (ehrSummaryCcd.EmailAttachNum != oldEhrSummaryCcd.EmailAttachNum)
        {
            if (command != "") command += ",";
            command += "EmailAttachNum = " + SOut.Long(ehrSummaryCcd.EmailAttachNum) + "";
        }

        if (command == "") return false;
        if (ehrSummaryCcd.ContentSummary == null) ehrSummaryCcd.ContentSummary = "";
        var paramContentSummary = new OdSqlParameter("paramContentSummary", OdDbType.Text, SOut.StringParam(ehrSummaryCcd.ContentSummary));
        command = "UPDATE ehrsummaryccd SET " + command
                                              + " WHERE EhrSummaryCcdNum = " + SOut.Long(ehrSummaryCcd.EhrSummaryCcdNum);
        Db.NonQ(command, paramContentSummary);
        return true;
    }

    public static bool UpdateComparison(EhrSummaryCcd ehrSummaryCcd, EhrSummaryCcd oldEhrSummaryCcd)
    {
        if (ehrSummaryCcd.PatNum != oldEhrSummaryCcd.PatNum) return true;
        if (ehrSummaryCcd.DateSummary.Date != oldEhrSummaryCcd.DateSummary.Date) return true;
        if (ehrSummaryCcd.ContentSummary != oldEhrSummaryCcd.ContentSummary) return true;
        if (ehrSummaryCcd.EmailAttachNum != oldEhrSummaryCcd.EmailAttachNum) return true;
        return false;
    }

    public static void Delete(long ehrSummaryCcdNum)
    {
        var command = "DELETE FROM ehrsummaryccd "
                      + "WHERE EhrSummaryCcdNum = " + SOut.Long(ehrSummaryCcdNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrSummaryCcdNums)
    {
        if (listEhrSummaryCcdNums == null || listEhrSummaryCcdNums.Count == 0) return;
        var command = "DELETE FROM ehrsummaryccd "
                      + "WHERE EhrSummaryCcdNum IN(" + string.Join(",", listEhrSummaryCcdNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}