#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PatFieldDefCrud
{
    public static PatFieldDef SelectOne(long patFieldDefNum)
    {
        var command = "SELECT * FROM patfielddef "
                      + "WHERE PatFieldDefNum = " + SOut.Long(patFieldDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PatFieldDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PatFieldDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PatFieldDef> TableToList(DataTable table)
    {
        var retVal = new List<PatFieldDef>();
        PatFieldDef patFieldDef;
        foreach (DataRow row in table.Rows)
        {
            patFieldDef = new PatFieldDef();
            patFieldDef.PatFieldDefNum = SIn.Long(row["PatFieldDefNum"].ToString());
            patFieldDef.FieldName = SIn.String(row["FieldName"].ToString());
            patFieldDef.FieldType = (PatFieldType) SIn.Int(row["FieldType"].ToString());
            patFieldDef.PickList = SIn.String(row["PickList"].ToString());
            patFieldDef.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            patFieldDef.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            retVal.Add(patFieldDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PatFieldDef> listPatFieldDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PatFieldDef";
        var table = new DataTable(tableName);
        table.Columns.Add("PatFieldDefNum");
        table.Columns.Add("FieldName");
        table.Columns.Add("FieldType");
        table.Columns.Add("PickList");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("IsHidden");
        foreach (var patFieldDef in listPatFieldDefs)
            table.Rows.Add(SOut.Long(patFieldDef.PatFieldDefNum), patFieldDef.FieldName, SOut.Int((int) patFieldDef.FieldType), patFieldDef.PickList, SOut.Int(patFieldDef.ItemOrder), SOut.Bool(patFieldDef.IsHidden));
        return table;
    }

    public static long Insert(PatFieldDef patFieldDef)
    {
        return Insert(patFieldDef, false);
    }

    public static long Insert(PatFieldDef patFieldDef, bool useExistingPK)
    {
        var command = "INSERT INTO patfielddef (";

        command += "FieldName,FieldType,PickList,ItemOrder,IsHidden) VALUES(";

        command +=
            "'" + SOut.String(patFieldDef.FieldName) + "',"
            + SOut.Int((int) patFieldDef.FieldType) + ","
            + DbHelper.ParamChar + "paramPickList,"
            + SOut.Int(patFieldDef.ItemOrder) + ","
            + SOut.Bool(patFieldDef.IsHidden) + ")";
        if (patFieldDef.PickList == null) patFieldDef.PickList = "";
        var paramPickList = new OdSqlParameter("paramPickList", OdDbType.Text, SOut.StringParam(patFieldDef.PickList));
        {
            patFieldDef.PatFieldDefNum = Db.NonQ(command, true, "PatFieldDefNum", "patFieldDef", paramPickList);
        }
        return patFieldDef.PatFieldDefNum;
    }

    public static long InsertNoCache(PatFieldDef patFieldDef)
    {
        return InsertNoCache(patFieldDef, false);
    }

    public static long InsertNoCache(PatFieldDef patFieldDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO patfielddef (";
        if (isRandomKeys || useExistingPK) command += "PatFieldDefNum,";
        command += "FieldName,FieldType,PickList,ItemOrder,IsHidden) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(patFieldDef.PatFieldDefNum) + ",";
        command +=
            "'" + SOut.String(patFieldDef.FieldName) + "',"
            + SOut.Int((int) patFieldDef.FieldType) + ","
            + DbHelper.ParamChar + "paramPickList,"
            + SOut.Int(patFieldDef.ItemOrder) + ","
            + SOut.Bool(patFieldDef.IsHidden) + ")";
        if (patFieldDef.PickList == null) patFieldDef.PickList = "";
        var paramPickList = new OdSqlParameter("paramPickList", OdDbType.Text, SOut.StringParam(patFieldDef.PickList));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramPickList);
        else
            patFieldDef.PatFieldDefNum = Db.NonQ(command, true, "PatFieldDefNum", "patFieldDef", paramPickList);
        return patFieldDef.PatFieldDefNum;
    }

    public static void Update(PatFieldDef patFieldDef)
    {
        var command = "UPDATE patfielddef SET "
                      + "FieldName     = '" + SOut.String(patFieldDef.FieldName) + "', "
                      + "FieldType     =  " + SOut.Int((int) patFieldDef.FieldType) + ", "
                      + "PickList      =  " + DbHelper.ParamChar + "paramPickList, "
                      + "ItemOrder     =  " + SOut.Int(patFieldDef.ItemOrder) + ", "
                      + "IsHidden      =  " + SOut.Bool(patFieldDef.IsHidden) + " "
                      + "WHERE PatFieldDefNum = " + SOut.Long(patFieldDef.PatFieldDefNum);
        if (patFieldDef.PickList == null) patFieldDef.PickList = "";
        var paramPickList = new OdSqlParameter("paramPickList", OdDbType.Text, SOut.StringParam(patFieldDef.PickList));
        Db.NonQ(command, paramPickList);
    }

    public static bool Update(PatFieldDef patFieldDef, PatFieldDef oldPatFieldDef)
    {
        var command = "";
        if (patFieldDef.FieldName != oldPatFieldDef.FieldName)
        {
            if (command != "") command += ",";
            command += "FieldName = '" + SOut.String(patFieldDef.FieldName) + "'";
        }

        if (patFieldDef.FieldType != oldPatFieldDef.FieldType)
        {
            if (command != "") command += ",";
            command += "FieldType = " + SOut.Int((int) patFieldDef.FieldType) + "";
        }

        if (patFieldDef.PickList != oldPatFieldDef.PickList)
        {
            if (command != "") command += ",";
            command += "PickList = " + DbHelper.ParamChar + "paramPickList";
        }

        if (patFieldDef.ItemOrder != oldPatFieldDef.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(patFieldDef.ItemOrder) + "";
        }

        if (patFieldDef.IsHidden != oldPatFieldDef.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(patFieldDef.IsHidden) + "";
        }

        if (command == "") return false;
        if (patFieldDef.PickList == null) patFieldDef.PickList = "";
        var paramPickList = new OdSqlParameter("paramPickList", OdDbType.Text, SOut.StringParam(patFieldDef.PickList));
        command = "UPDATE patfielddef SET " + command
                                            + " WHERE PatFieldDefNum = " + SOut.Long(patFieldDef.PatFieldDefNum);
        Db.NonQ(command, paramPickList);
        return true;
    }

    public static bool UpdateComparison(PatFieldDef patFieldDef, PatFieldDef oldPatFieldDef)
    {
        if (patFieldDef.FieldName != oldPatFieldDef.FieldName) return true;
        if (patFieldDef.FieldType != oldPatFieldDef.FieldType) return true;
        if (patFieldDef.PickList != oldPatFieldDef.PickList) return true;
        if (patFieldDef.ItemOrder != oldPatFieldDef.ItemOrder) return true;
        if (patFieldDef.IsHidden != oldPatFieldDef.IsHidden) return true;
        return false;
    }

    public static void Delete(long patFieldDefNum)
    {
        var command = "DELETE FROM patfielddef "
                      + "WHERE PatFieldDefNum = " + SOut.Long(patFieldDefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPatFieldDefNums)
    {
        if (listPatFieldDefNums == null || listPatFieldDefNums.Count == 0) return;
        var command = "DELETE FROM patfielddef "
                      + "WHERE PatFieldDefNum IN(" + string.Join(",", listPatFieldDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<PatFieldDef> listNew, List<PatFieldDef> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<PatFieldDef>();
        var listUpdNew = new List<PatFieldDef>();
        var listUpdDB = new List<PatFieldDef>();
        var listDel = new List<PatFieldDef>();
        listNew.Sort((x, y) => { return x.PatFieldDefNum.CompareTo(y.PatFieldDefNum); });
        listDB.Sort((x, y) => { return x.PatFieldDefNum.CompareTo(y.PatFieldDefNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        PatFieldDef fieldNew;
        PatFieldDef fieldDB;
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

            if (fieldNew.PatFieldDefNum < fieldDB.PatFieldDefNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.PatFieldDefNum > fieldDB.PatFieldDefNum)
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

        DeleteMany(listDel.Select(x => x.PatFieldDefNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}