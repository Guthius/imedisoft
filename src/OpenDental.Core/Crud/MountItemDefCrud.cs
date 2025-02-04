using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class MountItemDefCrud
{
    public static List<MountItemDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MountItemDef> TableToList(DataTable table)
    {
        var retVal = new List<MountItemDef>();
        foreach (DataRow row in table.Rows)
        {
            var mountItemDef = new MountItemDef
            {
                MountItemDefNum = SIn.Long(row["MountItemDefNum"].ToString()),
                MountDefNum = SIn.Long(row["MountDefNum"].ToString()),
                Xpos = SIn.Int(row["Xpos"].ToString()),
                Ypos = SIn.Int(row["Ypos"].ToString()),
                Width = SIn.Int(row["Width"].ToString()),
                Height = SIn.Int(row["Height"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString()),
                RotateOnAcquire = SIn.Int(row["RotateOnAcquire"].ToString()),
                ToothNumbers = SIn.String(row["ToothNumbers"].ToString()),
                TextShowing = SIn.String(row["TextShowing"].ToString()),
                FontSize = SIn.Float(row["FontSize"].ToString())
            };
            retVal.Add(mountItemDef);
        }

        return retVal;
    }

    public static void Insert(MountItemDef mountItemDef)
    {
        var command = "INSERT INTO mountitemdef (";

        command += "MountDefNum,Xpos,Ypos,Width,Height,ItemOrder,RotateOnAcquire,ToothNumbers,TextShowing,FontSize) VALUES(";

        command +=
            SOut.Long(mountItemDef.MountDefNum) + ","
                                                + SOut.Int(mountItemDef.Xpos) + ","
                                                + SOut.Int(mountItemDef.Ypos) + ","
                                                + SOut.Int(mountItemDef.Width) + ","
                                                + SOut.Int(mountItemDef.Height) + ","
                                                + SOut.Int(mountItemDef.ItemOrder) + ","
                                                + SOut.Int(mountItemDef.RotateOnAcquire) + ","
                                                + "'" + SOut.String(mountItemDef.ToothNumbers) + "',"
                                                + DbHelper.ParamChar + "paramTextShowing,"
                                                + SOut.Float(mountItemDef.FontSize) + ")";
        if (mountItemDef.TextShowing == null) mountItemDef.TextShowing = "";
        var paramTextShowing = new OdSqlParameter("paramTextShowing", SOut.StringParam(mountItemDef.TextShowing));
        {
            mountItemDef.MountItemDefNum = Db.NonQ(command, true, "MountItemDefNum", "mountItemDef", paramTextShowing);
        }
    }

    public static void Update(MountItemDef mountItemDef)
    {
        var command = "UPDATE mountitemdef SET "
                      + "MountDefNum    =  " + SOut.Long(mountItemDef.MountDefNum) + ", "
                      + "Xpos           =  " + SOut.Int(mountItemDef.Xpos) + ", "
                      + "Ypos           =  " + SOut.Int(mountItemDef.Ypos) + ", "
                      + "Width          =  " + SOut.Int(mountItemDef.Width) + ", "
                      + "Height         =  " + SOut.Int(mountItemDef.Height) + ", "
                      + "ItemOrder      =  " + SOut.Int(mountItemDef.ItemOrder) + ", "
                      + "RotateOnAcquire=  " + SOut.Int(mountItemDef.RotateOnAcquire) + ", "
                      + "ToothNumbers   = '" + SOut.String(mountItemDef.ToothNumbers) + "', "
                      + "TextShowing    =  " + DbHelper.ParamChar + "paramTextShowing, "
                      + "FontSize       =  " + SOut.Float(mountItemDef.FontSize) + " "
                      + "WHERE MountItemDefNum = " + SOut.Long(mountItemDef.MountItemDefNum);
        if (mountItemDef.TextShowing == null) mountItemDef.TextShowing = "";
        var paramTextShowing = new OdSqlParameter("paramTextShowing", SOut.StringParam(mountItemDef.TextShowing));
        Db.NonQ(command, paramTextShowing);
    }
}