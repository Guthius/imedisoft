#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class LanguagePatCrud
{
    public static LanguagePat SelectOne(long languagePatNum)
    {
        var command = "SELECT * FROM languagepat "
                      + "WHERE LanguagePatNum = " + SOut.Long(languagePatNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static LanguagePat SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<LanguagePat> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<LanguagePat> TableToList(DataTable table)
    {
        var retVal = new List<LanguagePat>();
        LanguagePat languagePat;
        foreach (DataRow row in table.Rows)
        {
            languagePat = new LanguagePat();
            languagePat.LanguagePatNum = SIn.Long(row["LanguagePatNum"].ToString());
            languagePat.PrefName = SIn.String(row["PrefName"].ToString());
            languagePat.Language = SIn.String(row["Language"].ToString());
            languagePat.Translation = SIn.String(row["Translation"].ToString());
            languagePat.EFormFieldDefNum = SIn.Long(row["EFormFieldDefNum"].ToString());
            retVal.Add(languagePat);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<LanguagePat> listLanguagePats, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "LanguagePat";
        var table = new DataTable(tableName);
        table.Columns.Add("LanguagePatNum");
        table.Columns.Add("PrefName");
        table.Columns.Add("Language");
        table.Columns.Add("Translation");
        table.Columns.Add("EFormFieldDefNum");
        foreach (var languagePat in listLanguagePats)
            table.Rows.Add(SOut.Long(languagePat.LanguagePatNum), languagePat.PrefName, languagePat.Language, languagePat.Translation, SOut.Long(languagePat.EFormFieldDefNum));
        return table;
    }

    public static long Insert(LanguagePat languagePat)
    {
        return Insert(languagePat, false);
    }

    public static long Insert(LanguagePat languagePat, bool useExistingPK)
    {
        var command = "INSERT INTO languagepat (";

        command += "PrefName,Language,Translation,EFormFieldDefNum) VALUES(";

        command +=
            "'" + SOut.String(languagePat.PrefName) + "',"
            + "'" + SOut.String(languagePat.Language) + "',"
            + DbHelper.ParamChar + "paramTranslation,"
            + SOut.Long(languagePat.EFormFieldDefNum) + ")";
        if (languagePat.Translation == null) languagePat.Translation = "";
        var paramTranslation = new OdSqlParameter("paramTranslation", OdDbType.Text, SOut.StringParam(languagePat.Translation));
        {
            languagePat.LanguagePatNum = Db.NonQ(command, true, "LanguagePatNum", "languagePat", paramTranslation);
        }
        return languagePat.LanguagePatNum;
    }

    public static long InsertNoCache(LanguagePat languagePat)
    {
        return InsertNoCache(languagePat, false);
    }

    public static long InsertNoCache(LanguagePat languagePat, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO languagepat (";
        if (isRandomKeys || useExistingPK) command += "LanguagePatNum,";
        command += "PrefName,Language,Translation,EFormFieldDefNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(languagePat.LanguagePatNum) + ",";
        command +=
            "'" + SOut.String(languagePat.PrefName) + "',"
            + "'" + SOut.String(languagePat.Language) + "',"
            + DbHelper.ParamChar + "paramTranslation,"
            + SOut.Long(languagePat.EFormFieldDefNum) + ")";
        if (languagePat.Translation == null) languagePat.Translation = "";
        var paramTranslation = new OdSqlParameter("paramTranslation", OdDbType.Text, SOut.StringParam(languagePat.Translation));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramTranslation);
        else
            languagePat.LanguagePatNum = Db.NonQ(command, true, "LanguagePatNum", "languagePat", paramTranslation);
        return languagePat.LanguagePatNum;
    }

    public static void Update(LanguagePat languagePat)
    {
        var command = "UPDATE languagepat SET "
                      + "PrefName        = '" + SOut.String(languagePat.PrefName) + "', "
                      + "Language        = '" + SOut.String(languagePat.Language) + "', "
                      + "Translation     =  " + DbHelper.ParamChar + "paramTranslation, "
                      + "EFormFieldDefNum=  " + SOut.Long(languagePat.EFormFieldDefNum) + " "
                      + "WHERE LanguagePatNum = " + SOut.Long(languagePat.LanguagePatNum);
        if (languagePat.Translation == null) languagePat.Translation = "";
        var paramTranslation = new OdSqlParameter("paramTranslation", OdDbType.Text, SOut.StringParam(languagePat.Translation));
        Db.NonQ(command, paramTranslation);
    }

    public static bool Update(LanguagePat languagePat, LanguagePat oldLanguagePat)
    {
        var command = "";
        if (languagePat.PrefName != oldLanguagePat.PrefName)
        {
            if (command != "") command += ",";
            command += "PrefName = '" + SOut.String(languagePat.PrefName) + "'";
        }

        if (languagePat.Language != oldLanguagePat.Language)
        {
            if (command != "") command += ",";
            command += "Language = '" + SOut.String(languagePat.Language) + "'";
        }

        if (languagePat.Translation != oldLanguagePat.Translation)
        {
            if (command != "") command += ",";
            command += "Translation = " + DbHelper.ParamChar + "paramTranslation";
        }

        if (languagePat.EFormFieldDefNum != oldLanguagePat.EFormFieldDefNum)
        {
            if (command != "") command += ",";
            command += "EFormFieldDefNum = " + SOut.Long(languagePat.EFormFieldDefNum) + "";
        }

        if (command == "") return false;
        if (languagePat.Translation == null) languagePat.Translation = "";
        var paramTranslation = new OdSqlParameter("paramTranslation", OdDbType.Text, SOut.StringParam(languagePat.Translation));
        command = "UPDATE languagepat SET " + command
                                            + " WHERE LanguagePatNum = " + SOut.Long(languagePat.LanguagePatNum);
        Db.NonQ(command, paramTranslation);
        return true;
    }

    public static bool UpdateComparison(LanguagePat languagePat, LanguagePat oldLanguagePat)
    {
        if (languagePat.PrefName != oldLanguagePat.PrefName) return true;
        if (languagePat.Language != oldLanguagePat.Language) return true;
        if (languagePat.Translation != oldLanguagePat.Translation) return true;
        if (languagePat.EFormFieldDefNum != oldLanguagePat.EFormFieldDefNum) return true;
        return false;
    }

    public static void Delete(long languagePatNum)
    {
        var command = "DELETE FROM languagepat "
                      + "WHERE LanguagePatNum = " + SOut.Long(languagePatNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listLanguagePatNums)
    {
        if (listLanguagePatNums == null || listLanguagePatNums.Count == 0) return;
        var command = "DELETE FROM languagepat "
                      + "WHERE LanguagePatNum IN(" + string.Join(",", listLanguagePatNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}