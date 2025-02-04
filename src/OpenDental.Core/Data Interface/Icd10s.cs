using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Icd10s
{
    public static void Insert(Icd10 icd10)
    {
        Icd10Crud.Insert(icd10);
    }

    public static void Update(Icd10 icd10)
    {
        Icd10Crud.Update(icd10);
    }

    public static List<Icd10> GetAll()
    {
        var command = "SELECT * FROM icd10";
        return Icd10Crud.SelectMany(command);
    }

    public static Icd10 GetByCode(string Icd10Code)
    {
        var command = "SELECT * FROM icd10 WHERE Icd10Code='" + SOut.String(Icd10Code) + "'";
        return Icd10Crud.SelectOne(command);
    }

    public static List<Icd10> GetBySearchText(string searchText)
    {
        var listSearchTokens = searchText.Split(' ').ToList();
        var command = @"SELECT * FROM icd10 ";
        for (var i = 0; i < listSearchTokens.Count; i++)
        {
            if (i == 0)
                command += "WHERE ";
            else
                command += "AND ";
            command += "(Icd10Code LIKE '%" + SOut.String(listSearchTokens[i]) + "%' OR Description LIKE '%" + SOut.String(listSearchTokens[i]) + "%') ";
        }

        return Icd10Crud.SelectMany(command);
    }

    public static string GetCodeAndDescription(string icd10Code)
    {
        if (string.IsNullOrEmpty(icd10Code)) return "";

        var icd10 = GetByCode(icd10Code);
        if (icd10 == null) return "";
        return icd10.Icd10Code + "-" + icd10.Description;
    }
}