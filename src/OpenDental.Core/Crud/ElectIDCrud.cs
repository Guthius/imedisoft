#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ElectIDCrud
{
    public static ElectID SelectOne(long electIDNum)
    {
        var command = "SELECT * FROM electid "
                      + "WHERE ElectIDNum = " + SOut.Long(electIDNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ElectID SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ElectID> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ElectID> TableToList(DataTable table)
    {
        var retVal = new List<ElectID>();
        ElectID electID;
        foreach (DataRow row in table.Rows)
        {
            electID = new ElectID();
            electID.ElectIDNum = SIn.Long(row["ElectIDNum"].ToString());
            electID.PayorID = SIn.String(row["PayorID"].ToString());
            electID.CarrierName = SIn.String(row["CarrierName"].ToString());
            electID.IsMedicaid = SIn.Bool(row["IsMedicaid"].ToString());
            electID.ProviderTypes = SIn.String(row["ProviderTypes"].ToString());
            electID.Comments = SIn.String(row["Comments"].ToString());
            electID.CommBridge = (EclaimsCommBridge) SIn.Int(row["CommBridge"].ToString());
            electID.Attributes = SIn.String(row["Attributes"].ToString());
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

    public static long Insert(ElectID electID)
    {
        return Insert(electID, false);
    }

    public static long Insert(ElectID electID, bool useExistingPK)
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
        var paramComments = new OdSqlParameter("paramComments", OdDbType.Text, SOut.StringParam(electID.Comments));
        {
            electID.ElectIDNum = Db.NonQ(command, true, "ElectIDNum", "electID", paramComments);
        }
        return electID.ElectIDNum;
    }

    public static long InsertNoCache(ElectID electID)
    {
        return InsertNoCache(electID, false);
    }

    public static long InsertNoCache(ElectID electID, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO electid (";
        if (isRandomKeys || useExistingPK) command += "ElectIDNum,";
        command += "PayorID,CarrierName,IsMedicaid,ProviderTypes,Comments,CommBridge,Attributes) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(electID.ElectIDNum) + ",";
        command +=
            "'" + SOut.String(electID.PayorID) + "',"
            + "'" + SOut.String(electID.CarrierName) + "',"
            + SOut.Bool(electID.IsMedicaid) + ","
            + "'" + SOut.String(electID.ProviderTypes) + "',"
            + DbHelper.ParamChar + "paramComments,"
            + SOut.Int((int) electID.CommBridge) + ","
            + "'" + SOut.String(electID.Attributes) + "')";
        if (electID.Comments == null) electID.Comments = "";
        var paramComments = new OdSqlParameter("paramComments", OdDbType.Text, SOut.StringParam(electID.Comments));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramComments);
        else
            electID.ElectIDNum = Db.NonQ(command, true, "ElectIDNum", "electID", paramComments);
        return electID.ElectIDNum;
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
        var paramComments = new OdSqlParameter("paramComments", OdDbType.Text, SOut.StringParam(electID.Comments));
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
        var paramComments = new OdSqlParameter("paramComments", OdDbType.Text, SOut.StringParam(electID.Comments));
        command = "UPDATE electid SET " + command
                                        + " WHERE ElectIDNum = " + SOut.Long(electID.ElectIDNum);
        Db.NonQ(command, paramComments);
        return true;
    }

    public static bool UpdateComparison(ElectID electID, ElectID oldElectID)
    {
        if (electID.PayorID != oldElectID.PayorID) return true;
        if (electID.CarrierName != oldElectID.CarrierName) return true;
        if (electID.IsMedicaid != oldElectID.IsMedicaid) return true;
        if (electID.ProviderTypes != oldElectID.ProviderTypes) return true;
        if (electID.Comments != oldElectID.Comments) return true;
        if (electID.CommBridge != oldElectID.CommBridge) return true;
        if (electID.Attributes != oldElectID.Attributes) return true;
        return false;
    }

    public static void Delete(long electIDNum)
    {
        var command = "DELETE FROM electid "
                      + "WHERE ElectIDNum = " + SOut.Long(electIDNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listElectIDNums)
    {
        if (listElectIDNums == null || listElectIDNums.Count == 0) return;
        var command = "DELETE FROM electid "
                      + "WHERE ElectIDNum IN(" + string.Join(",", listElectIDNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}