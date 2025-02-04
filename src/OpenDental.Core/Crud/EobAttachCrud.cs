using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class EobAttachCrud
{
    public static EobAttach SelectOne(long eobAttachNum)
    {
        var command = "SELECT * FROM eobattach "
                      + "WHERE EobAttachNum = " + SOut.Long(eobAttachNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EobAttach> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EobAttach> TableToList(DataTable table)
    {
        var retVal = new List<EobAttach>();
        foreach (DataRow row in table.Rows)
        {
            var eobAttach = new EobAttach
            {
                EobAttachNum = SIn.Long(row["EobAttachNum"].ToString()),
                ClaimPaymentNum = SIn.Long(row["ClaimPaymentNum"].ToString()),
                DateTCreated = SIn.DateTime(row["DateTCreated"].ToString()),
                FileName = SIn.String(row["FileName"].ToString()),
                RawBase64 = SIn.String(row["RawBase64"].ToString())
            };
            retVal.Add(eobAttach);
        }

        return retVal;
    }

    public static long Insert(EobAttach eobAttach)
    {
        var command = "INSERT INTO eobattach (";

        command += "ClaimPaymentNum,DateTCreated,FileName,RawBase64) VALUES(";

        command +=
            SOut.Long(eobAttach.ClaimPaymentNum) + ","
                                                 + SOut.DateTime(eobAttach.DateTCreated) + ","
                                                 + "'" + SOut.String(eobAttach.FileName) + "',"
                                                 + DbHelper.ParamChar + "paramRawBase64)";
        if (eobAttach.RawBase64 == null) eobAttach.RawBase64 = "";
        var paramRawBase64 = new OdSqlParameter("paramRawBase64", SOut.StringParam(eobAttach.RawBase64));
        {
            eobAttach.EobAttachNum = Db.NonQ(command, true, "EobAttachNum", "eobAttach", paramRawBase64);
        }
        return eobAttach.EobAttachNum;
    }

    public static void Update(EobAttach eobAttach)
    {
        var command = "UPDATE eobattach SET "
                      + "ClaimPaymentNum=  " + SOut.Long(eobAttach.ClaimPaymentNum) + ", "
                      + "DateTCreated   =  " + SOut.DateTime(eobAttach.DateTCreated) + ", "
                      + "FileName       = '" + SOut.String(eobAttach.FileName) + "', "
                      + "RawBase64      =  " + DbHelper.ParamChar + "paramRawBase64 "
                      + "WHERE EobAttachNum = " + SOut.Long(eobAttach.EobAttachNum);
        if (eobAttach.RawBase64 == null) eobAttach.RawBase64 = "";
        var paramRawBase64 = new OdSqlParameter("paramRawBase64", SOut.StringParam(eobAttach.RawBase64));
        Db.NonQ(command, paramRawBase64);
    }
}