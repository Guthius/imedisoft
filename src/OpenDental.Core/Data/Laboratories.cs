using System;
using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Laboratories
{
    public static List<Laboratory> Refresh()
    {
        return LaboratoryCrud.SelectMany("SELECT * FROM laboratory ORDER BY Description");
    }

    public static Laboratory GetOne(long laboratoryNum)
    {
        return LaboratoryCrud.SelectOne("SELECT * FROM laboratory WHERE LaboratoryNum=" + laboratoryNum);
    }

    public static List<Laboratory> GetMany(List<long> laboratoryNums)
    {
        return laboratoryNums is not {Count: > 0} ? [] : LaboratoryCrud.SelectMany("SELECT * FROM laboratory WHERE LaboratoryNum IN(" + string.Join(",", laboratoryNums) + ") ORDER BY Description");
    }

    public static void Insert(Laboratory laboratory)
    {
        LaboratoryCrud.Insert(laboratory);
    }

    public static void Update(Laboratory laboratory)
    {
        LaboratoryCrud.Update(laboratory);
    }

    public static void Delete(long laboratoryNum)
    {
        var table = DataCore.GetTable(
            "SELECT LName,FName FROM patient,labcase " +
            "WHERE patient.PatNum=labcase.PatNum " +
            "AND LaboratoryNum =" + laboratoryNum + " " +
            "LIMIT 30");

        if (table.Rows.Count > 0)
        {
            var pats = "";
            for (var i = 0; i < table.Rows.Count; i++)
            {
                pats += "\r";
                pats += table.Rows[i][0] + ", " + table.Rows[i][1];
            }

            throw new Exception("Cannot delete Laboratory because cases exist for" + pats);
        }

        Db.NonQ("DELETE FROM laboratory WHERE LaboratoryNum = " + laboratoryNum);
    }
}