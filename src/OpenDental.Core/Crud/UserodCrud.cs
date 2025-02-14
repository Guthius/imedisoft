using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class UserodCrud
{
    public static Userod SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Userod> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Userod> TableToList(DataTable table)
    {
        var retVal = new List<Userod>();
        foreach (DataRow row in table.Rows)
        {
            var userod = new Userod
            {
                UserNum = SIn.Long(row["UserNum"].ToString()),
                UserName = SIn.String(row["UserName"].ToString()),
                Password = SIn.String(row["Password"].ToString()),
                EmployeeNum = SIn.Long(row["EmployeeNum"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                ProvNum = SIn.Long(row["ProvNum"].ToString()),
                IsHidden = SIn.Bool(row["IsHidden"].ToString()),
                TaskListInBox = SIn.Long(row["TaskListInBox"].ToString()),
                DefaultHidePopups = SIn.Bool(row["DefaultHidePopups"].ToString()),
                PasswordIsStrong = SIn.Bool(row["PasswordIsStrong"].ToString()),
                ClinicIsRestricted = SIn.Bool(row["ClinicIsRestricted"].ToString()),
                DateTFail = SIn.DateTime(row["DateTFail"].ToString()),
                FailedAttempts = SIn.Byte(row["FailedAttempts"].ToString()),
                IsPasswordResetRequired = SIn.Bool(row["IsPasswordResetRequired"].ToString()),
                DateTLastLogin = SIn.DateTime(row["DateTLastLogin"].ToString())
            };
            retVal.Add(userod);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Userod> listUserods, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Userod";
        var table = new DataTable(tableName);
        table.Columns.Add("UserNum");
        table.Columns.Add("UserName");
        table.Columns.Add("Password");
        table.Columns.Add("UserGroupNum");
        table.Columns.Add("EmployeeNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("IsHidden");
        table.Columns.Add("TaskListInBox");
        table.Columns.Add("AnesthProvType");
        table.Columns.Add("DefaultHidePopups");
        table.Columns.Add("PasswordIsStrong");
        table.Columns.Add("ClinicIsRestricted");
        table.Columns.Add("InboxHidePopups");
        table.Columns.Add("UserNumCEMT");
        table.Columns.Add("DateTFail");
        table.Columns.Add("FailedAttempts");
        table.Columns.Add("DomainUser");
        table.Columns.Add("IsPasswordResetRequired");
        table.Columns.Add("MobileWebPin");
        table.Columns.Add("MobileWebPinFailedAttempts");
        table.Columns.Add("DateTLastLogin");
        table.Columns.Add("EClipboardClinicalPin");
        table.Columns.Add("BadgeId");
        foreach (var userod in listUserods)
            table.Rows.Add(SOut.Long(userod.UserNum), userod.UserName, userod.Password, SOut.Long(userod.EmployeeNum), SOut.Long(userod.ClinicNum), SOut.Long(userod.ProvNum), SOut.Bool(userod.IsHidden), SOut.Long(userod.TaskListInBox), SOut.Bool(userod.DefaultHidePopups), SOut.Bool(userod.PasswordIsStrong), SOut.Bool(userod.ClinicIsRestricted), SOut.DateTime(userod.DateTFail, false), SOut.Byte(userod.FailedAttempts), SOut.Bool(userod.IsPasswordResetRequired), SOut.DateTime(userod.DateTLastLogin, false));
        return table;
    }

    public static long Insert(Userod userod)
    {
        var command = "INSERT INTO userod (";

        command += "UserName,Password,UserGroupNum,EmployeeNum,ClinicNum,ProvNum,IsHidden,TaskListInBox,AnesthProvType,DefaultHidePopups,PasswordIsStrong,ClinicIsRestricted,InboxHidePopups,UserNumCEMT,DateTFail,FailedAttempts,DomainUser,IsPasswordResetRequired,MobileWebPin,MobileWebPinFailedAttempts,DateTLastLogin,EClipboardClinicalPin,BadgeId) VALUES(";

        command +=
            "'" + SOut.String(userod.UserName) + "',"
            + "'" + SOut.String(userod.Password) + "',"
            + SOut.Long(userod.EmployeeNum) + ","
            + SOut.Long(userod.ClinicNum) + ","
            + SOut.Long(userod.ProvNum) + ","
            + SOut.Bool(userod.IsHidden) + ","
            + SOut.Long(userod.TaskListInBox) + ","
            + SOut.Bool(userod.DefaultHidePopups) + ","
            + SOut.Bool(userod.PasswordIsStrong) + ","
            + SOut.Bool(userod.ClinicIsRestricted) + ","
            + SOut.DateTime(userod.DateTFail) + ","
            + SOut.Byte(userod.FailedAttempts) + ","
            + SOut.Bool(userod.IsPasswordResetRequired) + ","
            + SOut.DateTime(userod.DateTLastLogin) + ",";
        {
            userod.UserNum = Db.NonQ(command, true, "UserNum", "userod");
        }
        return userod.UserNum;
    }

    public static void Update(Userod userod)
    {
        var command = "UPDATE userod SET "
                      + "UserName                  = '" + SOut.String(userod.UserName) + "', "
                      + "Password                  = '" + SOut.String(userod.Password) + "', "
                      + "EmployeeNum               =  " + SOut.Long(userod.EmployeeNum) + ", "
                      + "ClinicNum                 =  " + SOut.Long(userod.ClinicNum) + ", "
                      + "ProvNum                   =  " + SOut.Long(userod.ProvNum) + ", "
                      + "IsHidden                  =  " + SOut.Bool(userod.IsHidden) + ", "
                      + "TaskListInBox             =  " + SOut.Long(userod.TaskListInBox) + ", "
                      + "DefaultHidePopups         =  " + SOut.Bool(userod.DefaultHidePopups) + ", "
                      + "PasswordIsStrong          =  " + SOut.Bool(userod.PasswordIsStrong) + ", "
                      + "ClinicIsRestricted        =  " + SOut.Bool(userod.ClinicIsRestricted) + ", "
                      + "DateTFail                 =  " + SOut.DateTime(userod.DateTFail) + ", "
                      + "FailedAttempts            =  " + SOut.Byte(userod.FailedAttempts) + ", "
                      + "IsPasswordResetRequired   =  " + SOut.Bool(userod.IsPasswordResetRequired) + ", "
                      + "DateTLastLogin            =  " + SOut.DateTime(userod.DateTLastLogin) + ", "
                      + "WHERE UserNum = " + SOut.Long(userod.UserNum);
        Db.NonQ(command);
    }

    public static void Update(Userod userod, Userod oldUserod)
    {
        var command = "";
        if (userod.UserName != oldUserod.UserName)
        {
            if (command != "") command += ",";
            command += "UserName = '" + SOut.String(userod.UserName) + "'";
        }

        if (userod.Password != oldUserod.Password)
        {
            if (command != "") command += ",";
            command += "Password = '" + SOut.String(userod.Password) + "'";
        }
        
        if (userod.EmployeeNum != oldUserod.EmployeeNum)
        {
            if (command != "") command += ",";
            command += "EmployeeNum = " + SOut.Long(userod.EmployeeNum) + "";
        }

        if (userod.ClinicNum != oldUserod.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(userod.ClinicNum) + "";
        }

        if (userod.ProvNum != oldUserod.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(userod.ProvNum) + "";
        }

        if (userod.IsHidden != oldUserod.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(userod.IsHidden) + "";
        }

        if (userod.TaskListInBox != oldUserod.TaskListInBox)
        {
            if (command != "") command += ",";
            command += "TaskListInBox = " + SOut.Long(userod.TaskListInBox) + "";
        }
        
        if (userod.DefaultHidePopups != oldUserod.DefaultHidePopups)
        {
            if (command != "") command += ",";
            command += "DefaultHidePopups = " + SOut.Bool(userod.DefaultHidePopups) + "";
        }

        if (userod.PasswordIsStrong != oldUserod.PasswordIsStrong)
        {
            if (command != "") command += ",";
            command += "PasswordIsStrong = " + SOut.Bool(userod.PasswordIsStrong) + "";
        }

        if (userod.ClinicIsRestricted != oldUserod.ClinicIsRestricted)
        {
            if (command != "") command += ",";
            command += "ClinicIsRestricted = " + SOut.Bool(userod.ClinicIsRestricted) + "";
        }
        
        if (userod.DateTFail != oldUserod.DateTFail)
        {
            if (command != "") command += ",";
            command += "DateTFail = " + SOut.DateTime(userod.DateTFail) + "";
        }

        if (userod.FailedAttempts != oldUserod.FailedAttempts)
        {
            if (command != "") command += ",";
            command += "FailedAttempts = " + SOut.Byte(userod.FailedAttempts) + "";
        }
        
        if (userod.IsPasswordResetRequired != oldUserod.IsPasswordResetRequired)
        {
            if (command != "") command += ",";
            command += "IsPasswordResetRequired = " + SOut.Bool(userod.IsPasswordResetRequired) + "";
        }
        
        if (userod.DateTLastLogin != oldUserod.DateTLastLogin)
        {
            if (command != "") command += ",";
            command += "DateTLastLogin = " + SOut.DateTime(userod.DateTLastLogin) + "";
        }

        if (command == "") return;
        command = "UPDATE userod SET " + command
                                       + " WHERE UserNum = " + SOut.Long(userod.UserNum);
        Db.NonQ(command);
    }
}