using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using ODCrypt;

namespace OpenDentBusiness;

public class OrthoCharts
{
    public static List<OrthoChart> GetPatientData(long patNum)
    {
        var command = "SELECT * FROM orthochart WHERE PatNum =" + (patNum)
                                                                //FieldValue='' were stored as a result of a bug. DBM now removes those rows from the DB. This prevents them from being seen until DBM is run.
                                                                + " AND FieldValue!=''";
        return OrthoChartCrud.SelectMany(command);
    }

    public static List<OrthoChart> GetByOrthoChartRowNums(List<long> listOrthoChartRowNums)
    {
        if (listOrthoChartRowNums.IsNullOrEmpty()) return [];

        var command = $"SELECT * FROM orthochart WHERE OrthoChartRowNum IN({string.Join(",", listOrthoChartRowNums.Select(x => (x)))}) ORDER BY OrthoChartNum";
        return OrthoChartCrud.SelectMany(command);
    }

    public static List<string> GetDistinctFieldNames()
    {
        var command = "SELECT FieldName FROM orthochart GROUP BY FieldName";
        var listFieldNames = Db.GetListString(command);
        listFieldNames.Add("Signature"); //OrthoChart will always have a Signature field.
        return listFieldNames.Distinct().ToList();
    }

    public static bool IsInUse(string fieldName)
    {
        var command = "SELECT COUNT(*) FROM orthochart WHERE FieldName = '" + SOut.String(fieldName) + "'";
        var count = Db.GetCount(command);
        if (count == "0") return false;
        return true;
    }

    public static void Insert(OrthoChart orthoChart)
    {
        OrthoChartCrud.Insert(orthoChart);
    }

    public static void Update(OrthoChart orthoChart, OrthoChart orthoChartOld)
    {
        var command = "";
        if (orthoChart.PatNum != orthoChartOld.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + (orthoChart.PatNum) + "";
        }

        if (orthoChart.DateService != orthoChartOld.DateService)
        {
            if (command != "") command += ",";
            command += "DateService = " + SOut.Date(orthoChart.DateService) + "";
        }

        if (orthoChart.FieldName != orthoChartOld.FieldName)
        {
            if (command != "") command += ",";
            command += "FieldName = '" + SOut.String(orthoChart.FieldName) + "'";
        }

        if (orthoChart.FieldValue != orthoChartOld.FieldValue)
        {
            if (command != "") command += ",";
            command += "FieldValue = '" + SOut.String(orthoChart.FieldValue) + "'";
        }

        if (orthoChart.UserNum != orthoChartOld.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = '" + (orthoChart.UserNum) + "'";
        }

        if (command == "") return;
        command = "UPDATE orthochart SET " + command
                                           + " WHERE OrthoChartNum = " + (orthoChartOld.OrthoChartNum);
        Db.NonQ(command);
        //Crud.OrthoChartCrud.Update(orthoChartNew,orthoChartOld);
    }

    public static void Delete(long orthoChartNum)
    {
        var command = "DELETE FROM orthochart WHERE OrthoChartNum = " + (orthoChartNum);
        Db.NonQ(command);
    }

