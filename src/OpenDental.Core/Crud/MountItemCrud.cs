using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class MountItemCrud
{
    public static List<MountItem> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MountItem> TableToList(DataTable table)
    {
        var retVal = new List<MountItem>();
        foreach (DataRow row in table.Rows)
        {
            var mountItem = new MountItem
            {
                MountItemNum = SIn.Long(row["MountItemNum"].ToString()),
                MountNum = SIn.Long(row["MountNum"].ToString()),
                Xpos = SIn.Int(row["Xpos"].ToString()),
                Ypos = SIn.Int(row["Ypos"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString()),
                Width = SIn.Int(row["Width"].ToString()),
                Height = SIn.Int(row["Height"].ToString()),
                RotateOnAcquire = SIn.Int(row["RotateOnAcquire"].ToString()),
                ToothNumbers = SIn.String(row["ToothNumbers"].ToString()),
                TextShowing = SIn.String(row["TextShowing"].ToString()),
                FontSize = SIn.Float(row["FontSize"].ToString())
            };
            retVal.Add(mountItem);
        }

        return retVal;
    }

    public static void Insert(MountItem mountItem)
    {
        var command = "INSERT INTO mountitem (";

        command += "MountNum,Xpos,Ypos,ItemOrder,Width,Height,RotateOnAcquire,ToothNumbers,TextShowing,FontSize) VALUES(";

        command +=
            SOut.Long(mountItem.MountNum) + ","
                                          + SOut.Int(mountItem.Xpos) + ","
                                          + SOut.Int(mountItem.Ypos) + ","
                                          + SOut.Int(mountItem.ItemOrder) + ","
                                          + SOut.Int(mountItem.Width) + ","
                                          + SOut.Int(mountItem.Height) + ","
                                          + SOut.Int(mountItem.RotateOnAcquire) + ","
                                          + "'" + SOut.String(mountItem.ToothNumbers) + "',"
                                          + DbHelper.ParamChar + "paramTextShowing,"
                                          + SOut.Float(mountItem.FontSize) + ")";
        if (mountItem.TextShowing == null) mountItem.TextShowing = "";
        var paramTextShowing = new OdSqlParameter("paramTextShowing", SOut.StringParam(mountItem.TextShowing));
        {
            mountItem.MountItemNum = Db.NonQ(command, true, "MountItemNum", "mountItem", paramTextShowing);
        }
    }

    public static void Update(MountItem mountItem)
    {
        var command = "UPDATE mountitem SET "
                      + "MountNum       =  " + SOut.Long(mountItem.MountNum) + ", "
                      + "Xpos           =  " + SOut.Int(mountItem.Xpos) + ", "
                      + "Ypos           =  " + SOut.Int(mountItem.Ypos) + ", "
                      + "ItemOrder      =  " + SOut.Int(mountItem.ItemOrder) + ", "
                      + "Width          =  " + SOut.Int(mountItem.Width) + ", "
                      + "Height         =  " + SOut.Int(mountItem.Height) + ", "
                      + "RotateOnAcquire=  " + SOut.Int(mountItem.RotateOnAcquire) + ", "
                      + "ToothNumbers   = '" + SOut.String(mountItem.ToothNumbers) + "', "
                      + "TextShowing    =  " + DbHelper.ParamChar + "paramTextShowing, "
                      + "FontSize       =  " + SOut.Float(mountItem.FontSize) + " "
                      + "WHERE MountItemNum = " + SOut.Long(mountItem.MountItemNum);
        if (mountItem.TextShowing == null) mountItem.TextShowing = "";
        var paramTextShowing = new OdSqlParameter("paramTextShowing", SOut.StringParam(mountItem.TextShowing));
        Db.NonQ(command, paramTextShowing);
    }
}