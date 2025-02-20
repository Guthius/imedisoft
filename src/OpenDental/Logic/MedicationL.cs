using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Newtonsoft.Json;
using OpenDentBusiness;

namespace OpenDental;

public static class MedicationL
{
    private sealed class MedicationExport
    {
        public readonly string MedName;
        public readonly string GenericName;
        public readonly string Notes;
        public readonly long RxCui;

        public MedicationExport(Medication medication = null)
        {
            if (medication == null)
            {
                return;
            }

            MedName = medication.MedName;
            GenericName = Medications.GetGenericName(medication.GenericNum);
            Notes = medication.Notes;
            RxCui = medication.RxCui;
        }
    }

    public static int ImportMedications(List<Medication> medicationsToImport, List<Medication> existingMedications)
    {
        var count = 0;

        foreach (var medication in medicationsToImport)
        {
            if (IsDuplicate(medication, existingMedications))
            {
                continue;
            }

            InsertNewMedication(medication, existingMedications);

            count++;
        }

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Imported " + count + " medications.");

        return count;
    }

    private static bool IsDuplicate(Medication medication, List<Medication> existingMedications)
    {
        if (existingMedications.All(x => !string.Equals(x.MedName.Trim(), medication.MedName.Trim(), StringComparison.CurrentCultureIgnoreCase)))
        {
            return false;
        }

        if (existingMedications.All(x => !string.Equals(Medications.GetGenericName(x.GenericNum).Trim(), medication.GenericName.Trim(), StringComparison.CurrentCultureIgnoreCase)))
        {
            return false;
        }

        if (existingMedications.All(x => x.RxCui != medication.RxCui))
        {
            return false;
        }

        return string.IsNullOrEmpty(medication.Notes) || existingMedications.Any(x => string.Equals(x.Notes.Trim(), medication.Notes.Trim(), StringComparison.CurrentCultureIgnoreCase));
    }

    private static void InsertNewMedication(Medication medication, List<Medication> existingMedications)
    {
        long genericNum = 0;

        var existingMedication = existingMedications.Find(x => x.MedName == medication.GenericName);
        if (existingMedication is not null)
        {
            genericNum = existingMedication.GenericNum;
        }

        if (genericNum != 0)
        {
            medication.GenericNum = genericNum;
        }

        Medications.Insert(medication);

        if (genericNum == 0)
        {
            medication.GenericNum = medication.MedicationNum;
            Medications.Update(medication);
        }

        existingMedications.Add(medication);
    }

    public static int ExportMedications(string filename, List<Medication> medications)
    {
        var medicationsToExport = new List<MedicationExport>();

        foreach (var medication in medications)
        {
            medicationsToExport.Add(new MedicationExport(medication));
        }

        var json = JsonConvert.SerializeObject(medicationsToExport, Formatting.Indented);

        File.WriteAllText(filename, json);

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Exported " + SOut.Int(medications.Count) + " medications to: " + filename);

        return medications.Count;
    }

    public static List<Medication> GetMedicationsFromFile(string filename, bool isTempFile = false)
    {
        var medications = new List<Medication>();
        if (string.IsNullOrEmpty(filename))
        {
            return medications;
        }

        var medicationData = File.ReadAllText(filename);
        if (isTempFile)
        {
            File.Delete(filename);
        }

        if (string.IsNullOrWhiteSpace(medicationData))
        {
            return medications;
        }

        List<MedicationExport> exportedMedications;
        try
        {
            exportedMedications = JsonConvert.DeserializeObject<List<MedicationExport>>(medicationData);
        }
        catch
        {
            return [];
        }

        foreach (var exportedMedication in exportedMedications)
        {
            medications.Add(new Medication
            {
                MedName = exportedMedication.MedName,
                GenericName = exportedMedication.GenericName,
                Notes = exportedMedication.Notes,
                RxCui = exportedMedication.RxCui
            });
        }

        return SortMedicationsGenericsFirst(medications);
    }

    private static List<Medication> SortMedicationsGenericsFirst(List<Medication> medications)
    {
        var genericMedications = new List<Medication>();
        var brandedMedications = new List<Medication>();

        foreach (var medication in medications)
        {
            if (string.IsNullOrWhiteSpace(medication.GenericName) || medication.MedName == medication.GenericName)
            {
                genericMedications.Add(medication);
                continue;
            }

            brandedMedications.Add(medication);
        }

        genericMedications.AddRange(brandedMedications);

        return genericMedications;
    }
}