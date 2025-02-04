using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class LabCaseCrud
{
    public static LabCase SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<LabCase> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<LabCase> TableToList(DataTable table)
    {
        var retVal = new List<LabCase>();
        foreach (DataRow row in table.Rows)
        {
            var labCase = new LabCase
            {
                LabCaseNum = SIn.Long(row["LabCaseNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                LaboratoryNum = SIn.Long(row["LaboratoryNum"].ToString()),
                AptNum = SIn.Long(row["AptNum"].ToString()),
                PlannedAptNum = SIn.Long(row["PlannedAptNum"].ToString()),
                DateTimeDue = SIn.DateTime(row["DateTimeDue"].ToString()),
                DateTimeCreated = SIn.DateTime(row["DateTimeCreated"].ToString()),
                DateTimeSent = SIn.DateTime(row["DateTimeSent"].ToString()),
                DateTimeRecd = SIn.DateTime(row["DateTimeRecd"].ToString()),
                DateTimeChecked = SIn.DateTime(row["DateTimeChecked"].ToString()),
                ProvNum = SIn.Long(row["ProvNum"].ToString()),
                Instructions = SIn.String(row["Instructions"].ToString()),
                LabFee = SIn.Double(row["LabFee"].ToString()),
                InvoiceNum = SIn.String(row["InvoiceNum"].ToString())
            };
            retVal.Add(labCase);
        }

        return retVal;
    }

    public static void Insert(LabCase labCase)
    {
        var command = "INSERT INTO labcase (";

        command += "PatNum,LaboratoryNum,AptNum,PlannedAptNum,DateTimeDue,DateTimeCreated,DateTimeSent,DateTimeRecd,DateTimeChecked,ProvNum,Instructions,LabFee,InvoiceNum) VALUES(";

        command +=
            SOut.Long(labCase.PatNum) + ","
                                      + SOut.Long(labCase.LaboratoryNum) + ","
                                      + SOut.Long(labCase.AptNum) + ","
                                      + SOut.Long(labCase.PlannedAptNum) + ","
                                      + SOut.DateTime(labCase.DateTimeDue) + ","
                                      + SOut.DateTime(labCase.DateTimeCreated) + ","
                                      + SOut.DateTime(labCase.DateTimeSent) + ","
                                      + SOut.DateTime(labCase.DateTimeRecd) + ","
                                      + SOut.DateTime(labCase.DateTimeChecked) + ","
                                      + SOut.Long(labCase.ProvNum) + ","
                                      + DbHelper.ParamChar + "paramInstructions,"
                                      + SOut.Double(labCase.LabFee) + ","
                                      //DateTStamp can only be set by MySQL
                                      + "'" + SOut.String(labCase.InvoiceNum) + "')";
        if (labCase.Instructions == null) labCase.Instructions = "";
        var paramInstructions = new OdSqlParameter("paramInstructions", SOut.StringParam(labCase.Instructions));
        {
            labCase.LabCaseNum = Db.NonQ(command, true, "LabCaseNum", "labCase", paramInstructions);
        }
    }

    public static void Update(LabCase labCase)
    {
        var command = "UPDATE labcase SET "
                      + "PatNum         =  " + SOut.Long(labCase.PatNum) + ", "
                      + "LaboratoryNum  =  " + SOut.Long(labCase.LaboratoryNum) + ", "
                      + "AptNum         =  " + SOut.Long(labCase.AptNum) + ", "
                      + "PlannedAptNum  =  " + SOut.Long(labCase.PlannedAptNum) + ", "
                      + "DateTimeDue    =  " + SOut.DateTime(labCase.DateTimeDue) + ", "
                      + "DateTimeCreated=  " + SOut.DateTime(labCase.DateTimeCreated) + ", "
                      + "DateTimeSent   =  " + SOut.DateTime(labCase.DateTimeSent) + ", "
                      + "DateTimeRecd   =  " + SOut.DateTime(labCase.DateTimeRecd) + ", "
                      + "DateTimeChecked=  " + SOut.DateTime(labCase.DateTimeChecked) + ", "
                      + "ProvNum        =  " + SOut.Long(labCase.ProvNum) + ", "
                      + "Instructions   =  " + DbHelper.ParamChar + "paramInstructions, "
                      + "LabFee         =  " + SOut.Double(labCase.LabFee) + ", "
                      //DateTStamp can only be set by MySQL
                      + "InvoiceNum     = '" + SOut.String(labCase.InvoiceNum) + "' "
                      + "WHERE LabCaseNum = " + SOut.Long(labCase.LabCaseNum);
        if (labCase.Instructions == null) labCase.Instructions = "";
        var paramInstructions = new OdSqlParameter("paramInstructions", SOut.StringParam(labCase.Instructions));
        Db.NonQ(command, paramInstructions);
    }
}