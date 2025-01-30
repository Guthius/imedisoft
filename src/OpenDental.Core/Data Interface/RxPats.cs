using System;
using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class RxPats
{
    public static List<RxPat> GetAllForPat(long patNum, RxTypes rxTypes = RxTypes.Rx)
    {
        var command = "SELECT * FROM rxpat WHERE PatNum=" + SOut.Long(patNum) + " AND RxType=" + SOut.Enum(rxTypes);
        return RxPatCrud.SelectMany(command);
    }

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

    public static RxPat GetErxByIdForPat(string erxGuid, long patNum = 0)
    {
        var command = "SELECT * FROM rxpat WHERE ErxGuid='" + SOut.String(erxGuid) + "' AND RxType=" + SOut.Enum(RxTypes.Rx);
        if (patNum != 0) command += " AND PatNum=" + SOut.Long(patNum);
        var listRxPats = RxPatCrud.SelectMany(command);
        if (listRxPats.Count == 0) return null;
        return listRxPats[0];
    }

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

    public static bool UpdateComparison(RxPat rxPatNew, RxPat rxPatOld)
    {
        if (rxPatNew == null && rxPatOld == null) return false;
        if (rxPatNew != null && rxPatOld == null) return true;
        if (rxPatNew == null && rxPatOld != null) return true;
        return RxPatCrud.UpdateComparison(rxPatNew, rxPatOld);
    }
}