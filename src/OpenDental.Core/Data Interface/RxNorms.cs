using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class RxNorms
{
    public static bool IsRxNormTableSmall()
    {
        var command = "SELECT COUNT(*) FROM rxnorm";
        if (SIn.Int(Db.GetCount(command)) < 50) return true;
        return false;
    }

    public static RxNorm GetByRxCUI(string rxCui)
    {
        var command = "SELECT * FROM rxnorm WHERE RxCui='" + SOut.String(rxCui) + "' AND MmslCode=''";
        return RxNormCrud.SelectOne(command);
    }

    public static List<RxNorm> GetListByCodeOrDesc(string codeOrDesc, bool isExact, bool ignoreNumbers)
    {
        var command = "SELECT * FROM rxnorm WHERE MmslCode='' ";
        if (isExact)
        {
            command += "AND (RxCui = '" + SOut.String(codeOrDesc) + "' OR Description = '" + SOut.String(codeOrDesc) + "')";
        }
        else
        {
            //Similar matches
            var charArraySeparators = new[] {' ', '\t', '\r', '\n'};
            var listSearchWords = codeOrDesc.Split(charArraySeparators, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (listSearchWords.Count > 0)
                command += "AND ("
                           + "RxCui LIKE '%" + SOut.String(codeOrDesc) + "%' "
                           + " OR "
                           + "(" + string.Join(" AND ", listSearchWords.Select(x => "Description LIKE '%" + SOut.String(x) + "%'")) + ") "
                           + ")";
        }

        if (ignoreNumbers) command += "AND Description NOT REGEXP '.*[0-9]+.*' ";
        command += " ORDER BY Description";
        return RxNormCrud.SelectMany(command);
    }

    public static string GetDescByRxCui(string rxCui)
    {
        var command = "SELECT Description FROM rxnorm WHERE MmslCode='' AND RxCui='" + rxCui + "'";
        return DataCore.GetScalar(command);
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
        var command = "SELECT * FROM rxnorm";
        return RxNormCrud.SelectMany(command);
    }

    public static long GetCodeCount()
    {
        var command = "SELECT COUNT(*) FROM rxnorm";
        return SIn.Long(Db.GetCount(command));
    }
}