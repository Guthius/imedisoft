#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class LetterMergeCrud
{
    public static LetterMerge SelectOne(long letterMergeNum)
    {
        var command = "SELECT * FROM lettermerge "
                      + "WHERE LetterMergeNum = " + SOut.Long(letterMergeNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static LetterMerge SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<LetterMerge> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<LetterMerge> TableToList(DataTable table)
    {
        var retVal = new List<LetterMerge>();
        LetterMerge letterMerge;
        foreach (DataRow row in table.Rows)
        {
            letterMerge = new LetterMerge();
            letterMerge.LetterMergeNum = SIn.Long(row["LetterMergeNum"].ToString());
            letterMerge.Description = SIn.String(row["Description"].ToString());
            letterMerge.TemplateName = SIn.String(row["TemplateName"].ToString());
            letterMerge.DataFileName = SIn.String(row["DataFileName"].ToString());
            letterMerge.Category = SIn.Long(row["Category"].ToString());
            letterMerge.ImageFolder = SIn.Long(row["ImageFolder"].ToString());
            retVal.Add(letterMerge);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<LetterMerge> listLetterMerges, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "LetterMerge";
        var table = new DataTable(tableName);
        table.Columns.Add("LetterMergeNum");
        table.Columns.Add("Description");
        table.Columns.Add("TemplateName");
        table.Columns.Add("DataFileName");
        table.Columns.Add("Category");
        table.Columns.Add("ImageFolder");
        foreach (var letterMerge in listLetterMerges)
            table.Rows.Add(SOut.Long(letterMerge.LetterMergeNum), letterMerge.Description, letterMerge.TemplateName, letterMerge.DataFileName, SOut.Long(letterMerge.Category), SOut.Long(letterMerge.ImageFolder));
        return table;
    }

    public static long Insert(LetterMerge letterMerge)
    {
        return Insert(letterMerge, false);
    }

    public static long Insert(LetterMerge letterMerge, bool useExistingPK)
    {
        var command = "INSERT INTO lettermerge (";

        command += "Description,TemplateName,DataFileName,Category,ImageFolder) VALUES(";

        command +=
            "'" + SOut.String(letterMerge.Description) + "',"
            + "'" + SOut.String(letterMerge.TemplateName) + "',"
            + "'" + SOut.String(letterMerge.DataFileName) + "',"
            + SOut.Long(letterMerge.Category) + ","
            + SOut.Long(letterMerge.ImageFolder) + ")";
        {
            letterMerge.LetterMergeNum = Db.NonQ(command, true, "LetterMergeNum", "letterMerge");
        }
        return letterMerge.LetterMergeNum;
    }

    public static long InsertNoCache(LetterMerge letterMerge)
    {
        return InsertNoCache(letterMerge, false);
    }

    public static long InsertNoCache(LetterMerge letterMerge, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO lettermerge (";
        if (isRandomKeys || useExistingPK) command += "LetterMergeNum,";
        command += "Description,TemplateName,DataFileName,Category,ImageFolder) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(letterMerge.LetterMergeNum) + ",";
        command +=
            "'" + SOut.String(letterMerge.Description) + "',"
            + "'" + SOut.String(letterMerge.TemplateName) + "',"
            + "'" + SOut.String(letterMerge.DataFileName) + "',"
            + SOut.Long(letterMerge.Category) + ","
            + SOut.Long(letterMerge.ImageFolder) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            letterMerge.LetterMergeNum = Db.NonQ(command, true, "LetterMergeNum", "letterMerge");
        return letterMerge.LetterMergeNum;
    }

    public static void Update(LetterMerge letterMerge)
    {
        var command = "UPDATE lettermerge SET "
                      + "Description   = '" + SOut.String(letterMerge.Description) + "', "
                      + "TemplateName  = '" + SOut.String(letterMerge.TemplateName) + "', "
                      + "DataFileName  = '" + SOut.String(letterMerge.DataFileName) + "', "
                      + "Category      =  " + SOut.Long(letterMerge.Category) + ", "
                      + "ImageFolder   =  " + SOut.Long(letterMerge.ImageFolder) + " "
                      + "WHERE LetterMergeNum = " + SOut.Long(letterMerge.LetterMergeNum);
        Db.NonQ(command);
    }

    public static bool Update(LetterMerge letterMerge, LetterMerge oldLetterMerge)
    {
        var command = "";
        if (letterMerge.Description != oldLetterMerge.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(letterMerge.Description) + "'";
        }

        if (letterMerge.TemplateName != oldLetterMerge.TemplateName)
        {
            if (command != "") command += ",";
            command += "TemplateName = '" + SOut.String(letterMerge.TemplateName) + "'";
        }

        if (letterMerge.DataFileName != oldLetterMerge.DataFileName)
        {
            if (command != "") command += ",";
            command += "DataFileName = '" + SOut.String(letterMerge.DataFileName) + "'";
        }

        if (letterMerge.Category != oldLetterMerge.Category)
        {
            if (command != "") command += ",";
            command += "Category = " + SOut.Long(letterMerge.Category) + "";
        }

        if (letterMerge.ImageFolder != oldLetterMerge.ImageFolder)
        {
            if (command != "") command += ",";
            command += "ImageFolder = " + SOut.Long(letterMerge.ImageFolder) + "";
        }

        if (command == "") return false;
        command = "UPDATE lettermerge SET " + command
                                            + " WHERE LetterMergeNum = " + SOut.Long(letterMerge.LetterMergeNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(LetterMerge letterMerge, LetterMerge oldLetterMerge)
    {
        if (letterMerge.Description != oldLetterMerge.Description) return true;
        if (letterMerge.TemplateName != oldLetterMerge.TemplateName) return true;
        if (letterMerge.DataFileName != oldLetterMerge.DataFileName) return true;
        if (letterMerge.Category != oldLetterMerge.Category) return true;
        if (letterMerge.ImageFolder != oldLetterMerge.ImageFolder) return true;
        return false;
    }

    public static void Delete(long letterMergeNum)
    {
        var command = "DELETE FROM lettermerge "
                      + "WHERE LetterMergeNum = " + SOut.Long(letterMergeNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listLetterMergeNums)
    {
        if (listLetterMergeNums == null || listLetterMergeNums.Count == 0) return;
        var command = "DELETE FROM lettermerge "
                      + "WHERE LetterMergeNum IN(" + string.Join(",", listLetterMergeNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}