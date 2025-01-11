using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class PlannedAppts
{
    ///<summary>Gets all planned appt objects for a patient.</summary>
    public static List<PlannedAppt> Refresh(long patNum)
    {
        var command = "SELECT * FROM plannedappt WHERE PatNum=" + SOut.Long(patNum);
        return PlannedApptCrud.SelectMany(command);
    }

    ///<Summary>Gets one plannedAppt from the database.</Summary>
    public static PlannedAppt GetOne(long plannedApptNum)
    {
        return PlannedApptCrud.SelectOne(plannedApptNum);
    }

    ///<summary>Gets one plannedAppt by patient, ordered by ItemOrder</summary>
    public static PlannedAppt GetOneOrderedByItemOrder(long patNum)
    {
        var command = @$"SELECT plannedappt.* FROM plannedappt
				LEFT JOIN appointment ON plannedappt.AptNum=appointment.NextAptNum
				WHERE plannedappt.PatNum={SOut.Long(patNum)}
				AND (appointment.AptStatus IS NULL OR appointment.AptStatus!={SOut.Enum(ApptStatus.Complete)})
				GROUP BY plannedappt.AptNum
				ORDER BY plannedappt.ItemOrder";
        command = DbHelper.LimitOrderBy(command, 1);
        return PlannedApptCrud.SelectOne(command);
    }

    
    public static long Insert(PlannedAppt plannedAppt)
    {
        return PlannedApptCrud.Insert(plannedAppt);
    }

    
    public static void Update(PlannedAppt plannedAppt)
    {
        PlannedApptCrud.Update(plannedAppt);
    }

    
    public static void Update(PlannedAppt plannedAppt, PlannedAppt oldPlannedAppt)
    {
        PlannedApptCrud.Update(plannedAppt, oldPlannedAppt);
    }
}