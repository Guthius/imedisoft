#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PatientPortalInviteCrud
{
    public static PatientPortalInvite SelectOne(long patientPortalInviteNum)
    {
        var command = "SELECT * FROM patientportalinvite "
                      + "WHERE PatientPortalInviteNum = " + SOut.Long(patientPortalInviteNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PatientPortalInvite SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PatientPortalInvite> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PatientPortalInvite> TableToList(DataTable table)
    {
        var retVal = new List<PatientPortalInvite>();
        PatientPortalInvite patientPortalInvite;
        foreach (DataRow row in table.Rows)
        {
            patientPortalInvite = new PatientPortalInvite();
            patientPortalInvite.PatientPortalInviteNum = SIn.Long(row["PatientPortalInviteNum"].ToString());
            patientPortalInvite.PatNum = SIn.Long(row["PatNum"].ToString());
            patientPortalInvite.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            patientPortalInvite.SendStatus = (AutoCommStatus) SIn.Int(row["SendStatus"].ToString());
            patientPortalInvite.MessageType = (CommType) SIn.Int(row["MessageType"].ToString());
            patientPortalInvite.MessageFk = SIn.Long(row["MessageFk"].ToString());
            patientPortalInvite.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            patientPortalInvite.DateTimeSent = SIn.DateTime(row["DateTimeSent"].ToString());
            patientPortalInvite.ResponseDescript = SIn.String(row["ResponseDescript"].ToString());
            patientPortalInvite.ApptReminderRuleNum = SIn.Long(row["ApptReminderRuleNum"].ToString());
            patientPortalInvite.ApptNum = SIn.Long(row["ApptNum"].ToString());
            patientPortalInvite.ApptDateTime = SIn.DateTime(row["ApptDateTime"].ToString());
            patientPortalInvite.TSPrior = TimeSpan.FromTicks(SIn.Long(row["TSPrior"].ToString()));
            retVal.Add(patientPortalInvite);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PatientPortalInvite> listPatientPortalInvites, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PatientPortalInvite";
        var table = new DataTable(tableName);
        table.Columns.Add("PatientPortalInviteNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("SendStatus");
        table.Columns.Add("MessageType");
        table.Columns.Add("MessageFk");
        table.Columns.Add("DateTimeEntry");
        table.Columns.Add("DateTimeSent");
        table.Columns.Add("ResponseDescript");
        table.Columns.Add("ApptReminderRuleNum");
        table.Columns.Add("ApptNum");
        table.Columns.Add("ApptDateTime");
        table.Columns.Add("TSPrior");
        foreach (var patientPortalInvite in listPatientPortalInvites)
            table.Rows.Add(SOut.Long(patientPortalInvite.PatientPortalInviteNum), SOut.Long(patientPortalInvite.PatNum), SOut.Long(patientPortalInvite.ClinicNum), SOut.Int((int) patientPortalInvite.SendStatus), SOut.Int((int) patientPortalInvite.MessageType), SOut.Long(patientPortalInvite.MessageFk), SOut.DateT(patientPortalInvite.DateTimeEntry, false), SOut.DateT(patientPortalInvite.DateTimeSent, false), patientPortalInvite.ResponseDescript, SOut.Long(patientPortalInvite.ApptReminderRuleNum), SOut.Long(patientPortalInvite.ApptNum), SOut.DateT(patientPortalInvite.ApptDateTime, false), SOut.Long(patientPortalInvite.TSPrior.Ticks));
        return table;
    }

    public static long Insert(PatientPortalInvite patientPortalInvite)
    {
        return Insert(patientPortalInvite, false);
    }

    public static long Insert(PatientPortalInvite patientPortalInvite, bool useExistingPK)
    {
        var command = "INSERT INTO patientportalinvite (";

        command += "PatNum,ClinicNum,SendStatus,MessageType,MessageFk,DateTimeEntry,DateTimeSent,ResponseDescript,ApptReminderRuleNum,ApptNum,ApptDateTime,TSPrior) VALUES(";

        command +=
            SOut.Long(patientPortalInvite.PatNum) + ","
                                                  + SOut.Long(patientPortalInvite.ClinicNum) + ","
                                                  + SOut.Int((int) patientPortalInvite.SendStatus) + ","
                                                  + SOut.Int((int) patientPortalInvite.MessageType) + ","
                                                  + SOut.Long(patientPortalInvite.MessageFk) + ","
                                                  + DbHelper.Now() + ","
                                                  + SOut.DateT(patientPortalInvite.DateTimeSent) + ","
                                                  + DbHelper.ParamChar + "paramResponseDescript,"
                                                  + SOut.Long(patientPortalInvite.ApptReminderRuleNum) + ","
                                                  + SOut.Long(patientPortalInvite.ApptNum) + ","
                                                  + SOut.DateT(patientPortalInvite.ApptDateTime) + ","
                                                  + "'" + SOut.Long(patientPortalInvite.TSPrior.Ticks) + "')";
        if (patientPortalInvite.ResponseDescript == null) patientPortalInvite.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(patientPortalInvite.ResponseDescript));
        {
            patientPortalInvite.PatientPortalInviteNum = Db.NonQ(command, true, "PatientPortalInviteNum", "patientPortalInvite", paramResponseDescript);
        }
        return patientPortalInvite.PatientPortalInviteNum;
    }

    public static void InsertMany(List<PatientPortalInvite> listPatientPortalInvites)
    {
        InsertMany(listPatientPortalInvites, false);
    }

    public static void InsertMany(List<PatientPortalInvite> listPatientPortalInvites, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listPatientPortalInvites.Count)
        {
            var patientPortalInvite = listPatientPortalInvites[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO patientportalinvite (");
                if (useExistingPK) sbCommands.Append("PatientPortalInviteNum,");
                sbCommands.Append("PatNum,ClinicNum,SendStatus,MessageType,MessageFk,DateTimeEntry,DateTimeSent,ResponseDescript,ApptReminderRuleNum,ApptNum,ApptDateTime,TSPrior) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(patientPortalInvite.PatientPortalInviteNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(patientPortalInvite.PatNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(patientPortalInvite.ClinicNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) patientPortalInvite.SendStatus));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) patientPortalInvite.MessageType));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(patientPortalInvite.MessageFk));
            sbRow.Append(",");
            sbRow.Append(DbHelper.Now());
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(patientPortalInvite.DateTimeSent));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(patientPortalInvite.ResponseDescript) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(patientPortalInvite.ApptReminderRuleNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(patientPortalInvite.ApptNum));
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(patientPortalInvite.ApptDateTime));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.Long(patientPortalInvite.TSPrior.Ticks) + "'");
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
                if (index == listPatientPortalInvites.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(PatientPortalInvite patientPortalInvite)
    {
        return InsertNoCache(patientPortalInvite, false);
    }

    public static long InsertNoCache(PatientPortalInvite patientPortalInvite, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO patientportalinvite (";
        if (isRandomKeys || useExistingPK) command += "PatientPortalInviteNum,";
        command += "PatNum,ClinicNum,SendStatus,MessageType,MessageFk,DateTimeEntry,DateTimeSent,ResponseDescript,ApptReminderRuleNum,ApptNum,ApptDateTime,TSPrior) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(patientPortalInvite.PatientPortalInviteNum) + ",";
        command +=
            SOut.Long(patientPortalInvite.PatNum) + ","
                                                  + SOut.Long(patientPortalInvite.ClinicNum) + ","
                                                  + SOut.Int((int) patientPortalInvite.SendStatus) + ","
                                                  + SOut.Int((int) patientPortalInvite.MessageType) + ","
                                                  + SOut.Long(patientPortalInvite.MessageFk) + ","
                                                  + DbHelper.Now() + ","
                                                  + SOut.DateT(patientPortalInvite.DateTimeSent) + ","
                                                  + DbHelper.ParamChar + "paramResponseDescript,"
                                                  + SOut.Long(patientPortalInvite.ApptReminderRuleNum) + ","
                                                  + SOut.Long(patientPortalInvite.ApptNum) + ","
                                                  + SOut.DateT(patientPortalInvite.ApptDateTime) + ","
                                                  + "'" + SOut.Long(patientPortalInvite.TSPrior.Ticks) + "')";
        if (patientPortalInvite.ResponseDescript == null) patientPortalInvite.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(patientPortalInvite.ResponseDescript));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramResponseDescript);
        else
            patientPortalInvite.PatientPortalInviteNum = Db.NonQ(command, true, "PatientPortalInviteNum", "patientPortalInvite", paramResponseDescript);
        return patientPortalInvite.PatientPortalInviteNum;
    }

    public static void Update(PatientPortalInvite patientPortalInvite)
    {
        var command = "UPDATE patientportalinvite SET "
                      + "PatNum                =  " + SOut.Long(patientPortalInvite.PatNum) + ", "
                      + "ClinicNum             =  " + SOut.Long(patientPortalInvite.ClinicNum) + ", "
                      + "SendStatus            =  " + SOut.Int((int) patientPortalInvite.SendStatus) + ", "
                      + "MessageType           =  " + SOut.Int((int) patientPortalInvite.MessageType) + ", "
                      + "MessageFk             =  " + SOut.Long(patientPortalInvite.MessageFk) + ", "
                      //DateTimeEntry not allowed to change
                      + "DateTimeSent          =  " + SOut.DateT(patientPortalInvite.DateTimeSent) + ", "
                      + "ResponseDescript      =  " + DbHelper.ParamChar + "paramResponseDescript, "
                      + "ApptReminderRuleNum   =  " + SOut.Long(patientPortalInvite.ApptReminderRuleNum) + ", "
                      + "ApptNum               =  " + SOut.Long(patientPortalInvite.ApptNum) + ", "
                      + "ApptDateTime          =  " + SOut.DateT(patientPortalInvite.ApptDateTime) + ", "
                      + "TSPrior               =  " + SOut.Long(patientPortalInvite.TSPrior.Ticks) + " "
                      + "WHERE PatientPortalInviteNum = " + SOut.Long(patientPortalInvite.PatientPortalInviteNum);
        if (patientPortalInvite.ResponseDescript == null) patientPortalInvite.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(patientPortalInvite.ResponseDescript));
        Db.NonQ(command, paramResponseDescript);
    }

    public static bool Update(PatientPortalInvite patientPortalInvite, PatientPortalInvite oldPatientPortalInvite)
    {
        var command = "";
        if (patientPortalInvite.PatNum != oldPatientPortalInvite.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(patientPortalInvite.PatNum) + "";
        }

        if (patientPortalInvite.ClinicNum != oldPatientPortalInvite.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(patientPortalInvite.ClinicNum) + "";
        }

        if (patientPortalInvite.SendStatus != oldPatientPortalInvite.SendStatus)
        {
            if (command != "") command += ",";
            command += "SendStatus = " + SOut.Int((int) patientPortalInvite.SendStatus) + "";
        }

        if (patientPortalInvite.MessageType != oldPatientPortalInvite.MessageType)
        {
            if (command != "") command += ",";
            command += "MessageType = " + SOut.Int((int) patientPortalInvite.MessageType) + "";
        }

        if (patientPortalInvite.MessageFk != oldPatientPortalInvite.MessageFk)
        {
            if (command != "") command += ",";
            command += "MessageFk = " + SOut.Long(patientPortalInvite.MessageFk) + "";
        }

        //DateTimeEntry not allowed to change
        if (patientPortalInvite.DateTimeSent != oldPatientPortalInvite.DateTimeSent)
        {
            if (command != "") command += ",";
            command += "DateTimeSent = " + SOut.DateT(patientPortalInvite.DateTimeSent) + "";
        }

        if (patientPortalInvite.ResponseDescript != oldPatientPortalInvite.ResponseDescript)
        {
            if (command != "") command += ",";
            command += "ResponseDescript = " + DbHelper.ParamChar + "paramResponseDescript";
        }

        if (patientPortalInvite.ApptReminderRuleNum != oldPatientPortalInvite.ApptReminderRuleNum)
        {
            if (command != "") command += ",";
            command += "ApptReminderRuleNum = " + SOut.Long(patientPortalInvite.ApptReminderRuleNum) + "";
        }

        if (patientPortalInvite.ApptNum != oldPatientPortalInvite.ApptNum)
        {
            if (command != "") command += ",";
            command += "ApptNum = " + SOut.Long(patientPortalInvite.ApptNum) + "";
        }

        if (patientPortalInvite.ApptDateTime != oldPatientPortalInvite.ApptDateTime)
        {
            if (command != "") command += ",";
            command += "ApptDateTime = " + SOut.DateT(patientPortalInvite.ApptDateTime) + "";
        }

        if (patientPortalInvite.TSPrior != oldPatientPortalInvite.TSPrior)
        {
            if (command != "") command += ",";
            command += "TSPrior = '" + SOut.Long(patientPortalInvite.TSPrior.Ticks) + "'";
        }

        if (command == "") return false;
        if (patientPortalInvite.ResponseDescript == null) patientPortalInvite.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(patientPortalInvite.ResponseDescript));
        command = "UPDATE patientportalinvite SET " + command
                                                    + " WHERE PatientPortalInviteNum = " + SOut.Long(patientPortalInvite.PatientPortalInviteNum);
        Db.NonQ(command, paramResponseDescript);
        return true;
    }

    public static bool UpdateComparison(PatientPortalInvite patientPortalInvite, PatientPortalInvite oldPatientPortalInvite)
    {
        if (patientPortalInvite.PatNum != oldPatientPortalInvite.PatNum) return true;
        if (patientPortalInvite.ClinicNum != oldPatientPortalInvite.ClinicNum) return true;
        if (patientPortalInvite.SendStatus != oldPatientPortalInvite.SendStatus) return true;
        if (patientPortalInvite.MessageType != oldPatientPortalInvite.MessageType) return true;
        if (patientPortalInvite.MessageFk != oldPatientPortalInvite.MessageFk) return true;
        //DateTimeEntry not allowed to change
        if (patientPortalInvite.DateTimeSent != oldPatientPortalInvite.DateTimeSent) return true;
        if (patientPortalInvite.ResponseDescript != oldPatientPortalInvite.ResponseDescript) return true;
        if (patientPortalInvite.ApptReminderRuleNum != oldPatientPortalInvite.ApptReminderRuleNum) return true;
        if (patientPortalInvite.ApptNum != oldPatientPortalInvite.ApptNum) return true;
        if (patientPortalInvite.ApptDateTime != oldPatientPortalInvite.ApptDateTime) return true;
        if (patientPortalInvite.TSPrior != oldPatientPortalInvite.TSPrior) return true;
        return false;
    }

    public static void Delete(long patientPortalInviteNum)
    {
        var command = "DELETE FROM patientportalinvite "
                      + "WHERE PatientPortalInviteNum = " + SOut.Long(patientPortalInviteNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPatientPortalInviteNums)
    {
        if (listPatientPortalInviteNums == null || listPatientPortalInviteNums.Count == 0) return;
        var command = "DELETE FROM patientportalinvite "
                      + "WHERE PatientPortalInviteNum IN(" + string.Join(",", listPatientPortalInviteNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}