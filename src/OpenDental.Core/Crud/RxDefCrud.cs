#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class RxDefCrud
{
    public static RxDef SelectOne(long rxDefNum)
    {
        var command = "SELECT * FROM rxdef "
                      + "WHERE RxDefNum = " + SOut.Long(rxDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static RxDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<RxDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RxDef> TableToList(DataTable table)
    {
        var retVal = new List<RxDef>();
        RxDef rxDef;
        foreach (DataRow row in table.Rows)
        {
            rxDef = new RxDef();
            rxDef.RxDefNum = SIn.Long(row["RxDefNum"].ToString());
            rxDef.Drug = SIn.String(row["Drug"].ToString());
            rxDef.Sig = SIn.String(row["Sig"].ToString());
            rxDef.Disp = SIn.String(row["Disp"].ToString());
            rxDef.Refills = SIn.String(row["Refills"].ToString());
            rxDef.Notes = SIn.String(row["Notes"].ToString());
            rxDef.IsControlled = SIn.Bool(row["IsControlled"].ToString());
            rxDef.RxCui = SIn.Long(row["RxCui"].ToString());
            rxDef.IsProcRequired = SIn.Bool(row["IsProcRequired"].ToString());
            rxDef.PatientInstruction = SIn.String(row["PatientInstruction"].ToString());
            retVal.Add(rxDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<RxDef> listRxDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "RxDef";
        var table = new DataTable(tableName);
        table.Columns.Add("RxDefNum");
        table.Columns.Add("Drug");
        table.Columns.Add("Sig");
        table.Columns.Add("Disp");
        table.Columns.Add("Refills");
        table.Columns.Add("Notes");
        table.Columns.Add("IsControlled");
        table.Columns.Add("RxCui");
        table.Columns.Add("IsProcRequired");
        table.Columns.Add("PatientInstruction");
        foreach (var rxDef in listRxDefs)
            table.Rows.Add(SOut.Long(rxDef.RxDefNum), rxDef.Drug, rxDef.Sig, rxDef.Disp, rxDef.Refills, rxDef.Notes, SOut.Bool(rxDef.IsControlled), SOut.Long(rxDef.RxCui), SOut.Bool(rxDef.IsProcRequired), rxDef.PatientInstruction);
        return table;
    }

    public static long Insert(RxDef rxDef)
    {
        return Insert(rxDef, false);
    }

    public static long Insert(RxDef rxDef, bool useExistingPK)
    {
        var command = "INSERT INTO rxdef (";

        command += "Drug,Sig,Disp,Refills,Notes,IsControlled,RxCui,IsProcRequired,PatientInstruction) VALUES(";

        command +=
            "'" + SOut.String(rxDef.Drug) + "',"
            + "'" + SOut.String(rxDef.Sig) + "',"
            + "'" + SOut.String(rxDef.Disp) + "',"
            + "'" + SOut.String(rxDef.Refills) + "',"
            + "'" + SOut.String(rxDef.Notes) + "',"
            + SOut.Bool(rxDef.IsControlled) + ","
            + SOut.Long(rxDef.RxCui) + ","
            + SOut.Bool(rxDef.IsProcRequired) + ","
            + DbHelper.ParamChar + "paramPatientInstruction)";
        if (rxDef.PatientInstruction == null) rxDef.PatientInstruction = "";
        var paramPatientInstruction = new OdSqlParameter("paramPatientInstruction", OdDbType.Text, SOut.StringParam(rxDef.PatientInstruction));
        {
            rxDef.RxDefNum = Db.NonQ(command, true, "RxDefNum", "rxDef", paramPatientInstruction);
        }
        return rxDef.RxDefNum;
    }

    public static long InsertNoCache(RxDef rxDef)
    {
        return InsertNoCache(rxDef, false);
    }

    public static long InsertNoCache(RxDef rxDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO rxdef (";
        if (isRandomKeys || useExistingPK) command += "RxDefNum,";
        command += "Drug,Sig,Disp,Refills,Notes,IsControlled,RxCui,IsProcRequired,PatientInstruction) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(rxDef.RxDefNum) + ",";
        command +=
            "'" + SOut.String(rxDef.Drug) + "',"
            + "'" + SOut.String(rxDef.Sig) + "',"
            + "'" + SOut.String(rxDef.Disp) + "',"
            + "'" + SOut.String(rxDef.Refills) + "',"
            + "'" + SOut.String(rxDef.Notes) + "',"
            + SOut.Bool(rxDef.IsControlled) + ","
            + SOut.Long(rxDef.RxCui) + ","
            + SOut.Bool(rxDef.IsProcRequired) + ","
            + DbHelper.ParamChar + "paramPatientInstruction)";
        if (rxDef.PatientInstruction == null) rxDef.PatientInstruction = "";
        var paramPatientInstruction = new OdSqlParameter("paramPatientInstruction", OdDbType.Text, SOut.StringParam(rxDef.PatientInstruction));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramPatientInstruction);
        else
            rxDef.RxDefNum = Db.NonQ(command, true, "RxDefNum", "rxDef", paramPatientInstruction);
        return rxDef.RxDefNum;
    }

    public static void Update(RxDef rxDef)
    {
        var command = "UPDATE rxdef SET "
                      + "Drug              = '" + SOut.String(rxDef.Drug) + "', "
                      + "Sig               = '" + SOut.String(rxDef.Sig) + "', "
                      + "Disp              = '" + SOut.String(rxDef.Disp) + "', "
                      + "Refills           = '" + SOut.String(rxDef.Refills) + "', "
                      + "Notes             = '" + SOut.String(rxDef.Notes) + "', "
                      + "IsControlled      =  " + SOut.Bool(rxDef.IsControlled) + ", "
                      + "RxCui             =  " + SOut.Long(rxDef.RxCui) + ", "
                      + "IsProcRequired    =  " + SOut.Bool(rxDef.IsProcRequired) + ", "
                      + "PatientInstruction=  " + DbHelper.ParamChar + "paramPatientInstruction "
                      + "WHERE RxDefNum = " + SOut.Long(rxDef.RxDefNum);
        if (rxDef.PatientInstruction == null) rxDef.PatientInstruction = "";
        var paramPatientInstruction = new OdSqlParameter("paramPatientInstruction", OdDbType.Text, SOut.StringParam(rxDef.PatientInstruction));
        Db.NonQ(command, paramPatientInstruction);
    }

    public static bool Update(RxDef rxDef, RxDef oldRxDef)
    {
        var command = "";
        if (rxDef.Drug != oldRxDef.Drug)
        {
            if (command != "") command += ",";
            command += "Drug = '" + SOut.String(rxDef.Drug) + "'";
        }

        if (rxDef.Sig != oldRxDef.Sig)
        {
            if (command != "") command += ",";
            command += "Sig = '" + SOut.String(rxDef.Sig) + "'";
        }

        if (rxDef.Disp != oldRxDef.Disp)
        {
            if (command != "") command += ",";
            command += "Disp = '" + SOut.String(rxDef.Disp) + "'";
        }

        if (rxDef.Refills != oldRxDef.Refills)
        {
            if (command != "") command += ",";
            command += "Refills = '" + SOut.String(rxDef.Refills) + "'";
        }

        if (rxDef.Notes != oldRxDef.Notes)
        {
            if (command != "") command += ",";
            command += "Notes = '" + SOut.String(rxDef.Notes) + "'";
        }

        if (rxDef.IsControlled != oldRxDef.IsControlled)
        {
            if (command != "") command += ",";
            command += "IsControlled = " + SOut.Bool(rxDef.IsControlled) + "";
        }

        if (rxDef.RxCui != oldRxDef.RxCui)
        {
            if (command != "") command += ",";
            command += "RxCui = " + SOut.Long(rxDef.RxCui) + "";
        }

        if (rxDef.IsProcRequired != oldRxDef.IsProcRequired)
        {
            if (command != "") command += ",";
            command += "IsProcRequired = " + SOut.Bool(rxDef.IsProcRequired) + "";
        }

        if (rxDef.PatientInstruction != oldRxDef.PatientInstruction)
        {
            if (command != "") command += ",";
            command += "PatientInstruction = " + DbHelper.ParamChar + "paramPatientInstruction";
        }

        if (command == "") return false;
        if (rxDef.PatientInstruction == null) rxDef.PatientInstruction = "";
        var paramPatientInstruction = new OdSqlParameter("paramPatientInstruction", OdDbType.Text, SOut.StringParam(rxDef.PatientInstruction));
        command = "UPDATE rxdef SET " + command
                                      + " WHERE RxDefNum = " + SOut.Long(rxDef.RxDefNum);
        Db.NonQ(command, paramPatientInstruction);
        return true;
    }


    public static bool UpdateComparison(RxDef rxDef, RxDef oldRxDef)
    {
        if (rxDef.Drug != oldRxDef.Drug) return true;
        if (rxDef.Sig != oldRxDef.Sig) return true;
        if (rxDef.Disp != oldRxDef.Disp) return true;
        if (rxDef.Refills != oldRxDef.Refills) return true;
        if (rxDef.Notes != oldRxDef.Notes) return true;
        if (rxDef.IsControlled != oldRxDef.IsControlled) return true;
        if (rxDef.RxCui != oldRxDef.RxCui) return true;
        if (rxDef.IsProcRequired != oldRxDef.IsProcRequired) return true;
        if (rxDef.PatientInstruction != oldRxDef.PatientInstruction) return true;
        return false;
    }


    public static void Delete(long rxDefNum)
    {
        var command = "DELETE FROM rxdef "
                      + "WHERE RxDefNum = " + SOut.Long(rxDefNum);
        Db.NonQ(command);
    }


    public static void DeleteMany(List<long> listRxDefNums)
    {
        if (listRxDefNums == null || listRxDefNums.Count == 0) return;
        var command = "DELETE FROM rxdef "
                      + "WHERE RxDefNum IN(" + string.Join(",", listRxDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}