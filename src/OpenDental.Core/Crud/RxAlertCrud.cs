#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class RxAlertCrud
{
    public static RxAlert SelectOne(long rxAlertNum)
    {
        var command = "SELECT * FROM rxalert "
                      + "WHERE RxAlertNum = " + SOut.Long(rxAlertNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static RxAlert SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<RxAlert> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RxAlert> TableToList(DataTable table)
    {
        var retVal = new List<RxAlert>();
        RxAlert rxAlert;
        foreach (DataRow row in table.Rows)
        {
            rxAlert = new RxAlert();
            rxAlert.RxAlertNum = SIn.Long(row["RxAlertNum"].ToString());
            rxAlert.RxDefNum = SIn.Long(row["RxDefNum"].ToString());
            rxAlert.DiseaseDefNum = SIn.Long(row["DiseaseDefNum"].ToString());
            rxAlert.AllergyDefNum = SIn.Long(row["AllergyDefNum"].ToString());
            rxAlert.MedicationNum = SIn.Long(row["MedicationNum"].ToString());
            rxAlert.NotificationMsg = SIn.String(row["NotificationMsg"].ToString());
            rxAlert.IsHighSignificance = SIn.Bool(row["IsHighSignificance"].ToString());
            retVal.Add(rxAlert);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<RxAlert> listRxAlerts, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "RxAlert";
        var table = new DataTable(tableName);
        table.Columns.Add("RxAlertNum");
        table.Columns.Add("RxDefNum");
        table.Columns.Add("DiseaseDefNum");
        table.Columns.Add("AllergyDefNum");
        table.Columns.Add("MedicationNum");
        table.Columns.Add("NotificationMsg");
        table.Columns.Add("IsHighSignificance");
        foreach (var rxAlert in listRxAlerts)
            table.Rows.Add(SOut.Long(rxAlert.RxAlertNum), SOut.Long(rxAlert.RxDefNum), SOut.Long(rxAlert.DiseaseDefNum), SOut.Long(rxAlert.AllergyDefNum), SOut.Long(rxAlert.MedicationNum), rxAlert.NotificationMsg, SOut.Bool(rxAlert.IsHighSignificance));
        return table;
    }

    public static long Insert(RxAlert rxAlert)
    {
        return Insert(rxAlert, false);
    }

    public static long Insert(RxAlert rxAlert, bool useExistingPK)
    {
        var command = "INSERT INTO rxalert (";

        command += "RxDefNum,DiseaseDefNum,AllergyDefNum,MedicationNum,NotificationMsg,IsHighSignificance) VALUES(";

        command +=
            SOut.Long(rxAlert.RxDefNum) + ","
                                        + SOut.Long(rxAlert.DiseaseDefNum) + ","
                                        + SOut.Long(rxAlert.AllergyDefNum) + ","
                                        + SOut.Long(rxAlert.MedicationNum) + ","
                                        + "'" + SOut.String(rxAlert.NotificationMsg) + "',"
                                        + SOut.Bool(rxAlert.IsHighSignificance) + ")";
        {
            rxAlert.RxAlertNum = Db.NonQ(command, true, "RxAlertNum", "rxAlert");
        }
        return rxAlert.RxAlertNum;
    }

    public static long InsertNoCache(RxAlert rxAlert)
    {
        return InsertNoCache(rxAlert, false);
    }

    public static long InsertNoCache(RxAlert rxAlert, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO rxalert (";
        if (isRandomKeys || useExistingPK) command += "RxAlertNum,";
        command += "RxDefNum,DiseaseDefNum,AllergyDefNum,MedicationNum,NotificationMsg,IsHighSignificance) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(rxAlert.RxAlertNum) + ",";
        command +=
            SOut.Long(rxAlert.RxDefNum) + ","
                                        + SOut.Long(rxAlert.DiseaseDefNum) + ","
                                        + SOut.Long(rxAlert.AllergyDefNum) + ","
                                        + SOut.Long(rxAlert.MedicationNum) + ","
                                        + "'" + SOut.String(rxAlert.NotificationMsg) + "',"
                                        + SOut.Bool(rxAlert.IsHighSignificance) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            rxAlert.RxAlertNum = Db.NonQ(command, true, "RxAlertNum", "rxAlert");
        return rxAlert.RxAlertNum;
    }

    public static void Update(RxAlert rxAlert)
    {
        var command = "UPDATE rxalert SET "
                      + "RxDefNum          =  " + SOut.Long(rxAlert.RxDefNum) + ", "
                      + "DiseaseDefNum     =  " + SOut.Long(rxAlert.DiseaseDefNum) + ", "
                      + "AllergyDefNum     =  " + SOut.Long(rxAlert.AllergyDefNum) + ", "
                      + "MedicationNum     =  " + SOut.Long(rxAlert.MedicationNum) + ", "
                      + "NotificationMsg   = '" + SOut.String(rxAlert.NotificationMsg) + "', "
                      + "IsHighSignificance=  " + SOut.Bool(rxAlert.IsHighSignificance) + " "
                      + "WHERE RxAlertNum = " + SOut.Long(rxAlert.RxAlertNum);
        Db.NonQ(command);
    }

    public static bool Update(RxAlert rxAlert, RxAlert oldRxAlert)
    {
        var command = "";
        if (rxAlert.RxDefNum != oldRxAlert.RxDefNum)
        {
            if (command != "") command += ",";
            command += "RxDefNum = " + SOut.Long(rxAlert.RxDefNum) + "";
        }

        if (rxAlert.DiseaseDefNum != oldRxAlert.DiseaseDefNum)
        {
            if (command != "") command += ",";
            command += "DiseaseDefNum = " + SOut.Long(rxAlert.DiseaseDefNum) + "";
        }

        if (rxAlert.AllergyDefNum != oldRxAlert.AllergyDefNum)
        {
            if (command != "") command += ",";
            command += "AllergyDefNum = " + SOut.Long(rxAlert.AllergyDefNum) + "";
        }

        if (rxAlert.MedicationNum != oldRxAlert.MedicationNum)
        {
            if (command != "") command += ",";
            command += "MedicationNum = " + SOut.Long(rxAlert.MedicationNum) + "";
        }

        if (rxAlert.NotificationMsg != oldRxAlert.NotificationMsg)
        {
            if (command != "") command += ",";
            command += "NotificationMsg = '" + SOut.String(rxAlert.NotificationMsg) + "'";
        }

        if (rxAlert.IsHighSignificance != oldRxAlert.IsHighSignificance)
        {
            if (command != "") command += ",";
            command += "IsHighSignificance = " + SOut.Bool(rxAlert.IsHighSignificance) + "";
        }

        if (command == "") return false;
        command = "UPDATE rxalert SET " + command
                                        + " WHERE RxAlertNum = " + SOut.Long(rxAlert.RxAlertNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(RxAlert rxAlert, RxAlert oldRxAlert)
    {
        if (rxAlert.RxDefNum != oldRxAlert.RxDefNum) return true;
        if (rxAlert.DiseaseDefNum != oldRxAlert.DiseaseDefNum) return true;
        if (rxAlert.AllergyDefNum != oldRxAlert.AllergyDefNum) return true;
        if (rxAlert.MedicationNum != oldRxAlert.MedicationNum) return true;
        if (rxAlert.NotificationMsg != oldRxAlert.NotificationMsg) return true;
        if (rxAlert.IsHighSignificance != oldRxAlert.IsHighSignificance) return true;
        return false;
    }

    public static void Delete(long rxAlertNum)
    {
        var command = "DELETE FROM rxalert "
                      + "WHERE RxAlertNum = " + SOut.Long(rxAlertNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listRxAlertNums)
    {
        if (listRxAlertNums == null || listRxAlertNums.Count == 0) return;
        var command = "DELETE FROM rxalert "
                      + "WHERE RxAlertNum IN(" + string.Join(",", listRxAlertNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}