using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class LanguageCrud
{
    public static List<Language> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Language> TableToList(DataTable table)
    {
        var retVal = new List<Language>();
        foreach (DataRow row in table.Rows)
        {
            var language = new Language
            {
                LanguageNum = SIn.Long(row["LanguageNum"].ToString()),
                EnglishComments = SIn.String(row["EnglishComments"].ToString()),
                ClassType = SIn.String(row["ClassType"].ToString()),
                English = SIn.String(row["English"].ToString()),
                IsObsolete = SIn.Bool(row["IsObsolete"].ToString())
            };
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
}