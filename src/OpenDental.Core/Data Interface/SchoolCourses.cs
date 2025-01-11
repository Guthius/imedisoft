using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class SchoolCourses
{
    
    public static void Update(SchoolCourse schoolCourse)
    {
        SchoolCourseCrud.Update(schoolCourse);
    }

    
    public static long Insert(SchoolCourse schoolCourse)
    {
        return SchoolCourseCrud.Insert(schoolCourse);
    }

    
    public static void InsertOrUpdate(SchoolCourse schoolCourse, bool isNew)
    {
        //if(IsRepeating && DateTask.Year>1880){
        //	throw new Exception(Lans.g(this,"Task cannot be tagged repeating and also have a date."));
        //}
        if (isNew)
            Insert(schoolCourse);
        else
            Update(schoolCourse);
    }

    
    public static void Delete(long courseNum)
    {
        //check for attached reqneededs---------------------------------------------------------------------
        var command = "SELECT COUNT(*) FROM reqneeded WHERE SchoolCourseNum = '"
                      + SOut.Long(courseNum) + "'";
        var table = DataCore.GetTable(command);
        if (SIn.String(table.Rows[0][0].ToString()) != "0") throw new Exception(Lans.g("SchoolCourses", "Course already in use by 'requirements needed' table."));
        //check for attached reqstudents--------------------------------------------------------------------------
        command = "SELECT COUNT(*) FROM reqstudent WHERE SchoolCourseNum = '"
                  + SOut.Long(courseNum) + "'";
        table = DataCore.GetTable(command);
        if (SIn.String(table.Rows[0][0].ToString()) != "0") throw new Exception(Lans.g("SchoolCourses", "Course already in use by 'student requirements' table."));
        //delete---------------------------------------------------------------------------------------------
        command = "DELETE from schoolcourse WHERE SchoolCourseNum = '"
                  + SOut.Long(courseNum) + "'";
        Db.NonQ(command);
    }

    ///<summary>Description is CourseID Descript.</summary>
    public static string GetDescript(long schoolCourseNum)
    {
        var schoolCourse = GetFirstOrDefault(x => x.SchoolCourseNum == schoolCourseNum);
        if (schoolCourse == null) return "";
        return GetDescript(schoolCourse);
    }

    public static string GetDescript(SchoolCourse schoolCourse)
    {
        return schoolCourse.CourseID + " " + schoolCourse.Descript;
    }

    public static string GetCourseID(long schoolCourseNum)
    {
        var schoolCourse = GetFirstOrDefault(x => x.SchoolCourseNum == schoolCourseNum);
        if (schoolCourse == null) return "";
        return schoolCourse.CourseID;
    }

    #region CachePattern

    private class SchoolCourseCache : CacheListAbs<SchoolCourse>
    {
        protected override List<SchoolCourse> GetCacheFromDb()
        {
            var command =
                "SELECT * FROM schoolcourse "
                + "ORDER BY CourseID";
            return SchoolCourseCrud.SelectMany(command);
        }

        protected override List<SchoolCourse> TableToList(DataTable dataTable)
        {
            return SchoolCourseCrud.TableToList(dataTable);
        }

        protected override SchoolCourse Copy(SchoolCourse item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<SchoolCourse> items)
        {
            return SchoolCourseCrud.ListToTable(items, "SchoolCourse");
        }

        protected override void FillCacheIfNeeded()
        {
            SchoolCourses.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly SchoolCourseCache _schoolCourseCache = new();

    public static List<SchoolCourse> GetDeepCopy(bool isShort = false)
    {
        return _schoolCourseCache.GetDeepCopy(isShort);
    }

    public static SchoolCourse GetFirstOrDefault(Func<SchoolCourse, bool> funcMatch, bool isShort = false)
    {
        return _schoolCourseCache.GetFirstOrDefault(funcMatch, isShort);
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
        _schoolCourseCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _schoolCourseCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _schoolCourseCache.ClearCache();
    }

    #endregion
}