    public static void Sync(Patient patient, List<OrthoChartRow> listOrthoChartRows, List<OrthoChart> listOrthoChartsNew, List<DisplayField> listDisplayFieldsOrth)
    {
        var listOrthoChartRowsForPat = OrthoChartRows.GetAllForPatient(patient.PatNum, false);
        var listOrthoChartsDB = GetByOrthoChartRowNums(listOrthoChartRowsForPat.Select(x => x.OrthoChartRowNum).ToList());
        OrthoChartLogs.Log("OrthoCharts.Sync() - orthocharts after getting from the database.", listOrthoChartsDB, 0, Environment.MachineName, patient.PatNum, Security.CurUser.UserNum);
        //This code is mostly a copy of the Crud sync.  Differences include sort and logging.
        //Inserts, updates, or deletes database rows to match supplied list.
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listOrthoChartsIns = new List<OrthoChart>();
        var listOrthoChartsUpdNew = new List<OrthoChart>();
        var listOrthoChartsUpdDB = new List<OrthoChart>();
        var listOrthoChartsDel = new List<OrthoChart>();
        var listColNames = new List<string>();
        //Remove fields from both lists that are not currently set to display.
        for (var i = 0; i < listDisplayFieldsOrth.Count; i++) listColNames.Add(listDisplayFieldsOrth[i].Description);
        for (var i = listOrthoChartsDB.Count - 1; i >= 0; i--)
            if (!listColNames.Contains(listOrthoChartsDB[i].FieldName))
                listOrthoChartsDB.RemoveAt(i);

        listOrthoChartsNew = listOrthoChartsNew.FindAll(x => listColNames.Contains(x.FieldName));
        listOrthoChartsNew = listOrthoChartsNew.OrderBy(x => x.OrthoChartNum).ToList();
        OrthoChartLogs.Log("OrthoCharts.Sync() - orthocharts passed into method after sorting.", listOrthoChartsNew, 0, Environment.MachineName, patient.PatNum, Security.CurUser.UserNum);
        listOrthoChartsDB = listOrthoChartsDB.OrderBy(x => x.OrthoChartNum).ToList();
        OrthoChartLogs.Log("OrthoCharts.Sync() - orthocharts from db after sorting.", listOrthoChartsDB, 0, Environment.MachineName, patient.PatNum, Security.CurUser.UserNum);
        var idxNew = 0;
        var idxDB = 0;
        OrthoChart orthoChartFieldNew;
        OrthoChart orthoChartFieldDB;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (true)
        {
            if (idxNew >= listOrthoChartsNew.Count) break;
            if (idxDB >= listOrthoChartsDB.Count) break;
            orthoChartFieldNew = null;
            if (idxNew < listOrthoChartsNew.Count) orthoChartFieldNew = listOrthoChartsNew[idxNew];
            orthoChartFieldDB = null;
            if (idxDB < listOrthoChartsDB.Count) orthoChartFieldDB = listOrthoChartsDB[idxDB];
            //begin compare
            if (orthoChartFieldNew != null && orthoChartFieldDB == null)
            {
                //listNew has more items, listDB does not.
                listOrthoChartsIns.Add(orthoChartFieldNew);
                idxNew++;
                continue;
            }

            if (orthoChartFieldNew == null && orthoChartFieldDB != null)
            {
                //listDB has more items, listNew does not.
                listOrthoChartsDel.Add(orthoChartFieldDB);
                idxDB++;
                continue;
            }

            if (orthoChartFieldNew.OrthoChartNum < orthoChartFieldDB.OrthoChartNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listOrthoChartsIns.Add(orthoChartFieldNew);
                idxNew++;
                continue;
            }

            if (orthoChartFieldNew.OrthoChartNum > orthoChartFieldDB.OrthoChartNum)
            {
                //dbPK less than newPK, dbItem is 'next'
                listOrthoChartsDel.Add(orthoChartFieldDB);
                idxDB++;
                continue;
            }

            //Both lists contain the 'next' item, update required
            listOrthoChartsUpdNew.Add(orthoChartFieldNew);
            listOrthoChartsUpdDB.Add(orthoChartFieldDB);
            idxNew++;
            idxDB++;
        }

        //Commit changes to DB
        for (var i = 0; i < listOrthoChartsIns.Count; i++)
        {
            if (listOrthoChartsIns[i].FieldValue == "") //do not insert new blank values. This happens when fields from today are not used.
                continue;
            OrthoChartLogs.LogDb("Sync orthochart.Insert(), FieldName:" + listOrthoChartsIns[i].FieldName + ", FieldValue:" + listOrthoChartsIns[i].FieldValue, Environment.MachineName, listOrthoChartsIns[i].OrthoChartRowNum, Security.CurUser.UserNum);
            Insert(listOrthoChartsIns[i]);
        }

        for (var i = 0; i < listOrthoChartsUpdNew.Count; i++)
        {
            if (listOrthoChartsUpdDB[i].FieldValue == listOrthoChartsUpdNew[i].FieldValue) continue; //values equal. do not update/create log entry.
            if (listOrthoChartsUpdNew[i].FieldValue != "")
            {
                //Actually update rows that have a new value.
                OrthoChartLogs.LogDb("Sync orthochart.Update(), FieldName:" + listOrthoChartsUpdNew[i].FieldName + ", FieldValue:" + listOrthoChartsUpdNew[i].FieldValue, Environment.MachineName, listOrthoChartsUpdNew[i].OrthoChartRowNum, Security.CurUser.UserNum);
                Update(listOrthoChartsUpdNew[i], listOrthoChartsUpdDB[i]);
            }
            else
            {
                //instead of updating to a blank value, we delete the row from the DB.
                OrthoChartLogs.LogDb("Sync orthochart.Add(), FieldName:" + listOrthoChartsUpdNew[i].FieldName + ", FieldValue:" + listOrthoChartsUpdNew[i].FieldValue, Environment.MachineName, listOrthoChartsUpdNew[i].OrthoChartRowNum, Security.CurUser.UserNum);
                listOrthoChartsDel.Add(listOrthoChartsUpdDB[i]);
            }

            #region security log entry

            var logText = Lans.g("OrthoCharts", "Ortho chart field edited. ")
                          + Lans.g("OrthoCharts", "Field name") + ": " + listOrthoChartsUpdNew[i].FieldName + "\r\n";
            //Do not log the Base64 information into the audit trail if this is a signature column, log some short descriptive text instead.
            logText += Lans.g("OrthoCharts", "Old value") + ": \"" + listOrthoChartsUpdDB[i].FieldValue + "\"  "
                       + Lans.g("OrthoCharts", "New value") + ": \"" + listOrthoChartsUpdNew[i].FieldValue + "\" ";
            logText += Lans.g("OrthoCharts", "OrthoChartRowNum") + ": \"" + listOrthoChartsUpdNew[i].OrthoChartRowNum + "\" ";
            var orthoChartRow = listOrthoChartRows.Find(x => x.OrthoChartRowNum == listOrthoChartsUpdNew[i].OrthoChartRowNum);
            if (orthoChartRow != null) logText += orthoChartRow.DateTimeService.ToString("yyyyMMdd");
            SecurityLogs.MakeLogEntry(EnumPermType.OrthoChartEditFull, patient.PatNum, logText);

            #endregion security log entry
        }

        for (var i = 0; i < listOrthoChartsDel.Count; i++) //All logging should have been performed above in the "Update block"
            Delete(listOrthoChartsDel[i].OrthoChartNum);
    }

