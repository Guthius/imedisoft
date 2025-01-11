using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ClaimSnapshotCrud
{
    public static List<ClaimSnapshot> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ClaimSnapshot> TableToList(DataTable table)
    {
        var retVal = new List<ClaimSnapshot>();
        ClaimSnapshot claimSnapshot;
        foreach (DataRow row in table.Rows)
        {
            claimSnapshot = new ClaimSnapshot();
            claimSnapshot.ClaimSnapshotNum = SIn.Long(row["ClaimSnapshotNum"].ToString());
            claimSnapshot.ProcNum = SIn.Long(row["ProcNum"].ToString());
            claimSnapshot.ClaimType = SIn.String(row["ClaimType"].ToString());
            claimSnapshot.Writeoff = SIn.Double(row["Writeoff"].ToString());
            claimSnapshot.InsPayEst = SIn.Double(row["InsPayEst"].ToString());
            claimSnapshot.Fee = SIn.Double(row["Fee"].ToString());
            claimSnapshot.DateTEntry = SIn.DateTime(row["DateTEntry"].ToString());
            claimSnapshot.ClaimProcNum = SIn.Long(row["ClaimProcNum"].ToString());
            claimSnapshot.SnapshotTrigger = (ClaimSnapshotTrigger) SIn.Int(row["SnapshotTrigger"].ToString());
            retVal.Add(claimSnapshot);
        }

        return retVal;
    }

    public static long Insert(ClaimSnapshot claimSnapshot)
    {
        var command = "INSERT INTO claimsnapshot (";

        command += "ProcNum,ClaimType,Writeoff,InsPayEst,Fee,DateTEntry,ClaimProcNum,SnapshotTrigger) VALUES(";

        command +=
            SOut.Long(claimSnapshot.ProcNum) + ","
                                             + "'" + SOut.String(claimSnapshot.ClaimType) + "',"
                                             + SOut.Double(claimSnapshot.Writeoff) + ","
                                             + SOut.Double(claimSnapshot.InsPayEst) + ","
                                             + SOut.Double(claimSnapshot.Fee) + ","
                                             + DbHelper.Now() + ","
                                             + SOut.Long(claimSnapshot.ClaimProcNum) + ","
                                             + SOut.Int((int) claimSnapshot.SnapshotTrigger) + ")";
        {
            claimSnapshot.ClaimSnapshotNum = Db.NonQ(command, true, "ClaimSnapshotNum", "claimSnapshot");
        }
        return claimSnapshot.ClaimSnapshotNum;
    }

    public static void Update(ClaimSnapshot claimSnapshot)
    {
        var command = "UPDATE claimsnapshot SET "
                      + "ProcNum         =  " + SOut.Long(claimSnapshot.ProcNum) + ", "
                      + "ClaimType       = '" + SOut.String(claimSnapshot.ClaimType) + "', "
                      + "Writeoff        =  " + SOut.Double(claimSnapshot.Writeoff) + ", "
                      + "InsPayEst       =  " + SOut.Double(claimSnapshot.InsPayEst) + ", "
                      + "Fee             =  " + SOut.Double(claimSnapshot.Fee) + ", "
                      //DateTEntry not allowed to change
                      + "ClaimProcNum    =  " + SOut.Long(claimSnapshot.ClaimProcNum) + ", "
                      + "SnapshotTrigger =  " + SOut.Int((int) claimSnapshot.SnapshotTrigger) + " "
                      + "WHERE ClaimSnapshotNum = " + SOut.Long(claimSnapshot.ClaimSnapshotNum);
        Db.NonQ(command);
    }
}