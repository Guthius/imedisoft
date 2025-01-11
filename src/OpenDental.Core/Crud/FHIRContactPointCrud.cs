#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class FHIRContactPointCrud
{
    public static FHIRContactPoint SelectOne(long fHIRContactPointNum)
    {
        var command = "SELECT * FROM fhircontactpoint "
                      + "WHERE FHIRContactPointNum = " + SOut.Long(fHIRContactPointNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static FHIRContactPoint SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<FHIRContactPoint> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<FHIRContactPoint> TableToList(DataTable table)
    {
        var retVal = new List<FHIRContactPoint>();
        FHIRContactPoint fHIRContactPoint;
        foreach (DataRow row in table.Rows)
        {
            fHIRContactPoint = new FHIRContactPoint();
            fHIRContactPoint.FHIRContactPointNum = SIn.Long(row["FHIRContactPointNum"].ToString());
            fHIRContactPoint.FHIRSubscriptionNum = SIn.Long(row["FHIRSubscriptionNum"].ToString());
            fHIRContactPoint.ContactSystem = (ContactPointSystem) SIn.Int(row["ContactSystem"].ToString());
            fHIRContactPoint.ContactValue = SIn.String(row["ContactValue"].ToString());
            fHIRContactPoint.ContactUse = (ContactPointUse) SIn.Int(row["ContactUse"].ToString());
            fHIRContactPoint.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            fHIRContactPoint.DateStart = SIn.Date(row["DateStart"].ToString());
            fHIRContactPoint.DateEnd = SIn.Date(row["DateEnd"].ToString());
            retVal.Add(fHIRContactPoint);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<FHIRContactPoint> listFHIRContactPoints, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "FHIRContactPoint";
        var table = new DataTable(tableName);
        table.Columns.Add("FHIRContactPointNum");
        table.Columns.Add("FHIRSubscriptionNum");
        table.Columns.Add("ContactSystem");
        table.Columns.Add("ContactValue");
        table.Columns.Add("ContactUse");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("DateStart");
        table.Columns.Add("DateEnd");
        foreach (var fHIRContactPoint in listFHIRContactPoints)
            table.Rows.Add(SOut.Long(fHIRContactPoint.FHIRContactPointNum), SOut.Long(fHIRContactPoint.FHIRSubscriptionNum), SOut.Int((int) fHIRContactPoint.ContactSystem), fHIRContactPoint.ContactValue, SOut.Int((int) fHIRContactPoint.ContactUse), SOut.Int(fHIRContactPoint.ItemOrder), SOut.DateT(fHIRContactPoint.DateStart, false), SOut.DateT(fHIRContactPoint.DateEnd, false));
        return table;
    }

    public static long Insert(FHIRContactPoint fHIRContactPoint)
    {
        return Insert(fHIRContactPoint, false);
    }

    public static long Insert(FHIRContactPoint fHIRContactPoint, bool useExistingPK)
    {
        var command = "INSERT INTO fhircontactpoint (";

        command += "FHIRSubscriptionNum,ContactSystem,ContactValue,ContactUse,ItemOrder,DateStart,DateEnd) VALUES(";

        command +=
            SOut.Long(fHIRContactPoint.FHIRSubscriptionNum) + ","
                                                            + SOut.Int((int) fHIRContactPoint.ContactSystem) + ","
                                                            + "'" + SOut.String(fHIRContactPoint.ContactValue) + "',"
                                                            + SOut.Int((int) fHIRContactPoint.ContactUse) + ","
                                                            + SOut.Int(fHIRContactPoint.ItemOrder) + ","
                                                            + SOut.Date(fHIRContactPoint.DateStart) + ","
                                                            + SOut.Date(fHIRContactPoint.DateEnd) + ")";
        {
            fHIRContactPoint.FHIRContactPointNum = Db.NonQ(command, true, "FHIRContactPointNum", "fHIRContactPoint");
        }
        return fHIRContactPoint.FHIRContactPointNum;
    }

    public static long InsertNoCache(FHIRContactPoint fHIRContactPoint)
    {
        return InsertNoCache(fHIRContactPoint, false);
    }

    public static long InsertNoCache(FHIRContactPoint fHIRContactPoint, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO fhircontactpoint (";
        if (isRandomKeys || useExistingPK) command += "FHIRContactPointNum,";
        command += "FHIRSubscriptionNum,ContactSystem,ContactValue,ContactUse,ItemOrder,DateStart,DateEnd) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(fHIRContactPoint.FHIRContactPointNum) + ",";
        command +=
            SOut.Long(fHIRContactPoint.FHIRSubscriptionNum) + ","
                                                            + SOut.Int((int) fHIRContactPoint.ContactSystem) + ","
                                                            + "'" + SOut.String(fHIRContactPoint.ContactValue) + "',"
                                                            + SOut.Int((int) fHIRContactPoint.ContactUse) + ","
                                                            + SOut.Int(fHIRContactPoint.ItemOrder) + ","
                                                            + SOut.Date(fHIRContactPoint.DateStart) + ","
                                                            + SOut.Date(fHIRContactPoint.DateEnd) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            fHIRContactPoint.FHIRContactPointNum = Db.NonQ(command, true, "FHIRContactPointNum", "fHIRContactPoint");
        return fHIRContactPoint.FHIRContactPointNum;
    }

    public static void Update(FHIRContactPoint fHIRContactPoint)
    {
        var command = "UPDATE fhircontactpoint SET "
                      + "FHIRSubscriptionNum=  " + SOut.Long(fHIRContactPoint.FHIRSubscriptionNum) + ", "
                      + "ContactSystem      =  " + SOut.Int((int) fHIRContactPoint.ContactSystem) + ", "
                      + "ContactValue       = '" + SOut.String(fHIRContactPoint.ContactValue) + "', "
                      + "ContactUse         =  " + SOut.Int((int) fHIRContactPoint.ContactUse) + ", "
                      + "ItemOrder          =  " + SOut.Int(fHIRContactPoint.ItemOrder) + ", "
                      + "DateStart          =  " + SOut.Date(fHIRContactPoint.DateStart) + ", "
                      + "DateEnd            =  " + SOut.Date(fHIRContactPoint.DateEnd) + " "
                      + "WHERE FHIRContactPointNum = " + SOut.Long(fHIRContactPoint.FHIRContactPointNum);
        Db.NonQ(command);
    }

    public static bool Update(FHIRContactPoint fHIRContactPoint, FHIRContactPoint oldFHIRContactPoint)
    {
        var command = "";
        if (fHIRContactPoint.FHIRSubscriptionNum != oldFHIRContactPoint.FHIRSubscriptionNum)
        {
            if (command != "") command += ",";
            command += "FHIRSubscriptionNum = " + SOut.Long(fHIRContactPoint.FHIRSubscriptionNum) + "";
        }

        if (fHIRContactPoint.ContactSystem != oldFHIRContactPoint.ContactSystem)
        {
            if (command != "") command += ",";
            command += "ContactSystem = " + SOut.Int((int) fHIRContactPoint.ContactSystem) + "";
        }

        if (fHIRContactPoint.ContactValue != oldFHIRContactPoint.ContactValue)
        {
            if (command != "") command += ",";
            command += "ContactValue = '" + SOut.String(fHIRContactPoint.ContactValue) + "'";
        }

        if (fHIRContactPoint.ContactUse != oldFHIRContactPoint.ContactUse)
        {
            if (command != "") command += ",";
            command += "ContactUse = " + SOut.Int((int) fHIRContactPoint.ContactUse) + "";
        }

        if (fHIRContactPoint.ItemOrder != oldFHIRContactPoint.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(fHIRContactPoint.ItemOrder) + "";
        }

        if (fHIRContactPoint.DateStart.Date != oldFHIRContactPoint.DateStart.Date)
        {
            if (command != "") command += ",";
            command += "DateStart = " + SOut.Date(fHIRContactPoint.DateStart) + "";
        }

        if (fHIRContactPoint.DateEnd.Date != oldFHIRContactPoint.DateEnd.Date)
        {
            if (command != "") command += ",";
            command += "DateEnd = " + SOut.Date(fHIRContactPoint.DateEnd) + "";
        }

        if (command == "") return false;
        command = "UPDATE fhircontactpoint SET " + command
                                                 + " WHERE FHIRContactPointNum = " + SOut.Long(fHIRContactPoint.FHIRContactPointNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(FHIRContactPoint fHIRContactPoint, FHIRContactPoint oldFHIRContactPoint)
    {
        if (fHIRContactPoint.FHIRSubscriptionNum != oldFHIRContactPoint.FHIRSubscriptionNum) return true;
        if (fHIRContactPoint.ContactSystem != oldFHIRContactPoint.ContactSystem) return true;
        if (fHIRContactPoint.ContactValue != oldFHIRContactPoint.ContactValue) return true;
        if (fHIRContactPoint.ContactUse != oldFHIRContactPoint.ContactUse) return true;
        if (fHIRContactPoint.ItemOrder != oldFHIRContactPoint.ItemOrder) return true;
        if (fHIRContactPoint.DateStart.Date != oldFHIRContactPoint.DateStart.Date) return true;
        if (fHIRContactPoint.DateEnd.Date != oldFHIRContactPoint.DateEnd.Date) return true;
        return false;
    }

    public static void Delete(long fHIRContactPointNum)
    {
        var command = "DELETE FROM fhircontactpoint "
                      + "WHERE FHIRContactPointNum = " + SOut.Long(fHIRContactPointNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listFHIRContactPointNums)
    {
        if (listFHIRContactPointNums == null || listFHIRContactPointNums.Count == 0) return;
        var command = "DELETE FROM fhircontactpoint "
                      + "WHERE FHIRContactPointNum IN(" + string.Join(",", listFHIRContactPointNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}