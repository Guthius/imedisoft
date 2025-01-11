using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ConfirmationRequestCrud
{
    public static List<ConfirmationRequest> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ConfirmationRequest> TableToList(DataTable table)
    {
        var retVal = new List<ConfirmationRequest>();
        ConfirmationRequest confirmationRequest;
        foreach (DataRow row in table.Rows)
        {
            confirmationRequest = new ConfirmationRequest();
            confirmationRequest.ConfirmationRequestNum = SIn.Long(row["ConfirmationRequestNum"].ToString());
            confirmationRequest.DateTimeConfirmExpire = SIn.DateTime(row["DateTimeConfirmExpire"].ToString());
            confirmationRequest.ConfirmCode = SIn.String(row["ConfirmCode"].ToString());
            confirmationRequest.DateTimeConfirmTransmit = SIn.DateTime(row["DateTimeConfirmTransmit"].ToString());
            confirmationRequest.DateTimeRSVP = SIn.DateTime(row["DateTimeRSVP"].ToString());
            confirmationRequest.RSVPStatus = (RSVPStatusCodes) SIn.Int(row["RSVPStatus"].ToString());
            confirmationRequest.GuidMessageFromMobile = SIn.String(row["GuidMessageFromMobile"].ToString());
            confirmationRequest.DoNotResend = SIn.Bool(row["DoNotResend"].ToString());
            confirmationRequest.ShortGUID = SIn.String(row["ShortGUID"].ToString());
            confirmationRequest.ApptNum = SIn.Long(row["ApptNum"].ToString());
            confirmationRequest.ApptDateTime = SIn.DateTime(row["ApptDateTime"].ToString());
            confirmationRequest.TSPrior = TimeSpan.FromTicks(SIn.Long(row["TSPrior"].ToString()));
            confirmationRequest.PatNum = SIn.Long(row["PatNum"].ToString());
            confirmationRequest.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            confirmationRequest.SendStatus = (AutoCommStatus) SIn.Int(row["SendStatus"].ToString());
            confirmationRequest.MessageType = (CommType) SIn.Int(row["MessageType"].ToString());
            confirmationRequest.MessageFk = SIn.Long(row["MessageFk"].ToString());
            confirmationRequest.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            confirmationRequest.DateTimeSent = SIn.DateTime(row["DateTimeSent"].ToString());
            confirmationRequest.ResponseDescript = SIn.String(row["ResponseDescript"].ToString());
            confirmationRequest.ApptReminderRuleNum = SIn.Long(row["ApptReminderRuleNum"].ToString());
            retVal.Add(confirmationRequest);
        }

        return retVal;
    }

    public static long Insert(ConfirmationRequest confirmationRequest)
    {
        var command = "INSERT INTO confirmationrequest (";

        command += "DateTimeConfirmExpire,ConfirmCode,DateTimeConfirmTransmit,DateTimeRSVP,RSVPStatus,GuidMessageFromMobile,DoNotResend,ShortGUID,ApptNum,ApptDateTime,TSPrior,PatNum,ClinicNum,SendStatus,MessageType,MessageFk,DateTimeEntry,DateTimeSent,ResponseDescript,ApptReminderRuleNum) VALUES(";

        command +=
            SOut.DateT(confirmationRequest.DateTimeConfirmExpire) + ","
                                                                  + "'" + SOut.String(confirmationRequest.ConfirmCode) + "',"
                                                                  + SOut.DateT(confirmationRequest.DateTimeConfirmTransmit) + ","
                                                                  + SOut.DateT(confirmationRequest.DateTimeRSVP) + ","
                                                                  + SOut.Int((int) confirmationRequest.RSVPStatus) + ","
                                                                  + DbHelper.ParamChar + "paramGuidMessageFromMobile,"
                                                                  + SOut.Bool(confirmationRequest.DoNotResend) + ","
                                                                  + "'" + SOut.String(confirmationRequest.ShortGUID) + "',"
                                                                  + SOut.Long(confirmationRequest.ApptNum) + ","
                                                                  + SOut.DateT(confirmationRequest.ApptDateTime) + ","
                                                                  + "'" + SOut.Long(confirmationRequest.TSPrior.Ticks) + "',"
                                                                  + SOut.Long(confirmationRequest.PatNum) + ","
                                                                  + SOut.Long(confirmationRequest.ClinicNum) + ","
                                                                  + SOut.Int((int) confirmationRequest.SendStatus) + ","
                                                                  + SOut.Int((int) confirmationRequest.MessageType) + ","
                                                                  + SOut.Long(confirmationRequest.MessageFk) + ","
                                                                  + DbHelper.Now() + ","
                                                                  + SOut.DateT(confirmationRequest.DateTimeSent) + ","
                                                                  + DbHelper.ParamChar + "paramResponseDescript,"
                                                                  + SOut.Long(confirmationRequest.ApptReminderRuleNum) + ")";
        if (confirmationRequest.GuidMessageFromMobile == null) confirmationRequest.GuidMessageFromMobile = "";
        var paramGuidMessageFromMobile = new OdSqlParameter("paramGuidMessageFromMobile", OdDbType.Text, SOut.StringParam(confirmationRequest.GuidMessageFromMobile));
        if (confirmationRequest.ResponseDescript == null) confirmationRequest.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(confirmationRequest.ResponseDescript));
        {
            confirmationRequest.ConfirmationRequestNum = Db.NonQ(command, true, "ConfirmationRequestNum", "confirmationRequest", paramGuidMessageFromMobile, paramResponseDescript);
        }
        return confirmationRequest.ConfirmationRequestNum;
    }

    public static void Update(ConfirmationRequest confirmationRequest)
    {
        var command = "UPDATE confirmationrequest SET "
                      + "DateTimeConfirmExpire  =  " + SOut.DateT(confirmationRequest.DateTimeConfirmExpire) + ", "
                      + "ConfirmCode            = '" + SOut.String(confirmationRequest.ConfirmCode) + "', "
                      + "DateTimeConfirmTransmit=  " + SOut.DateT(confirmationRequest.DateTimeConfirmTransmit) + ", "
                      + "DateTimeRSVP           =  " + SOut.DateT(confirmationRequest.DateTimeRSVP) + ", "
                      + "RSVPStatus             =  " + SOut.Int((int) confirmationRequest.RSVPStatus) + ", "
                      + "GuidMessageFromMobile  =  " + DbHelper.ParamChar + "paramGuidMessageFromMobile, "
                      + "DoNotResend            =  " + SOut.Bool(confirmationRequest.DoNotResend) + ", "
                      + "ShortGUID              = '" + SOut.String(confirmationRequest.ShortGUID) + "', "
                      + "ApptNum                =  " + SOut.Long(confirmationRequest.ApptNum) + ", "
                      + "ApptDateTime           =  " + SOut.DateT(confirmationRequest.ApptDateTime) + ", "
                      + "TSPrior                =  " + SOut.Long(confirmationRequest.TSPrior.Ticks) + ", "
                      + "PatNum                 =  " + SOut.Long(confirmationRequest.PatNum) + ", "
                      + "ClinicNum              =  " + SOut.Long(confirmationRequest.ClinicNum) + ", "
                      + "SendStatus             =  " + SOut.Int((int) confirmationRequest.SendStatus) + ", "
                      + "MessageType            =  " + SOut.Int((int) confirmationRequest.MessageType) + ", "
                      + "MessageFk              =  " + SOut.Long(confirmationRequest.MessageFk) + ", "
                      //DateTimeEntry not allowed to change
                      + "DateTimeSent           =  " + SOut.DateT(confirmationRequest.DateTimeSent) + ", "
                      + "ResponseDescript       =  " + DbHelper.ParamChar + "paramResponseDescript, "
                      + "ApptReminderRuleNum    =  " + SOut.Long(confirmationRequest.ApptReminderRuleNum) + " "
                      + "WHERE ConfirmationRequestNum = " + SOut.Long(confirmationRequest.ConfirmationRequestNum);
        if (confirmationRequest.GuidMessageFromMobile == null) confirmationRequest.GuidMessageFromMobile = "";
        var paramGuidMessageFromMobile = new OdSqlParameter("paramGuidMessageFromMobile", OdDbType.Text, SOut.StringParam(confirmationRequest.GuidMessageFromMobile));
        if (confirmationRequest.ResponseDescript == null) confirmationRequest.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(confirmationRequest.ResponseDescript));
        Db.NonQ(command, paramGuidMessageFromMobile, paramResponseDescript);
    }
}