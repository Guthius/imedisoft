using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class DefLinkCrud
{
    public static List<DefLink> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DefLink> TableToList(DataTable table)
    {
        var retVal = new List<DefLink>();
        foreach (DataRow row in table.Rows)
        {
            var defLink = new DefLink
            {
                DefLinkNum = SIn.Long(row["DefLinkNum"].ToString()),
                DefNum = SIn.Long(row["DefNum"].ToString()),
                FKey = SIn.Long(row["FKey"].ToString()),
                LinkType = (DefLinkType) SIn.Int(row["LinkType"].ToString())
            };
            retVal.Add(defLink);
        }

        return retVal;
    }

    public static void Insert(DefLink defLink)
    {
        var command = "INSERT INTO deflink (";

        command += "DefNum,FKey,LinkType) VALUES(";

        command +=
            SOut.Long(defLink.DefNum) + ","
                                      + SOut.Long(defLink.FKey) + ","
                                      + SOut.Int((int) defLink.LinkType) + ")";
        {
            defLink.DefLinkNum = Db.NonQ(command, true, "DefLinkNum", "defLink");
        }
    }

    public static void InsertMany(List<DefLink> listDefLinks)
    {
        InsertMany(listDefLinks, false);
    }

    public static void InsertMany(List<DefLink> listDefLinks, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listDefLinks.Count)
        {
            var defLink = listDefLinks[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO deflink (");
                if (useExistingPK) sbCommands.Append("DefLinkNum,");
                sbCommands.Append("DefNum,FKey,LinkType) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(defLink.DefLinkNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(defLink.DefNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(defLink.FKey));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) defLink.LinkType));
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
                if (index == listDefLinks.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static void Update(DefLink defLink)
    {
        var command = "UPDATE deflink SET "
                      + "DefNum    =  " + SOut.Long(defLink.DefNum) + ", "
                      + "FKey      =  " + SOut.Long(defLink.FKey) + ", "
                      + "LinkType  =  " + SOut.Int((int) defLink.LinkType) + " "
                      + "WHERE DefLinkNum = " + SOut.Long(defLink.DefLinkNum);
        Db.NonQ(command);
    }

    public static void Delete(long defLinkNum)
    {
        var command = "DELETE FROM deflink "
                      + "WHERE DefLinkNum = " + SOut.Long(defLinkNum);
        Db.NonQ(command);
    }
}