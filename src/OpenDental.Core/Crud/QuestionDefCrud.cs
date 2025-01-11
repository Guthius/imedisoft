#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class QuestionDefCrud
{
    public static QuestionDef SelectOne(long questionDefNum)
    {
        var command = "SELECT * FROM questiondef "
                      + "WHERE QuestionDefNum = " + SOut.Long(questionDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static QuestionDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<QuestionDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<QuestionDef> TableToList(DataTable table)
    {
        var retVal = new List<QuestionDef>();
        QuestionDef questionDef;
        foreach (DataRow row in table.Rows)
        {
            questionDef = new QuestionDef();
            questionDef.QuestionDefNum = SIn.Long(row["QuestionDefNum"].ToString());
            questionDef.Description = SIn.String(row["Description"].ToString());
            questionDef.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            questionDef.QuestType = (QuestionType) SIn.Int(row["QuestType"].ToString());
            retVal.Add(questionDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<QuestionDef> listQuestionDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "QuestionDef";
        var table = new DataTable(tableName);
        table.Columns.Add("QuestionDefNum");
        table.Columns.Add("Description");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("QuestType");
        foreach (var questionDef in listQuestionDefs)
            table.Rows.Add(SOut.Long(questionDef.QuestionDefNum), questionDef.Description, SOut.Int(questionDef.ItemOrder), SOut.Int((int) questionDef.QuestType));
        return table;
    }

    public static long Insert(QuestionDef questionDef)
    {
        return Insert(questionDef, false);
    }

    public static long Insert(QuestionDef questionDef, bool useExistingPK)
    {
        var command = "INSERT INTO questiondef (";

        command += "Description,ItemOrder,QuestType) VALUES(";

        command +=
            DbHelper.ParamChar + "paramDescription,"
                               + SOut.Int(questionDef.ItemOrder) + ","
                               + SOut.Int((int) questionDef.QuestType) + ")";
        if (questionDef.Description == null) questionDef.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(questionDef.Description));
        {
            questionDef.QuestionDefNum = Db.NonQ(command, true, "QuestionDefNum", "questionDef", paramDescription);
        }
        return questionDef.QuestionDefNum;
    }

    public static long InsertNoCache(QuestionDef questionDef)
    {
        return InsertNoCache(questionDef, false);
    }

    public static long InsertNoCache(QuestionDef questionDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO questiondef (";
        if (isRandomKeys || useExistingPK) command += "QuestionDefNum,";
        command += "Description,ItemOrder,QuestType) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(questionDef.QuestionDefNum) + ",";
        command +=
            DbHelper.ParamChar + "paramDescription,"
                               + SOut.Int(questionDef.ItemOrder) + ","
                               + SOut.Int((int) questionDef.QuestType) + ")";
        if (questionDef.Description == null) questionDef.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(questionDef.Description));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDescription);
        else
            questionDef.QuestionDefNum = Db.NonQ(command, true, "QuestionDefNum", "questionDef", paramDescription);
        return questionDef.QuestionDefNum;
    }

    public static void Update(QuestionDef questionDef)
    {
        var command = "UPDATE questiondef SET "
                      + "Description   =  " + DbHelper.ParamChar + "paramDescription, "
                      + "ItemOrder     =  " + SOut.Int(questionDef.ItemOrder) + ", "
                      + "QuestType     =  " + SOut.Int((int) questionDef.QuestType) + " "
                      + "WHERE QuestionDefNum = " + SOut.Long(questionDef.QuestionDefNum);
        if (questionDef.Description == null) questionDef.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(questionDef.Description));
        Db.NonQ(command, paramDescription);
    }

    public static bool Update(QuestionDef questionDef, QuestionDef oldQuestionDef)
    {
        var command = "";
        if (questionDef.Description != oldQuestionDef.Description)
        {
            if (command != "") command += ",";
            command += "Description = " + DbHelper.ParamChar + "paramDescription";
        }

        if (questionDef.ItemOrder != oldQuestionDef.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(questionDef.ItemOrder) + "";
        }

        if (questionDef.QuestType != oldQuestionDef.QuestType)
        {
            if (command != "") command += ",";
            command += "QuestType = " + SOut.Int((int) questionDef.QuestType) + "";
        }

        if (command == "") return false;
        if (questionDef.Description == null) questionDef.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(questionDef.Description));
        command = "UPDATE questiondef SET " + command
                                            + " WHERE QuestionDefNum = " + SOut.Long(questionDef.QuestionDefNum);
        Db.NonQ(command, paramDescription);
        return true;
    }

    public static bool UpdateComparison(QuestionDef questionDef, QuestionDef oldQuestionDef)
    {
        if (questionDef.Description != oldQuestionDef.Description) return true;
        if (questionDef.ItemOrder != oldQuestionDef.ItemOrder) return true;
        if (questionDef.QuestType != oldQuestionDef.QuestType) return true;
        return false;
    }

    public static void Delete(long questionDefNum)
    {
        var command = "DELETE FROM questiondef "
                      + "WHERE QuestionDefNum = " + SOut.Long(questionDefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listQuestionDefNums)
    {
        if (listQuestionDefNums == null || listQuestionDefNums.Count == 0) return;
        var command = "DELETE FROM questiondef "
                      + "WHERE QuestionDefNum IN(" + string.Join(",", listQuestionDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}