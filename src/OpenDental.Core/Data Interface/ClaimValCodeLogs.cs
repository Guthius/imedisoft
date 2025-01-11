using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class ClaimValCodeLogs
{
    public static double GetValAmountTotal(long claimNum, string valCode)
    {
        //double total = 0;
        var command = "SELECT SUM(ValAmount) FROM claimvalcodelog WHERE ClaimNum=" + SOut.Long(claimNum) + " AND ValCode='" + SOut.String(valCode) + "'";
        return SIn.Double(DataCore.GetScalar(command));
        //DataTable table=DataCore.GetTable(command);
        //for(int i=0;i<table.Rows.Count;i++){
        //	total+=PIn.Double(table.Rows[i][4].ToString());
        //}
        //return total;
    }

    public static List<ClaimValCodeLog> GetForClaim(long claimNum)
    {
        var command = "SELECT * FROM claimvalcodelog WHERE ClaimNum=" + SOut.Long(claimNum);
        return ClaimValCodeLogCrud.SelectMany(command);
    }

    public static void UpdateList(List<ClaimValCodeLog> listClaimValCodeLogs)
    {
        for (var i = 0; i < listClaimValCodeLogs.Count; i++)
            if (listClaimValCodeLogs[i].ClaimValCodeLogNum == 0)
                ClaimValCodeLogCrud.Insert(listClaimValCodeLogs[i]);
            else
                ClaimValCodeLogCrud.Update(listClaimValCodeLogs[i]);
    }
}