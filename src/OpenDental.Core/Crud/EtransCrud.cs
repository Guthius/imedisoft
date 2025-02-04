using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class EtransCrud
{
    public static Etrans SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Etrans> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Etrans> TableToList(DataTable table)
    {
        var retVal = new List<Etrans>();
        foreach (DataRow row in table.Rows)
        {
            var etrans = new Etrans
            {
                EtransNum = SIn.Long(row["EtransNum"].ToString()),
                DateTimeTrans = SIn.DateTime(row["DateTimeTrans"].ToString()),
                ClearingHouseNum = SIn.Long(row["ClearingHouseNum"].ToString()),
                Etype = (EtransType) SIn.Int(row["Etype"].ToString()),
                ClaimNum = SIn.Long(row["ClaimNum"].ToString()),
                OfficeSequenceNumber = SIn.Int(row["OfficeSequenceNumber"].ToString()),
                CarrierTransCounter = SIn.Int(row["CarrierTransCounter"].ToString()),
                CarrierTransCounter2 = SIn.Int(row["CarrierTransCounter2"].ToString()),
                CarrierNum = SIn.Long(row["CarrierNum"].ToString()),
                CarrierNum2 = SIn.Long(row["CarrierNum2"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                BatchNumber = SIn.Int(row["BatchNumber"].ToString()),
                AckCode = SIn.String(row["AckCode"].ToString()),
                TransSetNum = SIn.Int(row["TransSetNum"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                EtransMessageTextNum = SIn.Long(row["EtransMessageTextNum"].ToString()),
                AckEtransNum = SIn.Long(row["AckEtransNum"].ToString()),
                PlanNum = SIn.Long(row["PlanNum"].ToString()),
                InsSubNum = SIn.Long(row["InsSubNum"].ToString()),
                TranSetId835 = SIn.String(row["TranSetId835"].ToString()),
                CarrierNameRaw = SIn.String(row["CarrierNameRaw"].ToString()),
                PatientNameRaw = SIn.String(row["PatientNameRaw"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString())
            };
            retVal.Add(etrans);
        }

        return retVal;
    }

    public static long Insert(Etrans etrans)
    {
        var command = "INSERT INTO etrans (";

        command += "DateTimeTrans,ClearingHouseNum,Etype,ClaimNum,OfficeSequenceNumber,CarrierTransCounter,CarrierTransCounter2,CarrierNum,CarrierNum2,PatNum,BatchNumber,AckCode,TransSetNum,Note,EtransMessageTextNum,AckEtransNum,PlanNum,InsSubNum,TranSetId835,CarrierNameRaw,PatientNameRaw,UserNum) VALUES(";

        command +=
            "NOW()" + ","
                    + SOut.Long(etrans.ClearingHouseNum) + ","
                    + SOut.Int((int) etrans.Etype) + ","
                    + SOut.Long(etrans.ClaimNum) + ","
                    + SOut.Int(etrans.OfficeSequenceNumber) + ","
                    + SOut.Int(etrans.CarrierTransCounter) + ","
                    + SOut.Int(etrans.CarrierTransCounter2) + ","
                    + SOut.Long(etrans.CarrierNum) + ","
                    + SOut.Long(etrans.CarrierNum2) + ","
                    + SOut.Long(etrans.PatNum) + ","
                    + SOut.Int(etrans.BatchNumber) + ","
                    + "'" + SOut.String(etrans.AckCode) + "',"
                    + SOut.Int(etrans.TransSetNum) + ","
                    + DbHelper.ParamChar + "paramNote,"
                    + SOut.Long(etrans.EtransMessageTextNum) + ","
                    + SOut.Long(etrans.AckEtransNum) + ","
                    + SOut.Long(etrans.PlanNum) + ","
                    + SOut.Long(etrans.InsSubNum) + ","
                    + "'" + SOut.String(etrans.TranSetId835) + "',"
                    + "'" + SOut.String(etrans.CarrierNameRaw) + "',"
                    + "'" + SOut.String(etrans.PatientNameRaw) + "',"
                    + SOut.Long(etrans.UserNum) + ")";
        if (etrans.Note == null) etrans.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(etrans.Note));
        {
            etrans.EtransNum = Db.NonQ(command, true, "EtransNum", "etrans", paramNote);
        }
        return etrans.EtransNum;
    }

    public static void Update(Etrans etrans)
    {
        var command = "UPDATE etrans SET "
                      + "DateTimeTrans       =  " + SOut.DateTime(etrans.DateTimeTrans) + ", "
                      + "ClearingHouseNum    =  " + SOut.Long(etrans.ClearingHouseNum) + ", "
                      + "Etype               =  " + SOut.Int((int) etrans.Etype) + ", "
                      + "ClaimNum            =  " + SOut.Long(etrans.ClaimNum) + ", "
                      + "OfficeSequenceNumber=  " + SOut.Int(etrans.OfficeSequenceNumber) + ", "
                      + "CarrierTransCounter =  " + SOut.Int(etrans.CarrierTransCounter) + ", "
                      + "CarrierTransCounter2=  " + SOut.Int(etrans.CarrierTransCounter2) + ", "
                      + "CarrierNum          =  " + SOut.Long(etrans.CarrierNum) + ", "
                      + "CarrierNum2         =  " + SOut.Long(etrans.CarrierNum2) + ", "
                      + "PatNum              =  " + SOut.Long(etrans.PatNum) + ", "
                      + "BatchNumber         =  " + SOut.Int(etrans.BatchNumber) + ", "
                      + "AckCode             = '" + SOut.String(etrans.AckCode) + "', "
                      + "TransSetNum         =  " + SOut.Int(etrans.TransSetNum) + ", "
                      + "Note                =  " + DbHelper.ParamChar + "paramNote, "
                      + "EtransMessageTextNum=  " + SOut.Long(etrans.EtransMessageTextNum) + ", "
                      + "AckEtransNum        =  " + SOut.Long(etrans.AckEtransNum) + ", "
                      + "PlanNum             =  " + SOut.Long(etrans.PlanNum) + ", "
                      + "InsSubNum           =  " + SOut.Long(etrans.InsSubNum) + ", "
                      + "TranSetId835        = '" + SOut.String(etrans.TranSetId835) + "', "
                      + "CarrierNameRaw      = '" + SOut.String(etrans.CarrierNameRaw) + "', "
                      + "PatientNameRaw      = '" + SOut.String(etrans.PatientNameRaw) + "', "
                      + "UserNum             =  " + SOut.Long(etrans.UserNum) + " "
                      + "WHERE EtransNum = " + SOut.Long(etrans.EtransNum);
        if (etrans.Note == null) etrans.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(etrans.Note));
        Db.NonQ(command, paramNote);
    }

    public static void Update(Etrans etrans, Etrans oldEtrans)
    {
        var command = "";
        if (etrans.DateTimeTrans != oldEtrans.DateTimeTrans)
        {
            if (command != "") command += ",";
            command += "DateTimeTrans = " + SOut.DateTime(etrans.DateTimeTrans) + "";
        }

        if (etrans.ClearingHouseNum != oldEtrans.ClearingHouseNum)
        {
            if (command != "") command += ",";
            command += "ClearingHouseNum = " + SOut.Long(etrans.ClearingHouseNum) + "";
        }

        if (etrans.Etype != oldEtrans.Etype)
        {
            if (command != "") command += ",";
            command += "Etype = " + SOut.Int((int) etrans.Etype) + "";
        }

        if (etrans.ClaimNum != oldEtrans.ClaimNum)
        {
            if (command != "") command += ",";
            command += "ClaimNum = " + SOut.Long(etrans.ClaimNum) + "";
        }

        if (etrans.OfficeSequenceNumber != oldEtrans.OfficeSequenceNumber)
        {
            if (command != "") command += ",";
            command += "OfficeSequenceNumber = " + SOut.Int(etrans.OfficeSequenceNumber) + "";
        }

        if (etrans.CarrierTransCounter != oldEtrans.CarrierTransCounter)
        {
            if (command != "") command += ",";
            command += "CarrierTransCounter = " + SOut.Int(etrans.CarrierTransCounter) + "";
        }

        if (etrans.CarrierTransCounter2 != oldEtrans.CarrierTransCounter2)
        {
            if (command != "") command += ",";
            command += "CarrierTransCounter2 = " + SOut.Int(etrans.CarrierTransCounter2) + "";
        }

        if (etrans.CarrierNum != oldEtrans.CarrierNum)
        {
            if (command != "") command += ",";
            command += "CarrierNum = " + SOut.Long(etrans.CarrierNum) + "";
        }

        if (etrans.CarrierNum2 != oldEtrans.CarrierNum2)
        {
            if (command != "") command += ",";
            command += "CarrierNum2 = " + SOut.Long(etrans.CarrierNum2) + "";
        }

        if (etrans.PatNum != oldEtrans.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(etrans.PatNum) + "";
        }

        if (etrans.BatchNumber != oldEtrans.BatchNumber)
        {
            if (command != "") command += ",";
            command += "BatchNumber = " + SOut.Int(etrans.BatchNumber) + "";
        }

        if (etrans.AckCode != oldEtrans.AckCode)
        {
            if (command != "") command += ",";
            command += "AckCode = '" + SOut.String(etrans.AckCode) + "'";
        }

        if (etrans.TransSetNum != oldEtrans.TransSetNum)
        {
            if (command != "") command += ",";
            command += "TransSetNum = " + SOut.Int(etrans.TransSetNum) + "";
        }

        if (etrans.Note != oldEtrans.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (etrans.EtransMessageTextNum != oldEtrans.EtransMessageTextNum)
        {
            if (command != "") command += ",";
            command += "EtransMessageTextNum = " + SOut.Long(etrans.EtransMessageTextNum) + "";
        }

        if (etrans.AckEtransNum != oldEtrans.AckEtransNum)
        {
            if (command != "") command += ",";
            command += "AckEtransNum = " + SOut.Long(etrans.AckEtransNum) + "";
        }

        if (etrans.PlanNum != oldEtrans.PlanNum)
        {
            if (command != "") command += ",";
            command += "PlanNum = " + SOut.Long(etrans.PlanNum) + "";
        }

        if (etrans.InsSubNum != oldEtrans.InsSubNum)
        {
            if (command != "") command += ",";
            command += "InsSubNum = " + SOut.Long(etrans.InsSubNum) + "";
        }

        if (etrans.TranSetId835 != oldEtrans.TranSetId835)
        {
            if (command != "") command += ",";
            command += "TranSetId835 = '" + SOut.String(etrans.TranSetId835) + "'";
        }

        if (etrans.CarrierNameRaw != oldEtrans.CarrierNameRaw)
        {
            if (command != "") command += ",";
            command += "CarrierNameRaw = '" + SOut.String(etrans.CarrierNameRaw) + "'";
        }

        if (etrans.PatientNameRaw != oldEtrans.PatientNameRaw)
        {
            if (command != "") command += ",";
            command += "PatientNameRaw = '" + SOut.String(etrans.PatientNameRaw) + "'";
        }

        if (etrans.UserNum != oldEtrans.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(etrans.UserNum) + "";
        }

        if (command == "") return;
        if (etrans.Note == null) etrans.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(etrans.Note));
        command = "UPDATE etrans SET " + command
                                       + " WHERE EtransNum = " + SOut.Long(etrans.EtransNum);
        Db.NonQ(command, paramNote);
    }
}