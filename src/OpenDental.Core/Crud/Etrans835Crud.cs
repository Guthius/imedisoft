using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class Etrans835Crud
{
    public static List<Etrans835> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Etrans835> TableToList(DataTable table)
    {
        var retVal = new List<Etrans835>();
        foreach (DataRow row in table.Rows)
        {
            var etrans835 = new Etrans835
            {
                Etrans835Num = SIn.Long(row["Etrans835Num"].ToString()),
                EtransNum = SIn.Long(row["EtransNum"].ToString()),
                PayerName = SIn.String(row["PayerName"].ToString()),
                TransRefNum = SIn.String(row["TransRefNum"].ToString()),
                InsPaid = SIn.Double(row["InsPaid"].ToString()),
                ControlId = SIn.String(row["ControlId"].ToString()),
                PaymentMethodCode = SIn.String(row["PaymentMethodCode"].ToString()),
                PatientName = SIn.String(row["PatientName"].ToString()),
                Status = (X835Status) SIn.Int(row["Status"].ToString()),
                AutoProcessed = (X835AutoProcessed) SIn.Int(row["AutoProcessed"].ToString()),
                IsApproved = SIn.Bool(row["IsApproved"].ToString())
            };
            retVal.Add(etrans835);
        }

        return retVal;
    }

    public static void Insert(Etrans835 etrans835)
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
    }

    public static void Update(Etrans835 etrans835, Etrans835 oldEtrans835)
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

        if (command == "") return;
        command = "UPDATE etrans835 SET " + command
                                          + " WHERE Etrans835Num = " + SOut.Long(etrans835.Etrans835Num);
        Db.NonQ(command);
    }
}