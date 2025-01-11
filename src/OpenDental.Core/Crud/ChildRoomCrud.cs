using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ChildRoomCrud
{
    public static ChildRoom SelectOne(long childRoomNum)
    {
        var command = "SELECT * FROM childroom "
                      + "WHERE ChildRoomNum = " + SOut.Long(childRoomNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ChildRoom> TableToList(DataTable table)
    {
        var retVal = new List<ChildRoom>();
        ChildRoom childRoom;
        foreach (DataRow row in table.Rows)
        {
            childRoom = new ChildRoom();
            childRoom.ChildRoomNum = SIn.Long(row["ChildRoomNum"].ToString());
            childRoom.RoomId = SIn.String(row["RoomId"].ToString());
            childRoom.Notes = SIn.String(row["Notes"].ToString());
            childRoom.Ratio = SIn.Double(row["Ratio"].ToString());
            retVal.Add(childRoom);
        }

        return retVal;
    }

    public static long Insert(ChildRoom childRoom)
    {
        var command = "INSERT INTO childroom (";

        command += "RoomId,Notes,Ratio) VALUES(";

        command +=
            "'" + SOut.String(childRoom.RoomId) + "',"
            + "'" + SOut.String(childRoom.Notes) + "',"
            + SOut.Double(childRoom.Ratio) + ")";
        {
            childRoom.ChildRoomNum = Db.NonQ(command, true, "ChildRoomNum", "childRoom");
        }
        return childRoom.ChildRoomNum;
    }

    public static void Update(ChildRoom childRoom)
    {
        var command = "UPDATE childroom SET "
                      + "RoomId      = '" + SOut.String(childRoom.RoomId) + "', "
                      + "Notes       = '" + SOut.String(childRoom.Notes) + "', "
                      + "Ratio       =  " + SOut.Double(childRoom.Ratio) + " "
                      + "WHERE ChildRoomNum = " + SOut.Long(childRoom.ChildRoomNum);
        Db.NonQ(command);
    }
}