using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class DiscountPlanSubCrud
{
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
        foreach (DataRow row in table.Rows)
        {
            var discountPlanSub = new DiscountPlanSub
            {
                DiscountSubNum = SIn.Long(row["DiscountSubNum"].ToString()),
                DiscountPlanNum = SIn.Long(row["DiscountPlanNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                DateEffective = SIn.Date(row["DateEffective"].ToString()),
                DateTerm = SIn.Date(row["DateTerm"].ToString()),
                SubNote = SIn.String(row["SubNote"].ToString())
            };
            retVal.Add(discountPlanSub);
        }

        return retVal;
    }

    public static void Insert(DiscountPlanSub discountPlanSub)
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
        var paramSubNote = new OdSqlParameter("paramSubNote", SOut.StringParam(discountPlanSub.SubNote));
        {
            discountPlanSub.DiscountSubNum = Db.NonQ(command, true, "DiscountSubNum", "discountPlanSub", paramSubNote);
        }
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
        var paramSubNote = new OdSqlParameter("paramSubNote", SOut.StringParam(discountPlanSub.SubNote));
        Db.NonQ(command, paramSubNote);
    }

    public static void Delete(long discountSubNum)
    {
        var command = "DELETE FROM discountplansub "
                      + "WHERE DiscountSubNum = " + SOut.Long(discountSubNum);
        Db.NonQ(command);
    }
}