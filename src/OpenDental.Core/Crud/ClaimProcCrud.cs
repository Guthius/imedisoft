using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ClaimProcCrud
{
    public static ClaimProc SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ClaimProc> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ClaimProc> TableToList(DataTable table)
    {
        var retVal = new List<ClaimProc>();
        ClaimProc claimProc;
        foreach (DataRow row in table.Rows)
        {
            claimProc = new ClaimProc();
            claimProc.ClaimProcNum = SIn.Long(row["ClaimProcNum"].ToString());
            claimProc.ProcNum = SIn.Long(row["ProcNum"].ToString());
            claimProc.ClaimNum = SIn.Long(row["ClaimNum"].ToString());
            claimProc.PatNum = SIn.Long(row["PatNum"].ToString());
            claimProc.ProvNum = SIn.Long(row["ProvNum"].ToString());
            claimProc.FeeBilled = SIn.Double(row["FeeBilled"].ToString());
            claimProc.InsPayEst = SIn.Double(row["InsPayEst"].ToString());
            claimProc.DedApplied = SIn.Double(row["DedApplied"].ToString());
            claimProc.Status = (ClaimProcStatus) SIn.Int(row["Status"].ToString());
            claimProc.InsPayAmt = SIn.Double(row["InsPayAmt"].ToString());
            claimProc.Remarks = SIn.String(row["Remarks"].ToString());
            claimProc.ClaimPaymentNum = SIn.Long(row["ClaimPaymentNum"].ToString());
            claimProc.PlanNum = SIn.Long(row["PlanNum"].ToString());
            claimProc.DateCP = SIn.Date(row["DateCP"].ToString());
            claimProc.WriteOff = SIn.Double(row["WriteOff"].ToString());
            claimProc.CodeSent = SIn.String(row["CodeSent"].ToString());
            claimProc.AllowedOverride = SIn.Double(row["AllowedOverride"].ToString());
            claimProc.Percentage = SIn.Int(row["Percentage"].ToString());
            claimProc.PercentOverride = SIn.Int(row["PercentOverride"].ToString());
            claimProc.CopayAmt = SIn.Double(row["CopayAmt"].ToString());
            claimProc.NoBillIns = SIn.Bool(row["NoBillIns"].ToString());
            claimProc.PaidOtherIns = SIn.Double(row["PaidOtherIns"].ToString());
            claimProc.BaseEst = SIn.Double(row["BaseEst"].ToString());
            claimProc.CopayOverride = SIn.Double(row["CopayOverride"].ToString());
            claimProc.ProcDate = SIn.Date(row["ProcDate"].ToString());
            claimProc.DateEntry = SIn.Date(row["DateEntry"].ToString());
            claimProc.LineNumber = SIn.Byte(row["LineNumber"].ToString());
            claimProc.DedEst = SIn.Double(row["DedEst"].ToString());
            claimProc.DedEstOverride = SIn.Double(row["DedEstOverride"].ToString());
            claimProc.InsEstTotal = SIn.Double(row["InsEstTotal"].ToString());
            claimProc.InsEstTotalOverride = SIn.Double(row["InsEstTotalOverride"].ToString());
            claimProc.PaidOtherInsOverride = SIn.Double(row["PaidOtherInsOverride"].ToString());
            claimProc.EstimateNote = SIn.String(row["EstimateNote"].ToString());
            claimProc.WriteOffEst = SIn.Double(row["WriteOffEst"].ToString());
            claimProc.WriteOffEstOverride = SIn.Double(row["WriteOffEstOverride"].ToString());
            claimProc.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            claimProc.InsSubNum = SIn.Long(row["InsSubNum"].ToString());
            claimProc.PaymentRow = SIn.Int(row["PaymentRow"].ToString());
            claimProc.PayPlanNum = SIn.Long(row["PayPlanNum"].ToString());
            claimProc.ClaimPaymentTracking = SIn.Long(row["ClaimPaymentTracking"].ToString());
            claimProc.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            claimProc.SecDateEntry = SIn.Date(row["SecDateEntry"].ToString());
            claimProc.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            claimProc.DateSuppReceived = SIn.Date(row["DateSuppReceived"].ToString());
            claimProc.DateInsFinalized = SIn.Date(row["DateInsFinalized"].ToString());
            claimProc.IsTransfer = SIn.Bool(row["IsTransfer"].ToString());
            claimProc.ClaimAdjReasonCodes = SIn.String(row["ClaimAdjReasonCodes"].ToString());
            claimProc.IsOverpay = SIn.Bool(row["IsOverpay"].ToString());
            claimProc.SecurityHash = SIn.String(row["SecurityHash"].ToString());
            retVal.Add(claimProc);
        }

        return retVal;
    }

    public static ClaimProc RowToObj(IDataRecord row)
    {
        return new ClaimProc
        {
            ClaimProcNum = SIn.Long(row["ClaimProcNum"].ToString()),
            ProcNum = SIn.Long(row["ProcNum"].ToString()),
            ClaimNum = SIn.Long(row["ClaimNum"].ToString()),
            PatNum = SIn.Long(row["PatNum"].ToString()),
            ProvNum = SIn.Long(row["ProvNum"].ToString()),
            FeeBilled = SIn.Double(row["FeeBilled"].ToString()),
            InsPayEst = SIn.Double(row["InsPayEst"].ToString()),
            DedApplied = SIn.Double(row["DedApplied"].ToString()),
            Status = (ClaimProcStatus) SIn.Int(row["Status"].ToString()),
            InsPayAmt = SIn.Double(row["InsPayAmt"].ToString()),
            Remarks = SIn.String(row["Remarks"].ToString()),
            ClaimPaymentNum = SIn.Long(row["ClaimPaymentNum"].ToString()),
            PlanNum = SIn.Long(row["PlanNum"].ToString()),
            DateCP = SIn.Date(row["DateCP"].ToString()),
            WriteOff = SIn.Double(row["WriteOff"].ToString()),
            CodeSent = SIn.String(row["CodeSent"].ToString()),
            AllowedOverride = SIn.Double(row["AllowedOverride"].ToString()),
            Percentage = SIn.Int(row["Percentage"].ToString()),
            PercentOverride = SIn.Int(row["PercentOverride"].ToString()),
            CopayAmt = SIn.Double(row["CopayAmt"].ToString()),
            NoBillIns = SIn.Bool(row["NoBillIns"].ToString()),
            PaidOtherIns = SIn.Double(row["PaidOtherIns"].ToString()),
            BaseEst = SIn.Double(row["BaseEst"].ToString()),
            CopayOverride = SIn.Double(row["CopayOverride"].ToString()),
            ProcDate = SIn.Date(row["ProcDate"].ToString()),
            DateEntry = SIn.Date(row["DateEntry"].ToString()),
            LineNumber = SIn.Byte(row["LineNumber"].ToString()),
            DedEst = SIn.Double(row["DedEst"].ToString()),
            DedEstOverride = SIn.Double(row["DedEstOverride"].ToString()),
            InsEstTotal = SIn.Double(row["InsEstTotal"].ToString()),
            InsEstTotalOverride = SIn.Double(row["InsEstTotalOverride"].ToString()),
            PaidOtherInsOverride = SIn.Double(row["PaidOtherInsOverride"].ToString()),
            EstimateNote = SIn.String(row["EstimateNote"].ToString()),
            WriteOffEst = SIn.Double(row["WriteOffEst"].ToString()),
            WriteOffEstOverride = SIn.Double(row["WriteOffEstOverride"].ToString()),
            ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
            InsSubNum = SIn.Long(row["InsSubNum"].ToString()),
            PaymentRow = SIn.Int(row["PaymentRow"].ToString()),
            PayPlanNum = SIn.Long(row["PayPlanNum"].ToString()),
            ClaimPaymentTracking = SIn.Long(row["ClaimPaymentTracking"].ToString()),
            SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString()),
            SecDateEntry = SIn.Date(row["SecDateEntry"].ToString()),
            SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString()),
            DateSuppReceived = SIn.Date(row["DateSuppReceived"].ToString()),
            DateInsFinalized = SIn.Date(row["DateInsFinalized"].ToString()),
            IsTransfer = SIn.Bool(row["IsTransfer"].ToString()),
            ClaimAdjReasonCodes = SIn.String(row["ClaimAdjReasonCodes"].ToString()),
            IsOverpay = SIn.Bool(row["IsOverpay"].ToString()),
            SecurityHash = SIn.String(row["SecurityHash"].ToString())
        };
    }

    public static long Insert(ClaimProc claimProc)
    {
        var command = "INSERT INTO claimproc (";

        command += "ProcNum,ClaimNum,PatNum,ProvNum,FeeBilled,InsPayEst,DedApplied,Status,InsPayAmt,Remarks,ClaimPaymentNum,PlanNum,DateCP,WriteOff,CodeSent,AllowedOverride,Percentage,PercentOverride,CopayAmt,NoBillIns,PaidOtherIns,BaseEst,CopayOverride,ProcDate,DateEntry,LineNumber,DedEst,DedEstOverride,InsEstTotal,InsEstTotalOverride,PaidOtherInsOverride,EstimateNote,WriteOffEst,WriteOffEstOverride,ClinicNum,InsSubNum,PaymentRow,PayPlanNum,ClaimPaymentTracking,SecUserNumEntry,SecDateEntry,DateSuppReceived,DateInsFinalized,IsTransfer,ClaimAdjReasonCodes,IsOverpay,SecurityHash) VALUES(";

        command +=
            SOut.Long(claimProc.ProcNum) + ","
                                         + SOut.Long(claimProc.ClaimNum) + ","
                                         + SOut.Long(claimProc.PatNum) + ","
                                         + SOut.Long(claimProc.ProvNum) + ","
                                         + SOut.Double(claimProc.FeeBilled) + ","
                                         + SOut.Double(claimProc.InsPayEst) + ","
                                         + SOut.Double(claimProc.DedApplied) + ","
                                         + SOut.Int((int) claimProc.Status) + ","
                                         + SOut.Double(claimProc.InsPayAmt) + ","
                                         + "'" + SOut.String(claimProc.Remarks) + "',"
                                         + SOut.Long(claimProc.ClaimPaymentNum) + ","
                                         + SOut.Long(claimProc.PlanNum) + ","
                                         + SOut.Date(claimProc.DateCP) + ","
                                         + SOut.Double(claimProc.WriteOff) + ","
                                         + "'" + SOut.String(claimProc.CodeSent) + "',"
                                         + SOut.Double(claimProc.AllowedOverride) + ","
                                         + SOut.Int(claimProc.Percentage) + ","
                                         + SOut.Int(claimProc.PercentOverride) + ","
                                         + SOut.Double(claimProc.CopayAmt) + ","
                                         + SOut.Bool(claimProc.NoBillIns) + ","
                                         + SOut.Double(claimProc.PaidOtherIns) + ","
                                         + SOut.Double(claimProc.BaseEst) + ","
                                         + SOut.Double(claimProc.CopayOverride) + ","
                                         + SOut.Date(claimProc.ProcDate) + ","
                                         + SOut.Date(claimProc.DateEntry) + ","
                                         + SOut.Byte(claimProc.LineNumber) + ","
                                         + SOut.Double(claimProc.DedEst) + ","
                                         + SOut.Double(claimProc.DedEstOverride) + ","
                                         + SOut.Double(claimProc.InsEstTotal) + ","
                                         + SOut.Double(claimProc.InsEstTotalOverride) + ","
                                         + SOut.Double(claimProc.PaidOtherInsOverride) + ","
                                         + "'" + SOut.String(claimProc.EstimateNote) + "',"
                                         + SOut.Double(claimProc.WriteOffEst) + ","
                                         + SOut.Double(claimProc.WriteOffEstOverride) + ","
                                         + SOut.Long(claimProc.ClinicNum) + ","
                                         + SOut.Long(claimProc.InsSubNum) + ","
                                         + SOut.Int(claimProc.PaymentRow) + ","
                                         + SOut.Long(claimProc.PayPlanNum) + ","
                                         + SOut.Long(claimProc.ClaimPaymentTracking) + ","
                                         + SOut.Long(claimProc.SecUserNumEntry) + ","
                                         + DbHelper.Now() + ","
                                         //SecDateTEdit can only be set by MySQL
                                         + SOut.Date(claimProc.DateSuppReceived) + ","
                                         + SOut.Date(claimProc.DateInsFinalized) + ","
                                         + SOut.Bool(claimProc.IsTransfer) + ","
                                         + "'" + SOut.String(claimProc.ClaimAdjReasonCodes) + "',"
                                         + SOut.Bool(claimProc.IsOverpay) + ","
                                         + "'" + SOut.String(claimProc.SecurityHash) + "')";
        {
            claimProc.ClaimProcNum = Db.NonQ(command, true, "ClaimProcNum", "claimProc");
        }
        return claimProc.ClaimProcNum;
    }

    public static void Update(ClaimProc claimProc)
    {
        var command = "UPDATE claimproc SET "
                      + "ProcNum             =  " + SOut.Long(claimProc.ProcNum) + ", "
                      + "ClaimNum            =  " + SOut.Long(claimProc.ClaimNum) + ", "
                      + "PatNum              =  " + SOut.Long(claimProc.PatNum) + ", "
                      + "ProvNum             =  " + SOut.Long(claimProc.ProvNum) + ", "
                      + "FeeBilled           =  " + SOut.Double(claimProc.FeeBilled) + ", "
                      + "InsPayEst           =  " + SOut.Double(claimProc.InsPayEst) + ", "
                      + "DedApplied          =  " + SOut.Double(claimProc.DedApplied) + ", "
                      + "Status              =  " + SOut.Int((int) claimProc.Status) + ", "
                      + "InsPayAmt           =  " + SOut.Double(claimProc.InsPayAmt) + ", "
                      + "Remarks             = '" + SOut.String(claimProc.Remarks) + "', "
                      + "ClaimPaymentNum     =  " + SOut.Long(claimProc.ClaimPaymentNum) + ", "
                      + "PlanNum             =  " + SOut.Long(claimProc.PlanNum) + ", "
                      + "DateCP              =  " + SOut.Date(claimProc.DateCP) + ", "
                      + "WriteOff            =  " + SOut.Double(claimProc.WriteOff) + ", "
                      + "CodeSent            = '" + SOut.String(claimProc.CodeSent) + "', "
                      + "AllowedOverride     =  " + SOut.Double(claimProc.AllowedOverride) + ", "
                      + "Percentage          =  " + SOut.Int(claimProc.Percentage) + ", "
                      + "PercentOverride     =  " + SOut.Int(claimProc.PercentOverride) + ", "
                      + "CopayAmt            =  " + SOut.Double(claimProc.CopayAmt) + ", "
                      + "NoBillIns           =  " + SOut.Bool(claimProc.NoBillIns) + ", "
                      + "PaidOtherIns        =  " + SOut.Double(claimProc.PaidOtherIns) + ", "
                      + "BaseEst             =  " + SOut.Double(claimProc.BaseEst) + ", "
                      + "CopayOverride       =  " + SOut.Double(claimProc.CopayOverride) + ", "
                      + "ProcDate            =  " + SOut.Date(claimProc.ProcDate) + ", "
                      + "DateEntry           =  " + SOut.Date(claimProc.DateEntry) + ", "
                      + "LineNumber          =  " + SOut.Byte(claimProc.LineNumber) + ", "
                      + "DedEst              =  " + SOut.Double(claimProc.DedEst) + ", "
                      + "DedEstOverride      =  " + SOut.Double(claimProc.DedEstOverride) + ", "
                      + "InsEstTotal         =  " + SOut.Double(claimProc.InsEstTotal) + ", "
                      + "InsEstTotalOverride =  " + SOut.Double(claimProc.InsEstTotalOverride) + ", "
                      + "PaidOtherInsOverride=  " + SOut.Double(claimProc.PaidOtherInsOverride) + ", "
                      + "EstimateNote        = '" + SOut.String(claimProc.EstimateNote) + "', "
                      + "WriteOffEst         =  " + SOut.Double(claimProc.WriteOffEst) + ", "
                      + "WriteOffEstOverride =  " + SOut.Double(claimProc.WriteOffEstOverride) + ", "
                      + "ClinicNum           =  " + SOut.Long(claimProc.ClinicNum) + ", "
                      + "InsSubNum           =  " + SOut.Long(claimProc.InsSubNum) + ", "
                      + "PaymentRow          =  " + SOut.Int(claimProc.PaymentRow) + ", "
                      + "PayPlanNum          =  " + SOut.Long(claimProc.PayPlanNum) + ", "
                      + "ClaimPaymentTracking=  " + SOut.Long(claimProc.ClaimPaymentTracking) + ", "
                      //SecUserNumEntry excluded from update
                      //SecDateEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "DateSuppReceived    =  " + SOut.Date(claimProc.DateSuppReceived) + ", "
                      + "DateInsFinalized    =  " + SOut.Date(claimProc.DateInsFinalized) + ", "
                      + "IsTransfer          =  " + SOut.Bool(claimProc.IsTransfer) + ", "
                      + "ClaimAdjReasonCodes = '" + SOut.String(claimProc.ClaimAdjReasonCodes) + "', "
                      + "IsOverpay           =  " + SOut.Bool(claimProc.IsOverpay) + ", "
                      + "SecurityHash        = '" + SOut.String(claimProc.SecurityHash) + "' "
                      + "WHERE ClaimProcNum = " + SOut.Long(claimProc.ClaimProcNum);
        Db.NonQ(command);
    }

    public static void Update(ClaimProc claimProc, ClaimProc oldClaimProc)
    {
        var command = "";
        if (claimProc.ProcNum != oldClaimProc.ProcNum)
        {
            if (command != "") command += ",";
            command += "ProcNum = " + SOut.Long(claimProc.ProcNum) + "";
        }

        if (claimProc.ClaimNum != oldClaimProc.ClaimNum)
        {
            if (command != "") command += ",";
            command += "ClaimNum = " + SOut.Long(claimProc.ClaimNum) + "";
        }

        if (claimProc.PatNum != oldClaimProc.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(claimProc.PatNum) + "";
        }

        if (claimProc.ProvNum != oldClaimProc.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(claimProc.ProvNum) + "";
        }

        if (claimProc.FeeBilled != oldClaimProc.FeeBilled)
        {
            if (command != "") command += ",";
            command += "FeeBilled = " + SOut.Double(claimProc.FeeBilled) + "";
        }

        if (claimProc.InsPayEst != oldClaimProc.InsPayEst)
        {
            if (command != "") command += ",";
            command += "InsPayEst = " + SOut.Double(claimProc.InsPayEst) + "";
        }

        if (claimProc.DedApplied != oldClaimProc.DedApplied)
        {
            if (command != "") command += ",";
            command += "DedApplied = " + SOut.Double(claimProc.DedApplied) + "";
        }

        if (claimProc.Status != oldClaimProc.Status)
        {
            if (command != "") command += ",";
            command += "Status = " + SOut.Int((int) claimProc.Status) + "";
        }

        if (claimProc.InsPayAmt != oldClaimProc.InsPayAmt)
        {
            if (command != "") command += ",";
            command += "InsPayAmt = " + SOut.Double(claimProc.InsPayAmt) + "";
        }

        if (claimProc.Remarks != oldClaimProc.Remarks)
        {
            if (command != "") command += ",";
            command += "Remarks = '" + SOut.String(claimProc.Remarks) + "'";
        }

        if (claimProc.ClaimPaymentNum != oldClaimProc.ClaimPaymentNum)
        {
            if (command != "") command += ",";
            command += "ClaimPaymentNum = " + SOut.Long(claimProc.ClaimPaymentNum) + "";
        }

        if (claimProc.PlanNum != oldClaimProc.PlanNum)
        {
            if (command != "") command += ",";
            command += "PlanNum = " + SOut.Long(claimProc.PlanNum) + "";
        }

        if (claimProc.DateCP.Date != oldClaimProc.DateCP.Date)
        {
            if (command != "") command += ",";
            command += "DateCP = " + SOut.Date(claimProc.DateCP) + "";
        }

        if (claimProc.WriteOff != oldClaimProc.WriteOff)
        {
            if (command != "") command += ",";
            command += "WriteOff = " + SOut.Double(claimProc.WriteOff) + "";
        }

        if (claimProc.CodeSent != oldClaimProc.CodeSent)
        {
            if (command != "") command += ",";
            command += "CodeSent = '" + SOut.String(claimProc.CodeSent) + "'";
        }

        if (claimProc.AllowedOverride != oldClaimProc.AllowedOverride)
        {
            if (command != "") command += ",";
            command += "AllowedOverride = " + SOut.Double(claimProc.AllowedOverride) + "";
        }

        if (claimProc.Percentage != oldClaimProc.Percentage)
        {
            if (command != "") command += ",";
            command += "Percentage = " + SOut.Int(claimProc.Percentage) + "";
        }

        if (claimProc.PercentOverride != oldClaimProc.PercentOverride)
        {
            if (command != "") command += ",";
            command += "PercentOverride = " + SOut.Int(claimProc.PercentOverride) + "";
        }

        if (claimProc.CopayAmt != oldClaimProc.CopayAmt)
        {
            if (command != "") command += ",";
            command += "CopayAmt = " + SOut.Double(claimProc.CopayAmt) + "";
        }

        if (claimProc.NoBillIns != oldClaimProc.NoBillIns)
        {
            if (command != "") command += ",";
            command += "NoBillIns = " + SOut.Bool(claimProc.NoBillIns) + "";
        }

        if (claimProc.PaidOtherIns != oldClaimProc.PaidOtherIns)
        {
            if (command != "") command += ",";
            command += "PaidOtherIns = " + SOut.Double(claimProc.PaidOtherIns) + "";
        }

        if (claimProc.BaseEst != oldClaimProc.BaseEst)
        {
            if (command != "") command += ",";
            command += "BaseEst = " + SOut.Double(claimProc.BaseEst) + "";
        }

        if (claimProc.CopayOverride != oldClaimProc.CopayOverride)
        {
            if (command != "") command += ",";
            command += "CopayOverride = " + SOut.Double(claimProc.CopayOverride) + "";
        }

        if (claimProc.ProcDate.Date != oldClaimProc.ProcDate.Date)
        {
            if (command != "") command += ",";
            command += "ProcDate = " + SOut.Date(claimProc.ProcDate) + "";
        }

        if (claimProc.DateEntry.Date != oldClaimProc.DateEntry.Date)
        {
            if (command != "") command += ",";
            command += "DateEntry = " + SOut.Date(claimProc.DateEntry) + "";
        }

        if (claimProc.LineNumber != oldClaimProc.LineNumber)
        {
            if (command != "") command += ",";
            command += "LineNumber = " + SOut.Byte(claimProc.LineNumber) + "";
        }

        if (claimProc.DedEst != oldClaimProc.DedEst)
        {
            if (command != "") command += ",";
            command += "DedEst = " + SOut.Double(claimProc.DedEst) + "";
        }

        if (claimProc.DedEstOverride != oldClaimProc.DedEstOverride)
        {
            if (command != "") command += ",";
            command += "DedEstOverride = " + SOut.Double(claimProc.DedEstOverride) + "";
        }

        if (claimProc.InsEstTotal != oldClaimProc.InsEstTotal)
        {
            if (command != "") command += ",";
            command += "InsEstTotal = " + SOut.Double(claimProc.InsEstTotal) + "";
        }

        if (claimProc.InsEstTotalOverride != oldClaimProc.InsEstTotalOverride)
        {
            if (command != "") command += ",";
            command += "InsEstTotalOverride = " + SOut.Double(claimProc.InsEstTotalOverride) + "";
        }

        if (claimProc.PaidOtherInsOverride != oldClaimProc.PaidOtherInsOverride)
        {
            if (command != "") command += ",";
            command += "PaidOtherInsOverride = " + SOut.Double(claimProc.PaidOtherInsOverride) + "";
        }

        if (claimProc.EstimateNote != oldClaimProc.EstimateNote)
        {
            if (command != "") command += ",";
            command += "EstimateNote = '" + SOut.String(claimProc.EstimateNote) + "'";
        }

        if (claimProc.WriteOffEst != oldClaimProc.WriteOffEst)
        {
            if (command != "") command += ",";
            command += "WriteOffEst = " + SOut.Double(claimProc.WriteOffEst) + "";
        }

        if (claimProc.WriteOffEstOverride != oldClaimProc.WriteOffEstOverride)
        {
            if (command != "") command += ",";
            command += "WriteOffEstOverride = " + SOut.Double(claimProc.WriteOffEstOverride) + "";
        }

        if (claimProc.ClinicNum != oldClaimProc.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(claimProc.ClinicNum) + "";
        }

        if (claimProc.InsSubNum != oldClaimProc.InsSubNum)
        {
            if (command != "") command += ",";
            command += "InsSubNum = " + SOut.Long(claimProc.InsSubNum) + "";
        }

        if (claimProc.PaymentRow != oldClaimProc.PaymentRow)
        {
            if (command != "") command += ",";
            command += "PaymentRow = " + SOut.Int(claimProc.PaymentRow) + "";
        }

        if (claimProc.PayPlanNum != oldClaimProc.PayPlanNum)
        {
            if (command != "") command += ",";
            command += "PayPlanNum = " + SOut.Long(claimProc.PayPlanNum) + "";
        }

        if (claimProc.ClaimPaymentTracking != oldClaimProc.ClaimPaymentTracking)
        {
            if (command != "") command += ",";
            command += "ClaimPaymentTracking = " + SOut.Long(claimProc.ClaimPaymentTracking) + "";
        }

        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (claimProc.DateSuppReceived.Date != oldClaimProc.DateSuppReceived.Date)
        {
            if (command != "") command += ",";
            command += "DateSuppReceived = " + SOut.Date(claimProc.DateSuppReceived) + "";
        }

        if (claimProc.DateInsFinalized.Date != oldClaimProc.DateInsFinalized.Date)
        {
            if (command != "") command += ",";
            command += "DateInsFinalized = " + SOut.Date(claimProc.DateInsFinalized) + "";
        }

        if (claimProc.IsTransfer != oldClaimProc.IsTransfer)
        {
            if (command != "") command += ",";
            command += "IsTransfer = " + SOut.Bool(claimProc.IsTransfer) + "";
        }

        if (claimProc.ClaimAdjReasonCodes != oldClaimProc.ClaimAdjReasonCodes)
        {
            if (command != "") command += ",";
            command += "ClaimAdjReasonCodes = '" + SOut.String(claimProc.ClaimAdjReasonCodes) + "'";
        }

        if (claimProc.IsOverpay != oldClaimProc.IsOverpay)
        {
            if (command != "") command += ",";
            command += "IsOverpay = " + SOut.Bool(claimProc.IsOverpay) + "";
        }

        if (claimProc.SecurityHash != oldClaimProc.SecurityHash)
        {
            if (command != "") command += ",";
            command += "SecurityHash = '" + SOut.String(claimProc.SecurityHash) + "'";
        }

        if (command == "") return;
        command = "UPDATE claimproc SET " + command
                                          + " WHERE ClaimProcNum = " + SOut.Long(claimProc.ClaimProcNum);
        Db.NonQ(command);
    }

    public static bool UpdateComparison(ClaimProc claimProc, ClaimProc oldClaimProc)
    {
        if (claimProc.ProcNum != oldClaimProc.ProcNum) return true;
        if (claimProc.ClaimNum != oldClaimProc.ClaimNum) return true;
        if (claimProc.PatNum != oldClaimProc.PatNum) return true;
        if (claimProc.ProvNum != oldClaimProc.ProvNum) return true;
        if (claimProc.FeeBilled != oldClaimProc.FeeBilled) return true;
        if (claimProc.InsPayEst != oldClaimProc.InsPayEst) return true;
        if (claimProc.DedApplied != oldClaimProc.DedApplied) return true;
        if (claimProc.Status != oldClaimProc.Status) return true;
        if (claimProc.InsPayAmt != oldClaimProc.InsPayAmt) return true;
        if (claimProc.Remarks != oldClaimProc.Remarks) return true;
        if (claimProc.ClaimPaymentNum != oldClaimProc.ClaimPaymentNum) return true;
        if (claimProc.PlanNum != oldClaimProc.PlanNum) return true;
        if (claimProc.DateCP.Date != oldClaimProc.DateCP.Date) return true;
        if (claimProc.WriteOff != oldClaimProc.WriteOff) return true;
        if (claimProc.CodeSent != oldClaimProc.CodeSent) return true;
        if (claimProc.AllowedOverride != oldClaimProc.AllowedOverride) return true;
        if (claimProc.Percentage != oldClaimProc.Percentage) return true;
        if (claimProc.PercentOverride != oldClaimProc.PercentOverride) return true;
        if (claimProc.CopayAmt != oldClaimProc.CopayAmt) return true;
        if (claimProc.NoBillIns != oldClaimProc.NoBillIns) return true;
        if (claimProc.PaidOtherIns != oldClaimProc.PaidOtherIns) return true;
        if (claimProc.BaseEst != oldClaimProc.BaseEst) return true;
        if (claimProc.CopayOverride != oldClaimProc.CopayOverride) return true;
        if (claimProc.ProcDate.Date != oldClaimProc.ProcDate.Date) return true;
        if (claimProc.DateEntry.Date != oldClaimProc.DateEntry.Date) return true;
        if (claimProc.LineNumber != oldClaimProc.LineNumber) return true;
        if (claimProc.DedEst != oldClaimProc.DedEst) return true;
        if (claimProc.DedEstOverride != oldClaimProc.DedEstOverride) return true;
        if (claimProc.InsEstTotal != oldClaimProc.InsEstTotal) return true;
        if (claimProc.InsEstTotalOverride != oldClaimProc.InsEstTotalOverride) return true;
        if (claimProc.PaidOtherInsOverride != oldClaimProc.PaidOtherInsOverride) return true;
        if (claimProc.EstimateNote != oldClaimProc.EstimateNote) return true;
        if (claimProc.WriteOffEst != oldClaimProc.WriteOffEst) return true;
        if (claimProc.WriteOffEstOverride != oldClaimProc.WriteOffEstOverride) return true;
        if (claimProc.ClinicNum != oldClaimProc.ClinicNum) return true;
        if (claimProc.InsSubNum != oldClaimProc.InsSubNum) return true;
        if (claimProc.PaymentRow != oldClaimProc.PaymentRow) return true;
        if (claimProc.PayPlanNum != oldClaimProc.PayPlanNum) return true;
        if (claimProc.ClaimPaymentTracking != oldClaimProc.ClaimPaymentTracking) return true;
        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (claimProc.DateSuppReceived.Date != oldClaimProc.DateSuppReceived.Date) return true;
        if (claimProc.DateInsFinalized.Date != oldClaimProc.DateInsFinalized.Date) return true;
        if (claimProc.IsTransfer != oldClaimProc.IsTransfer) return true;
        if (claimProc.ClaimAdjReasonCodes != oldClaimProc.ClaimAdjReasonCodes) return true;
        if (claimProc.IsOverpay != oldClaimProc.IsOverpay) return true;
        if (claimProc.SecurityHash != oldClaimProc.SecurityHash) return true;
        return false;
    }
}