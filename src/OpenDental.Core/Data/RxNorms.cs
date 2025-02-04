using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class RxNorms
{
    public static bool IsRxNormTableSmall()
    {
        return SIn.Int(Db.GetCount("SELECT COUNT(*) FROM rxnorm")) < 50;
    }

    public static List<RxNorm> GetListByCodeOrDesc(string codeOrDesc, bool isExact, bool ignoreNumbers)
    {
        var commandText = "SELECT * FROM rxnorm WHERE MmslCode=''";

        if (isExact)
        {
            commandText += " AND (RxCui = '" + SOut.String(codeOrDesc) + "' OR Description = '" + SOut.String(codeOrDesc) + "')";
        }
        else
        {
            var separators = new[] {' ', '\t', '\r', '\n'};

            var searchWords = codeOrDesc.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (searchWords.Count > 0)
            {
                commandText += " AND (RxCui LIKE '%" + SOut.String(codeOrDesc) + "%' OR (" + string.Join(" AND ", searchWords.Select(x => "Description LIKE '%" + SOut.String(x) + "%'")) + "))";
            }
        }

        if (ignoreNumbers)
        {
            commandText += "AND Description NOT REGEXP '.*[0-9]+.*' ";
        }

        commandText += " ORDER BY Description";

        return RxNormCrud.SelectMany(commandText);
    }

    public static string GetDescByRxCui(string rxCui)
    {
        return DataCore.GetScalar("SELECT Description FROM rxnorm WHERE MmslCode='' AND RxCui='" + rxCui + "'");
    }

    public static void Insert(RxNorm rxNorm)
    {
        RxNormCrud.Insert(rxNorm);
    }

    public static void Update(RxNorm rxNorm)
    {
        RxNormCrud.Update(rxNorm);
    }

    public static List<RxNorm> GetAll()
    {
        return RxNormCrud.SelectMany("SELECT * FROM rxnorm");
    }
}