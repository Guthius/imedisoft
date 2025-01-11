using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class SecurityLogHashes
{
    #region Get Methods

    public static SecurityLogHash GetOne(long securityLogHashNum)
    {
        return SecurityLogHashCrud.SelectOne(securityLogHashNum);
    }

    #endregion

    ///<summary>Inserts securityloghash into Db.</summary>
    public static long Insert(SecurityLogHash securityLogHash)
    {
        return SecurityLogHashCrud.Insert(securityLogHash);
    }

    /// <summary>
    ///     Insertion logic that doesn't use the cache. Has special cases for generating random PK's and handling Oracle
    ///     insertions.
    /// </summary>
    public static long InsertNoCache(SecurityLogHash securityLogHash)
    {
        return SecurityLogHashCrud.InsertNoCache(securityLogHash);
    }

    ///<summary>Creates a new SecurityLogHash entry in the Db.</summary>
    public static void InsertSecurityLogHash(long securityLogNum)
    {
        var securityLog = SecurityLogs.GetOne(securityLogNum); //need a fresh copy because of time stamps, etc.
        //Attempted fix for NADG problems with SecurityLogHash Insert attempts throwing null reference UEs. Job #695
        if (securityLog == null)
        {
            Thread.Sleep(100);
            securityLog = SecurityLogs.GetOne(securityLogNum); //need a fresh copy because of time stamps, etc.
        }

        if (securityLog == null)
            //We give up at this point.  The end result will be the securitylog row shows up as RED in the audit trail.
            //We don't want other things to fail/practice flow to be interrupted just because of securitylog issues.
            return;
        var securityLogHash = new SecurityLogHash();
        //Set the FK
        securityLogHash.SecurityLogNum = securityLog.SecurityLogNum;
        //Hash the securityLog
        securityLogHash.LogHash = GetHashString(securityLog);
        Insert(securityLogHash);
    }

    ///<summary>Used for inserting without using the cache.  Usually used when multithreading connections.</summary>
    public static long InsertSecurityLogHashNoCache(long securityLogNum)
    {
        var securityLog = SecurityLogCrud.SelectOne(securityLogNum);
        var securityLogHash = new SecurityLogHash();
        securityLogHash.SecurityLogNum = securityLog.SecurityLogNum;
        securityLogHash.LogHash = GetHashString(securityLog);
        return InsertNoCache(securityLogHash);
    }

    
    public static void InsertMany(List<SecurityLogHash> listSecurityLogHashes)
    {
        SecurityLogHashCrud.InsertMany(listSecurityLogHashes);
    }

    /// <summary>
    ///     Does not make a call to the db.  Returns a SHA-256 hash of the entire security log.  Length of 32 bytes.  Only
    ///     called from CreateSecurityLogHash() and FormAudit.FillGrid()
    /// </summary>
    public static string GetHashString(SecurityLog securityLog)
    {
        HashAlgorithm hashAlgorithm = SHA256.Create();
        //Build string to hash
        var logString = "";
        //logString+=securityLog.SecurityLogNum;
        logString += ((int) securityLog.PermType).ToString();
        logString += securityLog.UserNum;
        logString += SOut.DateT(securityLog.LogDateTime, false);
        logString += securityLog.LogText;
        //logString+=securityLog.CompName;
        logString += securityLog.PatNum;
        //logString+=securityLog.FKey.ToString();
        if (securityLog.DateTPrevious != DateTime.MinValue) logString += SOut.DateT(securityLog.DateTPrevious, false);
        var byteArrayUnicode = Encoding.Unicode.GetBytes(logString);
        var byteArray = hashAlgorithm.ComputeHash(byteArrayUnicode);
        return Convert.ToBase64String(byteArray);
    }

    #region Delete

    public static void DeleteWithMaxPriKey(long securityLogHashNumMax)
    {
        if (securityLogHashNumMax == 0) return;
        var command = "DELETE FROM securityloghash WHERE SecurityLogHashNum <= " + SOut.Long(securityLogHashNumMax);
        Db.NonQ(command);
    }

    public static void DeleteForSecurityLogEntries(List<long> listSecurityLogNums)
    {
        if (listSecurityLogNums.Count < 1) return;

        var command = $"DELETE FROM securityloghash WHERE SecurityLogNum IN ({string.Join(",", listSecurityLogNums)})";
        Db.NonQ(command);
    }

    #endregion

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<SecurityLogHash> Refresh(long patNum){

        string command="SELECT * FROM securityloghash WHERE PatNum = "+POut.Long(patNum);
        return Crud.SecurityLogHashCrud.SelectMany(command);
    }

    ///<summary>Gets one SecurityLogHash from the db.</summary>
    public static SecurityLogHash GetOne(long securityLogHashNum){

        return Crud.SecurityLogHashCrud.SelectOne(securityLogHashNum);
    }

    
    public static void Update(SecurityLogHash securityLogHash){

        Crud.SecurityLogHashCrud.Update(securityLogHash);
    }

    
    public static void Delete(long securityLogHashNum) {

        string command= "DELETE FROM securityloghash WHERE SecurityLogHashNum = "+POut.Long(securityLogHashNum);
        Db.NonQ(command);
    }
    */
}