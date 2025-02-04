using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class MedicationPatCrud
{
    public static List<MedicationPat> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MedicationPat> TableToList(DataTable table)
    {
        var retVal = new List<MedicationPat>();
        foreach (DataRow row in table.Rows)
        {
            var medicationPat = new MedicationPat
            {
                MedicationPatNum = SIn.Long(row["MedicationPatNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                MedicationNum = SIn.Long(row["MedicationNum"].ToString()),
                PatNote = SIn.String(row["PatNote"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString()),
                DateStart = SIn.Date(row["DateStart"].ToString()),
                DateStop = SIn.Date(row["DateStop"].ToString()),
                ProvNum = SIn.Long(row["ProvNum"].ToString()),
                MedDescript = SIn.String(row["MedDescript"].ToString()),
                RxCui = SIn.Long(row["RxCui"].ToString()),
                ErxGuid = SIn.String(row["ErxGuid"].ToString()),
                IsCpoe = SIn.Bool(row["IsCpoe"].ToString())
            };
            retVal.Add(medicationPat);
        }

        return retVal;
    }

    public static void Insert(MedicationPat medicationPat)
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
        var paramPatNote = new OdSqlParameter("paramPatNote", SOut.StringParam(medicationPat.PatNote));
        {
            medicationPat.MedicationPatNum = Db.NonQ(command, true, "MedicationPatNum", "medicationPat", paramPatNote);
        }
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
        var paramPatNote = new OdSqlParameter("paramPatNote", SOut.StringParam(medicationPat.PatNote));
        Db.NonQ(command, paramPatNote);
    }
}