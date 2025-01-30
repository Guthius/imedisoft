using System;
using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;
using OpenDentBusiness.HL7;

namespace Imedisoft.Core.Data;

public static class HL7Msgs
{
    public static List<HL7Msg> GetOnePending()
    {
        return HL7MsgCrud.SelectMany("SELECT * FROM hl7msg WHERE HL7Status = " + (int) HL7MessageStatus.OutPending + " LIMIT 1");
    }

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

    public static string GetControlId(HL7Msg hL7Msg)
    {
        if (hL7Msg == null)
        {
            return string.Empty;
        }

        var controlIdOrder = 0;
        var messageHL7 = new MessageHL7(hL7Msg.MsgText);

        var hL7Def = HL7Defs.GetOneDeepEnabled();
        if (hL7Def is null)
        {
            return string.Empty;
        }

        HL7DefMessage hL7DefMessage = null;

        foreach (var hl7DefMessage in hL7Def.hl7DefMessages)
        {
            if (hl7DefMessage.MessageType != messageHL7.MsgType)
            {
                continue;
            }

            hL7DefMessage = hl7DefMessage;
            break;
        }

        if (hL7DefMessage is null)
        {
            return string.Empty;
        }

        foreach (var hl7DefSegment in hL7DefMessage.ListHL7DefSegments)
        {
            if (hl7DefSegment.SegmentName != SegmentNameHL7.MSH)
            {
                continue;
            }

            foreach (var hl7DefField in hl7DefSegment.hl7DefFields)
            {
                if (hl7DefField.FieldName != "messageControlId")
                {
                    continue;
                }

                controlIdOrder = hl7DefField.OrdinalPos;
                break;
            }

            break;
        }

        if (controlIdOrder == 0)
        {
            return string.Empty;
        }

        foreach (var segmentHL7 in messageHL7.Segments)
        {
            if (segmentHL7.Name == SegmentNameHL7.MSH)
            {
                return segmentHL7.Fields[controlIdOrder].ToString();
            }
        }

        return string.Empty;
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

    public static void DeleteOldMsgText()
    {
        Db.NonQ("UPDATE hl7msg SET MsgText = '' WHERE DateTStamp < ADDDATE(CURDATE(), INTERVAL -4 MONTH)");
    }

    public static List<HL7Msg> GetOneExisting(HL7Msg hL7Msg)
    {
        return HL7MsgCrud.SelectMany("SELECT * FROM hl7msg WHERE MsgText = '" + SOut.String(hL7Msg.MsgText) + "' LIMIT 1");
    }

    public static void UpdateDateTStamp(HL7Msg hL7Msg)
    {
        if (string.IsNullOrWhiteSpace(hL7Msg.MsgText))
        {
            return;
        }

        Db.NonQ("UPDATE hl7msg SET DateTStamp = CURRENT_TIMESTAMP WHERE MsgText = '" + SOut.String(hL7Msg.MsgText) + "'");
    }
}