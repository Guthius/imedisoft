#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class LabPanelCrud
{
    public static LabPanel SelectOne(long labPanelNum)
    {
        var command = "SELECT * FROM labpanel "
                      + "WHERE LabPanelNum = " + SOut.Long(labPanelNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static LabPanel SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<LabPanel> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<LabPanel> TableToList(DataTable table)
    {
        var retVal = new List<LabPanel>();
        LabPanel labPanel;
        foreach (DataRow row in table.Rows)
        {
            labPanel = new LabPanel();
            labPanel.LabPanelNum = SIn.Long(row["LabPanelNum"].ToString());
            labPanel.PatNum = SIn.Long(row["PatNum"].ToString());
            labPanel.RawMessage = SIn.String(row["RawMessage"].ToString());
            labPanel.LabNameAddress = SIn.String(row["LabNameAddress"].ToString());
            labPanel.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            labPanel.SpecimenCondition = SIn.String(row["SpecimenCondition"].ToString());
            labPanel.SpecimenSource = SIn.String(row["SpecimenSource"].ToString());
            labPanel.ServiceId = SIn.String(row["ServiceId"].ToString());
            labPanel.ServiceName = SIn.String(row["ServiceName"].ToString());
            labPanel.MedicalOrderNum = SIn.Long(row["MedicalOrderNum"].ToString());
            retVal.Add(labPanel);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<LabPanel> listLabPanels, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "LabPanel";
        var table = new DataTable(tableName);
        table.Columns.Add("LabPanelNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("RawMessage");
        table.Columns.Add("LabNameAddress");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("SpecimenCondition");
        table.Columns.Add("SpecimenSource");
        table.Columns.Add("ServiceId");
        table.Columns.Add("ServiceName");
        table.Columns.Add("MedicalOrderNum");
        foreach (var labPanel in listLabPanels)
            table.Rows.Add(SOut.Long(labPanel.LabPanelNum), SOut.Long(labPanel.PatNum), labPanel.RawMessage, labPanel.LabNameAddress, SOut.DateT(labPanel.DateTStamp, false), labPanel.SpecimenCondition, labPanel.SpecimenSource, labPanel.ServiceId, labPanel.ServiceName, SOut.Long(labPanel.MedicalOrderNum));
        return table;
    }

    public static long Insert(LabPanel labPanel)
    {
        return Insert(labPanel, false);
    }

    public static long Insert(LabPanel labPanel, bool useExistingPK)
    {
        var command = "INSERT INTO labpanel (";

        command += "PatNum,RawMessage,LabNameAddress,SpecimenCondition,SpecimenSource,ServiceId,ServiceName,MedicalOrderNum) VALUES(";

        command +=
            SOut.Long(labPanel.PatNum) + ","
                                       + DbHelper.ParamChar + "paramRawMessage,"
                                       + "'" + SOut.String(labPanel.LabNameAddress) + "',"
                                       //DateTStamp can only be set by MySQL
                                       + "'" + SOut.String(labPanel.SpecimenCondition) + "',"
                                       + "'" + SOut.String(labPanel.SpecimenSource) + "',"
                                       + "'" + SOut.String(labPanel.ServiceId) + "',"
                                       + "'" + SOut.String(labPanel.ServiceName) + "',"
                                       + SOut.Long(labPanel.MedicalOrderNum) + ")";
        if (labPanel.RawMessage == null) labPanel.RawMessage = "";
        var paramRawMessage = new OdSqlParameter("paramRawMessage", OdDbType.Text, SOut.StringParam(labPanel.RawMessage));
        {
            labPanel.LabPanelNum = Db.NonQ(command, true, "LabPanelNum", "labPanel", paramRawMessage);
        }
        return labPanel.LabPanelNum;
    }

    public static long InsertNoCache(LabPanel labPanel)
    {
        return InsertNoCache(labPanel, false);
    }

    public static long InsertNoCache(LabPanel labPanel, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO labpanel (";
        if (isRandomKeys || useExistingPK) command += "LabPanelNum,";
        command += "PatNum,RawMessage,LabNameAddress,SpecimenCondition,SpecimenSource,ServiceId,ServiceName,MedicalOrderNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(labPanel.LabPanelNum) + ",";
        command +=
            SOut.Long(labPanel.PatNum) + ","
                                       + DbHelper.ParamChar + "paramRawMessage,"
                                       + "'" + SOut.String(labPanel.LabNameAddress) + "',"
                                       //DateTStamp can only be set by MySQL
                                       + "'" + SOut.String(labPanel.SpecimenCondition) + "',"
                                       + "'" + SOut.String(labPanel.SpecimenSource) + "',"
                                       + "'" + SOut.String(labPanel.ServiceId) + "',"
                                       + "'" + SOut.String(labPanel.ServiceName) + "',"
                                       + SOut.Long(labPanel.MedicalOrderNum) + ")";
        if (labPanel.RawMessage == null) labPanel.RawMessage = "";
        var paramRawMessage = new OdSqlParameter("paramRawMessage", OdDbType.Text, SOut.StringParam(labPanel.RawMessage));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramRawMessage);
        else
            labPanel.LabPanelNum = Db.NonQ(command, true, "LabPanelNum", "labPanel", paramRawMessage);
        return labPanel.LabPanelNum;
    }

    public static void Update(LabPanel labPanel)
    {
        var command = "UPDATE labpanel SET "
                      + "PatNum           =  " + SOut.Long(labPanel.PatNum) + ", "
                      + "RawMessage       =  " + DbHelper.ParamChar + "paramRawMessage, "
                      + "LabNameAddress   = '" + SOut.String(labPanel.LabNameAddress) + "', "
                      //DateTStamp can only be set by MySQL
                      + "SpecimenCondition= '" + SOut.String(labPanel.SpecimenCondition) + "', "
                      + "SpecimenSource   = '" + SOut.String(labPanel.SpecimenSource) + "', "
                      + "ServiceId        = '" + SOut.String(labPanel.ServiceId) + "', "
                      + "ServiceName      = '" + SOut.String(labPanel.ServiceName) + "', "
                      + "MedicalOrderNum  =  " + SOut.Long(labPanel.MedicalOrderNum) + " "
                      + "WHERE LabPanelNum = " + SOut.Long(labPanel.LabPanelNum);
        if (labPanel.RawMessage == null) labPanel.RawMessage = "";
        var paramRawMessage = new OdSqlParameter("paramRawMessage", OdDbType.Text, SOut.StringParam(labPanel.RawMessage));
        Db.NonQ(command, paramRawMessage);
    }

    public static bool Update(LabPanel labPanel, LabPanel oldLabPanel)
    {
        var command = "";
        if (labPanel.PatNum != oldLabPanel.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(labPanel.PatNum) + "";
        }

        if (labPanel.RawMessage != oldLabPanel.RawMessage)
        {
            if (command != "") command += ",";
            command += "RawMessage = " + DbHelper.ParamChar + "paramRawMessage";
        }

        if (labPanel.LabNameAddress != oldLabPanel.LabNameAddress)
        {
            if (command != "") command += ",";
            command += "LabNameAddress = '" + SOut.String(labPanel.LabNameAddress) + "'";
        }

        //DateTStamp can only be set by MySQL
        if (labPanel.SpecimenCondition != oldLabPanel.SpecimenCondition)
        {
            if (command != "") command += ",";
            command += "SpecimenCondition = '" + SOut.String(labPanel.SpecimenCondition) + "'";
        }

        if (labPanel.SpecimenSource != oldLabPanel.SpecimenSource)
        {
            if (command != "") command += ",";
            command += "SpecimenSource = '" + SOut.String(labPanel.SpecimenSource) + "'";
        }

        if (labPanel.ServiceId != oldLabPanel.ServiceId)
        {
            if (command != "") command += ",";
            command += "ServiceId = '" + SOut.String(labPanel.ServiceId) + "'";
        }

        if (labPanel.ServiceName != oldLabPanel.ServiceName)
        {
            if (command != "") command += ",";
            command += "ServiceName = '" + SOut.String(labPanel.ServiceName) + "'";
        }

        if (labPanel.MedicalOrderNum != oldLabPanel.MedicalOrderNum)
        {
            if (command != "") command += ",";
            command += "MedicalOrderNum = " + SOut.Long(labPanel.MedicalOrderNum) + "";
        }

        if (command == "") return false;
        if (labPanel.RawMessage == null) labPanel.RawMessage = "";
        var paramRawMessage = new OdSqlParameter("paramRawMessage", OdDbType.Text, SOut.StringParam(labPanel.RawMessage));
        command = "UPDATE labpanel SET " + command
                                         + " WHERE LabPanelNum = " + SOut.Long(labPanel.LabPanelNum);
        Db.NonQ(command, paramRawMessage);
        return true;
    }

    public static bool UpdateComparison(LabPanel labPanel, LabPanel oldLabPanel)
    {
        if (labPanel.PatNum != oldLabPanel.PatNum) return true;
        if (labPanel.RawMessage != oldLabPanel.RawMessage) return true;
        if (labPanel.LabNameAddress != oldLabPanel.LabNameAddress) return true;
        //DateTStamp can only be set by MySQL
        if (labPanel.SpecimenCondition != oldLabPanel.SpecimenCondition) return true;
        if (labPanel.SpecimenSource != oldLabPanel.SpecimenSource) return true;
        if (labPanel.ServiceId != oldLabPanel.ServiceId) return true;
        if (labPanel.ServiceName != oldLabPanel.ServiceName) return true;
        if (labPanel.MedicalOrderNum != oldLabPanel.MedicalOrderNum) return true;
        return false;
    }

    public static void Delete(long labPanelNum)
    {
        var command = "DELETE FROM labpanel "
                      + "WHERE LabPanelNum = " + SOut.Long(labPanelNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listLabPanelNums)
    {
        if (listLabPanelNums == null || listLabPanelNums.Count == 0) return;
        var command = "DELETE FROM labpanel "
                      + "WHERE LabPanelNum IN(" + string.Join(",", listLabPanelNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}