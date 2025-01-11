#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ReferralCrud
{
    public static Referral SelectOne(long referralNum)
    {
        var command = "SELECT * FROM referral "
                      + "WHERE ReferralNum = " + SOut.Long(referralNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Referral SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Referral> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Referral> TableToList(DataTable table)
    {
        var retVal = new List<Referral>();
        Referral referral;
        foreach (DataRow row in table.Rows)
        {
            referral = new Referral();
            referral.ReferralNum = SIn.Long(row["ReferralNum"].ToString());
            referral.LName = SIn.String(row["LName"].ToString());
            referral.FName = SIn.String(row["FName"].ToString());
            referral.MName = SIn.String(row["MName"].ToString());
            referral.SSN = SIn.String(row["SSN"].ToString());
            referral.UsingTIN = SIn.Bool(row["UsingTIN"].ToString());
            referral.Specialty = SIn.Long(row["Specialty"].ToString());
            referral.ST = SIn.String(row["ST"].ToString());
            referral.Telephone = SIn.String(row["Telephone"].ToString());
            referral.Address = SIn.String(row["Address"].ToString());
            referral.Address2 = SIn.String(row["Address2"].ToString());
            referral.City = SIn.String(row["City"].ToString());
            referral.Zip = SIn.String(row["Zip"].ToString());
            referral.Note = SIn.String(row["Note"].ToString());
            referral.Phone2 = SIn.String(row["Phone2"].ToString());
            referral.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            referral.NotPerson = SIn.Bool(row["NotPerson"].ToString());
            referral.Title = SIn.String(row["Title"].ToString());
            referral.EMail = SIn.String(row["EMail"].ToString());
            referral.PatNum = SIn.Long(row["PatNum"].ToString());
            referral.NationalProvID = SIn.String(row["NationalProvID"].ToString());
            referral.Slip = SIn.Long(row["Slip"].ToString());
            referral.IsDoctor = SIn.Bool(row["IsDoctor"].ToString());
            referral.IsTrustedDirect = SIn.Bool(row["IsTrustedDirect"].ToString());
            referral.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            referral.IsPreferred = SIn.Bool(row["IsPreferred"].ToString());
            referral.BusinessName = SIn.String(row["BusinessName"].ToString());
            referral.DisplayNote = SIn.String(row["DisplayNote"].ToString());
            retVal.Add(referral);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Referral> listReferrals, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Referral";
        var table = new DataTable(tableName);
        table.Columns.Add("ReferralNum");
        table.Columns.Add("LName");
        table.Columns.Add("FName");
        table.Columns.Add("MName");
        table.Columns.Add("SSN");
        table.Columns.Add("UsingTIN");
        table.Columns.Add("Specialty");
        table.Columns.Add("ST");
        table.Columns.Add("Telephone");
        table.Columns.Add("Address");
        table.Columns.Add("Address2");
        table.Columns.Add("City");
        table.Columns.Add("Zip");
        table.Columns.Add("Note");
        table.Columns.Add("Phone2");
        table.Columns.Add("IsHidden");
        table.Columns.Add("NotPerson");
        table.Columns.Add("Title");
        table.Columns.Add("EMail");
        table.Columns.Add("PatNum");
        table.Columns.Add("NationalProvID");
        table.Columns.Add("Slip");
        table.Columns.Add("IsDoctor");
        table.Columns.Add("IsTrustedDirect");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("IsPreferred");
        table.Columns.Add("BusinessName");
        table.Columns.Add("DisplayNote");
        foreach (var referral in listReferrals)
            table.Rows.Add(SOut.Long(referral.ReferralNum), referral.LName, referral.FName, referral.MName, referral.SSN, SOut.Bool(referral.UsingTIN), SOut.Long(referral.Specialty), referral.ST, referral.Telephone, referral.Address, referral.Address2, referral.City, referral.Zip, referral.Note, referral.Phone2, SOut.Bool(referral.IsHidden), SOut.Bool(referral.NotPerson), referral.Title, referral.EMail, SOut.Long(referral.PatNum), referral.NationalProvID, SOut.Long(referral.Slip), SOut.Bool(referral.IsDoctor), SOut.Bool(referral.IsTrustedDirect), SOut.DateT(referral.DateTStamp, false), SOut.Bool(referral.IsPreferred), referral.BusinessName, referral.DisplayNote);
        return table;
    }

    public static long Insert(Referral referral)
    {
        return Insert(referral, false);
    }

    public static long Insert(Referral referral, bool useExistingPK)
    {
        var command = "INSERT INTO referral (";

        command += "LName,FName,MName,SSN,UsingTIN,Specialty,ST,Telephone,Address,Address2,City,Zip,Note,Phone2,IsHidden,NotPerson,Title,EMail,PatNum,NationalProvID,Slip,IsDoctor,IsTrustedDirect,IsPreferred,BusinessName,DisplayNote) VALUES(";

        command +=
            "'" + SOut.String(referral.LName) + "',"
            + "'" + SOut.String(referral.FName) + "',"
            + "'" + SOut.String(referral.MName) + "',"
            + "'" + SOut.String(referral.SSN) + "',"
            + SOut.Bool(referral.UsingTIN) + ","
            + SOut.Long(referral.Specialty) + ","
            + "'" + SOut.String(referral.ST) + "',"
            + "'" + SOut.String(referral.Telephone) + "',"
            + "'" + SOut.String(referral.Address) + "',"
            + "'" + SOut.String(referral.Address2) + "',"
            + "'" + SOut.String(referral.City) + "',"
            + "'" + SOut.String(referral.Zip) + "',"
            + DbHelper.ParamChar + "paramNote,"
            + "'" + SOut.String(referral.Phone2) + "',"
            + SOut.Bool(referral.IsHidden) + ","
            + SOut.Bool(referral.NotPerson) + ","
            + "'" + SOut.String(referral.Title) + "',"
            + "'" + SOut.String(referral.EMail) + "',"
            + SOut.Long(referral.PatNum) + ","
            + "'" + SOut.String(referral.NationalProvID) + "',"
            + SOut.Long(referral.Slip) + ","
            + SOut.Bool(referral.IsDoctor) + ","
            + SOut.Bool(referral.IsTrustedDirect) + ","
            //DateTStamp can only be set by MySQL
            + SOut.Bool(referral.IsPreferred) + ","
            + "'" + SOut.String(referral.BusinessName) + "',"
            + "'" + SOut.String(referral.DisplayNote) + "')";
        if (referral.Note == null) referral.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(referral.Note));
        {
            referral.ReferralNum = Db.NonQ(command, true, "ReferralNum", "referral", paramNote);
        }
        return referral.ReferralNum;
    }

    public static long InsertNoCache(Referral referral)
    {
        return InsertNoCache(referral, false);
    }

    public static long InsertNoCache(Referral referral, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO referral (";
        if (isRandomKeys || useExistingPK) command += "ReferralNum,";
        command += "LName,FName,MName,SSN,UsingTIN,Specialty,ST,Telephone,Address,Address2,City,Zip,Note,Phone2,IsHidden,NotPerson,Title,EMail,PatNum,NationalProvID,Slip,IsDoctor,IsTrustedDirect,IsPreferred,BusinessName,DisplayNote) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(referral.ReferralNum) + ",";
        command +=
            "'" + SOut.String(referral.LName) + "',"
            + "'" + SOut.String(referral.FName) + "',"
            + "'" + SOut.String(referral.MName) + "',"
            + "'" + SOut.String(referral.SSN) + "',"
            + SOut.Bool(referral.UsingTIN) + ","
            + SOut.Long(referral.Specialty) + ","
            + "'" + SOut.String(referral.ST) + "',"
            + "'" + SOut.String(referral.Telephone) + "',"
            + "'" + SOut.String(referral.Address) + "',"
            + "'" + SOut.String(referral.Address2) + "',"
            + "'" + SOut.String(referral.City) + "',"
            + "'" + SOut.String(referral.Zip) + "',"
            + DbHelper.ParamChar + "paramNote,"
            + "'" + SOut.String(referral.Phone2) + "',"
            + SOut.Bool(referral.IsHidden) + ","
            + SOut.Bool(referral.NotPerson) + ","
            + "'" + SOut.String(referral.Title) + "',"
            + "'" + SOut.String(referral.EMail) + "',"
            + SOut.Long(referral.PatNum) + ","
            + "'" + SOut.String(referral.NationalProvID) + "',"
            + SOut.Long(referral.Slip) + ","
            + SOut.Bool(referral.IsDoctor) + ","
            + SOut.Bool(referral.IsTrustedDirect) + ","
            //DateTStamp can only be set by MySQL
            + SOut.Bool(referral.IsPreferred) + ","
            + "'" + SOut.String(referral.BusinessName) + "',"
            + "'" + SOut.String(referral.DisplayNote) + "')";
        if (referral.Note == null) referral.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(referral.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            referral.ReferralNum = Db.NonQ(command, true, "ReferralNum", "referral", paramNote);
        return referral.ReferralNum;
    }

    public static void Update(Referral referral)
    {
        var command = "UPDATE referral SET "
                      + "LName          = '" + SOut.String(referral.LName) + "', "
                      + "FName          = '" + SOut.String(referral.FName) + "', "
                      + "MName          = '" + SOut.String(referral.MName) + "', "
                      + "SSN            = '" + SOut.String(referral.SSN) + "', "
                      + "UsingTIN       =  " + SOut.Bool(referral.UsingTIN) + ", "
                      + "Specialty      =  " + SOut.Long(referral.Specialty) + ", "
                      + "ST             = '" + SOut.String(referral.ST) + "', "
                      + "Telephone      = '" + SOut.String(referral.Telephone) + "', "
                      + "Address        = '" + SOut.String(referral.Address) + "', "
                      + "Address2       = '" + SOut.String(referral.Address2) + "', "
                      + "City           = '" + SOut.String(referral.City) + "', "
                      + "Zip            = '" + SOut.String(referral.Zip) + "', "
                      + "Note           =  " + DbHelper.ParamChar + "paramNote, "
                      + "Phone2         = '" + SOut.String(referral.Phone2) + "', "
                      + "IsHidden       =  " + SOut.Bool(referral.IsHidden) + ", "
                      + "NotPerson      =  " + SOut.Bool(referral.NotPerson) + ", "
                      + "Title          = '" + SOut.String(referral.Title) + "', "
                      + "EMail          = '" + SOut.String(referral.EMail) + "', "
                      + "PatNum         =  " + SOut.Long(referral.PatNum) + ", "
                      + "NationalProvID = '" + SOut.String(referral.NationalProvID) + "', "
                      + "Slip           =  " + SOut.Long(referral.Slip) + ", "
                      + "IsDoctor       =  " + SOut.Bool(referral.IsDoctor) + ", "
                      + "IsTrustedDirect=  " + SOut.Bool(referral.IsTrustedDirect) + ", "
                      //DateTStamp can only be set by MySQL
                      + "IsPreferred    =  " + SOut.Bool(referral.IsPreferred) + ", "
                      + "BusinessName   = '" + SOut.String(referral.BusinessName) + "', "
                      + "DisplayNote    = '" + SOut.String(referral.DisplayNote) + "' "
                      + "WHERE ReferralNum = " + SOut.Long(referral.ReferralNum);
        if (referral.Note == null) referral.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(referral.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(Referral referral, Referral oldReferral)
    {
        var command = "";
        if (referral.LName != oldReferral.LName)
        {
            if (command != "") command += ",";
            command += "LName = '" + SOut.String(referral.LName) + "'";
        }

        if (referral.FName != oldReferral.FName)
        {
            if (command != "") command += ",";
            command += "FName = '" + SOut.String(referral.FName) + "'";
        }

        if (referral.MName != oldReferral.MName)
        {
            if (command != "") command += ",";
            command += "MName = '" + SOut.String(referral.MName) + "'";
        }

        if (referral.SSN != oldReferral.SSN)
        {
            if (command != "") command += ",";
            command += "SSN = '" + SOut.String(referral.SSN) + "'";
        }

        if (referral.UsingTIN != oldReferral.UsingTIN)
        {
            if (command != "") command += ",";
            command += "UsingTIN = " + SOut.Bool(referral.UsingTIN) + "";
        }

        if (referral.Specialty != oldReferral.Specialty)
        {
            if (command != "") command += ",";
            command += "Specialty = " + SOut.Long(referral.Specialty) + "";
        }

        if (referral.ST != oldReferral.ST)
        {
            if (command != "") command += ",";
            command += "ST = '" + SOut.String(referral.ST) + "'";
        }

        if (referral.Telephone != oldReferral.Telephone)
        {
            if (command != "") command += ",";
            command += "Telephone = '" + SOut.String(referral.Telephone) + "'";
        }

        if (referral.Address != oldReferral.Address)
        {
            if (command != "") command += ",";
            command += "Address = '" + SOut.String(referral.Address) + "'";
        }

        if (referral.Address2 != oldReferral.Address2)
        {
            if (command != "") command += ",";
            command += "Address2 = '" + SOut.String(referral.Address2) + "'";
        }

        if (referral.City != oldReferral.City)
        {
            if (command != "") command += ",";
            command += "City = '" + SOut.String(referral.City) + "'";
        }

        if (referral.Zip != oldReferral.Zip)
        {
            if (command != "") command += ",";
            command += "Zip = '" + SOut.String(referral.Zip) + "'";
        }

        if (referral.Note != oldReferral.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (referral.Phone2 != oldReferral.Phone2)
        {
            if (command != "") command += ",";
            command += "Phone2 = '" + SOut.String(referral.Phone2) + "'";
        }

        if (referral.IsHidden != oldReferral.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(referral.IsHidden) + "";
        }

        if (referral.NotPerson != oldReferral.NotPerson)
        {
            if (command != "") command += ",";
            command += "NotPerson = " + SOut.Bool(referral.NotPerson) + "";
        }

        if (referral.Title != oldReferral.Title)
        {
            if (command != "") command += ",";
            command += "Title = '" + SOut.String(referral.Title) + "'";
        }

        if (referral.EMail != oldReferral.EMail)
        {
            if (command != "") command += ",";
            command += "EMail = '" + SOut.String(referral.EMail) + "'";
        }

        if (referral.PatNum != oldReferral.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(referral.PatNum) + "";
        }

        if (referral.NationalProvID != oldReferral.NationalProvID)
        {
            if (command != "") command += ",";
            command += "NationalProvID = '" + SOut.String(referral.NationalProvID) + "'";
        }

        if (referral.Slip != oldReferral.Slip)
        {
            if (command != "") command += ",";
            command += "Slip = " + SOut.Long(referral.Slip) + "";
        }

        if (referral.IsDoctor != oldReferral.IsDoctor)
        {
            if (command != "") command += ",";
            command += "IsDoctor = " + SOut.Bool(referral.IsDoctor) + "";
        }

        if (referral.IsTrustedDirect != oldReferral.IsTrustedDirect)
        {
            if (command != "") command += ",";
            command += "IsTrustedDirect = " + SOut.Bool(referral.IsTrustedDirect) + "";
        }

        //DateTStamp can only be set by MySQL
        if (referral.IsPreferred != oldReferral.IsPreferred)
        {
            if (command != "") command += ",";
            command += "IsPreferred = " + SOut.Bool(referral.IsPreferred) + "";
        }

        if (referral.BusinessName != oldReferral.BusinessName)
        {
            if (command != "") command += ",";
            command += "BusinessName = '" + SOut.String(referral.BusinessName) + "'";
        }

        if (referral.DisplayNote != oldReferral.DisplayNote)
        {
            if (command != "") command += ",";
            command += "DisplayNote = '" + SOut.String(referral.DisplayNote) + "'";
        }

        if (command == "") return false;
        if (referral.Note == null) referral.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(referral.Note));
        command = "UPDATE referral SET " + command
                                         + " WHERE ReferralNum = " + SOut.Long(referral.ReferralNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(Referral referral, Referral oldReferral)
    {
        if (referral.LName != oldReferral.LName) return true;
        if (referral.FName != oldReferral.FName) return true;
        if (referral.MName != oldReferral.MName) return true;
        if (referral.SSN != oldReferral.SSN) return true;
        if (referral.UsingTIN != oldReferral.UsingTIN) return true;
        if (referral.Specialty != oldReferral.Specialty) return true;
        if (referral.ST != oldReferral.ST) return true;
        if (referral.Telephone != oldReferral.Telephone) return true;
        if (referral.Address != oldReferral.Address) return true;
        if (referral.Address2 != oldReferral.Address2) return true;
        if (referral.City != oldReferral.City) return true;
        if (referral.Zip != oldReferral.Zip) return true;
        if (referral.Note != oldReferral.Note) return true;
        if (referral.Phone2 != oldReferral.Phone2) return true;
        if (referral.IsHidden != oldReferral.IsHidden) return true;
        if (referral.NotPerson != oldReferral.NotPerson) return true;
        if (referral.Title != oldReferral.Title) return true;
        if (referral.EMail != oldReferral.EMail) return true;
        if (referral.PatNum != oldReferral.PatNum) return true;
        if (referral.NationalProvID != oldReferral.NationalProvID) return true;
        if (referral.Slip != oldReferral.Slip) return true;
        if (referral.IsDoctor != oldReferral.IsDoctor) return true;
        if (referral.IsTrustedDirect != oldReferral.IsTrustedDirect) return true;
        //DateTStamp can only be set by MySQL
        if (referral.IsPreferred != oldReferral.IsPreferred) return true;
        if (referral.BusinessName != oldReferral.BusinessName) return true;
        if (referral.DisplayNote != oldReferral.DisplayNote) return true;
        return false;
    }

    public static void Delete(long referralNum)
    {
        var command = "DELETE FROM referral "
                      + "WHERE ReferralNum = " + SOut.Long(referralNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listReferralNums)
    {
        if (listReferralNums == null || listReferralNums.Count == 0) return;
        var command = "DELETE FROM referral "
                      + "WHERE ReferralNum IN(" + string.Join(",", listReferralNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}