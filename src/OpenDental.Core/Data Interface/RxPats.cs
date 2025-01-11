using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class RxPats
{
    ///<summary>Returns a list of RxPats containing the passed in PatNum.</summary>
    public static List<RxPat> GetAllForPat(long patNum, RxTypes rxTypes = RxTypes.Rx)
    {
        var command = "SELECT * FROM rxpat WHERE PatNum=" + SOut.Long(patNum) + " AND RxType=" + SOut.Enum(rxTypes);
        return RxPatCrud.SelectMany(command);
    }

    ///<summary>Used in Ehr.  Excludes controlled substances.</summary>
    public static List<RxPat> GetPermissableForDateRange(long patNum, DateTime dateStart, DateTime dateStop)
    {
        var command = "SELECT * FROM rxpat WHERE PatNum=" + SOut.Long(patNum) + " "
                      + "AND RxDate >= " + SOut.Date(dateStart) + " "
                      + "AND RxDate <= " + SOut.Date(dateStop) + " "
                      + "AND IsControlled = 0 "
                      + "AND RxType=" + SOut.Enum(RxTypes.Rx);
        return RxPatCrud.SelectMany(command);
    }

    
    public static RxPat GetRx(long rxNum)
    {
        return RxPatCrud.SelectOne(rxNum);
    }

    
    public static void Update(RxPat rxPat)
    {
        RxPatCrud.Update(rxPat);
    }

    public static bool Update(RxPat rxPat, RxPat rxPatOld)
    {
        return RxPatCrud.Update(rxPat, rxPatOld);
    }

    
    public static long Insert(RxPat rxPat)
    {
        return RxPatCrud.Insert(rxPat);
    }

    
    public static void Delete(long rxNum)
    {
        RxPatCrud.Delete(rxNum);
    }

    public static List<long> GetChangedSinceRxNums(DateTime dateTChangedSince)
    {
        var command = "SELECT RxNum FROM rxpat WHERE DateTStamp > " + SOut.DateT(dateTChangedSince) + " AND RxType=" + SOut.Enum(RxTypes.Rx);
        var tableRxNums = DataCore.GetTable(command);
        var listRxNums = new List<long>(tableRxNums.Rows.Count);
        for (var i = 0; i < tableRxNums.Rows.Count; i++) listRxNums.Add(SIn.Long(tableRxNums.Rows[i]["RxNum"].ToString()));
        return listRxNums;
    }

    ///<summary>Used along with GetChangedSinceRxNums</summary>
    public static List<RxPat> GetMultRxPats(List<long> listRxNums)
    {
        if (listRxNums.Count == 0) return new List<RxPat>();
        var command = "SELECT * FROM rxpat WHERE ";
        for (var i = 0; i < listRxNums.Count; i++)
        {
            if (i > 0) command += "OR ";
            command += "RxNum='" + listRxNums[i] + "' ";
        }

        return RxPatCrud.SelectMany(command);
    }

    ///<summary>Used in FormRxSend to fill electronic queue.</summary>
    public static List<RxPat> GetQueue()
    {
        var command = "SELECT * FROM rxpat WHERE SendStatus=1 AND RxType=" + SOut.Enum(RxTypes.Rx);
        return RxPatCrud.SelectMany(command);
    }

    
    public static RxPat GetErxByIdForPat(string erxGuid, long patNum = 0)
    {
        var command = "SELECT * FROM rxpat WHERE ErxGuid='" + SOut.String(erxGuid) + "' AND RxType=" + SOut.Enum(RxTypes.Rx);
        if (patNum != 0) command += " AND PatNum=" + SOut.Long(patNum);
        var listRxPats = RxPatCrud.SelectMany(command);
        if (listRxPats.Count == 0) return null;
        return listRxPats[0];
    }

    ///<summary>Gets a list of rxpats optionally filtered for the API. Returns an empty list if not found.</summary>
    public static List<RxPat> GetRxPatsForApi(int limit, int offset, long patNum)
    {
        var command = "SELECT * FROM rxpat WHERE DateTStamp >= " + SOut.DateT(DateTime.MinValue) + " ";
        if (patNum > 0) command += "AND PatNum=" + SOut.Long(patNum) + " ";
        command += "ORDER BY RxNum " //Ensure order for limit and offset.
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return RxPatCrud.SelectMany(command);
    }

    /// <summary>
    ///     Zeros securitylog FKey column for rows that are using the matching rxNum as FKey and are related to RxPat.
    ///     Permtypes are generated from the AuditPerms property of the CrudTableAttribute within the RxPat table type.
    /// </summary>
    public static void ClearFkey(long rxNum)
    {
        RxPatCrud.ClearFkey(rxNum);
    }

    /// <summary>
    ///     Zeros securitylog FKey column for rows that are using the matching rxNums as FKey and are related to RxPat.
    ///     Permtypes are generated from the AuditPerms property of the CrudTableAttribute within the RxPat table type.
    /// </summary>
    public static void ClearFkey(List<long> listRxNums)
    {
        RxPatCrud.ClearFkey(listRxNums);
    }

    ///<summary>Creates an RxPat when pdmp bridge is used and inserts it into RxPat</summary>
    public static void CreatePdmpAccessLog(Patient patient, Userod userod, Program program)
    {
        var rxPat = new RxPat();
        rxPat.PatNum = patient.PatNum;
        rxPat.UserNum = userod.UserNum;
        rxPat.ProvNum = userod.ProvNum;
        rxPat.ClinicNum = 0;
        if (true) rxPat.ClinicNum = patient.ClinicNum;
        rxPat.RxDate = DateTime.Today;
        if (program.ProgName == ProgramName.PDMP.ToString())
            rxPat.RxType = RxTypes.LogicoyAccess;
        else
            rxPat.RxType = RxTypes.BambooAccess;
        Insert(rxPat);
    }

    ///<summary>Returns true if RxPatNew differs from RxPatOld</summary>
    public static bool UpdateComparison(RxPat rxPatNew, RxPat rxPatOld)
    {
        if (rxPatNew == null && rxPatOld == null) return false;
        if (rxPatNew != null && rxPatOld == null) return true;
        if (rxPatNew == null && rxPatOld != null) return true;
        return RxPatCrud.UpdateComparison(rxPatNew, rxPatOld);
    }
}