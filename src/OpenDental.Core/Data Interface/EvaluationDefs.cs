using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EvaluationDefs
{
    //If this table type will exist as cached data, uncomment the CachePattern region below and edit.
    /*
    #region CachePattern

    private class EvaluationDefCache : CacheListAbs<EvaluationDef> {
        protected override List<EvaluationDef> GetCacheFromDb() {
            string command="SELECT * FROM EvaluationDef ORDER BY ItemOrder";
            return Crud.EvaluationDefCrud.SelectMany(command);
        }
        protected override List<EvaluationDef> TableToList(DataTable table) {
            return Crud.EvaluationDefCrud.TableToList(table);
        }
        protected override EvaluationDef Copy(EvaluationDef EvaluationDef) {
            return EvaluationDef.Clone();
        }
        protected override DataTable ListToTable(List<EvaluationDef> listEvaluationDefs) {
            return Crud.EvaluationDefCrud.ListToTable(listEvaluationDefs,"EvaluationDef");
        }
        protected override void FillCacheIfNeeded() {
            EvaluationDefs.GetTableFromCache(false);
        }
        protected override bool IsInListShort(EvaluationDef EvaluationDef) {
            return !EvaluationDef.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static EvaluationDefCache _EvaluationDefCache=new EvaluationDefCache();

    ///<summary>A list of all EvaluationDefs. Returns a deep copy.</summary>
    public static List<EvaluationDef> ListDeep {
        get {
            return _EvaluationDefCache.ListDeep;
        }
    }

    ///<summary>A list of all visible EvaluationDefs. Returns a deep copy.</summary>
    public static List<EvaluationDef> ListShortDeep {
        get {
            return _EvaluationDefCache.ListShortDeep;
        }
    }

    ///<summary>A list of all EvaluationDefs. Returns a shallow copy.</summary>
    public static List<EvaluationDef> ListShallow {
        get {
            return _EvaluationDefCache.ListShallow;
        }
    }

    ///<summary>A list of all visible EvaluationDefs. Returns a shallow copy.</summary>
    public static List<EvaluationDef> ListShort {
        get {
            return _EvaluationDefCache.ListShallowShort;
        }
    }

    ///<summary>Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's cache.</summary>
    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _EvaluationDefCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _EvaluationDefCache.GetTableFromCache(doRefreshCache);
    }

    #endregion
    */

    ///<summary>Gets all EvaluationDefs from the DB.</summary>
    public static List<EvaluationDef> Refresh()
    {
        var command = "SELECT * FROM evaluationdef";
        return EvaluationDefCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets all EvaluationDefs from the DB that are attached to the specified course. If course is blank then it will
    ///     get all of the defs.
    /// </summary>
    public static DataTable GetAllByCourse(long schoolCourseNum)
    {
        var command = "SELECT evaluationdef.EvaluationDefNum, evaluationdef.EvalTitle, schoolcourse.CourseID FROM evaluationdef "
                      + "INNER JOIN schoolcourse ON schoolcourse.SchoolCourseNum=evaluationdef.SchoolCourseNum "
                      + "WHERE TRUE";
        if (schoolCourseNum != 0) command += " AND schoolcourse.SchoolCourseNum = '" + SOut.Long(schoolCourseNum) + "'";
        command += " ORDER BY CourseID,EvalTitle";
        return DataCore.GetTable(command);
    }

    ///<summary>Gets one EvaluationDef from the db.</summary>
    public static EvaluationDef GetOne(long evaluationDefNum)
    {
        return EvaluationDefCrud.SelectOne(evaluationDefNum);
    }

    
    public static long Insert(EvaluationDef evaluationDef)
    {
        return EvaluationDefCrud.Insert(evaluationDef);
    }

    
    public static void Update(EvaluationDef evaluationDef)
    {
        EvaluationDefCrud.Update(evaluationDef);
    }

    ///<summary>Deletes an EvaluationDef and all EvaluationCriterionDefs attached to it.</summary>
    public static void Delete(long evaluationDefNum)
    {
        var command = "DELETE FROM evaluationdef WHERE EvaluationDefNum = " + SOut.Long(evaluationDefNum);
        Db.NonQ(command);
        command = "DELETE FROM evaluationcriteriondef WHERE EvaluationDefNum = " + SOut.Long(evaluationDefNum);
        Db.NonQ(command);
    }
}