using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class RepeatChargeCrud
{
    public static List<RepeatCharge> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RepeatCharge> TableToList(DataTable table)
    {
        var retVal = new List<RepeatCharge>();
        foreach (DataRow row in table.Rows)
        {
            var repeatCharge = new RepeatCharge
            {
                RepeatChargeNum = SIn.Long(row["RepeatChargeNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                ProcCode = SIn.String(row["ProcCode"].ToString()),
                ChargeAmt = SIn.Double(row["ChargeAmt"].ToString()),
                DateStart = SIn.Date(row["DateStart"].ToString()),
                DateStop = SIn.Date(row["DateStop"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                CopyNoteToProc = SIn.Bool(row["CopyNoteToProc"].ToString()),
                CreatesClaim = SIn.Bool(row["CreatesClaim"].ToString()),
                IsEnabled = SIn.Bool(row["IsEnabled"].ToString()),
                UsePrepay = SIn.Bool(row["UsePrepay"].ToString()),
                Npi = SIn.String(row["Npi"].ToString()),
                ErxAccountId = SIn.String(row["ErxAccountId"].ToString()),
                ProviderName = SIn.String(row["ProviderName"].ToString()),
                ChargeAmtAlt = SIn.Double(row["ChargeAmtAlt"].ToString()),
                UnearnedTypes = SIn.String(row["UnearnedTypes"].ToString()),
                Frequency = (EnumRepeatChargeFrequency) SIn.Int(row["Frequency"].ToString())
            };
            retVal.Add(repeatCharge);
        }

        return retVal;
    }

    public static long Insert(RepeatCharge repeatCharge)
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(repeatCharge.Note));
        if (repeatCharge.Npi == null) repeatCharge.Npi = "";
        var paramNpi = new OdSqlParameter("paramNpi", SOut.StringParam(repeatCharge.Npi));
        if (repeatCharge.ErxAccountId == null) repeatCharge.ErxAccountId = "";
        var paramErxAccountId = new OdSqlParameter("paramErxAccountId", SOut.StringParam(repeatCharge.ErxAccountId));
        if (repeatCharge.ProviderName == null) repeatCharge.ProviderName = "";
        var paramProviderName = new OdSqlParameter("paramProviderName", SOut.StringParam(repeatCharge.ProviderName));
        {
            repeatCharge.RepeatChargeNum = Db.NonQ(command, true, "RepeatChargeNum", "repeatCharge", paramNote, paramNpi, paramErxAccountId, paramProviderName);
        }
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(repeatCharge.Note));
        if (repeatCharge.Npi == null) repeatCharge.Npi = "";
        var paramNpi = new OdSqlParameter("paramNpi", SOut.StringParam(repeatCharge.Npi));
        if (repeatCharge.ErxAccountId == null) repeatCharge.ErxAccountId = "";
        var paramErxAccountId = new OdSqlParameter("paramErxAccountId", SOut.StringParam(repeatCharge.ErxAccountId));
        if (repeatCharge.ProviderName == null) repeatCharge.ProviderName = "";
        var paramProviderName = new OdSqlParameter("paramProviderName", SOut.StringParam(repeatCharge.ProviderName));
        Db.NonQ(command, paramNote, paramNpi, paramErxAccountId, paramProviderName);
    }
}