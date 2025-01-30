using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Family
{
    public Family()
    {
        ListPats = [];
    }

    public Family(List<Patient> listPats)
    {
        ListPats = listPats.ToArray();
    }

    public Patient[] ListPats;

    public Patient Guarantor
    {
        get { return ListPats.FirstOrDefault(x => x.Guarantor == x.PatNum); }
    }

    public string GetNameInFamLF(long myPatNum)
    {
        foreach (var patient in ListPats)
        {
            if (patient.PatNum == myPatNum)
            {
                return patient.GetNameLF();
            }
        }

        return GetLim(myPatNum).GetNameLF();
    }

    public string GetNameInFamLFI(int myi)
    {
        return Patients.GetNameLF(ListPats[myi].LName, ListPats[myi].FName, ListPats[myi].Preferred, ListPats[myi].MiddleI);
    }

    public string GetNameInFamFL(long myPatNum)
    {
        foreach (var patient in ListPats)
        {
            if (patient.PatNum == myPatNum)
            {
                return patient.GetNameFL();
            }
        }

        return GetLim(myPatNum).GetNameFL();
    }

    public string GetNameInFamFLnoPref(long myPatNum)
    {
        foreach (var patient in ListPats)
        {
            if (patient.PatNum == myPatNum)
            {
                return patient.GetNameFLnoPref();
            }
        }

        return GetLim(myPatNum).GetNameFLnoPref();
    }

    public string GetNameInFamFLI(int myi)
    {
        var retStr = "";
        if (ListPats[myi].Preferred != "")
        {
            retStr = "'" + ListPats[myi].Preferred + "' ";
        }

        retStr += Patients.GetNameFLnoPref(ListPats[myi].LName, ListPats[myi].FName, ListPats[myi].MiddleI);
        return retStr;
    }

    public string GetNameInFamFirst(long myPatNum)
    {
        foreach (var patient in ListPats)
        {
            if (patient.PatNum == myPatNum)
            {
                return patient.GetNameFirst();
            }
        }

        return GetLim(myPatNum).GetNameFirst();
    }

    public string GetNameInFamFirstOrPreferredOrLast(long myPatNum)
    {
        foreach (var patient in ListPats)
        {
            if (patient.PatNum == myPatNum)
            {
                return patient.GetNameFirstOrPreferredOrLast();
            }
        }

        return GetLim(myPatNum).GetNameFirstOrPreferredOrLast();
    }

    public List<long> GetPatNums()
    {
        return ListPats.IsNullOrEmpty() ? [] : ListPats.Select(x => x.PatNum).Distinct().ToList();
    }

    public int GetIndex(long patNum)
    {
        for (var i = 0; i < ListPats.Length; i++)
        {
            if (ListPats[i].PatNum == patNum)
            {
                return i;
            }
        }

        return -1;
    }

    public Patient GetPatient(long patNum)
    {
        return ListPats.Where(patient => patient.PatNum == patNum).Select(patient => patient.Copy()).FirstOrDefault();
    }

    public static Patient GetLim(long patNum)
    {
        if (patNum == 0)
        {
            return new Patient();
        }

        var commandText = "SELECT PatNum,LName,FName,MiddleI,Preferred,CreditType,Guarantor,HasIns,SSN FROM patient WHERE PatNum = '" + patNum + "'";
        
        var dataTable = DataCore.GetTable(commandText);
        if (dataTable.Rows.Count == 0)
        {
            return new Patient();
        }

        return new Patient
        {
            PatNum = SIn.Long(dataTable.Rows[0][0].ToString()),
            LName = SIn.String(dataTable.Rows[0][1].ToString()),
            FName = SIn.String(dataTable.Rows[0][2].ToString()),
            MiddleI = SIn.String(dataTable.Rows[0][3].ToString()),
            Preferred = SIn.String(dataTable.Rows[0][4].ToString()),
            CreditType = SIn.String(dataTable.Rows[0][5].ToString()),
            Guarantor = SIn.Long(dataTable.Rows[0][6].ToString()),
            HasIns = SIn.String(dataTable.Rows[0][7].ToString()),
            SSN = SIn.String(dataTable.Rows[0][8].ToString())
        };
    }

    public bool IsInFamily(long patNum)
    {
        return ListPats.Any(x => x.PatNum == patNum);
    }

    public bool HasArchivedMember()
    {
        return ListPats.Any(x => x.PatStatus == PatientStatus.Archived);
    }

    public static string ReplaceFamily(string message, Patient pat)
    {
        if (pat == null)
        {
            return message;
        }

        var fam = Patients.GetFamily(pat.PatNum);
        
        return fam == null ? message : message.Replace("[FamilyList]", string.Join(",", fam.ListPats.Select(x => Patients.GetNameFirstOrPrefML(x.LName, x.FName, x.Preferred, x.MiddleI))));
    }
}