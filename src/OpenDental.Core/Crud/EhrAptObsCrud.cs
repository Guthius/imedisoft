#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrAptObsCrud
{
    public static EhrAptObs SelectOne(long ehrAptObsNum)
    {
        var command = "SELECT * FROM ehraptobs "
                      + "WHERE EhrAptObsNum = " + SOut.Long(ehrAptObsNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrAptObs SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrAptObs> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrAptObs> TableToList(DataTable table)
    {
        var retVal = new List<EhrAptObs>();
        EhrAptObs ehrAptObs;
        foreach (DataRow row in table.Rows)
        {
            ehrAptObs = new EhrAptObs();
            ehrAptObs.EhrAptObsNum = SIn.Long(row["EhrAptObsNum"].ToString());
            ehrAptObs.AptNum = SIn.Long(row["AptNum"].ToString());
            ehrAptObs.IdentifyingCode = (EhrAptObsIdentifier) SIn.Int(row["IdentifyingCode"].ToString());
            ehrAptObs.ValType = (EhrAptObsType) SIn.Int(row["ValType"].ToString());
            ehrAptObs.ValReported = SIn.String(row["ValReported"].ToString());
            ehrAptObs.UcumCode = SIn.String(row["UcumCode"].ToString());
            ehrAptObs.ValCodeSystem = SIn.String(row["ValCodeSystem"].ToString());
            retVal.Add(ehrAptObs);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrAptObs> listEhrAptObss, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrAptObs";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrAptObsNum");
        table.Columns.Add("AptNum");
        table.Columns.Add("IdentifyingCode");
        table.Columns.Add("ValType");
        table.Columns.Add("ValReported");
        table.Columns.Add("UcumCode");
        table.Columns.Add("ValCodeSystem");
        foreach (var ehrAptObs in listEhrAptObss)
            table.Rows.Add(SOut.Long(ehrAptObs.EhrAptObsNum), SOut.Long(ehrAptObs.AptNum), SOut.Int((int) ehrAptObs.IdentifyingCode), SOut.Int((int) ehrAptObs.ValType), ehrAptObs.ValReported, ehrAptObs.UcumCode, ehrAptObs.ValCodeSystem);
        return table;
    }

    public static long Insert(EhrAptObs ehrAptObs)
    {
        return Insert(ehrAptObs, false);
    }

    public static long Insert(EhrAptObs ehrAptObs, bool useExistingPK)
    {
        var command = "INSERT INTO ehraptobs (";

        command += "AptNum,IdentifyingCode,ValType,ValReported,UcumCode,ValCodeSystem) VALUES(";

        command +=
            SOut.Long(ehrAptObs.AptNum) + ","
                                        + SOut.Int((int) ehrAptObs.IdentifyingCode) + ","
                                        + SOut.Int((int) ehrAptObs.ValType) + ","
                                        + "'" + SOut.String(ehrAptObs.ValReported) + "',"
                                        + "'" + SOut.String(ehrAptObs.UcumCode) + "',"
                                        + "'" + SOut.String(ehrAptObs.ValCodeSystem) + "')";
        {
            ehrAptObs.EhrAptObsNum = Db.NonQ(command, true, "EhrAptObsNum", "ehrAptObs");
        }
        return ehrAptObs.EhrAptObsNum;
    }

    public static long InsertNoCache(EhrAptObs ehrAptObs)
    {
        return InsertNoCache(ehrAptObs, false);
    }

    public static long InsertNoCache(EhrAptObs ehrAptObs, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehraptobs (";
        if (isRandomKeys || useExistingPK) command += "EhrAptObsNum,";
        command += "AptNum,IdentifyingCode,ValType,ValReported,UcumCode,ValCodeSystem) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrAptObs.EhrAptObsNum) + ",";
        command +=
            SOut.Long(ehrAptObs.AptNum) + ","
                                        + SOut.Int((int) ehrAptObs.IdentifyingCode) + ","
                                        + SOut.Int((int) ehrAptObs.ValType) + ","
                                        + "'" + SOut.String(ehrAptObs.ValReported) + "',"
                                        + "'" + SOut.String(ehrAptObs.UcumCode) + "',"
                                        + "'" + SOut.String(ehrAptObs.ValCodeSystem) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            ehrAptObs.EhrAptObsNum = Db.NonQ(command, true, "EhrAptObsNum", "ehrAptObs");
        return ehrAptObs.EhrAptObsNum;
    }

    public static void Update(EhrAptObs ehrAptObs)
    {
        var command = "UPDATE ehraptobs SET "
                      + "AptNum         =  " + SOut.Long(ehrAptObs.AptNum) + ", "
                      + "IdentifyingCode=  " + SOut.Int((int) ehrAptObs.IdentifyingCode) + ", "
                      + "ValType        =  " + SOut.Int((int) ehrAptObs.ValType) + ", "
                      + "ValReported    = '" + SOut.String(ehrAptObs.ValReported) + "', "
                      + "UcumCode       = '" + SOut.String(ehrAptObs.UcumCode) + "', "
                      + "ValCodeSystem  = '" + SOut.String(ehrAptObs.ValCodeSystem) + "' "
                      + "WHERE EhrAptObsNum = " + SOut.Long(ehrAptObs.EhrAptObsNum);
        Db.NonQ(command);
    }

    public static bool Update(EhrAptObs ehrAptObs, EhrAptObs oldEhrAptObs)
    {
        var command = "";
        if (ehrAptObs.AptNum != oldEhrAptObs.AptNum)
        {
            if (command != "") command += ",";
            command += "AptNum = " + SOut.Long(ehrAptObs.AptNum) + "";
        }

        if (ehrAptObs.IdentifyingCode != oldEhrAptObs.IdentifyingCode)
        {
            if (command != "") command += ",";
            command += "IdentifyingCode = " + SOut.Int((int) ehrAptObs.IdentifyingCode) + "";
        }

        if (ehrAptObs.ValType != oldEhrAptObs.ValType)
        {
            if (command != "") command += ",";
            command += "ValType = " + SOut.Int((int) ehrAptObs.ValType) + "";
        }

        if (ehrAptObs.ValReported != oldEhrAptObs.ValReported)
        {
            if (command != "") command += ",";
            command += "ValReported = '" + SOut.String(ehrAptObs.ValReported) + "'";
        }

        if (ehrAptObs.UcumCode != oldEhrAptObs.UcumCode)
        {
            if (command != "") command += ",";
            command += "UcumCode = '" + SOut.String(ehrAptObs.UcumCode) + "'";
        }

        if (ehrAptObs.ValCodeSystem != oldEhrAptObs.ValCodeSystem)
        {
            if (command != "") command += ",";
            command += "ValCodeSystem = '" + SOut.String(ehrAptObs.ValCodeSystem) + "'";
        }

        if (command == "") return false;
        command = "UPDATE ehraptobs SET " + command
                                          + " WHERE EhrAptObsNum = " + SOut.Long(ehrAptObs.EhrAptObsNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EhrAptObs ehrAptObs, EhrAptObs oldEhrAptObs)
    {
        if (ehrAptObs.AptNum != oldEhrAptObs.AptNum) return true;
        if (ehrAptObs.IdentifyingCode != oldEhrAptObs.IdentifyingCode) return true;
        if (ehrAptObs.ValType != oldEhrAptObs.ValType) return true;
        if (ehrAptObs.ValReported != oldEhrAptObs.ValReported) return true;
        if (ehrAptObs.UcumCode != oldEhrAptObs.UcumCode) return true;
        if (ehrAptObs.ValCodeSystem != oldEhrAptObs.ValCodeSystem) return true;
        return false;
    }

    public static void Delete(long ehrAptObsNum)
    {
        var command = "DELETE FROM ehraptobs "
                      + "WHERE EhrAptObsNum = " + SOut.Long(ehrAptObsNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrAptObsNums)
    {
        if (listEhrAptObsNums == null || listEhrAptObsNums.Count == 0) return;
        var command = "DELETE FROM ehraptobs "
                      + "WHERE EhrAptObsNum IN(" + string.Join(",", listEhrAptObsNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}