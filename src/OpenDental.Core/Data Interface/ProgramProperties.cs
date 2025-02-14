using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness.WebTypes.Shared.XWeb;
using EdgeExpressProps = OpenDentBusiness.ProgramProperties.PropertyDescs.EdgeExpress;

namespace OpenDentBusiness;

public class ProgramProperties
{
    public static void Update(ProgramProperty programProp)
    {
        ProgramPropertyCrud.Update(programProp);
    }

    public static void Update(ProgramProperty programProp, ProgramProperty programPropOld)
    {
        ProgramPropertyCrud.Update(programProp, programPropOld);
    }

    public static void UpdateProgramPropertyWithValue(ProgramProperty programProp, string newValue)
    {
        if (programProp.PropertyValue == newValue)
        {
            return;
        }
        
        programProp.PropertyValue = newValue;
        
        Update(programProp);
    }

    public static void Insert(ProgramProperty programProp)
    {
        ProgramPropertyCrud.Insert(programProp);
    }

    public static bool InsertForClinic(long programNum, List<long> clinicNums)
    {
        if (clinicNums == null || clinicNums.Count == 0)
        {
            return false;
        }

        var commandText = "INSERT INTO programproperty (ProgramNum,PropertyDesc,PropertyValue,ComputerName,ClinicNum) ";
        
        for (var i = 0; i < clinicNums.Count; i++)
        {
            if (i > 0)
            {
                commandText += " UNION ";
            }
            
            commandText += 
                "SELECT ProgramNum,PropertyDesc,PropertyValue,ComputerName," + clinicNums[i] + " " + 
                "FROM programproperty " + 
                "WHERE ProgramNum=" + programNum + " " + 
                "AND ClinicNum=0";
        }

        return Db.NonQ(commandText) > 0;
    }

    public static bool IsAdvertisingDisabled(ProgramName progName)
    {
        var program = Programs.GetCur(progName);
        
        return IsAdvertisingDisabled(program);
    }

    public static bool IsAdvertisingDisabled(Program program)
    {
        if (program == null)
        {
            return true;
        }
        
        if (program.Enabled)
        {
            return false;
        }
        
        return GetForProgram(program.ProgramNum).Any(x => 
            (x.PropertyDesc == "Disable Advertising" && x.PropertyValue == "1") ||
            (x.PropertyDesc == "Disable Advertising HQ" && x.PropertyValue == "1"));
    }

    public static bool IsAdvertisingBridge(long programNum)
    {
        return GetForProgram(programNum).Any(x => x.PropertyDesc is "Disable Advertising" or "Disable Advertising HQ");
    }

    public static List<ProgramProperty> GetListForProgramAndClinic(long programNum, long clinicNum)
    {
        return GetWhere(x => x.ProgramNum == programNum && x.ClinicNum == clinicNum && x.PropertyDesc != "");
    }

    public static List<ProgramProperty> GetListForProgramAndClinicWithDefault(long programNum, long clinicNum)
    {
        var properties = GetWhere(x => x.ProgramNum == programNum && x.ClinicNum == clinicNum);
        if (clinicNum == 0)
        {
            return properties;
        }
        
        var clinicAndDefaultProperties = GetWhere(x => x.ProgramNum == programNum && x.ClinicNum == 0 && properties.All(y => y.PropertyDesc != x.PropertyDesc));
        
        clinicAndDefaultProperties.AddRange(properties);
        
        return clinicAndDefaultProperties;
    }

    public static string GetPropValForClinicOrDefault(long programNum, string desc, long clinicNum)
    {
        return GetListForProgramAndClinicWithDefault(programNum, clinicNum).First(x => x.PropertyDesc == desc).PropertyValue;
    }

    public static List<ProgramProperty> GetForProgram(long programNum)
    {
        return GetWhere(x => x.ProgramNum == programNum && x.PropertyDesc != "").OrderBy(x => x.ClinicNum).ThenBy(x => x.ProgramPropertyNum).ToList();
    }

