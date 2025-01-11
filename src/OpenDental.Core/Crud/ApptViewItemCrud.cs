using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ApptViewItemCrud
{
    public static List<ApptViewItem> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ApptViewItem> TableToList(DataTable table)
    {
        var retVal = new List<ApptViewItem>();
        ApptViewItem apptViewItem;
        foreach (DataRow row in table.Rows)
        {
            apptViewItem = new ApptViewItem();
            apptViewItem.ApptViewItemNum = SIn.Long(row["ApptViewItemNum"].ToString());
            apptViewItem.ApptViewNum = SIn.Long(row["ApptViewNum"].ToString());
            apptViewItem.OpNum = SIn.Long(row["OpNum"].ToString());
            apptViewItem.ProvNum = SIn.Long(row["ProvNum"].ToString());
            apptViewItem.ElementDesc = SIn.String(row["ElementDesc"].ToString());
            apptViewItem.ElementOrder = SIn.Byte(row["ElementOrder"].ToString());
            apptViewItem.ElementColor = Color.FromArgb(SIn.Int(row["ElementColor"].ToString()));
            apptViewItem.ElementAlignment = (ApptViewAlignment) SIn.Int(row["ElementAlignment"].ToString());
            apptViewItem.ApptFieldDefNum = SIn.Long(row["ApptFieldDefNum"].ToString());
            apptViewItem.PatFieldDefNum = SIn.Long(row["PatFieldDefNum"].ToString());
            apptViewItem.IsMobile = SIn.Bool(row["IsMobile"].ToString());
            retVal.Add(apptViewItem);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ApptViewItem> listApptViewItems, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ApptViewItem";
        var table = new DataTable(tableName);
        table.Columns.Add("ApptViewItemNum");
        table.Columns.Add("ApptViewNum");
        table.Columns.Add("OpNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("ElementDesc");
        table.Columns.Add("ElementOrder");
        table.Columns.Add("ElementColor");
        table.Columns.Add("ElementAlignment");
        table.Columns.Add("ApptFieldDefNum");
        table.Columns.Add("PatFieldDefNum");
        table.Columns.Add("IsMobile");
        foreach (var apptViewItem in listApptViewItems)
            table.Rows.Add(SOut.Long(apptViewItem.ApptViewItemNum), SOut.Long(apptViewItem.ApptViewNum), SOut.Long(apptViewItem.OpNum), SOut.Long(apptViewItem.ProvNum), apptViewItem.ElementDesc, SOut.Byte(apptViewItem.ElementOrder), SOut.Int(apptViewItem.ElementColor.ToArgb()), SOut.Int((int) apptViewItem.ElementAlignment), SOut.Long(apptViewItem.ApptFieldDefNum), SOut.Long(apptViewItem.PatFieldDefNum), SOut.Bool(apptViewItem.IsMobile));
        return table;
    }

    public static void Insert(ApptViewItem apptViewItem)
    {
        var command = "INSERT INTO apptviewitem (";

        command += "ApptViewNum,OpNum,ProvNum,ElementDesc,ElementOrder,ElementColor,ElementAlignment,ApptFieldDefNum,PatFieldDefNum,IsMobile) VALUES(";

        command +=
            SOut.Long(apptViewItem.ApptViewNum) + ","
                                                + SOut.Long(apptViewItem.OpNum) + ","
                                                + SOut.Long(apptViewItem.ProvNum) + ","
                                                + "'" + SOut.String(apptViewItem.ElementDesc) + "',"
                                                + SOut.Byte(apptViewItem.ElementOrder) + ","
                                                + SOut.Int(apptViewItem.ElementColor.ToArgb()) + ","
                                                + SOut.Int((int) apptViewItem.ElementAlignment) + ","
                                                + SOut.Long(apptViewItem.ApptFieldDefNum) + ","
                                                + SOut.Long(apptViewItem.PatFieldDefNum) + ","
                                                + SOut.Bool(apptViewItem.IsMobile) + ")";
        {
            apptViewItem.ApptViewItemNum = Db.NonQ(command, true, "ApptViewItemNum", "apptViewItem");
        }
    }

    public static void InsertMany(List<ApptViewItem> listApptViewItems)
    {
        InsertMany(listApptViewItems, false);
    }

    public static void InsertMany(List<ApptViewItem> listApptViewItems, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listApptViewItems.Count)
        {
            var apptViewItem = listApptViewItems[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO apptviewitem (");
                if (useExistingPK) sbCommands.Append("ApptViewItemNum,");
                sbCommands.Append("ApptViewNum,OpNum,ProvNum,ElementDesc,ElementOrder,ElementColor,ElementAlignment,ApptFieldDefNum,PatFieldDefNum,IsMobile) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(apptViewItem.ApptViewItemNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(apptViewItem.ApptViewNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(apptViewItem.OpNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(apptViewItem.ProvNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(apptViewItem.ElementDesc) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Byte(apptViewItem.ElementOrder));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(apptViewItem.ElementColor.ToArgb()));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) apptViewItem.ElementAlignment));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(apptViewItem.ApptFieldDefNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(apptViewItem.PatFieldDefNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(apptViewItem.IsMobile));
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
                if (index == listApptViewItems.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }
}