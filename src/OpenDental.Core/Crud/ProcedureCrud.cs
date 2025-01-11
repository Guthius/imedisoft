#region

using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ProcedureCrud
{
    public static Procedure SelectOne(long procNum)
    {
        var command = "SELECT * FROM procedurelog "
                      + "WHERE ProcNum = " + SOut.Long(procNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Procedure SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Procedure> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Procedure> TableToList(DataTable table)
    {
        var retVal = new List<Procedure>();
        Procedure procedure;
        foreach (DataRow row in table.Rows)
        {
            procedure = new Procedure();
            procedure.ProcNum = SIn.Long(row["ProcNum"].ToString());
            procedure.PatNum = SIn.Long(row["PatNum"].ToString());
            procedure.AptNum = SIn.Long(row["AptNum"].ToString());
            procedure.OldCode = SIn.String(row["OldCode"].ToString());
            procedure.ProcDate = SIn.Date(row["ProcDate"].ToString());
            procedure.ProcFee = SIn.Double(row["ProcFee"].ToString());
            procedure.Surf = SIn.String(row["Surf"].ToString());
            procedure.ToothNum = SIn.String(row["ToothNum"].ToString());
            procedure.ToothRange = SIn.String(row["ToothRange"].ToString());
            procedure.Priority = SIn.Long(row["Priority"].ToString());
            procedure.ProcStatus = (ProcStat) SIn.Int(row["ProcStatus"].ToString());
            procedure.ProvNum = SIn.Long(row["ProvNum"].ToString());
            procedure.Dx = SIn.Long(row["Dx"].ToString());
            procedure.PlannedAptNum = SIn.Long(row["PlannedAptNum"].ToString());
            procedure.PlaceService = (PlaceOfService) SIn.Int(row["PlaceService"].ToString());
            procedure.Prosthesis = SIn.String(row["Prosthesis"].ToString());
            procedure.DateOriginalProsth = SIn.Date(row["DateOriginalProsth"].ToString());
            procedure.ClaimNote = SIn.String(row["ClaimNote"].ToString());
            procedure.DateEntryC = SIn.Date(row["DateEntryC"].ToString());
            procedure.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            procedure.MedicalCode = SIn.String(row["MedicalCode"].ToString());
            procedure.DiagnosticCode = SIn.String(row["DiagnosticCode"].ToString());
            procedure.IsPrincDiag = SIn.Bool(row["IsPrincDiag"].ToString());
            procedure.ProcNumLab = SIn.Long(row["ProcNumLab"].ToString());
            procedure.BillingTypeOne = SIn.Long(row["BillingTypeOne"].ToString());
            procedure.BillingTypeTwo = SIn.Long(row["BillingTypeTwo"].ToString());
            procedure.CodeNum = SIn.Long(row["CodeNum"].ToString());
            procedure.CodeMod1 = SIn.String(row["CodeMod1"].ToString());
            procedure.CodeMod2 = SIn.String(row["CodeMod2"].ToString());
            procedure.CodeMod3 = SIn.String(row["CodeMod3"].ToString());
            procedure.CodeMod4 = SIn.String(row["CodeMod4"].ToString());
            procedure.RevCode = SIn.String(row["RevCode"].ToString());
            procedure.UnitQty = SIn.Int(row["UnitQty"].ToString());
            procedure.BaseUnits = SIn.Int(row["BaseUnits"].ToString());
            procedure.StartTime = SIn.Int(row["StartTime"].ToString());
            procedure.StopTime = SIn.Int(row["StopTime"].ToString());
            procedure.DateTP = SIn.Date(row["DateTP"].ToString());
            procedure.SiteNum = SIn.Long(row["SiteNum"].ToString());
            procedure.HideGraphics = SIn.Bool(row["HideGraphics"].ToString());
            procedure.CanadianTypeCodes = SIn.String(row["CanadianTypeCodes"].ToString());
            procedure.ProcTime = SIn.TimeSpan(row["ProcTime"].ToString());
            procedure.ProcTimeEnd = SIn.TimeSpan(row["ProcTimeEnd"].ToString());
            procedure.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            procedure.Prognosis = SIn.Long(row["Prognosis"].ToString());
            procedure.DrugUnit = (EnumProcDrugUnit) SIn.Int(row["DrugUnit"].ToString());
            procedure.DrugQty = SIn.Float(row["DrugQty"].ToString());
            procedure.UnitQtyType = (ProcUnitQtyType) SIn.Int(row["UnitQtyType"].ToString());
            procedure.StatementNum = SIn.Long(row["StatementNum"].ToString());
            procedure.IsLocked = SIn.Bool(row["IsLocked"].ToString());
            procedure.BillingNote = SIn.String(row["BillingNote"].ToString());
            procedure.RepeatChargeNum = SIn.Long(row["RepeatChargeNum"].ToString());
            procedure.DiagnosticCode2 = SIn.String(row["DiagnosticCode2"].ToString());
            procedure.DiagnosticCode3 = SIn.String(row["DiagnosticCode3"].ToString());
            procedure.DiagnosticCode4 = SIn.String(row["DiagnosticCode4"].ToString());
            procedure.Discount = SIn.Double(row["Discount"].ToString());
            procedure.SnomedBodySite = SIn.String(row["SnomedBodySite"].ToString());
            procedure.ProvOrderOverride = SIn.Long(row["ProvOrderOverride"].ToString());
            procedure.IsDateProsthEst = SIn.Bool(row["IsDateProsthEst"].ToString());
            procedure.IcdVersion = SIn.Byte(row["IcdVersion"].ToString());
            procedure.IsCpoe = SIn.Bool(row["IsCpoe"].ToString());
            procedure.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            procedure.SecDateEntry = SIn.DateTime(row["SecDateEntry"].ToString());
            procedure.DateComplete = SIn.Date(row["DateComplete"].ToString());
            procedure.OrderingReferralNum = SIn.Long(row["OrderingReferralNum"].ToString());
            procedure.TaxAmt = SIn.Double(row["TaxAmt"].ToString());
            procedure.Urgency = (ProcUrgency) SIn.Int(row["Urgency"].ToString());
            procedure.DiscountPlanAmt = SIn.Double(row["DiscountPlanAmt"].ToString());
            retVal.Add(procedure);
        }

        return retVal;
    }

    public static Procedure RowToObj(IDataRecord row)
    {
        return new Procedure
        {
            ProcNum = SIn.Long(row["ProcNum"].ToString()),
            PatNum = SIn.Long(row["PatNum"].ToString()),
            AptNum = SIn.Long(row["AptNum"].ToString()),
            OldCode = SIn.String(row["OldCode"].ToString()),
            ProcDate = SIn.Date(row["ProcDate"].ToString()),
            ProcFee = SIn.Double(row["ProcFee"].ToString()),
            Surf = SIn.String(row["Surf"].ToString()),
            ToothNum = SIn.String(row["ToothNum"].ToString()),
            ToothRange = SIn.String(row["ToothRange"].ToString()),
            Priority = SIn.Long(row["Priority"].ToString()),
            ProcStatus = (ProcStat) SIn.Int(row["ProcStatus"].ToString()),
            ProvNum = SIn.Long(row["ProvNum"].ToString()),
            Dx = SIn.Long(row["Dx"].ToString()),
            PlannedAptNum = SIn.Long(row["PlannedAptNum"].ToString()),
            PlaceService = (PlaceOfService) SIn.Int(row["PlaceService"].ToString()),
            Prosthesis = SIn.String(row["Prosthesis"].ToString()),
            DateOriginalProsth = SIn.Date(row["DateOriginalProsth"].ToString()),
            ClaimNote = SIn.String(row["ClaimNote"].ToString()),
            DateEntryC = SIn.Date(row["DateEntryC"].ToString()),
            ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
            MedicalCode = SIn.String(row["MedicalCode"].ToString()),
            DiagnosticCode = SIn.String(row["DiagnosticCode"].ToString()),
            IsPrincDiag = SIn.Bool(row["IsPrincDiag"].ToString()),
            ProcNumLab = SIn.Long(row["ProcNumLab"].ToString()),
            BillingTypeOne = SIn.Long(row["BillingTypeOne"].ToString()),
            BillingTypeTwo = SIn.Long(row["BillingTypeTwo"].ToString()),
            CodeNum = SIn.Long(row["CodeNum"].ToString()),
            CodeMod1 = SIn.String(row["CodeMod1"].ToString()),
            CodeMod2 = SIn.String(row["CodeMod2"].ToString()),
            CodeMod3 = SIn.String(row["CodeMod3"].ToString()),
            CodeMod4 = SIn.String(row["CodeMod4"].ToString()),
            RevCode = SIn.String(row["RevCode"].ToString()),
            UnitQty = SIn.Int(row["UnitQty"].ToString()),
            BaseUnits = SIn.Int(row["BaseUnits"].ToString()),
            StartTime = SIn.Int(row["StartTime"].ToString()),
            StopTime = SIn.Int(row["StopTime"].ToString()),
            DateTP = SIn.Date(row["DateTP"].ToString()),
            SiteNum = SIn.Long(row["SiteNum"].ToString()),
            HideGraphics = SIn.Bool(row["HideGraphics"].ToString()),
            CanadianTypeCodes = SIn.String(row["CanadianTypeCodes"].ToString()),
            ProcTime = SIn.TimeSpan(row["ProcTime"].ToString()),
            ProcTimeEnd = SIn.TimeSpan(row["ProcTimeEnd"].ToString()),
            DateTStamp = SIn.DateTime(row["DateTStamp"].ToString()),
            Prognosis = SIn.Long(row["Prognosis"].ToString()),
            DrugUnit = (EnumProcDrugUnit) SIn.Int(row["DrugUnit"].ToString()),
            DrugQty = SIn.Float(row["DrugQty"].ToString()),
            UnitQtyType = (ProcUnitQtyType) SIn.Int(row["UnitQtyType"].ToString()),
            StatementNum = SIn.Long(row["StatementNum"].ToString()),
            IsLocked = SIn.Bool(row["IsLocked"].ToString()),
            BillingNote = SIn.String(row["BillingNote"].ToString()),
            RepeatChargeNum = SIn.Long(row["RepeatChargeNum"].ToString()),
            DiagnosticCode2 = SIn.String(row["DiagnosticCode2"].ToString()),
            DiagnosticCode3 = SIn.String(row["DiagnosticCode3"].ToString()),
            DiagnosticCode4 = SIn.String(row["DiagnosticCode4"].ToString()),
            Discount = SIn.Double(row["Discount"].ToString()),
            SnomedBodySite = SIn.String(row["SnomedBodySite"].ToString()),
            ProvOrderOverride = SIn.Long(row["ProvOrderOverride"].ToString()),
            IsDateProsthEst = SIn.Bool(row["IsDateProsthEst"].ToString()),
            IcdVersion = SIn.Byte(row["IcdVersion"].ToString()),
            IsCpoe = SIn.Bool(row["IsCpoe"].ToString()),
            SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString()),
            SecDateEntry = SIn.DateTime(row["SecDateEntry"].ToString()),
            DateComplete = SIn.Date(row["DateComplete"].ToString()),
            OrderingReferralNum = SIn.Long(row["OrderingReferralNum"].ToString()),
            TaxAmt = SIn.Double(row["TaxAmt"].ToString()),
            Urgency = (ProcUrgency) SIn.Int(row["Urgency"].ToString()),
            DiscountPlanAmt = SIn.Double(row["DiscountPlanAmt"].ToString())
        };
    }

    public static DataTable ListToTable(List<Procedure> listProcedures, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Procedure";
        var table = new DataTable(tableName);
        table.Columns.Add("ProcNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("AptNum");
        table.Columns.Add("OldCode");
        table.Columns.Add("ProcDate");
        table.Columns.Add("ProcFee");
        table.Columns.Add("Surf");
        table.Columns.Add("ToothNum");
        table.Columns.Add("ToothRange");
        table.Columns.Add("Priority");
        table.Columns.Add("ProcStatus");
        table.Columns.Add("ProvNum");
        table.Columns.Add("Dx");
        table.Columns.Add("PlannedAptNum");
        table.Columns.Add("PlaceService");
        table.Columns.Add("Prosthesis");
        table.Columns.Add("DateOriginalProsth");
        table.Columns.Add("ClaimNote");
        table.Columns.Add("DateEntryC");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("MedicalCode");
        table.Columns.Add("DiagnosticCode");
        table.Columns.Add("IsPrincDiag");
        table.Columns.Add("ProcNumLab");
        table.Columns.Add("BillingTypeOne");
        table.Columns.Add("BillingTypeTwo");
        table.Columns.Add("CodeNum");
        table.Columns.Add("CodeMod1");
        table.Columns.Add("CodeMod2");
        table.Columns.Add("CodeMod3");
        table.Columns.Add("CodeMod4");
        table.Columns.Add("RevCode");
        table.Columns.Add("UnitQty");
        table.Columns.Add("BaseUnits");
        table.Columns.Add("StartTime");
        table.Columns.Add("StopTime");
        table.Columns.Add("DateTP");
        table.Columns.Add("SiteNum");
        table.Columns.Add("HideGraphics");
        table.Columns.Add("CanadianTypeCodes");
        table.Columns.Add("ProcTime");
        table.Columns.Add("ProcTimeEnd");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("Prognosis");
        table.Columns.Add("DrugUnit");
        table.Columns.Add("DrugQty");
        table.Columns.Add("UnitQtyType");
        table.Columns.Add("StatementNum");
        table.Columns.Add("IsLocked");
        table.Columns.Add("BillingNote");
        table.Columns.Add("RepeatChargeNum");
        table.Columns.Add("DiagnosticCode2");
        table.Columns.Add("DiagnosticCode3");
        table.Columns.Add("DiagnosticCode4");
        table.Columns.Add("Discount");
        table.Columns.Add("SnomedBodySite");
        table.Columns.Add("ProvOrderOverride");
        table.Columns.Add("IsDateProsthEst");
        table.Columns.Add("IcdVersion");
        table.Columns.Add("IsCpoe");
        table.Columns.Add("SecUserNumEntry");
        table.Columns.Add("SecDateEntry");
        table.Columns.Add("DateComplete");
        table.Columns.Add("OrderingReferralNum");
        table.Columns.Add("TaxAmt");
        table.Columns.Add("Urgency");
        table.Columns.Add("DiscountPlanAmt");
        foreach (var procedure in listProcedures)
            table.Rows.Add(SOut.Long(procedure.ProcNum), SOut.Long(procedure.PatNum), SOut.Long(procedure.AptNum), procedure.OldCode, SOut.DateT(procedure.ProcDate, false), SOut.Double(procedure.ProcFee), procedure.Surf, procedure.ToothNum, procedure.ToothRange, SOut.Long(procedure.Priority), SOut.Int((int) procedure.ProcStatus), SOut.Long(procedure.ProvNum), SOut.Long(procedure.Dx), SOut.Long(procedure.PlannedAptNum), SOut.Int((int) procedure.PlaceService), procedure.Prosthesis, SOut.DateT(procedure.DateOriginalProsth, false), procedure.ClaimNote, SOut.DateT(procedure.DateEntryC, false), SOut.Long(procedure.ClinicNum), procedure.MedicalCode, procedure.DiagnosticCode, SOut.Bool(procedure.IsPrincDiag), SOut.Long(procedure.ProcNumLab), SOut.Long(procedure.BillingTypeOne), SOut.Long(procedure.BillingTypeTwo), SOut.Long(procedure.CodeNum), procedure.CodeMod1, procedure.CodeMod2, procedure.CodeMod3, procedure.CodeMod4, procedure.RevCode, SOut.Int(procedure.UnitQty), SOut.Int(procedure.BaseUnits), SOut.Int(procedure.StartTime), SOut.Int(procedure.StopTime), SOut.DateT(procedure.DateTP, false), SOut.Long(procedure.SiteNum), SOut.Bool(procedure.HideGraphics), procedure.CanadianTypeCodes, SOut.Time(procedure.ProcTime, false), SOut.Time(procedure.ProcTimeEnd, false), SOut.DateT(procedure.DateTStamp, false), SOut.Long(procedure.Prognosis), SOut.Int((int) procedure.DrugUnit), SOut.Float(procedure.DrugQty), SOut.Int((int) procedure.UnitQtyType), SOut.Long(procedure.StatementNum), SOut.Bool(procedure.IsLocked), procedure.BillingNote, SOut.Long(procedure.RepeatChargeNum), procedure.DiagnosticCode2, procedure.DiagnosticCode3, procedure.DiagnosticCode4, SOut.Double(procedure.Discount), procedure.SnomedBodySite, SOut.Long(procedure.ProvOrderOverride), SOut.Bool(procedure.IsDateProsthEst), SOut.Byte(procedure.IcdVersion), SOut.Bool(procedure.IsCpoe), SOut.Long(procedure.SecUserNumEntry), SOut.DateT(procedure.SecDateEntry, false), SOut.DateT(procedure.DateComplete, false), SOut.Long(procedure.OrderingReferralNum), SOut.Double(procedure.TaxAmt), SOut.Int((int) procedure.Urgency), SOut.Double(procedure.DiscountPlanAmt));
        return table;
    }

    public static long Insert(Procedure procedure)
    {
        return Insert(procedure, false);
    }

    public static long Insert(Procedure procedure, bool useExistingPK)
    {
        var command = "INSERT INTO procedurelog (";

        command += "PatNum,AptNum,OldCode,ProcDate,ProcFee,Surf,ToothNum,ToothRange,Priority,ProcStatus,ProvNum,Dx,PlannedAptNum,PlaceService,Prosthesis,DateOriginalProsth,ClaimNote,DateEntryC,ClinicNum,MedicalCode,DiagnosticCode,IsPrincDiag,ProcNumLab,BillingTypeOne,BillingTypeTwo,CodeNum,CodeMod1,CodeMod2,CodeMod3,CodeMod4,RevCode,UnitQty,BaseUnits,StartTime,StopTime,DateTP,SiteNum,HideGraphics,CanadianTypeCodes,ProcTime,ProcTimeEnd,Prognosis,DrugUnit,DrugQty,UnitQtyType,StatementNum,IsLocked,BillingNote,RepeatChargeNum,DiagnosticCode2,DiagnosticCode3,DiagnosticCode4,Discount,SnomedBodySite,ProvOrderOverride,IsDateProsthEst,IcdVersion,IsCpoe,SecUserNumEntry,SecDateEntry,DateComplete,OrderingReferralNum,TaxAmt,Urgency,DiscountPlanAmt) VALUES(";

        command +=
            SOut.Long(procedure.PatNum) + ","
                                        + SOut.Long(procedure.AptNum) + ","
                                        + "'" + SOut.String(procedure.OldCode) + "',"
                                        + SOut.Date(procedure.ProcDate) + ","
                                        + SOut.Double(procedure.ProcFee) + ","
                                        + "'" + SOut.String(procedure.Surf) + "',"
                                        + "'" + SOut.String(procedure.ToothNum) + "',"
                                        + "'" + SOut.String(procedure.ToothRange) + "',"
                                        + SOut.Long(procedure.Priority) + ","
                                        + SOut.Int((int) procedure.ProcStatus) + ","
                                        + SOut.Long(procedure.ProvNum) + ","
                                        + SOut.Long(procedure.Dx) + ","
                                        + SOut.Long(procedure.PlannedAptNum) + ","
                                        + SOut.Int((int) procedure.PlaceService) + ","
                                        + "'" + SOut.String(procedure.Prosthesis) + "',"
                                        + SOut.Date(procedure.DateOriginalProsth) + ","
                                        + "'" + SOut.String(procedure.ClaimNote) + "',"
                                        + DbHelper.Now() + ","
                                        + SOut.Long(procedure.ClinicNum) + ","
                                        + "'" + SOut.String(procedure.MedicalCode) + "',"
                                        + "'" + SOut.String(procedure.DiagnosticCode) + "',"
                                        + SOut.Bool(procedure.IsPrincDiag) + ","
                                        + SOut.Long(procedure.ProcNumLab) + ","
                                        + SOut.Long(procedure.BillingTypeOne) + ","
                                        + SOut.Long(procedure.BillingTypeTwo) + ","
                                        + SOut.Long(procedure.CodeNum) + ","
                                        + "'" + SOut.String(procedure.CodeMod1) + "',"
                                        + "'" + SOut.String(procedure.CodeMod2) + "',"
                                        + "'" + SOut.String(procedure.CodeMod3) + "',"
                                        + "'" + SOut.String(procedure.CodeMod4) + "',"
                                        + "'" + SOut.String(procedure.RevCode) + "',"
                                        + SOut.Int(procedure.UnitQty) + ","
                                        + SOut.Int(procedure.BaseUnits) + ","
                                        + SOut.Int(procedure.StartTime) + ","
                                        + SOut.Int(procedure.StopTime) + ","
                                        + SOut.Date(procedure.DateTP) + ","
                                        + SOut.Long(procedure.SiteNum) + ","
                                        + SOut.Bool(procedure.HideGraphics) + ","
                                        + "'" + SOut.String(procedure.CanadianTypeCodes) + "',"
                                        + SOut.Time(procedure.ProcTime) + ","
                                        + SOut.Time(procedure.ProcTimeEnd) + ","
                                        //DateTStamp can only be set by MySQL
                                        + SOut.Long(procedure.Prognosis) + ","
                                        + SOut.Int((int) procedure.DrugUnit) + ","
                                        + SOut.Float(procedure.DrugQty) + ","
                                        + SOut.Int((int) procedure.UnitQtyType) + ","
                                        + SOut.Long(procedure.StatementNum) + ","
                                        + SOut.Bool(procedure.IsLocked) + ","
                                        + "'" + SOut.String(procedure.BillingNote) + "',"
                                        + SOut.Long(procedure.RepeatChargeNum) + ","
                                        + "'" + SOut.String(procedure.DiagnosticCode2) + "',"
                                        + "'" + SOut.String(procedure.DiagnosticCode3) + "',"
                                        + "'" + SOut.String(procedure.DiagnosticCode4) + "',"
                                        + SOut.Double(procedure.Discount) + ","
                                        + "'" + SOut.String(procedure.SnomedBodySite) + "',"
                                        + SOut.Long(procedure.ProvOrderOverride) + ","
                                        + SOut.Bool(procedure.IsDateProsthEst) + ","
                                        + SOut.Byte(procedure.IcdVersion) + ","
                                        + SOut.Bool(procedure.IsCpoe) + ","
                                        + SOut.Long(procedure.SecUserNumEntry) + ","
                                        + DbHelper.Now() + ","
                                        + SOut.Date(procedure.DateComplete) + ","
                                        + SOut.Long(procedure.OrderingReferralNum) + ","
                                        + SOut.Double(procedure.TaxAmt) + ","
                                        + SOut.Int((int) procedure.Urgency) + ","
                                        + SOut.Double(procedure.DiscountPlanAmt) + ")";
        {
            procedure.ProcNum = Db.NonQ(command, true, "ProcNum", "procedure");
        }
        return procedure.ProcNum;
    }

    public static void InsertMany(List<Procedure> listProcedures)
    {
        InsertMany(listProcedures, false);
    }

    public static void InsertMany(List<Procedure> listProcedures, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listProcedures.Count)
        {
            var procedure = listProcedures[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO procedurelog (");
                if (useExistingPK) sbCommands.Append("ProcNum,");
                sbCommands.Append("PatNum,AptNum,OldCode,ProcDate,ProcFee,Surf,ToothNum,ToothRange,Priority,ProcStatus,ProvNum,Dx,PlannedAptNum,PlaceService,Prosthesis,DateOriginalProsth,ClaimNote,DateEntryC,ClinicNum,MedicalCode,DiagnosticCode,IsPrincDiag,ProcNumLab,BillingTypeOne,BillingTypeTwo,CodeNum,CodeMod1,CodeMod2,CodeMod3,CodeMod4,RevCode,UnitQty,BaseUnits,StartTime,StopTime,DateTP,SiteNum,HideGraphics,CanadianTypeCodes,ProcTime,ProcTimeEnd,Prognosis,DrugUnit,DrugQty,UnitQtyType,StatementNum,IsLocked,BillingNote,RepeatChargeNum,DiagnosticCode2,DiagnosticCode3,DiagnosticCode4,Discount,SnomedBodySite,ProvOrderOverride,IsDateProsthEst,IcdVersion,IsCpoe,SecUserNumEntry,SecDateEntry,DateComplete,OrderingReferralNum,TaxAmt,Urgency,DiscountPlanAmt) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(procedure.ProcNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(procedure.PatNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procedure.AptNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.OldCode) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Date(procedure.ProcDate));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(procedure.ProcFee));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.Surf) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.ToothNum) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.ToothRange) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procedure.Priority));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) procedure.ProcStatus));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procedure.ProvNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procedure.Dx));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procedure.PlannedAptNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) procedure.PlaceService));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.Prosthesis) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Date(procedure.DateOriginalProsth));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.ClaimNote) + "'");
            sbRow.Append(",");
            sbRow.Append(DbHelper.Now());
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procedure.ClinicNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.MedicalCode) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.DiagnosticCode) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(procedure.IsPrincDiag));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procedure.ProcNumLab));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procedure.BillingTypeOne));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procedure.BillingTypeTwo));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procedure.CodeNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.CodeMod1) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.CodeMod2) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.CodeMod3) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.CodeMod4) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.RevCode) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int(procedure.UnitQty));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(procedure.BaseUnits));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(procedure.StartTime));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(procedure.StopTime));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(procedure.DateTP));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procedure.SiteNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(procedure.HideGraphics));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.CanadianTypeCodes) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Time(procedure.ProcTime));
            sbRow.Append(",");
            sbRow.Append(SOut.Time(procedure.ProcTimeEnd));
            sbRow.Append(",");
            //DateTStamp can only be set by MySQL
            sbRow.Append(SOut.Long(procedure.Prognosis));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) procedure.DrugUnit));
            sbRow.Append(",");
            sbRow.Append(SOut.Float(procedure.DrugQty));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) procedure.UnitQtyType));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procedure.StatementNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(procedure.IsLocked));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.BillingNote) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procedure.RepeatChargeNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.DiagnosticCode2) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.DiagnosticCode3) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.DiagnosticCode4) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Double(procedure.Discount));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedure.SnomedBodySite) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procedure.ProvOrderOverride));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(procedure.IsDateProsthEst));
            sbRow.Append(",");
            sbRow.Append(SOut.Byte(procedure.IcdVersion));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(procedure.IsCpoe));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procedure.SecUserNumEntry));
            sbRow.Append(",");
            sbRow.Append(DbHelper.Now());
            sbRow.Append(",");
            sbRow.Append(SOut.Date(procedure.DateComplete));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procedure.OrderingReferralNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(procedure.TaxAmt));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) procedure.Urgency));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(procedure.DiscountPlanAmt));
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
                if (index == listProcedures.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(Procedure procedure)
    {
        return InsertNoCache(procedure, false);
    }

    public static long InsertNoCache(Procedure procedure, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO procedurelog (";
        if (isRandomKeys || useExistingPK) command += "ProcNum,";
        command += "PatNum,AptNum,OldCode,ProcDate,ProcFee,Surf,ToothNum,ToothRange,Priority,ProcStatus,ProvNum,Dx,PlannedAptNum,PlaceService,Prosthesis,DateOriginalProsth,ClaimNote,DateEntryC,ClinicNum,MedicalCode,DiagnosticCode,IsPrincDiag,ProcNumLab,BillingTypeOne,BillingTypeTwo,CodeNum,CodeMod1,CodeMod2,CodeMod3,CodeMod4,RevCode,UnitQty,BaseUnits,StartTime,StopTime,DateTP,SiteNum,HideGraphics,CanadianTypeCodes,ProcTime,ProcTimeEnd,Prognosis,DrugUnit,DrugQty,UnitQtyType,StatementNum,IsLocked,BillingNote,RepeatChargeNum,DiagnosticCode2,DiagnosticCode3,DiagnosticCode4,Discount,SnomedBodySite,ProvOrderOverride,IsDateProsthEst,IcdVersion,IsCpoe,SecUserNumEntry,SecDateEntry,DateComplete,OrderingReferralNum,TaxAmt,Urgency,DiscountPlanAmt) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(procedure.ProcNum) + ",";
        command +=
            SOut.Long(procedure.PatNum) + ","
                                        + SOut.Long(procedure.AptNum) + ","
                                        + "'" + SOut.String(procedure.OldCode) + "',"
                                        + SOut.Date(procedure.ProcDate) + ","
                                        + SOut.Double(procedure.ProcFee) + ","
                                        + "'" + SOut.String(procedure.Surf) + "',"
                                        + "'" + SOut.String(procedure.ToothNum) + "',"
                                        + "'" + SOut.String(procedure.ToothRange) + "',"
                                        + SOut.Long(procedure.Priority) + ","
                                        + SOut.Int((int) procedure.ProcStatus) + ","
                                        + SOut.Long(procedure.ProvNum) + ","
                                        + SOut.Long(procedure.Dx) + ","
                                        + SOut.Long(procedure.PlannedAptNum) + ","
                                        + SOut.Int((int) procedure.PlaceService) + ","
                                        + "'" + SOut.String(procedure.Prosthesis) + "',"
                                        + SOut.Date(procedure.DateOriginalProsth) + ","
                                        + "'" + SOut.String(procedure.ClaimNote) + "',"
                                        + DbHelper.Now() + ","
                                        + SOut.Long(procedure.ClinicNum) + ","
                                        + "'" + SOut.String(procedure.MedicalCode) + "',"
                                        + "'" + SOut.String(procedure.DiagnosticCode) + "',"
                                        + SOut.Bool(procedure.IsPrincDiag) + ","
                                        + SOut.Long(procedure.ProcNumLab) + ","
                                        + SOut.Long(procedure.BillingTypeOne) + ","
                                        + SOut.Long(procedure.BillingTypeTwo) + ","
                                        + SOut.Long(procedure.CodeNum) + ","
                                        + "'" + SOut.String(procedure.CodeMod1) + "',"
                                        + "'" + SOut.String(procedure.CodeMod2) + "',"
                                        + "'" + SOut.String(procedure.CodeMod3) + "',"
                                        + "'" + SOut.String(procedure.CodeMod4) + "',"
                                        + "'" + SOut.String(procedure.RevCode) + "',"
                                        + SOut.Int(procedure.UnitQty) + ","
                                        + SOut.Int(procedure.BaseUnits) + ","
                                        + SOut.Int(procedure.StartTime) + ","
                                        + SOut.Int(procedure.StopTime) + ","
                                        + SOut.Date(procedure.DateTP) + ","
                                        + SOut.Long(procedure.SiteNum) + ","
                                        + SOut.Bool(procedure.HideGraphics) + ","
                                        + "'" + SOut.String(procedure.CanadianTypeCodes) + "',"
                                        + SOut.Time(procedure.ProcTime) + ","
                                        + SOut.Time(procedure.ProcTimeEnd) + ","
                                        //DateTStamp can only be set by MySQL
                                        + SOut.Long(procedure.Prognosis) + ","
                                        + SOut.Int((int) procedure.DrugUnit) + ","
                                        + SOut.Float(procedure.DrugQty) + ","
                                        + SOut.Int((int) procedure.UnitQtyType) + ","
                                        + SOut.Long(procedure.StatementNum) + ","
                                        + SOut.Bool(procedure.IsLocked) + ","
                                        + "'" + SOut.String(procedure.BillingNote) + "',"
                                        + SOut.Long(procedure.RepeatChargeNum) + ","
                                        + "'" + SOut.String(procedure.DiagnosticCode2) + "',"
                                        + "'" + SOut.String(procedure.DiagnosticCode3) + "',"
                                        + "'" + SOut.String(procedure.DiagnosticCode4) + "',"
                                        + SOut.Double(procedure.Discount) + ","
                                        + "'" + SOut.String(procedure.SnomedBodySite) + "',"
                                        + SOut.Long(procedure.ProvOrderOverride) + ","
                                        + SOut.Bool(procedure.IsDateProsthEst) + ","
                                        + SOut.Byte(procedure.IcdVersion) + ","
                                        + SOut.Bool(procedure.IsCpoe) + ","
                                        + SOut.Long(procedure.SecUserNumEntry) + ","
                                        + DbHelper.Now() + ","
                                        + SOut.Date(procedure.DateComplete) + ","
                                        + SOut.Long(procedure.OrderingReferralNum) + ","
                                        + SOut.Double(procedure.TaxAmt) + ","
                                        + SOut.Int((int) procedure.Urgency) + ","
                                        + SOut.Double(procedure.DiscountPlanAmt) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            procedure.ProcNum = Db.NonQ(command, true, "ProcNum", "procedure");
        return procedure.ProcNum;
    }

    public static void Update(Procedure procedure)
    {
        var command = "UPDATE procedurelog SET "
                      + "PatNum             =  " + SOut.Long(procedure.PatNum) + ", "
                      + "AptNum             =  " + SOut.Long(procedure.AptNum) + ", "
                      + "OldCode            = '" + SOut.String(procedure.OldCode) + "', "
                      + "ProcDate           =  " + SOut.Date(procedure.ProcDate) + ", "
                      + "ProcFee            =  " + SOut.Double(procedure.ProcFee) + ", "
                      + "Surf               = '" + SOut.String(procedure.Surf) + "', "
                      + "ToothNum           = '" + SOut.String(procedure.ToothNum) + "', "
                      + "ToothRange         = '" + SOut.String(procedure.ToothRange) + "', "
                      + "Priority           =  " + SOut.Long(procedure.Priority) + ", "
                      + "ProcStatus         =  " + SOut.Int((int) procedure.ProcStatus) + ", "
                      + "ProvNum            =  " + SOut.Long(procedure.ProvNum) + ", "
                      + "Dx                 =  " + SOut.Long(procedure.Dx) + ", "
                      + "PlannedAptNum      =  " + SOut.Long(procedure.PlannedAptNum) + ", "
                      + "PlaceService       =  " + SOut.Int((int) procedure.PlaceService) + ", "
                      + "Prosthesis         = '" + SOut.String(procedure.Prosthesis) + "', "
                      + "DateOriginalProsth =  " + SOut.Date(procedure.DateOriginalProsth) + ", "
                      + "ClaimNote          = '" + SOut.String(procedure.ClaimNote) + "', "
                      + "DateEntryC         =  " + SOut.Date(procedure.DateEntryC) + ", "
                      + "ClinicNum          =  " + SOut.Long(procedure.ClinicNum) + ", "
                      + "MedicalCode        = '" + SOut.String(procedure.MedicalCode) + "', "
                      + "DiagnosticCode     = '" + SOut.String(procedure.DiagnosticCode) + "', "
                      + "IsPrincDiag        =  " + SOut.Bool(procedure.IsPrincDiag) + ", "
                      + "ProcNumLab         =  " + SOut.Long(procedure.ProcNumLab) + ", "
                      + "BillingTypeOne     =  " + SOut.Long(procedure.BillingTypeOne) + ", "
                      + "BillingTypeTwo     =  " + SOut.Long(procedure.BillingTypeTwo) + ", "
                      + "CodeNum            =  " + SOut.Long(procedure.CodeNum) + ", "
                      + "CodeMod1           = '" + SOut.String(procedure.CodeMod1) + "', "
                      + "CodeMod2           = '" + SOut.String(procedure.CodeMod2) + "', "
                      + "CodeMod3           = '" + SOut.String(procedure.CodeMod3) + "', "
                      + "CodeMod4           = '" + SOut.String(procedure.CodeMod4) + "', "
                      + "RevCode            = '" + SOut.String(procedure.RevCode) + "', "
                      + "UnitQty            =  " + SOut.Int(procedure.UnitQty) + ", "
                      + "BaseUnits          =  " + SOut.Int(procedure.BaseUnits) + ", "
                      + "StartTime          =  " + SOut.Int(procedure.StartTime) + ", "
                      + "StopTime           =  " + SOut.Int(procedure.StopTime) + ", "
                      + "DateTP             =  " + SOut.Date(procedure.DateTP) + ", "
                      + "SiteNum            =  " + SOut.Long(procedure.SiteNum) + ", "
                      + "HideGraphics       =  " + SOut.Bool(procedure.HideGraphics) + ", "
                      + "CanadianTypeCodes  = '" + SOut.String(procedure.CanadianTypeCodes) + "', "
                      + "ProcTime           =  " + SOut.Time(procedure.ProcTime) + ", "
                      + "ProcTimeEnd        =  " + SOut.Time(procedure.ProcTimeEnd) + ", "
                      //DateTStamp can only be set by MySQL
                      + "Prognosis          =  " + SOut.Long(procedure.Prognosis) + ", "
                      + "DrugUnit           =  " + SOut.Int((int) procedure.DrugUnit) + ", "
                      + "DrugQty            =  " + SOut.Float(procedure.DrugQty) + ", "
                      + "UnitQtyType        =  " + SOut.Int((int) procedure.UnitQtyType) + ", "
                      + "StatementNum       =  " + SOut.Long(procedure.StatementNum) + ", "
                      + "IsLocked           =  " + SOut.Bool(procedure.IsLocked) + ", "
                      + "BillingNote        = '" + SOut.String(procedure.BillingNote) + "', "
                      + "RepeatChargeNum    =  " + SOut.Long(procedure.RepeatChargeNum) + ", "
                      + "DiagnosticCode2    = '" + SOut.String(procedure.DiagnosticCode2) + "', "
                      + "DiagnosticCode3    = '" + SOut.String(procedure.DiagnosticCode3) + "', "
                      + "DiagnosticCode4    = '" + SOut.String(procedure.DiagnosticCode4) + "', "
                      + "Discount           =  " + SOut.Double(procedure.Discount) + ", "
                      + "SnomedBodySite     = '" + SOut.String(procedure.SnomedBodySite) + "', "
                      + "ProvOrderOverride  =  " + SOut.Long(procedure.ProvOrderOverride) + ", "
                      + "IsDateProsthEst    =  " + SOut.Bool(procedure.IsDateProsthEst) + ", "
                      + "IcdVersion         =  " + SOut.Byte(procedure.IcdVersion) + ", "
                      + "IsCpoe             =  " + SOut.Bool(procedure.IsCpoe) + ", "
                      //SecUserNumEntry excluded from update
                      //SecDateEntry not allowed to change
                      + "DateComplete       =  " + SOut.Date(procedure.DateComplete) + ", "
                      + "OrderingReferralNum=  " + SOut.Long(procedure.OrderingReferralNum) + ", "
                      + "TaxAmt             =  " + SOut.Double(procedure.TaxAmt) + ", "
                      + "Urgency            =  " + SOut.Int((int) procedure.Urgency) + ", "
                      + "DiscountPlanAmt    =  " + SOut.Double(procedure.DiscountPlanAmt) + " "
                      + "WHERE ProcNum = " + SOut.Long(procedure.ProcNum);
        Db.NonQ(command);
    }

    public static bool Update(Procedure procedure, Procedure oldProcedure)
    {
        var command = "";
        if (procedure.PatNum != oldProcedure.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(procedure.PatNum) + "";
        }

        if (procedure.AptNum != oldProcedure.AptNum)
        {
            if (command != "") command += ",";
            command += "AptNum = " + SOut.Long(procedure.AptNum) + "";
        }

        if (procedure.OldCode != oldProcedure.OldCode)
        {
            if (command != "") command += ",";
            command += "OldCode = '" + SOut.String(procedure.OldCode) + "'";
        }

        if (procedure.ProcDate.Date != oldProcedure.ProcDate.Date)
        {
            if (command != "") command += ",";
            command += "ProcDate = " + SOut.Date(procedure.ProcDate) + "";
        }

        if (procedure.ProcFee != oldProcedure.ProcFee)
        {
            if (command != "") command += ",";
            command += "ProcFee = " + SOut.Double(procedure.ProcFee) + "";
        }

        if (procedure.Surf != oldProcedure.Surf)
        {
            if (command != "") command += ",";
            command += "Surf = '" + SOut.String(procedure.Surf) + "'";
        }

        if (procedure.ToothNum != oldProcedure.ToothNum)
        {
            if (command != "") command += ",";
            command += "ToothNum = '" + SOut.String(procedure.ToothNum) + "'";
        }

        if (procedure.ToothRange != oldProcedure.ToothRange)
        {
            if (command != "") command += ",";
            command += "ToothRange = '" + SOut.String(procedure.ToothRange) + "'";
        }

        if (procedure.Priority != oldProcedure.Priority)
        {
            if (command != "") command += ",";
            command += "Priority = " + SOut.Long(procedure.Priority) + "";
        }

        if (procedure.ProcStatus != oldProcedure.ProcStatus)
        {
            if (command != "") command += ",";
            command += "ProcStatus = " + SOut.Int((int) procedure.ProcStatus) + "";
        }

        if (procedure.ProvNum != oldProcedure.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(procedure.ProvNum) + "";
        }

        if (procedure.Dx != oldProcedure.Dx)
        {
            if (command != "") command += ",";
            command += "Dx = " + SOut.Long(procedure.Dx) + "";
        }

        if (procedure.PlannedAptNum != oldProcedure.PlannedAptNum)
        {
            if (command != "") command += ",";
            command += "PlannedAptNum = " + SOut.Long(procedure.PlannedAptNum) + "";
        }

        if (procedure.PlaceService != oldProcedure.PlaceService)
        {
            if (command != "") command += ",";
            command += "PlaceService = " + SOut.Int((int) procedure.PlaceService) + "";
        }

        if (procedure.Prosthesis != oldProcedure.Prosthesis)
        {
            if (command != "") command += ",";
            command += "Prosthesis = '" + SOut.String(procedure.Prosthesis) + "'";
        }

        if (procedure.DateOriginalProsth.Date != oldProcedure.DateOriginalProsth.Date)
        {
            if (command != "") command += ",";
            command += "DateOriginalProsth = " + SOut.Date(procedure.DateOriginalProsth) + "";
        }

        if (procedure.ClaimNote != oldProcedure.ClaimNote)
        {
            if (command != "") command += ",";
            command += "ClaimNote = '" + SOut.String(procedure.ClaimNote) + "'";
        }

        if (procedure.DateEntryC.Date != oldProcedure.DateEntryC.Date)
        {
            if (command != "") command += ",";
            command += "DateEntryC = " + SOut.Date(procedure.DateEntryC) + "";
        }

        if (procedure.ClinicNum != oldProcedure.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(procedure.ClinicNum) + "";
        }

        if (procedure.MedicalCode != oldProcedure.MedicalCode)
        {
            if (command != "") command += ",";
            command += "MedicalCode = '" + SOut.String(procedure.MedicalCode) + "'";
        }

        if (procedure.DiagnosticCode != oldProcedure.DiagnosticCode)
        {
            if (command != "") command += ",";
            command += "DiagnosticCode = '" + SOut.String(procedure.DiagnosticCode) + "'";
        }

        if (procedure.IsPrincDiag != oldProcedure.IsPrincDiag)
        {
            if (command != "") command += ",";
            command += "IsPrincDiag = " + SOut.Bool(procedure.IsPrincDiag) + "";
        }

        if (procedure.ProcNumLab != oldProcedure.ProcNumLab)
        {
            if (command != "") command += ",";
            command += "ProcNumLab = " + SOut.Long(procedure.ProcNumLab) + "";
        }

        if (procedure.BillingTypeOne != oldProcedure.BillingTypeOne)
        {
            if (command != "") command += ",";
            command += "BillingTypeOne = " + SOut.Long(procedure.BillingTypeOne) + "";
        }

        if (procedure.BillingTypeTwo != oldProcedure.BillingTypeTwo)
        {
            if (command != "") command += ",";
            command += "BillingTypeTwo = " + SOut.Long(procedure.BillingTypeTwo) + "";
        }

        if (procedure.CodeNum != oldProcedure.CodeNum)
        {
            if (command != "") command += ",";
            command += "CodeNum = " + SOut.Long(procedure.CodeNum) + "";
        }

        if (procedure.CodeMod1 != oldProcedure.CodeMod1)
        {
            if (command != "") command += ",";
            command += "CodeMod1 = '" + SOut.String(procedure.CodeMod1) + "'";
        }

        if (procedure.CodeMod2 != oldProcedure.CodeMod2)
        {
            if (command != "") command += ",";
            command += "CodeMod2 = '" + SOut.String(procedure.CodeMod2) + "'";
        }

        if (procedure.CodeMod3 != oldProcedure.CodeMod3)
        {
            if (command != "") command += ",";
            command += "CodeMod3 = '" + SOut.String(procedure.CodeMod3) + "'";
        }

        if (procedure.CodeMod4 != oldProcedure.CodeMod4)
        {
            if (command != "") command += ",";
            command += "CodeMod4 = '" + SOut.String(procedure.CodeMod4) + "'";
        }

        if (procedure.RevCode != oldProcedure.RevCode)
        {
            if (command != "") command += ",";
            command += "RevCode = '" + SOut.String(procedure.RevCode) + "'";
        }

        if (procedure.UnitQty != oldProcedure.UnitQty)
        {
            if (command != "") command += ",";
            command += "UnitQty = " + SOut.Int(procedure.UnitQty) + "";
        }

        if (procedure.BaseUnits != oldProcedure.BaseUnits)
        {
            if (command != "") command += ",";
            command += "BaseUnits = " + SOut.Int(procedure.BaseUnits) + "";
        }

        if (procedure.StartTime != oldProcedure.StartTime)
        {
            if (command != "") command += ",";
            command += "StartTime = " + SOut.Int(procedure.StartTime) + "";
        }

        if (procedure.StopTime != oldProcedure.StopTime)
        {
            if (command != "") command += ",";
            command += "StopTime = " + SOut.Int(procedure.StopTime) + "";
        }

        if (procedure.DateTP.Date != oldProcedure.DateTP.Date)
        {
            if (command != "") command += ",";
            command += "DateTP = " + SOut.Date(procedure.DateTP) + "";
        }

        if (procedure.SiteNum != oldProcedure.SiteNum)
        {
            if (command != "") command += ",";
            command += "SiteNum = " + SOut.Long(procedure.SiteNum) + "";
        }

        if (procedure.HideGraphics != oldProcedure.HideGraphics)
        {
            if (command != "") command += ",";
            command += "HideGraphics = " + SOut.Bool(procedure.HideGraphics) + "";
        }

        if (procedure.CanadianTypeCodes != oldProcedure.CanadianTypeCodes)
        {
            if (command != "") command += ",";
            command += "CanadianTypeCodes = '" + SOut.String(procedure.CanadianTypeCodes) + "'";
        }

        if (procedure.ProcTime != oldProcedure.ProcTime)
        {
            if (command != "") command += ",";
            command += "ProcTime = " + SOut.Time(procedure.ProcTime) + "";
        }

        if (procedure.ProcTimeEnd != oldProcedure.ProcTimeEnd)
        {
            if (command != "") command += ",";
            command += "ProcTimeEnd = " + SOut.Time(procedure.ProcTimeEnd) + "";
        }

        //DateTStamp can only be set by MySQL
        if (procedure.Prognosis != oldProcedure.Prognosis)
        {
            if (command != "") command += ",";
            command += "Prognosis = " + SOut.Long(procedure.Prognosis) + "";
        }

        if (procedure.DrugUnit != oldProcedure.DrugUnit)
        {
            if (command != "") command += ",";
            command += "DrugUnit = " + SOut.Int((int) procedure.DrugUnit) + "";
        }

        if (procedure.DrugQty != oldProcedure.DrugQty)
        {
            if (command != "") command += ",";
            command += "DrugQty = " + SOut.Float(procedure.DrugQty) + "";
        }

        if (procedure.UnitQtyType != oldProcedure.UnitQtyType)
        {
            if (command != "") command += ",";
            command += "UnitQtyType = " + SOut.Int((int) procedure.UnitQtyType) + "";
        }

        if (procedure.StatementNum != oldProcedure.StatementNum)
        {
            if (command != "") command += ",";
            command += "StatementNum = " + SOut.Long(procedure.StatementNum) + "";
        }

        if (procedure.IsLocked != oldProcedure.IsLocked)
        {
            if (command != "") command += ",";
            command += "IsLocked = " + SOut.Bool(procedure.IsLocked) + "";
        }

        if (procedure.BillingNote != oldProcedure.BillingNote)
        {
            if (command != "") command += ",";
            command += "BillingNote = '" + SOut.String(procedure.BillingNote) + "'";
        }

        if (procedure.RepeatChargeNum != oldProcedure.RepeatChargeNum)
        {
            if (command != "") command += ",";
            command += "RepeatChargeNum = " + SOut.Long(procedure.RepeatChargeNum) + "";
        }

        if (procedure.DiagnosticCode2 != oldProcedure.DiagnosticCode2)
        {
            if (command != "") command += ",";
            command += "DiagnosticCode2 = '" + SOut.String(procedure.DiagnosticCode2) + "'";
        }

        if (procedure.DiagnosticCode3 != oldProcedure.DiagnosticCode3)
        {
            if (command != "") command += ",";
            command += "DiagnosticCode3 = '" + SOut.String(procedure.DiagnosticCode3) + "'";
        }

        if (procedure.DiagnosticCode4 != oldProcedure.DiagnosticCode4)
        {
            if (command != "") command += ",";
            command += "DiagnosticCode4 = '" + SOut.String(procedure.DiagnosticCode4) + "'";
        }

        if (procedure.Discount != oldProcedure.Discount)
        {
            if (command != "") command += ",";
            command += "Discount = " + SOut.Double(procedure.Discount) + "";
        }

        if (procedure.SnomedBodySite != oldProcedure.SnomedBodySite)
        {
            if (command != "") command += ",";
            command += "SnomedBodySite = '" + SOut.String(procedure.SnomedBodySite) + "'";
        }

        if (procedure.ProvOrderOverride != oldProcedure.ProvOrderOverride)
        {
            if (command != "") command += ",";
            command += "ProvOrderOverride = " + SOut.Long(procedure.ProvOrderOverride) + "";
        }

        if (procedure.IsDateProsthEst != oldProcedure.IsDateProsthEst)
        {
            if (command != "") command += ",";
            command += "IsDateProsthEst = " + SOut.Bool(procedure.IsDateProsthEst) + "";
        }

        if (procedure.IcdVersion != oldProcedure.IcdVersion)
        {
            if (command != "") command += ",";
            command += "IcdVersion = " + SOut.Byte(procedure.IcdVersion) + "";
        }

        if (procedure.IsCpoe != oldProcedure.IsCpoe)
        {
            if (command != "") command += ",";
            command += "IsCpoe = " + SOut.Bool(procedure.IsCpoe) + "";
        }

        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        if (procedure.DateComplete.Date != oldProcedure.DateComplete.Date)
        {
            if (command != "") command += ",";
            command += "DateComplete = " + SOut.Date(procedure.DateComplete) + "";
        }

        if (procedure.OrderingReferralNum != oldProcedure.OrderingReferralNum)
        {
            if (command != "") command += ",";
            command += "OrderingReferralNum = " + SOut.Long(procedure.OrderingReferralNum) + "";
        }

        if (procedure.TaxAmt != oldProcedure.TaxAmt)
        {
            if (command != "") command += ",";
            command += "TaxAmt = " + SOut.Double(procedure.TaxAmt) + "";
        }

        if (procedure.Urgency != oldProcedure.Urgency)
        {
            if (command != "") command += ",";
            command += "Urgency = " + SOut.Int((int) procedure.Urgency) + "";
        }

        if (procedure.DiscountPlanAmt != oldProcedure.DiscountPlanAmt)
        {
            if (command != "") command += ",";
            command += "DiscountPlanAmt = " + SOut.Double(procedure.DiscountPlanAmt) + "";
        }

        if (command == "") return false;
        command = "UPDATE procedurelog SET " + command
                                             + " WHERE ProcNum = " + SOut.Long(procedure.ProcNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Procedure procedure, Procedure oldProcedure)
    {
        if (procedure.PatNum != oldProcedure.PatNum) return true;
        if (procedure.AptNum != oldProcedure.AptNum) return true;
        if (procedure.OldCode != oldProcedure.OldCode) return true;
        if (procedure.ProcDate.Date != oldProcedure.ProcDate.Date) return true;
        if (procedure.ProcFee != oldProcedure.ProcFee) return true;
        if (procedure.Surf != oldProcedure.Surf) return true;
        if (procedure.ToothNum != oldProcedure.ToothNum) return true;
        if (procedure.ToothRange != oldProcedure.ToothRange) return true;
        if (procedure.Priority != oldProcedure.Priority) return true;
        if (procedure.ProcStatus != oldProcedure.ProcStatus) return true;
        if (procedure.ProvNum != oldProcedure.ProvNum) return true;
        if (procedure.Dx != oldProcedure.Dx) return true;
        if (procedure.PlannedAptNum != oldProcedure.PlannedAptNum) return true;
        if (procedure.PlaceService != oldProcedure.PlaceService) return true;
        if (procedure.Prosthesis != oldProcedure.Prosthesis) return true;
        if (procedure.DateOriginalProsth.Date != oldProcedure.DateOriginalProsth.Date) return true;
        if (procedure.ClaimNote != oldProcedure.ClaimNote) return true;
        if (procedure.DateEntryC.Date != oldProcedure.DateEntryC.Date) return true;
        if (procedure.ClinicNum != oldProcedure.ClinicNum) return true;
        if (procedure.MedicalCode != oldProcedure.MedicalCode) return true;
        if (procedure.DiagnosticCode != oldProcedure.DiagnosticCode) return true;
        if (procedure.IsPrincDiag != oldProcedure.IsPrincDiag) return true;
        if (procedure.ProcNumLab != oldProcedure.ProcNumLab) return true;
        if (procedure.BillingTypeOne != oldProcedure.BillingTypeOne) return true;
        if (procedure.BillingTypeTwo != oldProcedure.BillingTypeTwo) return true;
        if (procedure.CodeNum != oldProcedure.CodeNum) return true;
        if (procedure.CodeMod1 != oldProcedure.CodeMod1) return true;
        if (procedure.CodeMod2 != oldProcedure.CodeMod2) return true;
        if (procedure.CodeMod3 != oldProcedure.CodeMod3) return true;
        if (procedure.CodeMod4 != oldProcedure.CodeMod4) return true;
        if (procedure.RevCode != oldProcedure.RevCode) return true;
        if (procedure.UnitQty != oldProcedure.UnitQty) return true;
        if (procedure.BaseUnits != oldProcedure.BaseUnits) return true;
        if (procedure.StartTime != oldProcedure.StartTime) return true;
        if (procedure.StopTime != oldProcedure.StopTime) return true;
        if (procedure.DateTP.Date != oldProcedure.DateTP.Date) return true;
        if (procedure.SiteNum != oldProcedure.SiteNum) return true;
        if (procedure.HideGraphics != oldProcedure.HideGraphics) return true;
        if (procedure.CanadianTypeCodes != oldProcedure.CanadianTypeCodes) return true;
        if (procedure.ProcTime != oldProcedure.ProcTime) return true;
        if (procedure.ProcTimeEnd != oldProcedure.ProcTimeEnd) return true;
        //DateTStamp can only be set by MySQL
        if (procedure.Prognosis != oldProcedure.Prognosis) return true;
        if (procedure.DrugUnit != oldProcedure.DrugUnit) return true;
        if (procedure.DrugQty != oldProcedure.DrugQty) return true;
        if (procedure.UnitQtyType != oldProcedure.UnitQtyType) return true;
        if (procedure.StatementNum != oldProcedure.StatementNum) return true;
        if (procedure.IsLocked != oldProcedure.IsLocked) return true;
        if (procedure.BillingNote != oldProcedure.BillingNote) return true;
        if (procedure.RepeatChargeNum != oldProcedure.RepeatChargeNum) return true;
        if (procedure.DiagnosticCode2 != oldProcedure.DiagnosticCode2) return true;
        if (procedure.DiagnosticCode3 != oldProcedure.DiagnosticCode3) return true;
        if (procedure.DiagnosticCode4 != oldProcedure.DiagnosticCode4) return true;
        if (procedure.Discount != oldProcedure.Discount) return true;
        if (procedure.SnomedBodySite != oldProcedure.SnomedBodySite) return true;
        if (procedure.ProvOrderOverride != oldProcedure.ProvOrderOverride) return true;
        if (procedure.IsDateProsthEst != oldProcedure.IsDateProsthEst) return true;
        if (procedure.IcdVersion != oldProcedure.IcdVersion) return true;
        if (procedure.IsCpoe != oldProcedure.IsCpoe) return true;
        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        if (procedure.DateComplete.Date != oldProcedure.DateComplete.Date) return true;
        if (procedure.OrderingReferralNum != oldProcedure.OrderingReferralNum) return true;
        if (procedure.TaxAmt != oldProcedure.TaxAmt) return true;
        if (procedure.Urgency != oldProcedure.Urgency) return true;
        if (procedure.DiscountPlanAmt != oldProcedure.DiscountPlanAmt) return true;
        return false;
    }
    //Delete not allowed for this table
    //public static void Delete(long procNum) {
    //
    //}
    //Delete not allowed for this table
    //public static void DeleteMany(List<long> listProcNums) {
    //
    //}

    public static bool Sync(List<Procedure> listNew, List<Procedure> listDB, long userNum)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<Procedure>();
        var listUpdNew = new List<Procedure>();
        var listUpdDB = new List<Procedure>();
        var listDel = new List<Procedure>();
        listNew.Sort((x, y) => { return x.ProcNum.CompareTo(y.ProcNum); });
        listDB.Sort((x, y) => { return x.ProcNum.CompareTo(y.ProcNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        Procedure fieldNew;
        Procedure fieldDB;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            fieldDB = null;
            if (idxDB < listDB.Count) fieldDB = listDB[idxDB];
            //begin compare
            if (fieldNew != null && fieldDB == null)
            {
                //listNew has more items, listDB does not.
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew == null && fieldDB != null)
            {
                //listDB has more items, listNew does not.
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            if (fieldNew.ProcNum < fieldDB.ProcNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.ProcNum > fieldDB.ProcNum)
            {
                //dbPK less than newPK, dbItem is 'next'
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            //Both lists contain the 'next' item, update required
            listUpdNew.Add(fieldNew);
            listUpdDB.Add(fieldDB);
            idxNew++;
            idxDB++;
        }

        //Commit changes to DB
        for (var i = 0; i < listIns.Count; i++)
        {
            listIns[i].SecUserNumEntry = userNum;
            Insert(listIns[i]);
        }

        for (var i = 0; i < listUpdNew.Count; i++)
            if (Update(listUpdNew[i], listUpdDB[i]))
                rowsUpdatedCount++;

        for (var i = 0; i < listDel.Count; i++) Procedures.Delete(listDel[i].ProcNum);
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}