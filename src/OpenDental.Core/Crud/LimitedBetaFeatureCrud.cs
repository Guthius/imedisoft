#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class LimitedBetaFeatureCrud
{
    public static LimitedBetaFeature SelectOne(long limitedBetaFeatureNum)
    {
        var command = "SELECT * FROM limitedbetafeature "
                      + "WHERE LimitedBetaFeatureNum = " + SOut.Long(limitedBetaFeatureNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static LimitedBetaFeature SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<LimitedBetaFeature> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<LimitedBetaFeature> TableToList(DataTable table)
    {
        var retVal = new List<LimitedBetaFeature>();
        LimitedBetaFeature limitedBetaFeature;
        foreach (DataRow row in table.Rows)
        {
            limitedBetaFeature = new LimitedBetaFeature();
            limitedBetaFeature.LimitedBetaFeatureNum = SIn.Long(row["LimitedBetaFeatureNum"].ToString());
            limitedBetaFeature.LimitedBetaFeatureTypeNum = SIn.Long(row["LimitedBetaFeatureTypeNum"].ToString());
            limitedBetaFeature.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            limitedBetaFeature.IsSignedUp = SIn.Bool(row["IsSignedUp"].ToString());
            retVal.Add(limitedBetaFeature);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<LimitedBetaFeature> listLimitedBetaFeatures, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "LimitedBetaFeature";
        var table = new DataTable(tableName);
        table.Columns.Add("LimitedBetaFeatureNum");
        table.Columns.Add("LimitedBetaFeatureTypeNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("IsSignedUp");
        foreach (var limitedBetaFeature in listLimitedBetaFeatures)
            table.Rows.Add(SOut.Long(limitedBetaFeature.LimitedBetaFeatureNum), SOut.Long(limitedBetaFeature.LimitedBetaFeatureTypeNum), SOut.Long(limitedBetaFeature.ClinicNum), SOut.Bool(limitedBetaFeature.IsSignedUp));
        return table;
    }

    public static long Insert(LimitedBetaFeature limitedBetaFeature)
    {
        return Insert(limitedBetaFeature, false);
    }

    public static long Insert(LimitedBetaFeature limitedBetaFeature, bool useExistingPK)
    {
        var command = "INSERT INTO limitedbetafeature (";

        command += "LimitedBetaFeatureTypeNum,ClinicNum,IsSignedUp) VALUES(";

        command +=
            SOut.Long(limitedBetaFeature.LimitedBetaFeatureTypeNum) + ","
                                                                    + SOut.Long(limitedBetaFeature.ClinicNum) + ","
                                                                    + SOut.Bool(limitedBetaFeature.IsSignedUp) + ")";
        {
            limitedBetaFeature.LimitedBetaFeatureNum = Db.NonQ(command, true, "LimitedBetaFeatureNum", "limitedBetaFeature");
        }
        return limitedBetaFeature.LimitedBetaFeatureNum;
    }

    public static void InsertMany(List<LimitedBetaFeature> listLimitedBetaFeatures)
    {
        InsertMany(listLimitedBetaFeatures, false);
    }

    public static void InsertMany(List<LimitedBetaFeature> listLimitedBetaFeatures, bool useExistingPK)
    {
        {
            StringBuilder sbCommands = null;
            var index = 0;
            var countRows = 0;
            while (index < listLimitedBetaFeatures.Count)
            {
                var limitedBetaFeature = listLimitedBetaFeatures[index];
                var sbRow = new StringBuilder("(");
                var hasComma = false;
                if (sbCommands == null)
                {
                    sbCommands = new StringBuilder();
                    sbCommands.Append("INSERT INTO limitedbetafeature (");
                    if (useExistingPK) sbCommands.Append("LimitedBetaFeatureNum,");
                    sbCommands.Append("LimitedBetaFeatureTypeNum,ClinicNum,IsSignedUp) VALUES ");
                    countRows = 0;
                }
                else
                {
                    hasComma = true;
                }

                if (useExistingPK)
                {
                    sbRow.Append(SOut.Long(limitedBetaFeature.LimitedBetaFeatureNum));
                    sbRow.Append(",");
                }

                sbRow.Append(SOut.Long(limitedBetaFeature.LimitedBetaFeatureTypeNum));
                sbRow.Append(",");
                sbRow.Append(SOut.Long(limitedBetaFeature.ClinicNum));
                sbRow.Append(",");
                sbRow.Append(SOut.Bool(limitedBetaFeature.IsSignedUp));
                sbRow.Append(")");
                if (sbCommands.Length + sbRow.Length + 1 > TableBase.MaxAllowedPacketCount && countRows > 0)
                {
                    Db.NonQ(sbCommands.ToString());
                    sbCommands = null;
                }
                else
                {
                    if (hasComma) sbCommands.Append(",");
                    sbCommands.Append(sbRow);
                    countRows++;
                    if (index == listLimitedBetaFeatures.Count - 1) Db.NonQ(sbCommands.ToString());
                    index++;
                }
            }
        }
    }

    public static long InsertNoCache(LimitedBetaFeature limitedBetaFeature)
    {
        return InsertNoCache(limitedBetaFeature, false);
    }

    public static long InsertNoCache(LimitedBetaFeature limitedBetaFeature, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO limitedbetafeature (";
        if (isRandomKeys || useExistingPK) command += "LimitedBetaFeatureNum,";
        command += "LimitedBetaFeatureTypeNum,ClinicNum,IsSignedUp) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(limitedBetaFeature.LimitedBetaFeatureNum) + ",";
        command +=
            SOut.Long(limitedBetaFeature.LimitedBetaFeatureTypeNum) + ","
                                                                    + SOut.Long(limitedBetaFeature.ClinicNum) + ","
                                                                    + SOut.Bool(limitedBetaFeature.IsSignedUp) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            limitedBetaFeature.LimitedBetaFeatureNum = Db.NonQ(command, true, "LimitedBetaFeatureNum", "limitedBetaFeature");
        return limitedBetaFeature.LimitedBetaFeatureNum;
    }

    public static void Update(LimitedBetaFeature limitedBetaFeature)
    {
        var command = "UPDATE limitedbetafeature SET "
                      + "LimitedBetaFeatureTypeNum=  " + SOut.Long(limitedBetaFeature.LimitedBetaFeatureTypeNum) + ", "
                      + "ClinicNum                =  " + SOut.Long(limitedBetaFeature.ClinicNum) + ", "
                      + "IsSignedUp               =  " + SOut.Bool(limitedBetaFeature.IsSignedUp) + " "
                      + "WHERE LimitedBetaFeatureNum = " + SOut.Long(limitedBetaFeature.LimitedBetaFeatureNum);
        Db.NonQ(command);
    }

    public static bool Update(LimitedBetaFeature limitedBetaFeature, LimitedBetaFeature oldLimitedBetaFeature)
    {
        var command = "";
        if (limitedBetaFeature.LimitedBetaFeatureTypeNum != oldLimitedBetaFeature.LimitedBetaFeatureTypeNum)
        {
            if (command != "") command += ",";
            command += "LimitedBetaFeatureTypeNum = " + SOut.Long(limitedBetaFeature.LimitedBetaFeatureTypeNum) + "";
        }

        if (limitedBetaFeature.ClinicNum != oldLimitedBetaFeature.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(limitedBetaFeature.ClinicNum) + "";
        }

        if (limitedBetaFeature.IsSignedUp != oldLimitedBetaFeature.IsSignedUp)
        {
            if (command != "") command += ",";
            command += "IsSignedUp = " + SOut.Bool(limitedBetaFeature.IsSignedUp) + "";
        }

        if (command == "") return false;
        command = "UPDATE limitedbetafeature SET " + command
                                                   + " WHERE LimitedBetaFeatureNum = " + SOut.Long(limitedBetaFeature.LimitedBetaFeatureNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(LimitedBetaFeature limitedBetaFeature, LimitedBetaFeature oldLimitedBetaFeature)
    {
        if (limitedBetaFeature.LimitedBetaFeatureTypeNum != oldLimitedBetaFeature.LimitedBetaFeatureTypeNum) return true;
        if (limitedBetaFeature.ClinicNum != oldLimitedBetaFeature.ClinicNum) return true;
        if (limitedBetaFeature.IsSignedUp != oldLimitedBetaFeature.IsSignedUp) return true;
        return false;
    }

    public static void Delete(long limitedBetaFeatureNum)
    {
        var command = "DELETE FROM limitedbetafeature "
                      + "WHERE LimitedBetaFeatureNum = " + SOut.Long(limitedBetaFeatureNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listLimitedBetaFeatureNums)
    {
        if (listLimitedBetaFeatureNums == null || listLimitedBetaFeatureNums.Count == 0) return;
        var command = "DELETE FROM limitedbetafeature "
                      + "WHERE LimitedBetaFeatureNum IN(" + string.Join(",", listLimitedBetaFeatureNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}