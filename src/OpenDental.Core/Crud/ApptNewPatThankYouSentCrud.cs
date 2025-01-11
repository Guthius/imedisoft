using System.Collections.Generic;
using System.Text;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ApptNewPatThankYouSentCrud
{
    public static void InsertMany(List<ApptNewPatThankYouSent> listApptNewPatThankYouSents, bool useExistingPK = false)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listApptNewPatThankYouSents.Count)
        {
            var apptNewPatThankYouSent = listApptNewPatThankYouSents[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO apptnewpatthankyousent (");
                if (useExistingPK) sbCommands.Append("ApptNewPatThankYouSentNum,");
                sbCommands.Append("ApptSecDateTEntry,DateTimeNewPatThankYouTransmit,PatNum,ClinicNum,SendStatus,MessageType,MessageFk,DateTimeEntry,DateTimeSent,ResponseDescript,ApptReminderRuleNum,ApptNum,ApptDateTime,TSPrior,ShortGUID) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(apptNewPatThankYouSent.ApptNewPatThankYouSentNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.DateT(apptNewPatThankYouSent.ApptSecDateTEntry));
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(apptNewPatThankYouSent.DateTimeNewPatThankYouTransmit));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(apptNewPatThankYouSent.PatNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(apptNewPatThankYouSent.ClinicNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) apptNewPatThankYouSent.SendStatus));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) apptNewPatThankYouSent.MessageType));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(apptNewPatThankYouSent.MessageFk));
            sbRow.Append(",");
            sbRow.Append(DbHelper.Now());
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(apptNewPatThankYouSent.DateTimeSent));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(apptNewPatThankYouSent.ResponseDescript) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(apptNewPatThankYouSent.ApptReminderRuleNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(apptNewPatThankYouSent.ApptNum));
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(apptNewPatThankYouSent.ApptDateTime));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.Long(apptNewPatThankYouSent.TSPrior.Ticks) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(apptNewPatThankYouSent.ShortGUID) + "'");
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
                if (index == listApptNewPatThankYouSents.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }
}