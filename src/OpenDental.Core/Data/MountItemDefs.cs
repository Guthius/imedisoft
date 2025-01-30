using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class MountItemDefs
{
    public static List<MountItemDef> GetForMountDef(long mountDefNum)
    {
        return MountItemDefCrud.SelectMany("SELECT * FROM mountitemdef WHERE MountDefNum=" + mountDefNum + " ORDER BY ItemOrder");
    }

    public static List<MountItemDef> GetAll()
    {
        return MountItemDefCrud.SelectMany("SELECT * FROM mountitemdef ORDER BY ItemOrder");
    }

    public static void Update(MountItemDef mountItemDef)
    {
        MountItemDefCrud.Update(mountItemDef);
    }

    public static void Insert(MountItemDef mountItemDef)
    {
        MountItemDefCrud.Insert(mountItemDef);
    }

    public static void Delete(long mountItemDefNum)
    {
        Db.NonQ("DELETE FROM mountitemdef WHERE MountItemDefNum=" + mountItemDefNum);
    }

    public static void DeleteForMount(long mountDefNum)
    {
        Db.NonQ("DELETE FROM mountitemdef WHERE MountDefNum=" + mountDefNum);
    }
}