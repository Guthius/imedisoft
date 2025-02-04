using System;
using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class MedicationPats
{
    public static List<MedicationPat> Refresh(long patNum, bool includeDiscontinued)
    {
        var commandText = "SELECT * FROM medicationpat WHERE PatNum = " + patNum;
        
        if (!includeDiscontinued)
        {
            commandText += " AND (DateStop < " + SOut.Date(new DateTime(1880, 1, 1)) + " OR DateStop >= CURDATE())";
        }

        return MedicationPatCrud.SelectMany(commandText);
    }

    public static List<MedicationPat> GetPatientData(long patNum)
    {
        return MedicationPatCrud.SelectMany("SELECT * FROM medicationpat WHERE PatNum = " + patNum);
    }

    public static void Update(MedicationPat medicationPat)
    {
        MedicationPatCrud.Update(medicationPat);
    }

    public static void Insert(MedicationPat medicationPat)
    {
        MedicationPatCrud.Insert(medicationPat);
    }

    public static void UpdateRxCuiForMedication(long medicationNum, long rxCui)
    {
        Db.NonQ("UPDATE medicationpat SET RxCui = " + rxCui + " WHERE MedicationNum = " + medicationNum);
    }

    public static bool IsMedActive(MedicationPat medicationPat)
    {
        return medicationPat.DateStop.Year < 1880 || medicationPat.DateStop >= DateTime.Today;
    }

    public static List<MedicationPat> GetForRxCuis(List<string> rxCuis)
    {
        if (rxCuis is null || rxCuis.Count == 0)
        {
            return [];
        }

        return MedicationPatCrud.SelectMany("SELECT * FROM medicationpat WHERE RxCui IN (" + string.Join(", ", rxCuis) + ")");
    }

    public static List<MedicationPat> GetAllMissingMedications()
    {
        return MedicationPatCrud.SelectMany("SELECT * FROM medicationpat WHERE MedicationNum = 0");
    }

    public static void UpdateMedicationNumForMany(long medicationNum, List<long> medicationPatNums)
    {
        if (medicationPatNums is null || medicationPatNums.Count < 1)
        {
            return;
        }

        Db.NonQ("UPDATE medicationpat SET MedicationNum = " + medicationNum + " WHERE MedicationPatNum IN (" + string.Join(", ", medicationPatNums) + ")");
    }
}