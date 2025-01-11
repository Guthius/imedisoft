#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PatientLinkCrud
{
    public static PatientLink SelectOne(long patientLinkNum)
    {
        var command = "SELECT * FROM patientlink "
                      + "WHERE PatientLinkNum = " + SOut.Long(patientLinkNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PatientLink SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PatientLink> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PatientLink> TableToList(DataTable table)
    {
        var retVal = new List<PatientLink>();
        PatientLink patientLink;
        foreach (DataRow row in table.Rows)
        {
            patientLink = new PatientLink();
            patientLink.PatientLinkNum = SIn.Long(row["PatientLinkNum"].ToString());
            patientLink.PatNumFrom = SIn.Long(row["PatNumFrom"].ToString());
            patientLink.PatNumTo = SIn.Long(row["PatNumTo"].ToString());
            patientLink.LinkType = (PatientLinkType) SIn.Int(row["LinkType"].ToString());
            patientLink.DateTimeLink = SIn.DateTime(row["DateTimeLink"].ToString());
            retVal.Add(patientLink);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PatientLink> listPatientLinks, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PatientLink";
        var table = new DataTable(tableName);
        table.Columns.Add("PatientLinkNum");
        table.Columns.Add("PatNumFrom");
        table.Columns.Add("PatNumTo");
        table.Columns.Add("LinkType");
        table.Columns.Add("DateTimeLink");
        foreach (var patientLink in listPatientLinks)
            table.Rows.Add(SOut.Long(patientLink.PatientLinkNum), SOut.Long(patientLink.PatNumFrom), SOut.Long(patientLink.PatNumTo), SOut.Int((int) patientLink.LinkType), SOut.DateT(patientLink.DateTimeLink, false));
        return table;
    }

    public static long Insert(PatientLink patientLink)
    {
        return Insert(patientLink, false);
    }

    public static long Insert(PatientLink patientLink, bool useExistingPK)
    {
        var command = "INSERT INTO patientlink (";

        command += "PatNumFrom,PatNumTo,LinkType,DateTimeLink) VALUES(";

        command +=
            SOut.Long(patientLink.PatNumFrom) + ","
                                              + SOut.Long(patientLink.PatNumTo) + ","
                                              + SOut.Int((int) patientLink.LinkType) + ","
                                              + DbHelper.Now() + ")";
        {
            patientLink.PatientLinkNum = Db.NonQ(command, true, "PatientLinkNum", "patientLink");
        }
        return patientLink.PatientLinkNum;
    }

    public static long InsertNoCache(PatientLink patientLink)
    {
        return InsertNoCache(patientLink, false);
    }

    public static long InsertNoCache(PatientLink patientLink, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO patientlink (";
        if (isRandomKeys || useExistingPK) command += "PatientLinkNum,";
        command += "PatNumFrom,PatNumTo,LinkType,DateTimeLink) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(patientLink.PatientLinkNum) + ",";
        command +=
            SOut.Long(patientLink.PatNumFrom) + ","
                                              + SOut.Long(patientLink.PatNumTo) + ","
                                              + SOut.Int((int) patientLink.LinkType) + ","
                                              + DbHelper.Now() + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            patientLink.PatientLinkNum = Db.NonQ(command, true, "PatientLinkNum", "patientLink");
        return patientLink.PatientLinkNum;
    }

    public static void Update(PatientLink patientLink)
    {
        var command = "UPDATE patientlink SET "
                      + "PatNumFrom    =  " + SOut.Long(patientLink.PatNumFrom) + ", "
                      + "PatNumTo      =  " + SOut.Long(patientLink.PatNumTo) + ", "
                      + "LinkType      =  " + SOut.Int((int) patientLink.LinkType) + " "
                      //DateTimeLink not allowed to change
                      + "WHERE PatientLinkNum = " + SOut.Long(patientLink.PatientLinkNum);
        Db.NonQ(command);
    }

    public static bool Update(PatientLink patientLink, PatientLink oldPatientLink)
    {
        var command = "";
        if (patientLink.PatNumFrom != oldPatientLink.PatNumFrom)
        {
            if (command != "") command += ",";
            command += "PatNumFrom = " + SOut.Long(patientLink.PatNumFrom) + "";
        }

        if (patientLink.PatNumTo != oldPatientLink.PatNumTo)
        {
            if (command != "") command += ",";
            command += "PatNumTo = " + SOut.Long(patientLink.PatNumTo) + "";
        }

        if (patientLink.LinkType != oldPatientLink.LinkType)
        {
            if (command != "") command += ",";
            command += "LinkType = " + SOut.Int((int) patientLink.LinkType) + "";
        }

        //DateTimeLink not allowed to change
        if (command == "") return false;
        command = "UPDATE patientlink SET " + command
                                            + " WHERE PatientLinkNum = " + SOut.Long(patientLink.PatientLinkNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(PatientLink patientLink, PatientLink oldPatientLink)
    {
        if (patientLink.PatNumFrom != oldPatientLink.PatNumFrom) return true;
        if (patientLink.PatNumTo != oldPatientLink.PatNumTo) return true;
        if (patientLink.LinkType != oldPatientLink.LinkType) return true;
        //DateTimeLink not allowed to change
        return false;
    }

    public static void Delete(long patientLinkNum)
    {
        var command = "DELETE FROM patientlink "
                      + "WHERE PatientLinkNum = " + SOut.Long(patientLinkNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPatientLinkNums)
    {
        if (listPatientLinkNums == null || listPatientLinkNums.Count == 0) return;
        var command = "DELETE FROM patientlink "
                      + "WHERE PatientLinkNum IN(" + string.Join(",", listPatientLinkNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}