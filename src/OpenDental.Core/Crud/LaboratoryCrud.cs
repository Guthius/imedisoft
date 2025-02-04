using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class LaboratoryCrud
{
    public static Laboratory SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Laboratory> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Laboratory> TableToList(DataTable table)
    {
        var retVal = new List<Laboratory>();
        foreach (DataRow row in table.Rows)
        {
            var laboratory = new Laboratory
            {
                LaboratoryNum = SIn.Long(row["LaboratoryNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                Phone = SIn.String(row["Phone"].ToString()),
                Notes = SIn.String(row["Notes"].ToString()),
                Slip = SIn.Long(row["Slip"].ToString()),
                Address = SIn.String(row["Address"].ToString()),
                City = SIn.String(row["City"].ToString()),
                State = SIn.String(row["State"].ToString()),
                Zip = SIn.String(row["Zip"].ToString()),
                Email = SIn.String(row["Email"].ToString()),
                WirelessPhone = SIn.String(row["WirelessPhone"].ToString()),
                IsHidden = SIn.Bool(row["IsHidden"].ToString())
            };
            retVal.Add(laboratory);
        }

        return retVal;
    }

    public static void Insert(Laboratory laboratory)
    {
        var command = "INSERT INTO laboratory (";

        command += "Description,Phone,Notes,Slip,Address,City,State,Zip,Email,WirelessPhone,IsHidden) VALUES(";

        command +=
            "'" + SOut.String(laboratory.Description) + "',"
            + "'" + SOut.String(laboratory.Phone) + "',"
            + DbHelper.ParamChar + "paramNotes,"
            + SOut.Long(laboratory.Slip) + ","
            + "'" + SOut.String(laboratory.Address) + "',"
            + "'" + SOut.String(laboratory.City) + "',"
            + "'" + SOut.String(laboratory.State) + "',"
            + "'" + SOut.String(laboratory.Zip) + "',"
            + "'" + SOut.String(laboratory.Email) + "',"
            + "'" + SOut.String(laboratory.WirelessPhone) + "',"
            + SOut.Bool(laboratory.IsHidden) + ")";
        if (laboratory.Notes == null) laboratory.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", SOut.StringParam(laboratory.Notes));
        {
            laboratory.LaboratoryNum = Db.NonQ(command, true, "LaboratoryNum", "laboratory", paramNotes);
        }
    }

    public static void Update(Laboratory laboratory)
    {
        var command = "UPDATE laboratory SET "
                      + "Description  = '" + SOut.String(laboratory.Description) + "', "
                      + "Phone        = '" + SOut.String(laboratory.Phone) + "', "
                      + "Notes        =  " + DbHelper.ParamChar + "paramNotes, "
                      + "Slip         =  " + SOut.Long(laboratory.Slip) + ", "
                      + "Address      = '" + SOut.String(laboratory.Address) + "', "
                      + "City         = '" + SOut.String(laboratory.City) + "', "
                      + "State        = '" + SOut.String(laboratory.State) + "', "
                      + "Zip          = '" + SOut.String(laboratory.Zip) + "', "
                      + "Email        = '" + SOut.String(laboratory.Email) + "', "
                      + "WirelessPhone= '" + SOut.String(laboratory.WirelessPhone) + "', "
                      + "IsHidden     =  " + SOut.Bool(laboratory.IsHidden) + " "
                      + "WHERE LaboratoryNum = " + SOut.Long(laboratory.LaboratoryNum);
        if (laboratory.Notes == null) laboratory.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", SOut.StringParam(laboratory.Notes));
        Db.NonQ(command, paramNotes);
    }
}