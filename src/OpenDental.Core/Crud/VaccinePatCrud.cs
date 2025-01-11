#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class VaccinePatCrud
{
    public static VaccinePat SelectOne(long vaccinePatNum)
    {
        var command = "SELECT * FROM vaccinepat "
                      + "WHERE VaccinePatNum = " + SOut.Long(vaccinePatNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static VaccinePat SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<VaccinePat> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<VaccinePat> TableToList(DataTable table)
    {
        var retVal = new List<VaccinePat>();
        VaccinePat vaccinePat;
        foreach (DataRow row in table.Rows)
        {
            vaccinePat = new VaccinePat();
            vaccinePat.VaccinePatNum = SIn.Long(row["VaccinePatNum"].ToString());
            vaccinePat.VaccineDefNum = SIn.Long(row["VaccineDefNum"].ToString());
            vaccinePat.DateTimeStart = SIn.DateTime(row["DateTimeStart"].ToString());
            vaccinePat.DateTimeEnd = SIn.DateTime(row["DateTimeEnd"].ToString());
            vaccinePat.AdministeredAmt = SIn.Float(row["AdministeredAmt"].ToString());
            vaccinePat.DrugUnitNum = SIn.Long(row["DrugUnitNum"].ToString());
            vaccinePat.LotNumber = SIn.String(row["LotNumber"].ToString());
            vaccinePat.PatNum = SIn.Long(row["PatNum"].ToString());
            vaccinePat.Note = SIn.String(row["Note"].ToString());
            vaccinePat.FilledCity = SIn.String(row["FilledCity"].ToString());
            vaccinePat.FilledST = SIn.String(row["FilledST"].ToString());
            vaccinePat.CompletionStatus = (VaccineCompletionStatus) SIn.Int(row["CompletionStatus"].ToString());
            vaccinePat.AdministrationNoteCode = (VaccineAdministrationNote) SIn.Int(row["AdministrationNoteCode"].ToString());
            vaccinePat.UserNum = SIn.Long(row["UserNum"].ToString());
            vaccinePat.ProvNumOrdering = SIn.Long(row["ProvNumOrdering"].ToString());
            vaccinePat.ProvNumAdminister = SIn.Long(row["ProvNumAdminister"].ToString());
            vaccinePat.DateExpire = SIn.Date(row["DateExpire"].ToString());
            vaccinePat.RefusalReason = (VaccineRefusalReason) SIn.Int(row["RefusalReason"].ToString());
            vaccinePat.ActionCode = (VaccineAction) SIn.Int(row["ActionCode"].ToString());
            vaccinePat.AdministrationRoute = (VaccineAdministrationRoute) SIn.Int(row["AdministrationRoute"].ToString());
            vaccinePat.AdministrationSite = (VaccineAdministrationSite) SIn.Int(row["AdministrationSite"].ToString());
            retVal.Add(vaccinePat);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<VaccinePat> listVaccinePats, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "VaccinePat";
        var table = new DataTable(tableName);
        table.Columns.Add("VaccinePatNum");
        table.Columns.Add("VaccineDefNum");
        table.Columns.Add("DateTimeStart");
        table.Columns.Add("DateTimeEnd");
        table.Columns.Add("AdministeredAmt");
        table.Columns.Add("DrugUnitNum");
        table.Columns.Add("LotNumber");
        table.Columns.Add("PatNum");
        table.Columns.Add("Note");
        table.Columns.Add("FilledCity");
        table.Columns.Add("FilledST");
        table.Columns.Add("CompletionStatus");
        table.Columns.Add("AdministrationNoteCode");
        table.Columns.Add("UserNum");
        table.Columns.Add("ProvNumOrdering");
        table.Columns.Add("ProvNumAdminister");
        table.Columns.Add("DateExpire");
        table.Columns.Add("RefusalReason");
        table.Columns.Add("ActionCode");
        table.Columns.Add("AdministrationRoute");
        table.Columns.Add("AdministrationSite");
        foreach (var vaccinePat in listVaccinePats)
            table.Rows.Add(SOut.Long(vaccinePat.VaccinePatNum), SOut.Long(vaccinePat.VaccineDefNum), SOut.DateT(vaccinePat.DateTimeStart, false), SOut.DateT(vaccinePat.DateTimeEnd, false), SOut.Float(vaccinePat.AdministeredAmt), SOut.Long(vaccinePat.DrugUnitNum), vaccinePat.LotNumber, SOut.Long(vaccinePat.PatNum), vaccinePat.Note, vaccinePat.FilledCity, vaccinePat.FilledST, SOut.Int((int) vaccinePat.CompletionStatus), SOut.Int((int) vaccinePat.AdministrationNoteCode), SOut.Long(vaccinePat.UserNum), SOut.Long(vaccinePat.ProvNumOrdering), SOut.Long(vaccinePat.ProvNumAdminister), SOut.DateT(vaccinePat.DateExpire, false), SOut.Int((int) vaccinePat.RefusalReason), SOut.Int((int) vaccinePat.ActionCode), SOut.Int((int) vaccinePat.AdministrationRoute), SOut.Int((int) vaccinePat.AdministrationSite));
        return table;
    }

    public static long Insert(VaccinePat vaccinePat)
    {
        return Insert(vaccinePat, false);
    }

    public static long Insert(VaccinePat vaccinePat, bool useExistingPK)
    {
        var command = "INSERT INTO vaccinepat (";

        command += "VaccineDefNum,DateTimeStart,DateTimeEnd,AdministeredAmt,DrugUnitNum,LotNumber,PatNum,Note,FilledCity,FilledST,CompletionStatus,AdministrationNoteCode,UserNum,ProvNumOrdering,ProvNumAdminister,DateExpire,RefusalReason,ActionCode,AdministrationRoute,AdministrationSite) VALUES(";

        command +=
            SOut.Long(vaccinePat.VaccineDefNum) + ","
                                                + SOut.DateT(vaccinePat.DateTimeStart) + ","
                                                + SOut.DateT(vaccinePat.DateTimeEnd) + ","
                                                + SOut.Float(vaccinePat.AdministeredAmt) + ","
                                                + SOut.Long(vaccinePat.DrugUnitNum) + ","
                                                + "'" + SOut.String(vaccinePat.LotNumber) + "',"
                                                + SOut.Long(vaccinePat.PatNum) + ","
                                                + DbHelper.ParamChar + "paramNote,"
                                                + "'" + SOut.String(vaccinePat.FilledCity) + "',"
                                                + "'" + SOut.String(vaccinePat.FilledST) + "',"
                                                + SOut.Int((int) vaccinePat.CompletionStatus) + ","
                                                + SOut.Int((int) vaccinePat.AdministrationNoteCode) + ","
                                                + SOut.Long(vaccinePat.UserNum) + ","
                                                + SOut.Long(vaccinePat.ProvNumOrdering) + ","
                                                + SOut.Long(vaccinePat.ProvNumAdminister) + ","
                                                + SOut.Date(vaccinePat.DateExpire) + ","
                                                + SOut.Int((int) vaccinePat.RefusalReason) + ","
                                                + SOut.Int((int) vaccinePat.ActionCode) + ","
                                                + SOut.Int((int) vaccinePat.AdministrationRoute) + ","
                                                + SOut.Int((int) vaccinePat.AdministrationSite) + ")";
        if (vaccinePat.Note == null) vaccinePat.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(vaccinePat.Note));
        {
            vaccinePat.VaccinePatNum = Db.NonQ(command, true, "VaccinePatNum", "vaccinePat", paramNote);
        }
        return vaccinePat.VaccinePatNum;
    }

    public static long InsertNoCache(VaccinePat vaccinePat)
    {
        return InsertNoCache(vaccinePat, false);
    }

    public static long InsertNoCache(VaccinePat vaccinePat, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO vaccinepat (";
        if (isRandomKeys || useExistingPK) command += "VaccinePatNum,";
        command += "VaccineDefNum,DateTimeStart,DateTimeEnd,AdministeredAmt,DrugUnitNum,LotNumber,PatNum,Note,FilledCity,FilledST,CompletionStatus,AdministrationNoteCode,UserNum,ProvNumOrdering,ProvNumAdminister,DateExpire,RefusalReason,ActionCode,AdministrationRoute,AdministrationSite) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(vaccinePat.VaccinePatNum) + ",";
        command +=
            SOut.Long(vaccinePat.VaccineDefNum) + ","
                                                + SOut.DateT(vaccinePat.DateTimeStart) + ","
                                                + SOut.DateT(vaccinePat.DateTimeEnd) + ","
                                                + SOut.Float(vaccinePat.AdministeredAmt) + ","
                                                + SOut.Long(vaccinePat.DrugUnitNum) + ","
                                                + "'" + SOut.String(vaccinePat.LotNumber) + "',"
                                                + SOut.Long(vaccinePat.PatNum) + ","
                                                + DbHelper.ParamChar + "paramNote,"
                                                + "'" + SOut.String(vaccinePat.FilledCity) + "',"
                                                + "'" + SOut.String(vaccinePat.FilledST) + "',"
                                                + SOut.Int((int) vaccinePat.CompletionStatus) + ","
                                                + SOut.Int((int) vaccinePat.AdministrationNoteCode) + ","
                                                + SOut.Long(vaccinePat.UserNum) + ","
                                                + SOut.Long(vaccinePat.ProvNumOrdering) + ","
                                                + SOut.Long(vaccinePat.ProvNumAdminister) + ","
                                                + SOut.Date(vaccinePat.DateExpire) + ","
                                                + SOut.Int((int) vaccinePat.RefusalReason) + ","
                                                + SOut.Int((int) vaccinePat.ActionCode) + ","
                                                + SOut.Int((int) vaccinePat.AdministrationRoute) + ","
                                                + SOut.Int((int) vaccinePat.AdministrationSite) + ")";
        if (vaccinePat.Note == null) vaccinePat.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(vaccinePat.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            vaccinePat.VaccinePatNum = Db.NonQ(command, true, "VaccinePatNum", "vaccinePat", paramNote);
        return vaccinePat.VaccinePatNum;
    }

    public static void Update(VaccinePat vaccinePat)
    {
        var command = "UPDATE vaccinepat SET "
                      + "VaccineDefNum         =  " + SOut.Long(vaccinePat.VaccineDefNum) + ", "
                      + "DateTimeStart         =  " + SOut.DateT(vaccinePat.DateTimeStart) + ", "
                      + "DateTimeEnd           =  " + SOut.DateT(vaccinePat.DateTimeEnd) + ", "
                      + "AdministeredAmt       =  " + SOut.Float(vaccinePat.AdministeredAmt) + ", "
                      + "DrugUnitNum           =  " + SOut.Long(vaccinePat.DrugUnitNum) + ", "
                      + "LotNumber             = '" + SOut.String(vaccinePat.LotNumber) + "', "
                      + "PatNum                =  " + SOut.Long(vaccinePat.PatNum) + ", "
                      + "Note                  =  " + DbHelper.ParamChar + "paramNote, "
                      + "FilledCity            = '" + SOut.String(vaccinePat.FilledCity) + "', "
                      + "FilledST              = '" + SOut.String(vaccinePat.FilledST) + "', "
                      + "CompletionStatus      =  " + SOut.Int((int) vaccinePat.CompletionStatus) + ", "
                      + "AdministrationNoteCode=  " + SOut.Int((int) vaccinePat.AdministrationNoteCode) + ", "
                      + "UserNum               =  " + SOut.Long(vaccinePat.UserNum) + ", "
                      + "ProvNumOrdering       =  " + SOut.Long(vaccinePat.ProvNumOrdering) + ", "
                      + "ProvNumAdminister     =  " + SOut.Long(vaccinePat.ProvNumAdminister) + ", "
                      + "DateExpire            =  " + SOut.Date(vaccinePat.DateExpire) + ", "
                      + "RefusalReason         =  " + SOut.Int((int) vaccinePat.RefusalReason) + ", "
                      + "ActionCode            =  " + SOut.Int((int) vaccinePat.ActionCode) + ", "
                      + "AdministrationRoute   =  " + SOut.Int((int) vaccinePat.AdministrationRoute) + ", "
                      + "AdministrationSite    =  " + SOut.Int((int) vaccinePat.AdministrationSite) + " "
                      + "WHERE VaccinePatNum = " + SOut.Long(vaccinePat.VaccinePatNum);
        if (vaccinePat.Note == null) vaccinePat.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(vaccinePat.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(VaccinePat vaccinePat, VaccinePat oldVaccinePat)
    {
        var command = "";
        if (vaccinePat.VaccineDefNum != oldVaccinePat.VaccineDefNum)
        {
            if (command != "") command += ",";
            command += "VaccineDefNum = " + SOut.Long(vaccinePat.VaccineDefNum) + "";
        }

        if (vaccinePat.DateTimeStart != oldVaccinePat.DateTimeStart)
        {
            if (command != "") command += ",";
            command += "DateTimeStart = " + SOut.DateT(vaccinePat.DateTimeStart) + "";
        }

        if (vaccinePat.DateTimeEnd != oldVaccinePat.DateTimeEnd)
        {
            if (command != "") command += ",";
            command += "DateTimeEnd = " + SOut.DateT(vaccinePat.DateTimeEnd) + "";
        }

        if (vaccinePat.AdministeredAmt != oldVaccinePat.AdministeredAmt)
        {
            if (command != "") command += ",";
            command += "AdministeredAmt = " + SOut.Float(vaccinePat.AdministeredAmt) + "";
        }

        if (vaccinePat.DrugUnitNum != oldVaccinePat.DrugUnitNum)
        {
            if (command != "") command += ",";
            command += "DrugUnitNum = " + SOut.Long(vaccinePat.DrugUnitNum) + "";
        }

        if (vaccinePat.LotNumber != oldVaccinePat.LotNumber)
        {
            if (command != "") command += ",";
            command += "LotNumber = '" + SOut.String(vaccinePat.LotNumber) + "'";
        }

        if (vaccinePat.PatNum != oldVaccinePat.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(vaccinePat.PatNum) + "";
        }

        if (vaccinePat.Note != oldVaccinePat.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (vaccinePat.FilledCity != oldVaccinePat.FilledCity)
        {
            if (command != "") command += ",";
            command += "FilledCity = '" + SOut.String(vaccinePat.FilledCity) + "'";
        }

        if (vaccinePat.FilledST != oldVaccinePat.FilledST)
        {
            if (command != "") command += ",";
            command += "FilledST = '" + SOut.String(vaccinePat.FilledST) + "'";
        }

        if (vaccinePat.CompletionStatus != oldVaccinePat.CompletionStatus)
        {
            if (command != "") command += ",";
            command += "CompletionStatus = " + SOut.Int((int) vaccinePat.CompletionStatus) + "";
        }

        if (vaccinePat.AdministrationNoteCode != oldVaccinePat.AdministrationNoteCode)
        {
            if (command != "") command += ",";
            command += "AdministrationNoteCode = " + SOut.Int((int) vaccinePat.AdministrationNoteCode) + "";
        }

        if (vaccinePat.UserNum != oldVaccinePat.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(vaccinePat.UserNum) + "";
        }

        if (vaccinePat.ProvNumOrdering != oldVaccinePat.ProvNumOrdering)
        {
            if (command != "") command += ",";
            command += "ProvNumOrdering = " + SOut.Long(vaccinePat.ProvNumOrdering) + "";
        }

        if (vaccinePat.ProvNumAdminister != oldVaccinePat.ProvNumAdminister)
        {
            if (command != "") command += ",";
            command += "ProvNumAdminister = " + SOut.Long(vaccinePat.ProvNumAdminister) + "";
        }

        if (vaccinePat.DateExpire.Date != oldVaccinePat.DateExpire.Date)
        {
            if (command != "") command += ",";
            command += "DateExpire = " + SOut.Date(vaccinePat.DateExpire) + "";
        }

        if (vaccinePat.RefusalReason != oldVaccinePat.RefusalReason)
        {
            if (command != "") command += ",";
            command += "RefusalReason = " + SOut.Int((int) vaccinePat.RefusalReason) + "";
        }

        if (vaccinePat.ActionCode != oldVaccinePat.ActionCode)
        {
            if (command != "") command += ",";
            command += "ActionCode = " + SOut.Int((int) vaccinePat.ActionCode) + "";
        }

        if (vaccinePat.AdministrationRoute != oldVaccinePat.AdministrationRoute)
        {
            if (command != "") command += ",";
            command += "AdministrationRoute = " + SOut.Int((int) vaccinePat.AdministrationRoute) + "";
        }

        if (vaccinePat.AdministrationSite != oldVaccinePat.AdministrationSite)
        {
            if (command != "") command += ",";
            command += "AdministrationSite = " + SOut.Int((int) vaccinePat.AdministrationSite) + "";
        }

        if (command == "") return false;
        if (vaccinePat.Note == null) vaccinePat.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(vaccinePat.Note));
        command = "UPDATE vaccinepat SET " + command
                                           + " WHERE VaccinePatNum = " + SOut.Long(vaccinePat.VaccinePatNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(VaccinePat vaccinePat, VaccinePat oldVaccinePat)
    {
        if (vaccinePat.VaccineDefNum != oldVaccinePat.VaccineDefNum) return true;
        if (vaccinePat.DateTimeStart != oldVaccinePat.DateTimeStart) return true;
        if (vaccinePat.DateTimeEnd != oldVaccinePat.DateTimeEnd) return true;
        if (vaccinePat.AdministeredAmt != oldVaccinePat.AdministeredAmt) return true;
        if (vaccinePat.DrugUnitNum != oldVaccinePat.DrugUnitNum) return true;
        if (vaccinePat.LotNumber != oldVaccinePat.LotNumber) return true;
        if (vaccinePat.PatNum != oldVaccinePat.PatNum) return true;
        if (vaccinePat.Note != oldVaccinePat.Note) return true;
        if (vaccinePat.FilledCity != oldVaccinePat.FilledCity) return true;
        if (vaccinePat.FilledST != oldVaccinePat.FilledST) return true;
        if (vaccinePat.CompletionStatus != oldVaccinePat.CompletionStatus) return true;
        if (vaccinePat.AdministrationNoteCode != oldVaccinePat.AdministrationNoteCode) return true;
        if (vaccinePat.UserNum != oldVaccinePat.UserNum) return true;
        if (vaccinePat.ProvNumOrdering != oldVaccinePat.ProvNumOrdering) return true;
        if (vaccinePat.ProvNumAdminister != oldVaccinePat.ProvNumAdminister) return true;
        if (vaccinePat.DateExpire.Date != oldVaccinePat.DateExpire.Date) return true;
        if (vaccinePat.RefusalReason != oldVaccinePat.RefusalReason) return true;
        if (vaccinePat.ActionCode != oldVaccinePat.ActionCode) return true;
        if (vaccinePat.AdministrationRoute != oldVaccinePat.AdministrationRoute) return true;
        if (vaccinePat.AdministrationSite != oldVaccinePat.AdministrationSite) return true;
        return false;
    }

    public static void Delete(long vaccinePatNum)
    {
        var command = "DELETE FROM vaccinepat "
                      + "WHERE VaccinePatNum = " + SOut.Long(vaccinePatNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listVaccinePatNums)
    {
        if (listVaccinePatNums == null || listVaccinePatNums.Count == 0) return;
        var command = "DELETE FROM vaccinepat "
                      + "WHERE VaccinePatNum IN(" + string.Join(",", listVaccinePatNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}