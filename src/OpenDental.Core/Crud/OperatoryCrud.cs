using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class OperatoryCrud
{
    public static List<Operatory> TableToList(DataTable table)
    {
        var retVal = new List<Operatory>();
        foreach (DataRow row in table.Rows)
        {
            var operatory = new Operatory
            {
                OperatoryNum = SIn.Long(row["OperatoryNum"].ToString()),
                OpName = SIn.String(row["OpName"].ToString()),
                Abbrev = SIn.String(row["Abbrev"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString()),
                IsHidden = SIn.Bool(row["IsHidden"].ToString()),
                ProvDentist = SIn.Long(row["ProvDentist"].ToString()),
                ProvHygienist = SIn.Long(row["ProvHygienist"].ToString()),
                IsHygiene = SIn.Bool(row["IsHygiene"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                SetProspective = SIn.Bool(row["SetProspective"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString()),
                IsWebSched = SIn.Bool(row["IsWebSched"].ToString()),
                IsNewPatAppt = SIn.Bool(row["IsNewPatAppt"].ToString()),
                OperatoryType = SIn.Long(row["OperatoryType"].ToString())
            };
            retVal.Add(operatory);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Operatory> listOperatorys, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Operatory";
        var table = new DataTable(tableName);
        table.Columns.Add("OperatoryNum");
        table.Columns.Add("OpName");
        table.Columns.Add("Abbrev");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("IsHidden");
        table.Columns.Add("ProvDentist");
        table.Columns.Add("ProvHygienist");
        table.Columns.Add("IsHygiene");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("SetProspective");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("IsWebSched");
        table.Columns.Add("IsNewPatAppt");
        table.Columns.Add("OperatoryType");
        foreach (var operatory in listOperatorys)
            table.Rows.Add(SOut.Long(operatory.OperatoryNum), operatory.OpName, operatory.Abbrev, SOut.Int(operatory.ItemOrder), SOut.Bool(operatory.IsHidden), SOut.Long(operatory.ProvDentist), SOut.Long(operatory.ProvHygienist), SOut.Bool(operatory.IsHygiene), SOut.Long(operatory.ClinicNum), SOut.Bool(operatory.SetProspective), SOut.DateTime(operatory.DateTStamp, false), SOut.Bool(operatory.IsWebSched), SOut.Bool(operatory.IsNewPatAppt), SOut.Long(operatory.OperatoryType));
        return table;
    }

    public static void Insert(Operatory operatory)
    {
        var command = "INSERT INTO operatory (";

        command += "OpName,Abbrev,ItemOrder,IsHidden,ProvDentist,ProvHygienist,IsHygiene,ClinicNum,SetProspective,IsWebSched,IsNewPatAppt,OperatoryType) VALUES(";

        command +=
            "'" + SOut.String(operatory.OpName) + "',"
            + "'" + SOut.String(operatory.Abbrev) + "',"
            + SOut.Int(operatory.ItemOrder) + ","
            + SOut.Bool(operatory.IsHidden) + ","
            + SOut.Long(operatory.ProvDentist) + ","
            + SOut.Long(operatory.ProvHygienist) + ","
            + SOut.Bool(operatory.IsHygiene) + ","
            + SOut.Long(operatory.ClinicNum) + ","
            + SOut.Bool(operatory.SetProspective) + ","
            //DateTStamp can only be set by MySQL
            + SOut.Bool(operatory.IsWebSched) + ","
            + SOut.Bool(operatory.IsNewPatAppt) + ","
            + SOut.Long(operatory.OperatoryType) + ")";
        {
            operatory.OperatoryNum = Db.NonQ(command, true, "OperatoryNum", "operatory");
        }
    }

    public static bool Update(Operatory operatory, Operatory oldOperatory)
    {
        var command = "";
        if (operatory.OpName != oldOperatory.OpName)
        {
            if (command != "") command += ",";
            command += "OpName = '" + SOut.String(operatory.OpName) + "'";
        }

        if (operatory.Abbrev != oldOperatory.Abbrev)
        {
            if (command != "") command += ",";
            command += "Abbrev = '" + SOut.String(operatory.Abbrev) + "'";
        }

        if (operatory.ItemOrder != oldOperatory.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(operatory.ItemOrder) + "";
        }

        if (operatory.IsHidden != oldOperatory.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(operatory.IsHidden) + "";
        }

        if (operatory.ProvDentist != oldOperatory.ProvDentist)
        {
            if (command != "") command += ",";
            command += "ProvDentist = " + SOut.Long(operatory.ProvDentist) + "";
        }

        if (operatory.ProvHygienist != oldOperatory.ProvHygienist)
        {
            if (command != "") command += ",";
            command += "ProvHygienist = " + SOut.Long(operatory.ProvHygienist) + "";
        }

        if (operatory.IsHygiene != oldOperatory.IsHygiene)
        {
            if (command != "") command += ",";
            command += "IsHygiene = " + SOut.Bool(operatory.IsHygiene) + "";
        }

        if (operatory.ClinicNum != oldOperatory.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(operatory.ClinicNum) + "";
        }

        if (operatory.SetProspective != oldOperatory.SetProspective)
        {
            if (command != "") command += ",";
            command += "SetProspective = " + SOut.Bool(operatory.SetProspective) + "";
        }

        //DateTStamp can only be set by MySQL
        if (operatory.IsWebSched != oldOperatory.IsWebSched)
        {
            if (command != "") command += ",";
            command += "IsWebSched = " + SOut.Bool(operatory.IsWebSched) + "";
        }

        if (operatory.IsNewPatAppt != oldOperatory.IsNewPatAppt)
        {
            if (command != "") command += ",";
            command += "IsNewPatAppt = " + SOut.Bool(operatory.IsNewPatAppt) + "";
        }

        if (operatory.OperatoryType != oldOperatory.OperatoryType)
        {
            if (command != "") command += ",";
            command += "OperatoryType = " + SOut.Long(operatory.OperatoryType) + "";
        }

        if (command == "") return false;
        command = "UPDATE operatory SET " + command
                                          + " WHERE OperatoryNum = " + SOut.Long(operatory.OperatoryNum);
        Db.NonQ(command);
        return true;
    }

    public static void DeleteMany(List<long> listOperatoryNums)
    {
        if (listOperatoryNums == null || listOperatoryNums.Count == 0) return;
        var command = "DELETE FROM operatory "
                      + "WHERE OperatoryNum IN(" + string.Join(",", listOperatoryNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static void Sync(List<Operatory> listNew, List<Operatory> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<Operatory>();
        var listUpdNew = new List<Operatory>();
        var listUpdDB = new List<Operatory>();
        var listDel = new List<Operatory>();
        listNew.Sort((x, y) => { return x.OperatoryNum.CompareTo(y.OperatoryNum); });
        listDB.Sort((x, y) => { return x.OperatoryNum.CompareTo(y.OperatoryNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            Operatory fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            Operatory fieldDB = null;
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

            if (fieldNew.OperatoryNum < fieldDB.OperatoryNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.OperatoryNum > fieldDB.OperatoryNum)
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

        DeleteMany(listDel.Select(x => x.OperatoryNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return;
    }
}