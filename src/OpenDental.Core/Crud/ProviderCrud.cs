#region

using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ProviderCrud
{
    public static Provider SelectOne(long provNum)
    {
        var command = "SELECT * FROM provider "
                      + "WHERE ProvNum = " + SOut.Long(provNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Provider SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Provider> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Provider> TableToList(DataTable table)
    {
        var retVal = new List<Provider>();
        Provider provider;
        foreach (DataRow row in table.Rows)
        {
            provider = new Provider();
            provider.ProvNum = SIn.Long(row["ProvNum"].ToString());
            provider.Abbr = SIn.String(row["Abbr"].ToString());
            provider.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            provider.LName = SIn.String(row["LName"].ToString());
            provider.FName = SIn.String(row["FName"].ToString());
            provider.MI = SIn.String(row["MI"].ToString());
            provider.Suffix = SIn.String(row["Suffix"].ToString());
            provider.FeeSched = SIn.Long(row["FeeSched"].ToString());
            provider.Specialty = SIn.Long(row["Specialty"].ToString());
            provider.SSN = SIn.String(row["SSN"].ToString());
            provider.StateLicense = SIn.String(row["StateLicense"].ToString());
            provider.DEANum = SIn.String(row["DEANum"].ToString());
            provider.IsSecondary = SIn.Bool(row["IsSecondary"].ToString());
            provider.ProvColor = Color.FromArgb(SIn.Int(row["ProvColor"].ToString()));
            provider.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            provider.UsingTIN = SIn.Bool(row["UsingTIN"].ToString());
            provider.BlueCrossID = SIn.String(row["BlueCrossID"].ToString());
            provider.SigOnFile = SIn.Bool(row["SigOnFile"].ToString());
            provider.MedicaidID = SIn.String(row["MedicaidID"].ToString());
            provider.OutlineColor = Color.FromArgb(SIn.Int(row["OutlineColor"].ToString()));
            provider.SchoolClassNum = SIn.Long(row["SchoolClassNum"].ToString());
            provider.NationalProvID = SIn.String(row["NationalProvID"].ToString());
            provider.CanadianOfficeNum = SIn.String(row["CanadianOfficeNum"].ToString());
            provider.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            provider.AnesthProvType = SIn.Long(row["AnesthProvType"].ToString());
            provider.TaxonomyCodeOverride = SIn.String(row["TaxonomyCodeOverride"].ToString());
            provider.IsCDAnet = SIn.Bool(row["IsCDAnet"].ToString());
            provider.EcwID = SIn.String(row["EcwID"].ToString());
            provider.StateRxID = SIn.String(row["StateRxID"].ToString());
            provider.IsNotPerson = SIn.Bool(row["IsNotPerson"].ToString());
            provider.StateWhereLicensed = SIn.String(row["StateWhereLicensed"].ToString());
            provider.EmailAddressNum = SIn.Long(row["EmailAddressNum"].ToString());
            provider.IsInstructor = SIn.Bool(row["IsInstructor"].ToString());
            provider.EhrMuStage = SIn.Int(row["EhrMuStage"].ToString());
            provider.ProvNumBillingOverride = SIn.Long(row["ProvNumBillingOverride"].ToString());
            provider.CustomID = SIn.String(row["CustomID"].ToString());
            provider.ProvStatus = (ProviderStatus) SIn.Int(row["ProvStatus"].ToString());
            provider.IsHiddenReport = SIn.Bool(row["IsHiddenReport"].ToString());
            provider.IsErxEnabled = (ErxEnabledStatus) SIn.Int(row["IsErxEnabled"].ToString());
            provider.SchedNote = SIn.String(row["SchedNote"].ToString());
            provider.Birthdate = SIn.Date(row["Birthdate"].ToString());
            provider.WebSchedDescript = SIn.String(row["WebSchedDescript"].ToString());
            provider.WebSchedImageLocation = SIn.String(row["WebSchedImageLocation"].ToString());
            provider.HourlyProdGoalAmt = SIn.Double(row["HourlyProdGoalAmt"].ToString());
            provider.DateTerm = SIn.Date(row["DateTerm"].ToString());
            provider.PreferredName = SIn.String(row["PreferredName"].ToString());
            retVal.Add(provider);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Provider> listProviders, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Provider";
        var table = new DataTable(tableName);
        table.Columns.Add("ProvNum");
        table.Columns.Add("Abbr");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("LName");
        table.Columns.Add("FName");
        table.Columns.Add("MI");
        table.Columns.Add("Suffix");
        table.Columns.Add("FeeSched");
        table.Columns.Add("Specialty");
        table.Columns.Add("SSN");
        table.Columns.Add("StateLicense");
        table.Columns.Add("DEANum");
        table.Columns.Add("IsSecondary");
        table.Columns.Add("ProvColor");
        table.Columns.Add("IsHidden");
        table.Columns.Add("UsingTIN");
        table.Columns.Add("BlueCrossID");
        table.Columns.Add("SigOnFile");
        table.Columns.Add("MedicaidID");
        table.Columns.Add("OutlineColor");
        table.Columns.Add("SchoolClassNum");
        table.Columns.Add("NationalProvID");
        table.Columns.Add("CanadianOfficeNum");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("AnesthProvType");
        table.Columns.Add("TaxonomyCodeOverride");
        table.Columns.Add("IsCDAnet");
        table.Columns.Add("EcwID");
        table.Columns.Add("StateRxID");
        table.Columns.Add("IsNotPerson");
        table.Columns.Add("StateWhereLicensed");
        table.Columns.Add("EmailAddressNum");
        table.Columns.Add("IsInstructor");
        table.Columns.Add("EhrMuStage");
        table.Columns.Add("ProvNumBillingOverride");
        table.Columns.Add("CustomID");
        table.Columns.Add("ProvStatus");
        table.Columns.Add("IsHiddenReport");
        table.Columns.Add("IsErxEnabled");
        table.Columns.Add("SchedNote");
        table.Columns.Add("Birthdate");
        table.Columns.Add("WebSchedDescript");
        table.Columns.Add("WebSchedImageLocation");
        table.Columns.Add("HourlyProdGoalAmt");
        table.Columns.Add("DateTerm");
        table.Columns.Add("PreferredName");
        foreach (var provider in listProviders)
            table.Rows.Add(SOut.Long(provider.ProvNum), provider.Abbr, SOut.Int(provider.ItemOrder), provider.LName, provider.FName, provider.MI, provider.Suffix, SOut.Long(provider.FeeSched), SOut.Long(provider.Specialty), provider.SSN, provider.StateLicense, provider.DEANum, SOut.Bool(provider.IsSecondary), SOut.Int(provider.ProvColor.ToArgb()), SOut.Bool(provider.IsHidden), SOut.Bool(provider.UsingTIN), provider.BlueCrossID, SOut.Bool(provider.SigOnFile), provider.MedicaidID, SOut.Int(provider.OutlineColor.ToArgb()), SOut.Long(provider.SchoolClassNum), provider.NationalProvID, provider.CanadianOfficeNum, SOut.DateT(provider.DateTStamp, false), SOut.Long(provider.AnesthProvType), provider.TaxonomyCodeOverride, SOut.Bool(provider.IsCDAnet), provider.EcwID, provider.StateRxID, SOut.Bool(provider.IsNotPerson), provider.StateWhereLicensed, SOut.Long(provider.EmailAddressNum), SOut.Bool(provider.IsInstructor), SOut.Int(provider.EhrMuStage), SOut.Long(provider.ProvNumBillingOverride), provider.CustomID, SOut.Int((int) provider.ProvStatus), SOut.Bool(provider.IsHiddenReport), SOut.Int((int) provider.IsErxEnabled), provider.SchedNote, SOut.DateT(provider.Birthdate, false), provider.WebSchedDescript, provider.WebSchedImageLocation, SOut.Double(provider.HourlyProdGoalAmt), SOut.DateT(provider.DateTerm, false), provider.PreferredName);
        return table;
    }

    public static long Insert(Provider provider)
    {
        return Insert(provider, false);
    }

    public static long Insert(Provider provider, bool useExistingPK)
    {
        var command = "INSERT INTO provider (";

        command += "Abbr,ItemOrder,LName,FName,MI,Suffix,FeeSched,Specialty,SSN,StateLicense,DEANum,IsSecondary,ProvColor,IsHidden,UsingTIN,BlueCrossID,SigOnFile,MedicaidID,OutlineColor,SchoolClassNum,NationalProvID,CanadianOfficeNum,AnesthProvType,TaxonomyCodeOverride,IsCDAnet,EcwID,StateRxID,IsNotPerson,StateWhereLicensed,EmailAddressNum,IsInstructor,EhrMuStage,ProvNumBillingOverride,CustomID,ProvStatus,IsHiddenReport,IsErxEnabled,SchedNote,Birthdate,WebSchedDescript,WebSchedImageLocation,HourlyProdGoalAmt,DateTerm,PreferredName) VALUES(";

        command +=
            "'" + SOut.String(provider.Abbr) + "',"
            + SOut.Int(provider.ItemOrder) + ","
            + "'" + SOut.String(provider.LName) + "',"
            + "'" + SOut.String(provider.FName) + "',"
            + "'" + SOut.String(provider.MI) + "',"
            + "'" + SOut.String(provider.Suffix) + "',"
            + SOut.Long(provider.FeeSched) + ","
            + SOut.Long(provider.Specialty) + ","
            + "'" + SOut.String(provider.SSN) + "',"
            + "'" + SOut.String(provider.StateLicense) + "',"
            + "'" + SOut.String(provider.DEANum) + "',"
            + SOut.Bool(provider.IsSecondary) + ","
            + SOut.Int(provider.ProvColor.ToArgb()) + ","
            + SOut.Bool(provider.IsHidden) + ","
            + SOut.Bool(provider.UsingTIN) + ","
            + "'" + SOut.String(provider.BlueCrossID) + "',"
            + SOut.Bool(provider.SigOnFile) + ","
            + "'" + SOut.String(provider.MedicaidID) + "',"
            + SOut.Int(provider.OutlineColor.ToArgb()) + ","
            + SOut.Long(provider.SchoolClassNum) + ","
            + "'" + SOut.String(provider.NationalProvID) + "',"
            + "'" + SOut.String(provider.CanadianOfficeNum) + "',"
            //DateTStamp can only be set by MySQL
            + SOut.Long(provider.AnesthProvType) + ","
            + "'" + SOut.String(provider.TaxonomyCodeOverride) + "',"
            + SOut.Bool(provider.IsCDAnet) + ","
            + "'" + SOut.String(provider.EcwID) + "',"
            + "'" + SOut.String(provider.StateRxID) + "',"
            + SOut.Bool(provider.IsNotPerson) + ","
            + "'" + SOut.String(provider.StateWhereLicensed) + "',"
            + SOut.Long(provider.EmailAddressNum) + ","
            + SOut.Bool(provider.IsInstructor) + ","
            + SOut.Int(provider.EhrMuStage) + ","
            + SOut.Long(provider.ProvNumBillingOverride) + ","
            + "'" + SOut.String(provider.CustomID) + "',"
            + SOut.Int((int) provider.ProvStatus) + ","
            + SOut.Bool(provider.IsHiddenReport) + ","
            + SOut.Int((int) provider.IsErxEnabled) + ","
            + "'" + SOut.String(provider.SchedNote) + "',"
            + SOut.Date(provider.Birthdate) + ","
            + "'" + SOut.String(provider.WebSchedDescript) + "',"
            + "'" + SOut.String(provider.WebSchedImageLocation) + "',"
            + SOut.Double(provider.HourlyProdGoalAmt) + ","
            + SOut.Date(provider.DateTerm) + ","
            + "'" + SOut.String(provider.PreferredName) + "')";
        {
            provider.ProvNum = Db.NonQ(command, true, "ProvNum", "provider");
        }
        return provider.ProvNum;
    }

    public static long InsertNoCache(Provider provider)
    {
        return InsertNoCache(provider, false);
    }

    public static long InsertNoCache(Provider provider, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO provider (";
        if (isRandomKeys || useExistingPK) command += "ProvNum,";
        command += "Abbr,ItemOrder,LName,FName,MI,Suffix,FeeSched,Specialty,SSN,StateLicense,DEANum,IsSecondary,ProvColor,IsHidden,UsingTIN,BlueCrossID,SigOnFile,MedicaidID,OutlineColor,SchoolClassNum,NationalProvID,CanadianOfficeNum,AnesthProvType,TaxonomyCodeOverride,IsCDAnet,EcwID,StateRxID,IsNotPerson,StateWhereLicensed,EmailAddressNum,IsInstructor,EhrMuStage,ProvNumBillingOverride,CustomID,ProvStatus,IsHiddenReport,IsErxEnabled,SchedNote,Birthdate,WebSchedDescript,WebSchedImageLocation,HourlyProdGoalAmt,DateTerm,PreferredName) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(provider.ProvNum) + ",";
        command +=
            "'" + SOut.String(provider.Abbr) + "',"
            + SOut.Int(provider.ItemOrder) + ","
            + "'" + SOut.String(provider.LName) + "',"
            + "'" + SOut.String(provider.FName) + "',"
            + "'" + SOut.String(provider.MI) + "',"
            + "'" + SOut.String(provider.Suffix) + "',"
            + SOut.Long(provider.FeeSched) + ","
            + SOut.Long(provider.Specialty) + ","
            + "'" + SOut.String(provider.SSN) + "',"
            + "'" + SOut.String(provider.StateLicense) + "',"
            + "'" + SOut.String(provider.DEANum) + "',"
            + SOut.Bool(provider.IsSecondary) + ","
            + SOut.Int(provider.ProvColor.ToArgb()) + ","
            + SOut.Bool(provider.IsHidden) + ","
            + SOut.Bool(provider.UsingTIN) + ","
            + "'" + SOut.String(provider.BlueCrossID) + "',"
            + SOut.Bool(provider.SigOnFile) + ","
            + "'" + SOut.String(provider.MedicaidID) + "',"
            + SOut.Int(provider.OutlineColor.ToArgb()) + ","
            + SOut.Long(provider.SchoolClassNum) + ","
            + "'" + SOut.String(provider.NationalProvID) + "',"
            + "'" + SOut.String(provider.CanadianOfficeNum) + "',"
            //DateTStamp can only be set by MySQL
            + SOut.Long(provider.AnesthProvType) + ","
            + "'" + SOut.String(provider.TaxonomyCodeOverride) + "',"
            + SOut.Bool(provider.IsCDAnet) + ","
            + "'" + SOut.String(provider.EcwID) + "',"
            + "'" + SOut.String(provider.StateRxID) + "',"
            + SOut.Bool(provider.IsNotPerson) + ","
            + "'" + SOut.String(provider.StateWhereLicensed) + "',"
            + SOut.Long(provider.EmailAddressNum) + ","
            + SOut.Bool(provider.IsInstructor) + ","
            + SOut.Int(provider.EhrMuStage) + ","
            + SOut.Long(provider.ProvNumBillingOverride) + ","
            + "'" + SOut.String(provider.CustomID) + "',"
            + SOut.Int((int) provider.ProvStatus) + ","
            + SOut.Bool(provider.IsHiddenReport) + ","
            + SOut.Int((int) provider.IsErxEnabled) + ","
            + "'" + SOut.String(provider.SchedNote) + "',"
            + SOut.Date(provider.Birthdate) + ","
            + "'" + SOut.String(provider.WebSchedDescript) + "',"
            + "'" + SOut.String(provider.WebSchedImageLocation) + "',"
            + SOut.Double(provider.HourlyProdGoalAmt) + ","
            + SOut.Date(provider.DateTerm) + ","
            + "'" + SOut.String(provider.PreferredName) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            provider.ProvNum = Db.NonQ(command, true, "ProvNum", "provider");
        return provider.ProvNum;
    }

    public static void Update(Provider provider)
    {
        var command = "UPDATE provider SET "
                      + "Abbr                  = '" + SOut.String(provider.Abbr) + "', "
                      + "ItemOrder             =  " + SOut.Int(provider.ItemOrder) + ", "
                      + "LName                 = '" + SOut.String(provider.LName) + "', "
                      + "FName                 = '" + SOut.String(provider.FName) + "', "
                      + "MI                    = '" + SOut.String(provider.MI) + "', "
                      + "Suffix                = '" + SOut.String(provider.Suffix) + "', "
                      + "FeeSched              =  " + SOut.Long(provider.FeeSched) + ", "
                      + "Specialty             =  " + SOut.Long(provider.Specialty) + ", "
                      + "SSN                   = '" + SOut.String(provider.SSN) + "', "
                      + "StateLicense          = '" + SOut.String(provider.StateLicense) + "', "
                      + "DEANum                = '" + SOut.String(provider.DEANum) + "', "
                      + "IsSecondary           =  " + SOut.Bool(provider.IsSecondary) + ", "
                      + "ProvColor             =  " + SOut.Int(provider.ProvColor.ToArgb()) + ", "
                      + "IsHidden              =  " + SOut.Bool(provider.IsHidden) + ", "
                      + "UsingTIN              =  " + SOut.Bool(provider.UsingTIN) + ", "
                      + "BlueCrossID           = '" + SOut.String(provider.BlueCrossID) + "', "
                      + "SigOnFile             =  " + SOut.Bool(provider.SigOnFile) + ", "
                      + "MedicaidID            = '" + SOut.String(provider.MedicaidID) + "', "
                      + "OutlineColor          =  " + SOut.Int(provider.OutlineColor.ToArgb()) + ", "
                      + "SchoolClassNum        =  " + SOut.Long(provider.SchoolClassNum) + ", "
                      + "NationalProvID        = '" + SOut.String(provider.NationalProvID) + "', "
                      + "CanadianOfficeNum     = '" + SOut.String(provider.CanadianOfficeNum) + "', "
                      //DateTStamp can only be set by MySQL
                      + "AnesthProvType        =  " + SOut.Long(provider.AnesthProvType) + ", "
                      + "TaxonomyCodeOverride  = '" + SOut.String(provider.TaxonomyCodeOverride) + "', "
                      + "IsCDAnet              =  " + SOut.Bool(provider.IsCDAnet) + ", "
                      + "EcwID                 = '" + SOut.String(provider.EcwID) + "', "
                      + "StateRxID             = '" + SOut.String(provider.StateRxID) + "', "
                      + "IsNotPerson           =  " + SOut.Bool(provider.IsNotPerson) + ", "
                      + "StateWhereLicensed    = '" + SOut.String(provider.StateWhereLicensed) + "', "
                      + "EmailAddressNum       =  " + SOut.Long(provider.EmailAddressNum) + ", "
                      + "IsInstructor          =  " + SOut.Bool(provider.IsInstructor) + ", "
                      + "EhrMuStage            =  " + SOut.Int(provider.EhrMuStage) + ", "
                      + "ProvNumBillingOverride=  " + SOut.Long(provider.ProvNumBillingOverride) + ", "
                      + "CustomID              = '" + SOut.String(provider.CustomID) + "', "
                      + "ProvStatus            =  " + SOut.Int((int) provider.ProvStatus) + ", "
                      + "IsHiddenReport        =  " + SOut.Bool(provider.IsHiddenReport) + ", "
                      + "IsErxEnabled          =  " + SOut.Int((int) provider.IsErxEnabled) + ", "
                      + "SchedNote             = '" + SOut.String(provider.SchedNote) + "', "
                      + "Birthdate             =  " + SOut.Date(provider.Birthdate) + ", "
                      + "WebSchedDescript      = '" + SOut.String(provider.WebSchedDescript) + "', "
                      + "WebSchedImageLocation = '" + SOut.String(provider.WebSchedImageLocation) + "', "
                      + "HourlyProdGoalAmt     =  " + SOut.Double(provider.HourlyProdGoalAmt) + ", "
                      + "DateTerm              =  " + SOut.Date(provider.DateTerm) + ", "
                      + "PreferredName         = '" + SOut.String(provider.PreferredName) + "' "
                      + "WHERE ProvNum = " + SOut.Long(provider.ProvNum);
        Db.NonQ(command);
    }

    public static bool Update(Provider provider, Provider oldProvider)
    {
        var command = "";
        if (provider.Abbr != oldProvider.Abbr)
        {
            if (command != "") command += ",";
            command += "Abbr = '" + SOut.String(provider.Abbr) + "'";
        }

        if (provider.ItemOrder != oldProvider.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(provider.ItemOrder) + "";
        }

        if (provider.LName != oldProvider.LName)
        {
            if (command != "") command += ",";
            command += "LName = '" + SOut.String(provider.LName) + "'";
        }

        if (provider.FName != oldProvider.FName)
        {
            if (command != "") command += ",";
            command += "FName = '" + SOut.String(provider.FName) + "'";
        }

        if (provider.MI != oldProvider.MI)
        {
            if (command != "") command += ",";
            command += "MI = '" + SOut.String(provider.MI) + "'";
        }

        if (provider.Suffix != oldProvider.Suffix)
        {
            if (command != "") command += ",";
            command += "Suffix = '" + SOut.String(provider.Suffix) + "'";
        }

        if (provider.FeeSched != oldProvider.FeeSched)
        {
            if (command != "") command += ",";
            command += "FeeSched = " + SOut.Long(provider.FeeSched) + "";
        }

        if (provider.Specialty != oldProvider.Specialty)
        {
            if (command != "") command += ",";
            command += "Specialty = " + SOut.Long(provider.Specialty) + "";
        }

        if (provider.SSN != oldProvider.SSN)
        {
            if (command != "") command += ",";
            command += "SSN = '" + SOut.String(provider.SSN) + "'";
        }

        if (provider.StateLicense != oldProvider.StateLicense)
        {
            if (command != "") command += ",";
            command += "StateLicense = '" + SOut.String(provider.StateLicense) + "'";
        }

        if (provider.DEANum != oldProvider.DEANum)
        {
            if (command != "") command += ",";
            command += "DEANum = '" + SOut.String(provider.DEANum) + "'";
        }

        if (provider.IsSecondary != oldProvider.IsSecondary)
        {
            if (command != "") command += ",";
            command += "IsSecondary = " + SOut.Bool(provider.IsSecondary) + "";
        }

        if (provider.ProvColor != oldProvider.ProvColor)
        {
            if (command != "") command += ",";
            command += "ProvColor = " + SOut.Int(provider.ProvColor.ToArgb()) + "";
        }

        if (provider.IsHidden != oldProvider.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(provider.IsHidden) + "";
        }

        if (provider.UsingTIN != oldProvider.UsingTIN)
        {
            if (command != "") command += ",";
            command += "UsingTIN = " + SOut.Bool(provider.UsingTIN) + "";
        }

        if (provider.BlueCrossID != oldProvider.BlueCrossID)
        {
            if (command != "") command += ",";
            command += "BlueCrossID = '" + SOut.String(provider.BlueCrossID) + "'";
        }

        if (provider.SigOnFile != oldProvider.SigOnFile)
        {
            if (command != "") command += ",";
            command += "SigOnFile = " + SOut.Bool(provider.SigOnFile) + "";
        }

        if (provider.MedicaidID != oldProvider.MedicaidID)
        {
            if (command != "") command += ",";
            command += "MedicaidID = '" + SOut.String(provider.MedicaidID) + "'";
        }

        if (provider.OutlineColor != oldProvider.OutlineColor)
        {
            if (command != "") command += ",";
            command += "OutlineColor = " + SOut.Int(provider.OutlineColor.ToArgb()) + "";
        }

        if (provider.SchoolClassNum != oldProvider.SchoolClassNum)
        {
            if (command != "") command += ",";
            command += "SchoolClassNum = " + SOut.Long(provider.SchoolClassNum) + "";
        }

        if (provider.NationalProvID != oldProvider.NationalProvID)
        {
            if (command != "") command += ",";
            command += "NationalProvID = '" + SOut.String(provider.NationalProvID) + "'";
        }

        if (provider.CanadianOfficeNum != oldProvider.CanadianOfficeNum)
        {
            if (command != "") command += ",";
            command += "CanadianOfficeNum = '" + SOut.String(provider.CanadianOfficeNum) + "'";
        }

        //DateTStamp can only be set by MySQL
        if (provider.AnesthProvType != oldProvider.AnesthProvType)
        {
            if (command != "") command += ",";
            command += "AnesthProvType = " + SOut.Long(provider.AnesthProvType) + "";
        }

        if (provider.TaxonomyCodeOverride != oldProvider.TaxonomyCodeOverride)
        {
            if (command != "") command += ",";
            command += "TaxonomyCodeOverride = '" + SOut.String(provider.TaxonomyCodeOverride) + "'";
        }

        if (provider.IsCDAnet != oldProvider.IsCDAnet)
        {
            if (command != "") command += ",";
            command += "IsCDAnet = " + SOut.Bool(provider.IsCDAnet) + "";
        }

        if (provider.EcwID != oldProvider.EcwID)
        {
            if (command != "") command += ",";
            command += "EcwID = '" + SOut.String(provider.EcwID) + "'";
        }

        if (provider.StateRxID != oldProvider.StateRxID)
        {
            if (command != "") command += ",";
            command += "StateRxID = '" + SOut.String(provider.StateRxID) + "'";
        }

        if (provider.IsNotPerson != oldProvider.IsNotPerson)
        {
            if (command != "") command += ",";
            command += "IsNotPerson = " + SOut.Bool(provider.IsNotPerson) + "";
        }

        if (provider.StateWhereLicensed != oldProvider.StateWhereLicensed)
        {
            if (command != "") command += ",";
            command += "StateWhereLicensed = '" + SOut.String(provider.StateWhereLicensed) + "'";
        }

        if (provider.EmailAddressNum != oldProvider.EmailAddressNum)
        {
            if (command != "") command += ",";
            command += "EmailAddressNum = " + SOut.Long(provider.EmailAddressNum) + "";
        }

        if (provider.IsInstructor != oldProvider.IsInstructor)
        {
            if (command != "") command += ",";
            command += "IsInstructor = " + SOut.Bool(provider.IsInstructor) + "";
        }

        if (provider.EhrMuStage != oldProvider.EhrMuStage)
        {
            if (command != "") command += ",";
            command += "EhrMuStage = " + SOut.Int(provider.EhrMuStage) + "";
        }

        if (provider.ProvNumBillingOverride != oldProvider.ProvNumBillingOverride)
        {
            if (command != "") command += ",";
            command += "ProvNumBillingOverride = " + SOut.Long(provider.ProvNumBillingOverride) + "";
        }

        if (provider.CustomID != oldProvider.CustomID)
        {
            if (command != "") command += ",";
            command += "CustomID = '" + SOut.String(provider.CustomID) + "'";
        }

        if (provider.ProvStatus != oldProvider.ProvStatus)
        {
            if (command != "") command += ",";
            command += "ProvStatus = " + SOut.Int((int) provider.ProvStatus) + "";
        }

        if (provider.IsHiddenReport != oldProvider.IsHiddenReport)
        {
            if (command != "") command += ",";
            command += "IsHiddenReport = " + SOut.Bool(provider.IsHiddenReport) + "";
        }

        if (provider.IsErxEnabled != oldProvider.IsErxEnabled)
        {
            if (command != "") command += ",";
            command += "IsErxEnabled = " + SOut.Int((int) provider.IsErxEnabled) + "";
        }

        if (provider.SchedNote != oldProvider.SchedNote)
        {
            if (command != "") command += ",";
            command += "SchedNote = '" + SOut.String(provider.SchedNote) + "'";
        }

        if (provider.Birthdate.Date != oldProvider.Birthdate.Date)
        {
            if (command != "") command += ",";
            command += "Birthdate = " + SOut.Date(provider.Birthdate) + "";
        }

        if (provider.WebSchedDescript != oldProvider.WebSchedDescript)
        {
            if (command != "") command += ",";
            command += "WebSchedDescript = '" + SOut.String(provider.WebSchedDescript) + "'";
        }

        if (provider.WebSchedImageLocation != oldProvider.WebSchedImageLocation)
        {
            if (command != "") command += ",";
            command += "WebSchedImageLocation = '" + SOut.String(provider.WebSchedImageLocation) + "'";
        }

        if (provider.HourlyProdGoalAmt != oldProvider.HourlyProdGoalAmt)
        {
            if (command != "") command += ",";
            command += "HourlyProdGoalAmt = " + SOut.Double(provider.HourlyProdGoalAmt) + "";
        }

        if (provider.DateTerm.Date != oldProvider.DateTerm.Date)
        {
            if (command != "") command += ",";
            command += "DateTerm = " + SOut.Date(provider.DateTerm) + "";
        }

        if (provider.PreferredName != oldProvider.PreferredName)
        {
            if (command != "") command += ",";
            command += "PreferredName = '" + SOut.String(provider.PreferredName) + "'";
        }

        if (command == "") return false;
        command = "UPDATE provider SET " + command
                                         + " WHERE ProvNum = " + SOut.Long(provider.ProvNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Provider provider, Provider oldProvider)
    {
        if (provider.Abbr != oldProvider.Abbr) return true;
        if (provider.ItemOrder != oldProvider.ItemOrder) return true;
        if (provider.LName != oldProvider.LName) return true;
        if (provider.FName != oldProvider.FName) return true;
        if (provider.MI != oldProvider.MI) return true;
        if (provider.Suffix != oldProvider.Suffix) return true;
        if (provider.FeeSched != oldProvider.FeeSched) return true;
        if (provider.Specialty != oldProvider.Specialty) return true;
        if (provider.SSN != oldProvider.SSN) return true;
        if (provider.StateLicense != oldProvider.StateLicense) return true;
        if (provider.DEANum != oldProvider.DEANum) return true;
        if (provider.IsSecondary != oldProvider.IsSecondary) return true;
        if (provider.ProvColor != oldProvider.ProvColor) return true;
        if (provider.IsHidden != oldProvider.IsHidden) return true;
        if (provider.UsingTIN != oldProvider.UsingTIN) return true;
        if (provider.BlueCrossID != oldProvider.BlueCrossID) return true;
        if (provider.SigOnFile != oldProvider.SigOnFile) return true;
        if (provider.MedicaidID != oldProvider.MedicaidID) return true;
        if (provider.OutlineColor != oldProvider.OutlineColor) return true;
        if (provider.SchoolClassNum != oldProvider.SchoolClassNum) return true;
        if (provider.NationalProvID != oldProvider.NationalProvID) return true;
        if (provider.CanadianOfficeNum != oldProvider.CanadianOfficeNum) return true;
        //DateTStamp can only be set by MySQL
        if (provider.AnesthProvType != oldProvider.AnesthProvType) return true;
        if (provider.TaxonomyCodeOverride != oldProvider.TaxonomyCodeOverride) return true;
        if (provider.IsCDAnet != oldProvider.IsCDAnet) return true;
        if (provider.EcwID != oldProvider.EcwID) return true;
        if (provider.StateRxID != oldProvider.StateRxID) return true;
        if (provider.IsNotPerson != oldProvider.IsNotPerson) return true;
        if (provider.StateWhereLicensed != oldProvider.StateWhereLicensed) return true;
        if (provider.EmailAddressNum != oldProvider.EmailAddressNum) return true;
        if (provider.IsInstructor != oldProvider.IsInstructor) return true;
        if (provider.EhrMuStage != oldProvider.EhrMuStage) return true;
        if (provider.ProvNumBillingOverride != oldProvider.ProvNumBillingOverride) return true;
        if (provider.CustomID != oldProvider.CustomID) return true;
        if (provider.ProvStatus != oldProvider.ProvStatus) return true;
        if (provider.IsHiddenReport != oldProvider.IsHiddenReport) return true;
        if (provider.IsErxEnabled != oldProvider.IsErxEnabled) return true;
        if (provider.SchedNote != oldProvider.SchedNote) return true;
        if (provider.Birthdate.Date != oldProvider.Birthdate.Date) return true;
        if (provider.WebSchedDescript != oldProvider.WebSchedDescript) return true;
        if (provider.WebSchedImageLocation != oldProvider.WebSchedImageLocation) return true;
        if (provider.HourlyProdGoalAmt != oldProvider.HourlyProdGoalAmt) return true;
        if (provider.DateTerm.Date != oldProvider.DateTerm.Date) return true;
        if (provider.PreferredName != oldProvider.PreferredName) return true;
        return false;
    }

    public static void Delete(long provNum)
    {
        var command = "DELETE FROM provider "
                      + "WHERE ProvNum = " + SOut.Long(provNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listProvNums)
    {
        if (listProvNums == null || listProvNums.Count == 0) return;
        var command = "DELETE FROM provider "
                      + "WHERE ProvNum IN(" + string.Join(",", listProvNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}