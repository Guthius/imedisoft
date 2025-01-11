#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

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
        Patient patient;
        foreach (DataRow row in table.Rows)
        {
            patient = new Patient();
            patient.PatNum = SIn.Long(row["PatNum"].ToString());
            patient.LName = SIn.String(row["LName"].ToString());
            patient.FName = SIn.String(row["FName"].ToString());
            patient.MiddleI = SIn.String(row["MiddleI"].ToString());
            patient.Preferred = SIn.String(row["Preferred"].ToString());
            patient.PatStatus = (PatientStatus) SIn.Int(row["PatStatus"].ToString());
            patient.Gender = (PatientGender) SIn.Int(row["Gender"].ToString());
            patient.Position = (PatientPosition) SIn.Int(row["Position"].ToString());
            patient.Birthdate = SIn.Date(row["Birthdate"].ToString());
            patient.SSN = SIn.String(row["SSN"].ToString());
            patient.Address = SIn.String(row["Address"].ToString());
            patient.Address2 = SIn.String(row["Address2"].ToString());
            patient.City = SIn.String(row["City"].ToString());
            patient.State = SIn.String(row["State"].ToString());
            patient.Zip = SIn.String(row["Zip"].ToString());
            patient.HmPhone = SIn.String(row["HmPhone"].ToString());
            patient.WkPhone = SIn.String(row["WkPhone"].ToString());
            patient.WirelessPhone = SIn.String(row["WirelessPhone"].ToString());
            patient.Guarantor = SIn.Long(row["Guarantor"].ToString());
            patient.CreditType = SIn.String(row["CreditType"].ToString());
            patient.Email = SIn.String(row["Email"].ToString());
            patient.Salutation = SIn.String(row["Salutation"].ToString());
            patient.EstBalance = SIn.Double(row["EstBalance"].ToString());
            patient.PriProv = SIn.Long(row["PriProv"].ToString());
            patient.SecProv = SIn.Long(row["SecProv"].ToString());
            patient.FeeSched = SIn.Long(row["FeeSched"].ToString());
            patient.BillingType = SIn.Long(row["BillingType"].ToString());
            patient.ImageFolder = SIn.String(row["ImageFolder"].ToString());
            patient.AddrNote = SIn.String(row["AddrNote"].ToString());
            patient.FamFinUrgNote = SIn.String(row["FamFinUrgNote"].ToString());
            patient.MedUrgNote = SIn.String(row["MedUrgNote"].ToString());
            patient.ApptModNote = SIn.String(row["ApptModNote"].ToString());
            patient.StudentStatus = SIn.String(row["StudentStatus"].ToString());
            patient.SchoolName = SIn.String(row["SchoolName"].ToString());
            patient.ChartNumber = SIn.String(row["ChartNumber"].ToString());
            patient.MedicaidID = SIn.String(row["MedicaidID"].ToString());
            patient.Bal_0_30 = SIn.Double(row["Bal_0_30"].ToString());
            patient.Bal_31_60 = SIn.Double(row["Bal_31_60"].ToString());
            patient.Bal_61_90 = SIn.Double(row["Bal_61_90"].ToString());
            patient.BalOver90 = SIn.Double(row["BalOver90"].ToString());
            patient.InsEst = SIn.Double(row["InsEst"].ToString());
            patient.BalTotal = SIn.Double(row["BalTotal"].ToString());
            patient.EmployerNum = SIn.Long(row["EmployerNum"].ToString());
            patient.EmploymentNote = SIn.String(row["EmploymentNote"].ToString());
            patient.County = SIn.String(row["County"].ToString());
            patient.GradeLevel = (PatientGrade) SIn.Int(row["GradeLevel"].ToString());
            patient.Urgency = (TreatmentUrgency) SIn.Int(row["Urgency"].ToString());
            patient.DateFirstVisit = SIn.Date(row["DateFirstVisit"].ToString());
            patient.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            patient.HasIns = SIn.String(row["HasIns"].ToString());
            patient.TrophyFolder = SIn.String(row["TrophyFolder"].ToString());
            patient.PlannedIsDone = SIn.Bool(row["PlannedIsDone"].ToString());
            patient.Premed = SIn.Bool(row["Premed"].ToString());
            patient.Ward = SIn.String(row["Ward"].ToString());
            patient.PreferConfirmMethod = (ContactMethod) SIn.Int(row["PreferConfirmMethod"].ToString());
            patient.PreferContactMethod = (ContactMethod) SIn.Int(row["PreferContactMethod"].ToString());
            patient.PreferRecallMethod = (ContactMethod) SIn.Int(row["PreferRecallMethod"].ToString());
            patient.SchedBeforeTime = SIn.TimeSpan(row["SchedBeforeTime"].ToString());
            patient.SchedAfterTime = SIn.TimeSpan(row["SchedAfterTime"].ToString());
            patient.SchedDayOfWeek = SIn.Byte(row["SchedDayOfWeek"].ToString());
            patient.Language = SIn.String(row["Language"].ToString());
            patient.AdmitDate = SIn.Date(row["AdmitDate"].ToString());
            patient.Title = SIn.String(row["Title"].ToString());
            patient.PayPlanDue = SIn.Double(row["PayPlanDue"].ToString());
            patient.SiteNum = SIn.Long(row["SiteNum"].ToString());
            patient.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            patient.ResponsParty = SIn.Long(row["ResponsParty"].ToString());
            patient.CanadianEligibilityCode = SIn.Byte(row["CanadianEligibilityCode"].ToString());
            patient.AskToArriveEarly = SIn.Int(row["AskToArriveEarly"].ToString());
            patient.PreferContactConfidential = (ContactMethod) SIn.Int(row["PreferContactConfidential"].ToString());
            patient.SuperFamily = SIn.Long(row["SuperFamily"].ToString());
            patient.TxtMsgOk = (YN) SIn.Int(row["TxtMsgOk"].ToString());
            patient.SmokingSnoMed = SIn.String(row["SmokingSnoMed"].ToString());
            patient.Country = SIn.String(row["Country"].ToString());
            patient.DateTimeDeceased = SIn.DateTime(row["DateTimeDeceased"].ToString());
            patient.BillingCycleDay = SIn.Int(row["BillingCycleDay"].ToString());
            patient.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            patient.SecDateEntry = SIn.Date(row["SecDateEntry"].ToString());
            patient.HasSuperBilling = SIn.Bool(row["HasSuperBilling"].ToString());
            patient.PatNumCloneFrom = SIn.Long(row["PatNumCloneFrom"].ToString());
            patient.DiscountPlanNum = SIn.Long(row["DiscountPlanNum"].ToString());
            patient.HasSignedTil = SIn.Bool(row["HasSignedTil"].ToString());
            patient.ShortCodeOptIn = (YN) SIn.Int(row["ShortCodeOptIn"].ToString());
            patient.SecurityHash = SIn.String(row["SecurityHash"].ToString());
            retVal.Add(patient);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Patient> listPatients, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Patient";
        var table = new DataTable(tableName);
        table.Columns.Add("PatNum");
        table.Columns.Add("LName");
        table.Columns.Add("FName");
        table.Columns.Add("MiddleI");
        table.Columns.Add("Preferred");
        table.Columns.Add("PatStatus");
        table.Columns.Add("Gender");
        table.Columns.Add("Position");
        table.Columns.Add("Birthdate");
        table.Columns.Add("SSN");
        table.Columns.Add("Address");
        table.Columns.Add("Address2");
        table.Columns.Add("City");
        table.Columns.Add("State");
        table.Columns.Add("Zip");
        table.Columns.Add("HmPhone");
        table.Columns.Add("WkPhone");
        table.Columns.Add("WirelessPhone");
        table.Columns.Add("Guarantor");
        table.Columns.Add("CreditType");
        table.Columns.Add("Email");
        table.Columns.Add("Salutation");
        table.Columns.Add("EstBalance");
        table.Columns.Add("PriProv");
        table.Columns.Add("SecProv");
        table.Columns.Add("FeeSched");
        table.Columns.Add("BillingType");
        table.Columns.Add("ImageFolder");
        table.Columns.Add("AddrNote");
        table.Columns.Add("FamFinUrgNote");
        table.Columns.Add("MedUrgNote");
        table.Columns.Add("ApptModNote");
        table.Columns.Add("StudentStatus");
        table.Columns.Add("SchoolName");
        table.Columns.Add("ChartNumber");
        table.Columns.Add("MedicaidID");
        table.Columns.Add("Bal_0_30");
        table.Columns.Add("Bal_31_60");
        table.Columns.Add("Bal_61_90");
        table.Columns.Add("BalOver90");
        table.Columns.Add("InsEst");
        table.Columns.Add("BalTotal");
        table.Columns.Add("EmployerNum");
        table.Columns.Add("EmploymentNote");
        table.Columns.Add("County");
        table.Columns.Add("GradeLevel");
        table.Columns.Add("Urgency");
        table.Columns.Add("DateFirstVisit");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("HasIns");
        table.Columns.Add("TrophyFolder");
        table.Columns.Add("PlannedIsDone");
        table.Columns.Add("Premed");
        table.Columns.Add("Ward");
        table.Columns.Add("PreferConfirmMethod");
        table.Columns.Add("PreferContactMethod");
        table.Columns.Add("PreferRecallMethod");
        table.Columns.Add("SchedBeforeTime");
        table.Columns.Add("SchedAfterTime");
        table.Columns.Add("SchedDayOfWeek");
        table.Columns.Add("Language");
        table.Columns.Add("AdmitDate");
        table.Columns.Add("Title");
        table.Columns.Add("PayPlanDue");
        table.Columns.Add("SiteNum");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("ResponsParty");
        table.Columns.Add("CanadianEligibilityCode");
        table.Columns.Add("AskToArriveEarly");
        table.Columns.Add("PreferContactConfidential");
        table.Columns.Add("SuperFamily");
        table.Columns.Add("TxtMsgOk");
        table.Columns.Add("SmokingSnoMed");
        table.Columns.Add("Country");
        table.Columns.Add("DateTimeDeceased");
        table.Columns.Add("BillingCycleDay");
        table.Columns.Add("SecUserNumEntry");
        table.Columns.Add("SecDateEntry");
        table.Columns.Add("HasSuperBilling");
        table.Columns.Add("PatNumCloneFrom");
        table.Columns.Add("DiscountPlanNum");
        table.Columns.Add("HasSignedTil");
        table.Columns.Add("ShortCodeOptIn");
        table.Columns.Add("SecurityHash");
        foreach (var patient in listPatients)
            table.Rows.Add(SOut.Long(patient.PatNum), patient.LName, patient.FName, patient.MiddleI, patient.Preferred, SOut.Int((int) patient.PatStatus), SOut.Int((int) patient.Gender), SOut.Int((int) patient.Position), SOut.DateT(patient.Birthdate, false), patient.SSN, patient.Address, patient.Address2, patient.City, patient.State, patient.Zip, patient.HmPhone, patient.WkPhone, patient.WirelessPhone, SOut.Long(patient.Guarantor), patient.CreditType, patient.Email, patient.Salutation, SOut.Double(patient.EstBalance), SOut.Long(patient.PriProv), SOut.Long(patient.SecProv), SOut.Long(patient.FeeSched), SOut.Long(patient.BillingType), patient.ImageFolder, patient.AddrNote, patient.FamFinUrgNote, patient.MedUrgNote, patient.ApptModNote, patient.StudentStatus, patient.SchoolName, patient.ChartNumber, patient.MedicaidID, SOut.Double(patient.Bal_0_30), SOut.Double(patient.Bal_31_60), SOut.Double(patient.Bal_61_90), SOut.Double(patient.BalOver90), SOut.Double(patient.InsEst), SOut.Double(patient.BalTotal), SOut.Long(patient.EmployerNum), patient.EmploymentNote, patient.County, SOut.Int((int) patient.GradeLevel), SOut.Int((int) patient.Urgency), SOut.DateT(patient.DateFirstVisit, false), SOut.Long(patient.ClinicNum), patient.HasIns, patient.TrophyFolder, SOut.Bool(patient.PlannedIsDone), SOut.Bool(patient.Premed), patient.Ward, SOut.Int((int) patient.PreferConfirmMethod), SOut.Int((int) patient.PreferContactMethod), SOut.Int((int) patient.PreferRecallMethod), SOut.Time(patient.SchedBeforeTime, false), SOut.Time(patient.SchedAfterTime, false), SOut.Byte(patient.SchedDayOfWeek), patient.Language, SOut.DateT(patient.AdmitDate, false), patient.Title, SOut.Double(patient.PayPlanDue), SOut.Long(patient.SiteNum), SOut.DateT(patient.DateTStamp, false), SOut.Long(patient.ResponsParty), SOut.Byte(patient.CanadianEligibilityCode), SOut.Int(patient.AskToArriveEarly), SOut.Int((int) patient.PreferContactConfidential), SOut.Long(patient.SuperFamily), SOut.Int((int) patient.TxtMsgOk), patient.SmokingSnoMed, patient.Country, SOut.DateT(patient.DateTimeDeceased, false), SOut.Int(patient.BillingCycleDay), SOut.Long(patient.SecUserNumEntry), SOut.DateT(patient.SecDateEntry, false), SOut.Bool(patient.HasSuperBilling), SOut.Long(patient.PatNumCloneFrom), SOut.Long(patient.DiscountPlanNum), SOut.Bool(patient.HasSignedTil), SOut.Int((int) patient.ShortCodeOptIn), patient.SecurityHash);
        return table;
    }

    public static long Insert(Patient patient)
    {
        return Insert(patient, false);
    }

    public static long Insert(Patient patient, bool useExistingPK)
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
            + SOut.DateT(patient.DateTimeDeceased) + ","
            + SOut.Int(patient.BillingCycleDay) + ","
            + SOut.Long(patient.SecUserNumEntry) + ","
            + DbHelper.Now() + ","
            + SOut.Bool(patient.HasSuperBilling) + ","
            + SOut.Long(patient.PatNumCloneFrom) + ","
            + SOut.Long(patient.DiscountPlanNum) + ","
            + SOut.Bool(patient.HasSignedTil) + ","
            + SOut.Int((int) patient.ShortCodeOptIn) + ","
            + "'" + SOut.String(patient.SecurityHash) + "')";
        if (patient.AddrNote == null) patient.AddrNote = "";
        var paramAddrNote = new OdSqlParameter("paramAddrNote", OdDbType.Text, SOut.StringNote(patient.AddrNote));
        if (patient.FamFinUrgNote == null) patient.FamFinUrgNote = "";
        var paramFamFinUrgNote = new OdSqlParameter("paramFamFinUrgNote", OdDbType.Text, SOut.StringNote(patient.FamFinUrgNote));
        {
            patient.PatNum = Db.NonQ(command, true, "PatNum", "patient", paramAddrNote, paramFamFinUrgNote);
        }
        return patient.PatNum;
    }

    public static void InsertMany(List<Patient> listPatients)
    {
        InsertMany(listPatients, false);
    }

    public static void InsertMany(List<Patient> listPatients, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listPatients.Count)
        {
            var patient = listPatients[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO patient (");
                if (useExistingPK) sbCommands.Append("PatNum,");
                sbCommands.Append("LName,FName,MiddleI,Preferred,PatStatus,Gender,Position,Birthdate,SSN,Address,Address2,City,State,Zip,HmPhone,WkPhone,WirelessPhone,Guarantor,CreditType,Email,Salutation,EstBalance,PriProv,SecProv,FeeSched,BillingType,ImageFolder,AddrNote,FamFinUrgNote,MedUrgNote,ApptModNote,StudentStatus,SchoolName,ChartNumber,MedicaidID,Bal_0_30,Bal_31_60,Bal_61_90,BalOver90,InsEst,BalTotal,EmployerNum,EmploymentNote,County,GradeLevel,Urgency,DateFirstVisit,ClinicNum,HasIns,TrophyFolder,PlannedIsDone,Premed,Ward,PreferConfirmMethod,PreferContactMethod,PreferRecallMethod,SchedBeforeTime,SchedAfterTime,SchedDayOfWeek,Language,AdmitDate,Title,PayPlanDue,SiteNum,ResponsParty,CanadianEligibilityCode,AskToArriveEarly,PreferContactConfidential,SuperFamily,TxtMsgOk,SmokingSnoMed,Country,DateTimeDeceased,BillingCycleDay,SecUserNumEntry,SecDateEntry,HasSuperBilling,PatNumCloneFrom,DiscountPlanNum,HasSignedTil,ShortCodeOptIn,SecurityHash) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(patient.PatNum));
                sbRow.Append(",");
            }

            sbRow.Append("'" + SOut.String(patient.LName) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.FName) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.MiddleI) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.Preferred) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) patient.PatStatus));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) patient.Gender));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) patient.Position));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(patient.Birthdate));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.SSN) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.StringNote(patient.Address, true) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.StringNote(patient.Address2, true) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.City) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.State) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.Zip) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.HmPhone) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.WkPhone) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.WirelessPhone) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(patient.Guarantor));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.CreditType) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.Email) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.Salutation) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Double(patient.EstBalance));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(patient.PriProv));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(patient.SecProv));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(patient.FeeSched));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(patient.BillingType));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.ImageFolder) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.AddrNote) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.FamFinUrgNote) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.StringNote(patient.MedUrgNote, true) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.ApptModNote) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.StudentStatus) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.SchoolName) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.ChartNumber) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.MedicaidID) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Double(patient.Bal_0_30));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(patient.Bal_31_60));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(patient.Bal_61_90));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(patient.BalOver90));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(patient.InsEst));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(patient.BalTotal));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(patient.EmployerNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.EmploymentNote) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.County) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) patient.GradeLevel));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) patient.Urgency));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(patient.DateFirstVisit));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(patient.ClinicNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.HasIns) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.TrophyFolder) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(patient.PlannedIsDone));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(patient.Premed));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.Ward) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) patient.PreferConfirmMethod));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) patient.PreferContactMethod));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) patient.PreferRecallMethod));
            sbRow.Append(",");
            sbRow.Append(SOut.Time(patient.SchedBeforeTime));
            sbRow.Append(",");
            sbRow.Append(SOut.Time(patient.SchedAfterTime));
            sbRow.Append(",");
            sbRow.Append(SOut.Byte(patient.SchedDayOfWeek));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.Language) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Date(patient.AdmitDate));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.Title) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Double(patient.PayPlanDue));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(patient.SiteNum));
            sbRow.Append(",");
            //DateTStamp can only be set by MySQL
            sbRow.Append(SOut.Long(patient.ResponsParty));
            sbRow.Append(",");
            sbRow.Append(SOut.Byte(patient.CanadianEligibilityCode));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(patient.AskToArriveEarly));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) patient.PreferContactConfidential));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(patient.SuperFamily));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) patient.TxtMsgOk));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.SmokingSnoMed) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.Country) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(patient.DateTimeDeceased));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(patient.BillingCycleDay));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(patient.SecUserNumEntry));
            sbRow.Append(",");
            sbRow.Append(DbHelper.Now());
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(patient.HasSuperBilling));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(patient.PatNumCloneFrom));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(patient.DiscountPlanNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(patient.HasSignedTil));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) patient.ShortCodeOptIn));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patient.SecurityHash) + "'");
            sbRow.Append(")");
            if (sbCommands.Length + sbRow.Length + 1 > TableBase.MaxAllowedPacketCount && countRows > 0)
            {
                Db.NonQ(sbCommands.ToString());
                sbCommands = null;
            }
            else
            {
                if (hasComma) sbCommands.Append(",");
                sbCommands.Append(sbRow);
                countRows++;
                if (index == listPatients.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(Patient patient)
    {
        return InsertNoCache(patient, false);
    }

    public static long InsertNoCache(Patient patient, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO patient (";
        if (isRandomKeys || useExistingPK) command += "PatNum,";
        command += "LName,FName,MiddleI,Preferred,PatStatus,Gender,Position,Birthdate,SSN,Address,Address2,City,State,Zip,HmPhone,WkPhone,WirelessPhone,Guarantor,CreditType,Email,Salutation,EstBalance,PriProv,SecProv,FeeSched,BillingType,ImageFolder,AddrNote,FamFinUrgNote,MedUrgNote,ApptModNote,StudentStatus,SchoolName,ChartNumber,MedicaidID,Bal_0_30,Bal_31_60,Bal_61_90,BalOver90,InsEst,BalTotal,EmployerNum,EmploymentNote,County,GradeLevel,Urgency,DateFirstVisit,ClinicNum,HasIns,TrophyFolder,PlannedIsDone,Premed,Ward,PreferConfirmMethod,PreferContactMethod,PreferRecallMethod,SchedBeforeTime,SchedAfterTime,SchedDayOfWeek,Language,AdmitDate,Title,PayPlanDue,SiteNum,ResponsParty,CanadianEligibilityCode,AskToArriveEarly,PreferContactConfidential,SuperFamily,TxtMsgOk,SmokingSnoMed,Country,DateTimeDeceased,BillingCycleDay,SecUserNumEntry,SecDateEntry,HasSuperBilling,PatNumCloneFrom,DiscountPlanNum,HasSignedTil,ShortCodeOptIn,SecurityHash) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(patient.PatNum) + ",";
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
            + SOut.DateT(patient.DateTimeDeceased) + ","
            + SOut.Int(patient.BillingCycleDay) + ","
            + SOut.Long(patient.SecUserNumEntry) + ","
            + DbHelper.Now() + ","
            + SOut.Bool(patient.HasSuperBilling) + ","
            + SOut.Long(patient.PatNumCloneFrom) + ","
            + SOut.Long(patient.DiscountPlanNum) + ","
            + SOut.Bool(patient.HasSignedTil) + ","
            + SOut.Int((int) patient.ShortCodeOptIn) + ","
            + "'" + SOut.String(patient.SecurityHash) + "')";
        if (patient.AddrNote == null) patient.AddrNote = "";
        var paramAddrNote = new OdSqlParameter("paramAddrNote", OdDbType.Text, SOut.StringNote(patient.AddrNote));
        if (patient.FamFinUrgNote == null) patient.FamFinUrgNote = "";
        var paramFamFinUrgNote = new OdSqlParameter("paramFamFinUrgNote", OdDbType.Text, SOut.StringNote(patient.FamFinUrgNote));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramAddrNote, paramFamFinUrgNote);
        else
            patient.PatNum = Db.NonQ(command, true, "PatNum", "patient", paramAddrNote, paramFamFinUrgNote);
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
                      + "DateTimeDeceased         =  " + SOut.DateT(patient.DateTimeDeceased) + ", "
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
        var paramAddrNote = new OdSqlParameter("paramAddrNote", OdDbType.Text, SOut.StringNote(patient.AddrNote));
        if (patient.FamFinUrgNote == null) patient.FamFinUrgNote = "";
        var paramFamFinUrgNote = new OdSqlParameter("paramFamFinUrgNote", OdDbType.Text, SOut.StringNote(patient.FamFinUrgNote));
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
            command += "DateTimeDeceased = " + SOut.DateT(patient.DateTimeDeceased) + "";
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
        var paramAddrNote = new OdSqlParameter("paramAddrNote", OdDbType.Text, SOut.StringNote(patient.AddrNote));
        if (patient.FamFinUrgNote == null) patient.FamFinUrgNote = "";
        var paramFamFinUrgNote = new OdSqlParameter("paramFamFinUrgNote", OdDbType.Text, SOut.StringNote(patient.FamFinUrgNote));
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

    public static void Delete(long patNum)
    {
        ClearFkey(patNum);
        var command = "DELETE FROM patient "
                      + "WHERE PatNum = " + SOut.Long(patNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPatNums)
    {
        if (listPatNums == null || listPatNums.Count == 0) return;
        ClearFkey(listPatNums);
        var command = "DELETE FROM patient "
                      + "WHERE PatNum IN(" + string.Join(",", listPatNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static void ClearFkey(long patNum)
    {
        if (patNum == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey=" + SOut.Long(patNum) + " AND PermType IN (75)";
        Db.NonQ(command);
    }

    public static void ClearFkey(List<long> listPatNums)
    {
        if (listPatNums == null || listPatNums.FindAll(x => x != 0).Count == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey IN(" + string.Join(",", listPatNums.FindAll(x => x != 0)) + ") AND PermType IN (75)";
        Db.NonQ(command);
    }
}