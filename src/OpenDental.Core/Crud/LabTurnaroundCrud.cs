using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class LabTurnaroundCrud
{
    public static void Insert(LabTurnaround labTurnaround)
    {
        var command = "INSERT INTO labturnaround (";

        command += "LaboratoryNum,Description,DaysPublished,DaysActual) VALUES(";

        command +=
            SOut.Long(labTurnaround.LaboratoryNum) + ","
                                                   + "'" + SOut.String(labTurnaround.Description) + "',"
                                                   + SOut.Int(labTurnaround.DaysPublished) + ","
                                                   + SOut.Int(labTurnaround.DaysActual) + ")";
        {
            labTurnaround.LabTurnaroundNum = Db.NonQ(command, true, "LabTurnaroundNum", "labTurnaround");
        }
    }
}