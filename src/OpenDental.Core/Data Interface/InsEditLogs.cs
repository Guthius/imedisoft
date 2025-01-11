using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class InsEditLogs
{
    ///<summary>Gets logs from the passed in datetime and before.</summary>
    public static List<InsEditLog> GetLogsForPlan(long planNum, long carrierNum, long employerNum)
    {
        var listCarrierNums = GetAssociatedCarrierNums(planNum);
        listCarrierNums.Add(carrierNum);
        var listInsEditLogsRet = new List<InsEditLog>();
        var command = @"SELECT PlanNum FROM insplan WHERE PlanNum = " + SOut.Long(planNum);
        var insPlanNum = Db.GetLong(command);
        command = @"SELECT CarrierNum FROM carrier WHERE CarrierNum IN (" + string.Join(",", listCarrierNums) + @")";
        listCarrierNums = Db.GetListLong(command);
        command = @"SELECT EmployerNum FROM employer WHERE EmployerNum=" + SOut.Long(employerNum);
        var empNum = Db.GetLong(command);
        var listWhereOrs = new List<string>();
        if (insPlanNum > 0) listWhereOrs.Add("(LogType=" + SOut.Int((int) InsEditLogType.InsPlan) + " AND FKey = " + SOut.Long(insPlanNum) + ")");
        if (listCarrierNums.Count > 0) listWhereOrs.Add("(LogType=" + SOut.Int((int) InsEditLogType.Carrier) + " AND FKey IN (" + string.Join(",", listCarrierNums) + "))");
        if (empNum > 0) listWhereOrs.Add("(LogType=" + SOut.Int((int) InsEditLogType.Employer) + " AND FKey=" + SOut.Long(empNum) + ")");
        listWhereOrs.Add("(LogType=" + SOut.Int((int) InsEditLogType.Benefit) + " AND ParentKey=" + SOut.Long(planNum) + ")");
        command = @"SELECT * FROM inseditlog
				WHERE " + string.Join(@"
				OR ", listWhereOrs);
        var listInsEditLogs = InsEditLogCrud.SelectMany(command); //get all of the logs
        var listInsVerifyHists = InsVerifyHists.GetForFKeyByType(planNum, VerifyTypes.InsuranceBenefit)
            .OrderByDescending(x => x.SecDateTEdit)
            //Before 19.3.32, SecDateTEdit was not getting set. We'll use InsVerifyHistNum as a fallback.
            .ThenByDescending(x => x.InsVerifyHistNum).ToList();
        var insVerify = InsVerifies.GetOneByFKey(planNum, VerifyTypes.InsuranceBenefit);
        //The most recent inserted insverifyhist will be the current insverify. Go through the list of insverifyhist and construct inseditlog objects.
        //These inseditlogs will not get inserted into the db. 
        for (var i = 0; i < listInsVerifyHists.Count; i++)
        {
            var insVerifyHistNew = listInsVerifyHists[i];
            InsVerifyHist insVerifyHistOld = null;
            if (i < listInsVerifyHists.Count - 1) insVerifyHistOld = listInsVerifyHists[i + 1]; //get next
            //If the insVerifyOld is null, then the previous value was MinDate. 
            var oldVal = Lans.g("FormInsEditLog", "NEW");
            if (insVerifyHistOld != null) oldVal = insVerifyHistOld.DateLastVerified.ToShortDateString();
            var insEditLog = new InsEditLog();
            insEditLog.FKey = insPlanNum;
            insEditLog.LogType = InsEditLogType.InsPlan;
            insEditLog.FieldName = Lans.g("FormInsEditLog", "Benefits Last Verified");
            insEditLog.OldValue = oldVal;
            insEditLog.NewValue = insVerifyHistNew.DateLastVerified.ToShortDateString();
            insEditLog.UserNum = insVerifyHistNew.VerifyUserNum;
            insEditLog.DateTStamp = insVerifyHistNew.SecDateTEdit;
            insEditLog.ParentKey = 0;
            insEditLog.Description = "";
            listInsEditLogs.Add(insEditLog);
        }

        //get any logs that show that InsPlan's PlanNum changed
        return GetChangedLogs(listInsEditLogs).OrderBy(x => x.DateTStamp)
            .ThenBy(x => x.LogType != InsEditLogType.InsPlan)
            .ThenBy(x => x.LogType != InsEditLogType.Carrier)
            .ThenBy(x => x.LogType != InsEditLogType.Employer)
            .ThenBy(x => x.LogType != InsEditLogType.Benefit)
            .ThenBy(x => x.FKey)
            //primary keys first
            .ThenBy(x => x.LogType == InsEditLogType.Benefit ? x.FieldName != "BenefitNum"
                : x.LogType == InsEditLogType.Carrier ? x.FieldName != "CarrierNum"
                : x.LogType == InsEditLogType.Employer ? x.FieldName != "EmployerNum"
                : x.FieldName != "PlanNum").ToList();
    }

    /// <summary>
    ///     Gets all logs with the passed-in FKey of the specified LogType.
    ///     Only returns logs that occurred before the passed-in log.
    ///     Called from GetChangedLogs and, between the two methods, recursively retrieves logs linked to the logs that are
    ///     returned from this method.
    /// </summary>
    private static List<InsEditLog> GetLinkedLogs(long FKey, InsEditLogType insEditLogType, InsEditLog insEditLog, List<InsEditLog> listInsEditLogs)
    {
        var command = "SELECT * FROM inseditlog "
                      + "WHERE FKey = " + SOut.Long(FKey) + " "
                      + "AND LogType = " + SOut.Int((int) insEditLogType) + " "
                      + "AND DateTStamp < " + SOut.DateT(insEditLog.DateTStamp) + " "
                      + "AND InsEditLogNum NOT IN( " + string.Join(",", listInsEditLogs.Select(x => SOut.Long(x.InsEditLogNum)).ToList()) + ")";
        var listInsEditLogsLinked = InsEditLogCrud.SelectMany(command);
        GetChangedLogs(listInsEditLogsLinked);
        return listInsEditLogsLinked;
    }

    /// <summary>
    ///     Looks for logs that show that the insplan or carrier changed and retrieves the previous insplan/carrier's
    ///     information.
    ///     Called from GetLinkedLogs and, between the two methods, recursively retrieves logs linked to the logs that are
    ///     returned from this method.
    /// </summary>
    private static List<InsEditLog> GetChangedLogs(List<InsEditLog> listInsEditLogs)
    {
        var listInsEditLogsPlanChanged = listInsEditLogs.FindAll(x =>
            ((x.LogType == InsEditLogType.InsPlan && x.FieldName == "PlanNum")
             || (x.LogType == InsEditLogType.Carrier && x.FieldName == "CarrierNum"))
            && x.OldValue != "NEW" && x.NewValue != "DELETED").ToList();
        for (var i = 0; i < listInsEditLogsPlanChanged.Count; i++)
        {
            if (listInsEditLogsPlanChanged[i].FieldName == "PlanNum")
            {
                var insPlanOld = InsPlans.GetPlan(SIn.Long(listInsEditLogsPlanChanged[i].OldValue), null);
                if (insPlanOld != null) listInsEditLogs.AddRange(GetLinkedLogs(insPlanOld.CarrierNum, InsEditLogType.Carrier, listInsEditLogsPlanChanged[i], listInsEditLogs));
                listInsEditLogs.AddRange(GetLinkedLogs(SIn.Long(listInsEditLogsPlanChanged[i].OldValue), InsEditLogType.InsPlan, listInsEditLogsPlanChanged[i], listInsEditLogs));
            }

            if (listInsEditLogsPlanChanged[i].FieldName == "CarrierNum") listInsEditLogs.AddRange(GetLinkedLogs(SIn.Long(listInsEditLogsPlanChanged[i].OldValue), InsEditLogType.Carrier, listInsEditLogsPlanChanged[i], listInsEditLogs));
        }

        return listInsEditLogs;
    }

    /// <summary>
    ///     Gets a list of carrierNums that can all be linked to the passed in carrierNum via Insurance Edit Log entries for
    ///     carrierNum changes.
    /// </summary>
    public static List<long> GetAssociatedCarrierNums(long insPlanNum)
    {
        //Get carrierNums associated to this insPlanNum, using carrierNum as a starting point.
        var command = @"SELECT inseditlog.OldValue,inseditlog.NewValue
				FROM inseditlog
				WHERE inseditlog.LogType=" + SOut.Long((long) InsEditLogType.InsPlan) + @"
					AND inseditlog.FieldName='CarrierNum' 
					AND inseditlog.OldValue!=inseditlog.NewValue
					AND inseditlog.OldValue!=0 
					AND inseditlog.NewValue!=0
					AND inseditlog.FKey=" + SOut.Long(insPlanNum);
        var table = DataCore.GetTable(command);
        var listCarrierNums = new List<long>();
        for (var i = 0; i < table.Rows.Count; i++)
        {
            listCarrierNums.Add(SIn.Long(table.Rows[i][0].ToString()));
            listCarrierNums.Add(SIn.Long(table.Rows[i][1].ToString()));
        }

        return listCarrierNums.Distinct().ToList();
    }

    /// <summary>
    ///     Automatic log entry. Fills in table and column names based on items passed in.
    ///     Compares whole table excluding CrudColumnSpecialTypes of DateEntry, DateTEntry, ExcludeFromUpdate, and TimeStamp.
    ///     Pass in null for ItemOld if the item was just inserted. Pass in null for ItemCur if the item was just deleted.
    ///     Both itemCur and itemOld cannot be null.
    /// </summary>
    public static void MakeLogEntry<T>(T itemCur, T itemOld, InsEditLogType insEditLogType, long userNumCur)
    {
        var priKeyItem = itemCur;
        if (itemCur == null) priKeyItem = itemOld;
        var fieldInfoPriKey = priKeyItem.GetType().GetFields().Where(x => x.IsDefined(typeof(CrudColumnAttribute))
                                                                          && ((CrudColumnAttribute) x.GetCustomAttribute(typeof(CrudColumnAttribute))).IsPriKey).First();
        var priKey = (long) fieldInfoPriKey.GetValue(priKeyItem);
        var priKeyColName = fieldInfoPriKey.Name;
        long parentKey = 0; //parentKey only filled for Benefits.
        if (priKeyItem.GetType() == typeof(Benefit)) parentKey = ((Benefit) (object) priKeyItem).PlanNum;
        var descript = "";
        switch (insEditLogType)
        {
            //always default the descript to the priKeyItem (preferring current unless deleted).
            case InsEditLogType.InsPlan:
                if (priKeyItem is InsPlan) descript = (priKeyItem as InsPlan).GroupNum + " - " + (priKeyItem as InsPlan).GroupName;
                break;
            case InsEditLogType.Carrier:
                if (priKeyItem is Carrier) descript = (priKeyItem as Carrier).CarrierNum + " - " + (priKeyItem as Carrier).CarrierName;
                break;
            case InsEditLogType.Benefit:
                if (priKeyItem is Benefit) descript = Benefits.GetCategoryString(priKeyItem as Benefit);
                break;
            case InsEditLogType.Employer:
                descript = (priKeyItem as Employer)?.EmpName;
                if (descript == null) descript = "";
                break;
        }

        InsEditLog insEditLog;
        if (itemOld == null)
        {
            //new, just inserted. Show PriKey Column only.
            insEditLog = new InsEditLog();
            insEditLog.FieldName = priKeyColName;
            insEditLog.UserNum = userNumCur;
            insEditLog.OldValue = "NEW";
            insEditLog.NewValue = priKey.ToString();
            insEditLog.LogType = insEditLogType;
            insEditLog.FKey = priKey;
            insEditLog.ParentKey = parentKey;
            insEditLog.Description = descript;
            Insert(insEditLog);
            return;
        }

        var listInsEditLogsForInsert = new List<InsEditLog>();
        var listFieldInfos = priKeyItem.GetType().GetFields().ToList();
        for (var i = 0; i < listFieldInfos.Count; i++)
        {
            if (listFieldInfos[i].IsDefined(typeof(CrudColumnAttribute)))
            {
                var crudColumnAttribute = (CrudColumnAttribute) listFieldInfos[i].GetCustomAttribute(typeof(CrudColumnAttribute));
                if (crudColumnAttribute.SpecialType.HasFlag(CrudSpecialColType.DateEntry)
                    || crudColumnAttribute.SpecialType.HasFlag(CrudSpecialColType.DateTEntry)
                    || crudColumnAttribute.SpecialType.HasFlag(CrudSpecialColType.ExcludeFromUpdate)
                    || crudColumnAttribute.SpecialType.HasFlag(CrudSpecialColType.TimeStamp))
                    continue; //skip logs that are not user editable.
            }

            var objValOld = listFieldInfos[i].GetValue(itemOld);
            if (objValOld == null) objValOld = "";
            if (itemCur == null)
            {
                //deleted, show all deleted columns
                insEditLog = new InsEditLog();
                insEditLog.FieldName = listFieldInfos[i].Name;
                insEditLog.UserNum = userNumCur;
                insEditLog.OldValue = objValOld.ToString();
                insEditLog.NewValue = "DELETED";
                insEditLog.LogType = insEditLogType;
                insEditLog.FKey = priKey;
                insEditLog.ParentKey = parentKey;
                insEditLog.Description = descript;
                listInsEditLogsForInsert.Add(insEditLog);
                continue;
            }

            //updated, just show changes.
            var valCur = listFieldInfos[i].GetValue(itemCur);
            if (valCur == null) valCur = "";
            if (valCur.ToString() == objValOld.ToString()) continue;
            insEditLog = new InsEditLog();
            insEditLog.FieldName = listFieldInfos[i].Name;
            insEditLog.UserNum = userNumCur;
            insEditLog.OldValue = objValOld.ToString();
            insEditLog.NewValue = valCur.ToString();
            insEditLog.LogType = insEditLogType;
            insEditLog.FKey = priKey;
            insEditLog.ParentKey = parentKey;
            insEditLog.Description = descript;
            listInsEditLogsForInsert.Add(insEditLog);
        }

        if (listInsEditLogsForInsert.Count > 0) InsertMany(listInsEditLogsForInsert);
    }

    public static void InsertMany(List<InsEditLog> listInsEditLogs)
    {
        InsEditLogCrud.InsertMany(listInsEditLogs);
    }

    public static long Insert(InsEditLog insEditLog)
    {
        return InsEditLogCrud.Insert(insEditLog);
    }

    /// <summary>
    ///     Manual log entry. Creates a new InsEditLog based on information passed in. PKey should be 0 unless LogType =
    ///     Benefit.
    ///     Use the automatic MakeLogEntry overload if possible. This only be used when manual UPDATE/INSERT/DELETE queries are
    ///     run on the logged tables.
    /// </summary>
    public static InsEditLog MakeLogEntry(string fieldName, long userNum, string oldVal, string newVal, InsEditLogType insEditLogType, long fKey, long pKey, string descript, bool doInsert = true)
    {
        var insEditLog = new InsEditLog();
        insEditLog.FieldName = fieldName;
        insEditLog.UserNum = userNum;
        insEditLog.OldValue = oldVal;
        insEditLog.NewValue = newVal;
        insEditLog.LogType = insEditLogType;
        insEditLog.FKey = fKey;
        insEditLog.ParentKey = pKey;
        insEditLog.Description = descript;
        if (doInsert) Insert(insEditLog);
        return insEditLog;
    }

    public static void DeletePreInsertedLogsForPlanNum(long planNum)
    {
        var command = "DELETE FROM inseditlog "
                      + "WHERE LogType=" + SOut.Int((int) InsEditLogType.Benefit) + " AND ParentKey=" + SOut.Long(planNum);
        Db.NonQ(command);
        command = "DELETE FROM inseditlog "
                  + "WHERE LogType=" + SOut.Int((int) InsEditLogType.InsPlan) + " AND FKey=" + SOut.Long(planNum);
        Db.NonQ(command);
    }
}