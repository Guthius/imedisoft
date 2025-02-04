using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ContactCrud
{
    public static List<Contact> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Contact> TableToList(DataTable table)
    {
        var retVal = new List<Contact>();
        foreach (DataRow row in table.Rows)
        {
            var contact = new Contact
            {
                ContactNum = SIn.Long(row["ContactNum"].ToString()),
                LName = SIn.String(row["LName"].ToString()),
                FName = SIn.String(row["FName"].ToString()),
                WkPhone = SIn.String(row["WkPhone"].ToString()),
                Fax = SIn.String(row["Fax"].ToString()),
                Category = SIn.Long(row["Category"].ToString()),
                Notes = SIn.String(row["Notes"].ToString())
            };
            retVal.Add(contact);
        }

        return retVal;
    }

    public static void Insert(Contact contact)
    {
        var command = "INSERT INTO contact (";

        command += "LName,FName,WkPhone,Fax,Category,Notes) VALUES(";

        command +=
            "'" + SOut.String(contact.LName) + "',"
            + "'" + SOut.String(contact.FName) + "',"
            + "'" + SOut.String(contact.WkPhone) + "',"
            + "'" + SOut.String(contact.Fax) + "',"
            + SOut.Long(contact.Category) + ","
            + DbHelper.ParamChar + "paramNotes)";
        if (contact.Notes == null) contact.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", SOut.StringParam(contact.Notes));
        {
            contact.ContactNum = Db.NonQ(command, true, "ContactNum", "contact", paramNotes);
        }
    }

    public static void Update(Contact contact)
    {
        var command = "UPDATE contact SET "
                      + "LName     = '" + SOut.String(contact.LName) + "', "
                      + "FName     = '" + SOut.String(contact.FName) + "', "
                      + "WkPhone   = '" + SOut.String(contact.WkPhone) + "', "
                      + "Fax       = '" + SOut.String(contact.Fax) + "', "
                      + "Category  =  " + SOut.Long(contact.Category) + ", "
                      + "Notes     =  " + DbHelper.ParamChar + "paramNotes "
                      + "WHERE ContactNum = " + SOut.Long(contact.ContactNum);
        if (contact.Notes == null) contact.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", SOut.StringParam(contact.Notes));
        Db.NonQ(command, paramNotes);
    }
}