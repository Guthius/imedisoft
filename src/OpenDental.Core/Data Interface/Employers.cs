using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

/// <summary>
///     Employers are refreshed as needed. A full refresh is frequently triggered if an employerNum cannot be found in
///     the HList.  Important retrieval is done directly from the db.
/// </summary>
public class Employers
{
    /*
     * Not using this because it turned out to be more efficient to refresh the whole
     * list if an empnum could not be found.
    ///<summary>Just refreshes Cur from the db with info for one employer.</summary>
    public static void Refresh(int employerNum){
        Cur=new Employer();//just in case no rows are returned
        if(employerNum==0) return;
        string command="SELECT * FROM employer WHERE EmployerNum = '"+employerNum+"'";
        DataTable table=DataCore.GetTable(command);;
        for(int i=0;i<table.Rows.Count;i++){//almost always just 1 row, but sometimes 0
            Cur.EmployerNum   =PIn.PInt   (table.Rows[i][0].ToString());
            Cur.EmpName       =PIn.PString(table.Rows[i][1].ToString());
        }
    }*/

    ///<summary>Gets employers from database. Returns an empty list if none found.</summary>
    public static List<Employer> GetEmployersForApi(int limit, int offset)
    {
        var command = "SELECT * FROM employer ";
        command += "ORDER BY employernum " //Ensure order for limit and offset
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return EmployerCrud.SelectMany(command);
    }

    public static void Update(Employer employerNew, Employer employerOld)
    {
        EmployerCrud.Update(employerNew, employerOld);
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        InsEditLogs.MakeLogEntry(employerNew, employerOld, InsEditLogType.Employer, Security.CurUser.UserNum);
    }

