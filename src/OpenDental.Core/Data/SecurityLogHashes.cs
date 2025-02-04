using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class SecurityLogHashes
{
    public static void Insert(SecurityLogHash securityLogHash)
    {
        SecurityLogHashCrud.Insert(securityLogHash);
    }

    public static void InsertNoCache(SecurityLogHash securityLogHash)
    {
        SecurityLogHashCrud.InsertNoCache(securityLogHash);
    }

    public static void InsertSecurityLogHash(long securityLogNum)
    {
        var securityLog = SecurityLogs.GetOne(securityLogNum);
        if (securityLog is null)
        {
            Thread.Sleep(100);
            securityLog = SecurityLogs.GetOne(securityLogNum);
        }

        if (securityLog is null)
        {
            return;
        }

        Insert(new SecurityLogHash
        {
            SecurityLogNum = securityLog.SecurityLogNum,
            LogHash = GetHashString(securityLog)
        });
    }

    public static void InsertSecurityLogHashNoCache(long securityLogNum)
    {
        var securityLog = SecurityLogCrud.SelectOne(securityLogNum);

        InsertNoCache(new SecurityLogHash
        {
            SecurityLogNum = securityLog.SecurityLogNum,
            LogHash = GetHashString(securityLog)
        });
    }

    public static void InsertMany(List<SecurityLogHash> securityLogHashes)
    {
        SecurityLogHashCrud.InsertMany(securityLogHashes);
    }

    public static string GetHashString(SecurityLog securityLog)
    {
        HashAlgorithm hashAlgorithm = SHA256.Create();

        var str = "";

        str += ((int) securityLog.PermType).ToString();
        str += securityLog.UserNum;
        str += SOut.DateTime(securityLog.LogDateTime, false);
        str += securityLog.LogText;
        str += securityLog.PatNum;

        if (securityLog.DateTPrevious != DateTime.MinValue)
        {
            str += SOut.DateTime(securityLog.DateTPrevious, false);
        }

        var bytes = Encoding.Unicode.GetBytes(str);
        var hash = hashAlgorithm.ComputeHash(bytes);

        return Convert.ToBase64String(hash);
    }
}