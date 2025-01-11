#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ProgramPropertyCrud
{
    public static ProgramProperty SelectOne(long programPropertyNum)
    {
        var command = "SELECT * FROM programproperty "
                      + "WHERE ProgramPropertyNum = " + SOut.Long(programPropertyNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ProgramProperty SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ProgramProperty> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProgramProperty> TableToList(DataTable table)
    {
        var retVal = new List<ProgramProperty>();
        ProgramProperty programProperty;
        foreach (DataRow row in table.Rows)
        {
            programProperty = new ProgramProperty();
            programProperty.ProgramPropertyNum = SIn.Long(row["ProgramPropertyNum"].ToString());
            programProperty.ProgramNum = SIn.Long(row["ProgramNum"].ToString());
            programProperty.PropertyDesc = SIn.String(row["PropertyDesc"].ToString());
            programProperty.PropertyValue = SIn.String(row["PropertyValue"].ToString());
            programProperty.ComputerName = SIn.String(row["ComputerName"].ToString());
            programProperty.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            programProperty.IsMasked = SIn.Bool(row["IsMasked"].ToString());
            programProperty.IsHighSecurity = SIn.Bool(row["IsHighSecurity"].ToString());
            retVal.Add(programProperty);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ProgramProperty> listProgramPropertys, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ProgramProperty";
        var table = new DataTable(tableName);
        table.Columns.Add("ProgramPropertyNum");
        table.Columns.Add("ProgramNum");
        table.Columns.Add("PropertyDesc");
        table.Columns.Add("PropertyValue");
        table.Columns.Add("ComputerName");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("IsMasked");
        table.Columns.Add("IsHighSecurity");
        foreach (var programProperty in listProgramPropertys)
            table.Rows.Add(SOut.Long(programProperty.ProgramPropertyNum), SOut.Long(programProperty.ProgramNum), programProperty.PropertyDesc, programProperty.PropertyValue, programProperty.ComputerName, SOut.Long(programProperty.ClinicNum), SOut.Bool(programProperty.IsMasked), SOut.Bool(programProperty.IsHighSecurity));
        return table;
    }

    public static long Insert(ProgramProperty programProperty)
    {
        return Insert(programProperty, false);
    }

    public static long Insert(ProgramProperty programProperty, bool useExistingPK)
    {
        var command = "INSERT INTO programproperty (";

        command += "ProgramNum,PropertyDesc,PropertyValue,ComputerName,ClinicNum,IsMasked,IsHighSecurity) VALUES(";

        command +=
            SOut.Long(programProperty.ProgramNum) + ","
                                                  + "'" + SOut.String(programProperty.PropertyDesc) + "',"
                                                  + DbHelper.ParamChar + "paramPropertyValue,"
                                                  + "'" + SOut.String(programProperty.ComputerName) + "',"
                                                  + SOut.Long(programProperty.ClinicNum) + ","
                                                  + SOut.Bool(programProperty.IsMasked) + ","
                                                  + SOut.Bool(programProperty.IsHighSecurity) + ")";
        if (programProperty.PropertyValue == null) programProperty.PropertyValue = "";
        var paramPropertyValue = new OdSqlParameter("paramPropertyValue", OdDbType.Text, SOut.StringParam(programProperty.PropertyValue));
        {
            programProperty.ProgramPropertyNum = Db.NonQ(command, true, "ProgramPropertyNum", "programProperty", paramPropertyValue);
        }
        return programProperty.ProgramPropertyNum;
    }

    public static void InsertMany(List<ProgramProperty> listProgramPropertys)
    {
        InsertMany(listProgramPropertys, false);
    }

    public static void InsertMany(List<ProgramProperty> listProgramPropertys, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listProgramPropertys.Count)
        {
            var programProperty = listProgramPropertys[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO programproperty (");
                if (useExistingPK) sbCommands.Append("ProgramPropertyNum,");
                sbCommands.Append("ProgramNum,PropertyDesc,PropertyValue,ComputerName,ClinicNum,IsMasked,IsHighSecurity) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(programProperty.ProgramPropertyNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(programProperty.ProgramNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(programProperty.PropertyDesc) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(programProperty.PropertyValue) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(programProperty.ComputerName) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(programProperty.ClinicNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(programProperty.IsMasked));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(programProperty.IsHighSecurity));
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
                if (index == listProgramPropertys.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(ProgramProperty programProperty)
    {
        return InsertNoCache(programProperty, false);
    }

    public static long InsertNoCache(ProgramProperty programProperty, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO programproperty (";
        if (isRandomKeys || useExistingPK) command += "ProgramPropertyNum,";
        command += "ProgramNum,PropertyDesc,PropertyValue,ComputerName,ClinicNum,IsMasked,IsHighSecurity) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(programProperty.ProgramPropertyNum) + ",";
        command +=
            SOut.Long(programProperty.ProgramNum) + ","
                                                  + "'" + SOut.String(programProperty.PropertyDesc) + "',"
                                                  + DbHelper.ParamChar + "paramPropertyValue,"
                                                  + "'" + SOut.String(programProperty.ComputerName) + "',"
                                                  + SOut.Long(programProperty.ClinicNum) + ","
                                                  + SOut.Bool(programProperty.IsMasked) + ","
                                                  + SOut.Bool(programProperty.IsHighSecurity) + ")";
        if (programProperty.PropertyValue == null) programProperty.PropertyValue = "";
        var paramPropertyValue = new OdSqlParameter("paramPropertyValue", OdDbType.Text, SOut.StringParam(programProperty.PropertyValue));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramPropertyValue);
        else
            programProperty.ProgramPropertyNum = Db.NonQ(command, true, "ProgramPropertyNum", "programProperty", paramPropertyValue);
        return programProperty.ProgramPropertyNum;
    }

    public static void Update(ProgramProperty programProperty)
    {
        var command = "UPDATE programproperty SET "
                      + "ProgramNum        =  " + SOut.Long(programProperty.ProgramNum) + ", "
                      + "PropertyDesc      = '" + SOut.String(programProperty.PropertyDesc) + "', "
                      + "PropertyValue     =  " + DbHelper.ParamChar + "paramPropertyValue, "
                      + "ComputerName      = '" + SOut.String(programProperty.ComputerName) + "', "
                      + "ClinicNum         =  " + SOut.Long(programProperty.ClinicNum) + ", "
                      + "IsMasked          =  " + SOut.Bool(programProperty.IsMasked) + ", "
                      + "IsHighSecurity    =  " + SOut.Bool(programProperty.IsHighSecurity) + " "
                      + "WHERE ProgramPropertyNum = " + SOut.Long(programProperty.ProgramPropertyNum);
        if (programProperty.PropertyValue == null) programProperty.PropertyValue = "";
        var paramPropertyValue = new OdSqlParameter("paramPropertyValue", OdDbType.Text, SOut.StringParam(programProperty.PropertyValue));
        Db.NonQ(command, paramPropertyValue);
    }

    public static bool Update(ProgramProperty programProperty, ProgramProperty oldProgramProperty)
    {
        var command = "";
        if (programProperty.ProgramNum != oldProgramProperty.ProgramNum)
        {
            if (command != "") command += ",";
            command += "ProgramNum = " + SOut.Long(programProperty.ProgramNum) + "";
        }

        if (programProperty.PropertyDesc != oldProgramProperty.PropertyDesc)
        {
            if (command != "") command += ",";
            command += "PropertyDesc = '" + SOut.String(programProperty.PropertyDesc) + "'";
        }

        if (programProperty.PropertyValue != oldProgramProperty.PropertyValue)
        {
            if (command != "") command += ",";
            command += "PropertyValue = " + DbHelper.ParamChar + "paramPropertyValue";
        }

        if (programProperty.ComputerName != oldProgramProperty.ComputerName)
        {
            if (command != "") command += ",";
            command += "ComputerName = '" + SOut.String(programProperty.ComputerName) + "'";
        }

        if (programProperty.ClinicNum != oldProgramProperty.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(programProperty.ClinicNum) + "";
        }

        if (programProperty.IsMasked != oldProgramProperty.IsMasked)
        {
            if (command != "") command += ",";
            command += "IsMasked = " + SOut.Bool(programProperty.IsMasked) + "";
        }

        if (programProperty.IsHighSecurity != oldProgramProperty.IsHighSecurity)
        {
            if (command != "") command += ",";
            command += "IsHighSecurity = " + SOut.Bool(programProperty.IsHighSecurity) + "";
        }

        if (command == "") return false;
        if (programProperty.PropertyValue == null) programProperty.PropertyValue = "";
        var paramPropertyValue = new OdSqlParameter("paramPropertyValue", OdDbType.Text, SOut.StringParam(programProperty.PropertyValue));
        command = "UPDATE programproperty SET " + command
                                                + " WHERE ProgramPropertyNum = " + SOut.Long(programProperty.ProgramPropertyNum);
        Db.NonQ(command, paramPropertyValue);
        return true;
    }

    public static bool UpdateComparison(ProgramProperty programProperty, ProgramProperty oldProgramProperty)
    {
        if (programProperty.ProgramNum != oldProgramProperty.ProgramNum) return true;
        if (programProperty.PropertyDesc != oldProgramProperty.PropertyDesc) return true;
        if (programProperty.PropertyValue != oldProgramProperty.PropertyValue) return true;
        if (programProperty.ComputerName != oldProgramProperty.ComputerName) return true;
        if (programProperty.ClinicNum != oldProgramProperty.ClinicNum) return true;
        if (programProperty.IsMasked != oldProgramProperty.IsMasked) return true;
        if (programProperty.IsHighSecurity != oldProgramProperty.IsHighSecurity) return true;
        return false;
    }

    public static void Delete(long programPropertyNum)
    {
        var command = "DELETE FROM programproperty "
                      + "WHERE ProgramPropertyNum = " + SOut.Long(programPropertyNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listProgramPropertyNums)
    {
        if (listProgramPropertyNums == null || listProgramPropertyNums.Count == 0) return;
        var command = "DELETE FROM programproperty "
                      + "WHERE ProgramPropertyNum IN(" + string.Join(",", listProgramPropertyNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<ProgramProperty> listNew, List<ProgramProperty> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<ProgramProperty>();
        var listUpdNew = new List<ProgramProperty>();
        var listUpdDB = new List<ProgramProperty>();
        var listDel = new List<ProgramProperty>();
        listNew.Sort((x, y) => { return x.ProgramPropertyNum.CompareTo(y.ProgramPropertyNum); });
        listDB.Sort((x, y) => { return x.ProgramPropertyNum.CompareTo(y.ProgramPropertyNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        ProgramProperty fieldNew;
        ProgramProperty fieldDB;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            fieldDB = null;
            if (idxDB < listDB.Count) fieldDB = listDB[idxDB];
            //begin compare
            if (fieldNew != null && fieldDB == null)
            {
                //listNew has more items, listDB does not.
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew == null && fieldDB != null)
            {
                //listDB has more items, listNew does not.
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            if (fieldNew.ProgramPropertyNum < fieldDB.ProgramPropertyNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.ProgramPropertyNum > fieldDB.ProgramPropertyNum)
            {
                //dbPK less than newPK, dbItem is 'next'
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            //Both lists contain the 'next' item, update required
            listUpdNew.Add(fieldNew);
            listUpdDB.Add(fieldDB);
            idxNew++;
            idxDB++;
        }

        //Commit changes to DB
        for (var i = 0; i < listIns.Count; i++) Insert(listIns[i]);
        for (var i = 0; i < listUpdNew.Count; i++)
            if (Update(listUpdNew[i], listUpdDB[i]))
                rowsUpdatedCount++;

        DeleteMany(listDel.Select(x => x.ProgramPropertyNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}