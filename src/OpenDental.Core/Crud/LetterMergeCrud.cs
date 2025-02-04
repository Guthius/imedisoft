using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class LetterMergeCrud
{
    public static List<LetterMerge> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<LetterMerge> TableToList(DataTable table)
    {
        var retVal = new List<LetterMerge>();
        foreach (DataRow row in table.Rows)
        {
            var letterMerge = new LetterMerge
            {
                LetterMergeNum = SIn.Long(row["LetterMergeNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                TemplateName = SIn.String(row["TemplateName"].ToString()),
                DataFileName = SIn.String(row["DataFileName"].ToString()),
                Category = SIn.Long(row["Category"].ToString()),
                ImageFolder = SIn.Long(row["ImageFolder"].ToString())
            };
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

    public static void Insert(LetterMerge letterMerge)
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
}