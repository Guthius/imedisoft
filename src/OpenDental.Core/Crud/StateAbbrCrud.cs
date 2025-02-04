using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class StateAbbrCrud
{
    public static List<StateAbbr> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<StateAbbr> TableToList(DataTable table)
    {
        var retVal = new List<StateAbbr>();
        foreach (DataRow row in table.Rows)
        {
            var stateAbbr = new StateAbbr
            {
                StateAbbrNum = SIn.Long(row["StateAbbrNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                Abbr = SIn.String(row["Abbr"].ToString()),
                MedicaidIDLength = SIn.Int(row["MedicaidIDLength"].ToString())
            };
            retVal.Add(stateAbbr);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<StateAbbr> listStateAbbrs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "StateAbbr";
        var table = new DataTable(tableName);
        table.Columns.Add("StateAbbrNum");
        table.Columns.Add("Description");
        table.Columns.Add("Abbr");
        table.Columns.Add("MedicaidIDLength");
        foreach (var stateAbbr in listStateAbbrs)
            table.Rows.Add(SOut.Long(stateAbbr.StateAbbrNum), stateAbbr.Description, stateAbbr.Abbr, SOut.Int(stateAbbr.MedicaidIDLength));
        return table;
    }

    public static void Insert(StateAbbr stateAbbr)
    {
        var command = "INSERT INTO stateabbr (";

        command += "Description,Abbr,MedicaidIDLength) VALUES(";

        command +=
            "'" + SOut.String(stateAbbr.Description) + "',"
            + "'" + SOut.String(stateAbbr.Abbr) + "',"
            + SOut.Int(stateAbbr.MedicaidIDLength) + ")";
        {
            stateAbbr.StateAbbrNum = Db.NonQ(command, true, "StateAbbrNum", "stateAbbr");
        }
    }

    public static void Update(StateAbbr stateAbbr)
    {
        var command = "UPDATE stateabbr SET "
                      + "Description     = '" + SOut.String(stateAbbr.Description) + "', "
                      + "Abbr            = '" + SOut.String(stateAbbr.Abbr) + "', "
                      + "MedicaidIDLength=  " + SOut.Int(stateAbbr.MedicaidIDLength) + " "
                      + "WHERE StateAbbrNum = " + SOut.Long(stateAbbr.StateAbbrNum);
        Db.NonQ(command);
    }

    public static void Delete(long stateAbbrNum)
    {
        var command = "DELETE FROM stateabbr "
                      + "WHERE StateAbbrNum = " + SOut.Long(stateAbbrNum);
        Db.NonQ(command);
    }
}