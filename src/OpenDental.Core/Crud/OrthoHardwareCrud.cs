using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class OrthoHardwareCrud
{
    public static List<OrthoHardware> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OrthoHardware> TableToList(DataTable table)
    {
        var retVal = new List<OrthoHardware>();
        foreach (DataRow row in table.Rows)
        {
            var orthoHardware = new OrthoHardware
            {
                OrthoHardwareNum = SIn.Long(row["OrthoHardwareNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                DateExam = SIn.Date(row["DateExam"].ToString()),
                OrthoHardwareType = (EnumOrthoHardwareType) SIn.Int(row["OrthoHardwareType"].ToString()),
                OrthoHardwareSpecNum = SIn.Long(row["OrthoHardwareSpecNum"].ToString()),
                ToothRange = SIn.String(row["ToothRange"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                IsHidden = SIn.Bool(row["IsHidden"].ToString())
            };
            retVal.Add(orthoHardware);
        }

        return retVal;
    }

    public static void Insert(OrthoHardware orthoHardware)
    {
        var command = "INSERT INTO orthohardware (";

        command += "PatNum,DateExam,OrthoHardwareType,OrthoHardwareSpecNum,ToothRange,Note,IsHidden) VALUES(";

        command +=
            SOut.Long(orthoHardware.PatNum) + ","
                                            + SOut.Date(orthoHardware.DateExam) + ","
                                            + SOut.Int((int) orthoHardware.OrthoHardwareType) + ","
                                            + SOut.Long(orthoHardware.OrthoHardwareSpecNum) + ","
                                            + "'" + SOut.String(orthoHardware.ToothRange) + "',"
                                            + "'" + SOut.String(orthoHardware.Note) + "',"
                                            + SOut.Bool(orthoHardware.IsHidden) + ")";
        {
            orthoHardware.OrthoHardwareNum = Db.NonQ(command, true, "OrthoHardwareNum", "orthoHardware");
        }
    }

    public static void Update(OrthoHardware orthoHardware)
    {
        var command = "UPDATE orthohardware SET "
                      + "PatNum              =  " + SOut.Long(orthoHardware.PatNum) + ", "
                      + "DateExam            =  " + SOut.Date(orthoHardware.DateExam) + ", "
                      + "OrthoHardwareType   =  " + SOut.Int((int) orthoHardware.OrthoHardwareType) + ", "
                      + "OrthoHardwareSpecNum=  " + SOut.Long(orthoHardware.OrthoHardwareSpecNum) + ", "
                      + "ToothRange          = '" + SOut.String(orthoHardware.ToothRange) + "', "
                      + "Note                = '" + SOut.String(orthoHardware.Note) + "', "
                      + "IsHidden            =  " + SOut.Bool(orthoHardware.IsHidden) + " "
                      + "WHERE OrthoHardwareNum = " + SOut.Long(orthoHardware.OrthoHardwareNum);
        Db.NonQ(command);
    }

    public static void Delete(long orthoHardwareNum)
    {
        var command = "DELETE FROM orthohardware "
                      + "WHERE OrthoHardwareNum = " + SOut.Long(orthoHardwareNum);
        Db.NonQ(command);
    }
}