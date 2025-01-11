using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class MountItems
{
    public static long Insert(MountItem mountItem)
    {
        return MountItemCrud.Insert(mountItem);
    }

    public static void Update(MountItem mountItem)
    {
        MountItemCrud.Update(mountItem);
    }

    ///<summary>Will throw an exception if any document(image) attached to this mountitem.</summary>
    public static void Delete(MountItem mountItem)
    {
        var command = "SELECT COUNT(*) FROM document WHERE MountItemNum=" + SOut.Long(mountItem.MountItemNum);
        var count = Db.GetCount(command);
        if (count != "0") throw new ApplicationException(Lans.g("MountItems", "Not allowed to delete a MountItem that has an attached image."));
        command = "DELETE FROM mountitem WHERE MountItemNum='" + SOut.Long(mountItem.MountItemNum) + "'";
        Db.NonQ(command);
    }

    /// <summary>
    ///     Returns the list of mount items associated with the given mount key. In order by ItemOrder, which is
    ///     1-indexed.
    /// </summary>
    public static List<MountItem> GetItemsForMount(long mountNum)
    {
        var command = "SELECT * FROM mountitem WHERE MountNum='" + SOut.Long(mountNum) + "' ORDER BY ItemOrder";
        return MountItemCrud.SelectMany(command);
    }

    /*
    ///<summary>Returns the list of MountItems containing the passed list of MountItemNums. Ordered by MountItemNum.</summary>
    public static List<MountItem> GetMountItems(List<long> listMountItemNums) {

        string command="SELECT * FROM mountitem WHERE MountItemNum IN("+string.Join(",",listMountItemNums)+") ORDER BY MountItemNum";
        return Crud.MountItemCrud.SelectMany(command);
    }*/
}