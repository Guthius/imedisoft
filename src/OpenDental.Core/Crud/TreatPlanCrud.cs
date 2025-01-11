#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class TreatPlanCrud
{
    public static TreatPlan SelectOne(long treatPlanNum)
    {
        var command = "SELECT * FROM treatplan "
                      + "WHERE TreatPlanNum = " + SOut.Long(treatPlanNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

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
        TreatPlan treatPlan;
        foreach (DataRow row in table.Rows)
        {
            treatPlan = new TreatPlan();
            treatPlan.TreatPlanNum = SIn.Long(row["TreatPlanNum"].ToString());
            treatPlan.PatNum = SIn.Long(row["PatNum"].ToString());
            treatPlan.DateTP = SIn.Date(row["DateTP"].ToString());
            treatPlan.Heading = SIn.String(row["Heading"].ToString());
            treatPlan.Note = SIn.String(row["Note"].ToString());
            treatPlan.Signature = SIn.String(row["Signature"].ToString());
            treatPlan.SigIsTopaz = SIn.Bool(row["SigIsTopaz"].ToString());
            treatPlan.ResponsParty = SIn.Long(row["ResponsParty"].ToString());
            treatPlan.DocNum = SIn.Long(row["DocNum"].ToString());
            treatPlan.TPStatus = (TreatPlanStatus) SIn.Int(row["TPStatus"].ToString());
            treatPlan.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            treatPlan.SecDateEntry = SIn.Date(row["SecDateEntry"].ToString());
            treatPlan.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            treatPlan.UserNumPresenter = SIn.Long(row["UserNumPresenter"].ToString());
            treatPlan.TPType = (TreatPlanType) SIn.Int(row["TPType"].ToString());
            treatPlan.SignaturePractice = SIn.String(row["SignaturePractice"].ToString());
            treatPlan.DateTSigned = SIn.DateTime(row["DateTSigned"].ToString());
            treatPlan.DateTPracticeSigned = SIn.DateTime(row["DateTPracticeSigned"].ToString());
            treatPlan.SignatureText = SIn.String(row["SignatureText"].ToString());
            treatPlan.SignaturePracticeText = SIn.String(row["SignaturePracticeText"].ToString());
            treatPlan.MobileAppDeviceNum = SIn.Long(row["MobileAppDeviceNum"].ToString());
            retVal.Add(treatPlan);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<TreatPlan> listTreatPlans, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "TreatPlan";
        var table = new DataTable(tableName);
        table.Columns.Add("TreatPlanNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("DateTP");
        table.Columns.Add("Heading");
        table.Columns.Add("Note");
        table.Columns.Add("Signature");
        table.Columns.Add("SigIsTopaz");
        table.Columns.Add("ResponsParty");
        table.Columns.Add("DocNum");
        table.Columns.Add("TPStatus");
        table.Columns.Add("SecUserNumEntry");
        table.Columns.Add("SecDateEntry");
        table.Columns.Add("SecDateTEdit");
        table.Columns.Add("UserNumPresenter");
        table.Columns.Add("TPType");
        table.Columns.Add("SignaturePractice");
        table.Columns.Add("DateTSigned");
        table.Columns.Add("DateTPracticeSigned");
        table.Columns.Add("SignatureText");
        table.Columns.Add("SignaturePracticeText");
        table.Columns.Add("MobileAppDeviceNum");
        foreach (var treatPlan in listTreatPlans)
            table.Rows.Add(SOut.Long(treatPlan.TreatPlanNum), SOut.Long(treatPlan.PatNum), SOut.DateT(treatPlan.DateTP, false), treatPlan.Heading, treatPlan.Note, treatPlan.Signature, SOut.Bool(treatPlan.SigIsTopaz), SOut.Long(treatPlan.ResponsParty), SOut.Long(treatPlan.DocNum), SOut.Int((int) treatPlan.TPStatus), SOut.Long(treatPlan.SecUserNumEntry), SOut.DateT(treatPlan.SecDateEntry, false), SOut.DateT(treatPlan.SecDateTEdit, false), SOut.Long(treatPlan.UserNumPresenter), SOut.Int((int) treatPlan.TPType), treatPlan.SignaturePractice, SOut.DateT(treatPlan.DateTSigned, false), SOut.DateT(treatPlan.DateTPracticeSigned, false), treatPlan.SignatureText, treatPlan.SignaturePracticeText, SOut.Long(treatPlan.MobileAppDeviceNum));
        return table;
    }

    public static long Insert(TreatPlan treatPlan)
    {
        return Insert(treatPlan, false);
    }

    public static long Insert(TreatPlan treatPlan, bool useExistingPK)
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
                                        + DbHelper.Now() + ","
                                        //SecDateTEdit can only be set by MySQL
                                        + SOut.Long(treatPlan.UserNumPresenter) + ","
                                        + SOut.Int((int) treatPlan.TPType) + ","
                                        + DbHelper.ParamChar + "paramSignaturePractice,"
                                        + SOut.DateT(treatPlan.DateTSigned) + ","
                                        + SOut.DateT(treatPlan.DateTPracticeSigned) + ","
                                        + "'" + SOut.String(treatPlan.SignatureText) + "',"
                                        + "'" + SOut.String(treatPlan.SignaturePracticeText) + "',"
                                        + SOut.Long(treatPlan.MobileAppDeviceNum) + ")";
        if (treatPlan.Note == null) treatPlan.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(treatPlan.Note));
        if (treatPlan.Signature == null) treatPlan.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(treatPlan.Signature));
        if (treatPlan.SignaturePractice == null) treatPlan.SignaturePractice = "";
        var paramSignaturePractice = new OdSqlParameter("paramSignaturePractice", OdDbType.Text, SOut.StringParam(treatPlan.SignaturePractice));
        {
            treatPlan.TreatPlanNum = Db.NonQ(command, true, "TreatPlanNum", "treatPlan", paramNote, paramSignature, paramSignaturePractice);
        }
        return treatPlan.TreatPlanNum;
    }

    public static long InsertNoCache(TreatPlan treatPlan)
    {
        return InsertNoCache(treatPlan, false);
    }

    public static long InsertNoCache(TreatPlan treatPlan, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO treatplan (";
        if (isRandomKeys || useExistingPK) command += "TreatPlanNum,";
        command += "PatNum,DateTP,Heading,Note,Signature,SigIsTopaz,ResponsParty,DocNum,TPStatus,SecUserNumEntry,SecDateEntry,UserNumPresenter,TPType,SignaturePractice,DateTSigned,DateTPracticeSigned,SignatureText,SignaturePracticeText,MobileAppDeviceNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(treatPlan.TreatPlanNum) + ",";
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
                                        + DbHelper.Now() + ","
                                        //SecDateTEdit can only be set by MySQL
                                        + SOut.Long(treatPlan.UserNumPresenter) + ","
                                        + SOut.Int((int) treatPlan.TPType) + ","
                                        + DbHelper.ParamChar + "paramSignaturePractice,"
                                        + SOut.DateT(treatPlan.DateTSigned) + ","
                                        + SOut.DateT(treatPlan.DateTPracticeSigned) + ","
                                        + "'" + SOut.String(treatPlan.SignatureText) + "',"
                                        + "'" + SOut.String(treatPlan.SignaturePracticeText) + "',"
                                        + SOut.Long(treatPlan.MobileAppDeviceNum) + ")";
        if (treatPlan.Note == null) treatPlan.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(treatPlan.Note));
        if (treatPlan.Signature == null) treatPlan.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(treatPlan.Signature));
        if (treatPlan.SignaturePractice == null) treatPlan.SignaturePractice = "";
        var paramSignaturePractice = new OdSqlParameter("paramSignaturePractice", OdDbType.Text, SOut.StringParam(treatPlan.SignaturePractice));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote, paramSignature, paramSignaturePractice);
        else
            treatPlan.TreatPlanNum = Db.NonQ(command, true, "TreatPlanNum", "treatPlan", paramNote, paramSignature, paramSignaturePractice);
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
                      + "DateTSigned          =  " + SOut.DateT(treatPlan.DateTSigned) + ", "
                      + "DateTPracticeSigned  =  " + SOut.DateT(treatPlan.DateTPracticeSigned) + ", "
                      + "SignatureText        = '" + SOut.String(treatPlan.SignatureText) + "', "
                      + "SignaturePracticeText= '" + SOut.String(treatPlan.SignaturePracticeText) + "', "
                      + "MobileAppDeviceNum   =  " + SOut.Long(treatPlan.MobileAppDeviceNum) + " "
                      + "WHERE TreatPlanNum = " + SOut.Long(treatPlan.TreatPlanNum);
        if (treatPlan.Note == null) treatPlan.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(treatPlan.Note));
        if (treatPlan.Signature == null) treatPlan.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(treatPlan.Signature));
        if (treatPlan.SignaturePractice == null) treatPlan.SignaturePractice = "";
        var paramSignaturePractice = new OdSqlParameter("paramSignaturePractice", OdDbType.Text, SOut.StringParam(treatPlan.SignaturePractice));
        Db.NonQ(command, paramNote, paramSignature, paramSignaturePractice);
    }

    public static bool Update(TreatPlan treatPlan, TreatPlan oldTreatPlan)
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
            command += "DateTSigned = " + SOut.DateT(treatPlan.DateTSigned) + "";
        }

        if (treatPlan.DateTPracticeSigned != oldTreatPlan.DateTPracticeSigned)
        {
            if (command != "") command += ",";
            command += "DateTPracticeSigned = " + SOut.DateT(treatPlan.DateTPracticeSigned) + "";
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

        if (command == "") return false;
        if (treatPlan.Note == null) treatPlan.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(treatPlan.Note));
        if (treatPlan.Signature == null) treatPlan.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(treatPlan.Signature));
        if (treatPlan.SignaturePractice == null) treatPlan.SignaturePractice = "";
        var paramSignaturePractice = new OdSqlParameter("paramSignaturePractice", OdDbType.Text, SOut.StringParam(treatPlan.SignaturePractice));
        command = "UPDATE treatplan SET " + command
                                          + " WHERE TreatPlanNum = " + SOut.Long(treatPlan.TreatPlanNum);
        Db.NonQ(command, paramNote, paramSignature, paramSignaturePractice);
        return true;
    }

    public static bool UpdateComparison(TreatPlan treatPlan, TreatPlan oldTreatPlan)
    {
        if (treatPlan.PatNum != oldTreatPlan.PatNum) return true;
        if (treatPlan.DateTP.Date != oldTreatPlan.DateTP.Date) return true;
        if (treatPlan.Heading != oldTreatPlan.Heading) return true;
        if (treatPlan.Note != oldTreatPlan.Note) return true;
        if (treatPlan.Signature != oldTreatPlan.Signature) return true;
        if (treatPlan.SigIsTopaz != oldTreatPlan.SigIsTopaz) return true;
        if (treatPlan.ResponsParty != oldTreatPlan.ResponsParty) return true;
        if (treatPlan.DocNum != oldTreatPlan.DocNum) return true;
        if (treatPlan.TPStatus != oldTreatPlan.TPStatus) return true;
        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (treatPlan.UserNumPresenter != oldTreatPlan.UserNumPresenter) return true;
        if (treatPlan.TPType != oldTreatPlan.TPType) return true;
        if (treatPlan.SignaturePractice != oldTreatPlan.SignaturePractice) return true;
        if (treatPlan.DateTSigned != oldTreatPlan.DateTSigned) return true;
        if (treatPlan.DateTPracticeSigned != oldTreatPlan.DateTPracticeSigned) return true;
        if (treatPlan.SignatureText != oldTreatPlan.SignatureText) return true;
        if (treatPlan.SignaturePracticeText != oldTreatPlan.SignaturePracticeText) return true;
        if (treatPlan.MobileAppDeviceNum != oldTreatPlan.MobileAppDeviceNum) return true;
        return false;
    }

    public static void Delete(long treatPlanNum)
    {
        var command = "DELETE FROM treatplan "
                      + "WHERE TreatPlanNum = " + SOut.Long(treatPlanNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTreatPlanNums)
    {
        if (listTreatPlanNums == null || listTreatPlanNums.Count == 0) return;
        var command = "DELETE FROM treatplan "
                      + "WHERE TreatPlanNum IN(" + string.Join(",", listTreatPlanNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}