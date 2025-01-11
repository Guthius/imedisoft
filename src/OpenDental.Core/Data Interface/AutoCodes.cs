using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class AutoCodes
{
    public static long Insert(AutoCode autoCode)
    {
        return AutoCodeCrud.Insert(autoCode);
    }
    
    public static void Update(AutoCode autoCode)
    {
        AutoCodeCrud.Update(autoCode);
    }

    public static void Delete(AutoCode autoCode)
    {
        //look for dependencies in ProcButton table.
        var strInUse = "";
        var listProcButtons = ProcButtons.GetDeepCopy();
        var listProcButtonItems = ProcButtonItems.GetDeepCopy();
        for (var i = 0; i < listProcButtons.Count; i++)
        for (var j = 0; j < listProcButtonItems.Count; j++)
            if (listProcButtonItems[j].ProcButtonNum == listProcButtons[i].ProcButtonNum
                && listProcButtonItems[j].AutoCodeNum == autoCode.AutoCodeNum)
            {
                if (strInUse != "") strInUse += "; ";

                //Add the procedure button description to the list for display.
                strInUse += listProcButtons[i].Description;
                break; //Button already added to the description, check the other buttons in the list.
            }

        if (strInUse != "") throw new ApplicationException(Lans.g("AutoCodes", "Not allowed to delete autocode because it is in use.  Procedure buttons using this autocode include ") + strInUse);
        var listAutoCodeItems = AutoCodeItems.GetListForCode(autoCode.AutoCodeNum);
        for (var i = 0; i < listAutoCodeItems.Count; i++)
        {
            var AutoCodeItem = listAutoCodeItems[i];
            AutoCodeConds.DeleteForItemNum(AutoCodeItem.AutoCodeItemNum);
            AutoCodeItems.Delete(AutoCodeItem);
        }

        AutoCodeCrud.Delete(autoCode.AutoCodeNum);
    }

    public static long GetNumFromDescript(string descript)
    {
        var autoCode = Cache.GetFirstOrDefault(x => x.Description == descript, true);
        long autoCodeNum = 0;
        if (autoCode != null) autoCodeNum = autoCode.AutoCodeNum;
        return autoCodeNum;
    }

    public static void ApplyAutoCodeToProcedure(Procedure procedure, long verifyCodeNum, List<PatPlan> listPatPlans, List<InsSub> listInsSubs, List<InsPlan> listInsPlan, Patient patient, List<ClaimProc> listClaimProcs, List<Benefit> listBenefits, ProcedureCode procedureCode, string strTeeth)
    {
        var procedureOld = procedure.Copy();
        procedure.CodeNum = verifyCodeNum;
        var listProcStats = new List<ProcStat>();
        listProcStats.Add(ProcStat.TP);
        listProcStats.Add(ProcStat.C);
        listProcStats.Add(ProcStat.TPi);
        listProcStats.Add(ProcStat.Cn);
        if (listProcStats.Contains(procedure.ProcStatus))
        {
            //Only change the fee if Complete, TP, TPi, or Cn.
            InsSub insSub = null;
            InsPlan insPlan = null;
            if (listPatPlans.Count > 0)
            {
                insSub = InsSubs.GetSub(listPatPlans[0].InsSubNum, listInsSubs);
                insPlan = InsPlans.GetPlan(insSub.PlanNum, listInsPlan);
            }

            procedure.ProcFee = Fees.GetAmount0(procedure.CodeNum, FeeScheds.GetFeeSched(patient, listInsPlan, listPatPlans, listInsSubs, procedure.ProvNum),
                procedure.ClinicNum, procedure.ProvNum);
            if (insPlan != null && insPlan.PlanType == "p")
            {
                //PPO
                var standardFee = Fees.GetAmount0(procedure.CodeNum, Providers.GetProv(Patients.GetProvNum(patient)).FeeSched, procedure.ClinicNum,
                    procedure.ProvNum);
                procedure.ProcFee = Math.Max(procedure.ProcFee, standardFee);
            }
        }

        Procedures.Update(procedure, procedureOld);
        //Compute estimates required, otherwise if adding through quick add, it could have incorrect WO or InsEst if code changed.
        Procedures.ComputeEstimates(procedure, patient.PatNum, listClaimProcs, true, listInsPlan, listPatPlans, listBenefits, patient.Age, listInsSubs);
        Recalls.Synch(procedure.PatNum);
        if (!procedure.ProcStatus.In(ProcStat.C, ProcStat.EO, ProcStat.EC)) return;
        var strLogText = procedureCode.ProcCode + " (" + procedure.ProcStatus + "), ";
        if (strTeeth != null && strTeeth.Trim() != "") strLogText += Lans.g("FrmAutoCodeLessIntrusive", "Teeth") + ": " + strTeeth + ", ";
        strLogText += Lans.g("FrmAutoCodeLessIntrusive", "Fee") + ": " + procedure.ProcFee.ToString("F") + ", " + procedureCode.Descript;
        if (procedure.ProcStatus.In(ProcStat.EO, ProcStat.EC)) SecurityLogs.MakeLogEntry(EnumPermType.ProcExistingEdit, patient.PatNum, strLogText);
    }
    
    public static void SetToDefault()
    {
        var command = "DELETE FROM autocode";
        Db.NonQ(command);
        command = "DELETE FROM autocodecond";
        Db.NonQ(command);
        command = "DELETE FROM autocodeitem";
        Db.NonQ(command);
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            //Canadian. en-CA or fr-CA
            SetToDefaultCanada();
            return;
        }

        SetToDefaultMySQL();
    }

    private static void SetToDefaultMySQL()
    {
        long autoCodeNum;
        long autoCodeItemNum;
        //Amalgam-------------------------------------------------------------------------------------------------------
        var command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('Amalgam',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        //1Surf
        if (ProcedureCodes.IsValidCode("D2140"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D2140") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
        }

        //2Surf
        if (ProcedureCodes.IsValidCode("D2150"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D2150") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
        }

        //3Surf
        if (ProcedureCodes.IsValidCode("D2160"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D2160") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
        }

        //4Surf
        if (ProcedureCodes.IsValidCode("D2161"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D2161") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
        }

        //5Surf
        if (ProcedureCodes.IsValidCode("D2161"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D2161") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
        }

        //Composite-------------------------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('Composite',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        //1SurfAnt
        if (ProcedureCodes.IsValidCode("D2330"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D2330") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
        }

        //2SurfAnt
        if (ProcedureCodes.IsValidCode("D2331"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D2331") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
        }

        //3SurfAnt
        if (ProcedureCodes.IsValidCode("D2332"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D2332") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
        }

        //4SurfAnt
        if (ProcedureCodes.IsValidCode("D2335"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D2335") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
        }

        //5SurfAnt
        if (ProcedureCodes.IsValidCode("D2335"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D2335") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
        }

        //Posterior Composite----------------------------------------------------------------------------------------------
        //1SurfPost
        if (ProcedureCodes.IsValidCode("D2391"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D2391") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Posterior) + ")";
            Db.NonQ(command);
        }

        //2SurfPost
        if (ProcedureCodes.IsValidCode("D2392"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D2392") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Posterior) + ")";
            Db.NonQ(command);
        }

        //3SurfPost
        if (ProcedureCodes.IsValidCode("D2393"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D2393") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Posterior) + ")";
            Db.NonQ(command);
        }

        //4SurfPost
        if (ProcedureCodes.IsValidCode("D2394"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D2394") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Posterior) + ")";
            Db.NonQ(command);
        }

        //5SurfPost
        if (ProcedureCodes.IsValidCode("D2394"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D2394") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Posterior) + ")";
            Db.NonQ(command);
        }

        //Root Canal-------------------------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('Root Canal',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        //Ant
        if (ProcedureCodes.IsValidCode("D3310"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D3310") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
        }

        //Premolar
        if (ProcedureCodes.IsValidCode("D3320"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D3320") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
        }

        //Molar
        if (ProcedureCodes.IsValidCode("D3330"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D3330") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
        }

        //PFM Bridge-------------------------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('PFM Bridge',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        //Pontic
        if (ProcedureCodes.IsValidCode("D6242"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D6242") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Pontic) + ")";
            Db.NonQ(command);
        }

        //Retainer
        if (ProcedureCodes.IsValidCode("D6752"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D6752") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Retainer) + ")";
            Db.NonQ(command);
        }

        //Ceramic Bridge-------------------------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('Ceramic Bridge',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        //Pontic
        if (ProcedureCodes.IsValidCode("D6245"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D6245") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Pontic) + ")";
            Db.NonQ(command);
        }

        //Retainer
        if (ProcedureCodes.IsValidCode("D6740"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D6740") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Retainer) + ")";
            Db.NonQ(command);
        }

        //Denture-------------------------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('Denture',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        //Max
        if (ProcedureCodes.IsValidCode("D5110"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D5110") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Maxillary) + ")";
            Db.NonQ(command);
        }

        //Mand
        if (ProcedureCodes.IsValidCode("D5120"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D5120") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Mandibular) + ")";
            Db.NonQ(command);
        }

        //Immediate Denture--------------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('Immediate Denture',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        //Max
        if (ProcedureCodes.IsValidCode("D5130"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D5130") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond(AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Maxillary) + ")";
            Db.NonQ(command);
        }

        //Mand
        if (ProcedureCodes.IsValidCode("D5140"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D5140") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Mandibular) + ")";
            Db.NonQ(command);
        }

        //Resin Based Partial-------------------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('Resin Base Partial',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        //Max
        if (ProcedureCodes.IsValidCode("D5211"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D5211") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Maxillary) + ")";
            Db.NonQ(command);
        }

        //Mand
        if (ProcedureCodes.IsValidCode("D5212"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D5212") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Mandibular) + ")";
            Db.NonQ(command);
        }

        //Cast Metal Partial--------------------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('Cast Metal Partial',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        //Max
        if (ProcedureCodes.IsValidCode("D5213"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D5213") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Maxillary) + ")";
            Db.NonQ(command);
        }

        //Mand
        if (ProcedureCodes.IsValidCode("D5214"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D5214") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Mandibular) + ")";
            Db.NonQ(command);
        }

        //Flex Base Partial--------------------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('Flex Base Partial',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        //Max
        if (ProcedureCodes.IsValidCode("D5225"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D5225") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Maxillary) + ")";
            Db.NonQ(command);
        }

        //Mand
        if (ProcedureCodes.IsValidCode("D5226"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D5226") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Mandibular) + ")";
            Db.NonQ(command);
        }

        //BU/P&C-------------------------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('BU/P&C',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        //BU
        if (ProcedureCodes.IsValidCode("D2950"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D2950") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Posterior) + ")";
            Db.NonQ(command);
        }

        //P&C
        if (ProcedureCodes.IsValidCode("D2954"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D2954") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
        }

        //Root Canal Retreat--------------------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('RCT Retreat',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        //Ant
        if (ProcedureCodes.IsValidCode("D3346"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D3346") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
        }

        //Premolar
        if (ProcedureCodes.IsValidCode("D3347"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D3347") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
        }

        //Molar
        if (ProcedureCodes.IsValidCode("D3348"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("D3348") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
        }
    }

    public static void SetToDefaultCanada()
    {
        string command;
        long autoCodeNum;
        long autoCodeItemNum;
        //Amalgam-Bonded--------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('Amalgam-Bonded',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        //1SurfPrimaryMolar
        if (ProcedureCodes.IsValidCode("21121"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21121") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //1SurfPrimaryAnterior
        if (ProcedureCodes.IsValidCode("21121"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21121") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //2SurfPrimaryAnterior
        if (ProcedureCodes.IsValidCode("21122"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21122") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //2SurfPrimaryMolar
        if (ProcedureCodes.IsValidCode("21122"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21122") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //3SurfPrimaryAnterior
        if (ProcedureCodes.IsValidCode("21123"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21123") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //3SurfPrimaryMolar
        if (ProcedureCodes.IsValidCode("21123"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21123") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //4SurfPrimaryAnterior
        if (ProcedureCodes.IsValidCode("21124"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21124") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //4SurfPrimaryMolar
        if (ProcedureCodes.IsValidCode("21124"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21124") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //5SurfPrimaryAnterior
        if (ProcedureCodes.IsValidCode("21125"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21125") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //5SurfPrimaryMolar
        if (ProcedureCodes.IsValidCode("21125"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21125") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //1SurfPermanentAnterior
        if (ProcedureCodes.IsValidCode("21231"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21231") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //1SurfPermanentPremolar
        if (ProcedureCodes.IsValidCode("21231"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21231") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //2SurfPermanentAnterior
        if (ProcedureCodes.IsValidCode("21232"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21232") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //2SurfPermanentPremolar
        if (ProcedureCodes.IsValidCode("21232"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21232") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //3SurfPermanentAnterior
        if (ProcedureCodes.IsValidCode("21233"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21233") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //3SurfPermanentPremolar
        if (ProcedureCodes.IsValidCode("21233"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21233") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //4SurfPermanentAnterior
        if (ProcedureCodes.IsValidCode("21234"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21234") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //4SurfPermanentPremolar
        if (ProcedureCodes.IsValidCode("21234"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21234") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //5SurfPermanentAnterior
        if (ProcedureCodes.IsValidCode("21235"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21235") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //5SurfPermanentPremolar
        if (ProcedureCodes.IsValidCode("21235"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21235") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //1SurfPermanentMolar
        if (ProcedureCodes.IsValidCode("21241"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21241") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //2SurfPermanentMolar
        if (ProcedureCodes.IsValidCode("21242"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21242") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //3SurfPermanentMolar
        if (ProcedureCodes.IsValidCode("21243"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21243") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //4SurfPermanentMolar
        if (ProcedureCodes.IsValidCode("21244"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21244") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //5SurfPermanentMolar
        if (ProcedureCodes.IsValidCode("21245"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21245") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //Amalgam Non-Bonded----------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('Amalgam Non-Bonded',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        //1SurfPrimaryAnterior
        if (ProcedureCodes.IsValidCode("21111"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21111") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //1SurfPrimaryMolar
        if (ProcedureCodes.IsValidCode("21111"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21111") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //2SurfPrimaryAnterior
        if (ProcedureCodes.IsValidCode("21112"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21112") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //2SurfPrimaryMolar
        if (ProcedureCodes.IsValidCode("21112"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21112") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //3SurfPrimaryMolar
        if (ProcedureCodes.IsValidCode("21113"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21113") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //3SurfPrimaryAnterior
        if (ProcedureCodes.IsValidCode("21113"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21113") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //4SurfPrimaryAnterior
        if (ProcedureCodes.IsValidCode("21114"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21114") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //4SurfPrimaryMolar
        if (ProcedureCodes.IsValidCode("21114"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21114") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //5SurfPrimaryAnterior
        if (ProcedureCodes.IsValidCode("21115"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21115") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //5SurfPrimaryMolar
        if (ProcedureCodes.IsValidCode("21115"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21115") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //1SurfPermanentAnterior
        if (ProcedureCodes.IsValidCode("21211"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21211") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //1SurfPermanentPremolar
        if (ProcedureCodes.IsValidCode("21211"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21211") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //2SurfPermanentAnterior
        if (ProcedureCodes.IsValidCode("21212"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21212") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //2SurfPermanentPremolar
        if (ProcedureCodes.IsValidCode("21212"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21212") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //3SurfPermanentAnterior
        if (ProcedureCodes.IsValidCode("21213"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21213") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //3SurfPermanentPremolar
        if (ProcedureCodes.IsValidCode("21213"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21213") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //4SurfPermanentAnterior
        if (ProcedureCodes.IsValidCode("21214"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21214") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //4SurfPermanentPremolar
        if (ProcedureCodes.IsValidCode("21214"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21214") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //5SurfPermanentAnterior
        if (ProcedureCodes.IsValidCode("21215"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21215") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //5SurfPermanentPremolar
        if (ProcedureCodes.IsValidCode("21215"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21215") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //1SurfPermanentMolar
        if (ProcedureCodes.IsValidCode("21221"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21221") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //2SurfPermanentMolar
        if (ProcedureCodes.IsValidCode("21222"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21222") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //3SurfPermanentMolar
        if (ProcedureCodes.IsValidCode("21223"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21223") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //4SurfPermanentMolar
        if (ProcedureCodes.IsValidCode("21224"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21224") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //5SurfPermanentMolar
        if (ProcedureCodes.IsValidCode("21225"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("21225") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //Composite-------------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('Composite',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        //1SurfPermanentAnterior
        if (ProcedureCodes.IsValidCode("23111"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23111") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //2SurfPermanentAnterior
        if (ProcedureCodes.IsValidCode("23112"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23112") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //3SurfPermanentAnterior
        if (ProcedureCodes.IsValidCode("23113"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23113") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //4SurfPermanentAnterior
        if (ProcedureCodes.IsValidCode("23114"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23114") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //5SurfPermanentAnterior
        if (ProcedureCodes.IsValidCode("23115"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23115") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //1SurfPermanentPremolar
        if (ProcedureCodes.IsValidCode("23311"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23311") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //2SurfPermanentPremolar
        if (ProcedureCodes.IsValidCode("23312"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23312") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //3SurfPermanentPremolar
        if (ProcedureCodes.IsValidCode("23313"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23313") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //4SurfPermanentPremolar
        if (ProcedureCodes.IsValidCode("23314"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23314") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //5SurfPermanentPremolar
        if (ProcedureCodes.IsValidCode("23315"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23315") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //1SurfPermanentMolar
        if (ProcedureCodes.IsValidCode("23321"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23321") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //2SurfPermanentMolar
        if (ProcedureCodes.IsValidCode("23322"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23322") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //3SurfPermanentMolar
        if (ProcedureCodes.IsValidCode("23323"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23323") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //4SurfPermanentMolar
        if (ProcedureCodes.IsValidCode("23324"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23324") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //5SurfPermanentMolar
        if (ProcedureCodes.IsValidCode("23325"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23325") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Permanent) + ")";
            Db.NonQ(command);
        }

        //1SurfPrimaryAnterior
        if (ProcedureCodes.IsValidCode("23411"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23411") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //2SurfPrimaryAnterior
        if (ProcedureCodes.IsValidCode("23412"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23412") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //3SurfPrimaryAnterior
        if (ProcedureCodes.IsValidCode("23413"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23413") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //4SurfPrimaryAnterior
        if (ProcedureCodes.IsValidCode("23414"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23414") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //5SurfPrimaryAnterior
        if (ProcedureCodes.IsValidCode("23415"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23415") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //1SurfPrimaryMolar
        if (ProcedureCodes.IsValidCode("23511"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23511") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //2SurfPrimaryMolar
        if (ProcedureCodes.IsValidCode("23512"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23512") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //3SurfPrimaryMolar
        if (ProcedureCodes.IsValidCode("23513"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23513") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //4SurfPrimaryMolar
        if (ProcedureCodes.IsValidCode("23514"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23514") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //5SurfPrimaryMolar
        if (ProcedureCodes.IsValidCode("23515"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("23515") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Primary) + ")";
            Db.NonQ(command);
        }

        //Gold Inlay----------------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('Gold Inlay',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        if (ProcedureCodes.IsValidCode("25113"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("25113") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
        }

        if (ProcedureCodes.IsValidCode("25112"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("25112") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
        }

        if (ProcedureCodes.IsValidCode("25111"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("25111") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
        }

        if (ProcedureCodes.IsValidCode("25114"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("25114") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
        }

        if (ProcedureCodes.IsValidCode("25114"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("25114") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
        }

        //Open&Drain----------------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('Open&Drain',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        if (ProcedureCodes.IsValidCode("39201"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("39201") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
        }

        if (ProcedureCodes.IsValidCode("39201"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("39201") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
        }

        if (ProcedureCodes.IsValidCode("39202"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("39202") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
        }

        //OpenThruCrown-------------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('OpenThruCrown',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        if (ProcedureCodes.IsValidCode("39212"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("39212") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
        }

        if (ProcedureCodes.IsValidCode("39211"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("39211") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
        }

        if (ProcedureCodes.IsValidCode("39211"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("39211") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
        }

        //PFM Bridge----------------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('PFM Bridge',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        //Pontic
        if (ProcedureCodes.IsValidCode("62501"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("62501") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Pontic) + ")";
            Db.NonQ(command);
        }

        //Retainer
        if (ProcedureCodes.IsValidCode("67211"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("67211") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Retainer) + ")";
            Db.NonQ(command);
        }

        //Porcelain Inlay-----------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('Porcelain Inlay',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        if (ProcedureCodes.IsValidCode("25141"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("25141") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.One_Surf) + ")";
            Db.NonQ(command);
        }

        if (ProcedureCodes.IsValidCode("25142"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("25142") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Two_Surf) + ")";
            Db.NonQ(command);
        }

        if (ProcedureCodes.IsValidCode("25143"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("25143") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Three_Surf) + ")";
            Db.NonQ(command);
        }

        if (ProcedureCodes.IsValidCode("25144"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("25144") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Four_Surf) + ")";
            Db.NonQ(command);
        }

        if (ProcedureCodes.IsValidCode("25144"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("25144") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Five_Surf) + ")";
            Db.NonQ(command);
        }

        //RCTDifficult--------------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('RCTDifficult',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        if (ProcedureCodes.IsValidCode("33122"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("33122") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
        }

        if (ProcedureCodes.IsValidCode("33112"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("33112") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
        }

        if (ProcedureCodes.IsValidCode("33132"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("33112") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
        }

        //RCTSimple-------------------------------------------------------------------------------------------
        command = "INSERT INTO autocode (Description,IsHidden,LessIntrusive) VALUES ('RCTSimple',0,0)";
        autoCodeNum = Db.NonQ(command, true);
        //Anterior
        if (ProcedureCodes.IsValidCode("33111"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("33111") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Anterior) + ")";
            Db.NonQ(command);
        }

        //Premolar
        if (ProcedureCodes.IsValidCode("33121"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("33121") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Premolar) + ")";
            Db.NonQ(command);
        }

        //Molar
        if (ProcedureCodes.IsValidCode("33131"))
        {
            command = "INSERT INTO autocodeitem (AutoCodeNum,CodeNum) VALUES (" + SOut.Long(autoCodeNum) + ","
                      + ProcedureCodes.GetCodeNum("33131") + ")";
            autoCodeItemNum = Db.NonQ(command, true);
            command = "INSERT INTO autocodecond (AutoCodeItemNum,Cond) VALUES (" + SOut.Long(autoCodeItemNum) + ","
                      + SOut.Long((int) AutoCondition.Molar) + ")";
            Db.NonQ(command);
        }
    }
    
    private class AutoCodeCache : CacheDictAbs<AutoCode, long, AutoCode>
    {
        protected override List<AutoCode> GetCacheFromDb()
        {
            var command = "SELECT * from autocode";
            return AutoCodeCrud.SelectMany(command);
        }

        protected override List<AutoCode> TableToList(DataTable dataTable)
        {
            return AutoCodeCrud.TableToList(dataTable);
        }

        protected override AutoCode Copy(AutoCode item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(Dictionary<long, AutoCode> dict)
        {
            return AutoCodeCrud.ListToTable(dict.Values.ToList(), "AutoCode");
        }

        protected override void FillCacheIfNeeded()
        {
            AutoCodes.GetTableFromCache(false);
        }

        protected override bool IsInDictShort(AutoCode item)
        {
            return !item.IsHidden;
        }

        protected override long GetDictKey(AutoCode item)
        {
            return item.AutoCodeNum;
        }

        protected override AutoCode GetDictValue(AutoCode item)
        {
            return item;
        }

        protected override AutoCode CopyValue(AutoCode autoCode)
        {
            return autoCode.Copy();
        }
    }

    private static readonly AutoCodeCache Cache = new();

    public static List<AutoCode> GetListDeep(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort).Values.ToList();
    }

    public static AutoCode GetOne(long codeNum)
    {
        return Cache.GetOne(codeNum);
    }

    public static bool GetContainsKey(long codeNum)
    {
        return Cache.GetContainsKey(codeNum);
    }

    public static int GetCount(bool isShort = false)
    {
        return Cache.GetCount(isShort);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}