using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ReferralCrud
{
    public static List<Referral> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Referral> TableToList(DataTable table)
    {
        var retVal = new List<Referral>();
        foreach (DataRow row in table.Rows)
        {
            var referral = new Referral
            {
                ReferralNum = SIn.Long(row["ReferralNum"].ToString()),
                LName = SIn.String(row["LName"].ToString()),
                FName = SIn.String(row["FName"].ToString()),
                MName = SIn.String(row["MName"].ToString()),
                SSN = SIn.String(row["SSN"].ToString()),
                UsingTIN = SIn.Bool(row["UsingTIN"].ToString()),
                Specialty = SIn.Long(row["Specialty"].ToString()),
                ST = SIn.String(row["ST"].ToString()),
                Telephone = SIn.String(row["Telephone"].ToString()),
                Address = SIn.String(row["Address"].ToString()),
                Address2 = SIn.String(row["Address2"].ToString()),
                City = SIn.String(row["City"].ToString()),
                Zip = SIn.String(row["Zip"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                Phone2 = SIn.String(row["Phone2"].ToString()),
                IsHidden = SIn.Bool(row["IsHidden"].ToString()),
                NotPerson = SIn.Bool(row["NotPerson"].ToString()),
                Title = SIn.String(row["Title"].ToString()),
                EMail = SIn.String(row["EMail"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                NationalProvID = SIn.String(row["NationalProvID"].ToString()),
                Slip = SIn.Long(row["Slip"].ToString()),
                IsDoctor = SIn.Bool(row["IsDoctor"].ToString()),
                IsTrustedDirect = SIn.Bool(row["IsTrustedDirect"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString()),
                IsPreferred = SIn.Bool(row["IsPreferred"].ToString()),
                BusinessName = SIn.String(row["BusinessName"].ToString()),
                DisplayNote = SIn.String(row["DisplayNote"].ToString())
            };
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
            table.Rows.Add(SOut.Long(referral.ReferralNum), referral.LName, referral.FName, referral.MName, referral.SSN, SOut.Bool(referral.UsingTIN), SOut.Long(referral.Specialty), referral.ST, referral.Telephone, referral.Address, referral.Address2, referral.City, referral.Zip, referral.Note, referral.Phone2, SOut.Bool(referral.IsHidden), SOut.Bool(referral.NotPerson), referral.Title, referral.EMail, SOut.Long(referral.PatNum), referral.NationalProvID, SOut.Long(referral.Slip), SOut.Bool(referral.IsDoctor), SOut.Bool(referral.IsTrustedDirect), SOut.DateTime(referral.DateTStamp, false), SOut.Bool(referral.IsPreferred), referral.BusinessName, referral.DisplayNote);
        return table;
    }

    public static void Insert(Referral referral)
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(referral.Note));
        {
            referral.ReferralNum = Db.NonQ(command, true, "ReferralNum", "referral", paramNote);
        }
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(referral.Note));
        Db.NonQ(command, paramNote);
    }

    public static void Delete(long referralNum)
    {
        var command = "DELETE FROM referral "
                      + "WHERE ReferralNum = " + SOut.Long(referralNum);
        Db.NonQ(command);
    }
}