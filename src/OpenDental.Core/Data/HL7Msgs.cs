using System;
using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class HL7Msgs
{
    public static HL7Msg GetOne(long hl7MsgNum)
    {
        return HL7MsgCrud.SelectOne("SELECT * FROM hl7msg WHERE HL7MsgNum = " + hl7MsgNum);
    }

    public static List<HL7Msg> GetHL7Msgs(DateTime dateStart, DateTime dateEnd, long patNum, int status)
    {
        var commandText =
            """
            SELECT HL7MsgNum, HL7Status, '' AS MsgText, AptNum, DateTStamp, PatNum, Note 
            FROM hl7msg WHERE 
            """ +
            "DATE(hl7msg.DateTStamp) BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd);

        if (patNum > 0)
        {
            commandText += " AND hl7msg.PatNum=" + patNum;
        }

        if (status > 0)
        {
            commandText += " AND hl7msg.HL7Status = " + (status - 1);
        }

        commandText += " ORDER BY hl7msg.DateTStamp";

        return HL7MsgCrud.SelectMany(commandText);
    }

    public static long Insert(HL7Msg hL7Msg)
    {
        return HL7MsgCrud.Insert(hL7Msg);
    }

    public static void Update(HL7Msg hL7Msg)
    {
        HL7MsgCrud.Update(hL7Msg);
    }

    public static bool MessageWasSent(long aptNum)
    {
        return Db.GetCount("SELECT COUNT(*) FROM hl7msg WHERE AptNum = " + aptNum + " AND (HL7Status = " + (int) HL7MessageStatus.OutSent + " OR HL7Status = " + (int) HL7MessageStatus.OutPending + ")") != "0";
    }
}