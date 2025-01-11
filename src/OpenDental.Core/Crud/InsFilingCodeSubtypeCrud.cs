#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class InsFilingCodeSubtypeCrud
{
    public static InsFilingCodeSubtype SelectOne(long insFilingCodeSubtypeNum)
    {
        var command = "SELECT * FROM insfilingcodesubtype "
                      + "WHERE InsFilingCodeSubtypeNum = " + SOut.Long(insFilingCodeSubtypeNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static InsFilingCodeSubtype SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<InsFilingCodeSubtype> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InsFilingCodeSubtype> TableToList(DataTable table)
    {
        var retVal = new List<InsFilingCodeSubtype>();
        InsFilingCodeSubtype insFilingCodeSubtype;
        foreach (DataRow row in table.Rows)
        {
            insFilingCodeSubtype = new InsFilingCodeSubtype();
            insFilingCodeSubtype.InsFilingCodeSubtypeNum = SIn.Long(row["InsFilingCodeSubtypeNum"].ToString());
            insFilingCodeSubtype.InsFilingCodeNum = SIn.Long(row["InsFilingCodeNum"].ToString());
            insFilingCodeSubtype.Descript = SIn.String(row["Descript"].ToString());
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

    public static long Insert(InsFilingCodeSubtype insFilingCodeSubtype)
    {
        return Insert(insFilingCodeSubtype, false);
    }

    public static long Insert(InsFilingCodeSubtype insFilingCodeSubtype, bool useExistingPK)
    {
        var command = "INSERT INTO insfilingcodesubtype (";

        command += "InsFilingCodeNum,Descript) VALUES(";

        command +=
            SOut.Long(insFilingCodeSubtype.InsFilingCodeNum) + ","
                                                             + "'" + SOut.String(insFilingCodeSubtype.Descript) + "')";
        {
            insFilingCodeSubtype.InsFilingCodeSubtypeNum = Db.NonQ(command, true, "InsFilingCodeSubtypeNum", "insFilingCodeSubtype");
        }
        return insFilingCodeSubtype.InsFilingCodeSubtypeNum;
    }

    public static long InsertNoCache(InsFilingCodeSubtype insFilingCodeSubtype)
    {
        return InsertNoCache(insFilingCodeSubtype, false);
    }

    public static long InsertNoCache(InsFilingCodeSubtype insFilingCodeSubtype, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO insfilingcodesubtype (";
        if (isRandomKeys || useExistingPK) command += "InsFilingCodeSubtypeNum,";
        command += "InsFilingCodeNum,Descript) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(insFilingCodeSubtype.InsFilingCodeSubtypeNum) + ",";
        command +=
            SOut.Long(insFilingCodeSubtype.InsFilingCodeNum) + ","
                                                             + "'" + SOut.String(insFilingCodeSubtype.Descript) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            insFilingCodeSubtype.InsFilingCodeSubtypeNum = Db.NonQ(command, true, "InsFilingCodeSubtypeNum", "insFilingCodeSubtype");
        return insFilingCodeSubtype.InsFilingCodeSubtypeNum;
    }

    public static void Update(InsFilingCodeSubtype insFilingCodeSubtype)
    {
        var command = "UPDATE insfilingcodesubtype SET "
                      + "InsFilingCodeNum       =  " + SOut.Long(insFilingCodeSubtype.InsFilingCodeNum) + ", "
                      + "Descript               = '" + SOut.String(insFilingCodeSubtype.Descript) + "' "
                      + "WHERE InsFilingCodeSubtypeNum = " + SOut.Long(insFilingCodeSubtype.InsFilingCodeSubtypeNum);
        Db.NonQ(command);
    }

    public static bool Update(InsFilingCodeSubtype insFilingCodeSubtype, InsFilingCodeSubtype oldInsFilingCodeSubtype)
    {
        var command = "";
        if (insFilingCodeSubtype.InsFilingCodeNum != oldInsFilingCodeSubtype.InsFilingCodeNum)
        {
            if (command != "") command += ",";
            command += "InsFilingCodeNum = " + SOut.Long(insFilingCodeSubtype.InsFilingCodeNum) + "";
        }

        if (insFilingCodeSubtype.Descript != oldInsFilingCodeSubtype.Descript)
        {
            if (command != "") command += ",";
            command += "Descript = '" + SOut.String(insFilingCodeSubtype.Descript) + "'";
        }

        if (command == "") return false;
        command = "UPDATE insfilingcodesubtype SET " + command
                                                     + " WHERE InsFilingCodeSubtypeNum = " + SOut.Long(insFilingCodeSubtype.InsFilingCodeSubtypeNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(InsFilingCodeSubtype insFilingCodeSubtype, InsFilingCodeSubtype oldInsFilingCodeSubtype)
    {
        if (insFilingCodeSubtype.InsFilingCodeNum != oldInsFilingCodeSubtype.InsFilingCodeNum) return true;
        if (insFilingCodeSubtype.Descript != oldInsFilingCodeSubtype.Descript) return true;
        return false;
    }

    public static void Delete(long insFilingCodeSubtypeNum)
    {
        var command = "DELETE FROM insfilingcodesubtype "
                      + "WHERE InsFilingCodeSubtypeNum = " + SOut.Long(insFilingCodeSubtypeNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listInsFilingCodeSubtypeNums)
    {
        if (listInsFilingCodeSubtypeNums == null || listInsFilingCodeSubtypeNums.Count == 0) return;
        var command = "DELETE FROM insfilingcodesubtype "
                      + "WHERE InsFilingCodeSubtypeNum IN(" + string.Join(",", listInsFilingCodeSubtypeNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}