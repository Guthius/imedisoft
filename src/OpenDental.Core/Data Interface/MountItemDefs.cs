using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class MountItemDefs
{
    public static List<MountItemDef> GetForMountDef(long mountDefNum)
    {
        var command = "SELECT * FROM mountitemdef WHERE MountDefNum='" + SOut.Long(mountDefNum) + "' ORDER BY ItemOrder";
        return MountItemDefCrud.SelectMany(command);
    }

    public static List<MountItemDef> GetAll()
    {
        var command = "SELECT * FROM mountitemdef ORDER BY ItemOrder";
        return MountItemDefCrud.SelectMany(command);
    }

    
    public static void Update(MountItemDef mountItemDef)
    {
        MountItemDefCrud.Update(mountItemDef);
    }

    
    public static long Insert(MountItemDef mountItemDef)
    {
        return MountItemDefCrud.Insert(mountItemDef);
    }

    ///<summary>No need to surround with try/catch, because all deletions are allowed.</summary>
    public static void Delete(long mountItemDefNum)
    {
        var command = "DELETE FROM mountitemdef WHERE MountItemDefNum=" + SOut.Long(mountItemDefNum);
        Db.NonQ(command);
    }

    
    public static void DeleteForMount(long mountDefNum)
    {
        var command = "DELETE FROM mountitemdef WHERE MountDefNum=" + SOut.Long(mountDefNum);
        Db.NonQ(command);
    }
}