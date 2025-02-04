using System.Collections.Generic;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class SecurityLogHashCrud
{
    public static void Insert(SecurityLogHash securityLogHash)
    {
        var command = "INSERT INTO securityloghash (";

        command += "SecurityLogNum,LogHash) VALUES(";

        command +=
            SOut.Long(securityLogHash.SecurityLogNum) + ","
                                                      + "'" + SOut.String(securityLogHash.LogHash) + "')";
        {
            securityLogHash.SecurityLogHashNum = Db.NonQ(command, true, "SecurityLogHashNum", "securityLogHash");
        }
    }

    public static void InsertMany(List<SecurityLogHash> listSecurityLogHashs)
    {
        InsertMany(listSecurityLogHashs, false);
    }

    public static void InsertMany(List<SecurityLogHash> listSecurityLogHashs, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listSecurityLogHashs.Count)
        {
            var securityLogHash = listSecurityLogHashs[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO securityloghash (");
                if (useExistingPK) sbCommands.Append("SecurityLogHashNum,");
                sbCommands.Append("SecurityLogNum,LogHash) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(securityLogHash.SecurityLogHashNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(securityLogHash.SecurityLogNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(securityLogHash.LogHash) + "'");
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
                if (index == listSecurityLogHashs.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static void InsertNoCache(SecurityLogHash securityLogHash, bool useExistingPK = false)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO securityloghash (";
        if (isRandomKeys || useExistingPK) command += "SecurityLogHashNum,";
        command += "SecurityLogNum,LogHash) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(securityLogHash.SecurityLogHashNum) + ",";
        command +=
            SOut.Long(securityLogHash.SecurityLogNum) + ","
                                                      + "'" + SOut.String(securityLogHash.LogHash) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            securityLogHash.SecurityLogHashNum = Db.NonQ(command, true, "SecurityLogHashNum", "securityLogHash");
    }
}