using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ClaimFormItemCrud
{
    public static List<ClaimFormItem> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ClaimFormItem> TableToList(DataTable table)
    {
        var retVal = new List<ClaimFormItem>();
        ClaimFormItem claimFormItem;
        foreach (DataRow row in table.Rows)
        {
            claimFormItem = new ClaimFormItem();
            claimFormItem.ClaimFormItemNum = SIn.Long(row["ClaimFormItemNum"].ToString());
            claimFormItem.ClaimFormNum = SIn.Long(row["ClaimFormNum"].ToString());
            claimFormItem.ImageFileName = SIn.String(row["ImageFileName"].ToString());
            claimFormItem.FieldName = SIn.String(row["FieldName"].ToString());
            claimFormItem.FormatString = SIn.String(row["FormatString"].ToString());
            claimFormItem.XPos = SIn.Float(row["XPos"].ToString());
            claimFormItem.YPos = SIn.Float(row["YPos"].ToString());
            claimFormItem.Width = SIn.Float(row["Width"].ToString());
            claimFormItem.Height = SIn.Float(row["Height"].ToString());
            retVal.Add(claimFormItem);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ClaimFormItem> listClaimFormItems, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ClaimFormItem";
        var table = new DataTable(tableName);
        table.Columns.Add("ClaimFormItemNum");
        table.Columns.Add("ClaimFormNum");
        table.Columns.Add("ImageFileName");
        table.Columns.Add("FieldName");
        table.Columns.Add("FormatString");
        table.Columns.Add("XPos");
        table.Columns.Add("YPos");
        table.Columns.Add("Width");
        table.Columns.Add("Height");
        foreach (var claimFormItem in listClaimFormItems)
            table.Rows.Add(SOut.Long(claimFormItem.ClaimFormItemNum), SOut.Long(claimFormItem.ClaimFormNum), claimFormItem.ImageFileName, claimFormItem.FieldName, claimFormItem.FormatString, SOut.Float(claimFormItem.XPos), SOut.Float(claimFormItem.YPos), SOut.Float(claimFormItem.Width), SOut.Float(claimFormItem.Height));
        return table;
    }

    public static long Insert(ClaimFormItem claimFormItem)
    {
        var command = "INSERT INTO claimformitem (";

        command += "ClaimFormNum,ImageFileName,FieldName,FormatString,XPos,YPos,Width,Height) VALUES(";

        command +=
            SOut.Long(claimFormItem.ClaimFormNum) + ","
                                                  + "'" + SOut.String(claimFormItem.ImageFileName) + "',"
                                                  + "'" + SOut.String(claimFormItem.FieldName) + "',"
                                                  + "'" + SOut.String(claimFormItem.FormatString) + "',"
                                                  + SOut.Float(claimFormItem.XPos) + ","
                                                  + SOut.Float(claimFormItem.YPos) + ","
                                                  + SOut.Float(claimFormItem.Width) + ","
                                                  + SOut.Float(claimFormItem.Height) + ")";
        {
            claimFormItem.ClaimFormItemNum = Db.NonQ(command, true, "ClaimFormItemNum", "claimFormItem");
        }
        return claimFormItem.ClaimFormItemNum;
    }

    public static void Update(ClaimFormItem claimFormItem)
    {
        var command = "UPDATE claimformitem SET "
                      + "ClaimFormNum    =  " + SOut.Long(claimFormItem.ClaimFormNum) + ", "
                      + "ImageFileName   = '" + SOut.String(claimFormItem.ImageFileName) + "', "
                      + "FieldName       = '" + SOut.String(claimFormItem.FieldName) + "', "
                      + "FormatString    = '" + SOut.String(claimFormItem.FormatString) + "', "
                      + "XPos            =  " + SOut.Float(claimFormItem.XPos) + ", "
                      + "YPos            =  " + SOut.Float(claimFormItem.YPos) + ", "
                      + "Width           =  " + SOut.Float(claimFormItem.Width) + ", "
                      + "Height          =  " + SOut.Float(claimFormItem.Height) + " "
                      + "WHERE ClaimFormItemNum = " + SOut.Long(claimFormItem.ClaimFormItemNum);
        Db.NonQ(command);
    }
}