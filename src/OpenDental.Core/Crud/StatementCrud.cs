#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class StatementCrud
{
    public static Statement SelectOne(long statementNum)
    {
        var command = "SELECT * FROM statement "
                      + "WHERE StatementNum = " + SOut.Long(statementNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Statement SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Statement> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Statement> TableToList(DataTable table)
    {
        var retVal = new List<Statement>();
        Statement statement;
        foreach (DataRow row in table.Rows)
        {
            statement = new Statement();
            statement.StatementNum = SIn.Long(row["StatementNum"].ToString());
            statement.PatNum = SIn.Long(row["PatNum"].ToString());
            statement.SuperFamily = SIn.Long(row["SuperFamily"].ToString());
            statement.DateSent = SIn.Date(row["DateSent"].ToString());
            statement.DateRangeFrom = SIn.Date(row["DateRangeFrom"].ToString());
            statement.DateRangeTo = SIn.Date(row["DateRangeTo"].ToString());
            statement.Note = SIn.String(row["Note"].ToString());
            statement.NoteBold = SIn.String(row["NoteBold"].ToString());
            statement.Mode_ = (StatementMode) SIn.Int(row["Mode_"].ToString());
            statement.HidePayment = SIn.Bool(row["HidePayment"].ToString());
            statement.SinglePatient = SIn.Bool(row["SinglePatient"].ToString());
            statement.Intermingled = SIn.Bool(row["Intermingled"].ToString());
            statement.IsSent = SIn.Bool(row["IsSent"].ToString());
            statement.DocNum = SIn.Long(row["DocNum"].ToString());
            statement.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            statement.IsReceipt = SIn.Bool(row["IsReceipt"].ToString());
            statement.IsInvoice = SIn.Bool(row["IsInvoice"].ToString());
            statement.IsInvoiceCopy = SIn.Bool(row["IsInvoiceCopy"].ToString());
            statement.EmailSubject = SIn.String(row["EmailSubject"].ToString());
            statement.EmailBody = SIn.String(row["EmailBody"].ToString());
            statement.IsBalValid = SIn.Bool(row["IsBalValid"].ToString());
            statement.InsEst = SIn.Double(row["InsEst"].ToString());
            statement.BalTotal = SIn.Double(row["BalTotal"].ToString());
            var statementType = row["StatementType"].ToString();
            if (statementType == "")
                statement.StatementType = 0;
            else
                try
                {
                    statement.StatementType = (StmtType) Enum.Parse(typeof(StmtType), statementType);
                }
                catch
                {
                    statement.StatementType = 0;
                }

            statement.ShortGUID = SIn.String(row["ShortGUID"].ToString());
            statement.StatementURL = SIn.String(row["StatementURL"].ToString());
            statement.StatementShortURL = SIn.String(row["StatementShortURL"].ToString());
            statement.SmsSendStatus = (AutoCommStatus) SIn.Int(row["SmsSendStatus"].ToString());
            statement.LimitedCustomFamily = (EnumLimitedCustomFamily) SIn.Int(row["LimitedCustomFamily"].ToString());
            retVal.Add(statement);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Statement> listStatements, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Statement";
        var table = new DataTable(tableName);
        table.Columns.Add("StatementNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("SuperFamily");
        table.Columns.Add("DateSent");
        table.Columns.Add("DateRangeFrom");
        table.Columns.Add("DateRangeTo");
        table.Columns.Add("Note");
        table.Columns.Add("NoteBold");
        table.Columns.Add("Mode_");
        table.Columns.Add("HidePayment");
        table.Columns.Add("SinglePatient");
        table.Columns.Add("Intermingled");
        table.Columns.Add("IsSent");
        table.Columns.Add("DocNum");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("IsReceipt");
        table.Columns.Add("IsInvoice");
        table.Columns.Add("IsInvoiceCopy");
        table.Columns.Add("EmailSubject");
        table.Columns.Add("EmailBody");
        table.Columns.Add("IsBalValid");
        table.Columns.Add("InsEst");
        table.Columns.Add("BalTotal");
        table.Columns.Add("StatementType");
        table.Columns.Add("ShortGUID");
        table.Columns.Add("StatementURL");
        table.Columns.Add("StatementShortURL");
        table.Columns.Add("SmsSendStatus");
        table.Columns.Add("LimitedCustomFamily");
        foreach (var statement in listStatements)
            table.Rows.Add(SOut.Long(statement.StatementNum), SOut.Long(statement.PatNum), SOut.Long(statement.SuperFamily), SOut.DateT(statement.DateSent, false), SOut.DateT(statement.DateRangeFrom, false), SOut.DateT(statement.DateRangeTo, false), statement.Note, statement.NoteBold, SOut.Int((int) statement.Mode_), SOut.Bool(statement.HidePayment), SOut.Bool(statement.SinglePatient), SOut.Bool(statement.Intermingled), SOut.Bool(statement.IsSent), SOut.Long(statement.DocNum), SOut.DateT(statement.DateTStamp, false), SOut.Bool(statement.IsReceipt), SOut.Bool(statement.IsInvoice), SOut.Bool(statement.IsInvoiceCopy), statement.EmailSubject, statement.EmailBody, SOut.Bool(statement.IsBalValid), SOut.Double(statement.InsEst), SOut.Double(statement.BalTotal), SOut.Int((int) statement.StatementType), statement.ShortGUID, statement.StatementURL, statement.StatementShortURL, SOut.Int((int) statement.SmsSendStatus), SOut.Int((int) statement.LimitedCustomFamily));
        return table;
    }

    public static long Insert(Statement statement)
    {
        return Insert(statement, false);
    }

    public static long Insert(Statement statement, bool useExistingPK)
    {
        var command = "INSERT INTO statement (";

        command += "PatNum,SuperFamily,DateSent,DateRangeFrom,DateRangeTo,Note,NoteBold,Mode_,HidePayment,SinglePatient,Intermingled,IsSent,DocNum,IsReceipt,IsInvoice,IsInvoiceCopy,EmailSubject,EmailBody,IsBalValid,InsEst,BalTotal,StatementType,ShortGUID,StatementURL,StatementShortURL,SmsSendStatus,LimitedCustomFamily) VALUES(";

        command +=
            SOut.Long(statement.PatNum) + ","
                                        + SOut.Long(statement.SuperFamily) + ","
                                        + SOut.Date(statement.DateSent) + ","
                                        + SOut.Date(statement.DateRangeFrom) + ","
                                        + SOut.Date(statement.DateRangeTo) + ","
                                        + DbHelper.ParamChar + "paramNote,"
                                        + DbHelper.ParamChar + "paramNoteBold,"
                                        + SOut.Int((int) statement.Mode_) + ","
                                        + SOut.Bool(statement.HidePayment) + ","
                                        + SOut.Bool(statement.SinglePatient) + ","
                                        + SOut.Bool(statement.Intermingled) + ","
                                        + SOut.Bool(statement.IsSent) + ","
                                        + SOut.Long(statement.DocNum) + ","
                                        //DateTStamp can only be set by MySQL
                                        + SOut.Bool(statement.IsReceipt) + ","
                                        + SOut.Bool(statement.IsInvoice) + ","
                                        + SOut.Bool(statement.IsInvoiceCopy) + ","
                                        + "'" + SOut.String(statement.EmailSubject) + "',"
                                        + DbHelper.ParamChar + "paramEmailBody,"
                                        + SOut.Bool(statement.IsBalValid) + ","
                                        + SOut.Double(statement.InsEst) + ","
                                        + SOut.Double(statement.BalTotal) + ","
                                        + "'" + SOut.String(statement.StatementType.ToString()) + "',"
                                        + "'" + SOut.String(statement.ShortGUID) + "',"
                                        + "'" + SOut.String(statement.StatementURL) + "',"
                                        + "'" + SOut.String(statement.StatementShortURL) + "',"
                                        + SOut.Int((int) statement.SmsSendStatus) + ","
                                        + SOut.Int((int) statement.LimitedCustomFamily) + ")";
        if (statement.Note == null) statement.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(statement.Note));
        if (statement.NoteBold == null) statement.NoteBold = "";
        var paramNoteBold = new OdSqlParameter("paramNoteBold", OdDbType.Text, SOut.StringParam(statement.NoteBold));
        if (statement.EmailBody == null) statement.EmailBody = "";
        var paramEmailBody = new OdSqlParameter("paramEmailBody", OdDbType.Text, SOut.StringParam(statement.EmailBody));
        {
            statement.StatementNum = Db.NonQ(command, true, "StatementNum", "statement", paramNote, paramNoteBold, paramEmailBody);
        }
        return statement.StatementNum;
    }

    public static void InsertMany(List<Statement> listStatements)
    {
        InsertMany(listStatements, false);
    }

    public static void InsertMany(List<Statement> listStatements, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listStatements.Count)
        {
            var statement = listStatements[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO statement (");
                if (useExistingPK) sbCommands.Append("StatementNum,");
                sbCommands.Append("PatNum,SuperFamily,DateSent,DateRangeFrom,DateRangeTo,Note,NoteBold,Mode_,HidePayment,SinglePatient,Intermingled,IsSent,DocNum,IsReceipt,IsInvoice,IsInvoiceCopy,EmailSubject,EmailBody,IsBalValid,InsEst,BalTotal,StatementType,ShortGUID,StatementURL,StatementShortURL,SmsSendStatus,LimitedCustomFamily) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(statement.StatementNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(statement.PatNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(statement.SuperFamily));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(statement.DateSent));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(statement.DateRangeFrom));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(statement.DateRangeTo));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(statement.Note) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(statement.NoteBold) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) statement.Mode_));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(statement.HidePayment));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(statement.SinglePatient));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(statement.Intermingled));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(statement.IsSent));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(statement.DocNum));
            sbRow.Append(",");
            //DateTStamp can only be set by MySQL
            sbRow.Append(SOut.Bool(statement.IsReceipt));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(statement.IsInvoice));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(statement.IsInvoiceCopy));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(statement.EmailSubject) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(statement.EmailBody) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(statement.IsBalValid));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(statement.InsEst));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(statement.BalTotal));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(statement.StatementType.ToString()) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(statement.ShortGUID) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(statement.StatementURL) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(statement.StatementShortURL) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) statement.SmsSendStatus));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) statement.LimitedCustomFamily));
            sbRow.Append(")");
            if (sbCommands.Length + sbRow.Length + 1 > TableBase.MaxAllowedPacketCount && countRows > 0)
            {
                Db.NonQ(sbCommands.ToString());
                sbCommands = null;
            }
            else
            {
                if (hasComma) sbCommands.Append(",");
                sbCommands.Append(sbRow);
                countRows++;
                if (index == listStatements.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(Statement statement)
    {
        return InsertNoCache(statement, false);
    }

    public static long InsertNoCache(Statement statement, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO statement (";
        if (isRandomKeys || useExistingPK) command += "StatementNum,";
        command += "PatNum,SuperFamily,DateSent,DateRangeFrom,DateRangeTo,Note,NoteBold,Mode_,HidePayment,SinglePatient,Intermingled,IsSent,DocNum,IsReceipt,IsInvoice,IsInvoiceCopy,EmailSubject,EmailBody,IsBalValid,InsEst,BalTotal,StatementType,ShortGUID,StatementURL,StatementShortURL,SmsSendStatus,LimitedCustomFamily) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(statement.StatementNum) + ",";
        command +=
            SOut.Long(statement.PatNum) + ","
                                        + SOut.Long(statement.SuperFamily) + ","
                                        + SOut.Date(statement.DateSent) + ","
                                        + SOut.Date(statement.DateRangeFrom) + ","
                                        + SOut.Date(statement.DateRangeTo) + ","
                                        + DbHelper.ParamChar + "paramNote,"
                                        + DbHelper.ParamChar + "paramNoteBold,"
                                        + SOut.Int((int) statement.Mode_) + ","
                                        + SOut.Bool(statement.HidePayment) + ","
                                        + SOut.Bool(statement.SinglePatient) + ","
                                        + SOut.Bool(statement.Intermingled) + ","
                                        + SOut.Bool(statement.IsSent) + ","
                                        + SOut.Long(statement.DocNum) + ","
                                        //DateTStamp can only be set by MySQL
                                        + SOut.Bool(statement.IsReceipt) + ","
                                        + SOut.Bool(statement.IsInvoice) + ","
                                        + SOut.Bool(statement.IsInvoiceCopy) + ","
                                        + "'" + SOut.String(statement.EmailSubject) + "',"
                                        + DbHelper.ParamChar + "paramEmailBody,"
                                        + SOut.Bool(statement.IsBalValid) + ","
                                        + SOut.Double(statement.InsEst) + ","
                                        + SOut.Double(statement.BalTotal) + ","
                                        + "'" + SOut.String(statement.StatementType.ToString()) + "',"
                                        + "'" + SOut.String(statement.ShortGUID) + "',"
                                        + "'" + SOut.String(statement.StatementURL) + "',"
                                        + "'" + SOut.String(statement.StatementShortURL) + "',"
                                        + SOut.Int((int) statement.SmsSendStatus) + ","
                                        + SOut.Int((int) statement.LimitedCustomFamily) + ")";
        if (statement.Note == null) statement.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(statement.Note));
        if (statement.NoteBold == null) statement.NoteBold = "";
        var paramNoteBold = new OdSqlParameter("paramNoteBold", OdDbType.Text, SOut.StringParam(statement.NoteBold));
        if (statement.EmailBody == null) statement.EmailBody = "";
        var paramEmailBody = new OdSqlParameter("paramEmailBody", OdDbType.Text, SOut.StringParam(statement.EmailBody));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote, paramNoteBold, paramEmailBody);
        else
            statement.StatementNum = Db.NonQ(command, true, "StatementNum", "statement", paramNote, paramNoteBold, paramEmailBody);
        return statement.StatementNum;
    }

    public static void Update(Statement statement)
    {
        var command = "UPDATE statement SET "
                      + "PatNum             =  " + SOut.Long(statement.PatNum) + ", "
                      + "SuperFamily        =  " + SOut.Long(statement.SuperFamily) + ", "
                      + "DateSent           =  " + SOut.Date(statement.DateSent) + ", "
                      + "DateRangeFrom      =  " + SOut.Date(statement.DateRangeFrom) + ", "
                      + "DateRangeTo        =  " + SOut.Date(statement.DateRangeTo) + ", "
                      + "Note               =  " + DbHelper.ParamChar + "paramNote, "
                      + "NoteBold           =  " + DbHelper.ParamChar + "paramNoteBold, "
                      + "Mode_              =  " + SOut.Int((int) statement.Mode_) + ", "
                      + "HidePayment        =  " + SOut.Bool(statement.HidePayment) + ", "
                      + "SinglePatient      =  " + SOut.Bool(statement.SinglePatient) + ", "
                      + "Intermingled       =  " + SOut.Bool(statement.Intermingled) + ", "
                      + "IsSent             =  " + SOut.Bool(statement.IsSent) + ", "
                      + "DocNum             =  " + SOut.Long(statement.DocNum) + ", "
                      //DateTStamp can only be set by MySQL
                      + "IsReceipt          =  " + SOut.Bool(statement.IsReceipt) + ", "
                      + "IsInvoice          =  " + SOut.Bool(statement.IsInvoice) + ", "
                      + "IsInvoiceCopy      =  " + SOut.Bool(statement.IsInvoiceCopy) + ", "
                      + "EmailSubject       = '" + SOut.String(statement.EmailSubject) + "', "
                      + "EmailBody          =  " + DbHelper.ParamChar + "paramEmailBody, "
                      + "IsBalValid         =  " + SOut.Bool(statement.IsBalValid) + ", "
                      + "InsEst             =  " + SOut.Double(statement.InsEst) + ", "
                      + "BalTotal           =  " + SOut.Double(statement.BalTotal) + ", "
                      + "StatementType      = '" + SOut.String(statement.StatementType.ToString()) + "', "
                      + "ShortGUID          = '" + SOut.String(statement.ShortGUID) + "', "
                      + "StatementURL       = '" + SOut.String(statement.StatementURL) + "', "
                      + "StatementShortURL  = '" + SOut.String(statement.StatementShortURL) + "', "
                      + "SmsSendStatus      =  " + SOut.Int((int) statement.SmsSendStatus) + ", "
                      + "LimitedCustomFamily=  " + SOut.Int((int) statement.LimitedCustomFamily) + " "
                      + "WHERE StatementNum = " + SOut.Long(statement.StatementNum);
        if (statement.Note == null) statement.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(statement.Note));
        if (statement.NoteBold == null) statement.NoteBold = "";
        var paramNoteBold = new OdSqlParameter("paramNoteBold", OdDbType.Text, SOut.StringParam(statement.NoteBold));
        if (statement.EmailBody == null) statement.EmailBody = "";
        var paramEmailBody = new OdSqlParameter("paramEmailBody", OdDbType.Text, SOut.StringParam(statement.EmailBody));
        Db.NonQ(command, paramNote, paramNoteBold, paramEmailBody);
    }

    public static bool Update(Statement statement, Statement oldStatement)
    {
        var command = "";
        if (statement.PatNum != oldStatement.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(statement.PatNum) + "";
        }

        if (statement.SuperFamily != oldStatement.SuperFamily)
        {
            if (command != "") command += ",";
            command += "SuperFamily = " + SOut.Long(statement.SuperFamily) + "";
        }

        if (statement.DateSent.Date != oldStatement.DateSent.Date)
        {
            if (command != "") command += ",";
            command += "DateSent = " + SOut.Date(statement.DateSent) + "";
        }

        if (statement.DateRangeFrom.Date != oldStatement.DateRangeFrom.Date)
        {
            if (command != "") command += ",";
            command += "DateRangeFrom = " + SOut.Date(statement.DateRangeFrom) + "";
        }

        if (statement.DateRangeTo.Date != oldStatement.DateRangeTo.Date)
        {
            if (command != "") command += ",";
            command += "DateRangeTo = " + SOut.Date(statement.DateRangeTo) + "";
        }

        if (statement.Note != oldStatement.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (statement.NoteBold != oldStatement.NoteBold)
        {
            if (command != "") command += ",";
            command += "NoteBold = " + DbHelper.ParamChar + "paramNoteBold";
        }

        if (statement.Mode_ != oldStatement.Mode_)
        {
            if (command != "") command += ",";
            command += "Mode_ = " + SOut.Int((int) statement.Mode_) + "";
        }

        if (statement.HidePayment != oldStatement.HidePayment)
        {
            if (command != "") command += ",";
            command += "HidePayment = " + SOut.Bool(statement.HidePayment) + "";
        }

        if (statement.SinglePatient != oldStatement.SinglePatient)
        {
            if (command != "") command += ",";
            command += "SinglePatient = " + SOut.Bool(statement.SinglePatient) + "";
        }

        if (statement.Intermingled != oldStatement.Intermingled)
        {
            if (command != "") command += ",";
            command += "Intermingled = " + SOut.Bool(statement.Intermingled) + "";
        }

        if (statement.IsSent != oldStatement.IsSent)
        {
            if (command != "") command += ",";
            command += "IsSent = " + SOut.Bool(statement.IsSent) + "";
        }

        if (statement.DocNum != oldStatement.DocNum)
        {
            if (command != "") command += ",";
            command += "DocNum = " + SOut.Long(statement.DocNum) + "";
        }

        //DateTStamp can only be set by MySQL
        if (statement.IsReceipt != oldStatement.IsReceipt)
        {
            if (command != "") command += ",";
            command += "IsReceipt = " + SOut.Bool(statement.IsReceipt) + "";
        }

        if (statement.IsInvoice != oldStatement.IsInvoice)
        {
            if (command != "") command += ",";
            command += "IsInvoice = " + SOut.Bool(statement.IsInvoice) + "";
        }

        if (statement.IsInvoiceCopy != oldStatement.IsInvoiceCopy)
        {
            if (command != "") command += ",";
            command += "IsInvoiceCopy = " + SOut.Bool(statement.IsInvoiceCopy) + "";
        }

        if (statement.EmailSubject != oldStatement.EmailSubject)
        {
            if (command != "") command += ",";
            command += "EmailSubject = '" + SOut.String(statement.EmailSubject) + "'";
        }

        if (statement.EmailBody != oldStatement.EmailBody)
        {
            if (command != "") command += ",";
            command += "EmailBody = " + DbHelper.ParamChar + "paramEmailBody";
        }

        if (statement.IsBalValid != oldStatement.IsBalValid)
        {
            if (command != "") command += ",";
            command += "IsBalValid = " + SOut.Bool(statement.IsBalValid) + "";
        }

        if (statement.InsEst != oldStatement.InsEst)
        {
            if (command != "") command += ",";
            command += "InsEst = " + SOut.Double(statement.InsEst) + "";
        }

        if (statement.BalTotal != oldStatement.BalTotal)
        {
            if (command != "") command += ",";
            command += "BalTotal = " + SOut.Double(statement.BalTotal) + "";
        }

        if (statement.StatementType != oldStatement.StatementType)
        {
            if (command != "") command += ",";
            command += "StatementType = '" + SOut.String(statement.StatementType.ToString()) + "'";
        }

        if (statement.ShortGUID != oldStatement.ShortGUID)
        {
            if (command != "") command += ",";
            command += "ShortGUID = '" + SOut.String(statement.ShortGUID) + "'";
        }

        if (statement.StatementURL != oldStatement.StatementURL)
        {
            if (command != "") command += ",";
            command += "StatementURL = '" + SOut.String(statement.StatementURL) + "'";
        }

        if (statement.StatementShortURL != oldStatement.StatementShortURL)
        {
            if (command != "") command += ",";
            command += "StatementShortURL = '" + SOut.String(statement.StatementShortURL) + "'";
        }

        if (statement.SmsSendStatus != oldStatement.SmsSendStatus)
        {
            if (command != "") command += ",";
            command += "SmsSendStatus = " + SOut.Int((int) statement.SmsSendStatus) + "";
        }

        if (statement.LimitedCustomFamily != oldStatement.LimitedCustomFamily)
        {
            if (command != "") command += ",";
            command += "LimitedCustomFamily = " + SOut.Int((int) statement.LimitedCustomFamily) + "";
        }

        if (command == "") return false;
        if (statement.Note == null) statement.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(statement.Note));
        if (statement.NoteBold == null) statement.NoteBold = "";
        var paramNoteBold = new OdSqlParameter("paramNoteBold", OdDbType.Text, SOut.StringParam(statement.NoteBold));
        if (statement.EmailBody == null) statement.EmailBody = "";
        var paramEmailBody = new OdSqlParameter("paramEmailBody", OdDbType.Text, SOut.StringParam(statement.EmailBody));
        command = "UPDATE statement SET " + command
                                          + " WHERE StatementNum = " + SOut.Long(statement.StatementNum);
        Db.NonQ(command, paramNote, paramNoteBold, paramEmailBody);
        return true;
    }

    public static bool UpdateComparison(Statement statement, Statement oldStatement)
    {
        if (statement.PatNum != oldStatement.PatNum) return true;
        if (statement.SuperFamily != oldStatement.SuperFamily) return true;
        if (statement.DateSent.Date != oldStatement.DateSent.Date) return true;
        if (statement.DateRangeFrom.Date != oldStatement.DateRangeFrom.Date) return true;
        if (statement.DateRangeTo.Date != oldStatement.DateRangeTo.Date) return true;
        if (statement.Note != oldStatement.Note) return true;
        if (statement.NoteBold != oldStatement.NoteBold) return true;
        if (statement.Mode_ != oldStatement.Mode_) return true;
        if (statement.HidePayment != oldStatement.HidePayment) return true;
        if (statement.SinglePatient != oldStatement.SinglePatient) return true;
        if (statement.Intermingled != oldStatement.Intermingled) return true;
        if (statement.IsSent != oldStatement.IsSent) return true;
        if (statement.DocNum != oldStatement.DocNum) return true;
        //DateTStamp can only be set by MySQL
        if (statement.IsReceipt != oldStatement.IsReceipt) return true;
        if (statement.IsInvoice != oldStatement.IsInvoice) return true;
        if (statement.IsInvoiceCopy != oldStatement.IsInvoiceCopy) return true;
        if (statement.EmailSubject != oldStatement.EmailSubject) return true;
        if (statement.EmailBody != oldStatement.EmailBody) return true;
        if (statement.IsBalValid != oldStatement.IsBalValid) return true;
        if (statement.InsEst != oldStatement.InsEst) return true;
        if (statement.BalTotal != oldStatement.BalTotal) return true;
        if (statement.StatementType != oldStatement.StatementType) return true;
        if (statement.ShortGUID != oldStatement.ShortGUID) return true;
        if (statement.StatementURL != oldStatement.StatementURL) return true;
        if (statement.StatementShortURL != oldStatement.StatementShortURL) return true;
        if (statement.SmsSendStatus != oldStatement.SmsSendStatus) return true;
        if (statement.LimitedCustomFamily != oldStatement.LimitedCustomFamily) return true;
        return false;
    }

    public static void Delete(long statementNum)
    {
        var command = "DELETE FROM statement "
                      + "WHERE StatementNum = " + SOut.Long(statementNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listStatementNums)
    {
        if (listStatementNums == null || listStatementNums.Count == 0) return;
        var command = "DELETE FROM statement "
                      + "WHERE StatementNum IN(" + string.Join(",", listStatementNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}