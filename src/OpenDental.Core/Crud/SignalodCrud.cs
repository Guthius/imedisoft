using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class SignalodCrud
{
    public static List<Signalod> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Signalod> TableToList(DataTable table)
    {
        var retVal = new List<Signalod>();
        foreach (DataRow row in table.Rows)
        {
            var signalod = new Signalod
            {
                SignalNum = SIn.Long(row["SignalNum"].ToString()),
                DateViewing = SIn.Date(row["DateViewing"].ToString()),
                SigDateTime = SIn.DateTime(row["SigDateTime"].ToString()),
                FKey = SIn.Long(row["FKey"].ToString())
            };
            var fKeyType = row["FKeyType"].ToString();
            if (fKeyType == "")
                signalod.FKeyType = 0;
            else
                try
                {
                    signalod.FKeyType = (KeyType) Enum.Parse(typeof(KeyType), fKeyType);
                }
                catch
                {
                    signalod.FKeyType = 0;
                }

            signalod.IType = (InvalidType) SIn.Int(row["IType"].ToString());
            signalod.RemoteRole = SIn.Int(row["RemoteRole"].ToString());
            signalod.MsgValue = SIn.String(row["MsgValue"].ToString());
            retVal.Add(signalod);
        }

        return retVal;
    }

    public static long Insert(Signalod signalod)
    {
        var command = "INSERT INTO signalod (";

        command += "DateViewing,SigDateTime,FKey,FKeyType,IType,RemoteRole,MsgValue) VALUES(";

        command +=
            SOut.Date(signalod.DateViewing) + ","
                                            + "NOW()" + ","
                                            + SOut.Long(signalod.FKey) + ","
                                            + "'" + SOut.String(signalod.FKeyType.ToString()) + "',"
                                            + SOut.Int((int) signalod.IType) + ","
                                            + SOut.Int(signalod.RemoteRole) + ","
                                            + DbHelper.ParamChar + "paramMsgValue)";
        if (signalod.MsgValue == null) signalod.MsgValue = "";
        var paramMsgValue = new OdSqlParameter("paramMsgValue", SOut.StringParam(signalod.MsgValue));
        {
            signalod.SignalNum = Db.NonQ(command, true, "SignalNum", "signalod", paramMsgValue);
        }
        return signalod.SignalNum;
    }

    public static void InsertMany(List<Signalod> listSignalods)
    {
        InsertMany(listSignalods, false);
    }

    public static void InsertMany(List<Signalod> listSignalods, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listSignalods.Count)
        {
            var signalod = listSignalods[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO signalod (");
                if (useExistingPK) sbCommands.Append("SignalNum,");
                sbCommands.Append("DateViewing,SigDateTime,FKey,FKeyType,IType,RemoteRole,MsgValue) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(signalod.SignalNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Date(signalod.DateViewing));
            sbRow.Append(",");
            sbRow.Append("NOW()");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(signalod.FKey));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(signalod.FKeyType.ToString()) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) signalod.IType));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(signalod.RemoteRole));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(signalod.MsgValue) + "'");
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
                if (index == listSignalods.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }
}