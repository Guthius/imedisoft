using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class UserodApptViewCrud
{
    public static UserodApptView SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<UserodApptView> TableToList(DataTable table)
    {
        var retVal = new List<UserodApptView>();
        foreach (DataRow row in table.Rows)
        {
            var userodApptView = new UserodApptView
            {
                UserodApptViewNum = SIn.Long(row["UserodApptViewNum"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                ApptViewNum = SIn.Long(row["ApptViewNum"].ToString())
            };
            retVal.Add(userodApptView);
        }

        return retVal;
    }

    public static void Insert(UserodApptView userodApptView)
    {
        var command = "INSERT INTO userodapptview (";

        command += "UserNum,ClinicNum,ApptViewNum) VALUES(";

        command +=
            SOut.Long(userodApptView.UserNum) + ","
                                              + SOut.Long(userodApptView.ClinicNum) + ","
                                              + SOut.Long(userodApptView.ApptViewNum) + ")";
        {
            userodApptView.UserodApptViewNum = Db.NonQ(command, true, "UserodApptViewNum", "userodApptView");
        }
    }

    public static void Update(UserodApptView userodApptView)
    {
        var command = "UPDATE userodapptview SET "
                      + "UserNum          =  " + SOut.Long(userodApptView.UserNum) + ", "
                      + "ClinicNum        =  " + SOut.Long(userodApptView.ClinicNum) + ", "
                      + "ApptViewNum      =  " + SOut.Long(userodApptView.ApptViewNum) + " "
                      + "WHERE UserodApptViewNum = " + SOut.Long(userodApptView.UserodApptViewNum);
        Db.NonQ(command);
    }
}