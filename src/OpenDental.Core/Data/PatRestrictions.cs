using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class PatRestrictions
{
    public static List<PatRestriction> GetPatientData(long patNum)
    {
        return PatRestrictionCrud.SelectMany("SELECT * FROM patrestriction WHERE PatNum=" + patNum);
    }

    public static List<PatRestriction> GetAllForPat(long patNum)
    {
        return PatRestrictionCrud.SelectMany("SELECT * FROM patrestriction WHERE PatNum=" + patNum);
    }

    public static void Upsert(long patNum, PatRestrict patRestrictType)
    {
        var patRestricts = GetAllForPat(patNum).FindAll(x => x.PatRestrictType == patRestrictType);
        if (patRestricts.Count > 0)
        {
            return;
        }

        PatRestrictionCrud.Insert(new PatRestriction {PatNum = patNum, PatRestrictType = patRestrictType});
    }

    public static bool IsRestricted(long patNum, PatRestrict patRestrictType)
    {
        return SIn.Int(Db.GetCount("SELECT COUNT(*) FROM patrestriction WHERE PatNum=" + patNum + " AND PatRestrictType=" + (int) patRestrictType)) > 0;
    }

    public static string GetPatRestrictDesc(PatRestrict patRestrictType)
    {
        switch (patRestrictType)
        {
            case PatRestrict.ApptSchedule:
                return "Appointment Scheduling";

            case PatRestrict.None:
            default:
                return "";
        }
    }

    public static void RemovePatRestriction(long patNum, PatRestrict patRestrictType)
    {
        Db.NonQ("DELETE FROM patrestriction WHERE PatNum=" + patNum + " AND PatRestrictType=" + (int) patRestrictType);
    }

    public static void InsertPatRestrictApptChangeSecurityLog(long patNum, bool isPatRestrictedOld, bool isPatRestrictedNew)
    {
        if (isPatRestrictedOld != isPatRestrictedNew)
        {
            SecurityLogs.MakeLogEntry(EnumPermType.PatientApptRestrict, patNum, "Patient restriction type changed from " + isPatRestrictedOld + " to " + isPatRestrictedNew);
        }
    }
}