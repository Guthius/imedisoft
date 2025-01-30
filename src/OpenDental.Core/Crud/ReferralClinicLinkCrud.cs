using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ReferralClinicLinkCrud
{
    public static List<ReferralClinicLink> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ReferralClinicLink> TableToList(DataTable table)
    {
        var retVal = new List<ReferralClinicLink>();
        ReferralClinicLink referralClinicLink;
        foreach (DataRow row in table.Rows)
        {
            referralClinicLink = new ReferralClinicLink();
            referralClinicLink.ReferralClinicLinkNum = SIn.Long(row["ReferralClinicLinkNum"].ToString());
            referralClinicLink.ReferralNum = SIn.Long(row["ReferralNum"].ToString());
            referralClinicLink.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            retVal.Add(referralClinicLink);
        }

        return retVal;
    }

    public static void InsertMany(List<ReferralClinicLink> listReferralClinicLinks)
    {
        InsertMany(listReferralClinicLinks, false);
    }

    public static void InsertMany(List<ReferralClinicLink> listReferralClinicLinks, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listReferralClinicLinks.Count)
        {
            var referralClinicLink = listReferralClinicLinks[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO referralcliniclink (");
                if (useExistingPK) sbCommands.Append("ReferralClinicLinkNum,");
                sbCommands.Append("ReferralNum,ClinicNum) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(referralClinicLink.ReferralClinicLinkNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(referralClinicLink.ReferralNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(referralClinicLink.ClinicNum));
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
                if (index == listReferralClinicLinks.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }
}