using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class LanguagePatCrud
{
    public static List<LanguagePat> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<LanguagePat> TableToList(DataTable table)
    {
        var retVal = new List<LanguagePat>();
        foreach (DataRow row in table.Rows)
        {
            var languagePat = new LanguagePat
            {
                LanguagePatNum = SIn.Long(row["LanguagePatNum"].ToString()),
                PrefName = SIn.String(row["PrefName"].ToString()),
                Language = SIn.String(row["Language"].ToString()),
                Translation = SIn.String(row["Translation"].ToString()),
                EFormFieldDefNum = SIn.Long(row["EFormFieldDefNum"].ToString())
            };
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

    public static void Insert(LanguagePat languagePat)
    {
        var command = "INSERT INTO languagepat (";

        command += "PrefName,Language,Translation,EFormFieldDefNum) VALUES(";

        command +=
            "'" + SOut.String(languagePat.PrefName) + "',"
            + "'" + SOut.String(languagePat.Language) + "',"
            + DbHelper.ParamChar + "paramTranslation,"
            + SOut.Long(languagePat.EFormFieldDefNum) + ")";
        if (languagePat.Translation == null) languagePat.Translation = "";
        var paramTranslation = new OdSqlParameter("paramTranslation", SOut.StringParam(languagePat.Translation));
        {
            languagePat.LanguagePatNum = Db.NonQ(command, true, "LanguagePatNum", "languagePat", paramTranslation);
        }
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
        var paramTranslation = new OdSqlParameter("paramTranslation", SOut.StringParam(languagePat.Translation));
        Db.NonQ(command, paramTranslation);
    }

    public static void Update(LanguagePat languagePat, LanguagePat oldLanguagePat)
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

        if (command == "") return;
        if (languagePat.Translation == null) languagePat.Translation = "";
        var paramTranslation = new OdSqlParameter("paramTranslation", SOut.StringParam(languagePat.Translation));
        command = "UPDATE languagepat SET " + command
                                            + " WHERE LanguagePatNum = " + SOut.Long(languagePat.LanguagePatNum);
        Db.NonQ(command, paramTranslation);
    }
}