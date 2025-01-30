using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Ucums
{
    public static void Insert(Ucum ucum)
    {
        UcumCrud.Insert(ucum);
    }

    public static void Update(Ucum ucum)
    {
        UcumCrud.Update(ucum);
    }

    public static List<Ucum> GetAll()
    {
        return UcumCrud.SelectMany("SELECT * FROM ucum ORDER BY UcumCode");
    }

    public static List<string> GetAllCodes()
    {
        var codes = new List<string>();
  
        var dataTable = DataCore.GetTable("SELECT UcumCode FROM ucum");
        
        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            codes.Add(dataTable.Rows[i]["UcumCode"].ToString());
        }

        return codes;
    }

    public static Ucum GetByCode(string ucumCode)
    {
        return UcumCrud.SelectOne("SELECT * FROM ucum WHERE CAST(UcumCode AS BINARY)=CAST('" + SOut.String(ucumCode) + "' AS BINARY)");
    }

    public static List<Ucum> GetBySearchText(string searchText)
    {
        var tokens = searchText.Split(' ');
        
        var commandText = @"SELECT * FROM ucum ";
        for (var i = 0; i < tokens.Length; i++)
        {
            commandText += (i == 0 ? "WHERE " : "AND ") + "(UcumCode LIKE '%" + SOut.String(tokens[i]) + "%' OR Description LIKE '%" + SOut.String(tokens[i]) + "%') ";
        }
        
        return UcumCrud.SelectMany(commandText);
    }
}