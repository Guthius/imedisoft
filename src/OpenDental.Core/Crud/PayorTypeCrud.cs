using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PayorTypeCrud
{
    public static PayorType SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PayorType> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PayorType> TableToList(DataTable table)
    {
        var retVal = new List<PayorType>();
        PayorType payorType;
        foreach (DataRow row in table.Rows)
        {
            payorType = new PayorType();
            payorType.PayorTypeNum = SIn.Long(row["PayorTypeNum"].ToString());
            payorType.PatNum = SIn.Long(row["PatNum"].ToString());
            payorType.DateStart = SIn.Date(row["DateStart"].ToString());
            payorType.SopCode = SIn.String(row["SopCode"].ToString());
            payorType.Note = SIn.String(row["Note"].ToString());
            retVal.Add(payorType);
        }

        return retVal;
    }

    public static void Insert(PayorType payorType)
    {
        var command = "INSERT INTO payortype (";

        command += "PatNum,DateStart,SopCode,Note) VALUES(";

        command +=
            SOut.Long(payorType.PatNum) + ","
                                        + SOut.Date(payorType.DateStart) + ","
                                        + "'" + SOut.String(payorType.SopCode) + "',"
                                        + DbHelper.ParamChar + "paramNote)";
        if (payorType.Note == null) payorType.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(payorType.Note));
        {
            payorType.PayorTypeNum = Db.NonQ(command, true, "PayorTypeNum", "payorType", paramNote);
        }
    }

    public static void Update(PayorType payorType)
    {
        var command = "UPDATE payortype SET "
                      + "PatNum      =  " + SOut.Long(payorType.PatNum) + ", "
                      + "DateStart   =  " + SOut.Date(payorType.DateStart) + ", "
                      + "SopCode     = '" + SOut.String(payorType.SopCode) + "', "
                      + "Note        =  " + DbHelper.ParamChar + "paramNote "
                      + "WHERE PayorTypeNum = " + SOut.Long(payorType.PayorTypeNum);
        if (payorType.Note == null) payorType.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(payorType.Note));
        Db.NonQ(command, paramNote);
    }
}