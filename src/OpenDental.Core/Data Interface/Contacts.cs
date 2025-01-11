using System.Collections.Generic;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class Contacts
{
    public static List<Contact> Refresh(long category)
    {
        return ContactCrud.SelectMany("SELECT * from contact WHERE category = " + category + " ORDER BY LName");
    }

    public static void Insert(Contact contact)
    {
        ContactCrud.Insert(contact);
    }

    public static void Update(Contact contact)
    {
        ContactCrud.Update(contact);
    }

    public static void Delete(Contact contact)
    {
        Db.NonQ("DELETE FROM contact WHERE contactnum = " + contact.ContactNum);
    }
}