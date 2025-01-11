#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PerioMeasureCrud
{
    public static PerioMeasure SelectOne(long perioMeasureNum)
    {
        var command = "SELECT * FROM periomeasure "
                      + "WHERE PerioMeasureNum = " + SOut.Long(perioMeasureNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PerioMeasure SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PerioMeasure> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PerioMeasure> TableToList(DataTable table)
    {
        var retVal = new List<PerioMeasure>();
        PerioMeasure perioMeasure;
        foreach (DataRow row in table.Rows)
        {
            perioMeasure = new PerioMeasure();
            perioMeasure.PerioMeasureNum = SIn.Long(row["PerioMeasureNum"].ToString());
            perioMeasure.PerioExamNum = SIn.Long(row["PerioExamNum"].ToString());
            perioMeasure.SequenceType = (PerioSequenceType) SIn.Int(row["SequenceType"].ToString());
            perioMeasure.IntTooth = SIn.Int(row["IntTooth"].ToString());
            perioMeasure.ToothValue = SIn.Int(row["ToothValue"].ToString());
            perioMeasure.MBvalue = SIn.Int(row["MBvalue"].ToString());
            perioMeasure.Bvalue = SIn.Int(row["Bvalue"].ToString());
            perioMeasure.DBvalue = SIn.Int(row["DBvalue"].ToString());
            perioMeasure.MLvalue = SIn.Int(row["MLvalue"].ToString());
            perioMeasure.Lvalue = SIn.Int(row["Lvalue"].ToString());
            perioMeasure.DLvalue = SIn.Int(row["DLvalue"].ToString());
            perioMeasure.SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString());
            perioMeasure.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            retVal.Add(perioMeasure);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PerioMeasure> listPerioMeasures, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PerioMeasure";
        var table = new DataTable(tableName);
        table.Columns.Add("PerioMeasureNum");
        table.Columns.Add("PerioExamNum");
        table.Columns.Add("SequenceType");
        table.Columns.Add("IntTooth");
        table.Columns.Add("ToothValue");
        table.Columns.Add("MBvalue");
        table.Columns.Add("Bvalue");
        table.Columns.Add("DBvalue");
        table.Columns.Add("MLvalue");
        table.Columns.Add("Lvalue");
        table.Columns.Add("DLvalue");
        table.Columns.Add("SecDateTEntry");
        table.Columns.Add("SecDateTEdit");
        foreach (var perioMeasure in listPerioMeasures)
            table.Rows.Add(SOut.Long(perioMeasure.PerioMeasureNum), SOut.Long(perioMeasure.PerioExamNum), SOut.Int((int) perioMeasure.SequenceType), SOut.Int(perioMeasure.IntTooth), SOut.Int(perioMeasure.ToothValue), SOut.Int(perioMeasure.MBvalue), SOut.Int(perioMeasure.Bvalue), SOut.Int(perioMeasure.DBvalue), SOut.Int(perioMeasure.MLvalue), SOut.Int(perioMeasure.Lvalue), SOut.Int(perioMeasure.DLvalue), SOut.DateT(perioMeasure.SecDateTEntry, false), SOut.DateT(perioMeasure.SecDateTEdit, false));
        return table;
    }

    public static long Insert(PerioMeasure perioMeasure)
    {
        return Insert(perioMeasure, false);
    }

    public static long Insert(PerioMeasure perioMeasure, bool useExistingPK)
    {
        var command = "INSERT INTO periomeasure (";

        command += "PerioExamNum,SequenceType,IntTooth,ToothValue,MBvalue,Bvalue,DBvalue,MLvalue,Lvalue,DLvalue,SecDateTEntry) VALUES(";

        command +=
            SOut.Long(perioMeasure.PerioExamNum) + ","
                                                 + SOut.Int((int) perioMeasure.SequenceType) + ","
                                                 + SOut.Int(perioMeasure.IntTooth) + ","
                                                 + SOut.Int(perioMeasure.ToothValue) + ","
                                                 + SOut.Int(perioMeasure.MBvalue) + ","
                                                 + SOut.Int(perioMeasure.Bvalue) + ","
                                                 + SOut.Int(perioMeasure.DBvalue) + ","
                                                 + SOut.Int(perioMeasure.MLvalue) + ","
                                                 + SOut.Int(perioMeasure.Lvalue) + ","
                                                 + SOut.Int(perioMeasure.DLvalue) + ","
                                                 + DbHelper.Now() + ")";
        //SecDateTEdit can only be set by MySQL

        perioMeasure.PerioMeasureNum = Db.NonQ(command, true, "PerioMeasureNum", "perioMeasure");
        return perioMeasure.PerioMeasureNum;
    }

    public static void InsertMany(List<PerioMeasure> listPerioMeasures)
    {
        InsertMany(listPerioMeasures, false);
    }

    public static void InsertMany(List<PerioMeasure> listPerioMeasures, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listPerioMeasures.Count)
        {
            var perioMeasure = listPerioMeasures[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO periomeasure (");
                if (useExistingPK) sbCommands.Append("PerioMeasureNum,");
                sbCommands.Append("PerioExamNum,SequenceType,IntTooth,ToothValue,MBvalue,Bvalue,DBvalue,MLvalue,Lvalue,DLvalue,SecDateTEntry) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(perioMeasure.PerioMeasureNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(perioMeasure.PerioExamNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) perioMeasure.SequenceType));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(perioMeasure.IntTooth));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(perioMeasure.ToothValue));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(perioMeasure.MBvalue));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(perioMeasure.Bvalue));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(perioMeasure.DBvalue));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(perioMeasure.MLvalue));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(perioMeasure.Lvalue));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(perioMeasure.DLvalue));
            sbRow.Append(",");
            sbRow.Append(DbHelper.Now());
            sbRow.Append(")");
            //SecDateTEdit can only be set by MySQL
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
                if (index == listPerioMeasures.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(PerioMeasure perioMeasure)
    {
        return InsertNoCache(perioMeasure, false);
    }

    public static long InsertNoCache(PerioMeasure perioMeasure, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO periomeasure (";
        if (isRandomKeys || useExistingPK) command += "PerioMeasureNum,";
        command += "PerioExamNum,SequenceType,IntTooth,ToothValue,MBvalue,Bvalue,DBvalue,MLvalue,Lvalue,DLvalue,SecDateTEntry) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(perioMeasure.PerioMeasureNum) + ",";
        command +=
            SOut.Long(perioMeasure.PerioExamNum) + ","
                                                 + SOut.Int((int) perioMeasure.SequenceType) + ","
                                                 + SOut.Int(perioMeasure.IntTooth) + ","
                                                 + SOut.Int(perioMeasure.ToothValue) + ","
                                                 + SOut.Int(perioMeasure.MBvalue) + ","
                                                 + SOut.Int(perioMeasure.Bvalue) + ","
                                                 + SOut.Int(perioMeasure.DBvalue) + ","
                                                 + SOut.Int(perioMeasure.MLvalue) + ","
                                                 + SOut.Int(perioMeasure.Lvalue) + ","
                                                 + SOut.Int(perioMeasure.DLvalue) + ","
                                                 + DbHelper.Now() + ")";
        //SecDateTEdit can only be set by MySQL
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            perioMeasure.PerioMeasureNum = Db.NonQ(command, true, "PerioMeasureNum", "perioMeasure");
        return perioMeasure.PerioMeasureNum;
    }

    public static void Update(PerioMeasure perioMeasure)
    {
        var command = "UPDATE periomeasure SET "
                      + "PerioExamNum   =  " + SOut.Long(perioMeasure.PerioExamNum) + ", "
                      + "SequenceType   =  " + SOut.Int((int) perioMeasure.SequenceType) + ", "
                      + "IntTooth       =  " + SOut.Int(perioMeasure.IntTooth) + ", "
                      + "ToothValue     =  " + SOut.Int(perioMeasure.ToothValue) + ", "
                      + "MBvalue        =  " + SOut.Int(perioMeasure.MBvalue) + ", "
                      + "Bvalue         =  " + SOut.Int(perioMeasure.Bvalue) + ", "
                      + "DBvalue        =  " + SOut.Int(perioMeasure.DBvalue) + ", "
                      + "MLvalue        =  " + SOut.Int(perioMeasure.MLvalue) + ", "
                      + "Lvalue         =  " + SOut.Int(perioMeasure.Lvalue) + ", "
                      + "DLvalue        =  " + SOut.Int(perioMeasure.DLvalue) + " "
                      //SecDateTEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "WHERE PerioMeasureNum = " + SOut.Long(perioMeasure.PerioMeasureNum);
        Db.NonQ(command);
    }

    public static bool Update(PerioMeasure perioMeasure, PerioMeasure oldPerioMeasure)
    {
        var command = "";
        if (perioMeasure.PerioExamNum != oldPerioMeasure.PerioExamNum)
        {
            if (command != "") command += ",";
            command += "PerioExamNum = " + SOut.Long(perioMeasure.PerioExamNum) + "";
        }

        if (perioMeasure.SequenceType != oldPerioMeasure.SequenceType)
        {
            if (command != "") command += ",";
            command += "SequenceType = " + SOut.Int((int) perioMeasure.SequenceType) + "";
        }

        if (perioMeasure.IntTooth != oldPerioMeasure.IntTooth)
        {
            if (command != "") command += ",";
            command += "IntTooth = " + SOut.Int(perioMeasure.IntTooth) + "";
        }

        if (perioMeasure.ToothValue != oldPerioMeasure.ToothValue)
        {
            if (command != "") command += ",";
            command += "ToothValue = " + SOut.Int(perioMeasure.ToothValue) + "";
        }

        if (perioMeasure.MBvalue != oldPerioMeasure.MBvalue)
        {
            if (command != "") command += ",";
            command += "MBvalue = " + SOut.Int(perioMeasure.MBvalue) + "";
        }

        if (perioMeasure.Bvalue != oldPerioMeasure.Bvalue)
        {
            if (command != "") command += ",";
            command += "Bvalue = " + SOut.Int(perioMeasure.Bvalue) + "";
        }

        if (perioMeasure.DBvalue != oldPerioMeasure.DBvalue)
        {
            if (command != "") command += ",";
            command += "DBvalue = " + SOut.Int(perioMeasure.DBvalue) + "";
        }

        if (perioMeasure.MLvalue != oldPerioMeasure.MLvalue)
        {
            if (command != "") command += ",";
            command += "MLvalue = " + SOut.Int(perioMeasure.MLvalue) + "";
        }

        if (perioMeasure.Lvalue != oldPerioMeasure.Lvalue)
        {
            if (command != "") command += ",";
            command += "Lvalue = " + SOut.Int(perioMeasure.Lvalue) + "";
        }

        if (perioMeasure.DLvalue != oldPerioMeasure.DLvalue)
        {
            if (command != "") command += ",";
            command += "DLvalue = " + SOut.Int(perioMeasure.DLvalue) + "";
        }

        //SecDateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (command == "") return false;
        command = "UPDATE periomeasure SET " + command
                                             + " WHERE PerioMeasureNum = " + SOut.Long(perioMeasure.PerioMeasureNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(PerioMeasure perioMeasure, PerioMeasure oldPerioMeasure)
    {
        if (perioMeasure.PerioExamNum != oldPerioMeasure.PerioExamNum) return true;
        if (perioMeasure.SequenceType != oldPerioMeasure.SequenceType) return true;
        if (perioMeasure.IntTooth != oldPerioMeasure.IntTooth) return true;
        if (perioMeasure.ToothValue != oldPerioMeasure.ToothValue) return true;
        if (perioMeasure.MBvalue != oldPerioMeasure.MBvalue) return true;
        if (perioMeasure.Bvalue != oldPerioMeasure.Bvalue) return true;
        if (perioMeasure.DBvalue != oldPerioMeasure.DBvalue) return true;
        if (perioMeasure.MLvalue != oldPerioMeasure.MLvalue) return true;
        if (perioMeasure.Lvalue != oldPerioMeasure.Lvalue) return true;
        if (perioMeasure.DLvalue != oldPerioMeasure.DLvalue) return true;
        //SecDateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        return false;
    }

    public static void Delete(long perioMeasureNum)
    {
        var command = "DELETE FROM periomeasure "
                      + "WHERE PerioMeasureNum = " + SOut.Long(perioMeasureNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPerioMeasureNums)
    {
        if (listPerioMeasureNums == null || listPerioMeasureNums.Count == 0) return;
        var command = "DELETE FROM periomeasure "
                      + "WHERE PerioMeasureNum IN(" + string.Join(",", listPerioMeasureNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<PerioMeasure> listNew, List<PerioMeasure> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<PerioMeasure>();
        var listUpdNew = new List<PerioMeasure>();
        var listUpdDB = new List<PerioMeasure>();
        var listDel = new List<PerioMeasure>();
        listNew.Sort((x, y) => { return x.PerioMeasureNum.CompareTo(y.PerioMeasureNum); });
        listDB.Sort((x, y) => { return x.PerioMeasureNum.CompareTo(y.PerioMeasureNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        PerioMeasure fieldNew;
        PerioMeasure fieldDB;
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

            if (fieldNew.PerioMeasureNum < fieldDB.PerioMeasureNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.PerioMeasureNum > fieldDB.PerioMeasureNum)
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

        DeleteMany(listDel.Select(x => x.PerioMeasureNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}