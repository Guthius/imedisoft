using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ProcButtonQuickCrud
{
    public static List<ProcButtonQuick> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcButtonQuick> TableToList(DataTable table)
    {
        var retVal = new List<ProcButtonQuick>();
        foreach (DataRow row in table.Rows)
        {
            var procButtonQuick = new ProcButtonQuick
            {
                ProcButtonQuickNum = SIn.Long(row["ProcButtonQuickNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                CodeValue = SIn.String(row["CodeValue"].ToString()),
                Surf = SIn.String(row["Surf"].ToString()),
                YPos = SIn.Int(row["YPos"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString()),
                IsLabel = SIn.Bool(row["IsLabel"].ToString())
            };
            retVal.Add(procButtonQuick);
        }

        return retVal;
    }

    public static void Insert(ProcButtonQuick procButtonQuick)
    {
        var command = "INSERT INTO procbuttonquick (";

        command += "Description,CodeValue,Surf,YPos,ItemOrder,IsLabel) VALUES(";

        command +=
            "'" + SOut.String(procButtonQuick.Description) + "',"
            + "'" + SOut.String(procButtonQuick.CodeValue) + "',"
            + "'" + SOut.String(procButtonQuick.Surf) + "',"
            + SOut.Int(procButtonQuick.YPos) + ","
            + SOut.Int(procButtonQuick.ItemOrder) + ","
            + SOut.Bool(procButtonQuick.IsLabel) + ")";
        {
            procButtonQuick.ProcButtonQuickNum = Db.NonQ(command, true, "ProcButtonQuickNum", "procButtonQuick");
        }
    }

    public static void Update(ProcButtonQuick procButtonQuick)
    {
        var command = "UPDATE procbuttonquick SET "
                      + "Description       = '" + SOut.String(procButtonQuick.Description) + "', "
                      + "CodeValue         = '" + SOut.String(procButtonQuick.CodeValue) + "', "
                      + "Surf              = '" + SOut.String(procButtonQuick.Surf) + "', "
                      + "YPos              =  " + SOut.Int(procButtonQuick.YPos) + ", "
                      + "ItemOrder         =  " + SOut.Int(procButtonQuick.ItemOrder) + ", "
                      + "IsLabel           =  " + SOut.Bool(procButtonQuick.IsLabel) + " "
                      + "WHERE ProcButtonQuickNum = " + SOut.Long(procButtonQuick.ProcButtonQuickNum);
        Db.NonQ(command);
    }
}