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
        return Icd10Crud.SelectMany("SELECT * FROM icd10");
    }

    public static Icd10 GetByCode(string icd10Code)
    {
        return Icd10Crud.SelectOne("SELECT * FROM icd10 WHERE Icd10Code = '" + SOut.String(icd10Code) + "'");
    }

    public static List<Icd10> GetBySearchText(string searchText)
    {
        var tokens = searchText.Split(' ').ToList();

        var commandText = @"SELECT * FROM icd10 ";
        for (var i = 0; i < tokens.Count; i++)
        {
            if (i == 0)
            {
                commandText += "WHERE ";
            }
            else
            {
                commandText += "AND ";
            }

            commandText += "(Icd10Code LIKE '%" + SOut.String(tokens[i]) + "%' OR Description LIKE '%" + SOut.String(tokens[i]) + "%') ";
        }

        return Icd10Crud.SelectMany(commandText);
    }

    public static string GetCodeAndDescription(string icd10Code)
    {
        if (string.IsNullOrEmpty(icd10Code))
        {
            return string.Empty;
        }

        var icd10 = GetByCode(icd10Code);
        if (icd10 == null)
        {
            return string.Empty;
        }

        return icd10.Icd10Code + "-" + icd10.Description;
    }
}