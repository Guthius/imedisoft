#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class UserodCrud
{
    public static Userod SelectOne(long userNum)
    {
        var command = "SELECT * FROM userod "
                      + "WHERE UserNum = " + SOut.Long(userNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

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
        Userod userod;
        foreach (DataRow row in table.Rows)
        {
            userod = new Userod();
            userod.UserNum = SIn.Long(row["UserNum"].ToString());
            userod.UserName = SIn.String(row["UserName"].ToString());
            userod.Password = SIn.String(row["Password"].ToString());
            userod.UserGroupNum = SIn.Long(row["UserGroupNum"].ToString());
            userod.EmployeeNum = SIn.Long(row["EmployeeNum"].ToString());
            userod.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            userod.ProvNum = SIn.Long(row["ProvNum"].ToString());
            userod.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            userod.TaskListInBox = SIn.Long(row["TaskListInBox"].ToString());
            userod.AnesthProvType = SIn.Int(row["AnesthProvType"].ToString());
            userod.DefaultHidePopups = SIn.Bool(row["DefaultHidePopups"].ToString());
            userod.PasswordIsStrong = SIn.Bool(row["PasswordIsStrong"].ToString());
            userod.ClinicIsRestricted = SIn.Bool(row["ClinicIsRestricted"].ToString());
            userod.InboxHidePopups = SIn.Bool(row["InboxHidePopups"].ToString());
            userod.UserNumCEMT = SIn.Long(row["UserNumCEMT"].ToString());
            userod.DateTFail = SIn.DateTime(row["DateTFail"].ToString());
            userod.FailedAttempts = SIn.Byte(row["FailedAttempts"].ToString());
            userod.DomainUser = SIn.String(row["DomainUser"].ToString());
            userod.IsPasswordResetRequired = SIn.Bool(row["IsPasswordResetRequired"].ToString());
            userod.MobileWebPin = SIn.String(row["MobileWebPin"].ToString());
            userod.MobileWebPinFailedAttempts = SIn.Byte(row["MobileWebPinFailedAttempts"].ToString());
            userod.DateTLastLogin = SIn.DateTime(row["DateTLastLogin"].ToString());
            userod.EClipboardClinicalPin = SIn.String(row["EClipboardClinicalPin"].ToString());
            userod.BadgeId = SIn.String(row["BadgeId"].ToString());
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
            table.Rows.Add(SOut.Long(userod.UserNum), userod.UserName, userod.Password, SOut.Long(userod.UserGroupNum), SOut.Long(userod.EmployeeNum), SOut.Long(userod.ClinicNum), SOut.Long(userod.ProvNum), SOut.Bool(userod.IsHidden), SOut.Long(userod.TaskListInBox), SOut.Int(userod.AnesthProvType), SOut.Bool(userod.DefaultHidePopups), SOut.Bool(userod.PasswordIsStrong), SOut.Bool(userod.ClinicIsRestricted), SOut.Bool(userod.InboxHidePopups), SOut.Long(userod.UserNumCEMT), SOut.DateT(userod.DateTFail, false), SOut.Byte(userod.FailedAttempts), userod.DomainUser, SOut.Bool(userod.IsPasswordResetRequired), userod.MobileWebPin, SOut.Byte(userod.MobileWebPinFailedAttempts), SOut.DateT(userod.DateTLastLogin, false), userod.EClipboardClinicalPin, userod.BadgeId);
        return table;
    }

    public static long Insert(Userod userod)
    {
        return Insert(userod, false);
    }

    public static long Insert(Userod userod, bool useExistingPK)
    {
        var command = "INSERT INTO userod (";

        command += "UserName,Password,UserGroupNum,EmployeeNum,ClinicNum,ProvNum,IsHidden,TaskListInBox,AnesthProvType,DefaultHidePopups,PasswordIsStrong,ClinicIsRestricted,InboxHidePopups,UserNumCEMT,DateTFail,FailedAttempts,DomainUser,IsPasswordResetRequired,MobileWebPin,MobileWebPinFailedAttempts,DateTLastLogin,EClipboardClinicalPin,BadgeId) VALUES(";

        command +=
            "'" + SOut.String(userod.UserName) + "',"
            + "'" + SOut.String(userod.Password) + "',"
            + SOut.Long(userod.UserGroupNum) + ","
            + SOut.Long(userod.EmployeeNum) + ","
            + SOut.Long(userod.ClinicNum) + ","
            + SOut.Long(userod.ProvNum) + ","
            + SOut.Bool(userod.IsHidden) + ","
            + SOut.Long(userod.TaskListInBox) + ","
            + SOut.Int(userod.AnesthProvType) + ","
            + SOut.Bool(userod.DefaultHidePopups) + ","
            + SOut.Bool(userod.PasswordIsStrong) + ","
            + SOut.Bool(userod.ClinicIsRestricted) + ","
            + SOut.Bool(userod.InboxHidePopups) + ","
            + SOut.Long(userod.UserNumCEMT) + ","
            + SOut.DateT(userod.DateTFail) + ","
            + SOut.Byte(userod.FailedAttempts) + ","
            + "'" + SOut.String(userod.DomainUser) + "',"
            + SOut.Bool(userod.IsPasswordResetRequired) + ","
            + "'" + SOut.String(userod.MobileWebPin) + "',"
            + SOut.Byte(userod.MobileWebPinFailedAttempts) + ","
            + SOut.DateT(userod.DateTLastLogin) + ","
            + "'" + SOut.String(userod.EClipboardClinicalPin) + "',"
            + "'" + SOut.String(userod.BadgeId) + "')";
        {
            userod.UserNum = Db.NonQ(command, true, "UserNum", "userod");
        }
        return userod.UserNum;
    }

    public static long InsertNoCache(Userod userod)
    {
        return InsertNoCache(userod, false);
    }

    public static long InsertNoCache(Userod userod, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO userod (";
        if (isRandomKeys || useExistingPK) command += "UserNum,";
        command += "UserName,Password,UserGroupNum,EmployeeNum,ClinicNum,ProvNum,IsHidden,TaskListInBox,AnesthProvType,DefaultHidePopups,PasswordIsStrong,ClinicIsRestricted,InboxHidePopups,UserNumCEMT,DateTFail,FailedAttempts,DomainUser,IsPasswordResetRequired,MobileWebPin,MobileWebPinFailedAttempts,DateTLastLogin,EClipboardClinicalPin,BadgeId) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(userod.UserNum) + ",";
        command +=
            "'" + SOut.String(userod.UserName) + "',"
            + "'" + SOut.String(userod.Password) + "',"
            + SOut.Long(userod.UserGroupNum) + ","
            + SOut.Long(userod.EmployeeNum) + ","
            + SOut.Long(userod.ClinicNum) + ","
            + SOut.Long(userod.ProvNum) + ","
            + SOut.Bool(userod.IsHidden) + ","
            + SOut.Long(userod.TaskListInBox) + ","
            + SOut.Int(userod.AnesthProvType) + ","
            + SOut.Bool(userod.DefaultHidePopups) + ","
            + SOut.Bool(userod.PasswordIsStrong) + ","
            + SOut.Bool(userod.ClinicIsRestricted) + ","
            + SOut.Bool(userod.InboxHidePopups) + ","
            + SOut.Long(userod.UserNumCEMT) + ","
            + SOut.DateT(userod.DateTFail) + ","
            + SOut.Byte(userod.FailedAttempts) + ","
            + "'" + SOut.String(userod.DomainUser) + "',"
            + SOut.Bool(userod.IsPasswordResetRequired) + ","
            + "'" + SOut.String(userod.MobileWebPin) + "',"
            + SOut.Byte(userod.MobileWebPinFailedAttempts) + ","
            + SOut.DateT(userod.DateTLastLogin) + ","
            + "'" + SOut.String(userod.EClipboardClinicalPin) + "',"
            + "'" + SOut.String(userod.BadgeId) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            userod.UserNum = Db.NonQ(command, true, "UserNum", "userod");
        return userod.UserNum;
    }

    public static void Update(Userod userod)
    {
        var command = "UPDATE userod SET "
                      + "UserName                  = '" + SOut.String(userod.UserName) + "', "
                      + "Password                  = '" + SOut.String(userod.Password) + "', "
                      + "UserGroupNum              =  " + SOut.Long(userod.UserGroupNum) + ", "
                      + "EmployeeNum               =  " + SOut.Long(userod.EmployeeNum) + ", "
                      + "ClinicNum                 =  " + SOut.Long(userod.ClinicNum) + ", "
                      + "ProvNum                   =  " + SOut.Long(userod.ProvNum) + ", "
                      + "IsHidden                  =  " + SOut.Bool(userod.IsHidden) + ", "
                      + "TaskListInBox             =  " + SOut.Long(userod.TaskListInBox) + ", "
                      + "AnesthProvType            =  " + SOut.Int(userod.AnesthProvType) + ", "
                      + "DefaultHidePopups         =  " + SOut.Bool(userod.DefaultHidePopups) + ", "
                      + "PasswordIsStrong          =  " + SOut.Bool(userod.PasswordIsStrong) + ", "
                      + "ClinicIsRestricted        =  " + SOut.Bool(userod.ClinicIsRestricted) + ", "
                      + "InboxHidePopups           =  " + SOut.Bool(userod.InboxHidePopups) + ", "
                      + "UserNumCEMT               =  " + SOut.Long(userod.UserNumCEMT) + ", "
                      + "DateTFail                 =  " + SOut.DateT(userod.DateTFail) + ", "
                      + "FailedAttempts            =  " + SOut.Byte(userod.FailedAttempts) + ", "
                      + "DomainUser                = '" + SOut.String(userod.DomainUser) + "', "
                      + "IsPasswordResetRequired   =  " + SOut.Bool(userod.IsPasswordResetRequired) + ", "
                      + "MobileWebPin              = '" + SOut.String(userod.MobileWebPin) + "', "
                      + "MobileWebPinFailedAttempts=  " + SOut.Byte(userod.MobileWebPinFailedAttempts) + ", "
                      + "DateTLastLogin            =  " + SOut.DateT(userod.DateTLastLogin) + ", "
                      + "EClipboardClinicalPin     = '" + SOut.String(userod.EClipboardClinicalPin) + "', "
                      + "BadgeId                   = '" + SOut.String(userod.BadgeId) + "' "
                      + "WHERE UserNum = " + SOut.Long(userod.UserNum);
        Db.NonQ(command);
    }

    public static bool Update(Userod userod, Userod oldUserod)
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

        if (userod.UserGroupNum != oldUserod.UserGroupNum)
        {
            if (command != "") command += ",";
            command += "UserGroupNum = " + SOut.Long(userod.UserGroupNum) + "";
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

        if (userod.AnesthProvType != oldUserod.AnesthProvType)
        {
            if (command != "") command += ",";
            command += "AnesthProvType = " + SOut.Int(userod.AnesthProvType) + "";
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

        if (userod.InboxHidePopups != oldUserod.InboxHidePopups)
        {
            if (command != "") command += ",";
            command += "InboxHidePopups = " + SOut.Bool(userod.InboxHidePopups) + "";
        }

        if (userod.UserNumCEMT != oldUserod.UserNumCEMT)
        {
            if (command != "") command += ",";
            command += "UserNumCEMT = " + SOut.Long(userod.UserNumCEMT) + "";
        }

        if (userod.DateTFail != oldUserod.DateTFail)
        {
            if (command != "") command += ",";
            command += "DateTFail = " + SOut.DateT(userod.DateTFail) + "";
        }

        if (userod.FailedAttempts != oldUserod.FailedAttempts)
        {
            if (command != "") command += ",";
            command += "FailedAttempts = " + SOut.Byte(userod.FailedAttempts) + "";
        }

        if (userod.DomainUser != oldUserod.DomainUser)
        {
            if (command != "") command += ",";
            command += "DomainUser = '" + SOut.String(userod.DomainUser) + "'";
        }

        if (userod.IsPasswordResetRequired != oldUserod.IsPasswordResetRequired)
        {
            if (command != "") command += ",";
            command += "IsPasswordResetRequired = " + SOut.Bool(userod.IsPasswordResetRequired) + "";
        }

        if (userod.MobileWebPin != oldUserod.MobileWebPin)
        {
            if (command != "") command += ",";
            command += "MobileWebPin = '" + SOut.String(userod.MobileWebPin) + "'";
        }

        if (userod.MobileWebPinFailedAttempts != oldUserod.MobileWebPinFailedAttempts)
        {
            if (command != "") command += ",";
            command += "MobileWebPinFailedAttempts = " + SOut.Byte(userod.MobileWebPinFailedAttempts) + "";
        }

        if (userod.DateTLastLogin != oldUserod.DateTLastLogin)
        {
            if (command != "") command += ",";
            command += "DateTLastLogin = " + SOut.DateT(userod.DateTLastLogin) + "";
        }

        if (userod.EClipboardClinicalPin != oldUserod.EClipboardClinicalPin)
        {
            if (command != "") command += ",";
            command += "EClipboardClinicalPin = '" + SOut.String(userod.EClipboardClinicalPin) + "'";
        }

        if (userod.BadgeId != oldUserod.BadgeId)
        {
            if (command != "") command += ",";
            command += "BadgeId = '" + SOut.String(userod.BadgeId) + "'";
        }

        if (command == "") return false;
        command = "UPDATE userod SET " + command
                                       + " WHERE UserNum = " + SOut.Long(userod.UserNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Userod userod, Userod oldUserod)
    {
        if (userod.UserName != oldUserod.UserName) return true;
        if (userod.Password != oldUserod.Password) return true;
        if (userod.UserGroupNum != oldUserod.UserGroupNum) return true;
        if (userod.EmployeeNum != oldUserod.EmployeeNum) return true;
        if (userod.ClinicNum != oldUserod.ClinicNum) return true;
        if (userod.ProvNum != oldUserod.ProvNum) return true;
        if (userod.IsHidden != oldUserod.IsHidden) return true;
        if (userod.TaskListInBox != oldUserod.TaskListInBox) return true;
        if (userod.AnesthProvType != oldUserod.AnesthProvType) return true;
        if (userod.DefaultHidePopups != oldUserod.DefaultHidePopups) return true;
        if (userod.PasswordIsStrong != oldUserod.PasswordIsStrong) return true;
        if (userod.ClinicIsRestricted != oldUserod.ClinicIsRestricted) return true;
        if (userod.InboxHidePopups != oldUserod.InboxHidePopups) return true;
        if (userod.UserNumCEMT != oldUserod.UserNumCEMT) return true;
        if (userod.DateTFail != oldUserod.DateTFail) return true;
        if (userod.FailedAttempts != oldUserod.FailedAttempts) return true;
        if (userod.DomainUser != oldUserod.DomainUser) return true;
        if (userod.IsPasswordResetRequired != oldUserod.IsPasswordResetRequired) return true;
        if (userod.MobileWebPin != oldUserod.MobileWebPin) return true;
        if (userod.MobileWebPinFailedAttempts != oldUserod.MobileWebPinFailedAttempts) return true;
        if (userod.DateTLastLogin != oldUserod.DateTLastLogin) return true;
        if (userod.EClipboardClinicalPin != oldUserod.EClipboardClinicalPin) return true;
        if (userod.BadgeId != oldUserod.BadgeId) return true;
        return false;
    }

    public static void UpdateCemt(Userod userod)
    {
        var command = "UPDATE userod SET "
                      + "UserName             = '" + SOut.String(userod.UserName) + "', "
                      + "Password             = '" + SOut.String(userod.Password) + "', "
                      + "ClinicNum            =  " + SOut.Long(userod.ClinicNum) + ", "
                      + "IsHidden             =  " + SOut.Bool(userod.IsHidden) + ", "
                      + "TaskListInBox        =  " + SOut.Long(userod.TaskListInBox) + ", "
                      + "AnesthProvType       =  " + SOut.Int(userod.AnesthProvType) + ", "
                      + "DefaultHidePopups    =  " + SOut.Bool(userod.DefaultHidePopups) + ", "
                      + "PasswordIsStrong     =  " + SOut.Bool(userod.PasswordIsStrong) + ", "
                      + "ClinicIsRestricted   =  " + SOut.Bool(userod.ClinicIsRestricted) + ", "
                      + "InboxHidePopups      =  " + SOut.Bool(userod.InboxHidePopups) + ", "
                      + "DomainUser           = '" + SOut.String(userod.DomainUser) + "', "
                      + "DateTLastLogin       =  " + SOut.DateT(userod.DateTLastLogin) + ", "
                      + "EClipboardClinicalPin= '" + SOut.String(userod.EClipboardClinicalPin) + "', "
                      + "BadgeId              = '" + SOut.String(userod.BadgeId) + "' "
                      + "WHERE UserNumCEMT = " + SOut.Long(userod.UserNumCEMT);
        Db.NonQ(command);
    }

    public static void Delete(long userNum)
    {
        var command = "DELETE FROM userod "
                      + "WHERE UserNum = " + SOut.Long(userNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listUserNums)
    {
        if (listUserNums == null || listUserNums.Count == 0) return;
        var command = "DELETE FROM userod "
                      + "WHERE UserNum IN(" + string.Join(",", listUserNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}