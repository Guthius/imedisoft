using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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
        foreach (DataRow row in table.Rows)
        {
            var procedure = new Procedure
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

    public static void Insert(Procedure procedure)
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
                                        + "NOW()" + ","
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
                                        + "NOW()" + ","
                                        + SOut.Date(procedure.DateComplete) + ","
                                        + SOut.Long(procedure.OrderingReferralNum) + ","
                                        + SOut.Double(procedure.TaxAmt) + ","
                                        + SOut.Int((int) procedure.Urgency) + ","
                                        + SOut.Double(procedure.DiscountPlanAmt) + ")";
        {
            procedure.ProcNum = Db.NonQ(command, true, "ProcNum", "procedure");
        }
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
    
    public static void Sync(List<Procedure> listNew, List<Procedure> listDB, long userNum)
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
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            Procedure fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            Procedure fieldDB = null;
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
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return;
    }
}