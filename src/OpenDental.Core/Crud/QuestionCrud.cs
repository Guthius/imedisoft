#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class QuestionCrud
{
    public static Question SelectOne(long questionNum)
    {
        var command = "SELECT * FROM question "
                      + "WHERE QuestionNum = " + SOut.Long(questionNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Question SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Question> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Question> TableToList(DataTable table)
    {
        var retVal = new List<Question>();
        Question question;
        foreach (DataRow row in table.Rows)
        {
            question = new Question();
            question.QuestionNum = SIn.Long(row["QuestionNum"].ToString());
            question.PatNum = SIn.Long(row["PatNum"].ToString());
            question.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            question.Description = SIn.String(row["Description"].ToString());
            question.Answer = SIn.String(row["Answer"].ToString());
            question.FormPatNum = SIn.Long(row["FormPatNum"].ToString());
            retVal.Add(question);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Question> listQuestions, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Question";
        var table = new DataTable(tableName);
        table.Columns.Add("QuestionNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("Description");
        table.Columns.Add("Answer");
        table.Columns.Add("FormPatNum");
        foreach (var question in listQuestions)
            table.Rows.Add(SOut.Long(question.QuestionNum), SOut.Long(question.PatNum), SOut.Int(question.ItemOrder), question.Description, question.Answer, SOut.Long(question.FormPatNum));
        return table;
    }

    public static long Insert(Question question)
    {
        return Insert(question, false);
    }

    public static long Insert(Question question, bool useExistingPK)
    {
        var command = "INSERT INTO question (";

        command += "PatNum,ItemOrder,Description,Answer,FormPatNum) VALUES(";

        command +=
            SOut.Long(question.PatNum) + ","
                                       + SOut.Int(question.ItemOrder) + ","
                                       + DbHelper.ParamChar + "paramDescription,"
                                       + DbHelper.ParamChar + "paramAnswer,"
                                       + SOut.Long(question.FormPatNum) + ")";
        if (question.Description == null) question.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(question.Description));
        if (question.Answer == null) question.Answer = "";
        var paramAnswer = new OdSqlParameter("paramAnswer", OdDbType.Text, SOut.StringParam(question.Answer));
        {
            question.QuestionNum = Db.NonQ(command, true, "QuestionNum", "question", paramDescription, paramAnswer);
        }
        return question.QuestionNum;
    }

    public static long InsertNoCache(Question question)
    {
        return InsertNoCache(question, false);
    }

    public static long InsertNoCache(Question question, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO question (";
        if (isRandomKeys || useExistingPK) command += "QuestionNum,";
        command += "PatNum,ItemOrder,Description,Answer,FormPatNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(question.QuestionNum) + ",";
        command +=
            SOut.Long(question.PatNum) + ","
                                       + SOut.Int(question.ItemOrder) + ","
                                       + DbHelper.ParamChar + "paramDescription,"
                                       + DbHelper.ParamChar + "paramAnswer,"
                                       + SOut.Long(question.FormPatNum) + ")";
        if (question.Description == null) question.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(question.Description));
        if (question.Answer == null) question.Answer = "";
        var paramAnswer = new OdSqlParameter("paramAnswer", OdDbType.Text, SOut.StringParam(question.Answer));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDescription, paramAnswer);
        else
            question.QuestionNum = Db.NonQ(command, true, "QuestionNum", "question", paramDescription, paramAnswer);
        return question.QuestionNum;
    }

    public static void Update(Question question)
    {
        var command = "UPDATE question SET "
                      + "PatNum     =  " + SOut.Long(question.PatNum) + ", "
                      + "ItemOrder  =  " + SOut.Int(question.ItemOrder) + ", "
                      + "Description=  " + DbHelper.ParamChar + "paramDescription, "
                      + "Answer     =  " + DbHelper.ParamChar + "paramAnswer, "
                      + "FormPatNum =  " + SOut.Long(question.FormPatNum) + " "
                      + "WHERE QuestionNum = " + SOut.Long(question.QuestionNum);
        if (question.Description == null) question.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(question.Description));
        if (question.Answer == null) question.Answer = "";
        var paramAnswer = new OdSqlParameter("paramAnswer", OdDbType.Text, SOut.StringParam(question.Answer));
        Db.NonQ(command, paramDescription, paramAnswer);
    }

    public static bool Update(Question question, Question oldQuestion)
    {
        var command = "";
        if (question.PatNum != oldQuestion.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(question.PatNum) + "";
        }

        if (question.ItemOrder != oldQuestion.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(question.ItemOrder) + "";
        }

        if (question.Description != oldQuestion.Description)
        {
            if (command != "") command += ",";
            command += "Description = " + DbHelper.ParamChar + "paramDescription";
        }

        if (question.Answer != oldQuestion.Answer)
        {
            if (command != "") command += ",";
            command += "Answer = " + DbHelper.ParamChar + "paramAnswer";
        }

        if (question.FormPatNum != oldQuestion.FormPatNum)
        {
            if (command != "") command += ",";
            command += "FormPatNum = " + SOut.Long(question.FormPatNum) + "";
        }

        if (command == "") return false;
        if (question.Description == null) question.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(question.Description));
        if (question.Answer == null) question.Answer = "";
        var paramAnswer = new OdSqlParameter("paramAnswer", OdDbType.Text, SOut.StringParam(question.Answer));
        command = "UPDATE question SET " + command
                                         + " WHERE QuestionNum = " + SOut.Long(question.QuestionNum);
        Db.NonQ(command, paramDescription, paramAnswer);
        return true;
    }

    public static bool UpdateComparison(Question question, Question oldQuestion)
    {
        if (question.PatNum != oldQuestion.PatNum) return true;
        if (question.ItemOrder != oldQuestion.ItemOrder) return true;
        if (question.Description != oldQuestion.Description) return true;
        if (question.Answer != oldQuestion.Answer) return true;
        if (question.FormPatNum != oldQuestion.FormPatNum) return true;
        return false;
    }

    public static void Delete(long questionNum)
    {
        var command = "DELETE FROM question "
                      + "WHERE QuestionNum = " + SOut.Long(questionNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listQuestionNums)
    {
        if (listQuestionNums == null || listQuestionNums.Count == 0) return;
        var command = "DELETE FROM question "
                      + "WHERE QuestionNum IN(" + string.Join(",", listQuestionNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}