    public static long SetProperty(long programNum, string desc, string propval)
    {
        return Db.NonQ(
            $"""
             UPDATE programproperty SET PropertyValue='{SOut.String(propval)}'
             WHERE ProgramNum={programNum}
             AND PropertyDesc='{SOut.String(desc)}'
             """);
    }

    public static ProgramProperty GetCur(List<ProgramProperty> properties, string desc)
    {
        return properties.FirstOrDefault(x => x.PropertyDesc == desc);
    }

    public static string GetPropVal(long programNum, string desc)
    {
        var programProperty = GetFirstOrDefault(x => x.ProgramNum == programNum && x.PropertyDesc == desc);
        if (programProperty is not null)
        {
            return programProperty.PropertyValue;
        }
        
        throw new ApplicationException("Property not found: " + desc);
    }

    public static string GetPropVal(ProgramName programName, string desc)
    {
        var programNum = Programs.GetProgramNum(programName);
        
        return GetPropVal(programNum, desc);
    }

    public static string GetPropVal(long programNum, string desc, long clinicNum)
    {
        return GetPropValFromList(GetWhere(x => x.ProgramNum == programNum), desc, clinicNum);
    }

    public static string GetPropValFromList(List<ProgramProperty> properties, string propertyDesc, long clinicNum = 0)
    {
        var prop = properties.Where(x => x.ClinicNum == clinicNum).FirstOrDefault(x => x.PropertyDesc == propertyDesc);
        
        return prop is not null ? prop.PropertyValue : string.Empty;
    }

    public static ProgramProperty GetPropForProgByDesc(long programNum, string propertyDesc)
    {
        return GetForProgram(programNum).FirstOrDefault(x => x.PropertyDesc == propertyDesc);
    }

    public static ProgramProperty GetPropForProgByDesc(long programNum, string propertyDesc, long clinicNum)
    {
        return GetForProgram(programNum).FirstOrDefault(x => x.PropertyDesc == propertyDesc && x.ClinicNum == clinicNum);
    }

    public static string GetValFromDb(long programNum, string desc)
    {
        var table = DataCore.GetTable("SELECT PropertyValue FROM programproperty WHERE ProgramNum=" + programNum + " AND PropertyDesc='" + SOut.String(desc) + "'");
        
        return table.Rows.Count == 0 ? "" : table.Rows[0][0].ToString();
    }

    public static string GetLocalPathOverrideForProgram(long programNum)
    {
        var programProperty = GetFirstOrDefault(x => 
            x.ProgramNum == programNum && 
            x.PropertyDesc == "" && 
            x.ComputerName.ToUpper() == Environment.MachineName.ToUpper());
        
        return programProperty == null ? "" : programProperty.PropertyValue;
    }

    public static void InsertOrUpdateLocalOverridePath(long programNum, string newPath)
    {
        var programProperty = GetFirstOrDefault(x => x.ProgramNum == programNum
                                                     && x.PropertyDesc == ""
                                                     && x.ComputerName.ToUpper() == Environment.MachineName.ToUpper());
        if (programProperty != null)
        {
            programProperty.PropertyValue = newPath;
            Update(programProperty);
            return; //Will only be one override per computer per program.
        }

        //Path override does not exist for the current computer so create a new one.
        var pp = new ProgramProperty();
        pp.ProgramNum = programNum;
        pp.PropertyValue = newPath;
        pp.ComputerName = Environment.MachineName.ToUpper();
        Insert(pp);
    }

    public static void Sync(List<ProgramProperty> listProgPropsNew, long programNum)
    {
        //prevents delete of program properties for clinics added while editing program properties.
        var listClinicNums = listProgPropsNew.Select(x => x.ClinicNum).Distinct().ToList();
        var listProgPropsDb = GetWhere(x => x.ProgramNum == programNum
                                            && x.PropertyDesc != ""
                                            && listClinicNums.Contains(x.ClinicNum));
        ProgramPropertyCrud.Sync(listProgPropsNew, listProgPropsDb);
    }

