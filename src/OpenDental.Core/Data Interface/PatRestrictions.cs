using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class PatRestrictions
{
    ///<summary>Gets all patrestrictions for the specified patient.</summary>
    public static List<PatRestriction> GetPatientData(long patNum)
    {
        var command = "SELECT * FROM patrestriction WHERE PatNum=" + SOut.Long(patNum);
        return PatRestrictionCrud.SelectMany(command);
    }

    ///<summary>Gets all patrestrictions for the specified patient.</summary>
    public static List<PatRestriction> GetAllForPat(long patNum)
    {
        var command = "SELECT * FROM patrestriction WHERE PatNum=" + SOut.Long(patNum);
        return PatRestrictionCrud.SelectMany(command);
    }

    /// <summary>
    ///     This will only insert a new PatRestriction if there is not already an existing PatRestriction in the db for this
    ///     patient and type.
    ///     If exists, returns the PatRestrictionNum of the first one found.  Otherwise returns the PatRestrictionNum of the
    ///     newly inserted one.
    /// </summary>
    public static long Upsert(long patNum, PatRestrict patRestrictType)
    {
        var listPatRestricts = GetAllForPat(patNum).FindAll(x => x.PatRestrictType == patRestrictType);
        if (listPatRestricts.Count > 0) return listPatRestricts[0].PatRestrictionNum;
        return PatRestrictionCrud.Insert(new PatRestriction {PatNum = patNum, PatRestrictType = patRestrictType});
    }

    /// <summary>
    ///     This will only insert a new PatRestriction if there is not already an existing PatRestriction in the db for the
    ///     family member and type.
    ///     Returns the list of PatNums that were restricted.
    /// </summary>
    public static List<long> InsertForFam(Family fam, PatRestrict patRestrictType)
    {
        var listFamPatNums = fam.ListPats.Select(x => x.PatNum).ToList();
        var command = $@"SELECT PatNum FROM patrestriction
				WHERE PatNum IN ({string.Join(",", listFamPatNums.Select(x => SOut.Long(x)))})
				AND PatRestrictType={SOut.Enum(patRestrictType)}";
        var listPatsToSkip = Db.GetListLong(command);
        var listPatNumsToRestrict = listFamPatNums.FindAll(x => !listPatsToSkip.Contains(x));
        for (var i = 0; i < listPatNumsToRestrict.Count; i++) PatRestrictionCrud.Insert(new PatRestriction {PatNum = listPatNumsToRestrict[i], PatRestrictType = patRestrictType});
        return listPatNumsToRestrict;
    }

    /// <summary>
    ///     Checks for an existing patrestriction for the specified patient and PatRestrictType.
    ///     If one exists, returns true (IsRestricted).  If none exist, returns false (!IsRestricted).
    /// </summary>
    public static bool IsRestricted(long patNum, PatRestrict patRestrictType)
    {
        var command = "SELECT COUNT(*) FROM patrestriction WHERE PatNum=" + SOut.Long(patNum) + " AND PatRestrictType=" + SOut.Int((int) patRestrictType);
        if (SIn.Int(Db.GetCount(command)) > 0) return true;

        return false;
    }

    /// <summary>Given a list of PatNums, returns a filtered list of PatNums that are not part of the given RestrictType.</summary>
    public static List<long> GetUnrestrictedPatNumsFromList(List<long> listPatNums, PatRestrict patRestrictType)
    {
        if (listPatNums.IsNullOrEmpty()) return new List<long>();

        var command = "SELECT * FROM patrestriction WHERE PatNum IN(" + string.Join(",", listPatNums) + ") AND PatRestrictType=" + SOut.Int((int) patRestrictType);
        var listRestrictedPatNums = PatRestrictionCrud.SelectMany(command).Select(x => x.PatNum).ToList();
        return listPatNums.Except(listRestrictedPatNums).ToList();
    }

    /// <summary>Get a list of all PatNums that are associated with a given restriction type</summary>
    public static List<long> GetAllRestrictedForType(PatRestrict patRestrictType)
    {
        var command = "SELECT * FROM patrestriction WHERE PatRestrictType=" + SOut.Int((int) patRestrictType);
        return PatRestrictionCrud.SelectMany(command).Select(x => x.PatNum).ToList();
    }

    /// <summary>
    ///     Gets the human readable description of the patrestriction, passed through Lans.g.
    ///     Returns empty string if the enum was not found in the switch statement.
    /// </summary>
    public static string GetPatRestrictDesc(PatRestrict patRestrictType)
    {
        switch (patRestrictType)
        {
            case PatRestrict.ApptSchedule:
                return Lans.g("patRestrictEnum", "Appointment Scheduling");
            case PatRestrict.None:
            default:
                return "";
        }
    }

    ///<summary>Deletes any patrestrictions for the specified patient and type.</summary>
    public static void RemovePatRestriction(long patNum, PatRestrict patRestrictType)
    {
        var command = "DELETE FROM patrestriction WHERE PatNum=" + SOut.Long(patNum) + " AND PatRestrictType=" + SOut.Int((int) patRestrictType);
        Db.NonQ(command);
    }

    ///<summary>Inserts a security log message when a PatRestrict.ApptSchedule value is changed.</summary>
    public static void InsertPatRestrictApptChangeSecurityLog(long patNum, bool isPatRestrictedOld, bool isPatRestrictedNew)
    {
        if (isPatRestrictedOld != isPatRestrictedNew) SecurityLogs.MakeLogEntry(EnumPermType.PatientApptRestrict, patNum, "Patient restriction type changed from " + isPatRestrictedOld + " to " + isPatRestrictedNew);
    }

    //Only pull out the methods below as you need them.  Otherwise, leave them commented out.
    /*
    ///<summary>Gets one PatRestriction from the db.</summary>
    public static PatRestriction GetOne(long patRestrictionNum){

        return Crud.PatRestrictionCrud.SelectOne(patRestrictionNum);
    }

    
    public static void Update(PatRestrict patRestrict){

        Crud.PatRestrictionCrud.Update(patRestrict);
    }

    
    public static void Delete(long patRestrictionNum) {

        Crud.PatRestrictionCrud.Delete(patRestrictionNum);
    }
    */
}