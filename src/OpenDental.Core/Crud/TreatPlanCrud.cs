using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class TreatPlanCrud
{
    public static TreatPlan SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<TreatPlan> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TreatPlan> TableToList(DataTable table)
    {
        var retVal = new List<TreatPlan>();
        foreach (DataRow row in table.Rows)
        {
            var treatPlan = new TreatPlan
            {
                TreatPlanNum = SIn.Long(row["TreatPlanNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                DateTP = SIn.Date(row["DateTP"].ToString()),
                Heading = SIn.String(row["Heading"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                Signature = SIn.String(row["Signature"].ToString()),
                SigIsTopaz = SIn.Bool(row["SigIsTopaz"].ToString()),
                ResponsParty = SIn.Long(row["ResponsParty"].ToString()),
                DocNum = SIn.Long(row["DocNum"].ToString()),
                TPStatus = (TreatPlanStatus) SIn.Int(row["TPStatus"].ToString()),
                SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString()),
                SecDateEntry = SIn.Date(row["SecDateEntry"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString()),
                UserNumPresenter = SIn.Long(row["UserNumPresenter"].ToString()),
                TPType = (TreatPlanType) SIn.Int(row["TPType"].ToString()),
                SignaturePractice = SIn.String(row["SignaturePractice"].ToString()),
                DateTSigned = SIn.DateTime(row["DateTSigned"].ToString()),
                DateTPracticeSigned = SIn.DateTime(row["DateTPracticeSigned"].ToString()),
                SignatureText = SIn.String(row["SignatureText"].ToString()),
                SignaturePracticeText = SIn.String(row["SignaturePracticeText"].ToString()),
                MobileAppDeviceNum = SIn.Long(row["MobileAppDeviceNum"].ToString())
            };
            retVal.Add(treatPlan);
        }

        return retVal;
    }

    public static long Insert(TreatPlan treatPlan)
    {
        var command = "INSERT INTO treatplan (";

        command += "PatNum,DateTP,Heading,Note,Signature,SigIsTopaz,ResponsParty,DocNum,TPStatus,SecUserNumEntry,SecDateEntry,UserNumPresenter,TPType,SignaturePractice,DateTSigned,DateTPracticeSigned,SignatureText,SignaturePracticeText,MobileAppDeviceNum) VALUES(";

        command +=
            SOut.Long(treatPlan.PatNum) + ","
                                        + SOut.Date(treatPlan.DateTP) + ","
                                        + "'" + SOut.String(treatPlan.Heading) + "',"
                                        + DbHelper.ParamChar + "paramNote,"
                                        + DbHelper.ParamChar + "paramSignature,"
                                        + SOut.Bool(treatPlan.SigIsTopaz) + ","
                                        + SOut.Long(treatPlan.ResponsParty) + ","
                                        + SOut.Long(treatPlan.DocNum) + ","
                                        + SOut.Int((int) treatPlan.TPStatus) + ","
                                        + SOut.Long(treatPlan.SecUserNumEntry) + ","
                                        + "NOW()" + ","
                                        //SecDateTEdit can only be set by MySQL
                                        + SOut.Long(treatPlan.UserNumPresenter) + ","
                                        + SOut.Int((int) treatPlan.TPType) + ","
                                        + DbHelper.ParamChar + "paramSignaturePractice,"
                                        + SOut.DateTime(treatPlan.DateTSigned) + ","
                                        + SOut.DateTime(treatPlan.DateTPracticeSigned) + ","
                                        + "'" + SOut.String(treatPlan.SignatureText) + "',"
                                        + "'" + SOut.String(treatPlan.SignaturePracticeText) + "',"
                                        + SOut.Long(treatPlan.MobileAppDeviceNum) + ")";
        if (treatPlan.Note == null) treatPlan.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(treatPlan.Note));
        if (treatPlan.Signature == null) treatPlan.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", SOut.StringParam(treatPlan.Signature));
        if (treatPlan.SignaturePractice == null) treatPlan.SignaturePractice = "";
        var paramSignaturePractice = new OdSqlParameter("paramSignaturePractice", SOut.StringParam(treatPlan.SignaturePractice));
        {
            treatPlan.TreatPlanNum = Db.NonQ(command, true, "TreatPlanNum", "treatPlan", paramNote, paramSignature, paramSignaturePractice);
        }
        return treatPlan.TreatPlanNum;
    }

    public static void Update(TreatPlan treatPlan)
    {
        var command = "UPDATE treatplan SET "
                      + "PatNum               =  " + SOut.Long(treatPlan.PatNum) + ", "
                      + "DateTP               =  " + SOut.Date(treatPlan.DateTP) + ", "
                      + "Heading              = '" + SOut.String(treatPlan.Heading) + "', "
                      + "Note                 =  " + DbHelper.ParamChar + "paramNote, "
                      + "Signature            =  " + DbHelper.ParamChar + "paramSignature, "
                      + "SigIsTopaz           =  " + SOut.Bool(treatPlan.SigIsTopaz) + ", "
                      + "ResponsParty         =  " + SOut.Long(treatPlan.ResponsParty) + ", "
                      + "DocNum               =  " + SOut.Long(treatPlan.DocNum) + ", "
                      + "TPStatus             =  " + SOut.Int((int) treatPlan.TPStatus) + ", "
                      //SecUserNumEntry excluded from update
                      //SecDateEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "UserNumPresenter     =  " + SOut.Long(treatPlan.UserNumPresenter) + ", "
                      + "TPType               =  " + SOut.Int((int) treatPlan.TPType) + ", "
                      + "SignaturePractice    =  " + DbHelper.ParamChar + "paramSignaturePractice, "
                      + "DateTSigned          =  " + SOut.DateTime(treatPlan.DateTSigned) + ", "
                      + "DateTPracticeSigned  =  " + SOut.DateTime(treatPlan.DateTPracticeSigned) + ", "
                      + "SignatureText        = '" + SOut.String(treatPlan.SignatureText) + "', "
                      + "SignaturePracticeText= '" + SOut.String(treatPlan.SignaturePracticeText) + "', "
                      + "MobileAppDeviceNum   =  " + SOut.Long(treatPlan.MobileAppDeviceNum) + " "
                      + "WHERE TreatPlanNum = " + SOut.Long(treatPlan.TreatPlanNum);
        if (treatPlan.Note == null) treatPlan.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(treatPlan.Note));
        if (treatPlan.Signature == null) treatPlan.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", SOut.StringParam(treatPlan.Signature));
        if (treatPlan.SignaturePractice == null) treatPlan.SignaturePractice = "";
        var paramSignaturePractice = new OdSqlParameter("paramSignaturePractice", SOut.StringParam(treatPlan.SignaturePractice));
        Db.NonQ(command, paramNote, paramSignature, paramSignaturePractice);
    }

    public static void Update(TreatPlan treatPlan, TreatPlan oldTreatPlan)
    {
        var command = "";
        if (treatPlan.PatNum != oldTreatPlan.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(treatPlan.PatNum) + "";
        }

        if (treatPlan.DateTP.Date != oldTreatPlan.DateTP.Date)
        {
            if (command != "") command += ",";
            command += "DateTP = " + SOut.Date(treatPlan.DateTP) + "";
        }

        if (treatPlan.Heading != oldTreatPlan.Heading)
        {
            if (command != "") command += ",";
            command += "Heading = '" + SOut.String(treatPlan.Heading) + "'";
        }

        if (treatPlan.Note != oldTreatPlan.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (treatPlan.Signature != oldTreatPlan.Signature)
        {
            if (command != "") command += ",";
            command += "Signature = " + DbHelper.ParamChar + "paramSignature";
        }

        if (treatPlan.SigIsTopaz != oldTreatPlan.SigIsTopaz)
        {
            if (command != "") command += ",";
            command += "SigIsTopaz = " + SOut.Bool(treatPlan.SigIsTopaz) + "";
        }

        if (treatPlan.ResponsParty != oldTreatPlan.ResponsParty)
        {
            if (command != "") command += ",";
            command += "ResponsParty = " + SOut.Long(treatPlan.ResponsParty) + "";
        }

        if (treatPlan.DocNum != oldTreatPlan.DocNum)
        {
            if (command != "") command += ",";
            command += "DocNum = " + SOut.Long(treatPlan.DocNum) + "";
        }

        if (treatPlan.TPStatus != oldTreatPlan.TPStatus)
        {
            if (command != "") command += ",";
            command += "TPStatus = " + SOut.Int((int) treatPlan.TPStatus) + "";
        }

        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (treatPlan.UserNumPresenter != oldTreatPlan.UserNumPresenter)
        {
            if (command != "") command += ",";
            command += "UserNumPresenter = " + SOut.Long(treatPlan.UserNumPresenter) + "";
        }

        if (treatPlan.TPType != oldTreatPlan.TPType)
        {
            if (command != "") command += ",";
            command += "TPType = " + SOut.Int((int) treatPlan.TPType) + "";
        }

        if (treatPlan.SignaturePractice != oldTreatPlan.SignaturePractice)
        {
            if (command != "") command += ",";
            command += "SignaturePractice = " + DbHelper.ParamChar + "paramSignaturePractice";
        }

        if (treatPlan.DateTSigned != oldTreatPlan.DateTSigned)
        {
            if (command != "") command += ",";
            command += "DateTSigned = " + SOut.DateTime(treatPlan.DateTSigned) + "";
        }

        if (treatPlan.DateTPracticeSigned != oldTreatPlan.DateTPracticeSigned)
        {
            if (command != "") command += ",";
            command += "DateTPracticeSigned = " + SOut.DateTime(treatPlan.DateTPracticeSigned) + "";
        }

        if (treatPlan.SignatureText != oldTreatPlan.SignatureText)
        {
            if (command != "") command += ",";
            command += "SignatureText = '" + SOut.String(treatPlan.SignatureText) + "'";
        }

        if (treatPlan.SignaturePracticeText != oldTreatPlan.SignaturePracticeText)
        {
            if (command != "") command += ",";
            command += "SignaturePracticeText = '" + SOut.String(treatPlan.SignaturePracticeText) + "'";
        }

        if (treatPlan.MobileAppDeviceNum != oldTreatPlan.MobileAppDeviceNum)
        {
            if (command != "") command += ",";
            command += "MobileAppDeviceNum = " + SOut.Long(treatPlan.MobileAppDeviceNum) + "";
        }

        if (command == "") return;
        if (treatPlan.Note == null) treatPlan.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(treatPlan.Note));
        if (treatPlan.Signature == null) treatPlan.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", SOut.StringParam(treatPlan.Signature));
        if (treatPlan.SignaturePractice == null) treatPlan.SignaturePractice = "";
        var paramSignaturePractice = new OdSqlParameter("paramSignaturePractice", SOut.StringParam(treatPlan.SignaturePractice));
        command = "UPDATE treatplan SET " + command
                                          + " WHERE TreatPlanNum = " + SOut.Long(treatPlan.TreatPlanNum);
        Db.NonQ(command, paramNote, paramSignature, paramSignaturePractice);
    }

    public static void Delete(long treatPlanNum)
    {
        var command = "DELETE FROM treatplan "
                      + "WHERE TreatPlanNum = " + SOut.Long(treatPlanNum);
        Db.NonQ(command);
    }
}