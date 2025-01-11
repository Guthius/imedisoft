#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class MedicationPatCrud
{
    public static MedicationPat SelectOne(long medicationPatNum)
    {
        var command = "SELECT * FROM medicationpat "
                      + "WHERE MedicationPatNum = " + SOut.Long(medicationPatNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static MedicationPat SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<MedicationPat> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MedicationPat> TableToList(DataTable table)
    {
        var retVal = new List<MedicationPat>();
        MedicationPat medicationPat;
        foreach (DataRow row in table.Rows)
        {
            medicationPat = new MedicationPat();
            medicationPat.MedicationPatNum = SIn.Long(row["MedicationPatNum"].ToString());
            medicationPat.PatNum = SIn.Long(row["PatNum"].ToString());
            medicationPat.MedicationNum = SIn.Long(row["MedicationNum"].ToString());
            medicationPat.PatNote = SIn.String(row["PatNote"].ToString());
            medicationPat.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            medicationPat.DateStart = SIn.Date(row["DateStart"].ToString());
            medicationPat.DateStop = SIn.Date(row["DateStop"].ToString());
            medicationPat.ProvNum = SIn.Long(row["ProvNum"].ToString());
            medicationPat.MedDescript = SIn.String(row["MedDescript"].ToString());
            medicationPat.RxCui = SIn.Long(row["RxCui"].ToString());
            medicationPat.ErxGuid = SIn.String(row["ErxGuid"].ToString());
            medicationPat.IsCpoe = SIn.Bool(row["IsCpoe"].ToString());
            retVal.Add(medicationPat);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<MedicationPat> listMedicationPats, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "MedicationPat";
        var table = new DataTable(tableName);
        table.Columns.Add("MedicationPatNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("MedicationNum");
        table.Columns.Add("PatNote");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("DateStart");
        table.Columns.Add("DateStop");
        table.Columns.Add("ProvNum");
        table.Columns.Add("MedDescript");
        table.Columns.Add("RxCui");
        table.Columns.Add("ErxGuid");
        table.Columns.Add("IsCpoe");
        foreach (var medicationPat in listMedicationPats)
            table.Rows.Add(SOut.Long(medicationPat.MedicationPatNum), SOut.Long(medicationPat.PatNum), SOut.Long(medicationPat.MedicationNum), medicationPat.PatNote, SOut.DateT(medicationPat.DateTStamp, false), SOut.DateT(medicationPat.DateStart, false), SOut.DateT(medicationPat.DateStop, false), SOut.Long(medicationPat.ProvNum), medicationPat.MedDescript, SOut.Long(medicationPat.RxCui), medicationPat.ErxGuid, SOut.Bool(medicationPat.IsCpoe));
        return table;
    }

    public static long Insert(MedicationPat medicationPat)
    {
        return Insert(medicationPat, false);
    }

    public static long Insert(MedicationPat medicationPat, bool useExistingPK)
    {
        var command = "INSERT INTO medicationpat (";

        command += "PatNum,MedicationNum,PatNote,DateStart,DateStop,ProvNum,MedDescript,RxCui,ErxGuid,IsCpoe) VALUES(";

        command +=
            SOut.Long(medicationPat.PatNum) + ","
                                            + SOut.Long(medicationPat.MedicationNum) + ","
                                            + DbHelper.ParamChar + "paramPatNote,"
                                            //DateTStamp can only be set by MySQL
                                            + SOut.Date(medicationPat.DateStart) + ","
                                            + SOut.Date(medicationPat.DateStop) + ","
                                            + SOut.Long(medicationPat.ProvNum) + ","
                                            + "'" + SOut.String(medicationPat.MedDescript) + "',"
                                            + SOut.Long(medicationPat.RxCui) + ","
                                            + "'" + SOut.String(medicationPat.ErxGuid) + "',"
                                            + SOut.Bool(medicationPat.IsCpoe) + ")";
        if (medicationPat.PatNote == null) medicationPat.PatNote = "";
        var paramPatNote = new OdSqlParameter("paramPatNote", OdDbType.Text, SOut.StringParam(medicationPat.PatNote));
        {
            medicationPat.MedicationPatNum = Db.NonQ(command, true, "MedicationPatNum", "medicationPat", paramPatNote);
        }
        return medicationPat.MedicationPatNum;
    }

    public static long InsertNoCache(MedicationPat medicationPat)
    {
        return InsertNoCache(medicationPat, false);
    }

    public static long InsertNoCache(MedicationPat medicationPat, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO medicationpat (";
        if (isRandomKeys || useExistingPK) command += "MedicationPatNum,";
        command += "PatNum,MedicationNum,PatNote,DateStart,DateStop,ProvNum,MedDescript,RxCui,ErxGuid,IsCpoe) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(medicationPat.MedicationPatNum) + ",";
        command +=
            SOut.Long(medicationPat.PatNum) + ","
                                            + SOut.Long(medicationPat.MedicationNum) + ","
                                            + DbHelper.ParamChar + "paramPatNote,"
                                            //DateTStamp can only be set by MySQL
                                            + SOut.Date(medicationPat.DateStart) + ","
                                            + SOut.Date(medicationPat.DateStop) + ","
                                            + SOut.Long(medicationPat.ProvNum) + ","
                                            + "'" + SOut.String(medicationPat.MedDescript) + "',"
                                            + SOut.Long(medicationPat.RxCui) + ","
                                            + "'" + SOut.String(medicationPat.ErxGuid) + "',"
                                            + SOut.Bool(medicationPat.IsCpoe) + ")";
        if (medicationPat.PatNote == null) medicationPat.PatNote = "";
        var paramPatNote = new OdSqlParameter("paramPatNote", OdDbType.Text, SOut.StringParam(medicationPat.PatNote));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramPatNote);
        else
            medicationPat.MedicationPatNum = Db.NonQ(command, true, "MedicationPatNum", "medicationPat", paramPatNote);
        return medicationPat.MedicationPatNum;
    }

    public static void Update(MedicationPat medicationPat)
    {
        var command = "UPDATE medicationpat SET "
                      + "PatNum          =  " + SOut.Long(medicationPat.PatNum) + ", "
                      + "MedicationNum   =  " + SOut.Long(medicationPat.MedicationNum) + ", "
                      + "PatNote         =  " + DbHelper.ParamChar + "paramPatNote, "
                      //DateTStamp can only be set by MySQL
                      + "DateStart       =  " + SOut.Date(medicationPat.DateStart) + ", "
                      + "DateStop        =  " + SOut.Date(medicationPat.DateStop) + ", "
                      + "ProvNum         =  " + SOut.Long(medicationPat.ProvNum) + ", "
                      + "MedDescript     = '" + SOut.String(medicationPat.MedDescript) + "', "
                      + "RxCui           =  " + SOut.Long(medicationPat.RxCui) + ", "
                      + "ErxGuid         = '" + SOut.String(medicationPat.ErxGuid) + "', "
                      + "IsCpoe          =  " + SOut.Bool(medicationPat.IsCpoe) + " "
                      + "WHERE MedicationPatNum = " + SOut.Long(medicationPat.MedicationPatNum);
        if (medicationPat.PatNote == null) medicationPat.PatNote = "";
        var paramPatNote = new OdSqlParameter("paramPatNote", OdDbType.Text, SOut.StringParam(medicationPat.PatNote));
        Db.NonQ(command, paramPatNote);
    }

    public static bool Update(MedicationPat medicationPat, MedicationPat oldMedicationPat)
    {
        var command = "";
        if (medicationPat.PatNum != oldMedicationPat.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(medicationPat.PatNum) + "";
        }

        if (medicationPat.MedicationNum != oldMedicationPat.MedicationNum)
        {
            if (command != "") command += ",";
            command += "MedicationNum = " + SOut.Long(medicationPat.MedicationNum) + "";
        }

        if (medicationPat.PatNote != oldMedicationPat.PatNote)
        {
            if (command != "") command += ",";
            command += "PatNote = " + DbHelper.ParamChar + "paramPatNote";
        }

        //DateTStamp can only be set by MySQL
        if (medicationPat.DateStart.Date != oldMedicationPat.DateStart.Date)
        {
            if (command != "") command += ",";
            command += "DateStart = " + SOut.Date(medicationPat.DateStart) + "";
        }

        if (medicationPat.DateStop.Date != oldMedicationPat.DateStop.Date)
        {
            if (command != "") command += ",";
            command += "DateStop = " + SOut.Date(medicationPat.DateStop) + "";
        }

        if (medicationPat.ProvNum != oldMedicationPat.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(medicationPat.ProvNum) + "";
        }

        if (medicationPat.MedDescript != oldMedicationPat.MedDescript)
        {
            if (command != "") command += ",";
            command += "MedDescript = '" + SOut.String(medicationPat.MedDescript) + "'";
        }

        if (medicationPat.RxCui != oldMedicationPat.RxCui)
        {
            if (command != "") command += ",";
            command += "RxCui = " + SOut.Long(medicationPat.RxCui) + "";
        }

        if (medicationPat.ErxGuid != oldMedicationPat.ErxGuid)
        {
            if (command != "") command += ",";
            command += "ErxGuid = '" + SOut.String(medicationPat.ErxGuid) + "'";
        }

        if (medicationPat.IsCpoe != oldMedicationPat.IsCpoe)
        {
            if (command != "") command += ",";
            command += "IsCpoe = " + SOut.Bool(medicationPat.IsCpoe) + "";
        }

        if (command == "") return false;
        if (medicationPat.PatNote == null) medicationPat.PatNote = "";
        var paramPatNote = new OdSqlParameter("paramPatNote", OdDbType.Text, SOut.StringParam(medicationPat.PatNote));
        command = "UPDATE medicationpat SET " + command
                                              + " WHERE MedicationPatNum = " + SOut.Long(medicationPat.MedicationPatNum);
        Db.NonQ(command, paramPatNote);
        return true;
    }

    public static bool UpdateComparison(MedicationPat medicationPat, MedicationPat oldMedicationPat)
    {
        if (medicationPat.PatNum != oldMedicationPat.PatNum) return true;
        if (medicationPat.MedicationNum != oldMedicationPat.MedicationNum) return true;
        if (medicationPat.PatNote != oldMedicationPat.PatNote) return true;
        //DateTStamp can only be set by MySQL
        if (medicationPat.DateStart.Date != oldMedicationPat.DateStart.Date) return true;
        if (medicationPat.DateStop.Date != oldMedicationPat.DateStop.Date) return true;
        if (medicationPat.ProvNum != oldMedicationPat.ProvNum) return true;
        if (medicationPat.MedDescript != oldMedicationPat.MedDescript) return true;
        if (medicationPat.RxCui != oldMedicationPat.RxCui) return true;
        if (medicationPat.ErxGuid != oldMedicationPat.ErxGuid) return true;
        if (medicationPat.IsCpoe != oldMedicationPat.IsCpoe) return true;
        return false;
    }

    public static void Delete(long medicationPatNum)
    {
        var command = "DELETE FROM medicationpat "
                      + "WHERE MedicationPatNum = " + SOut.Long(medicationPatNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listMedicationPatNums)
    {
        if (listMedicationPatNums == null || listMedicationPatNums.Count == 0) return;
        var command = "DELETE FROM medicationpat "
                      + "WHERE MedicationPatNum IN(" + string.Join(",", listMedicationPatNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}