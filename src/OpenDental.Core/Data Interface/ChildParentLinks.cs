using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ChildParentLinks
{
    #region Methods - Get

    ///<summary>Returns a list of all ChildParentLinks with the given childNum.</summary>
    public static List<ChildParentLink> GetChildParentLinksByChildNum(long childNum)
    {
        var command = "SELECT * FROM childparentlink WHERE ChildNum=" + SOut.Long(childNum);
        return ChildParentLinkCrud.SelectMany(command);
    }

    ///<summary>Returns a list of all ChildParentLinks with the given childParentNum.</summary>
    public static List<ChildParentLink> GetAllByChildParentNum(long childParentNum)
    {
        var command = "SELECT * FROM childparentlink WHERE ChildParentNum=" + SOut.Long(childParentNum);
        return ChildParentLinkCrud.SelectMany(command);
    }

    #endregion Methods - Get

    #region Methods - Modify

    
    public static long Insert(ChildParentLink childParentLink)
    {
        return ChildParentLinkCrud.Insert(childParentLink);
    }

    
    public static void Update(ChildParentLink childParentLink)
    {
        ChildParentLinkCrud.Update(childParentLink);
    }

    
    public static void Delete(long childParentLinkNum)
    {
        ChildParentLinkCrud.Delete(childParentLinkNum);
    }

    #endregion Methods - Modify
}