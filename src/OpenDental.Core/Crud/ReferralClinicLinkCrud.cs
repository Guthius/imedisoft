#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ReferralClinicLinkCrud
{
    public static ReferralClinicLink SelectOne(long referralClinicLinkNum)
    {
        var command = "SELECT * FROM referralcliniclink "
                      + "WHERE ReferralClinicLinkNum = " + SOut.Long(referralClinicLinkNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ReferralClinicLink SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

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

    public static DataTable ListToTable(List<ReferralClinicLink> listReferralClinicLinks, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ReferralClinicLink";
        var table = new DataTable(tableName);
        table.Columns.Add("ReferralClinicLinkNum");
        table.Columns.Add("ReferralNum");
        table.Columns.Add("ClinicNum");
        foreach (var referralClinicLink in listReferralClinicLinks)
            table.Rows.Add(SOut.Long(referralClinicLink.ReferralClinicLinkNum), SOut.Long(referralClinicLink.ReferralNum), SOut.Long(referralClinicLink.ClinicNum));
        return table;
    }

    public static long Insert(ReferralClinicLink referralClinicLink)
    {
        return Insert(referralClinicLink, false);
    }

    public static long Insert(ReferralClinicLink referralClinicLink, bool useExistingPK)
    {
        var command = "INSERT INTO referralcliniclink (";

        command += "ReferralNum,ClinicNum) VALUES(";

        command +=
            SOut.Long(referralClinicLink.ReferralNum) + ","
                                                      + SOut.Long(referralClinicLink.ClinicNum) + ")";
        {
            referralClinicLink.ReferralClinicLinkNum = Db.NonQ(command, true, "ReferralClinicLinkNum", "referralClinicLink");
        }
        return referralClinicLink.ReferralClinicLinkNum;
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

    public static long InsertNoCache(ReferralClinicLink referralClinicLink)
    {
        return InsertNoCache(referralClinicLink, false);
    }

    public static long InsertNoCache(ReferralClinicLink referralClinicLink, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO referralcliniclink (";
        if (isRandomKeys || useExistingPK) command += "ReferralClinicLinkNum,";
        command += "ReferralNum,ClinicNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(referralClinicLink.ReferralClinicLinkNum) + ",";
        command +=
            SOut.Long(referralClinicLink.ReferralNum) + ","
                                                      + SOut.Long(referralClinicLink.ClinicNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            referralClinicLink.ReferralClinicLinkNum = Db.NonQ(command, true, "ReferralClinicLinkNum", "referralClinicLink");
        return referralClinicLink.ReferralClinicLinkNum;
    }

    public static void Update(ReferralClinicLink referralClinicLink)
    {
        var command = "UPDATE referralcliniclink SET "
                      + "ReferralNum          =  " + SOut.Long(referralClinicLink.ReferralNum) + ", "
                      + "ClinicNum            =  " + SOut.Long(referralClinicLink.ClinicNum) + " "
                      + "WHERE ReferralClinicLinkNum = " + SOut.Long(referralClinicLink.ReferralClinicLinkNum);
        Db.NonQ(command);
    }

    public static bool Update(ReferralClinicLink referralClinicLink, ReferralClinicLink oldReferralClinicLink)
    {
        var command = "";
        if (referralClinicLink.ReferralNum != oldReferralClinicLink.ReferralNum)
        {
            if (command != "") command += ",";
            command += "ReferralNum = " + SOut.Long(referralClinicLink.ReferralNum) + "";
        }

        if (referralClinicLink.ClinicNum != oldReferralClinicLink.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(referralClinicLink.ClinicNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE referralcliniclink SET " + command
                                                   + " WHERE ReferralClinicLinkNum = " + SOut.Long(referralClinicLink.ReferralClinicLinkNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ReferralClinicLink referralClinicLink, ReferralClinicLink oldReferralClinicLink)
    {
        if (referralClinicLink.ReferralNum != oldReferralClinicLink.ReferralNum) return true;
        if (referralClinicLink.ClinicNum != oldReferralClinicLink.ClinicNum) return true;
        return false;
    }

    public static void Delete(long referralClinicLinkNum)
    {
        var command = "DELETE FROM referralcliniclink "
                      + "WHERE ReferralClinicLinkNum = " + SOut.Long(referralClinicLinkNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listReferralClinicLinkNums)
    {
        if (listReferralClinicLinkNums == null || listReferralClinicLinkNums.Count == 0) return;
        var command = "DELETE FROM referralcliniclink "
                      + "WHERE ReferralClinicLinkNum IN(" + string.Join(",", listReferralClinicLinkNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}