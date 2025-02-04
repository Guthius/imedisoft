using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class PatientLinks
{
    public static void Insert(PatientLink patientLink)
    {
        PatientLinkCrud.Insert(patientLink);
    }

    public static void UpdateFromPatientClonesAfterMerge(long patNumFromOriginal, long patNumFromNew)
    {
        var command = "UPDATE patientlink SET PatNumFrom=" + (patNumFromNew) + " "
                      + "WHERE PatNumFrom=" + (patNumFromOriginal) + " "
                      + "AND LinkType=" + SOut.Int((int) PatientLinkType.Clone);
        Db.NonQ(command);
    }

    public static List<Patient> GetPatientsLinked(long patNum)
    {
        var listPatNums = new List<long>();
        //We must find the master Patient's PatNum in order to show the complete list of clones on each clone account.
        //The master Patient's PatNum is the only one that can pull all clones at once.
        var command = "SELECT PatNumFrom FROM patientlink "
                      + "WHERE PatNumTo=" + (patNum) + " "
                      + "AND LinkType=" + SOut.Int((int) PatientLinkType.Clone);
        var patNumMasterPatient = Db.GetLong(command);
        //If Patient doesn't have a PatNumFrom, then it is the master.
        if (patNumMasterPatient == 0) patNumMasterPatient = patNum;
        listPatNums.Add(patNumMasterPatient);
        command = "SELECT PatNumTo FROM patientlink "
                  + "WHERE PatNumFrom=" + (patNumMasterPatient) + " "
                  + "AND LinkType=" + SOut.Int((int) PatientLinkType.Clone);
        listPatNums.AddRange(Db.GetListLong(command));
        var patientArray = Patients.GetMultPats(listPatNums);
        var listPatients = patientArray.ToList();
        return listPatients;
    }

    public static List<long> GetPatNumsLinkedFrom(long patNumFrom, PatientLinkType patLinkType)
    {
        var command = "SELECT PatNumTo FROM patientlink "
                      + "WHERE PatNumFrom=" + (patNumFrom) + " "
                      + "AND LinkType=" + SOut.Int((int) patLinkType);
        return Db.GetListLong(command);
    }

    public static List<long> GetPatNumsLinkedTo(long patNumTo, PatientLinkType patLinkType)
    {
        var command = "SELECT PatNumFrom FROM patientlink "
                      + "WHERE PatNumTo=" + (patNumTo) + " "
                      + "AND LinkType=" + SOut.Int((int) patLinkType);
        return Db.GetListLong(command);
    }

    public static List<PatientLink> GetLinks(long patNum, PatientLinkType patLinkType)
    {
        return GetLinks([patNum], patLinkType);
    }

    public static List<PatientLink> GetLinks(List<long> listPatNums, PatientLinkType patLinkType)
    {
        if (listPatNums.Count == 0) return [];

        var command = "SELECT * FROM patientlink "
                      + "WHERE (PatNumTo IN(" + string.Join(",", listPatNums.Select(x => (x))) + ") "
                      + "OR PatNumFrom IN(" + string.Join(",", listPatNums.Select(x => (x))) + ")) "
                      + "AND LinkType=" + SOut.Int((int) patLinkType);
        return PatientLinkCrud.SelectMany(command);
    }

    public static List<long> GetPatNumsLinkedFromRecursive(long patNumFrom, PatientLinkType patLinkType)
    {
        var listPatNums = new List<long> {patNumFrom};
        AddPatNumsLinkedFromRecursive(patNumFrom, patLinkType, listPatNums);
        return listPatNums;
    }

    private static void AddPatNumsLinkedFromRecursive(long patNumFrom, PatientLinkType patLinkType, List<long> listPatNums)
    {
        var command = "SELECT PatNumTo FROM patientlink "
                      + "WHERE PatNumFrom=" + (patNumFrom) + " "
                      + "AND LinkType=" + SOut.Int((int) patLinkType);
        var listPatNumTos = Db.GetListLong(command);
        if (listPatNumTos.Count == 0) return; //Base case
        foreach (var patNumTo in listPatNumTos)
        {
            if (listPatNums.Contains(patNumTo)) continue; //So that a patient that links to itself does not cause an infinite circle of recursion.
            listPatNums.Add(patNumTo);
            AddPatNumsLinkedFromRecursive(patNumTo, patLinkType, listPatNums); //Find all the patients that are linked to the "To" patient.
        }
    }

    public static List<long> GetPatNumsLinkedToRecursive(long patNumTo, PatientLinkType patLinkType)
    {
        var listPatNums = new List<long> {patNumTo};
        AddPatNumsLinkedToRecursive(patNumTo, patLinkType, listPatNums);
        return listPatNums;
    }

    private static void AddPatNumsLinkedToRecursive(long patNumTo, PatientLinkType patLinkType, List<long> listPatNums)
    {
        var command = "SELECT PatNumFrom FROM patientlink "
                      + "WHERE PatNumTo=" + (patNumTo) + " "
                      + "AND LinkType=" + SOut.Int((int) patLinkType);
        var listPatNumFroms = Db.GetListLong(command);
        if (listPatNumFroms.Count == 0) return; //Base case
        foreach (var patNumFrom in listPatNumFroms)
        {
            if (listPatNums.Contains(patNumFrom)) continue; //So that a patient that links to itself does not cause an infinite circle of recursion.
            listPatNums.Add(patNumFrom);
            AddPatNumsLinkedToRecursive(patNumFrom, patLinkType, listPatNums); //Find all the patients that are linked to the from patient.
        }
    }

    public static void DeletePatNumFroms(long patNumFrom, PatientLinkType patLinkType)
    {
        var command = "DELETE FROM patientlink "
                      + "WHERE PatNumFrom=" + (patNumFrom) + " "
                      + "AND LinkType=" + SOut.Int((int) patLinkType);
        Db.NonQ(command);
    }

    public static void DeletePatNumTos(long patNumTo, PatientLinkType patLinkType)
    {
        var command = "DELETE FROM patientlink "
                      + "WHERE PatNumTo=" + (patNumTo) + " "
                      + "AND LinkType=" + SOut.Int((int) patLinkType);
        Db.NonQ(command);
    }

    public static void DeleteCloneBetweenToAndFrom(long patNumTo, long patNumFrom)
    {
        var command = "DELETE FROM patientlink WHERE ((PatNumTo=" + (patNumTo) + " AND PatNumFrom=" + (patNumFrom) + ") " +
                      "OR (PatNumTo=" + (patNumFrom) + " AND PatNumFrom=" + (patNumTo) + ")) AND LinkType=" + SOut.Int((int) PatientLinkType.Clone);
        Db.NonQ(command);
    }

    public static long GetOriginalPatNumFromClone(long patNum)
    {
        if (!IsPatientAClone(patNum)) return patNum; //Not a clone so this patient must be the original.
        long patNumOriginal = 0;
        var listMatchingClonePatNums = GetPatNumsLinkedTo(patNum, PatientLinkType.Clone);
        if (listMatchingClonePatNums != null && listMatchingClonePatNums.Count > 0) patNumOriginal = listMatchingClonePatNums[0];
        return patNumOriginal;
    }

    public static bool IsPatientAClone(long patNum)
    {
        var listMatchingClonePatNums = GetPatNumsLinkedTo(patNum, PatientLinkType.Clone);
        return listMatchingClonePatNums.Count > 0;
    }

    public static bool IsPatientACloneOrOriginal(long patNum)
    {
        var listMatchingClonePatNums = GetPatNumsLinkedTo(patNum, PatientLinkType.Clone);
        var listMatchingMasterPatNums = GetPatNumsLinkedFrom(patNum, PatientLinkType.Clone);
        return listMatchingClonePatNums.Count > 0 || listMatchingMasterPatNums.Count > 0;
    }

    public static bool ArePatientsClonesOfEachOther(long patNum1, long patNum2)
    {
        if (patNum1 == patNum2) //A patient is not considered a clone of themselves.  Even if the database has this scenario we do not honor it.
            return false;
        //First check if patNum1 is a master of patNum2
        var listPatNums = GetPatNumsLinkedFrom(patNum1, PatientLinkType.Clone);
        if (listPatNums.Contains(patNum2)) return true;
        //Then check if patNum2 is a master of patNum1
        listPatNums = GetPatNumsLinkedFrom(patNum2, PatientLinkType.Clone);
        if (listPatNums.Contains(patNum1)) return true;
        //Finally, check to see if both patients passed in are clones and if they are, check if they share the same master.
        if (IsPatientAClone(patNum1) && IsPatientAClone(patNum2))
        {
            var patNum1Master = GetOriginalPatNumFromClone(patNum1);
            var patNum2Master = GetOriginalPatNumFromClone(patNum2);
            return patNum1Master == patNum2Master;
        }

        return false; //The two patients are not clones and / or are not clones of eachother.
    }

    public static bool WasPatientMerged(long patNum, List<PatientLink> listMergeLinks = null)
    {
        listMergeLinks = listMergeLinks ?? GetLinks(patNum, PatientLinkType.Merge);
        var mergeLink = listMergeLinks.OrderBy(x => x.DateTimeLink).LastOrDefault(x => x.PatNumFrom == patNum);
        if (mergeLink == null) return false; //This patient has never been merged into another patient.
        if (listMergeLinks.Any(x => x.PatNumTo == patNum && x.DateTimeLink > mergeLink.DateTimeLink)) return false; //After our patient was merged into another patient, a different patient was merged into our patient. Our patient is no longer
        //considered a "merged" patient.
        return true; //This patient has been merged into another patient.
    }
}