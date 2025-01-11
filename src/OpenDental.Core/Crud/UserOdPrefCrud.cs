using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class UserOdPrefCrud
{
    public static List<UserOdPref> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<UserOdPref> TableToList(DataTable table)
    {
        var retVal = new List<UserOdPref>();
        UserOdPref userOdPref;
        foreach (DataRow row in table.Rows)
        {
            userOdPref = new UserOdPref();
            userOdPref.UserOdPrefNum = SIn.Long(row["UserOdPrefNum"].ToString());
            userOdPref.UserNum = SIn.Long(row["UserNum"].ToString());
            userOdPref.Fkey = SIn.Long(row["Fkey"].ToString());
            userOdPref.FkeyType = (UserOdFkeyType) SIn.Int(row["FkeyType"].ToString());
            userOdPref.ValueString = SIn.String(row["ValueString"].ToString());
            userOdPref.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            retVal.Add(userOdPref);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<UserOdPref> listUserOdPrefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "UserOdPref";
        var table = new DataTable(tableName);
        table.Columns.Add("UserOdPrefNum");
        table.Columns.Add("UserNum");
        table.Columns.Add("Fkey");
        table.Columns.Add("FkeyType");
        table.Columns.Add("ValueString");
        table.Columns.Add("ClinicNum");
        foreach (var userOdPref in listUserOdPrefs)
            table.Rows.Add(SOut.Long(userOdPref.UserOdPrefNum), SOut.Long(userOdPref.UserNum), SOut.Long(userOdPref.Fkey), SOut.Int((int) userOdPref.FkeyType), userOdPref.ValueString, SOut.Long(userOdPref.ClinicNum));
        return table;
    }

    public static long Insert(UserOdPref userOdPref)
    {
        var command = "INSERT INTO userodpref (";

        command += "UserNum,Fkey,FkeyType,ValueString,ClinicNum) VALUES(";

        command +=
            SOut.Long(userOdPref.UserNum) + ","
                                          + SOut.Long(userOdPref.Fkey) + ","
                                          + SOut.Int((int) userOdPref.FkeyType) + ","
                                          + DbHelper.ParamChar + "paramValueString,"
                                          + SOut.Long(userOdPref.ClinicNum) + ")";
        if (userOdPref.ValueString == null) userOdPref.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", OdDbType.Text, SOut.StringParam(userOdPref.ValueString));
        {
            userOdPref.UserOdPrefNum = Db.NonQ(command, true, "UserOdPrefNum", "userOdPref", paramValueString);
        }
        return userOdPref.UserOdPrefNum;
    }

    public static void InsertMany(List<UserOdPref> listUserOdPrefs)
    {
        InsertMany(listUserOdPrefs, false);
    }

    public static void InsertMany(List<UserOdPref> listUserOdPrefs, bool useExistingPk)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listUserOdPrefs.Count)
        {
            var userOdPref = listUserOdPrefs[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO userodpref (");
                if (useExistingPk) sbCommands.Append("UserOdPrefNum,");
                sbCommands.Append("UserNum,Fkey,FkeyType,ValueString,ClinicNum) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPk)
            {
                sbRow.Append(SOut.Long(userOdPref.UserOdPrefNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(userOdPref.UserNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(userOdPref.Fkey));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) userOdPref.FkeyType));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(userOdPref.ValueString) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(userOdPref.ClinicNum));
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
                if (index == listUserOdPrefs.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static void Update(UserOdPref userOdPref)
    {
        var command = "UPDATE userodpref SET "
                      + "UserNum      =  " + SOut.Long(userOdPref.UserNum) + ", "
                      + "Fkey         =  " + SOut.Long(userOdPref.Fkey) + ", "
                      + "FkeyType     =  " + SOut.Int((int) userOdPref.FkeyType) + ", "
                      + "ValueString  =  " + DbHelper.ParamChar + "paramValueString, "
                      + "ClinicNum    =  " + SOut.Long(userOdPref.ClinicNum) + " "
                      + "WHERE UserOdPrefNum = " + SOut.Long(userOdPref.UserOdPrefNum);
        if (userOdPref.ValueString == null) userOdPref.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", OdDbType.Text, SOut.StringParam(userOdPref.ValueString));
        Db.NonQ(command, paramValueString);
    }

    public static bool Update(UserOdPref userOdPref, UserOdPref oldUserOdPref)
    {
        var command = "";
        if (userOdPref.UserNum != oldUserOdPref.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(userOdPref.UserNum) + "";
        }

        if (userOdPref.Fkey != oldUserOdPref.Fkey)
        {
            if (command != "") command += ",";
            command += "Fkey = " + SOut.Long(userOdPref.Fkey) + "";
        }

        if (userOdPref.FkeyType != oldUserOdPref.FkeyType)
        {
            if (command != "") command += ",";
            command += "FkeyType = " + SOut.Int((int) userOdPref.FkeyType) + "";
        }

        if (userOdPref.ValueString != oldUserOdPref.ValueString)
        {
            if (command != "") command += ",";
            command += "ValueString = " + DbHelper.ParamChar + "paramValueString";
        }

        if (userOdPref.ClinicNum != oldUserOdPref.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(userOdPref.ClinicNum) + "";
        }

        if (command == "") return false;
        if (userOdPref.ValueString == null) userOdPref.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", OdDbType.Text, SOut.StringParam(userOdPref.ValueString));
        command = "UPDATE userodpref SET " + command
                                           + " WHERE UserOdPrefNum = " + SOut.Long(userOdPref.UserOdPrefNum);
        Db.NonQ(command, paramValueString);
        return true;
    }

    public static void Delete(long userOdPrefNum)
    {
        var command = "DELETE FROM userodpref "
                      + "WHERE UserOdPrefNum = " + SOut.Long(userOdPrefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listUserOdPrefNums)
    {
        if (listUserOdPrefNums == null || listUserOdPrefNums.Count == 0) return;
        var command = "DELETE FROM userodpref "
                      + "WHERE UserOdPrefNum IN(" + string.Join(",", listUserOdPrefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<UserOdPref> listNew, List<UserOdPref> listDb)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<UserOdPref>();
        var listUpdNew = new List<UserOdPref>();
        var listUpdDb = new List<UserOdPref>();
        var listDel = new List<UserOdPref>();
        listNew.Sort((x, y) => { return x.UserOdPrefNum.CompareTo(y.UserOdPrefNum); });
        listDb.Sort((x, y) => { return x.UserOdPrefNum.CompareTo(y.UserOdPrefNum); });
        var idxNew = 0;
        var idxDb = 0;
        var rowsUpdatedCount = 0;
        UserOdPref fieldNew;
        UserOdPref fieldDb;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDb < listDb.Count)
        {
            fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            fieldDb = null;
            if (idxDb < listDb.Count) fieldDb = listDb[idxDb];
            //begin compare
            if (fieldNew != null && fieldDb == null)
            {
                //listNew has more items, listDB does not.
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew == null && fieldDb != null)
            {
                //listDB has more items, listNew does not.
                listDel.Add(fieldDb);
                idxDb++;
                continue;
            }

            if (fieldNew.UserOdPrefNum < fieldDb.UserOdPrefNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.UserOdPrefNum > fieldDb.UserOdPrefNum)
            {
                //dbPK less than newPK, dbItem is 'next'
                listDel.Add(fieldDb);
                idxDb++;
                continue;
            }

            //Both lists contain the 'next' item, update required
            listUpdNew.Add(fieldNew);
            listUpdDb.Add(fieldDb);
            idxNew++;
            idxDb++;
        }

        //Commit changes to DB
        for (var i = 0; i < listIns.Count; i++) Insert(listIns[i]);
        for (var i = 0; i < listUpdNew.Count; i++)
            if (Update(listUpdNew[i], listUpdDb[i]))
                rowsUpdatedCount++;

        DeleteMany(listDel.Select(x => x.UserOdPrefNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}