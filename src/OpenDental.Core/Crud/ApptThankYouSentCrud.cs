using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ApptThankYouSentCrud
{
    public static List<ApptThankYouSent> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ApptThankYouSent> TableToList(DataTable table)
    {
        var retVal = new List<ApptThankYouSent>();
        ApptThankYouSent apptThankYouSent;
        foreach (DataRow row in table.Rows)
        {
            apptThankYouSent = new ApptThankYouSent();
            apptThankYouSent.ApptThankYouSentNum = SIn.Long(row["ApptThankYouSentNum"].ToString());
            apptThankYouSent.ApptSecDateTEntry = SIn.DateTime(row["ApptSecDateTEntry"].ToString());
            apptThankYouSent.DateTimeThankYouTransmit = SIn.DateTime(row["DateTimeThankYouTransmit"].ToString());
            apptThankYouSent.DoNotResend = SIn.Bool(row["DoNotResend"].ToString());
            apptThankYouSent.PatNum = SIn.Long(row["PatNum"].ToString());
            apptThankYouSent.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            apptThankYouSent.SendStatus = (AutoCommStatus) SIn.Int(row["SendStatus"].ToString());
            apptThankYouSent.MessageType = (CommType) SIn.Int(row["MessageType"].ToString());
            apptThankYouSent.MessageFk = SIn.Long(row["MessageFk"].ToString());
            apptThankYouSent.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            apptThankYouSent.DateTimeSent = SIn.DateTime(row["DateTimeSent"].ToString());
            apptThankYouSent.ResponseDescript = SIn.String(row["ResponseDescript"].ToString());
            apptThankYouSent.ApptReminderRuleNum = SIn.Long(row["ApptReminderRuleNum"].ToString());
            apptThankYouSent.ApptNum = SIn.Long(row["ApptNum"].ToString());
            apptThankYouSent.ApptDateTime = SIn.DateTime(row["ApptDateTime"].ToString());
            apptThankYouSent.TSPrior = TimeSpan.FromTicks(SIn.Long(row["TSPrior"].ToString()));
            apptThankYouSent.ShortGUID = SIn.String(row["ShortGUID"].ToString());
            retVal.Add(apptThankYouSent);
        }

        return retVal;
    }

    public static void Update(ApptThankYouSent apptThankYouSent)
    {
        var command = "UPDATE apptthankyousent SET "
                      + "ApptSecDateTEntry       =  " + SOut.DateT(apptThankYouSent.ApptSecDateTEntry) + ", "
                      + "DateTimeThankYouTransmit=  " + SOut.DateT(apptThankYouSent.DateTimeThankYouTransmit) + ", "
                      + "DoNotResend             =  " + SOut.Bool(apptThankYouSent.DoNotResend) + ", "
                      + "PatNum                  =  " + SOut.Long(apptThankYouSent.PatNum) + ", "
                      + "ClinicNum               =  " + SOut.Long(apptThankYouSent.ClinicNum) + ", "
                      + "SendStatus              =  " + SOut.Int((int) apptThankYouSent.SendStatus) + ", "
                      + "MessageType             =  " + SOut.Int((int) apptThankYouSent.MessageType) + ", "
                      + "MessageFk               =  " + SOut.Long(apptThankYouSent.MessageFk) + ", "
                      //DateTimeEntry not allowed to change
                      + "DateTimeSent            =  " + SOut.DateT(apptThankYouSent.DateTimeSent) + ", "
                      + "ResponseDescript        =  " + DbHelper.ParamChar + "paramResponseDescript, "
                      + "ApptReminderRuleNum     =  " + SOut.Long(apptThankYouSent.ApptReminderRuleNum) + ", "
                      + "ApptNum                 =  " + SOut.Long(apptThankYouSent.ApptNum) + ", "
                      + "ApptDateTime            =  " + SOut.DateT(apptThankYouSent.ApptDateTime) + ", "
                      + "TSPrior                 =  " + SOut.Long(apptThankYouSent.TSPrior.Ticks) + ", "
                      + "ShortGUID               = '" + SOut.String(apptThankYouSent.ShortGUID) + "' "
                      + "WHERE ApptThankYouSentNum = " + SOut.Long(apptThankYouSent.ApptThankYouSentNum);
        if (apptThankYouSent.ResponseDescript == null) apptThankYouSent.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(apptThankYouSent.ResponseDescript));
        Db.NonQ(command, paramResponseDescript);
    }
}