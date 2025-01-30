using System.Collections.Generic;
using System.Linq;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class ReferralClinicLinks
{
    public static void InsertClinicLinksForReferral(long referralNum, List<long> clinicNums, bool deleteOldLinks = false)
    {
        if (clinicNums is not {Count: > 0})
        {
            return;
        }

        if (deleteOldLinks)
        {
            Db.NonQ("DELETE FROM referralcliniclink WHERE ReferralNum = " + referralNum);
        }

        clinicNums.RemoveAll(x => x == 0);

        var refClinicLinks = clinicNums
            .Select(x => new ReferralClinicLink {ClinicNum = x, ReferralNum = referralNum})
            .ToList();

        ReferralClinicLinkCrud.InsertMany(refClinicLinks);
    }

    public static List<ReferralClinicLink> GetAllForClinic(long clinicNum)
    {
        return ReferralClinicLinkCrud.SelectMany("SELECT * FROM referralcliniclink WHERE ClinicNum = " + clinicNum);
    }

    public static List<ReferralClinicLink> GetAllForReferral(long referralNum)
    {
        return ReferralClinicLinkCrud.SelectMany("SELECT * FROM referralcliniclink WHERE ReferralNum = " + referralNum);
    }

    public static List<long> GetReferralNumsWithLinks()
    {
        return Db.GetListLong("SELECT DISTINCT ReferralNum FROM referralcliniclink");
    }
}