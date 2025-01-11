#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrLabImageCrud
{
    public static EhrLabImage SelectOne(long ehrLabImageNum)
    {
        var command = "SELECT * FROM ehrlabimage "
                      + "WHERE EhrLabImageNum = " + SOut.Long(ehrLabImageNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrLabImage SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrLabImage> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrLabImage> TableToList(DataTable table)
    {
        var retVal = new List<EhrLabImage>();
        EhrLabImage ehrLabImage;
        foreach (DataRow row in table.Rows)
        {
            ehrLabImage = new EhrLabImage();
            ehrLabImage.EhrLabImageNum = SIn.Long(row["EhrLabImageNum"].ToString());
            ehrLabImage.EhrLabNum = SIn.Long(row["EhrLabNum"].ToString());
            ehrLabImage.DocNum = SIn.Long(row["DocNum"].ToString());
            retVal.Add(ehrLabImage);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrLabImage> listEhrLabImages, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrLabImage";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrLabImageNum");
        table.Columns.Add("EhrLabNum");
        table.Columns.Add("DocNum");
        foreach (var ehrLabImage in listEhrLabImages)
            table.Rows.Add(SOut.Long(ehrLabImage.EhrLabImageNum), SOut.Long(ehrLabImage.EhrLabNum), SOut.Long(ehrLabImage.DocNum));
        return table;
    }

    public static long Insert(EhrLabImage ehrLabImage)
    {
        return Insert(ehrLabImage, false);
    }

    public static long Insert(EhrLabImage ehrLabImage, bool useExistingPK)
    {
        var command = "INSERT INTO ehrlabimage (";

        command += "EhrLabNum,DocNum) VALUES(";

        command +=
            SOut.Long(ehrLabImage.EhrLabNum) + ","
                                             + SOut.Long(ehrLabImage.DocNum) + ")";
        {
            ehrLabImage.EhrLabImageNum = Db.NonQ(command, true, "EhrLabImageNum", "ehrLabImage");
        }
        return ehrLabImage.EhrLabImageNum;
    }

    public static long InsertNoCache(EhrLabImage ehrLabImage)
    {
        return InsertNoCache(ehrLabImage, false);
    }

    public static long InsertNoCache(EhrLabImage ehrLabImage, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrlabimage (";
        if (isRandomKeys || useExistingPK) command += "EhrLabImageNum,";
        command += "EhrLabNum,DocNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrLabImage.EhrLabImageNum) + ",";
        command +=
            SOut.Long(ehrLabImage.EhrLabNum) + ","
                                             + SOut.Long(ehrLabImage.DocNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            ehrLabImage.EhrLabImageNum = Db.NonQ(command, true, "EhrLabImageNum", "ehrLabImage");
        return ehrLabImage.EhrLabImageNum;
    }

    public static void Update(EhrLabImage ehrLabImage)
    {
        var command = "UPDATE ehrlabimage SET "
                      + "EhrLabNum     =  " + SOut.Long(ehrLabImage.EhrLabNum) + ", "
                      + "DocNum        =  " + SOut.Long(ehrLabImage.DocNum) + " "
                      + "WHERE EhrLabImageNum = " + SOut.Long(ehrLabImage.EhrLabImageNum);
        Db.NonQ(command);
    }

    public static bool Update(EhrLabImage ehrLabImage, EhrLabImage oldEhrLabImage)
    {
        var command = "";
        if (ehrLabImage.EhrLabNum != oldEhrLabImage.EhrLabNum)
        {
            if (command != "") command += ",";
            command += "EhrLabNum = " + SOut.Long(ehrLabImage.EhrLabNum) + "";
        }

        if (ehrLabImage.DocNum != oldEhrLabImage.DocNum)
        {
            if (command != "") command += ",";
            command += "DocNum = " + SOut.Long(ehrLabImage.DocNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE ehrlabimage SET " + command
                                            + " WHERE EhrLabImageNum = " + SOut.Long(ehrLabImage.EhrLabImageNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EhrLabImage ehrLabImage, EhrLabImage oldEhrLabImage)
    {
        if (ehrLabImage.EhrLabNum != oldEhrLabImage.EhrLabNum) return true;
        if (ehrLabImage.DocNum != oldEhrLabImage.DocNum) return true;
        return false;
    }

    public static void Delete(long ehrLabImageNum)
    {
        var command = "DELETE FROM ehrlabimage "
                      + "WHERE EhrLabImageNum = " + SOut.Long(ehrLabImageNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrLabImageNums)
    {
        if (listEhrLabImageNums == null || listEhrLabImageNums.Count == 0) return;
        var command = "DELETE FROM ehrlabimage "
                      + "WHERE EhrLabImageNum IN(" + string.Join(",", listEhrLabImageNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}