    public static void GetXWebCreds(long clinicNum, out WebPaymentProperties xwebProperties)
    {
        string xWebID;
        string authKey;
        string terminalID;
        long paymentTypeDefNum;
        string paymentTypeDefString;
        bool isPaymentsAllowed;
        bool isXWeb;
        xwebProperties = new WebPaymentProperties();
        //Secure arguments are held in the db.
        var prog = Programs.GetCur(ProgramName.EdgeExpress);
        if (prog is {Enabled: true})
        {
            var edgeExpressProperties = GetListForProgramAndClinic(prog.ProgramNum, clinicNum);
            
            xWebID = GetPropValFromList(edgeExpressProperties, EdgeExpressProps.XWebID, clinicNum);
            authKey = GetPropValFromList(edgeExpressProperties, EdgeExpressProps.AuthKey, clinicNum);
            terminalID = GetPropValFromList(edgeExpressProperties, EdgeExpressProps.TerminalID, clinicNum);
            paymentTypeDefString = GetPropValFromList(edgeExpressProperties, EdgeExpressProps.PaymentType, clinicNum);
            isPaymentsAllowed = SIn.Bool(GetPropValFromList(edgeExpressProperties, EdgeExpressProps.IsOnlinePaymentsEnabled, clinicNum));
            isXWeb = false;
        }
        else
        {
            prog = Programs.GetCur(ProgramName.Xcharge);
            if (prog == null)
            {
                throw new ODException("X-Charge program link not found.", ODException.ErrorCodes.XWebProgramProperties);
            }
            
            if (!prog.Enabled)
            {
                throw new ODException("EdgeExpress program link is disabled.", ODException.ErrorCodes.XWebProgramProperties);
            }
            
            var listXchargeProperties = GetListForProgramAndClinic(prog.ProgramNum, clinicNum);
            xWebID = GetPropValFromList(listXchargeProperties, "XWebID", clinicNum);
            authKey = GetPropValFromList(listXchargeProperties, "AuthKey", clinicNum);
            terminalID = GetPropValFromList(listXchargeProperties, "TerminalID", clinicNum);
            paymentTypeDefString = GetPropValFromList(listXchargeProperties, "PaymentType", clinicNum);
            isPaymentsAllowed = SIn.Bool(GetPropValFromList(listXchargeProperties, "IsOnlinePaymentsEnabled", clinicNum));
            isXWeb = true;
        }

        //Validate ALL XWebID, AuthKey, and TerminalID.  Each is required for X-Web to work.
        if (string.IsNullOrEmpty(xWebID) || string.IsNullOrEmpty(authKey) || string.IsNullOrEmpty(terminalID) ||
            !long.TryParse(paymentTypeDefString, out paymentTypeDefNum))
            throw new ODException("X-Web program properties not found.", ODException.ErrorCodes.XWebProgramProperties);
        //XWeb ID must be 12 digits, Auth Key 32 alphanumeric characters, and Terminal ID 8 digits.
        if (!Regex.IsMatch(xWebID, "^[0-9]{12}$")
            || !Regex.IsMatch(authKey, "^[A-Za-z0-9]{32}$")
            || !Regex.IsMatch(terminalID, "^[0-9]{8}$"))
            throw new ODException("X-Web program properties not valid.", ODException.ErrorCodes.XWebProgramProperties);
        xwebProperties.XWebID = xWebID;
        xwebProperties.TerminalID = terminalID;
        xwebProperties.AuthKey = authKey;
        xwebProperties.PaymentTypeDefNum = paymentTypeDefNum;
        xwebProperties.IsPaymentsAllowed = isPaymentsAllowed;
    }

