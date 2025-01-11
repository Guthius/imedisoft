using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Ucums
{
    public static long Insert(Ucum ucum)
    {
        return UcumCrud.Insert(ucum);
    }

    public static void Update(Ucum ucum)
    {
        UcumCrud.Update(ucum);
    }

    public static List<Ucum> GetAll()
    {
        var command = "SELECT * FROM ucum ORDER BY UcumCode";
        return UcumCrud.SelectMany(command);
    }

    ///<summary>Returns a list of just the codes for use in update or insert logic.</summary>
    public static List<string> GetAllCodes()
    {
        var listUcumCodes = new List<string>();
        var command = "SELECT UcumCode FROM ucum";
        var table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++) listUcumCodes.Add(table.Rows[i]["UcumCode"].ToString());

        return listUcumCodes;
    }

    public static Ucum GetByCode(string ucumCode)
    {
        var command =
            //because when we search for UnumCode 'a' for 'year [time]' used for age we sometimes get 'A' for 'Ampere [electric current]'
            //since MySQL is case insensitive, so we compare the binary values of 'a' and 'A' which are 0x61 and 0x41 in Hex respectively.
            "SELECT * FROM ucum WHERE CAST(UcumCode AS BINARY)=CAST('" + SOut.String(ucumCode) + "' AS BINARY)";

        return UcumCrud.SelectOne(command);
    }

    public static List<Ucum> GetBySearchText(string searchText)
    {
        var stringArraySearchTokens = searchText.Split(' ');
        var command = @"SELECT * FROM ucum ";
        for (var i = 0; i < stringArraySearchTokens.Length; i++) command += (i == 0 ? "WHERE " : "AND ") + "(UcumCode LIKE '%" + SOut.String(stringArraySearchTokens[i]) + "%' OR Description LIKE '%" + SOut.String(stringArraySearchTokens[i]) + "%') ";

        return UcumCrud.SelectMany(command);
    }
}