using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ProgramPropertyCrud
{
    public static List<ProgramProperty> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProgramProperty> TableToList(DataTable table)
    {
        var retVal = new List<ProgramProperty>();
        foreach (DataRow row in table.Rows)
        {
            var programProperty = new ProgramProperty
            {
                ProgramPropertyNum = SIn.Long(row["ProgramPropertyNum"].ToString()),
                ProgramNum = SIn.Long(row["ProgramNum"].ToString()),
                PropertyDesc = SIn.String(row["PropertyDesc"].ToString()),
                PropertyValue = SIn.String(row["PropertyValue"].ToString()),
                ComputerName = SIn.String(row["ComputerName"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                IsMasked = SIn.Bool(row["IsMasked"].ToString()),
                IsHighSecurity = SIn.Bool(row["IsHighSecurity"].ToString())
            };
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

    public static void Insert(ProgramProperty programProperty)
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
        var paramPropertyValue = new OdSqlParameter("paramPropertyValue", SOut.StringParam(programProperty.PropertyValue));
        {
            programProperty.ProgramPropertyNum = Db.NonQ(command, true, "ProgramPropertyNum", "programProperty", paramPropertyValue);
        }
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
        var paramPropertyValue = new OdSqlParameter("paramPropertyValue", SOut.StringParam(programProperty.PropertyValue));
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
        var paramPropertyValue = new OdSqlParameter("paramPropertyValue", SOut.StringParam(programProperty.PropertyValue));
        command = "UPDATE programproperty SET " + command
                                                + " WHERE ProgramPropertyNum = " + SOut.Long(programProperty.ProgramPropertyNum);
        Db.NonQ(command, paramPropertyValue);
        return true;
    }

    public static void DeleteMany(List<long> listProgramPropertyNums)
    {
        if (listProgramPropertyNums == null || listProgramPropertyNums.Count == 0) return;
        var command = "DELETE FROM programproperty "
                      + "WHERE ProgramPropertyNum IN(" + string.Join(",", listProgramPropertyNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static void Sync(List<ProgramProperty> listNew, List<ProgramProperty> listDB)
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
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            ProgramProperty fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            ProgramProperty fieldDB = null;
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
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return;
    }
}