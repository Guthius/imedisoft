using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class InsFilingCodeSubtypeCrud
{
    public static List<InsFilingCodeSubtype> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InsFilingCodeSubtype> TableToList(DataTable table)
    {
        var retVal = new List<InsFilingCodeSubtype>();
        foreach (DataRow row in table.Rows)
        {
            var insFilingCodeSubtype = new InsFilingCodeSubtype
            {
                InsFilingCodeSubtypeNum = SIn.Long(row["InsFilingCodeSubtypeNum"].ToString()),
                InsFilingCodeNum = SIn.Long(row["InsFilingCodeNum"].ToString()),
                Descript = SIn.String(row["Descript"].ToString())
            };
            retVal.Add(insFilingCodeSubtype);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<InsFilingCodeSubtype> listInsFilingCodeSubtypes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "InsFilingCodeSubtype";
        var table = new DataTable(tableName);
        table.Columns.Add("InsFilingCodeSubtypeNum");
        table.Columns.Add("InsFilingCodeNum");
        table.Columns.Add("Descript");
        foreach (var insFilingCodeSubtype in listInsFilingCodeSubtypes)
            table.Rows.Add(SOut.Long(insFilingCodeSubtype.InsFilingCodeSubtypeNum), SOut.Long(insFilingCodeSubtype.InsFilingCodeNum), insFilingCodeSubtype.Descript);
        return table;
    }

    public static void Insert(InsFilingCodeSubtype insFilingCodeSubtype)
    {
        var command = "INSERT INTO insfilingcodesubtype (";

        command += "InsFilingCodeNum,Descript) VALUES(";

        command +=
            SOut.Long(insFilingCodeSubtype.InsFilingCodeNum) + ","
                                                             + "'" + SOut.String(insFilingCodeSubtype.Descript) + "')";
        {
            insFilingCodeSubtype.InsFilingCodeSubtypeNum = Db.NonQ(command, true, "InsFilingCodeSubtypeNum", "insFilingCodeSubtype");
        }
    }

    public static void Update(InsFilingCodeSubtype insFilingCodeSubtype)
    {
        var command = "UPDATE insfilingcodesubtype SET "
                      + "InsFilingCodeNum       =  " + SOut.Long(insFilingCodeSubtype.InsFilingCodeNum) + ", "
                      + "Descript               = '" + SOut.String(insFilingCodeSubtype.Descript) + "' "
                      + "WHERE InsFilingCodeSubtypeNum = " + SOut.Long(insFilingCodeSubtype.InsFilingCodeSubtypeNum);
        Db.NonQ(command);
    }

    public static void Delete(long insFilingCodeSubtypeNum)
    {
        var command = "DELETE FROM insfilingcodesubtype "
                      + "WHERE InsFilingCodeSubtypeNum = " + SOut.Long(insFilingCodeSubtypeNum);
        Db.NonQ(command);
    }
}