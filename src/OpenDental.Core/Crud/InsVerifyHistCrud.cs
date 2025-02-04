using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class InsVerifyHistCrud
{
    public static List<InsVerifyHist> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InsVerifyHist> TableToList(DataTable table)
    {
        var retVal = new List<InsVerifyHist>();
        foreach (DataRow row in table.Rows)
        {
            var insVerifyHist = new InsVerifyHist
            {
                InsVerifyHistNum = SIn.Long(row["InsVerifyHistNum"].ToString()),
                VerifyUserNum = SIn.Long(row["VerifyUserNum"].ToString()),
                InsVerifyNum = SIn.Long(row["InsVerifyNum"].ToString()),
                DateLastVerified = SIn.Date(row["DateLastVerified"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                VerifyType = (VerifyTypes) SIn.Int(row["VerifyType"].ToString()),
                FKey = SIn.Long(row["FKey"].ToString()),
                DefNum = SIn.Long(row["DefNum"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                DateLastAssigned = SIn.Date(row["DateLastAssigned"].ToString()),
                DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString()),
                HoursAvailableForVerification = SIn.Double(row["HoursAvailableForVerification"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString())
            };
            retVal.Add(insVerifyHist);
        }

        return retVal;
    }

    public static void Insert(InsVerifyHist insVerifyHist)
    {
        var command = "INSERT INTO insverifyhist (";

        command += "VerifyUserNum,InsVerifyNum,DateLastVerified,UserNum,VerifyType,FKey,DefNum,Note,DateLastAssigned,DateTimeEntry,HoursAvailableForVerification) VALUES(";

        command +=
            SOut.Long(insVerifyHist.VerifyUserNum) + ","
                                                   + SOut.Long(insVerifyHist.InsVerifyNum) + ","
                                                   + SOut.Date(insVerifyHist.DateLastVerified) + ","
                                                   + SOut.Long(insVerifyHist.UserNum) + ","
                                                   + SOut.Int((int) insVerifyHist.VerifyType) + ","
                                                   + SOut.Long(insVerifyHist.FKey) + ","
                                                   + SOut.Long(insVerifyHist.DefNum) + ","
                                                   + DbHelper.ParamChar + "paramNote,"
                                                   + SOut.Date(insVerifyHist.DateLastAssigned) + ","
                                                   + SOut.DateTime(insVerifyHist.DateTimeEntry) + ","
                                                   + SOut.Double(insVerifyHist.HoursAvailableForVerification) + ")";
        //SecDateTEdit can only be set by MySQL
        if (insVerifyHist.Note == null) insVerifyHist.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringNote(insVerifyHist.Note));
        {
            insVerifyHist.InsVerifyHistNum = Db.NonQ(command, true, "InsVerifyHistNum", "insVerifyHist", paramNote);
        }
    }
}