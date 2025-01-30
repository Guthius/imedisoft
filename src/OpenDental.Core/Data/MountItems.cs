using System;
using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class MountItems
{
    public static void Insert(MountItem mountItem)
    {
        MountItemCrud.Insert(mountItem);
    }

    public static void Update(MountItem mountItem)
    {
        MountItemCrud.Update(mountItem);
    }

    public static void Delete(MountItem mountItem)
    {
        var command = "SELECT COUNT(*) FROM document WHERE MountItemNum=" + mountItem.MountItemNum;

        var count = Db.GetCount(command);
        if (count != "0")
        {
            throw new ApplicationException("Not allowed to delete a MountItem that has an attached image.");
        }

        Db.NonQ("DELETE FROM mountitem WHERE MountItemNum=" + mountItem.MountItemNum);
    }

    public static List<MountItem> GetItemsForMount(long mountNum)
    {
        return MountItemCrud.SelectMany("SELECT * FROM mountitem WHERE MountNum=" + mountNum + " ORDER BY ItemOrder");
    }
}