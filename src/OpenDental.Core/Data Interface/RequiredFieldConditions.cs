using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class RequiredFieldConditions
{
    ///<summary>Gets the requiredfieldconditions for one required field.</summary>
    public static List<RequiredFieldCondition> GetForRequiredField(long requiredFieldNum)
    {
        return GetWhere(x => x.RequiredFieldNum == requiredFieldNum);
    }

    
    public static long Insert(RequiredFieldCondition requiredFieldCondition)
    {
        return RequiredFieldConditionCrud.Insert(requiredFieldCondition);
    }

    
    public static void Update(RequiredFieldCondition requiredFieldCondition)
    {
        RequiredFieldConditionCrud.Update(requiredFieldCondition);
    }

    public static void DeleteAll(List<long> listRequiredFieldConditionNums)
    {
        if (listRequiredFieldConditionNums.Count < 1) return;
        var command = "DELETE FROM requiredfieldcondition WHERE RequiredFieldConditionNum IN(" + string.Join(",", listRequiredFieldConditionNums) + ")";
        Db.NonQ(command);
    }

    ///<summary>Returns true if the conditions for StudentStatus are true.</summary>
    public static bool CheckStudentStatusConditions(int condCurIndex, List<RequiredFieldCondition> listRequiredFieldConditions, bool isNonStudent, bool isFullTimeStudent, bool isPartTimeStudent)
    {
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA")) //Canadian. en-CA or fr-CA
            return true;
        if (listRequiredFieldConditions[condCurIndex].Operator == ConditionOperator.Equals)
        {
            if ((isNonStudent && listRequiredFieldConditions[condCurIndex].ConditionValue == "Nonstudent")
                || (isFullTimeStudent && listRequiredFieldConditions[condCurIndex].ConditionValue == "Fulltime")
                || (isPartTimeStudent && listRequiredFieldConditions[condCurIndex].ConditionValue == "Parttime"))
                return true;
            return false;
        }

        //condCur.Operator==ConditionOperator.NotEquals
        var listRequiredFieldConditionsStudent = listRequiredFieldConditions.FindAll(x => x.ConditionType == RequiredFieldName.StudentStatus);
        if ((isNonStudent && listRequiredFieldConditionsStudent.Any(x => x.ConditionValue == "Nonstudent"))
            || (isFullTimeStudent && listRequiredFieldConditionsStudent.Any(x => x.ConditionValue == "Fulltime"))
            || (isPartTimeStudent && listRequiredFieldConditionsStudent.Any(x => x.ConditionValue == "Parttime")))
            return false;
        return true;
    }

    /// <summary>
    ///     Checks to see if the Medicaid ID is the proper number of digits for the Medicaid State. If medicaid id is the
    ///     proper number of digits, returns empty string.
    ///     If medicaid id is NOT the proper number of digits, returns the proper number of digits as a string.
    /// </summary>
    public static string CheckMedicaidIDLength(string medicaidState, string medicaidID)
    {
        var reqLength = StateAbbrs.GetMedicaidIDLength(medicaidState);
        if (reqLength == 0 || reqLength == medicaidID.Length) return "";
        return reqLength.ToString();
    }

    ///<summary>Returns true if the conditions for MedicaidID/MedicaidState are true.</summary>
    public static bool CheckMedicaidConditions(string val, int condCurIndex, List<RequiredFieldCondition> listRequiredFieldConditions)
    {
        if (PrefC.GetBool(PrefName.EasyHideMedicaid)) return true;
        //The only possible value for ConditionValue is '' (an empty string)
        if ((listRequiredFieldConditions[condCurIndex].Operator == ConditionOperator.Equals && val == "")
            || (listRequiredFieldConditions[condCurIndex].Operator == ConditionOperator.NotEquals && val != ""))
            return true;
        return false;
    }

    /// <summary>
    ///     Returns true if the operator is Equals and the value is in the list of conditions or if the operator is NotEquals
    ///     and the value is
    ///     not in the list of conditions.
    /// </summary>
    public static bool ConditionComparerHelper(string val, int condCurIndex, List<RequiredFieldCondition> listRequiredFieldConditions)
    {
        var requiredFieldCondition = listRequiredFieldConditions[condCurIndex]; //Variable for convenience
        if (requiredFieldCondition.ConditionType == RequiredFieldName.Clinic && !true) return true;
        switch (requiredFieldCondition.Operator)
        {
            case ConditionOperator.Equals:
                return listRequiredFieldConditions.Any(x => x.ConditionType == requiredFieldCondition.ConditionType && x.ConditionValue == val);
            case ConditionOperator.NotEquals:
                return !listRequiredFieldConditions.Any(x => x.ConditionType == requiredFieldCondition.ConditionType && x.ConditionValue == val);
            default:
                return false;
        }
    }

    ///<summary>Returns true if the conditions for this date condition are true. Handles pref checks as well.</summary>
    public static bool CheckDateConditions(string dateStr, int condCurIndex, List<RequiredFieldCondition> listRequiredFieldConditions)
    {
        var requiredFieldCondition = listRequiredFieldConditions[condCurIndex]; //Variable for convenience
        if (requiredFieldCondition.ConditionType == RequiredFieldName.AdmitDate && PrefC.GetBool(PrefName.EasyHideHospitals)) return true;
        if (requiredFieldCondition.ConditionType == RequiredFieldName.DischargeDate && PrefC.GetBool(PrefName.EasyHideHospitals)) return true;
        if (requiredFieldCondition.ConditionType == RequiredFieldName.DateTimeDeceased && !PrefC.GetBool(PrefName.ShowFeatureEhr)) return true;
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

    ///<summary>Evaluates two dates using the provided operator.</summary>
    public static bool CondOpComparer(DateTime dateTime1, ConditionOperator conditionOperator, DateTime dateTime2)
    {
        return CondOpComparer(DateTime.Compare(dateTime1, dateTime2), conditionOperator, 0);
    }

    ///<summary>Evaluates two integers using the provided operator.</summary>
    public static bool CondOpComparer(int value1, ConditionOperator conditionOperator, int value2)
    {
        switch (conditionOperator)
        {
            case ConditionOperator.Equals:
                return value1 == value2;
            case ConditionOperator.NotEquals:
                return value1 != value2;
            case ConditionOperator.GreaterThan:
                return value1 > value2;
            case ConditionOperator.GreaterThanOrEqual:
                return value1 >= value2;
            case ConditionOperator.LessThan:
                return value1 < value2;
            case ConditionOperator.LessThanOrEqual:
                return value1 <= value2;
        }

        return false;
    }

    #region CachePattern

    private class RequiredFieldConditionCache : CacheListAbs<RequiredFieldCondition>
    {
        protected override List<RequiredFieldCondition> GetCacheFromDb()
        {
            var command = "SELECT * FROM requiredfieldcondition ORDER BY ConditionType,RequiredFieldConditionNum";
            return RequiredFieldConditionCrud.SelectMany(command);
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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly RequiredFieldConditionCache _requiredFieldConditionCache = new();

    public static List<RequiredFieldCondition> GetWhere(Predicate<RequiredFieldCondition> match, bool isShort = false)
    {
        return _requiredFieldConditionCache.GetWhere(match, isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _requiredFieldConditionCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _requiredFieldConditionCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _requiredFieldConditionCache.ClearCache();
    }

    #endregion

    /*
    
    public static void Delete(long requiredFieldConditionNum) {

        Crud.RequiredFieldConditionCrud.Delete(requiredFieldConditionNum);
    }

    
    public static List<RequiredFieldCondition> Refresh(long patNum){

        string command="SELECT * FROM requiredfieldcondition WHERE PatNum = "+POut.Long(patNum);
        return Crud.RequiredFieldConditionCrud.SelectMany(command);
    }

    ///<summary>Gets one RequiredFieldCondition from the db.</summary>
    public static RequiredFieldCondition GetOne(long requiredFieldConditionNum){

        return Crud.RequiredFieldConditionCrud.SelectOne(requiredFieldConditionNum);
    }
    */
}