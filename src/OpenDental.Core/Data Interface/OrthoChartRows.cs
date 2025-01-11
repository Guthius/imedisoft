using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class OrthoChartRows
{
    #region Methods - Misc

    public static bool Sync(List<OrthoChartRow> listOrthoChartRowsNew, long patNum)
    {
        var listOrthoChartRowsDB = GetAllForPatient(patNum, false);
        //This code is just a straight copy of the Crud sync.  It's here in preparation for possibly adding logging for signature.
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listOrthoChartRowsIns = new List<OrthoChartRow>();
        var listOrthoChartRowsUpdNew = new List<OrthoChartRow>();
        var listOrthoChartRowsUpdDB = new List<OrthoChartRow>();
        var listOrthoChartRowsDel = new List<OrthoChartRow>();
        listOrthoChartRowsNew = listOrthoChartRowsNew.OrderBy(x => x.OrthoChartRowNum).ToList();
        listOrthoChartRowsDB = listOrthoChartRowsDB.OrderBy(x => x.OrthoChartRowNum).ToList();
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        OrthoChartRow orthoChartRowFieldNew;
        OrthoChartRow orthoChartRowFieldDB;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (true)
        {
            if (idxNew >= listOrthoChartRowsNew.Count) break;
            if (idxDB >= listOrthoChartRowsDB.Count) break;
            orthoChartRowFieldNew = null;
            if (idxNew < listOrthoChartRowsNew.Count) orthoChartRowFieldNew = listOrthoChartRowsNew[idxNew];
            orthoChartRowFieldDB = null;
            if (idxDB < listOrthoChartRowsDB.Count) orthoChartRowFieldDB = listOrthoChartRowsDB[idxDB];
            //begin compare
            if (orthoChartRowFieldNew != null && orthoChartRowFieldDB == null)
            {
                //listNew has more items, listDB does not.
                listOrthoChartRowsIns.Add(orthoChartRowFieldNew);
                idxNew++;
                continue;
            }

            if (orthoChartRowFieldNew == null && orthoChartRowFieldDB != null)
            {
                //listDB has more items, listNew does not.
                listOrthoChartRowsDel.Add(orthoChartRowFieldDB);
                idxDB++;
                continue;
            }

            if (orthoChartRowFieldNew.OrthoChartRowNum < orthoChartRowFieldDB.OrthoChartRowNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listOrthoChartRowsIns.Add(orthoChartRowFieldNew);
                idxNew++;
                continue;
            }

            if (orthoChartRowFieldNew.OrthoChartRowNum > orthoChartRowFieldDB.OrthoChartRowNum)
            {
                //dbPK less than newPK, dbItem is 'next'
                listOrthoChartRowsDel.Add(orthoChartRowFieldDB);
                idxDB++;
                continue;
            }

            //Both lists contain the 'next' item, update required
            listOrthoChartRowsUpdNew.Add(orthoChartRowFieldNew);
            listOrthoChartRowsUpdDB.Add(orthoChartRowFieldDB);
            idxNew++;
            idxDB++;
        }

        //Commit changes to DB
        for (var i = 0; i < listOrthoChartRowsIns.Count; i++)
        {
            if (listOrthoChartRowsIns[i].UserNum == 0) listOrthoChartRowsIns[i].UserNum = Security.CurUser.UserNum;
            OrthoChartLogs.LogDb("Sync orthochartrow.Insert()", Environment.MachineName, listOrthoChartRowsIns[i], Security.CurUser.UserNum);
            Insert(listOrthoChartRowsIns[i]);
        }

        for (var i = 0; i < listOrthoChartRowsUpdNew.Count; i++)
        {
            var hasUpdated = OrthoChartRowCrud.Update(listOrthoChartRowsUpdNew[i], listOrthoChartRowsUpdDB[i]);
            if (!hasUpdated) continue;
            OrthoChartLogs.LogDb("Sync orthochartrow.Update()", Environment.MachineName, listOrthoChartRowsUpdNew[i], Security.CurUser.UserNum);
            rowsUpdatedCount++;
            var logText = "";
            //Do not log the Base64 information into the audit trail if this is a signature column, log some short descriptive text instead.
            if (!string.IsNullOrEmpty(listOrthoChartRowsUpdDB[i].Signature) && !string.IsNullOrEmpty(listOrthoChartRowsUpdNew[i].Signature))
                logText += Lans.g("OrthoCharts", "Signature modified.") + " ";
            else if (!string.IsNullOrEmpty(listOrthoChartRowsUpdDB[i].Signature) && string.IsNullOrEmpty(listOrthoChartRowsUpdNew[i].Signature)) logText += Lans.g("OrthoCharts", "Signature deleted.") + " ";
            if (!string.IsNullOrEmpty(logText))
            {
                logText += listOrthoChartRowsUpdDB[i].DateTimeService.ToString("yyyyMMdd"); //This date stamp must be the last 8 characters for new OrthoEdit audit trail entries.
                SecurityLogs.MakeLogEntry(EnumPermType.OrthoChartEditFull, listOrthoChartRowsUpdNew[i].PatNum, logText);
            }
        }

        OrthoChartRowCrud.DeleteMany(listOrthoChartRowsDel.Select(x => x.OrthoChartRowNum).ToList());
        if (rowsUpdatedCount > 0 || listOrthoChartRowsIns.Count > 0 || listOrthoChartRowsDel.Count > 0) return true;
        return false;
    }

    #endregion Methods - Misc

    #region Methods - Get

    ///<summary>Returns a list of all OrthoChartRows for the patnum passed in. Includes the list of orthocharts by default.</summary>
    public static List<OrthoChartRow> GetAllForPatient(long patNum, bool doIncludeOrthoCharts = true)
    {
        var command = "SELECT * FROM orthochartrow WHERE PatNum = " + SOut.Long(patNum);
        var listOrthoChartRows = OrthoChartRowCrud.SelectMany(command);
        if (!doIncludeOrthoCharts) return listOrthoChartRows;
        var listOrthoChartRowNums = listOrthoChartRows.Select(x => x.OrthoChartRowNum).ToList();
        var listOrthoChartsAll = OrthoCharts.GetByOrthoChartRowNums(listOrthoChartRowNums).ToList();
        for (var i = 0; i < listOrthoChartRows.Count; i++)
        {
            var listOrthoCharts = listOrthoChartsAll.FindAll(x => x.OrthoChartRowNum == listOrthoChartRows[i].OrthoChartRowNum);
            if (listOrthoCharts.IsNullOrEmpty()) continue;
            listOrthoChartRows[i].ListOrthoCharts = listOrthoCharts;
        }

        return listOrthoChartRows;
    }

    public static List<OrthoChartRow> GetPatientData(long patNum)
    {
        var command = "SELECT * FROM orthochartrow WHERE PatNum = " + SOut.Long(patNum);
        var listOrthoChartRows = OrthoChartRowCrud.SelectMany(command);
        return listOrthoChartRows;
    }

    ///<summary>Gets one OrthoChartRow from the db.</summary>
    public static OrthoChartRow GetOne(long orthoChartRowNum)
    {
        return OrthoChartRowCrud.SelectOne(orthoChartRowNum);
    }

    #endregion Methods - Get

    #region Methods - Modify

    
    public static long Insert(OrthoChartRow orthoChartRow)
    {
        return OrthoChartRowCrud.Insert(orthoChartRow);
    }

    
    public static void Update(OrthoChartRow orthoChartRow)
    {
        OrthoChartRowCrud.Update(orthoChartRow);
    }

    
    public static void Delete(long orthoChartRowNum)
    {
        OrthoChartRowCrud.Delete(orthoChartRowNum);
    }

    #endregion Methods - Modify
}