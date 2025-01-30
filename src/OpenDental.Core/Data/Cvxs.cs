using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Cvxs
{
    public static void Insert(Cvx cvx)
    {
        CvxCrud.Insert(cvx);
    }

    public static void Update(Cvx cvx)
    {
        CvxCrud.Update(cvx);
    }

    public static List<Cvx> GetAll()
    {
        return CvxCrud.SelectMany("SELECT * FROM cvx");
    }

    public static Cvx GetByCode(string cvxCode)
    {
        return CvxCrud.SelectOne("SELECT * FROM Cvx WHERE CvxCode='" + SOut.String(cvxCode) + "'");
    }

    public static Cvx GetOneFromDb(string cvxCode)
    {
        return CvxCrud.SelectOne("SELECT * FROM cvx WHERE CvxCode='" + SOut.String(cvxCode) + "'");
    }

    public static bool CodeExists(string cvxCode)
    {
        var count = Db.GetCount("SELECT COUNT(*) FROM cvx WHERE CvxCode='" + SOut.String(cvxCode) + "'");

        return count != "0";
    }

    public static long GetCodeCount()
    {
        return SIn.Long(Db.GetCount("SELECT COUNT(*) FROM cvx"));
    }

    public static List<Cvx> GetBySearchText(string searchText)
    {
        var token = searchText.Split(' ').ToList();

        var commandText = @"SELECT * FROM cvx WHERE ";

        for (var i = 0; i < token.Count; i++)
        {
            if (i > 0)
            {
                commandText += "AND ";
            }

            commandText += "(CvxCode LIKE '%" + SOut.String(token[i]) + "%' OR Description LIKE '%" + SOut.String(token[i]) + "%') ";
        }

        return CvxCrud.SelectMany(commandText);
    }
}