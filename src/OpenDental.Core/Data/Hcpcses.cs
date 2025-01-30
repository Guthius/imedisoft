using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Hcpcses
{
    public static void Insert(Hcpcs hcpcs)
    {
        HcpcsCrud.Insert(hcpcs);
    }

    public static void Update(Hcpcs hcpcs)
    {
        HcpcsCrud.Update(hcpcs);
    }

    public static List<Hcpcs> GetAll()
    {
        return HcpcsCrud.SelectMany("SELECT * FROM hcpcs");
    }

    public static long GetCodeCount()
    {
        return SIn.Long(Db.GetCount("SELECT COUNT(*) FROM hcpcs"));
    }

    public static Hcpcs GetByCode(string hcpcsCode)
    {
        return HcpcsCrud.SelectOne("SELECT * FROM hcpcs WHERE HcpcsCode='" + SOut.String(hcpcsCode) + "'");
    }

    public static List<Hcpcs> GetBySearchText(string searchText)
    {
        var searchTokens = searchText.Split(' ').ToList();

        var command = @"SELECT * FROM hcpcs ";
        for (var i = 0; i < searchTokens.Count; i++)
        {
            if (i == 0)
            {
                command += "WHERE (HcpcsCode LIKE '%" + SOut.String(searchTokens[i]) + "%' OR DescriptionShort LIKE '%" + SOut.String(searchTokens[i]) + "%') ";
            }
            else
            {
                command += "AND (HcpcsCode LIKE '%" + SOut.String(searchTokens[i]) + "%' OR DescriptionShort LIKE '%" + SOut.String(searchTokens[i]) + "%') ";
            }
        }

        return HcpcsCrud.SelectMany(command);
    }
}