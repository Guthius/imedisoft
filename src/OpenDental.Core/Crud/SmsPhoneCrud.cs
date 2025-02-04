using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class SmsPhoneCrud
{
    public static List<SmsPhone> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SmsPhone> TableToList(DataTable table)
    {
        var retVal = new List<SmsPhone>();
        foreach (DataRow row in table.Rows)
        {
            var smsPhone = new SmsPhone
            {
                SmsPhoneNum = SIn.Long(row["SmsPhoneNum"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                PhoneNumber = SIn.String(row["PhoneNumber"].ToString()),
                DateTimeActive = SIn.DateTime(row["DateTimeActive"].ToString()),
                DateTimeInactive = SIn.DateTime(row["DateTimeInactive"].ToString()),
                InactiveCode = SIn.String(row["InactiveCode"].ToString()),
                CountryCode = SIn.String(row["CountryCode"].ToString())
            };
            retVal.Add(smsPhone);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SmsPhone> listSmsPhones, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SmsPhone";
        var table = new DataTable(tableName);
        table.Columns.Add("SmsPhoneNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("PhoneNumber");
        table.Columns.Add("DateTimeActive");
        table.Columns.Add("DateTimeInactive");
        table.Columns.Add("InactiveCode");
        table.Columns.Add("CountryCode");
        foreach (var smsPhone in listSmsPhones)
            table.Rows.Add(SOut.Long(smsPhone.SmsPhoneNum), SOut.Long(smsPhone.ClinicNum), smsPhone.PhoneNumber, SOut.DateTime(smsPhone.DateTimeActive, false), SOut.DateTime(smsPhone.DateTimeInactive, false), smsPhone.InactiveCode, smsPhone.CountryCode);
        return table;
    }
}