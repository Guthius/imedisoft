using System.Collections.Generic;
using System.Linq;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class FeeSchedNotes
{
    public static void Insert(FeeSchedNote feeSchedNote)
    {
        feeSchedNote.SecUserNumEntry = Security.CurUser.UserNum;
        FeeSchedNoteCrud.Insert(feeSchedNote);
    }

    public static void Update(FeeSchedNote feeSchedNote)
    {
        FeeSchedNoteCrud.Update(feeSchedNote);
    }

    public static void Delete(long feeSchedNoteNum)
    {
        FeeSchedNoteCrud.Delete(feeSchedNoteNum);
    }

    public static List<FeeSchedNote> GetNotesForGlobal(long feeSchedNum)
    {
        var dataTable = DataCore.GetTable("Select * from feeschednote where feeSchedNum = " + feeSchedNum);

        return FeeSchedNoteCrud.TableToList(dataTable);
    }

    public static string ConvertClinicNumsToString(List<long> clinicNums)
    {
        return string.Join(",", clinicNums);
    }

    public static List<long> ConvertStringToClinicsNums(string s)
    {
        var clinicNumStrs = s.Split(',').ToList();
        var clinicNums = new List<long>();

        foreach (var str in clinicNumStrs)
        {
            if (string.IsNullOrEmpty(str))
            {
                break;
            }

            clinicNums.Add(long.Parse(str));
        }

        return clinicNums;
    }

    public static List<FeeSchedNote> ConvertListStringsToClinicNums(List<FeeSchedNote> feeSchedNotes)
    {
        foreach (var feeSchedNote in feeSchedNotes)
        {
            feeSchedNote.ListClinicNums = ConvertStringToClinicsNums(feeSchedNote.ClinicNums);
        }

        return feeSchedNotes;
    }
}