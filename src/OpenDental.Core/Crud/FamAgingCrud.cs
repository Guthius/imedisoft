#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class FamAgingCrud
{
    public static FamAging SelectOne(long patNum)
    {
        var command = "SELECT * FROM famaging "
                      + "WHERE PatNum = " + SOut.Long(patNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

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

    public static DataTable ListToTable(List<FamAging> listFamAgings, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "FamAging";
        var table = new DataTable(tableName);
        table.Columns.Add("PatNum");
        table.Columns.Add("Bal_0_30");
        table.Columns.Add("Bal_31_60");
        table.Columns.Add("Bal_61_90");
        table.Columns.Add("BalOver90");
        table.Columns.Add("InsEst");
        table.Columns.Add("BalTotal");
        table.Columns.Add("PayPlanDue");
        foreach (var famAging in listFamAgings)
            table.Rows.Add(SOut.Long(famAging.PatNum), SOut.Double(famAging.Bal_0_30), SOut.Double(famAging.Bal_31_60), SOut.Double(famAging.Bal_61_90), SOut.Double(famAging.BalOver90), SOut.Double(famAging.InsEst), SOut.Double(famAging.BalTotal), SOut.Double(famAging.PayPlanDue));
        return table;
    }

    public static long Insert(FamAging famAging)
    {
        return Insert(famAging, false);
    }

    public static long Insert(FamAging famAging, bool useExistingPK)
    {
        var command = "INSERT INTO famaging (";

        command += "Bal_0_30,Bal_31_60,Bal_61_90,BalOver90,InsEst,BalTotal,PayPlanDue) VALUES(";

        command +=
            SOut.Double(famAging.Bal_0_30) + ","
                                           + SOut.Double(famAging.Bal_31_60) + ","
                                           + SOut.Double(famAging.Bal_61_90) + ","
                                           + SOut.Double(famAging.BalOver90) + ","
                                           + SOut.Double(famAging.InsEst) + ","
                                           + SOut.Double(famAging.BalTotal) + ","
                                           + SOut.Double(famAging.PayPlanDue) + ")";
        {
            famAging.PatNum = Db.NonQ(command, true, "PatNum", "famAging");
        }
        return famAging.PatNum;
    }

    public static void InsertMany(List<FamAging> listFamAgings)
    {
        InsertMany(listFamAgings, false);
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

    public static long InsertNoCache(FamAging famAging)
    {
        return InsertNoCache(famAging, false);
    }

    public static long InsertNoCache(FamAging famAging, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO famaging (";
        if (isRandomKeys || useExistingPK) command += "PatNum,";
        command += "Bal_0_30,Bal_31_60,Bal_61_90,BalOver90,InsEst,BalTotal,PayPlanDue) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(famAging.PatNum) + ",";
        command +=
            SOut.Double(famAging.Bal_0_30) + ","
                                           + SOut.Double(famAging.Bal_31_60) + ","
                                           + SOut.Double(famAging.Bal_61_90) + ","
                                           + SOut.Double(famAging.BalOver90) + ","
                                           + SOut.Double(famAging.InsEst) + ","
                                           + SOut.Double(famAging.BalTotal) + ","
                                           + SOut.Double(famAging.PayPlanDue) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            famAging.PatNum = Db.NonQ(command, true, "PatNum", "famAging");
        return famAging.PatNum;
    }

    public static void Update(FamAging famAging)
    {
        var command = "UPDATE famaging SET "
                      + "Bal_0_30  =  " + SOut.Double(famAging.Bal_0_30) + ", "
                      + "Bal_31_60 =  " + SOut.Double(famAging.Bal_31_60) + ", "
                      + "Bal_61_90 =  " + SOut.Double(famAging.Bal_61_90) + ", "
                      + "BalOver90 =  " + SOut.Double(famAging.BalOver90) + ", "
                      + "InsEst    =  " + SOut.Double(famAging.InsEst) + ", "
                      + "BalTotal  =  " + SOut.Double(famAging.BalTotal) + ", "
                      + "PayPlanDue=  " + SOut.Double(famAging.PayPlanDue) + " "
                      + "WHERE PatNum = " + SOut.Long(famAging.PatNum);
        Db.NonQ(command);
    }

    public static bool Update(FamAging famAging, FamAging oldFamAging)
    {
        var command = "";
        if (famAging.Bal_0_30 != oldFamAging.Bal_0_30)
        {
            if (command != "") command += ",";
            command += "Bal_0_30 = " + SOut.Double(famAging.Bal_0_30) + "";
        }

        if (famAging.Bal_31_60 != oldFamAging.Bal_31_60)
        {
            if (command != "") command += ",";
            command += "Bal_31_60 = " + SOut.Double(famAging.Bal_31_60) + "";
        }

        if (famAging.Bal_61_90 != oldFamAging.Bal_61_90)
        {
            if (command != "") command += ",";
            command += "Bal_61_90 = " + SOut.Double(famAging.Bal_61_90) + "";
        }

        if (famAging.BalOver90 != oldFamAging.BalOver90)
        {
            if (command != "") command += ",";
            command += "BalOver90 = " + SOut.Double(famAging.BalOver90) + "";
        }

        if (famAging.InsEst != oldFamAging.InsEst)
        {
            if (command != "") command += ",";
            command += "InsEst = " + SOut.Double(famAging.InsEst) + "";
        }

        if (famAging.BalTotal != oldFamAging.BalTotal)
        {
            if (command != "") command += ",";
            command += "BalTotal = " + SOut.Double(famAging.BalTotal) + "";
        }

        if (famAging.PayPlanDue != oldFamAging.PayPlanDue)
        {
            if (command != "") command += ",";
            command += "PayPlanDue = " + SOut.Double(famAging.PayPlanDue) + "";
        }

        if (command == "") return false;
        command = "UPDATE famaging SET " + command
                                         + " WHERE PatNum = " + SOut.Long(famAging.PatNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(FamAging famAging, FamAging oldFamAging)
    {
        if (famAging.Bal_0_30 != oldFamAging.Bal_0_30) return true;
        if (famAging.Bal_31_60 != oldFamAging.Bal_31_60) return true;
        if (famAging.Bal_61_90 != oldFamAging.Bal_61_90) return true;
        if (famAging.BalOver90 != oldFamAging.BalOver90) return true;
        if (famAging.InsEst != oldFamAging.InsEst) return true;
        if (famAging.BalTotal != oldFamAging.BalTotal) return true;
        if (famAging.PayPlanDue != oldFamAging.PayPlanDue) return true;
        return false;
    }

    public static void Delete(long patNum)
    {
        var command = "DELETE FROM famaging "
                      + "WHERE PatNum = " + SOut.Long(patNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPatNums)
    {
        if (listPatNums == null || listPatNums.Count == 0) return;
        var command = "DELETE FROM famaging "
                      + "WHERE PatNum IN(" + string.Join(",", listPatNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}