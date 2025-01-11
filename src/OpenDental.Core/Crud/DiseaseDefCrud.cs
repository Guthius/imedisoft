#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DiseaseDefCrud
{
    public static DiseaseDef SelectOne(long diseaseDefNum)
    {
        var command = "SELECT * FROM diseasedef "
                      + "WHERE DiseaseDefNum = " + SOut.Long(diseaseDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static DiseaseDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<DiseaseDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DiseaseDef> TableToList(DataTable table)
    {
        var retVal = new List<DiseaseDef>();
        DiseaseDef diseaseDef;
        foreach (DataRow row in table.Rows)
        {
            diseaseDef = new DiseaseDef();
            diseaseDef.DiseaseDefNum = SIn.Long(row["DiseaseDefNum"].ToString());
            diseaseDef.DiseaseName = SIn.String(row["DiseaseName"].ToString());
            diseaseDef.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            diseaseDef.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            diseaseDef.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            diseaseDef.ICD9Code = SIn.String(row["ICD9Code"].ToString());
            diseaseDef.SnomedCode = SIn.String(row["SnomedCode"].ToString());
            diseaseDef.Icd10Code = SIn.String(row["Icd10Code"].ToString());
            retVal.Add(diseaseDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<DiseaseDef> listDiseaseDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "DiseaseDef";
        var table = new DataTable(tableName);
        table.Columns.Add("DiseaseDefNum");
        table.Columns.Add("DiseaseName");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("IsHidden");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("ICD9Code");
        table.Columns.Add("SnomedCode");
        table.Columns.Add("Icd10Code");
        foreach (var diseaseDef in listDiseaseDefs)
            table.Rows.Add(SOut.Long(diseaseDef.DiseaseDefNum), diseaseDef.DiseaseName, SOut.Int(diseaseDef.ItemOrder), SOut.Bool(diseaseDef.IsHidden), SOut.DateT(diseaseDef.DateTStamp, false), diseaseDef.ICD9Code, diseaseDef.SnomedCode, diseaseDef.Icd10Code);
        return table;
    }

    public static long Insert(DiseaseDef diseaseDef)
    {
        return Insert(diseaseDef, false);
    }

    public static long Insert(DiseaseDef diseaseDef, bool useExistingPK)
    {
        var command = "INSERT INTO diseasedef (";

        command += "DiseaseName,ItemOrder,IsHidden,ICD9Code,SnomedCode,Icd10Code) VALUES(";

        command +=
            "'" + SOut.StringNote(diseaseDef.DiseaseName, true) + "',"
            + SOut.Int(diseaseDef.ItemOrder) + ","
            + SOut.Bool(diseaseDef.IsHidden) + ","
            //DateTStamp can only be set by MySQL
            + "'" + SOut.String(diseaseDef.ICD9Code) + "',"
            + "'" + SOut.String(diseaseDef.SnomedCode) + "',"
            + "'" + SOut.String(diseaseDef.Icd10Code) + "')";
        {
            diseaseDef.DiseaseDefNum = Db.NonQ(command, true, "DiseaseDefNum", "diseaseDef");
        }
        return diseaseDef.DiseaseDefNum;
    }

    public static long InsertNoCache(DiseaseDef diseaseDef)
    {
        return InsertNoCache(diseaseDef, false);
    }

    public static long InsertNoCache(DiseaseDef diseaseDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO diseasedef (";
        if (isRandomKeys || useExistingPK) command += "DiseaseDefNum,";
        command += "DiseaseName,ItemOrder,IsHidden,ICD9Code,SnomedCode,Icd10Code) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(diseaseDef.DiseaseDefNum) + ",";
        command +=
            "'" + SOut.StringNote(diseaseDef.DiseaseName, true) + "',"
            + SOut.Int(diseaseDef.ItemOrder) + ","
            + SOut.Bool(diseaseDef.IsHidden) + ","
            //DateTStamp can only be set by MySQL
            + "'" + SOut.String(diseaseDef.ICD9Code) + "',"
            + "'" + SOut.String(diseaseDef.SnomedCode) + "',"
            + "'" + SOut.String(diseaseDef.Icd10Code) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            diseaseDef.DiseaseDefNum = Db.NonQ(command, true, "DiseaseDefNum", "diseaseDef");
        return diseaseDef.DiseaseDefNum;
    }

    public static void Update(DiseaseDef diseaseDef)
    {
        var command = "UPDATE diseasedef SET "
                      + "DiseaseName  = '" + SOut.StringNote(diseaseDef.DiseaseName, true) + "', "
                      + "ItemOrder    =  " + SOut.Int(diseaseDef.ItemOrder) + ", "
                      + "IsHidden     =  " + SOut.Bool(diseaseDef.IsHidden) + ", "
                      //DateTStamp can only be set by MySQL
                      + "ICD9Code     = '" + SOut.String(diseaseDef.ICD9Code) + "', "
                      + "SnomedCode   = '" + SOut.String(diseaseDef.SnomedCode) + "', "
                      + "Icd10Code    = '" + SOut.String(diseaseDef.Icd10Code) + "' "
                      + "WHERE DiseaseDefNum = " + SOut.Long(diseaseDef.DiseaseDefNum);
        Db.NonQ(command);
    }

    public static bool Update(DiseaseDef diseaseDef, DiseaseDef oldDiseaseDef)
    {
        var command = "";
        if (diseaseDef.DiseaseName != oldDiseaseDef.DiseaseName)
        {
            if (command != "") command += ",";
            command += "DiseaseName = '" + SOut.StringNote(diseaseDef.DiseaseName, true) + "'";
        }

        if (diseaseDef.ItemOrder != oldDiseaseDef.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(diseaseDef.ItemOrder) + "";
        }

        if (diseaseDef.IsHidden != oldDiseaseDef.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(diseaseDef.IsHidden) + "";
        }

        //DateTStamp can only be set by MySQL
        if (diseaseDef.ICD9Code != oldDiseaseDef.ICD9Code)
        {
            if (command != "") command += ",";
            command += "ICD9Code = '" + SOut.String(diseaseDef.ICD9Code) + "'";
        }

        if (diseaseDef.SnomedCode != oldDiseaseDef.SnomedCode)
        {
            if (command != "") command += ",";
            command += "SnomedCode = '" + SOut.String(diseaseDef.SnomedCode) + "'";
        }

        if (diseaseDef.Icd10Code != oldDiseaseDef.Icd10Code)
        {
            if (command != "") command += ",";
            command += "Icd10Code = '" + SOut.String(diseaseDef.Icd10Code) + "'";
        }

        if (command == "") return false;
        command = "UPDATE diseasedef SET " + command
                                           + " WHERE DiseaseDefNum = " + SOut.Long(diseaseDef.DiseaseDefNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(DiseaseDef diseaseDef, DiseaseDef oldDiseaseDef)
    {
        if (diseaseDef.DiseaseName != oldDiseaseDef.DiseaseName) return true;
        if (diseaseDef.ItemOrder != oldDiseaseDef.ItemOrder) return true;
        if (diseaseDef.IsHidden != oldDiseaseDef.IsHidden) return true;
        //DateTStamp can only be set by MySQL
        if (diseaseDef.ICD9Code != oldDiseaseDef.ICD9Code) return true;
        if (diseaseDef.SnomedCode != oldDiseaseDef.SnomedCode) return true;
        if (diseaseDef.Icd10Code != oldDiseaseDef.Icd10Code) return true;
        return false;
    }

    public static void Delete(long diseaseDefNum)
    {
        var command = "DELETE FROM diseasedef "
                      + "WHERE DiseaseDefNum = " + SOut.Long(diseaseDefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDiseaseDefNums)
    {
        if (listDiseaseDefNums == null || listDiseaseDefNums.Count == 0) return;
        var command = "DELETE FROM diseasedef "
                      + "WHERE DiseaseDefNum IN(" + string.Join(",", listDiseaseDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<DiseaseDef> listNew, List<DiseaseDef> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<DiseaseDef>();
        var listUpdNew = new List<DiseaseDef>();
        var listUpdDB = new List<DiseaseDef>();
        var listDel = new List<DiseaseDef>();
        listNew.Sort((x, y) => { return x.DiseaseDefNum.CompareTo(y.DiseaseDefNum); });
        listDB.Sort((x, y) => { return x.DiseaseDefNum.CompareTo(y.DiseaseDefNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        DiseaseDef fieldNew;
        DiseaseDef fieldDB;
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

            if (fieldNew.DiseaseDefNum < fieldDB.DiseaseDefNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.DiseaseDefNum > fieldDB.DiseaseDefNum)
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

        DeleteMany(listDel.Select(x => x.DiseaseDefNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}