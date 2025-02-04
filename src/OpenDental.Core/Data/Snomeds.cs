using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Snomeds
{
    public static List<Snomed> GetByCodeOrDescription(string searchText)
    {
        return SnomedCrud.SelectMany(
            "SELECT * FROM snomed " +
            "WHERE SnomedCode LIKE '%" + SOut.String(searchText) + "%' " +
            "OR Description LIKE '%" + SOut.String(searchText) + "%' " +
            "ORDER BY SnomedCode " +
            "LIMIT 10000");
    }

    public static List<Snomed> GetByCodes(string searchTxt)
    {
        var codes = searchTxt.Split(',');

        var commandText = "SELECT * FROM snomed WHERE SnomedCode IN (";

        for (var i = 0; i < codes.Length; i++)
        {
            if (i > 0)
            {
                commandText += ",";
            }

            commandText += "'" + SOut.String(codes[i]) + "'";
        }

        commandText += ") LIMIT 10000";

        return SnomedCrud.SelectMany(commandText);
    }

    public static void Insert(Snomed snomed)
    {
        SnomedCrud.Insert(snomed);
    }

    public static void Update(Snomed snomed)
    {
        SnomedCrud.Update(snomed);
    }

    public static string GetCodeAndDescription(string snomedCode)
    {
        return DataCore.GetScalar(
            "SELECT CONCAT(CONCAT(SnomedCode, '-'), Description) AS CodeAndDescription " +
            "FROM snomed WHERE SnomedCode = '" + SOut.String(snomedCode) + "'");
    }

    public static Snomed GetByCode(string snomedCode)
    {
        return string.IsNullOrEmpty(snomedCode) ? null : SnomedCrud.SelectOne("SELECT * FROM snomed WHERE SnomedCode='" + SOut.String(snomedCode) + "'");
    }

    public static List<Snomed> GetAll()
    {
        return SnomedCrud.SelectMany("SELECT * FROM snomed");
    }

    public static long GetCodeCount()
    {
        return SIn.Long(Db.GetCount("SELECT COUNT(*) FROM snomed"));
    }
}