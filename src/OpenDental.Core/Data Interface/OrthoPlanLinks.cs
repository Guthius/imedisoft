using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class OrthoPlanLinks
{
    public static void Insert(OrthoPlanLink orthoPlanLink)
    {
        OrthoPlanLinkCrud.Insert(orthoPlanLink);
    }

    public static void Update(OrthoPlanLink orthoPlanLinkNew, OrthoPlanLink orthoPlanLinkOld)
    {
        OrthoPlanLinkCrud.Update(orthoPlanLinkNew, orthoPlanLinkOld);
    }

    public static OrthoPlanLink GetOneForOrthoCaseByType(long orthoCaseNum, OrthoPlanLinkType orthoPlanLinkType)
    {
        var command = $@"SELECT * FROM orthoplanlink WHERE orthoplanlink.OrthoCaseNum={orthoCaseNum}
				AND orthoplanlink.LinkType={SOut.Int((int) orthoPlanLinkType)}";
        return OrthoPlanLinkCrud.SelectOne(command);
    }

    public static List<OrthoPlanLink> GetAllForOrthoCasesByType(List<long> listOrthoCaseNums, OrthoPlanLinkType orthoPlanLinkType)
    {
        if (listOrthoCaseNums.Count <= 0) return [];

        var command = $@"SELECT * FROM orthoplanlink WHERE orthoplanlink.LinkType={SOut.Int((int) orthoPlanLinkType)}
				AND orthoplanlink.OrthoCaseNum IN({string.Join(",", listOrthoCaseNums)})";
        return OrthoPlanLinkCrud.SelectMany(command);
    }

    public static List<OrthoPlanLink> GetManyForOrthoCases(List<long> listOrthoCaseNums)
    {
        if (listOrthoCaseNums.Count <= 0) return [];

        var command = $"SELECT * FROM orthoplanlink WHERE orthoplanlink.OrthoCaseNum IN({string.Join(",", listOrthoCaseNums)})";
        return OrthoPlanLinkCrud.SelectMany(command);
    }

    public static OrthoPlanLink GetOrthoPlanLinkOfType(OrthoPlanLinkType orthoPlanLinkType, long payPlanNum)
    {
        var command = $"SELECT * FROM orthoplanlink WHERE LinkType={SOut.Enum(orthoPlanLinkType)} AND FKey={payPlanNum}";
        return OrthoPlanLinkCrud.SelectOne(command);
    }
}