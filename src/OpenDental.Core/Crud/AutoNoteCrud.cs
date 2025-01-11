using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class AutoNoteCrud
{
    public static List<AutoNote> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<AutoNote> TableToList(DataTable table)
    {
        var retVal = new List<AutoNote>();
        AutoNote autoNote;
        foreach (DataRow row in table.Rows)
        {
            autoNote = new AutoNote();
            autoNote.AutoNoteNum = SIn.Long(row["AutoNoteNum"].ToString());
            autoNote.AutoNoteName = SIn.String(row["AutoNoteName"].ToString());
            autoNote.MainText = SIn.String(row["MainText"].ToString());
            autoNote.Category = SIn.Long(row["Category"].ToString());
            retVal.Add(autoNote);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<AutoNote> listAutoNotes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "AutoNote";
        var table = new DataTable(tableName);
        table.Columns.Add("AutoNoteNum");
        table.Columns.Add("AutoNoteName");
        table.Columns.Add("MainText");
        table.Columns.Add("Category");
        foreach (var autoNote in listAutoNotes)
            table.Rows.Add(SOut.Long(autoNote.AutoNoteNum), autoNote.AutoNoteName, autoNote.MainText, SOut.Long(autoNote.Category));
        return table;
    }

    public static long Insert(AutoNote autoNote)
    {
        var command = "INSERT INTO autonote (";

        command += "AutoNoteName,MainText,Category) VALUES(";

        command +=
            "'" + SOut.String(autoNote.AutoNoteName) + "',"
            + DbHelper.ParamChar + "paramMainText,"
            + SOut.Long(autoNote.Category) + ")";
        if (autoNote.MainText == null) autoNote.MainText = "";
        var paramMainText = new OdSqlParameter("paramMainText", OdDbType.Text, SOut.StringParam(autoNote.MainText));
        {
            autoNote.AutoNoteNum = Db.NonQ(command, true, "AutoNoteNum", "autoNote", paramMainText);
        }
        return autoNote.AutoNoteNum;
    }

    public static void InsertMany(List<AutoNote> listAutoNotes, bool useExistingPK = false)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listAutoNotes.Count)
        {
            var autoNote = listAutoNotes[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO autonote (");
                if (useExistingPK) sbCommands.Append("AutoNoteNum,");
                sbCommands.Append("AutoNoteName,MainText,Category) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(autoNote.AutoNoteNum));
                sbRow.Append(",");
            }

            sbRow.Append("'" + SOut.String(autoNote.AutoNoteName) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(autoNote.MainText) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(autoNote.Category));
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
                if (index == listAutoNotes.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static void Update(AutoNote autoNote)
    {
        var command = "UPDATE autonote SET "
                      + "AutoNoteName= '" + SOut.String(autoNote.AutoNoteName) + "', "
                      + "MainText    =  " + DbHelper.ParamChar + "paramMainText, "
                      + "Category    =  " + SOut.Long(autoNote.Category) + " "
                      + "WHERE AutoNoteNum = " + SOut.Long(autoNote.AutoNoteNum);
        if (autoNote.MainText == null) autoNote.MainText = "";
        var paramMainText = new OdSqlParameter("paramMainText", OdDbType.Text, SOut.StringParam(autoNote.MainText));
        Db.NonQ(command, paramMainText);
    }
}