using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class FeeSchedCrud
{
    public static List<FeeSched> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<FeeSched> TableToList(DataTable table)
    {
        var retVal = new List<FeeSched>();
        foreach (DataRow row in table.Rows)
        {
            var feeSched = new FeeSched
            {
                FeeSchedNum = SIn.Long(row["FeeSchedNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                FeeSchedType = (FeeScheduleType) SIn.Int(row["FeeSchedType"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString()),
                IsHidden = SIn.Bool(row["IsHidden"].ToString()),
                IsGlobal = SIn.Bool(row["IsGlobal"].ToString()),
                SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString()),
                SecDateEntry = SIn.Date(row["SecDateEntry"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString())
            };
            retVal.Add(feeSched);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<FeeSched> listFeeScheds, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "FeeSched";
        var table = new DataTable(tableName);
        table.Columns.Add("FeeSchedNum");
        table.Columns.Add("Description");
        table.Columns.Add("FeeSchedType");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("IsHidden");
        table.Columns.Add("IsGlobal");
        table.Columns.Add("SecUserNumEntry");
        table.Columns.Add("SecDateEntry");
        table.Columns.Add("SecDateTEdit");
        foreach (var feeSched in listFeeScheds)
            table.Rows.Add(SOut.Long(feeSched.FeeSchedNum), feeSched.Description, SOut.Int((int) feeSched.FeeSchedType), SOut.Int(feeSched.ItemOrder), SOut.Bool(feeSched.IsHidden), SOut.Bool(feeSched.IsGlobal), SOut.Long(feeSched.SecUserNumEntry), SOut.DateTime(feeSched.SecDateEntry, false), SOut.DateTime(feeSched.SecDateTEdit, false));
        return table;
    }

    public static void Insert(FeeSched feeSched)
    {
        var command = "INSERT INTO feesched (";

        command += "Description,FeeSchedType,ItemOrder,IsHidden,IsGlobal,SecUserNumEntry,SecDateEntry) VALUES(";

        command +=
            "'" + SOut.String(feeSched.Description) + "',"
            + SOut.Int((int) feeSched.FeeSchedType) + ","
            + SOut.Int(feeSched.ItemOrder) + ","
            + SOut.Bool(feeSched.IsHidden) + ","
            + SOut.Bool(feeSched.IsGlobal) + ","
            + SOut.Long(feeSched.SecUserNumEntry) + ","
            + "NOW()" + ")";
        //SecDateTEdit can only be set by MySQL

        feeSched.FeeSchedNum = Db.NonQ(command, true, "FeeSchedNum", "feeSched");
    }

    public static void Update(FeeSched feeSched)
    {
        var command = "UPDATE feesched SET "
                      + "Description    = '" + SOut.String(feeSched.Description) + "', "
                      + "FeeSchedType   =  " + SOut.Int((int) feeSched.FeeSchedType) + ", "
                      + "ItemOrder      =  " + SOut.Int(feeSched.ItemOrder) + ", "
                      + "IsHidden       =  " + SOut.Bool(feeSched.IsHidden) + ", "
                      + "IsGlobal       =  " + SOut.Bool(feeSched.IsGlobal) + " "
                      //SecUserNumEntry excluded from update
                      //SecDateEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "WHERE FeeSchedNum = " + SOut.Long(feeSched.FeeSchedNum);
        Db.NonQ(command);
    }
}