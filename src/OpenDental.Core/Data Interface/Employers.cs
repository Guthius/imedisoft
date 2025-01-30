using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Employers
{
    public static void Update(Employer employerNew, Employer employerOld)
    {
        EmployerCrud.Update(employerNew, employerOld);
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        InsEditLogs.MakeLogEntry(employerNew, employerOld, InsEditLogType.Employer, Security.CurUser.UserNum);
    }

    public static void Insert(Employer employer)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        InsEditLogs.MakeLogEntry(employer, null, InsEditLogType.Employer, Security.CurUser.UserNum);
        EmployerCrud.Insert(employer);
    }

    public static void Delete(Employer employer)
    {
        var command = "DELETE from employer WHERE EmployerNum = '" + employer.EmployerNum + "'";
        Db.NonQ(command);
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        InsEditLogs.MakeLogEntry(null, employer, InsEditLogType.Employer, Security.CurUser.UserNum);
    }

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

    public static string GetName(long employerNum)
    {
        var employer = GetEmployer(employerNum);
        if (employer.EmpName == null) return "";
        return employer.EmpName;
    }

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

    public static Employer GetByName(string empName)
    {
        var command = "SELECT * FROM employer WHERE EmpName = '" + SOut.String(empName) + "'";
        return EmployerCrud.SelectOne(command);
    }

    public static List<Employer> GetAllByName(string empName)
    {
        var command = "SELECT * FROM employer WHERE EmpName = '" + SOut.String(empName) + "'";
        return EmployerCrud.SelectMany(command);
    }

    public static List<Employer> GetSimilarNames(string empName)
    {
        return Cache.GetWhere(x => x.EmpName.StartsWith(empName, StringComparison.CurrentCultureIgnoreCase));
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

    private static readonly EmployerCache Cache = new();

    public static Employer GetOne(long employerNum)
    {
        return Cache.GetOne(employerNum);
    }

    public static List<Employer> GetListDeep(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort).Values.ToList();
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