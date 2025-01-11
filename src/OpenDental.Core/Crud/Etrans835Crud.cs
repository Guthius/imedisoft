#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class Etrans835Crud
{
    public static Etrans835 SelectOne(long etrans835Num)
    {
        var command = "SELECT * FROM etrans835 "
                      + "WHERE Etrans835Num = " + SOut.Long(etrans835Num);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Etrans835 SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Etrans835> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Etrans835> TableToList(DataTable table)
    {
        var retVal = new List<Etrans835>();
        Etrans835 etrans835;
        foreach (DataRow row in table.Rows)
        {
            etrans835 = new Etrans835();
            etrans835.Etrans835Num = SIn.Long(row["Etrans835Num"].ToString());
            etrans835.EtransNum = SIn.Long(row["EtransNum"].ToString());
            etrans835.PayerName = SIn.String(row["PayerName"].ToString());
            etrans835.TransRefNum = SIn.String(row["TransRefNum"].ToString());
            etrans835.InsPaid = SIn.Double(row["InsPaid"].ToString());
            etrans835.ControlId = SIn.String(row["ControlId"].ToString());
            etrans835.PaymentMethodCode = SIn.String(row["PaymentMethodCode"].ToString());
            etrans835.PatientName = SIn.String(row["PatientName"].ToString());
            etrans835.Status = (X835Status) SIn.Int(row["Status"].ToString());
            etrans835.AutoProcessed = (X835AutoProcessed) SIn.Int(row["AutoProcessed"].ToString());
            etrans835.IsApproved = SIn.Bool(row["IsApproved"].ToString());
            retVal.Add(etrans835);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Etrans835> listEtrans835s, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Etrans835";
        var table = new DataTable(tableName);
        table.Columns.Add("Etrans835Num");
        table.Columns.Add("EtransNum");
        table.Columns.Add("PayerName");
        table.Columns.Add("TransRefNum");
        table.Columns.Add("InsPaid");
        table.Columns.Add("ControlId");
        table.Columns.Add("PaymentMethodCode");
        table.Columns.Add("PatientName");
        table.Columns.Add("Status");
        table.Columns.Add("AutoProcessed");
        table.Columns.Add("IsApproved");
        foreach (var etrans835 in listEtrans835s)
            table.Rows.Add(SOut.Long(etrans835.Etrans835Num), SOut.Long(etrans835.EtransNum), etrans835.PayerName, etrans835.TransRefNum, SOut.Double(etrans835.InsPaid), etrans835.ControlId, etrans835.PaymentMethodCode, etrans835.PatientName, SOut.Int((int) etrans835.Status), SOut.Int((int) etrans835.AutoProcessed), SOut.Bool(etrans835.IsApproved));
        return table;
    }

    public static long Insert(Etrans835 etrans835)
    {
        return Insert(etrans835, false);
    }

    public static long Insert(Etrans835 etrans835, bool useExistingPK)
    {
        var command = "INSERT INTO etrans835 (";

        command += "EtransNum,PayerName,TransRefNum,InsPaid,ControlId,PaymentMethodCode,PatientName,Status,AutoProcessed,IsApproved) VALUES(";

        command +=
            SOut.Long(etrans835.EtransNum) + ","
                                           + "'" + SOut.String(etrans835.PayerName) + "',"
                                           + "'" + SOut.String(etrans835.TransRefNum) + "',"
                                           + SOut.Double(etrans835.InsPaid) + ","
                                           + "'" + SOut.String(etrans835.ControlId) + "',"
                                           + "'" + SOut.String(etrans835.PaymentMethodCode) + "',"
                                           + "'" + SOut.String(etrans835.PatientName) + "',"
                                           + SOut.Int((int) etrans835.Status) + ","
                                           + SOut.Int((int) etrans835.AutoProcessed) + ","
                                           + SOut.Bool(etrans835.IsApproved) + ")";
        {
            etrans835.Etrans835Num = Db.NonQ(command, true, "Etrans835Num", "etrans835");
        }
        return etrans835.Etrans835Num;
    }

    public static long InsertNoCache(Etrans835 etrans835)
    {
        return InsertNoCache(etrans835, false);
    }

    public static long InsertNoCache(Etrans835 etrans835, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO etrans835 (";
        if (isRandomKeys || useExistingPK) command += "Etrans835Num,";
        command += "EtransNum,PayerName,TransRefNum,InsPaid,ControlId,PaymentMethodCode,PatientName,Status,AutoProcessed,IsApproved) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(etrans835.Etrans835Num) + ",";
        command +=
            SOut.Long(etrans835.EtransNum) + ","
                                           + "'" + SOut.String(etrans835.PayerName) + "',"
                                           + "'" + SOut.String(etrans835.TransRefNum) + "',"
                                           + SOut.Double(etrans835.InsPaid) + ","
                                           + "'" + SOut.String(etrans835.ControlId) + "',"
                                           + "'" + SOut.String(etrans835.PaymentMethodCode) + "',"
                                           + "'" + SOut.String(etrans835.PatientName) + "',"
                                           + SOut.Int((int) etrans835.Status) + ","
                                           + SOut.Int((int) etrans835.AutoProcessed) + ","
                                           + SOut.Bool(etrans835.IsApproved) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            etrans835.Etrans835Num = Db.NonQ(command, true, "Etrans835Num", "etrans835");
        return etrans835.Etrans835Num;
    }

    public static void Update(Etrans835 etrans835)
    {
        var command = "UPDATE etrans835 SET "
                      + "EtransNum        =  " + SOut.Long(etrans835.EtransNum) + ", "
                      + "PayerName        = '" + SOut.String(etrans835.PayerName) + "', "
                      + "TransRefNum      = '" + SOut.String(etrans835.TransRefNum) + "', "
                      + "InsPaid          =  " + SOut.Double(etrans835.InsPaid) + ", "
                      + "ControlId        = '" + SOut.String(etrans835.ControlId) + "', "
                      + "PaymentMethodCode= '" + SOut.String(etrans835.PaymentMethodCode) + "', "
                      + "PatientName      = '" + SOut.String(etrans835.PatientName) + "', "
                      + "Status           =  " + SOut.Int((int) etrans835.Status) + ", "
                      + "AutoProcessed    =  " + SOut.Int((int) etrans835.AutoProcessed) + ", "
                      + "IsApproved       =  " + SOut.Bool(etrans835.IsApproved) + " "
                      + "WHERE Etrans835Num = " + SOut.Long(etrans835.Etrans835Num);
        Db.NonQ(command);
    }

    public static bool Update(Etrans835 etrans835, Etrans835 oldEtrans835)
    {
        var command = "";
        if (etrans835.EtransNum != oldEtrans835.EtransNum)
        {
            if (command != "") command += ",";
            command += "EtransNum = " + SOut.Long(etrans835.EtransNum) + "";
        }

        if (etrans835.PayerName != oldEtrans835.PayerName)
        {
            if (command != "") command += ",";
            command += "PayerName = '" + SOut.String(etrans835.PayerName) + "'";
        }

        if (etrans835.TransRefNum != oldEtrans835.TransRefNum)
        {
            if (command != "") command += ",";
            command += "TransRefNum = '" + SOut.String(etrans835.TransRefNum) + "'";
        }

        if (etrans835.InsPaid != oldEtrans835.InsPaid)
        {
            if (command != "") command += ",";
            command += "InsPaid = " + SOut.Double(etrans835.InsPaid) + "";
        }

        if (etrans835.ControlId != oldEtrans835.ControlId)
        {
            if (command != "") command += ",";
            command += "ControlId = '" + SOut.String(etrans835.ControlId) + "'";
        }

        if (etrans835.PaymentMethodCode != oldEtrans835.PaymentMethodCode)
        {
            if (command != "") command += ",";
            command += "PaymentMethodCode = '" + SOut.String(etrans835.PaymentMethodCode) + "'";
        }

        if (etrans835.PatientName != oldEtrans835.PatientName)
        {
            if (command != "") command += ",";
            command += "PatientName = '" + SOut.String(etrans835.PatientName) + "'";
        }

        if (etrans835.Status != oldEtrans835.Status)
        {
            if (command != "") command += ",";
            command += "Status = " + SOut.Int((int) etrans835.Status) + "";
        }

        if (etrans835.AutoProcessed != oldEtrans835.AutoProcessed)
        {
            if (command != "") command += ",";
            command += "AutoProcessed = " + SOut.Int((int) etrans835.AutoProcessed) + "";
        }

        if (etrans835.IsApproved != oldEtrans835.IsApproved)
        {
            if (command != "") command += ",";
            command += "IsApproved = " + SOut.Bool(etrans835.IsApproved) + "";
        }

        if (command == "") return false;
        command = "UPDATE etrans835 SET " + command
                                          + " WHERE Etrans835Num = " + SOut.Long(etrans835.Etrans835Num);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Etrans835 etrans835, Etrans835 oldEtrans835)
    {
        if (etrans835.EtransNum != oldEtrans835.EtransNum) return true;
        if (etrans835.PayerName != oldEtrans835.PayerName) return true;
        if (etrans835.TransRefNum != oldEtrans835.TransRefNum) return true;
        if (etrans835.InsPaid != oldEtrans835.InsPaid) return true;
        if (etrans835.ControlId != oldEtrans835.ControlId) return true;
        if (etrans835.PaymentMethodCode != oldEtrans835.PaymentMethodCode) return true;
        if (etrans835.PatientName != oldEtrans835.PatientName) return true;
        if (etrans835.Status != oldEtrans835.Status) return true;
        if (etrans835.AutoProcessed != oldEtrans835.AutoProcessed) return true;
        if (etrans835.IsApproved != oldEtrans835.IsApproved) return true;
        return false;
    }

    public static void Delete(long etrans835Num)
    {
        var command = "DELETE FROM etrans835 "
                      + "WHERE Etrans835Num = " + SOut.Long(etrans835Num);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEtrans835Nums)
    {
        if (listEtrans835Nums == null || listEtrans835Nums.Count == 0) return;
        var command = "DELETE FROM etrans835 "
                      + "WHERE Etrans835Num IN(" + string.Join(",", listEtrans835Nums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}