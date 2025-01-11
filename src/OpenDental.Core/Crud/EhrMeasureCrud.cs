#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrMeasureCrud
{
    public static EhrMeasure SelectOne(long ehrMeasureNum)
    {
        var command = "SELECT * FROM ehrmeasure "
                      + "WHERE EhrMeasureNum = " + SOut.Long(ehrMeasureNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrMeasure SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrMeasure> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrMeasure> TableToList(DataTable table)
    {
        var retVal = new List<EhrMeasure>();
        EhrMeasure ehrMeasure;
        foreach (DataRow row in table.Rows)
        {
            ehrMeasure = new EhrMeasure();
            ehrMeasure.EhrMeasureNum = SIn.Long(row["EhrMeasureNum"].ToString());
            ehrMeasure.MeasureType = (EhrMeasureType) SIn.Int(row["MeasureType"].ToString());
            ehrMeasure.Numerator = SIn.Int(row["Numerator"].ToString());
            ehrMeasure.Denominator = SIn.Int(row["Denominator"].ToString());
            retVal.Add(ehrMeasure);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrMeasure> listEhrMeasures, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrMeasure";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrMeasureNum");
        table.Columns.Add("MeasureType");
        table.Columns.Add("Numerator");
        table.Columns.Add("Denominator");
        foreach (var ehrMeasure in listEhrMeasures)
            table.Rows.Add(SOut.Long(ehrMeasure.EhrMeasureNum), SOut.Int((int) ehrMeasure.MeasureType), SOut.Int(ehrMeasure.Numerator), SOut.Int(ehrMeasure.Denominator));
        return table;
    }

    public static long Insert(EhrMeasure ehrMeasure)
    {
        return Insert(ehrMeasure, false);
    }

    public static long Insert(EhrMeasure ehrMeasure, bool useExistingPK)
    {
        var command = "INSERT INTO ehrmeasure (";

        command += "MeasureType,Numerator,Denominator) VALUES(";

        command +=
            SOut.Int((int) ehrMeasure.MeasureType) + ","
                                                   + SOut.Int(ehrMeasure.Numerator) + ","
                                                   + SOut.Int(ehrMeasure.Denominator) + ")";
        {
            ehrMeasure.EhrMeasureNum = Db.NonQ(command, true, "EhrMeasureNum", "ehrMeasure");
        }
        return ehrMeasure.EhrMeasureNum;
    }

    public static long InsertNoCache(EhrMeasure ehrMeasure)
    {
        return InsertNoCache(ehrMeasure, false);
    }

    public static long InsertNoCache(EhrMeasure ehrMeasure, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrmeasure (";
        if (isRandomKeys || useExistingPK) command += "EhrMeasureNum,";
        command += "MeasureType,Numerator,Denominator) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrMeasure.EhrMeasureNum) + ",";
        command +=
            SOut.Int((int) ehrMeasure.MeasureType) + ","
                                                   + SOut.Int(ehrMeasure.Numerator) + ","
                                                   + SOut.Int(ehrMeasure.Denominator) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            ehrMeasure.EhrMeasureNum = Db.NonQ(command, true, "EhrMeasureNum", "ehrMeasure");
        return ehrMeasure.EhrMeasureNum;
    }

    public static void Update(EhrMeasure ehrMeasure)
    {
        var command = "UPDATE ehrmeasure SET "
                      + "MeasureType  =  " + SOut.Int((int) ehrMeasure.MeasureType) + ", "
                      + "Numerator    =  " + SOut.Int(ehrMeasure.Numerator) + ", "
                      + "Denominator  =  " + SOut.Int(ehrMeasure.Denominator) + " "
                      + "WHERE EhrMeasureNum = " + SOut.Long(ehrMeasure.EhrMeasureNum);
        Db.NonQ(command);
    }

    public static bool Update(EhrMeasure ehrMeasure, EhrMeasure oldEhrMeasure)
    {
        var command = "";
        if (ehrMeasure.MeasureType != oldEhrMeasure.MeasureType)
        {
            if (command != "") command += ",";
            command += "MeasureType = " + SOut.Int((int) ehrMeasure.MeasureType) + "";
        }

        if (ehrMeasure.Numerator != oldEhrMeasure.Numerator)
        {
            if (command != "") command += ",";
            command += "Numerator = " + SOut.Int(ehrMeasure.Numerator) + "";
        }

        if (ehrMeasure.Denominator != oldEhrMeasure.Denominator)
        {
            if (command != "") command += ",";
            command += "Denominator = " + SOut.Int(ehrMeasure.Denominator) + "";
        }

        if (command == "") return false;
        command = "UPDATE ehrmeasure SET " + command
                                           + " WHERE EhrMeasureNum = " + SOut.Long(ehrMeasure.EhrMeasureNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EhrMeasure ehrMeasure, EhrMeasure oldEhrMeasure)
    {
        if (ehrMeasure.MeasureType != oldEhrMeasure.MeasureType) return true;
        if (ehrMeasure.Numerator != oldEhrMeasure.Numerator) return true;
        if (ehrMeasure.Denominator != oldEhrMeasure.Denominator) return true;
        return false;
    }

    public static void Delete(long ehrMeasureNum)
    {
        var command = "DELETE FROM ehrmeasure "
                      + "WHERE EhrMeasureNum = " + SOut.Long(ehrMeasureNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrMeasureNums)
    {
        if (listEhrMeasureNums == null || listEhrMeasureNums.Count == 0) return;
        var command = "DELETE FROM ehrmeasure "
                      + "WHERE EhrMeasureNum IN(" + string.Join(",", listEhrMeasureNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}