    public static void GetPayConnectPatPortalCreds(long clinicNum, out PayConnect.WebPaymentProperties payConnectProps)
    {
        //Secure arguments are held in the db.
        payConnectProps = new PayConnect.WebPaymentProperties();
        var programPayConnect = Programs.GetCur(ProgramName.PayConnect);
        if (programPayConnect == null) //PayConnect not setup.
            throw new ODException("PayConnect program link not found.", ODException.ErrorCodes.PayConnectProgramProperties);
        if (!programPayConnect.Enabled) //PayConnect not turned on.
            throw new ODException("PayConnect program link is disabled.", ODException.ErrorCodes.PayConnectProgramProperties);
        //Validate the online token, since it is requiored for PayConnect online payments to work.
        var listPayConnectProperties = GetListForProgramAndClinic(programPayConnect.ProgramNum, clinicNum);
        payConnectProps.Token = GetPropValFromList(listPayConnectProperties, PayConnect.ProgramProperties.PatientPortalPaymentsToken, clinicNum);
        payConnectProps.ProgramVersion = SIn.Int(GetPropValFromList(listPayConnectProperties, PayConnect.ProgramProperties.ProgramVersion, clinicNum));
        if (payConnectProps.ProgramVersion == 1 && string.IsNullOrEmpty(payConnectProps.Token))
            //PayConnect version 1 uses the Token property.
            throw new ODException("PayConnect online token not found.", ODException.ErrorCodes.PayConnectProgramProperties);
        var paymentsAllowedVal = GetPropValFromList(listPayConnectProperties, PayConnect.ProgramProperties.PatientPortalPaymentsEnabled, clinicNum);
        payConnectProps.IsPaymentsAllowed = SIn.Bool(paymentsAllowedVal);
    }

    public static ProgramProperty GetOnlinePaymentsEnabledForClinic(long clinicNum, ProgramName programName)
    {
        var xchargeOnlinePaymentEnabled = GetWhere(x =>
                x.ProgramNum == Programs.GetCur(ProgramName.Xcharge).ProgramNum
                && x.ClinicNum == clinicNum
                && x.PropertyDesc == "IsOnlinePaymentsEnabled")
            .FirstOrDefault();
        var edgeExpressOnlinePaymentEnabled = GetWhere(x =>
                x.ProgramNum == Programs.GetCur(ProgramName.EdgeExpress).ProgramNum
                && x.ClinicNum == clinicNum
                && x.PropertyDesc == EdgeExpressProps.IsOnlinePaymentsEnabled)
            .FirstOrDefault();
        var payConnectOnlinePaymentEnabled = GetWhere(x =>
                x.ProgramNum == Programs.GetCur(ProgramName.PayConnect).ProgramNum
                && x.ClinicNum == clinicNum
                && x.PropertyDesc == PayConnect.ProgramProperties.PatientPortalPaymentsEnabled)
            .FirstOrDefault();
        var paySimpleOnlinePaymentsEnabled = GetWhere(x =>
                x.ProgramNum == Programs.GetCur(ProgramName.PaySimple).ProgramNum
                && x.ClinicNum == clinicNum
                && x.PropertyDesc == PaySimple.PropertyDescs.PaySimpleIsOnlinePaymentsEnabled)
            .FirstOrDefault();
        var hasXchargeOnlinePaymentEnabled = xchargeOnlinePaymentEnabled != null && xchargeOnlinePaymentEnabled.PropertyValue == "1";
        var hasEdgeExpressOnlinePaymentEnabled = edgeExpressOnlinePaymentEnabled != null && edgeExpressOnlinePaymentEnabled.PropertyValue == "1";
        var hasPayConnectOnlinePaymentEnabled = payConnectOnlinePaymentEnabled != null && payConnectOnlinePaymentEnabled.PropertyValue == "1";
        var hasPaySimpleOnlinePaymentsEnabled = paySimpleOnlinePaymentsEnabled != null && paySimpleOnlinePaymentsEnabled.PropertyValue == "1";
        //If the program is not the one passed in and online payments are enabled, that is the one we'll return. There can only be one enabled per clinic.
        //Otherwise, there aren't any enabled.
        ProgramProperty programProperty = null;
        if (programName != ProgramName.EdgeExpress && hasEdgeExpressOnlinePaymentEnabled)
            programProperty = edgeExpressOnlinePaymentEnabled;
        else if (programName != ProgramName.PayConnect && hasPayConnectOnlinePaymentEnabled)
            programProperty = payConnectOnlinePaymentEnabled;
        else if (programName != ProgramName.Xcharge && hasXchargeOnlinePaymentEnabled)
            programProperty = xchargeOnlinePaymentEnabled;
        else if (programName != ProgramName.PaySimple && hasPaySimpleOnlinePaymentsEnabled) programProperty = paySimpleOnlinePaymentsEnabled;
        return programProperty;
    }

