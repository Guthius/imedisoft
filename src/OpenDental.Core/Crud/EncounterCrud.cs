using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class EncounterCrud
{
    public static void Insert(Encounter encounter)
    {
        var command = "INSERT INTO encounter (";

        command += "PatNum,ProvNum,CodeValue,CodeSystem,Note,DateEncounter) VALUES(";

        command +=
            SOut.Long(encounter.PatNum) + ","
                                        + SOut.Long(encounter.ProvNum) + ","
                                        + "'" + SOut.String(encounter.CodeValue) + "',"
                                        + "'" + SOut.String(encounter.CodeSystem) + "',"
                                        + DbHelper.ParamChar + "paramNote,"
                                        + SOut.Date(encounter.DateEncounter) + ")";
        if (encounter.Note == null) encounter.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(encounter.Note));
        {
            encounter.EncounterNum = Db.NonQ(command, true, "EncounterNum", "encounter", paramNote);
        }
    }
}