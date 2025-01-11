#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrLabNoteCrud
{
    public static EhrLabNote SelectOne(long ehrLabNoteNum)
    {
        var command = "SELECT * FROM ehrlabnote "
                      + "WHERE EhrLabNoteNum = " + SOut.Long(ehrLabNoteNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrLabNote SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrLabNote> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrLabNote> TableToList(DataTable table)
    {
        var retVal = new List<EhrLabNote>();
        EhrLabNote ehrLabNote;
        foreach (DataRow row in table.Rows)
        {
            ehrLabNote = new EhrLabNote();
            ehrLabNote.EhrLabNoteNum = SIn.Long(row["EhrLabNoteNum"].ToString());
            ehrLabNote.EhrLabNum = SIn.Long(row["EhrLabNum"].ToString());
            ehrLabNote.EhrLabResultNum = SIn.Long(row["EhrLabResultNum"].ToString());
            ehrLabNote.Comments = SIn.String(row["Comments"].ToString());
            retVal.Add(ehrLabNote);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrLabNote> listEhrLabNotes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrLabNote";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrLabNoteNum");
        table.Columns.Add("EhrLabNum");
        table.Columns.Add("EhrLabResultNum");
        table.Columns.Add("Comments");
        foreach (var ehrLabNote in listEhrLabNotes)
            table.Rows.Add(SOut.Long(ehrLabNote.EhrLabNoteNum), SOut.Long(ehrLabNote.EhrLabNum), SOut.Long(ehrLabNote.EhrLabResultNum), ehrLabNote.Comments);
        return table;
    }

    public static long Insert(EhrLabNote ehrLabNote)
    {
        return Insert(ehrLabNote, false);
    }

    public static long Insert(EhrLabNote ehrLabNote, bool useExistingPK)
    {
        var command = "INSERT INTO ehrlabnote (";

        command += "EhrLabNum,EhrLabResultNum,Comments) VALUES(";

        command +=
            SOut.Long(ehrLabNote.EhrLabNum) + ","
                                            + SOut.Long(ehrLabNote.EhrLabResultNum) + ","
                                            + DbHelper.ParamChar + "paramComments)";
        if (ehrLabNote.Comments == null) ehrLabNote.Comments = "";
        var paramComments = new OdSqlParameter("paramComments", OdDbType.Text, SOut.StringParam(ehrLabNote.Comments));
        {
            ehrLabNote.EhrLabNoteNum = Db.NonQ(command, true, "EhrLabNoteNum", "ehrLabNote", paramComments);
        }
        return ehrLabNote.EhrLabNoteNum;
    }

    public static long InsertNoCache(EhrLabNote ehrLabNote)
    {
        return InsertNoCache(ehrLabNote, false);
    }

    public static long InsertNoCache(EhrLabNote ehrLabNote, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrlabnote (";
        if (isRandomKeys || useExistingPK) command += "EhrLabNoteNum,";
        command += "EhrLabNum,EhrLabResultNum,Comments) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrLabNote.EhrLabNoteNum) + ",";
        command +=
            SOut.Long(ehrLabNote.EhrLabNum) + ","
                                            + SOut.Long(ehrLabNote.EhrLabResultNum) + ","
                                            + DbHelper.ParamChar + "paramComments)";
        if (ehrLabNote.Comments == null) ehrLabNote.Comments = "";
        var paramComments = new OdSqlParameter("paramComments", OdDbType.Text, SOut.StringParam(ehrLabNote.Comments));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramComments);
        else
            ehrLabNote.EhrLabNoteNum = Db.NonQ(command, true, "EhrLabNoteNum", "ehrLabNote", paramComments);
        return ehrLabNote.EhrLabNoteNum;
    }

    public static void Update(EhrLabNote ehrLabNote)
    {
        var command = "UPDATE ehrlabnote SET "
                      + "EhrLabNum      =  " + SOut.Long(ehrLabNote.EhrLabNum) + ", "
                      + "EhrLabResultNum=  " + SOut.Long(ehrLabNote.EhrLabResultNum) + ", "
                      + "Comments       =  " + DbHelper.ParamChar + "paramComments "
                      + "WHERE EhrLabNoteNum = " + SOut.Long(ehrLabNote.EhrLabNoteNum);
        if (ehrLabNote.Comments == null) ehrLabNote.Comments = "";
        var paramComments = new OdSqlParameter("paramComments", OdDbType.Text, SOut.StringParam(ehrLabNote.Comments));
        Db.NonQ(command, paramComments);
    }

    public static bool Update(EhrLabNote ehrLabNote, EhrLabNote oldEhrLabNote)
    {
        var command = "";
        if (ehrLabNote.EhrLabNum != oldEhrLabNote.EhrLabNum)
        {
            if (command != "") command += ",";
            command += "EhrLabNum = " + SOut.Long(ehrLabNote.EhrLabNum) + "";
        }

        if (ehrLabNote.EhrLabResultNum != oldEhrLabNote.EhrLabResultNum)
        {
            if (command != "") command += ",";
            command += "EhrLabResultNum = " + SOut.Long(ehrLabNote.EhrLabResultNum) + "";
        }

        if (ehrLabNote.Comments != oldEhrLabNote.Comments)
        {
            if (command != "") command += ",";
            command += "Comments = " + DbHelper.ParamChar + "paramComments";
        }

        if (command == "") return false;
        if (ehrLabNote.Comments == null) ehrLabNote.Comments = "";
        var paramComments = new OdSqlParameter("paramComments", OdDbType.Text, SOut.StringParam(ehrLabNote.Comments));
        command = "UPDATE ehrlabnote SET " + command
                                           + " WHERE EhrLabNoteNum = " + SOut.Long(ehrLabNote.EhrLabNoteNum);
        Db.NonQ(command, paramComments);
        return true;
    }

    public static bool UpdateComparison(EhrLabNote ehrLabNote, EhrLabNote oldEhrLabNote)
    {
        if (ehrLabNote.EhrLabNum != oldEhrLabNote.EhrLabNum) return true;
        if (ehrLabNote.EhrLabResultNum != oldEhrLabNote.EhrLabResultNum) return true;
        if (ehrLabNote.Comments != oldEhrLabNote.Comments) return true;
        return false;
    }

    public static void Delete(long ehrLabNoteNum)
    {
        var command = "DELETE FROM ehrlabnote "
                      + "WHERE EhrLabNoteNum = " + SOut.Long(ehrLabNoteNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrLabNoteNums)
    {
        if (listEhrLabNoteNums == null || listEhrLabNoteNums.Count == 0) return;
        var command = "DELETE FROM ehrlabnote "
                      + "WHERE EhrLabNoteNum IN(" + string.Join(",", listEhrLabNoteNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}