using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ERoutingActionCrud
{
    public static List<ERoutingAction> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ERoutingAction> TableToList(DataTable table)
    {
        var retVal = new List<ERoutingAction>();
        ERoutingAction eRoutingAction;
        foreach (DataRow row in table.Rows)
        {
            eRoutingAction = new ERoutingAction();
            eRoutingAction.ERoutingActionNum = SIn.Long(row["ERoutingActionNum"].ToString());
            eRoutingAction.ERoutingNum = SIn.Long(row["ERoutingNum"].ToString());
            eRoutingAction.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            eRoutingAction.ERoutingActionType = (EnumERoutingActionType) SIn.Int(row["ERoutingActionType"].ToString());
            eRoutingAction.UserNum = SIn.Long(row["UserNum"].ToString());
            eRoutingAction.IsComplete = SIn.Bool(row["IsComplete"].ToString());
            eRoutingAction.DateTimeComplete = SIn.DateTime(row["DateTimeComplete"].ToString());
            eRoutingAction.ForeignKey = SIn.Long(row["ForeignKey"].ToString());
            eRoutingAction.ForeignKeyType = (EnumERoutingFKType) SIn.Int(row["ForeignKeyType"].ToString());
            eRoutingAction.LabelOverride = SIn.String(row["LabelOverride"].ToString());
            retVal.Add(eRoutingAction);
        }

        return retVal;
    }
}