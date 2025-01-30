using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ERoutingDefCrud
{
    public static List<ERoutingDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ERoutingDef> TableToList(DataTable table)
    {
        var retVal = new List<ERoutingDef>();
        ERoutingDef eRoutingDef;
        foreach (DataRow row in table.Rows)
        {
            eRoutingDef = new ERoutingDef();
            eRoutingDef.ERoutingDefNum = SIn.Long(row["ERoutingDefNum"].ToString());
            eRoutingDef.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            eRoutingDef.Description = SIn.String(row["Description"].ToString());
            eRoutingDef.UserNumCreated = SIn.Long(row["UserNumCreated"].ToString());
            eRoutingDef.UserNumModified = SIn.Long(row["UserNumModified"].ToString());
            eRoutingDef.SecDateTEntered = SIn.DateTime(row["SecDateTEntered"].ToString());
            eRoutingDef.DateLastModified = SIn.DateTime(row["DateLastModified"].ToString());
            retVal.Add(eRoutingDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ERoutingDef> listERoutingDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ERoutingDef";
        var table = new DataTable(tableName);
        table.Columns.Add("ERoutingDefNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("Description");
        table.Columns.Add("UserNumCreated");
        table.Columns.Add("UserNumModified");
        table.Columns.Add("SecDateTEntered");
        table.Columns.Add("DateLastModified");
        foreach (var eRoutingDef in listERoutingDefs)
            table.Rows.Add(SOut.Long(eRoutingDef.ERoutingDefNum), SOut.Long(eRoutingDef.ClinicNum), eRoutingDef.Description, SOut.Long(eRoutingDef.UserNumCreated), SOut.Long(eRoutingDef.UserNumModified), SOut.DateTime(eRoutingDef.SecDateTEntered, false), SOut.DateTime(eRoutingDef.DateLastModified, false));
        return table;
    }

    public static long Insert(ERoutingDef eRoutingDef)
    {
        var command = "INSERT INTO eroutingdef (";

        command += "ClinicNum,Description,UserNumCreated,UserNumModified,SecDateTEntered,DateLastModified) VALUES(";

        command +=
            SOut.Long(eRoutingDef.ClinicNum) + ","
                                             + "'" + SOut.String(eRoutingDef.Description) + "',"
                                             + SOut.Long(eRoutingDef.UserNumCreated) + ","
                                             + SOut.Long(eRoutingDef.UserNumModified) + ","
                                             + "NOW()" + ","
                                             + SOut.DateTime(eRoutingDef.DateLastModified) + ")";
        {
            eRoutingDef.ERoutingDefNum = Db.NonQ(command, true, "ERoutingDefNum", "eRoutingDef");
        }
        return eRoutingDef.ERoutingDefNum;
    }

    public static void Update(ERoutingDef eRoutingDef)
    {
        var command = "UPDATE eroutingdef SET "
                      + "ClinicNum       =  " + SOut.Long(eRoutingDef.ClinicNum) + ", "
                      + "Description     = '" + SOut.String(eRoutingDef.Description) + "', "
                      + "UserNumCreated  =  " + SOut.Long(eRoutingDef.UserNumCreated) + ", "
                      + "UserNumModified =  " + SOut.Long(eRoutingDef.UserNumModified) + ", "
                      //SecDateTEntered not allowed to change
                      + "DateLastModified=  " + SOut.DateTime(eRoutingDef.DateLastModified) + " "
                      + "WHERE ERoutingDefNum = " + SOut.Long(eRoutingDef.ERoutingDefNum);
        Db.NonQ(command);
    }

    public static void Delete(long eRoutingDefNum)
    {
        var command = "DELETE FROM eroutingdef "
                      + "WHERE ERoutingDefNum = " + SOut.Long(eRoutingDefNum);
        Db.NonQ(command);
    }
}