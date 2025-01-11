using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ApptFieldDefs
{
	/// <summary>
	///     Must supply the old field name so that the apptFields attached to appointments can be updated.  Will throw
	///     exception if new FieldName is already in use.
	/// </summary>
	public static void Update(ApptFieldDef apptFieldDef, string fieldNameOld)
    {
        var command = "SELECT COUNT(*) FROM apptfielddef WHERE FieldName='" + SOut.String(apptFieldDef.FieldName) + "' "
                      + "AND ApptFieldDefNum != " + SOut.Long(apptFieldDef.ApptFieldDefNum);
        if (Db.GetCount(command) != "0") throw new ApplicationException(Lans.g("FormApptFieldDefEdit", "Field name already in use."));
        ApptFieldDefCrud.Update(apptFieldDef);
        command = "UPDATE apptfield SET FieldName='" + SOut.String(apptFieldDef.FieldName) + "' "
                  + "WHERE FieldName='" + SOut.String(fieldNameOld) + "'";
        Db.NonQ(command);
    }

    ///<summary>Surround with try/catch in case field name already in use.</summary>
    public static long Insert(ApptFieldDef apptFieldDef)
    {
        var command = "SELECT COUNT(*) FROM apptfielddef WHERE FieldName='" + SOut.String(apptFieldDef.FieldName) + "'";
        if (Db.GetCount(command) != "0") throw new ApplicationException(Lans.g("FormApptFieldDefEdit", "Field name already in use."));
        return ApptFieldDefCrud.Insert(apptFieldDef);
    }

    ///<summary>Surround with try/catch, because it will throw an exception if any appointment is using this def.</summary>
    public static void Delete(ApptFieldDef apptFieldDef)
    {
        var command = "SELECT LName,FName,AptDateTime "
                      + "FROM patient,apptfield,appointment WHERE "
                      + "patient.PatNum=appointment.PatNum "
                      + "AND appointment.AptNum=apptfield.AptNum "
                      + "AND FieldName='" + SOut.String(apptFieldDef.FieldName) + "'";
        var table = DataCore.GetTable(command);
        DateTime aptDateTime;
        if (table.Rows.Count > 0)
        {
            var s = Lans.g("FormApptFieldDefEdit", "Not allowed to delete. Already in use by ") + table.Rows.Count
                                                                                                + " " + Lans.g("FormApptFieldDefEdit", "appointments, including") + " \r\n";
            for (var i = 0; i < table.Rows.Count; i++)
            {
                if (i > 5) break;
                aptDateTime = SIn.DateTime(table.Rows[i]["AptDateTime"].ToString());
                s += table.Rows[i]["LName"] + ", " + table.Rows[i]["FName"] + SOut.DateT(aptDateTime, false) + "\r\n";
            }

            throw new ApplicationException(s);
        }

        command = "DELETE FROM apptfielddef WHERE ApptFieldDefNum =" + SOut.Long(apptFieldDef.ApptFieldDefNum);
        Db.NonQ(command);
    }

    public static bool Sync(List<ApptFieldDef> listApptFieldDefsNew)
    {
        var command = "SELECT * FROM apptfielddef";
        var listApptFieldDefsDB = ApptFieldDefCrud.SelectMany(command);
        return ApptFieldDefCrud.Sync(listApptFieldDefsNew, listApptFieldDefsDB);
    }

    public static string GetFieldName(long apptFieldDefNum)
    {
        var apptFieldDef = GetFirstOrDefault(x => x.ApptFieldDefNum == apptFieldDefNum);
        var fieldName = apptFieldDef == null ? "" : apptFieldDef.FieldName;
        return fieldName;
    }

    /// <summary>GetPickListByFieldName returns the pick list identified by the field name passed as a parameter.</summary>
    public static string GetPickListByFieldName(string fieldName)
    {
        var apptFieldDef = GetFirstOrDefault(x => x.FieldName == fieldName);
        var pickList = apptFieldDef == null ? "" : apptFieldDef.PickList;
        return pickList;
    }

    ///<summary>Returns true if there are any duplicate field names in the entire apptfielddef table.</summary>
    public static bool HasDuplicateFieldNames()
    {
        var command = "SELECT COUNT(*) FROM apptfielddef GROUP BY FieldName HAVING COUNT(FieldName) > 1";
        return DataCore.GetScalar(command) != "";
    }

    /// <summary>
    ///     Returns the ApptFieldDef for the specified field name. Returns null if an ApptFieldDef does not exist for that
    ///     field name.
    /// </summary>
    public static ApptFieldDef GetFieldDefByFieldName(string fieldName)
    {
        return GetFirstOrDefault(x => x.FieldName == fieldName);
    }

    #region CachePattern

    private class ApptFieldDefCache : CacheListAbs<ApptFieldDef>
    {
        protected override List<ApptFieldDef> GetCacheFromDb()
        {
            var command = "SELECT * FROM apptfielddef ORDER BY ItemOrder";
            return ApptFieldDefCrud.SelectMany(command);
        }

        protected override List<ApptFieldDef> TableToList(DataTable dataTable)
        {
            return ApptFieldDefCrud.TableToList(dataTable);
        }

        protected override ApptFieldDef Copy(ApptFieldDef item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<ApptFieldDef> items)
        {
            return ApptFieldDefCrud.ListToTable(items, "ApptFieldDef");
        }

        protected override void FillCacheIfNeeded()
        {
            ApptFieldDefs.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ApptFieldDefCache _apptFieldDefCache = new();

    public static int GetCount(bool isShort = false)
    {
        return _apptFieldDefCache.GetCount(isShort);
    }

    public static bool GetExists(Predicate<ApptFieldDef> match, bool isShort = false)
    {
        return _apptFieldDefCache.GetExists(match, isShort);
    }

    public static List<ApptFieldDef> GetDeepCopy(bool isShort = false)
    {
        return _apptFieldDefCache.GetDeepCopy(isShort);
    }

    public static ApptFieldDef GetFirstOrDefault(Func<ApptFieldDef, bool> match, bool isShort = false)
    {
        return _apptFieldDefCache.GetFirstOrDefault(match, isShort);
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
        _apptFieldDefCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _apptFieldDefCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _apptFieldDefCache.ClearCache();
    }

    #endregion
}