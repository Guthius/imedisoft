using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class VaccineObsCrud
{
    public static List<VaccineObs> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<VaccineObs> TableToList(DataTable table)
    {
        var retVal = new List<VaccineObs>();
        VaccineObs vaccineObs;
        foreach (DataRow row in table.Rows)
        {
            vaccineObs = new VaccineObs();
            vaccineObs.VaccineObsNum = SIn.Long(row["VaccineObsNum"].ToString());
            vaccineObs.VaccinePatNum = SIn.Long(row["VaccinePatNum"].ToString());
            vaccineObs.ValType = (VaccineObsType) SIn.Int(row["ValType"].ToString());
            vaccineObs.IdentifyingCode = (VaccineObsIdentifier) SIn.Int(row["IdentifyingCode"].ToString());
            vaccineObs.ValReported = SIn.String(row["ValReported"].ToString());
            vaccineObs.ValCodeSystem = (VaccineObsValCodeSystem) SIn.Int(row["ValCodeSystem"].ToString());
            vaccineObs.VaccineObsNumGroup = SIn.Long(row["VaccineObsNumGroup"].ToString());
            vaccineObs.UcumCode = SIn.String(row["UcumCode"].ToString());
            vaccineObs.DateObs = SIn.Date(row["DateObs"].ToString());
            vaccineObs.MethodCode = SIn.String(row["MethodCode"].ToString());
            retVal.Add(vaccineObs);
        }

        return retVal;
    }

    public static void Insert(VaccineObs vaccineObs)
    {
        var command = "INSERT INTO vaccineobs (";

        command += "VaccinePatNum,ValType,IdentifyingCode,ValReported,ValCodeSystem,VaccineObsNumGroup,UcumCode,DateObs,MethodCode) VALUES(";

        command +=
            SOut.Long(vaccineObs.VaccinePatNum) + ","
                                                + SOut.Int((int) vaccineObs.ValType) + ","
                                                + SOut.Int((int) vaccineObs.IdentifyingCode) + ","
                                                + "'" + SOut.String(vaccineObs.ValReported) + "',"
                                                + SOut.Int((int) vaccineObs.ValCodeSystem) + ","
                                                + SOut.Long(vaccineObs.VaccineObsNumGroup) + ","
                                                + "'" + SOut.String(vaccineObs.UcumCode) + "',"
                                                + SOut.Date(vaccineObs.DateObs) + ","
                                                + "'" + SOut.String(vaccineObs.MethodCode) + "')";
        {
            vaccineObs.VaccineObsNum = Db.NonQ(command, true, "VaccineObsNum", "vaccineObs");
        }
    }

    public static void Update(VaccineObs vaccineObs)
    {
        var command = "UPDATE vaccineobs SET "
                      + "VaccinePatNum     =  " + SOut.Long(vaccineObs.VaccinePatNum) + ", "
                      + "ValType           =  " + SOut.Int((int) vaccineObs.ValType) + ", "
                      + "IdentifyingCode   =  " + SOut.Int((int) vaccineObs.IdentifyingCode) + ", "
                      + "ValReported       = '" + SOut.String(vaccineObs.ValReported) + "', "
                      + "ValCodeSystem     =  " + SOut.Int((int) vaccineObs.ValCodeSystem) + ", "
                      + "VaccineObsNumGroup=  " + SOut.Long(vaccineObs.VaccineObsNumGroup) + ", "
                      + "UcumCode          = '" + SOut.String(vaccineObs.UcumCode) + "', "
                      + "DateObs           =  " + SOut.Date(vaccineObs.DateObs) + ", "
                      + "MethodCode        = '" + SOut.String(vaccineObs.MethodCode) + "' "
                      + "WHERE VaccineObsNum = " + SOut.Long(vaccineObs.VaccineObsNum);
        Db.NonQ(command);
    }
}