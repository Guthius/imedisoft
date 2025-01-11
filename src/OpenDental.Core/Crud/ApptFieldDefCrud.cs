using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ApptFieldDefCrud
{
    public static List<ApptFieldDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ApptFieldDef> TableToList(DataTable table)
    {
        var retVal = new List<ApptFieldDef>();
        ApptFieldDef apptFieldDef;
        foreach (DataRow row in table.Rows)
        {
            apptFieldDef = new ApptFieldDef();
            apptFieldDef.ApptFieldDefNum = SIn.Long(row["ApptFieldDefNum"].ToString());
            apptFieldDef.FieldName = SIn.String(row["FieldName"].ToString());
            apptFieldDef.FieldType = (ApptFieldType) SIn.Int(row["FieldType"].ToString());
            apptFieldDef.PickList = SIn.String(row["PickList"].ToString());
            apptFieldDef.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            retVal.Add(apptFieldDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ApptFieldDef> listApptFieldDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ApptFieldDef";
        var table = new DataTable(tableName);
        table.Columns.Add("ApptFieldDefNum");
        table.Columns.Add("FieldName");
        table.Columns.Add("FieldType");
        table.Columns.Add("PickList");
        table.Columns.Add("ItemOrder");
        foreach (var apptFieldDef in listApptFieldDefs)
            table.Rows.Add(SOut.Long(apptFieldDef.ApptFieldDefNum), apptFieldDef.FieldName, SOut.Int((int) apptFieldDef.FieldType), apptFieldDef.PickList, SOut.Int(apptFieldDef.ItemOrder));
        return table;
    }

    public static long Insert(ApptFieldDef apptFieldDef)
    {
        var command = "INSERT INTO apptfielddef (";

        command += "FieldName,FieldType,PickList,ItemOrder) VALUES(";

        command +=
            "'" + SOut.String(apptFieldDef.FieldName) + "',"
            + SOut.Int((int) apptFieldDef.FieldType) + ","
            + DbHelper.ParamChar + "paramPickList,"
            + SOut.Int(apptFieldDef.ItemOrder) + ")";
        if (apptFieldDef.PickList == null) apptFieldDef.PickList = "";
        var paramPickList = new OdSqlParameter("paramPickList", OdDbType.Text, SOut.StringParam(apptFieldDef.PickList));
        {
            apptFieldDef.ApptFieldDefNum = Db.NonQ(command, true, "ApptFieldDefNum", "apptFieldDef", paramPickList);
        }
        return apptFieldDef.ApptFieldDefNum;
    }

    public static void Update(ApptFieldDef apptFieldDef)
    {
        var command = "UPDATE apptfielddef SET "
                      + "FieldName      = '" + SOut.String(apptFieldDef.FieldName) + "', "
                      + "FieldType      =  " + SOut.Int((int) apptFieldDef.FieldType) + ", "
                      + "PickList       =  " + DbHelper.ParamChar + "paramPickList, "
                      + "ItemOrder      =  " + SOut.Int(apptFieldDef.ItemOrder) + " "
                      + "WHERE ApptFieldDefNum = " + SOut.Long(apptFieldDef.ApptFieldDefNum);
        if (apptFieldDef.PickList == null) apptFieldDef.PickList = "";
        var paramPickList = new OdSqlParameter("paramPickList", OdDbType.Text, SOut.StringParam(apptFieldDef.PickList));
        Db.NonQ(command, paramPickList);
    }

    public static bool Update(ApptFieldDef apptFieldDef, ApptFieldDef oldApptFieldDef)
    {
        var command = "";
        if (apptFieldDef.FieldName != oldApptFieldDef.FieldName)
        {
            if (command != "") command += ",";
            command += "FieldName = '" + SOut.String(apptFieldDef.FieldName) + "'";
        }

        if (apptFieldDef.FieldType != oldApptFieldDef.FieldType)
        {
            if (command != "") command += ",";
            command += "FieldType = " + SOut.Int((int) apptFieldDef.FieldType) + "";
        }

        if (apptFieldDef.PickList != oldApptFieldDef.PickList)
        {
            if (command != "") command += ",";
            command += "PickList = " + DbHelper.ParamChar + "paramPickList";
        }

        if (apptFieldDef.ItemOrder != oldApptFieldDef.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(apptFieldDef.ItemOrder) + "";
        }

        if (command == "") return false;
        if (apptFieldDef.PickList == null) apptFieldDef.PickList = "";
        var paramPickList = new OdSqlParameter("paramPickList", OdDbType.Text, SOut.StringParam(apptFieldDef.PickList));
        command = "UPDATE apptfielddef SET " + command
                                             + " WHERE ApptFieldDefNum = " + SOut.Long(apptFieldDef.ApptFieldDefNum);
        Db.NonQ(command, paramPickList);
        return true;
    }

    public static void DeleteMany(List<long> listApptFieldDefNums)
    {
        if (listApptFieldDefNums == null || listApptFieldDefNums.Count == 0) return;
        var command = "DELETE FROM apptfielddef "
                      + "WHERE ApptFieldDefNum IN(" + string.Join(",", listApptFieldDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<ApptFieldDef> listNew, List<ApptFieldDef> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<ApptFieldDef>();
        var listUpdNew = new List<ApptFieldDef>();
        var listUpdDB = new List<ApptFieldDef>();
        var listDel = new List<ApptFieldDef>();
        listNew.Sort((x, y) => { return x.ApptFieldDefNum.CompareTo(y.ApptFieldDefNum); });
        listDB.Sort((x, y) => { return x.ApptFieldDefNum.CompareTo(y.ApptFieldDefNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        ApptFieldDef fieldNew;
        ApptFieldDef fieldDB;
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

            if (fieldNew.ApptFieldDefNum < fieldDB.ApptFieldDefNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.ApptFieldDefNum > fieldDB.ApptFieldDefNum)
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

        DeleteMany(listDel.Select(x => x.ApptFieldDefNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}