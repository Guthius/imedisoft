#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrMeasureEventCrud
{
    public static EhrMeasureEvent SelectOne(long ehrMeasureEventNum)
    {
        var command = "SELECT * FROM ehrmeasureevent "
                      + "WHERE EhrMeasureEventNum = " + SOut.Long(ehrMeasureEventNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrMeasureEvent SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrMeasureEvent> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrMeasureEvent> TableToList(DataTable table)
    {
        var retVal = new List<EhrMeasureEvent>();
        EhrMeasureEvent ehrMeasureEvent;
        foreach (DataRow row in table.Rows)
        {
            ehrMeasureEvent = new EhrMeasureEvent();
            ehrMeasureEvent.EhrMeasureEventNum = SIn.Long(row["EhrMeasureEventNum"].ToString());
            ehrMeasureEvent.DateTEvent = SIn.DateTime(row["DateTEvent"].ToString());
            ehrMeasureEvent.EventType = (EhrMeasureEventType) SIn.Int(row["EventType"].ToString());
            ehrMeasureEvent.PatNum = SIn.Long(row["PatNum"].ToString());
            ehrMeasureEvent.MoreInfo = SIn.String(row["MoreInfo"].ToString());
            ehrMeasureEvent.CodeValueEvent = SIn.String(row["CodeValueEvent"].ToString());
            ehrMeasureEvent.CodeSystemEvent = SIn.String(row["CodeSystemEvent"].ToString());
            ehrMeasureEvent.CodeValueResult = SIn.String(row["CodeValueResult"].ToString());
            ehrMeasureEvent.CodeSystemResult = SIn.String(row["CodeSystemResult"].ToString());
            ehrMeasureEvent.FKey = SIn.Long(row["FKey"].ToString());
            ehrMeasureEvent.DateStartTobacco = SIn.Date(row["DateStartTobacco"].ToString());
            ehrMeasureEvent.TobaccoCessationDesire = SIn.Byte(row["TobaccoCessationDesire"].ToString());
            retVal.Add(ehrMeasureEvent);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrMeasureEvent> listEhrMeasureEvents, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrMeasureEvent";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrMeasureEventNum");
        table.Columns.Add("DateTEvent");
        table.Columns.Add("EventType");
        table.Columns.Add("PatNum");
        table.Columns.Add("MoreInfo");
        table.Columns.Add("CodeValueEvent");
        table.Columns.Add("CodeSystemEvent");
        table.Columns.Add("CodeValueResult");
        table.Columns.Add("CodeSystemResult");
        table.Columns.Add("FKey");
        table.Columns.Add("DateStartTobacco");
        table.Columns.Add("TobaccoCessationDesire");
        foreach (var ehrMeasureEvent in listEhrMeasureEvents)
            table.Rows.Add(SOut.Long(ehrMeasureEvent.EhrMeasureEventNum), SOut.DateT(ehrMeasureEvent.DateTEvent, false), SOut.Int((int) ehrMeasureEvent.EventType), SOut.Long(ehrMeasureEvent.PatNum), ehrMeasureEvent.MoreInfo, ehrMeasureEvent.CodeValueEvent, ehrMeasureEvent.CodeSystemEvent, ehrMeasureEvent.CodeValueResult, ehrMeasureEvent.CodeSystemResult, SOut.Long(ehrMeasureEvent.FKey), SOut.DateT(ehrMeasureEvent.DateStartTobacco, false), SOut.Byte(ehrMeasureEvent.TobaccoCessationDesire));
        return table;
    }

    public static long Insert(EhrMeasureEvent ehrMeasureEvent)
    {
        return Insert(ehrMeasureEvent, false);
    }

    public static long Insert(EhrMeasureEvent ehrMeasureEvent, bool useExistingPK)
    {
        var command = "INSERT INTO ehrmeasureevent (";

        command += "DateTEvent,EventType,PatNum,MoreInfo,CodeValueEvent,CodeSystemEvent,CodeValueResult,CodeSystemResult,FKey,DateStartTobacco,TobaccoCessationDesire) VALUES(";

        command +=
            SOut.DateT(ehrMeasureEvent.DateTEvent) + ","
                                                   + SOut.Int((int) ehrMeasureEvent.EventType) + ","
                                                   + SOut.Long(ehrMeasureEvent.PatNum) + ","
                                                   + "'" + SOut.String(ehrMeasureEvent.MoreInfo) + "',"
                                                   + "'" + SOut.String(ehrMeasureEvent.CodeValueEvent) + "',"
                                                   + "'" + SOut.String(ehrMeasureEvent.CodeSystemEvent) + "',"
                                                   + "'" + SOut.String(ehrMeasureEvent.CodeValueResult) + "',"
                                                   + "'" + SOut.String(ehrMeasureEvent.CodeSystemResult) + "',"
                                                   + SOut.Long(ehrMeasureEvent.FKey) + ","
                                                   + SOut.Date(ehrMeasureEvent.DateStartTobacco) + ","
                                                   + SOut.Byte(ehrMeasureEvent.TobaccoCessationDesire) + ")";
        {
            ehrMeasureEvent.EhrMeasureEventNum = Db.NonQ(command, true, "EhrMeasureEventNum", "ehrMeasureEvent");
        }
        return ehrMeasureEvent.EhrMeasureEventNum;
    }

    public static void InsertMany(List<EhrMeasureEvent> listEhrMeasureEvents)
    {
        InsertMany(listEhrMeasureEvents, false);
    }

    public static void InsertMany(List<EhrMeasureEvent> listEhrMeasureEvents, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listEhrMeasureEvents.Count)
        {
            var ehrMeasureEvent = listEhrMeasureEvents[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO ehrmeasureevent (");
                if (useExistingPK) sbCommands.Append("EhrMeasureEventNum,");
                sbCommands.Append("DateTEvent,EventType,PatNum,MoreInfo,CodeValueEvent,CodeSystemEvent,CodeValueResult,CodeSystemResult,FKey,DateStartTobacco,TobaccoCessationDesire) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(ehrMeasureEvent.EhrMeasureEventNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.DateT(ehrMeasureEvent.DateTEvent));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) ehrMeasureEvent.EventType));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(ehrMeasureEvent.PatNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(ehrMeasureEvent.MoreInfo) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(ehrMeasureEvent.CodeValueEvent) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(ehrMeasureEvent.CodeSystemEvent) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(ehrMeasureEvent.CodeValueResult) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(ehrMeasureEvent.CodeSystemResult) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(ehrMeasureEvent.FKey));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(ehrMeasureEvent.DateStartTobacco));
            sbRow.Append(",");
            sbRow.Append(SOut.Byte(ehrMeasureEvent.TobaccoCessationDesire));
            sbRow.Append(")");
            if (sbCommands.Length + sbRow.Length + 1 > TableBase.MaxAllowedPacketCount && countRows > 0)
            {
                Db.NonQ(sbCommands.ToString());
                sbCommands = null;
            }
            else
            {
                if (hasComma) sbCommands.Append(",");
                sbCommands.Append(sbRow);
                countRows++;
                if (index == listEhrMeasureEvents.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(EhrMeasureEvent ehrMeasureEvent)
    {
        return InsertNoCache(ehrMeasureEvent, false);
    }

    public static long InsertNoCache(EhrMeasureEvent ehrMeasureEvent, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrmeasureevent (";
        if (isRandomKeys || useExistingPK) command += "EhrMeasureEventNum,";
        command += "DateTEvent,EventType,PatNum,MoreInfo,CodeValueEvent,CodeSystemEvent,CodeValueResult,CodeSystemResult,FKey,DateStartTobacco,TobaccoCessationDesire) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrMeasureEvent.EhrMeasureEventNum) + ",";
        command +=
            SOut.DateT(ehrMeasureEvent.DateTEvent) + ","
                                                   + SOut.Int((int) ehrMeasureEvent.EventType) + ","
                                                   + SOut.Long(ehrMeasureEvent.PatNum) + ","
                                                   + "'" + SOut.String(ehrMeasureEvent.MoreInfo) + "',"
                                                   + "'" + SOut.String(ehrMeasureEvent.CodeValueEvent) + "',"
                                                   + "'" + SOut.String(ehrMeasureEvent.CodeSystemEvent) + "',"
                                                   + "'" + SOut.String(ehrMeasureEvent.CodeValueResult) + "',"
                                                   + "'" + SOut.String(ehrMeasureEvent.CodeSystemResult) + "',"
                                                   + SOut.Long(ehrMeasureEvent.FKey) + ","
                                                   + SOut.Date(ehrMeasureEvent.DateStartTobacco) + ","
                                                   + SOut.Byte(ehrMeasureEvent.TobaccoCessationDesire) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            ehrMeasureEvent.EhrMeasureEventNum = Db.NonQ(command, true, "EhrMeasureEventNum", "ehrMeasureEvent");
        return ehrMeasureEvent.EhrMeasureEventNum;
    }

    public static void Update(EhrMeasureEvent ehrMeasureEvent)
    {
        var command = "UPDATE ehrmeasureevent SET "
                      + "DateTEvent            =  " + SOut.DateT(ehrMeasureEvent.DateTEvent) + ", "
                      + "EventType             =  " + SOut.Int((int) ehrMeasureEvent.EventType) + ", "
                      + "PatNum                =  " + SOut.Long(ehrMeasureEvent.PatNum) + ", "
                      + "MoreInfo              = '" + SOut.String(ehrMeasureEvent.MoreInfo) + "', "
                      + "CodeValueEvent        = '" + SOut.String(ehrMeasureEvent.CodeValueEvent) + "', "
                      + "CodeSystemEvent       = '" + SOut.String(ehrMeasureEvent.CodeSystemEvent) + "', "
                      + "CodeValueResult       = '" + SOut.String(ehrMeasureEvent.CodeValueResult) + "', "
                      + "CodeSystemResult      = '" + SOut.String(ehrMeasureEvent.CodeSystemResult) + "', "
                      + "FKey                  =  " + SOut.Long(ehrMeasureEvent.FKey) + ", "
                      + "DateStartTobacco      =  " + SOut.Date(ehrMeasureEvent.DateStartTobacco) + ", "
                      + "TobaccoCessationDesire=  " + SOut.Byte(ehrMeasureEvent.TobaccoCessationDesire) + " "
                      + "WHERE EhrMeasureEventNum = " + SOut.Long(ehrMeasureEvent.EhrMeasureEventNum);
        Db.NonQ(command);
    }

    public static bool Update(EhrMeasureEvent ehrMeasureEvent, EhrMeasureEvent oldEhrMeasureEvent)
    {
        var command = "";
        if (ehrMeasureEvent.DateTEvent != oldEhrMeasureEvent.DateTEvent)
        {
            if (command != "") command += ",";
            command += "DateTEvent = " + SOut.DateT(ehrMeasureEvent.DateTEvent) + "";
        }

        if (ehrMeasureEvent.EventType != oldEhrMeasureEvent.EventType)
        {
            if (command != "") command += ",";
            command += "EventType = " + SOut.Int((int) ehrMeasureEvent.EventType) + "";
        }

        if (ehrMeasureEvent.PatNum != oldEhrMeasureEvent.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(ehrMeasureEvent.PatNum) + "";
        }

        if (ehrMeasureEvent.MoreInfo != oldEhrMeasureEvent.MoreInfo)
        {
            if (command != "") command += ",";
            command += "MoreInfo = '" + SOut.String(ehrMeasureEvent.MoreInfo) + "'";
        }

        if (ehrMeasureEvent.CodeValueEvent != oldEhrMeasureEvent.CodeValueEvent)
        {
            if (command != "") command += ",";
            command += "CodeValueEvent = '" + SOut.String(ehrMeasureEvent.CodeValueEvent) + "'";
        }

        if (ehrMeasureEvent.CodeSystemEvent != oldEhrMeasureEvent.CodeSystemEvent)
        {
            if (command != "") command += ",";
            command += "CodeSystemEvent = '" + SOut.String(ehrMeasureEvent.CodeSystemEvent) + "'";
        }

        if (ehrMeasureEvent.CodeValueResult != oldEhrMeasureEvent.CodeValueResult)
        {
            if (command != "") command += ",";
            command += "CodeValueResult = '" + SOut.String(ehrMeasureEvent.CodeValueResult) + "'";
        }

        if (ehrMeasureEvent.CodeSystemResult != oldEhrMeasureEvent.CodeSystemResult)
        {
            if (command != "") command += ",";
            command += "CodeSystemResult = '" + SOut.String(ehrMeasureEvent.CodeSystemResult) + "'";
        }

        if (ehrMeasureEvent.FKey != oldEhrMeasureEvent.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(ehrMeasureEvent.FKey) + "";
        }

        if (ehrMeasureEvent.DateStartTobacco.Date != oldEhrMeasureEvent.DateStartTobacco.Date)
        {
            if (command != "") command += ",";
            command += "DateStartTobacco = " + SOut.Date(ehrMeasureEvent.DateStartTobacco) + "";
        }

        if (ehrMeasureEvent.TobaccoCessationDesire != oldEhrMeasureEvent.TobaccoCessationDesire)
        {
            if (command != "") command += ",";
            command += "TobaccoCessationDesire = " + SOut.Byte(ehrMeasureEvent.TobaccoCessationDesire) + "";
        }

        if (command == "") return false;
        command = "UPDATE ehrmeasureevent SET " + command
                                                + " WHERE EhrMeasureEventNum = " + SOut.Long(ehrMeasureEvent.EhrMeasureEventNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EhrMeasureEvent ehrMeasureEvent, EhrMeasureEvent oldEhrMeasureEvent)
    {
        if (ehrMeasureEvent.DateTEvent != oldEhrMeasureEvent.DateTEvent) return true;
        if (ehrMeasureEvent.EventType != oldEhrMeasureEvent.EventType) return true;
        if (ehrMeasureEvent.PatNum != oldEhrMeasureEvent.PatNum) return true;
        if (ehrMeasureEvent.MoreInfo != oldEhrMeasureEvent.MoreInfo) return true;
        if (ehrMeasureEvent.CodeValueEvent != oldEhrMeasureEvent.CodeValueEvent) return true;
        if (ehrMeasureEvent.CodeSystemEvent != oldEhrMeasureEvent.CodeSystemEvent) return true;
        if (ehrMeasureEvent.CodeValueResult != oldEhrMeasureEvent.CodeValueResult) return true;
        if (ehrMeasureEvent.CodeSystemResult != oldEhrMeasureEvent.CodeSystemResult) return true;
        if (ehrMeasureEvent.FKey != oldEhrMeasureEvent.FKey) return true;
        if (ehrMeasureEvent.DateStartTobacco.Date != oldEhrMeasureEvent.DateStartTobacco.Date) return true;
        if (ehrMeasureEvent.TobaccoCessationDesire != oldEhrMeasureEvent.TobaccoCessationDesire) return true;
        return false;
    }

    public static void Delete(long ehrMeasureEventNum)
    {
        var command = "DELETE FROM ehrmeasureevent "
                      + "WHERE EhrMeasureEventNum = " + SOut.Long(ehrMeasureEventNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrMeasureEventNums)
    {
        if (listEhrMeasureEventNums == null || listEhrMeasureEventNums.Count == 0) return;
        var command = "DELETE FROM ehrmeasureevent "
                      + "WHERE EhrMeasureEventNum IN(" + string.Join(",", listEhrMeasureEventNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}