    public static List<string> GetQuickBooksOnlineEntityNames(string propertyValue)
    {
        var names = new List<string>();
        if (propertyValue.IsNullOrEmpty())
        {
            return names;
        }
        
        var entities = propertyValue.Split('|');
        
        names.AddRange(entities.Select(t => t.Split(',')[0]));
        names.Sort();
        
        return names;
    }

    public static bool CanEditProperties(List<ProgramProperty> properties, bool suppressMesssage = true)
    {
        return !properties.Any(x => x.IsHighSecurity) || Security.IsAuthorized(EnumPermType.ManageHighSecurityProgProperties, suppressMesssage);
    }

    public static void Delete(ProgramProperty prop)
    {
        if (!GetDeletablePropertyDescriptions().Contains(prop.PropertyDesc))
        {
            throw new Exception("Not allowed to delete the ProgramProperty with a description of: " + prop.PropertyDesc);
        }
        
        Db.NonQ("DELETE FROM programproperty WHERE ProgramPropertyNum=" + prop.ProgramPropertyNum);
    }

    private static List<string> GetDeletablePropertyDescriptions()
    {
        return
        [
            PropertyDescs.ClinicHideButton,
            PropertyDescs.DisableAdvertising,
            PropertyDescs.DisableAdvertisingHQ
        ];
    }

    public class PropertyDescs
    {
        public const string ImageFolder = "Image Folder";
        public const string PatOrChartNum = "Enter 0 to use PatientNum, or 1 to use ChartNum";
        public const string ClinicHideButton = "ClinicHideButton";
        public const string DisableAdvertising = "Disable Advertising";
        public const string DisableAdvertisingHQ = "Disable Advertising HQ";

        public static class TransWorld
        {
            public const string SyncExcludePosAdjType = "SyncExcludePosAdjType";
            public const string SyncExcludeNegAdjType = "SyncExcludeNegAdjType";
        }

        public static class XCharge
        {
            public const string XChargeForceRecurringCharge = "XChargeForceRecurringCharge";
            public const string XChargePreventSavingNewCC = "XChargePreventSavingNewCC";
        }

        public static class EdgeExpress
        {
            public const string ForceRecurringCharge = "EdgeExpressForceRecurringCharge";
            public const string PreventSavingNewCC = "EdgeExpressPreventSavingNewCC";
            public const string PromptSignature = "EdgeExpressPromptSignature";
            public const string PrintReceipt = "EdgeExpressPrintReceipt";
            public const string XWebID = "EdgeExpressXWebID";
            public const string AuthKey = "EdgeExpressAuthKey";
            public const string TerminalID = "EdgeExpressTerminalID";
            public const string PaymentType = "EdgeExpressPaymentType";
            public const string IsOnlinePaymentsEnabled = "EdgeExpressIsOnlinePaymentsEnabled";
        }
    }

    private class ProgramPropertyCache : CacheListAbs<ProgramProperty>
    {
        protected override List<ProgramProperty> GetCacheFromDb()
        {
            return ProgramPropertyCrud.SelectMany("SELECT * FROM programproperty");
        }

        protected override List<ProgramProperty> TableToList(DataTable dataTable)
        {
            return ProgramPropertyCrud.TableToList(dataTable);
        }

        protected override ProgramProperty Copy(ProgramProperty item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ProgramProperty> items)
        {
            return ProgramPropertyCrud.ListToTable(items, "ProgramProperty");
        }

        protected override void FillCacheIfNeeded()
        {
            ProgramProperties.GetTableFromCache(false);
        }
    }

    private static readonly ProgramPropertyCache Cache = new();

    public static ProgramProperty GetFirstOrDefault(Func<ProgramProperty, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static List<ProgramProperty> GetWhere(Predicate<ProgramProperty> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static void GetTableFromCache(bool refreshCache)
    {
        Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}