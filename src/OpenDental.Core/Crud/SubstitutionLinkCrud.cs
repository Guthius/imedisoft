#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SubstitutionLinkCrud
{
    public static SubstitutionLink SelectOne(long substitutionLinkNum)
    {
        var command = "SELECT * FROM substitutionlink "
                      + "WHERE SubstitutionLinkNum = " + SOut.Long(substitutionLinkNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SubstitutionLink SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SubstitutionLink> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SubstitutionLink> TableToList(DataTable table)
    {
        var retVal = new List<SubstitutionLink>();
        SubstitutionLink substitutionLink;
        foreach (DataRow row in table.Rows)
        {
            substitutionLink = new SubstitutionLink();
            substitutionLink.SubstitutionLinkNum = SIn.Long(row["SubstitutionLinkNum"].ToString());
            substitutionLink.PlanNum = SIn.Long(row["PlanNum"].ToString());
            substitutionLink.CodeNum = SIn.Long(row["CodeNum"].ToString());
            substitutionLink.SubstitutionCode = SIn.String(row["SubstitutionCode"].ToString());
            substitutionLink.SubstOnlyIf = (SubstitutionCondition) SIn.Int(row["SubstOnlyIf"].ToString());
            retVal.Add(substitutionLink);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SubstitutionLink> listSubstitutionLinks, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SubstitutionLink";
        var table = new DataTable(tableName);
        table.Columns.Add("SubstitutionLinkNum");
        table.Columns.Add("PlanNum");
        table.Columns.Add("CodeNum");
        table.Columns.Add("SubstitutionCode");
        table.Columns.Add("SubstOnlyIf");
        foreach (var substitutionLink in listSubstitutionLinks)
            table.Rows.Add(SOut.Long(substitutionLink.SubstitutionLinkNum), SOut.Long(substitutionLink.PlanNum), SOut.Long(substitutionLink.CodeNum), substitutionLink.SubstitutionCode, SOut.Int((int) substitutionLink.SubstOnlyIf));
        return table;
    }

    public static long Insert(SubstitutionLink substitutionLink)
    {
        return Insert(substitutionLink, false);
    }

    public static long Insert(SubstitutionLink substitutionLink, bool useExistingPK)
    {
        var command = "INSERT INTO substitutionlink (";

        command += "PlanNum,CodeNum,SubstitutionCode,SubstOnlyIf) VALUES(";

        command +=
            SOut.Long(substitutionLink.PlanNum) + ","
                                                + SOut.Long(substitutionLink.CodeNum) + ","
                                                + "'" + SOut.String(substitutionLink.SubstitutionCode) + "',"
                                                + SOut.Int((int) substitutionLink.SubstOnlyIf) + ")";
        {
            substitutionLink.SubstitutionLinkNum = Db.NonQ(command, true, "SubstitutionLinkNum", "substitutionLink");
        }
        return substitutionLink.SubstitutionLinkNum;
    }

    public static void InsertMany(List<SubstitutionLink> listSubstitutionLinks)
    {
        InsertMany(listSubstitutionLinks, false);
    }

    public static void InsertMany(List<SubstitutionLink> listSubstitutionLinks, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listSubstitutionLinks.Count)
        {
            var substitutionLink = listSubstitutionLinks[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO substitutionlink (");
                if (useExistingPK) sbCommands.Append("SubstitutionLinkNum,");
                sbCommands.Append("PlanNum,CodeNum,SubstitutionCode,SubstOnlyIf) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(substitutionLink.SubstitutionLinkNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(substitutionLink.PlanNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(substitutionLink.CodeNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(substitutionLink.SubstitutionCode) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) substitutionLink.SubstOnlyIf));
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
                if (index == listSubstitutionLinks.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(SubstitutionLink substitutionLink)
    {
        return InsertNoCache(substitutionLink, false);
    }

    public static long InsertNoCache(SubstitutionLink substitutionLink, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO substitutionlink (";
        if (isRandomKeys || useExistingPK) command += "SubstitutionLinkNum,";
        command += "PlanNum,CodeNum,SubstitutionCode,SubstOnlyIf) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(substitutionLink.SubstitutionLinkNum) + ",";
        command +=
            SOut.Long(substitutionLink.PlanNum) + ","
                                                + SOut.Long(substitutionLink.CodeNum) + ","
                                                + "'" + SOut.String(substitutionLink.SubstitutionCode) + "',"
                                                + SOut.Int((int) substitutionLink.SubstOnlyIf) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            substitutionLink.SubstitutionLinkNum = Db.NonQ(command, true, "SubstitutionLinkNum", "substitutionLink");
        return substitutionLink.SubstitutionLinkNum;
    }

    public static void Update(SubstitutionLink substitutionLink)
    {
        var command = "UPDATE substitutionlink SET "
                      + "PlanNum            =  " + SOut.Long(substitutionLink.PlanNum) + ", "
                      + "CodeNum            =  " + SOut.Long(substitutionLink.CodeNum) + ", "
                      + "SubstitutionCode   = '" + SOut.String(substitutionLink.SubstitutionCode) + "', "
                      + "SubstOnlyIf        =  " + SOut.Int((int) substitutionLink.SubstOnlyIf) + " "
                      + "WHERE SubstitutionLinkNum = " + SOut.Long(substitutionLink.SubstitutionLinkNum);
        Db.NonQ(command);
    }

    public static bool Update(SubstitutionLink substitutionLink, SubstitutionLink oldSubstitutionLink)
    {
        var command = "";
        if (substitutionLink.PlanNum != oldSubstitutionLink.PlanNum)
        {
            if (command != "") command += ",";
            command += "PlanNum = " + SOut.Long(substitutionLink.PlanNum) + "";
        }

        if (substitutionLink.CodeNum != oldSubstitutionLink.CodeNum)
        {
            if (command != "") command += ",";
            command += "CodeNum = " + SOut.Long(substitutionLink.CodeNum) + "";
        }

        if (substitutionLink.SubstitutionCode != oldSubstitutionLink.SubstitutionCode)
        {
            if (command != "") command += ",";
            command += "SubstitutionCode = '" + SOut.String(substitutionLink.SubstitutionCode) + "'";
        }

        if (substitutionLink.SubstOnlyIf != oldSubstitutionLink.SubstOnlyIf)
        {
            if (command != "") command += ",";
            command += "SubstOnlyIf = " + SOut.Int((int) substitutionLink.SubstOnlyIf) + "";
        }

        if (command == "") return false;
        command = "UPDATE substitutionlink SET " + command
                                                 + " WHERE SubstitutionLinkNum = " + SOut.Long(substitutionLink.SubstitutionLinkNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(SubstitutionLink substitutionLink, SubstitutionLink oldSubstitutionLink)
    {
        if (substitutionLink.PlanNum != oldSubstitutionLink.PlanNum) return true;
        if (substitutionLink.CodeNum != oldSubstitutionLink.CodeNum) return true;
        if (substitutionLink.SubstitutionCode != oldSubstitutionLink.SubstitutionCode) return true;
        if (substitutionLink.SubstOnlyIf != oldSubstitutionLink.SubstOnlyIf) return true;
        return false;
    }

    public static void Delete(long substitutionLinkNum)
    {
        var command = "DELETE FROM substitutionlink "
                      + "WHERE SubstitutionLinkNum = " + SOut.Long(substitutionLinkNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSubstitutionLinkNums)
    {
        if (listSubstitutionLinkNums == null || listSubstitutionLinkNums.Count == 0) return;
        var command = "DELETE FROM substitutionlink "
                      + "WHERE SubstitutionLinkNum IN(" + string.Join(",", listSubstitutionLinkNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<SubstitutionLink> listNew, List<SubstitutionLink> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<SubstitutionLink>();
        var listUpdNew = new List<SubstitutionLink>();
        var listUpdDB = new List<SubstitutionLink>();
        var listDel = new List<SubstitutionLink>();
        listNew.Sort((x, y) => { return x.SubstitutionLinkNum.CompareTo(y.SubstitutionLinkNum); });
        listDB.Sort((x, y) => { return x.SubstitutionLinkNum.CompareTo(y.SubstitutionLinkNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        SubstitutionLink fieldNew;
        SubstitutionLink fieldDB;
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

            if (fieldNew.SubstitutionLinkNum < fieldDB.SubstitutionLinkNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.SubstitutionLinkNum > fieldDB.SubstitutionLinkNum)
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

        DeleteMany(listDel.Select(x => x.SubstitutionLinkNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}