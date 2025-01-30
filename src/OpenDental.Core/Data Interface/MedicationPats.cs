using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class MedicationPats
{
    public static List<MedicationPat> Refresh(long patNum, bool includeDiscontinued)
    {
        var command = "SELECT * FROM medicationpat WHERE PatNum = " + SOut.Long(patNum);
        if (includeDiscontinued)
        {
            //this only happens when user checks box to show discontinued or for MU.
            //no restriction on DateStop
        }
        else
        {
            //exclude discontinued.  This is the default.
            command += " AND (DateStop < " + SOut.Date(new DateTime(1880, 1, 1)) //include all the meds that are not discontinued.
                                           + " OR DateStop >= ";
            command += "CURDATE()" + ")"; //Show medications that are today or a future stopdate - they are not yet discontinued.
        }

        return MedicationPatCrud.SelectMany(command);
    }

    public static List<MedicationPat> GetPatientData(long patNum)
    {
        var command = "SELECT * FROM medicationpat WHERE PatNum = " + SOut.Long(patNum);
        return MedicationPatCrud.SelectMany(command);
    }

    public static List<MedicationPat> GetMedPatsForReconcile(long patNum)
    {
        var command = "SELECT * FROM medicationpat WHERE PatNum = " + SOut.Long(patNum)
                                                                    + " AND (DateStop < " + SOut.Date(new DateTime(1880, 1, 1)) //include all the meds that are not discontinued.
                                                                    + " OR DateStop > CURDATE())"; //Show medications that are a future stopdate.
        return MedicationPatCrud.SelectMany(command);
    }

    public static MedicationPat GetOne(long medicationPatNum)
    {
        var command = "SELECT * FROM medicationpat WHERE MedicationPatNum = " + SOut.Long(medicationPatNum);
        return MedicationPatCrud.SelectOne(command);
    }

    public static void Update(MedicationPat medicationPat)
    {
        MedicationPatCrud.Update(medicationPat);
    }

    public static long Insert(MedicationPat medicationPat)
    {
        return MedicationPatCrud.Insert(medicationPat);
    }

    public static void Delete(MedicationPat medicationPat)
    {
        var command = "DELETE from medicationpat WHERE medicationpatNum = '"
                      + medicationPat.MedicationPatNum + "'";
        Db.NonQ(command);
    }

    public static void ResetTimeStamps(long patNum, bool onlyActive)
    {
        var command = "UPDATE medicationpat SET DateTStamp = CURRENT_TIMESTAMP WHERE PatNum = " + SOut.Long(patNum);
        if (onlyActive) command += " AND (DateStop > 1880 OR DateStop <= CURDATE())";
        Db.NonQ(command);
    }

    public static MedicationPat GetMedicationOrderByErxIdAndPat(string erxGuid, long patNum = 0)
    {
        var command = "SELECT * FROM medicationpat WHERE ErxGuid='" + SOut.String(erxGuid) + "'";
        if (patNum != 0) command += " AND PatNum=" + SOut.Long(patNum);
        var listMedicationPats = MedicationPatCrud.SelectMany(command);
        if (listMedicationPats.Count == 0) return null;
        return listMedicationPats[0];
    }

    public static void UpdateRxCuiForMedication(long medicationNum, long rxCui)
    {
        var command = "UPDATE medicationpat SET RxCui=" + SOut.Long(rxCui) + " WHERE MedicationNum=" + SOut.Long(medicationNum);
        Db.NonQ(command);
    }

    public static bool IsMedActive(MedicationPat medicationPat)
    {
        if (medicationPat.DateStop.Year < 1880 || medicationPat.DateStop >= DateTime.Today) return true;
        return false;
    }

    public static List<string> GetAllForRxCuis(List<string> listRxCuis)
    {
        if (listRxCuis == null || listRxCuis.Count == 0) return new List<string>();

        var command = "SELECT RxCui FROM medicationpat WHERE RxCui IN(" + string.Join(",", listRxCuis) + ") "
                      + "AND DATE(DateStart)>=" + SOut.Date(MiscData.GetNowDateTime().AddYears(-1)) + " "
                      + "GROUP BY RxCui";
        return Db.GetListString(command);
    }

    public static List<MedicationPat> GetForRxCuis(List<string> listRxCuis)
    {
        if (listRxCuis == null || listRxCuis.Count == 0) return new List<MedicationPat>();

        var command = "SELECT * FROM medicationpat WHERE RxCui IN(" + string.Join(",", listRxCuis) + ")";
        return MedicationPatCrud.SelectMany(command);
    }

    public static List<MedicationPat> GetAllMissingMedications()
    {
        var command = "SELECT * FROM medicationpat WHERE MedicationNum=0";
        return MedicationPatCrud.SelectMany(command);
    }

    public static void UpdateMedicationNumForMany(long medicationNum, List<long> listMedicationPatNums)
    {
        if (listMedicationPatNums == null || listMedicationPatNums.Count < 1) return;

        var command = "UPDATE medicationpat SET MedicationNum=" + SOut.Long(medicationNum) + " "
                      + "WHERE MedicationPatNum IN (" + string.Join(",", listMedicationPatNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}