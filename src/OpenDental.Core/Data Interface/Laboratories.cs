using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Laboratories
{
    ///<summary>Refresh all Laboratories</summary>
    public static List<Laboratory> Refresh()
    {
        var command = "SELECT * FROM laboratory ORDER BY Description";
        return LaboratoryCrud.SelectMany(command);
    }

    ///<summary>Gets one laboratory from the database.</summary>
    public static Laboratory GetOne(long laboratoryNum)
    {
        var command = "SELECT * FROM laboratory WHERE LaboratoryNum=" + SOut.Long(laboratoryNum);
        return LaboratoryCrud.SelectOne(command);
    }

    public static List<Laboratory> GetMany(List<long> listLaboratoryNums)
    {
        if (listLaboratoryNums.IsNullOrEmpty()) return new List<Laboratory>();

        var command = "SELECT * FROM laboratory "
                      + "WHERE LaboratoryNum IN(" + string.Join(",", listLaboratoryNums.Select(x => SOut.Long(x)).ToArray()) + ") "
                      + "ORDER BY Description";
        return LaboratoryCrud.SelectMany(command);
    }

    ///<summary>Gets a list of laboratories for the API. Returns an empty list if none found.</summary>
    public static List<Laboratory> GetLaboratoriesForApi(int limit, int offset)
    {
        var command = "SELECT * FROM laboratory ";
        command += "ORDER BY LaboratoryNum " //Ensure order for limit and offset.
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return LaboratoryCrud.SelectMany(command);
    }

    
    public static long Insert(Laboratory laboratory)
    {
        return LaboratoryCrud.Insert(laboratory);
    }

    
    public static void Update(Laboratory laboratory)
    {
        LaboratoryCrud.Update(laboratory);
    }

    ///<summary>Checks dependencies first.  Throws exception if can't delete.</summary>
    public static void Delete(long laboratoryNum)
    {
        string command;
        //check lab cases for dependencies
        command = "SELECT LName,FName FROM patient,labcase "
                  + "WHERE patient.PatNum=labcase.PatNum "
                  + "AND LaboratoryNum =" + SOut.Long(laboratoryNum) + " "
                  + DbHelper.LimitAnd(30);
        var table = DataCore.GetTable(command);
        if (table.Rows.Count > 0)
        {
            var pats = "";
            for (var i = 0; i < table.Rows.Count; i++)
            {
                pats += "\r";
                pats += table.Rows[i][0] + ", " + table.Rows[i][1];
            }

            throw new Exception(Lans.g("Laboratories", "Cannot delete Laboratory because cases exist for") + pats);
        }

        //delete
        command = "DELETE FROM laboratory WHERE LaboratoryNum = " + SOut.Long(laboratoryNum);
        Db.NonQ(command);
    }
}