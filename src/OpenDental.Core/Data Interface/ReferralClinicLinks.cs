using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ReferralClinicLinks
{
    #region Methods - Modify

    /// <summary>
    ///     Creates new referral links for the clinics passed in. Optionally delete all existing links for the referral
    ///     passed in.
    /// </summary>
    public static void InsertClinicLinksForReferral(long referralNum, List<long> listClinicNums, bool doDeleteOldLinks = false)
    {
        if (listClinicNums.IsNullOrEmpty()) return;

        if (doDeleteOldLinks)
        {
            var command = "DELETE FROM referralcliniclink WHERE ReferralNum=" + SOut.Long(referralNum);
            Db.NonQ(command);
        }

        listClinicNums.RemoveAll(x => x == 0);
        var listRefClinicLinks = listClinicNums.Select(x => new ReferralClinicLink {ClinicNum = x, ReferralNum = referralNum}).ToList();
        ReferralClinicLinkCrud.InsertMany(listRefClinicLinks);
    }

    
    //public static long Insert(ReferralClinicLink referralLink) {
    //	
    //	return Crud.ReferralClinicLinkCrud.Insert(referralLink);
    //}

    //
    //public static void Update(ReferralClinicLink referralClinicLink){
    //	
    //	Crud.ReferralClinicLinkCrud.Update(referralClinicLink);
    //}

    //
    //public static void Delete(long referralClinicLinkNum) {
    //	
    //	Crud.ReferralClinicLinkCrud.Delete(referralClinicLinkNum);
    //}

    //
    //public static void DeleteAllForReferral(List<long> listReferralClinicLinkNums) {
    //	
    //	Crud.ReferralClinicLinkCrud.DeleteMany(listReferralClinicLinkNums);
    //}

    #endregion Methods - Modify

    #region Methods - Get

    public static List<ReferralClinicLink> GetAllForClinic(long clinicNum)
    {
        var command = "SELECT * FROM referralcliniclink WHERE ClinicNum=" + SOut.Long(clinicNum);
        return ReferralClinicLinkCrud.SelectMany(command);
    }

    public static List<ReferralClinicLink> GetAllForReferral(long referralNum)
    {
        var command = "SELECT * FROM referralcliniclink WHERE ReferralNum=" + SOut.Long(referralNum);
        return ReferralClinicLinkCrud.SelectMany(command);
    }

    public static List<long> GetReferralNumsWithLinks()
    {
        var command = "SELECT DISTINCT ReferralNum FROM referralcliniclink";
        return Db.GetListLong(command);
    }

    #endregion Methods - Get
}