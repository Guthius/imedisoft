#region

using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

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

    public static ProcedureCode SelectOne(string command)
    {
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
        ProcedureCode procedureCode;
        foreach (DataRow row in table.Rows)
        {
            procedureCode = new ProcedureCode();
            procedureCode.CodeNum = SIn.Long(row["CodeNum"].ToString());
            procedureCode.ProcCode = SIn.String(row["ProcCode"].ToString());
            procedureCode.Descript = SIn.String(row["Descript"].ToString());
            procedureCode.AbbrDesc = SIn.String(row["AbbrDesc"].ToString());
            procedureCode.ProcTime = SIn.String(row["ProcTime"].ToString());
            procedureCode.ProcCat = SIn.Long(row["ProcCat"].ToString());
            procedureCode.TreatArea = (TreatmentArea) SIn.Int(row["TreatArea"].ToString());
            procedureCode.NoBillIns = SIn.Bool(row["NoBillIns"].ToString());
            procedureCode.IsProsth = SIn.Bool(row["IsProsth"].ToString());
            procedureCode.DefaultNote = SIn.String(row["DefaultNote"].ToString());
            procedureCode.IsHygiene = SIn.Bool(row["IsHygiene"].ToString());
            procedureCode.GTypeNum = SIn.Int(row["GTypeNum"].ToString());
            procedureCode.AlternateCode1 = SIn.String(row["AlternateCode1"].ToString());
            procedureCode.MedicalCode = SIn.String(row["MedicalCode"].ToString());
            procedureCode.IsTaxed = SIn.Bool(row["IsTaxed"].ToString());
            procedureCode.PaintType = (ToothPaintingType) SIn.Int(row["PaintType"].ToString());
            procedureCode.GraphicColor = Color.FromArgb(SIn.Int(row["GraphicColor"].ToString()));
            procedureCode.LaymanTerm = SIn.String(row["LaymanTerm"].ToString());
            procedureCode.IsCanadianLab = SIn.Bool(row["IsCanadianLab"].ToString());
            procedureCode.PreExisting = SIn.Bool(row["PreExisting"].ToString());
            procedureCode.BaseUnits = SIn.Int(row["BaseUnits"].ToString());
            procedureCode.SubstitutionCode = SIn.String(row["SubstitutionCode"].ToString());
            procedureCode.SubstOnlyIf = (SubstitutionCondition) SIn.Int(row["SubstOnlyIf"].ToString());
            procedureCode.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            procedureCode.IsMultiVisit = SIn.Bool(row["IsMultiVisit"].ToString());
            procedureCode.DrugNDC = SIn.String(row["DrugNDC"].ToString());
            procedureCode.RevenueCodeDefault = SIn.String(row["RevenueCodeDefault"].ToString());
            procedureCode.ProvNumDefault = SIn.Long(row["ProvNumDefault"].ToString());
            procedureCode.CanadaTimeUnits = SIn.Double(row["CanadaTimeUnits"].ToString());
            procedureCode.IsRadiology = SIn.Bool(row["IsRadiology"].ToString());
            procedureCode.DefaultClaimNote = SIn.String(row["DefaultClaimNote"].ToString());
            procedureCode.DefaultTPNote = SIn.String(row["DefaultTPNote"].ToString());
            procedureCode.BypassGlobalLock = (BypassLockStatus) SIn.Int(row["BypassGlobalLock"].ToString());
            procedureCode.TaxCode = SIn.String(row["TaxCode"].ToString());
            procedureCode.PaintText = SIn.String(row["PaintText"].ToString());
            procedureCode.AreaAlsoToothRange = SIn.Bool(row["AreaAlsoToothRange"].ToString());
            procedureCode.DiagnosticCodes = SIn.String(row["DiagnosticCodes"].ToString());
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
            table.Rows.Add(SOut.Long(procedureCode.CodeNum), procedureCode.ProcCode, procedureCode.Descript, procedureCode.AbbrDesc, procedureCode.ProcTime, SOut.Long(procedureCode.ProcCat), SOut.Int((int) procedureCode.TreatArea), SOut.Bool(procedureCode.NoBillIns), SOut.Bool(procedureCode.IsProsth), procedureCode.DefaultNote, SOut.Bool(procedureCode.IsHygiene), SOut.Int(procedureCode.GTypeNum), procedureCode.AlternateCode1, procedureCode.MedicalCode, SOut.Bool(procedureCode.IsTaxed), SOut.Int((int) procedureCode.PaintType), SOut.Int(procedureCode.GraphicColor.ToArgb()), procedureCode.LaymanTerm, SOut.Bool(procedureCode.IsCanadianLab), SOut.Bool(procedureCode.PreExisting), SOut.Int(procedureCode.BaseUnits), procedureCode.SubstitutionCode, SOut.Int((int) procedureCode.SubstOnlyIf), SOut.DateT(procedureCode.DateTStamp, false), SOut.Bool(procedureCode.IsMultiVisit), procedureCode.DrugNDC, procedureCode.RevenueCodeDefault, SOut.Long(procedureCode.ProvNumDefault), SOut.Double(procedureCode.CanadaTimeUnits), SOut.Bool(procedureCode.IsRadiology), procedureCode.DefaultClaimNote, procedureCode.DefaultTPNote, SOut.Int((int) procedureCode.BypassGlobalLock), procedureCode.TaxCode, procedureCode.PaintText, SOut.Bool(procedureCode.AreaAlsoToothRange), procedureCode.DiagnosticCodes);
        return table;
    }

    public static long Insert(ProcedureCode procedureCode)
    {
        return Insert(procedureCode, false);
    }

    public static long Insert(ProcedureCode procedureCode, bool useExistingPK)
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
        var paramDefaultNote = new OdSqlParameter("paramDefaultNote", OdDbType.Text, SOut.StringParam(procedureCode.DefaultNote));
        if (procedureCode.DefaultClaimNote == null) procedureCode.DefaultClaimNote = "";
        var paramDefaultClaimNote = new OdSqlParameter("paramDefaultClaimNote", OdDbType.Text, SOut.StringParam(procedureCode.DefaultClaimNote));
        if (procedureCode.DefaultTPNote == null) procedureCode.DefaultTPNote = "";
        var paramDefaultTPNote = new OdSqlParameter("paramDefaultTPNote", OdDbType.Text, SOut.StringParam(procedureCode.DefaultTPNote));
        {
            procedureCode.CodeNum = Db.NonQ(command, true, "CodeNum", "procedureCode", paramDefaultNote, paramDefaultClaimNote, paramDefaultTPNote);
        }
        return procedureCode.CodeNum;
    }

    public static void InsertMany(List<ProcedureCode> listProcedureCodes)
    {
        InsertMany(listProcedureCodes, false);
    }

    public static void InsertMany(List<ProcedureCode> listProcedureCodes, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listProcedureCodes.Count)
        {
            var procedureCode = listProcedureCodes[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO procedurecode (");
                if (useExistingPK) sbCommands.Append("CodeNum,");
                sbCommands.Append("ProcCode,Descript,AbbrDesc,ProcTime,ProcCat,TreatArea,NoBillIns,IsProsth,DefaultNote,IsHygiene,GTypeNum,AlternateCode1,MedicalCode,IsTaxed,PaintType,GraphicColor,LaymanTerm,IsCanadianLab,PreExisting,BaseUnits,SubstitutionCode,SubstOnlyIf,IsMultiVisit,DrugNDC,RevenueCodeDefault,ProvNumDefault,CanadaTimeUnits,IsRadiology,DefaultClaimNote,DefaultTPNote,BypassGlobalLock,TaxCode,PaintText,AreaAlsoToothRange,DiagnosticCodes) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(procedureCode.CodeNum));
                sbRow.Append(",");
            }

            sbRow.Append("'" + SOut.String(procedureCode.ProcCode) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedureCode.Descript) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedureCode.AbbrDesc) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedureCode.ProcTime) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procedureCode.ProcCat));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) procedureCode.TreatArea));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(procedureCode.NoBillIns));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(procedureCode.IsProsth));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedureCode.DefaultNote) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(procedureCode.IsHygiene));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(procedureCode.GTypeNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedureCode.AlternateCode1) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedureCode.MedicalCode) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(procedureCode.IsTaxed));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) procedureCode.PaintType));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(procedureCode.GraphicColor.ToArgb()));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedureCode.LaymanTerm) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(procedureCode.IsCanadianLab));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(procedureCode.PreExisting));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(procedureCode.BaseUnits));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedureCode.SubstitutionCode) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) procedureCode.SubstOnlyIf));
            sbRow.Append(",");
            //DateTStamp can only be set by MySQL
            sbRow.Append(SOut.Bool(procedureCode.IsMultiVisit));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedureCode.DrugNDC) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedureCode.RevenueCodeDefault) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procedureCode.ProvNumDefault));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(procedureCode.CanadaTimeUnits));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(procedureCode.IsRadiology));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedureCode.DefaultClaimNote) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedureCode.DefaultTPNote) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) procedureCode.BypassGlobalLock));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedureCode.TaxCode) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedureCode.PaintText) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(procedureCode.AreaAlsoToothRange));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(procedureCode.DiagnosticCodes) + "'");
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
                if (index == listProcedureCodes.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(ProcedureCode procedureCode)
    {
        return InsertNoCache(procedureCode, false);
    }

    public static long InsertNoCache(ProcedureCode procedureCode, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO procedurecode (";
        if (isRandomKeys || useExistingPK) command += "CodeNum,";
        command += "ProcCode,Descript,AbbrDesc,ProcTime,ProcCat,TreatArea,NoBillIns,IsProsth,DefaultNote,IsHygiene,GTypeNum,AlternateCode1,MedicalCode,IsTaxed,PaintType,GraphicColor,LaymanTerm,IsCanadianLab,PreExisting,BaseUnits,SubstitutionCode,SubstOnlyIf,IsMultiVisit,DrugNDC,RevenueCodeDefault,ProvNumDefault,CanadaTimeUnits,IsRadiology,DefaultClaimNote,DefaultTPNote,BypassGlobalLock,TaxCode,PaintText,AreaAlsoToothRange,DiagnosticCodes) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(procedureCode.CodeNum) + ",";
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
        var paramDefaultNote = new OdSqlParameter("paramDefaultNote", OdDbType.Text, SOut.StringParam(procedureCode.DefaultNote));
        if (procedureCode.DefaultClaimNote == null) procedureCode.DefaultClaimNote = "";
        var paramDefaultClaimNote = new OdSqlParameter("paramDefaultClaimNote", OdDbType.Text, SOut.StringParam(procedureCode.DefaultClaimNote));
        if (procedureCode.DefaultTPNote == null) procedureCode.DefaultTPNote = "";
        var paramDefaultTPNote = new OdSqlParameter("paramDefaultTPNote", OdDbType.Text, SOut.StringParam(procedureCode.DefaultTPNote));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDefaultNote, paramDefaultClaimNote, paramDefaultTPNote);
        else
            procedureCode.CodeNum = Db.NonQ(command, true, "CodeNum", "procedureCode", paramDefaultNote, paramDefaultClaimNote, paramDefaultTPNote);
        return procedureCode.CodeNum;
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
        var paramDefaultNote = new OdSqlParameter("paramDefaultNote", OdDbType.Text, SOut.StringParam(procedureCode.DefaultNote));
        if (procedureCode.DefaultClaimNote == null) procedureCode.DefaultClaimNote = "";
        var paramDefaultClaimNote = new OdSqlParameter("paramDefaultClaimNote", OdDbType.Text, SOut.StringParam(procedureCode.DefaultClaimNote));
        if (procedureCode.DefaultTPNote == null) procedureCode.DefaultTPNote = "";
        var paramDefaultTPNote = new OdSqlParameter("paramDefaultTPNote", OdDbType.Text, SOut.StringParam(procedureCode.DefaultTPNote));
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
        var paramDefaultNote = new OdSqlParameter("paramDefaultNote", OdDbType.Text, SOut.StringParam(procedureCode.DefaultNote));
        if (procedureCode.DefaultClaimNote == null) procedureCode.DefaultClaimNote = "";
        var paramDefaultClaimNote = new OdSqlParameter("paramDefaultClaimNote", OdDbType.Text, SOut.StringParam(procedureCode.DefaultClaimNote));
        if (procedureCode.DefaultTPNote == null) procedureCode.DefaultTPNote = "";
        var paramDefaultTPNote = new OdSqlParameter("paramDefaultTPNote", OdDbType.Text, SOut.StringParam(procedureCode.DefaultTPNote));
        command = "UPDATE procedurecode SET " + command
                                              + " WHERE CodeNum = " + SOut.Long(procedureCode.CodeNum);
        Db.NonQ(command, paramDefaultNote, paramDefaultClaimNote, paramDefaultTPNote);
        return true;
    }

    public static bool UpdateComparison(ProcedureCode procedureCode, ProcedureCode oldProcedureCode)
    {
        //ProcCode excluded from update
        if (procedureCode.Descript != oldProcedureCode.Descript) return true;
        if (procedureCode.AbbrDesc != oldProcedureCode.AbbrDesc) return true;
        if (procedureCode.ProcTime != oldProcedureCode.ProcTime) return true;
        if (procedureCode.ProcCat != oldProcedureCode.ProcCat) return true;
        if (procedureCode.TreatArea != oldProcedureCode.TreatArea) return true;
        if (procedureCode.NoBillIns != oldProcedureCode.NoBillIns) return true;
        if (procedureCode.IsProsth != oldProcedureCode.IsProsth) return true;
        if (procedureCode.DefaultNote != oldProcedureCode.DefaultNote) return true;
        if (procedureCode.IsHygiene != oldProcedureCode.IsHygiene) return true;
        if (procedureCode.GTypeNum != oldProcedureCode.GTypeNum) return true;
        if (procedureCode.AlternateCode1 != oldProcedureCode.AlternateCode1) return true;
        if (procedureCode.MedicalCode != oldProcedureCode.MedicalCode) return true;
        if (procedureCode.IsTaxed != oldProcedureCode.IsTaxed) return true;
        if (procedureCode.PaintType != oldProcedureCode.PaintType) return true;
        if (procedureCode.GraphicColor != oldProcedureCode.GraphicColor) return true;
        if (procedureCode.LaymanTerm != oldProcedureCode.LaymanTerm) return true;
        if (procedureCode.IsCanadianLab != oldProcedureCode.IsCanadianLab) return true;
        if (procedureCode.PreExisting != oldProcedureCode.PreExisting) return true;
        if (procedureCode.BaseUnits != oldProcedureCode.BaseUnits) return true;
        if (procedureCode.SubstitutionCode != oldProcedureCode.SubstitutionCode) return true;
        if (procedureCode.SubstOnlyIf != oldProcedureCode.SubstOnlyIf) return true;
        //DateTStamp can only be set by MySQL
        if (procedureCode.IsMultiVisit != oldProcedureCode.IsMultiVisit) return true;
        if (procedureCode.DrugNDC != oldProcedureCode.DrugNDC) return true;
        if (procedureCode.RevenueCodeDefault != oldProcedureCode.RevenueCodeDefault) return true;
        if (procedureCode.ProvNumDefault != oldProcedureCode.ProvNumDefault) return true;
        if (procedureCode.CanadaTimeUnits != oldProcedureCode.CanadaTimeUnits) return true;
        if (procedureCode.IsRadiology != oldProcedureCode.IsRadiology) return true;
        if (procedureCode.DefaultClaimNote != oldProcedureCode.DefaultClaimNote) return true;
        if (procedureCode.DefaultTPNote != oldProcedureCode.DefaultTPNote) return true;
        if (procedureCode.BypassGlobalLock != oldProcedureCode.BypassGlobalLock) return true;
        if (procedureCode.TaxCode != oldProcedureCode.TaxCode) return true;
        if (procedureCode.PaintText != oldProcedureCode.PaintText) return true;
        if (procedureCode.AreaAlsoToothRange != oldProcedureCode.AreaAlsoToothRange) return true;
        if (procedureCode.DiagnosticCodes != oldProcedureCode.DiagnosticCodes) return true;
        return false;
    }

    public static void Delete(long codeNum)
    {
        ClearFkey(codeNum);
        var command = "DELETE FROM procedurecode "
                      + "WHERE CodeNum = " + SOut.Long(codeNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listCodeNums)
    {
        if (listCodeNums == null || listCodeNums.Count == 0) return;
        ClearFkey(listCodeNums);
        var command = "DELETE FROM procedurecode "
                      + "WHERE CodeNum IN(" + string.Join(",", listCodeNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static void ClearFkey(long codeNum)
    {
        if (codeNum == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey=" + SOut.Long(codeNum) + " AND PermType IN (64)";
        Db.NonQ(command);
    }

    public static void ClearFkey(List<long> listCodeNums)
    {
        if (listCodeNums == null || listCodeNums.FindAll(x => x != 0).Count == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey IN(" + string.Join(",", listCodeNums.FindAll(x => x != 0)) + ") AND PermType IN (64)";
        Db.NonQ(command);
    }
}