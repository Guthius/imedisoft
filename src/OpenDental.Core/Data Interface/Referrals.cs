using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDentBusiness;

public class Referrals
{
    public static void Update(Referral refer)
    {
        ReferralCrud.Update(refer);
    }

    public static void Insert(Referral refer)
    {
        ReferralCrud.Insert(refer);
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

    public static Referral GetFromList(long referralNum)
    {
        return GetFirstOrDefault(x => x.ReferralNum == referralNum);
    }

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

    public static Referral GetReferral(long referralNum)
    {
        if (referralNum == 0) return null;
        var referral = GetFirstOrDefault(x => x.ReferralNum == referralNum);
        if (referral == null) throw new ApplicationException("Error.  Referral not found: " + referralNum);
        return referral;
    }

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

    public static List<Referral> GetReferrals(List<long> listRefNums)
    {
        return GetWhere(x => listRefNums.Contains(x.ReferralNum));
    }

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

    public static int CountReferralAttach(long referralNum)
    {
        var command = "SELECT COUNT(*) FROM refattach "
                      + "WHERE ReferralNum=" + SOut.Long(referralNum);
        return SIn.Int(Db.GetCount(command));
    }

    public static bool IsSpecialtyInUse(long defNum)
    {
        var command = "SELECT COUNT(*) FROM referral WHERE Specialty=" + SOut.Long(defNum);
        if (Db.GetCount(command) == "0") return false;
        return true;
    }

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

    private static readonly ReferralCache Cache = new();

    public static bool GetExists(Predicate<Referral> match, bool isShort = false)
    {
        return Cache.GetExists(match, isShort);
    }

    public static List<Referral> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static List<Referral> GetWhere(Predicate<Referral> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static Referral GetFirstOrDefault(Func<Referral, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
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