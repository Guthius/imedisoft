using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Schema;
using System.Xml.Serialization;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Providers;
using Ionic.Zip;

namespace OpenDentBusiness.Eclaims;

public class Ramq
{
    public static string ErrorMessage = "";

    ///<summary>Called from Eclaims and includes multiple claims.</summary>
    public static string SendBatch(Clearinghouse clearinghouseClin, List<ClaimSendQueueItem> queueItems, int batchNum)
    {
        //STEP 1 - Build XML output.
        var listDps = new List<DP_RACINDP>();
        var listProcCodes = ProcedureCodes.GetAllCodes();
        var listEtrans = new List<Etrans>();
        foreach (var queueItem in queueItems)
        {
            var etrans = Etranss.SetClaimSentOrPrinted(queueItem.ClaimNum, queueItem.ClaimStatus, queueItem.PatNum,
                clearinghouseClin.HqClearinghouseNum, EtransType.Claim_Ramq, batchNum, Security.CurUser.UserNum);
            listEtrans.Add(etrans);
            //Now we need to update our cache of claims to reflect the change that took place in the database above in Etranss.SetClaimSentOrPrinted()
            queueItem.ClaimStatus = "S";
            var claim = Claims.GetClaim(queueItem.ClaimNum);
            var provClaimTreat = Providers.GetById(claim.ProvTreat);
            var dp = new DP_RACINDP();

            #region Header

            dp.CHN = DP_RACINDPCHN.Item06;
            dp.CHNSpecified = true;
            dp.ENRG = DP_RACINDPENRG.Item1;
            dp.ENRGSpecified = true;
            //We hijack the TaxID number for the TRNSM field.  The TRNSM is a office identifying number.  Test range for developers is 18000 to 18999.
            dp.TRNSM = clearinghouseClin.SenderTIN;
            dp.DISP = provClaimTreat.NationalProviderId;
            //dp.CPTE_ADMN=;//Administrative account number.  Not currently used.
            var calendar = new JulianCalendar();
            dp.ATTES = (DateTime.Now.Year % 10) //One digit for year
                       + calendar.GetDayOfYear(DateTime.Now).ToString().PadLeft(3, '0') //3 digits for Julian day of year.
                       + (etrans.CarrierTransCounter % 1000).ToString().PadLeft(3, '0'); //3 digits for sequence number.
            dp.NCE = (etrans.CarrierTransCounter % 10000).ToString().PadLeft(4, '0');
            dp.DISP_REFNT = claim.CanadianReferralProviderNum.Trim();
            //dp.DIAGN=;//Diagnostic code.  Not currently used.
            dp.ETAB = provClaimTreat.CanadianOfficeNumber; //Usually empty.
            //dp.ADMIS=;//Date of patient admission.  Not currently used.  This would be the same as the date of service for dental claims anyway.
            //dp.SORTI=;//Date patient discharged.  Not currently used.  This would be the same as the date of service for dental claims anyway.
            dp.TOT_DEM = claim.ClaimFee.ToString().Replace(".", "").PadLeft(6, '0');
            dp.COMPL = TidyStr(claim.ClaimNote, 200);
            //dp.CS=;//Not sure what this is.  Not currently used.
            //dp.AUTOR=claim.PreAuthString;//Authorization number when invoicing acrylic prostheses. Required if DAT_AUTOR is present. Not currently used.
            //dp.DAT_AUTOR=claim.CanadianDateInitialLower;//Date of authorization when invoicing acrylic prostheses. Format YYMMDD. Not currently used.
            dp.SERV = claim.DateService.ToString("yyMMdd");

            #endregion Header

            #region Insurance
            
            var pat = Patients.GetPat(claim.PatNum);
            var insSub = InsSubs.GetOne(claim.InsSubNum);
            var listPatPlans = PatPlans.Refresh(claim.PatNum);
            var patPlan = PatPlans.GetByInsSubNum(listPatPlans, insSub.InsSubNum);

            dp.PERS_ASSU = new DP_RACINDPPERS_ASSU();
            dp.PERS_ASSU.NAM = insSub.SubscriberID;
            dp.PERS_ASSU.PRE = TidyStr(pat.FName, 20);
            dp.PERS_ASSU.NOM = TidyStr(pat.LName, 30);
            if (pat.Birthdate.Year > 1880)
            {
                dp.PERS_ASSU.NAISS = pat.Birthdate.ToString("yyyyMMdd");
            }

            dp.PERS_ASSU.SEXE = pat.Gender switch
            {
                PatientGender.Male => DP_RACINDPPERS_ASSUSEXE.M,
                PatientGender.Female => DP_RACINDPPERS_ASSUSEXE.F,
                _ => dp.PERS_ASSU.SEXE
            };


            dp.PERS_ASSU.CAM = patPlan.PatID;
            if (insSub.DateTerm.Year > 1880)
            {
                dp.PERS_ASSU.EXPIR_CAM = insSub.DateTerm.ToString("yyMM");
            }

            var insPlan = InsPlans.RefreshOne(claim.PlanNum);
            var insPlan2 = InsPlans.RefreshOne(claim.PlanNum2);
            Carrier carrier;
            InsPlan insPlanForNoBillIns;
            if (claim.ClaimType == "S")
            {
                carrier = Carriers.GetCarrier(insPlan2.CarrierNum);
                insPlanForNoBillIns = insPlan2;
            }
            else
            {
                carrier = Carriers.GetCarrier(insPlan.CarrierNum);
                insPlanForNoBillIns = insPlan;
            }

            if (carrier.Address.Trim() != "")
            {
                dp.PERS_ASSU.ADR_1 = carrier.Address;
                dp.PERS_ASSU.ADR_2 = carrier.Address2;
                dp.PERS_ASSU.CP = carrier.Zip;
            }

            #endregion Insurance

            #region Procedures

            var listClaimProcsForPat = ClaimProcs.Refresh(claim.PatNum);
            var listClaimProcsForClaim = ClaimProcs.GetForSendClaim(listClaimProcsForPat, claim.ClaimNum); //Excludes labs.
            var listProcsForPat = Procedures.Refresh(claim.PatNum);
            var listProcs = new List<DP_RACINDPACTE>();
            foreach (var claimProc in listClaimProcsForClaim)
            {
                var proc = Procedures.GetProcFromList(listProcsForPat, claimProc.ProcNum);
                if (proc.ProcFee == 0)
                {
                    continue;
                }

                var procCode = ProcedureCodes.GetProcCode(proc.CodeNum, listProcCodes);
                if (InsPlanPreferences.NoBillIns(procCode, insPlanForNoBillIns))
                {
                    continue;
                }

                var acteProc = new DP_RACINDPACTE();
                acteProc.ACTE = procCode.ProcCode;
                if (procCode.ProcCode.Length > 5)
                {
                    acteProc.ACTE = procCode.ProcCode.Substring(0, 5);
                }

                acteProc.ROLE = "1"; //1 for principal role and 2 for assistant role.
                //acte.MODIF=;//Optional.  Not sure what to put here, so leaving blank for now.
                acteProc.UNIT = proc.UnitQty.ToString().PadLeft(3, '0');
                acteProc.MNT = proc.ProcFee.ToString("F").Replace(".", "").PadLeft(6, '0');
                acteProc.DENT = proc.ToothNum.PadLeft(2, '0');
                acteProc.SURF = proc.Surf.PadLeft(2, '0');
                listProcs.Add(acteProc);
                var listLabProcs = Procedures.GetCanadianLabFees(proc.ProcNum, listProcsForPat);
                foreach (var labProc in listLabProcs)
                {
                    if (labProc.ProcFee == 0)
                    {
                        continue;
                    }

                    var labProcCode = ProcedureCodes.GetProcCode(labProc.CodeNum, listProcCodes);
                    var acteLab = new DP_RACINDPACTE();
                    acteLab.ACTE = labProcCode.ProcCode;
                    if (labProcCode.ProcCode.Length > 5)
                    {
                        acteLab.ACTE = labProcCode.ProcCode.Substring(0, 5);
                    }

                    acteLab.ROLE = "1"; //1 for principal role and 2 for assistant role.
                    acteLab.MNT = labProc.ProcFee.ToString("F").Replace(".", "").PadLeft(6, '0');
                    listProcs.Add(acteLab);
                }
            }

            dp.ACTE = listProcs.ToArray();

            #endregion Procedures

            listDps.Add(dp);
        }

        var batch = new DP_RACIN();
        batch.DP = listDps.ToArray();
        var sw = new StringWriter();
        var serializer = new XmlSerializer(typeof(DP_RACIN));
        serializer.Serialize(sw, batch);
        var xml = sw.ToString();
        //Save a copy of the batch xml to each etrans entry (one per claim).
        var etransMsgText = new EtransMessageText();
        etransMsgText.MessageText = xml;
        EtransMessageTexts.Insert(etransMsgText);
        foreach (var etrans in listEtrans)
        {
            etrans.EtransMessageTextNum = etransMsgText.EtransMessageTextNum;
            Etranss.Update(etrans);
        }

        if (!Directory.Exists(clearinghouseClin.ExportPath))
        {
            MessageBox.Show(Lans.g("Ramq", "Could not create directory ") + clearinghouseClin.ExportPath + "\r\n" + Lans.g("Ramq", "Go to Setup, Family/Insurance, Clearinghouses, and double-click the desired clearinghouse to update the path."));
            return "";
        }

        //Step 2 - ZIP XML and save to report path.  The zip file name and file name within the zip file do not matter.
        var zipFilePath = ODFileUtils.CreateRandomFile(clearinghouseClin.ExportPath, ".zip", "claims");
        ZipFile zip = null;
        try
        {
            zip = new ZipFile();
            zip.UseZip64WhenSaving = Zip64Option.Always;
            zip.AddEntry("claims" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xml", xml);
            zip.Save(zipFilePath);
            zip.Dispose();
        }
        catch (Exception ex)
        {
            ex.ToString();
            if (zip != null)
            {
                zip.Dispose();
            }

            if (File.Exists(zipFilePath))
            {
                try
                {
                    File.Delete(zipFilePath);
                }
                catch (Exception ex2)
                {
                    ex2.ToString();
                }
            }
        }

        return xml;
    }

    private static string TidyStr(string str, int maxwidth)
    {
        if (str.Length > maxwidth)
        {
            return str.Substring(0, maxwidth);
        }

        return str;
    }

    ///<summary>Returns true if the communications were successful, and false if they failed.</summary>
    public static bool Launch(Clearinghouse clearinghouseClin, int batchNum)
    {
        ErrorMessage = "Not implemented.";
        //TODO: Part 2 - Send ZIP file to RAMQ web service.  Upload the ZIP file to
        //https://www4.prod.ramq.gouv.qc.ca/RFP/RFP_ServSysRemuAct/NM/NMA_RecvrDem/NMA5_GereEchgElctrB2B_svc/ServGereEchgElctrB2B.svc
        //using HTTPS.  See ClaimConnect.cs for an example.
        return true;
    }

    ///<summary>Sets the MissingData and Warnings fields on the queueItem as appropriate.</summary>
    public static void GetMissingData(Clearinghouse clearinghouseClin, ClaimSendQueueItem queueItem)
    {
        var sbErrors = new StringBuilder();
        var sbWarnings = new StringBuilder();
        var claim = Claims.GetClaim(queueItem.ClaimNum);
        var provClaimTreat = Providers.GetById(claim.ProvTreat);
        var insSub = InsSubs.GetOne(claim.InsSubNum);
        InsPlan insPlanForNoBillIns = null;
        if (claim.ClaimType == "S")
        {
            insPlanForNoBillIns = InsPlans.RefreshOne(claim.PlanNum2);
        }
        else
        {
            insPlanForNoBillIns = InsPlans.RefreshOne(claim.PlanNum);
        }

        #region Header

        //TRNSM
        if (!Regex.IsMatch(clearinghouseClin.SenderTIN, @"^[0-9]+$"))
        {
            if (sbErrors.Length != 0)
            {
                sbErrors.Append(",");
            }

            sbErrors.Append("Tax ID Number (RAMQ TRNSM) is invalid");
        }
        else
        {
            var trnsm = SIn.Int(clearinghouseClin.SenderTIN);
            if (clearinghouseClin.ISA15 != "T" && trnsm >= 18000 && trnsm <= 18999)
            {
                if (sbErrors.Length != 0)
                {
                    sbErrors.Append(",");
                }

                sbErrors.Append("Tax ID Number (RAMQ TRNSM) is a test value on this production claim");
            }
            else if (clearinghouseClin.ISA15 == "T" && (trnsm < 18000 || trnsm > 18999))
            {
                if (sbErrors.Length != 0)
                {
                    sbErrors.Append(",");
                }

                sbErrors.Append("Tax ID Number (RAMQ TRNSM) must be between 18000 and 18999 for this test claim");
            }
        }

        //DISP
        if (!Regex.IsMatch(provClaimTreat.NationalProviderId, @"^[27][0-9]{5}$"))
        {
            if (sbErrors.Length != 0)
            {
                sbErrors.Append(",");
            }

            sbErrors.Append("Claim treating provider CDA Number for RAMQ is invalid");
        }

        //DISP_REFNT
        if (claim.CanadianReferralProviderNum.Trim().Length > 0
            && !Regex.IsMatch(claim.CanadianReferralProviderNum.Trim(), @"^[0-9]{6}$"))
        {
            if (sbErrors.Length != 0)
            {
                sbErrors.Append(",");
            }

            sbErrors.Append("Referral provider CDA Number for RAMQ is invalid");
        }

        //ETAB
        if (!Regex.IsMatch(provClaimTreat.CanadianOfficeNumber, @"^[0-9]{5}$"))
        {
            if (sbErrors.Length != 0)
            {
                sbErrors.Append(",");
            }

            sbErrors.Append("Claim treating provider Office Number for RAMQ is invalid");
        }

        //SERV
        if (claim.DateService.Year < 1880)
        {
            if (sbErrors.Length != 0)
            {
                sbErrors.Append(",");
            }

            sbErrors.Append("Claim date of service is invalid or missing");
        }

        #endregion Header

        #region Insurance

        //Most fields in the insuranace section are optional and thus do not require validation.
        //NAM			
        if (!Regex.IsMatch(insSub.SubscriberID, @"^[a-zA-Z]{4}[0-9]{6}[a-zA-Z0-9][0-9]$"))
        {
            if (sbErrors.Length != 0)
            {
                sbErrors.Append(",");
            }

            sbErrors.Append("Subscriber ID for RAMQ is invalid");
        }

        #endregion Insurance

        #region Procedures

        var listProcCodes = ProcedureCodes.GetAllCodes();
        var listClaimProcsForPat = ClaimProcs.Refresh(claim.PatNum);
        var listClaimProcsForClaim = ClaimProcs.GetForSendClaim(listClaimProcsForPat, claim.ClaimNum); //Excludes labs.
        var listProcsForPat = Procedures.Refresh(claim.PatNum);
        foreach (var claimProc in listClaimProcsForClaim)
        {
            var proc = Procedures.GetProcFromList(listProcsForPat, claimProc.ProcNum);
            if (proc.ProcFee == 0)
            {
                continue;
            }

            var procCode = ProcedureCodes.GetProcCode(proc.CodeNum, listProcCodes);
            if (InsPlanPreferences.NoBillIns(procCode, insPlanForNoBillIns))
            {
                continue;
            }

            //ACTE
            if (procCode.ProcCode.Length < 5 || !Regex.IsMatch(procCode.ProcCode.Substring(0, 5), @"^[0-9]{5}$"))
            {
                if (sbErrors.Length != 0)
                {
                    sbErrors.Append(",");
                }

                sbErrors.Append("Procedure code invalid '" + procCode.ProcCode + "'");
            }

            var listLabProcs = Procedures.GetCanadianLabFees(proc.ProcNum, listProcsForPat);
            foreach (var labProc in listLabProcs)
            {
                if (labProc.ProcFee == 0)
                {
                    continue;
                }

                var labProcCode = ProcedureCodes.GetProcCode(labProc.CodeNum, listProcCodes);
                if (labProcCode.ProcCode.Length < 5 || !Regex.IsMatch(labProcCode.ProcCode.Substring(0, 5), @"^[0-9]{5}$"))
                {
                    if (sbErrors.Length != 0)
                    {
                        sbErrors.Append(",");
                    }

                    sbErrors.Append("Lab code invalid '" + labProcCode.ProcCode + "'");
                }
            }
        }

        #endregion Procedures

        queueItem.MissingData = sbErrors.ToString();
        queueItem.Warnings = sbWarnings.ToString();
    }
}

#region DP_RACIN - autogenerated by xsd.exe

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=4.0.30319.33440.
// 

/// <remarks/>
[GeneratedCode("xsd", "4.0.30319.33440")]
[Serializable()]
[DebuggerStepThrough()]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://tempuri.org/DP_DENTI.xsd")]
[XmlRoot(Namespace = "http://tempuri.org/DP_DENTI.xsd", IsNullable = false)]
public partial class DP_RACIN
{
    private DP_RACINDP[] dpField;

