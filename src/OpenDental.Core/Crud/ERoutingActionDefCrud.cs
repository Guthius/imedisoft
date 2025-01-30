using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ERoutingActionDefCrud
{
    public static List<ERoutingActionDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ERoutingActionDef> TableToList(DataTable table)
    {
        var retVal = new List<ERoutingActionDef>();
        ERoutingActionDef eRoutingActionDef;
        foreach (DataRow row in table.Rows)
        {
            eRoutingActionDef = new ERoutingActionDef();
            eRoutingActionDef.ERoutingActionDefNum = SIn.Long(row["ERoutingActionDefNum"].ToString());
            eRoutingActionDef.ERoutingDefNum = SIn.Long(row["ERoutingDefNum"].ToString());
            eRoutingActionDef.ERoutingActionType = (EnumERoutingActionType) SIn.Int(row["ERoutingActionType"].ToString());
            eRoutingActionDef.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            eRoutingActionDef.SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString());
            eRoutingActionDef.DateTLastModified = SIn.DateTime(row["DateTLastModified"].ToString());
            eRoutingActionDef.ForeignKey = SIn.Long(row["ForeignKey"].ToString());
            eRoutingActionDef.ForeignKeyType = (EnumERoutingDefFKType) SIn.Int(row["ForeignKeyType"].ToString());
            eRoutingActionDef.LabelOverride = SIn.String(row["LabelOverride"].ToString());
            retVal.Add(eRoutingActionDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ERoutingActionDef> listERoutingActionDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ERoutingActionDef";
        var table = new DataTable(tableName);
        table.Columns.Add("ERoutingActionDefNum");
        table.Columns.Add("ERoutingDefNum");
        table.Columns.Add("ERoutingActionType");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("SecDateTEntry");
        table.Columns.Add("DateTLastModified");
        table.Columns.Add("ForeignKey");
        table.Columns.Add("ForeignKeyType");
        table.Columns.Add("LabelOverride");
        foreach (var eRoutingActionDef in listERoutingActionDefs)
            table.Rows.Add(SOut.Long(eRoutingActionDef.ERoutingActionDefNum), SOut.Long(eRoutingActionDef.ERoutingDefNum), SOut.Int((int) eRoutingActionDef.ERoutingActionType), SOut.Int(eRoutingActionDef.ItemOrder), SOut.DateTime(eRoutingActionDef.SecDateTEntry, false), SOut.DateTime(eRoutingActionDef.DateTLastModified, false), SOut.Long(eRoutingActionDef.ForeignKey), SOut.Int((int) eRoutingActionDef.ForeignKeyType), eRoutingActionDef.LabelOverride);
        return table;
    }

    public static void Insert(ERoutingActionDef eRoutingActionDef)
    {
        var command = "INSERT INTO eroutingactiondef (";

        command += "ERoutingDefNum,ERoutingActionType,ItemOrder,SecDateTEntry,DateTLastModified,ForeignKey,ForeignKeyType,LabelOverride) VALUES(";

        command +=
            SOut.Long(eRoutingActionDef.ERoutingDefNum) + ","
                                                        + SOut.Int((int) eRoutingActionDef.ERoutingActionType) + ","
                                                        + SOut.Int(eRoutingActionDef.ItemOrder) + ","
                                                        + "NOW()" + ","
                                                        + SOut.DateTime(eRoutingActionDef.DateTLastModified) + ","
                                                        + SOut.Long(eRoutingActionDef.ForeignKey) + ","
                                                        + SOut.Int((int) eRoutingActionDef.ForeignKeyType) + ","
                                                        + "'" + SOut.String(eRoutingActionDef.LabelOverride) + "')";
        {
            eRoutingActionDef.ERoutingActionDefNum = Db.NonQ(command, true, "ERoutingActionDefNum", "eRoutingActionDef");
        }
    }

    public static void Update(ERoutingActionDef eRoutingActionDef)
    {
        var command = "UPDATE eroutingactiondef SET "
                      + "ERoutingDefNum      =  " + SOut.Long(eRoutingActionDef.ERoutingDefNum) + ", "
                      + "ERoutingActionType  =  " + SOut.Int((int) eRoutingActionDef.ERoutingActionType) + ", "
                      + "ItemOrder           =  " + SOut.Int(eRoutingActionDef.ItemOrder) + ", "
                      //SecDateTEntry not allowed to change
                      + "DateTLastModified   =  " + SOut.DateTime(eRoutingActionDef.DateTLastModified) + ", "
                      + "ForeignKey          =  " + SOut.Long(eRoutingActionDef.ForeignKey) + ", "
                      + "ForeignKeyType      =  " + SOut.Int((int) eRoutingActionDef.ForeignKeyType) + ", "
                      + "LabelOverride       = '" + SOut.String(eRoutingActionDef.LabelOverride) + "' "
                      + "WHERE ERoutingActionDefNum = " + SOut.Long(eRoutingActionDef.ERoutingActionDefNum);
        Db.NonQ(command);
    }

    public static void Delete(long eRoutingActionDefNum)
    {
        var command = "DELETE FROM eroutingactiondef "
                      + "WHERE ERoutingActionDefNum = " + SOut.Long(eRoutingActionDefNum);
        Db.NonQ(command);
    }
}