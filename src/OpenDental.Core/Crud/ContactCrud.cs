using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

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
        Contact contact;
        foreach (DataRow row in table.Rows)
        {
            contact = new Contact();
            contact.ContactNum = SIn.Long(row["ContactNum"].ToString());
            contact.LName = SIn.String(row["LName"].ToString());
            contact.FName = SIn.String(row["FName"].ToString());
            contact.WkPhone = SIn.String(row["WkPhone"].ToString());
            contact.Fax = SIn.String(row["Fax"].ToString());
            contact.Category = SIn.Long(row["Category"].ToString());
            contact.Notes = SIn.String(row["Notes"].ToString());
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
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(contact.Notes));
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
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(contact.Notes));
        Db.NonQ(command, paramNotes);
    }
}