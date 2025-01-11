using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Referrals
{
    
    public static void Update(Referral refer)
    {
        ReferralCrud.Update(refer);
    }

    
    public static long Insert(Referral refer)
    {
        return ReferralCrud.Insert(refer);
    }

    
    public static void Delete(Referral refer)
    {
        if (RefAttaches.IsReferralAttached(refer.ReferralNum)) throw new ApplicationException(Lans.g("FormReferralEdit", "Cannot delete Referral because it is attached to patients"));
        if (Claims.IsReferralAttached(refer.ReferralNum)) throw new ApplicationException(Lans.g("FormReferralEdit", "Cannot delete Referral because it is attached to claims"));
        if (Procedures.IsReferralAttached(refer.ReferralNum)) throw new ApplicationException(Lans.g("FormReferralEdit", "Cannot delete Referral because it is attached to procedures"));
        var command = "DELETE FROM referral "
                      + "WHERE ReferralNum = '" + SOut.Long(refer.ReferralNum) + "'";
        Db.NonQ(command);
    }

    ///<summary>Get all matching rows where input email is found in the Email column.</summary>
    public static List<Referral> GetEmailMatch(string email)
    {
        var command = "SELECT * FROM referral "
                      + "WHERE IsDoctor=1 AND UPPER(EMail) LIKE '%" + email.ToUpper() + "%'";
        return ReferralCrud.SelectMany(command);
    }

    public static Referral GetFromList(long referralNum)
    {
        return GetFirstOrDefault(x => x.ReferralNum == referralNum);
    }

    ///<summary>Includes title like DMD on the end.</summary>
    public static string GetNameLF(long referralNum)
    {
        if (referralNum == 0) return "";
        var referral = GetFromList(referralNum);
        if (referral == null) return "";
        var retVal = referral.LName;
        if (referral.FName != "") retVal += ", " + referral.FName;
        if (referral.MName != "") retVal += " " + referral.MName;
        if (referral.Title != "") retVal += ", " + referral.Title;
        //specialty seems to wordy to add here
        return retVal;
    }

    ///<summary>Includes title, such as DMD.</summary>
    public static string GetNameFL(long referralNum)
    {
        if (referralNum == 0) return "";
        var referral = GetFromList(referralNum);
        if (referral == null) return "";
        return referral.GetNameFL();
    }

    
    public static string GetPhone(long referralNum)
    {
        var referral = GetFirstOrDefault(x => x.ReferralNum == referralNum);
        if (referral != null)
        {
            if (referral.Telephone.Length == 10) return TelephoneNumbers.ReFormat(referral.Telephone);
            return referral.Telephone;
        }

        return "";
    }

    public static List<Referral> GetAllReferrals()
    {
        var command = "SELECT * FROM Referral";
        return ReferralCrud.SelectMany(command);
    }

    /// <summary>
    ///     Returns a list of Referrals with names similar to the supplied string.  Used in dropdown list from referral
    ///     field in FormPatientAddAll for faster entry.
    /// </summary>
    public static List<Referral> GetSimilarNames(string referralLName)
    {
        return GetWhere(x => x.LName.ToUpper().IndexOf(referralLName.ToUpper()) == 0);
    }

    /// <summary>
    ///     Used by API. Returns a single Referral with LName exactly matching the supplied string, or null if no match
    ///     found.
    /// </summary>
    public static Referral GetReferralByLName(string LName)
    {
        var command = "SELECT * FROM Referral WHERE LName LIKE '" + SOut.String(LName) + "'"; //matches regardless of case 
        return ReferralCrud.SelectOne(command);
    }

    /// <summary>
    ///     Gets Referral info from memory.  Does not make a call to the database unless needed.
    ///     Returns the true if the referral for the passed in referralNum could be found and sets the out parameter
    ///     accordingly.
    ///     Otherwise returns false and referral will be null.
    /// </summary>
    [Obsolete("Use GetReferral() and surround with try/catch")]
    public static bool TryGetReferral(long referralNum, out Referral referral)
    {
        referral = null;
        try
        {
            referral = GetReferral(referralNum);
        }
        catch
        {
        }

        return referral != null;
    }

    /// <summary>
    ///     Gets Referral info from memory.  Does not make a call to the database unless needed.
    ///     Returns the first referral matching the referralNum passed in, null if 0 is passed in, or throws an exception if no
    ///     match found.
    /// </summary>
    public static Referral GetReferral(long referralNum)
    {
        if (referralNum == 0) return null;
        var referral = GetFirstOrDefault(x => x.ReferralNum == referralNum);
        if (referral == null) throw new ApplicationException("Error.  Referral not found: " + referralNum);
        return referral;
    }

    ///<summary>Gets the first referral "from" for the given patient.  Will return null if no "from" found for patient.</summary>
    public static Referral GetReferralForPat(long patNum, List<RefAttach> listRefAttaches = null)
    {
        listRefAttaches = listRefAttaches ?? RefAttaches.Refresh(patNum);
        for (var i = 0; i < listRefAttaches.Count; i++)
            if (listRefAttaches[i].RefType == ReferralType.RefFrom)
            {
                Referral referral;
                if (TryGetReferral(listRefAttaches[i].ReferralNum, out referral)) return referral;
            }

        return null;
    }

    ///<summary>Gets a referral from the database.</summary>
    public static Referral GetReferralForApi(long referralNum)
    {
        var command = "SELECT * FROM referral WHERE ReferralNum='" + SOut.Long(referralNum) + "'";
        return ReferralCrud.SelectOne(command);
    }

    ///<summary>Gets all Referrals from the database. Returns empty list if not found.</summary>
    public static List<Referral> GetReferralsForApi(int limit, int offset, bool isHidden, bool notPerson, bool isDoctor, bool isPreferred, bool isPatient)
    {
        var command = "SELECT * FROM referral WHERE DateTStamp>=" + SOut.DateT(DateTime.MinValue) + " ";
        if (isHidden) command += "AND IsHidden=1 ";
        if (notPerson) command += "AND NotPerson=1 ";
        if (isDoctor) command += "AND IsDoctor=1 ";
        if (isPreferred) command += "AND IsPreferred=1 ";
        if (isPatient) command += "AND PatNum>0 ";
        command += "ORDER BY referralNum " //same fixed order each time
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return ReferralCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets IsDoctors referred "from" referrals for the given patient.  Will return empty list if no "from" and
    ///     IsDoctor found for patient.
    /// </summary>
    public static List<Referral> GetIsDoctorReferralsForPat(long patNum, List<RefAttach> listRefAttaches = null)
    {
        var retVal = new List<Referral>();
        listRefAttaches = listRefAttaches ?? RefAttaches.Refresh(patNum);
        for (var i = 0; i < listRefAttaches.Count; i++)
            if (listRefAttaches[i].RefType == ReferralType.RefFrom)
            {
                Referral referral;
                if (TryGetReferral(listRefAttaches[i].ReferralNum, out referral) && referral.IsDoctor) retVal.Add(referral);
            }

        return retVal;
    }

    /// <summary>
    ///     Replaces all patient's referral "From" and "IsDoctor" fields in the given message.  Returns the resulting string.
    ///     Replaces: [ReferredFromProvInitialReferralNum], [ReferredFromProvInitialNameF],etc.
    /// </summary>
    public static string ReplaceRefProvider(string message, Patient pat)
    {
        if (pat == null) return message;
        var listRefFrom = GetIsDoctorReferralsForPat(pat.PatNum);
        if (listRefFrom.Count == 0) return message;
        var retVal = message;
        //The oldest referral 'From".
        var refOldest = listRefFrom.FirstOrDefault();
        retVal = retVal.Replace("[ReferredFromProvInitialReferralNum]", refOldest.ReferralNum.ToString());
        retVal = retVal.Replace("[ReferredFromProvInitialNameF]", refOldest.FName);
        retVal = retVal.Replace("[ReferredFromProvInitialNameL]", refOldest.LName);
        retVal = retVal.Replace("[ReferredFromProvInitialPhone]", refOldest.Telephone);
        retVal = retVal.Replace("[ReferredFromProvInitialAddress]", refOldest.Address);
        retVal = retVal.Replace("[ReferredFromProvInitialAddress2]", refOldest.Address2);
        retVal = retVal.Replace("[ReferredFromProvInitialCity]", refOldest.City);
        retVal = retVal.Replace("[ReferredFromProvInitialState]", refOldest.ST);
        retVal = retVal.Replace("[ReferredFromProvInitialZip]", refOldest.Zip);
        //The most recent referral "From".
        var refNewest = listRefFrom.LastOrDefault();
        retVal = retVal.Replace("[ReferredFromProvMostRecentReferralNum]", refNewest.ReferralNum.ToString());
        retVal = retVal.Replace("[ReferredFromProvMostRecentNameF]", refNewest.FName);
        retVal = retVal.Replace("[ReferredFromProvMostRecentNameL]", refNewest.LName);
        retVal = retVal.Replace("[ReferredFromProvMostRecentPhone]", refNewest.Telephone);
        retVal = retVal.Replace("[ReferredFromProvMostRecentAddress]", refNewest.Address);
        retVal = retVal.Replace("[ReferredFromProvMostRecentAddress2]", refNewest.Address2);
        retVal = retVal.Replace("[ReferredFromProvMostRecentCity]", refNewest.City);
        retVal = retVal.Replace("[ReferredFromProvMostRecentState]", refNewest.ST);
        retVal = retVal.Replace("[ReferredFromProvMostRecentZip]", refNewest.Zip);
        return retVal;
    }

    ///<summary>Gets all referrals by RefNum.  Returns an empty list if no matches.</summary>
    public static List<Referral> GetReferrals(List<long> listRefNums)
    {
        return GetWhere(x => listRefNums.Contains(x.ReferralNum));
    }

    /// <summary>
    ///     Gets the referral information string that is displayed on the Patient Edit window. Returns a 'Result' object. Pass
    ///     in textbox if you want this method to
    ///     abbreviate the generated string so that it fits within certain bounds. If there was an error generating the string,
    ///     returns the result object with the 'IsSuccess' flag
    ///     set to false, and possibly with an error message as well. If successful, then we return the result object with the
    ///     'IsSuccess' flag set to true, and with the
    ///     full string in Result.Msg and the abbreviated string in Result.Msg2.
    /// </summary>
    public static Result GetReferralText(long patNum, TextBox textBox = null)
    {
        var result = new Result {IsSuccess = false};
        var listRefAttaches = RefAttaches.Refresh(patNum);
        var firstRefNameTypeAbbr = "";
        var firstRefType = "";
        var firstRefFullName = "";
        var refAttach = listRefAttaches.FirstOrDefault(x => x.RefType == ReferralType.RefFrom);
        if (refAttach == null) return result;
        Referral referral = null;
        try
        {
            referral = GetReferral(refAttach.ReferralNum);
        }
        catch (ApplicationException appEx)
        {
            result.Msg = Lans.g("Referrals", "Could not retrieve referral. Please run Database Maintenance or call support.");
            return result;
        }

        firstRefFullName = GetNameLF(referral.ReferralNum);
        if (referral.PatNum > 0)
            firstRefType = " (patient)";
        else if (referral.IsDoctor) firstRefType = " (doctor)";
        var suffix = "";
        if (listRefAttaches.Count(x => x.RefType == ReferralType.RefFrom) > 1) suffix = " (+" + (listRefAttaches.Count(x => x.RefType == ReferralType.RefFrom) - 1) + " more)";
        if (textBox != null)
        {
            firstRefNameTypeAbbr = firstRefFullName;
            for (var i = 1; i < firstRefFullName.Length + 1; i++)
            {
                //i is used as the length to substring, not an index, so i<firstRefName.Length+1 is safe
                if (TextRenderer.MeasureText(firstRefFullName.Substring(0, i) + firstRefType + suffix, textBox.Font).Width < textBox.Width) continue;
                firstRefNameTypeAbbr = firstRefFullName.Substring(0, i - 1);
                break;
            }

            firstRefNameTypeAbbr += firstRefType + suffix; //both firstRefType and suffix could be blank, but they will show regardless of the length of firstRefName
            //Example: Schmidt, John Jacob Jingleheimer, DDS (doctor) (+5 more) 
            //might be shortened to : Schmidt, John Jaco (doctor) (+5 more) 
        }
        else
        {
            firstRefNameTypeAbbr = firstRefFullName + firstRefType + suffix;
        }

        result.IsSuccess = true;
        result.Msg = firstRefFullName + firstRefType + suffix; //Full string
        result.Msg2 = firstRefNameTypeAbbr; //Possibly abbreviated string
        return result;
    }

    ///<summary>Merges two referrals into a single referral. Returns false if both referrals are the same.</summary>
    public static bool MergeReferrals(long refNumInto, long refNumFrom)
    {
        if (refNumInto == refNumFrom)
            //Do not merge the same referral onto itself.
            return false;
        var command = "UPDATE claim "
                      + "SET ReferringProv=" + SOut.Long(refNumInto) + " "
                      + "WHERE ReferringProv=" + SOut.Long(refNumFrom);
        Db.NonQ(command);
        command = "UPDATE refattach "
                  + "SET ReferralNum=" + SOut.Long(refNumInto) + " "
                  + "WHERE ReferralNum=" + SOut.Long(refNumFrom);
        Db.NonQ(command);
        command = "DELETE FROM referralcliniclink "
                  + "WHERE ReferralNum=" + SOut.Long(refNumFrom);
        Db.NonQ(command);
        ReferralCrud.Delete(refNumFrom);
        return true;
    }

    ///<summary>Returns the number of refattaches that this referral has.</summary>
    public static int CountReferralAttach(long referralNum)
    {
        var command = "SELECT COUNT(*) FROM refattach "
                      + "WHERE ReferralNum=" + SOut.Long(referralNum);
        return SIn.Int(Db.GetCount(command));
    }

    ///<summary>Used to check if a specialty is in use when user is trying to hide it.</summary>
    public static bool IsSpecialtyInUse(long defNum)
    {
        var command = "SELECT COUNT(*) FROM referral WHERE Specialty=" + SOut.Long(defNum);
        if (Db.GetCount(command) == "0") return false;
        return true;
    }

    /// <summary>
    ///     When importing referrals from forms, we attach them to a referral with LName=Other and FName empty. This
    ///     single referral gets reused by any import. This method gets that referral, inserting it if it does not yet exist.
    /// </summary>
    public static Referral GetOther()
    {
        var referral = GetFirstOrDefault(x => x.LName == "Other" && x.FName == "");
        if (referral != null) return referral;
        referral = new Referral();
        referral.LName = "Other";
        Insert(referral);
        Signalods.SetInvalid(InvalidType.Referral);
        var listClinicNums = Clinics.GetDeepCopy().Select(x => x.Id).ToList();
        ReferralClinicLinks.InsertClinicLinksForReferral(referral.ReferralNum, listClinicNums);
        RefreshCache();
        return referral;
    }

    #region CachePattern

    private class ReferralCache : CacheListAbs<Referral>
    {
        protected override List<Referral> GetCacheFromDb()
        {
            var command = "SELECT * FROM referral ORDER BY LName";
            return ReferralCrud.SelectMany(command);
        }

        protected override List<Referral> TableToList(DataTable dataTable)
        {
            return ReferralCrud.TableToList(dataTable);
        }

        protected override Referral Copy(Referral item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<Referral> items)
        {
            return ReferralCrud.ListToTable(items, "Referral");
        }

        protected override void FillCacheIfNeeded()
        {
            Referrals.GetTableFromCache(false);
        }

        protected override bool IsInListShort(Referral item)
        {
            return !item.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ReferralCache _referralCache = new();

    public static bool GetExists(Predicate<Referral> match, bool isShort = false)
    {
        return _referralCache.GetExists(match, isShort);
    }

    public static List<Referral> GetDeepCopy(bool isShort = false)
    {
        return _referralCache.GetDeepCopy(isShort);
    }

    public static List<Referral> GetWhere(Predicate<Referral> match, bool isShort = false)
    {
        return _referralCache.GetWhere(match, isShort);
    }

    public static Referral GetFirstOrDefault(Func<Referral, bool> match, bool isShort = false)
    {
        return _referralCache.GetFirstOrDefault(match, isShort);
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
        _referralCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _referralCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _referralCache.ClearCache();
    }

    #endregion
}