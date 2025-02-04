using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class RxNormCrud
{
    public static List<RxNorm> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RxNorm> TableToList(DataTable table)
    {
        var retVal = new List<RxNorm>();
        foreach (DataRow row in table.Rows)
        {
            var rxNorm = new RxNorm
            {
                RxNormNum = SIn.Long(row["RxNormNum"].ToString()),
                RxCui = SIn.String(row["RxCui"].ToString()),
                MmslCode = SIn.String(row["MmslCode"].ToString()),
                Description = SIn.String(row["Description"].ToString())
            };
            retVal.Add(rxNorm);
        }

        return retVal;
    }

    public static void Insert(RxNorm rxNorm)
    {
        var command = "INSERT INTO rxnorm (";

        command += "RxCui,MmslCode,Description) VALUES(";

        command +=
            "'" + SOut.String(rxNorm.RxCui) + "',"
            + "'" + SOut.String(rxNorm.MmslCode) + "',"
            + DbHelper.ParamChar + "paramDescription)";
        if (rxNorm.Description == null) rxNorm.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", SOut.StringParam(rxNorm.Description));
        {
            rxNorm.RxNormNum = Db.NonQ(command, true, "RxNormNum", "rxNorm", paramDescription);
        }
    }

    public static void Update(RxNorm rxNorm)
    {
        var command = "UPDATE rxnorm SET "
                      + "RxCui      = '" + SOut.String(rxNorm.RxCui) + "', "
                      + "MmslCode   = '" + SOut.String(rxNorm.MmslCode) + "', "
                      + "Description=  " + DbHelper.ParamChar + "paramDescription "
                      + "WHERE RxNormNum = " + SOut.Long(rxNorm.RxNormNum);
        if (rxNorm.Description == null) rxNorm.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", SOut.StringParam(rxNorm.Description));
        Db.NonQ(command, paramDescription);
    }
}