using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Children
{
    ///<summary>Gets one Child from the db.</summary>
    public static Child GetOne(long childNum)
    {
        return ChildCrud.SelectOne(childNum);
    }

    ///<summary>Returns a list containing all children.</summary>
    public static List<Child> GetAll()
    {
        var command = "SELECT * FROM child ORDER BY LName";
        return ChildCrud.SelectMany(command);
    }

    ///<summary>Returns the full name of the child passed in. Returns an empty string if child is null.</summary>
    public static string GetName(Child child)
    {
        if (child == null) return "";

        return child.FName + " " + child.LName;
    }

    /// <summary>
    ///     Gets the first child with the matching badgeId passed in. Expecting int with 4 digits or less.  Returns null
    ///     if not found.
    /// </summary>
    public static Child GetUserByBadgeId(string badgeId)
    {
        var command = "SELECT * FROM child WHERE BadgeId <> '' AND BadgeId = RIGHT('" + SOut.String(badgeId) + "', LENGTH(BadgeId))";
        //Example BadgeId in db="123". Select compares "123" with RIGHT('00000123',3)
        var listChildren = ChildCrud.TableToList(DataCore.GetTable(command));
        return listChildren.FirstOrDefault();
    }

    #region Methods - Modify

    
    public static long Insert(Child child)
    {
        return ChildCrud.Insert(child);
    }

    
    public static void Update(Child child)
    {
        ChildCrud.Update(child);
    }

    
    public static void Delete(long childNum)
    {
        ChildCrud.Delete(childNum);
    }

    #endregion Methods - Modify
}