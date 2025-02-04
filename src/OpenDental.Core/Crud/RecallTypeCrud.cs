using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class RecallTypeCrud
{
    public static List<RecallType> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RecallType> TableToList(DataTable table)
    {
        var retVal = new List<RecallType>();
        foreach (DataRow row in table.Rows)
        {
            var recallType = new RecallType
            {
                RecallTypeNum = SIn.Long(row["RecallTypeNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                DefaultInterval = new Interval(SIn.Int(row["DefaultInterval"].ToString())),
                TimePattern = SIn.String(row["TimePattern"].ToString()),
                Procedures = SIn.String(row["Procedures"].ToString()),
                AppendToSpecial = SIn.Bool(row["AppendToSpecial"].ToString())
            };
            retVal.Add(recallType);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<RecallType> listRecallTypes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "RecallType";
        var table = new DataTable(tableName);
        table.Columns.Add("RecallTypeNum");
        table.Columns.Add("Description");
        table.Columns.Add("DefaultInterval");
        table.Columns.Add("TimePattern");
        table.Columns.Add("Procedures");
        table.Columns.Add("AppendToSpecial");
        foreach (var recallType in listRecallTypes)
            table.Rows.Add(SOut.Long(recallType.RecallTypeNum), recallType.Description, SOut.Int(recallType.DefaultInterval.ToInt()), recallType.TimePattern, recallType.Procedures, SOut.Bool(recallType.AppendToSpecial));
        return table;
    }

    public static void Insert(RecallType recallType)
    {
        var command = "INSERT INTO recalltype (";

        command += "Description,DefaultInterval,TimePattern,Procedures,AppendToSpecial) VALUES(";

        command +=
            "'" + SOut.String(recallType.Description) + "',"
            + SOut.Int(recallType.DefaultInterval.ToInt()) + ","
            + "'" + SOut.String(recallType.TimePattern) + "',"
            + "'" + SOut.String(recallType.Procedures) + "',"
            + SOut.Bool(recallType.AppendToSpecial) + ")";
        {
            recallType.RecallTypeNum = Db.NonQ(command, true, "RecallTypeNum", "recallType");
        }
    }

    public static void Update(RecallType recallType)
    {
        var command = "UPDATE recalltype SET "
                      + "Description    = '" + SOut.String(recallType.Description) + "', "
                      + "DefaultInterval=  " + SOut.Int(recallType.DefaultInterval.ToInt()) + ", "
                      + "TimePattern    = '" + SOut.String(recallType.TimePattern) + "', "
                      + "Procedures     = '" + SOut.String(recallType.Procedures) + "', "
                      + "AppendToSpecial=  " + SOut.Bool(recallType.AppendToSpecial) + " "
                      + "WHERE RecallTypeNum = " + SOut.Long(recallType.RecallTypeNum);
        Db.NonQ(command);
    }
}