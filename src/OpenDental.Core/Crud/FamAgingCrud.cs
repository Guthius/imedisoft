using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class FamAgingCrud
{
    public static FamAging SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<FamAging> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<FamAging> TableToList(DataTable table)
    {
        var retVal = new List<FamAging>();
        FamAging famAging;
        foreach (DataRow row in table.Rows)
        {
            famAging = new FamAging();
            famAging.PatNum = SIn.Long(row["PatNum"].ToString());
            famAging.Bal_0_30 = SIn.Double(row["Bal_0_30"].ToString());
            famAging.Bal_31_60 = SIn.Double(row["Bal_31_60"].ToString());
            famAging.Bal_61_90 = SIn.Double(row["Bal_61_90"].ToString());
            famAging.BalOver90 = SIn.Double(row["BalOver90"].ToString());
            famAging.InsEst = SIn.Double(row["InsEst"].ToString());
            famAging.BalTotal = SIn.Double(row["BalTotal"].ToString());
            famAging.PayPlanDue = SIn.Double(row["PayPlanDue"].ToString());
            retVal.Add(famAging);
        }

        return retVal;
    }

    public static void InsertMany(List<FamAging> listFamAgings, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listFamAgings.Count)
        {
            var famAging = listFamAgings[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO famaging (");
                if (useExistingPK) sbCommands.Append("PatNum,");
                sbCommands.Append("Bal_0_30,Bal_31_60,Bal_61_90,BalOver90,InsEst,BalTotal,PayPlanDue) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(famAging.PatNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Double(famAging.Bal_0_30));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(famAging.Bal_31_60));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(famAging.Bal_61_90));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(famAging.BalOver90));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(famAging.InsEst));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(famAging.BalTotal));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(famAging.PayPlanDue));
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
                if (index == listFamAgings.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }
}