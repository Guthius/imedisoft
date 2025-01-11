using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class AutoNoteControlCrud
{
    public static List<AutoNoteControl> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<AutoNoteControl> TableToList(DataTable table)
    {
        var retVal = new List<AutoNoteControl>();
        AutoNoteControl autoNoteControl;
        foreach (DataRow row in table.Rows)
        {
            autoNoteControl = new AutoNoteControl();
            autoNoteControl.AutoNoteControlNum = SIn.Long(row["AutoNoteControlNum"].ToString());
            autoNoteControl.Descript = SIn.String(row["Descript"].ToString());
            autoNoteControl.ControlType = SIn.String(row["ControlType"].ToString());
            autoNoteControl.ControlLabel = SIn.String(row["ControlLabel"].ToString());
            autoNoteControl.ControlOptions = SIn.String(row["ControlOptions"].ToString());
            retVal.Add(autoNoteControl);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<AutoNoteControl> listAutoNoteControls, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "AutoNoteControl";
        var table = new DataTable(tableName);
        table.Columns.Add("AutoNoteControlNum");
        table.Columns.Add("Descript");
        table.Columns.Add("ControlType");
        table.Columns.Add("ControlLabel");
        table.Columns.Add("ControlOptions");
        foreach (var autoNoteControl in listAutoNoteControls)
            table.Rows.Add(SOut.Long(autoNoteControl.AutoNoteControlNum), autoNoteControl.Descript, autoNoteControl.ControlType, autoNoteControl.ControlLabel, autoNoteControl.ControlOptions);
        return table;
    }

    public static long Insert(AutoNoteControl autoNoteControl)
    {
        var command = "INSERT INTO autonotecontrol (";

        command += "Descript,ControlType,ControlLabel,ControlOptions) VALUES(";

        command +=
            "'" + SOut.String(autoNoteControl.Descript) + "',"
            + "'" + SOut.String(autoNoteControl.ControlType) + "',"
            + "'" + SOut.String(autoNoteControl.ControlLabel) + "',"
            + DbHelper.ParamChar + "paramControlOptions)";
        if (autoNoteControl.ControlOptions == null) autoNoteControl.ControlOptions = "";
        var paramControlOptions = new OdSqlParameter("paramControlOptions", OdDbType.Text, SOut.StringParam(autoNoteControl.ControlOptions));
        {
            autoNoteControl.AutoNoteControlNum = Db.NonQ(command, true, "AutoNoteControlNum", "autoNoteControl", paramControlOptions);
        }
        return autoNoteControl.AutoNoteControlNum;
    }

    public static void InsertMany(List<AutoNoteControl> listAutoNoteControls)
    {
        InsertMany(listAutoNoteControls, false);
    }

    public static void InsertMany(List<AutoNoteControl> listAutoNoteControls, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listAutoNoteControls.Count)
        {
            var autoNoteControl = listAutoNoteControls[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO autonotecontrol (");
                if (useExistingPK) sbCommands.Append("AutoNoteControlNum,");
                sbCommands.Append("Descript,ControlType,ControlLabel,ControlOptions) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(autoNoteControl.AutoNoteControlNum));
                sbRow.Append(",");
            }

            sbRow.Append("'" + SOut.String(autoNoteControl.Descript) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(autoNoteControl.ControlType) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(autoNoteControl.ControlLabel) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(autoNoteControl.ControlOptions) + "'");
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
                if (index == listAutoNoteControls.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static void Update(AutoNoteControl autoNoteControl)
    {
        var command = "UPDATE autonotecontrol SET "
                      + "Descript          = '" + SOut.String(autoNoteControl.Descript) + "', "
                      + "ControlType       = '" + SOut.String(autoNoteControl.ControlType) + "', "
                      + "ControlLabel      = '" + SOut.String(autoNoteControl.ControlLabel) + "', "
                      + "ControlOptions    =  " + DbHelper.ParamChar + "paramControlOptions "
                      + "WHERE AutoNoteControlNum = " + SOut.Long(autoNoteControl.AutoNoteControlNum);
        if (autoNoteControl.ControlOptions == null) autoNoteControl.ControlOptions = "";
        var paramControlOptions = new OdSqlParameter("paramControlOptions", OdDbType.Text, SOut.StringParam(autoNoteControl.ControlOptions));
        Db.NonQ(command, paramControlOptions);
    }
}