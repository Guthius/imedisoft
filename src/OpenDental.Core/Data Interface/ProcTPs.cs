using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ProcTPs
{
    #region Update

    ///<summary>Sets the priority for the procedures passed in that are associated to the designated treatment plan.</summary>
    public static void SetPriorityForTreatPlanProcs(long priority, long treatPlanNum, List<long> listProcNums)
    {
        if (listProcNums.IsNullOrEmpty()) return;

        Db.NonQ($@"UPDATE proctp SET Priority = {SOut.Long(priority)}
				WHERE TreatPlanNum = {SOut.Long(treatPlanNum)}
				AND ProcNumOrig IN({string.Join(",", listProcNums.Select(x => SOut.Long(x)))})");
    }

    #endregion

    ///<summary>Gets all ProcTPs for a given Patient ordered by ItemOrder.</summary>
    public static List<ProcTP> Refresh(long patNum)
    {
        var command = "SELECT * FROM proctp "
                      + "WHERE PatNum=" + SOut.Long(patNum)
                      + " ORDER BY ItemOrder";
        return ProcTPCrud.SelectMany(command);
    }

    ///<summary>Ordered by ItemOrder.</summary>
    public static List<ProcTP> RefreshForTP(long tpNum)
    {
        var command = "SELECT * FROM proctp "
                      + "WHERE TreatPlanNum=" + SOut.Long(tpNum)
                      + " ORDER BY ItemOrder";
        var table = DataCore.GetTable(command);
        return ProcTPCrud.SelectMany(command);
    }

    
    public static void Update(ProcTP proc)
    {
        ProcTPCrud.Update(proc);
    }

    
    public static long Insert(ProcTP proc)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        proc.SecUserNumEntry = Security.CurUser.UserNum;
        return ProcTPCrud.Insert(proc);
    }

    
    public static void InsertOrUpdate(ProcTP proc, bool isNew)
    {
        if (isNew)
            Insert(proc);
        else
            Update(proc);
    }

    ///<summary>There are no dependencies.</summary>
    public static void Delete(ProcTP proc)
    {
        var command = "DELETE from proctp WHERE ProcTPNum = '" + SOut.Long(proc.ProcTPNum) + "'";
        Db.NonQ(command);
    }

    ///<summary>No dependencies to worry about.</summary>
    public static void DeleteForTP(long treatPlanNum)
    {
        var command = "DELETE FROM proctp "
                      + "WHERE TreatPlanNum=" + SOut.Long(treatPlanNum);
        Db.NonQ(command);
    }

    public static List<ProcTP> GetForProcs(List<long> listProcNums)
    {
        if (listProcNums.Count == 0) return new List<ProcTP>();
        var command = "SELECT * FROM proctp "
                      + "WHERE proctp.ProcNumOrig IN (" + string.Join(",", listProcNums) + ")";
        return ProcTPCrud.SelectMany(command);
    }

    ///<summary>Returns only three columns from all ProcTPs -- TreatPlanNum, PatNum, and ProcNumOrig.</summary>
    public static List<ProcTP> GetAllLim(List<long> listTreatPlanNums)
    {
        if (listTreatPlanNums.IsNullOrEmpty()) //No need to go through middletier if we know listTreatPlanNums is empty. Return early.
            return new List<ProcTP>();

        var command = "SELECT TreatPlanNum,PatNum,ProcNumOrig FROM proctp "
                      + "WHERE proctp.TreatPlanNum IN (" + string.Join(",", listTreatPlanNums) + ")";
        var table = DataCore.GetTable(command);
        var listProcTpsLim = new List<ProcTP>();
        foreach (DataRow row in table.Rows)
        {
            var procTp = new ProcTP();
            procTp.TreatPlanNum = SIn.Long(row["TreatPlanNum"].ToString());
            procTp.PatNum = SIn.Long(row["PatNum"].ToString());
            procTp.ProcNumOrig = SIn.Long(row["ProcNumOrig"].ToString());
            listProcTpsLim.Add(procTp);
        }

        return listProcTpsLim;
    }

    public static List<ProcTP> GetProcTPsFromTpRows(long patNum, List<TpRow> listTpRows, List<Procedure> listProcedures, List<TreatPlanAttach> listTreatPlanAttaches)
    {
        var listProcTPs = new List<ProcTP>();
        for (var i = 0; i < listTpRows.Count; i++)
        {
            var procedure = listProcedures[i].Copy();
            //procList.Add(proc);
            var procTP = new ProcTP();
            //procTP.TreatPlanNum=tp.TreatPlanNum;
            procTP.PatNum = patNum;
            procTP.ProcNumOrig = procedure.ProcNum;
            procTP.ItemOrder = i;
            procTP.Priority = listTreatPlanAttaches.FirstOrDefault(x => x.ProcNum == procedure.ProcNum).Priority; //proc.Priority;
            procTP.ToothNumTP = Tooth.Display(procedure.ToothNum);
            if (ProcedureCodes.GetProcCode(procedure.CodeNum).TreatArea == TreatmentArea.Surf)
                procTP.Surf = Tooth.SurfTidyFromDbToDisplay(procedure.Surf, procedure.ToothNum);
            else
                procTP.Surf = procedure.Surf; //for UR, L, etc.
            procTP.ProcCode = ProcedureCodes.GetStringProcCode(procedure.CodeNum);
            procTP.Descript = listTpRows[i].Description;
            procTP.FeeAmt = SIn.Double(listTpRows[i].Fee.ToString());
            procTP.PriInsAmt = SIn.Double(listTpRows[i].PriIns.ToString());
            procTP.SecInsAmt = SIn.Double(listTpRows[i].SecIns.ToString());
            procTP.Discount = SIn.Double(listTpRows[i].Discount.ToString());
            procTP.PatAmt = SIn.Double(listTpRows[i].Pat.ToString());
            procTP.Prognosis = listTpRows[i].Prognosis;
            procTP.Dx = listTpRows[i].Dx;
            procTP.ProcAbbr = listTpRows[i].ProcAbbr;
            procTP.FeeAllowed = SIn.Double(listTpRows[i].FeeAllowed.ToString());
            procTP.TaxAmt = SIn.Double(listTpRows[i].TaxEst.ToString());
            procTP.ProvNum = listTpRows[i].ProvNum;
            procTP.DateTP = SIn.Date(listTpRows[i].DateTP.ToString());
            procTP.ClinicNum = listTpRows[i].ClinicNum;
            procTP.CatPercUCR = (double) listTpRows[i].CatPercUCR;
            procTP.TagOD = procedure.ProcNumLab; //Used for selection logic. See ControlTreat.gridMain_CellClick(...).
            listProcTPs.Add(procTP);
            listTpRows[i].Tag = procTP;
        }

        return listProcTPs;
    }

    ///<summary>Gets one ProcTP object from the database using the primary key. Returns null if not found.</summary>
    public static ProcTP GetOneyByProcTPNum(long procTPNum)
    {
        return ProcTPCrud.SelectOne(procTPNum);
    }
}