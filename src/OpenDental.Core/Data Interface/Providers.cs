using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Providers
{
    #region Get Methods

    /// <summary>
    ///     Checks to see if the providers passed in have term dates that occur before the date passed in.
    ///     Returns a list of the ProvNums that have invalid term dates.  Otherwise; empty list.
    /// </summary>
    public static List<long> GetInvalidProvsByTermDate(List<long> listProvNums, DateTime dateCompare)
    {
        return GetWhere(x => listProvNums.Any(y => y == x.ProvNum) && x.DateTerm.Year > 1880 && x.DateTerm.Date < dateCompare.Date)
            .Select(x => x.ProvNum).ToList();
    }

    #endregion

    #region Misc Methods

    /// <summary>
    ///     Checks the appointment's provider and hygienist's term dates to see if an appointment should be scheduled or marked
    ///     complete.
    ///     Returns an empty string if the appointment does not violate the Term Date for the provider or hygienist.
    ///     A non-empty return value should be displayed to the user in a message box (already translated).
    ///     isSetComplete simply modifies the message. Use this when checking if an appointment should be set complete.
    /// </summary>
    public static string CheckApptProvidersTermDates(Appointment apt, bool isSetComplete = false)
    {
        var message = "";
        var listProvNums = new List<long> {apt.ProvNum, apt.ProvHyg};
        var listInvalidProvNums = GetInvalidProvsByTermDate(listProvNums, apt.AptDateTime);
        if (listInvalidProvNums.Count == 0) return message;
        if (listInvalidProvNums.Contains(apt.ProvNum)) message += "provider";
        if (listInvalidProvNums.Contains(apt.ProvHyg))
        {
            if (message != "") message += " and ";
            message += "hygienist";
        }

        if (listInvalidProvNums.Contains(apt.ProvNum) && listInvalidProvNums.Contains(apt.ProvHyg)) //used for grammar
            message = "The " + message + " selected for this appointment have Term Dates prior to the selected day and time. "
                      + "Please select another " + message + (isSetComplete ? " to set the appointment complete." : ".");
        else
            message = "The " + message + " selected for this appointment has a Term Date prior to the selected day and time. "
                      + "Please select another " + message + (isSetComplete ? " to set the appointment complete." : ".");
        Lans.g("Providers", message);
        return message;
    }

    #endregion

    ///<summary>Gets list of all providers from the database.  Returns an empty list if none are found.</summary>
    public static List<Provider> GetAll()
    {
        var command = "SELECT * FROM provider";
        return ProviderCrud.SelectMany(command);
    }

    
    public static void Update(Provider provider)
    {
        ProviderCrud.Update(provider);
    }

    
    public static long Insert(Provider provider)
    {
        return ProviderCrud.Insert(provider);
    }

    /// <summary>
    ///     This checks for the maximum number of provnum in the database and then returns the one directly after.  Not
    ///     guaranteed to be a unique primary key.
    /// </summary>
    public static long GetNextAvailableProvNum()
    {
        var command = "SELECT MAX(provNum) FROM provider";
        return SIn.Long(DataCore.GetScalar(command)) + 1;
    }

    /// <summary>
    ///     Increments all (privider.ItemOrder)s that are >= the ItemOrder of the provider passed in
    ///     but does not change the item order of the provider passed in.
    /// </summary>
    public static void MoveDownBelow(Provider provider)
    {
        //Add 1 to all item orders equal to or greater than new provider's item order
        Db.NonQ("UPDATE provider SET ItemOrder=ItemOrder+1"
                + " WHERE ProvNum!=" + provider.ProvNum
                + " AND ItemOrder>=" + provider.ItemOrder);
    }

    ///<summary>Only used from FormProvEdit if user clicks cancel before finishing entering a new provider.</summary>
    public static void Delete(Provider prov)
    {
        var command = "DELETE from provider WHERE provnum = '" + prov.ProvNum + "'";
        Db.NonQ(command);
    }

    ///<summary>Gets table for the FormProviderSetup window.  Always orders by ItemOrder.</summary>
    public static DataTable RefreshStandard(bool canShowPatCount)
    {
        //Max function used because some providers may have multiple user names.
        var command = "SELECT Abbr,LName,FName,provider.IsHidden,provider.ItemOrder,provider.ProvNum,MAX(UserName) UserName,ProvStatus,IsHiddenReport ";
        if (canShowPatCount) command += ",PatCountPri,PatCountSec ";
        command += "FROM provider "
                   + "LEFT JOIN userod ON userod.ProvNum=provider.ProvNum "; //there can be multiple userods attached to one provider
        if (canShowPatCount)
        {
            command += "LEFT JOIN (SELECT PriProv, COUNT(*) PatCountPri FROM patient "
                       + "WHERE patient.PatStatus!=" + SOut.Int((int) PatientStatus.Deleted) + " AND patient.PatStatus!=" + SOut.Int((int) PatientStatus.Deceased) + " "
                       + "GROUP BY PriProv) patPri ON provider.ProvNum=patPri.PriProv  ";
            command += "LEFT JOIN (SELECT SecProv,COUNT(*) PatCountSec FROM patient "
                       + "WHERE patient.PatStatus!=" + SOut.Int((int) PatientStatus.Deleted) + " AND patient.PatStatus!=" + SOut.Int((int) PatientStatus.Deceased) + " "
                       + "GROUP BY SecProv) patSec ON provider.ProvNum=patSec.SecProv ";
        }

        command += "GROUP BY Abbr,LName,FName,provider.IsHidden,provider.ItemOrder,provider.ProvNum,ProvStatus,IsHiddenReport ";
        if (canShowPatCount) command += ",PatCountPri,PatCountSec ";
        command += "ORDER BY ItemOrder";
        return DataCore.GetTable(command);
    }

    /// <summary>
    ///     Gets table for main provider edit list when in dental school mode.  Always orders alphabetically, but there
    ///     will be lots of filters to get the list shorter.  Must be very fast because refreshes while typing.  selectAll will
    ///     trump selectInstructors and always return all providers.
    /// </summary>
    public static DataTable RefreshForDentalSchool(long schoolClassNum, string lastName, string firstName, string provNum, bool selectInstructors, bool selectAll)
    {
        var command = "SELECT Abbr,LName,FName,provider.IsHidden,provider.ItemOrder,provider.ProvNum,GradYear,IsInstructor,Descript,"
                      + "MAX(UserName) UserName," //Max function used for Oracle compatability (some providers may have multiple user names).
                      + "PatCountPri,PatCountSec,ProvStatus,IsHiddenReport "
                      + "FROM provider LEFT JOIN schoolclass ON provider.SchoolClassNum=schoolclass.SchoolClassNum "
                      + "LEFT JOIN userod ON userod.ProvNum=provider.ProvNum " //there can be multiple userods attached to one provider
                      + "LEFT JOIN (SELECT PriProv, COUNT(*) PatCountPri FROM patient "
                      + "WHERE patient.PatStatus!=" + SOut.Int((int) PatientStatus.Deleted) + " AND patient.PatStatus!=" + SOut.Int((int) PatientStatus.Deceased) + " "
                      + "GROUP BY PriProv) pat ON provider.ProvNum=pat.PriProv ";
        command += "LEFT JOIN (SELECT SecProv,COUNT(*) PatCountSec FROM patient "
                   + "WHERE patient.PatStatus!=" + SOut.Int((int) PatientStatus.Deleted) + " AND patient.PatStatus!=" + SOut.Int((int) PatientStatus.Deceased) + " "
                   + "GROUP BY SecProv) patSec ON provider.ProvNum=patSec.SecProv ";
        command += "WHERE TRUE "; //This is here so that we can prevent nested if-statements
        if (schoolClassNum > 0) command += "AND provider.SchoolClassNum=" + SOut.Long(schoolClassNum) + " ";
        if (lastName != "") command += "AND provider.LName LIKE '%" + SOut.String(lastName) + "%' ";
        if (firstName != "") command += "AND provider.FName LIKE '%" + SOut.String(firstName) + "%' ";
        if (provNum != "") command += "AND provider.ProvNum LIKE '%" + SOut.String(provNum) + "%' ";
        if (!selectAll)
        {
            command += "AND provider.IsInstructor=" + SOut.Bool(selectInstructors) + " ";
            if (!selectInstructors) command += "AND provider.SchoolClassNum!=0 ";
        }

        command += "GROUP BY Abbr,LName,FName,provider.IsHidden,provider.ItemOrder,provider.ProvNum,GradYear,IsInstructor,Descript,PatCountPri,PatCountSec "
                   + "ORDER BY LName,FName";
        return DataCore.GetTable(command);
    }

    ///<summary>Gets list of all instructors.  Returns an empty list if none are found.</summary>
    public static List<Provider> GetInstructors()
    {
        return GetWhere(x => x.IsInstructor);
    }

    public static List<Provider> GetChangedSince(DateTime changedSince)
    {
        var command = "SELECT * FROM provider WHERE DateTStamp > " + SOut.DateT(changedSince);
        //DataTable table=DataCore.GetTable(command);
        //return TableToList(table);
        return ProviderCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets all providers changed since a certain DateTime, returning a list of providers with the server's current
    ///     DateTime.
    /// </summary>
    public static List<ProviderForApi> GetChangedSinceForApi(int limit, int offset, DateTime changedSince)
    {
        var command = "SELECT * FROM provider WHERE DateTStamp >= " + SOut.DateT(changedSince) + " ORDER BY provnum "
                      + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        var commandDateTime = "SELECT " + DbHelper.Now();
        var dateTimeServer = SIn.DateTime(DataCore.GetScalar(commandDateTime)); //run before providers for rigorous inclusion of providers
        var listProviders = ProviderCrud.TableToList(DataCore.GetTable(command));
        var listProviderForApis = new List<ProviderForApi>();
        for (var i = 0; i < listProviders.Count; i++)
        {
            var ProviderForApi = new ProviderForApi();
            ProviderForApi.ProviderCur = listProviders[i];
            ProviderForApi.DateTimeServer = dateTimeServer;
            listProviderForApis.Add(ProviderForApi);
        }

        return listProviderForApis;
    }

    
    public static string GetAbbr(long provNum, bool includeHidden = false)
    {
        var prov = _providerCache.GetFirstOrDefault(x => x.ProvNum == provNum);
        if (prov == null) return "";
        if (includeHidden) return prov.GetAbbr();
        return prov.Abbr; //keeping old behavior, just in case
    }

    
    public static string GetLName(long provNum, List<Provider> listProvs = null)
    {
        Provider provider;
        if (listProvs == null) //Use the cache.
            provider = GetLastOrDefault(x => x.ProvNum == provNum);
        else //Use the custom list passed in.
            provider = listProvs.LastOrDefault(x => x.ProvNum == provNum);
        return provider == null ? "" : provider.LName;
    }

    ///<summary>First Last, Suffix</summary>
    public static string GetFormalName(long provNum)
    {
        var provider = GetLastOrDefault(x => x.ProvNum == provNum);
        var retStr = "";
        if (provider != null)
        {
            retStr = provider.FName + " " + provider.LName;
            if (provider.Suffix != "") retStr += ", " + provider.Suffix;
        }

        return retStr;
    }

    ///<summary>Returns provider's preferred name.</summary>
    public static string GetPreferredName(long provNum)
    {
        var provider = GetLastOrDefault(x => x.ProvNum == provNum);
        var preferredName = "";
        if (provider != null) preferredName = provider.PreferredName;
        return preferredName;
    }

    ///<summary>Abbr - LName, FName (hidden).  For dental schools -- ProvNum - LName, FName (hidden).</summary>
    public static string GetLongDesc(long provNum)
    {
        var provider = GetFirstOrDefault(x => x.ProvNum == provNum);
        return provider == null ? "" : provider.GetLongDesc();
    }

    
    public static Color GetColor(long provNum)
    {
        var prov = _providerCache.GetFirstOrDefault(x => x.ProvNum == provNum);
        if (prov == null) return Color.White;
        if (prov.ProvColor.ToArgb() == Color.Transparent.ToArgb() || prov.ProvColor.ToArgb() == 0) return Color.White;
        return prov.ProvColor;
    }

    
    public static Color GetOutlineColor(long provNum)
    {
        var prov = _providerCache.GetFirstOrDefault(x => x.ProvNum == provNum);
        if (prov == null) return Color.Black;
        if (prov.OutlineColor.ToArgb() == Color.Transparent.ToArgb() || prov.OutlineColor.ToArgb() == 0) return Color.Black;
        return prov.OutlineColor;
    }

    
    public static bool GetIsSec(long provNum)
    {
        var prov = _providerCache.GetFirstOrDefault(x => x.ProvNum == provNum);
        if (prov == null) return false;
        return prov.IsSecondary;
    }

    ///<summary>Gets a provider from Cache.  If provnum is not valid, then it returns null.</summary>
    public static Provider GetProv(long provNum)
    {
        return _providerCache.GetFirstOrDefault(x => x.ProvNum == provNum);
    }

    ///<summary>Gets a provider from the DB. Returns null if not found. Required for API.</summary>
    public static Provider GetProvFromDb(long provNum)
    {
        if (provNum == 0) return null;

        return ProviderCrud.SelectOne(provNum);
    }

    ///<summary>Gets all providers that have the matching prov nums from ListLong.  Returns an empty list if no matches.</summary>
    public static List<Provider> GetProvsByProvNums(List<long> listProvNums, bool isShort = false)
    {
        return GetWhere(x => listProvNums.Contains(x.ProvNum), isShort);
    }

    /// <summary>
    ///     Gets a list of providers from ListLong.  If none found or if either LName or FName are an empty string,
    ///     returns an empty list.  There may be more than on provider with the same FName and LName so we will return a list
    ///     of all such providers.  Usually only one will exist with the FName and LName provided so list returned will have
    ///     count 0 or 1 normally.  Name match is not case sensitive.
    /// </summary>
    public static List<Provider> GetProvsByFLName(string lName, string fName)
    {
        if (string.IsNullOrWhiteSpace(lName) || string.IsNullOrWhiteSpace(fName)) return new List<Provider>();
        //GetListLong already returns a copy of the prov from the cache, no need to .Copy
        return GetWhere(x => x.LName.ToLower() == lName.ToLower() && x.FName.ToLower() == fName.ToLower());
    }

    /// <summary>
    ///     Gets a list of providers from ListLong with either the NPI provided or a blank NPI and the Medicaid ID provided.
    ///     medicaidId can be blank.  If the npi param is blank, or there are no matching provs, returns an empty list.
    ///     Shouldn't be two separate functions or we would have to compare the results of the two lists.
    /// </summary>
    public static List<Provider> GetProvsByNpiOrMedicaidId(string npi, string medicaidId)
    {
        var retval = new List<Provider>();
        if (npi == "") return retval;
        var listProvs = GetDeepCopy();
        for (var i = 0; i < listProvs.Count; i++)
            //if the prov has a NPI set and it's a match, add this prov to the list
            if (listProvs[i].NationalProvID != "")
            {
                if (listProvs[i].NationalProvID.Trim().ToLower() == npi.Trim().ToLower()) retval.Add(listProvs[i].Copy());
            }
            else
            {
                //if the NPI is blank and the Medicaid ID is set and it's a match, add this prov to the list
                if (listProvs[i].MedicaidID != ""
                    && listProvs[i].MedicaidID.Trim().ToLower() == medicaidId.Trim().ToLower())
                    retval.Add(listProvs[i].Copy());
            }

        return retval;
    }

    /// <summary>
    ///     Gets all providers associated to users that have a clinic set to the clinic passed in.  Passing in 0 will get
    ///     a list of providers not assigned to any clinic or to any users.
    /// </summary>
    public static List<Provider> GetProvsByClinic(long clinicNum)
    {
        var listProvsWithClinics = new List<Provider>();
        var listUsersShort = Userods.GetDeepCopy(true);
        for (var i = 0; i < listUsersShort.Count; i++)
        {
            var prov = GetProv(listUsersShort[i].ProvNum);
            if (prov == null) continue;
            var listUserClinics = UserClinics.GetForUser(listUsersShort[i].UserNum);
            //If filtering by a specific clinic, make sure the clinic matches the clinic passed in.
            //If the user is associated to multiple clinics we check to make sure one of them isn't the clinic in question.
            if (clinicNum > 0 && !listUserClinics.Exists(x => x.ClinicNum == clinicNum)) continue;
            if (listUsersShort[i].ClinicNum > 0) //User is associated to a clinic, add the provider to the list of provs with clinics.
                listProvsWithClinics.Add(prov);
        }

        if (clinicNum == 0)
        {
            //Return the list of providers without clinics.
            //We need to find all providers not associated to a clinic (via userod) and also include all providers not even associated to a user.
            //Since listProvsWithClinics is comprised of all providers associated to a clinic, simply loop through the provider cache and remove providers present in listProvsWithClinics.
            var listProvsUnassigned = GetDeepCopy(true);
            for (var i = listProvsUnassigned.Count - 1; i >= 0; i--)
            for (var j = 0; j < listProvsWithClinics.Count; j++)
                if (listProvsWithClinics[j].ProvNum == listProvsUnassigned[i].ProvNum)
                {
                    listProvsUnassigned.RemoveAt(i);
                    break;
                }

            return listProvsUnassigned;
        }

        return listProvsWithClinics;
    }

    ///<summary>Gets all providers from the database.  Doesn't use the cache.</summary>
    public static List<Provider> GetProvsNoCache()
    {
        var command = "SELECT * FROM provider";
        return ProviderCrud.SelectMany(command);
    }

    ///<summary>Gets a provider from the List.  If EcwID is not found, then it returns null.</summary>
    public static Provider GetProvByEcwID(string eID)
    {
        if (eID == "") return null;
        var provider = GetFirstOrDefault(x => x.EcwID == eID);
        if (provider != null) return provider;
        //If using eCW, a provider might have been added from the business layer.
        //The UI layer won't know about the addition.
        //So we need to refresh if we can't initially find the prov.
        RefreshCache();
        return GetFirstOrDefault(x => x.EcwID == eID);
    }

    ///<summary>Within the regular list of visible providers.  Will return -1 if the specified provider is not in the list.</summary>
    public static int GetIndex(long provNum)
    {
        return _providerCache.GetFindIndex(x => x.ProvNum == provNum, true);
    }

    public static List<Userod> GetAttachedUsers(long provNum)
    {
        var command = "SELECT userod.* FROM userod,provider "
                      + "WHERE userod.ProvNum=provider.ProvNum "
                      + "AND provider.provNum=" + SOut.Long(provNum);
        return UserodCrud.SelectMany(command);
    }

    /// <summary>
    ///     Returns the billing provnum to use based on practice/clinic settings.  Takes the treating provider provnum and
    ///     clinicnum.
    ///     If clinics are enabled and clinicnum is passed in, the clinic's billing provider will be used.  Otherwise will use
    ///     pactice defaults.
    ///     It will return a valid provNum unless the supplied treatProv was invalid.
    /// </summary>
    public static long GetBillingProvNum(long treatProv, long clinicNum)
    {
        if (clinicNum == 0 || !true)
        {
            //If clinics are disabled don't use the clinic defaults, even if a clinicnum was passed in.
            if (PrefC.GetLong(PrefName.InsBillingProv) == 0) //default=0
                return PrefC.GetLong(PrefName.PracticeDefaultProv);

            if (PrefC.GetLong(PrefName.InsBillingProv) == -1) //treat=-1
                return treatProv;

            return PrefC.GetLong(PrefName.InsBillingProv);
        } //Using clinics, and a clinic was pased in

        var clinicInsBillingProv = Clinics.GetClinic(clinicNum).BillingProviderId??0;
        if (clinicInsBillingProv == 0) //default=0
            return PrefC.GetLong(PrefName.PracticeDefaultProv);

        if (clinicInsBillingProv == -1) //treat=-1
            return treatProv;

        return clinicInsBillingProv;
    }

    /*
    ///<summary>Used when adding a provider to get the next available itemOrder.</summary>
    public static int GetNextItemOrder(){

        //Is this valid in Oracle??
        string command="SELECT MAX(ItemOrder) FROM provider";
        DataTable table=DataCore.GetTable(command);
        if(table.Rows.Count==0){
            return 0;
        }
        return PIn.Int(table.Rows[0][0].ToString())+1;
    }*/

    /// <summary>
    ///     Returns list of providers that are either not restricted to a clinic, or are restricted to the ClinicNum
    ///     provided. Excludes hidden provs.  Passing ClinicNum=0 returns all unrestricted providers. Ordered by
    ///     provider.ItemOrder.
    /// </summary>
    public static List<Provider> GetProvsForClinic(long clinicNum)
    {
        return GetProvsForClinicList(new List<long> {clinicNum});
    }

    /// <summary>
    ///     Returns list of providers that are either not restricted to a clinic, or are restricted to the list of ClinicNums
    ///     provided. Excludes hidden provs.
    ///     Considers user restricted clinics. Passing a ClinicNum=0 returns all unrestricted providers. Ordered by
    ///     provider.ItemOrder.
    /// </summary>
    public static List<Provider> GetProvsForClinicList(List<long> listClinicNums)
    {
        if (listClinicNums.IsNullOrEmpty()) return new List<Provider>();

        if (!true) return GetDeepCopy(true); //if clinics not enabled, return all visible providers.
        ProviderClinicLinks.RefreshCache();
        //Creates a dictionary of all UserNums specifically associated with a clinic and the list of each one's associated clinicNums 
        //The GetWhere uses a "UserClinicNum>-1" in its selection to behave as a "Where true" to retrieve everything from the cache 
        var dictUserClinicsReference = UserClinics.GetWhere(x => x.UserClinicNum > -1)
            .GroupBy(x => x.UserNum)
            .ToDictionary(x => x.Key, x => x.Select(y => y.ClinicNum) //kvp (UserNumsAssociatedWithClinics, listAssociatedClinicNums)
                .ToList());
        //Creates a dictionary of all UserNums and list of either each one's associated clinics (if in above dictionary) or an empty list
        var dictUserClinics = Userods.GetDeepCopy()
            .ToDictionary(x => x.UserNum, x => dictUserClinicsReference.ContainsKey(x.UserNum) ? dictUserClinicsReference[x.UserNum] : new List<long>()); //kvp (AllUserNums, listAssociatedClinicNumsIfAny)
        //Creates a dictionary of all ProvNums with each's list of associated UserNums (likely just one UserNum)
        var dictProvUsers = Userods.GetWhere(x => x.ProvNum > 0) //Where the user is a provider
            .GroupBy(x => x.ProvNum)
            .ToDictionary(x => x.Key, x => x.Select(y => y.UserNum) //kvp (provNum, listAssociatedUserNums)
                .ToList());
        //List of providers restricted to clinics not in listClinicNums
        var listProvsRestrictedOtherClinics = ProviderClinicLinks.GetProvsRestrictedToOtherClinics(listClinicNums);
        //Get providers that are not associated with any users OR a user not restricted to any clinic OR a provider associated to a user that is restricted to current clinic
        var listProviders = GetWhere(x =>
            !dictProvUsers.ContainsKey(x.ProvNum) //provider not associated to any users.
            || dictProvUsers[x.ProvNum].Any(y => dictUserClinics[y].Count == 0) //provider associated with user not restricted to any clinics
            || dictProvUsers[x.ProvNum].Any(y => dictUserClinics[y].Any(z => listClinicNums.Contains(z))), true); //provider associated to user restricted to clinic in listClinicNums
        //returns list of ProvNums from the above providers that are also not restricted to a clinic in listClinicNums
        return listProviders.Where(x => !listProvsRestrictedOtherClinics.Contains(x.ProvNum)).OrderBy(x => x.ItemOrder).ToList();
    }

    ///<summary>Returns a list of providers that have a CareCreditMerchantId override. Can return null.</summary>
    public static List<Provider> GetProvsWithCareCreditOverrides(long clinicNum = -1)
    {
        var listProvNumsWithOverrides = ProviderClinics.GetProvNumsWithCareCreditMerchantNums(clinicNum);
        if (listProvNumsWithOverrides.IsNullOrEmpty()) return null;
        return GetWhere(x => listProvNumsWithOverrides.Contains(x.ProvNum));
    }

    ///<Summary>Used once in the Provider Select window to warn user of duplicate Abbrs.</Summary>
    public static string GetDuplicateAbbrs()
    {
        var command = "SELECT Abbr FROM provider WHERE ProvStatus!=" + SOut.Int((int) ProviderStatus.Deleted);
        var listDuplicates = Db.GetListString(command).GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
        return string.Join(",", listDuplicates); //returns empty string when listDuplicates is empty
    }

    ///<summary>Returns the default practice provider. Returns null if there is no default practice provider set.</summary>
    public static Provider GetDefaultProvider()
    {
        return GetDefaultProvider(0);
    }

    /// <summary>
    ///     Returns the default provider for the clinic if it exists, else returns the default practice provider.
    ///     Pass 0 to get practice default.  Can return null if no clinic or practice default provider found.
    /// </summary>
    public static Provider GetDefaultProvider(long clinicNum)
    {
        var clinic = Clinics.GetClinic(clinicNum);
        Provider provider = null;
        if (clinic != null && clinic.DefaultProviderId.HasValue) //the clinic exists
            provider = GetProv(clinic.DefaultProviderId.Value);
        if (provider == null) //If not using clinics or if the specified clinic does not have a valid default provider set.
            provider = GetProv(PrefC.GetLong(PrefName.PracticeDefaultProv)); //Try to get the practice default.
        return provider;
    }

    public static DataTable GetDefaultPracticeProvider()
    {
        var command = @"SELECT FName,LName,Suffix,StateLicense
				FROM provider
        WHERE provnum=" + PrefC.GetString(PrefName.PracticeDefaultProv);
        return DataCore.GetTable(command);
    }

    /// <summary>
    ///     We should merge these results with GetDefaultPracticeProvider(), but
    ///     that would require restructuring indexes in different places in the code and this is
    ///     faster to do as we are just moving the queries down in to the business layer for now.
    /// </summary>
    public static DataTable GetDefaultPracticeProvider2()
    {
        var command = @"SELECT FName,LName,Specialty " +
                      "FROM provider WHERE provnum=" +
                      SOut.Long(PrefC.GetLong(PrefName.PracticeDefaultProv));
        //Convert.ToInt32(((Pref)PrefC.HList["PracticeDefaultProv"]).ValueString);
        return DataCore.GetTable(command);
    }

    /// <summary>
    ///     We should merge these results with GetDefaultPracticeProvider(), but
    ///     that would require restructuring indexes in different places in the code and this is
    ///     faster to do as we are just moving the queries down in to the business layer for now.
    /// </summary>
    public static DataTable GetDefaultPracticeProvider3()
    {
        var command = @"SELECT NationalProvID " +
                      "FROM provider WHERE provnum=" +
                      SOut.Long(PrefC.GetLong(PrefName.PracticeDefaultProv));
        return DataCore.GetTable(command);
    }

    public static DataTable GetPrimaryProviders(long PatNum)
    {
        var command = @"SELECT Fname,Lname from provider
                        WHERE provnum in (select priprov from 
                        patient where patnum = " + PatNum + ")";
        return DataCore.GetTable(command);
    }

    ///<summary>Returns the patient's last seen hygienist.  Returns null if no hygienist has been seen.</summary>
    public static Provider GetLastSeenHygienistForPat(long patNum)
    {
        //Look at all completed appointments and get the most recent secondary provider on it.
        var command = @"SELECT appointment.ProvHyg
				FROM appointment
				WHERE appointment.PatNum=" + SOut.Long(patNum) + @"
				AND appointment.ProvHyg!=0
				AND appointment.AptStatus=" + SOut.Int((int) ApptStatus.Complete) + @"
				ORDER BY AptDateTime DESC";
        var listPatHygNums = Db.GetListLong(command);
        //Now that we have all hygienists for this patient.  Lets find the last non-hidden hygienist and return that one.
        var listProviders = GetDeepCopy(true);
        var listProvNums = listProviders.Select(x => x.ProvNum).Distinct().ToList();
        var lastHygNum = listPatHygNums.FirstOrDefault(x => listProvNums.Contains(x));
        return listProviders.FirstOrDefault(x => x.ProvNum == lastHygNum);
    }

    ///<summary>Gets a list of providers based for the patient passed in based on the WebSchedProviderRule preference.</summary>
    public static List<Provider> GetProvidersForWebSched(long patNum, long clinicNum)
    {
        var listProviders = GetDeepCopy(true);
        var providerRule = SIn.Enum<WebSchedProviderRules>(
            ClinicPrefs.GetPref(PrefName.WebSchedProviderRule, clinicNum)?.ValueString ?? PrefC.GetString(PrefName.WebSchedProviderRule));
        switch (providerRule)
        {
            case WebSchedProviderRules.PrimaryProvider:
                var patPri = Patients.GetPat(patNum);
                var patPriProv = listProviders.Find(x => x.ProvNum == patPri.PriProv);
                if (patPriProv == null) throw new Exception(Lans.g("Providers", "Invalid primary provider set for patient."));
                return new List<Provider> {patPriProv};
            case WebSchedProviderRules.SecondaryProvider:
                var patSec = Patients.GetPat(patNum);
                var patSecProv = listProviders.Find(x => x.ProvNum == patSec.SecProv);
                if (patSecProv == null) throw new Exception(Lans.g("Providers", "No secondary provider set for patient."));
                return new List<Provider> {patSecProv};
            case WebSchedProviderRules.LastSeenHygienist:
                var lastHygProvider = GetLastSeenHygienistForPat(patNum);
                if (lastHygProvider == null) throw new Exception(Lans.g("Providers", "No last seen hygienist found for patient."));
                return new List<Provider> {lastHygProvider};
            case WebSchedProviderRules.FirstAvailable:
            default:
                return listProviders;
        }
    }

    ///<summary>Gets a list of providers that are allowed to have new patient appointments scheduled for them.</summary>
    public static List<Provider> GetProvidersForWebSchedNewPatAppt(long clinicNum = 0)
    {
        //Currently all providers are allowed to be considered for new patient appointments.
        //This follows the "WebSchedProviderRules.FirstAvailable" logic for recall Web Sched appointments which is what Nathan agreed upon.
        //This method is here so that we have a central location to go and get these types of providers in case we change this in the future.
        if (clinicNum == 0) return GetWhere(x => !x.IsNotPerson, true); //Make sure that we only return not is not persons.

        return GetProvsForClinic(clinicNum).Where(x => !x.IsNotPerson).ToList();
    }

    public static List<long> GetChangedSinceProvNums(DateTime changedSince)
    {
        var command = "SELECT ProvNum FROM provider WHERE DateTStamp > " + SOut.DateT(changedSince);
        var dt = DataCore.GetTable(command);
        var provnums = new List<long>(dt.Rows.Count);
        for (var i = 0; i < dt.Rows.Count; i++) provnums.Add(SIn.Long(dt.Rows[i]["ProvNum"].ToString()));
        return provnums;
    }

    ///<summary>Used along with GetChangedSinceProvNums</summary>
    public static List<Provider> GetMultProviders(List<long> provNums)
    {
        var provNumsDistinct = provNums.Distinct().ToList();
        if (provNumsDistinct.Count < 1) return new List<Provider>();
        var command = "";
        command = "SELECT * FROM provider WHERE ProvNum IN (" + string.Join(",", provNumsDistinct) + ")";
        return ProviderCrud.SelectMany(command);
    }

    /// <summary>
    ///     Currently only used for Dental Schools and will only return Providers.ListShort if Dental Schools is not
    ///     active.  Otherwise this will return a filtered provider list.
    /// </summary>
    public static List<Provider> GetFilteredProviderList(long provNum, string lName, string fName, long classNum)
    {
        var listProvs = GetDeepCopy(true);
        if (PrefC.GetBool(PrefName.EasyHideDentalSchools)) //This is here to save doing the logic below for users who have no way to filter the provider picker list.
            return listProvs;
        for (var i = listProvs.Count - 1; i >= 0; i--)
        {
            if (provNum != 0 && !listProvs[i].ProvNum.ToString().Contains(provNum.ToString()))
            {
                listProvs.Remove(listProvs[i]);
                continue;
            }

            if (!string.IsNullOrWhiteSpace(lName) && !listProvs[i].LName.Contains(lName))
            {
                listProvs.Remove(listProvs[i]);
                continue;
            }

            if (!string.IsNullOrWhiteSpace(fName) && !listProvs[i].FName.Contains(fName))
            {
                listProvs.Remove(listProvs[i]);
                continue;
            }

            if (classNum != 0 && classNum != listProvs[i].SchoolClassNum) listProvs.Remove(listProvs[i]);
        }

        return listProvs;
    }

    ///<summary>Returns a dictionary, with the key being ProvNum and the value being the production goal amount.</summary>
    public static decimal GetProductionGoalForProviders(List<long> listProvNums, List<long> listOpNums, DateTime start, DateTime end)
    {
        var dictProvSchedHrs = Schedules.GetHoursSchedForProvsInRange(listProvNums, listOpNums, start, end);
        decimal amt = 0;
        foreach (var kvp in dictProvSchedHrs)
        {
            var prov = GetProv(kvp.Key);
            if (prov != null) amt += (decimal) (kvp.Value * prov.HourlyProdGoalAmt);
        }

        return amt;
    }

    ///<summary>Removes a provider from the future schedule.  Currently called after a provider is hidden.</summary>
    public static void RemoveProvFromFutureSchedule(long provNum)
    {
        if (provNum < 1) //Invalid provNum, nothing to do.
            return;
        var provNums = new List<long>();
        provNums.Add(provNum);
        RemoveProvsFromFutureSchedule(provNums);
    }

    /// <summary>
    ///     Removes the providers from the future schedule.  Currently called from DBM to clean up hidden providers still
    ///     on the schedule.
    /// </summary>
    public static void RemoveProvsFromFutureSchedule(List<long> provNums)
    {
        var provs = "";
        for (var i = 0; i < provNums.Count; i++)
        {
            if (provNums[i] < 1) //Invalid provNum, nothing to do.
                continue;
            if (i > 0) provs += ",";
            provs += provNums[i].ToString();
        }

        if (provs == "") //No valid provNums were passed in.  Simply return.
            return;
        var command = "SELECT ScheduleNum FROM schedule WHERE ProvNum IN (" + provs + ") AND SchedDate > " + DbHelper.Now();
        var table = DataCore.GetTable(command);
        var listScheduleNums = new List<string>(); //Used for deleting scheduleops below
        for (var i = 0; i < table.Rows.Count; i++) listScheduleNums.Add(table.Rows[i]["ScheduleNum"].ToString());
        if (listScheduleNums.Count != 0)
        {
            command = "DELETE FROM scheduleop WHERE ScheduleNum IN(" + SOut.String(string.Join(",", listScheduleNums)) + ")";
            Db.NonQ(command);
        }

        command = "DELETE FROM schedule WHERE ProvNum IN (" + provs + ") AND SchedDate > " + DbHelper.Now();
        Db.NonQ(command);
    }

    public static bool IsAttachedToUser(long provNum)
    {
        var command = "SELECT COUNT(*) FROM userod,provider "
                      + "WHERE userod.ProvNum=provider.ProvNum "
                      + "AND provider.provNum=" + SOut.Long(provNum);
        var count = SIn.Int(Db.GetCount(command));
        if (count > 0) return true;
        return false;
    }

    ///<summary>Used to check if a specialty is in use when user is trying to hide it.</summary>
    public static bool IsSpecialtyInUse(long defNum)
    {
        var command = "SELECT COUNT(*) FROM provider WHERE Specialty=" + SOut.Long(defNum);
        if (Db.GetCount(command) == "0") return false;
        return true;
    }

    /// <summary>
    ///     Used to get a list of providers that are scheduled for today.
    ///     Pass in specific clinicNum for providers scheduled in specific clinic, clinicNum of -1 for all clinics
    /// </summary>
    public static List<Provider> GetProvsScheduledToday(long clinicNum = -1)
    {
        var listSchedulesForDate = Schedules.GetAllForDateAndType(DateTime.Today, ScheduleType.Provider);
        if (true && clinicNum >= 0) listSchedulesForDate.FindAll(x => x.ClinicNum == clinicNum);
        var listProvNums = listSchedulesForDate.Select(x => x.ProvNum).ToList();
        return GetMultProviders(listProvNums);
    }

    ///<summary>Provider merge tool.  Returns the number of rows changed when the tool is used.</summary>
    public static long Merge(long provNumFrom, long provNumInto)
    {
        var provNumForeignKeys = new[]
        {
            //add any new FKs to this list.
            "adjustment.ProvNum",
            "appointment.ProvNum",
            "appointment.ProvHyg",
            "apptviewitem.ProvNum",
            "claim.ProvTreat",
            "claim.ProvBill",
            "claim.ReferringProv",
            "claim.ProvOrderOverride",
            "claimproc.ProvNum",
            "clinic.DefaultProv",
            "clinic.InsBillingProv",
            "dispsupply.ProvNum",
            "ehrnotperformed.ProvNum",
            "emailmessage.ProvNumWebMail",
            "encounter.ProvNum",
            "equipment.ProvNumCheckedOut",
            "erxlog.ProvNum",
            "evaluation.InstructNum",
            "evaluation.StudentNum",
            "fee.ProvNum",
            "intervention.ProvNum",
            "labcase.ProvNum",
            "medicalorder.ProvNum",
            "medicationpat.ProvNum",
            "operatory.ProvDentist",
            "operatory.ProvHygienist",
            "orthocase.ProvNum",
            //"orthocharlog.ProvNum",
            "patient.PriProv",
            "patient.SecProv",
            "payplancharge.ProvNum",
            "paysplit.ProvNum",
            "perioexam.ProvNum",
            "proccodenote.ProvNum",
            "procedurecode.ProvNumDefault",
            "procedurelog.ProvNum",
            "procedurelog.ProvOrderOverride",
            "provider.ProvNumBillingOverride",
            //"providerclinic.ProvNum",
            "providerident.ProvNum",
            "refattach.ProvNum",
            "reqstudent.ProvNum",
            "reqstudent.InstructorNum",
            "rxpat.ProvNum",
            "schedule.ProvNum",
            "userod.ProvNum",
            "vaccinepat.ProvNumAdminister",
            "vaccinepat.ProvNumOrdering"
        };
        var command = "";
        long retVal = 0;
        for (var i = 0; i < provNumForeignKeys.Length; i++)
        {
            //actually change all of the FKs in the above tables.
            var tableAndKeyName = provNumForeignKeys[i].Split('.');
            command = "UPDATE " + tableAndKeyName[0]
                                + " SET " + tableAndKeyName[1] + "=" + SOut.Long(provNumInto)
                                + " WHERE " + tableAndKeyName[1] + "=" + SOut.Long(provNumFrom);
            retVal += Db.NonQ(command);
        }

        //Merge any providerclinic rows associated to the FROM provider where the INTO provider does not have a row for said clinic.
        var listProviderClinicsAll = ProviderClinics.GetByProvNums(new List<long> {provNumFrom, provNumInto});
        var listProviderClinicsFrom = listProviderClinicsAll.FindAll(x => x.ProvNum == provNumFrom);
        var listProviderClinicsInto = listProviderClinicsAll.FindAll(x => x.ProvNum == provNumInto);
        var listProviderClinicNums = listProviderClinicsFrom.Where(x => !listProviderClinicsInto.Select(y => y.ClinicNum).Contains(x.ClinicNum))
            .Select(x => x.ProviderClinicNum)
            .ToList();
        if (!listProviderClinicNums.IsNullOrEmpty())
        {
            command = $@"UPDATE providerclinic SET ProvNum = {SOut.Long(provNumInto)}
					WHERE ProviderClinicNum IN({string.Join(",", listProviderClinicNums.Select(x => SOut.Long(x)))})";
            Db.NonQ(command);
        }

        command = "UPDATE provider SET IsHidden=1 WHERE ProvNum=" + SOut.Long(provNumFrom);
        Db.NonQ(command);
        command = "UPDATE provider SET ProvStatus=" + SOut.Int((int) ProviderStatus.Deleted) + " WHERE ProvNum=" + SOut.Long(provNumFrom);
        Db.NonQ(command);
        return retVal;
    }

    public static long CountPats(long provNum)
    {
        var command = "SELECT COUNT(DISTINCT patient.PatNum) FROM patient WHERE (patient.PriProv=" + SOut.Long(provNum)
                                                                                                   + " OR patient.SecProv=" + SOut.Long(provNum) + ")"
                                                                                                   + " AND patient.PatStatus=0";
        var retVal = DataCore.GetScalar(command);
        return SIn.Long(retVal);
    }

    public static long CountClaims(long provNum)
    {
        var command = "SELECT COUNT(DISTINCT claim.ClaimNum) FROM claim WHERE claim.ProvBill=" + SOut.Long(provNum)
                                                                                               + " OR claim.ProvTreat=" + SOut.Long(provNum);
        var retVal = DataCore.GetScalar(command);
        return SIn.Long(retVal);
    }

    ///<summary>Gets the 0 provider for reports that display payments.</summary>
    public static Provider GetUnearnedProv()
    {
        return new Provider
        {
            ProvNum = 0,
            Abbr = Lans.g("Providers", "No Provider")
        };
    }

    ///<summary>Only for reports. Includes all providers where IsHiddenReport = 0 and ProvStatus != Deleted.</summary>
    public static List<Provider> GetListReports()
    {
        return GetWhere(x => !x.IsHiddenReport && x.ProvStatus != ProviderStatus.Deleted);
    }

    ///<summary>Only for reports. Includes all providers for a clinic where IsHiddenReport = 0 and ProvStatus != Deleted.</summary>
    public static List<Provider> GetListProvidersForClinic(long clinicNum)
    {
        return GetProvsForClinic(clinicNum).Where(x => !x.IsHiddenReport && x.ProvStatus != ProviderStatus.Deleted).ToList();
    }

    #region CachePattern

    private class ProviderCache : CacheListAbs<Provider>
    {
        protected override List<Provider> GetCacheFromDb()
        {
            var command = "SELECT * FROM provider";
            if (PrefC.GetBool(PrefName.EasyHideDentalSchools)) command += " ORDER BY ItemOrder";
            return ProviderCrud.SelectMany(command);
        }

        protected override List<Provider> TableToList(DataTable dataTable)
        {
            return ProviderCrud.TableToList(dataTable);
        }

        protected override Provider Copy(Provider item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<Provider> items)
        {
            return ProviderCrud.ListToTable(items, "Provider");
        }

        protected override void FillCacheIfNeeded()
        {
            Providers.GetTableFromCache(false);
        }

        protected override bool IsInListShort(Provider item)
        {
            return !item.IsHidden && item.ProvStatus != ProviderStatus.Deleted;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ProviderCache _providerCache = new();

    public static List<Provider> GetDeepCopy(bool isShort = false)
    {
        return _providerCache.GetDeepCopy(isShort);
    }

    public static bool GetExists(Predicate<Provider> match, bool isShort = false)
    {
        return _providerCache.GetExists(match, isShort);
    }

    public static int GetFindIndex(Predicate<Provider> match, bool isShort = false)
    {
        return _providerCache.GetFindIndex(match, isShort);
    }

    public static Provider GetFirst(bool isShort = false)
    {
        return _providerCache.GetFirst(isShort);
    }

    public static Provider GetFirst(Func<Provider, bool> match, bool isShort = false)
    {
        return _providerCache.GetFirst(match, isShort);
    }

    public static Provider GetFirstOrDefault(Func<Provider, bool> match, bool isShort = false)
    {
        return _providerCache.GetFirstOrDefault(match, isShort);
    }

    public static Provider GetLastOrDefault(Func<Provider, bool> match, bool isShort = false)
    {
        return _providerCache.GetLastOrDefault(match, isShort);
    }

    public static List<Provider> GetWhere(Predicate<Provider> match, bool isShort = false)
    {
        return _providerCache.GetWhere(match, isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _providerCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _providerCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _providerCache.ClearCache();
    }

    #endregion
}

public class ProviderForApi
{
    public DateTime DateTimeServer;
    public Provider ProviderCur;
}