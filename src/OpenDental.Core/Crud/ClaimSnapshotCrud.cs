using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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
        foreach (DataRow row in table.Rows)
        {
            var claimSnapshot = new ClaimSnapshot
            {
                ClaimSnapshotNum = SIn.Long(row["ClaimSnapshotNum"].ToString()),
                ProcNum = SIn.Long(row["ProcNum"].ToString()),
                ClaimType = SIn.String(row["ClaimType"].ToString()),
                Writeoff = SIn.Double(row["Writeoff"].ToString()),
                InsPayEst = SIn.Double(row["InsPayEst"].ToString()),
                Fee = SIn.Double(row["Fee"].ToString()),
                DateTEntry = SIn.DateTime(row["DateTEntry"].ToString()),
                ClaimProcNum = SIn.Long(row["ClaimProcNum"].ToString()),
                SnapshotTrigger = (ClaimSnapshotTrigger) SIn.Int(row["SnapshotTrigger"].ToString())
            };
            retVal.Add(claimSnapshot);
        }

        return retVal;
    }

    public static void Insert(ClaimSnapshot claimSnapshot)
    {
        var command = "INSERT INTO claimsnapshot (";

        command += "ProcNum,ClaimType,Writeoff,InsPayEst,Fee,DateTEntry,ClaimProcNum,SnapshotTrigger) VALUES(";

        command +=
            SOut.Long(claimSnapshot.ProcNum) + ","
                                             + "'" + SOut.String(claimSnapshot.ClaimType) + "',"
                                             + SOut.Double(claimSnapshot.Writeoff) + ","
                                             + SOut.Double(claimSnapshot.InsPayEst) + ","
                                             + SOut.Double(claimSnapshot.Fee) + ","
                                             + "NOW()" + ","
                                             + SOut.Long(claimSnapshot.ClaimProcNum) + ","
                                             + SOut.Int((int) claimSnapshot.SnapshotTrigger) + ")";
        {
            claimSnapshot.ClaimSnapshotNum = Db.NonQ(command, true, "ClaimSnapshotNum", "claimSnapshot");
        }
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