#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DiscountPlanSubCrud
{
    public static DiscountPlanSub SelectOne(long discountSubNum)
    {
        var command = "SELECT * FROM discountplansub "
                      + "WHERE DiscountSubNum = " + SOut.Long(discountSubNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static DiscountPlanSub SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<DiscountPlanSub> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DiscountPlanSub> TableToList(DataTable table)
    {
        var retVal = new List<DiscountPlanSub>();
        DiscountPlanSub discountPlanSub;
        foreach (DataRow row in table.Rows)
        {
            discountPlanSub = new DiscountPlanSub();
            discountPlanSub.DiscountSubNum = SIn.Long(row["DiscountSubNum"].ToString());
            discountPlanSub.DiscountPlanNum = SIn.Long(row["DiscountPlanNum"].ToString());
            discountPlanSub.PatNum = SIn.Long(row["PatNum"].ToString());
            discountPlanSub.DateEffective = SIn.Date(row["DateEffective"].ToString());
            discountPlanSub.DateTerm = SIn.Date(row["DateTerm"].ToString());
            discountPlanSub.SubNote = SIn.String(row["SubNote"].ToString());
            retVal.Add(discountPlanSub);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<DiscountPlanSub> listDiscountPlanSubs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "DiscountPlanSub";
        var table = new DataTable(tableName);
        table.Columns.Add("DiscountSubNum");
        table.Columns.Add("DiscountPlanNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("DateEffective");
        table.Columns.Add("DateTerm");
        table.Columns.Add("SubNote");
        foreach (var discountPlanSub in listDiscountPlanSubs)
            table.Rows.Add(SOut.Long(discountPlanSub.DiscountSubNum), SOut.Long(discountPlanSub.DiscountPlanNum), SOut.Long(discountPlanSub.PatNum), SOut.DateT(discountPlanSub.DateEffective, false), SOut.DateT(discountPlanSub.DateTerm, false), discountPlanSub.SubNote);
        return table;
    }

    public static long Insert(DiscountPlanSub discountPlanSub)
    {
        return Insert(discountPlanSub, false);
    }

    public static long Insert(DiscountPlanSub discountPlanSub, bool useExistingPK)
    {
        var command = "INSERT INTO discountplansub (";

        command += "DiscountPlanNum,PatNum,DateEffective,DateTerm,SubNote) VALUES(";

        command +=
            SOut.Long(discountPlanSub.DiscountPlanNum) + ","
                                                       + SOut.Long(discountPlanSub.PatNum) + ","
                                                       + SOut.Date(discountPlanSub.DateEffective) + ","
                                                       + SOut.Date(discountPlanSub.DateTerm) + ","
                                                       + DbHelper.ParamChar + "paramSubNote)";
        if (discountPlanSub.SubNote == null) discountPlanSub.SubNote = "";
        var paramSubNote = new OdSqlParameter("paramSubNote", OdDbType.Text, SOut.StringParam(discountPlanSub.SubNote));
        {
            discountPlanSub.DiscountSubNum = Db.NonQ(command, true, "DiscountSubNum", "discountPlanSub", paramSubNote);
        }
        return discountPlanSub.DiscountSubNum;
    }

    public static long InsertNoCache(DiscountPlanSub discountPlanSub)
    {
        return InsertNoCache(discountPlanSub, false);
    }

    public static long InsertNoCache(DiscountPlanSub discountPlanSub, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO discountplansub (";
        if (isRandomKeys || useExistingPK) command += "DiscountSubNum,";
        command += "DiscountPlanNum,PatNum,DateEffective,DateTerm,SubNote) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(discountPlanSub.DiscountSubNum) + ",";
        command +=
            SOut.Long(discountPlanSub.DiscountPlanNum) + ","
                                                       + SOut.Long(discountPlanSub.PatNum) + ","
                                                       + SOut.Date(discountPlanSub.DateEffective) + ","
                                                       + SOut.Date(discountPlanSub.DateTerm) + ","
                                                       + DbHelper.ParamChar + "paramSubNote)";
        if (discountPlanSub.SubNote == null) discountPlanSub.SubNote = "";
        var paramSubNote = new OdSqlParameter("paramSubNote", OdDbType.Text, SOut.StringParam(discountPlanSub.SubNote));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramSubNote);
        else
            discountPlanSub.DiscountSubNum = Db.NonQ(command, true, "DiscountSubNum", "discountPlanSub", paramSubNote);
        return discountPlanSub.DiscountSubNum;
    }

    public static void Update(DiscountPlanSub discountPlanSub)
    {
        var command = "UPDATE discountplansub SET "
                      + "DiscountPlanNum=  " + SOut.Long(discountPlanSub.DiscountPlanNum) + ", "
                      + "PatNum         =  " + SOut.Long(discountPlanSub.PatNum) + ", "
                      + "DateEffective  =  " + SOut.Date(discountPlanSub.DateEffective) + ", "
                      + "DateTerm       =  " + SOut.Date(discountPlanSub.DateTerm) + ", "
                      + "SubNote        =  " + DbHelper.ParamChar + "paramSubNote "
                      + "WHERE DiscountSubNum = " + SOut.Long(discountPlanSub.DiscountSubNum);
        if (discountPlanSub.SubNote == null) discountPlanSub.SubNote = "";
        var paramSubNote = new OdSqlParameter("paramSubNote", OdDbType.Text, SOut.StringParam(discountPlanSub.SubNote));
        Db.NonQ(command, paramSubNote);
    }

    public static bool Update(DiscountPlanSub discountPlanSub, DiscountPlanSub oldDiscountPlanSub)
    {
        var command = "";
        if (discountPlanSub.DiscountPlanNum != oldDiscountPlanSub.DiscountPlanNum)
        {
            if (command != "") command += ",";
            command += "DiscountPlanNum = " + SOut.Long(discountPlanSub.DiscountPlanNum) + "";
        }

        if (discountPlanSub.PatNum != oldDiscountPlanSub.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(discountPlanSub.PatNum) + "";
        }

        if (discountPlanSub.DateEffective.Date != oldDiscountPlanSub.DateEffective.Date)
        {
            if (command != "") command += ",";
            command += "DateEffective = " + SOut.Date(discountPlanSub.DateEffective) + "";
        }

        if (discountPlanSub.DateTerm.Date != oldDiscountPlanSub.DateTerm.Date)
        {
            if (command != "") command += ",";
            command += "DateTerm = " + SOut.Date(discountPlanSub.DateTerm) + "";
        }

        if (discountPlanSub.SubNote != oldDiscountPlanSub.SubNote)
        {
            if (command != "") command += ",";
            command += "SubNote = " + DbHelper.ParamChar + "paramSubNote";
        }

        if (command == "") return false;
        if (discountPlanSub.SubNote == null) discountPlanSub.SubNote = "";
        var paramSubNote = new OdSqlParameter("paramSubNote", OdDbType.Text, SOut.StringParam(discountPlanSub.SubNote));
        command = "UPDATE discountplansub SET " + command
                                                + " WHERE DiscountSubNum = " + SOut.Long(discountPlanSub.DiscountSubNum);
        Db.NonQ(command, paramSubNote);
        return true;
    }

    public static bool UpdateComparison(DiscountPlanSub discountPlanSub, DiscountPlanSub oldDiscountPlanSub)
    {
        if (discountPlanSub.DiscountPlanNum != oldDiscountPlanSub.DiscountPlanNum) return true;
        if (discountPlanSub.PatNum != oldDiscountPlanSub.PatNum) return true;
        if (discountPlanSub.DateEffective.Date != oldDiscountPlanSub.DateEffective.Date) return true;
        if (discountPlanSub.DateTerm.Date != oldDiscountPlanSub.DateTerm.Date) return true;
        if (discountPlanSub.SubNote != oldDiscountPlanSub.SubNote) return true;
        return false;
    }

    public static void Delete(long discountSubNum)
    {
        var command = "DELETE FROM discountplansub "
                      + "WHERE DiscountSubNum = " + SOut.Long(discountSubNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDiscountSubNums)
    {
        if (listDiscountSubNums == null || listDiscountSubNums.Count == 0) return;
        var command = "DELETE FROM discountplansub "
                      + "WHERE DiscountSubNum IN(" + string.Join(",", listDiscountSubNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}