    /// <remarks/>
    [XmlElement("DP", Form = XmlSchemaForm.Unqualified)]
    public DP_RACINDP[] DP
    {
        get { return this.dpField; }
        set { this.dpField = value; }
    }
}

/// <remarks/>
[GeneratedCode("xsd", "4.0.30319.33440")]
[Serializable()]
[DebuggerStepThrough()]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://tempuri.org/DP_DENTI.xsd")]
public partial class DP_RACINDP
{
    private DP_RACINDPPERS_ASSU pERS_ASSUField;

    private DP_RACINDPACTE[] aCTEField;

    private DP_RACINDPCHN cHNField;

    private bool cHNFieldSpecified;

    private DP_RACINDPENRG eNRGField;

    private bool eNRGFieldSpecified;

    private string tRNSMField;

    private string dISPField;

    private string cPTE_ADMNField;

    private string aTTESField;

    private string nCEField;

    private string dISP_REFNTField;

    private string dIAGNField;

    private string eTABField;

    private string aDMISField;

    private string sORTIField;

    private string tOT_DEMField;

    private string cOMPLField;

    private string csField;

    private string aUTORField;

    private string dAT_AUTORField;

    private string sERVField;

    /// <remarks/>
    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public DP_RACINDPPERS_ASSU PERS_ASSU
    {
        get { return this.pERS_ASSUField; }
        set { this.pERS_ASSUField = value; }
    }

