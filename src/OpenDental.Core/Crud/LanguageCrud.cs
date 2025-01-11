#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class LanguageCrud
{
    public static Language SelectOne(long languageNum)
    {
        var command = "SELECT * FROM language "
                      + "WHERE LanguageNum = " + SOut.Long(languageNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Language SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Language> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Language> TableToList(DataTable table)
    {
        var retVal = new List<Language>();
        Language language;
        foreach (DataRow row in table.Rows)
        {
            language = new Language();
            language.LanguageNum = SIn.Long(row["LanguageNum"].ToString());
            language.EnglishComments = SIn.String(row["EnglishComments"].ToString());
            language.ClassType = SIn.String(row["ClassType"].ToString());
            language.English = SIn.String(row["English"].ToString());
            language.IsObsolete = SIn.Bool(row["IsObsolete"].ToString());
            retVal.Add(language);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Language> listLanguages, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Language";
        var table = new DataTable(tableName);
        table.Columns.Add("LanguageNum");
        table.Columns.Add("EnglishComments");
        table.Columns.Add("ClassType");
        table.Columns.Add("English");
        table.Columns.Add("IsObsolete");
        foreach (var language in listLanguages)
            table.Rows.Add(SOut.Long(language.LanguageNum), language.EnglishComments, language.ClassType, language.English, SOut.Bool(language.IsObsolete));
        return table;
    }

    public static long Insert(Language language)
    {
        return Insert(language, false);
    }

    public static long Insert(Language language, bool useExistingPK)
    {
        var command = "INSERT INTO language (";

        command += "EnglishComments,ClassType,English,IsObsolete) VALUES(";

        command +=
            DbHelper.ParamChar + "paramEnglishComments,"
                               + DbHelper.ParamChar + "paramClassType,"
                               + DbHelper.ParamChar + "paramEnglish,"
                               + SOut.Bool(language.IsObsolete) + ")";
        if (language.EnglishComments == null) language.EnglishComments = "";
        var paramEnglishComments = new OdSqlParameter("paramEnglishComments", OdDbType.Text, SOut.StringParam(language.EnglishComments));
        if (language.ClassType == null) language.ClassType = "";
        var paramClassType = new OdSqlParameter("paramClassType", OdDbType.Text, SOut.StringParam(language.ClassType));
        if (language.English == null) language.English = "";
        var paramEnglish = new OdSqlParameter("paramEnglish", OdDbType.Text, SOut.StringParam(language.English));
        {
            language.LanguageNum = Db.NonQ(command, true, "LanguageNum", "language", paramEnglishComments, paramClassType, paramEnglish);
        }
        return language.LanguageNum;
    }

    public static long InsertNoCache(Language language)
    {
        return InsertNoCache(language, false);
    }

    public static long InsertNoCache(Language language, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO language (";
        if (isRandomKeys || useExistingPK) command += "LanguageNum,";
        command += "EnglishComments,ClassType,English,IsObsolete) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(language.LanguageNum) + ",";
        command +=
            DbHelper.ParamChar + "paramEnglishComments,"
                               + DbHelper.ParamChar + "paramClassType,"
                               + DbHelper.ParamChar + "paramEnglish,"
                               + SOut.Bool(language.IsObsolete) + ")";
        if (language.EnglishComments == null) language.EnglishComments = "";
        var paramEnglishComments = new OdSqlParameter("paramEnglishComments", OdDbType.Text, SOut.StringParam(language.EnglishComments));
        if (language.ClassType == null) language.ClassType = "";
        var paramClassType = new OdSqlParameter("paramClassType", OdDbType.Text, SOut.StringParam(language.ClassType));
        if (language.English == null) language.English = "";
        var paramEnglish = new OdSqlParameter("paramEnglish", OdDbType.Text, SOut.StringParam(language.English));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramEnglishComments, paramClassType, paramEnglish);
        else
            language.LanguageNum = Db.NonQ(command, true, "LanguageNum", "language", paramEnglishComments, paramClassType, paramEnglish);
        return language.LanguageNum;
    }

    public static void Update(Language language)
    {
        var command = "UPDATE language SET "
                      + "EnglishComments=  " + DbHelper.ParamChar + "paramEnglishComments, "
                      + "ClassType      =  " + DbHelper.ParamChar + "paramClassType, "
                      + "English        =  " + DbHelper.ParamChar + "paramEnglish, "
                      + "IsObsolete     =  " + SOut.Bool(language.IsObsolete) + " "
                      + "WHERE LanguageNum = " + SOut.Long(language.LanguageNum);
        if (language.EnglishComments == null) language.EnglishComments = "";
        var paramEnglishComments = new OdSqlParameter("paramEnglishComments", OdDbType.Text, SOut.StringParam(language.EnglishComments));
        if (language.ClassType == null) language.ClassType = "";
        var paramClassType = new OdSqlParameter("paramClassType", OdDbType.Text, SOut.StringParam(language.ClassType));
        if (language.English == null) language.English = "";
        var paramEnglish = new OdSqlParameter("paramEnglish", OdDbType.Text, SOut.StringParam(language.English));
        Db.NonQ(command, paramEnglishComments, paramClassType, paramEnglish);
    }

    public static bool Update(Language language, Language oldLanguage)
    {
        var command = "";
        if (language.EnglishComments != oldLanguage.EnglishComments)
        {
            if (command != "") command += ",";
            command += "EnglishComments = " + DbHelper.ParamChar + "paramEnglishComments";
        }

        if (language.ClassType != oldLanguage.ClassType)
        {
            if (command != "") command += ",";
            command += "ClassType = " + DbHelper.ParamChar + "paramClassType";
        }

        if (language.English != oldLanguage.English)
        {
            if (command != "") command += ",";
            command += "English = " + DbHelper.ParamChar + "paramEnglish";
        }

        if (language.IsObsolete != oldLanguage.IsObsolete)
        {
            if (command != "") command += ",";
            command += "IsObsolete = " + SOut.Bool(language.IsObsolete) + "";
        }

        if (command == "") return false;
        if (language.EnglishComments == null) language.EnglishComments = "";
        var paramEnglishComments = new OdSqlParameter("paramEnglishComments", OdDbType.Text, SOut.StringParam(language.EnglishComments));
        if (language.ClassType == null) language.ClassType = "";
        var paramClassType = new OdSqlParameter("paramClassType", OdDbType.Text, SOut.StringParam(language.ClassType));
        if (language.English == null) language.English = "";
        var paramEnglish = new OdSqlParameter("paramEnglish", OdDbType.Text, SOut.StringParam(language.English));
        command = "UPDATE language SET " + command
                                         + " WHERE LanguageNum = " + SOut.Long(language.LanguageNum);
        Db.NonQ(command, paramEnglishComments, paramClassType, paramEnglish);
        return true;
    }

    public static bool UpdateComparison(Language language, Language oldLanguage)
    {
        if (language.EnglishComments != oldLanguage.EnglishComments) return true;
        if (language.ClassType != oldLanguage.ClassType) return true;
        if (language.English != oldLanguage.English) return true;
        if (language.IsObsolete != oldLanguage.IsObsolete) return true;
        return false;
    }

    public static void Delete(long languageNum)
    {
        var command = "DELETE FROM language "
                      + "WHERE LanguageNum = " + SOut.Long(languageNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listLanguageNums)
    {
        if (listLanguageNums == null || listLanguageNums.Count == 0) return;
        var command = "DELETE FROM language "
                      + "WHERE LanguageNum IN(" + string.Join(",", listLanguageNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}