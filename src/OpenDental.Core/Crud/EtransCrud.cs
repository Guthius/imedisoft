#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EtransCrud
{
    public static Etrans SelectOne(long etransNum)
    {
        var command = "SELECT * FROM etrans "
                      + "WHERE EtransNum = " + SOut.Long(etransNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

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
        Etrans etrans;
        foreach (DataRow row in table.Rows)
        {
            etrans = new Etrans();
            etrans.EtransNum = SIn.Long(row["EtransNum"].ToString());
            etrans.DateTimeTrans = SIn.DateTime(row["DateTimeTrans"].ToString());
            etrans.ClearingHouseNum = SIn.Long(row["ClearingHouseNum"].ToString());
            etrans.Etype = (EtransType) SIn.Int(row["Etype"].ToString());
            etrans.ClaimNum = SIn.Long(row["ClaimNum"].ToString());
            etrans.OfficeSequenceNumber = SIn.Int(row["OfficeSequenceNumber"].ToString());
            etrans.CarrierTransCounter = SIn.Int(row["CarrierTransCounter"].ToString());
            etrans.CarrierTransCounter2 = SIn.Int(row["CarrierTransCounter2"].ToString());
            etrans.CarrierNum = SIn.Long(row["CarrierNum"].ToString());
            etrans.CarrierNum2 = SIn.Long(row["CarrierNum2"].ToString());
            etrans.PatNum = SIn.Long(row["PatNum"].ToString());
            etrans.BatchNumber = SIn.Int(row["BatchNumber"].ToString());
            etrans.AckCode = SIn.String(row["AckCode"].ToString());
            etrans.TransSetNum = SIn.Int(row["TransSetNum"].ToString());
            etrans.Note = SIn.String(row["Note"].ToString());
            etrans.EtransMessageTextNum = SIn.Long(row["EtransMessageTextNum"].ToString());
            etrans.AckEtransNum = SIn.Long(row["AckEtransNum"].ToString());
            etrans.PlanNum = SIn.Long(row["PlanNum"].ToString());
            etrans.InsSubNum = SIn.Long(row["InsSubNum"].ToString());
            etrans.TranSetId835 = SIn.String(row["TranSetId835"].ToString());
            etrans.CarrierNameRaw = SIn.String(row["CarrierNameRaw"].ToString());
            etrans.PatientNameRaw = SIn.String(row["PatientNameRaw"].ToString());
            etrans.UserNum = SIn.Long(row["UserNum"].ToString());
            retVal.Add(etrans);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Etrans> listEtranss, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Etrans";
        var table = new DataTable(tableName);
        table.Columns.Add("EtransNum");
        table.Columns.Add("DateTimeTrans");
        table.Columns.Add("ClearingHouseNum");
        table.Columns.Add("Etype");
        table.Columns.Add("ClaimNum");
        table.Columns.Add("OfficeSequenceNumber");
        table.Columns.Add("CarrierTransCounter");
        table.Columns.Add("CarrierTransCounter2");
        table.Columns.Add("CarrierNum");
        table.Columns.Add("CarrierNum2");
        table.Columns.Add("PatNum");
        table.Columns.Add("BatchNumber");
        table.Columns.Add("AckCode");
        table.Columns.Add("TransSetNum");
        table.Columns.Add("Note");
        table.Columns.Add("EtransMessageTextNum");
        table.Columns.Add("AckEtransNum");
        table.Columns.Add("PlanNum");
        table.Columns.Add("InsSubNum");
        table.Columns.Add("TranSetId835");
        table.Columns.Add("CarrierNameRaw");
        table.Columns.Add("PatientNameRaw");
        table.Columns.Add("UserNum");
        foreach (var etrans in listEtranss)
            table.Rows.Add(SOut.Long(etrans.EtransNum), SOut.DateT(etrans.DateTimeTrans, false), SOut.Long(etrans.ClearingHouseNum), SOut.Int((int) etrans.Etype), SOut.Long(etrans.ClaimNum), SOut.Int(etrans.OfficeSequenceNumber), SOut.Int(etrans.CarrierTransCounter), SOut.Int(etrans.CarrierTransCounter2), SOut.Long(etrans.CarrierNum), SOut.Long(etrans.CarrierNum2), SOut.Long(etrans.PatNum), SOut.Int(etrans.BatchNumber), etrans.AckCode, SOut.Int(etrans.TransSetNum), etrans.Note, SOut.Long(etrans.EtransMessageTextNum), SOut.Long(etrans.AckEtransNum), SOut.Long(etrans.PlanNum), SOut.Long(etrans.InsSubNum), etrans.TranSetId835, etrans.CarrierNameRaw, etrans.PatientNameRaw, SOut.Long(etrans.UserNum));
        return table;
    }

    public static long Insert(Etrans etrans)
    {
        return Insert(etrans, false);
    }

    public static long Insert(Etrans etrans, bool useExistingPK)
    {
        var command = "INSERT INTO etrans (";

        command += "DateTimeTrans,ClearingHouseNum,Etype,ClaimNum,OfficeSequenceNumber,CarrierTransCounter,CarrierTransCounter2,CarrierNum,CarrierNum2,PatNum,BatchNumber,AckCode,TransSetNum,Note,EtransMessageTextNum,AckEtransNum,PlanNum,InsSubNum,TranSetId835,CarrierNameRaw,PatientNameRaw,UserNum) VALUES(";

        command +=
            DbHelper.Now() + ","
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
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(etrans.Note));
        {
            etrans.EtransNum = Db.NonQ(command, true, "EtransNum", "etrans", paramNote);
        }
        return etrans.EtransNum;
    }

    public static long InsertNoCache(Etrans etrans)
    {
        return InsertNoCache(etrans, false);
    }

    public static long InsertNoCache(Etrans etrans, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO etrans (";
        if (isRandomKeys || useExistingPK) command += "EtransNum,";
        command += "DateTimeTrans,ClearingHouseNum,Etype,ClaimNum,OfficeSequenceNumber,CarrierTransCounter,CarrierTransCounter2,CarrierNum,CarrierNum2,PatNum,BatchNumber,AckCode,TransSetNum,Note,EtransMessageTextNum,AckEtransNum,PlanNum,InsSubNum,TranSetId835,CarrierNameRaw,PatientNameRaw,UserNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(etrans.EtransNum) + ",";
        command +=
            DbHelper.Now() + ","
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
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(etrans.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            etrans.EtransNum = Db.NonQ(command, true, "EtransNum", "etrans", paramNote);
        return etrans.EtransNum;
    }

    public static void Update(Etrans etrans)
    {
        var command = "UPDATE etrans SET "
                      + "DateTimeTrans       =  " + SOut.DateT(etrans.DateTimeTrans) + ", "
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
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(etrans.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(Etrans etrans, Etrans oldEtrans)
    {
        var command = "";
        if (etrans.DateTimeTrans != oldEtrans.DateTimeTrans)
        {
            if (command != "") command += ",";
            command += "DateTimeTrans = " + SOut.DateT(etrans.DateTimeTrans) + "";
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

        if (command == "") return false;
        if (etrans.Note == null) etrans.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(etrans.Note));
        command = "UPDATE etrans SET " + command
                                       + " WHERE EtransNum = " + SOut.Long(etrans.EtransNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(Etrans etrans, Etrans oldEtrans)
    {
        if (etrans.DateTimeTrans != oldEtrans.DateTimeTrans) return true;
        if (etrans.ClearingHouseNum != oldEtrans.ClearingHouseNum) return true;
        if (etrans.Etype != oldEtrans.Etype) return true;
        if (etrans.ClaimNum != oldEtrans.ClaimNum) return true;
        if (etrans.OfficeSequenceNumber != oldEtrans.OfficeSequenceNumber) return true;
        if (etrans.CarrierTransCounter != oldEtrans.CarrierTransCounter) return true;
        if (etrans.CarrierTransCounter2 != oldEtrans.CarrierTransCounter2) return true;
        if (etrans.CarrierNum != oldEtrans.CarrierNum) return true;
        if (etrans.CarrierNum2 != oldEtrans.CarrierNum2) return true;
        if (etrans.PatNum != oldEtrans.PatNum) return true;
        if (etrans.BatchNumber != oldEtrans.BatchNumber) return true;
        if (etrans.AckCode != oldEtrans.AckCode) return true;
        if (etrans.TransSetNum != oldEtrans.TransSetNum) return true;
        if (etrans.Note != oldEtrans.Note) return true;
        if (etrans.EtransMessageTextNum != oldEtrans.EtransMessageTextNum) return true;
        if (etrans.AckEtransNum != oldEtrans.AckEtransNum) return true;
        if (etrans.PlanNum != oldEtrans.PlanNum) return true;
        if (etrans.InsSubNum != oldEtrans.InsSubNum) return true;
        if (etrans.TranSetId835 != oldEtrans.TranSetId835) return true;
        if (etrans.CarrierNameRaw != oldEtrans.CarrierNameRaw) return true;
        if (etrans.PatientNameRaw != oldEtrans.PatientNameRaw) return true;
        if (etrans.UserNum != oldEtrans.UserNum) return true;
        return false;
    }

    public static void Delete(long etransNum)
    {
        var command = "DELETE FROM etrans "
                      + "WHERE EtransNum = " + SOut.Long(etransNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEtransNums)
    {
        if (listEtransNums == null || listEtransNums.Count == 0) return;
        var command = "DELETE FROM etrans "
                      + "WHERE EtransNum IN(" + string.Join(",", listEtransNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}