    public static DateTime GetOrthoDateFromLog(SecurityLog securityLog)
    {
        //There are 3 cases to try, in order of ascending complexity. If a simple case succeeds at parsing a date, that date is returned.
        //1) Using the new log text, there should be an 8 digit number at the end of each log entry. This is in the format "YYYYMMDD" and should be culture invariant.
        //2) Using the old log text, the Date of service appeared as a string in the middle of the text block.
        //3) Using the old log text, the Date of service appeared as a string in the middle of the text block in a culture dependant format.
        var dateRetVal = DateTime.MinValue;

        #region Ideal Case, Culture invariant

        try
        {
            var dateString = securityLog.LogText.Substring(securityLog.LogText.Length - 8, 8);
            dateRetVal = new DateTime(int.Parse(dateString.Substring(0, 4)), int.Parse(dateString.Substring(4, 2)), int.Parse(dateString.Substring(6, 2)));
            if (dateRetVal != DateTime.MinValue) return dateRetVal;
        }
        catch (Exception ex)
        {
        }

        #endregion Ideal Case, Culture invariant

        #region Deprecated, log written in english

        try
        {
            if (securityLog.LogText.StartsWith("Ortho chart field edited.  Field date: "))
            {
                dateRetVal = DateTime.Parse(securityLog.LogText.Substring("Ortho chart field edited.  Field date: ".Length, 10)); //Date usually in the format MM/DD/YYYY, unless using en-UK for example
                if (dateRetVal != DateTime.MinValue) return dateRetVal;
            }
        }
        catch (Exception ex)
        {
        }

        #endregion Deprecated, log written in english

        #region Deprecated, log written in current culture

        try
        {
            if (securityLog.LogText.StartsWith(Lans.g("FormOrthoChart", "Ortho chart field edited.  Field date")))
            {
                var tokens = securityLog.LogText.Split([": "], StringSplitOptions.None);
                dateRetVal = DateTime.Parse(tokens[1].Replace(Lans.g("FormOrthoChart", "Field name"), ""));
                if (dateRetVal != DateTime.MinValue) return dateRetVal;
            }
        }
        catch (Exception ex)
        {
        }

        #endregion Deprecated, log written in current culture

        #region Deprecated, log written in non-english non-current culture

        //not particularly common or useful.

        #endregion Deprecated, log written in non-english non-current culture

        return dateRetVal; //Should be DateTime.MinVal if we are returning here.
    }

    public static string GetKeyDataForSignatureSaving(List<OrthoChart> listOrthoCharts, DateTime dateService)
    {
        var keyData = GetKeyDataForSignatureHash(null, listOrthoCharts, dateService);
        return GetHashStringForSignature(keyData);
    }

    public static string GetKeyDataForSignatureHash(Patient patient, List<OrthoChart> listOrthoCharts, DateTime dateService, bool doUsePatName = false)
    {
        var stringBuilder = new StringBuilder();
        if (doUsePatName)
        {
            stringBuilder.Append(patient.FName);
            stringBuilder.Append(patient.LName);
        }

        var strDateService = dateService.ToString("yyyyMMdd");
        if (dateService.TimeOfDay != TimeSpan.Zero) strDateService = dateService.ToString();
        stringBuilder.Append(strDateService);
        listOrthoCharts = listOrthoCharts.OrderBy(x => x.FieldName).ToList();
        for (var i = 0; i < listOrthoCharts.Count; i++)
        {
            stringBuilder.Append(listOrthoCharts[i].FieldName);
            stringBuilder.Append(listOrthoCharts[i].FieldValue);
        }

        return stringBuilder.ToString();
    }

    public static string GetHashStringForSignature(string str)
    {
        return Encoding.ASCII.GetString(MD5.Hash(Encoding.UTF8.GetBytes(str)));
    }
}