using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PearlRequestCrud
{
    public static PearlRequest SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PearlRequest> TableToList(DataTable table)
    {
        var retVal = new List<PearlRequest>();
        PearlRequest pearlRequest;
        foreach (DataRow row in table.Rows)
        {
            pearlRequest = new PearlRequest();
            pearlRequest.PearlRequestNum = SIn.Long(row["PearlRequestNum"].ToString());
            pearlRequest.RequestId = SIn.String(row["RequestId"].ToString());
            pearlRequest.DocNum = SIn.Long(row["DocNum"].ToString());
            pearlRequest.RequestStatus = (EnumPearlStatus) SIn.Int(row["RequestStatus"].ToString());
            pearlRequest.DateTSent = SIn.Date(row["DateTSent"].ToString());
            pearlRequest.DateTChecked = SIn.Date(row["DateTChecked"].ToString());
            retVal.Add(pearlRequest);
        }

        return retVal;
    }

    public static void Insert(PearlRequest pearlRequest)
    {
        var command = "INSERT INTO pearlrequest (";

        command += "RequestId,DocNum,RequestStatus,DateTSent,DateTChecked) VALUES(";

        command +=
            "'" + SOut.String(pearlRequest.RequestId) + "',"
            + SOut.Long(pearlRequest.DocNum) + ","
            + SOut.Int((int) pearlRequest.RequestStatus) + ","
            + SOut.Date(pearlRequest.DateTSent) + ","
            + SOut.Date(pearlRequest.DateTChecked) + ")";
        {
            pearlRequest.PearlRequestNum = Db.NonQ(command, true, "PearlRequestNum", "pearlRequest");
        }
    }

    public static void Update(PearlRequest pearlRequest)
    {
        var command = "UPDATE pearlrequest SET "
                      + "RequestId      = '" + SOut.String(pearlRequest.RequestId) + "', "
                      + "DocNum         =  " + SOut.Long(pearlRequest.DocNum) + ", "
                      + "RequestStatus  =  " + SOut.Int((int) pearlRequest.RequestStatus) + ", "
                      + "DateTSent      =  " + SOut.Date(pearlRequest.DateTSent) + ", "
                      + "DateTChecked   =  " + SOut.Date(pearlRequest.DateTChecked) + " "
                      + "WHERE PearlRequestNum = " + SOut.Long(pearlRequest.PearlRequestNum);
        Db.NonQ(command);
    }

    public static void Delete(long pearlRequestNum)
    {
        var command = "DELETE FROM pearlrequest "
                      + "WHERE PearlRequestNum = " + SOut.Long(pearlRequestNum);
        Db.NonQ(command);
    }
}