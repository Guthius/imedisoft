using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ApptFieldDefs
{
    public static void Update(ApptFieldDef apptFieldDef, string fieldNameOld)
    {
        var command = "SELECT COUNT(*) FROM apptfielddef WHERE FieldName='" + SOut.String(apptFieldDef.FieldName) + "' "
                      + "AND ApptFieldDefNum != " + apptFieldDef.ApptFieldDefNum;
        if (Db.GetCount(command) != "0") throw new ApplicationException(Lans.g("FormApptFieldDefEdit", "Field name already in use."));
        ApptFieldDefCrud.Update(apptFieldDef);
        command = "UPDATE apptfield SET FieldName='" + SOut.String(apptFieldDef.FieldName) + "' "
                  + "WHERE FieldName='" + SOut.String(fieldNameOld) + "'";
        Db.NonQ(command);
    }

    public static void Insert(ApptFieldDef apptFieldDef)
    {
        var command = "SELECT COUNT(*) FROM apptfielddef WHERE FieldName='" + SOut.String(apptFieldDef.FieldName) + "'";
        if (Db.GetCount(command) != "0") throw new ApplicationException(Lans.g("FormApptFieldDefEdit", "Field name already in use."));
        ApptFieldDefCrud.Insert(apptFieldDef);
    }

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
                s += table.Rows[i]["LName"] + ", " + table.Rows[i]["FName"] + SOut.DateTime(aptDateTime, false) + "\r\n";
            }

            throw new ApplicationException(s);
        }

        command = "DELETE FROM apptfielddef WHERE ApptFieldDefNum =" + apptFieldDef.ApptFieldDefNum;
        Db.NonQ(command);
    }

    public static void Sync(List<ApptFieldDef> listApptFieldDefsNew)
    {
        var command = "SELECT * FROM apptfielddef";
        var listApptFieldDefsDB = ApptFieldDefCrud.SelectMany(command);
        ApptFieldDefCrud.Sync(listApptFieldDefsNew, listApptFieldDefsDB);
    }

    public static string GetFieldName(long apptFieldDefNum)
    {
        var apptFieldDef = GetFirstOrDefault(x => x.ApptFieldDefNum == apptFieldDefNum);
        var fieldName = apptFieldDef == null ? "" : apptFieldDef.FieldName;
        return fieldName;
    }

    public static string GetPickListByFieldName(string fieldName)
    {
        var apptFieldDef = GetFirstOrDefault(x => x.FieldName == fieldName);
        var pickList = apptFieldDef == null ? "" : apptFieldDef.PickList;
        return pickList;
    }

    public static bool HasDuplicateFieldNames()
    {
        var command = "SELECT COUNT(*) FROM apptfielddef GROUP BY FieldName HAVING COUNT(FieldName) > 1";
        return DataCore.GetScalar(command) != "";
    }

    public static ApptFieldDef GetFieldDefByFieldName(string fieldName)
    {
        return GetFirstOrDefault(x => x.FieldName == fieldName);
    }
    
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

    private static readonly ApptFieldDefCache Cache = new();

    public static bool GetExists(Predicate<ApptFieldDef> match, bool isShort = false)
    {
        return Cache.GetExists(match, isShort);
    }

    public static List<ApptFieldDef> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static ApptFieldDef GetFirstOrDefault(Func<ApptFieldDef, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}