using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class CodeGroupCrud
{
    public static List<CodeGroup> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<CodeGroup> TableToList(DataTable table)
    {
        var retVal = new List<CodeGroup>();
        CodeGroup codeGroup;
        foreach (DataRow row in table.Rows)
        {
            codeGroup = new CodeGroup();
            codeGroup.CodeGroupNum = SIn.Long(row["CodeGroupNum"].ToString());
            codeGroup.GroupName = SIn.String(row["GroupName"].ToString());
            codeGroup.ProcCodes = SIn.String(row["ProcCodes"].ToString());
            codeGroup.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            codeGroup.CodeGroupFixed = (EnumCodeGroupFixed) SIn.Int(row["CodeGroupFixed"].ToString());
            codeGroup.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            codeGroup.ShowInAgeLimit = SIn.Bool(row["ShowInAgeLimit"].ToString());
            retVal.Add(codeGroup);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<CodeGroup> listCodeGroups, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "CodeGroup";
        var table = new DataTable(tableName);
        table.Columns.Add("CodeGroupNum");
        table.Columns.Add("GroupName");
        table.Columns.Add("ProcCodes");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("CodeGroupFixed");
        table.Columns.Add("IsHidden");
        table.Columns.Add("ShowInAgeLimit");
        foreach (var codeGroup in listCodeGroups)
            table.Rows.Add(SOut.Long(codeGroup.CodeGroupNum), codeGroup.GroupName, codeGroup.ProcCodes, SOut.Int(codeGroup.ItemOrder), SOut.Int((int) codeGroup.CodeGroupFixed), SOut.Bool(codeGroup.IsHidden), SOut.Bool(codeGroup.ShowInAgeLimit));
        return table;
    }

    public static long Insert(CodeGroup codeGroup)
    {
        var command = "INSERT INTO codegroup (";

        command += "GroupName,ProcCodes,ItemOrder,CodeGroupFixed,IsHidden,ShowInAgeLimit) VALUES(";

        command +=
            "'" + SOut.String(codeGroup.GroupName) + "',"
            + DbHelper.ParamChar + "paramProcCodes,"
            + SOut.Int(codeGroup.ItemOrder) + ","
            + SOut.Int((int) codeGroup.CodeGroupFixed) + ","
            + SOut.Bool(codeGroup.IsHidden) + ","
            + SOut.Bool(codeGroup.ShowInAgeLimit) + ")";
        if (codeGroup.ProcCodes == null) codeGroup.ProcCodes = "";
        var paramProcCodes = new OdSqlParameter("paramProcCodes", OdDbType.Text, SOut.StringParam(codeGroup.ProcCodes));
        {
            codeGroup.CodeGroupNum = Db.NonQ(command, true, "CodeGroupNum", "codeGroup", paramProcCodes);
        }
        return codeGroup.CodeGroupNum;
    }

    public static void Update(CodeGroup codeGroup)
    {
        var command = "UPDATE codegroup SET "
                      + "GroupName     = '" + SOut.String(codeGroup.GroupName) + "', "
                      + "ProcCodes     =  " + DbHelper.ParamChar + "paramProcCodes, "
                      + "ItemOrder     =  " + SOut.Int(codeGroup.ItemOrder) + ", "
                      + "CodeGroupFixed=  " + SOut.Int((int) codeGroup.CodeGroupFixed) + ", "
                      + "IsHidden      =  " + SOut.Bool(codeGroup.IsHidden) + ", "
                      + "ShowInAgeLimit=  " + SOut.Bool(codeGroup.ShowInAgeLimit) + " "
                      + "WHERE CodeGroupNum = " + SOut.Long(codeGroup.CodeGroupNum);
        if (codeGroup.ProcCodes == null) codeGroup.ProcCodes = "";
        var paramProcCodes = new OdSqlParameter("paramProcCodes", OdDbType.Text, SOut.StringParam(codeGroup.ProcCodes));
        Db.NonQ(command, paramProcCodes);
    }

    public static bool Update(CodeGroup codeGroup, CodeGroup oldCodeGroup)
    {
        var command = "";
        if (codeGroup.GroupName != oldCodeGroup.GroupName)
        {
            if (command != "") command += ",";
            command += "GroupName = '" + SOut.String(codeGroup.GroupName) + "'";
        }

        if (codeGroup.ProcCodes != oldCodeGroup.ProcCodes)
        {
            if (command != "") command += ",";
            command += "ProcCodes = " + DbHelper.ParamChar + "paramProcCodes";
        }

        if (codeGroup.ItemOrder != oldCodeGroup.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(codeGroup.ItemOrder) + "";
        }

        if (codeGroup.CodeGroupFixed != oldCodeGroup.CodeGroupFixed)
        {
            if (command != "") command += ",";
            command += "CodeGroupFixed = " + SOut.Int((int) codeGroup.CodeGroupFixed) + "";
        }

        if (codeGroup.IsHidden != oldCodeGroup.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(codeGroup.IsHidden) + "";
        }

        if (codeGroup.ShowInAgeLimit != oldCodeGroup.ShowInAgeLimit)
        {
            if (command != "") command += ",";
            command += "ShowInAgeLimit = " + SOut.Bool(codeGroup.ShowInAgeLimit) + "";
        }

        if (command == "") return false;
        if (codeGroup.ProcCodes == null) codeGroup.ProcCodes = "";
        var paramProcCodes = new OdSqlParameter("paramProcCodes", OdDbType.Text, SOut.StringParam(codeGroup.ProcCodes));
        command = "UPDATE codegroup SET " + command
                                          + " WHERE CodeGroupNum = " + SOut.Long(codeGroup.CodeGroupNum);
        Db.NonQ(command, paramProcCodes);
        return true;
    }

    public static void DeleteMany(List<long> listCodeGroupNums)
    {
        if (listCodeGroupNums == null || listCodeGroupNums.Count == 0) return;
        var command = "DELETE FROM codegroup "
                      + "WHERE CodeGroupNum IN(" + string.Join(",", listCodeGroupNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<CodeGroup> listNew, List<CodeGroup> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<CodeGroup>();
        var listUpdNew = new List<CodeGroup>();
        var listUpdDB = new List<CodeGroup>();
        var listDel = new List<CodeGroup>();
        listNew.Sort((x, y) => { return x.CodeGroupNum.CompareTo(y.CodeGroupNum); });
        listDB.Sort((x, y) => { return x.CodeGroupNum.CompareTo(y.CodeGroupNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        CodeGroup fieldNew;
        CodeGroup fieldDB;
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

            if (fieldNew.CodeGroupNum < fieldDB.CodeGroupNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.CodeGroupNum > fieldDB.CodeGroupNum)
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

        DeleteMany(listDel.Select(x => x.CodeGroupNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}