    /// <remarks/>
    [XmlElement("ACTE", Form = XmlSchemaForm.Unqualified)]
    public DP_RACINDPACTE[] ACTE
    {
        get { return this.aCTEField; }
        set { this.aCTEField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public DP_RACINDPCHN CHN
    {
        get { return this.cHNField; }
        set { this.cHNField = value; }
    }

    /// <remarks/>
    [XmlIgnore()]
    public bool CHNSpecified
    {
        get { return this.cHNFieldSpecified; }
        set { this.cHNFieldSpecified = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public DP_RACINDPENRG ENRG
    {
        get { return this.eNRGField; }
        set { this.eNRGField = value; }
    }

    /// <remarks/>
    [XmlIgnore()]
    public bool ENRGSpecified
    {
        get { return this.eNRGFieldSpecified; }
        set { this.eNRGFieldSpecified = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string TRNSM
    {
        get { return this.tRNSMField; }
        set { this.tRNSMField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string DISP
    {
        get { return this.dISPField; }
        set { this.dISPField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string CPTE_ADMN
    {
        get { return this.cPTE_ADMNField; }
        set { this.cPTE_ADMNField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string ATTES
    {
        get { return this.aTTESField; }
        set { this.aTTESField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string NCE
    {
        get { return this.nCEField; }
        set { this.nCEField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string DISP_REFNT
    {
        get { return this.dISP_REFNTField; }
        set { this.dISP_REFNTField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string DIAGN
    {
        get { return this.dIAGNField; }
        set { this.dIAGNField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string ETAB
    {
        get { return this.eTABField; }
        set { this.eTABField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string ADMIS
    {
        get { return this.aDMISField; }
        set { this.aDMISField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string SORTI
    {
        get { return this.sORTIField; }
        set { this.sORTIField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "nonNegativeInteger")]
    public string TOT_DEM
    {
        get { return this.tOT_DEMField; }
        set { this.tOT_DEMField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string COMPL
    {
        get { return this.cOMPLField; }
        set { this.cOMPLField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string CS
    {
        get { return this.csField; }
        set { this.csField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string AUTOR
    {
        get { return this.aUTORField; }
        set { this.aUTORField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string DAT_AUTOR
    {
        get { return this.dAT_AUTORField; }
        set { this.dAT_AUTORField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string SERV
    {
        get { return this.sERVField; }
        set { this.sERVField = value; }
    }
}

/// <remarks/>
[GeneratedCode("xsd", "4.0.30319.33440")]
[Serializable()]
[DebuggerStepThrough()]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://tempuri.org/DP_DENTI.xsd")]
public partial class DP_RACINDPPERS_ASSU
{
    private string nAMField;

    private string pREField;

    private string nOMField;

    private string nAISSField;

    private DP_RACINDPPERS_ASSUSEXE sEXEField;

    private bool sEXEFieldSpecified;

    private string cAMField;

    private string eXPIR_CAMField;

    private string aDR_1Field;

    private string aDR_2Field;

    private string cpField;

    /// <remarks/>
    [XmlAttribute()]
    public string NAM
    {
        get { return this.nAMField; }
        set { this.nAMField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string PRE
    {
        get { return this.pREField; }
        set { this.pREField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string NOM
    {
        get { return this.nOMField; }
        set { this.nOMField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string NAISS
    {
        get { return this.nAISSField; }
        set { this.nAISSField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public DP_RACINDPPERS_ASSUSEXE SEXE
    {
        get { return this.sEXEField; }
        set { this.sEXEField = value; }
    }

    /// <remarks/>
    [XmlIgnore()]
    public bool SEXESpecified
    {
        get { return this.sEXEFieldSpecified; }
        set { this.sEXEFieldSpecified = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string CAM
    {
        get { return this.cAMField; }
        set { this.cAMField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string EXPIR_CAM
    {
        get { return this.eXPIR_CAMField; }
        set { this.eXPIR_CAMField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string ADR_1
    {
        get { return this.aDR_1Field; }
        set { this.aDR_1Field = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string ADR_2
    {
        get { return this.aDR_2Field; }
        set { this.aDR_2Field = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string CP
    {
        get { return this.cpField; }
        set { this.cpField = value; }
    }
}

/// <remarks/>
[GeneratedCode("xsd", "4.0.30319.33440")]
[Serializable()]
[XmlType(AnonymousType = true, Namespace = "http://tempuri.org/DP_DENTI.xsd")]
public enum DP_RACINDPPERS_ASSUSEXE
{
    /// <remarks/>
    M,

    /// <remarks/>
    F,
}

/// <remarks/>
[GeneratedCode("xsd", "4.0.30319.33440")]
[Serializable()]
[DebuggerStepThrough()]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://tempuri.org/DP_DENTI.xsd")]
public partial class DP_RACINDPACTE
{
    private string aCTEField;

    private string rOLEField;

    private string mODIFField;

    private string uNITField;

    private string mNTField;

    private string dENTField;

    private string sURFField;

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string ACTE
    {
        get { return this.aCTEField; }
        set { this.aCTEField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string ROLE
    {
        get { return this.rOLEField; }
        set { this.rOLEField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string MODIF
    {
        get { return this.mODIFField; }
        set { this.mODIFField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string UNIT
    {
        get { return this.uNITField; }
        set { this.uNITField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string MNT
    {
        get { return this.mNTField; }
        set { this.mNTField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string DENT
    {
        get { return this.dENTField; }
        set { this.dENTField = value; }
    }

    /// <remarks/>
    [XmlAttribute(DataType = "positiveInteger")]
    public string SURF
    {
        get { return this.sURFField; }
        set { this.sURFField = value; }
    }
}

/// <remarks/>
[GeneratedCode("xsd", "4.0.30319.33440")]
[Serializable()]
[XmlType(AnonymousType = true, Namespace = "http://tempuri.org/DP_DENTI.xsd")]
public enum DP_RACINDPCHN
{
    /// <remarks/>
    [XmlEnum("06")]
    Item06,
}

/// <remarks/>
[GeneratedCode("xsd", "4.0.30319.33440")]
[Serializable()]
[XmlType(AnonymousType = true, Namespace = "http://tempuri.org/DP_DENTI.xsd")]
public enum DP_RACINDPENRG
{
    /// <remarks/>
    [XmlEnum("1")]
    Item1,
}

#endregion DP_RACIN - autogenerated by xsd.exe