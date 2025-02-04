using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ElectIDCrud
{
    public static List<ElectID> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ElectID> TableToList(DataTable table)
    {
        var retVal = new List<ElectID>();
        foreach (DataRow row in table.Rows)
        {
            var electID = new ElectID
            {
                ElectIDNum = SIn.Long(row["ElectIDNum"].ToString()),
                PayorID = SIn.String(row["PayorID"].ToString()),
                CarrierName = SIn.String(row["CarrierName"].ToString()),
                IsMedicaid = SIn.Bool(row["IsMedicaid"].ToString()),
                ProviderTypes = SIn.String(row["ProviderTypes"].ToString()),
                Comments = SIn.String(row["Comments"].ToString()),
                CommBridge = (EclaimsCommBridge) SIn.Int(row["CommBridge"].ToString()),
                Attributes = SIn.String(row["Attributes"].ToString())
            };
            retVal.Add(electID);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ElectID> listElectIDs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ElectID";
        var table = new DataTable(tableName);
        table.Columns.Add("ElectIDNum");
        table.Columns.Add("PayorID");
        table.Columns.Add("CarrierName");
        table.Columns.Add("IsMedicaid");
        table.Columns.Add("ProviderTypes");
        table.Columns.Add("Comments");
        table.Columns.Add("CommBridge");
        table.Columns.Add("Attributes");
        foreach (var electID in listElectIDs)
            table.Rows.Add(SOut.Long(electID.ElectIDNum), electID.PayorID, electID.CarrierName, SOut.Bool(electID.IsMedicaid), electID.ProviderTypes, electID.Comments, SOut.Int((int) electID.CommBridge), electID.Attributes);
        return table;
    }

    public static void Insert(ElectID electID)
    {
        var command = "INSERT INTO electid (";

        command += "PayorID,CarrierName,IsMedicaid,ProviderTypes,Comments,CommBridge,Attributes) VALUES(";

        command +=
            "'" + SOut.String(electID.PayorID) + "',"
            + "'" + SOut.String(electID.CarrierName) + "',"
            + SOut.Bool(electID.IsMedicaid) + ","
            + "'" + SOut.String(electID.ProviderTypes) + "',"
            + DbHelper.ParamChar + "paramComments,"
            + SOut.Int((int) electID.CommBridge) + ","
            + "'" + SOut.String(electID.Attributes) + "')";
        if (electID.Comments == null) electID.Comments = "";
        var paramComments = new OdSqlParameter("paramComments", SOut.StringParam(electID.Comments));
        {
            electID.ElectIDNum = Db.NonQ(command, true, "ElectIDNum", "electID", paramComments);
        }
    }

    public static void Update(ElectID electID)
    {
        var command = "UPDATE electid SET "
                      + "PayorID      = '" + SOut.String(electID.PayorID) + "', "
                      + "CarrierName  = '" + SOut.String(electID.CarrierName) + "', "
                      + "IsMedicaid   =  " + SOut.Bool(electID.IsMedicaid) + ", "
                      + "ProviderTypes= '" + SOut.String(electID.ProviderTypes) + "', "
                      + "Comments     =  " + DbHelper.ParamChar + "paramComments, "
                      + "CommBridge   =  " + SOut.Int((int) electID.CommBridge) + ", "
                      + "Attributes   = '" + SOut.String(electID.Attributes) + "' "
                      + "WHERE ElectIDNum = " + SOut.Long(electID.ElectIDNum);
        if (electID.Comments == null) electID.Comments = "";
        var paramComments = new OdSqlParameter("paramComments", SOut.StringParam(electID.Comments));
        Db.NonQ(command, paramComments);
    }

    public static bool Update(ElectID electID, ElectID oldElectID)
    {
        var command = "";
        if (electID.PayorID != oldElectID.PayorID)
        {
            if (command != "") command += ",";
            command += "PayorID = '" + SOut.String(electID.PayorID) + "'";
        }

        if (electID.CarrierName != oldElectID.CarrierName)
        {
            if (command != "") command += ",";
            command += "CarrierName = '" + SOut.String(electID.CarrierName) + "'";
        }

        if (electID.IsMedicaid != oldElectID.IsMedicaid)
        {
            if (command != "") command += ",";
            command += "IsMedicaid = " + SOut.Bool(electID.IsMedicaid) + "";
        }

        if (electID.ProviderTypes != oldElectID.ProviderTypes)
        {
            if (command != "") command += ",";
            command += "ProviderTypes = '" + SOut.String(electID.ProviderTypes) + "'";
        }

        if (electID.Comments != oldElectID.Comments)
        {
            if (command != "") command += ",";
            command += "Comments = " + DbHelper.ParamChar + "paramComments";
        }

        if (electID.CommBridge != oldElectID.CommBridge)
        {
            if (command != "") command += ",";
            command += "CommBridge = " + SOut.Int((int) electID.CommBridge) + "";
        }

        if (electID.Attributes != oldElectID.Attributes)
        {
            if (command != "") command += ",";
            command += "Attributes = '" + SOut.String(electID.Attributes) + "'";
        }

        if (command == "") return false;
        if (electID.Comments == null) electID.Comments = "";
        var paramComments = new OdSqlParameter("paramComments", SOut.StringParam(electID.Comments));
        command = "UPDATE electid SET " + command
                                        + " WHERE ElectIDNum = " + SOut.Long(electID.ElectIDNum);
        Db.NonQ(command, paramComments);
        return true;
    }
}