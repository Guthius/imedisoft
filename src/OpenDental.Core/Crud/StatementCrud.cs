using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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
        foreach (DataRow row in table.Rows)
        {
            var statement = new Statement
            {
                StatementNum = SIn.Long(row["StatementNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                SuperFamily = SIn.Long(row["SuperFamily"].ToString()),
                DateSent = SIn.Date(row["DateSent"].ToString()),
                DateRangeFrom = SIn.Date(row["DateRangeFrom"].ToString()),
                DateRangeTo = SIn.Date(row["DateRangeTo"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                NoteBold = SIn.String(row["NoteBold"].ToString()),
                Mode_ = (StatementMode) SIn.Int(row["Mode_"].ToString()),
                HidePayment = SIn.Bool(row["HidePayment"].ToString()),
                SinglePatient = SIn.Bool(row["SinglePatient"].ToString()),
                Intermingled = SIn.Bool(row["Intermingled"].ToString()),
                IsSent = SIn.Bool(row["IsSent"].ToString()),
                DocNum = SIn.Long(row["DocNum"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString()),
                IsReceipt = SIn.Bool(row["IsReceipt"].ToString()),
                IsInvoice = SIn.Bool(row["IsInvoice"].ToString()),
                IsInvoiceCopy = SIn.Bool(row["IsInvoiceCopy"].ToString()),
                EmailSubject = SIn.String(row["EmailSubject"].ToString()),
                EmailBody = SIn.String(row["EmailBody"].ToString()),
                IsBalValid = SIn.Bool(row["IsBalValid"].ToString()),
                InsEst = SIn.Double(row["InsEst"].ToString()),
                BalTotal = SIn.Double(row["BalTotal"].ToString())
            };
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

    public static long Insert(Statement statement)
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(statement.Note));
        if (statement.NoteBold == null) statement.NoteBold = "";
        var paramNoteBold = new OdSqlParameter("paramNoteBold", SOut.StringParam(statement.NoteBold));
        if (statement.EmailBody == null) statement.EmailBody = "";
        var paramEmailBody = new OdSqlParameter("paramEmailBody", SOut.StringParam(statement.EmailBody));
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(statement.Note));
        if (statement.NoteBold == null) statement.NoteBold = "";
        var paramNoteBold = new OdSqlParameter("paramNoteBold", SOut.StringParam(statement.NoteBold));
        if (statement.EmailBody == null) statement.EmailBody = "";
        var paramEmailBody = new OdSqlParameter("paramEmailBody", SOut.StringParam(statement.EmailBody));
        Db.NonQ(command, paramNote, paramNoteBold, paramEmailBody);
    }
}