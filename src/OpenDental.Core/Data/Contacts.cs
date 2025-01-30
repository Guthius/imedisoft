using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Contacts
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