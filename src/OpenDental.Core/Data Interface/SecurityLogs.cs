using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class SecurityLogs
{
    public static LogSources LogSource = LogSources.None;

    public static SecurityLog[] Refresh(DateTime dateFrom, DateTime dateTo, EnumPermType permType, long patNum, DateTime datePreviousFrom, DateTime datePreviousTo, int limit = 0, long userNum = -1, int logSource = -1)
    {
        var command = "SELECT securitylog.*,LName,FName,Preferred,MiddleI,LogHash FROM securitylog "
                      + "LEFT JOIN patient ON patient.PatNum=securitylog.PatNum "
                      + "LEFT JOIN securityloghash ON securityloghash.SecurityLogNum=securitylog.SecurityLogNum "
                      + "WHERE LogDateTime >= " + SOut.Date(dateFrom) + " "
                      + "AND LogDateTime <= " + SOut.Date(dateTo.AddDays(1)) + " "
                      + "AND DateTPrevious >= " + SOut.Date(datePreviousFrom) + " "
                      + "AND DateTPrevious <= " + SOut.Date(datePreviousTo.AddDays(1));
        if (patNum != 0)
            command += " AND securitylog.PatNum IN (" + string.Join(",",
                PatientLinks.GetPatNumsLinkedToRecursive(patNum, PatientLinkType.Merge).Select(x => x)) + ")";
        if (permType != EnumPermType.None) command += " AND PermType=" + (int) permType;
        if (userNum >= 0) //Greater than or equal to 0, since 0 is no/unknown user, and we want to be able to filter by that option in some cases.
            command += " AND UserNum=" + userNum;
        if (logSource >= 0) //Greater than or equal to 0, since 0 is Automation/unknown, and we want to be able to filter by that option in some cases.
            command += " AND LogSource=" + logSource;
        command += " ORDER BY LogDateTime DESC"; //Using DESC so that the most recent ones appear in the list
        if (limit > 0) command = DbHelper.LimitOrderBy(command, limit);
        var table = DataCore.GetTable(command);
        var listSecurityLogs = SecurityLogCrud.TableToList(table);
        for (var i = 0; i < listSecurityLogs.Count; i++)
        {
            if (table.Rows[i]["PatNum"].ToString() == "0")
                listSecurityLogs[i].PatientName = "";
            else
                listSecurityLogs[i].PatientName = table.Rows[i]["PatNum"] + "-"
                                                                          + Patients.GetNameLF(table.Rows[i]["LName"].ToString()
                                                                              , table.Rows[i]["FName"].ToString()
                                                                              , table.Rows[i]["Preferred"].ToString()
                                                                              , table.Rows[i]["MiddleI"].ToString());
            listSecurityLogs[i].LogHash = table.Rows[i]["LogHash"].ToString();
        }

        return listSecurityLogs.OrderBy(x => x.LogDateTime).ToArray();
    }

    public static long Insert(SecurityLog securityLog)
    {
        return SecurityLogCrud.Insert(securityLog);
    }

    public static SecurityLog[] Refresh(long patNum, List<EnumPermType> listPermissionsEnums, long fKey)
    {
        return Refresh(patNum, listPermissionsEnums, [fKey]);
    }

    public static SecurityLog[] Refresh(long patNum, List<EnumPermType> listPermissionsEnums, List<long> listFKeys)
    {
        var types = "";
        for (var i = 0; i < listPermissionsEnums.Count; i++)
        {
            if (i > 0) types += " OR";
            types += " PermType=" + (int) listPermissionsEnums[i];
        }

        var command = "SELECT * FROM securitylog "
                      + "WHERE (" + types + ") ";
        if (listFKeys != null && listFKeys.Count > 0) command += "AND FKey IN (" + string.Join(",", listFKeys) + ") ";
        if (patNum != 0) //appointments
            command += " AND PatNum IN (" + string.Join(",",
                PatientLinks.GetPatNumsLinkedToRecursive(patNum, PatientLinkType.Merge).Select(x => x)) + ")";
        command += "ORDER BY LogDateTime";
        var listSecurityLogs = SecurityLogCrud.SelectMany(command);
        return listSecurityLogs.OrderBy(x => x.LogDateTime).ToArray();
    }

    public static void MakeLogEntries(EnumPermType permType, long patNum, List<string> listLogTexts)
    {
        if (listLogTexts == null || listLogTexts.Count == 0) return;

        for (var i = 0; i < listLogTexts.Count; i++) MakeLogEntry(permType, patNum, listLogTexts[i]);
    }

    public static void MakeLogEntry(EnumPermType permType, long patNum, string logText)
    {
        MakeLogEntry(permType, patNum, logText, 0, LogSource, DateTime.MinValue);
    }

    public static void MakeLogEntry(EnumPermType permType, long patNum, string logText, LogSources logSource)
    {
        MakeLogEntry(permType, patNum, logText, 0, logSource, DateTime.MinValue);
    }

    public static void MakeLogEntry(EnumPermType permType, long patNum, string logText, long fKey, DateTime DateTPrevious)
    {
        MakeLogEntry(permType, patNum, logText, fKey, LogSource, DateTPrevious);
    }

    public static void MakeLogEntry(EnumPermType permType, long patNum, string logText, long fKey, LogSources logSource, DateTime DateTPrevious)
    {
        MakeLogEntry(permType, patNum, logText, fKey, logSource, 0, 0, DateTPrevious);
    }

    public static void MakeLogEntry(EnumPermType permType, long patNum, string logText, long fKey, LogSources logSource, DateTime DateTPrevious, long userNum)
    {
        var securityLog = MakeLogEntryNoInsert(permType, patNum, logText, fKey, logSource, 0, 0, DateTPrevious, userNum);
        MakeLogEntry(securityLog);
    }

    public static void MakeLogEntry(EnumPermType permType, long patNum, string logText, long fKey, LogSources logSource, long defNum, long defNumError, DateTime DateTPrevious)
    {
        var securityLog = MakeLogEntryNoInsert(permType, patNum, logText, fKey, logSource, defNum, defNumError, DateTPrevious);
        MakeLogEntry(securityLog);
    }

    public static void MakeLogEntry(SecurityLog securityLog)
    {
        securityLog.SecurityLogNum = Insert(securityLog);
        
        SecurityLogHashes.InsertSecurityLogHash(securityLog.SecurityLogNum);
        
        if (securityLog.PermType == EnumPermType.AppointmentCreate)
        {
            EntryLogs.Insert(new EntryLog
            {
                UserNum = securityLog.UserNum,
                FKeyType = EntryLogFKeyType.Appointment,
                FKey = securityLog.FKey,
                LogSource = securityLog.LogSource
            });
        }
    }

    public static void MakeLogEntry(EnumPermType permType, List<long> patNums, string logText)
    {
        var securityLogs = new List<SecurityLog>();
        
        foreach (var patNum in patNums)
        {
            var securityLog = MakeLogEntryNoInsert(permType, patNum, logText, 0, LogSource);
            
            Insert(securityLog);
            
            securityLogs.Add(securityLog);
        }

        var listSecurityLogHashes = new List<SecurityLogHash>();
        var listEntryLogs = new List<EntryLog>();
        var listSecurityLogNums = securityLogs.Select(x => x.SecurityLogNum).ToList();
        var sQLWhere = SQLWhere.CreateIn(nameof(SecurityLog.SecurityLogNum), listSecurityLogNums);
        securityLogs = GetMany(sQLWhere);
        foreach (var securityLog in securityLogs)
        {
            var securityLogHash = new SecurityLogHash
            {
                SecurityLogNum = securityLog.SecurityLogNum,
                LogHash = SecurityLogHashes.GetHashString(securityLog)
            };
            
            listSecurityLogHashes.Add(securityLogHash);
            
            if (securityLog.PermType == EnumPermType.AppointmentCreate)
            {
                listEntryLogs.Add(new EntryLog
                {
                    UserNum = securityLog.UserNum,
                    FKeyType = EntryLogFKeyType.Appointment,
                    FKey = securityLog.FKey,
                    LogSource = securityLog.LogSource
                });
            }
        }

        EntryLogs.InsertMany(listEntryLogs);
        SecurityLogHashes.InsertMany(listSecurityLogHashes);
    }

    public static SecurityLog MakeLogEntryNoInsert(EnumPermType permType, long patNum, string logText, long fKey, LogSources logSource, long defNum = 0, long defNumError = 0, DateTime DateTPrevious = default, long userNum = 0)
    {
        var securityLog = new SecurityLog
        {
            PermType = permType,
            UserNum = userNum,
            LogText = logText,
            CompName = Environment.MachineName,
            PatNum = patNum,
            FKey = fKey,
            LogSource = logSource,
            DefNum = defNum,
            DefNumError = defNumError,
            DateTPrevious = DateTPrevious
        };

        if (userNum == 0)
        {
            securityLog.UserNum = Security.CurUser.UserNum;
        }
        
        return securityLog;
    }

    public static string AppendProcCompleteEditSecurityLog(Procedure procNew, Procedure procOld)
    {
        var logText = "";
        if (procNew == null || procOld == null) return logText;
        if (procNew.CodeNum != procOld.CodeNum)
        {
            var oldProcCode = ProcedureCodes.GetStringProcCode(procOld.CodeNum, doThrowIfMissing: false);
            var newProcCode = ProcedureCodes.GetStringProcCode(procNew.CodeNum, doThrowIfMissing: false);
            logText += "\nCode " + oldProcCode + " with fee of " + procOld.ProcFee.ToString("F") + " changed to code " + newProcCode + " with fee of " + procNew.ProcFee.ToString("F");
        }

        if (procNew.ProcDate != procOld.ProcDate) logText += "\nProcDate changed from " + procOld.ProcDate.ToShortDateString() + " to " + procNew.ProcDate.ToShortDateString();
        if (procNew.Surf != procOld.Surf)
        {
            //because Surf could be changed to or from blank, print "none" instead
            logText += "\nSurf changed from ";
            if (procOld.Surf == null)
                logText += "none to ";
            else
                logText += procOld.Surf + " to ";
            if (procNew.Surf == null)
                logText += "none";
            else
                logText += procNew.Surf;
        }

        if (procNew.ToothNum != procOld.ToothNum)
        {
            //because ToothNum could be changed to or from blank, print "none" instead
            logText += Lans.g("Procedures", "\nToothNum changed from ");
            if (procOld.ToothNum == null)
                logText += Lans.g("Procedures", "none to");
            else
                logText += procOld.ToothNum + " to ";
            if (procNew.ToothNum == null)
                logText += Lans.g("Procedures", "none");
            else
                logText += procNew.ToothNum;
        }

        if (procNew.ToothRange != procOld.ToothRange) logText += Lans.g("Procedures", "\nToothRange changed from ") + procOld.ToothRange + Lans.g("Procedures", " to ") + procNew.ToothRange;
        return logText;
    }

    public static SecurityLog GetOne(long securityLogNum)
    {
        return SecurityLogCrud.SelectOne(securityLogNum);
    }

    public static List<SecurityLog> GetMany(params SQLWhere[] sQLWhereArray)
    {
        return GetMany(sQLWhereArray.ToList());
    }

    public static List<SecurityLog> GetMany(List<SQLWhere> listSQLWheres)
    {
        var command = "SELECT * FROM securitylog ";
        if (listSQLWheres != null && listSQLWheres.Count > 0) command += "WHERE " + string.Join(" AND ", listSQLWheres);
        return SecurityLogCrud.SelectMany(command);
    }
}