    public static long Insert(Employer employer)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        InsEditLogs.MakeLogEntry(employer, null, InsEditLogType.Employer, Security.CurUser.UserNum);
        return EmployerCrud.Insert(employer);
    }

    /// <summary>
    ///     There MUST not be any dependencies before calling this or there will be invalid foreign keys.
    ///     This is only called from FormEmployers after proper validation.
    /// </summary>
    public static void Delete(Employer employer)
    {
        var command = "DELETE from employer WHERE EmployerNum = '" + employer.EmployerNum + "'";
        Db.NonQ(command);
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        InsEditLogs.MakeLogEntry(null, employer, InsEditLogType.Employer, Security.CurUser.UserNum);
    }

    /// <summary>
    ///     Returns a list of patients that are dependent on the Cur employer. The list includes carriage returns for easy
    ///     display.  Used before deleting an employer to make sure employer is not in use.
    /// </summary>
    public static string DependentPatients(Employer employer)
    {
        var command = "SELECT CONCAT(CONCAT(LName,', '),FName) FROM patient"
                      + " WHERE EmployerNum = '" + SOut.Long(employer.EmployerNum) + "'";
        var table = DataCore.GetTable(command);
        var retStr = "";
        for (var i = 0; i < table.Rows.Count; i++)
        {
            if (i > 0) retStr += "\r\n"; //return, newline for multiple names.
            retStr += SIn.String(table.Rows[i][0].ToString());
        }

        return retStr;
    }

    /// <summary>
    ///     Returns a list of insplans that are dependent on the Cur employer. The list includes carriage returns for easy
    ///     display.  Used before deleting an employer to make sure employer is not in use.
    /// </summary>
    public static string DependentInsPlans(Employer employer)
    {
        var command = "SELECT carrier.CarrierName,CONCAT(CONCAT(patient.LName,', '),patient.FName) "
                      + "FROM insplan "
                      + "LEFT JOIN inssub ON insplan.PlanNum=inssub.PlanNum "
                      + "LEFT JOIN patient ON inssub.Subscriber=patient.PatNum "
                      + "LEFT JOIN carrier ON insplan.CarrierNum=carrier.CarrierNum "
                      + "WHERE insplan.EmployerNum = " + SOut.Long(employer.EmployerNum);
        var table = DataCore.GetTable(command);
        var retStr = "";
        for (var i = 0; i < table.Rows.Count; i++)
        {
            if (i > 0) retStr += "\r\n"; //return, newline for multiple names.
            retStr += SIn.String(table.Rows[i][1].ToString()) + ": " + SIn.String(table.Rows[i][0].ToString());
        }

        return retStr;
    }

    /// <summary>
    ///     Gets the name of an employer based on the employerNum.  This also refreshes the list if necessary, so it will
    ///     work even if the list has not been refreshed recently.
    /// </summary>
    public static string GetName(long employerNum)
    {
        var employer = GetEmployer(employerNum);
        if (employer.EmpName == null) return "";
        return employer.EmpName;
    }

    /// <summary>
    ///     Gets an employer based on the employerNum. This will work even if the list has not been refreshed recently,
    ///     but if you are going to need a lot of names all at once, then it is faster to refresh first.
    /// </summary>
    public static Employer GetEmployer(long employerNum)
    {
        if (employerNum == 0) return new Employer();
        Employer employer = null;
        ODException.SwallowAnyException(() => { employer = GetOne(employerNum); });
        if (employer == null)
        {
            RefreshCache();
            ODException.SwallowAnyException(() => { employer = GetOne(employerNum); });
        }

        if (employer == null) return new Employer(); //Could only happen if corrupted or we're looking up an employer that no longer exists.
        return employer;
    }

    public static Employer GetEmployerNoCache(long employerNum)
    {
        if (employerNum == 0) return null;

        return EmployerCrud.SelectOne(employerNum);
    }

    /// <summary>
    ///     Gets an employerNum from the database based on the supplied name.  If that empName does not exist, then a new
    ///     employer is created, and the employerNum for the new employer is returned.
    /// </summary>
    public static long GetEmployerNum(string empName)
    {
        if (empName == "") return 0;
        var command = "SELECT EmployerNum FROM employer"
                      + " WHERE EmpName = '" + SOut.String(empName) + "'";
        var table = DataCore.GetTable(command);
        if (table.Rows.Count > 0) return SIn.Long(table.Rows[0][0].ToString());
        var employer = new Employer();
        employer.EmpName = empName;
        Insert(employer);
        Signalods.Insert(new Signalod {IType = InvalidType.Employers}); //Signal to other workstations to refresh their caches as a new employer was inserted into the DB.
        //MessageBox.Show(Cur.EmployerNum.ToString());
        return employer.EmployerNum;
    }

    /// <summary>
    ///     Returns an employer if an exact match is found for the text supplied in the database.  Returns null if nothing
    ///     found.
    /// </summary>
    public static Employer GetByName(string empName)
    {
        var command = "SELECT * FROM employer WHERE EmpName = '" + SOut.String(empName) + "'";
        return EmployerCrud.SelectOne(command);
    }

    ///<summary>Returns all employers with matching name, case-insensitive.</summary>
    public static List<Employer> GetAllByName(string empName)
    {
        var command = "SELECT * FROM employer WHERE EmpName = '" + SOut.String(empName) + "'";
        return EmployerCrud.SelectMany(command);
    }

    /// <summary>
    ///     Returns an arraylist of Employers with names similar to the supplied string.  Used in dropdown list from
    ///     employer field for faster entry.  There is a small chance that the list will not be completely refreshed when this
    ///     is run, but it won't really matter if one employer doesn't show in dropdown.
    /// </summary>
    public static List<Employer> GetSimilarNames(string empName)
    {
        return _employerCache.GetWhere(x => x.EmpName.StartsWith(empName, StringComparison.CurrentCultureIgnoreCase));
    }

    public static void MakeLog(Employer employer, LogSources logSources = LogSources.None)
    {
        var retVal = "";
        retVal = "Creating 'EmployerNum #" + employer.EmployerNum + ":\r\n";
        retVal += "   Employer Name: " + employer.EmpName + "\r\n";
        if (!employer.Phone.IsNullOrEmpty()) retVal += "   Phone: " + employer.Phone + "\r\n";
        if (!employer.Address.IsNullOrEmpty()) retVal += "   Address: " + employer.Address + "'\r\n";
        if (logSources == LogSources.EmployerImport834) retVal += "from Import 834.";
        SecurityLogs.MakeLogEntry(EnumPermType.EmployerCreate, 0, retVal, logSources);
    }

    ///<summary>Combines all the given employers into one. Updates patient and insplan. Then deletes all the others.</summary>
    public static void Combine(List<long> listEmployerNums)
    {
        var newNum = listEmployerNums[0];
        for (var i = 1; i < listEmployerNums.Count; i++)
        {
            var command = "SELECT PatNum FROM patient WHERE EmployerNum = " + SOut.Long(listEmployerNums[i]) + "";
            var listPatNums = Db.GetListLong(command);
            for (var j = 0; j < listPatNums.Count; j++)
            {
                command = "UPDATE patient SET EmployerNum = " + SOut.Long(newNum) + " WHERE PatNum = " + SOut.Long(listPatNums[j]) + "";
                Db.NonQ(command);
            }

            command = "SELECT * FROM insplan WHERE EmployerNum = " + SOut.Long(listEmployerNums[i]);
            var listInsPlans = InsPlanCrud.SelectMany(command);
            //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
            for (var j = 0; j < listInsPlans.Count; j++)
            {
                command = "UPDATE insplan SET EmployerNum = " + SOut.Long(newNum) + " WHERE PlanNum = " + SOut.Long(listInsPlans[j].PlanNum);
                Db.NonQ(command);
                InsEditLogs.MakeLogEntry("EmployerNum", Security.CurUser.UserNum, listEmployerNums[i].ToString(), newNum.ToString(),
                    InsEditLogType.InsPlan, listInsPlans[j].PlanNum, 0, listInsPlans[j].GroupNum + " - " + listInsPlans[j].GroupName);
            }

            var employer = GetEmployer(listEmployerNums[i]); //from the cache
            Delete(employer); //logging taken care of in Delete method.
        }
    }

    #region Cache Pattern

    private class EmployerCache : CacheDictAbs<Employer, long, Employer>
    {
        protected override List<Employer> GetCacheFromDb()
        {
            var command = "SELECT EmployerNum,EmpName,'' Address,'' Address2,'' City,'' State,'' Zip,'' Phone FROM employer";
            return EmployerCrud.SelectMany(command);
        }

        protected override List<Employer> TableToList(DataTable dataTable)
        {
            return EmployerCrud.TableToList(dataTable);
        }

        protected override Employer Copy(Employer item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(Dictionary<long, Employer> dict)
        {
            return EmployerCrud.ListToTable(dict.Values.ToList(), "Employer");
        }

        protected override void FillCacheIfNeeded()
        {
            Employers.GetTableFromCache(false);
        }

        protected override long GetDictKey(Employer item)
        {
            return item.EmployerNum;
        }

        protected override Employer GetDictValue(Employer item)
        {
            return item;
        }

        protected override Employer CopyValue(Employer employer)
        {
            return employer.Copy();
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly EmployerCache _employerCache = new();

    public static Employer GetOne(long employerNum)
    {
        return _employerCache.GetOne(employerNum);
    }

    public static List<Employer> GetListDeep(bool isShort = false)
    {
        return _employerCache.GetDeepCopy(isShort).Values.ToList();
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
        _employerCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _employerCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _employerCache.ClearCache();
    }

    #endregion Cache Pattern
}