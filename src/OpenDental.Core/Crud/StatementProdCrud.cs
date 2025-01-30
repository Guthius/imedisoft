using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class StatementProdCrud
{
    public static List<StatementProd> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<StatementProd> TableToList(DataTable table)
    {
        var retVal = new List<StatementProd>();
        StatementProd statementProd;
        foreach (DataRow row in table.Rows)
        {
            statementProd = new StatementProd();
            statementProd.StatementProdNum = SIn.Long(row["StatementProdNum"].ToString());
            statementProd.StatementNum = SIn.Long(row["StatementNum"].ToString());
            statementProd.DocNum = SIn.Long(row["DocNum"].ToString());
            statementProd.FKey = SIn.Long(row["FKey"].ToString());
            statementProd.ProdType = (ProductionType) SIn.Int(row["ProdType"].ToString());
            statementProd.LateChargeAdjNum = SIn.Long(row["LateChargeAdjNum"].ToString());
            retVal.Add(statementProd);
        }

        return retVal;
    }

    public static void Insert(StatementProd statementProd)
    {
        var command = "INSERT INTO statementprod (";

        command += "StatementNum,DocNum,FKey,ProdType,LateChargeAdjNum) VALUES(";
        command +=
            SOut.Long(statementProd.StatementNum) + ","
                                                  + SOut.Long(statementProd.DocNum) + ","
                                                  + SOut.Long(statementProd.FKey) + ","
                                                  + SOut.Int((int) statementProd.ProdType) + ","
                                                  + SOut.Long(statementProd.LateChargeAdjNum) + ")";
        statementProd.StatementProdNum = Db.NonQ(command, true, "StatementProdNum", "statementProd");
    }

    public static bool Update(StatementProd statementProd, StatementProd oldStatementProd)
    {
        var command = "";
        if (statementProd.StatementNum != oldStatementProd.StatementNum)
        {
            if (command != "") command += ",";

            command += "StatementNum = " + SOut.Long(statementProd.StatementNum) + "";
        }

        if (statementProd.DocNum != oldStatementProd.DocNum)
        {
            if (command != "") command += ",";

            command += "DocNum = " + SOut.Long(statementProd.DocNum) + "";
        }

        if (statementProd.FKey != oldStatementProd.FKey)
        {
            if (command != "") command += ",";

            command += "FKey = " + SOut.Long(statementProd.FKey) + "";
        }

        if (statementProd.ProdType != oldStatementProd.ProdType)
        {
            if (command != "") command += ",";

            command += "ProdType = " + SOut.Int((int) statementProd.ProdType) + "";
        }

        if (statementProd.LateChargeAdjNum != oldStatementProd.LateChargeAdjNum)
        {
            if (command != "") command += ",";

            command += "LateChargeAdjNum = " + SOut.Long(statementProd.LateChargeAdjNum) + "";
        }

        if (command == "") return false;

        command = "UPDATE statementprod SET " + command
                                              + " WHERE StatementProdNum = " + SOut.Long(statementProd.StatementProdNum);
        Db.NonQ(command);
        return true;
    }

    public static void DeleteMany(List<long> listStatementProdNums)
    {
        if (listStatementProdNums == null || listStatementProdNums.Count == 0) return;

        var command = "DELETE FROM statementprod "
                      + "WHERE StatementProdNum IN(" + string.Join(",", listStatementProdNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static void Sync(List<StatementProd> listNew, List<StatementProd> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<StatementProd>();
        var listUpdNew = new List<StatementProd>();
        var listUpdDB = new List<StatementProd>();
        var listDel = new List<StatementProd>();
        listNew.Sort((x, y) => { return x.StatementProdNum.CompareTo(y.StatementProdNum); });
        listDB.Sort((x, y) => { return x.StatementProdNum.CompareTo(y.StatementProdNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        StatementProd fieldNew;
        StatementProd fieldDB;
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

            if (fieldNew.StatementProdNum < fieldDB.StatementProdNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.StatementProdNum > fieldDB.StatementProdNum)
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

        DeleteMany(listDel.Select(x => x.StatementProdNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return;
    }
}