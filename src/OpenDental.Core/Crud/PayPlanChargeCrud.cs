using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PayPlanChargeCrud
{
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
        foreach (DataRow row in table.Rows)
        {
            var payPlanCharge = new PayPlanCharge
            {
                PayPlanChargeNum = SIn.Long(row["PayPlanChargeNum"].ToString()),
                PayPlanNum = SIn.Long(row["PayPlanNum"].ToString()),
                Guarantor = SIn.Long(row["Guarantor"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                ChargeDate = SIn.Date(row["ChargeDate"].ToString()),
                Principal = SIn.Double(row["Principal"].ToString()),
                Interest = SIn.Double(row["Interest"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                ProvNum = SIn.Long(row["ProvNum"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                ChargeType = (PayPlanChargeType) SIn.Int(row["ChargeType"].ToString()),
                ProcNum = SIn.Long(row["ProcNum"].ToString()),
                SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString()),
                StatementNum = SIn.Long(row["StatementNum"].ToString()),
                FKey = SIn.Long(row["FKey"].ToString()),
                LinkType = (PayPlanLinkType) SIn.Int(row["LinkType"].ToString()),
                IsOffset = SIn.Bool(row["IsOffset"].ToString())
            };
            retVal.Add(payPlanCharge);
        }

        return retVal;
    }

    public static long Insert(PayPlanCharge payPlanCharge)
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
                                                + "NOW()" + ","
                                                //SecDateTEdit can only be set by MySQL
                                                + SOut.Long(payPlanCharge.StatementNum) + ","
                                                + SOut.Long(payPlanCharge.FKey) + ","
                                                + SOut.Int((int) payPlanCharge.LinkType) + ","
                                                + SOut.Bool(payPlanCharge.IsOffset) + ")";
        if (payPlanCharge.Note == null) payPlanCharge.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(payPlanCharge.Note));
        {
            payPlanCharge.PayPlanChargeNum = Db.NonQ(command, true, "PayPlanChargeNum", "payPlanCharge", paramNote);
        }
        return payPlanCharge.PayPlanChargeNum;
    }

    public static void InsertMany(List<PayPlanCharge> listPayPlanCharges, bool useExistingPK = false)
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
            sbRow.Append("NOW()");
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(payPlanCharge.Note));
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(payPlanCharge.Note));
        command = "UPDATE payplancharge SET " + command
                                              + " WHERE PayPlanChargeNum = " + SOut.Long(payPlanCharge.PayPlanChargeNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static void DeleteMany(List<long> listPayPlanChargeNums)
    {
        if (listPayPlanChargeNums == null || listPayPlanChargeNums.Count == 0) return;
        var command = "DELETE FROM payplancharge "
                      + "WHERE PayPlanChargeNum IN(" + string.Join(",", listPayPlanChargeNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static void Sync(List<PayPlanCharge> listNew, List<PayPlanCharge> listDB)
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
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            PayPlanCharge fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            PayPlanCharge fieldDB = null;
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
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return;
    }
}