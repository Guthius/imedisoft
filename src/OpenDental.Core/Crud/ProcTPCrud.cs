using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ProcTPCrud
{
    public static List<ProcTP> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcTP> TableToList(DataTable table)
    {
        var retVal = new List<ProcTP>();
        foreach (DataRow row in table.Rows)
        {
            var procTP = new ProcTP
            {
                ProcTPNum = SIn.Long(row["ProcTPNum"].ToString()),
                TreatPlanNum = SIn.Long(row["TreatPlanNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                ProcNumOrig = SIn.Long(row["ProcNumOrig"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString()),
                Priority = SIn.Long(row["Priority"].ToString()),
                ToothNumTP = SIn.String(row["ToothNumTP"].ToString()),
                Surf = SIn.String(row["Surf"].ToString()),
                ProcCode = SIn.String(row["ProcCode"].ToString()),
                Descript = SIn.String(row["Descript"].ToString()),
                FeeAmt = SIn.Double(row["FeeAmt"].ToString()),
                PriInsAmt = SIn.Double(row["PriInsAmt"].ToString()),
                SecInsAmt = SIn.Double(row["SecInsAmt"].ToString()),
                PatAmt = SIn.Double(row["PatAmt"].ToString()),
                Discount = SIn.Double(row["Discount"].ToString()),
                Prognosis = SIn.String(row["Prognosis"].ToString()),
                Dx = SIn.String(row["Dx"].ToString()),
                ProcAbbr = SIn.String(row["ProcAbbr"].ToString()),
                SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString()),
                SecDateEntry = SIn.Date(row["SecDateEntry"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString()),
                FeeAllowed = SIn.Double(row["FeeAllowed"].ToString()),
                TaxAmt = SIn.Double(row["TaxAmt"].ToString()),
                ProvNum = SIn.Long(row["ProvNum"].ToString()),
                DateTP = SIn.Date(row["DateTP"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                CatPercUCR = SIn.Double(row["CatPercUCR"].ToString())
            };
            retVal.Add(procTP);
        }

        return retVal;
    }

    public static void Insert(ProcTP procTP)
    {
        var command = "INSERT INTO proctp (";

        command += "TreatPlanNum,PatNum,ProcNumOrig,ItemOrder,Priority,ToothNumTP,Surf,ProcCode,Descript,FeeAmt,PriInsAmt,SecInsAmt,PatAmt,Discount,Prognosis,Dx,ProcAbbr,SecUserNumEntry,SecDateEntry,FeeAllowed,TaxAmt,ProvNum,DateTP,ClinicNum,CatPercUCR) VALUES(";

        command +=
            SOut.Long(procTP.TreatPlanNum) + ","
                                           + SOut.Long(procTP.PatNum) + ","
                                           + SOut.Long(procTP.ProcNumOrig) + ","
                                           + SOut.Int(procTP.ItemOrder) + ","
                                           + SOut.Long(procTP.Priority) + ","
                                           + "'" + SOut.String(procTP.ToothNumTP) + "',"
                                           + "'" + SOut.String(procTP.Surf) + "',"
                                           + "'" + SOut.String(procTP.ProcCode) + "',"
                                           + "'" + SOut.String(procTP.Descript) + "',"
                                           + SOut.Double(procTP.FeeAmt) + ","
                                           + SOut.Double(procTP.PriInsAmt) + ","
                                           + SOut.Double(procTP.SecInsAmt) + ","
                                           + SOut.Double(procTP.PatAmt) + ","
                                           + SOut.Double(procTP.Discount) + ","
                                           + "'" + SOut.String(procTP.Prognosis) + "',"
                                           + "'" + SOut.String(procTP.Dx) + "',"
                                           + "'" + SOut.String(procTP.ProcAbbr) + "',"
                                           + SOut.Long(procTP.SecUserNumEntry) + ","
                                           + "NOW()" + ","
                                           //SecDateTEdit can only be set by MySQL
                                           + SOut.Double(procTP.FeeAllowed) + ","
                                           + SOut.Double(procTP.TaxAmt) + ","
                                           + SOut.Long(procTP.ProvNum) + ","
                                           + SOut.Date(procTP.DateTP) + ","
                                           + SOut.Long(procTP.ClinicNum) + ","
                                           + SOut.Double(procTP.CatPercUCR) + ")";
        {
            procTP.ProcTPNum = Db.NonQ(command, true, "ProcTPNum", "procTP");
        }
    }

    public static void Update(ProcTP procTP)
    {
        var command = "UPDATE proctp SET "
                      + "TreatPlanNum   =  " + SOut.Long(procTP.TreatPlanNum) + ", "
                      + "PatNum         =  " + SOut.Long(procTP.PatNum) + ", "
                      + "ProcNumOrig    =  " + SOut.Long(procTP.ProcNumOrig) + ", "
                      + "ItemOrder      =  " + SOut.Int(procTP.ItemOrder) + ", "
                      + "Priority       =  " + SOut.Long(procTP.Priority) + ", "
                      + "ToothNumTP     = '" + SOut.String(procTP.ToothNumTP) + "', "
                      + "Surf           = '" + SOut.String(procTP.Surf) + "', "
                      + "ProcCode       = '" + SOut.String(procTP.ProcCode) + "', "
                      + "Descript       = '" + SOut.String(procTP.Descript) + "', "
                      + "FeeAmt         =  " + SOut.Double(procTP.FeeAmt) + ", "
                      + "PriInsAmt      =  " + SOut.Double(procTP.PriInsAmt) + ", "
                      + "SecInsAmt      =  " + SOut.Double(procTP.SecInsAmt) + ", "
                      + "PatAmt         =  " + SOut.Double(procTP.PatAmt) + ", "
                      + "Discount       =  " + SOut.Double(procTP.Discount) + ", "
                      + "Prognosis      = '" + SOut.String(procTP.Prognosis) + "', "
                      + "Dx             = '" + SOut.String(procTP.Dx) + "', "
                      + "ProcAbbr       = '" + SOut.String(procTP.ProcAbbr) + "', "
                      //SecUserNumEntry excluded from update
                      //SecDateEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "FeeAllowed     =  " + SOut.Double(procTP.FeeAllowed) + ", "
                      + "TaxAmt         =  " + SOut.Double(procTP.TaxAmt) + ", "
                      + "ProvNum        =  " + SOut.Long(procTP.ProvNum) + ", "
                      + "DateTP         =  " + SOut.Date(procTP.DateTP) + ", "
                      + "ClinicNum      =  " + SOut.Long(procTP.ClinicNum) + ", "
                      + "CatPercUCR     =  " + SOut.Double(procTP.CatPercUCR) + " "
                      + "WHERE ProcTPNum = " + SOut.Long(procTP.ProcTPNum);
        Db.NonQ(command);
    }
}