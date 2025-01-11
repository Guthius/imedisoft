using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ClinicPrefCrud
{
    public static List<ClinicPref> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ClinicPref> TableToList(DataTable table)
    {
        var retVal = new List<ClinicPref>();
        ClinicPref clinicPref;
        foreach (DataRow row in table.Rows)
        {
            clinicPref = new ClinicPref();
            clinicPref.ClinicPrefNum = SIn.Long(row["ClinicPrefNum"].ToString());
            clinicPref.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            var prefName = row["PrefName"].ToString();
            if (prefName == "")
                clinicPref.PrefName = 0;
            else
                try
                {
                    clinicPref.PrefName = (PrefName) Enum.Parse(typeof(PrefName), prefName);
                }
                catch
                {
                    clinicPref.PrefName = 0;
                }

            clinicPref.ValueString = SIn.String(row["ValueString"].ToString());
            retVal.Add(clinicPref);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ClinicPref> listClinicPrefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ClinicPref";
        var table = new DataTable(tableName);
        table.Columns.Add("ClinicPrefNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("PrefName");
        table.Columns.Add("ValueString");
        foreach (var clinicPref in listClinicPrefs)
            table.Rows.Add(SOut.Long(clinicPref.ClinicPrefNum), SOut.Long(clinicPref.ClinicNum), SOut.Int((int) clinicPref.PrefName), clinicPref.ValueString);
        return table;
    }

    public static long Insert(ClinicPref clinicPref)
    {
        var command = "INSERT INTO clinicpref (";

        command += "ClinicNum,PrefName,ValueString) VALUES(";

        command +=
            SOut.Long(clinicPref.ClinicNum) + ","
                                            + "'" + SOut.String(clinicPref.PrefName.ToString()) + "',"
                                            + DbHelper.ParamChar + "paramValueString)";
        if (clinicPref.ValueString == null) clinicPref.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", OdDbType.Text, SOut.StringParam(clinicPref.ValueString));
        {
            clinicPref.ClinicPrefNum = Db.NonQ(command, true, "ClinicPrefNum", "clinicPref", paramValueString);
        }
        return clinicPref.ClinicPrefNum;
    }

    public static void Update(ClinicPref clinicPref)
    {
        var command = "UPDATE clinicpref SET "
                      + "ClinicNum    =  " + SOut.Long(clinicPref.ClinicNum) + ", "
                      + "PrefName     = '" + SOut.String(clinicPref.PrefName.ToString()) + "', "
                      + "ValueString  =  " + DbHelper.ParamChar + "paramValueString "
                      + "WHERE ClinicPrefNum = " + SOut.Long(clinicPref.ClinicPrefNum);
        if (clinicPref.ValueString == null) clinicPref.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", OdDbType.Text, SOut.StringParam(clinicPref.ValueString));
        Db.NonQ(command, paramValueString);
    }

    public static bool Update(ClinicPref clinicPref, ClinicPref oldClinicPref)
    {
        var command = "";
        if (clinicPref.ClinicNum != oldClinicPref.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(clinicPref.ClinicNum) + "";
        }

        if (clinicPref.PrefName != oldClinicPref.PrefName)
        {
            if (command != "") command += ",";
            command += "PrefName = '" + SOut.String(clinicPref.PrefName.ToString()) + "'";
        }

        if (clinicPref.ValueString != oldClinicPref.ValueString)
        {
            if (command != "") command += ",";
            command += "ValueString = " + DbHelper.ParamChar + "paramValueString";
        }

        if (command == "") return false;
        if (clinicPref.ValueString == null) clinicPref.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", OdDbType.Text, SOut.StringParam(clinicPref.ValueString));
        command = "UPDATE clinicpref SET " + command
                                           + " WHERE ClinicPrefNum = " + SOut.Long(clinicPref.ClinicPrefNum);
        Db.NonQ(command, paramValueString);
        return true;
    }

    public static void Delete(long clinicPrefNum)
    {
        var command = "DELETE FROM clinicpref "
                      + "WHERE ClinicPrefNum = " + SOut.Long(clinicPrefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listClinicPrefNums)
    {
        if (listClinicPrefNums == null || listClinicPrefNums.Count == 0) return;
        var command = "DELETE FROM clinicpref "
                      + "WHERE ClinicPrefNum IN(" + string.Join(",", listClinicPrefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<ClinicPref> listNew, List<ClinicPref> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<ClinicPref>();
        var listUpdNew = new List<ClinicPref>();
        var listUpdDB = new List<ClinicPref>();
        var listDel = new List<ClinicPref>();
        listNew.Sort((x, y) => { return x.ClinicPrefNum.CompareTo(y.ClinicPrefNum); });
        listDB.Sort((x, y) => { return x.ClinicPrefNum.CompareTo(y.ClinicPrefNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        ClinicPref fieldNew;
        ClinicPref fieldDB;
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

            if (fieldNew.ClinicPrefNum < fieldDB.ClinicPrefNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.ClinicPrefNum > fieldDB.ClinicPrefNum)
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

        DeleteMany(listDel.Select(x => x.ClinicPrefNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}