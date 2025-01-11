#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class LanguageForeignCrud
{
    public static LanguageForeign SelectOne(long languageForeignNum)
    {
        var command = "SELECT * FROM languageforeign "
                      + "WHERE LanguageForeignNum = " + SOut.Long(languageForeignNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static LanguageForeign SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<LanguageForeign> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<LanguageForeign> TableToList(DataTable table)
    {
        var retVal = new List<LanguageForeign>();
        LanguageForeign languageForeign;
        foreach (DataRow row in table.Rows)
        {
            languageForeign = new LanguageForeign();
            languageForeign.LanguageForeignNum = SIn.Long(row["LanguageForeignNum"].ToString());
            languageForeign.ClassType = SIn.String(row["ClassType"].ToString());
            languageForeign.English = SIn.String(row["English"].ToString());
            languageForeign.Culture = SIn.String(row["Culture"].ToString());
            languageForeign.Translation = SIn.String(row["Translation"].ToString());
            languageForeign.Comments = SIn.String(row["Comments"].ToString());
            retVal.Add(languageForeign);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<LanguageForeign> listLanguageForeigns, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "LanguageForeign";
        var table = new DataTable(tableName);
        table.Columns.Add("LanguageForeignNum");
        table.Columns.Add("ClassType");
        table.Columns.Add("English");
        table.Columns.Add("Culture");
        table.Columns.Add("Translation");
        table.Columns.Add("Comments");
        foreach (var languageForeign in listLanguageForeigns)
            table.Rows.Add(SOut.Long(languageForeign.LanguageForeignNum), languageForeign.ClassType, languageForeign.English, languageForeign.Culture, languageForeign.Translation, languageForeign.Comments);
        return table;
    }

    public static long Insert(LanguageForeign languageForeign)
    {
        return Insert(languageForeign, false);
    }

    public static long Insert(LanguageForeign languageForeign, bool useExistingPK)
    {
        var command = "INSERT INTO languageforeign (";

        command += "ClassType,English,Culture,Translation,Comments) VALUES(";

        command +=
            DbHelper.ParamChar + "paramClassType,"
                               + DbHelper.ParamChar + "paramEnglish,"
                               + "'" + SOut.String(languageForeign.Culture) + "',"
                               + DbHelper.ParamChar + "paramTranslation,"
                               + DbHelper.ParamChar + "paramComments)";
        if (languageForeign.ClassType == null) languageForeign.ClassType = "";
        var paramClassType = new OdSqlParameter("paramClassType", OdDbType.Text, SOut.StringParam(languageForeign.ClassType));
        if (languageForeign.English == null) languageForeign.English = "";
        var paramEnglish = new OdSqlParameter("paramEnglish", OdDbType.Text, SOut.StringParam(languageForeign.English));
        if (languageForeign.Translation == null) languageForeign.Translation = "";
        var paramTranslation = new OdSqlParameter("paramTranslation", OdDbType.Text, SOut.StringParam(languageForeign.Translation));
        if (languageForeign.Comments == null) languageForeign.Comments = "";
        var paramComments = new OdSqlParameter("paramComments", OdDbType.Text, SOut.StringParam(languageForeign.Comments));
        {
            languageForeign.LanguageForeignNum = Db.NonQ(command, true, "LanguageForeignNum", "languageForeign", paramClassType, paramEnglish, paramTranslation, paramComments);
        }
        return languageForeign.LanguageForeignNum;
    }

    public static long InsertNoCache(LanguageForeign languageForeign)
    {
        return InsertNoCache(languageForeign, false);
    }

    public static long InsertNoCache(LanguageForeign languageForeign, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO languageforeign (";
        if (isRandomKeys || useExistingPK) command += "LanguageForeignNum,";
        command += "ClassType,English,Culture,Translation,Comments) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(languageForeign.LanguageForeignNum) + ",";
        command +=
            DbHelper.ParamChar + "paramClassType,"
                               + DbHelper.ParamChar + "paramEnglish,"
                               + "'" + SOut.String(languageForeign.Culture) + "',"
                               + DbHelper.ParamChar + "paramTranslation,"
                               + DbHelper.ParamChar + "paramComments)";
        if (languageForeign.ClassType == null) languageForeign.ClassType = "";
        var paramClassType = new OdSqlParameter("paramClassType", OdDbType.Text, SOut.StringParam(languageForeign.ClassType));
        if (languageForeign.English == null) languageForeign.English = "";
        var paramEnglish = new OdSqlParameter("paramEnglish", OdDbType.Text, SOut.StringParam(languageForeign.English));
        if (languageForeign.Translation == null) languageForeign.Translation = "";
        var paramTranslation = new OdSqlParameter("paramTranslation", OdDbType.Text, SOut.StringParam(languageForeign.Translation));
        if (languageForeign.Comments == null) languageForeign.Comments = "";
        var paramComments = new OdSqlParameter("paramComments", OdDbType.Text, SOut.StringParam(languageForeign.Comments));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramClassType, paramEnglish, paramTranslation, paramComments);
        else
            languageForeign.LanguageForeignNum = Db.NonQ(command, true, "LanguageForeignNum", "languageForeign", paramClassType, paramEnglish, paramTranslation, paramComments);
        return languageForeign.LanguageForeignNum;
    }

    public static void Update(LanguageForeign languageForeign)
    {
        var command = "UPDATE languageforeign SET "
                      + "ClassType         =  " + DbHelper.ParamChar + "paramClassType, "
                      + "English           =  " + DbHelper.ParamChar + "paramEnglish, "
                      + "Culture           = '" + SOut.String(languageForeign.Culture) + "', "
                      + "Translation       =  " + DbHelper.ParamChar + "paramTranslation, "
                      + "Comments          =  " + DbHelper.ParamChar + "paramComments "
                      + "WHERE LanguageForeignNum = " + SOut.Long(languageForeign.LanguageForeignNum);
        if (languageForeign.ClassType == null) languageForeign.ClassType = "";
        var paramClassType = new OdSqlParameter("paramClassType", OdDbType.Text, SOut.StringParam(languageForeign.ClassType));
        if (languageForeign.English == null) languageForeign.English = "";
        var paramEnglish = new OdSqlParameter("paramEnglish", OdDbType.Text, SOut.StringParam(languageForeign.English));
        if (languageForeign.Translation == null) languageForeign.Translation = "";
        var paramTranslation = new OdSqlParameter("paramTranslation", OdDbType.Text, SOut.StringParam(languageForeign.Translation));
        if (languageForeign.Comments == null) languageForeign.Comments = "";
        var paramComments = new OdSqlParameter("paramComments", OdDbType.Text, SOut.StringParam(languageForeign.Comments));
        Db.NonQ(command, paramClassType, paramEnglish, paramTranslation, paramComments);
    }

    public static bool Update(LanguageForeign languageForeign, LanguageForeign oldLanguageForeign)
    {
        var command = "";
        if (languageForeign.ClassType != oldLanguageForeign.ClassType)
        {
            if (command != "") command += ",";
            command += "ClassType = " + DbHelper.ParamChar + "paramClassType";
        }

        if (languageForeign.English != oldLanguageForeign.English)
        {
            if (command != "") command += ",";
            command += "English = " + DbHelper.ParamChar + "paramEnglish";
        }

        if (languageForeign.Culture != oldLanguageForeign.Culture)
        {
            if (command != "") command += ",";
            command += "Culture = '" + SOut.String(languageForeign.Culture) + "'";
        }

        if (languageForeign.Translation != oldLanguageForeign.Translation)
        {
            if (command != "") command += ",";
            command += "Translation = " + DbHelper.ParamChar + "paramTranslation";
        }

        if (languageForeign.Comments != oldLanguageForeign.Comments)
        {
            if (command != "") command += ",";
            command += "Comments = " + DbHelper.ParamChar + "paramComments";
        }

        if (command == "") return false;
        if (languageForeign.ClassType == null) languageForeign.ClassType = "";
        var paramClassType = new OdSqlParameter("paramClassType", OdDbType.Text, SOut.StringParam(languageForeign.ClassType));
        if (languageForeign.English == null) languageForeign.English = "";
        var paramEnglish = new OdSqlParameter("paramEnglish", OdDbType.Text, SOut.StringParam(languageForeign.English));
        if (languageForeign.Translation == null) languageForeign.Translation = "";
        var paramTranslation = new OdSqlParameter("paramTranslation", OdDbType.Text, SOut.StringParam(languageForeign.Translation));
        if (languageForeign.Comments == null) languageForeign.Comments = "";
        var paramComments = new OdSqlParameter("paramComments", OdDbType.Text, SOut.StringParam(languageForeign.Comments));
        command = "UPDATE languageforeign SET " + command
                                                + " WHERE LanguageForeignNum = " + SOut.Long(languageForeign.LanguageForeignNum);
        Db.NonQ(command, paramClassType, paramEnglish, paramTranslation, paramComments);
        return true;
    }

    public static bool UpdateComparison(LanguageForeign languageForeign, LanguageForeign oldLanguageForeign)
    {
        if (languageForeign.ClassType != oldLanguageForeign.ClassType) return true;
        if (languageForeign.English != oldLanguageForeign.English) return true;
        if (languageForeign.Culture != oldLanguageForeign.Culture) return true;
        if (languageForeign.Translation != oldLanguageForeign.Translation) return true;
        if (languageForeign.Comments != oldLanguageForeign.Comments) return true;
        return false;
    }

    public static void Delete(long languageForeignNum)
    {
        var command = "DELETE FROM languageforeign "
                      + "WHERE LanguageForeignNum = " + SOut.Long(languageForeignNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listLanguageForeignNums)
    {
        if (listLanguageForeignNums == null || listLanguageForeignNums.Count == 0) return;
        var command = "DELETE FROM languageforeign "
                      + "WHERE LanguageForeignNum IN(" + string.Join(",", listLanguageForeignNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}