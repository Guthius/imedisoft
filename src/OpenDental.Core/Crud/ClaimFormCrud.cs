using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ClaimFormCrud
{
    public static List<ClaimForm> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ClaimForm> TableToList(DataTable table)
    {
        var retVal = new List<ClaimForm>();
        ClaimForm claimForm;
        foreach (DataRow row in table.Rows)
        {
            claimForm = new ClaimForm();
            claimForm.ClaimFormNum = SIn.Long(row["ClaimFormNum"].ToString());
            claimForm.Description = SIn.String(row["Description"].ToString());
            claimForm.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            claimForm.FontName = SIn.String(row["FontName"].ToString());
            claimForm.FontSize = SIn.Float(row["FontSize"].ToString());
            claimForm.UniqueID = SIn.String(row["UniqueID"].ToString());
            claimForm.PrintImages = SIn.Bool(row["PrintImages"].ToString());
            claimForm.OffsetX = SIn.Int(row["OffsetX"].ToString());
            claimForm.OffsetY = SIn.Int(row["OffsetY"].ToString());
            claimForm.Width = SIn.Int(row["Width"].ToString());
            claimForm.Height = SIn.Int(row["Height"].ToString());
            retVal.Add(claimForm);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ClaimForm> listClaimForms, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ClaimForm";
        var table = new DataTable(tableName);
        table.Columns.Add("ClaimFormNum");
        table.Columns.Add("Description");
        table.Columns.Add("IsHidden");
        table.Columns.Add("FontName");
        table.Columns.Add("FontSize");
        table.Columns.Add("UniqueID");
        table.Columns.Add("PrintImages");
        table.Columns.Add("OffsetX");
        table.Columns.Add("OffsetY");
        table.Columns.Add("Width");
        table.Columns.Add("Height");
        foreach (var claimForm in listClaimForms)
            table.Rows.Add(SOut.Long(claimForm.ClaimFormNum), claimForm.Description, SOut.Bool(claimForm.IsHidden), claimForm.FontName, SOut.Float(claimForm.FontSize), claimForm.UniqueID, SOut.Bool(claimForm.PrintImages), SOut.Int(claimForm.OffsetX), SOut.Int(claimForm.OffsetY), SOut.Int(claimForm.Width), SOut.Int(claimForm.Height));
        return table;
    }

    public static long Insert(ClaimForm claimForm)
    {
        var command = "INSERT INTO claimform (";

        command += "Description,IsHidden,FontName,FontSize,UniqueID,PrintImages,OffsetX,OffsetY,Width,Height) VALUES(";

        command +=
            "'" + SOut.String(claimForm.Description) + "',"
            + SOut.Bool(claimForm.IsHidden) + ","
            + "'" + SOut.String(claimForm.FontName) + "',"
            + SOut.Float(claimForm.FontSize) + ","
            + "'" + SOut.String(claimForm.UniqueID) + "',"
            + SOut.Bool(claimForm.PrintImages) + ","
            + SOut.Int(claimForm.OffsetX) + ","
            + SOut.Int(claimForm.OffsetY) + ","
            + SOut.Int(claimForm.Width) + ","
            + SOut.Int(claimForm.Height) + ")";
        {
            claimForm.ClaimFormNum = Db.NonQ(command, true, "ClaimFormNum", "claimForm");
        }
        return claimForm.ClaimFormNum;
    }

    public static void Update(ClaimForm claimForm)
    {
        var command = "UPDATE claimform SET "
                      + "Description = '" + SOut.String(claimForm.Description) + "', "
                      + "IsHidden    =  " + SOut.Bool(claimForm.IsHidden) + ", "
                      + "FontName    = '" + SOut.String(claimForm.FontName) + "', "
                      + "FontSize    =  " + SOut.Float(claimForm.FontSize) + ", "
                      + "UniqueID    = '" + SOut.String(claimForm.UniqueID) + "', "
                      + "PrintImages =  " + SOut.Bool(claimForm.PrintImages) + ", "
                      + "OffsetX     =  " + SOut.Int(claimForm.OffsetX) + ", "
                      + "OffsetY     =  " + SOut.Int(claimForm.OffsetY) + ", "
                      + "Width       =  " + SOut.Int(claimForm.Width) + ", "
                      + "Height      =  " + SOut.Int(claimForm.Height) + " "
                      + "WHERE ClaimFormNum = " + SOut.Long(claimForm.ClaimFormNum);
        Db.NonQ(command);
    }
}