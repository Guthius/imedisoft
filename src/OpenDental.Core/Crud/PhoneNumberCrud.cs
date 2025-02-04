using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PhoneNumberCrud
{
    public static List<PhoneNumber> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PhoneNumber> TableToList(DataTable table)
    {
        var retVal = new List<PhoneNumber>();
        foreach (DataRow row in table.Rows)
        {
            var phoneNumber = new PhoneNumber
            {
                PhoneNumberNum = SIn.Long(row["PhoneNumberNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                PhoneNumberVal = SIn.String(row["PhoneNumberVal"].ToString()),
                PhoneNumberDigits = SIn.String(row["PhoneNumberDigits"].ToString()),
                PhoneType = (PhoneType) SIn.Int(row["PhoneType"].ToString())
            };
            retVal.Add(phoneNumber);
        }

        return retVal;
    }

    public static void InsertMany(List<PhoneNumber> listPhoneNumbers, bool useExistingPK = false)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listPhoneNumbers.Count)
        {
            var phoneNumber = listPhoneNumbers[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO phonenumber (");
                if (useExistingPK) sbCommands.Append("PhoneNumberNum,");
                sbCommands.Append("PatNum,PhoneNumberVal,PhoneNumberDigits,PhoneType) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(phoneNumber.PhoneNumberNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(phoneNumber.PatNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(phoneNumber.PhoneNumberVal) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(phoneNumber.PhoneNumberDigits) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) phoneNumber.PhoneType));
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
                if (index == listPhoneNumbers.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }
}