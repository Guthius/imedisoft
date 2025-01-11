#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class RepeatChargeCrud
{
    public static RepeatCharge SelectOne(long repeatChargeNum)
    {
        var command = "SELECT * FROM repeatcharge "
                      + "WHERE RepeatChargeNum = " + SOut.Long(repeatChargeNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static RepeatCharge SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<RepeatCharge> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RepeatCharge> TableToList(DataTable table)
    {
        var retVal = new List<RepeatCharge>();
        RepeatCharge repeatCharge;
        foreach (DataRow row in table.Rows)
        {
            repeatCharge = new RepeatCharge();
            repeatCharge.RepeatChargeNum = SIn.Long(row["RepeatChargeNum"].ToString());
            repeatCharge.PatNum = SIn.Long(row["PatNum"].ToString());
            repeatCharge.ProcCode = SIn.String(row["ProcCode"].ToString());
            repeatCharge.ChargeAmt = SIn.Double(row["ChargeAmt"].ToString());
            repeatCharge.DateStart = SIn.Date(row["DateStart"].ToString());
            repeatCharge.DateStop = SIn.Date(row["DateStop"].ToString());
            repeatCharge.Note = SIn.String(row["Note"].ToString());
            repeatCharge.CopyNoteToProc = SIn.Bool(row["CopyNoteToProc"].ToString());
            repeatCharge.CreatesClaim = SIn.Bool(row["CreatesClaim"].ToString());
            repeatCharge.IsEnabled = SIn.Bool(row["IsEnabled"].ToString());
            repeatCharge.UsePrepay = SIn.Bool(row["UsePrepay"].ToString());
            repeatCharge.Npi = SIn.String(row["Npi"].ToString());
            repeatCharge.ErxAccountId = SIn.String(row["ErxAccountId"].ToString());
            repeatCharge.ProviderName = SIn.String(row["ProviderName"].ToString());
            repeatCharge.ChargeAmtAlt = SIn.Double(row["ChargeAmtAlt"].ToString());
            repeatCharge.UnearnedTypes = SIn.String(row["UnearnedTypes"].ToString());
            repeatCharge.Frequency = (EnumRepeatChargeFrequency) SIn.Int(row["Frequency"].ToString());
            retVal.Add(repeatCharge);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<RepeatCharge> listRepeatCharges, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "RepeatCharge";
        var table = new DataTable(tableName);
        table.Columns.Add("RepeatChargeNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ProcCode");
        table.Columns.Add("ChargeAmt");
        table.Columns.Add("DateStart");
        table.Columns.Add("DateStop");
        table.Columns.Add("Note");
        table.Columns.Add("CopyNoteToProc");
        table.Columns.Add("CreatesClaim");
        table.Columns.Add("IsEnabled");
        table.Columns.Add("UsePrepay");
        table.Columns.Add("Npi");
        table.Columns.Add("ErxAccountId");
        table.Columns.Add("ProviderName");
        table.Columns.Add("ChargeAmtAlt");
        table.Columns.Add("UnearnedTypes");
        table.Columns.Add("Frequency");
        foreach (var repeatCharge in listRepeatCharges)
            table.Rows.Add(SOut.Long(repeatCharge.RepeatChargeNum), SOut.Long(repeatCharge.PatNum), repeatCharge.ProcCode, SOut.Double(repeatCharge.ChargeAmt), SOut.DateT(repeatCharge.DateStart, false), SOut.DateT(repeatCharge.DateStop, false), repeatCharge.Note, SOut.Bool(repeatCharge.CopyNoteToProc), SOut.Bool(repeatCharge.CreatesClaim), SOut.Bool(repeatCharge.IsEnabled), SOut.Bool(repeatCharge.UsePrepay), repeatCharge.Npi, repeatCharge.ErxAccountId, repeatCharge.ProviderName, SOut.Double(repeatCharge.ChargeAmtAlt, 4), repeatCharge.UnearnedTypes, SOut.Int((int) repeatCharge.Frequency));
        return table;
    }

    public static long Insert(RepeatCharge repeatCharge)
    {
        return Insert(repeatCharge, false);
    }

    public static long Insert(RepeatCharge repeatCharge, bool useExistingPK)
    {
        var command = "INSERT INTO repeatcharge (";

        command += "PatNum,ProcCode,ChargeAmt,DateStart,DateStop,Note,CopyNoteToProc,CreatesClaim,IsEnabled,UsePrepay,Npi,ErxAccountId,ProviderName,ChargeAmtAlt,UnearnedTypes,Frequency) VALUES(";

        command +=
            SOut.Long(repeatCharge.PatNum) + ","
                                           + "'" + SOut.String(repeatCharge.ProcCode) + "',"
                                           + SOut.Double(repeatCharge.ChargeAmt) + ","
                                           + SOut.Date(repeatCharge.DateStart) + ","
                                           + SOut.Date(repeatCharge.DateStop) + ","
                                           + DbHelper.ParamChar + "paramNote,"
                                           + SOut.Bool(repeatCharge.CopyNoteToProc) + ","
                                           + SOut.Bool(repeatCharge.CreatesClaim) + ","
                                           + SOut.Bool(repeatCharge.IsEnabled) + ","
                                           + SOut.Bool(repeatCharge.UsePrepay) + ","
                                           + DbHelper.ParamChar + "paramNpi,"
                                           + DbHelper.ParamChar + "paramErxAccountId,"
                                           + DbHelper.ParamChar + "paramProviderName,"
                                           + SOut.Double(repeatCharge.ChargeAmtAlt, 4) + ","
                                           + "'" + SOut.String(repeatCharge.UnearnedTypes) + "',"
                                           + SOut.Int((int) repeatCharge.Frequency) + ")";
        if (repeatCharge.Note == null) repeatCharge.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(repeatCharge.Note));
        if (repeatCharge.Npi == null) repeatCharge.Npi = "";
        var paramNpi = new OdSqlParameter("paramNpi", OdDbType.Text, SOut.StringParam(repeatCharge.Npi));
        if (repeatCharge.ErxAccountId == null) repeatCharge.ErxAccountId = "";
        var paramErxAccountId = new OdSqlParameter("paramErxAccountId", OdDbType.Text, SOut.StringParam(repeatCharge.ErxAccountId));
        if (repeatCharge.ProviderName == null) repeatCharge.ProviderName = "";
        var paramProviderName = new OdSqlParameter("paramProviderName", OdDbType.Text, SOut.StringParam(repeatCharge.ProviderName));
        {
            repeatCharge.RepeatChargeNum = Db.NonQ(command, true, "RepeatChargeNum", "repeatCharge", paramNote, paramNpi, paramErxAccountId, paramProviderName);
        }
        return repeatCharge.RepeatChargeNum;
    }

    public static long InsertNoCache(RepeatCharge repeatCharge)
    {
        return InsertNoCache(repeatCharge, false);
    }

    public static long InsertNoCache(RepeatCharge repeatCharge, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO repeatcharge (";
        if (isRandomKeys || useExistingPK) command += "RepeatChargeNum,";
        command += "PatNum,ProcCode,ChargeAmt,DateStart,DateStop,Note,CopyNoteToProc,CreatesClaim,IsEnabled,UsePrepay,Npi,ErxAccountId,ProviderName,ChargeAmtAlt,UnearnedTypes,Frequency) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(repeatCharge.RepeatChargeNum) + ",";
        command +=
            SOut.Long(repeatCharge.PatNum) + ","
                                           + "'" + SOut.String(repeatCharge.ProcCode) + "',"
                                           + SOut.Double(repeatCharge.ChargeAmt) + ","
                                           + SOut.Date(repeatCharge.DateStart) + ","
                                           + SOut.Date(repeatCharge.DateStop) + ","
                                           + DbHelper.ParamChar + "paramNote,"
                                           + SOut.Bool(repeatCharge.CopyNoteToProc) + ","
                                           + SOut.Bool(repeatCharge.CreatesClaim) + ","
                                           + SOut.Bool(repeatCharge.IsEnabled) + ","
                                           + SOut.Bool(repeatCharge.UsePrepay) + ","
                                           + DbHelper.ParamChar + "paramNpi,"
                                           + DbHelper.ParamChar + "paramErxAccountId,"
                                           + DbHelper.ParamChar + "paramProviderName,"
                                           + SOut.Double(repeatCharge.ChargeAmtAlt, 4) + ","
                                           + "'" + SOut.String(repeatCharge.UnearnedTypes) + "',"
                                           + SOut.Int((int) repeatCharge.Frequency) + ")";
        if (repeatCharge.Note == null) repeatCharge.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(repeatCharge.Note));
        if (repeatCharge.Npi == null) repeatCharge.Npi = "";
        var paramNpi = new OdSqlParameter("paramNpi", OdDbType.Text, SOut.StringParam(repeatCharge.Npi));
        if (repeatCharge.ErxAccountId == null) repeatCharge.ErxAccountId = "";
        var paramErxAccountId = new OdSqlParameter("paramErxAccountId", OdDbType.Text, SOut.StringParam(repeatCharge.ErxAccountId));
        if (repeatCharge.ProviderName == null) repeatCharge.ProviderName = "";
        var paramProviderName = new OdSqlParameter("paramProviderName", OdDbType.Text, SOut.StringParam(repeatCharge.ProviderName));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote, paramNpi, paramErxAccountId, paramProviderName);
        else
            repeatCharge.RepeatChargeNum = Db.NonQ(command, true, "RepeatChargeNum", "repeatCharge", paramNote, paramNpi, paramErxAccountId, paramProviderName);
        return repeatCharge.RepeatChargeNum;
    }

    public static void Update(RepeatCharge repeatCharge)
    {
        var command = "UPDATE repeatcharge SET "
                      + "PatNum         =  " + SOut.Long(repeatCharge.PatNum) + ", "
                      + "ProcCode       = '" + SOut.String(repeatCharge.ProcCode) + "', "
                      + "ChargeAmt      =  " + SOut.Double(repeatCharge.ChargeAmt) + ", "
                      + "DateStart      =  " + SOut.Date(repeatCharge.DateStart) + ", "
                      + "DateStop       =  " + SOut.Date(repeatCharge.DateStop) + ", "
                      + "Note           =  " + DbHelper.ParamChar + "paramNote, "
                      + "CopyNoteToProc =  " + SOut.Bool(repeatCharge.CopyNoteToProc) + ", "
                      + "CreatesClaim   =  " + SOut.Bool(repeatCharge.CreatesClaim) + ", "
                      + "IsEnabled      =  " + SOut.Bool(repeatCharge.IsEnabled) + ", "
                      + "UsePrepay      =  " + SOut.Bool(repeatCharge.UsePrepay) + ", "
                      + "Npi            =  " + DbHelper.ParamChar + "paramNpi, "
                      + "ErxAccountId   =  " + DbHelper.ParamChar + "paramErxAccountId, "
                      + "ProviderName   =  " + DbHelper.ParamChar + "paramProviderName, "
                      + "ChargeAmtAlt   =  " + SOut.Double(repeatCharge.ChargeAmtAlt, 4) + ", "
                      + "UnearnedTypes  = '" + SOut.String(repeatCharge.UnearnedTypes) + "', "
                      + "Frequency      =  " + SOut.Int((int) repeatCharge.Frequency) + " "
                      + "WHERE RepeatChargeNum = " + SOut.Long(repeatCharge.RepeatChargeNum);
        if (repeatCharge.Note == null) repeatCharge.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(repeatCharge.Note));
        if (repeatCharge.Npi == null) repeatCharge.Npi = "";
        var paramNpi = new OdSqlParameter("paramNpi", OdDbType.Text, SOut.StringParam(repeatCharge.Npi));
        if (repeatCharge.ErxAccountId == null) repeatCharge.ErxAccountId = "";
        var paramErxAccountId = new OdSqlParameter("paramErxAccountId", OdDbType.Text, SOut.StringParam(repeatCharge.ErxAccountId));
        if (repeatCharge.ProviderName == null) repeatCharge.ProviderName = "";
        var paramProviderName = new OdSqlParameter("paramProviderName", OdDbType.Text, SOut.StringParam(repeatCharge.ProviderName));
        Db.NonQ(command, paramNote, paramNpi, paramErxAccountId, paramProviderName);
    }

    public static bool Update(RepeatCharge repeatCharge, RepeatCharge oldRepeatCharge)
    {
        var command = "";
        if (repeatCharge.PatNum != oldRepeatCharge.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(repeatCharge.PatNum) + "";
        }

        if (repeatCharge.ProcCode != oldRepeatCharge.ProcCode)
        {
            if (command != "") command += ",";
            command += "ProcCode = '" + SOut.String(repeatCharge.ProcCode) + "'";
        }

        if (repeatCharge.ChargeAmt != oldRepeatCharge.ChargeAmt)
        {
            if (command != "") command += ",";
            command += "ChargeAmt = " + SOut.Double(repeatCharge.ChargeAmt) + "";
        }

        if (repeatCharge.DateStart.Date != oldRepeatCharge.DateStart.Date)
        {
            if (command != "") command += ",";
            command += "DateStart = " + SOut.Date(repeatCharge.DateStart) + "";
        }

        if (repeatCharge.DateStop.Date != oldRepeatCharge.DateStop.Date)
        {
            if (command != "") command += ",";
            command += "DateStop = " + SOut.Date(repeatCharge.DateStop) + "";
        }

        if (repeatCharge.Note != oldRepeatCharge.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (repeatCharge.CopyNoteToProc != oldRepeatCharge.CopyNoteToProc)
        {
            if (command != "") command += ",";
            command += "CopyNoteToProc = " + SOut.Bool(repeatCharge.CopyNoteToProc) + "";
        }

        if (repeatCharge.CreatesClaim != oldRepeatCharge.CreatesClaim)
        {
            if (command != "") command += ",";
            command += "CreatesClaim = " + SOut.Bool(repeatCharge.CreatesClaim) + "";
        }

        if (repeatCharge.IsEnabled != oldRepeatCharge.IsEnabled)
        {
            if (command != "") command += ",";
            command += "IsEnabled = " + SOut.Bool(repeatCharge.IsEnabled) + "";
        }

        if (repeatCharge.UsePrepay != oldRepeatCharge.UsePrepay)
        {
            if (command != "") command += ",";
            command += "UsePrepay = " + SOut.Bool(repeatCharge.UsePrepay) + "";
        }

        if (repeatCharge.Npi != oldRepeatCharge.Npi)
        {
            if (command != "") command += ",";
            command += "Npi = " + DbHelper.ParamChar + "paramNpi";
        }

        if (repeatCharge.ErxAccountId != oldRepeatCharge.ErxAccountId)
        {
            if (command != "") command += ",";
            command += "ErxAccountId = " + DbHelper.ParamChar + "paramErxAccountId";
        }

        if (repeatCharge.ProviderName != oldRepeatCharge.ProviderName)
        {
            if (command != "") command += ",";
            command += "ProviderName = " + DbHelper.ParamChar + "paramProviderName";
        }

        if (repeatCharge.ChargeAmtAlt != oldRepeatCharge.ChargeAmtAlt)
        {
            if (command != "") command += ",";
            command += "ChargeAmtAlt = " + SOut.Double(repeatCharge.ChargeAmtAlt, 4) + "";
        }

        if (repeatCharge.UnearnedTypes != oldRepeatCharge.UnearnedTypes)
        {
            if (command != "") command += ",";
            command += "UnearnedTypes = '" + SOut.String(repeatCharge.UnearnedTypes) + "'";
        }

        if (repeatCharge.Frequency != oldRepeatCharge.Frequency)
        {
            if (command != "") command += ",";
            command += "Frequency = " + SOut.Int((int) repeatCharge.Frequency) + "";
        }

        if (command == "") return false;
        if (repeatCharge.Note == null) repeatCharge.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(repeatCharge.Note));
        if (repeatCharge.Npi == null) repeatCharge.Npi = "";
        var paramNpi = new OdSqlParameter("paramNpi", OdDbType.Text, SOut.StringParam(repeatCharge.Npi));
        if (repeatCharge.ErxAccountId == null) repeatCharge.ErxAccountId = "";
        var paramErxAccountId = new OdSqlParameter("paramErxAccountId", OdDbType.Text, SOut.StringParam(repeatCharge.ErxAccountId));
        if (repeatCharge.ProviderName == null) repeatCharge.ProviderName = "";
        var paramProviderName = new OdSqlParameter("paramProviderName", OdDbType.Text, SOut.StringParam(repeatCharge.ProviderName));
        command = "UPDATE repeatcharge SET " + command
                                             + " WHERE RepeatChargeNum = " + SOut.Long(repeatCharge.RepeatChargeNum);
        Db.NonQ(command, paramNote, paramNpi, paramErxAccountId, paramProviderName);
        return true;
    }

    public static bool UpdateComparison(RepeatCharge repeatCharge, RepeatCharge oldRepeatCharge)
    {
        if (repeatCharge.PatNum != oldRepeatCharge.PatNum) return true;
        if (repeatCharge.ProcCode != oldRepeatCharge.ProcCode) return true;
        if (repeatCharge.ChargeAmt != oldRepeatCharge.ChargeAmt) return true;
        if (repeatCharge.DateStart.Date != oldRepeatCharge.DateStart.Date) return true;
        if (repeatCharge.DateStop.Date != oldRepeatCharge.DateStop.Date) return true;
        if (repeatCharge.Note != oldRepeatCharge.Note) return true;
        if (repeatCharge.CopyNoteToProc != oldRepeatCharge.CopyNoteToProc) return true;
        if (repeatCharge.CreatesClaim != oldRepeatCharge.CreatesClaim) return true;
        if (repeatCharge.IsEnabled != oldRepeatCharge.IsEnabled) return true;
        if (repeatCharge.UsePrepay != oldRepeatCharge.UsePrepay) return true;
        if (repeatCharge.Npi != oldRepeatCharge.Npi) return true;
        if (repeatCharge.ErxAccountId != oldRepeatCharge.ErxAccountId) return true;
        if (repeatCharge.ProviderName != oldRepeatCharge.ProviderName) return true;
        if (repeatCharge.ChargeAmtAlt != oldRepeatCharge.ChargeAmtAlt) return true;
        if (repeatCharge.UnearnedTypes != oldRepeatCharge.UnearnedTypes) return true;
        if (repeatCharge.Frequency != oldRepeatCharge.Frequency) return true;
        return false;
    }

    public static void Delete(long repeatChargeNum)
    {
        var command = "DELETE FROM repeatcharge "
                      + "WHERE RepeatChargeNum = " + SOut.Long(repeatChargeNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listRepeatChargeNums)
    {
        if (listRepeatChargeNums == null || listRepeatChargeNums.Count == 0) return;
        var command = "DELETE FROM repeatcharge "
                      + "WHERE RepeatChargeNum IN(" + string.Join(",", listRepeatChargeNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}