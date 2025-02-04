using System.Collections.Generic;
using System.Data;
using System.Drawing;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ProcedureCodeCrud
{
    public static ProcedureCode SelectOne(long codeNum)
    {
        var command = "SELECT * FROM procedurecode "
                      + "WHERE CodeNum = " + SOut.Long(codeNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ProcedureCode> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcedureCode> TableToList(DataTable table)
    {
        var retVal = new List<ProcedureCode>();
        foreach (DataRow row in table.Rows)
        {
            var procedureCode = new ProcedureCode
            {
                CodeNum = SIn.Long(row["CodeNum"].ToString()),
                ProcCode = SIn.String(row["ProcCode"].ToString()),
                Descript = SIn.String(row["Descript"].ToString()),
                AbbrDesc = SIn.String(row["AbbrDesc"].ToString()),
                ProcTime = SIn.String(row["ProcTime"].ToString()),
                ProcCat = SIn.Long(row["ProcCat"].ToString()),
                TreatArea = (TreatmentArea) SIn.Int(row["TreatArea"].ToString()),
                NoBillIns = SIn.Bool(row["NoBillIns"].ToString()),
                IsProsth = SIn.Bool(row["IsProsth"].ToString()),
                DefaultNote = SIn.String(row["DefaultNote"].ToString()),
                IsHygiene = SIn.Bool(row["IsHygiene"].ToString()),
                GTypeNum = SIn.Int(row["GTypeNum"].ToString()),
                AlternateCode1 = SIn.String(row["AlternateCode1"].ToString()),
                MedicalCode = SIn.String(row["MedicalCode"].ToString()),
                IsTaxed = SIn.Bool(row["IsTaxed"].ToString()),
                PaintType = (ToothPaintingType) SIn.Int(row["PaintType"].ToString()),
                GraphicColor = Color.FromArgb(SIn.Int(row["GraphicColor"].ToString())),
                LaymanTerm = SIn.String(row["LaymanTerm"].ToString()),
                IsCanadianLab = SIn.Bool(row["IsCanadianLab"].ToString()),
                PreExisting = SIn.Bool(row["PreExisting"].ToString()),
                BaseUnits = SIn.Int(row["BaseUnits"].ToString()),
                SubstitutionCode = SIn.String(row["SubstitutionCode"].ToString()),
                SubstOnlyIf = (SubstitutionCondition) SIn.Int(row["SubstOnlyIf"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString()),
                IsMultiVisit = SIn.Bool(row["IsMultiVisit"].ToString()),
                DrugNDC = SIn.String(row["DrugNDC"].ToString()),
                RevenueCodeDefault = SIn.String(row["RevenueCodeDefault"].ToString()),
                ProvNumDefault = SIn.Long(row["ProvNumDefault"].ToString()),
                CanadaTimeUnits = SIn.Double(row["CanadaTimeUnits"].ToString()),
                IsRadiology = SIn.Bool(row["IsRadiology"].ToString()),
                DefaultClaimNote = SIn.String(row["DefaultClaimNote"].ToString()),
                DefaultTPNote = SIn.String(row["DefaultTPNote"].ToString()),
                BypassGlobalLock = (BypassLockStatus) SIn.Int(row["BypassGlobalLock"].ToString()),
                TaxCode = SIn.String(row["TaxCode"].ToString()),
                PaintText = SIn.String(row["PaintText"].ToString()),
                AreaAlsoToothRange = SIn.Bool(row["AreaAlsoToothRange"].ToString()),
                DiagnosticCodes = SIn.String(row["DiagnosticCodes"].ToString())
            };
            retVal.Add(procedureCode);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ProcedureCode> listProcedureCodes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ProcedureCode";
        var table = new DataTable(tableName);
        table.Columns.Add("CodeNum");
        table.Columns.Add("ProcCode");
        table.Columns.Add("Descript");
        table.Columns.Add("AbbrDesc");
        table.Columns.Add("ProcTime");
        table.Columns.Add("ProcCat");
        table.Columns.Add("TreatArea");
        table.Columns.Add("NoBillIns");
        table.Columns.Add("IsProsth");
        table.Columns.Add("DefaultNote");
        table.Columns.Add("IsHygiene");
        table.Columns.Add("GTypeNum");
        table.Columns.Add("AlternateCode1");
        table.Columns.Add("MedicalCode");
        table.Columns.Add("IsTaxed");
        table.Columns.Add("PaintType");
        table.Columns.Add("GraphicColor");
        table.Columns.Add("LaymanTerm");
        table.Columns.Add("IsCanadianLab");
        table.Columns.Add("PreExisting");
        table.Columns.Add("BaseUnits");
        table.Columns.Add("SubstitutionCode");
        table.Columns.Add("SubstOnlyIf");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("IsMultiVisit");
        table.Columns.Add("DrugNDC");
        table.Columns.Add("RevenueCodeDefault");
        table.Columns.Add("ProvNumDefault");
        table.Columns.Add("CanadaTimeUnits");
        table.Columns.Add("IsRadiology");
        table.Columns.Add("DefaultClaimNote");
        table.Columns.Add("DefaultTPNote");
        table.Columns.Add("BypassGlobalLock");
        table.Columns.Add("TaxCode");
        table.Columns.Add("PaintText");
        table.Columns.Add("AreaAlsoToothRange");
        table.Columns.Add("DiagnosticCodes");
        foreach (var procedureCode in listProcedureCodes)
            table.Rows.Add(SOut.Long(procedureCode.CodeNum), procedureCode.ProcCode, procedureCode.Descript, procedureCode.AbbrDesc, procedureCode.ProcTime, SOut.Long(procedureCode.ProcCat), SOut.Int((int) procedureCode.TreatArea), SOut.Bool(procedureCode.NoBillIns), SOut.Bool(procedureCode.IsProsth), procedureCode.DefaultNote, SOut.Bool(procedureCode.IsHygiene), SOut.Int(procedureCode.GTypeNum), procedureCode.AlternateCode1, procedureCode.MedicalCode, SOut.Bool(procedureCode.IsTaxed), SOut.Int((int) procedureCode.PaintType), SOut.Int(procedureCode.GraphicColor.ToArgb()), procedureCode.LaymanTerm, SOut.Bool(procedureCode.IsCanadianLab), SOut.Bool(procedureCode.PreExisting), SOut.Int(procedureCode.BaseUnits), procedureCode.SubstitutionCode, SOut.Int((int) procedureCode.SubstOnlyIf), SOut.DateTime(procedureCode.DateTStamp, false), SOut.Bool(procedureCode.IsMultiVisit), procedureCode.DrugNDC, procedureCode.RevenueCodeDefault, SOut.Long(procedureCode.ProvNumDefault), SOut.Double(procedureCode.CanadaTimeUnits), SOut.Bool(procedureCode.IsRadiology), procedureCode.DefaultClaimNote, procedureCode.DefaultTPNote, SOut.Int((int) procedureCode.BypassGlobalLock), procedureCode.TaxCode, procedureCode.PaintText, SOut.Bool(procedureCode.AreaAlsoToothRange), procedureCode.DiagnosticCodes);
        return table;
    }

    public static void Insert(ProcedureCode procedureCode)
    {
        var command = "INSERT INTO procedurecode (";

        command += "ProcCode,Descript,AbbrDesc,ProcTime,ProcCat,TreatArea,NoBillIns,IsProsth,DefaultNote,IsHygiene,GTypeNum,AlternateCode1,MedicalCode,IsTaxed,PaintType,GraphicColor,LaymanTerm,IsCanadianLab,PreExisting,BaseUnits,SubstitutionCode,SubstOnlyIf,IsMultiVisit,DrugNDC,RevenueCodeDefault,ProvNumDefault,CanadaTimeUnits,IsRadiology,DefaultClaimNote,DefaultTPNote,BypassGlobalLock,TaxCode,PaintText,AreaAlsoToothRange,DiagnosticCodes) VALUES(";

        command +=
            "'" + SOut.String(procedureCode.ProcCode) + "',"
            + "'" + SOut.String(procedureCode.Descript) + "',"
            + "'" + SOut.String(procedureCode.AbbrDesc) + "',"
            + "'" + SOut.String(procedureCode.ProcTime) + "',"
            + SOut.Long(procedureCode.ProcCat) + ","
            + SOut.Int((int) procedureCode.TreatArea) + ","
            + SOut.Bool(procedureCode.NoBillIns) + ","
            + SOut.Bool(procedureCode.IsProsth) + ","
            + DbHelper.ParamChar + "paramDefaultNote,"
            + SOut.Bool(procedureCode.IsHygiene) + ","
            + SOut.Int(procedureCode.GTypeNum) + ","
            + "'" + SOut.String(procedureCode.AlternateCode1) + "',"
            + "'" + SOut.String(procedureCode.MedicalCode) + "',"
            + SOut.Bool(procedureCode.IsTaxed) + ","
            + SOut.Int((int) procedureCode.PaintType) + ","
            + SOut.Int(procedureCode.GraphicColor.ToArgb()) + ","
            + "'" + SOut.String(procedureCode.LaymanTerm) + "',"
            + SOut.Bool(procedureCode.IsCanadianLab) + ","
            + SOut.Bool(procedureCode.PreExisting) + ","
            + SOut.Int(procedureCode.BaseUnits) + ","
            + "'" + SOut.String(procedureCode.SubstitutionCode) + "',"
            + SOut.Int((int) procedureCode.SubstOnlyIf) + ","
            //DateTStamp can only be set by MySQL
            + SOut.Bool(procedureCode.IsMultiVisit) + ","
            + "'" + SOut.String(procedureCode.DrugNDC) + "',"
            + "'" + SOut.String(procedureCode.RevenueCodeDefault) + "',"
            + SOut.Long(procedureCode.ProvNumDefault) + ","
            + SOut.Double(procedureCode.CanadaTimeUnits) + ","
            + SOut.Bool(procedureCode.IsRadiology) + ","
            + DbHelper.ParamChar + "paramDefaultClaimNote,"
            + DbHelper.ParamChar + "paramDefaultTPNote,"
            + SOut.Int((int) procedureCode.BypassGlobalLock) + ","
            + "'" + SOut.String(procedureCode.TaxCode) + "',"
            + "'" + SOut.String(procedureCode.PaintText) + "',"
            + SOut.Bool(procedureCode.AreaAlsoToothRange) + ","
            + "'" + SOut.String(procedureCode.DiagnosticCodes) + "')";
        if (procedureCode.DefaultNote == null) procedureCode.DefaultNote = "";
        var paramDefaultNote = new OdSqlParameter("paramDefaultNote", SOut.StringParam(procedureCode.DefaultNote));
        if (procedureCode.DefaultClaimNote == null) procedureCode.DefaultClaimNote = "";
        var paramDefaultClaimNote = new OdSqlParameter("paramDefaultClaimNote", SOut.StringParam(procedureCode.DefaultClaimNote));
        if (procedureCode.DefaultTPNote == null) procedureCode.DefaultTPNote = "";
        var paramDefaultTPNote = new OdSqlParameter("paramDefaultTPNote", SOut.StringParam(procedureCode.DefaultTPNote));
        {
            procedureCode.CodeNum = Db.NonQ(command, true, "CodeNum", "procedureCode", paramDefaultNote, paramDefaultClaimNote, paramDefaultTPNote);
        }
    }

    public static void Update(ProcedureCode procedureCode)
    {
        var command = "UPDATE procedurecode SET "
                      //ProcCode excluded from update
                      + "Descript          = '" + SOut.String(procedureCode.Descript) + "', "
                      + "AbbrDesc          = '" + SOut.String(procedureCode.AbbrDesc) + "', "
                      + "ProcTime          = '" + SOut.String(procedureCode.ProcTime) + "', "
                      + "ProcCat           =  " + SOut.Long(procedureCode.ProcCat) + ", "
                      + "TreatArea         =  " + SOut.Int((int) procedureCode.TreatArea) + ", "
                      + "NoBillIns         =  " + SOut.Bool(procedureCode.NoBillIns) + ", "
                      + "IsProsth          =  " + SOut.Bool(procedureCode.IsProsth) + ", "
                      + "DefaultNote       =  " + DbHelper.ParamChar + "paramDefaultNote, "
                      + "IsHygiene         =  " + SOut.Bool(procedureCode.IsHygiene) + ", "
                      + "GTypeNum          =  " + SOut.Int(procedureCode.GTypeNum) + ", "
                      + "AlternateCode1    = '" + SOut.String(procedureCode.AlternateCode1) + "', "
                      + "MedicalCode       = '" + SOut.String(procedureCode.MedicalCode) + "', "
                      + "IsTaxed           =  " + SOut.Bool(procedureCode.IsTaxed) + ", "
                      + "PaintType         =  " + SOut.Int((int) procedureCode.PaintType) + ", "
                      + "GraphicColor      =  " + SOut.Int(procedureCode.GraphicColor.ToArgb()) + ", "
                      + "LaymanTerm        = '" + SOut.String(procedureCode.LaymanTerm) + "', "
                      + "IsCanadianLab     =  " + SOut.Bool(procedureCode.IsCanadianLab) + ", "
                      + "PreExisting       =  " + SOut.Bool(procedureCode.PreExisting) + ", "
                      + "BaseUnits         =  " + SOut.Int(procedureCode.BaseUnits) + ", "
                      + "SubstitutionCode  = '" + SOut.String(procedureCode.SubstitutionCode) + "', "
                      + "SubstOnlyIf       =  " + SOut.Int((int) procedureCode.SubstOnlyIf) + ", "
                      //DateTStamp can only be set by MySQL
                      + "IsMultiVisit      =  " + SOut.Bool(procedureCode.IsMultiVisit) + ", "
                      + "DrugNDC           = '" + SOut.String(procedureCode.DrugNDC) + "', "
                      + "RevenueCodeDefault= '" + SOut.String(procedureCode.RevenueCodeDefault) + "', "
                      + "ProvNumDefault    =  " + SOut.Long(procedureCode.ProvNumDefault) + ", "
                      + "CanadaTimeUnits   =  " + SOut.Double(procedureCode.CanadaTimeUnits) + ", "
                      + "IsRadiology       =  " + SOut.Bool(procedureCode.IsRadiology) + ", "
                      + "DefaultClaimNote  =  " + DbHelper.ParamChar + "paramDefaultClaimNote, "
                      + "DefaultTPNote     =  " + DbHelper.ParamChar + "paramDefaultTPNote, "
                      + "BypassGlobalLock  =  " + SOut.Int((int) procedureCode.BypassGlobalLock) + ", "
                      + "TaxCode           = '" + SOut.String(procedureCode.TaxCode) + "', "
                      + "PaintText         = '" + SOut.String(procedureCode.PaintText) + "', "
                      + "AreaAlsoToothRange=  " + SOut.Bool(procedureCode.AreaAlsoToothRange) + ", "
                      + "DiagnosticCodes   = '" + SOut.String(procedureCode.DiagnosticCodes) + "' "
                      + "WHERE CodeNum = " + SOut.Long(procedureCode.CodeNum);
        if (procedureCode.DefaultNote == null) procedureCode.DefaultNote = "";
        var paramDefaultNote = new OdSqlParameter("paramDefaultNote", SOut.StringParam(procedureCode.DefaultNote));
        if (procedureCode.DefaultClaimNote == null) procedureCode.DefaultClaimNote = "";
        var paramDefaultClaimNote = new OdSqlParameter("paramDefaultClaimNote", SOut.StringParam(procedureCode.DefaultClaimNote));
        if (procedureCode.DefaultTPNote == null) procedureCode.DefaultTPNote = "";
        var paramDefaultTPNote = new OdSqlParameter("paramDefaultTPNote", SOut.StringParam(procedureCode.DefaultTPNote));
        Db.NonQ(command, paramDefaultNote, paramDefaultClaimNote, paramDefaultTPNote);
    }

    public static bool Update(ProcedureCode procedureCode, ProcedureCode oldProcedureCode)
    {
        var command = "";
        //ProcCode excluded from update
        if (procedureCode.Descript != oldProcedureCode.Descript)
        {
            if (command != "") command += ",";
            command += "Descript = '" + SOut.String(procedureCode.Descript) + "'";
        }

        if (procedureCode.AbbrDesc != oldProcedureCode.AbbrDesc)
        {
            if (command != "") command += ",";
            command += "AbbrDesc = '" + SOut.String(procedureCode.AbbrDesc) + "'";
        }

        if (procedureCode.ProcTime != oldProcedureCode.ProcTime)
        {
            if (command != "") command += ",";
            command += "ProcTime = '" + SOut.String(procedureCode.ProcTime) + "'";
        }

        if (procedureCode.ProcCat != oldProcedureCode.ProcCat)
        {
            if (command != "") command += ",";
            command += "ProcCat = " + SOut.Long(procedureCode.ProcCat) + "";
        }

        if (procedureCode.TreatArea != oldProcedureCode.TreatArea)
        {
            if (command != "") command += ",";
            command += "TreatArea = " + SOut.Int((int) procedureCode.TreatArea) + "";
        }

        if (procedureCode.NoBillIns != oldProcedureCode.NoBillIns)
        {
            if (command != "") command += ",";
            command += "NoBillIns = " + SOut.Bool(procedureCode.NoBillIns) + "";
        }

        if (procedureCode.IsProsth != oldProcedureCode.IsProsth)
        {
            if (command != "") command += ",";
            command += "IsProsth = " + SOut.Bool(procedureCode.IsProsth) + "";
        }

        if (procedureCode.DefaultNote != oldProcedureCode.DefaultNote)
        {
            if (command != "") command += ",";
            command += "DefaultNote = " + DbHelper.ParamChar + "paramDefaultNote";
        }

        if (procedureCode.IsHygiene != oldProcedureCode.IsHygiene)
        {
            if (command != "") command += ",";
            command += "IsHygiene = " + SOut.Bool(procedureCode.IsHygiene) + "";
        }

        if (procedureCode.GTypeNum != oldProcedureCode.GTypeNum)
        {
            if (command != "") command += ",";
            command += "GTypeNum = " + SOut.Int(procedureCode.GTypeNum) + "";
        }

        if (procedureCode.AlternateCode1 != oldProcedureCode.AlternateCode1)
        {
            if (command != "") command += ",";
            command += "AlternateCode1 = '" + SOut.String(procedureCode.AlternateCode1) + "'";
        }

        if (procedureCode.MedicalCode != oldProcedureCode.MedicalCode)
        {
            if (command != "") command += ",";
            command += "MedicalCode = '" + SOut.String(procedureCode.MedicalCode) + "'";
        }

        if (procedureCode.IsTaxed != oldProcedureCode.IsTaxed)
        {
            if (command != "") command += ",";
            command += "IsTaxed = " + SOut.Bool(procedureCode.IsTaxed) + "";
        }

        if (procedureCode.PaintType != oldProcedureCode.PaintType)
        {
            if (command != "") command += ",";
            command += "PaintType = " + SOut.Int((int) procedureCode.PaintType) + "";
        }

        if (procedureCode.GraphicColor != oldProcedureCode.GraphicColor)
        {
            if (command != "") command += ",";
            command += "GraphicColor = " + SOut.Int(procedureCode.GraphicColor.ToArgb()) + "";
        }

        if (procedureCode.LaymanTerm != oldProcedureCode.LaymanTerm)
        {
            if (command != "") command += ",";
            command += "LaymanTerm = '" + SOut.String(procedureCode.LaymanTerm) + "'";
        }

        if (procedureCode.IsCanadianLab != oldProcedureCode.IsCanadianLab)
        {
            if (command != "") command += ",";
            command += "IsCanadianLab = " + SOut.Bool(procedureCode.IsCanadianLab) + "";
        }

        if (procedureCode.PreExisting != oldProcedureCode.PreExisting)
        {
            if (command != "") command += ",";
            command += "PreExisting = " + SOut.Bool(procedureCode.PreExisting) + "";
        }

        if (procedureCode.BaseUnits != oldProcedureCode.BaseUnits)
        {
            if (command != "") command += ",";
            command += "BaseUnits = " + SOut.Int(procedureCode.BaseUnits) + "";
        }

        if (procedureCode.SubstitutionCode != oldProcedureCode.SubstitutionCode)
        {
            if (command != "") command += ",";
            command += "SubstitutionCode = '" + SOut.String(procedureCode.SubstitutionCode) + "'";
        }

        if (procedureCode.SubstOnlyIf != oldProcedureCode.SubstOnlyIf)
        {
            if (command != "") command += ",";
            command += "SubstOnlyIf = " + SOut.Int((int) procedureCode.SubstOnlyIf) + "";
        }

        //DateTStamp can only be set by MySQL
        if (procedureCode.IsMultiVisit != oldProcedureCode.IsMultiVisit)
        {
            if (command != "") command += ",";
            command += "IsMultiVisit = " + SOut.Bool(procedureCode.IsMultiVisit) + "";
        }

        if (procedureCode.DrugNDC != oldProcedureCode.DrugNDC)
        {
            if (command != "") command += ",";
            command += "DrugNDC = '" + SOut.String(procedureCode.DrugNDC) + "'";
        }

        if (procedureCode.RevenueCodeDefault != oldProcedureCode.RevenueCodeDefault)
        {
            if (command != "") command += ",";
            command += "RevenueCodeDefault = '" + SOut.String(procedureCode.RevenueCodeDefault) + "'";
        }

        if (procedureCode.ProvNumDefault != oldProcedureCode.ProvNumDefault)
        {
            if (command != "") command += ",";
            command += "ProvNumDefault = " + SOut.Long(procedureCode.ProvNumDefault) + "";
        }

        if (procedureCode.CanadaTimeUnits != oldProcedureCode.CanadaTimeUnits)
        {
            if (command != "") command += ",";
            command += "CanadaTimeUnits = " + SOut.Double(procedureCode.CanadaTimeUnits) + "";
        }

        if (procedureCode.IsRadiology != oldProcedureCode.IsRadiology)
        {
            if (command != "") command += ",";
            command += "IsRadiology = " + SOut.Bool(procedureCode.IsRadiology) + "";
        }

        if (procedureCode.DefaultClaimNote != oldProcedureCode.DefaultClaimNote)
        {
            if (command != "") command += ",";
            command += "DefaultClaimNote = " + DbHelper.ParamChar + "paramDefaultClaimNote";
        }

        if (procedureCode.DefaultTPNote != oldProcedureCode.DefaultTPNote)
        {
            if (command != "") command += ",";
            command += "DefaultTPNote = " + DbHelper.ParamChar + "paramDefaultTPNote";
        }

        if (procedureCode.BypassGlobalLock != oldProcedureCode.BypassGlobalLock)
        {
            if (command != "") command += ",";
            command += "BypassGlobalLock = " + SOut.Int((int) procedureCode.BypassGlobalLock) + "";
        }

        if (procedureCode.TaxCode != oldProcedureCode.TaxCode)
        {
            if (command != "") command += ",";
            command += "TaxCode = '" + SOut.String(procedureCode.TaxCode) + "'";
        }

        if (procedureCode.PaintText != oldProcedureCode.PaintText)
        {
            if (command != "") command += ",";
            command += "PaintText = '" + SOut.String(procedureCode.PaintText) + "'";
        }

        if (procedureCode.AreaAlsoToothRange != oldProcedureCode.AreaAlsoToothRange)
        {
            if (command != "") command += ",";
            command += "AreaAlsoToothRange = " + SOut.Bool(procedureCode.AreaAlsoToothRange) + "";
        }

        if (procedureCode.DiagnosticCodes != oldProcedureCode.DiagnosticCodes)
        {
            if (command != "") command += ",";
            command += "DiagnosticCodes = '" + SOut.String(procedureCode.DiagnosticCodes) + "'";
        }

        if (command == "") return false;
        if (procedureCode.DefaultNote == null) procedureCode.DefaultNote = "";
        var paramDefaultNote = new OdSqlParameter("paramDefaultNote", SOut.StringParam(procedureCode.DefaultNote));
        if (procedureCode.DefaultClaimNote == null) procedureCode.DefaultClaimNote = "";
        var paramDefaultClaimNote = new OdSqlParameter("paramDefaultClaimNote", SOut.StringParam(procedureCode.DefaultClaimNote));
        if (procedureCode.DefaultTPNote == null) procedureCode.DefaultTPNote = "";
        var paramDefaultTPNote = new OdSqlParameter("paramDefaultTPNote", SOut.StringParam(procedureCode.DefaultTPNote));
        command = "UPDATE procedurecode SET " + command
                                              + " WHERE CodeNum = " + SOut.Long(procedureCode.CodeNum);
        Db.NonQ(command, paramDefaultNote, paramDefaultClaimNote, paramDefaultTPNote);
        return true;
    }

    public static void ClearFkey(List<long> listCodeNums)
    {
        if (listCodeNums == null || listCodeNums.FindAll(x => x != 0).Count == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey IN(" + string.Join(",", listCodeNums.FindAll(x => x != 0)) + ") AND PermType IN (64)";
        Db.NonQ(command);
    }
}