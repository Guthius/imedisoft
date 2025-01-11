#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DefLinkCrud
{
    public static DefLink SelectOne(long defLinkNum)
    {
        var command = "SELECT * FROM deflink "
                      + "WHERE DefLinkNum = " + SOut.Long(defLinkNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static DefLink SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<DefLink> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DefLink> TableToList(DataTable table)
    {
        var retVal = new List<DefLink>();
        DefLink defLink;
        foreach (DataRow row in table.Rows)
        {
            defLink = new DefLink();
            defLink.DefLinkNum = SIn.Long(row["DefLinkNum"].ToString());
            defLink.DefNum = SIn.Long(row["DefNum"].ToString());
            defLink.FKey = SIn.Long(row["FKey"].ToString());
            defLink.LinkType = (DefLinkType) SIn.Int(row["LinkType"].ToString());
            retVal.Add(defLink);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<DefLink> listDefLinks, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "DefLink";
        var table = new DataTable(tableName);
        table.Columns.Add("DefLinkNum");
        table.Columns.Add("DefNum");
        table.Columns.Add("FKey");
        table.Columns.Add("LinkType");
        foreach (var defLink in listDefLinks)
            table.Rows.Add(SOut.Long(defLink.DefLinkNum), SOut.Long(defLink.DefNum), SOut.Long(defLink.FKey), SOut.Int((int) defLink.LinkType));
        return table;
    }

    public static long Insert(DefLink defLink)
    {
        return Insert(defLink, false);
    }

    public static long Insert(DefLink defLink, bool useExistingPK)
    {
        var command = "INSERT INTO deflink (";

        command += "DefNum,FKey,LinkType) VALUES(";

        command +=
            SOut.Long(defLink.DefNum) + ","
                                      + SOut.Long(defLink.FKey) + ","
                                      + SOut.Int((int) defLink.LinkType) + ")";
        {
            defLink.DefLinkNum = Db.NonQ(command, true, "DefLinkNum", "defLink");
        }
        return defLink.DefLinkNum;
    }

    public static void InsertMany(List<DefLink> listDefLinks)
    {
        InsertMany(listDefLinks, false);
    }

    public static void InsertMany(List<DefLink> listDefLinks, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listDefLinks.Count)
        {
            var defLink = listDefLinks[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO deflink (");
                if (useExistingPK) sbCommands.Append("DefLinkNum,");
                sbCommands.Append("DefNum,FKey,LinkType) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(defLink.DefLinkNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(defLink.DefNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(defLink.FKey));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) defLink.LinkType));
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
                if (index == listDefLinks.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(DefLink defLink)
    {
        return InsertNoCache(defLink, false);
    }

    public static long InsertNoCache(DefLink defLink, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO deflink (";
        if (isRandomKeys || useExistingPK) command += "DefLinkNum,";
        command += "DefNum,FKey,LinkType) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(defLink.DefLinkNum) + ",";
        command +=
            SOut.Long(defLink.DefNum) + ","
                                      + SOut.Long(defLink.FKey) + ","
                                      + SOut.Int((int) defLink.LinkType) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            defLink.DefLinkNum = Db.NonQ(command, true, "DefLinkNum", "defLink");
        return defLink.DefLinkNum;
    }

    public static void Update(DefLink defLink)
    {
        var command = "UPDATE deflink SET "
                      + "DefNum    =  " + SOut.Long(defLink.DefNum) + ", "
                      + "FKey      =  " + SOut.Long(defLink.FKey) + ", "
                      + "LinkType  =  " + SOut.Int((int) defLink.LinkType) + " "
                      + "WHERE DefLinkNum = " + SOut.Long(defLink.DefLinkNum);
        Db.NonQ(command);
    }

    public static bool Update(DefLink defLink, DefLink oldDefLink)
    {
        var command = "";
        if (defLink.DefNum != oldDefLink.DefNum)
        {
            if (command != "") command += ",";
            command += "DefNum = " + SOut.Long(defLink.DefNum) + "";
        }

        if (defLink.FKey != oldDefLink.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(defLink.FKey) + "";
        }

        if (defLink.LinkType != oldDefLink.LinkType)
        {
            if (command != "") command += ",";
            command += "LinkType = " + SOut.Int((int) defLink.LinkType) + "";
        }

        if (command == "") return false;
        command = "UPDATE deflink SET " + command
                                        + " WHERE DefLinkNum = " + SOut.Long(defLink.DefLinkNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(DefLink defLink, DefLink oldDefLink)
    {
        if (defLink.DefNum != oldDefLink.DefNum) return true;
        if (defLink.FKey != oldDefLink.FKey) return true;
        if (defLink.LinkType != oldDefLink.LinkType) return true;
        return false;
    }

    public static void Delete(long defLinkNum)
    {
        var command = "DELETE FROM deflink "
                      + "WHERE DefLinkNum = " + SOut.Long(defLinkNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDefLinkNums)
    {
        if (listDefLinkNums == null || listDefLinkNums.Count == 0) return;
        var command = "DELETE FROM deflink "
                      + "WHERE DefLinkNum IN(" + string.Join(",", listDefLinkNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<DefLink> listNew, List<DefLink> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<DefLink>();
        var listUpdNew = new List<DefLink>();
        var listUpdDB = new List<DefLink>();
        var listDel = new List<DefLink>();
        listNew.Sort((x, y) => { return x.DefLinkNum.CompareTo(y.DefLinkNum); });
        listDB.Sort((x, y) => { return x.DefLinkNum.CompareTo(y.DefLinkNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        DefLink fieldNew;
        DefLink fieldDB;
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

            if (fieldNew.DefLinkNum < fieldDB.DefLinkNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.DefLinkNum > fieldDB.DefLinkNum)
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

        DeleteMany(listDel.Select(x => x.DefLinkNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}