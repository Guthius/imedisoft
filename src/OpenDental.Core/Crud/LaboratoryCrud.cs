#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class LaboratoryCrud
{
    public static Laboratory SelectOne(long laboratoryNum)
    {
        var command = "SELECT * FROM laboratory "
                      + "WHERE LaboratoryNum = " + SOut.Long(laboratoryNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

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
        Laboratory laboratory;
        foreach (DataRow row in table.Rows)
        {
            laboratory = new Laboratory();
            laboratory.LaboratoryNum = SIn.Long(row["LaboratoryNum"].ToString());
            laboratory.Description = SIn.String(row["Description"].ToString());
            laboratory.Phone = SIn.String(row["Phone"].ToString());
            laboratory.Notes = SIn.String(row["Notes"].ToString());
            laboratory.Slip = SIn.Long(row["Slip"].ToString());
            laboratory.Address = SIn.String(row["Address"].ToString());
            laboratory.City = SIn.String(row["City"].ToString());
            laboratory.State = SIn.String(row["State"].ToString());
            laboratory.Zip = SIn.String(row["Zip"].ToString());
            laboratory.Email = SIn.String(row["Email"].ToString());
            laboratory.WirelessPhone = SIn.String(row["WirelessPhone"].ToString());
            laboratory.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            retVal.Add(laboratory);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Laboratory> listLaboratorys, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Laboratory";
        var table = new DataTable(tableName);
        table.Columns.Add("LaboratoryNum");
        table.Columns.Add("Description");
        table.Columns.Add("Phone");
        table.Columns.Add("Notes");
        table.Columns.Add("Slip");
        table.Columns.Add("Address");
        table.Columns.Add("City");
        table.Columns.Add("State");
        table.Columns.Add("Zip");
        table.Columns.Add("Email");
        table.Columns.Add("WirelessPhone");
        table.Columns.Add("IsHidden");
        foreach (var laboratory in listLaboratorys)
            table.Rows.Add(SOut.Long(laboratory.LaboratoryNum), laboratory.Description, laboratory.Phone, laboratory.Notes, SOut.Long(laboratory.Slip), laboratory.Address, laboratory.City, laboratory.State, laboratory.Zip, laboratory.Email, laboratory.WirelessPhone, SOut.Bool(laboratory.IsHidden));
        return table;
    }

    public static long Insert(Laboratory laboratory)
    {
        return Insert(laboratory, false);
    }

    public static long Insert(Laboratory laboratory, bool useExistingPK)
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
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(laboratory.Notes));
        {
            laboratory.LaboratoryNum = Db.NonQ(command, true, "LaboratoryNum", "laboratory", paramNotes);
        }
        return laboratory.LaboratoryNum;
    }

    public static long InsertNoCache(Laboratory laboratory)
    {
        return InsertNoCache(laboratory, false);
    }

    public static long InsertNoCache(Laboratory laboratory, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO laboratory (";
        if (isRandomKeys || useExistingPK) command += "LaboratoryNum,";
        command += "Description,Phone,Notes,Slip,Address,City,State,Zip,Email,WirelessPhone,IsHidden) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(laboratory.LaboratoryNum) + ",";
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
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(laboratory.Notes));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNotes);
        else
            laboratory.LaboratoryNum = Db.NonQ(command, true, "LaboratoryNum", "laboratory", paramNotes);
        return laboratory.LaboratoryNum;
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
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(laboratory.Notes));
        Db.NonQ(command, paramNotes);
    }

    public static bool Update(Laboratory laboratory, Laboratory oldLaboratory)
    {
        var command = "";
        if (laboratory.Description != oldLaboratory.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(laboratory.Description) + "'";
        }

        if (laboratory.Phone != oldLaboratory.Phone)
        {
            if (command != "") command += ",";
            command += "Phone = '" + SOut.String(laboratory.Phone) + "'";
        }

        if (laboratory.Notes != oldLaboratory.Notes)
        {
            if (command != "") command += ",";
            command += "Notes = " + DbHelper.ParamChar + "paramNotes";
        }

        if (laboratory.Slip != oldLaboratory.Slip)
        {
            if (command != "") command += ",";
            command += "Slip = " + SOut.Long(laboratory.Slip) + "";
        }

        if (laboratory.Address != oldLaboratory.Address)
        {
            if (command != "") command += ",";
            command += "Address = '" + SOut.String(laboratory.Address) + "'";
        }

        if (laboratory.City != oldLaboratory.City)
        {
            if (command != "") command += ",";
            command += "City = '" + SOut.String(laboratory.City) + "'";
        }

        if (laboratory.State != oldLaboratory.State)
        {
            if (command != "") command += ",";
            command += "State = '" + SOut.String(laboratory.State) + "'";
        }

        if (laboratory.Zip != oldLaboratory.Zip)
        {
            if (command != "") command += ",";
            command += "Zip = '" + SOut.String(laboratory.Zip) + "'";
        }

        if (laboratory.Email != oldLaboratory.Email)
        {
            if (command != "") command += ",";
            command += "Email = '" + SOut.String(laboratory.Email) + "'";
        }

        if (laboratory.WirelessPhone != oldLaboratory.WirelessPhone)
        {
            if (command != "") command += ",";
            command += "WirelessPhone = '" + SOut.String(laboratory.WirelessPhone) + "'";
        }

        if (laboratory.IsHidden != oldLaboratory.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(laboratory.IsHidden) + "";
        }

        if (command == "") return false;
        if (laboratory.Notes == null) laboratory.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(laboratory.Notes));
        command = "UPDATE laboratory SET " + command
                                           + " WHERE LaboratoryNum = " + SOut.Long(laboratory.LaboratoryNum);
        Db.NonQ(command, paramNotes);
        return true;
    }

    public static bool UpdateComparison(Laboratory laboratory, Laboratory oldLaboratory)
    {
        if (laboratory.Description != oldLaboratory.Description) return true;
        if (laboratory.Phone != oldLaboratory.Phone) return true;
        if (laboratory.Notes != oldLaboratory.Notes) return true;
        if (laboratory.Slip != oldLaboratory.Slip) return true;
        if (laboratory.Address != oldLaboratory.Address) return true;
        if (laboratory.City != oldLaboratory.City) return true;
        if (laboratory.State != oldLaboratory.State) return true;
        if (laboratory.Zip != oldLaboratory.Zip) return true;
        if (laboratory.Email != oldLaboratory.Email) return true;
        if (laboratory.WirelessPhone != oldLaboratory.WirelessPhone) return true;
        if (laboratory.IsHidden != oldLaboratory.IsHidden) return true;
        return false;
    }

    public static void Delete(long laboratoryNum)
    {
        var command = "DELETE FROM laboratory "
                      + "WHERE LaboratoryNum = " + SOut.Long(laboratoryNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listLaboratoryNums)
    {
        if (listLaboratoryNums == null || listLaboratoryNums.Count == 0) return;
        var command = "DELETE FROM laboratory "
                      + "WHERE LaboratoryNum IN(" + string.Join(",", listLaboratoryNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}