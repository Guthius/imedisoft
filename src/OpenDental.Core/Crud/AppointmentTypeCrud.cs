using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class AppointmentTypeCrud
{
    public static List<AppointmentType> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<AppointmentType> TableToList(DataTable table)
    {
        var retVal = new List<AppointmentType>();
        AppointmentType appointmentType;
        foreach (DataRow row in table.Rows)
        {
            appointmentType = new AppointmentType();
            appointmentType.AppointmentTypeNum = SIn.Long(row["AppointmentTypeNum"].ToString());
            appointmentType.AppointmentTypeName = SIn.String(row["AppointmentTypeName"].ToString());
            appointmentType.AppointmentTypeColor = Color.FromArgb(SIn.Int(row["AppointmentTypeColor"].ToString()));
            appointmentType.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            appointmentType.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            appointmentType.Pattern = SIn.String(row["Pattern"].ToString());
            appointmentType.CodeStr = SIn.String(row["CodeStr"].ToString());
            appointmentType.CodeStrRequired = SIn.String(row["CodeStrRequired"].ToString());
            appointmentType.RequiredProcCodesNeeded = (EnumRequiredProcCodesNeeded) SIn.Int(row["RequiredProcCodesNeeded"].ToString());
            appointmentType.BlockoutTypes = SIn.String(row["BlockoutTypes"].ToString());
            retVal.Add(appointmentType);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<AppointmentType> listAppointmentTypes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "AppointmentType";
        var table = new DataTable(tableName);
        table.Columns.Add("AppointmentTypeNum");
        table.Columns.Add("AppointmentTypeName");
        table.Columns.Add("AppointmentTypeColor");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("IsHidden");
        table.Columns.Add("Pattern");
        table.Columns.Add("CodeStr");
        table.Columns.Add("CodeStrRequired");
        table.Columns.Add("RequiredProcCodesNeeded");
        table.Columns.Add("BlockoutTypes");
        foreach (var appointmentType in listAppointmentTypes)
            table.Rows.Add(SOut.Long(appointmentType.AppointmentTypeNum), appointmentType.AppointmentTypeName, SOut.Int(appointmentType.AppointmentTypeColor.ToArgb()), SOut.Int(appointmentType.ItemOrder), SOut.Bool(appointmentType.IsHidden), appointmentType.Pattern, appointmentType.CodeStr, appointmentType.CodeStrRequired, SOut.Int((int) appointmentType.RequiredProcCodesNeeded), appointmentType.BlockoutTypes);
        return table;
    }

    public static long Insert(AppointmentType appointmentType)
    {
        var command = "INSERT INTO appointmenttype (";

        command += "AppointmentTypeName,AppointmentTypeColor,ItemOrder,IsHidden,Pattern,CodeStr,CodeStrRequired,RequiredProcCodesNeeded,BlockoutTypes) VALUES(";

        command +=
            "'" + SOut.String(appointmentType.AppointmentTypeName) + "',"
            + SOut.Int(appointmentType.AppointmentTypeColor.ToArgb()) + ","
            + SOut.Int(appointmentType.ItemOrder) + ","
            + SOut.Bool(appointmentType.IsHidden) + ","
            + "'" + SOut.String(appointmentType.Pattern) + "',"
            + "'" + SOut.String(appointmentType.CodeStr) + "',"
            + "'" + SOut.String(appointmentType.CodeStrRequired) + "',"
            + SOut.Int((int) appointmentType.RequiredProcCodesNeeded) + ","
            + "'" + SOut.String(appointmentType.BlockoutTypes) + "')";
        {
            appointmentType.AppointmentTypeNum = Db.NonQ(command, true, "AppointmentTypeNum", "appointmentType");
        }
        return appointmentType.AppointmentTypeNum;
    }

    public static bool Update(AppointmentType appointmentType, AppointmentType oldAppointmentType)
    {
        var command = "";
        if (appointmentType.AppointmentTypeName != oldAppointmentType.AppointmentTypeName)
        {
            if (command != "") command += ",";
            command += "AppointmentTypeName = '" + SOut.String(appointmentType.AppointmentTypeName) + "'";
        }

        if (appointmentType.AppointmentTypeColor != oldAppointmentType.AppointmentTypeColor)
        {
            if (command != "") command += ",";
            command += "AppointmentTypeColor = " + SOut.Int(appointmentType.AppointmentTypeColor.ToArgb()) + "";
        }

        if (appointmentType.ItemOrder != oldAppointmentType.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(appointmentType.ItemOrder) + "";
        }

        if (appointmentType.IsHidden != oldAppointmentType.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(appointmentType.IsHidden) + "";
        }

        if (appointmentType.Pattern != oldAppointmentType.Pattern)
        {
            if (command != "") command += ",";
            command += "Pattern = '" + SOut.String(appointmentType.Pattern) + "'";
        }

        if (appointmentType.CodeStr != oldAppointmentType.CodeStr)
        {
            if (command != "") command += ",";
            command += "CodeStr = '" + SOut.String(appointmentType.CodeStr) + "'";
        }

        if (appointmentType.CodeStrRequired != oldAppointmentType.CodeStrRequired)
        {
            if (command != "") command += ",";
            command += "CodeStrRequired = '" + SOut.String(appointmentType.CodeStrRequired) + "'";
        }

        if (appointmentType.RequiredProcCodesNeeded != oldAppointmentType.RequiredProcCodesNeeded)
        {
            if (command != "") command += ",";
            command += "RequiredProcCodesNeeded = " + SOut.Int((int) appointmentType.RequiredProcCodesNeeded) + "";
        }

        if (appointmentType.BlockoutTypes != oldAppointmentType.BlockoutTypes)
        {
            if (command != "") command += ",";
            command += "BlockoutTypes = '" + SOut.String(appointmentType.BlockoutTypes) + "'";
        }

        if (command == "") return false;
        command = "UPDATE appointmenttype SET " + command
                                                + " WHERE AppointmentTypeNum = " + SOut.Long(appointmentType.AppointmentTypeNum);
        Db.NonQ(command);
        return true;
    }

    public static void DeleteMany(List<long> listAppointmentTypeNums)
    {
        if (listAppointmentTypeNums == null || listAppointmentTypeNums.Count == 0) return;
        var command = "DELETE FROM appointmenttype "
                      + "WHERE AppointmentTypeNum IN(" + string.Join(",", listAppointmentTypeNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static void Sync(List<AppointmentType> listNew, List<AppointmentType> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<AppointmentType>();
        var listUpdNew = new List<AppointmentType>();
        var listUpdDB = new List<AppointmentType>();
        var listDel = new List<AppointmentType>();
        listNew.Sort((x, y) => { return x.AppointmentTypeNum.CompareTo(y.AppointmentTypeNum); });
        listDB.Sort((x, y) => { return x.AppointmentTypeNum.CompareTo(y.AppointmentTypeNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        AppointmentType fieldNew;
        AppointmentType fieldDB;
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

            if (fieldNew.AppointmentTypeNum < fieldDB.AppointmentTypeNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.AppointmentTypeNum > fieldDB.AppointmentTypeNum)
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

        DeleteMany(listDel.Select(x => x.AppointmentTypeNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return;
    }
}