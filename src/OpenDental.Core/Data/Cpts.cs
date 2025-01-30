using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Cpts
{
    public static List<Cpt> GetBySearchText(string searchText)
    {
        var tokens = searchText.Split(' ').ToList();
        
        var command = @"SELECT * FROM cpt WHERE ";
        
        for (var i = 0; i < tokens.Count; i++)
        {
            if (i > 0)
            {
                command += "AND ";
            }
            
            command += "(CptCode LIKE '%" + SOut.String(tokens[i]) + "%' OR Description LIKE '%" + SOut.String(tokens[i]) + "%') ";
        }

        return CptCrud.SelectMany(command);
    }
    
    public static void Insert(Cpt cpt)
    {
        CptCrud.Insert(cpt);
    }

    public static List<Cpt> GetAll()
    {
        return CptCrud.SelectMany("SELECT * FROM cpt");
    }

    public static Cpt GetByCode(string cptCode)
    {
        return CptCrud.SelectOne("SELECT * FROM cpt WHERE CptCode='" + SOut.String(cptCode) + "'");
    }

    public static long GetCodeCount()
    {
        return SIn.Long(Db.GetCount("SELECT COUNT(*) FROM cpt"));
    }
    
    public static void UpdateDescription(string cptCode, string description, string versionID)
    {
        var cpt = GetByCode(SOut.String(cptCode));
        
        var versionIDs = cpt.VersionIDs.Split(',').ToList();
        var foundVersionId = false;
        var versionIdMax = "";
        
        foreach (var versionId in versionIDs)
        {
            if (string.CompareOrdinal(versionId, versionIdMax) > 0)
            {
                versionIdMax = versionId;
            }
            
            if (versionId == versionID)
            {
                foundVersionId = true;
            }
        }

        if (!foundVersionId)
        {
            cpt.VersionIDs += ',' + versionID;
        }
        
        if (string.CompareOrdinal(versionID, versionIdMax) >= 0)
        {
            cpt.Description = description;
        }
        
        CptCrud.Update(cpt);
    }
}