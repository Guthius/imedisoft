#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class LetterCrud
{
    public static Letter SelectOne(long letterNum)
    {
        var command = "SELECT * FROM letter "
                      + "WHERE LetterNum = " + SOut.Long(letterNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Letter SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Letter> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Letter> TableToList(DataTable table)
    {
        var retVal = new List<Letter>();
        Letter letter;
        foreach (DataRow row in table.Rows)
        {
            letter = new Letter();
            letter.LetterNum = SIn.Long(row["LetterNum"].ToString());
            letter.Description = SIn.String(row["Description"].ToString());
            letter.BodyText = SIn.String(row["BodyText"].ToString());
            retVal.Add(letter);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Letter> listLetters, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Letter";
        var table = new DataTable(tableName);
        table.Columns.Add("LetterNum");
        table.Columns.Add("Description");
        table.Columns.Add("BodyText");
        foreach (var letter in listLetters)
            table.Rows.Add(SOut.Long(letter.LetterNum), letter.Description, letter.BodyText);
        return table;
    }

    public static long Insert(Letter letter)
    {
        return Insert(letter, false);
    }

    public static long Insert(Letter letter, bool useExistingPK)
    {
        var command = "INSERT INTO letter (";

        command += "Description,BodyText) VALUES(";

        command +=
            "'" + SOut.String(letter.Description) + "',"
            + DbHelper.ParamChar + "paramBodyText)";
        if (letter.BodyText == null) letter.BodyText = "";
        var paramBodyText = new OdSqlParameter("paramBodyText", OdDbType.Text, SOut.StringParam(letter.BodyText));
        {
            letter.LetterNum = Db.NonQ(command, true, "LetterNum", "letter", paramBodyText);
        }
        return letter.LetterNum;
    }

    public static long InsertNoCache(Letter letter)
    {
        return InsertNoCache(letter, false);
    }

    public static long InsertNoCache(Letter letter, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO letter (";
        if (isRandomKeys || useExistingPK) command += "LetterNum,";
        command += "Description,BodyText) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(letter.LetterNum) + ",";
        command +=
            "'" + SOut.String(letter.Description) + "',"
            + DbHelper.ParamChar + "paramBodyText)";
        if (letter.BodyText == null) letter.BodyText = "";
        var paramBodyText = new OdSqlParameter("paramBodyText", OdDbType.Text, SOut.StringParam(letter.BodyText));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramBodyText);
        else
            letter.LetterNum = Db.NonQ(command, true, "LetterNum", "letter", paramBodyText);
        return letter.LetterNum;
    }

    public static void Update(Letter letter)
    {
        var command = "UPDATE letter SET "
                      + "Description= '" + SOut.String(letter.Description) + "', "
                      + "BodyText   =  " + DbHelper.ParamChar + "paramBodyText "
                      + "WHERE LetterNum = " + SOut.Long(letter.LetterNum);
        if (letter.BodyText == null) letter.BodyText = "";
        var paramBodyText = new OdSqlParameter("paramBodyText", OdDbType.Text, SOut.StringParam(letter.BodyText));
        Db.NonQ(command, paramBodyText);
    }

    public static bool Update(Letter letter, Letter oldLetter)
    {
        var command = "";
        if (letter.Description != oldLetter.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(letter.Description) + "'";
        }

        if (letter.BodyText != oldLetter.BodyText)
        {
            if (command != "") command += ",";
            command += "BodyText = " + DbHelper.ParamChar + "paramBodyText";
        }

        if (command == "") return false;
        if (letter.BodyText == null) letter.BodyText = "";
        var paramBodyText = new OdSqlParameter("paramBodyText", OdDbType.Text, SOut.StringParam(letter.BodyText));
        command = "UPDATE letter SET " + command
                                       + " WHERE LetterNum = " + SOut.Long(letter.LetterNum);
        Db.NonQ(command, paramBodyText);
        return true;
    }

    public static bool UpdateComparison(Letter letter, Letter oldLetter)
    {
        if (letter.Description != oldLetter.Description) return true;
        if (letter.BodyText != oldLetter.BodyText) return true;
        return false;
    }

    public static void Delete(long letterNum)
    {
        var command = "DELETE FROM letter "
                      + "WHERE LetterNum = " + SOut.Long(letterNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listLetterNums)
    {
        if (listLetterNums == null || listLetterNums.Count == 0) return;
        var command = "DELETE FROM letter "
                      + "WHERE LetterNum IN(" + string.Join(",", listLetterNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}