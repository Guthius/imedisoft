using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Loincs
{
    public static void Insert(Loinc loinc)
    {
        LoincCrud.Insert(loinc);
    }

    public static void Update(Loinc loinc)
    {
        LoincCrud.Update(loinc);
    }

    public static List<Loinc> GetAll()
    {
        return LoincCrud.SelectMany("SELECT * FROM loinc");
    }

    public static List<Loinc> GetBySearchString(string searchText)
    {
        return LoincCrud.SelectMany(
            "SELECT * FROM loinc " +
            "WHERE LoincCode LIKE '%" + SOut.String(searchText) + "%' OR NameLongCommon LIKE '%" + SOut.String(searchText) + "%' " +
            "ORDER BY RankCommonTests=0, RankCommonTests");
    }

    public static long GetCodeCount()
    {
        return SIn.Long(Db.GetCount("SELECT COUNT(*) FROM loinc"));
    }

    public static Loinc GetByCode(string loincCode)
    {
        var loincs = LoincCrud.SelectMany("SELECT * FROM loinc WHERE LoincCode='" + SOut.String(loincCode) + "'");

        return loincs.Count > 0 ? loincs[0] : null;
    }

    public static List<Loinc> GetForCodeList(string codeList)
    {
        var listCodes = codeList.Split(',').ToList();

        var command = "SELECT * FROM loinc WHERE LoincCode IN(";
        for (var i = 0; i < listCodes.Count; i++)
        {
            if (i > 0)
            {
                command += ",";
            }

            command += "'" + SOut.String(listCodes[i]) + "'";
        }

        command += ") ";

        return LoincCrud.SelectMany(command);
    }
}