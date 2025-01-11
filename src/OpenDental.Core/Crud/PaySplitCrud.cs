#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PaySplitCrud
{
    public static PaySplit SelectOne(long splitNum)
    {
        var command = "SELECT * FROM paysplit "
                      + "WHERE SplitNum = " + SOut.Long(splitNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PaySplit SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PaySplit> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PaySplit> TableToList(DataTable table)
    {
        var retVal = new List<PaySplit>();
        PaySplit paySplit;
        foreach (DataRow row in table.Rows)
        {
            paySplit = new PaySplit();
            paySplit.SplitNum = SIn.Long(row["SplitNum"].ToString());
            paySplit.SplitAmt = SIn.Double(row["SplitAmt"].ToString());
            paySplit.PatNum = SIn.Long(row["PatNum"].ToString());
            paySplit.ProcDate = SIn.Date(row["ProcDate"].ToString());
            paySplit.PayNum = SIn.Long(row["PayNum"].ToString());
            paySplit.IsDiscount = SIn.Bool(row["IsDiscount"].ToString());
            paySplit.DiscountType = SIn.Byte(row["DiscountType"].ToString());
            paySplit.ProvNum = SIn.Long(row["ProvNum"].ToString());
            paySplit.PayPlanNum = SIn.Long(row["PayPlanNum"].ToString());
            paySplit.DatePay = SIn.Date(row["DatePay"].ToString());
            paySplit.ProcNum = SIn.Long(row["ProcNum"].ToString());
            paySplit.DateEntry = SIn.Date(row["DateEntry"].ToString());
            paySplit.UnearnedType = SIn.Long(row["UnearnedType"].ToString());
            paySplit.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            paySplit.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            paySplit.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            paySplit.FSplitNum = SIn.Long(row["FSplitNum"].ToString());
            paySplit.AdjNum = SIn.Long(row["AdjNum"].ToString());
            paySplit.PayPlanChargeNum = SIn.Long(row["PayPlanChargeNum"].ToString());
            paySplit.PayPlanDebitType = (PayPlanDebitTypes) SIn.Int(row["PayPlanDebitType"].ToString());
            paySplit.SecurityHash = SIn.String(row["SecurityHash"].ToString());
            retVal.Add(paySplit);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PaySplit> listPaySplits, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PaySplit";
        var table = new DataTable(tableName);
        table.Columns.Add("SplitNum");
        table.Columns.Add("SplitAmt");
        table.Columns.Add("PatNum");
        table.Columns.Add("ProcDate");
        table.Columns.Add("PayNum");
        table.Columns.Add("IsDiscount");
        table.Columns.Add("DiscountType");
        table.Columns.Add("ProvNum");
        table.Columns.Add("PayPlanNum");
        table.Columns.Add("DatePay");
        table.Columns.Add("ProcNum");
        table.Columns.Add("DateEntry");
        table.Columns.Add("UnearnedType");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("SecUserNumEntry");
        table.Columns.Add("SecDateTEdit");
        table.Columns.Add("FSplitNum");
        table.Columns.Add("AdjNum");
        table.Columns.Add("PayPlanChargeNum");
        table.Columns.Add("PayPlanDebitType");
        table.Columns.Add("SecurityHash");
        foreach (var paySplit in listPaySplits)
            table.Rows.Add(SOut.Long(paySplit.SplitNum), SOut.Double(paySplit.SplitAmt), SOut.Long(paySplit.PatNum), SOut.DateT(paySplit.ProcDate, false), SOut.Long(paySplit.PayNum), SOut.Bool(paySplit.IsDiscount), SOut.Byte(paySplit.DiscountType), SOut.Long(paySplit.ProvNum), SOut.Long(paySplit.PayPlanNum), SOut.DateT(paySplit.DatePay, false), SOut.Long(paySplit.ProcNum), SOut.DateT(paySplit.DateEntry, false), SOut.Long(paySplit.UnearnedType), SOut.Long(paySplit.ClinicNum), SOut.Long(paySplit.SecUserNumEntry), SOut.DateT(paySplit.SecDateTEdit, false), SOut.Long(paySplit.FSplitNum), SOut.Long(paySplit.AdjNum), SOut.Long(paySplit.PayPlanChargeNum), SOut.Int((int) paySplit.PayPlanDebitType), paySplit.SecurityHash);
        return table;
    }

    public static long Insert(PaySplit paySplit)
    {
        return Insert(paySplit, false);
    }

    public static long Insert(PaySplit paySplit, bool useExistingPK)
    {
        var command = "INSERT INTO paysplit (";

        command += "SplitAmt,PatNum,ProcDate,PayNum,IsDiscount,DiscountType,ProvNum,PayPlanNum,DatePay,ProcNum,DateEntry,UnearnedType,ClinicNum,SecUserNumEntry,FSplitNum,AdjNum,PayPlanChargeNum,PayPlanDebitType,SecurityHash) VALUES(";

        command +=
            SOut.Double(paySplit.SplitAmt) + ","
                                           + SOut.Long(paySplit.PatNum) + ","
                                           + SOut.Date(paySplit.ProcDate) + ","
                                           + SOut.Long(paySplit.PayNum) + ","
                                           + SOut.Bool(paySplit.IsDiscount) + ","
                                           + SOut.Byte(paySplit.DiscountType) + ","
                                           + SOut.Long(paySplit.ProvNum) + ","
                                           + SOut.Long(paySplit.PayPlanNum) + ","
                                           + SOut.Date(paySplit.DatePay) + ","
                                           + SOut.Long(paySplit.ProcNum) + ","
                                           + DbHelper.Now() + ","
                                           + SOut.Long(paySplit.UnearnedType) + ","
                                           + SOut.Long(paySplit.ClinicNum) + ","
                                           + SOut.Long(paySplit.SecUserNumEntry) + ","
                                           //SecDateTEdit can only be set by MySQL
                                           + SOut.Long(paySplit.FSplitNum) + ","
                                           + SOut.Long(paySplit.AdjNum) + ","
                                           + SOut.Long(paySplit.PayPlanChargeNum) + ","
                                           + SOut.Int((int) paySplit.PayPlanDebitType) + ","
                                           + "'" + SOut.String(paySplit.SecurityHash) + "')";
        {
            paySplit.SplitNum = Db.NonQ(command, true, "SplitNum", "paySplit");
        }
        return paySplit.SplitNum;
    }

    public static void InsertMany(List<PaySplit> listPaySplits)
    {
        InsertMany(listPaySplits, false);
    }

    public static void InsertMany(List<PaySplit> listPaySplits, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listPaySplits.Count)
        {
            var paySplit = listPaySplits[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO paysplit (");
                if (useExistingPK) sbCommands.Append("SplitNum,");
                sbCommands.Append("SplitAmt,PatNum,ProcDate,PayNum,IsDiscount,DiscountType,ProvNum,PayPlanNum,DatePay,ProcNum,DateEntry,UnearnedType,ClinicNum,SecUserNumEntry,FSplitNum,AdjNum,PayPlanChargeNum,PayPlanDebitType,SecurityHash) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(paySplit.SplitNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Double(paySplit.SplitAmt));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(paySplit.PatNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(paySplit.ProcDate));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(paySplit.PayNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(paySplit.IsDiscount));
            sbRow.Append(",");
            sbRow.Append(SOut.Byte(paySplit.DiscountType));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(paySplit.ProvNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(paySplit.PayPlanNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(paySplit.DatePay));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(paySplit.ProcNum));
            sbRow.Append(",");
            sbRow.Append(DbHelper.Now());
            sbRow.Append(",");
            sbRow.Append(SOut.Long(paySplit.UnearnedType));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(paySplit.ClinicNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(paySplit.SecUserNumEntry));
            sbRow.Append(",");
            //SecDateTEdit can only be set by MySQL
            sbRow.Append(SOut.Long(paySplit.FSplitNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(paySplit.AdjNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(paySplit.PayPlanChargeNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) paySplit.PayPlanDebitType));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(paySplit.SecurityHash) + "'");
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
                if (index == listPaySplits.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(PaySplit paySplit)
    {
        return InsertNoCache(paySplit, false);
    }

    public static long InsertNoCache(PaySplit paySplit, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO paysplit (";
        if (isRandomKeys || useExistingPK) command += "SplitNum,";
        command += "SplitAmt,PatNum,ProcDate,PayNum,IsDiscount,DiscountType,ProvNum,PayPlanNum,DatePay,ProcNum,DateEntry,UnearnedType,ClinicNum,SecUserNumEntry,FSplitNum,AdjNum,PayPlanChargeNum,PayPlanDebitType,SecurityHash) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(paySplit.SplitNum) + ",";
        command +=
            SOut.Double(paySplit.SplitAmt) + ","
                                           + SOut.Long(paySplit.PatNum) + ","
                                           + SOut.Date(paySplit.ProcDate) + ","
                                           + SOut.Long(paySplit.PayNum) + ","
                                           + SOut.Bool(paySplit.IsDiscount) + ","
                                           + SOut.Byte(paySplit.DiscountType) + ","
                                           + SOut.Long(paySplit.ProvNum) + ","
                                           + SOut.Long(paySplit.PayPlanNum) + ","
                                           + SOut.Date(paySplit.DatePay) + ","
                                           + SOut.Long(paySplit.ProcNum) + ","
                                           + DbHelper.Now() + ","
                                           + SOut.Long(paySplit.UnearnedType) + ","
                                           + SOut.Long(paySplit.ClinicNum) + ","
                                           + SOut.Long(paySplit.SecUserNumEntry) + ","
                                           //SecDateTEdit can only be set by MySQL
                                           + SOut.Long(paySplit.FSplitNum) + ","
                                           + SOut.Long(paySplit.AdjNum) + ","
                                           + SOut.Long(paySplit.PayPlanChargeNum) + ","
                                           + SOut.Int((int) paySplit.PayPlanDebitType) + ","
                                           + "'" + SOut.String(paySplit.SecurityHash) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            paySplit.SplitNum = Db.NonQ(command, true, "SplitNum", "paySplit");
        return paySplit.SplitNum;
    }

    public static void Update(PaySplit paySplit)
    {
        var command = "UPDATE paysplit SET "
                      + "SplitAmt        =  " + SOut.Double(paySplit.SplitAmt) + ", "
                      + "PatNum          =  " + SOut.Long(paySplit.PatNum) + ", "
                      + "ProcDate        =  " + SOut.Date(paySplit.ProcDate) + ", "
                      + "PayNum          =  " + SOut.Long(paySplit.PayNum) + ", "
                      + "IsDiscount      =  " + SOut.Bool(paySplit.IsDiscount) + ", "
                      + "DiscountType    =  " + SOut.Byte(paySplit.DiscountType) + ", "
                      + "ProvNum         =  " + SOut.Long(paySplit.ProvNum) + ", "
                      + "PayPlanNum      =  " + SOut.Long(paySplit.PayPlanNum) + ", "
                      + "DatePay         =  " + SOut.Date(paySplit.DatePay) + ", "
                      + "ProcNum         =  " + SOut.Long(paySplit.ProcNum) + ", "
                      //DateEntry not allowed to change
                      + "UnearnedType    =  " + SOut.Long(paySplit.UnearnedType) + ", "
                      + "ClinicNum       =  " + SOut.Long(paySplit.ClinicNum) + ", "
                      //SecUserNumEntry excluded from update
                      //SecDateTEdit can only be set by MySQL
                      + "FSplitNum       =  " + SOut.Long(paySplit.FSplitNum) + ", "
                      + "AdjNum          =  " + SOut.Long(paySplit.AdjNum) + ", "
                      + "PayPlanChargeNum=  " + SOut.Long(paySplit.PayPlanChargeNum) + ", "
                      + "PayPlanDebitType=  " + SOut.Int((int) paySplit.PayPlanDebitType) + ", "
                      + "SecurityHash    = '" + SOut.String(paySplit.SecurityHash) + "' "
                      + "WHERE SplitNum = " + SOut.Long(paySplit.SplitNum);
        Db.NonQ(command);
    }

    public static bool Update(PaySplit paySplit, PaySplit oldPaySplit)
    {
        var command = "";
        if (paySplit.SplitAmt != oldPaySplit.SplitAmt)
        {
            if (command != "") command += ",";
            command += "SplitAmt = " + SOut.Double(paySplit.SplitAmt) + "";
        }

        if (paySplit.PatNum != oldPaySplit.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(paySplit.PatNum) + "";
        }

        if (paySplit.ProcDate.Date != oldPaySplit.ProcDate.Date)
        {
            if (command != "") command += ",";
            command += "ProcDate = " + SOut.Date(paySplit.ProcDate) + "";
        }

        if (paySplit.PayNum != oldPaySplit.PayNum)
        {
            if (command != "") command += ",";
            command += "PayNum = " + SOut.Long(paySplit.PayNum) + "";
        }

        if (paySplit.IsDiscount != oldPaySplit.IsDiscount)
        {
            if (command != "") command += ",";
            command += "IsDiscount = " + SOut.Bool(paySplit.IsDiscount) + "";
        }

        if (paySplit.DiscountType != oldPaySplit.DiscountType)
        {
            if (command != "") command += ",";
            command += "DiscountType = " + SOut.Byte(paySplit.DiscountType) + "";
        }

        if (paySplit.ProvNum != oldPaySplit.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(paySplit.ProvNum) + "";
        }

        if (paySplit.PayPlanNum != oldPaySplit.PayPlanNum)
        {
            if (command != "") command += ",";
            command += "PayPlanNum = " + SOut.Long(paySplit.PayPlanNum) + "";
        }

        if (paySplit.DatePay.Date != oldPaySplit.DatePay.Date)
        {
            if (command != "") command += ",";
            command += "DatePay = " + SOut.Date(paySplit.DatePay) + "";
        }

        if (paySplit.ProcNum != oldPaySplit.ProcNum)
        {
            if (command != "") command += ",";
            command += "ProcNum = " + SOut.Long(paySplit.ProcNum) + "";
        }

        //DateEntry not allowed to change
        if (paySplit.UnearnedType != oldPaySplit.UnearnedType)
        {
            if (command != "") command += ",";
            command += "UnearnedType = " + SOut.Long(paySplit.UnearnedType) + "";
        }

        if (paySplit.ClinicNum != oldPaySplit.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(paySplit.ClinicNum) + "";
        }

        //SecUserNumEntry excluded from update
        //SecDateTEdit can only be set by MySQL
        if (paySplit.FSplitNum != oldPaySplit.FSplitNum)
        {
            if (command != "") command += ",";
            command += "FSplitNum = " + SOut.Long(paySplit.FSplitNum) + "";
        }

        if (paySplit.AdjNum != oldPaySplit.AdjNum)
        {
            if (command != "") command += ",";
            command += "AdjNum = " + SOut.Long(paySplit.AdjNum) + "";
        }

        if (paySplit.PayPlanChargeNum != oldPaySplit.PayPlanChargeNum)
        {
            if (command != "") command += ",";
            command += "PayPlanChargeNum = " + SOut.Long(paySplit.PayPlanChargeNum) + "";
        }

        if (paySplit.PayPlanDebitType != oldPaySplit.PayPlanDebitType)
        {
            if (command != "") command += ",";
            command += "PayPlanDebitType = " + SOut.Int((int) paySplit.PayPlanDebitType) + "";
        }

        if (paySplit.SecurityHash != oldPaySplit.SecurityHash)
        {
            if (command != "") command += ",";
            command += "SecurityHash = '" + SOut.String(paySplit.SecurityHash) + "'";
        }

        if (command == "") return false;
        command = "UPDATE paysplit SET " + command
                                         + " WHERE SplitNum = " + SOut.Long(paySplit.SplitNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(PaySplit paySplit, PaySplit oldPaySplit)
    {
        if (paySplit.SplitAmt != oldPaySplit.SplitAmt) return true;
        if (paySplit.PatNum != oldPaySplit.PatNum) return true;
        if (paySplit.ProcDate.Date != oldPaySplit.ProcDate.Date) return true;
        if (paySplit.PayNum != oldPaySplit.PayNum) return true;
        if (paySplit.IsDiscount != oldPaySplit.IsDiscount) return true;
        if (paySplit.DiscountType != oldPaySplit.DiscountType) return true;
        if (paySplit.ProvNum != oldPaySplit.ProvNum) return true;
        if (paySplit.PayPlanNum != oldPaySplit.PayPlanNum) return true;
        if (paySplit.DatePay.Date != oldPaySplit.DatePay.Date) return true;
        if (paySplit.ProcNum != oldPaySplit.ProcNum) return true;
        //DateEntry not allowed to change
        if (paySplit.UnearnedType != oldPaySplit.UnearnedType) return true;
        if (paySplit.ClinicNum != oldPaySplit.ClinicNum) return true;
        //SecUserNumEntry excluded from update
        //SecDateTEdit can only be set by MySQL
        if (paySplit.FSplitNum != oldPaySplit.FSplitNum) return true;
        if (paySplit.AdjNum != oldPaySplit.AdjNum) return true;
        if (paySplit.PayPlanChargeNum != oldPaySplit.PayPlanChargeNum) return true;
        if (paySplit.PayPlanDebitType != oldPaySplit.PayPlanDebitType) return true;
        if (paySplit.SecurityHash != oldPaySplit.SecurityHash) return true;
        return false;
    }

    public static void Delete(long splitNum)
    {
        var command = "DELETE FROM paysplit "
                      + "WHERE SplitNum = " + SOut.Long(splitNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSplitNums)
    {
        if (listSplitNums == null || listSplitNums.Count == 0) return;
        var command = "DELETE FROM paysplit "
                      + "WHERE SplitNum IN(" + string.Join(",", listSplitNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<PaySplit> listNew, List<PaySplit> listDB, long userNum)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<PaySplit>();
        var listUpdNew = new List<PaySplit>();
        var listUpdDB = new List<PaySplit>();
        var listDel = new List<PaySplit>();
        listNew.Sort((x, y) => { return x.SplitNum.CompareTo(y.SplitNum); });
        listDB.Sort((x, y) => { return x.SplitNum.CompareTo(y.SplitNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        PaySplit fieldNew;
        PaySplit fieldDB;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            fieldDB = null;
            if (idxDB < listDB.Count) fieldDB = listDB[idxDB];
            //begin compare
            if (fieldNew != null && fieldDB == null)
            {
                //listNew has more items, listDB does not.
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew == null && fieldDB != null)
            {
                //listDB has more items, listNew does not.
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            if (fieldNew.SplitNum < fieldDB.SplitNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.SplitNum > fieldDB.SplitNum)
            {
                //dbPK less than newPK, dbItem is 'next'
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            //Both lists contain the 'next' item, update required
            listUpdNew.Add(fieldNew);
            listUpdDB.Add(fieldDB);
            idxNew++;
            idxDB++;
        }

        //Commit changes to DB
        for (var i = 0; i < listIns.Count; i++)
        {
            listIns[i].SecUserNumEntry = userNum;
            Insert(listIns[i]);
        }

        for (var i = 0; i < listUpdNew.Count; i++)
            if (Update(listUpdNew[i], listUpdDB[i]))
                rowsUpdatedCount++;

        DeleteMany(listDel.Select(x => x.SplitNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}