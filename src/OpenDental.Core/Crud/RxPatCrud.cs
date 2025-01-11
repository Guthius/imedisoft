#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class RxPatCrud
{
    public static RxPat SelectOne(long rxNum)
    {
        var command = "SELECT * FROM rxpat "
                      + "WHERE RxNum = " + SOut.Long(rxNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static RxPat SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<RxPat> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RxPat> TableToList(DataTable table)
    {
        var retVal = new List<RxPat>();
        RxPat rxPat;
        foreach (DataRow row in table.Rows)
        {
            rxPat = new RxPat();
            rxPat.RxNum = SIn.Long(row["RxNum"].ToString());
            rxPat.PatNum = SIn.Long(row["PatNum"].ToString());
            rxPat.RxDate = SIn.Date(row["RxDate"].ToString());
            rxPat.Drug = SIn.String(row["Drug"].ToString());
            rxPat.Sig = SIn.String(row["Sig"].ToString());
            rxPat.Disp = SIn.String(row["Disp"].ToString());
            rxPat.Refills = SIn.String(row["Refills"].ToString());
            rxPat.ProvNum = SIn.Long(row["ProvNum"].ToString());
            rxPat.Notes = SIn.String(row["Notes"].ToString());
            rxPat.PharmacyNum = SIn.Long(row["PharmacyNum"].ToString());
            rxPat.IsControlled = SIn.Bool(row["IsControlled"].ToString());
            rxPat.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            rxPat.SendStatus = (RxSendStatus) SIn.Int(row["SendStatus"].ToString());
            rxPat.RxCui = SIn.Long(row["RxCui"].ToString());
            rxPat.DosageCode = SIn.String(row["DosageCode"].ToString());
            rxPat.ErxGuid = SIn.String(row["ErxGuid"].ToString());
            rxPat.IsErxOld = SIn.Bool(row["IsErxOld"].ToString());
            rxPat.ErxPharmacyInfo = SIn.String(row["ErxPharmacyInfo"].ToString());
            rxPat.IsProcRequired = SIn.Bool(row["IsProcRequired"].ToString());
            rxPat.ProcNum = SIn.Long(row["ProcNum"].ToString());
            rxPat.DaysOfSupply = SIn.Double(row["DaysOfSupply"].ToString());
            rxPat.PatientInstruction = SIn.String(row["PatientInstruction"].ToString());
            rxPat.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            rxPat.UserNum = SIn.Long(row["UserNum"].ToString());
            rxPat.RxType = (RxTypes) SIn.Int(row["RxType"].ToString());
            retVal.Add(rxPat);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<RxPat> listRxPats, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "RxPat";
        var table = new DataTable(tableName);
        table.Columns.Add("RxNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("RxDate");
        table.Columns.Add("Drug");
        table.Columns.Add("Sig");
        table.Columns.Add("Disp");
        table.Columns.Add("Refills");
        table.Columns.Add("ProvNum");
        table.Columns.Add("Notes");
        table.Columns.Add("PharmacyNum");
        table.Columns.Add("IsControlled");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("SendStatus");
        table.Columns.Add("RxCui");
        table.Columns.Add("DosageCode");
        table.Columns.Add("ErxGuid");
        table.Columns.Add("IsErxOld");
        table.Columns.Add("ErxPharmacyInfo");
        table.Columns.Add("IsProcRequired");
        table.Columns.Add("ProcNum");
        table.Columns.Add("DaysOfSupply");
        table.Columns.Add("PatientInstruction");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("UserNum");
        table.Columns.Add("RxType");
        foreach (var rxPat in listRxPats)
            table.Rows.Add(SOut.Long(rxPat.RxNum), SOut.Long(rxPat.PatNum), SOut.DateT(rxPat.RxDate, false), rxPat.Drug, rxPat.Sig, rxPat.Disp, rxPat.Refills, SOut.Long(rxPat.ProvNum), rxPat.Notes, SOut.Long(rxPat.PharmacyNum), SOut.Bool(rxPat.IsControlled), SOut.DateT(rxPat.DateTStamp, false), SOut.Int((int) rxPat.SendStatus), SOut.Long(rxPat.RxCui), rxPat.DosageCode, rxPat.ErxGuid, SOut.Bool(rxPat.IsErxOld), rxPat.ErxPharmacyInfo, SOut.Bool(rxPat.IsProcRequired), SOut.Long(rxPat.ProcNum), SOut.Double(rxPat.DaysOfSupply), rxPat.PatientInstruction, SOut.Long(rxPat.ClinicNum), SOut.Long(rxPat.UserNum), SOut.Int((int) rxPat.RxType));
        return table;
    }

    public static long Insert(RxPat rxPat)
    {
        return Insert(rxPat, false);
    }

    public static long Insert(RxPat rxPat, bool useExistingPK)
    {
        var command = "INSERT INTO rxpat (";

        command += "PatNum,RxDate,Drug,Sig,Disp,Refills,ProvNum,Notes,PharmacyNum,IsControlled,SendStatus,RxCui,DosageCode,ErxGuid,IsErxOld,ErxPharmacyInfo,IsProcRequired,ProcNum,DaysOfSupply,PatientInstruction,ClinicNum,UserNum,RxType) VALUES(";

        command +=
            SOut.Long(rxPat.PatNum) + ","
                                    + SOut.Date(rxPat.RxDate) + ","
                                    + "'" + SOut.String(rxPat.Drug) + "',"
                                    + "'" + SOut.String(rxPat.Sig) + "',"
                                    + "'" + SOut.String(rxPat.Disp) + "',"
                                    + "'" + SOut.String(rxPat.Refills) + "',"
                                    + SOut.Long(rxPat.ProvNum) + ","
                                    + "'" + SOut.String(rxPat.Notes) + "',"
                                    + SOut.Long(rxPat.PharmacyNum) + ","
                                    + SOut.Bool(rxPat.IsControlled) + ","
                                    //DateTStamp can only be set by MySQL
                                    + SOut.Int((int) rxPat.SendStatus) + ","
                                    + SOut.Long(rxPat.RxCui) + ","
                                    + "'" + SOut.String(rxPat.DosageCode) + "',"
                                    + "'" + SOut.String(rxPat.ErxGuid) + "',"
                                    + SOut.Bool(rxPat.IsErxOld) + ","
                                    + "'" + SOut.String(rxPat.ErxPharmacyInfo) + "',"
                                    + SOut.Bool(rxPat.IsProcRequired) + ","
                                    + SOut.Long(rxPat.ProcNum) + ","
                                    + SOut.Double(rxPat.DaysOfSupply) + ","
                                    + DbHelper.ParamChar + "paramPatientInstruction,"
                                    + SOut.Long(rxPat.ClinicNum) + ","
                                    + SOut.Long(rxPat.UserNum) + ","
                                    + SOut.Int((int) rxPat.RxType) + ")";
        if (rxPat.PatientInstruction == null) rxPat.PatientInstruction = "";
        var paramPatientInstruction = new OdSqlParameter("paramPatientInstruction", OdDbType.Text, SOut.StringParam(rxPat.PatientInstruction));
        {
            rxPat.RxNum = Db.NonQ(command, true, "RxNum", "rxPat", paramPatientInstruction);
        }
        return rxPat.RxNum;
    }


    public static long InsertNoCache(RxPat rxPat)
    {
        return InsertNoCache(rxPat, false);
    }


    public static long InsertNoCache(RxPat rxPat, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO rxpat (";
        if (isRandomKeys || useExistingPK) command += "RxNum,";
        command += "PatNum,RxDate,Drug,Sig,Disp,Refills,ProvNum,Notes,PharmacyNum,IsControlled,SendStatus,RxCui,DosageCode,ErxGuid,IsErxOld,ErxPharmacyInfo,IsProcRequired,ProcNum,DaysOfSupply,PatientInstruction,ClinicNum,UserNum,RxType) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(rxPat.RxNum) + ",";
        command +=
            SOut.Long(rxPat.PatNum) + ","
                                    + SOut.Date(rxPat.RxDate) + ","
                                    + "'" + SOut.String(rxPat.Drug) + "',"
                                    + "'" + SOut.String(rxPat.Sig) + "',"
                                    + "'" + SOut.String(rxPat.Disp) + "',"
                                    + "'" + SOut.String(rxPat.Refills) + "',"
                                    + SOut.Long(rxPat.ProvNum) + ","
                                    + "'" + SOut.String(rxPat.Notes) + "',"
                                    + SOut.Long(rxPat.PharmacyNum) + ","
                                    + SOut.Bool(rxPat.IsControlled) + ","
                                    //DateTStamp can only be set by MySQL
                                    + SOut.Int((int) rxPat.SendStatus) + ","
                                    + SOut.Long(rxPat.RxCui) + ","
                                    + "'" + SOut.String(rxPat.DosageCode) + "',"
                                    + "'" + SOut.String(rxPat.ErxGuid) + "',"
                                    + SOut.Bool(rxPat.IsErxOld) + ","
                                    + "'" + SOut.String(rxPat.ErxPharmacyInfo) + "',"
                                    + SOut.Bool(rxPat.IsProcRequired) + ","
                                    + SOut.Long(rxPat.ProcNum) + ","
                                    + SOut.Double(rxPat.DaysOfSupply) + ","
                                    + DbHelper.ParamChar + "paramPatientInstruction,"
                                    + SOut.Long(rxPat.ClinicNum) + ","
                                    + SOut.Long(rxPat.UserNum) + ","
                                    + SOut.Int((int) rxPat.RxType) + ")";
        if (rxPat.PatientInstruction == null) rxPat.PatientInstruction = "";
        var paramPatientInstruction = new OdSqlParameter("paramPatientInstruction", OdDbType.Text, SOut.StringParam(rxPat.PatientInstruction));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramPatientInstruction);
        else
            rxPat.RxNum = Db.NonQ(command, true, "RxNum", "rxPat", paramPatientInstruction);
        return rxPat.RxNum;
    }


    public static void Update(RxPat rxPat)
    {
        var command = "UPDATE rxpat SET "
                      + "PatNum            =  " + SOut.Long(rxPat.PatNum) + ", "
                      + "RxDate            =  " + SOut.Date(rxPat.RxDate) + ", "
                      + "Drug              = '" + SOut.String(rxPat.Drug) + "', "
                      + "Sig               = '" + SOut.String(rxPat.Sig) + "', "
                      + "Disp              = '" + SOut.String(rxPat.Disp) + "', "
                      + "Refills           = '" + SOut.String(rxPat.Refills) + "', "
                      + "ProvNum           =  " + SOut.Long(rxPat.ProvNum) + ", "
                      + "Notes             = '" + SOut.String(rxPat.Notes) + "', "
                      + "PharmacyNum       =  " + SOut.Long(rxPat.PharmacyNum) + ", "
                      + "IsControlled      =  " + SOut.Bool(rxPat.IsControlled) + ", "
                      //DateTStamp can only be set by MySQL
                      + "SendStatus        =  " + SOut.Int((int) rxPat.SendStatus) + ", "
                      + "RxCui             =  " + SOut.Long(rxPat.RxCui) + ", "
                      + "DosageCode        = '" + SOut.String(rxPat.DosageCode) + "', "
                      + "ErxGuid           = '" + SOut.String(rxPat.ErxGuid) + "', "
                      + "IsErxOld          =  " + SOut.Bool(rxPat.IsErxOld) + ", "
                      + "ErxPharmacyInfo   = '" + SOut.String(rxPat.ErxPharmacyInfo) + "', "
                      + "IsProcRequired    =  " + SOut.Bool(rxPat.IsProcRequired) + ", "
                      + "ProcNum           =  " + SOut.Long(rxPat.ProcNum) + ", "
                      + "DaysOfSupply      =  " + SOut.Double(rxPat.DaysOfSupply) + ", "
                      + "PatientInstruction=  " + DbHelper.ParamChar + "paramPatientInstruction, "
                      + "ClinicNum         =  " + SOut.Long(rxPat.ClinicNum) + ", "
                      + "UserNum           =  " + SOut.Long(rxPat.UserNum) + ", "
                      + "RxType            =  " + SOut.Int((int) rxPat.RxType) + " "
                      + "WHERE RxNum = " + SOut.Long(rxPat.RxNum);
        if (rxPat.PatientInstruction == null) rxPat.PatientInstruction = "";
        var paramPatientInstruction = new OdSqlParameter("paramPatientInstruction", OdDbType.Text, SOut.StringParam(rxPat.PatientInstruction));
        Db.NonQ(command, paramPatientInstruction);
    }


    public static bool Update(RxPat rxPat, RxPat oldRxPat)
    {
        var command = "";
        if (rxPat.PatNum != oldRxPat.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(rxPat.PatNum) + "";
        }

        if (rxPat.RxDate.Date != oldRxPat.RxDate.Date)
        {
            if (command != "") command += ",";
            command += "RxDate = " + SOut.Date(rxPat.RxDate) + "";
        }

        if (rxPat.Drug != oldRxPat.Drug)
        {
            if (command != "") command += ",";
            command += "Drug = '" + SOut.String(rxPat.Drug) + "'";
        }

        if (rxPat.Sig != oldRxPat.Sig)
        {
            if (command != "") command += ",";
            command += "Sig = '" + SOut.String(rxPat.Sig) + "'";
        }

        if (rxPat.Disp != oldRxPat.Disp)
        {
            if (command != "") command += ",";
            command += "Disp = '" + SOut.String(rxPat.Disp) + "'";
        }

        if (rxPat.Refills != oldRxPat.Refills)
        {
            if (command != "") command += ",";
            command += "Refills = '" + SOut.String(rxPat.Refills) + "'";
        }

        if (rxPat.ProvNum != oldRxPat.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(rxPat.ProvNum) + "";
        }

        if (rxPat.Notes != oldRxPat.Notes)
        {
            if (command != "") command += ",";
            command += "Notes = '" + SOut.String(rxPat.Notes) + "'";
        }

        if (rxPat.PharmacyNum != oldRxPat.PharmacyNum)
        {
            if (command != "") command += ",";
            command += "PharmacyNum = " + SOut.Long(rxPat.PharmacyNum) + "";
        }

        if (rxPat.IsControlled != oldRxPat.IsControlled)
        {
            if (command != "") command += ",";
            command += "IsControlled = " + SOut.Bool(rxPat.IsControlled) + "";
        }

        //DateTStamp can only be set by MySQL
        if (rxPat.SendStatus != oldRxPat.SendStatus)
        {
            if (command != "") command += ",";
            command += "SendStatus = " + SOut.Int((int) rxPat.SendStatus) + "";
        }

        if (rxPat.RxCui != oldRxPat.RxCui)
        {
            if (command != "") command += ",";
            command += "RxCui = " + SOut.Long(rxPat.RxCui) + "";
        }

        if (rxPat.DosageCode != oldRxPat.DosageCode)
        {
            if (command != "") command += ",";
            command += "DosageCode = '" + SOut.String(rxPat.DosageCode) + "'";
        }

        if (rxPat.ErxGuid != oldRxPat.ErxGuid)
        {
            if (command != "") command += ",";
            command += "ErxGuid = '" + SOut.String(rxPat.ErxGuid) + "'";
        }

        if (rxPat.IsErxOld != oldRxPat.IsErxOld)
        {
            if (command != "") command += ",";
            command += "IsErxOld = " + SOut.Bool(rxPat.IsErxOld) + "";
        }

        if (rxPat.ErxPharmacyInfo != oldRxPat.ErxPharmacyInfo)
        {
            if (command != "") command += ",";
            command += "ErxPharmacyInfo = '" + SOut.String(rxPat.ErxPharmacyInfo) + "'";
        }

        if (rxPat.IsProcRequired != oldRxPat.IsProcRequired)
        {
            if (command != "") command += ",";
            command += "IsProcRequired = " + SOut.Bool(rxPat.IsProcRequired) + "";
        }

        if (rxPat.ProcNum != oldRxPat.ProcNum)
        {
            if (command != "") command += ",";
            command += "ProcNum = " + SOut.Long(rxPat.ProcNum) + "";
        }

        if (rxPat.DaysOfSupply != oldRxPat.DaysOfSupply)
        {
            if (command != "") command += ",";
            command += "DaysOfSupply = " + SOut.Double(rxPat.DaysOfSupply) + "";
        }

        if (rxPat.PatientInstruction != oldRxPat.PatientInstruction)
        {
            if (command != "") command += ",";
            command += "PatientInstruction = " + DbHelper.ParamChar + "paramPatientInstruction";
        }

        if (rxPat.ClinicNum != oldRxPat.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(rxPat.ClinicNum) + "";
        }

        if (rxPat.UserNum != oldRxPat.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(rxPat.UserNum) + "";
        }

        if (rxPat.RxType != oldRxPat.RxType)
        {
            if (command != "") command += ",";
            command += "RxType = " + SOut.Int((int) rxPat.RxType) + "";
        }

        if (command == "") return false;
        if (rxPat.PatientInstruction == null) rxPat.PatientInstruction = "";
        var paramPatientInstruction = new OdSqlParameter("paramPatientInstruction", OdDbType.Text, SOut.StringParam(rxPat.PatientInstruction));
        command = "UPDATE rxpat SET " + command
                                      + " WHERE RxNum = " + SOut.Long(rxPat.RxNum);
        Db.NonQ(command, paramPatientInstruction);
        return true;
    }


    public static bool UpdateComparison(RxPat rxPat, RxPat oldRxPat)
    {
        if (rxPat.PatNum != oldRxPat.PatNum) return true;
        if (rxPat.RxDate.Date != oldRxPat.RxDate.Date) return true;
        if (rxPat.Drug != oldRxPat.Drug) return true;
        if (rxPat.Sig != oldRxPat.Sig) return true;
        if (rxPat.Disp != oldRxPat.Disp) return true;
        if (rxPat.Refills != oldRxPat.Refills) return true;
        if (rxPat.ProvNum != oldRxPat.ProvNum) return true;
        if (rxPat.Notes != oldRxPat.Notes) return true;
        if (rxPat.PharmacyNum != oldRxPat.PharmacyNum) return true;
        if (rxPat.IsControlled != oldRxPat.IsControlled) return true;
        //DateTStamp can only be set by MySQL
        if (rxPat.SendStatus != oldRxPat.SendStatus) return true;
        if (rxPat.RxCui != oldRxPat.RxCui) return true;
        if (rxPat.DosageCode != oldRxPat.DosageCode) return true;
        if (rxPat.ErxGuid != oldRxPat.ErxGuid) return true;
        if (rxPat.IsErxOld != oldRxPat.IsErxOld) return true;
        if (rxPat.ErxPharmacyInfo != oldRxPat.ErxPharmacyInfo) return true;
        if (rxPat.IsProcRequired != oldRxPat.IsProcRequired) return true;
        if (rxPat.ProcNum != oldRxPat.ProcNum) return true;
        if (rxPat.DaysOfSupply != oldRxPat.DaysOfSupply) return true;
        if (rxPat.PatientInstruction != oldRxPat.PatientInstruction) return true;
        if (rxPat.ClinicNum != oldRxPat.ClinicNum) return true;
        if (rxPat.UserNum != oldRxPat.UserNum) return true;
        if (rxPat.RxType != oldRxPat.RxType) return true;
        return false;
    }


    public static void Delete(long rxNum)
    {
        ClearFkey(rxNum);
        var command = "DELETE FROM rxpat "
                      + "WHERE RxNum = " + SOut.Long(rxNum);
        Db.NonQ(command);
    }


    public static void DeleteMany(List<long> listRxNums)
    {
        if (listRxNums == null || listRxNums.Count == 0) return;
        ClearFkey(listRxNums);
        var command = "DELETE FROM rxpat "
                      + "WHERE RxNum IN(" + string.Join(",", listRxNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }


    public static void ClearFkey(long rxNum)
    {
        if (rxNum == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey=" + SOut.Long(rxNum) + " AND PermType IN (9,76)";
        Db.NonQ(command);
    }


    public static void ClearFkey(List<long> listRxNums)
    {
        if (listRxNums == null || listRxNums.FindAll(x => x != 0).Count == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey IN(" + string.Join(",", listRxNums.FindAll(x => x != 0)) + ") AND PermType IN (9,76)";
        Db.NonQ(command);
    }
}