#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class MobileBrandingProfileCrud
{
    public static MobileBrandingProfile SelectOne(long mobileBrandingProfileNum)
    {
        var command = "SELECT * FROM mobilebrandingprofile "
                      + "WHERE MobileBrandingProfileNum = " + SOut.Long(mobileBrandingProfileNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static MobileBrandingProfile SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<MobileBrandingProfile> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MobileBrandingProfile> TableToList(DataTable table)
    {
        var retVal = new List<MobileBrandingProfile>();
        MobileBrandingProfile mobileBrandingProfile;
        foreach (DataRow row in table.Rows)
        {
            mobileBrandingProfile = new MobileBrandingProfile();
            mobileBrandingProfile.MobileBrandingProfileNum = SIn.Long(row["MobileBrandingProfileNum"].ToString());
            mobileBrandingProfile.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            mobileBrandingProfile.OfficeDescription = SIn.String(row["OfficeDescription"].ToString());
            mobileBrandingProfile.LogoFilePath = SIn.String(row["LogoFilePath"].ToString());
            mobileBrandingProfile.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            retVal.Add(mobileBrandingProfile);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<MobileBrandingProfile> listMobileBrandingProfiles, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "MobileBrandingProfile";
        var table = new DataTable(tableName);
        table.Columns.Add("MobileBrandingProfileNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("OfficeDescription");
        table.Columns.Add("LogoFilePath");
        table.Columns.Add("DateTStamp");
        foreach (var mobileBrandingProfile in listMobileBrandingProfiles)
            table.Rows.Add(SOut.Long(mobileBrandingProfile.MobileBrandingProfileNum), SOut.Long(mobileBrandingProfile.ClinicNum), mobileBrandingProfile.OfficeDescription, mobileBrandingProfile.LogoFilePath, SOut.DateT(mobileBrandingProfile.DateTStamp, false));
        return table;
    }

    public static long Insert(MobileBrandingProfile mobileBrandingProfile)
    {
        return Insert(mobileBrandingProfile, false);
    }

    public static long Insert(MobileBrandingProfile mobileBrandingProfile, bool useExistingPK)
    {
        var command = "INSERT INTO mobilebrandingprofile (";

        command += "ClinicNum,OfficeDescription,LogoFilePath) VALUES(";

        command +=
            SOut.Long(mobileBrandingProfile.ClinicNum) + ","
                                                       + "'" + SOut.String(mobileBrandingProfile.OfficeDescription) + "',"
                                                       + "'" + SOut.String(mobileBrandingProfile.LogoFilePath) + "')";
        //DateTStamp can only be set by MySQL

        mobileBrandingProfile.MobileBrandingProfileNum = Db.NonQ(command, true, "MobileBrandingProfileNum", "mobileBrandingProfile");
        return mobileBrandingProfile.MobileBrandingProfileNum;
    }

    public static long InsertNoCache(MobileBrandingProfile mobileBrandingProfile)
    {
        return InsertNoCache(mobileBrandingProfile, false);
    }

    public static long InsertNoCache(MobileBrandingProfile mobileBrandingProfile, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO mobilebrandingprofile (";
        if (isRandomKeys || useExistingPK) command += "MobileBrandingProfileNum,";
        command += "ClinicNum,OfficeDescription,LogoFilePath) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(mobileBrandingProfile.MobileBrandingProfileNum) + ",";
        command +=
            SOut.Long(mobileBrandingProfile.ClinicNum) + ","
                                                       + "'" + SOut.String(mobileBrandingProfile.OfficeDescription) + "',"
                                                       + "'" + SOut.String(mobileBrandingProfile.LogoFilePath) + "')";
        //DateTStamp can only be set by MySQL
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            mobileBrandingProfile.MobileBrandingProfileNum = Db.NonQ(command, true, "MobileBrandingProfileNum", "mobileBrandingProfile");
        return mobileBrandingProfile.MobileBrandingProfileNum;
    }

    public static void Update(MobileBrandingProfile mobileBrandingProfile)
    {
        var command = "UPDATE mobilebrandingprofile SET "
                      + "ClinicNum               =  " + SOut.Long(mobileBrandingProfile.ClinicNum) + ", "
                      + "OfficeDescription       = '" + SOut.String(mobileBrandingProfile.OfficeDescription) + "', "
                      + "LogoFilePath            = '" + SOut.String(mobileBrandingProfile.LogoFilePath) + "' "
                      //DateTStamp can only be set by MySQL
                      + "WHERE MobileBrandingProfileNum = " + SOut.Long(mobileBrandingProfile.MobileBrandingProfileNum);
        Db.NonQ(command);
    }

    public static bool Update(MobileBrandingProfile mobileBrandingProfile, MobileBrandingProfile oldMobileBrandingProfile)
    {
        var command = "";
        if (mobileBrandingProfile.ClinicNum != oldMobileBrandingProfile.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(mobileBrandingProfile.ClinicNum) + "";
        }

        if (mobileBrandingProfile.OfficeDescription != oldMobileBrandingProfile.OfficeDescription)
        {
            if (command != "") command += ",";
            command += "OfficeDescription = '" + SOut.String(mobileBrandingProfile.OfficeDescription) + "'";
        }

        if (mobileBrandingProfile.LogoFilePath != oldMobileBrandingProfile.LogoFilePath)
        {
            if (command != "") command += ",";
            command += "LogoFilePath = '" + SOut.String(mobileBrandingProfile.LogoFilePath) + "'";
        }

        //DateTStamp can only be set by MySQL
        if (command == "") return false;
        command = "UPDATE mobilebrandingprofile SET " + command
                                                      + " WHERE MobileBrandingProfileNum = " + SOut.Long(mobileBrandingProfile.MobileBrandingProfileNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(MobileBrandingProfile mobileBrandingProfile, MobileBrandingProfile oldMobileBrandingProfile)
    {
        if (mobileBrandingProfile.ClinicNum != oldMobileBrandingProfile.ClinicNum) return true;
        if (mobileBrandingProfile.OfficeDescription != oldMobileBrandingProfile.OfficeDescription) return true;
        if (mobileBrandingProfile.LogoFilePath != oldMobileBrandingProfile.LogoFilePath) return true;
        //DateTStamp can only be set by MySQL
        return false;
    }

    public static void Delete(long mobileBrandingProfileNum)
    {
        var command = "DELETE FROM mobilebrandingprofile "
                      + "WHERE MobileBrandingProfileNum = " + SOut.Long(mobileBrandingProfileNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listMobileBrandingProfileNums)
    {
        if (listMobileBrandingProfileNums == null || listMobileBrandingProfileNums.Count == 0) return;
        var command = "DELETE FROM mobilebrandingprofile "
                      + "WHERE MobileBrandingProfileNum IN(" + string.Join(",", listMobileBrandingProfileNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}