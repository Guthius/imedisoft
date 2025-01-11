using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ClinicErxCrud
{
    public static List<ClinicErx> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ClinicErx> TableToList(DataTable table)
    {
        var retVal = new List<ClinicErx>();
        ClinicErx clinicErx;
        foreach (DataRow row in table.Rows)
        {
            clinicErx = new ClinicErx();
            clinicErx.ClinicErxNum = SIn.Long(row["ClinicErxNum"].ToString());
            clinicErx.PatNum = SIn.Long(row["PatNum"].ToString());
            clinicErx.ClinicDesc = SIn.String(row["ClinicDesc"].ToString());
            clinicErx.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            clinicErx.EnabledStatus = (ErxStatus) SIn.Int(row["EnabledStatus"].ToString());
            clinicErx.ClinicId = SIn.String(row["ClinicId"].ToString());
            clinicErx.ClinicKey = SIn.String(row["ClinicKey"].ToString());
            clinicErx.AccountId = SIn.String(row["AccountId"].ToString());
            clinicErx.RegistrationKeyNum = SIn.Long(row["RegistrationKeyNum"].ToString());
            retVal.Add(clinicErx);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ClinicErx> listClinicErxs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ClinicErx";
        var table = new DataTable(tableName);
        table.Columns.Add("ClinicErxNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ClinicDesc");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("EnabledStatus");
        table.Columns.Add("ClinicId");
        table.Columns.Add("ClinicKey");
        table.Columns.Add("AccountId");
        table.Columns.Add("RegistrationKeyNum");
        foreach (var clinicErx in listClinicErxs)
            table.Rows.Add(SOut.Long(clinicErx.ClinicErxNum), SOut.Long(clinicErx.PatNum), clinicErx.ClinicDesc, SOut.Long(clinicErx.ClinicNum), SOut.Int((int) clinicErx.EnabledStatus), clinicErx.ClinicId, clinicErx.ClinicKey, clinicErx.AccountId, SOut.Long(clinicErx.RegistrationKeyNum));
        return table;
    }

    public static long Insert(ClinicErx clinicErx)
    {
        var command = "INSERT INTO clinicerx (";

        command += "PatNum,ClinicDesc,ClinicNum,EnabledStatus,ClinicId,ClinicKey,AccountId,RegistrationKeyNum) VALUES(";

        command +=
            SOut.Long(clinicErx.PatNum) + ","
                                        + "'" + SOut.String(clinicErx.ClinicDesc) + "',"
                                        + SOut.Long(clinicErx.ClinicNum) + ","
                                        + SOut.Int((int) clinicErx.EnabledStatus) + ","
                                        + "'" + SOut.String(clinicErx.ClinicId) + "',"
                                        + "'" + SOut.String(clinicErx.ClinicKey) + "',"
                                        + "'" + SOut.String(clinicErx.AccountId) + "',"
                                        + SOut.Long(clinicErx.RegistrationKeyNum) + ")";
        {
            clinicErx.ClinicErxNum = Db.NonQ(command, true, "ClinicErxNum", "clinicErx");
        }
        return clinicErx.ClinicErxNum;
    }

    public static void Update(ClinicErx clinicErx)
    {
        var command = "UPDATE clinicerx SET "
                      + "PatNum            =  " + SOut.Long(clinicErx.PatNum) + ", "
                      + "ClinicDesc        = '" + SOut.String(clinicErx.ClinicDesc) + "', "
                      + "ClinicNum         =  " + SOut.Long(clinicErx.ClinicNum) + ", "
                      + "EnabledStatus     =  " + SOut.Int((int) clinicErx.EnabledStatus) + ", "
                      + "ClinicId          = '" + SOut.String(clinicErx.ClinicId) + "', "
                      + "ClinicKey         = '" + SOut.String(clinicErx.ClinicKey) + "', "
                      + "AccountId         = '" + SOut.String(clinicErx.AccountId) + "', "
                      + "RegistrationKeyNum=  " + SOut.Long(clinicErx.RegistrationKeyNum) + " "
                      + "WHERE ClinicErxNum = " + SOut.Long(clinicErx.ClinicErxNum);
        Db.NonQ(command);
    }

    public static bool Update(ClinicErx clinicErx, ClinicErx oldClinicErx)
    {
        var command = "";
        if (clinicErx.PatNum != oldClinicErx.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(clinicErx.PatNum) + "";
        }

        if (clinicErx.ClinicDesc != oldClinicErx.ClinicDesc)
        {
            if (command != "") command += ",";
            command += "ClinicDesc = '" + SOut.String(clinicErx.ClinicDesc) + "'";
        }

        if (clinicErx.ClinicNum != oldClinicErx.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(clinicErx.ClinicNum) + "";
        }

        if (clinicErx.EnabledStatus != oldClinicErx.EnabledStatus)
        {
            if (command != "") command += ",";
            command += "EnabledStatus = " + SOut.Int((int) clinicErx.EnabledStatus) + "";
        }

        if (clinicErx.ClinicId != oldClinicErx.ClinicId)
        {
            if (command != "") command += ",";
            command += "ClinicId = '" + SOut.String(clinicErx.ClinicId) + "'";
        }

        if (clinicErx.ClinicKey != oldClinicErx.ClinicKey)
        {
            if (command != "") command += ",";
            command += "ClinicKey = '" + SOut.String(clinicErx.ClinicKey) + "'";
        }

        if (clinicErx.AccountId != oldClinicErx.AccountId)
        {
            if (command != "") command += ",";
            command += "AccountId = '" + SOut.String(clinicErx.AccountId) + "'";
        }

        if (clinicErx.RegistrationKeyNum != oldClinicErx.RegistrationKeyNum)
        {
            if (command != "") command += ",";
            command += "RegistrationKeyNum = " + SOut.Long(clinicErx.RegistrationKeyNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE clinicerx SET " + command
                                          + " WHERE ClinicErxNum = " + SOut.Long(clinicErx.ClinicErxNum);
        Db.NonQ(command);
        return true;
    }
}