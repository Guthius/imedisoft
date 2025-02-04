using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class Icd10Crud
{
    public static Icd10 SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Icd10> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Icd10> TableToList(DataTable table)
    {
        var retVal = new List<Icd10>();
        foreach (DataRow row in table.Rows)
        {
            var icd10 = new Icd10
            {
                Icd10Num = SIn.Long(row["Icd10Num"].ToString()),
                Icd10Code = SIn.String(row["Icd10Code"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                IsCode = SIn.String(row["IsCode"].ToString())
            };
            retVal.Add(icd10);
        }

        return retVal;
    }

    public static void Insert(Icd10 icd10)
    {
        var command = "INSERT INTO icd10 (";

        command += "Icd10Code,Description,IsCode) VALUES(";

        command +=
            "'" + SOut.String(icd10.Icd10Code) + "',"
            + "'" + SOut.String(icd10.Description) + "',"
            + "'" + SOut.String(icd10.IsCode) + "')";
        {
            icd10.Icd10Num = Db.NonQ(command, true, "Icd10Num", "icd10");
        }
    }

    public static void Update(Icd10 icd10)
    {
        var command = "UPDATE icd10 SET "
                      + "Icd10Code  = '" + SOut.String(icd10.Icd10Code) + "', "
                      + "Description= '" + SOut.String(icd10.Description) + "', "
                      + "IsCode     = '" + SOut.String(icd10.IsCode) + "' "
                      + "WHERE Icd10Num = " + SOut.Long(icd10.Icd10Num);
        Db.NonQ(command);
    }
}