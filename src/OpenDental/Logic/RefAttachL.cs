using System.Collections.Generic;
using Imedisoft.Core.Entities;
using WpfControls;

namespace OpenDental.Logic;

public class RefAttachL
{
    public static string GetReferringDr(List<RefAttach> refAttaches)
    {
        if (refAttaches.Count == 0)
        {
            return "";
        }

        if (refAttaches[0].RefType != ReferralType.RefFrom)
        {
            return "";
        }

        var referral = ReferralL.GetReferral(refAttaches[0].ReferralNum);
        if (referral is not {PatNum: 0})
        {
            return "";
        }

        var name = referral.FName + " " + referral.MName + " " + referral.LName;
        if (referral.Title != "")
        {
            name += ", " + referral.Title;
        }

        return name;
    }
}