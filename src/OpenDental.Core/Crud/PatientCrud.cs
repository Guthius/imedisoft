using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PatientCrud
{
    public static Patient SelectOne(long patNum)
    {
        var command = "SELECT * FROM patient "
                      + "WHERE PatNum = " + SOut.Long(patNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Patient SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Patient> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Patient> TableToList(DataTable table)
    {
        var retVal = new List<Patient>();
        foreach (DataRow row in table.Rows)
        {
            var patient = new Patient
            {
                PatNum = SIn.Long(row["PatNum"].ToString()),
                LName = SIn.String(row["LName"].ToString()),
                FName = SIn.String(row["FName"].ToString()),
                MiddleI = SIn.String(row["MiddleI"].ToString()),
                Preferred = SIn.String(row["Preferred"].ToString()),
                PatStatus = (PatientStatus) SIn.Int(row["PatStatus"].ToString()),
                Gender = (PatientGender) SIn.Int(row["Gender"].ToString()),
                Position = (PatientPosition) SIn.Int(row["Position"].ToString()),
                Birthdate = SIn.Date(row["Birthdate"].ToString()),
                SSN = SIn.String(row["SSN"].ToString()),
                Address = SIn.String(row["Address"].ToString()),
                Address2 = SIn.String(row["Address2"].ToString()),
                City = SIn.String(row["City"].ToString()),
                State = SIn.String(row["State"].ToString()),
                Zip = SIn.String(row["Zip"].ToString()),
                HmPhone = SIn.String(row["HmPhone"].ToString()),
                WkPhone = SIn.String(row["WkPhone"].ToString()),
                WirelessPhone = SIn.String(row["WirelessPhone"].ToString()),
                Guarantor = SIn.Long(row["Guarantor"].ToString()),
                CreditType = SIn.String(row["CreditType"].ToString()),
                Email = SIn.String(row["Email"].ToString()),
                Salutation = SIn.String(row["Salutation"].ToString()),
                EstBalance = SIn.Double(row["EstBalance"].ToString()),
                PriProv = SIn.Long(row["PriProv"].ToString()),
                SecProv = SIn.Long(row["SecProv"].ToString()),
                FeeSched = SIn.Long(row["FeeSched"].ToString()),
                BillingType = SIn.Long(row["BillingType"].ToString()),
                ImageFolder = SIn.String(row["ImageFolder"].ToString()),
                AddrNote = SIn.String(row["AddrNote"].ToString()),
                FamFinUrgNote = SIn.String(row["FamFinUrgNote"].ToString()),
                MedUrgNote = SIn.String(row["MedUrgNote"].ToString()),
                ApptModNote = SIn.String(row["ApptModNote"].ToString()),
                StudentStatus = SIn.String(row["StudentStatus"].ToString()),
                SchoolName = SIn.String(row["SchoolName"].ToString()),
                ChartNumber = SIn.String(row["ChartNumber"].ToString()),
                MedicaidID = SIn.String(row["MedicaidID"].ToString()),
                Bal_0_30 = SIn.Double(row["Bal_0_30"].ToString()),
                Bal_31_60 = SIn.Double(row["Bal_31_60"].ToString()),
                Bal_61_90 = SIn.Double(row["Bal_61_90"].ToString()),
                BalOver90 = SIn.Double(row["BalOver90"].ToString()),
                InsEst = SIn.Double(row["InsEst"].ToString()),
                BalTotal = SIn.Double(row["BalTotal"].ToString()),
                EmployerNum = SIn.Long(row["EmployerNum"].ToString()),
                EmploymentNote = SIn.String(row["EmploymentNote"].ToString()),
                County = SIn.String(row["County"].ToString()),
                GradeLevel = (PatientGrade) SIn.Int(row["GradeLevel"].ToString()),
                Urgency = (TreatmentUrgency) SIn.Int(row["Urgency"].ToString()),
                DateFirstVisit = SIn.Date(row["DateFirstVisit"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                HasIns = SIn.String(row["HasIns"].ToString()),
                TrophyFolder = SIn.String(row["TrophyFolder"].ToString()),
                PlannedIsDone = SIn.Bool(row["PlannedIsDone"].ToString()),
                Premed = SIn.Bool(row["Premed"].ToString()),
                Ward = SIn.String(row["Ward"].ToString()),
                PreferConfirmMethod = (ContactMethod) SIn.Int(row["PreferConfirmMethod"].ToString()),
                PreferContactMethod = (ContactMethod) SIn.Int(row["PreferContactMethod"].ToString()),
                PreferRecallMethod = (ContactMethod) SIn.Int(row["PreferRecallMethod"].ToString()),
                SchedBeforeTime = SIn.TimeSpan(row["SchedBeforeTime"].ToString()),
                SchedAfterTime = SIn.TimeSpan(row["SchedAfterTime"].ToString()),
                SchedDayOfWeek = SIn.Byte(row["SchedDayOfWeek"].ToString()),
                Language = SIn.String(row["Language"].ToString()),
                AdmitDate = SIn.Date(row["AdmitDate"].ToString()),
                Title = SIn.String(row["Title"].ToString()),
                PayPlanDue = SIn.Double(row["PayPlanDue"].ToString()),
                SiteNum = SIn.Long(row["SiteNum"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString()),
                ResponsParty = SIn.Long(row["ResponsParty"].ToString()),
                CanadianEligibilityCode = SIn.Byte(row["CanadianEligibilityCode"].ToString()),
                AskToArriveEarly = SIn.Int(row["AskToArriveEarly"].ToString()),
                PreferContactConfidential = (ContactMethod) SIn.Int(row["PreferContactConfidential"].ToString()),
                SuperFamily = SIn.Long(row["SuperFamily"].ToString()),
                TxtMsgOk = (YN) SIn.Int(row["TxtMsgOk"].ToString()),
                SmokingSnoMed = SIn.String(row["SmokingSnoMed"].ToString()),
                Country = SIn.String(row["Country"].ToString()),
                DateTimeDeceased = SIn.DateTime(row["DateTimeDeceased"].ToString()),
                BillingCycleDay = SIn.Int(row["BillingCycleDay"].ToString()),
                SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString()),
                SecDateEntry = SIn.Date(row["SecDateEntry"].ToString()),
                HasSuperBilling = SIn.Bool(row["HasSuperBilling"].ToString()),
                PatNumCloneFrom = SIn.Long(row["PatNumCloneFrom"].ToString()),
                DiscountPlanNum = SIn.Long(row["DiscountPlanNum"].ToString()),
                HasSignedTil = SIn.Bool(row["HasSignedTil"].ToString()),
                ShortCodeOptIn = (YN) SIn.Int(row["ShortCodeOptIn"].ToString()),
                SecurityHash = SIn.String(row["SecurityHash"].ToString())
            };
            retVal.Add(patient);
        }

        return retVal;
    }

    public static long Insert(Patient patient)
    {
        var command = "INSERT INTO patient (";

        command += "LName,FName,MiddleI,Preferred,PatStatus,Gender,Position,Birthdate,SSN,Address,Address2,City,State,Zip,HmPhone,WkPhone,WirelessPhone,Guarantor,CreditType,Email,Salutation,EstBalance,PriProv,SecProv,FeeSched,BillingType,ImageFolder,AddrNote,FamFinUrgNote,MedUrgNote,ApptModNote,StudentStatus,SchoolName,ChartNumber,MedicaidID,Bal_0_30,Bal_31_60,Bal_61_90,BalOver90,InsEst,BalTotal,EmployerNum,EmploymentNote,County,GradeLevel,Urgency,DateFirstVisit,ClinicNum,HasIns,TrophyFolder,PlannedIsDone,Premed,Ward,PreferConfirmMethod,PreferContactMethod,PreferRecallMethod,SchedBeforeTime,SchedAfterTime,SchedDayOfWeek,Language,AdmitDate,Title,PayPlanDue,SiteNum,ResponsParty,CanadianEligibilityCode,AskToArriveEarly,PreferContactConfidential,SuperFamily,TxtMsgOk,SmokingSnoMed,Country,DateTimeDeceased,BillingCycleDay,SecUserNumEntry,SecDateEntry,HasSuperBilling,PatNumCloneFrom,DiscountPlanNum,HasSignedTil,ShortCodeOptIn,SecurityHash) VALUES(";

        command +=
            "'" + SOut.String(patient.LName) + "',"
            + "'" + SOut.String(patient.FName) + "',"
            + "'" + SOut.String(patient.MiddleI) + "',"
            + "'" + SOut.String(patient.Preferred) + "',"
            + SOut.Int((int) patient.PatStatus) + ","
            + SOut.Int((int) patient.Gender) + ","
            + SOut.Int((int) patient.Position) + ","
            + SOut.Date(patient.Birthdate) + ","
            + "'" + SOut.String(patient.SSN) + "',"
            + "'" + SOut.StringNote(patient.Address, true) + "',"
            + "'" + SOut.StringNote(patient.Address2, true) + "',"
            + "'" + SOut.String(patient.City) + "',"
            + "'" + SOut.String(patient.State) + "',"
            + "'" + SOut.String(patient.Zip) + "',"
            + "'" + SOut.String(patient.HmPhone) + "',"
            + "'" + SOut.String(patient.WkPhone) + "',"
            + "'" + SOut.String(patient.WirelessPhone) + "',"
            + SOut.Long(patient.Guarantor) + ","
            + "'" + SOut.String(patient.CreditType) + "',"
            + "'" + SOut.String(patient.Email) + "',"
            + "'" + SOut.String(patient.Salutation) + "',"
            + SOut.Double(patient.EstBalance) + ","
            + SOut.Long(patient.PriProv) + ","
            + SOut.Long(patient.SecProv) + ","
            + SOut.Long(patient.FeeSched) + ","
            + SOut.Long(patient.BillingType) + ","
            + "'" + SOut.String(patient.ImageFolder) + "',"
            + DbHelper.ParamChar + "paramAddrNote,"
            + DbHelper.ParamChar + "paramFamFinUrgNote,"
            + "'" + SOut.StringNote(patient.MedUrgNote, true) + "',"
            + "'" + SOut.String(patient.ApptModNote) + "',"
            + "'" + SOut.String(patient.StudentStatus) + "',"
            + "'" + SOut.String(patient.SchoolName) + "',"
            + "'" + SOut.String(patient.ChartNumber) + "',"
            + "'" + SOut.String(patient.MedicaidID) + "',"
            + SOut.Double(patient.Bal_0_30) + ","
            + SOut.Double(patient.Bal_31_60) + ","
            + SOut.Double(patient.Bal_61_90) + ","
            + SOut.Double(patient.BalOver90) + ","
            + SOut.Double(patient.InsEst) + ","
            + SOut.Double(patient.BalTotal) + ","
            + SOut.Long(patient.EmployerNum) + ","
            + "'" + SOut.String(patient.EmploymentNote) + "',"
            + "'" + SOut.String(patient.County) + "',"
            + SOut.Int((int) patient.GradeLevel) + ","
            + SOut.Int((int) patient.Urgency) + ","
            + SOut.Date(patient.DateFirstVisit) + ","
            + SOut.Long(patient.ClinicNum) + ","
            + "'" + SOut.String(patient.HasIns) + "',"
            + "'" + SOut.String(patient.TrophyFolder) + "',"
            + SOut.Bool(patient.PlannedIsDone) + ","
            + SOut.Bool(patient.Premed) + ","
            + "'" + SOut.String(patient.Ward) + "',"
            + SOut.Int((int) patient.PreferConfirmMethod) + ","
            + SOut.Int((int) patient.PreferContactMethod) + ","
            + SOut.Int((int) patient.PreferRecallMethod) + ","
            + SOut.Time(patient.SchedBeforeTime) + ","
            + SOut.Time(patient.SchedAfterTime) + ","
            + SOut.Byte(patient.SchedDayOfWeek) + ","
            + "'" + SOut.String(patient.Language) + "',"
            + SOut.Date(patient.AdmitDate) + ","
            + "'" + SOut.String(patient.Title) + "',"
            + SOut.Double(patient.PayPlanDue) + ","
            + SOut.Long(patient.SiteNum) + ","
            //DateTStamp can only be set by MySQL
            + SOut.Long(patient.ResponsParty) + ","
            + SOut.Byte(patient.CanadianEligibilityCode) + ","
            + SOut.Int(patient.AskToArriveEarly) + ","
            + SOut.Int((int) patient.PreferContactConfidential) + ","
            + SOut.Long(patient.SuperFamily) + ","
            + SOut.Int((int) patient.TxtMsgOk) + ","
            + "'" + SOut.String(patient.SmokingSnoMed) + "',"
            + "'" + SOut.String(patient.Country) + "',"
            + SOut.DateTime(patient.DateTimeDeceased) + ","
            + SOut.Int(patient.BillingCycleDay) + ","
            + SOut.Long(patient.SecUserNumEntry) + ","
            + "NOW()" + ","
            + SOut.Bool(patient.HasSuperBilling) + ","
            + SOut.Long(patient.PatNumCloneFrom) + ","
            + SOut.Long(patient.DiscountPlanNum) + ","
            + SOut.Bool(patient.HasSignedTil) + ","
            + SOut.Int((int) patient.ShortCodeOptIn) + ","
            + "'" + SOut.String(patient.SecurityHash) + "')";
        if (patient.AddrNote == null) patient.AddrNote = "";
        var paramAddrNote = new OdSqlParameter("paramAddrNote", SOut.StringNote(patient.AddrNote));
        if (patient.FamFinUrgNote == null) patient.FamFinUrgNote = "";
        var paramFamFinUrgNote = new OdSqlParameter("paramFamFinUrgNote", SOut.StringNote(patient.FamFinUrgNote));
        {
            patient.PatNum = Db.NonQ(command, true, "PatNum", "patient", paramAddrNote, paramFamFinUrgNote);
        }
        return patient.PatNum;
    }

    public static void Update(Patient patient)
    {
        var command = "UPDATE patient SET "
                      + "LName                    = '" + SOut.String(patient.LName) + "', "
                      + "FName                    = '" + SOut.String(patient.FName) + "', "
                      + "MiddleI                  = '" + SOut.String(patient.MiddleI) + "', "
                      + "Preferred                = '" + SOut.String(patient.Preferred) + "', "
                      + "PatStatus                =  " + SOut.Int((int) patient.PatStatus) + ", "
                      + "Gender                   =  " + SOut.Int((int) patient.Gender) + ", "
                      + "Position                 =  " + SOut.Int((int) patient.Position) + ", "
                      + "Birthdate                =  " + SOut.Date(patient.Birthdate) + ", "
                      + "SSN                      = '" + SOut.String(patient.SSN) + "', "
                      + "Address                  = '" + SOut.StringNote(patient.Address, true) + "', "
                      + "Address2                 = '" + SOut.StringNote(patient.Address2, true) + "', "
                      + "City                     = '" + SOut.String(patient.City) + "', "
                      + "State                    = '" + SOut.String(patient.State) + "', "
                      + "Zip                      = '" + SOut.String(patient.Zip) + "', "
                      + "HmPhone                  = '" + SOut.String(patient.HmPhone) + "', "
                      + "WkPhone                  = '" + SOut.String(patient.WkPhone) + "', "
                      + "WirelessPhone            = '" + SOut.String(patient.WirelessPhone) + "', "
                      + "Guarantor                =  " + SOut.Long(patient.Guarantor) + ", "
                      + "CreditType               = '" + SOut.String(patient.CreditType) + "', "
                      + "Email                    = '" + SOut.String(patient.Email) + "', "
                      + "Salutation               = '" + SOut.String(patient.Salutation) + "', "
                      + "EstBalance               =  " + SOut.Double(patient.EstBalance) + ", "
                      + "PriProv                  =  " + SOut.Long(patient.PriProv) + ", "
                      + "SecProv                  =  " + SOut.Long(patient.SecProv) + ", "
                      + "FeeSched                 =  " + SOut.Long(patient.FeeSched) + ", "
                      + "BillingType              =  " + SOut.Long(patient.BillingType) + ", "
                      + "ImageFolder              = '" + SOut.String(patient.ImageFolder) + "', "
                      + "AddrNote                 =  " + DbHelper.ParamChar + "paramAddrNote, "
                      + "FamFinUrgNote            =  " + DbHelper.ParamChar + "paramFamFinUrgNote, "
                      + "MedUrgNote               = '" + SOut.StringNote(patient.MedUrgNote, true) + "', "
                      + "ApptModNote              = '" + SOut.String(patient.ApptModNote) + "', "
                      + "StudentStatus            = '" + SOut.String(patient.StudentStatus) + "', "
                      + "SchoolName               = '" + SOut.String(patient.SchoolName) + "', "
                      + "ChartNumber              = '" + SOut.String(patient.ChartNumber) + "', "
                      + "MedicaidID               = '" + SOut.String(patient.MedicaidID) + "', "
                      + "Bal_0_30                 =  " + SOut.Double(patient.Bal_0_30) + ", "
                      + "Bal_31_60                =  " + SOut.Double(patient.Bal_31_60) + ", "
                      + "Bal_61_90                =  " + SOut.Double(patient.Bal_61_90) + ", "
                      + "BalOver90                =  " + SOut.Double(patient.BalOver90) + ", "
                      + "InsEst                   =  " + SOut.Double(patient.InsEst) + ", "
                      + "BalTotal                 =  " + SOut.Double(patient.BalTotal) + ", "
                      + "EmployerNum              =  " + SOut.Long(patient.EmployerNum) + ", "
                      + "EmploymentNote           = '" + SOut.String(patient.EmploymentNote) + "', "
                      + "County                   = '" + SOut.String(patient.County) + "', "
                      + "GradeLevel               =  " + SOut.Int((int) patient.GradeLevel) + ", "
                      + "Urgency                  =  " + SOut.Int((int) patient.Urgency) + ", "
                      + "DateFirstVisit           =  " + SOut.Date(patient.DateFirstVisit) + ", "
                      + "ClinicNum                =  " + SOut.Long(patient.ClinicNum) + ", "
                      + "HasIns                   = '" + SOut.String(patient.HasIns) + "', "
                      + "TrophyFolder             = '" + SOut.String(patient.TrophyFolder) + "', "
                      + "PlannedIsDone            =  " + SOut.Bool(patient.PlannedIsDone) + ", "
                      + "Premed                   =  " + SOut.Bool(patient.Premed) + ", "
                      + "Ward                     = '" + SOut.String(patient.Ward) + "', "
                      + "PreferConfirmMethod      =  " + SOut.Int((int) patient.PreferConfirmMethod) + ", "
                      + "PreferContactMethod      =  " + SOut.Int((int) patient.PreferContactMethod) + ", "
                      + "PreferRecallMethod       =  " + SOut.Int((int) patient.PreferRecallMethod) + ", "
                      + "SchedBeforeTime          =  " + SOut.Time(patient.SchedBeforeTime) + ", "
                      + "SchedAfterTime           =  " + SOut.Time(patient.SchedAfterTime) + ", "
                      + "SchedDayOfWeek           =  " + SOut.Byte(patient.SchedDayOfWeek) + ", "
                      + "Language                 = '" + SOut.String(patient.Language) + "', "
                      + "AdmitDate                =  " + SOut.Date(patient.AdmitDate) + ", "
                      + "Title                    = '" + SOut.String(patient.Title) + "', "
                      + "PayPlanDue               =  " + SOut.Double(patient.PayPlanDue) + ", "
                      + "SiteNum                  =  " + SOut.Long(patient.SiteNum) + ", "
                      //DateTStamp can only be set by MySQL
                      + "ResponsParty             =  " + SOut.Long(patient.ResponsParty) + ", "
                      + "CanadianEligibilityCode  =  " + SOut.Byte(patient.CanadianEligibilityCode) + ", "
                      + "AskToArriveEarly         =  " + SOut.Int(patient.AskToArriveEarly) + ", "
                      + "PreferContactConfidential=  " + SOut.Int((int) patient.PreferContactConfidential) + ", "
                      + "SuperFamily              =  " + SOut.Long(patient.SuperFamily) + ", "
                      + "TxtMsgOk                 =  " + SOut.Int((int) patient.TxtMsgOk) + ", "
                      + "SmokingSnoMed            = '" + SOut.String(patient.SmokingSnoMed) + "', "
                      + "Country                  = '" + SOut.String(patient.Country) + "', "
                      + "DateTimeDeceased         =  " + SOut.DateTime(patient.DateTimeDeceased) + ", "
                      + "BillingCycleDay          =  " + SOut.Int(patient.BillingCycleDay) + ", "
                      //SecUserNumEntry excluded from update
                      //SecDateEntry not allowed to change
                      + "HasSuperBilling          =  " + SOut.Bool(patient.HasSuperBilling) + ", "
                      + "PatNumCloneFrom          =  " + SOut.Long(patient.PatNumCloneFrom) + ", "
                      + "DiscountPlanNum          =  " + SOut.Long(patient.DiscountPlanNum) + ", "
                      + "HasSignedTil             =  " + SOut.Bool(patient.HasSignedTil) + ", "
                      + "ShortCodeOptIn           =  " + SOut.Int((int) patient.ShortCodeOptIn) + ", "
                      + "SecurityHash             = '" + SOut.String(patient.SecurityHash) + "' "
                      + "WHERE PatNum = " + SOut.Long(patient.PatNum);
        if (patient.AddrNote == null) patient.AddrNote = "";
        var paramAddrNote = new OdSqlParameter("paramAddrNote", SOut.StringNote(patient.AddrNote));
        if (patient.FamFinUrgNote == null) patient.FamFinUrgNote = "";
        var paramFamFinUrgNote = new OdSqlParameter("paramFamFinUrgNote", SOut.StringNote(patient.FamFinUrgNote));
        Db.NonQ(command, paramAddrNote, paramFamFinUrgNote);
    }

    public static bool Update(Patient patient, Patient oldPatient)
    {
        var command = "";
        if (patient.LName != oldPatient.LName)
        {
            if (command != "") command += ",";
            command += "LName = '" + SOut.String(patient.LName) + "'";
        }

        if (patient.FName != oldPatient.FName)
        {
            if (command != "") command += ",";
            command += "FName = '" + SOut.String(patient.FName) + "'";
        }

        if (patient.MiddleI != oldPatient.MiddleI)
        {
            if (command != "") command += ",";
            command += "MiddleI = '" + SOut.String(patient.MiddleI) + "'";
        }

        if (patient.Preferred != oldPatient.Preferred)
        {
            if (command != "") command += ",";
            command += "Preferred = '" + SOut.String(patient.Preferred) + "'";
        }

        if (patient.PatStatus != oldPatient.PatStatus)
        {
            if (command != "") command += ",";
            command += "PatStatus = " + SOut.Int((int) patient.PatStatus) + "";
        }

        if (patient.Gender != oldPatient.Gender)
        {
            if (command != "") command += ",";
            command += "Gender = " + SOut.Int((int) patient.Gender) + "";
        }

        if (patient.Position != oldPatient.Position)
        {
            if (command != "") command += ",";
            command += "Position = " + SOut.Int((int) patient.Position) + "";
        }

        if (patient.Birthdate.Date != oldPatient.Birthdate.Date)
        {
            if (command != "") command += ",";
            command += "Birthdate = " + SOut.Date(patient.Birthdate) + "";
        }

        if (patient.SSN != oldPatient.SSN)
        {
            if (command != "") command += ",";
            command += "SSN = '" + SOut.String(patient.SSN) + "'";
        }

        if (patient.Address != oldPatient.Address)
        {
            if (command != "") command += ",";
            command += "Address = '" + SOut.StringNote(patient.Address, true) + "'";
        }

        if (patient.Address2 != oldPatient.Address2)
        {
            if (command != "") command += ",";
            command += "Address2 = '" + SOut.StringNote(patient.Address2, true) + "'";
        }

        if (patient.City != oldPatient.City)
        {
            if (command != "") command += ",";
            command += "City = '" + SOut.String(patient.City) + "'";
        }

        if (patient.State != oldPatient.State)
        {
            if (command != "") command += ",";
            command += "State = '" + SOut.String(patient.State) + "'";
        }

        if (patient.Zip != oldPatient.Zip)
        {
            if (command != "") command += ",";
            command += "Zip = '" + SOut.String(patient.Zip) + "'";
        }

        if (patient.HmPhone != oldPatient.HmPhone)
        {
            if (command != "") command += ",";
            command += "HmPhone = '" + SOut.String(patient.HmPhone) + "'";
        }

        if (patient.WkPhone != oldPatient.WkPhone)
        {
            if (command != "") command += ",";
            command += "WkPhone = '" + SOut.String(patient.WkPhone) + "'";
        }

        if (patient.WirelessPhone != oldPatient.WirelessPhone)
        {
            if (command != "") command += ",";
            command += "WirelessPhone = '" + SOut.String(patient.WirelessPhone) + "'";
        }

        if (patient.Guarantor != oldPatient.Guarantor)
        {
            if (command != "") command += ",";
            command += "Guarantor = " + SOut.Long(patient.Guarantor) + "";
        }

        if (patient.CreditType != oldPatient.CreditType)
        {
            if (command != "") command += ",";
            command += "CreditType = '" + SOut.String(patient.CreditType) + "'";
        }

        if (patient.Email != oldPatient.Email)
        {
            if (command != "") command += ",";
            command += "Email = '" + SOut.String(patient.Email) + "'";
        }

        if (patient.Salutation != oldPatient.Salutation)
        {
            if (command != "") command += ",";
            command += "Salutation = '" + SOut.String(patient.Salutation) + "'";
        }

        if (patient.EstBalance != oldPatient.EstBalance)
        {
            if (command != "") command += ",";
            command += "EstBalance = " + SOut.Double(patient.EstBalance) + "";
        }

        if (patient.PriProv != oldPatient.PriProv)
        {
            if (command != "") command += ",";
            command += "PriProv = " + SOut.Long(patient.PriProv) + "";
        }

        if (patient.SecProv != oldPatient.SecProv)
        {
            if (command != "") command += ",";
            command += "SecProv = " + SOut.Long(patient.SecProv) + "";
        }

        if (patient.FeeSched != oldPatient.FeeSched)
        {
            if (command != "") command += ",";
            command += "FeeSched = " + SOut.Long(patient.FeeSched) + "";
        }

        if (patient.BillingType != oldPatient.BillingType)
        {
            if (command != "") command += ",";
            command += "BillingType = " + SOut.Long(patient.BillingType) + "";
        }

        if (patient.ImageFolder != oldPatient.ImageFolder)
        {
            if (command != "") command += ",";
            command += "ImageFolder = '" + SOut.String(patient.ImageFolder) + "'";
        }

        if (patient.AddrNote != oldPatient.AddrNote)
        {
            if (command != "") command += ",";
            command += "AddrNote = " + DbHelper.ParamChar + "paramAddrNote";
        }

        if (patient.FamFinUrgNote != oldPatient.FamFinUrgNote)
        {
            if (command != "") command += ",";
            command += "FamFinUrgNote = " + DbHelper.ParamChar + "paramFamFinUrgNote";
        }

        if (patient.MedUrgNote != oldPatient.MedUrgNote)
        {
            if (command != "") command += ",";
            command += "MedUrgNote = '" + SOut.StringNote(patient.MedUrgNote, true) + "'";
        }

        if (patient.ApptModNote != oldPatient.ApptModNote)
        {
            if (command != "") command += ",";
            command += "ApptModNote = '" + SOut.String(patient.ApptModNote) + "'";
        }

        if (patient.StudentStatus != oldPatient.StudentStatus)
        {
            if (command != "") command += ",";
            command += "StudentStatus = '" + SOut.String(patient.StudentStatus) + "'";
        }

        if (patient.SchoolName != oldPatient.SchoolName)
        {
            if (command != "") command += ",";
            command += "SchoolName = '" + SOut.String(patient.SchoolName) + "'";
        }

        if (patient.ChartNumber != oldPatient.ChartNumber)
        {
            if (command != "") command += ",";
            command += "ChartNumber = '" + SOut.String(patient.ChartNumber) + "'";
        }

        if (patient.MedicaidID != oldPatient.MedicaidID)
        {
            if (command != "") command += ",";
            command += "MedicaidID = '" + SOut.String(patient.MedicaidID) + "'";
        }

        if (patient.Bal_0_30 != oldPatient.Bal_0_30)
        {
            if (command != "") command += ",";
            command += "Bal_0_30 = " + SOut.Double(patient.Bal_0_30) + "";
        }

        if (patient.Bal_31_60 != oldPatient.Bal_31_60)
        {
            if (command != "") command += ",";
            command += "Bal_31_60 = " + SOut.Double(patient.Bal_31_60) + "";
        }

        if (patient.Bal_61_90 != oldPatient.Bal_61_90)
        {
            if (command != "") command += ",";
            command += "Bal_61_90 = " + SOut.Double(patient.Bal_61_90) + "";
        }

        if (patient.BalOver90 != oldPatient.BalOver90)
        {
            if (command != "") command += ",";
            command += "BalOver90 = " + SOut.Double(patient.BalOver90) + "";
        }

        if (patient.InsEst != oldPatient.InsEst)
        {
            if (command != "") command += ",";
            command += "InsEst = " + SOut.Double(patient.InsEst) + "";
        }

        if (patient.BalTotal != oldPatient.BalTotal)
        {
            if (command != "") command += ",";
            command += "BalTotal = " + SOut.Double(patient.BalTotal) + "";
        }

        if (patient.EmployerNum != oldPatient.EmployerNum)
        {
            if (command != "") command += ",";
            command += "EmployerNum = " + SOut.Long(patient.EmployerNum) + "";
        }

        if (patient.EmploymentNote != oldPatient.EmploymentNote)
        {
            if (command != "") command += ",";
            command += "EmploymentNote = '" + SOut.String(patient.EmploymentNote) + "'";
        }

        if (patient.County != oldPatient.County)
        {
            if (command != "") command += ",";
            command += "County = '" + SOut.String(patient.County) + "'";
        }

        if (patient.GradeLevel != oldPatient.GradeLevel)
        {
            if (command != "") command += ",";
            command += "GradeLevel = " + SOut.Int((int) patient.GradeLevel) + "";
        }

        if (patient.Urgency != oldPatient.Urgency)
        {
            if (command != "") command += ",";
            command += "Urgency = " + SOut.Int((int) patient.Urgency) + "";
        }

        if (patient.DateFirstVisit.Date != oldPatient.DateFirstVisit.Date)
        {
            if (command != "") command += ",";
            command += "DateFirstVisit = " + SOut.Date(patient.DateFirstVisit) + "";
        }

        if (patient.ClinicNum != oldPatient.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(patient.ClinicNum) + "";
        }

        if (patient.HasIns != oldPatient.HasIns)
        {
            if (command != "") command += ",";
            command += "HasIns = '" + SOut.String(patient.HasIns) + "'";
        }

        if (patient.TrophyFolder != oldPatient.TrophyFolder)
        {
            if (command != "") command += ",";
            command += "TrophyFolder = '" + SOut.String(patient.TrophyFolder) + "'";
        }

        if (patient.PlannedIsDone != oldPatient.PlannedIsDone)
        {
            if (command != "") command += ",";
            command += "PlannedIsDone = " + SOut.Bool(patient.PlannedIsDone) + "";
        }

        if (patient.Premed != oldPatient.Premed)
        {
            if (command != "") command += ",";
            command += "Premed = " + SOut.Bool(patient.Premed) + "";
        }

        if (patient.Ward != oldPatient.Ward)
        {
            if (command != "") command += ",";
            command += "Ward = '" + SOut.String(patient.Ward) + "'";
        }

        if (patient.PreferConfirmMethod != oldPatient.PreferConfirmMethod)
        {
            if (command != "") command += ",";
            command += "PreferConfirmMethod = " + SOut.Int((int) patient.PreferConfirmMethod) + "";
        }

        if (patient.PreferContactMethod != oldPatient.PreferContactMethod)
        {
            if (command != "") command += ",";
            command += "PreferContactMethod = " + SOut.Int((int) patient.PreferContactMethod) + "";
        }

        if (patient.PreferRecallMethod != oldPatient.PreferRecallMethod)
        {
            if (command != "") command += ",";
            command += "PreferRecallMethod = " + SOut.Int((int) patient.PreferRecallMethod) + "";
        }

        if (patient.SchedBeforeTime != oldPatient.SchedBeforeTime)
        {
            if (command != "") command += ",";
            command += "SchedBeforeTime = " + SOut.Time(patient.SchedBeforeTime) + "";
        }

        if (patient.SchedAfterTime != oldPatient.SchedAfterTime)
        {
            if (command != "") command += ",";
            command += "SchedAfterTime = " + SOut.Time(patient.SchedAfterTime) + "";
        }

        if (patient.SchedDayOfWeek != oldPatient.SchedDayOfWeek)
        {
            if (command != "") command += ",";
            command += "SchedDayOfWeek = " + SOut.Byte(patient.SchedDayOfWeek) + "";
        }

        if (patient.Language != oldPatient.Language)
        {
            if (command != "") command += ",";
            command += "Language = '" + SOut.String(patient.Language) + "'";
        }

        if (patient.AdmitDate.Date != oldPatient.AdmitDate.Date)
        {
            if (command != "") command += ",";
            command += "AdmitDate = " + SOut.Date(patient.AdmitDate) + "";
        }

        if (patient.Title != oldPatient.Title)
        {
            if (command != "") command += ",";
            command += "Title = '" + SOut.String(patient.Title) + "'";
        }

        if (patient.PayPlanDue != oldPatient.PayPlanDue)
        {
            if (command != "") command += ",";
            command += "PayPlanDue = " + SOut.Double(patient.PayPlanDue) + "";
        }

        if (patient.SiteNum != oldPatient.SiteNum)
        {
            if (command != "") command += ",";
            command += "SiteNum = " + SOut.Long(patient.SiteNum) + "";
        }

        //DateTStamp can only be set by MySQL
        if (patient.ResponsParty != oldPatient.ResponsParty)
        {
            if (command != "") command += ",";
            command += "ResponsParty = " + SOut.Long(patient.ResponsParty) + "";
        }

        if (patient.CanadianEligibilityCode != oldPatient.CanadianEligibilityCode)
        {
            if (command != "") command += ",";
            command += "CanadianEligibilityCode = " + SOut.Byte(patient.CanadianEligibilityCode) + "";
        }

        if (patient.AskToArriveEarly != oldPatient.AskToArriveEarly)
        {
            if (command != "") command += ",";
            command += "AskToArriveEarly = " + SOut.Int(patient.AskToArriveEarly) + "";
        }

        if (patient.PreferContactConfidential != oldPatient.PreferContactConfidential)
        {
            if (command != "") command += ",";
            command += "PreferContactConfidential = " + SOut.Int((int) patient.PreferContactConfidential) + "";
        }

        if (patient.SuperFamily != oldPatient.SuperFamily)
        {
            if (command != "") command += ",";
            command += "SuperFamily = " + SOut.Long(patient.SuperFamily) + "";
        }

        if (patient.TxtMsgOk != oldPatient.TxtMsgOk)
        {
            if (command != "") command += ",";
            command += "TxtMsgOk = " + SOut.Int((int) patient.TxtMsgOk) + "";
        }

        if (patient.SmokingSnoMed != oldPatient.SmokingSnoMed)
        {
            if (command != "") command += ",";
            command += "SmokingSnoMed = '" + SOut.String(patient.SmokingSnoMed) + "'";
        }

        if (patient.Country != oldPatient.Country)
        {
            if (command != "") command += ",";
            command += "Country = '" + SOut.String(patient.Country) + "'";
        }

        if (patient.DateTimeDeceased != oldPatient.DateTimeDeceased)
        {
            if (command != "") command += ",";
            command += "DateTimeDeceased = " + SOut.DateTime(patient.DateTimeDeceased) + "";
        }

        if (patient.BillingCycleDay != oldPatient.BillingCycleDay)
        {
            if (command != "") command += ",";
            command += "BillingCycleDay = " + SOut.Int(patient.BillingCycleDay) + "";
        }

        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        if (patient.HasSuperBilling != oldPatient.HasSuperBilling)
        {
            if (command != "") command += ",";
            command += "HasSuperBilling = " + SOut.Bool(patient.HasSuperBilling) + "";
        }

        if (patient.PatNumCloneFrom != oldPatient.PatNumCloneFrom)
        {
            if (command != "") command += ",";
            command += "PatNumCloneFrom = " + SOut.Long(patient.PatNumCloneFrom) + "";
        }

        if (patient.DiscountPlanNum != oldPatient.DiscountPlanNum)
        {
            if (command != "") command += ",";
            command += "DiscountPlanNum = " + SOut.Long(patient.DiscountPlanNum) + "";
        }

        if (patient.HasSignedTil != oldPatient.HasSignedTil)
        {
            if (command != "") command += ",";
            command += "HasSignedTil = " + SOut.Bool(patient.HasSignedTil) + "";
        }

        if (patient.ShortCodeOptIn != oldPatient.ShortCodeOptIn)
        {
            if (command != "") command += ",";
            command += "ShortCodeOptIn = " + SOut.Int((int) patient.ShortCodeOptIn) + "";
        }

        if (patient.SecurityHash != oldPatient.SecurityHash)
        {
            if (command != "") command += ",";
            command += "SecurityHash = '" + SOut.String(patient.SecurityHash) + "'";
        }

        if (command == "") return false;
        if (patient.AddrNote == null) patient.AddrNote = "";
        var paramAddrNote = new OdSqlParameter("paramAddrNote", SOut.StringNote(patient.AddrNote));
        if (patient.FamFinUrgNote == null) patient.FamFinUrgNote = "";
        var paramFamFinUrgNote = new OdSqlParameter("paramFamFinUrgNote", SOut.StringNote(patient.FamFinUrgNote));
        command = "UPDATE patient SET " + command
                                        + " WHERE PatNum = " + SOut.Long(patient.PatNum);
        Db.NonQ(command, paramAddrNote, paramFamFinUrgNote);
        return true;
    }

    public static bool UpdateComparison(Patient patient, Patient oldPatient)
    {
        if (patient.LName != oldPatient.LName) return true;
        if (patient.FName != oldPatient.FName) return true;
        if (patient.MiddleI != oldPatient.MiddleI) return true;
        if (patient.Preferred != oldPatient.Preferred) return true;
        if (patient.PatStatus != oldPatient.PatStatus) return true;
        if (patient.Gender != oldPatient.Gender) return true;
        if (patient.Position != oldPatient.Position) return true;
        if (patient.Birthdate.Date != oldPatient.Birthdate.Date) return true;
        if (patient.SSN != oldPatient.SSN) return true;
        if (patient.Address != oldPatient.Address) return true;
        if (patient.Address2 != oldPatient.Address2) return true;
        if (patient.City != oldPatient.City) return true;
        if (patient.State != oldPatient.State) return true;
        if (patient.Zip != oldPatient.Zip) return true;
        if (patient.HmPhone != oldPatient.HmPhone) return true;
        if (patient.WkPhone != oldPatient.WkPhone) return true;
        if (patient.WirelessPhone != oldPatient.WirelessPhone) return true;
        if (patient.Guarantor != oldPatient.Guarantor) return true;
        if (patient.CreditType != oldPatient.CreditType) return true;
        if (patient.Email != oldPatient.Email) return true;
        if (patient.Salutation != oldPatient.Salutation) return true;
        if (patient.EstBalance != oldPatient.EstBalance) return true;
        if (patient.PriProv != oldPatient.PriProv) return true;
        if (patient.SecProv != oldPatient.SecProv) return true;
        if (patient.FeeSched != oldPatient.FeeSched) return true;
        if (patient.BillingType != oldPatient.BillingType) return true;
        if (patient.ImageFolder != oldPatient.ImageFolder) return true;
        if (patient.AddrNote != oldPatient.AddrNote) return true;
        if (patient.FamFinUrgNote != oldPatient.FamFinUrgNote) return true;
        if (patient.MedUrgNote != oldPatient.MedUrgNote) return true;
        if (patient.ApptModNote != oldPatient.ApptModNote) return true;
        if (patient.StudentStatus != oldPatient.StudentStatus) return true;
        if (patient.SchoolName != oldPatient.SchoolName) return true;
        if (patient.ChartNumber != oldPatient.ChartNumber) return true;
        if (patient.MedicaidID != oldPatient.MedicaidID) return true;
        if (patient.Bal_0_30 != oldPatient.Bal_0_30) return true;
        if (patient.Bal_31_60 != oldPatient.Bal_31_60) return true;
        if (patient.Bal_61_90 != oldPatient.Bal_61_90) return true;
        if (patient.BalOver90 != oldPatient.BalOver90) return true;
        if (patient.InsEst != oldPatient.InsEst) return true;
        if (patient.BalTotal != oldPatient.BalTotal) return true;
        if (patient.EmployerNum != oldPatient.EmployerNum) return true;
        if (patient.EmploymentNote != oldPatient.EmploymentNote) return true;
        if (patient.County != oldPatient.County) return true;
        if (patient.GradeLevel != oldPatient.GradeLevel) return true;
        if (patient.Urgency != oldPatient.Urgency) return true;
        if (patient.DateFirstVisit.Date != oldPatient.DateFirstVisit.Date) return true;
        if (patient.ClinicNum != oldPatient.ClinicNum) return true;
        if (patient.HasIns != oldPatient.HasIns) return true;
        if (patient.TrophyFolder != oldPatient.TrophyFolder) return true;
        if (patient.PlannedIsDone != oldPatient.PlannedIsDone) return true;
        if (patient.Premed != oldPatient.Premed) return true;
        if (patient.Ward != oldPatient.Ward) return true;
        if (patient.PreferConfirmMethod != oldPatient.PreferConfirmMethod) return true;
        if (patient.PreferContactMethod != oldPatient.PreferContactMethod) return true;
        if (patient.PreferRecallMethod != oldPatient.PreferRecallMethod) return true;
        if (patient.SchedBeforeTime != oldPatient.SchedBeforeTime) return true;
        if (patient.SchedAfterTime != oldPatient.SchedAfterTime) return true;
        if (patient.SchedDayOfWeek != oldPatient.SchedDayOfWeek) return true;
        if (patient.Language != oldPatient.Language) return true;
        if (patient.AdmitDate.Date != oldPatient.AdmitDate.Date) return true;
        if (patient.Title != oldPatient.Title) return true;
        if (patient.PayPlanDue != oldPatient.PayPlanDue) return true;
        if (patient.SiteNum != oldPatient.SiteNum) return true;
        //DateTStamp can only be set by MySQL
        if (patient.ResponsParty != oldPatient.ResponsParty) return true;
        if (patient.CanadianEligibilityCode != oldPatient.CanadianEligibilityCode) return true;
        if (patient.AskToArriveEarly != oldPatient.AskToArriveEarly) return true;
        if (patient.PreferContactConfidential != oldPatient.PreferContactConfidential) return true;
        if (patient.SuperFamily != oldPatient.SuperFamily) return true;
        if (patient.TxtMsgOk != oldPatient.TxtMsgOk) return true;
        if (patient.SmokingSnoMed != oldPatient.SmokingSnoMed) return true;
        if (patient.Country != oldPatient.Country) return true;
        if (patient.DateTimeDeceased != oldPatient.DateTimeDeceased) return true;
        if (patient.BillingCycleDay != oldPatient.BillingCycleDay) return true;
        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        if (patient.HasSuperBilling != oldPatient.HasSuperBilling) return true;
        if (patient.PatNumCloneFrom != oldPatient.PatNumCloneFrom) return true;
        if (patient.DiscountPlanNum != oldPatient.DiscountPlanNum) return true;
        if (patient.HasSignedTil != oldPatient.HasSignedTil) return true;
        if (patient.ShortCodeOptIn != oldPatient.ShortCodeOptIn) return true;
        if (patient.SecurityHash != oldPatient.SecurityHash) return true;
        return false;
    }
}