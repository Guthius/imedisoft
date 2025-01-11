#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class FeeCrud
{
    public static Fee SelectOne(long feeNum)
    {
        var command = "SELECT * FROM fee "
                      + "WHERE FeeNum = " + SOut.Long(feeNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Fee SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Fee> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Fee> TableToList(DataTable table)
    {
        var retVal = new List<Fee>();
        Fee fee;
        foreach (DataRow row in table.Rows)
        {
            fee = new Fee();
            fee.FeeNum = SIn.Long(row["FeeNum"].ToString());
            fee.Amount = SIn.Double(row["Amount"].ToString());
            fee.OldCode = SIn.String(row["OldCode"].ToString());
            fee.FeeSched = SIn.Long(row["FeeSched"].ToString());
            fee.UseDefaultFee = SIn.Bool(row["UseDefaultFee"].ToString());
            fee.UseDefaultCov = SIn.Bool(row["UseDefaultCov"].ToString());
            fee.CodeNum = SIn.Long(row["CodeNum"].ToString());
            fee.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            fee.ProvNum = SIn.Long(row["ProvNum"].ToString());
            fee.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            fee.SecDateEntry = SIn.Date(row["SecDateEntry"].ToString());
            fee.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            fee.DateEffective = SIn.Date(row["DateEffective"].ToString());
            retVal.Add(fee);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Fee> listFees, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Fee";
        var table = new DataTable(tableName);
        table.Columns.Add("FeeNum");
        table.Columns.Add("Amount");
        table.Columns.Add("OldCode");
        table.Columns.Add("FeeSched");
        table.Columns.Add("UseDefaultFee");
        table.Columns.Add("UseDefaultCov");
        table.Columns.Add("CodeNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("SecUserNumEntry");
        table.Columns.Add("SecDateEntry");
        table.Columns.Add("SecDateTEdit");
        table.Columns.Add("DateEffective");
        foreach (var fee in listFees)
            table.Rows.Add(SOut.Long(fee.FeeNum), SOut.Double(fee.Amount), fee.OldCode, SOut.Long(fee.FeeSched), SOut.Bool(fee.UseDefaultFee), SOut.Bool(fee.UseDefaultCov), SOut.Long(fee.CodeNum), SOut.Long(fee.ClinicNum), SOut.Long(fee.ProvNum), SOut.Long(fee.SecUserNumEntry), SOut.DateT(fee.SecDateEntry, false), SOut.DateT(fee.SecDateTEdit, false), SOut.DateT(fee.DateEffective, false));
        return table;
    }

    public static long Insert(Fee fee)
    {
        return Insert(fee, false);
    }

    public static long Insert(Fee fee, bool useExistingPK)
    {
        var command = "INSERT INTO fee (";

        command += "Amount,OldCode,FeeSched,UseDefaultFee,UseDefaultCov,CodeNum,ClinicNum,ProvNum,SecUserNumEntry,SecDateEntry,DateEffective) VALUES(";

        command +=
            SOut.Double(fee.Amount) + ","
                                    + "'" + SOut.String(fee.OldCode) + "',"
                                    + SOut.Long(fee.FeeSched) + ","
                                    + SOut.Bool(fee.UseDefaultFee) + ","
                                    + SOut.Bool(fee.UseDefaultCov) + ","
                                    + SOut.Long(fee.CodeNum) + ","
                                    + SOut.Long(fee.ClinicNum) + ","
                                    + SOut.Long(fee.ProvNum) + ","
                                    + SOut.Long(fee.SecUserNumEntry) + ","
                                    + DbHelper.Now() + ","
                                    //SecDateTEdit can only be set by MySQL
                                    + SOut.Date(fee.DateEffective) + ")";
        {
            fee.FeeNum = Db.NonQ(command, true, "FeeNum", "fee");
        }
        return fee.FeeNum;
    }

    public static void InsertMany(List<Fee> listFees)
    {
        InsertMany(listFees, false);
    }

    public static void InsertMany(List<Fee> listFees, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listFees.Count)
        {
            var fee = listFees[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO fee (");
                if (useExistingPK) sbCommands.Append("FeeNum,");
                sbCommands.Append("Amount,OldCode,FeeSched,UseDefaultFee,UseDefaultCov,CodeNum,ClinicNum,ProvNum,SecUserNumEntry,SecDateEntry,DateEffective) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(fee.FeeNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Double(fee.Amount));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(fee.OldCode) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(fee.FeeSched));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(fee.UseDefaultFee));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(fee.UseDefaultCov));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(fee.CodeNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(fee.ClinicNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(fee.ProvNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(fee.SecUserNumEntry));
            sbRow.Append(",");
            sbRow.Append(DbHelper.Now());
            sbRow.Append(",");
            //SecDateTEdit can only be set by MySQL
            sbRow.Append(SOut.Date(fee.DateEffective));
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
                if (index == listFees.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(Fee fee)
    {
        return InsertNoCache(fee, false);
    }

    public static long InsertNoCache(Fee fee, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO fee (";
        if (isRandomKeys || useExistingPK) command += "FeeNum,";
        command += "Amount,OldCode,FeeSched,UseDefaultFee,UseDefaultCov,CodeNum,ClinicNum,ProvNum,SecUserNumEntry,SecDateEntry,DateEffective) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(fee.FeeNum) + ",";
        command +=
            SOut.Double(fee.Amount) + ","
                                    + "'" + SOut.String(fee.OldCode) + "',"
                                    + SOut.Long(fee.FeeSched) + ","
                                    + SOut.Bool(fee.UseDefaultFee) + ","
                                    + SOut.Bool(fee.UseDefaultCov) + ","
                                    + SOut.Long(fee.CodeNum) + ","
                                    + SOut.Long(fee.ClinicNum) + ","
                                    + SOut.Long(fee.ProvNum) + ","
                                    + SOut.Long(fee.SecUserNumEntry) + ","
                                    + DbHelper.Now() + ","
                                    //SecDateTEdit can only be set by MySQL
                                    + SOut.Date(fee.DateEffective) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            fee.FeeNum = Db.NonQ(command, true, "FeeNum", "fee");
        return fee.FeeNum;
    }

    public static void Update(Fee fee)
    {
        var command = "UPDATE fee SET "
                      + "Amount         =  " + SOut.Double(fee.Amount) + ", "
                      + "OldCode        = '" + SOut.String(fee.OldCode) + "', "
                      + "FeeSched       =  " + SOut.Long(fee.FeeSched) + ", "
                      + "UseDefaultFee  =  " + SOut.Bool(fee.UseDefaultFee) + ", "
                      + "UseDefaultCov  =  " + SOut.Bool(fee.UseDefaultCov) + ", "
                      + "CodeNum        =  " + SOut.Long(fee.CodeNum) + ", "
                      + "ClinicNum      =  " + SOut.Long(fee.ClinicNum) + ", "
                      + "ProvNum        =  " + SOut.Long(fee.ProvNum) + ", "
                      //SecUserNumEntry excluded from update
                      //SecDateEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "DateEffective  =  " + SOut.Date(fee.DateEffective) + " "
                      + "WHERE FeeNum = " + SOut.Long(fee.FeeNum);
        Db.NonQ(command);
    }

    public static bool Update(Fee fee, Fee oldFee)
    {
        var command = "";
        if (fee.Amount != oldFee.Amount)
        {
            if (command != "") command += ",";
            command += "Amount = " + SOut.Double(fee.Amount) + "";
        }

        if (fee.OldCode != oldFee.OldCode)
        {
            if (command != "") command += ",";
            command += "OldCode = '" + SOut.String(fee.OldCode) + "'";
        }

        if (fee.FeeSched != oldFee.FeeSched)
        {
            if (command != "") command += ",";
            command += "FeeSched = " + SOut.Long(fee.FeeSched) + "";
        }

        if (fee.UseDefaultFee != oldFee.UseDefaultFee)
        {
            if (command != "") command += ",";
            command += "UseDefaultFee = " + SOut.Bool(fee.UseDefaultFee) + "";
        }

        if (fee.UseDefaultCov != oldFee.UseDefaultCov)
        {
            if (command != "") command += ",";
            command += "UseDefaultCov = " + SOut.Bool(fee.UseDefaultCov) + "";
        }

        if (fee.CodeNum != oldFee.CodeNum)
        {
            if (command != "") command += ",";
            command += "CodeNum = " + SOut.Long(fee.CodeNum) + "";
        }

        if (fee.ClinicNum != oldFee.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(fee.ClinicNum) + "";
        }

        if (fee.ProvNum != oldFee.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(fee.ProvNum) + "";
        }

        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (fee.DateEffective.Date != oldFee.DateEffective.Date)
        {
            if (command != "") command += ",";
            command += "DateEffective = " + SOut.Date(fee.DateEffective) + "";
        }

        if (command == "") return false;
        command = "UPDATE fee SET " + command
                                    + " WHERE FeeNum = " + SOut.Long(fee.FeeNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Fee fee, Fee oldFee)
    {
        if (fee.Amount != oldFee.Amount) return true;
        if (fee.OldCode != oldFee.OldCode) return true;
        if (fee.FeeSched != oldFee.FeeSched) return true;
        if (fee.UseDefaultFee != oldFee.UseDefaultFee) return true;
        if (fee.UseDefaultCov != oldFee.UseDefaultCov) return true;
        if (fee.CodeNum != oldFee.CodeNum) return true;
        if (fee.ClinicNum != oldFee.ClinicNum) return true;
        if (fee.ProvNum != oldFee.ProvNum) return true;
        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (fee.DateEffective.Date != oldFee.DateEffective.Date) return true;
        return false;
    }

    public static void Delete(long feeNum)
    {
        ClearFkey(feeNum);
        var command = "DELETE FROM fee "
                      + "WHERE FeeNum = " + SOut.Long(feeNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listFeeNums)
    {
        if (listFeeNums == null || listFeeNums.Count == 0) return;
        ClearFkey(listFeeNums);
        var command = "DELETE FROM fee "
                      + "WHERE FeeNum IN(" + string.Join(",", listFeeNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<Fee> listNew, List<Fee> listDB, long userNum)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<Fee>();
        var listUpdNew = new List<Fee>();
        var listUpdDB = new List<Fee>();
        var listDel = new List<Fee>();
        listNew.Sort((x, y) => { return x.FeeNum.CompareTo(y.FeeNum); });
        listDB.Sort((x, y) => { return x.FeeNum.CompareTo(y.FeeNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        Fee fieldNew;
        Fee fieldDB;
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

            if (fieldNew.FeeNum < fieldDB.FeeNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.FeeNum > fieldDB.FeeNum)
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
        listIns.ForEach(x => x.SecUserNumEntry = userNum);
        InsertMany(listIns);
        for (var i = 0; i < listUpdNew.Count; i++)
            if (Update(listUpdNew[i], listUpdDB[i]))
                rowsUpdatedCount++;

        DeleteMany(listDel.Select(x => x.FeeNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }


    public static void ClearFkey(long feeNum)
    {
        if (feeNum == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey=" + SOut.Long(feeNum) + " AND PermType IN (154)";
        Db.NonQ(command);
    }


    public static void ClearFkey(List<long> listFeeNums)
    {
        if (listFeeNums == null || listFeeNums.FindAll(x => x != 0).Count == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey IN(" + string.Join(",", listFeeNums.FindAll(x => x != 0)) + ") AND PermType IN (154)";
        Db.NonQ(command);
    }
}