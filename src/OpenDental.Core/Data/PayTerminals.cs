using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class PayTerminals
{
    public static List<PayTerminal> Refresh(long clinicNum)
    {
        return PayTerminalCrud.SelectMany("SELECT * FROM payterminal WHERE ClinicNum = " + clinicNum);
    }

    public static void Insert(PayTerminal payTerminal)
    {
        PayTerminalCrud.Insert(payTerminal);
    }

    public static void Update(PayTerminal payTerminal)
    {
        PayTerminalCrud.Update(payTerminal);
    }

    public static void Delete(long payTerminalNum)
    {
        PayTerminalCrud.Delete(payTerminalNum);
    }
}