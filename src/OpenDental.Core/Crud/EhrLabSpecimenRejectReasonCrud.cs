#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrLabSpecimenRejectReasonCrud
{
    public static EhrLabSpecimenRejectReason SelectOne(long ehrLabSpecimenRejectReasonNum)
    {
        var command = "SELECT * FROM ehrlabspecimenrejectreason "
                      + "WHERE EhrLabSpecimenRejectReasonNum = " + SOut.Long(ehrLabSpecimenRejectReasonNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrLabSpecimenRejectReason SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrLabSpecimenRejectReason> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrLabSpecimenRejectReason> TableToList(DataTable table)
    {
        var retVal = new List<EhrLabSpecimenRejectReason>();
        EhrLabSpecimenRejectReason ehrLabSpecimenRejectReason;
        foreach (DataRow row in table.Rows)
        {
            ehrLabSpecimenRejectReason = new EhrLabSpecimenRejectReason();
            ehrLabSpecimenRejectReason.EhrLabSpecimenRejectReasonNum = SIn.Long(row["EhrLabSpecimenRejectReasonNum"].ToString());
            ehrLabSpecimenRejectReason.EhrLabSpecimenNum = SIn.Long(row["EhrLabSpecimenNum"].ToString());
            ehrLabSpecimenRejectReason.SpecimenRejectReasonID = SIn.String(row["SpecimenRejectReasonID"].ToString());
            ehrLabSpecimenRejectReason.SpecimenRejectReasonText = SIn.String(row["SpecimenRejectReasonText"].ToString());
            ehrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemName = SIn.String(row["SpecimenRejectReasonCodeSystemName"].ToString());
            ehrLabSpecimenRejectReason.SpecimenRejectReasonIDAlt = SIn.String(row["SpecimenRejectReasonIDAlt"].ToString());
            ehrLabSpecimenRejectReason.SpecimenRejectReasonTextAlt = SIn.String(row["SpecimenRejectReasonTextAlt"].ToString());
            ehrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemNameAlt = SIn.String(row["SpecimenRejectReasonCodeSystemNameAlt"].ToString());
            ehrLabSpecimenRejectReason.SpecimenRejectReasonTextOriginal = SIn.String(row["SpecimenRejectReasonTextOriginal"].ToString());
            retVal.Add(ehrLabSpecimenRejectReason);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrLabSpecimenRejectReason> listEhrLabSpecimenRejectReasons, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrLabSpecimenRejectReason";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrLabSpecimenRejectReasonNum");
        table.Columns.Add("EhrLabSpecimenNum");
        table.Columns.Add("SpecimenRejectReasonID");
        table.Columns.Add("SpecimenRejectReasonText");
        table.Columns.Add("SpecimenRejectReasonCodeSystemName");
        table.Columns.Add("SpecimenRejectReasonIDAlt");
        table.Columns.Add("SpecimenRejectReasonTextAlt");
        table.Columns.Add("SpecimenRejectReasonCodeSystemNameAlt");
        table.Columns.Add("SpecimenRejectReasonTextOriginal");
        foreach (var ehrLabSpecimenRejectReason in listEhrLabSpecimenRejectReasons)
            table.Rows.Add(SOut.Long(ehrLabSpecimenRejectReason.EhrLabSpecimenRejectReasonNum), SOut.Long(ehrLabSpecimenRejectReason.EhrLabSpecimenNum), ehrLabSpecimenRejectReason.SpecimenRejectReasonID, ehrLabSpecimenRejectReason.SpecimenRejectReasonText, ehrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemName, ehrLabSpecimenRejectReason.SpecimenRejectReasonIDAlt, ehrLabSpecimenRejectReason.SpecimenRejectReasonTextAlt, ehrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemNameAlt, ehrLabSpecimenRejectReason.SpecimenRejectReasonTextOriginal);
        return table;
    }

    public static long Insert(EhrLabSpecimenRejectReason ehrLabSpecimenRejectReason)
    {
        return Insert(ehrLabSpecimenRejectReason, false);
    }

    public static long Insert(EhrLabSpecimenRejectReason ehrLabSpecimenRejectReason, bool useExistingPK)
    {
        var command = "INSERT INTO ehrlabspecimenrejectreason (";

        command += "EhrLabSpecimenNum,SpecimenRejectReasonID,SpecimenRejectReasonText,SpecimenRejectReasonCodeSystemName,SpecimenRejectReasonIDAlt,SpecimenRejectReasonTextAlt,SpecimenRejectReasonCodeSystemNameAlt,SpecimenRejectReasonTextOriginal) VALUES(";

        command +=
            SOut.Long(ehrLabSpecimenRejectReason.EhrLabSpecimenNum) + ","
                                                                    + "'" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonID) + "',"
                                                                    + "'" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonText) + "',"
                                                                    + "'" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemName) + "',"
                                                                    + "'" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonIDAlt) + "',"
                                                                    + "'" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonTextAlt) + "',"
                                                                    + "'" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemNameAlt) + "',"
                                                                    + "'" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonTextOriginal) + "')";
        {
            ehrLabSpecimenRejectReason.EhrLabSpecimenRejectReasonNum = Db.NonQ(command, true, "EhrLabSpecimenRejectReasonNum", "ehrLabSpecimenRejectReason");
        }
        return ehrLabSpecimenRejectReason.EhrLabSpecimenRejectReasonNum;
    }

    public static long InsertNoCache(EhrLabSpecimenRejectReason ehrLabSpecimenRejectReason)
    {
        return InsertNoCache(ehrLabSpecimenRejectReason, false);
    }

    public static long InsertNoCache(EhrLabSpecimenRejectReason ehrLabSpecimenRejectReason, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrlabspecimenrejectreason (";
        if (isRandomKeys || useExistingPK) command += "EhrLabSpecimenRejectReasonNum,";
        command += "EhrLabSpecimenNum,SpecimenRejectReasonID,SpecimenRejectReasonText,SpecimenRejectReasonCodeSystemName,SpecimenRejectReasonIDAlt,SpecimenRejectReasonTextAlt,SpecimenRejectReasonCodeSystemNameAlt,SpecimenRejectReasonTextOriginal) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrLabSpecimenRejectReason.EhrLabSpecimenRejectReasonNum) + ",";
        command +=
            SOut.Long(ehrLabSpecimenRejectReason.EhrLabSpecimenNum) + ","
                                                                    + "'" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonID) + "',"
                                                                    + "'" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonText) + "',"
                                                                    + "'" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemName) + "',"
                                                                    + "'" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonIDAlt) + "',"
                                                                    + "'" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonTextAlt) + "',"
                                                                    + "'" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemNameAlt) + "',"
                                                                    + "'" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonTextOriginal) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            ehrLabSpecimenRejectReason.EhrLabSpecimenRejectReasonNum = Db.NonQ(command, true, "EhrLabSpecimenRejectReasonNum", "ehrLabSpecimenRejectReason");
        return ehrLabSpecimenRejectReason.EhrLabSpecimenRejectReasonNum;
    }

    public static void Update(EhrLabSpecimenRejectReason ehrLabSpecimenRejectReason)
    {
        var command = "UPDATE ehrlabspecimenrejectreason SET "
                      + "EhrLabSpecimenNum                    =  " + SOut.Long(ehrLabSpecimenRejectReason.EhrLabSpecimenNum) + ", "
                      + "SpecimenRejectReasonID               = '" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonID) + "', "
                      + "SpecimenRejectReasonText             = '" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonText) + "', "
                      + "SpecimenRejectReasonCodeSystemName   = '" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemName) + "', "
                      + "SpecimenRejectReasonIDAlt            = '" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonIDAlt) + "', "
                      + "SpecimenRejectReasonTextAlt          = '" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonTextAlt) + "', "
                      + "SpecimenRejectReasonCodeSystemNameAlt= '" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemNameAlt) + "', "
                      + "SpecimenRejectReasonTextOriginal     = '" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonTextOriginal) + "' "
                      + "WHERE EhrLabSpecimenRejectReasonNum = " + SOut.Long(ehrLabSpecimenRejectReason.EhrLabSpecimenRejectReasonNum);
        Db.NonQ(command);
    }

    public static bool Update(EhrLabSpecimenRejectReason ehrLabSpecimenRejectReason, EhrLabSpecimenRejectReason oldEhrLabSpecimenRejectReason)
    {
        var command = "";
        if (ehrLabSpecimenRejectReason.EhrLabSpecimenNum != oldEhrLabSpecimenRejectReason.EhrLabSpecimenNum)
        {
            if (command != "") command += ",";
            command += "EhrLabSpecimenNum = " + SOut.Long(ehrLabSpecimenRejectReason.EhrLabSpecimenNum) + "";
        }

        if (ehrLabSpecimenRejectReason.SpecimenRejectReasonID != oldEhrLabSpecimenRejectReason.SpecimenRejectReasonID)
        {
            if (command != "") command += ",";
            command += "SpecimenRejectReasonID = '" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonID) + "'";
        }

        if (ehrLabSpecimenRejectReason.SpecimenRejectReasonText != oldEhrLabSpecimenRejectReason.SpecimenRejectReasonText)
        {
            if (command != "") command += ",";
            command += "SpecimenRejectReasonText = '" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonText) + "'";
        }

        if (ehrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemName != oldEhrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemName)
        {
            if (command != "") command += ",";
            command += "SpecimenRejectReasonCodeSystemName = '" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemName) + "'";
        }

        if (ehrLabSpecimenRejectReason.SpecimenRejectReasonIDAlt != oldEhrLabSpecimenRejectReason.SpecimenRejectReasonIDAlt)
        {
            if (command != "") command += ",";
            command += "SpecimenRejectReasonIDAlt = '" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonIDAlt) + "'";
        }

        if (ehrLabSpecimenRejectReason.SpecimenRejectReasonTextAlt != oldEhrLabSpecimenRejectReason.SpecimenRejectReasonTextAlt)
        {
            if (command != "") command += ",";
            command += "SpecimenRejectReasonTextAlt = '" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonTextAlt) + "'";
        }

        if (ehrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemNameAlt != oldEhrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemNameAlt)
        {
            if (command != "") command += ",";
            command += "SpecimenRejectReasonCodeSystemNameAlt = '" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemNameAlt) + "'";
        }

        if (ehrLabSpecimenRejectReason.SpecimenRejectReasonTextOriginal != oldEhrLabSpecimenRejectReason.SpecimenRejectReasonTextOriginal)
        {
            if (command != "") command += ",";
            command += "SpecimenRejectReasonTextOriginal = '" + SOut.String(ehrLabSpecimenRejectReason.SpecimenRejectReasonTextOriginal) + "'";
        }

        if (command == "") return false;
        command = "UPDATE ehrlabspecimenrejectreason SET " + command
                                                           + " WHERE EhrLabSpecimenRejectReasonNum = " + SOut.Long(ehrLabSpecimenRejectReason.EhrLabSpecimenRejectReasonNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EhrLabSpecimenRejectReason ehrLabSpecimenRejectReason, EhrLabSpecimenRejectReason oldEhrLabSpecimenRejectReason)
    {
        if (ehrLabSpecimenRejectReason.EhrLabSpecimenNum != oldEhrLabSpecimenRejectReason.EhrLabSpecimenNum) return true;
        if (ehrLabSpecimenRejectReason.SpecimenRejectReasonID != oldEhrLabSpecimenRejectReason.SpecimenRejectReasonID) return true;
        if (ehrLabSpecimenRejectReason.SpecimenRejectReasonText != oldEhrLabSpecimenRejectReason.SpecimenRejectReasonText) return true;
        if (ehrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemName != oldEhrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemName) return true;
        if (ehrLabSpecimenRejectReason.SpecimenRejectReasonIDAlt != oldEhrLabSpecimenRejectReason.SpecimenRejectReasonIDAlt) return true;
        if (ehrLabSpecimenRejectReason.SpecimenRejectReasonTextAlt != oldEhrLabSpecimenRejectReason.SpecimenRejectReasonTextAlt) return true;
        if (ehrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemNameAlt != oldEhrLabSpecimenRejectReason.SpecimenRejectReasonCodeSystemNameAlt) return true;
        if (ehrLabSpecimenRejectReason.SpecimenRejectReasonTextOriginal != oldEhrLabSpecimenRejectReason.SpecimenRejectReasonTextOriginal) return true;
        return false;
    }

    public static void Delete(long ehrLabSpecimenRejectReasonNum)
    {
        var command = "DELETE FROM ehrlabspecimenrejectreason "
                      + "WHERE EhrLabSpecimenRejectReasonNum = " + SOut.Long(ehrLabSpecimenRejectReasonNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrLabSpecimenRejectReasonNums)
    {
        if (listEhrLabSpecimenRejectReasonNums == null || listEhrLabSpecimenRejectReasonNums.Count == 0) return;
        var command = "DELETE FROM ehrlabspecimenrejectreason "
                      + "WHERE EhrLabSpecimenRejectReasonNum IN(" + string.Join(",", listEhrLabSpecimenRejectReasonNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}