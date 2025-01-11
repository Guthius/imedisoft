#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SigButDefCrud
{
    public static SigButDef SelectOne(long sigButDefNum)
    {
        var command = "SELECT * FROM sigbutdef "
                      + "WHERE SigButDefNum = " + SOut.Long(sigButDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SigButDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SigButDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SigButDef> TableToList(DataTable table)
    {
        var retVal = new List<SigButDef>();
        SigButDef sigButDef;
        foreach (DataRow row in table.Rows)
        {
            sigButDef = new SigButDef();
            sigButDef.SigButDefNum = SIn.Long(row["SigButDefNum"].ToString());
            sigButDef.ButtonText = SIn.String(row["ButtonText"].ToString());
            sigButDef.ButtonIndex = SIn.Int(row["ButtonIndex"].ToString());
            sigButDef.SynchIcon = SIn.Byte(row["SynchIcon"].ToString());
            sigButDef.ComputerName = SIn.String(row["ComputerName"].ToString());
            sigButDef.SigElementDefNumUser = SIn.Long(row["SigElementDefNumUser"].ToString());
            sigButDef.SigElementDefNumExtra = SIn.Long(row["SigElementDefNumExtra"].ToString());
            sigButDef.SigElementDefNumMsg = SIn.Long(row["SigElementDefNumMsg"].ToString());
            retVal.Add(sigButDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SigButDef> listSigButDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SigButDef";
        var table = new DataTable(tableName);
        table.Columns.Add("SigButDefNum");
        table.Columns.Add("ButtonText");
        table.Columns.Add("ButtonIndex");
        table.Columns.Add("SynchIcon");
        table.Columns.Add("ComputerName");
        table.Columns.Add("SigElementDefNumUser");
        table.Columns.Add("SigElementDefNumExtra");
        table.Columns.Add("SigElementDefNumMsg");
        foreach (var sigButDef in listSigButDefs)
            table.Rows.Add(SOut.Long(sigButDef.SigButDefNum), sigButDef.ButtonText, SOut.Int(sigButDef.ButtonIndex), SOut.Byte(sigButDef.SynchIcon), sigButDef.ComputerName, SOut.Long(sigButDef.SigElementDefNumUser), SOut.Long(sigButDef.SigElementDefNumExtra), SOut.Long(sigButDef.SigElementDefNumMsg));
        return table;
    }

    public static long Insert(SigButDef sigButDef)
    {
        return Insert(sigButDef, false);
    }

    public static long Insert(SigButDef sigButDef, bool useExistingPK)
    {
        var command = "INSERT INTO sigbutdef (";

        command += "ButtonText,ButtonIndex,SynchIcon,ComputerName,SigElementDefNumUser,SigElementDefNumExtra,SigElementDefNumMsg) VALUES(";

        command +=
            "'" + SOut.String(sigButDef.ButtonText) + "',"
            + SOut.Int(sigButDef.ButtonIndex) + ","
            + SOut.Byte(sigButDef.SynchIcon) + ","
            + "'" + SOut.String(sigButDef.ComputerName) + "',"
            + SOut.Long(sigButDef.SigElementDefNumUser) + ","
            + SOut.Long(sigButDef.SigElementDefNumExtra) + ","
            + SOut.Long(sigButDef.SigElementDefNumMsg) + ")";
        {
            sigButDef.SigButDefNum = Db.NonQ(command, true, "SigButDefNum", "sigButDef");
        }
        return sigButDef.SigButDefNum;
    }

    public static void InsertMany(List<SigButDef> listSigButDefs)
    {
        InsertMany(listSigButDefs, false);
    }

    public static void InsertMany(List<SigButDef> listSigButDefs, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listSigButDefs.Count)
        {
            var sigButDef = listSigButDefs[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO sigbutdef (");
                if (useExistingPK) sbCommands.Append("SigButDefNum,");
                sbCommands.Append("ButtonText,ButtonIndex,SynchIcon,ComputerName,SigElementDefNumUser,SigElementDefNumExtra,SigElementDefNumMsg) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(sigButDef.SigButDefNum));
                sbRow.Append(",");
            }

            sbRow.Append("'" + SOut.String(sigButDef.ButtonText) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int(sigButDef.ButtonIndex));
            sbRow.Append(",");
            sbRow.Append(SOut.Byte(sigButDef.SynchIcon));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(sigButDef.ComputerName) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(sigButDef.SigElementDefNumUser));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(sigButDef.SigElementDefNumExtra));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(sigButDef.SigElementDefNumMsg));
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
                if (index == listSigButDefs.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(SigButDef sigButDef)
    {
        return InsertNoCache(sigButDef, false);
    }

    public static long InsertNoCache(SigButDef sigButDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO sigbutdef (";
        if (isRandomKeys || useExistingPK) command += "SigButDefNum,";
        command += "ButtonText,ButtonIndex,SynchIcon,ComputerName,SigElementDefNumUser,SigElementDefNumExtra,SigElementDefNumMsg) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(sigButDef.SigButDefNum) + ",";
        command +=
            "'" + SOut.String(sigButDef.ButtonText) + "',"
            + SOut.Int(sigButDef.ButtonIndex) + ","
            + SOut.Byte(sigButDef.SynchIcon) + ","
            + "'" + SOut.String(sigButDef.ComputerName) + "',"
            + SOut.Long(sigButDef.SigElementDefNumUser) + ","
            + SOut.Long(sigButDef.SigElementDefNumExtra) + ","
            + SOut.Long(sigButDef.SigElementDefNumMsg) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            sigButDef.SigButDefNum = Db.NonQ(command, true, "SigButDefNum", "sigButDef");
        return sigButDef.SigButDefNum;
    }

    public static void Update(SigButDef sigButDef)
    {
        var command = "UPDATE sigbutdef SET "
                      + "ButtonText           = '" + SOut.String(sigButDef.ButtonText) + "', "
                      + "ButtonIndex          =  " + SOut.Int(sigButDef.ButtonIndex) + ", "
                      + "SynchIcon            =  " + SOut.Byte(sigButDef.SynchIcon) + ", "
                      + "ComputerName         = '" + SOut.String(sigButDef.ComputerName) + "', "
                      + "SigElementDefNumUser =  " + SOut.Long(sigButDef.SigElementDefNumUser) + ", "
                      + "SigElementDefNumExtra=  " + SOut.Long(sigButDef.SigElementDefNumExtra) + ", "
                      + "SigElementDefNumMsg  =  " + SOut.Long(sigButDef.SigElementDefNumMsg) + " "
                      + "WHERE SigButDefNum = " + SOut.Long(sigButDef.SigButDefNum);
        Db.NonQ(command);
    }

    public static bool Update(SigButDef sigButDef, SigButDef oldSigButDef)
    {
        var command = "";
        if (sigButDef.ButtonText != oldSigButDef.ButtonText)
        {
            if (command != "") command += ",";
            command += "ButtonText = '" + SOut.String(sigButDef.ButtonText) + "'";
        }

        if (sigButDef.ButtonIndex != oldSigButDef.ButtonIndex)
        {
            if (command != "") command += ",";
            command += "ButtonIndex = " + SOut.Int(sigButDef.ButtonIndex) + "";
        }

        if (sigButDef.SynchIcon != oldSigButDef.SynchIcon)
        {
            if (command != "") command += ",";
            command += "SynchIcon = " + SOut.Byte(sigButDef.SynchIcon) + "";
        }

        if (sigButDef.ComputerName != oldSigButDef.ComputerName)
        {
            if (command != "") command += ",";
            command += "ComputerName = '" + SOut.String(sigButDef.ComputerName) + "'";
        }

        if (sigButDef.SigElementDefNumUser != oldSigButDef.SigElementDefNumUser)
        {
            if (command != "") command += ",";
            command += "SigElementDefNumUser = " + SOut.Long(sigButDef.SigElementDefNumUser) + "";
        }

        if (sigButDef.SigElementDefNumExtra != oldSigButDef.SigElementDefNumExtra)
        {
            if (command != "") command += ",";
            command += "SigElementDefNumExtra = " + SOut.Long(sigButDef.SigElementDefNumExtra) + "";
        }

        if (sigButDef.SigElementDefNumMsg != oldSigButDef.SigElementDefNumMsg)
        {
            if (command != "") command += ",";
            command += "SigElementDefNumMsg = " + SOut.Long(sigButDef.SigElementDefNumMsg) + "";
        }

        if (command == "") return false;
        command = "UPDATE sigbutdef SET " + command
                                          + " WHERE SigButDefNum = " + SOut.Long(sigButDef.SigButDefNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(SigButDef sigButDef, SigButDef oldSigButDef)
    {
        if (sigButDef.ButtonText != oldSigButDef.ButtonText) return true;
        if (sigButDef.ButtonIndex != oldSigButDef.ButtonIndex) return true;
        if (sigButDef.SynchIcon != oldSigButDef.SynchIcon) return true;
        if (sigButDef.ComputerName != oldSigButDef.ComputerName) return true;
        if (sigButDef.SigElementDefNumUser != oldSigButDef.SigElementDefNumUser) return true;
        if (sigButDef.SigElementDefNumExtra != oldSigButDef.SigElementDefNumExtra) return true;
        if (sigButDef.SigElementDefNumMsg != oldSigButDef.SigElementDefNumMsg) return true;
        return false;
    }

    public static void Delete(long sigButDefNum)
    {
        var command = "DELETE FROM sigbutdef "
                      + "WHERE SigButDefNum = " + SOut.Long(sigButDefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSigButDefNums)
    {
        if (listSigButDefNums == null || listSigButDefNums.Count == 0) return;
        var command = "DELETE FROM sigbutdef "
                      + "WHERE SigButDefNum IN(" + string.Join(",", listSigButDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<SigButDef> listNew, List<SigButDef> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<SigButDef>();
        var listUpdNew = new List<SigButDef>();
        var listUpdDB = new List<SigButDef>();
        var listDel = new List<SigButDef>();
        listNew.Sort((x, y) => { return x.SigButDefNum.CompareTo(y.SigButDefNum); });
        listDB.Sort((x, y) => { return x.SigButDefNum.CompareTo(y.SigButDefNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        SigButDef fieldNew;
        SigButDef fieldDB;
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

            if (fieldNew.SigButDefNum < fieldDB.SigButDefNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.SigButDefNum > fieldDB.SigButDefNum)
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
        InsertMany(listIns);
        for (var i = 0; i < listUpdNew.Count; i++)
            if (Update(listUpdNew[i], listUpdDB[i]))
                rowsUpdatedCount++;

        DeleteMany(listDel.Select(x => x.SigButDefNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}