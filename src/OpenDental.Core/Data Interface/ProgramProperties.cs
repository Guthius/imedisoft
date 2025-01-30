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

    public static bool UpdateProgramPropertyWithValue(ProgramProperty programProp, string newValue)
    {
        if (programProp.PropertyValue == newValue) return false;
        programProp.PropertyValue = newValue;
        Update(programProp);
        return true;
    }

    public static void Insert(ProgramProperty programProp)
    {
        ProgramPropertyCrud.Insert(programProp);
    }

    public static bool InsertForClinic(long programNum, List<long> listClinicNums)
    {
        if (listClinicNums == null || listClinicNums.Count == 0) return false;
        var hasInsert = false;
        var command = "";
        command = "INSERT INTO programproperty (ProgramNum,PropertyDesc,PropertyValue,ComputerName,ClinicNum) ";
        for (var i = 0; i < listClinicNums.Count; i++)
        {
            if (i > 0) command += " UNION ";
            command += "SELECT ProgramNum,PropertyDesc,PropertyValue,ComputerName," + SOut.Long(listClinicNums[i]) + " "
                       + "FROM programproperty "
                       + "WHERE ProgramNum=" + SOut.Long(programNum) + " "
                       + "AND ClinicNum=0";
        }

        hasInsert = Db.NonQ(command) > 0;
        return hasInsert;
    }

    public static bool IsAdvertisingDisabled(ProgramName progName)
    {
        var program = Programs.GetCur(progName);
        return IsAdvertisingDisabled(program);
    }

    public static bool IsAdvertisingDisabled(Program program)
    {
        if (program == null) return true;
        if (program.Enabled) return false; //do not block advertising
        return GetForProgram(program.ProgramNum).Any(x => (x.PropertyDesc == "Disable Advertising" && x.PropertyValue == "1") //Office has decided to hide the advertising
                                                          || (x.PropertyDesc == "Disable Advertising HQ" && x.PropertyValue == "1")); //HQ has decided to hide the advertising
    }

    public static bool IsAdvertisingBridge(long programNum)
    {
        return GetForProgram(programNum).Any(x => x.PropertyDesc.In("Disable Advertising", "Disable Advertising HQ"));
    }

    public static List<ProgramProperty> GetListForProgramAndClinic(long programNum, long clinicNum)
    {
        return GetWhere(x => x.ProgramNum == programNum && x.ClinicNum == clinicNum && x.PropertyDesc != "");
    }

    public static List<ProgramProperty> GetListForProgramAndClinicWithDefault(long programNum, long clinicNum)
    {
        var listClinicProperties = GetWhere(x => x.ProgramNum == programNum && x.ClinicNum == clinicNum);
        if (clinicNum == 0) return listClinicProperties; //return the defaults cause ClinicNum of 0 is default.
        //Get all the defaults and return a list of defaults mixed with overrides.
        var listClinicAndDefaultProperties = GetWhere(x => x.ProgramNum == programNum && x.ClinicNum == 0
                                                                                      && !listClinicProperties.Any(y => y.PropertyDesc == x.PropertyDesc));
        listClinicAndDefaultProperties.AddRange(listClinicProperties);
        return listClinicAndDefaultProperties; //Clinic users need to have all properties, defaults with the clinic overrides.
    }

    public static string GetPropValForClinicOrDefault(long programNum, string desc, long clinicNum)
    {
        return GetListForProgramAndClinicWithDefault(programNum, clinicNum).FirstOrDefault(x => x.PropertyDesc == desc).PropertyValue;
    }

    public static List<ProgramProperty> GetForProgram(long programNum)
    {
        return GetWhere(x => x.ProgramNum == programNum && x.PropertyDesc != "").OrderBy(x => x.ClinicNum).ThenBy(x => x.ProgramPropertyNum).ToList();
    }

    public static long SetProperty(long programNum, string desc, string propval)
    {
        var command = $@"UPDATE programproperty SET PropertyValue='{SOut.String(propval)}'
				WHERE ProgramNum={SOut.Long(programNum)}
				AND PropertyDesc='{SOut.String(desc)}'";
        return Db.NonQ(command);
    }

    public static ProgramProperty GetCur(List<ProgramProperty> listForProgram, string desc)
    {
        return listForProgram.FirstOrDefault(x => x.PropertyDesc == desc);
    }

    public static string GetPropVal(long programNum, string desc)
    {
        var programProperty = GetFirstOrDefault(x => x.ProgramNum == programNum && x.PropertyDesc == desc);
        if (programProperty != null) return programProperty.PropertyValue;
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

    public static string GetPropValFromList(List<ProgramProperty> listProps, string propertyDesc, long clinicNum = 0)
    {
        var retval = "";
        var prop = listProps.Where(x => x.ClinicNum == clinicNum).Where(x => x.PropertyDesc == propertyDesc).FirstOrDefault();
        if (prop != null) retval = prop.PropertyValue;
        return retval;
    }

    public static ProgramProperty GetPropByDesc(string propertyDesc, List<ProgramProperty> listProperties)
    {
        ProgramProperty property = null;
        for (var i = 0; i < listProperties.Count; i++)
            if (listProperties[i].PropertyDesc == propertyDesc)
            {
                property = listProperties[i];
                break;
            }

        return property;
    }

    public static ProgramProperty GetPropForProgByDesc(long programNum, string propertyDesc)
    {
        return GetForProgram(programNum).FirstOrDefault(x => x.PropertyDesc == propertyDesc);
    }

    public static ProgramProperty GetPropForProgByDesc(long programNum, string propertyDesc, long clinicNum = 0)
    {
        return GetForProgram(programNum).FirstOrDefault(x => x.PropertyDesc == propertyDesc && x.ClinicNum == clinicNum);
    }

    public static string GetValFromDb(long programNum, string desc)
    {
        var command = "SELECT PropertyValue FROM programproperty WHERE ProgramNum=" + SOut.Long(programNum)
                                                                                    + " AND PropertyDesc='" + SOut.String(desc) + "'";
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return "";
        return table.Rows[0][0].ToString();
    }

    public static string GetLocalPathOverrideForProgram(long programNum)
    {
        var programProperty = GetFirstOrDefault(x => x.ProgramNum == programNum
                                                     && x.PropertyDesc == ""
                                                     && x.ComputerName.ToUpper() == ODEnvironment.MachineName.ToUpper());
        return programProperty == null ? "" : programProperty.PropertyValue;
    }

    public static void InsertOrUpdateLocalOverridePath(long programNum, string newPath)
    {
        var programProperty = GetFirstOrDefault(x => x.ProgramNum == programNum
                                                     && x.PropertyDesc == ""
                                                     && x.ComputerName.ToUpper() == ODEnvironment.MachineName.ToUpper());
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
        pp.ComputerName = ODEnvironment.MachineName.ToUpper();
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

    public static void Sync(List<ProgramProperty> listProgPropsNew, long programNum, List<long> listClinicNums)
    {
        var listProgPropsDb = GetWhere(x => x.ProgramNum == programNum && x.PropertyDesc != "" && listClinicNums.Contains(x.ClinicNum));
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
        if (prog != null && prog.Enabled)
        {
            var listEdgeExpressProperties = GetListForProgramAndClinic(prog.ProgramNum, clinicNum);
            xWebID = GetPropValFromList(listEdgeExpressProperties, EdgeExpressProps.XWebID, clinicNum);
            authKey = GetPropValFromList(listEdgeExpressProperties, EdgeExpressProps.AuthKey, clinicNum);
            terminalID = GetPropValFromList(listEdgeExpressProperties, EdgeExpressProps.TerminalID, clinicNum);
            paymentTypeDefString = GetPropValFromList(listEdgeExpressProperties, EdgeExpressProps.PaymentType, clinicNum);
            isPaymentsAllowed = SIn.Bool(GetPropValFromList(listEdgeExpressProperties, EdgeExpressProps.IsOnlinePaymentsEnabled, clinicNum));
            isXWeb = false;
        }
        else
        {
            prog = Programs.GetCur(ProgramName.Xcharge);
            if (prog == null) throw new ODException("X-Charge program link not found.", ODException.ErrorCodes.XWebProgramProperties);
            if (!prog.Enabled) //EdgeExpress and XCharge not turned on.
                throw new ODException("EdgeExpress program link is disabled.", ODException.ErrorCodes.XWebProgramProperties);
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
        xwebProperties.IsXWeb = isXWeb;
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
        var listNames = new List<string>();
        if (propertyValue.IsNullOrEmpty()) return listNames;
        var arrayEntities = propertyValue.Split('|');
        for (var i = 0; i < arrayEntities.Length; i++) listNames.Add(arrayEntities[i].Split(',')[0]);
        listNames.Sort();
        return listNames;
    }

    public static bool CanEditProperties(List<ProgramProperty> listProperties, bool suppressMesssage = true)
    {
        if (listProperties.Any(x => x.IsHighSecurity)) return Security.IsAuthorized(EnumPermType.ManageHighSecurityProgProperties, suppressMesssage);
        return true;
    }

    public static void Delete(ProgramProperty prop)
    {
        if (!GetDeletablePropertyDescriptions().Contains(prop.PropertyDesc)) throw new Exception("Not allowed to delete the ProgramProperty with a description of: " + prop.PropertyDesc);
        var command = "DELETE FROM programproperty WHERE ProgramPropertyNum=" + SOut.Long(prop.ProgramPropertyNum);
        Db.NonQ(command);
    }

    private static List<string> GetDeletablePropertyDescriptions()
    {
        return new List<string>
        {
            PropertyDescs.ClinicHideButton,
            PropertyDescs.DisableAdvertising,
            PropertyDescs.DisableAdvertisingHQ
        };
    }

    public class PropertyDescs
    {
        public const string ImageFolder = "Image Folder";
        public const string PatOrChartNum = "Enter 0 to use PatientNum, or 1 to use ChartNum";
        public const string ClinicHideButton = "ClinicHideButton";
        public const string DisableAdvertising = "Disable Advertising";
        public const string DisableAdvertisingHQ = "Disable Advertising HQ";

        private PropertyDescs()
        {
        }

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
            var command = "SELECT * FROM programproperty";
            return ProgramPropertyCrud.SelectMany(command);
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

    public static ProgramProperty GetFirstOrDefault(Func<ProgramProperty, bool> match, bool isShort = false)
    {
        var prop = Cache.GetFirstOrDefault(match, isShort);
        if (prop is null) return prop;
        prop.PropertyValue = GetHqPropertyValue(Programs.GetProgram(prop.ProgramNum), prop);
        return prop;
    }

    public static List<ProgramProperty> GetWhere(Predicate<ProgramProperty> match, bool isShort = false)
    {
        var listProps = Cache.GetWhere(match, isShort);
        foreach (var prop in listProps) prop.PropertyValue = GetHqPropertyValue(Programs.GetProgram(prop.ProgramNum), prop);
        return listProps;
    }

    private static bool DoUseCacheValues(Program prog, ProgramProperty property)
    {
        //Is not an OD defined program name or is not a program HQ is concerned with enabling/disabling.
        return !HqProgram.IsInitialized()
               || !HqProgram.GetAll().Any(x => x.ProgramNameAsString == prog.ProgName && x.ListProperties.Any(y => y.PropertyDesc == property.PropertyDesc));
    }

    private static string GetHqPropertyValue(Program prog, ProgramProperty property)
    {
        var retVal = "";
        if (DoUseCacheValues(prog, property))
        {
            retVal = property.PropertyValue;
        }
        else
        {
            var hqProg = HqProgram.GetAll().Where(x => x.ProgramNameAsString == prog.ProgName).FirstOrDefault();
            var hqProp = hqProg.ListProperties.FirstOrDefault(x => x.PropertyDesc == property.PropertyDesc);
            retVal = hqProp.PropertyValue;
        }

        return retVal;
    }

    public static List<ProgramProperty> FilterProperties(Program progCur, List<ProgramProperty> listProps)
    {
        //If any other programs need to apply filtration, add it here.
        var listProgramProperty = PDMP.FilterAndSortProperties(progCur, listProps);
        return listProgramProperty;
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}