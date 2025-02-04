using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class RequiredFieldConditions
{
    public static List<RequiredFieldCondition> GetForRequiredField(long requiredFieldNum)
    {
        return GetWhere(x => x.RequiredFieldNum == requiredFieldNum);
    }

    public static void Insert(RequiredFieldCondition requiredFieldCondition)
    {
        RequiredFieldConditionCrud.Insert(requiredFieldCondition);
    }

    public static void DeleteAll(List<long> requiredFieldConditionNums)
    {
        if (requiredFieldConditionNums.Count < 1)
        {
            return;
        }

        Db.NonQ("DELETE FROM requiredfieldcondition WHERE RequiredFieldConditionNum IN (" + string.Join(",", requiredFieldConditionNums) + ")");
    }

    public static bool CheckStudentStatusConditions(int condCurIndex, List<RequiredFieldCondition> requiredFieldConditions, bool isNonStudent, bool isFullTimeStudent, bool isPartTimeStudent)
    {
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            return true;
        }

        if (requiredFieldConditions[condCurIndex].Operator == ConditionOperator.Equals)
        {
            return (isNonStudent && requiredFieldConditions[condCurIndex].ConditionValue == "Nonstudent") ||
                   (isFullTimeStudent && requiredFieldConditions[condCurIndex].ConditionValue == "Fulltime") ||
                   (isPartTimeStudent && requiredFieldConditions[condCurIndex].ConditionValue == "Parttime");
        }

        var requiredFieldConditionsStudent = requiredFieldConditions.FindAll(x => x.ConditionType == RequiredFieldName.StudentStatus);
        return (!isNonStudent || requiredFieldConditionsStudent.All(x => x.ConditionValue != "Nonstudent")) &&
               (!isFullTimeStudent || requiredFieldConditionsStudent.All(x => x.ConditionValue != "Fulltime")) &&
               (!isPartTimeStudent || requiredFieldConditionsStudent.All(x => x.ConditionValue != "Parttime"));
    }

    public static string CheckMedicaidIDLength(string medicaidState, string medicaidId)
    {
        var reqLength = StateAbbrs.GetMedicaidIdLength(medicaidState);
        if (reqLength == 0 || reqLength == medicaidId.Length)
        {
            return "";
        }

        return reqLength.ToString();
    }

    public static bool CheckMedicaidConditions(string val, int condCurIndex, List<RequiredFieldCondition> requiredFieldConditions)
    {
        if (PrefC.GetBool(PrefName.EasyHideMedicaid))
        {
            return true;
        }

        return (requiredFieldConditions[condCurIndex].Operator == ConditionOperator.Equals && val == "") ||
               (requiredFieldConditions[condCurIndex].Operator == ConditionOperator.NotEquals && val != "");
    }

    public static bool ConditionComparerHelper(string val, int condCurIndex, List<RequiredFieldCondition> requiredFieldConditions)
    {
        var requiredFieldCondition = requiredFieldConditions[condCurIndex];

        return requiredFieldCondition.Operator switch
        {
            ConditionOperator.Equals => requiredFieldConditions.Any(x => x.ConditionType == requiredFieldCondition.ConditionType && x.ConditionValue == val),
            ConditionOperator.NotEquals => !requiredFieldConditions.Any(x => x.ConditionType == requiredFieldCondition.ConditionType && x.ConditionValue == val),
            _ => false
        };
    }

    public static bool CheckDateConditions(string dateStr, int condCurIndex, List<RequiredFieldCondition> listRequiredFieldConditions)
    {
        var requiredFieldCondition = listRequiredFieldConditions[condCurIndex]; //Variable for convenience
        if (requiredFieldCondition.ConditionType == RequiredFieldName.AdmitDate && PrefC.GetBool(PrefName.EasyHideHospitals)) return true;
        if (requiredFieldCondition.ConditionType == RequiredFieldName.DischargeDate && PrefC.GetBool(PrefName.EasyHideHospitals)) return true;
        if (requiredFieldCondition.ConditionType == RequiredFieldName.DateTimeDeceased && !false) return true;
        var dateTime = DateTime.MinValue;
        if (dateStr == "" || !DateTime.TryParse(dateStr, out dateTime)) return false;
        var listRequiredFieldConditionDate = listRequiredFieldConditions.FindAll(x => x.ConditionType == requiredFieldCondition.ConditionType);
        if (listRequiredFieldConditionDate.Count < 1) return false;
        //There should be no more than 2 conditions of a date type
        var listAreCondsMet = new List<bool>();
        for (var i = 0; i < listRequiredFieldConditionDate.Count; i++) listAreCondsMet.Add(CondOpComparer(dateTime, listRequiredFieldConditionDate[i].Operator, SIn.Date(listRequiredFieldConditionDate[i].ConditionValue)));
        if (listAreCondsMet.Count < 2 || listRequiredFieldConditionDate[1].ConditionRelationship == LogicalOperator.And) return !listAreCondsMet.Contains(false);
        return listAreCondsMet.Contains(true);
    }

    public static bool CondOpComparer(DateTime dateTime1, ConditionOperator conditionOperator, DateTime dateTime2)
    {
        return CondOpComparer(DateTime.Compare(dateTime1, dateTime2), conditionOperator, 0);
    }

    public static bool CondOpComparer(int value1, ConditionOperator conditionOperator, int value2)
    {
        return conditionOperator switch
        {
            ConditionOperator.Equals => value1 == value2,
            ConditionOperator.NotEquals => value1 != value2,
            ConditionOperator.GreaterThan => value1 > value2,
            ConditionOperator.GreaterThanOrEqual => value1 >= value2,
            ConditionOperator.LessThan => value1 < value2,
            ConditionOperator.LessThanOrEqual => value1 <= value2,
            _ => false
        };
    }

    private class RequiredFieldConditionCache : CacheListAbs<RequiredFieldCondition>
    {
        protected override List<RequiredFieldCondition> GetCacheFromDb()
        {
            return RequiredFieldConditionCrud.SelectMany("SELECT * FROM requiredfieldcondition ORDER BY ConditionType, RequiredFieldConditionNum");
        }

        protected override List<RequiredFieldCondition> TableToList(DataTable dataTable)
        {
            return RequiredFieldConditionCrud.TableToList(dataTable);
        }

        protected override RequiredFieldCondition Copy(RequiredFieldCondition item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<RequiredFieldCondition> items)
        {
            return RequiredFieldConditionCrud.ListToTable(items, "RequiredFieldCondition");
        }

        protected override void FillCacheIfNeeded()
        {
            RequiredFieldConditions.GetTableFromCache(false);
        }
    }

    private static readonly RequiredFieldConditionCache Cache = new();

    public static List<RequiredFieldCondition> GetWhere(Predicate<RequiredFieldCondition> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}