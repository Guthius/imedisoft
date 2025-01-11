#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class Etrans835AttachCrud
{
    public static Etrans835Attach SelectOne(long etrans835AttachNum)
    {
        var command = "SELECT * FROM etrans835attach "
                      + "WHERE Etrans835AttachNum = " + SOut.Long(etrans835AttachNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Etrans835Attach SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Etrans835Attach> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Etrans835Attach> TableToList(DataTable table)
    {
        var retVal = new List<Etrans835Attach>();
        Etrans835Attach etrans835Attach;
        foreach (DataRow row in table.Rows)
        {
            etrans835Attach = new Etrans835Attach();
            etrans835Attach.Etrans835AttachNum = SIn.Long(row["Etrans835AttachNum"].ToString());
            etrans835Attach.EtransNum = SIn.Long(row["EtransNum"].ToString());
            etrans835Attach.ClaimNum = SIn.Long(row["ClaimNum"].ToString());
            etrans835Attach.ClpSegmentIndex = SIn.Int(row["ClpSegmentIndex"].ToString());
            etrans835Attach.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            retVal.Add(etrans835Attach);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Etrans835Attach> listEtrans835Attachs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Etrans835Attach";
        var table = new DataTable(tableName);
        table.Columns.Add("Etrans835AttachNum");
        table.Columns.Add("EtransNum");
        table.Columns.Add("ClaimNum");
        table.Columns.Add("ClpSegmentIndex");
        table.Columns.Add("DateTimeEntry");
        foreach (var etrans835Attach in listEtrans835Attachs)
            table.Rows.Add(SOut.Long(etrans835Attach.Etrans835AttachNum), SOut.Long(etrans835Attach.EtransNum), SOut.Long(etrans835Attach.ClaimNum), SOut.Int(etrans835Attach.ClpSegmentIndex), SOut.DateT(etrans835Attach.DateTimeEntry, false));
        return table;
    }

    public static long Insert(Etrans835Attach etrans835Attach)
    {
        return Insert(etrans835Attach, false);
    }

    public static long Insert(Etrans835Attach etrans835Attach, bool useExistingPK)
    {
        var command = "INSERT INTO etrans835attach (";

        command += "EtransNum,ClaimNum,ClpSegmentIndex,DateTimeEntry) VALUES(";

        command +=
            SOut.Long(etrans835Attach.EtransNum) + ","
                                                 + SOut.Long(etrans835Attach.ClaimNum) + ","
                                                 + SOut.Int(etrans835Attach.ClpSegmentIndex) + ","
                                                 + DbHelper.Now() + ")";
        {
            etrans835Attach.Etrans835AttachNum = Db.NonQ(command, true, "Etrans835AttachNum", "etrans835Attach");
        }
        return etrans835Attach.Etrans835AttachNum;
    }

    public static long InsertNoCache(Etrans835Attach etrans835Attach)
    {
        return InsertNoCache(etrans835Attach, false);
    }

    public static long InsertNoCache(Etrans835Attach etrans835Attach, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO etrans835attach (";
        if (isRandomKeys || useExistingPK) command += "Etrans835AttachNum,";
        command += "EtransNum,ClaimNum,ClpSegmentIndex,DateTimeEntry) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(etrans835Attach.Etrans835AttachNum) + ",";
        command +=
            SOut.Long(etrans835Attach.EtransNum) + ","
                                                 + SOut.Long(etrans835Attach.ClaimNum) + ","
                                                 + SOut.Int(etrans835Attach.ClpSegmentIndex) + ","
                                                 + DbHelper.Now() + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            etrans835Attach.Etrans835AttachNum = Db.NonQ(command, true, "Etrans835AttachNum", "etrans835Attach");
        return etrans835Attach.Etrans835AttachNum;
    }

    public static void Update(Etrans835Attach etrans835Attach)
    {
        var command = "UPDATE etrans835attach SET "
                      + "EtransNum         =  " + SOut.Long(etrans835Attach.EtransNum) + ", "
                      + "ClaimNum          =  " + SOut.Long(etrans835Attach.ClaimNum) + ", "
                      + "ClpSegmentIndex   =  " + SOut.Int(etrans835Attach.ClpSegmentIndex) + " "
                      //DateTimeEntry not allowed to change
                      + "WHERE Etrans835AttachNum = " + SOut.Long(etrans835Attach.Etrans835AttachNum);
        Db.NonQ(command);
    }

    public static bool Update(Etrans835Attach etrans835Attach, Etrans835Attach oldEtrans835Attach)
    {
        var command = "";
        if (etrans835Attach.EtransNum != oldEtrans835Attach.EtransNum)
        {
            if (command != "") command += ",";
            command += "EtransNum = " + SOut.Long(etrans835Attach.EtransNum) + "";
        }

        if (etrans835Attach.ClaimNum != oldEtrans835Attach.ClaimNum)
        {
            if (command != "") command += ",";
            command += "ClaimNum = " + SOut.Long(etrans835Attach.ClaimNum) + "";
        }

        if (etrans835Attach.ClpSegmentIndex != oldEtrans835Attach.ClpSegmentIndex)
        {
            if (command != "") command += ",";
            command += "ClpSegmentIndex = " + SOut.Int(etrans835Attach.ClpSegmentIndex) + "";
        }

        //DateTimeEntry not allowed to change
        if (command == "") return false;
        command = "UPDATE etrans835attach SET " + command
                                                + " WHERE Etrans835AttachNum = " + SOut.Long(etrans835Attach.Etrans835AttachNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Etrans835Attach etrans835Attach, Etrans835Attach oldEtrans835Attach)
    {
        if (etrans835Attach.EtransNum != oldEtrans835Attach.EtransNum) return true;
        if (etrans835Attach.ClaimNum != oldEtrans835Attach.ClaimNum) return true;
        if (etrans835Attach.ClpSegmentIndex != oldEtrans835Attach.ClpSegmentIndex) return true;
        //DateTimeEntry not allowed to change
        return false;
    }

    public static void Delete(long etrans835AttachNum)
    {
        var command = "DELETE FROM etrans835attach "
                      + "WHERE Etrans835AttachNum = " + SOut.Long(etrans835AttachNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEtrans835AttachNums)
    {
        if (listEtrans835AttachNums == null || listEtrans835AttachNums.Count == 0) return;
        var command = "DELETE FROM etrans835attach "
                      + "WHERE Etrans835AttachNum IN(" + string.Join(",", listEtrans835AttachNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}