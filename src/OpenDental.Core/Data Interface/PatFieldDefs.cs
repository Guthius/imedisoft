using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class PatFieldDefs
{
    
    public static void Update(PatFieldDef patFieldDef)
    {
        PatFieldDefCrud.Update(patFieldDef);
    }

    
    public static long Insert(PatFieldDef patFieldDef)
    {
        return PatFieldDefCrud.Insert(patFieldDef);
    }

    ///<summary>Surround with try/catch, because it will throw an exception if any patient is using this def.</summary>
    public static void Delete(PatFieldDef patFieldDef)
    {
        var command = "SELECT LName,FName FROM patient,patfield WHERE "
                      + "patient.PatNum=patfield.PatNum "
                      + "AND FieldName='" + SOut.String(patFieldDef.FieldName) + "'";
        var table = DataCore.GetTable(command);
        if (table.Rows.Count > 0)
        {
            var s = Lans.g("PatFieldDef", "Not allowed to delete. Already in use by ") + table.Rows.Count
                                                                                       + " " + Lans.g("PatFieldDef", "patients, including") + " \r\n";
            for (var i = 0; i < table.Rows.Count; i++)
            {
                if (i > 5) break;
                s += table.Rows[i][0] + ", " + table.Rows[i][1] + "\r\n";
            }

            throw new ApplicationException(s);
        }

        if (GetListPatFieldTypesCareCredit().Contains(patFieldDef.FieldType))
        {
            var careCreditProgNum = Programs.GetProgramNum(ProgramName.CareCredit);
            command = $@"SELECT COUNT(*) FROM programproperty 
					WHERE ProgramNum={SOut.Long(careCreditProgNum)} 
					AND PropertyDesc IN ('{SOut.String(ProgramProperties.PropertyDescs.CareCredit.GetForPatFieldType(patFieldDef.FieldType))}') 
					AND PropertyValue='{SOut.String(patFieldDef.PatFieldDefNum.ToString())}'";
            if (Db.GetLong(command) != 0)
                //CareCredit type has a reference to program property, don't allow
                throw new ApplicationException(Lans.g("PatFieldDef", "Not allowed to delete. Already in use by CareCredit."));
        }

        command = "DELETE FROM patfielddef WHERE PatFieldDefNum =" + SOut.Long(patFieldDef.PatFieldDefNum);
        Db.NonQ(command);
    }

    public static PatFieldDef GetPatFieldCareCredit(PatFieldType patFieldType = PatFieldType.CareCreditStatus)
    {
        var listPatFieldTypes = Enum.GetValues(typeof(PatFieldType)).Cast<PatFieldType>().ToList();
        var listPatFieldTypesCareCredit = GetListPatFieldTypesCareCredit();
        if (!listPatFieldTypesCareCredit.Contains(patFieldType)) return null;
        var propertyDesc = ProgramProperties.PropertyDescs.CareCredit.CareCreditPatField;
        if (patFieldType == PatFieldType.CareCreditPreApprovalAmt)
            propertyDesc = ProgramProperties.PropertyDescs.CareCredit.CareCreditPatFieldPreApprovalAmt;
        else if (patFieldType == PatFieldType.CareCreditAvailableCredit) propertyDesc = ProgramProperties.PropertyDescs.CareCredit.CareCreditPatFieldAvailableCredit;
        var propCC = ProgramProperties.GetPropForProgByDesc(Programs.GetProgramNum(ProgramName.CareCredit), propertyDesc);
        if (propCC == null) return null;
        var propertyValue = SIn.Long(propCC.PropertyValue, false);
        var patFieldDef = GetFirstOrDefault(x => x.PatFieldDefNum == propertyValue && x.FieldType == patFieldType);
        return patFieldDef;
    }

    public static List<PatFieldType> GetListPatFieldTypesCareCredit()
    {
        var listPatFieldTypes = Enum.GetValues(typeof(PatFieldType)).Cast<PatFieldType>().ToList();
        var listPatFieldTypesCareCredit = listPatFieldTypes.FindAll(x => x.ToString().ToLower().Contains("carecredit"));
        return listPatFieldTypesCareCredit;
    }

    /// <summary>
    ///     Returns the PatFieldDef for the specified field name. Returns null if an PatFieldDef does not exist for that
    ///     field name.
    /// </summary>
    public static PatFieldDef GetFieldDefByFieldName(string fieldName)
    {
        return GetFirstOrDefault(x => x.FieldName == fieldName);
    }

    /// <summary>GetFieldName returns the field name identified by the field definition number passed as a parameter.</summary>
    public static string GetFieldName(long patFieldDefNum)
    {
        var patFieldDef = GetFirstOrDefault(x => x.PatFieldDefNum == patFieldDefNum, true);
        return patFieldDef == null ? "" : patFieldDef.FieldName;
    }

    /// <summary>GetPickListByFieldName returns the pick list identified by the field name passed as a parameter.</summary>
    public static string GetPickListByFieldName(string FieldName)
    {
        var patFieldDef = GetFirstOrDefault(x => x.FieldName == FieldName, true);
        return patFieldDef == null ? "" : patFieldDef.PickList;
    }

    ///<summary>Gets one PatFieldDef from the DB. Used in the API.</summary>
    public static PatFieldDef GetPatFieldDefForApi(long patFieldDefNum)
    {
        var command = "SELECT * FROM patfielddef"
                      + " WHERE PatFieldDefNum = " + SOut.Long(patFieldDefNum);
        return PatFieldDefCrud.SelectOne(patFieldDefNum);
    }

    ///<summary>Gets all PatFieldDefs from the DB. Used in the API.</summary>
    public static List<PatFieldDef> GetPatFieldDefsForApi(int limit, int offset)
    {
        var command = "SELECT * FROM patfielddef ";
        command += "ORDER BY PatFieldDefNum "
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit) + "";
        return PatFieldDefCrud.SelectMany(command);
    }

    ///<summary>Sync pattern, must sync entire table. Probably only to be used in the master problem list window.</summary>
    public static void Sync(List<PatFieldDef> listDefs, List<PatFieldDef> listDefsOld)
    {
        PatFieldDefCrud.Sync(listDefs, listDefsOld);
    }

    #region CachePattern

    private class PatFieldDefCache : CacheListAbs<PatFieldDef>
    {
        protected override List<PatFieldDef> GetCacheFromDb()
        {
            var command = "SELECT * FROM patfielddef ORDER BY ItemOrder";
            return PatFieldDefCrud.SelectMany(command);
        }

        protected override List<PatFieldDef> TableToList(DataTable dataTable)
        {
            return PatFieldDefCrud.TableToList(dataTable);
        }

        protected override PatFieldDef Copy(PatFieldDef item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<PatFieldDef> items)
        {
            return PatFieldDefCrud.ListToTable(items, "PatFieldDef");
        }

        protected override void FillCacheIfNeeded()
        {
            PatFieldDefs.GetTableFromCache(false);
        }

        protected override bool IsInListShort(PatFieldDef item)
        {
            return !item.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly PatFieldDefCache _patFieldDefCache = new();

    public static int GetCount(bool isShort = false)
    {
        return _patFieldDefCache.GetCount(isShort);
    }

    public static bool GetExists(Predicate<PatFieldDef> match, bool isShort = false)
    {
        return _patFieldDefCache.GetExists(match, isShort);
    }

    public static List<PatFieldDef> GetDeepCopy(bool isShort = false)
    {
        return _patFieldDefCache.GetDeepCopy(isShort);
    }

    public static PatFieldDef GetFirstOrDefault(Func<PatFieldDef, bool> match, bool isShort = false)
    {
        return _patFieldDefCache.GetFirstOrDefault(match, isShort);
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
        _patFieldDefCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _patFieldDefCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _patFieldDefCache.ClearCache();
    }

    #endregion
}