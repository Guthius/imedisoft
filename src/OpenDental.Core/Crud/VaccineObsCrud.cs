#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class VaccineObsCrud
{
    public static VaccineObs SelectOne(long vaccineObsNum)
    {
        var command = "SELECT * FROM vaccineobs "
                      + "WHERE VaccineObsNum = " + SOut.Long(vaccineObsNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static VaccineObs SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<VaccineObs> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<VaccineObs> TableToList(DataTable table)
    {
        var retVal = new List<VaccineObs>();
        VaccineObs vaccineObs;
        foreach (DataRow row in table.Rows)
        {
            vaccineObs = new VaccineObs();
            vaccineObs.VaccineObsNum = SIn.Long(row["VaccineObsNum"].ToString());
            vaccineObs.VaccinePatNum = SIn.Long(row["VaccinePatNum"].ToString());
            vaccineObs.ValType = (VaccineObsType) SIn.Int(row["ValType"].ToString());
            vaccineObs.IdentifyingCode = (VaccineObsIdentifier) SIn.Int(row["IdentifyingCode"].ToString());
            vaccineObs.ValReported = SIn.String(row["ValReported"].ToString());
            vaccineObs.ValCodeSystem = (VaccineObsValCodeSystem) SIn.Int(row["ValCodeSystem"].ToString());
            vaccineObs.VaccineObsNumGroup = SIn.Long(row["VaccineObsNumGroup"].ToString());
            vaccineObs.UcumCode = SIn.String(row["UcumCode"].ToString());
            vaccineObs.DateObs = SIn.Date(row["DateObs"].ToString());
            vaccineObs.MethodCode = SIn.String(row["MethodCode"].ToString());
            retVal.Add(vaccineObs);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<VaccineObs> listVaccineObss, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "VaccineObs";
        var table = new DataTable(tableName);
        table.Columns.Add("VaccineObsNum");
        table.Columns.Add("VaccinePatNum");
        table.Columns.Add("ValType");
        table.Columns.Add("IdentifyingCode");
        table.Columns.Add("ValReported");
        table.Columns.Add("ValCodeSystem");
        table.Columns.Add("VaccineObsNumGroup");
        table.Columns.Add("UcumCode");
        table.Columns.Add("DateObs");
        table.Columns.Add("MethodCode");
        foreach (var vaccineObs in listVaccineObss)
            table.Rows.Add(SOut.Long(vaccineObs.VaccineObsNum), SOut.Long(vaccineObs.VaccinePatNum), SOut.Int((int) vaccineObs.ValType), SOut.Int((int) vaccineObs.IdentifyingCode), vaccineObs.ValReported, SOut.Int((int) vaccineObs.ValCodeSystem), SOut.Long(vaccineObs.VaccineObsNumGroup), vaccineObs.UcumCode, SOut.DateT(vaccineObs.DateObs, false), vaccineObs.MethodCode);
        return table;
    }

    public static long Insert(VaccineObs vaccineObs)
    {
        return Insert(vaccineObs, false);
    }

    public static long Insert(VaccineObs vaccineObs, bool useExistingPK)
    {
        var command = "INSERT INTO vaccineobs (";

        command += "VaccinePatNum,ValType,IdentifyingCode,ValReported,ValCodeSystem,VaccineObsNumGroup,UcumCode,DateObs,MethodCode) VALUES(";

        command +=
            SOut.Long(vaccineObs.VaccinePatNum) + ","
                                                + SOut.Int((int) vaccineObs.ValType) + ","
                                                + SOut.Int((int) vaccineObs.IdentifyingCode) + ","
                                                + "'" + SOut.String(vaccineObs.ValReported) + "',"
                                                + SOut.Int((int) vaccineObs.ValCodeSystem) + ","
                                                + SOut.Long(vaccineObs.VaccineObsNumGroup) + ","
                                                + "'" + SOut.String(vaccineObs.UcumCode) + "',"
                                                + SOut.Date(vaccineObs.DateObs) + ","
                                                + "'" + SOut.String(vaccineObs.MethodCode) + "')";
        {
            vaccineObs.VaccineObsNum = Db.NonQ(command, true, "VaccineObsNum", "vaccineObs");
        }
        return vaccineObs.VaccineObsNum;
    }

    public static long InsertNoCache(VaccineObs vaccineObs)
    {
        return InsertNoCache(vaccineObs, false);
    }

    public static long InsertNoCache(VaccineObs vaccineObs, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO vaccineobs (";
        if (isRandomKeys || useExistingPK) command += "VaccineObsNum,";
        command += "VaccinePatNum,ValType,IdentifyingCode,ValReported,ValCodeSystem,VaccineObsNumGroup,UcumCode,DateObs,MethodCode) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(vaccineObs.VaccineObsNum) + ",";
        command +=
            SOut.Long(vaccineObs.VaccinePatNum) + ","
                                                + SOut.Int((int) vaccineObs.ValType) + ","
                                                + SOut.Int((int) vaccineObs.IdentifyingCode) + ","
                                                + "'" + SOut.String(vaccineObs.ValReported) + "',"
                                                + SOut.Int((int) vaccineObs.ValCodeSystem) + ","
                                                + SOut.Long(vaccineObs.VaccineObsNumGroup) + ","
                                                + "'" + SOut.String(vaccineObs.UcumCode) + "',"
                                                + SOut.Date(vaccineObs.DateObs) + ","
                                                + "'" + SOut.String(vaccineObs.MethodCode) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            vaccineObs.VaccineObsNum = Db.NonQ(command, true, "VaccineObsNum", "vaccineObs");
        return vaccineObs.VaccineObsNum;
    }

    public static void Update(VaccineObs vaccineObs)
    {
        var command = "UPDATE vaccineobs SET "
                      + "VaccinePatNum     =  " + SOut.Long(vaccineObs.VaccinePatNum) + ", "
                      + "ValType           =  " + SOut.Int((int) vaccineObs.ValType) + ", "
                      + "IdentifyingCode   =  " + SOut.Int((int) vaccineObs.IdentifyingCode) + ", "
                      + "ValReported       = '" + SOut.String(vaccineObs.ValReported) + "', "
                      + "ValCodeSystem     =  " + SOut.Int((int) vaccineObs.ValCodeSystem) + ", "
                      + "VaccineObsNumGroup=  " + SOut.Long(vaccineObs.VaccineObsNumGroup) + ", "
                      + "UcumCode          = '" + SOut.String(vaccineObs.UcumCode) + "', "
                      + "DateObs           =  " + SOut.Date(vaccineObs.DateObs) + ", "
                      + "MethodCode        = '" + SOut.String(vaccineObs.MethodCode) + "' "
                      + "WHERE VaccineObsNum = " + SOut.Long(vaccineObs.VaccineObsNum);
        Db.NonQ(command);
    }

    public static bool Update(VaccineObs vaccineObs, VaccineObs oldVaccineObs)
    {
        var command = "";
        if (vaccineObs.VaccinePatNum != oldVaccineObs.VaccinePatNum)
        {
            if (command != "") command += ",";
            command += "VaccinePatNum = " + SOut.Long(vaccineObs.VaccinePatNum) + "";
        }

        if (vaccineObs.ValType != oldVaccineObs.ValType)
        {
            if (command != "") command += ",";
            command += "ValType = " + SOut.Int((int) vaccineObs.ValType) + "";
        }

        if (vaccineObs.IdentifyingCode != oldVaccineObs.IdentifyingCode)
        {
            if (command != "") command += ",";
            command += "IdentifyingCode = " + SOut.Int((int) vaccineObs.IdentifyingCode) + "";
        }

        if (vaccineObs.ValReported != oldVaccineObs.ValReported)
        {
            if (command != "") command += ",";
            command += "ValReported = '" + SOut.String(vaccineObs.ValReported) + "'";
        }

        if (vaccineObs.ValCodeSystem != oldVaccineObs.ValCodeSystem)
        {
            if (command != "") command += ",";
            command += "ValCodeSystem = " + SOut.Int((int) vaccineObs.ValCodeSystem) + "";
        }

        if (vaccineObs.VaccineObsNumGroup != oldVaccineObs.VaccineObsNumGroup)
        {
            if (command != "") command += ",";
            command += "VaccineObsNumGroup = " + SOut.Long(vaccineObs.VaccineObsNumGroup) + "";
        }

        if (vaccineObs.UcumCode != oldVaccineObs.UcumCode)
        {
            if (command != "") command += ",";
            command += "UcumCode = '" + SOut.String(vaccineObs.UcumCode) + "'";
        }

        if (vaccineObs.DateObs.Date != oldVaccineObs.DateObs.Date)
        {
            if (command != "") command += ",";
            command += "DateObs = " + SOut.Date(vaccineObs.DateObs) + "";
        }

        if (vaccineObs.MethodCode != oldVaccineObs.MethodCode)
        {
            if (command != "") command += ",";
            command += "MethodCode = '" + SOut.String(vaccineObs.MethodCode) + "'";
        }

        if (command == "") return false;
        command = "UPDATE vaccineobs SET " + command
                                           + " WHERE VaccineObsNum = " + SOut.Long(vaccineObs.VaccineObsNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(VaccineObs vaccineObs, VaccineObs oldVaccineObs)
    {
        if (vaccineObs.VaccinePatNum != oldVaccineObs.VaccinePatNum) return true;
        if (vaccineObs.ValType != oldVaccineObs.ValType) return true;
        if (vaccineObs.IdentifyingCode != oldVaccineObs.IdentifyingCode) return true;
        if (vaccineObs.ValReported != oldVaccineObs.ValReported) return true;
        if (vaccineObs.ValCodeSystem != oldVaccineObs.ValCodeSystem) return true;
        if (vaccineObs.VaccineObsNumGroup != oldVaccineObs.VaccineObsNumGroup) return true;
        if (vaccineObs.UcumCode != oldVaccineObs.UcumCode) return true;
        if (vaccineObs.DateObs.Date != oldVaccineObs.DateObs.Date) return true;
        if (vaccineObs.MethodCode != oldVaccineObs.MethodCode) return true;
        return false;
    }

    public static void Delete(long vaccineObsNum)
    {
        var command = "DELETE FROM vaccineobs "
                      + "WHERE VaccineObsNum = " + SOut.Long(vaccineObsNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listVaccineObsNums)
    {
        if (listVaccineObsNums == null || listVaccineObsNums.Count == 0) return;
        var command = "DELETE FROM vaccineobs "
                      + "WHERE VaccineObsNum IN(" + string.Join(",", listVaccineObsNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}