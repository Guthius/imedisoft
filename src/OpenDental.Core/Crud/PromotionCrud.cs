#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PromotionCrud
{
    public static Promotion SelectOne(long promotionNum)
    {
        var command = "SELECT * FROM promotion "
                      + "WHERE PromotionNum = " + SOut.Long(promotionNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Promotion SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Promotion> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Promotion> TableToList(DataTable table)
    {
        var retVal = new List<Promotion>();
        Promotion promotion;
        foreach (DataRow row in table.Rows)
        {
            promotion = new Promotion();
            promotion.PromotionNum = SIn.Long(row["PromotionNum"].ToString());
            promotion.PromotionName = SIn.String(row["PromotionName"].ToString());
            promotion.DateTimeCreated = SIn.Date(row["DateTimeCreated"].ToString());
            promotion.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            promotion.TypePromotion = (PromotionType) SIn.Int(row["TypePromotion"].ToString());
            retVal.Add(promotion);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Promotion> listPromotions, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Promotion";
        var table = new DataTable(tableName);
        table.Columns.Add("PromotionNum");
        table.Columns.Add("PromotionName");
        table.Columns.Add("DateTimeCreated");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("TypePromotion");
        foreach (var promotion in listPromotions)
            table.Rows.Add(SOut.Long(promotion.PromotionNum), promotion.PromotionName, SOut.DateT(promotion.DateTimeCreated, false), SOut.Long(promotion.ClinicNum), SOut.Int((int) promotion.TypePromotion));
        return table;
    }

    public static long Insert(Promotion promotion)
    {
        return Insert(promotion, false);
    }

    public static long Insert(Promotion promotion, bool useExistingPK)
    {
        var command = "INSERT INTO promotion (";

        command += "PromotionName,DateTimeCreated,ClinicNum,TypePromotion) VALUES(";

        command +=
            "'" + SOut.String(promotion.PromotionName) + "',"
            + DbHelper.Now() + ","
            + SOut.Long(promotion.ClinicNum) + ","
            + SOut.Int((int) promotion.TypePromotion) + ")";
        {
            promotion.PromotionNum = Db.NonQ(command, true, "PromotionNum", "promotion");
        }
        return promotion.PromotionNum;
    }

    public static long InsertNoCache(Promotion promotion)
    {
        return InsertNoCache(promotion, false);
    }

    public static long InsertNoCache(Promotion promotion, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO promotion (";
        if (isRandomKeys || useExistingPK) command += "PromotionNum,";
        command += "PromotionName,DateTimeCreated,ClinicNum,TypePromotion) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(promotion.PromotionNum) + ",";
        command +=
            "'" + SOut.String(promotion.PromotionName) + "',"
            + DbHelper.Now() + ","
            + SOut.Long(promotion.ClinicNum) + ","
            + SOut.Int((int) promotion.TypePromotion) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            promotion.PromotionNum = Db.NonQ(command, true, "PromotionNum", "promotion");
        return promotion.PromotionNum;
    }

    public static void Update(Promotion promotion)
    {
        var command = "UPDATE promotion SET "
                      + "PromotionName  = '" + SOut.String(promotion.PromotionName) + "', "
                      //DateTimeCreated not allowed to change
                      + "ClinicNum      =  " + SOut.Long(promotion.ClinicNum) + ", "
                      + "TypePromotion  =  " + SOut.Int((int) promotion.TypePromotion) + " "
                      + "WHERE PromotionNum = " + SOut.Long(promotion.PromotionNum);
        Db.NonQ(command);
    }

    public static bool Update(Promotion promotion, Promotion oldPromotion)
    {
        var command = "";
        if (promotion.PromotionName != oldPromotion.PromotionName)
        {
            if (command != "") command += ",";
            command += "PromotionName = '" + SOut.String(promotion.PromotionName) + "'";
        }

        //DateTimeCreated not allowed to change
        if (promotion.ClinicNum != oldPromotion.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(promotion.ClinicNum) + "";
        }

        if (promotion.TypePromotion != oldPromotion.TypePromotion)
        {
            if (command != "") command += ",";
            command += "TypePromotion = " + SOut.Int((int) promotion.TypePromotion) + "";
        }

        if (command == "") return false;
        command = "UPDATE promotion SET " + command
                                          + " WHERE PromotionNum = " + SOut.Long(promotion.PromotionNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Promotion promotion, Promotion oldPromotion)
    {
        if (promotion.PromotionName != oldPromotion.PromotionName) return true;
        //DateTimeCreated not allowed to change
        if (promotion.ClinicNum != oldPromotion.ClinicNum) return true;
        if (promotion.TypePromotion != oldPromotion.TypePromotion) return true;
        return false;
    }

    public static void Delete(long promotionNum)
    {
        var command = "DELETE FROM promotion "
                      + "WHERE PromotionNum = " + SOut.Long(promotionNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPromotionNums)
    {
        if (listPromotionNums == null || listPromotionNums.Count == 0) return;
        var command = "DELETE FROM promotion "
                      + "WHERE PromotionNum IN(" + string.Join(",", listPromotionNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}