#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PayPlanChargeCrud
{
    public static PayPlanCharge SelectOne(long payPlanChargeNum)
    {
        var command = "SELECT * FROM payplancharge "
                      + "WHERE PayPlanChargeNum = " + SOut.Long(payPlanChargeNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PayPlanCharge SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PayPlanCharge> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PayPlanCharge> TableToList(DataTable table)
    {
        var retVal = new List<PayPlanCharge>();
        PayPlanCharge payPlanCharge;
        foreach (DataRow row in table.Rows)
        {
            payPlanCharge = new PayPlanCharge();
            payPlanCharge.PayPlanChargeNum = SIn.Long(row["PayPlanChargeNum"].ToString());
            payPlanCharge.PayPlanNum = SIn.Long(row["PayPlanNum"].ToString());
            payPlanCharge.Guarantor = SIn.Long(row["Guarantor"].ToString());
            payPlanCharge.PatNum = SIn.Long(row["PatNum"].ToString());
            payPlanCharge.ChargeDate = SIn.Date(row["ChargeDate"].ToString());
            payPlanCharge.Principal = SIn.Double(row["Principal"].ToString());
            payPlanCharge.Interest = SIn.Double(row["Interest"].ToString());
            payPlanCharge.Note = SIn.String(row["Note"].ToString());
            payPlanCharge.ProvNum = SIn.Long(row["ProvNum"].ToString());
            payPlanCharge.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            payPlanCharge.ChargeType = (PayPlanChargeType) SIn.Int(row["ChargeType"].ToString());
            payPlanCharge.ProcNum = SIn.Long(row["ProcNum"].ToString());
            payPlanCharge.SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString());
            payPlanCharge.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            payPlanCharge.StatementNum = SIn.Long(row["StatementNum"].ToString());
            payPlanCharge.FKey = SIn.Long(row["FKey"].ToString());
            payPlanCharge.LinkType = (PayPlanLinkType) SIn.Int(row["LinkType"].ToString());
            payPlanCharge.IsOffset = SIn.Bool(row["IsOffset"].ToString());
            retVal.Add(payPlanCharge);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PayPlanCharge> listPayPlanCharges, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PayPlanCharge";
        var table = new DataTable(tableName);
        table.Columns.Add("PayPlanChargeNum");
        table.Columns.Add("PayPlanNum");
        table.Columns.Add("Guarantor");
        table.Columns.Add("PatNum");
        table.Columns.Add("ChargeDate");
        table.Columns.Add("Principal");
        table.Columns.Add("Interest");
        table.Columns.Add("Note");
        table.Columns.Add("ProvNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("ChargeType");
        table.Columns.Add("ProcNum");
        table.Columns.Add("SecDateTEntry");
        table.Columns.Add("SecDateTEdit");
        table.Columns.Add("StatementNum");
        table.Columns.Add("FKey");
        table.Columns.Add("LinkType");
        table.Columns.Add("IsOffset");
        foreach (var payPlanCharge in listPayPlanCharges)
            table.Rows.Add(SOut.Long(payPlanCharge.PayPlanChargeNum), SOut.Long(payPlanCharge.PayPlanNum), SOut.Long(payPlanCharge.Guarantor), SOut.Long(payPlanCharge.PatNum), SOut.DateT(payPlanCharge.ChargeDate, false), SOut.Double(payPlanCharge.Principal), SOut.Double(payPlanCharge.Interest), payPlanCharge.Note, SOut.Long(payPlanCharge.ProvNum), SOut.Long(payPlanCharge.ClinicNum), SOut.Int((int) payPlanCharge.ChargeType), SOut.Long(payPlanCharge.ProcNum), SOut.DateT(payPlanCharge.SecDateTEntry, false), SOut.DateT(payPlanCharge.SecDateTEdit, false), SOut.Long(payPlanCharge.StatementNum), SOut.Long(payPlanCharge.FKey), SOut.Int((int) payPlanCharge.LinkType), SOut.Bool(payPlanCharge.IsOffset));
        return table;
    }

    public static long Insert(PayPlanCharge payPlanCharge)
    {
        return Insert(payPlanCharge, false);
    }

    public static long Insert(PayPlanCharge payPlanCharge, bool useExistingPK)
    {
        var command = "INSERT INTO payplancharge (";

        command += "PayPlanNum,Guarantor,PatNum,ChargeDate,Principal,Interest,Note,ProvNum,ClinicNum,ChargeType,ProcNum,SecDateTEntry,StatementNum,FKey,LinkType,IsOffset) VALUES(";

        command +=
            SOut.Long(payPlanCharge.PayPlanNum) + ","
                                                + SOut.Long(payPlanCharge.Guarantor) + ","
                                                + SOut.Long(payPlanCharge.PatNum) + ","
                                                + SOut.Date(payPlanCharge.ChargeDate) + ","
                                                + SOut.Double(payPlanCharge.Principal) + ","
                                                + SOut.Double(payPlanCharge.Interest) + ","
                                                + DbHelper.ParamChar + "paramNote,"
                                                + SOut.Long(payPlanCharge.ProvNum) + ","
                                                + SOut.Long(payPlanCharge.ClinicNum) + ","
                                                + SOut.Int((int) payPlanCharge.ChargeType) + ","
                                                + SOut.Long(payPlanCharge.ProcNum) + ","
                                                + DbHelper.Now() + ","
                                                //SecDateTEdit can only be set by MySQL
                                                + SOut.Long(payPlanCharge.StatementNum) + ","
                                                + SOut.Long(payPlanCharge.FKey) + ","
                                                + SOut.Int((int) payPlanCharge.LinkType) + ","
                                                + SOut.Bool(payPlanCharge.IsOffset) + ")";
        if (payPlanCharge.Note == null) payPlanCharge.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(payPlanCharge.Note));
        {
            payPlanCharge.PayPlanChargeNum = Db.NonQ(command, true, "PayPlanChargeNum", "payPlanCharge", paramNote);
        }
        return payPlanCharge.PayPlanChargeNum;
    }

    public static void InsertMany(List<PayPlanCharge> listPayPlanCharges)
    {
        InsertMany(listPayPlanCharges, false);
    }

    public static void InsertMany(List<PayPlanCharge> listPayPlanCharges, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listPayPlanCharges.Count)
        {
            var payPlanCharge = listPayPlanCharges[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO payplancharge (");
                if (useExistingPK) sbCommands.Append("PayPlanChargeNum,");
                sbCommands.Append("PayPlanNum,Guarantor,PatNum,ChargeDate,Principal,Interest,Note,ProvNum,ClinicNum,ChargeType,ProcNum,SecDateTEntry,StatementNum,FKey,LinkType,IsOffset) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(payPlanCharge.PayPlanChargeNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(payPlanCharge.PayPlanNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(payPlanCharge.Guarantor));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(payPlanCharge.PatNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(payPlanCharge.ChargeDate));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(payPlanCharge.Principal));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(payPlanCharge.Interest));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(payPlanCharge.Note) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(payPlanCharge.ProvNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(payPlanCharge.ClinicNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) payPlanCharge.ChargeType));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(payPlanCharge.ProcNum));
            sbRow.Append(",");
            sbRow.Append(DbHelper.Now());
            sbRow.Append(",");
            //SecDateTEdit can only be set by MySQL
            sbRow.Append(SOut.Long(payPlanCharge.StatementNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(payPlanCharge.FKey));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) payPlanCharge.LinkType));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(payPlanCharge.IsOffset));
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
                if (index == listPayPlanCharges.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(PayPlanCharge payPlanCharge)
    {
        return InsertNoCache(payPlanCharge, false);
    }

    public static long InsertNoCache(PayPlanCharge payPlanCharge, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO payplancharge (";
        if (isRandomKeys || useExistingPK) command += "PayPlanChargeNum,";
        command += "PayPlanNum,Guarantor,PatNum,ChargeDate,Principal,Interest,Note,ProvNum,ClinicNum,ChargeType,ProcNum,SecDateTEntry,StatementNum,FKey,LinkType,IsOffset) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(payPlanCharge.PayPlanChargeNum) + ",";
        command +=
            SOut.Long(payPlanCharge.PayPlanNum) + ","
                                                + SOut.Long(payPlanCharge.Guarantor) + ","
                                                + SOut.Long(payPlanCharge.PatNum) + ","
                                                + SOut.Date(payPlanCharge.ChargeDate) + ","
                                                + SOut.Double(payPlanCharge.Principal) + ","
                                                + SOut.Double(payPlanCharge.Interest) + ","
                                                + DbHelper.ParamChar + "paramNote,"
                                                + SOut.Long(payPlanCharge.ProvNum) + ","
                                                + SOut.Long(payPlanCharge.ClinicNum) + ","
                                                + SOut.Int((int) payPlanCharge.ChargeType) + ","
                                                + SOut.Long(payPlanCharge.ProcNum) + ","
                                                + DbHelper.Now() + ","
                                                //SecDateTEdit can only be set by MySQL
                                                + SOut.Long(payPlanCharge.StatementNum) + ","
                                                + SOut.Long(payPlanCharge.FKey) + ","
                                                + SOut.Int((int) payPlanCharge.LinkType) + ","
                                                + SOut.Bool(payPlanCharge.IsOffset) + ")";
        if (payPlanCharge.Note == null) payPlanCharge.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(payPlanCharge.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            payPlanCharge.PayPlanChargeNum = Db.NonQ(command, true, "PayPlanChargeNum", "payPlanCharge", paramNote);
        return payPlanCharge.PayPlanChargeNum;
    }

    public static void Update(PayPlanCharge payPlanCharge)
    {
        var command = "UPDATE payplancharge SET "
                      + "PayPlanNum      =  " + SOut.Long(payPlanCharge.PayPlanNum) + ", "
                      + "Guarantor       =  " + SOut.Long(payPlanCharge.Guarantor) + ", "
                      + "PatNum          =  " + SOut.Long(payPlanCharge.PatNum) + ", "
                      + "ChargeDate      =  " + SOut.Date(payPlanCharge.ChargeDate) + ", "
                      + "Principal       =  " + SOut.Double(payPlanCharge.Principal) + ", "
                      + "Interest        =  " + SOut.Double(payPlanCharge.Interest) + ", "
                      + "Note            =  " + DbHelper.ParamChar + "paramNote, "
                      + "ProvNum         =  " + SOut.Long(payPlanCharge.ProvNum) + ", "
                      + "ClinicNum       =  " + SOut.Long(payPlanCharge.ClinicNum) + ", "
                      + "ChargeType      =  " + SOut.Int((int) payPlanCharge.ChargeType) + ", "
                      + "ProcNum         =  " + SOut.Long(payPlanCharge.ProcNum) + ", "
                      //SecDateTEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "StatementNum    =  " + SOut.Long(payPlanCharge.StatementNum) + ", "
                      + "FKey            =  " + SOut.Long(payPlanCharge.FKey) + ", "
                      + "LinkType        =  " + SOut.Int((int) payPlanCharge.LinkType) + ", "
                      + "IsOffset        =  " + SOut.Bool(payPlanCharge.IsOffset) + " "
                      + "WHERE PayPlanChargeNum = " + SOut.Long(payPlanCharge.PayPlanChargeNum);
        if (payPlanCharge.Note == null) payPlanCharge.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(payPlanCharge.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(PayPlanCharge payPlanCharge, PayPlanCharge oldPayPlanCharge)
    {
        var command = "";
        if (payPlanCharge.PayPlanNum != oldPayPlanCharge.PayPlanNum)
        {
            if (command != "") command += ",";
            command += "PayPlanNum = " + SOut.Long(payPlanCharge.PayPlanNum) + "";
        }

        if (payPlanCharge.Guarantor != oldPayPlanCharge.Guarantor)
        {
            if (command != "") command += ",";
            command += "Guarantor = " + SOut.Long(payPlanCharge.Guarantor) + "";
        }

        if (payPlanCharge.PatNum != oldPayPlanCharge.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(payPlanCharge.PatNum) + "";
        }

        if (payPlanCharge.ChargeDate.Date != oldPayPlanCharge.ChargeDate.Date)
        {
            if (command != "") command += ",";
            command += "ChargeDate = " + SOut.Date(payPlanCharge.ChargeDate) + "";
        }

        if (payPlanCharge.Principal != oldPayPlanCharge.Principal)
        {
            if (command != "") command += ",";
            command += "Principal = " + SOut.Double(payPlanCharge.Principal) + "";
        }

        if (payPlanCharge.Interest != oldPayPlanCharge.Interest)
        {
            if (command != "") command += ",";
            command += "Interest = " + SOut.Double(payPlanCharge.Interest) + "";
        }

        if (payPlanCharge.Note != oldPayPlanCharge.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (payPlanCharge.ProvNum != oldPayPlanCharge.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(payPlanCharge.ProvNum) + "";
        }

        if (payPlanCharge.ClinicNum != oldPayPlanCharge.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(payPlanCharge.ClinicNum) + "";
        }

        if (payPlanCharge.ChargeType != oldPayPlanCharge.ChargeType)
        {
            if (command != "") command += ",";
            command += "ChargeType = " + SOut.Int((int) payPlanCharge.ChargeType) + "";
        }

        if (payPlanCharge.ProcNum != oldPayPlanCharge.ProcNum)
        {
            if (command != "") command += ",";
            command += "ProcNum = " + SOut.Long(payPlanCharge.ProcNum) + "";
        }

        //SecDateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (payPlanCharge.StatementNum != oldPayPlanCharge.StatementNum)
        {
            if (command != "") command += ",";
            command += "StatementNum = " + SOut.Long(payPlanCharge.StatementNum) + "";
        }

        if (payPlanCharge.FKey != oldPayPlanCharge.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(payPlanCharge.FKey) + "";
        }

        if (payPlanCharge.LinkType != oldPayPlanCharge.LinkType)
        {
            if (command != "") command += ",";
            command += "LinkType = " + SOut.Int((int) payPlanCharge.LinkType) + "";
        }

        if (payPlanCharge.IsOffset != oldPayPlanCharge.IsOffset)
        {
            if (command != "") command += ",";
            command += "IsOffset = " + SOut.Bool(payPlanCharge.IsOffset) + "";
        }

        if (command == "") return false;
        if (payPlanCharge.Note == null) payPlanCharge.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(payPlanCharge.Note));
        command = "UPDATE payplancharge SET " + command
                                              + " WHERE PayPlanChargeNum = " + SOut.Long(payPlanCharge.PayPlanChargeNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(PayPlanCharge payPlanCharge, PayPlanCharge oldPayPlanCharge)
    {
        if (payPlanCharge.PayPlanNum != oldPayPlanCharge.PayPlanNum) return true;
        if (payPlanCharge.Guarantor != oldPayPlanCharge.Guarantor) return true;
        if (payPlanCharge.PatNum != oldPayPlanCharge.PatNum) return true;
        if (payPlanCharge.ChargeDate.Date != oldPayPlanCharge.ChargeDate.Date) return true;
        if (payPlanCharge.Principal != oldPayPlanCharge.Principal) return true;
        if (payPlanCharge.Interest != oldPayPlanCharge.Interest) return true;
        if (payPlanCharge.Note != oldPayPlanCharge.Note) return true;
        if (payPlanCharge.ProvNum != oldPayPlanCharge.ProvNum) return true;
        if (payPlanCharge.ClinicNum != oldPayPlanCharge.ClinicNum) return true;
        if (payPlanCharge.ChargeType != oldPayPlanCharge.ChargeType) return true;
        if (payPlanCharge.ProcNum != oldPayPlanCharge.ProcNum) return true;
        //SecDateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (payPlanCharge.StatementNum != oldPayPlanCharge.StatementNum) return true;
        if (payPlanCharge.FKey != oldPayPlanCharge.FKey) return true;
        if (payPlanCharge.LinkType != oldPayPlanCharge.LinkType) return true;
        if (payPlanCharge.IsOffset != oldPayPlanCharge.IsOffset) return true;
        return false;
    }

    public static void Delete(long payPlanChargeNum)
    {
        var command = "DELETE FROM payplancharge "
                      + "WHERE PayPlanChargeNum = " + SOut.Long(payPlanChargeNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPayPlanChargeNums)
    {
        if (listPayPlanChargeNums == null || listPayPlanChargeNums.Count == 0) return;
        var command = "DELETE FROM payplancharge "
                      + "WHERE PayPlanChargeNum IN(" + string.Join(",", listPayPlanChargeNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<PayPlanCharge> listNew, List<PayPlanCharge> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<PayPlanCharge>();
        var listUpdNew = new List<PayPlanCharge>();
        var listUpdDB = new List<PayPlanCharge>();
        var listDel = new List<PayPlanCharge>();
        listNew.Sort((x, y) => { return x.PayPlanChargeNum.CompareTo(y.PayPlanChargeNum); });
        listDB.Sort((x, y) => { return x.PayPlanChargeNum.CompareTo(y.PayPlanChargeNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        PayPlanCharge fieldNew;
        PayPlanCharge fieldDB;
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

            if (fieldNew.PayPlanChargeNum < fieldDB.PayPlanChargeNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.PayPlanChargeNum > fieldDB.PayPlanChargeNum)
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
        for (var i = 0; i < listIns.Count; i++) Insert(listIns[i]);
        for (var i = 0; i < listUpdNew.Count; i++)
            if (Update(listUpdNew[i], listUpdDB[i]))
                rowsUpdatedCount++;

        DeleteMany(listDel.Select(x => x.PayPlanChargeNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}