using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class OrthoProcLinks
{
    public static void Insert(OrthoProcLink orthoProcLink)
    {
        OrthoProcLinkCrud.Insert(orthoProcLink);
    }

    public static void Update(OrthoProcLink orthoProcLinkNew, OrthoProcLink orthoProcLinkOld)
    {
        OrthoProcLinkCrud.Update(orthoProcLinkNew, orthoProcLinkOld);
    }

    public static List<OrthoProcLink> GetAll()
    {
        var command = "SELECT orthoproclink.* FROM orthoproclink";
        return OrthoProcLinkCrud.SelectMany(command);
    }

    public static List<OrthoProcLink> GetManyByOrthoCase(long orthoCaseNum)
    {
        var command = "SELECT * FROM orthoproclink WHERE orthoproclink.OrthoCaseNum = " + orthoCaseNum;
        return OrthoProcLinkCrud.SelectMany(command);
    }

    public static OrthoProcLink GetByType(long orthoCaseNum, OrthoProcType orthoProcType)
    {
        var command = $@"SELECT * FROM orthoproclink WHERE orthoproclink.OrthoCaseNum={orthoCaseNum}
				AND orthoproclink.ProcLinkType={SOut.Int((int) orthoProcType)}";
        return OrthoProcLinkCrud.SelectOne(command);
    }

    public static List<OrthoProcLink> GetPatientData(List<OrthoCase> listOrthoCases)
    {
        return GetManyByOrthoCases(listOrthoCases.Select(x => x.OrthoCaseNum).ToList());
    }

    public static List<OrthoProcLink> GetManyByOrthoCases(List<long> listOrthoCaseNums)
    {
        if (listOrthoCaseNums.Count <= 0) return [];

        var command = $"SELECT * FROM orthoproclink WHERE orthoproclink.OrthoCaseNum IN({string.Join(",", listOrthoCaseNums)})";
        return OrthoProcLinkCrud.SelectMany(command);
    }

    public static List<OrthoProcLink> GetManyForProcs(List<long> listProcNums)
    {
        if (listProcNums.Count <= 0) return [];

        var command = $@"SELECT * FROM orthoproclink
				WHERE orthoproclink.ProcNum IN({string.Join(",", listProcNums)})";
        return OrthoProcLinkCrud.SelectMany(command);
    }

    public static OrthoProcLink GetByProcNum(long procNum)
    {
        var command = "SELECT * FROM orthoproclink WHERE ProcNum=" + procNum;
        return OrthoProcLinkCrud.SelectOne(command);
    }

    public static List<OrthoProcLink> GetVisitLinksForOrthoCase(long orthoCaseNum)
    {
        var command = $@"SELECT * FROM orthoproclink WHERE orthoproclink.OrthoCaseNum={orthoCaseNum}
			AND orthoproclink.ProcLinkType={SOut.Int((int) OrthoProcType.Visit)}";
        return OrthoProcLinkCrud.SelectMany(command);
    }

    public static void Delete(long orthoProcLinkNum)
    {
        OrthoProcLinkCrud.Delete(orthoProcLinkNum);
    }

    public static void DeleteMany(List<long> listOrthoProcLinkNums)
    {
        if (listOrthoProcLinkNums.Count <= 0) return;

        var command = $"DELETE FROM orthoproclink WHERE OrthoProcLinkNum IN({string.Join(",", listOrthoProcLinkNums)})";
        Db.NonQ(command);
    }

    public static bool IsProcLinked(long procNum)
    {
        var command = "SELECT * FROM orthoproclink WHERE orthoproclink.ProcNum=" + procNum;
        return OrthoProcLinkCrud.SelectMany(command).Count > 0;
    }
}