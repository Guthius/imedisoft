using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EhrMeasures
{
    ///<summary>Positive/Negative Snomed indicators for lab results.</summary>
    private static readonly string _snomedLabResult = @"'393474000','394424008','10828004','393476003','394426005','260385009','61620004','90213003','260408008','272069004','4173002','61707005','272068007','85696000'";

    ///<summary>Select All EHRMeasures from combination of db, static data, and complex calculations.</summary>
    public static List<EhrMeasure> SelectAll(DateTime dateStart, DateTime dateEnd, long provNum)
    {
        var command = "SELECT * FROM ehrmeasure "
                      + "WHERE MeasureType IN ("
                      + SOut.Int((int) EhrMeasureType.ProblemList) + ","
                      + SOut.Int((int) EhrMeasureType.MedicationList) + ","
                      + SOut.Int((int) EhrMeasureType.AllergyList) + ","
                      + SOut.Int((int) EhrMeasureType.Demographics) + ","
                      + SOut.Int((int) EhrMeasureType.Education) + ","
                      + SOut.Int((int) EhrMeasureType.ElectronicCopyAccess) + ","
                      //+POut.Int((int)EhrMeasureType.TimelyAccess)+"," //Deprecated in 2014.  V/D/T (patient portal) now required.  ElectronicCopyAccess used instead.
                      + SOut.Int((int) EhrMeasureType.ProvOrderEntry) + ","
                      + SOut.Int((int) EhrMeasureType.CPOE_MedOrdersOnly) + ","
                      + SOut.Int((int) EhrMeasureType.CPOE_PreviouslyOrdered) + ","
                      + SOut.Int((int) EhrMeasureType.Rx) + ","
                      + SOut.Int((int) EhrMeasureType.VitalSigns) + ","
                      + SOut.Int((int) EhrMeasureType.VitalSigns2014) + ","
                      + SOut.Int((int) EhrMeasureType.VitalSignsBMIOnly) + ","
                      + SOut.Int((int) EhrMeasureType.VitalSignsBPOnly) + ","
                      + SOut.Int((int) EhrMeasureType.Smoking) + ","
                      + SOut.Int((int) EhrMeasureType.Lab) + ","
                      //+POut.Int((int)EhrMeasureType.ElectronicCopy)+"," //Deprecated in 2014.  V/D/T (patient portal) now required.  ElectronicCopyAccess used instead.
                      + SOut.Int((int) EhrMeasureType.ClinicalSummaries) + ","
                      + SOut.Int((int) EhrMeasureType.Reminders) + ","
                      + SOut.Int((int) EhrMeasureType.MedReconcile) + ","
                      + SOut.Int((int) EhrMeasureType.SummaryOfCare) + ", "
                      + SOut.Int((int) EhrMeasureType.VitalSigns2014) + ") "
                      + "ORDER BY MeasureType";
        var retVal = EhrMeasureCrud.SelectMany(command);
        var s = new Stopwatch();
        for (var i = 0; i < retVal.Count; i++)
        {
            s.Restart();
            retVal[i].Objective = GetObjective(retVal[i].MeasureType);
            retVal[i].Measure = GetMeasure(retVal[i].MeasureType);
            retVal[i].PercentThreshold = GetThreshold(retVal[i].MeasureType);
            var table = GetTable(retVal[i].MeasureType, dateStart, dateEnd, provNum);
            if (table == null)
            {
                retVal[i].Numerator = -1;
                retVal[i].Denominator = -1;
            }
            else
            {
                retVal[i].Numerator = CalcNumerator(table);
                retVal[i].Denominator = table.Rows.Count;
            }

            retVal[i].NumeratorExplain = GetNumeratorExplain(retVal[i].MeasureType);
            retVal[i].DenominatorExplain = GetDenominatorExplain(retVal[i].MeasureType);
            retVal[i].ExclusionExplain = GetExclusionExplain(retVal[i].MeasureType);
            retVal[i].ExclusionCount = GetExclusionCount(retVal[i].MeasureType, dateStart, dateEnd, provNum);
            retVal[i].ExclusionCountDescript = GetExclusionCountDescript(retVal[i].MeasureType);
            s.Stop();
            retVal[i].ElapsedTime = s.Elapsed;
        }

        return retVal;
    }

    ///<summary>Select All EHRMeasures from combination of db, static data, and complex calculations.</summary>
    public static List<EhrMeasure> SelectAllMu2(DateTime dateStart, DateTime dateEnd, long provNum)
    {
        var retVal = GetMU2List();
        retVal.RemoveAll(x =>
            x.MeasureType.In(
                EhrMeasureType.DrugDrugInteractChecking,
                EhrMeasureType.DrugFormularyChecking,
                EhrMeasureType.ProtectElectHealthInfo,
                EhrMeasureType.ImmunizationRegistries,
                EhrMeasureType.SyndromicSurveillance,
                EhrMeasureType.PatientList,
                EhrMeasureType.ClinicalInterventionRules)
        );
        var s = new Stopwatch();
        for (var i = 0; i < retVal.Count; i++)
        {
            s.Restart();
            retVal[i].Objective = GetObjectiveMu2(retVal[i].MeasureType);
            retVal[i].Measure = GetMeasureMu2(retVal[i].MeasureType);
            retVal[i].PercentThreshold = GetThresholdMu2(retVal[i].MeasureType);
            var table = GetTableMu2(retVal[i].MeasureType, dateStart, dateEnd, provNum);
            if (table == null)
            {
                retVal[i].Numerator = -1;
                retVal[i].Denominator = -1;
            }
            else
            {
                retVal[i].Numerator = CalcNumerator(table);
                retVal[i].Denominator = table.Rows.Count;
            }

            retVal[i].NumeratorExplain = GetNumeratorExplainMu2(retVal[i].MeasureType);
            retVal[i].DenominatorExplain = GetDenominatorExplainMu2(retVal[i].MeasureType);
            retVal[i].ExclusionExplain = GetExclusionExplainMu2(retVal[i].MeasureType);
            retVal[i].ExclusionCount = GetExclusionCountMu2(retVal[i].MeasureType, dateStart, dateEnd, provNum);
            retVal[i].ExclusionCountDescript = GetExclusionCountDescriptMu2(retVal[i].MeasureType);
            s.Stop();
            retVal[i].ElapsedTime = s.Elapsed;
        }

        return retVal;
    }

    ///<summary>Select All EHRMeasures from combination of db, static data, and complex calculations.</summary>
    public static List<EhrMeasure> SelectAllMu2Mod(DateTime dateStart, DateTime dateEnd, long provNum)
    {
        var retVal = GetMU2ModList();
        retVal.RemoveAll(x =>
            x.MeasureType.In(
                EhrMeasureType.ProtectElectHealthInfo,
                EhrMeasureType.ClinicalInterventionRules,
                EhrMeasureType.DrugDrugInteractChecking,
                EhrMeasureType.DrugFormularyChecking,
                EhrMeasureType.ImmunizationRegistries,
                EhrMeasureType.SyndromicSurveillance,
                EhrMeasureType.ProblemList,
                EhrMeasureType.MedicationList,
                EhrMeasureType.AllergyList)
        );
        var s = new Stopwatch();
        for (var i = 0; i < retVal.Count; i++)
        {
            s.Restart();
            retVal[i].Objective = GetObjectiveMu2(retVal[i].MeasureType);
            retVal[i].Measure = GetMeasureMu2Mod(retVal[i].MeasureType);
            retVal[i].PercentThreshold = GetThresholdMu2(retVal[i].MeasureType);
            var table = GetTableMu2Mod(retVal[i].MeasureType, dateStart, dateEnd, provNum);
            if (table == null)
            {
                retVal[i].Numerator = -1;
                retVal[i].Denominator = -1;
            }
            else
            {
                retVal[i].Numerator = CalcNumerator(table);
                retVal[i].Denominator = table.Rows.Count;
            }

            retVal[i].NumeratorExplain = GetNumeratorExplainMu2Mod(retVal[i].MeasureType);
            retVal[i].DenominatorExplain = GetDenominatorExplainMu2Mod(retVal[i].MeasureType);
            retVal[i].ExclusionExplain = GetExclusionExplainMu2Mod(retVal[i].MeasureType);
            retVal[i].ExclusionCount = GetExclusionCountMu2(retVal[i].MeasureType, dateStart, dateEnd, provNum);
            retVal[i].ExclusionCountDescript = GetExclusionCountDescriptMu2(retVal[i].MeasureType);
            s.Stop();
            retVal[i].ElapsedTime = s.Elapsed;
        }

        return retVal;
    }

    
    public static void Update(EhrMeasure ehrMeasure)
    {
        EhrMeasureCrud.Update(ehrMeasure);
    }

    #region Meaningful Use 1

    ///<summary>Returns the Objective text based on the EHR certification documents.</summary>
    private static string GetObjective(EhrMeasureType mtype)
    {
        switch (mtype)
        {
            case EhrMeasureType.ProblemList:
                return "Maintain an up-to-date problem list of current and active diagnoses.";
            case EhrMeasureType.MedicationList:
                return "Maintain active medication list.";
            case EhrMeasureType.AllergyList:
                return "Maintain active medication allergy list";
            case EhrMeasureType.Demographics:
                return "Record demographics: Preferred language, Gender, Race, Ethnicity, Date of Birth";
            case EhrMeasureType.Education:
                return "Use certified EHR technology to identify patient-specific education resources and provide those resources to the patient if appropriate.";
            case EhrMeasureType.ElectronicCopyAccess:
                return "Provide patients the ability to view online, download and transmit their health information within four business days of the information being available to the EP.";
            case EhrMeasureType.TimelyAccess:
                return "Provide patients with timely electronic access to their health information (including lab results, problem list, medication lists, medication allergies) within four business days of the information being available to the EP";
            case EhrMeasureType.ProvOrderEntry:
                return "Use CPOE for medication orders directly entered by any licensed healthcare professional who can enter orders into the medical record per state, local and professional guidelines.";
            case EhrMeasureType.CPOE_MedOrdersOnly:
                return "Use CPOE for medication orders directly entered by any licensed healthcare professional who can enter orders into the medical record per state, local and professional guidelines.";
            case EhrMeasureType.CPOE_PreviouslyOrdered:
                return "Use CPOE for medication orders directly entered by any licensed healthcare professional who can enter orders into the medical record per state, local and professional guidelines.";
            case EhrMeasureType.Rx:
                return "Generate and transmit permissible prescriptions electronically (eRx).";
            case EhrMeasureType.VitalSigns:
                return "Record and chart changes in vital signs: Height, Weight, Blood pressure for age 3 and over, Calculate and display BMI, Plot and display growth charts for children 2-20 years, including BMI";
            case EhrMeasureType.VitalSigns2014:
                return "Record and chart changes in vital signs: Height, Weight, Blood pressure for age 3 and over, Calculate and display BMI, Plot and display growth charts for children 2-20 years, including BMI";
            case EhrMeasureType.VitalSignsBMIOnly:
                return "Record and chart changes in vital signs: Height, Weight, Calculate and display BMI, Plot and display growth charts for children 2-20 years, including BMI";
            case EhrMeasureType.VitalSignsBPOnly:
                return "Record changes in blood pressure for age 3 and over";
            case EhrMeasureType.Smoking:
                return "Record smoking status for patients 13 years old or older.";
            case EhrMeasureType.Lab:
                return "Incorporate clinical lab-test results into certified EHR technology as structured data.";
            case EhrMeasureType.ElectronicCopy:
                return "Provide patients with an electronic copy of their health information (including diagnostic test results, problem list, medication lists, medication allergies), upon request.";
            case EhrMeasureType.ClinicalSummaries:
                return "Provide clinical summaries for patients for each office visit.";
            case EhrMeasureType.Reminders:
                return "Send reminders to patients per patient preference for preventive/ follow up care.";
            case EhrMeasureType.MedReconcile:
                return "The EP, eligible hospital or CAH who receives a patient from another setting of care or provider of care or believes an encounter is relevant should perform medication reconciliation.";
            case EhrMeasureType.SummaryOfCare:
                return "The EP, eligible hospital or CAH who transitions their patient to another setting of care or provider of care or refers their patient to another provider of care should provide summary of care record for each transition of care or referral.";
        }

        throw new ApplicationException("Type not found: " + mtype);
    }

    ///<summary>Returns the Measures text based on the EHR certification documents.</summary>
    private static string GetMeasure(EhrMeasureType mtype)
    {
        switch (mtype)
        {
            case EhrMeasureType.ProblemList:
                return "More than 80% of all unique patients seen by the Provider have at least one entry or an indication that no problems are known for the patient recorded as structured data.";
            //Leaving original wording so change will not require re-testing to meet 2011 certification.  The wording below may be used in 2014 MU 1 as it is more accurate.
            //return "More than 80% of all unique patients seen by the Provider have at least one problem entered with an ICD-9 code or SNOMED code attached or an indication that no problems are known for the patient recorded as structured data.";
            case EhrMeasureType.MedicationList:
                return "More than 80% of all unique patients seen by the Provider have at least one entry (or an indication that the patient is not currently prescribed any medication) recorded as structured data.";
            case EhrMeasureType.AllergyList:
                return "More than 80% of all unique patients seen by the Provider have at least one entry (or an indication that the patient has no known medication allergies) recorded as structured data.";
            case EhrMeasureType.Demographics:
                return "More than 50% of all unique patients seen by the Provider have demographics recorded as structured data.";
            case EhrMeasureType.Education:
                return "More than 10% of all unique patients seen by the Provider during the EHR reporting period are provided patient-specific education resources.";
            case EhrMeasureType.ElectronicCopyAccess:
                return "More than 50% of all unique patients seen by the EP during the EHR reporting period are provided timely (available to the patient within 4 business days after the information is available to the EP) online access to their health information.";
            case EhrMeasureType.TimelyAccess:
                return "More than 10% of all unique patients seen by the Provider are provided timely (available to the patient within four business days of being updated in the certified EHR technology) electronic access to their health information subject to the Provider�s discretion to withhold certain information.";
            case EhrMeasureType.ProvOrderEntry:
                return "More than 30% of unique patients with at least one medication in their medication list seen by the Provider have at least one medication order entered using CPOE.";
            case EhrMeasureType.CPOE_MedOrdersOnly:
                return "More than 30% of medication orders created by the Provider during the reporting period are entered using CPOE.";
            case EhrMeasureType.CPOE_PreviouslyOrdered:
                return "More than 30% of unique patients with at least one medication in their medication list seen by the Provider for whom the Provider has previously ordered medication have at least one medication order entered using CPOE.";
            case EhrMeasureType.Rx:
                return "More than 40% of all permissible prescriptions written by the Provider are transmitted electronically using certified EHR technology.";
            case EhrMeasureType.VitalSigns:
                return "More than 50% of all unique patients (age 3 and over for blood pressure) seen by the Provider, height, weight and blood pressure are recorded as structured data.";
            case EhrMeasureType.VitalSigns2014:
                return "More than 50% of all unique patients (age 3 and over for blood pressure) seen by the Provider, height, weight and blood pressure are recorded as structured data.";
            case EhrMeasureType.VitalSignsBMIOnly:
                return "More than 50% of all unique patients seen by the Provider, height and weight are recorded as structured data.";
            case EhrMeasureType.VitalSignsBPOnly:
                return "More than 50% of all unique patients age 3 and over seen by the Provider have blood pressure recorded as structured data.";
            case EhrMeasureType.Smoking:
                return "More than 50% of all unique patients 13 years old or older seen by the Provider have smoking status recorded as structured data.";
            case EhrMeasureType.Lab:
                return "More than 40% of all clinical lab tests results ordered by the Provider during the EHR reporting period whose results are either in a positive/negative or numerical format are incorporated in certified EHR technology as structured data.";
            case EhrMeasureType.ElectronicCopy:
                return "More than 50% of patients who request an electronic copy of their health information are provided it within 3 business days";
            case EhrMeasureType.ClinicalSummaries:
                return "Clinical summaries provided to patients for more than 50% of all office visits within 3 business days.";
            case EhrMeasureType.Reminders:
                return "More than 20% of all unique patients 65 years or older or 5 years old or younger were sent an appropriate reminder during the EHR reporting period.";
            case EhrMeasureType.MedReconcile:
                return "The Provider performs medication reconciliation for more than 50% of transitions of care in which the patient is transitioned into the care of the Provider.";
            case EhrMeasureType.SummaryOfCare:
                return "The Provider who transitions or refers their patient to another setting of care or provider of care provides a summary of care record for more than 50% of transitions of care and referrals.";
        }

        throw new ApplicationException("Type not found: " + mtype);
    }

    ///<summary>Returns the Measures text based on the EHR certification documents.</summary>
    private static int GetThreshold(EhrMeasureType mtype)
    {
        switch (mtype)
        {
            case EhrMeasureType.ProblemList:
                return 80;
            case EhrMeasureType.MedicationList:
                return 80;
            case EhrMeasureType.AllergyList:
                return 80;
            case EhrMeasureType.Demographics:
                return 50;
            case EhrMeasureType.Education:
                return 10;
            case EhrMeasureType.ElectronicCopyAccess:
                return 50;
            case EhrMeasureType.TimelyAccess:
                return 10;
            case EhrMeasureType.ProvOrderEntry:
                return 30;
            case EhrMeasureType.CPOE_MedOrdersOnly:
                return 30;
            case EhrMeasureType.CPOE_PreviouslyOrdered:
                return 30;
            case EhrMeasureType.Rx:
                return 40;
            case EhrMeasureType.VitalSigns:
                return 50;
            case EhrMeasureType.VitalSigns2014:
                return 50;
            case EhrMeasureType.VitalSignsBMIOnly:
                return 50;
            case EhrMeasureType.VitalSignsBPOnly:
                return 50;
            case EhrMeasureType.Smoking:
                return 50;
            case EhrMeasureType.Lab:
                return 40;
            case EhrMeasureType.ElectronicCopy:
                return 50;
            case EhrMeasureType.ClinicalSummaries:
                return 50;
            case EhrMeasureType.Reminders:
                return 20;
            case EhrMeasureType.MedReconcile:
                return 50;
            case EhrMeasureType.SummaryOfCare:
                return 50;
        }

        throw new ApplicationException("Type not found: " + mtype);
    }

    public static DataTable GetTable(EhrMeasureType mtype, DateTime dateStart, DateTime dateEnd, long provNum)
    {
        var command = "";
        var tableRaw = new DataTable();
        var provCur = Providers.GetProv(provNum);
        command = "SELECT " + DbHelper.GroupConcat("provider.ProvNum") + " FROM provider WHERE provider.ProvNum IN "
                  + "(SELECT ProvNum FROM provider pv, ehrprovkey epk WHERE pv.LName=epk.LName AND pv.FName=epk.FName "
                  + "AND epk.LName='" + SOut.String(provCur.LName) + "' AND epk.FName='" + SOut.String(provCur.FName) + "') ";
        var provs = DataCore.GetScalar(command);
        command = "SELECT " + DbHelper.GroupConcat("provider.NationalProvID") + " FROM provider WHERE provider.ProvNum IN "
                  + "(SELECT ProvNum FROM provider pv, ehrprovkey epk WHERE pv.LName=epk.LName AND pv.FName=epk.FName "
                  + "AND epk.LName='" + SOut.String(provCur.LName) + "' AND epk.FName='" + SOut.String(provCur.FName) + "') "
                  + "AND provider.NationalProvID !='' AND provider.NationalProvID !=','";
        var provNPIs = DataCore.GetScalar(command);
        provNPIs.Trim(',');
        if (provNPIs == "") provNPIs = "NULL"; //No NPI entered, queries below use IN statements and require at least one value.
        //Some measures use a temp table.  Create a random number to tack onto the end of the temp table name to avoid possible table collisions.
        var rnd = new Random();
        var rndStr = rnd.Next(1000000).ToString();
        switch (mtype)
        {
            #region ProblemList

            case EhrMeasureType.ProblemList:
                //Jordan's original query
                //command="SELECT PatNum,LName,FName, "
                //  +"(SELECT COUNT(*) FROM disease WHERE PatNum=patient.PatNum AND DiseaseDefNum="
                //    +POut.Long(PrefC.GetLong(PrefName.ProblemsIndicateNone))+") AS problemsNone, "
                //  +"(SELECT COUNT(*) FROM disease WHERE PatNum=patient.PatNum) AS problemsAll "
                //  +"FROM patient "
                //  +"WHERE EXISTS(SELECT * FROM procedurelog WHERE patient.PatNum=procedurelog.PatNum "
                //  +"AND procedurelog.ProcStatus=2 "//complete
                //  +"AND procedurelog.ProvNum IN("+POut.String(provs)+") "
                //  +"AND procedurelog.ProcDate >= "+POut.Date(dateStart)+" "
                //  +"AND procedurelog.ProcDate <= "+POut.Date(dateEnd)+")";
                //Query optimized to be faster by Cameron
                //command="SELECT A.*,COALESCE(problemsNone.Count,0) AS problemsNone,COALESCE(problemsAll.Count,0) AS problemsAll "
                //	+"FROM (SELECT patient.PatNum,LName,FName FROM patient "
                //	+"INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                //	+"AND procedurelog.ProvNum IN("+POut.String(provs)+") "
                //	+"AND procedurelog.ProcDate BETWEEN "+POut.Date(dateStart)+" AND "+POut.Date(dateEnd)+" "
                //	+"GROUP BY patient.PatNum) A "
                //	+"LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM disease WHERE DiseaseDefNum="+POut.Long(PrefC.GetLong(PrefName.ProblemsIndicateNone))+" "
                //	+"GROUP BY PatNum) problemsNone ON problemsNone.PatNum=A.PatNum "
                //	+"LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM disease GROUP BY PatNum) problemsAll ON problemsAll.PatNum=A.PatNum";
                //Query modified to count only problems with ICD9 or SNOMED code attached
                command = "SELECT A.*,COALESCE(problemsNone.Count,0) AS problemsNone,COALESCE(problemsAll.Count,0) AS problemsAll "
                          + "FROM (SELECT patient.PatNum,LName,FName FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ") "
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) A "
                          + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM disease WHERE DiseaseDefNum=" + SOut.Long(PrefC.GetLong(PrefName.ProblemsIndicateNone)) + " "
                          + "AND ProbStatus=0 GROUP BY PatNum) problemsNone ON problemsNone.PatNum=A.PatNum "
                          + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM disease "
                          + "INNER JOIN diseasedef ON disease.DiseaseDefNum=diseasedef.DiseaseDefNum "
                          + "AND disease.DiseaseDefNum!=" + SOut.Long(PrefC.GetLong(PrefName.ProblemsIndicateNone)) + " "
                          + "WHERE (diseasedef.SnomedCode!='' OR diseasedef.ICD9Code!='') "
                          + "GROUP BY PatNum) problemsAll ON problemsAll.PatNum=A.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region MedicationList

            case EhrMeasureType.MedicationList:
                command = "SELECT A.*,COALESCE(medsNone.Count,0) AS medsNone,COALESCE(medsAll.Count,0) AS medsAll "
                          + "FROM (SELECT patient.PatNum,LName,FName	FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum	AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) A "
                          + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM medicationpat "
                          + "WHERE MedicationNum=" + SOut.Long(PrefC.GetLong(PrefName.MedicationsIndicateNone)) + " "
                          + "AND (YEAR(DateStop)<1880 OR DateStop>" + SOut.Date(dateEnd) + ") GROUP BY PatNum) medsNone ON medsNone.PatNum=A.PatNum "
                          + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM medicationpat "
                          + "WHERE MedicationNum!=" + SOut.Long(PrefC.GetLong(PrefName.MedicationsIndicateNone)) + " "
                          + "GROUP BY PatNum) medsAll ON medsAll.PatNum=A.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region AllergyList

            case EhrMeasureType.AllergyList:
                //Jordan's original query
                //command="SELECT PatNum,LName,FName, "
                //  +"(SELECT COUNT(*) FROM allergy WHERE PatNum=patient.PatNum AND AllergyDefNum="
                //    +POut.Long(PrefC.GetLong(PrefName.AllergiesIndicateNone))+") AS allergiesNone, "
                //  +"(SELECT COUNT(*) FROM allergy WHERE PatNum=patient.PatNum) AS allergiesAll "
                //  +"FROM patient "
                //  +"WHERE EXISTS(SELECT * FROM procedurelog WHERE patient.PatNum=procedurelog.PatNum "
                //  +"AND procedurelog.ProcStatus=2 "//complete
                //  +"AND procedurelog.ProvNum="+POut.Long(provNum)+" "
                //  +"AND procedurelog.ProcDate >= "+POut.Date(dateStart)+" "
                //  +"AND procedurelog.ProcDate <= "+POut.Date(dateEnd)+")";
                //Query optimized to be faster by Cameron
                command = "SELECT A.*,COALESCE(allergiesNone.Count,0) AS allergiesNone,COALESCE(allergiesAll.Count,0) AS allergiesAll "
                          + "FROM (SELECT patient.PatNum,LName,FName	FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum	AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) A "
                          + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM allergy	"
                          + "WHERE AllergyDefNum=" + SOut.Long(PrefC.GetLong(PrefName.AllergiesIndicateNone)) + " AND StatusIsActive=1 "
                          + "GROUP BY PatNum) allergiesNone ON allergiesNone.PatNum=A.PatNum "
                          + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM allergy	"
                          + "WHERE AllergyDefNum!=" + SOut.Long(PrefC.GetLong(PrefName.AllergiesIndicateNone)) + " "
                          + "GROUP BY PatNum) allergiesAll ON allergiesAll.PatNum=A.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region Demographics

            case EhrMeasureType.Demographics:
                //language, gender, race, ethnicity, and birthdate
                //Jordan's original query
                //command="SELECT PatNum,LName,FName,Birthdate,Gender,Race,Language "
                //  +"FROM patient "
                //  +"WHERE EXISTS(SELECT * FROM procedurelog WHERE patient.PatNum=procedurelog.PatNum "
                //  +"AND procedurelog.ProcStatus=2 "//complete
                //  +"AND procedurelog.ProvNum="+POut.Long(provNum)+" "
                //  +"AND procedurelog.ProcDate >= "+POut.Date(dateStart)+" "
                //  +"AND procedurelog.ProcDate <= "+POut.Date(dateEnd)+")";
                //Query optimized to be faster by Cameron
                //command="SELECT patient.PatNum,LName,FName,Birthdate,Gender,Race,Language "
                //	+"FROM patient "
                //	+"INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                //	+"AND procedurelog.ProvNum IN("+POut.String(provs)+")	"
                //	+"AND procedurelog.ProcDate BETWEEN "+POut.Date(dateStart)+" AND "+POut.Date(dateEnd)+" "
                //	+"GROUP BY patient.PatNum";
                command = "SELECT patient.PatNum,LName,FName,Birthdate,Gender,Language,COALESCE(race.HasRace,0) AS HasRace,COALESCE(ethnicity.HasEthnicity,0) AS HasEthnicity "
                          + "FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "LEFT JOIN(SELECT PatNum, 1 AS HasRace FROM patientrace "
                          + "LEFT JOIN cdcrec ON cdcrec.CdcrecCode=patientrace.CdcrecCode "
                          + "WHERE (cdcrec.CdcrecCode IS NOT NULL AND  cdcrec.HeirarchicalCode LIKE 'R%') " //Race Hierarchical Codes start with an 'R'
                          + "OR patientrace.CdcrecCode='" + SOut.String(PatientRace.DECLINE_SPECIFY_RACE_CODE) + "' "
                          + "GROUP BY PatNum "
                          + ") AS race ON race.PatNum=patient.PatNum "
                          + "LEFT JOIN(SELECT PatNum, 1 AS HasEthnicity FROM patientrace "
                          + "LEFT JOIN cdcrec ON cdcrec.CdcrecCode=patientrace.CdcrecCode "
                          + "WHERE (cdcrec.CdcrecCode IS NOT NULL AND cdcrec.HeirarchicalCode LIKE 'E%') " //Ethnicity Hierarchical Codes start with an 'E'
                          + "OR patientrace.CdcrecCode='" + SOut.String(PatientRace.DECLINE_SPECIFY_ETHNICITY_CODE) + "' "
                          + "GROUP BY PatNum "
                          + ") AS ethnicity ON ethnicity.PatNum=patient.PatNum "
                          + "GROUP BY patient.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region Education

            case EhrMeasureType.Education:
                //Jordan's original query
                //command="SELECT PatNum,LName,FName, "
                //  +"(SELECT COUNT(*) FROM ehrmeasureevent WHERE PatNum=patient.PatNum AND EventType="+POut.Int((int)EhrMeasureEventType.EducationProvided)+") AS edCount "
                //  +"FROM patient "
                //  +"WHERE EXISTS(SELECT * FROM procedurelog WHERE patient.PatNum=procedurelog.PatNum "
                //  +"AND procedurelog.ProcStatus=2 "//complete
                //  +"AND procedurelog.ProvNum="+POut.Long(provNum)+" "
                //  +"AND procedurelog.ProcDate >= "+POut.Date(dateStart)+" "
                //  +"AND procedurelog.ProcDate <= "+POut.Date(dateEnd)+")";
                //Query optimized to be faster by Cameron
                command = "SELECT A.*,COALESCE(edCount.Count,0) AS edCount "
                          + "FROM (SELECT patient.PatNum,LName,FName	FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) A "
                          + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM ehrmeasureevent "
                          + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.EducationProvided) + " "
                          + "GROUP BY PatNum) edCount ON edCount.PatNum=A.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region ElectronicCopyAccess

            case EhrMeasureType.ElectronicCopyAccess:
                command = "SELECT patient.PatNum,patient.LName,patient.FName,OnlineAccess.dateProvided,MIN(procedurelog.ProcDate) as leastRecentDate "
                          + "FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum "
                          + "AND procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.C) + " "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "LEFT JOIN (SELECT ehrmeasureevent.PatNum, ehrmeasureevent.DateTEvent as dateProvided "
                          + "FROM ehrmeasureevent "
                          + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.OnlineAccessProvided) + ") OnlineAccess ON patient.PatNum=OnlineAccess.PatNum "
                          + "GROUP BY patient.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region TimelyAccess (Deprecated)

            case EhrMeasureType.TimelyAccess:
                //denominator is patients
                command = "DROP TABLE IF EXISTS tempehrmeasure" + rndStr;
                Db.NonQ(command);
                command = "CREATE TABLE tempehrmeasure" + rndStr + @" (
						PatNum bigint NOT NULL auto_increment PRIMARY KEY,
						LName varchar(255) NOT NULL,
						FName varchar(255) NOT NULL,
						lastVisitDate date NOT NULL,
						deadlineDate date NOT NULL,
						accessProvided tinyint NOT NULL
						) DEFAULT CHARSET=utf8";
                Db.NonQ(command);
                //get all patients who have been seen during the period, along with the most recent visit date during the period
                command = "INSERT INTO tempehrmeasure" + rndStr + " (PatNum,LName,FName,lastVisitDate) SELECT patient.PatNum,LName,FName, "
                          + "MAX(procedurelog.ProcDate) "
                          + "FROM patient,procedurelog,procedurecode "
                          + "WHERE patient.PatNum=procedurelog.PatNum "
                          + "AND procedurelog.CodeNum=procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "AND procedurelog.ProcStatus=2 " //complete
                          //+"AND procedurelog.ProvNum="+POut.Long(provNum)+" "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate >= " + SOut.Date(dateStart) + " "
                          + "AND procedurelog.ProcDate <= " + SOut.Date(dateEnd) + " "
                          + "GROUP BY patient.PatNum";
                tableRaw = DataCore.GetTable(command);
                //calculate the deadlineDate
                command = "UPDATE tempehrmeasure" + rndStr + " "
                          + "SET deadlineDate = ADDDATE(lastVisitDate, INTERVAL 4 DAY)";
                Db.NonQ(command);
                command = "UPDATE tempehrmeasure" + rndStr + " "
                          + "SET deadlineDate = ADDDate(lastVisitDate, INTERVAL 2 DAY) " //add 2 more days for weekend
                          + "WHERE DAYOFWEEK(lastVisitDate) IN(3,4,5,6)"; //tues, wed, thur, fri
                Db.NonQ(command);
                //date provided could be any date before deadline date if there was more than one visit
                command = "UPDATE tempehrmeasure" + rndStr + ",ehrmeasureevent SET accessProvided = 1 "
                          + "WHERE ehrmeasureevent.PatNum=tempehrmeasure" + rndStr + ".PatNum "
                          + "AND EventType=" + SOut.Int((int) EhrMeasureEventType.OnlineAccessProvided) + " "
                          + "AND DATE(ehrmeasureevent.DateTEvent) <= deadlineDate";
                Db.NonQ(command);
                command = "SELECT * FROM tempehrmeasure" + rndStr;
                tableRaw = DataCore.GetTable(command);
                command = "DROP TABLE IF EXISTS tempehrmeasure" + rndStr;
                Db.NonQ(command);
                break;

            #endregion

            #region ProvOrderEntry

            case EhrMeasureType.ProvOrderEntry:
                //Jordan's original query
                //command="SELECT PatNum,LName,FName, "
                //  +"(SELECT COUNT(*) FROM medicationpat mp2 WHERE mp2.PatNum=patient.PatNum "
                //  +"AND mp2.PatNote != '' AND mp2.DateStart > "+POut.Date(new DateTime(1880,1,1))+") AS countOrders "
                //  +"FROM patient "
                //  +"WHERE EXISTS(SELECT * FROM procedurelog WHERE patient.PatNum=procedurelog.PatNum "//at least one procedure in the period
                //  +"AND procedurelog.ProcStatus=2 "//complete
                //  +"AND procedurelog.ProvNum="+POut.Long(provNum)+" "
                //  +"AND procedurelog.ProcDate >= "+POut.Date(dateStart)+" "
                //  +"AND procedurelog.ProcDate <= "+POut.Date(dateEnd)+") "
                //  +"AND EXISTS(SELECT * FROM medicationpat WHERE medicationpat.PatNum=patient.PatNum)";//at least one medication
                //Query optimized to be faster by Cameron
                //command="SELECT A.*,COALESCE(countOrders.Count,0) AS countOrders "
                //	+"FROM (SELECT patient.PatNum,LName,FName FROM patient "
                //	+"INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                //	+"AND procedurelog.ProvNum IN("+POut.String(provs)+")	"
                //	+"AND procedurelog.ProcDate BETWEEN "+POut.Date(dateStart)+" AND "+POut.Date(dateEnd)+" "
                //	+"INNER JOIN medicationpat ON medicationpat.PatNum=patient.PatNum "
                //	+"GROUP BY patient.PatNum) A "
                //	+"LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM medicationpat mp2 "
                //	+"WHERE mp2.PatNote!='' AND mp2.DateStart > "+POut.Date(new DateTime(1880,1,1))+" "
                //	+"GROUP BY PatNum) countOrders ON countOrders.PatNum=A.PatNum";
                //Now using IsCpoe flag instead of PatNote and DateStart to mark as an order
                command = "SELECT allpats.*,COALESCE(CountCpoe.Count,0) AS CountCpoe "
                          + "FROM (SELECT patient.PatNum,patient.LName,patient.FName FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "INNER JOIN medicationpat ON medicationpat.PatNum=patient.PatNum "
                          + "AND MedicationNum!=" + SOut.Long(PrefC.GetLong(PrefName.MedicationsIndicateNone)) + " "
                          + "GROUP BY patient.PatNum) allpats " //allpats seen by provider in date range with medication in med list that is not the 'None' medication
                          + "LEFT JOIN (SELECT medicationpat.PatNum,COUNT(*) AS 'Count' FROM medicationpat "
                          + "WHERE medicationpat.IsCpoe=1 GROUP BY medicationpat.PatNum) CountCpoe ON CountCpoe.PatNum=allpats.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region CPOE_MedOrdersOnly

            case EhrMeasureType.CPOE_MedOrdersOnly:
                //This optional alternate no longer counts patients with meds in med list, instead we will count the orders created by the Provider during the reporting period and what percentage are CPOE (meaning they were entered through NewCrop)
                command = "SELECT patient.PatNum,patient.LName,patient.FName,medicationpat.MedicationPatNum,"
                          + "COALESCE(medication.MedName,medicationpat.MedDescript) AS MedName,medicationpat.DateStart,"
                          + "medicationpat.IsCpoe FROM patient "
                          + "INNER JOIN medicationpat ON medicationpat.PatNum=patient.PatNum "
                          + "AND medicationpat.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND medicationpat.PatNote!='' "
                          + "AND medicationpat.DateStart BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "AND medicationpat.MedicationNum=0 " //MedicationNum is 0 when creating an Rx either manually or through NewCrop.  MedicationNum is not 0 when manually added through the medical window.
                          + "LEFT JOIN medication ON medication.MedicationNum=medicationpat.MedicationNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region CPOE_PreviouslyOrdered

            case EhrMeasureType.CPOE_PreviouslyOrdered:
                //For details regarding this optional alternate see: https://questions.cms.gov/faq.php?id=5005&faqId=3257, summmary: If you prescribe more than 100 meds during the reporting period, maintain medication lists that include meds the Provider did not order, and orders meds for less than 30% of patients with meds in med list during the reporting period, then the denominator can be limited to only those patients for whom the Provider has previously ordered meds.
                command = "SELECT allpatsprevordered.*,COALESCE(CountCpoe.Count,0) AS CountCpoe "
                          + "FROM (SELECT patient.PatNum,patient.LName,patient.FName FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ") "
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "INNER JOIN medicationpat ON medicationpat.PatNum=patient.PatNum "
                          + "AND medicationpat.MedicationNum=0 "; //MedicationNum is 0 when creating an Rx either manually or through NewCrop.  MedicationNum is not 0 when manually added through the medical window.
                //this next join limits to only patients for whom the provider has previously ordered medications
                command += "INNER JOIN (SELECT PatNum FROM medicationpat "
                           + "WHERE PatNote!='' AND DateStart > " + SOut.Date(new DateTime(1880, 1, 1)) + " "
                           + "AND ProvNum IN(" + SOut.String(provs) + ") AND MedicationNum=0 GROUP BY PatNum) prevordered ON prevordered.PatNum=patient.PatNum "
                           + "GROUP BY patient.PatNum) allpatsprevordered "
                           + "LEFT JOIN (SELECT medicationpat.PatNum,COUNT(*) AS 'Count' FROM medicationpat "
                           + "WHERE medicationpat.IsCpoe=1 AND medicationpat.MedicationNum=0 GROUP BY PatNum) CountCpoe ON CountCpoe.PatNum=allpatsprevordered.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region Rx

            case EhrMeasureType.Rx:
                command = "SELECT patient.PatNum,LName,FName,SendStatus,RxDate "
                          + "FROM rxpat,patient "
                          + "WHERE rxpat.PatNum=patient.PatNum "
                          + "AND IsControlled = 0 "
                          //+"AND rxpat.ProvNum="+POut.Long(provNum)+" "
                          + "AND rxpat.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND RxDate >= " + SOut.Date(dateStart) + " "
                          + "AND RxDate <= " + SOut.Date(dateEnd);
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region VitalSigns

            case EhrMeasureType.VitalSigns:
                //Jordan's original query
                //command="SELECT PatNum,LName,FName, "
                //  +"(SELECT COUNT(*) FROM vitalsign WHERE vitalsign.PatNum=patient.PatNum AND Height>0 AND Weight>0) AS hwCount, "
                //  +"(SELECT COUNT(*) FROM vitalsign WHERE vitalsign.PatNum=patient.PatNum AND BpSystolic>0 AND BpDiastolic>0) AS bpCount "
                //  +"FROM patient "
                //  +"WHERE EXISTS(SELECT * FROM procedurelog WHERE patient.PatNum=procedurelog.PatNum "
                //  +"AND procedurelog.ProcStatus=2 "//complete
                //  +"AND procedurelog.ProvNum="+POut.Long(provNum)+" "
                //  +"AND procedurelog.ProcDate >= "+POut.Date(dateStart)+" "
                //  +"AND procedurelog.ProcDate <= "+POut.Date(dateEnd)+") "
                //  +"AND patient.Birthdate <= "+POut.Date(DateTime.Today.AddYears(-2));//2 and older
                //Query optimized to be faster by Cameron
                //command="SELECT A.*,COALESCE(hwCount.Count,0) AS hwCount,COALESCE(bpCount.Count,0) AS bpCount "
                //	+"FROM (SELECT patient.PatNum,LName,FName FROM patient "
                //	+"INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                //	+"AND procedurelog.ProvNum IN("+POut.String(provs)+")	"
                //	+"AND procedurelog.ProcDate BETWEEN "+POut.Date(dateStart)+" AND "+POut.Date(dateEnd)+" "
                //	+"WHERE patient.Birthdate <= "+POut.Date(DateTime.Today.AddYears(-2))+" "//2 and older
                //	+"GROUP BY patient.PatNum) A "
                //	+"LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM vitalsign	WHERE Height>0 AND Weight>0 GROUP BY PatNum) hwCount ON hwCount.PatNum=A.PatNum "
                //	+"LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM vitalsign WHERE BpSystolic>0 AND BpDiastolic>0 GROUP BY PatNum) bpCount ON bpCount.PatNum=A.PatNum";
                //Query modified for new requirements (Optional 2013, Required 2014 and beyond).  BP 3 and older only, Height/Weight all ages
                command = "SELECT A.*,COALESCE(hwCount.Count,0) AS hwCount,COALESCE(bpCount.Count,0) AS bpCount "
                          + "FROM (SELECT patient.PatNum,LName,FName FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "WHERE patient.Birthdate <= " + SOut.Date(DateTime.Today.AddYears(-2)) + " " //2 and older
                          + "GROUP BY patient.PatNum) A "
                          + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM vitalsign	WHERE Height>0 AND Weight>0 GROUP BY PatNum) hwCount ON hwCount.PatNum=A.PatNum "
                          + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM vitalsign WHERE BpSystolic>0 AND BpDiastolic>0 GROUP BY PatNum) bpCount ON bpCount.PatNum=A.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region VitalSigns2014

            case EhrMeasureType.VitalSigns2014:
                command = "SELECT A.*,COALESCE(hwCount.Count,0) AS hwCount,"
                          + "(CASE WHEN A.Birthdate <= (A.LastVisitInDateRange-INTERVAL 3 YEAR) "; //BP count only if 3 and older at time of last visit in date range
                command += "THEN COALESCE(bpCount.Count,0) ELSE 1 END) AS bpCount "
                           + "FROM (SELECT patient.PatNum,LName,FName,Birthdate,MAX(procedurelog.ProcDate) AS LastVisitInDateRange "
                           + "FROM patient "
                           + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                           + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                           + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                           + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                           + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                           + "GROUP BY patient.PatNum) A "
                           + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM vitalsign	WHERE Height>0 AND Weight>0 GROUP BY PatNum) hwCount ON hwCount.PatNum=A.PatNum "
                           + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM vitalsign WHERE BpSystolic>0 AND BpDiastolic>0 GROUP BY PatNum) bpCount ON bpCount.PatNum=A.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region VitalSignsBMIOnly

            case EhrMeasureType.VitalSignsBMIOnly:
                command = "SELECT A.*,COALESCE(hwCount.Count,0) AS hwCount "
                          + "FROM (SELECT patient.PatNum,LName,FName FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) A "
                          + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM vitalsign	WHERE Height>0 AND Weight>0 GROUP BY PatNum) hwCount ON hwCount.PatNum=A.PatNum ";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region VitalSignsBPOnly

            case EhrMeasureType.VitalSignsBPOnly:
                command = "SELECT patient.PatNum,LName,FName,Birthdate,COUNT(DISTINCT VitalsignNum) AS bpcount "
                          + "FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum "
                          + "AND procedurelog.ProcStatus=2	AND procedurelog.ProvNum IN(" + SOut.String(provs) + ") "
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "LEFT JOIN vitalsign ON vitalsign.PatNum=patient.PatNum AND BpSystolic!=0 AND BpDiastolic!=0 "
                          + "GROUP BY patient.PatNum "
                          + "HAVING Birthdate<=MAX(ProcDate)-INTERVAL 3 YEAR "; //only include in results if over 3 yrs old at date of last visit
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region Smoking

            case EhrMeasureType.Smoking:
                //Jordan's original query
                //command="SELECT PatNum,LName,FName,SmokeStatus "
                //  +"FROM patient "
                //  +"WHERE EXISTS(SELECT * FROM procedurelog WHERE patient.PatNum=procedurelog.PatNum "
                //  +"AND procedurelog.ProcStatus=2 "//complete
                //  +"AND procedurelog.ProvNum="+POut.Long(provNum)+" "
                //  +"AND procedurelog.ProcDate >= "+POut.Date(dateStart)+" "
                //  +"AND procedurelog.ProcDate <= "+POut.Date(dateEnd)+") "
                //  +"AND patient.Birthdate <= "+POut.Date(DateTime.Today.AddYears(-13));//13 and older
                //Query optimized to be faster by Cameron
                command = "SELECT patient.PatNum,LName,FName,SmokingSnoMed FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ") "
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "WHERE patient.Birthdate <= " + SOut.Date(DateTime.Today.AddYears(-13)) + " " //13 and older
                          + "GROUP BY patient.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region Lab

            case EhrMeasureType.Lab:
                //Jordan's original query
                //command="SELECT patient.PatNum,LName,FName,DateTimeOrder, "
                //  +"(SELECT COUNT(*) FROM labpanel WHERE labpanel.MedicalOrderNum=medicalorder.MedicalOrderNum) AS panelCount "
                //  +"FROM medicalorder,patient "
                //  +"WHERE medicalorder.PatNum=patient.PatNum "
                //  +"AND MedOrderType="+POut.Int((int)MedicalOrderType.Laboratory)+" "
                //  +"AND medicalorder.ProvNum="+POut.Long(provNum)+" "
                //  +"AND DATE(DateTimeOrder) >= "+POut.Date(dateStart)+" "
                //  +"AND DATE(DateTimeOrder) <= "+POut.Date(dateEnd);
                //Query optimized to be faster by Cameron
                //TODO: Combine these queries to get old and new lab data
                command = "SELECT 1 AS IsOldLab,patient.PatNum,LName,FName,DateTimeOrder,COALESCE(panels.Count,0) AS ResultCount FROM patient "
                          + "INNER JOIN medicalorder ON patient.PatNum=medicalorder.PatNum "
                          + "AND MedOrderType=" + SOut.Int((int) MedicalOrderType.Laboratory) + " "
                          + "AND medicalorder.ProvNum IN(" + SOut.String(provs) + ") "
                          + "AND DATE(DateTimeOrder) BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "LEFT JOIN (SELECT MedicalOrderNum,COUNT(*) AS 'Count' FROM labpanel GROUP BY MedicalOrderNum "
                          + ") panels ON panels.MedicalOrderNum=medicalorder.MedicalOrderNum "
                          + "UNION ALL "
                          + "SELECT 0 AS IsOldLab,patient.PatNum,LName,FName,STR_TO_DATE(ObservationDateTimeStart,'%Y%m%d') AS DateTimeOrder,COALESCE(ehrlabs.Count,0) AS ResultCount FROM patient "
                          + "INNER JOIN ehrlab ON patient.PatNum=ehrlab.PatNum "
                          + "LEFT JOIN (SELECT EhrLabNum, COUNT(*) AS 'Count' FROM ehrlabresult "
                          + "WHERE ehrlabresult.ValueType='NM' OR ehrlabresult.ValueType='SN' "
                          + "OR ehrlabresult.ObservationValueCodedElementID IN (" + _snomedLabResult + ") "
                          + "OR ehrlabresult.ObservationValueCodedElementIDAlt IN (" + _snomedLabResult + ") "
                          + "GROUP BY EhrLabNum "
                          + ") ehrlabs ON ehrlab.EhrLabNum=ehrlabs.EhrLabNum "
                          + "WHERE (CASE WHEN ehrlab.OrderingProviderIdentifierTypeCode='NPI' THEN ehrlab.OrderingProviderID IN(" + SOut.String(provNPIs) + ") " //When the lab is using a NPI number to determine provider.
                          + "WHEN ehrlab.OrderingProviderIdentifierTypeCode='PRN' THEN ( " //When the lab is using provider number to determine provider.
                          + "CASE WHEN ehrlab.OrderingProviderAssigningAuthorityUniversalID=( " //If the AssigningAuthority is OpenDental.
                          + "SELECT IDRoot FROM oidinternal WHERE IDType='Provider' GROUP BY IDType "
                          + ") THEN ehrlab.OrderingProviderID IN(" + SOut.String(provs) + ") END) " //Use the ProvNum to determine provider.
                          + "ELSE FALSE END) " //If the AssigningAuthority is not OpenDental, we have no way to tell who the provider is.
                          + "AND ehrlab.ObservationDateTimeStart BETWEEN DATE_FORMAT(" + SOut.Date(dateStart) + ",'%Y%m%d') AND DATE_FORMAT(" + SOut.Date(dateEnd) + ",'%Y%m%d') "
                          + "AND (CASE WHEN ehrlab.UsiCodeSystemName='LN' THEN ehrlab.UsiID WHEN ehrlab.UsiCodeSystemNameAlt='LN' THEN ehrlab.UsiIDAlt ELSE '' END) "
                          + "NOT IN (SELECT LoincCode FROM loinc WHERE loinc.ClassType LIKE '%RAD%')";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region ElectronicCopy (Deprecated)

            case EhrMeasureType.ElectronicCopy:
                command = "DROP TABLE IF EXISTS tempehrmeasure" + rndStr;
                Db.NonQ(command);
                command = "CREATE TABLE tempehrmeasure" + rndStr + @" (
						TempEhrMeasureNum bigint NOT NULL auto_increment PRIMARY KEY,
						PatNum bigint NOT NULL,
						LName varchar(255) NOT NULL,
						FName varchar(255) NOT NULL,
						dateRequested date NOT NULL,
						dateDeadline date NOT NULL,
						copyProvided tinyint NOT NULL,
						INDEX(PatNum)
						) DEFAULT CHARSET=utf8";
                Db.NonQ(command);
                command = "INSERT INTO tempehrmeasure" + rndStr + " (PatNum,LName,FName,dateRequested) SELECT patient.PatNum,LName,FName,DATE(DateTEvent) "
                          + "FROM ehrmeasureevent,patient "
                          + "WHERE patient.PatNum=ehrmeasureevent.PatNum "
                          + "AND EventType=" + SOut.Int((int) EhrMeasureEventType.ElectronicCopyRequested) + " "
                          + "AND DATE(DateTEvent) >= " + SOut.Date(dateStart) + " "
                          + "AND DATE(DateTEvent) <= " + SOut.Date(dateEnd) + " "
                          //+"AND patient.PriProv="+POut.Long(provNum);
                          + "AND patient.PriProv IN(" + SOut.String(provs) + ")";
                Db.NonQ(command);
                command = "UPDATE tempehrmeasure" + rndStr + " "
                          + "SET dateDeadline = ADDDATE(dateRequested, INTERVAL 3 DAY)";
                Db.NonQ(command);
                command = "UPDATE tempehrmeasure" + rndStr + " "
                          + "SET dateDeadline = ADDDate(dateDeadline, INTERVAL 2 DAY) " //add 2 more days for weekend
                          + "WHERE DAYOFWEEK(dateRequested) IN(4,5,6)"; //wed, thur, fri
                Db.NonQ(command);
                command = "UPDATE tempehrmeasure" + rndStr + ",ehrmeasureevent SET copyProvided = 1 "
                          + "WHERE ehrmeasureevent.PatNum=tempehrmeasure" + rndStr + ".PatNum AND EventType=" + SOut.Int((int) EhrMeasureEventType.ElectronicCopyProvidedToPt) + " "
                          + "AND DATE(ehrmeasureevent.DateTEvent) >= dateRequested "
                          + "AND DATE(ehrmeasureevent.DateTEvent) <= dateDeadline";
                Db.NonQ(command);
                command = "SELECT * FROM tempehrmeasure" + rndStr;
                tableRaw = DataCore.GetTable(command);
                command = "DROP TABLE IF EXISTS tempehrmeasure" + rndStr;
                Db.NonQ(command);
                break;

            #endregion

            #region ClinicalSummaries

            case EhrMeasureType.ClinicalSummaries:
                //command="DROP TABLE IF EXISTS tempehrmeasure"+rndStr;
                //Db.NonQ(command);
                //command="CREATE TABLE tempehrmeasure"+rndStr+@" (
                //	TempEhrMeasureNum bigint NOT NULL auto_increment PRIMARY KEY,
                //	PatNum bigint NOT NULL,
                //	LName varchar(255) NOT NULL,
                //	FName varchar(255) NOT NULL,
                //	visitDate date NOT NULL,
                //	deadlineDate date NOT NULL,
                //	summaryProvided tinyint NOT NULL,
                //	INDEX(PatNum)
                //	) DEFAULT CHARSET=utf8";
                //Db.NonQ(command);
                //command="INSERT INTO tempehrmeasure"+rndStr+" (PatNum,LName,FName,visitDate) SELECT patient.PatNum,LName,FName,ProcDate "
                //	+"FROM procedurelog "
                //	+"LEFT JOIN patient ON patient.PatNum=procedurelog.PatNum "
                //	+"WHERE ProcDate >= "+POut.Date(dateStart)+" "
                //	+"AND ProcDate <= "+POut.Date(dateEnd)+" "
                //	//+"AND procedurelog.ProvNum="+POut.Long(provNum)+" "
                //	+"AND procedurelog.ProvNum IN("+POut.String(provs)+") "
                //	+"AND procedurelog.ProcStatus="+POut.Int((int)ProcStat.C)+" "
                //	+"GROUP BY procedurelog.PatNum,ProcDate";
                //Db.NonQ(command);
                //command="UPDATE tempehrmeasure"+rndStr+" "
                //	+"SET deadlineDate = ADDDATE(visitDate, INTERVAL 3 DAY)";
                //Db.NonQ(command);
                //command="UPDATE tempehrmeasure"+rndStr+" "
                //	+"SET DeadlineDate = ADDDate(deadlineDate, INTERVAL 2 DAY) "//add 2 more days for weekend
                //	+"WHERE DAYOFWEEK(visitDate) IN(4,5,6)";//wed, thur, fri
                //Db.NonQ(command);
                //command="UPDATE tempehrmeasure"+rndStr+",ehrmeasureevent SET summaryProvided = 1 "
                //	+"WHERE ehrmeasureevent.PatNum=tempehrmeasure"+rndStr+".PatNum AND EventType="+POut.Int((int)EhrMeasureEventType.ClinicalSummaryProvidedToPt)+" "
                //	+"AND DATE(ehrmeasureevent.DateTEvent) >= visitDate "
                //	+"AND DATE(ehrmeasureevent.DateTEvent) <= deadlineDate";
                //Db.NonQ(command);
                //command="SELECT * FROM tempehrmeasure"+rndStr;
                //tableRaw=DataCore.GetTable(command);
                //command="DROP TABLE IF EXISTS tempehrmeasure"+rndStr;
                //Db.NonQ(command);
                //We no longer have a TempEhrMeasureNum because it was never being used in this table.
                command = "SELECT Procs.*, (CASE WHEN ISNULL(ehrmeasureevent.EhrMeasureEventNum) THEN 0 ELSE 1 END) AS summaryProvided "
                          + "FROM ( "
                          + "SELECT patient.PatNum,LName,FName,ProcDate AS visitDate, "
                          + "(CASE WHEN DAYOFWEEK(ProcDate) IN (4,5,6) THEN " + DbHelper.DateAddDay("ProcDate", "5") + " ELSE " + DbHelper.DateAddDay("ProcDate", "3") + " END) AS 'DeadlineDate' "
                          + "FROM procedurelog "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "LEFT JOIN patient ON patient.PatNum=procedurelog.PatNum "
                          + "WHERE ProcDate >= " + SOut.Date(dateStart) + " "
                          + "AND ProcDate <= " + SOut.Date(dateEnd) + " "
                          + "AND procedurelog.ProvNum IN (" + SOut.String(provs) + ") "
                          + "AND procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.C) + " "
                          + "GROUP BY procedurelog.PatNum,ProcDate "
                          + ") Procs "
                          + "LEFT JOIN ehrmeasureevent ON ehrmeasureevent.PatNum=Procs.PatNum "
                          + "AND EventType=" + SOut.Int((int) EhrMeasureEventType.ClinicalSummaryProvidedToPt) + " "
                          + "AND " + DbHelper.DtimeToDate("ehrmeasureevent.DateTEvent") + " >= Procs.visitDate "
                          + "AND " + DbHelper.DtimeToDate("ehrmeasureevent.DateTEvent") + " <= Procs.deadlineDate "
                          + "GROUP BY Procs.PatNum,Procs.visitDate;";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region Reminders

            case EhrMeasureType.Reminders:
                //Jordan's original query
                //command="SELECT PatNum,LName,FName, "
                //  +"(SELECT COUNT(*) FROM ehrmeasureevent WHERE PatNum=patient.PatNum AND EventType="+POut.Int((int)EhrMeasureEventType.ReminderSent)+" "
                //  +"AND DATE(ehrmeasureevent.DateTEvent) >= "+POut.Date(dateStart)+" "
                //  +"AND DATE(ehrmeasureevent.DateTEvent) <= "+POut.Date(dateEnd)+" "
                //  +") AS reminderCount "
                //  +"FROM patient "
                //  +"WHERE patient.Birthdate > '1880-01-01' "//a birthdate is entered
                //  +"AND (patient.Birthdate > "+POut.Date(DateTime.Today.AddYears(-6))+" "//5 years or younger
                //  +"OR patient.Birthdate <= "+POut.Date(DateTime.Today.AddYears(-65))+") "//65+
                //  +"AND patient.PatStatus="+POut.Int((int)PatientStatus.Patient)+" "
                //  +"AND patient.PriProv="+POut.Long(provNum);
                //Query optimized to be faster by Cameron
                //command="SELECT patient.PatNum,LName,FName,COALESCE(reminderCount.Count,0) AS reminderCount FROM patient "
                //	+"LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM ehrmeasureevent "
                //	+"WHERE EventType="+POut.Int((int)EhrMeasureEventType.ReminderSent)+" "
                //	+"AND DATE(ehrmeasureevent.DateTEvent) BETWEEN "+POut.Date(dateStart)+" AND "+POut.Date(dateEnd)+" "
                //	+"GROUP BY PatNum) reminderCount ON reminderCount.PatNum=patient.PatNum "
                //	+"WHERE patient.Birthdate > '1880-01-01' "//a birthdate is entered
                //	+"AND (patient.Birthdate > "+POut.Date(DateTime.Today.AddYears(-6))+" "//5 years or younger
                //	+"OR patient.Birthdate <= "+POut.Date(DateTime.Today.AddYears(-65))+") "//65+
                //	+"AND patient.PatStatus="+POut.Int((int)PatientStatus.Patient)+" "
                //	+"AND patient.PriProv IN("+POut.String(provs)+")";
                //Query modified to only return patients that have been seen by any provider in the last 3 years based on dateStart of measurement period
                command = "SELECT patient.PatNum,LName,FName,COALESCE(reminderCount.Count,0) AS reminderCount FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum "
                          + "AND ProcStatus=2 AND ProcDate>" + SOut.Date(dateStart) + "-INTERVAL 3 YEAR "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "LEFT JOIN (SELECT ehrmeasureevent.PatNum,COUNT(*) AS 'Count' FROM ehrmeasureevent "
                          + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.ReminderSent) + " "
                          + "AND DATE(ehrmeasureevent.DateTEvent) BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "GROUP BY ehrmeasureevent.PatNum) reminderCount ON reminderCount.PatNum=patient.PatNum "
                          + "WHERE patient.Birthdate > '1880-01-01' " //a birthdate is entered
                          + "AND (patient.Birthdate > " + SOut.Date(dateStart) + "-INTERVAL 5 YEAR " //5 years or younger as of start of measurement period
                          + "OR patient.Birthdate <= " + SOut.Date(dateStart) + "-INTERVAL 65 YEAR) " //65+ as of start of measurement period
                          + "AND patient.PatStatus=" + SOut.Int((int) PatientStatus.Patient) + " "
                          + "AND patient.PriProv IN(" + SOut.String(provs) + ") "
                          + "GROUP BY patient.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region MedReconcile

            case EhrMeasureType.MedReconcile:
                //command="DROP TABLE IF EXISTS tempehrmeasure"+rndStr;
                //Db.NonQ(command);
                //command="CREATE TABLE tempehrmeasure"+rndStr+@" (
                //	PatNum bigint NOT NULL PRIMARY KEY,
                //	LName varchar(255) NOT NULL,
                //	FName varchar(255) NOT NULL,
                //	RefCount int NOT NULL,
                //	ReconcileCount int NOT NULL
                //	) DEFAULT CHARSET=utf8";
                //Db.NonQ(command);
                //command="INSERT INTO tempehrmeasure"+rndStr+" (PatNum,LName,FName,RefCount) SELECT patient.PatNum,LName,FName,COUNT(*) "
                //	+"FROM refattach,patient "
                //	+"WHERE patient.PatNum=refattach.PatNum "
                //	//+"AND patient.PriProv="+POut.Long(provNum)+" "
                //	+"AND patient.PriProv IN("+POut.String(provs)+") "
                //	+"AND RefDate >= "+POut.Date(dateStart)+" "
                //	+"AND RefDate <= "+POut.Date(dateEnd)+" "
                //	+"AND RefType="+POut.Int((int)ReferralType.RefFrom)+" AND IsTransitionOfCare=1 "
                //	+"GROUP BY refattach.PatNum";
                //Db.NonQ(command);
                //command="UPDATE tempehrmeasure"+rndStr+" "
                //	+"SET ReconcileCount = (SELECT COUNT(*) FROM ehrmeasureevent "
                //	+"WHERE ehrmeasureevent.PatNum=tempehrmeasure"+rndStr+".PatNum AND EventType="+POut.Int((int)EhrMeasureEventType.MedicationReconcile)+" "
                //	+"AND DATE(ehrmeasureevent.DateTEvent) >= "+POut.Date(dateStart)+" "
                //	+"AND DATE(ehrmeasureevent.DateTEvent) <= "+POut.Date(dateEnd)+")";
                //Db.NonQ(command);
                //command="SELECT * FROM tempehrmeasure"+rndStr;
                //tableRaw=DataCore.GetTable(command);
                //command="DROP TABLE IF EXISTS tempehrmeasure"+rndStr;
                //Db.NonQ(command);
                //Reworked to only count patients seen by this provider in the date range
                command = "SELECT ptsRefCnt.*,COALESCE(RecCount,0) AS ReconcileCount "
                          + "FROM (SELECT ptsSeen.*,COUNT(DISTINCT refattach.RefAttachNum) AS RefCount "
                          + "FROM (SELECT patient.PatNum,LName,FName FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum "
                          + "AND ProcStatus=2 AND ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) ptsSeen "
                          + "INNER JOIN refattach ON ptsSeen.PatNum=refattach.PatNum "
                          + "AND RefDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "AND RefType=" + SOut.Int((int) ReferralType.RefFrom) + " AND IsTransitionOfCare=1 "
                          + "GROUP BY ptsSeen.PatNum) ptsRefCnt "
                          + "LEFT JOIN (SELECT ehrmeasureevent.PatNum,COUNT(*) AS RecCount FROM ehrmeasureevent "
                          + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.MedicationReconcile) + " "
                          + "AND DATE(ehrmeasureevent.DateTEvent) BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "GROUP BY ehrmeasureevent.PatNum) ptsRecCount ON ptsRefCnt.PatNum=ptsRecCount.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region SummaryOfCare

            case EhrMeasureType.SummaryOfCare:
                //command="DROP TABLE IF EXISTS tempehrmeasure"+rndStr;
                //Db.NonQ(command);
                //command="CREATE TABLE tempehrmeasure"+rndStr+@" (
                //	PatNum bigint NOT NULL PRIMARY KEY,
                //	LName varchar(255) NOT NULL,
                //	FName varchar(255) NOT NULL,
                //	RefCount int NOT NULL,
                //	CcdCount int NOT NULL
                //	) DEFAULT CHARSET=utf8";
                //Db.NonQ(command);
                //command="INSERT INTO tempehrmeasure"+rndStr+" (PatNum,LName,FName,RefCount) SELECT patient.PatNum,LName,FName,COUNT(*) "
                //	+"FROM refattach,patient "
                //	+"WHERE patient.PatNum=refattach.PatNum "
                //	//+"AND patient.PriProv="+POut.Long(provNum)+" "
                //	+"AND patient.PriProv IN("+POut.String(provs)+") "
                //	+"AND RefDate >= "+POut.Date(dateStart)+" "
                //	+"AND RefDate <= "+POut.Date(dateEnd)+" "
                //	+"AND RefType="+POut.Int((int)ReferralType.RefTo)+" AND IsTransitionOfCare=1 "
                //	+"GROUP BY refattach.PatNum";
                //Db.NonQ(command);
                //command="UPDATE tempehrmeasure"+rndStr+" "
                //	+"SET CcdCount = (SELECT COUNT(*) FROM ehrmeasureevent "
                //	+"WHERE ehrmeasureevent.PatNum=tempehrmeasure"+rndStr+".PatNum AND EventType="+POut.Int((int)EhrMeasureEventType.SummaryOfCareProvidedToDr)+" "
                //	+"AND DATE(ehrmeasureevent.DateTEvent) >= "+POut.Date(dateStart)+" "
                //	+"AND DATE(ehrmeasureevent.DateTEvent) <= "+POut.Date(dateEnd)+")";
                //Db.NonQ(command);
                //command="SELECT * FROM tempehrmeasure"+rndStr;
                //tableRaw=DataCore.GetTable(command);
                //command="DROP TABLE IF EXISTS tempehrmeasure"+rndStr;
                //Db.NonQ(command);
                //Reworked to only count patients seen by this provider in the date range
                command = "SELECT patient.PatNum,patient.LName,patient.FName,refattach.RefDate, "
                          + "referral.FName AS RefFName,referral.LName AS RefLName,SUM(CASE WHEN ISNULL(socevent.FKey) THEN 0 ELSE 1 END) AS SOCSent "
                          + "FROM refattach "
                          + "INNER JOIN referral ON referral.ReferralNum=refattach.ReferralNum "
                          + "INNER JOIN patient ON patient.PatNum=refattach.PatNum "
                          + "LEFT JOIN ( "
                          + "SELECT ehrmeasureevent.FKey "
                          + "FROM ehrmeasureevent "
                          + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.SummaryOfCareProvidedToDr) + " "
                          + "AND DATE(ehrmeasureevent.DateTEvent) BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + ") socevent ON socevent.FKey=refattach.RefAttachNum "
                          + "WHERE RefDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "AND RefType=" + SOut.Int((int) ReferralType.RefTo) + " AND IsTransitionOfCare=1 "
                          + "AND refattach.ProvNum IN(" + SOut.String(provs) + ") "
                          + "GROUP BY refattach.RefAttachNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            default:
                throw new ApplicationException("Type not found: " + mtype);
        }

        //PatNum, PatientName, Explanation, and Met (X).
        var table = new DataTable("audit");
        DataRow row;
        table.Columns.Add("PatNum");
        table.Columns.Add("patientName");
        table.Columns.Add("explanation");
        table.Columns.Add("met"); //X or empty
        var rows = new List<DataRow>();
        Patient pat;
        string explanation;
        for (var i = 0; i < tableRaw.Rows.Count; i++)
        {
            row = table.NewRow();
            row["PatNum"] = tableRaw.Rows[i]["PatNum"].ToString();
            pat = new Patient();
            pat.LName = tableRaw.Rows[i]["LName"].ToString();
            pat.FName = tableRaw.Rows[i]["FName"].ToString();
            pat.Preferred = "";
            row["patientName"] = pat.GetNameLF();
            row["met"] = "";
            explanation = "";
            switch (mtype)
            {
                #region ProblemList

                case EhrMeasureType.ProblemList:
                    if (tableRaw.Rows[i]["problemsNone"].ToString() != "0")
                    {
                        explanation = "Problems indicated 'None'.";
                        row["met"] = "X";
                    }
                    else if (tableRaw.Rows[i]["problemsAll"].ToString() != "0")
                    {
                        explanation = "Problems entered: " + tableRaw.Rows[i]["problemsAll"];
                        row["met"] = "X";
                    }
                    else
                    {
                        //explanation="No Problems entered";
                        explanation = "No Problems entered with ICD-9 code or SNOMED code attached.";
                    }

                    break;

                #endregion

                #region MedicationList

                case EhrMeasureType.MedicationList:
                    if (tableRaw.Rows[i]["medsNone"].ToString() != "0")
                    {
                        explanation = "Medications indicated 'None'";
                        row["met"] = "X";
                    }
                    else if (tableRaw.Rows[i]["medsAll"].ToString() != "0")
                    {
                        explanation = "Medications entered: " + tableRaw.Rows[i]["medsAll"];
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = "No Medications entered";
                    }

                    break;

                #endregion

                #region AllergyList

                case EhrMeasureType.AllergyList:
                    if (tableRaw.Rows[i]["allergiesNone"].ToString() != "0")
                    {
                        explanation = "Allergies indicated 'None'";
                        row["met"] = "X";
                    }
                    else if (tableRaw.Rows[i]["allergiesAll"].ToString() != "0")
                    {
                        explanation = "Allergies entered: " + tableRaw.Rows[i]["allergiesAll"];
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = "No Allergies entered";
                    }

                    break;

                #endregion

                #region Demographics

                case EhrMeasureType.Demographics:
                    if (SIn.Date(tableRaw.Rows[i]["Birthdate"].ToString()).Year < 1880) explanation += "birthdate"; //missing
                    if (tableRaw.Rows[i]["Language"].ToString() == "")
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "language";
                    }

                    if (SIn.Int(tableRaw.Rows[i]["Gender"].ToString()) == (int) PatientGender.Unknown)
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "gender";
                    }

                    //if(PatientRaces.GetForPatient(PIn.Long(row["PatNum"].ToString())).Count==0) {
                    //	if(explanation!="") {
                    //		explanation+=", ";
                    //	}
                    //	explanation+="race, ethnicity";
                    //}
                    if (SIn.Int(tableRaw.Rows[i]["HasRace"].ToString()) == 0)
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "race";
                    }

                    if (SIn.Int(tableRaw.Rows[i]["HasEthnicity"].ToString()) == 0)
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "ethnicity";
                    }

                    if (explanation == "")
                    {
                        explanation = "All demographic elements recorded";
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = "Missing: " + explanation;
                    }

                    break;

                #endregion

                #region Education

                case EhrMeasureType.Education:
                    if (tableRaw.Rows[i]["edCount"].ToString() == "0")
                    {
                        explanation = "No education resources";
                    }
                    else
                    {
                        explanation = "Education resources provided";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region ElectronicCopyAccess

                case EhrMeasureType.ElectronicCopyAccess:
                    var recentDate = SIn.Date(tableRaw.Rows[i]["leastRecentDate"].ToString());
                    var deadlineDate = SIn.Date(tableRaw.Rows[i]["leastRecentDate"].ToString());
                    var providedDate = SIn.Date(tableRaw.Rows[i]["dateProvided"].ToString());
                    deadlineDate = deadlineDate.AddDays(4);
                    if (recentDate.DayOfWeek > DayOfWeek.Tuesday) deadlineDate = deadlineDate.AddDays(2);
                    if (providedDate <= deadlineDate && providedDate.Year > 1880)
                    {
                        explanation = "Online access provided before " + deadlineDate.ToShortDateString();
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = recentDate.ToShortDateString() + " no online access provided";
                    }

                    break;

                #endregion

                #region TimelyAccess (Deprecated)

                case EhrMeasureType.TimelyAccess:
                    var lastVisitDate = SIn.Date(tableRaw.Rows[i]["lastVisitDate"].ToString());
                    var deadlineDateOld = SIn.Date(tableRaw.Rows[i]["deadlineDate"].ToString());
                    if (tableRaw.Rows[i]["accessProvided"].ToString() == "0")
                    {
                        explanation = lastVisitDate.ToShortDateString() + " no online access provided";
                    }
                    else
                    {
                        explanation = "Online access provided before " + deadlineDateOld.ToShortDateString();
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region ProvOrderEntry

                case EhrMeasureType.ProvOrderEntry:
                case EhrMeasureType.CPOE_PreviouslyOrdered:
                    if (tableRaw.Rows[i]["countCpoe"].ToString() == "0")
                    {
                        explanation = "No medication order through CPOE";
                    }
                    else
                    {
                        explanation = "Medication order in CPOE";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region CPOE_MedOrdersOnly

                case EhrMeasureType.CPOE_MedOrdersOnly:
                    var medOrderStartDate = SIn.Date(tableRaw.Rows[i]["DateStart"].ToString());
                    explanation = "Medication order: " + tableRaw.Rows[i]["MedName"] + ", start date: " + medOrderStartDate.ToShortDateString() + ".";
                    if (tableRaw.Rows[i]["IsCpoe"].ToString() == "1") row["met"] = "X";
                    break;

                #endregion

                #region Rx

                case EhrMeasureType.Rx:
                    var sendStatus = (RxSendStatus) SIn.Int(tableRaw.Rows[i]["SendStatus"].ToString());
                    var rxDate = SIn.Date(tableRaw.Rows[i]["rxDate"].ToString());
                    if (sendStatus == RxSendStatus.SentElect)
                    {
                        explanation = rxDate.ToShortDateString() + " Rx sent electronically.";
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = rxDate.ToShortDateString() + " Rx not sent electronically.";
                    }

                    break;

                #endregion

                #region VitalSigns

                case EhrMeasureType.VitalSigns:
                    if (tableRaw.Rows[i]["hwCount"].ToString() == "0") explanation += "height, weight";
                    if (tableRaw.Rows[i]["bpCount"].ToString() == "0")
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "blood pressure";
                    }

                    if (explanation == "")
                    {
                        explanation = "Vital signs entered";
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = "Missing: " + explanation;
                    }

                    break;

                #endregion

                #region VitalSigns2014

                case EhrMeasureType.VitalSigns2014:
                    if (tableRaw.Rows[i]["hwCount"].ToString() == "0") explanation += "height, weight";
                    if (tableRaw.Rows[i]["bpCount"].ToString() == "0")
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "blood pressure";
                    }

                    if (explanation == "")
                    {
                        explanation = "Vital signs entered";
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = "Missing: " + explanation;
                    }

                    break;

                #endregion

                #region VitalSignsBMIOnly

                case EhrMeasureType.VitalSignsBMIOnly:
                    if (tableRaw.Rows[i]["hwCount"].ToString() == "0") explanation += "height, weight";
                    if (explanation == "")
                    {
                        explanation = "Vital signs entered";
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = "Missing: " + explanation;
                    }

                    break;

                #endregion

                #region VitalSignsBPOnly

                case EhrMeasureType.VitalSignsBPOnly:
                    if (tableRaw.Rows[i]["bpCount"].ToString() == "0")
                    {
                        explanation = "Missing: blood pressure";
                    }
                    else
                    {
                        explanation = "Vital signs entered";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region Smoking

                case EhrMeasureType.Smoking:
                    var smokeSnoMed = tableRaw.Rows[i]["SmokingSnoMed"].ToString();
                    if (smokeSnoMed == "449868002" //Current every day smoker
                        || smokeSnoMed == "428041000124106" //Current some day smoker
                        || smokeSnoMed == "8517006" //Former smoker
                        || smokeSnoMed == "266919005" //Never smoker
                        || smokeSnoMed == "77176002" //Smoker, current status unknown
                        || smokeSnoMed == "266927001" //Unknown if ever smoked
                        || smokeSnoMed == "428071000124103" //Heavy tobacco smoker
                        || smokeSnoMed == "428061000124105") //Light tobacco smoker
                    {
                        explanation = "Smoking status entered.";
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation += "Smoking status not entered.";
                    }

                    break;

                #endregion

                #region Lab

                case EhrMeasureType.Lab:
                    var resultCount = SIn.Int(tableRaw.Rows[i]["ResultCount"].ToString());
                    var isOldLab = SIn.Bool(tableRaw.Rows[i]["IsOldLab"].ToString());
                    var dateOrder = SIn.Date(tableRaw.Rows[i]["DateTimeOrder"].ToString());
                    if (resultCount == 0)
                    {
                        explanation += dateOrder.ToShortDateString() + " results not attached.";
                        explanation += isOldLab ? " (2011 edition)" : "";
                    }
                    else
                    {
                        explanation = dateOrder.ToShortDateString() + " results attached.";
                        explanation += isOldLab ? " (2011 edition)" : "";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region ElectronicCopy (Deprecated)

                case EhrMeasureType.ElectronicCopy:
                    var dateRequested = SIn.Date(tableRaw.Rows[i]["dateRequested"].ToString());
                    if (tableRaw.Rows[i]["copyProvided"].ToString() == "0")
                    {
                        explanation = dateRequested.ToShortDateString() + " no copy provided to patient";
                    }
                    else
                    {
                        explanation = dateRequested.ToShortDateString() + " copy provided to patient";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region ClinicalSummaries

                case EhrMeasureType.ClinicalSummaries:
                    var visitDate = SIn.Date(tableRaw.Rows[i]["visitDate"].ToString());
                    if (tableRaw.Rows[i]["summaryProvided"].ToString() == "0")
                    {
                        explanation = visitDate.ToShortDateString() + " no summary provided to patient";
                    }
                    else
                    {
                        explanation = visitDate.ToShortDateString() + " summary provided to patient";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region Reminders

                case EhrMeasureType.Reminders:
                    if (tableRaw.Rows[i]["reminderCount"].ToString() == "0")
                    {
                        explanation = "No reminders sent";
                    }
                    else
                    {
                        explanation = "Reminders sent";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region MedReconcile

                case EhrMeasureType.MedReconcile:
                    var refCount = SIn.Int(tableRaw.Rows[i]["RefCount"].ToString()); //this will always be greater than zero
                    var reconcileCount = SIn.Int(tableRaw.Rows[i]["ReconcileCount"].ToString());
                    if (reconcileCount < refCount)
                    {
                        explanation = "Transitions of Care:" + refCount + ", Reconciles:" + reconcileCount;
                    }
                    else
                    {
                        explanation = "Reconciles performed for each transition of care.";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region SummaryOfCare

                case EhrMeasureType.SummaryOfCare:
                    var socSent = SIn.Int(tableRaw.Rows[i]["SOCSent"].ToString());
                    var refDate = SIn.DateTime(tableRaw.Rows[i]["RefDate"].ToString());
                    var refLName = SIn.String(tableRaw.Rows[i]["RefLName"].ToString());
                    var refFName = SIn.String(tableRaw.Rows[i]["RefFName"].ToString());
                    if (socSent < 1)
                    {
                        explanation = "Referral on: " + refDate.ToShortDateString() + " to " + refLName + ", " + refFName + " not sent summary of care.";
                    }
                    else
                    {
                        explanation = "Referral on: " + refDate.ToShortDateString() + " to " + refLName + ", " + refFName + " sent summary of care.";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                default:
                    throw new ApplicationException("Type not found: " + mtype);
            }

            row["explanation"] = explanation;
            rows.Add(row);
        }

        for (var i = 0; i < rows.Count; i++) table.Rows.Add(rows[i]);
        return table;
    }

    ///<summary>Just counts up the number of rows with an X in the met column.  Very simple.</summary>
    public static int CalcNumerator(DataTable table)
    {
        var retVal = 0;
        for (var i = 0; i < table.Rows.Count; i++)
            if (table.Rows[i]["met"].ToString() == "X")
                retVal++;

        return retVal;
    }

    ///<summary>Returns the explanation of the numerator based on the EHR certification documents.</summary>
    private static string GetNumeratorExplain(EhrMeasureType mtype)
    {
        switch (mtype)
        {
            case EhrMeasureType.ProblemList:
                return "Patients with at least one problem list entry or an indication of 'None' on problem list.";
            //Leaving original wording so change will not require re-testing to meet 2011 certification.  The wording below may be used in 2014 MU 1 as it is more accurate.
            //return "Patients with at least one problem entered with an ICD-9 code or SNOMED code attached or an indication of 'None' in their problem list.";
            case EhrMeasureType.MedicationList:
                return "Patients with at least one medication list entry or an indication of 'None' on medication list.";
            case EhrMeasureType.AllergyList:
                return "Patients with at least one allergy list entry or an indication of 'None' on allergy list.";
            case EhrMeasureType.Demographics:
                return "Patients with all required demographic elements recorded as structured data: language, gender, race, ethnicity, and birthdate.";
            case EhrMeasureType.Education:
                return "Patients provided patient-specific education resources, not dependent on requests.";
            case EhrMeasureType.ElectronicCopyAccess:
                return "Electronic copy received within 4 business days.";
            case EhrMeasureType.TimelyAccess:
                return "Electronic access of health information provided to seen patients within 4 business days of being entered into their EHR, not dependent on requests.";
            case EhrMeasureType.ProvOrderEntry:
                return "Patients with a medication order entered using CPOE.";
            case EhrMeasureType.CPOE_MedOrdersOnly:
                return "The number of medication orders entered by the Provider during the reporting period using CPOE.";
            case EhrMeasureType.CPOE_PreviouslyOrdered:
                return "Patients with a medication order entered using CPOE.";
            case EhrMeasureType.Rx:
                return "Permissible prescriptions transmitted electronically.";
            case EhrMeasureType.VitalSigns:
                return "Patients with height, weight, and blood pressure recorded.";
            case EhrMeasureType.VitalSigns2014:
                return "Patients with height, weight, and blood pressure recorded.";
            case EhrMeasureType.VitalSignsBMIOnly:
                return "Patients with height and weight recorded.";
            case EhrMeasureType.VitalSignsBPOnly:
                return "Patients with blood pressure recorded.";
            case EhrMeasureType.Smoking:
                return "Patients with smoking status recorded.";
            case EhrMeasureType.Lab:
                return "Lab results entered.";
            case EhrMeasureType.ElectronicCopy:
                return "Electronic copy received within 3 business days.";
            case EhrMeasureType.ClinicalSummaries:
                return "Clinical summaries of office visits provided to patients within 3 business days, not dependent on requests.";
            case EhrMeasureType.Reminders:
                return "Appropriate reminders sent during the reporting period.";
            case EhrMeasureType.MedReconcile:
                return "Medication reconciliation was performed for each transition of care.";
            case EhrMeasureType.SummaryOfCare:
                return "Summary of care record was provided for each transition or referral.";
        }

        throw new ApplicationException("Type not found: " + mtype);
    }

    ///<summary>Returns the explanation of the denominator based on the EHR certification documents.</summary>
    private static string GetDenominatorExplain(EhrMeasureType mtype)
    {
        switch (mtype)
        {
            case EhrMeasureType.ProblemList:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.MedicationList:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.AllergyList:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.Demographics:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.Education:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.TimelyAccess:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.ElectronicCopyAccess:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.ProvOrderEntry:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period and with at least one medication in their medication list.";
            case EhrMeasureType.CPOE_MedOrdersOnly:
                return "The number of medication orders created by the Provider during the reporting period.";
            case EhrMeasureType.CPOE_PreviouslyOrdered:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period, with at least one medication in their medication list, and for whom the Provider has previously ordered medications.";
            case EhrMeasureType.Rx:
                return "All permissible prescriptions by the Provider during the reporting period.";
            case EhrMeasureType.VitalSigns:
                return "All unique patients (age 3 and over for blood pressure) with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.VitalSigns2014:
                return "All unique patients (age 3 and over for blood pressure) with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.VitalSignsBMIOnly:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.VitalSignsBPOnly:
                return "All unique patients age 3 and over with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.Smoking:
                return "All unique patients 13 years or older with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.Lab:
                return "All lab orders by the Provider during the reporting period.";
            case EhrMeasureType.ElectronicCopy:
                return "All requests for electronic copies of health information during the reporting period.";
            case EhrMeasureType.ClinicalSummaries:
                return "All office visits during the reporting period.  An office visit is calculated as any number of completed procedures by the Provider for a given date.";
            case EhrMeasureType.Reminders:
                //return "All unique patients of the Provider 65+ or 5-.  Not restricted to those seen during the reporting period.  Must have status of Patient rather than Inactive, Nonpatient, Deceased, etc.";
                return "All unique patients of the Provider 65+ or 5-.  Must have status of Patient rather than Inactive, Nonpatient, Deceased, etc.";
            case EhrMeasureType.MedReconcile:
                return "Number of incoming transitions of care from another provider during the reporting period.";
            case EhrMeasureType.SummaryOfCare:
                return "Number of outgoing transitions of care and referrals during the reporting period.";
        }

        throw new ApplicationException("Type not found: " + mtype);
    }

    ///<summary>Returns the explanation of the exclusion if there is one, if none returns 'No exclusions.'.</summary>
    private static string GetExclusionExplain(EhrMeasureType mtype)
    {
        switch (mtype)
        {
            case EhrMeasureType.ProblemList:
            case EhrMeasureType.MedicationList:
            case EhrMeasureType.AllergyList:
            case EhrMeasureType.Demographics:
            case EhrMeasureType.Education:
                return "No exclusions.";
            case EhrMeasureType.ElectronicCopyAccess:
                return "Any Provider who neither orders nor creates any of the information listed for inclusion as part of both measures, except for Patient name and Provider's name and office contact information.";
            case EhrMeasureType.TimelyAccess:
                return "Any Provider that neither orders nor creates lab tests or information that would be contained in the problem list, medication list, or medication allergy list during the reporting period.";
            case EhrMeasureType.ProvOrderEntry:
            case EhrMeasureType.CPOE_MedOrdersOnly:
            case EhrMeasureType.CPOE_PreviouslyOrdered:
                return "Any Provider who writes fewer than 100 prescriptions during the reporting period.";
            case EhrMeasureType.Rx:
                return @"1. Any Provider who writes fewer than 100 prescriptions during the reporting period.
2. Any Provider who does not have a pharmacy within their organization and there are no pharmacies that accept electronic prescriptions within 10 miles of the practice at the start of the reporting period.";
            case EhrMeasureType.VitalSigns:
                return @"1. Any Provider who sees no patients 3 years or older is excluded from recording blood pressure.
2. Any Provider who believes that all three vital signs of height, weight, and blood pressure have no relevance to their scope of practice is excluded from recording them.
3. Any Provider who believes that height and weight are relevant to their scope of practice, but blood pressure is not, is excluded from recording blood pressure.
4. Any Provider who believes that blood pressure is relevant to their scope of practice, but height and weight are not, is excluded from recording height and weight.";
            case EhrMeasureType.VitalSigns2014:
                return @"1. Any Provider who sees no patients 3 years or older is excluded from recording blood pressure.
2. Any Provider who believes that all three vital signs of height, weight, and blood pressure have no relevance to their scope of practice is excluded from recording them.
3. Any Provider who believes that height and weight are relevant to their scope of practice, but blood pressure is not, is excluded from recording blood pressure.
4. Any Provider who believes that blood pressure is relevant to their scope of practice, but height and weight are not, is excluded from recording height and weight.";
            case EhrMeasureType.VitalSignsBMIOnly:
                return "Any Provider who believes that height and weight are not relevant to their scope of practice is excluded from recording them.";
            case EhrMeasureType.VitalSignsBPOnly:
                return @"1. Any Provider who sees no patients 3 years or older is excluded from recording blood pressure.
2. Any Provider who believes that blood pressure is not relevant to their scope of practice is excluded from recording it.";
            case EhrMeasureType.Smoking:
                return "Any Provider who sees no patients 13 years or older during the reporting period.";
            case EhrMeasureType.Lab:
                return "Any Provider who orders no lab tests whose results are either in a positive/negative or numeric format during the reporting period.";
            case EhrMeasureType.ElectronicCopy:
                return "Any Provider who has no requests from patients or their agents for an electronic copy of patient health information during the reporting period.";
            case EhrMeasureType.ClinicalSummaries:
                return "Any Provider who has no completed procedures during the reporting period.";
            case EhrMeasureType.Reminders:
                return "Any Provider who has no patients 65 years or older or 5 years or younger.";
            case EhrMeasureType.MedReconcile:
                return "Any Provider who was not the recipient of any transitions of care during the reporting period.";
            case EhrMeasureType.SummaryOfCare:
                return "Any Provider who neither transfers a patient to another setting nor refers a patient to another provider during the reporting period.";
        }

        throw new ApplicationException("Type not found: " + mtype);
    }

    /// <summary>
    ///     Returns the count the office will need to report in order to attest to being excluded from this measure.  Will
    ///     return -1 if there is no applicable count for this measure.
    /// </summary>
    private static int GetExclusionCount(EhrMeasureType mtype, DateTime dateStart, DateTime dateEnd, long provNum)
    {
        var retval = 0;
        var command = "";
        var tableRaw = new DataTable();
        var provCur = Providers.GetProv(provNum);
        command = "SELECT GROUP_CONCAT(provider.ProvNum) FROM provider WHERE provider.ProvNum IN "
                  + "(SELECT ProvNum FROM provider pv, ehrprovkey epk WHERE pv.LName=epk.LName AND pv.FName=epk.FName "
                  + "AND epk.LName='" + SOut.String(provCur.LName) + "' AND epk.FName='" + SOut.String(provCur.FName) + "')";
        var provs = DataCore.GetScalar(command);
        switch (mtype)
        {
            case EhrMeasureType.ProblemList:
            case EhrMeasureType.MedicationList:
            case EhrMeasureType.AllergyList:
            case EhrMeasureType.Demographics:
            case EhrMeasureType.Education:
            case EhrMeasureType.ElectronicCopyAccess:
            case EhrMeasureType.VitalSignsBMIOnly:
            case EhrMeasureType.ElectronicCopy:
            case EhrMeasureType.Lab:
            case EhrMeasureType.MedReconcile:
            case EhrMeasureType.SummaryOfCare:
                return retval = -1;

            #region TimelyAccess

            case EhrMeasureType.TimelyAccess:
                //Exlcuded if no lab tests are ordered or created for patients seen in reporting period
                command = "SELECT COUNT(*) AS 'Count' "
                          + "FROM (SELECT patient.PatNum	FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum	AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) A "
                          + "INNER JOIN medicalorder ON A.PatNum=medicalorder.PatNum "
                          + "AND MedOrderType=" + SOut.Int((int) MedicalOrderType.Laboratory) + " "
                          + "AND medicalorder.ProvNum IN(" + SOut.String(provs) + ") "
                          + "AND DATE(DateTimeOrder) BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " ";
                retval += SIn.Int(Db.GetCount(command));
                //Excluded if problems, medications, or medication allergy information is not ordered or created for patients seen in the reporting period
                command = "SELECT SUM(COALESCE(allergies.Count,0)+COALESCE(problems.Count,0)+COALESCE(meds.Count,0)) AS 'Count' "
                          + "FROM (SELECT patient.PatNum	FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum	AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) A ";
                //left join allergies with DateTStamp within reporting period
                command += "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM allergy "
                           + "WHERE " + DbHelper.DtimeToDate("DateTStamp") + " BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                           + "GROUP BY PatNum) allergies ON allergies.PatNum=A.PatNum ";
                //left join problems with DateTStamp within reporting period
                command += "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM disease "
                           + "WHERE " + DbHelper.DtimeToDate("DateTStamp") + " BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                           + "GROUP BY PatNum) problems ON problems.PatNum=A.PatNum ";
                //left join medications with DateStart or DateTStamp within reporting period
                command += "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM medicationpat "
                           + "WHERE DateStart BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                           + "OR " + DbHelper.DtimeToDate("DateTStamp") + " BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                           + "GROUP BY PatNum) meds ON meds.PatNum=A.PatNum";
                return retval += SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region CPOE_Rx

            case EhrMeasureType.ProvOrderEntry:
            case EhrMeasureType.CPOE_MedOrdersOnly:
            case EhrMeasureType.CPOE_PreviouslyOrdered:
            case EhrMeasureType.Rx:
                //Excluded if Provider writes fewer than 100 Tx's during the reporting period
                command = "SELECT COUNT(DISTINCT rxpat.RxNum) AS 'Count' "
                          + "FROM patient "
                          + "INNER JOIN rxpat ON rxpat.PatNum=patient.PatNum "
                          + "AND rxpat.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND RxDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd);
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region VitalSigns

            case EhrMeasureType.VitalSigns:
            case EhrMeasureType.VitalSigns2014:
            case EhrMeasureType.VitalSignsBPOnly:
                //Excluded if Provider sees no patients 3 years or older at the time of their last visit in reporting period.
                command = "SELECT SUM((CASE WHEN A.Birthdate <= (A.LastVisitInDateRange-INTERVAL 3 YEAR) THEN 1 ELSE 0 END)) AS 'Count' "
                          + "FROM (SELECT Birthdate,MAX(procedurelog.ProcDate) AS LastVisitInDateRange "
                          + "FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) A ";
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region Smoking

            case EhrMeasureType.Smoking:
                //Excluded if Provider sees no patients 13 years or older at the time of their last visit in reporting period.
                command = "SELECT SUM((CASE WHEN A.Birthdate <= (A.LastVisitInDateRange-INTERVAL 13 YEAR) THEN 1 ELSE 0 END)) AS 'Count' "
                          + "FROM (SELECT Birthdate,MAX(procedurelog.ProcDate) AS LastVisitInDateRange "
                          + "FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) A ";
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region ClinicalSummaries

            case EhrMeasureType.ClinicalSummaries:
                //Excluded if no completed procedures during the reporting period
                command = "SELECT COUNT(DISTINCT ProcNum) FROM procedurelog "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "WHERE ProcStatus=2 AND ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') ";
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region Reminders

            case EhrMeasureType.Reminders:
                //Excluded if Provider sees no patients 65 years or older or 5 years or younger at the time of their last visit in reporting period.
                command = "SELECT SUM((CASE WHEN (A.Birthdate > (A.LastVisitInDateRange-INTERVAL 6 YEAR) "; //6th birthday had not happened by date of last visit, 5 years or younger
                command += "OR A.Birthdate <= (A.LastVisitInDateRange-INTERVAL 65 YEAR)) "; //had 65th birthday by date of last vist, 65 or older
                command += "THEN 1 ELSE 0 END)) AS 'Count' "
                           + "FROM (SELECT Birthdate,MAX(procedurelog.ProcDate) AS LastVisitInDateRange "
                           + "FROM patient "
                           + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                           + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                           + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                           + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                           + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                           + "GROUP BY patient.PatNum) A ";
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion
        }

        throw new ApplicationException("Type not found: " + mtype);
    }

    /// <summary>
    ///     Returns the description of what the count displayed is.  May be count of patients under a certain age or
    ///     number of Rx's written, this will be the label that describes the number.
    /// </summary>
    private static string GetExclusionCountDescript(EhrMeasureType mtype)
    {
        switch (mtype)
        {
            case EhrMeasureType.ProblemList:
            case EhrMeasureType.MedicationList:
            case EhrMeasureType.AllergyList:
            case EhrMeasureType.Demographics:
            case EhrMeasureType.Education:
            case EhrMeasureType.ElectronicCopyAccess:
            case EhrMeasureType.VitalSignsBMIOnly:
            case EhrMeasureType.ElectronicCopy:
            case EhrMeasureType.Lab:
            case EhrMeasureType.MedReconcile:
            case EhrMeasureType.SummaryOfCare:
                return "";
            case EhrMeasureType.TimelyAccess:
                return "Count of lab orders, problems, medications, and medication allergies entered during the reporting period.";
            case EhrMeasureType.ProvOrderEntry:
            case EhrMeasureType.CPOE_MedOrdersOnly:
            case EhrMeasureType.CPOE_PreviouslyOrdered:
            case EhrMeasureType.Rx:
                return "Count of prescriptions entered during the reporting period.";
            case EhrMeasureType.VitalSigns:
            case EhrMeasureType.VitalSigns2014:
            case EhrMeasureType.VitalSignsBPOnly:
                return "Count of patients seen who were 3 years or older at the time of their last visit during the reporting period.";
            case EhrMeasureType.Smoking:
                return "Count of patients seen who were 13 years or older at the time of their last visit during the reporting period.";
            case EhrMeasureType.ClinicalSummaries:
                return "Count of procedures completed during the reporting period.";
            case EhrMeasureType.Reminders:
                return "Count of patients 65 years or older or 5 years or younger at the time of their last visit during the reporting period.";
        }

        throw new ApplicationException("Type not found: " + mtype);
    }

    /// <summary>
    ///     Only called from FormEHR to load the patient specific MU data and tell the user what action to take to get
    ///     closer to meeting MU.
    /// </summary>
    public static List<EhrMu> GetMu(Patient pat)
    {
        var list = new List<EhrMu>();
        //add one of each type
        EhrMu mu;
        string explanation;
        var retVal = GetMUList();
        var medList = MedicationPats.Refresh(pat.PatNum, true);
        var listMeasureEvents = EhrMeasureEvents.Refresh(pat.PatNum);
        var listRefAttach = RefAttaches.Refresh(pat.PatNum);
        for (var i = 0; i < retVal.Count; i++)
        {
            mu = new EhrMu();
            mu.Met = MuMet.False;
            mu.MeasureType = retVal[i].MeasureType;
            switch (mu.MeasureType)
            {
                #region ProblemList

                case EhrMeasureType.ProblemList:
                    var listDisease = Diseases.Refresh(pat.PatNum);
                    var validDiseaseCount = 0;
                    if (listDisease.Count == 0)
                    {
                        mu.Details = "No problems entered.";
                    }
                    else
                    {
                        var diseasesNone = false;
                        if (listDisease.Count == 1 && listDisease[0].DiseaseDefNum == PrefC.GetLong(PrefName.ProblemsIndicateNone)) diseasesNone = true;
                        if (diseasesNone)
                        {
                            mu.Met = MuMet.True;
                            mu.Details = "Problems marked 'none'.";
                        }
                        else
                        {
                            for (var m = 0; m < listDisease.Count; m++)
                            {
                                var diseaseCur = DiseaseDefs.GetItem(listDisease[m].DiseaseDefNum);
                                if (diseaseCur.ICD9Code == "" && diseaseCur.SnomedCode == "") continue;
                                validDiseaseCount++;
                            }

                            if (validDiseaseCount == 0)
                            {
                                mu.Details = "No problems with ICD-9 or Snomed code entered.";
                            }
                            else
                            {
                                mu.Met = MuMet.True;
                                mu.Details = "Problems with ICD-9 or Snomed code entered: " + validDiseaseCount;
                            }
                        }
                    }

                    mu.Action = "Enter problems";
                    break;

                #endregion

                #region MedicationList

                case EhrMeasureType.MedicationList:
                    if (medList.Count == 0)
                    {
                        mu.Details = "No medications entered.";
                    }
                    else
                    {
                        mu.Met = MuMet.True;
                        var medsNone = false;
                        if (medList.Count == 1 && medList[0].MedicationNum == PrefC.GetLong(PrefName.MedicationsIndicateNone)) medsNone = true;
                        if (medsNone)
                            mu.Details = "Medications marked 'none'.";
                        else
                            mu.Details = "Medications entered: " + medList.Count;
                    }

                    mu.Action = "Enter medications";
                    break;

                #endregion

                #region AllergyList

                case EhrMeasureType.AllergyList:
                    var listAllergies = Allergies.Refresh(pat.PatNum);
                    if (listAllergies.Count == 0)
                    {
                        mu.Details = "No allergies entered.";
                    }
                    else
                    {
                        mu.Met = MuMet.True;
                        var allergiesNone = false;
                        if (listAllergies.Count == 1 && listAllergies[0].AllergyDefNum == PrefC.GetLong(PrefName.AllergiesIndicateNone)) allergiesNone = true;
                        if (allergiesNone)
                            mu.Details = "Allergies marked 'none'.";
                        else
                            mu.Details = "Allergies entered: " + listAllergies.Count;
                    }

                    mu.Action = "Enter allergies";
                    break;

                #endregion

                #region Demographics

                case EhrMeasureType.Demographics:
                    explanation = "";
                    if (pat.Birthdate.Year < 1880) explanation += "birthdate"; //missing
                    if (pat.Language == "")
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "language";
                    }

                    if (pat.Gender == PatientGender.Unknown)
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "gender";
                    }

                    if (PatientRaces.GetForPatient(pat.PatNum).Count == 0)
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "race, ethnicity";
                    }

                    if (explanation == "")
                    {
                        mu.Details = "All demographic elements recorded";
                        mu.Met = MuMet.True;
                    }
                    else
                    {
                        mu.Details = "Missing: " + explanation;
                    }

                    mu.Action = "Enter demographics";
                    break;

                #endregion

                #region Education

                case EhrMeasureType.Education:
                    var listEd = EhrMeasureEvents.RefreshByType(pat.PatNum, EhrMeasureEventType.EducationProvided);
                    if (listEd.Count == 0)
                    {
                        mu.Details = "No education resources provided.";
                    }
                    else
                    {
                        mu.Details = "Education resources provided: " + listEd.Count;
                        mu.Met = MuMet.True;
                    }

                    mu.Action = "Provide education resources";
                    break;

                #endregion

                #region ElectronicCopyAccess

                //This used to be called TimelyAccess.  In 2014, CMS required offices use the new V/D/T measure (patient portal).  Electronic copy was removed as well due to this change.
                case EhrMeasureType.ElectronicCopyAccess:
                    var listOnline = EhrMeasureEvents.RefreshByType(pat.PatNum, EhrMeasureEventType.OnlineAccessProvided);
                    if (listOnline.Count == 0)
                    {
                        mu.Details = "No online access provided.";
                    }
                    else
                    {
                        mu.Details = "Online access provided: " + listOnline[listOnline.Count - 1].DateTEvent.ToShortDateString(); //most recent
                        mu.Met = MuMet.True;
                    }

                    mu.Action = "Provide online Access";
                    break;

                #endregion

                #region TimelyAccess (Deprecated)

                //This measure is now deprecated.  In 2014, CMS required offices use the new V/D/T measure (patient portal).  ElectronicCopyAccess is used instead.
                case EhrMeasureType.TimelyAccess:
                    var listAccess = EhrMeasureEvents.RefreshByType(pat.PatNum, EhrMeasureEventType.OnlineAccessProvided);
                    if (listAccess.Count == 0)
                    {
                        mu.Details = "No online access provided.";
                    }
                    else
                    {
                        mu.Details = "Online access provided: " + listAccess[listAccess.Count - 1].DateTEvent.ToShortDateString(); //most recent
                        mu.Met = MuMet.True;
                    }

                    mu.Action = "Provide online Access";
                    break;

                #endregion

                #region ProvOrderEntry

                case EhrMeasureType.ProvOrderEntry:
                    //int medOrderCount=0;
                    var medOrderCpoeCount = 0;
                    for (var mo = 0; mo < medList.Count; mo++)
                        //if(medList[mo].DateStart.Year>1880 && medList[mo].PatNote!=""){
                        if (medList[mo].IsCpoe)
                            medOrderCpoeCount++;

                    if (medList.Count == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No meds.";
                    }
                    else if (medOrderCpoeCount == 0)
                    {
                        mu.Details = "No medication order in CPOE.";
                    }
                    else
                    {
                        mu.Details = "Medications entered in CPOE: " + medOrderCpoeCount;
                        mu.Met = MuMet.True;
                    }

                    mu.Action = "(edit Rxs from Chart)";
                    break;

                #endregion

                #region CPOE_MedOrdersOnly

                case EhrMeasureType.CPOE_MedOrdersOnly:
                    var medOrderCount = 0;
                    medOrderCpoeCount = 0;
                    for (var m = 0; m < medList.Count; m++)
                        //Using the last year as the reporting period, following pattern in ElectronicCopy, ClinicalSummaries, Reminders...
                        if (medList[m].DateStart < DateTime.Now.AddYears(-1))
                        {
                            //either no start date so not an order, or not within the last year so not during the reporting period
                        }
                        else if (medList[m].PatNote != "" && medList[m].ProvNum == pat.PriProv)
                        {
                            //if there's a note and it was created by the patient's PriProv, then count as order created by this provider and would count toward the denominator for MU
                            medOrderCount++;
                            if (medList[m].IsCpoe)
                                //if also marked as CPOE, then this would count in the numerator of the calculation MU
                                medOrderCpoeCount++;
                        }

                    if (medOrderCount == 0)
                    {
                        mu.Details = "No medication order in CPOE.";
                    }
                    else
                    {
                        mu.Details = "Medications entered in CPOE: " + medOrderCount;
                        mu.Met = MuMet.True;
                    }

                    mu.Action = "(edit Rxs from Chart)";
                    break;

                #endregion

                #region CPOE_PreviouslyOrdered

                case EhrMeasureType.CPOE_PreviouslyOrdered:
                    //first determine if this patient has ever had a medication ordered by this Provider
                    var prevOrderExists = false;
                    for (var m = 0; m < medList.Count; m++)
                        //if this is an order (defined as having instructions and a start date) and was entered by this provider, then this pat will be counted in the denominator
                        if (medList[m].PatNote != "" && medList[m].DateStart.Year > 1880 && medList[m].ProvNum == pat.PriProv)
                        {
                            prevOrderExists = true;
                            break;
                        }

                    medOrderCpoeCount = 0;
                    for (var mo = 0; mo < medList.Count; mo++)
                        if (medList[mo].IsCpoe)
                            medOrderCpoeCount++;

                    if (medList.Count == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No meds.";
                    }
                    else if (!prevOrderExists)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No previous medication orders by this Provider.";
                    }
                    else if (medOrderCpoeCount == 0)
                    {
                        mu.Details = "No medication order in CPOE.";
                    }
                    else
                    {
                        mu.Details = "Medications entered in CPOE: " + medOrderCpoeCount;
                        mu.Met = MuMet.True;
                    }

                    mu.Action = "(edit Rxs from Chart)";
                    break;

                #endregion

                #region Rx

                case EhrMeasureType.Rx:
                    var listRx = RxPats.GetPermissableForDateRange(pat.PatNum, DateTime.Today.AddYears(-1), DateTime.Today);
                    if (listRx.Count == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No Rxs entered.";
                    }
                    else
                    {
                        explanation = "";
                        for (var rx = 0; rx < listRx.Count; rx++)
                        {
                            if (listRx[rx].SendStatus == RxSendStatus.SentElect) continue;
                            if (explanation != "") explanation += ", ";
                            explanation += listRx[rx].RxDate.ToShortDateString();
                        }

                        if (explanation == "")
                        {
                            mu.Met = MuMet.True;
                            mu.Details = "All Rxs sent electronically.";
                        }
                        else
                        {
                            mu.Met = MuMet.False;
                            mu.Details = "Rxs not sent electronically: " + explanation;
                        }
                    }

                    mu.Action = "(edit Rxs from Chart)"; //no action
                    break;

                #endregion

                #region VitalSigns

                case EhrMeasureType.VitalSigns:
                    var vitalsignList = Vitalsigns.Refresh(pat.PatNum);
                    if (vitalsignList.Count == 0)
                    {
                        mu.Details = "No vital signs entered.";
                    }
                    else
                    {
                        var hFound = false;
                        var wFound = false;
                        var bpFound = false;
                        for (var v = 0; v < vitalsignList.Count; v++)
                        {
                            if (vitalsignList[v].Height > 0) hFound = true;
                            if (vitalsignList[v].Weight > 0) wFound = true;
                            if (pat.Birthdate > DateTime.Today.AddYears(-3) //3 and older for BP
                                || (vitalsignList[v].BpDiastolic > 0 && vitalsignList[v].BpSystolic > 0))
                                bpFound = true;
                        }

                        explanation = "";
                        if (!hFound) explanation += "height"; //missing
                        if (!wFound)
                        {
                            if (explanation != "") explanation += ", ";
                            explanation += "weight";
                        }

                        if (!bpFound)
                        {
                            if (explanation != "") explanation += ", ";
                            explanation += "blood pressure";
                        }

                        if (explanation == "")
                        {
                            mu.Details = "Vital signs entered";
                            mu.Met = MuMet.True;
                        }
                        else
                        {
                            mu.Details = "Missing: " + explanation;
                        }
                    }

                    mu.Action = "Enter vital signs";
                    break;

                #endregion

                #region VitalSigns2014

                case EhrMeasureType.VitalSigns2014:
                    var vitalsignList2014 = Vitalsigns.Refresh(pat.PatNum);
                    if (vitalsignList2014.Count == 0)
                    {
                        mu.Details = "No vital signs entered.";
                    }
                    else
                    {
                        var hFound = false;
                        var wFound = false;
                        var bpFound = false;
                        for (var v = 0; v < vitalsignList2014.Count; v++)
                        {
                            if (vitalsignList2014[v].Height > 0) hFound = true;
                            if (vitalsignList2014[v].Weight > 0) wFound = true;
                            if (pat.Birthdate > DateTime.Today.AddYears(-3) //3 and older for BP
                                || (vitalsignList2014[v].BpDiastolic > 0 && vitalsignList2014[v].BpSystolic > 0))
                                bpFound = true;
                        }

                        explanation = "";
                        if (!hFound) explanation += "height"; //missing
                        if (!wFound)
                        {
                            if (explanation != "") explanation += ", ";
                            explanation += "weight";
                        }

                        if (!bpFound)
                        {
                            if (explanation != "") explanation += ", ";
                            explanation += "blood pressure";
                        }

                        if (explanation == "")
                        {
                            mu.Details = "Vital signs entered";
                            mu.Met = MuMet.True;
                        }
                        else
                        {
                            mu.Details = "Missing: " + explanation;
                        }
                    }

                    mu.Action = "Enter vital signs";
                    break;

                #endregion

                #region VitalSignsBMIOnly

                case EhrMeasureType.VitalSignsBMIOnly:
                    vitalsignList = Vitalsigns.Refresh(pat.PatNum);
                    if (vitalsignList.Count == 0)
                    {
                        mu.Details = "No vital signs entered.";
                    }
                    else
                    {
                        var hFound = false;
                        var wFound = false;
                        for (var v = 0; v < vitalsignList.Count; v++)
                        {
                            if (vitalsignList[v].Height > 0) hFound = true;
                            if (vitalsignList[v].Weight > 0) wFound = true;
                        }

                        explanation = "";
                        if (!hFound) explanation += "height"; //missing
                        if (!wFound)
                        {
                            if (explanation != "") explanation += ", ";
                            explanation += "weight";
                        }

                        if (explanation == "")
                        {
                            mu.Details = "Vital signs entered";
                            mu.Met = MuMet.True;
                        }
                        else
                        {
                            mu.Details = "Missing: " + explanation;
                        }
                    }

                    mu.Action = "Enter vital signs";
                    break;

                #endregion

                #region VitalSignsBPOnly

                case EhrMeasureType.VitalSignsBPOnly:
                    vitalsignList = Vitalsigns.Refresh(pat.PatNum);
                    if (pat.Birthdate > DateTime.Today.AddYears(-3))
                    {
                        //3 and older for BP
                        mu.Details = "Age 3 and older for BP.";
                        mu.Met = MuMet.NA;
                    }
                    else if (vitalsignList.Count == 0)
                    {
                        mu.Details = "No vital signs entered.";
                    }
                    else
                    {
                        for (var v = 0; v < vitalsignList.Count; v++)
                            if (vitalsignList[v].BpDiastolic > 0 && vitalsignList[v].BpSystolic > 0)
                            {
                                mu.Details = "Vital signs entered";
                                mu.Met = MuMet.True;
                            }
                            else
                            {
                                mu.Details = "Missing: blood pressure";
                            }
                    }

                    mu.Action = "Enter vital signs";
                    break;

                #endregion

                #region Smoking

                case EhrMeasureType.Smoking:
                    if (pat.SmokingSnoMed == "")
                    {
                        //None
                        mu.Details = "Smoking status not entered";
                    }
                    else
                    {
                        mu.Details = "Smoking status entered";
                        mu.Met = MuMet.True;
                    }

                    if (pat.Age < 13) //The smoking measure does not apply to patients under the age of 13.
                        mu.Met = MuMet.NA;
                    mu.Action = "Edit smoking status";
                    break;

                #endregion

                #region Lab

                case EhrMeasureType.Lab:
                    var listLabOrders = EhrLabs.GetAllForPatInDateRange(pat.PatNum, DateTime.Today.AddYears(-1), DateTime.Today);
                    if (listLabOrders.Count == 0)
                    {
                        mu.Details = "No lab orders";
                        mu.Met = MuMet.NA;
                    }
                    else
                    {
                        var labResultCount = 0;
                        for (var lo = 0; lo < listLabOrders.Count; lo++)
                        {
                            var listLabResults = EhrLabResults.GetForLab(listLabOrders[lo].EhrLabNum);
                            if (listLabResults.Count > 0) labResultCount++;
                        }

                        if (labResultCount < listLabOrders.Count)
                        {
                            mu.Met = MuMet.False;
                            mu.Details = "Lab orders missing results: " + (listLabOrders.Count - labResultCount);
                        }
                        else
                        {
                            mu.Details = "Lab results entered for each lab order.";
                            mu.Met = MuMet.True;
                        }
                    }

                    mu.Action = "Edit labs";
                    break;

                #endregion

                #region ElectronicCopy (Deprecated)

                //This measure is now deprecated.  In 2014, CMS required offices use the new V/D/T measure (patient portal) which is where the patient will get their "electronic copy".
                case EhrMeasureType.ElectronicCopy:
                    var listRequests = EhrMeasureEvents.GetByType(listMeasureEvents, EhrMeasureEventType.ElectronicCopyRequested);
                    var listRequestsPeriod = new List<EhrMeasureEvent>();
                    for (var r = 0; r < listRequests.Count; r++)
                    {
                        if (listRequests[r].DateTEvent < DateTime.Now.AddYears(-1)) //not within the last year
                            continue;
                        listRequestsPeriod.Add(listRequests[r]);
                    }

                    if (listRequestsPeriod.Count == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No requests within the last year.";
                    }
                    else
                    {
                        var countMissingCopies = 0;
                        bool copyProvidedinTime;
                        var listCopiesProvided = EhrMeasureEvents.GetByType(listMeasureEvents, EhrMeasureEventType.ElectronicCopyProvidedToPt);
                        for (var rp = 0; rp < listRequestsPeriod.Count; rp++)
                        {
                            copyProvidedinTime = false;
                            var deadlineDateCopy = listRequestsPeriod[rp].DateTEvent.Date.AddDays(3);
                            if (listRequestsPeriod[rp].DateTEvent.DayOfWeek == DayOfWeek.Wednesday
                                || listRequestsPeriod[rp].DateTEvent.DayOfWeek == DayOfWeek.Thursday
                                || listRequestsPeriod[rp].DateTEvent.DayOfWeek == DayOfWeek.Friday)
                                deadlineDateCopy.AddDays(2); //add two days for the weekend
                            for (var cp = 0; cp < listCopiesProvided.Count; cp++)
                            {
                                if (listCopiesProvided[cp].DateTEvent.Date > deadlineDateCopy) continue;
                                if (listCopiesProvided[cp].DateTEvent.Date < listRequestsPeriod[rp].DateTEvent.Date) continue;
                                copyProvidedinTime = true;
                            }

                            if (!copyProvidedinTime) countMissingCopies++;
                        }

                        if (countMissingCopies == 0)
                        {
                            mu.Met = MuMet.True;
                            mu.Details = "Electronic copy provided to Pt within 3 business days of each request.";
                        }
                        else
                        {
                            mu.Met = MuMet.False;
                            mu.Details = "Electronic copies not provided to Pt within 3 business days of a request:" + countMissingCopies;
                        }
                    }

                    mu.Action = "Provide elect copy to Pt"; //If this text ever changes then FormEHR grid will need to change for MU1
                    break;

                #endregion

                #region ClinicalSummaries

                case EhrMeasureType.ClinicalSummaries:
                    var listVisits = new List<DateTime>(); //for this year
                    var listProcs = Procedures.Refresh(pat.PatNum);
                    for (var p = 0; p < listProcs.Count; p++)
                    {
                        if (listProcs[p].ProcDate < DateTime.Now.AddYears(-1) || listProcs[p].ProcStatus != ProcStat.C) //not within the last year or not a completed procedure
                            continue;
                        if (!listVisits.Contains(listProcs[p].ProcDate)) listVisits.Add(listProcs[p].ProcDate);
                    }

                    if (listVisits.Count == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No visits within the last year.";
                    }
                    else
                    {
                        var countMissing = 0;
                        bool summaryProvidedinTime;
                        var listClinSum = EhrMeasureEvents.GetByType(listMeasureEvents, EhrMeasureEventType.ClinicalSummaryProvidedToPt);
                        for (var p = 0; p < listVisits.Count; p++)
                        {
                            summaryProvidedinTime = false;
                            var deadlineDate = listVisits[p].AddDays(3);
                            if (listVisits[p].DayOfWeek == DayOfWeek.Wednesday || listVisits[p].DayOfWeek == DayOfWeek.Thursday || listVisits[p].DayOfWeek == DayOfWeek.Friday) deadlineDate = deadlineDate.AddDays(2); //add two days for the weekend
                            for (var r = 0; r < listClinSum.Count; r++)
                            {
                                if (listClinSum[r].DateTEvent.Date > deadlineDate) continue;
                                if (listClinSum[r].DateTEvent.Date < listVisits[p]) continue;
                                summaryProvidedinTime = true;
                            }

                            if (!summaryProvidedinTime) countMissing++;
                        }

                        if (countMissing == 0)
                        {
                            mu.Met = MuMet.True;
                            mu.Details = "Clinical summary provided to Pt within 3 business days of each visit.";
                        }
                        else
                        {
                            mu.Met = MuMet.False;
                            mu.Details = "Clinical summaries not provided to Pt within 3 business days of a visit:" + countMissing;
                        }
                    }

                    mu.Action = "Send clinical summary to Pt";
                    break;

                #endregion

                #region Reminders

                case EhrMeasureType.Reminders:
                    if (pat.PatStatus != PatientStatus.Patient)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "Status not patient.";
                    }
                    else if (pat.Age == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "Age not entered.";
                    }
                    else if (pat.Age > 5 && pat.Age < 65)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "Patient age not 65+ or 5-.";
                    }
                    else
                    {
                        var listReminders = EhrMeasureEvents.GetByType(listMeasureEvents, EhrMeasureEventType.ReminderSent);
                        //during reporting period.
                        var withinLastYear = false;
                        for (var r = 0; r < listReminders.Count; r++)
                            if (listReminders[r].DateTEvent > DateTime.Now.AddYears(-1))
                                withinLastYear = true;

                        if (withinLastYear)
                        {
                            mu.Details = "Reminder sent within the last year.";
                            mu.Met = MuMet.True;
                        }
                        else
                        {
                            mu.Details = "No reminders sent within the last year for patient age 65+ or 5-.";
                        }
                    }

                    mu.Action = "Send reminders";
                    break;

                #endregion

                #region MedReconcile

                case EhrMeasureType.MedReconcile:
                    var countFromRef = 0;
                    var countFromRefPeriod = 0;
                    for (var c = 0; c < listRefAttach.Count; c++)
                        if (listRefAttach[c].RefType == ReferralType.RefFrom && listRefAttach[c].IsTransitionOfCare)
                        {
                            countFromRef++;
                            if (listRefAttach[c].RefDate > DateTime.Now.AddYears(-1))
                                //within the last year
                                countFromRefPeriod++;
                        }

                    if (countFromRef == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "Referral 'from' not entered.";
                    }
                    else if (countFromRefPeriod == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "Referral 'from' not entered within the last year.";
                    }
                    else if (countFromRefPeriod > 0)
                    {
                        var listReconciles = EhrMeasureEvents.GetByType(listMeasureEvents, EhrMeasureEventType.MedicationReconcile);
                        var countReconciles = 0; //during reporting period.
                        for (var r = 0; r < listReconciles.Count; r++)
                            if (listReconciles[r].DateTEvent > DateTime.Now.AddYears(-1))
                                //within the same period as the count for referrals.
                                countReconciles++;

                        mu.Details = "Referrals:" + countFromRefPeriod + ", Reconciles:" + countReconciles;
                        if (countReconciles >= countFromRefPeriod) mu.Met = MuMet.True;
                    }

                    mu.Action = "Electronic Reconciliation";
                    mu.Action2 = "Manual Reconciliation";
                    break;

                #endregion

                #region SummaryOfCare

                case EhrMeasureType.SummaryOfCare:
                    var countToRefPeriod = 0;
                    for (var c = 0; c < listRefAttach.Count; c++)
                        if (listRefAttach[c].RefType == ReferralType.RefTo && listRefAttach[c].IsTransitionOfCare && listRefAttach[c].ProvNum != 0)
                            if (listRefAttach[c].RefDate > DateTime.Now.AddYears(-1))
                                //within the last year
                                countToRefPeriod++;

                    if (countToRefPeriod == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No outgoing transitions of care within the last year.";
                    }
                    else
                    {
                        // > 0
                        var listCcds = EhrMeasureEvents.GetByType(listMeasureEvents, EhrMeasureEventType.SummaryOfCareProvidedToDr);
                        var countCcds = 0; //during reporting period.
                        for (var r = 0; r < listCcds.Count; r++)
                            if (listCcds[r].DateTEvent > DateTime.Now.AddYears(-1) && listCcds[r].FKey != 0)
                                //Quick filter for measure events that do not meet the criterion.
                                //Loop through all ref attaches and only count the ones that meet all measure requirements.
                                for (var c = 0; c < listRefAttach.Count; c++)
                                    if (listRefAttach[c].RefType == ReferralType.RefTo
                                        && listRefAttach[c].RefAttachNum == listCcds[r].FKey
                                        && listRefAttach[c].IsTransitionOfCare
                                        && listRefAttach[c].ProvNum != 0
                                        && listRefAttach[c].RefDate > DateTime.Now.AddYears(-1))
                                    {
                                        countCcds++;
                                        break;
                                    }

                        mu.Details = "Referrals:" + countToRefPeriod + ", Summaries:" + countCcds;
                        if (countCcds >= countToRefPeriod) mu.Met = MuMet.True;
                    }

                    mu.Action = "Send/Receive summary of care";
                    mu.Action2 = "Enter Referrals";
                    break;

                #endregion

                #region DrugDrugInteractChecking

                case EhrMeasureType.DrugDrugInteractChecking:
                    var listDDIC = EhrMeasureEvents.GetAllByTypeFromDB(new DateTime(DateTime.Now.Year, 1, 1), new DateTime(DateTime.Now.Year, 12, 31), EhrMeasureEventType.DrugDrugInteractChecking, false);
                    if (listDDIC.Count != 0 && !string.IsNullOrWhiteSpace(listDDIC[listDDIC.Count - 1].MoreInfo))
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Action taken/Explanation given";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Explanation missing";
                    }

                    mu.Action = "Edit Explanation";
                    break;

                #endregion

                #region DrugFormularyChecking

                case EhrMeasureType.DrugFormularyChecking:
                    var listDFC = EhrMeasureEvents.GetAllByTypeFromDB(new DateTime(DateTime.Now.Year, 1, 1), new DateTime(DateTime.Now.Year, 12, 31), EhrMeasureEventType.DrugFormularyChecking, false);
                    if (listDFC.Count != 0 && !string.IsNullOrWhiteSpace(listDFC[listDFC.Count - 1].MoreInfo))
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Action taken/Explanation given";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Explanation missing";
                    }

                    mu.Action = "Edit Explanation";
                    break;

                #endregion

                #region ProtectElectHealthInfo

                case EhrMeasureType.ProtectElectHealthInfo:
                    var listPEHI = EhrMeasureEvents.GetAllByTypeFromDB(new DateTime(DateTime.Now.Year, 1, 1), new DateTime(DateTime.Now.Year, 12, 31), EhrMeasureEventType.ProtectElectHealthInfo, false);
                    if (listPEHI.Count != 0 && !string.IsNullOrWhiteSpace(listPEHI[listPEHI.Count - 1].MoreInfo))
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Action taken/Explanation given";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Explanation missing";
                    }

                    mu.Action = "Edit Explanation";
                    break;

                #endregion

                #region ImmunizationRegistries

                case EhrMeasureType.ImmunizationRegistries:
                    var listIR = EhrMeasureEvents.GetAllByTypeFromDB(new DateTime(DateTime.Now.Year, 1, 1), new DateTime(DateTime.Now.Year, 12, 31), EhrMeasureEventType.ImmunizationRegistries, false);
                    if (listIR.Count != 0 && !string.IsNullOrWhiteSpace(listIR[listIR.Count - 1].MoreInfo))
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Action taken/Explanation given";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Explanation missing";
                    }

                    mu.Action = "Edit Explanation";
                    break;

                #endregion

                #region SyndromicSurveillance

                case EhrMeasureType.SyndromicSurveillance:
                    var listSS = EhrMeasureEvents.GetAllByTypeFromDB(new DateTime(DateTime.Now.Year, 1, 1), new DateTime(DateTime.Now.Year, 12, 31), EhrMeasureEventType.SyndromicSurveillance, false);
                    if (listSS.Count != 0 && !string.IsNullOrWhiteSpace(listSS[listSS.Count - 1].MoreInfo))
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Action taken/Explanation given";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Explanation missing";
                    }

                    mu.Action = "Edit Explanation";
                    break;

                #endregion

                #region PatientList

                case EhrMeasureType.PatientList:
                    var listPL = EhrMeasureEvents.GetAllByTypeFromDB(new DateTime(DateTime.Now.Year, 1, 1), new DateTime(DateTime.Now.Year, 12, 31), EhrMeasureEventType.PatientList, false);
                    if (listPL.Count != 0)
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "List generated";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "List not generated";
                    }

                    mu.Action = "Generate List";
                    break;

                #endregion

                #region ClinicalInterventionRules

                case EhrMeasureType.ClinicalInterventionRules:
                    var listEhrTriggers = EhrTriggers.GetAll();
                    if (listEhrTriggers.Count > 0)
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Triggers enabled";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "No triggers enabled";
                    }

                    mu.Action = "Edit triggers";
                    mu.Action2 = "Screenshot CDS Triggers for audit";
                    break;

                #endregion
            }

            list.Add(mu);
        }

        return list;
    }

    private static List<EhrMeasure> GetMUList()
    {
        var command = "SELECT * FROM ehrmeasure "
                      + "WHERE MeasureType IN ("
                      + SOut.Int((int) EhrMeasureType.ProblemList) + ","
                      + SOut.Int((int) EhrMeasureType.MedicationList) + ","
                      + SOut.Int((int) EhrMeasureType.AllergyList) + ","
                      + SOut.Int((int) EhrMeasureType.Demographics) + ","
                      + SOut.Int((int) EhrMeasureType.Education) + ","
                      + SOut.Int((int) EhrMeasureType.ElectronicCopyAccess) + ","
                      //+POut.Int((int)EhrMeasureType.TimelyAccess)+"," //Deprecated in 2014.  V/D/T (patient portal) now required.  ElectronicCopyAccess used instead.
                      + SOut.Int((int) EhrMeasureType.ProvOrderEntry) + ","
                      + SOut.Int((int) EhrMeasureType.CPOE_MedOrdersOnly) + ","
                      + SOut.Int((int) EhrMeasureType.CPOE_PreviouslyOrdered) + ","
                      + SOut.Int((int) EhrMeasureType.Rx) + ","
                      + SOut.Int((int) EhrMeasureType.VitalSigns) + ","
                      + SOut.Int((int) EhrMeasureType.VitalSigns2014) + ","
                      + SOut.Int((int) EhrMeasureType.VitalSignsBMIOnly) + ","
                      + SOut.Int((int) EhrMeasureType.VitalSignsBPOnly) + ","
                      + SOut.Int((int) EhrMeasureType.Smoking) + ","
                      + SOut.Int((int) EhrMeasureType.Lab) + ","
                      //+POut.Int((int)EhrMeasureType.ElectronicCopy)+"," //Deprecated in 2014.  V/D/T (patient portal) now required.  ElectronicCopyAccess used instead.
                      + SOut.Int((int) EhrMeasureType.ClinicalSummaries) + ","
                      + SOut.Int((int) EhrMeasureType.Reminders) + ","
                      + SOut.Int((int) EhrMeasureType.MedReconcile) + ","
                      + SOut.Int((int) EhrMeasureType.SummaryOfCare) + ","
                      + SOut.Int((int) EhrMeasureType.DrugDrugInteractChecking) + ","
                      + SOut.Int((int) EhrMeasureType.DrugFormularyChecking) + ","
                      + SOut.Int((int) EhrMeasureType.ProtectElectHealthInfo) + ","
                      + SOut.Int((int) EhrMeasureType.ImmunizationRegistries) + ","
                      + SOut.Int((int) EhrMeasureType.SyndromicSurveillance) + ","
                      + SOut.Int((int) EhrMeasureType.PatientList) + ","
                      + SOut.Int((int) EhrMeasureType.ClinicalInterventionRules) + ") ";
            command += "ORDER BY FIELD(MeasureType,"
                       + SOut.Int((int) EhrMeasureType.ProblemList) + ","
                       + SOut.Int((int) EhrMeasureType.MedicationList) + ","
                       + SOut.Int((int) EhrMeasureType.AllergyList) + ","
                       + SOut.Int((int) EhrMeasureType.Demographics) + ","
                       + SOut.Int((int) EhrMeasureType.Education) + ","
                       + SOut.Int((int) EhrMeasureType.ElectronicCopyAccess) + ","
                       //+POut.Int((int)EhrMeasureType.TimelyAccess)+","
                       + SOut.Int((int) EhrMeasureType.ProvOrderEntry) + ","
                       + SOut.Int((int) EhrMeasureType.CPOE_MedOrdersOnly) + ","
                       + SOut.Int((int) EhrMeasureType.CPOE_PreviouslyOrdered) + ","
                       + SOut.Int((int) EhrMeasureType.Rx) + ","
                       + SOut.Int((int) EhrMeasureType.VitalSigns) + ","
                       + SOut.Int((int) EhrMeasureType.VitalSigns2014) + ","
                       + SOut.Int((int) EhrMeasureType.VitalSignsBMIOnly) + ","
                       + SOut.Int((int) EhrMeasureType.VitalSignsBPOnly) + ","
                       + SOut.Int((int) EhrMeasureType.Smoking) + ","
                       + SOut.Int((int) EhrMeasureType.Lab) + ","
                       //+POut.Int((int)EhrMeasureType.ElectronicCopy)+","
                       + SOut.Int((int) EhrMeasureType.ClinicalSummaries) + ","
                       + SOut.Int((int) EhrMeasureType.Reminders) + ","
                       + SOut.Int((int) EhrMeasureType.MedReconcile) + ","
                       + SOut.Int((int) EhrMeasureType.SummaryOfCare) + ","
                       + SOut.Int((int) EhrMeasureType.DrugDrugInteractChecking) + ","
                       + SOut.Int((int) EhrMeasureType.DrugFormularyChecking) + ","
                       + SOut.Int((int) EhrMeasureType.ProtectElectHealthInfo) + ","
                       + SOut.Int((int) EhrMeasureType.ImmunizationRegistries) + ","
                       + SOut.Int((int) EhrMeasureType.SyndromicSurveillance) + ","
                       + SOut.Int((int) EhrMeasureType.PatientList) + ","
                       + SOut.Int((int) EhrMeasureType.ClinicalInterventionRules) + ") ";

        //TODO: Order the measures nicely for Oracle users.
        var retVal = EhrMeasureCrud.SelectMany(command);
        return retVal;
    }

    #endregion

    #region Meaningful Use 2

    ///<summary>Returns the Objective text based on the EHR certification documents.</summary>
    private static string GetObjectiveMu2(EhrMeasureType mtype)
    {
        switch (mtype)
        {
            case EhrMeasureType.CPOE_MedOrdersOnly:
                return "Use computerized provider order entry (CPOE) for medication orders directly entered by any licensed healthcare professional who can enter orders into the medical record per state, local and professional guidelines.";
            case EhrMeasureType.CPOE_LabOrdersOnly:
                return "Use computerized provider order entry (CPOE) for laboratory orders directly entered by any licensed healthcare professional who can enter orders into the medical record per state, local and professional guidelines.";
            case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                return "Use computerized provider order entry (CPOE) for radiology orders directly entered by any licensed healthcare professional who can enter orders into the medical record per state, local and professional guidelines.";
            case EhrMeasureType.Rx:
                return "Generate and transmit permissible prescriptions electronically (eRx).";
            case EhrMeasureType.Demographics:
                return "Record the following demographics: preferred language, sex, race, ethnicity, date of birth.";
            case EhrMeasureType.VitalSigns:
                return "Record and chart changes in the following vital signs: height/length and weight (no age limit); blood pressure (ages 3 and over); calculate and display body mass index (BMI); and plot and display growth charts for patients 0-20 years, including BMI.";
            case EhrMeasureType.VitalSignsBMIOnly:
                return "Record and chart changes in the following vital signs: height/length and weight (no age limit); calculate and display body mass index (BMI); and plot and display growth charts for patients 0-20 years, including BMI.";
            case EhrMeasureType.VitalSignsBPOnly:
                return "Record and chart changes in the following vital signs: blood pressure (ages 3 and over).";
            case EhrMeasureType.Smoking:
                return "Record smoking status for patients 13 years old or older.";
            case EhrMeasureType.ElectronicCopyAccess:
                return "Provide patients the ability to view online, download and transmit their health information within four business days of the information being available to the EP.";
            case EhrMeasureType.ElectronicCopy:
                return "Patient's will view online, download or transmit their health information within four business days of the information being available to the EP.";
            case EhrMeasureType.ClinicalSummaries:
                return "Provide clinical summaries for patients for each office visit.";
            case EhrMeasureType.Lab:
                return "Incorporate clinical lab-test results into Certified EHR Technology (CEHRT) as structured data.";
            case EhrMeasureType.Reminders:
                return "Use clinically relevant information to identify patients who should receive reminders for preventive/follow-up care and send these patients the reminders, per patient preference.";
            case EhrMeasureType.Education:
                return "Use clinically relevant information from Certified EHR Technology to identify patient-specific education resources and provide those resources to the patient.";
            case EhrMeasureType.MedReconcile:
                return "The EP who receives a patient from another setting of care or provider of care or believes an encounter is relevant should perform medication reconciliation.";
            case EhrMeasureType.SummaryOfCare:
                return "The EP who transitions their patient to another setting of care or provider of care or refers their patient to another provider of care should provide summary care record for each transition of care or referral.";
            case EhrMeasureType.SummaryOfCareElectronic:
                return "The EP who transitions their patient to another setting of care or provider of care or refers their patient to another provider of care should provide summary care record electronically for each transition of care or referral.";
            case EhrMeasureType.SecureMessaging:
                return "Use secure electronic messaging to communicate with patients on relevant health information.";
            case EhrMeasureType.FamilyHistory:
                return "Record patient family health history as structured data.";
            case EhrMeasureType.ElectronicNote:
                return "Record electronic notes in patient records.";
            case EhrMeasureType.LabImages:
                return "Imaging results consisting of the image itself and any explanation or other accompanying information are accessible through CEHRT.";
        }

        return "";
        //throw new ApplicationException("Type not in use for MU2: "+mtype.ToString());
    }

    ///<summary>Returns the Measures text based on the EHR certification documents.</summary>
    private static string GetMeasureMu2(EhrMeasureType mtype)
    {
        var thresh = GetThresholdMu2(mtype);
        switch (mtype)
        {
            case EhrMeasureType.CPOE_MedOrdersOnly:
                return "More than " + thresh + "% of medication orders created by the EP during the EHR reporting period are recorded using CPOE.";
            case EhrMeasureType.CPOE_LabOrdersOnly:
                return "More than " + thresh + "% of lab orders created by the EP during the EHR reporting period are recorded using CPOE.";
            case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                return "More than " + thresh + "% of radiology orders created by the EP during the EHR reporting period are recorded using CPOE.";
            case EhrMeasureType.Rx:
                return "More than " + thresh + "% of all permissible prescriptions, or all prescriptions, written by the EP are queried for a drug formulary and transmitted electronically using CEHRT.";
            case EhrMeasureType.Demographics:
                return "More than " + thresh + "% of all unique patients seen by the EP have demographics recorded as structured data.";
            case EhrMeasureType.VitalSigns:
                return "More than " + thresh + "% of all unique patients seen by the EP have blood pressure (for patients age 3 and over only) and/or height and weight (for all ages) recorded as structured data.";
            case EhrMeasureType.VitalSignsBMIOnly:
                return "More than " + thresh + "% of all unique patients seen by the EP have blood pressure (for patients age 3 and over only) and/or height and weight (for all ages) recorded as structured data.";
            case EhrMeasureType.VitalSignsBPOnly:
                return "More than " + thresh + "% of all unique patients seen by the EP have blood pressure (for patients age 3 and over only) and/or height and weight (for all ages) recorded as structured data.";
            case EhrMeasureType.Smoking:
                return "More than " + thresh + "% of all unique patients 13 years old or older seen by the EP have smoking status recorded as structured data.";
            case EhrMeasureType.ElectronicCopyAccess:
                return "More than " + thresh + "% of all unique patients seen by the EP during the EHR reporting period are provided timely (available to the patient within 4 business days after the information is available to the EP) online access to their health information.";
            case EhrMeasureType.ElectronicCopy:
                return "More than " + thresh + "% of all unique patients seen by the EP during the EHR reporting period (or their authorized representatives) view, download, or transmit to a third party their health information.";
            case EhrMeasureType.ClinicalSummaries:
                return "Clinical summaries provided to patients or patient-authorized representatives within one business day for more than " + thresh + "% of office visits.";
            case EhrMeasureType.Lab:
                return "More than " + thresh + "% of all clinical lab tests results ordered by the EP during the EHR reporting period whose results are either in a positive/negative or numerical format are incorporated in Certified EHR Technology as structured data.";
            case EhrMeasureType.Reminders:
                return "More than " + thresh + "% of all unique patients who have had 2 or more office visits with the EP within the 24 months before the beginning of the EHR reporting period were sent a reminder, per patient preference when available.";
            case EhrMeasureType.Education:
                return "Patient-specific education resources identified by Certified EHR Technology are provided to patients for more than " + thresh + "% of all unique patients with office visits seen by the EP during the EHR reporting period.";
            case EhrMeasureType.MedReconcile:
                return "The EP who performs medication reconciliation for more than " + thresh + "% of transitions of care in which the patient is transitioned into the care of the EP.";
            case EhrMeasureType.SummaryOfCare:
                return "The EP who transitions or refers their patient to another setting of care or provider of care provides a summary of care record for more than " + thresh + "% of transitions of care and referrals.";
            case EhrMeasureType.SummaryOfCareElectronic:
                return "The EP who transitions or refers their patient to another setting of care or provider of care provides a summary of care record for more than " + thresh + "% of such transitions, and referrals, electronically transmitted using CEHRT to a recipient";
            case EhrMeasureType.SecureMessaging:
                return "A secure message was sent using the electronic messaging function of CEHRT by more than " + thresh + "% of unique patients (or their authorized representatives) seen by the EP during the EHR reporting period.";
            case EhrMeasureType.FamilyHistory:
                return "More than " + thresh + "% of all unique patients seen by the EP during the EHR reporting period have a structured data entry for one or more first-degree relatives.";
            case EhrMeasureType.ElectronicNote:
                return "Enter at least one electronic progress note created, edited and signed by an EP for more than " + thresh + "% of unique patients with at least one office visit during the EHR Measure reporting period. The text of the electronic note must be text searchable and may contain drawings and other content";
            case EhrMeasureType.LabImages:
                return "More than " + thresh + "% of all tests whose result is one or more images ordered by the EP during the EHR reporting period are accessible through CEHRT.";
        }

        return "";
        //throw new ApplicationException("Type not in use for MU2: "+mtype.ToString());
    }

    ///<summary>Returns the Measures text based on the EHR certification documents.</summary>
    private static int GetThresholdMu2(EhrMeasureType mtype)
    {
        switch (mtype)
        {
            case EhrMeasureType.CPOE_MedOrdersOnly:
                return 60;
            case EhrMeasureType.CPOE_LabOrdersOnly:
                return 30;
            case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                return 30;
            case EhrMeasureType.Rx:
                return 50;
            case EhrMeasureType.Demographics:
                return 80;
            case EhrMeasureType.VitalSigns:
                return 80;
            case EhrMeasureType.VitalSignsBMIOnly:
                return 80;
            case EhrMeasureType.VitalSignsBPOnly:
                return 80;
            case EhrMeasureType.Smoking:
                return 80;
            case EhrMeasureType.ElectronicCopyAccess:
                return 50;
            case EhrMeasureType.ElectronicCopy:
                return 5;
            case EhrMeasureType.ClinicalSummaries:
                return 50;
            case EhrMeasureType.Lab:
                return 55;
            case EhrMeasureType.Reminders:
                return 10;
            case EhrMeasureType.Education:
                return 10;
            case EhrMeasureType.MedReconcile:
                return 50;
            case EhrMeasureType.SummaryOfCare:
                return 50;
            case EhrMeasureType.SummaryOfCareElectronic:
                return 10;
            case EhrMeasureType.SecureMessaging:
                return 5;
            case EhrMeasureType.FamilyHistory:
                return 20;
            case EhrMeasureType.ElectronicNote:
                return 30;
            case EhrMeasureType.LabImages:
                return 10;
        }

        return 0;
        //throw new ApplicationException("Type not found: "+mtype.ToString());
    }

    public static DataTable GetTableMu2(EhrMeasureType mtype, DateTime dateStart, DateTime dateEnd, long provNum)
    {
        var command = "";
        var tableRaw = new DataTable();
        var provCur = Providers.GetProv(provNum);
        command = "SELECT GROUP_CONCAT(provider.ProvNum) FROM provider WHERE provider.ProvNum IN "
                  + "(SELECT ProvNum FROM provider pv, ehrprovkey epk WHERE pv.LName=epk.LName AND pv.FName=epk.FName "
                  + "AND epk.LName='" + SOut.String(provCur.LName) + "' AND epk.FName='" + SOut.String(provCur.FName) + "')";
        var provs = DataCore.GetScalar(command);
        var tempProv = provs.Split(',');
        var provOID = "";
        for (var oi = 0; oi < tempProv.Length; oi++)
        {
            provOID = provOID + tempProv[oi];
            if (oi < tempProv.Length - 1) provOID += ",";
        }

        command = "SELECT GROUP_CONCAT(provider.NationalProvID) FROM provider WHERE provider.ProvNum IN "
                  + "(SELECT ProvNum FROM provider pv, ehrprovkey epk WHERE pv.LName=epk.LName AND pv.FName=epk.FName "
                  + "AND epk.LName='" + SOut.String(provCur.LName) + "' AND epk.FName='" + SOut.String(provCur.FName) + "') "
                  + "AND provider.NationalProvID !='' AND provider.NationalProvID !=','";
        var provNPIs = DataCore.GetScalar(command);
        provNPIs.Trim(',');
        if (provNPIs == "") provNPIs = "NULL"; //No NPI entered, queries below use IN statements and require at least one value.
        //Some measures use a temp table.  Create a random number to tack onto the end of the temp table name to avoid possible table collisions.
        var rnd = new Random();
        var rndStr = rnd.Next(1000000).ToString();
        switch (mtype)
        {
            #region CPOE_MedOrdersOnly

            case EhrMeasureType.CPOE_MedOrdersOnly:
                command = "SELECT patient.LName, patient.FName, medPat.* "
                          + "FROM medicationpat as medPat "
                          + "INNER JOIN patient ON patient.PatNum=medPat.PatNum "
                          + "LEFT JOIN ehrmeasureevent as eme ON medPat.MedicationPatNum=eme.FKey "
                          + "AND eme.EventType=" + SOut.Int((int) EhrMeasureEventType.CPOE_MedOrdered) + " "
                          + "WHERE medPat.ProvNum IN(" + SOut.String(provs) + ") "
                          + "AND medPat.MedicationNum=0 " //MedicationNum is 0 when creating an Rx either manually or through NewCrop.  MedicationNum is not 0 when manually added through the medical window.
                          + "AND medPat.DateStart BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd);
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region CPOE_LabOrdersOnly

            case EhrMeasureType.CPOE_LabOrdersOnly:
                command = "SELECT patient.PatNum,patient.LName,patient.FName,ehrlab.IsCpoe,STR_TO_DATE(ehrlab.ObservationDateTimeStart,'%Y%m%d') AS ObservationDateTimeStart "
                          + "FROM ehrlab "
                          + "INNER JOIN patient ON ehrlab.PatNum=patient.PatNum "
                          + "WHERE (CASE WHEN ehrlab.OrderingProviderIdentifierTypeCode='NPI' THEN ehrlab.OrderingProviderID IN(" + SOut.String(provNPIs) + ") " //When the lab is using a NPI number to determine provider.
                          + "WHEN ehrlab.OrderingProviderIdentifierTypeCode='PRN' THEN ( " //When the lab is using provider number to determine provider.
                          + "CASE WHEN ehrlab.OrderingProviderAssigningAuthorityUniversalID=( " //If the AssigningAuthority is OpenDental.
                          + "SELECT IDRoot FROM oidinternal WHERE IDType='Provider' GROUP BY IDType "
                          + ") THEN ehrlab.OrderingProviderID IN(" + SOut.String(provs) + ") END) " //Use the ProvNum to determine provider.
                          + "ELSE FALSE END) " //If the AssigningAuthority is not OpenDental, we have no way to tell who the provider is.
                          + "AND ehrlab.ObservationDateTimeStart BETWEEN DATE_FORMAT(" + SOut.Date(dateStart) + ",'%Y%m%d') AND DATE_FORMAT(" + SOut.Date(dateEnd) + ",'%Y%m%d') "
                          + "AND (CASE WHEN ehrlab.UsiCodeSystemName='LN' THEN ehrlab.UsiID WHEN ehrlab.UsiCodeSystemNameAlt='LN' THEN ehrlab.UsiIDAlt ELSE '' END) "
                          + "NOT IN (SELECT LoincCode FROM loinc WHERE loinc.ClassType LIKE '%RAD%')";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region CPOE_RadiologyOrdersOnly

            case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                command = "SELECT patient.PatNum,patient.LName,patient.FName,ehrlab.IsCpoe,STR_TO_DATE(ehrlab.ObservationDateTimeStart,'%Y%m%d') AS ObservationDateTimeStart, "
                          + "(CASE WHEN ehrlab.UsiCodeSystemName='LN' THEN ehrlab.UsiID WHEN ehrlab.UsiCodeSystemNameAlt='LN' THEN ehrlab.UsiIDAlt ELSE '' END) AS LoincCode "
                          + "FROM ehrlab "
                          + "INNER JOIN patient ON ehrlab.PatNum=patient.PatNum "
                          + "WHERE (CASE WHEN ehrlab.OrderingProviderIdentifierTypeCode='NPI' THEN ehrlab.OrderingProviderID IN(" + SOut.String(provNPIs) + ") " //When the lab is using a NPI number to determine provider.
                          + "WHEN ehrlab.OrderingProviderIdentifierTypeCode='PRN' THEN ( " //When the lab is using provider number to determine provider.
                          + "CASE WHEN ehrlab.OrderingProviderAssigningAuthorityUniversalID=( " //If the AssigningAuthority is OpenDental.
                          + "SELECT IDRoot FROM oidinternal WHERE IDType='Provider' GROUP BY IDType "
                          + ") THEN ehrlab.OrderingProviderID IN(" + SOut.String(provs) + ") END) " //Use the ProvNum to determine provider.
                          + "ELSE FALSE END) " //If the AssigningAuthority is not OpenDental, we have no way to tell who the provider is.
                          + "AND ehrlab.ObservationDateTimeStart BETWEEN DATE_FORMAT(" + SOut.Date(dateStart) + ",'%Y%m%d') AND DATE_FORMAT(" + SOut.Date(dateEnd) + ",'%Y%m%d') "
                          + "AND (CASE WHEN ehrlab.UsiCodeSystemName='LN' THEN ehrlab.UsiID WHEN ehrlab.UsiCodeSystemNameAlt='LN' THEN ehrlab.UsiIDAlt ELSE '' END) "
                          + "IN (SELECT LoincCode FROM loinc WHERE loinc.ClassType LIKE '%RAD%')";
                var dateStartRad154 = PrefC.GetDate(PrefName.RadiologyDateStartedUsing154);
                //Only count radiology orders via the procedurelog table if the date that the office updated to v15.4.1 is less than the end date.
                if (dateStartRad154 < dateEnd.Date)
                {
                    var dateStartStr = SOut.Date(dateStart);
                    //We need to use a start date of whichever date is greater, dateStart or the date the the office updated to v15.4.1.
                    if (dateStartRad154 > dateStart)
                        //The date that the office updated past 15.4.1 is greater than the reporting period's start date.
                        //Use the preference date instead so that the office isn't penalized for radiology orders prior to updating past v15.4.1.
                        dateStartStr = SOut.Date(dateStartRad154);
                    //As of v15.4 we started storing radiology orders at the procedure level by flagging the procedure itself as IsCpoe.
                    //We are going to keep on running both of these queries at the same time.  
                    //Users can simply unmark procedure codes as IsRadiology if they want to only count radiology orders the via the old way (lab order window).
                    command += " UNION ALL "; //Use union all so that duplicate columns do NOT get grouped together.
                    //Get all completed procedures within the date range that are associated to procedure codes that are flagged as IsRadiology.
                    command += "SELECT patient.PatNum,patient.LName,patient.FName,procedurelog.IsCpoe,procedurelog.DateEntryC,'' as LoincCode "
                               + "FROM procedurelog "
                               + "INNER JOIN procedurecode ON procedurelog.CodeNum=procedurecode.CodeNum AND procedurecode.IsRadiology=1 "
                               + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                               + "LEFT JOIN patient ON procedurelog.PatNum=patient.PatNum "
                               + "WHERE procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.C) + " "
                               + "AND procedurelog.ProvNum IN (" + SOut.String(provs) + ") "
                               + "AND procedurelog.DateEntryC BETWEEN " + dateStartStr + " AND " + SOut.Date(dateEnd);
                }

                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region Rx

            case EhrMeasureType.Rx:
                command = "SELECT patient.PatNum,LName,FName,SendStatus,RxDate "
                          + "FROM rxpat,patient "
                          + "WHERE rxpat.PatNum=patient.PatNum "
                          + "AND IsControlled = 0 "
                          //+"AND rxpat.ProvNum="+POut.Long(provNum)+" "
                          + "AND rxpat.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND RxDate >= " + SOut.Date(dateStart) + " "
                          + "AND RxDate <= " + SOut.Date(dateEnd);
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region Demographics

            case EhrMeasureType.Demographics:
                //command="SELECT patient.PatNum,LName,FName,Birthdate,Gender,Race,Language "
                //	+"FROM patient "
                //	+"INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                //	+"AND procedurelog.ProvNum IN("+POut.String(provs)+")	"
                //	+"AND procedurelog.ProcDate BETWEEN "+POut.Date(dateStart)+" AND "+POut.Date(dateEnd)+" "
                //	+"GROUP BY patient.PatNum";
                //tableRaw=DataCore.GetTable(command);
                command = "SELECT patient.PatNum,LName,FName,Birthdate,Gender,Language,COALESCE(race.HasRace,0) AS HasRace,COALESCE(race.HasEthnicity,0) AS HasEthnicity "
                          + "FROM patient "
                          + "INNER JOIN ( "
                          + "SELECT DISTINCT procedurelog.PatNum "
                          + "FROM procedurelog "
                          + "WHERE procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.C) + " ";
                var brokenApptCodes = string.Join(",", ProcedureCodes.GetWhereFromList(x => x.ProcCode == "D9986" || x.ProcCode == "D9987")
                    .Select(x => x.CodeNum));
                if (!string.IsNullOrEmpty(brokenApptCodes)) command += "AND CodeNum NOT IN (" + brokenApptCodes + ") ";
                command += "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                           + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                           + ") ProcCheck ON ProcCheck.PatNum = patient.PatNum "
                           + "LEFT JOIN ( "
                           + "SELECT PatNum, "
                           + "MAX(CASE WHEN cdcrec.HeirarchicalCode LIKE 'R%' " //Race Hierarchical Codes start with an 'R'
                           + "OR patientrace.CdcrecCode='" + SOut.String(PatientRace.DECLINE_SPECIFY_RACE_CODE) + "' "
                           + "THEN 1 ELSE 0 END) HasRace, "
                           + "MAX(CASE WHEN cdcrec.HeirarchicalCode LIKE 'E%' " //Ethnicity Hierarchical Codes start with an 'E'
                           + "OR patientrace.CdcrecCode='" + SOut.String(PatientRace.DECLINE_SPECIFY_ETHNICITY_CODE) + "' "
                           + "THEN 1 ELSE 0 END) HasEthnicity "
                           + "FROM patientrace "
                           + "LEFT JOIN cdcrec ON cdcrec.CdcrecCode=patientrace.CdcrecCode "
                           + "GROUP BY PatNum "
                           + "ORDER BY NULL "
                           + ") race ON race.PatNum=patient.PatNum;";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region VitalSigns

            case EhrMeasureType.VitalSigns:
                command = "SELECT A.*,COALESCE(hwCount.Count,0) AS hwCount,"
                          + "(CASE WHEN A.Birthdate <= (A.LastVisitInDateRange-INTERVAL 3 YEAR) "; //BP count only if 3 and older at time of last visit in date range
                command += "THEN COALESCE(bpCount.Count,0) ELSE 1 END) AS bpCount "
                           + "FROM (SELECT patient.PatNum,LName,FName,Birthdate,MAX(procedurelog.ProcDate) AS LastVisitInDateRange "
                           + "FROM patient "
                           + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                           + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                           + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                           + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                           + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                           + "GROUP BY patient.PatNum) A "
                           + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM vitalsign	WHERE Height>0 AND Weight>0 GROUP BY PatNum) hwCount ON hwCount.PatNum=A.PatNum "
                           + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM vitalsign WHERE BpSystolic>0 AND BpDiastolic>0 GROUP BY PatNum) bpCount ON bpCount.PatNum=A.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region VitalSignsBMIOnly

            case EhrMeasureType.VitalSignsBMIOnly:
                command = "SELECT A.*,COALESCE(hwCount.Count,0) AS hwCount "
                          + "FROM (SELECT patient.PatNum,LName,FName FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) A "
                          + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM vitalsign	WHERE Height>0 AND Weight>0 GROUP BY PatNum) hwCount ON hwCount.PatNum=A.PatNum ";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region VitalSignsBPOnly

            case EhrMeasureType.VitalSignsBPOnly:
                command = "SELECT patient.PatNum,LName,FName,Birthdate,COUNT(DISTINCT VitalsignNum) AS bpcount "
                          + "FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum "
                          + "AND procedurelog.ProcStatus=2	AND procedurelog.ProvNum IN(" + SOut.String(provs) + ") "
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "LEFT JOIN vitalsign ON vitalsign.PatNum=patient.PatNum AND BpSystolic!=0 AND BpDiastolic!=0 "
                          + "GROUP BY patient.PatNum "
                          + "HAVING Birthdate<=MAX(ProcDate)-INTERVAL 3 YEAR "; //only include in results if over 3 yrs old at date of last visit
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region Smoking

            case EhrMeasureType.Smoking:
                command = "SELECT patient.PatNum,LName,FName,SmokingSnoMed FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ") "
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "WHERE patient.Birthdate <= " + SOut.Date(DateTime.Today.AddYears(-13)) + " " //13 and older
                          + "GROUP BY patient.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region ElectronicCopyAccess

            case EhrMeasureType.ElectronicCopyAccess:
                command = "SELECT patient.PatNum,patient.LName,patient.FName,OnlineAccess.dateProvided,ProcCheck.MinProcDate AS leastRecentDate "
                          + "FROM patient "
                          + "INNER JOIN ( "
                          + "SELECT procedurelog.PatNum,MIN(procedurelog.ProcDate) AS MinProcDate "
                          + "FROM procedurelog "
                          + "WHERE procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.C) + " ";
                brokenApptCodes = string.Join(",", ProcedureCodes.GetWhereFromList(x => x.ProcCode == "D9986" || x.ProcCode == "D9987")
                    .Select(x => x.CodeNum));
                if (!string.IsNullOrEmpty(brokenApptCodes)) command += "AND CodeNum NOT IN (" + brokenApptCodes + ") ";
                command += "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                           + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                           + "GROUP BY procedurelog.PatNum "
                           + "ORDER BY NULL "
                           + ") ProcCheck ON ProcCheck.PatNum = patient.PatNum "
                           + "LEFT JOIN ( "
                           + "SELECT ehrmeasureevent.PatNum,MIN(ehrmeasureevent.DateTEvent) AS dateProvided "
                           + "FROM ehrmeasureevent "
                           + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.OnlineAccessProvided) + " "
                           + "GROUP BY ehrmeasureevent.PatNum "
                           + "ORDER BY NULL "
                           + ") OnlineAccess ON patient.PatNum=OnlineAccess.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region ElectronicCopy

            case EhrMeasureType.ElectronicCopy:
                command = "SELECT patient.PatNum,patient.LName,patient.FName,OnlineAccess.dateRequested "
                          + "FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "LEFT JOIN (SELECT ehrmeasureevent.PatNum, MIN(ehrmeasureevent.DateTEvent) as dateRequested FROM ehrmeasureevent "
                          + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.ElectronicCopyRequested) + " "
                          + "GROUP BY patnum) "
                          + "OnlineAccess ON patient.PatNum=OnlineAccess.PatNum "
                          + "GROUP BY patient.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region ClinicalSummaries

            case EhrMeasureType.ClinicalSummaries:
                command = "SELECT patient.PatNum,LName,FName,ClinSum.summaryProvided as summaryProvided,ProcCheck.ProcDate as procDate "
                          + "FROM patient "
                          + "INNER JOIN ( "
                          + "SELECT procedurelog.PatNum,procedurelog.ProcNum,procedurelog.ProcDate "
                          + "FROM procedurelog "
                          + "WHERE procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.C) + " ";
                brokenApptCodes = string.Join(",", ProcedureCodes.GetWhereFromList(x => x.ProcCode == "D9986" || x.ProcCode == "D9987")
                    .Select(x => x.CodeNum));
                if (!string.IsNullOrEmpty(brokenApptCodes)) command += "AND CodeNum NOT IN (" + brokenApptCodes + ") ";
                command += "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                           + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                           + ") ProcCheck ON ProcCheck.PatNum = patient.PatNum "
                           + "LEFT JOIN ( "
                           + "SELECT ehrmeasureevent.PatNum,MIN(ehrmeasureevent.DateTEvent) AS summaryProvided "
                           + "FROM ehrmeasureevent "
                           + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.ClinicalSummaryProvidedToPt) + " "
                           + "GROUP BY ehrmeasureevent.PatNum "
                           + "ORDER BY NULL "
                           + ") ClinSum ON patient.PatNum=ClinSum.PatNum "
                           + "GROUP BY patient.PatNum, ProcCheck.ProcDate";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region Lab

            case EhrMeasureType.Lab:
                command = "SELECT 1 AS IsOldLab,patient.PatNum,LName,FName,DateTimeOrder,COALESCE(panels.Count,0) AS ResultCount FROM patient "
                          + "INNER JOIN medicalorder ON patient.PatNum=medicalorder.PatNum "
                          + "AND MedOrderType=" + SOut.Int((int) MedicalOrderType.Laboratory) + " "
                          + "AND medicalorder.ProvNum IN(" + SOut.String(provs) + ") "
                          + "AND DATE(DateTimeOrder) BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "LEFT JOIN (SELECT MedicalOrderNum,COUNT(*) AS 'Count' FROM labpanel GROUP BY MedicalOrderNum "
                          + ") panels ON panels.MedicalOrderNum=medicalorder.MedicalOrderNum "
                          + "UNION ALL "
                          + "SELECT 0 AS IsOldLab,patient.PatNum,LName,FName,STR_TO_DATE(ObservationDateTimeStart,'%Y%m%d') AS DateTimeOrder,COALESCE(ehrlabs.Count,0) AS ResultCount FROM patient "
                          + "INNER JOIN ehrlab ON patient.PatNum=ehrlab.PatNum "
                          + "LEFT JOIN (SELECT EhrLabNum, COUNT(*) AS 'Count' FROM ehrlabresult "
                          + "WHERE ehrlabresult.ValueType='NM' OR ehrlabresult.ValueType='SN' "
                          + "OR ehrlabresult.ObservationValueCodedElementID IN (" + _snomedLabResult + ") "
                          + "OR ehrlabresult.ObservationValueCodedElementIDAlt IN (" + _snomedLabResult + ") "
                          + "GROUP BY EhrLabNum "
                          + ") ehrlabs ON ehrlab.EhrLabNum=ehrlabs.EhrLabNum "
                          + "WHERE (CASE WHEN ehrlab.OrderingProviderIdentifierTypeCode='NPI' THEN ehrlab.OrderingProviderID IN(" + SOut.String(provNPIs) + ") " //When the lab is using a NPI number to determine provider.
                          + "WHEN ehrlab.OrderingProviderIdentifierTypeCode='PRN' THEN ( " //When the lab is using provider number to determine provider.
                          + "CASE WHEN ehrlab.OrderingProviderAssigningAuthorityUniversalID=( " //If the AssigningAuthority is OpenDental.
                          + "SELECT IDRoot FROM oidinternal WHERE IDType='Provider' GROUP BY IDType "
                          + ") THEN ehrlab.OrderingProviderID IN(" + SOut.String(provs) + ") END) " //Use the ProvNum to determine provider.
                          + "ELSE FALSE END) " //If the AssigningAuthority is not OpenDental, we have no way to tell who the provider is.
                          + "AND ehrlab.ObservationDateTimeStart BETWEEN DATE_FORMAT(" + SOut.Date(dateStart) + ",'%Y%m%d') AND DATE_FORMAT(" + SOut.Date(dateEnd) + ",'%Y%m%d') "
                          + "AND (CASE WHEN ehrlab.UsiCodeSystemName='LN' THEN ehrlab.UsiID WHEN ehrlab.UsiCodeSystemNameAlt='LN' THEN ehrlab.UsiIDAlt ELSE '' END) "
                          + "NOT IN (SELECT LoincCode FROM loinc WHERE loinc.ClassType LIKE '%RAD%')"; //Not sure if we need this since rad labs shouldnt be set to numeric results
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region Reminders

            case EhrMeasureType.Reminders:
                command = "SELECT patient.PatNum,LName,FName,COALESCE(reminderCount.Count,0) AS reminderCount FROM patient "
                          + "INNER JOIN(SELECT PatNum FROM ( "
                          + "SELECT PatNum, ProcDate FROM procedurelog "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "WHERE ProcStatus=" + SOut.Int((int) ProcStat.C) + " "
                          + "AND ProcDate>" + SOut.Date(dateStart) + "-INTERVAL 2 YEAR "
                          + "AND ProcDate<" + SOut.Date(dateStart) + " GROUP BY PatNum,ProcDate) uniqueprocdates "
                          + "GROUP BY uniqueprocdates.PatNum HAVING COUNT(*)>1) procscomplete ON procscomplete.PatNum=patient.PatNum "
                          + "LEFT JOIN (SELECT ehrmeasureevent.PatNum,COUNT(*) AS 'Count' FROM ehrmeasureevent "
                          + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.ReminderSent) + " "
                          + "AND DATE(ehrmeasureevent.DateTEvent) BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "GROUP BY ehrmeasureevent.PatNum) reminderCount ON reminderCount.PatNum=patient.PatNum "
                          + "WHERE patient.Birthdate > '1880-01-01' " //a birthdate is entered
                          + "AND patient.PatStatus=" + SOut.Int((int) PatientStatus.Patient) + " "
                          + "AND patient.PriProv IN(" + SOut.String(provs) + ") "
                          + "GROUP BY patient.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region Education

            case EhrMeasureType.Education:
                command = "SELECT A.*,COALESCE(edCount.Count,0) AS edCount "
                          + "FROM (SELECT patient.PatNum,LName,FName	FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) A "
                          + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM ehrmeasureevent "
                          + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.EducationProvided) + " "
                          + "GROUP BY PatNum) edCount ON edCount.PatNum=A.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region MedReconcile

            case EhrMeasureType.MedReconcile:
                command = "SELECT ptsRefCnt.*,COALESCE(RecCount,0) AS ReconcileCount "
                          + "FROM (SELECT ptsSeen.*,COUNT(DISTINCT refattach.RefAttachNum) AS RefCount "
                          + "FROM (SELECT patient.PatNum,LName,FName FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum "
                          + "AND ProcStatus=2 AND ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) ptsSeen "
                          + "INNER JOIN refattach ON ptsSeen.PatNum=refattach.PatNum "
                          + "AND RefDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "AND RefType=" + SOut.Int((int) ReferralType.RefFrom) + " AND IsTransitionOfCare=1 "
                          + "GROUP BY ptsSeen.PatNum) ptsRefCnt "
                          + "LEFT JOIN (SELECT ehrmeasureevent.PatNum,COUNT(*) AS RecCount FROM ehrmeasureevent "
                          + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.MedicationReconcile) + " "
                          + "AND DATE(ehrmeasureevent.DateTEvent) BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "GROUP BY ehrmeasureevent.PatNum) ptsRecCount ON ptsRefCnt.PatNum=ptsRecCount.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region SummaryOfCare

            case EhrMeasureType.SummaryOfCare:
                command = "SELECT patient.PatNum,patient.LName,patient.FName,refattach.RefDate, "
                          + "referral.FName AS RefFName,referral.LName AS RefLName,SUM(CASE WHEN ISNULL(socevent.FKey) THEN 0 ELSE 1 END) AS SOCSent "
                          + "FROM refattach "
                          + "INNER JOIN referral ON referral.ReferralNum=refattach.ReferralNum "
                          + "INNER JOIN patient ON patient.PatNum=refattach.PatNum "
                          + "LEFT JOIN ( "
                          + "SELECT ehrmeasureevent.FKey "
                          + "FROM ehrmeasureevent "
                          + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.SummaryOfCareProvidedToDr) + " "
                          + "AND DATE(ehrmeasureevent.DateTEvent) BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + ") socevent ON socevent.FKey=refattach.RefAttachNum "
                          + "WHERE RefDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "AND RefType=" + SOut.Int((int) ReferralType.RefTo) + " AND IsTransitionOfCare=1 "
                          + "AND refattach.ProvNum IN(" + SOut.String(provs) + ") "
                          + "GROUP BY refattach.RefAttachNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region SummaryOfCareElectronic

            case EhrMeasureType.SummaryOfCareElectronic:
                command = "SELECT patient.PatNum,patient.LName,patient.FName,refattach.RefDate, "
                          + "referral.FName AS RefFName,referral.LName AS RefLName,SUM(CASE WHEN ISNULL(socevent.FKey) THEN 0 ELSE 1 END) AS ElecSOCSent "
                          + "FROM refattach "
                          + "INNER JOIN referral ON referral.ReferralNum=refattach.ReferralNum "
                          + "INNER JOIN patient ON patient.PatNum=refattach.PatNum "
                          + "LEFT JOIN ( "
                          + "SELECT ehrmeasureevent.FKey "
                          + "FROM ehrmeasureevent "
                          + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.SummaryOfCareProvidedToDrElectronic) + " "
                          + "AND DATE(ehrmeasureevent.DateTEvent) BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + ") socevent ON socevent.FKey=refattach.RefAttachNum "
                          + "WHERE RefDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "AND RefType=" + SOut.Int((int) ReferralType.RefTo) + " AND IsTransitionOfCare=1 "
                          + "AND refattach.ProvNum IN(" + SOut.String(provs) + ") "
                          + "GROUP BY refattach.RefAttachNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region SecureMessaging

            case EhrMeasureType.SecureMessaging:
                command = "SELECT A.*,secureMessageRead "
                          + "FROM (SELECT patient.PatNum,LName,FName, procedurelog.ProcDate as procDate "
                          + "FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd)
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + " GROUP BY procedurelog.PatNum) A "
                          + "LEFT JOIN (SELECT ehrmeasureevent.PatNum, ehrmeasureevent.DateTEvent as secureMessageRead FROM ehrmeasureevent "
                          + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.SecureMessageFromPat) + " GROUP BY ehrmeasureevent.PatNum) "
                          + "SecureMessage ON A.PatNum=SecureMessage.PatNum "
                          + "";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region FamilyHistory

            case EhrMeasureType.FamilyHistory:
                command = "SELECT * FROM (SELECT patient.PatNum,LName,FName, procedurelog.ProcDate as procDate "
                          + "FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) AS UniquePatsAndProcs "
                          + "LEFT JOIN familyhealth ON UniquePatsAndProcs.PatNum=familyhealth.PatNum "
                          + "GROUP BY UniquePatsAndProcs.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region ElectricNote

            case EhrMeasureType.ElectronicNote:
                //We changed this query to use temp tables so that we could add an index to the table. MySQL 5.6 adds indexes to subqueries
                //on the fly, so we could go back to the commented out version once most of our customers are on 5.6. In my testing the original query
                //took 1 hr 2 min on 5.5, the temp table query took 45 sec on either MySQL version, and the original query took 2 min on MySQL 5.6.
                //cmcgehee 01/23/2017.
                //command="SELECT patient.PatNum,patient.LName,patient.FName,notes.NumNotes "
                //	+"FROM patient "
                //	+"INNER JOIN ( "
                //		+"SELECT procedurelog.PatNum,MAX(procedurelog.ProvNum IN("+POut.String(provs)+") AND procedurelog.ProcStatus="+POut.Int((int)ProcStat.C)+") ComplProcs,"
                //		+"COUNT(DISTINCT procnotesigned.ProcNum) AS NumNotes "
                //		+"FROM procedurelog "
                //		+"LEFT JOIN ( "
                //			+"SELECT procnote.PatNum, procnote.ProcNum "
                //			+"FROM procnote "
                //			+"INNER JOIN ( "
                //				+"SELECT ProcNum,MAX(EntryDateTime) AS NewestNoteDateTime "
                //				+"FROM procnote "
                //				+"GROUP BY ProcNum "
                //				+"ORDER BY NULL"
                //			+") newestnote ON newestnote.ProcNum=procNote.ProcNum AND newestnote.NewestNoteDateTime=procnote.EntryDateTime "
                //			+"WHERE Signature!='' AND Note!='' "
                //			+"GROUP BY procnote.ProcNum "
                //			+"ORDER BY NULL"
                //		+") procnotesigned ON procedurelog.PatNum=procnotesigned.PatNum "
                //		+"WHERE procedurelog.ProcStatus!="+POut.Int((int)ProcStat.D)+" ";
                //brokenApptCodes=string.Join(",",ProcedureCodes.ListDeep.Where(x => x.ProcCode=="D9986" || x.ProcCode=="D9987")
                //	.Select(x => x.CodeNum));
                //if(!string.IsNullOrEmpty(brokenApptCodes)) {
                //	command+="AND CodeNum NOT IN ("+brokenApptCodes+") ";
                //}
                //command+="AND procedurelog.ProcDate BETWEEN "+POut.Date(dateStart)+" AND "+POut.Date(dateEnd)+" "
                //		+"GROUP BY procedurelog.PatNum "
                //		+"ORDER BY NULL"
                //	+") notes ON notes.PatNum=patient.PatNum AND notes.ComplProcs=1";
                //tableRaw=DataCore.GetTable(command);
                command = "CREATE TABLE tempprocnotesigned" + rndStr + @" (
						PatNum BIGINT(20) NOT NULL,
						ProcNum BIGINT(20) NOT NULL,
						INDEX(PatNum)
					)";
                Db.NonQ(command);
                command = "INSERT INTO tempprocnotesigned" + rndStr + @" (
						SELECT procnote.PatNum,procnote.ProcNum 
						FROM procnote 
						INNER JOIN (
							SELECT ProcNum,MAX(EntryDateTime) AS NewestNoteDateTime 
							FROM procnote 
							GROUP BY ProcNum 
							ORDER BY NULL
						) newestnote ON newestnote.ProcNum = procNote.ProcNum 
							AND newestnote.NewestNoteDateTime = procnote.EntryDateTime 
						WHERE Signature != '' 
						AND Note != '' 
						GROUP BY procnote.ProcNum 
						ORDER BY NULL
					)";
                Db.NonQ(command);
                command = @"SELECT patient.PatNum,patient.LName,patient.FName,notes.NumNotes 
						FROM patient 
						INNER JOIN (
							SELECT procedurelog.PatNum,
							MAX(procedurelog.ProvNum IN(" + SOut.String(provs) + ") AND procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.C) + @") ComplProcs,
							COUNT(DISTINCT tempprocnotesigned" + rndStr + @".ProcNum) AS NumNotes 
							FROM procedurelog 
							LEFT JOIN tempprocnotesigned" + rndStr + " ON procedurelog.PatNum=tempprocnotesigned" + rndStr + @".PatNum 
							WHERE procedurelog.ProcStatus !=" + SOut.Int((int) ProcStat.D) + " ";
                brokenApptCodes = string.Join(",", ProcedureCodes.GetWhereFromList(x => x.ProcCode == "D9986" || x.ProcCode == "D9987")
                    .Select(x => x.CodeNum));
                if (!string.IsNullOrEmpty(brokenApptCodes)) command += "AND CodeNum NOT IN (" + brokenApptCodes + ") ";
                command += "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + @"
							GROUP BY procedurelog.PatNum 
							ORDER BY NULL
						) notes ON notes.PatNum=patient.PatNum AND notes.ComplProcs=1";
                tableRaw = DataCore.GetTable(command);
                command = "DROP TABLE IF EXISTS tempprocnotesigned" + rndStr;
                Db.NonQ(command);
                break;

            #endregion

            #region LabImages

            case EhrMeasureType.LabImages:
                command = "SELECT labsTable.PatNum,labsTable.LName,labsTable.FName,labImage.DocNum,STR_TO_DATE(ObservationDateTimeStart,'%Y%m%d') AS ObservationDateTimeStart FROM ( "
                          + "SELECT patient.PatNum,patient.LName,patient.FName,ehrlab.ObservationDateTimeStart,ehrlab.EhrLabNum "
                          + "FROM ehrlab "
                          + "INNER JOIN patient ON ehrlab.PatNum=Patient.PatNum "
                          + "WHERE (CASE WHEN ehrlab.OrderingProviderIdentifierTypeCode='NPI' THEN ehrlab.OrderingProviderID IN(" + SOut.String(provNPIs) + ") " //When the lab is using a NPI number to determine provider.
                          + "WHEN ehrlab.OrderingProviderIdentifierTypeCode='PRN' THEN ( " //When the lab is using provider number to determine provider.
                          + "CASE WHEN ehrlab.OrderingProviderAssigningAuthorityUniversalID=( " //If the AssigningAuthority is OpenDental.
                          + "SELECT IDRoot FROM oidinternal WHERE IDType='Provider' GROUP BY IDType "
                          + ") THEN ehrlab.OrderingProviderID IN(" + SOut.String(provs) + ") END) " //Use the ProvNum to determine provider.
                          + "ELSE FALSE END) " //If the AssigningAuthority is not OpenDental, we have no way to tell who the provider is.
                          + "AND ehrlab.ObservationDateTimeStart BETWEEN DATE_FORMAT(" + SOut.Date(dateStart) + ",'%Y%m%d') AND DATE_FORMAT(" + SOut.Date(dateEnd) + ",'%Y%m%d') "
                          + ") as labsTable "
                          + "INNER JOIN (SELECT DISTINCT EhrLabNum,DocNum FROM ehrlabimage) AS labImage ON labsTable.EhrLabNum=labImage.EhrLabNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            //default:
            //throw new ApplicationException("Type not found: "+mtype.ToString());
        }

        //PatNum, PatientName, Explanation, and Met (X).
        var table = new DataTable("audit");
        DataRow row;
        table.Columns.Add("PatNum");
        table.Columns.Add("patientName");
        table.Columns.Add("explanation");
        table.Columns.Add("met"); //X or empty
        var rows = new List<DataRow>();
        Patient pat;
        string explanation;
        for (var i = 0; i < tableRaw.Rows.Count; i++)
        {
            row = table.NewRow();
            row["PatNum"] = tableRaw.Rows[i]["PatNum"].ToString();
            pat = new Patient();
            pat.LName = tableRaw.Rows[i]["LName"].ToString();
            pat.FName = tableRaw.Rows[i]["FName"].ToString();
            pat.Preferred = "";
            row["patientName"] = pat.GetNameLF();
            row["met"] = "";
            explanation = "";
            switch (mtype)
            {
                #region CPOE_MedOrdersOnly

                case EhrMeasureType.CPOE_MedOrdersOnly:
                    var medOrderStartDate = SIn.Date(tableRaw.Rows[i]["DateStart"].ToString());
                    explanation = "Medication order: " + tableRaw.Rows[i]["MedDescript"] + ", start date: " + medOrderStartDate.ToShortDateString() + ".";
                    if (tableRaw.Rows[i]["IsCpoe"].ToString() == "1") row["met"] = "X";
                    break;

                #endregion

                #region CPOE_LabOrdersOnly

                case EhrMeasureType.CPOE_LabOrdersOnly:
                    var labOrderStartDate = SIn.DateTime(tableRaw.Rows[i]["ObservationDateTimeStart"].ToString());
                    var labIsCpoe = SIn.Bool(tableRaw.Rows[i]["IsCpoe"].ToString());
                    explanation = "Laboratory order: " + labOrderStartDate.ToShortDateString() + " ";
                    if (labIsCpoe)
                    {
                        row["met"] = "X";
                        explanation += " is Cpoe";
                    }
                    else
                    {
                        explanation += " is not Cpoe";
                    }

                    break;

                #endregion

                #region CPOE_RadiologyOrdersOnly

                case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                    var radOrderStartDate = SIn.DateTime(tableRaw.Rows[i]["ObservationDateTimeStart"].ToString());
                    var radIsCpoe = SIn.Bool(tableRaw.Rows[i]["IsCpoe"].ToString());
                    explanation = "Radiology order: " + radOrderStartDate.ToShortDateString() + " ";
                    if (radIsCpoe)
                    {
                        row["met"] = "X";
                        explanation += " is Cpoe";
                    }
                    else
                    {
                        explanation += " is not Cpoe";
                    }

                    break;

                #endregion

                #region Rx

                case EhrMeasureType.Rx:
                    var sendStatus = (RxSendStatus) SIn.Int(tableRaw.Rows[i]["SendStatus"].ToString());
                    var rxDate = SIn.Date(tableRaw.Rows[i]["rxDate"].ToString());
                    if (sendStatus == RxSendStatus.SentElect)
                    {
                        explanation = rxDate.ToShortDateString() + " Rx sent electronically.";
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = rxDate.ToShortDateString() + " Rx not sent electronically.";
                    }

                    break;

                #endregion

                #region Demographics

                case EhrMeasureType.Demographics:
                    if (SIn.Date(tableRaw.Rows[i]["Birthdate"].ToString()).Year < 1880) explanation += "birthdate"; //missing
                    if (tableRaw.Rows[i]["Language"].ToString() == "")
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "language";
                    }

                    if (SIn.Int(tableRaw.Rows[i]["Gender"].ToString()) == (int) PatientGender.Unknown)
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "gender";
                    }

                    //if(PatientRaces.GetForPatient(PIn.Long(row["PatNum"].ToString())).Count==0) {
                    //	if(explanation!="") {
                    //		explanation+=", ";
                    //	}
                    //	explanation+="race, ethnicity";
                    //}
                    if (SIn.Int(tableRaw.Rows[i]["HasRace"].ToString()) == 0)
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "race";
                    }

                    if (SIn.Int(tableRaw.Rows[i]["HasEthnicity"].ToString()) == 0)
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "ethnicity";
                    }

                    if (explanation == "")
                    {
                        explanation = "All demographic elements recorded";
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = "Missing: " + explanation;
                    }

                    break;

                #endregion

                #region VitalSigns

                case EhrMeasureType.VitalSigns:
                    if (tableRaw.Rows[i]["hwCount"].ToString() == "0") explanation += "height, weight";
                    if (tableRaw.Rows[i]["bpCount"].ToString() == "0")
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "blood pressure";
                    }

                    if (explanation == "")
                    {
                        explanation = "Vital signs entered";
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = "Missing: " + explanation;
                    }

                    break;

                #endregion

                #region VitalSignsBMIOnly

                case EhrMeasureType.VitalSignsBMIOnly:
                    if (tableRaw.Rows[i]["hwCount"].ToString() == "0") explanation += "height, weight";
                    if (explanation == "")
                    {
                        explanation = "Vital signs entered";
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = "Missing: " + explanation;
                    }

                    break;

                #endregion

                #region VitalSignsBPOnly

                case EhrMeasureType.VitalSignsBPOnly:
                    if (tableRaw.Rows[i]["bpCount"].ToString() == "0")
                    {
                        explanation = "Missing: blood pressure";
                    }
                    else
                    {
                        explanation = "Vital signs entered";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region Smoking

                case EhrMeasureType.Smoking:
                    var smokeSnoMed = tableRaw.Rows[i]["SmokingSnoMed"].ToString();
                    if (smokeSnoMed == "449868002" //Current every day smoker
                        || smokeSnoMed == "428041000124106" //Current some day smoker
                        || smokeSnoMed == "8517006" //Former smoker
                        || smokeSnoMed == "266919005" //Never smoker
                        || smokeSnoMed == "77176002" //Smoker, current status unknown
                        || smokeSnoMed == "266927001" //Unknown if ever smoked
                        || smokeSnoMed == "428071000124103" //Heavy tobacco smoker
                        || smokeSnoMed == "428061000124105") //Light tobacco smoker
                    {
                        explanation = "Smoking status entered.";
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = "Smoking status not entered.";
                    }

                    break;

                #endregion

                #region ElectronicCopyAccess

                case EhrMeasureType.ElectronicCopyAccess:
                    var visitDate = SIn.Date(tableRaw.Rows[i]["leastRecentDate"].ToString());
                    var deadlineDate = SIn.Date(tableRaw.Rows[i]["leastRecentDate"].ToString());
                    var providedDate = SIn.Date(tableRaw.Rows[i]["dateProvided"].ToString());
                    deadlineDate = deadlineDate.AddDays(4);
                    if (visitDate.DayOfWeek > DayOfWeek.Tuesday) deadlineDate = deadlineDate.AddDays(2);
                    if (providedDate <= deadlineDate && providedDate.Year > 1880)
                    {
                        explanation = "Online access provided before " + deadlineDate.ToShortDateString();
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = visitDate.ToShortDateString() + " no online access provided";
                    }

                    break;

                #endregion

                #region ElectronicCopy

                case EhrMeasureType.ElectronicCopy:
                    var dateRequested = SIn.Date(tableRaw.Rows[i]["dateRequested"].ToString());
                    if (dateRequested > dateEnd)
                    {
                        explanation = "No requests in or before specified period.";
                    }
                    else if (dateRequested.Year > 1880)
                    {
                        explanation = "Requested on " + dateRequested.ToShortDateString();
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region ClinicalSummaries

                case EhrMeasureType.ClinicalSummaries:
                    var procDate = SIn.Date(tableRaw.Rows[i]["procDate"].ToString());
                    var deadlineDateClinSum = procDate.AddDays(1);
                    if (procDate.DayOfWeek == DayOfWeek.Friday) deadlineDateClinSum = deadlineDateClinSum.AddDays(2);
                    var summaryProvidedDate = SIn.Date(tableRaw.Rows[i]["summaryProvided"].ToString());
                    if (summaryProvidedDate == DateTime.MinValue)
                    {
                        explanation = procDate.ToShortDateString() + " no summary provided to patient";
                    }
                    else if (summaryProvidedDate <= deadlineDateClinSum)
                    {
                        explanation = procDate.ToShortDateString() + " summary provided to patient";
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = procDate.ToShortDateString() + " summary provided to patient after more than one buisness day";
                    }

                    break;

                #endregion

                #region Lab

                case EhrMeasureType.Lab:
                    var resultCount = SIn.Int(tableRaw.Rows[i]["ResultCount"].ToString());
                    var isOldLab = SIn.Bool(tableRaw.Rows[i]["IsOldLab"].ToString());
                    var dateOrder = SIn.Date(tableRaw.Rows[i]["DateTimeOrder"].ToString());
                    if (resultCount == 0)
                    {
                        explanation += dateOrder.ToShortDateString() + " results not attached.";
                        explanation += isOldLab ? " (2011 edition)" : "";
                    }
                    else
                    {
                        explanation = dateOrder.ToShortDateString() + " results attached.";
                        explanation += isOldLab ? " (2011 edition)" : "";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region Reminders

                case EhrMeasureType.Reminders:
                    if (tableRaw.Rows[i]["reminderCount"].ToString() == "0")
                    {
                        explanation = "No reminders sent";
                    }
                    else
                    {
                        explanation = "Reminders sent";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region Education

                case EhrMeasureType.Education:
                    if (tableRaw.Rows[i]["edCount"].ToString() == "0")
                    {
                        explanation = "No education resources";
                    }
                    else
                    {
                        explanation = "Education resources provided";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region MedReconcile

                case EhrMeasureType.MedReconcile:
                    var refCount = SIn.Int(tableRaw.Rows[i]["RefCount"].ToString()); //this will always be greater than zero
                    var reconcileCount = SIn.Int(tableRaw.Rows[i]["ReconcileCount"].ToString());
                    if (reconcileCount < refCount)
                    {
                        explanation = "Transitions of Care:" + refCount + ", Reconciles:" + reconcileCount;
                    }
                    else
                    {
                        explanation = "Reconciles performed for each transition of care.";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region SummaryOfCare

                case EhrMeasureType.SummaryOfCare:
                    var socSent = SIn.Int(tableRaw.Rows[i]["SOCSent"].ToString());
                    var refDate = SIn.DateTime(tableRaw.Rows[i]["RefDate"].ToString());
                    var refLName = SIn.String(tableRaw.Rows[i]["RefLName"].ToString());
                    var refFName = SIn.String(tableRaw.Rows[i]["RefFName"].ToString());
                    if (socSent < 1)
                    {
                        explanation = "Referral on: " + refDate.ToShortDateString() + " to " + refLName + ", " + refFName + " not sent summary of care.";
                    }
                    else
                    {
                        explanation = "Referral on: " + refDate.ToShortDateString() + " to " + refLName + ", " + refFName + " sent summary of care.";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region SummaryOfCareElectronic

                case EhrMeasureType.SummaryOfCareElectronic:
                    var elecSOCSent = SIn.Int(tableRaw.Rows[i]["ElecSOCSent"].ToString());
                    var elecRefDate = SIn.DateTime(tableRaw.Rows[i]["RefDate"].ToString());
                    var elecRefLName = SIn.String(tableRaw.Rows[i]["RefLName"].ToString());
                    var elecRefFName = SIn.String(tableRaw.Rows[i]["RefFName"].ToString());
                    if (elecSOCSent < 1)
                    {
                        explanation = "Referral on: " + elecRefDate.ToShortDateString() + " to " + elecRefLName + ", " + elecRefFName + " not sent electronic summary of care.";
                    }
                    else
                    {
                        explanation = "Referral on: " + elecRefDate.ToShortDateString() + " to " + elecRefLName + ", " + elecRefFName + " sent electronic summary of care.";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region SecureMessaging

                case EhrMeasureType.SecureMessaging:
                    if (SIn.DateTime(tableRaw.Rows[i]["secureMessageRead"].ToString()).Year > 1880) row["met"] = "X";
                    break;

                #endregion

                #region FamilyHistory

                case EhrMeasureType.FamilyHistory:
                    if (SIn.Long(tableRaw.Rows[i]["FamilyHealthNum"].ToString()) > 0) row["met"] = "X";
                    break;

                #endregion

                #region ElectricNote

                case EhrMeasureType.ElectronicNote:
                    if (SIn.Long(tableRaw.Rows[i]["NumNotes"].ToString()) > 0) row["met"] = "X";
                    break;

                #endregion

                #region LabImages

                case EhrMeasureType.LabImages:
                    var labImageStartDate = SIn.Date(tableRaw.Rows[i]["ObservationDateTimeStart"].ToString());
                    var docNum = SIn.Long(tableRaw.Rows[i]["DocNum"].ToString());
                    explanation = "Laboratory order: " + labImageStartDate.ToShortDateString() + " ";
                    if (docNum > 0)
                    {
                        row["met"] = "X";
                        explanation += " image attached.";
                    }
                    else
                    {
                        explanation += " image not attached.";
                    }

                    break;

                #endregion

                //default:
                //throw new ApplicationException("Type not found: "+mtype.ToString());
            }

            row["explanation"] = explanation;
            rows.Add(row);
        }

        for (var i = 0; i < rows.Count; i++) table.Rows.Add(rows[i]);
        return table;
    }

    ///<summary>Returns the explanation of the numerator based on the EHR certification documents.</summary>
    private static string GetNumeratorExplainMu2(EhrMeasureType mtype)
    {
        switch (mtype)
        {
            case EhrMeasureType.CPOE_MedOrdersOnly:
                return "The number of medication orders entered by the Provider during the reporting period using CPOE.";
            case EhrMeasureType.CPOE_LabOrdersOnly:
                return "The number of lab orders entered by the Provider during the reporting period using CPOE.";
            case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                return "The number of radiology orders entered by the Provider during the reporting period using CPOE.";
            case EhrMeasureType.Rx:
                return "Permissible prescriptions transmitted electronically.";
            case EhrMeasureType.Demographics:
                return "Patients with all required demographic elements recorded as structured data: language, gender, race, ethnicity, and birthdate.";
            case EhrMeasureType.VitalSigns:
                return "Patients with height, weight, and blood pressure recorded.";
            case EhrMeasureType.VitalSignsBMIOnly:
                return "Patients with height and weight recorded.";
            case EhrMeasureType.VitalSignsBPOnly:
                return "Patients with blood pressure recorded.";
            case EhrMeasureType.Smoking:
                return "Patients with smoking status recorded.";
            case EhrMeasureType.ElectronicCopyAccess:
                return "Electronic copy received within 4 business days.";
            case EhrMeasureType.ElectronicCopy:
                return "The number of unique patients in the denominator who have viewed online, downloaded, or transmitted to a third party the patient's health information.";
            case EhrMeasureType.ClinicalSummaries:
                return "Number of office visits in the denominator where the patient or a patient-authorized representative is provided a clinical summary of their visit within one business day.";
            case EhrMeasureType.Lab:
                return "Lab results entered.";
            case EhrMeasureType.Reminders:
                return "Number of patients in the denominator who were sent a reminder per patient preference when available during the EHR reporting period.";
            case EhrMeasureType.Education:
                return "Patients provided patient-specific education resources, not dependent on requests.";
            case EhrMeasureType.MedReconcile:
                return "Number of transitions of care in the denominator where medication reconciliation was performed.";
            case EhrMeasureType.SummaryOfCare:
                return "Number of transitions of care and referrals in the denominator where a summary of care record was provided.";
            case EhrMeasureType.SummaryOfCareElectronic:
                return "Number of transitions of care and referrals in the denominator where a summary of care record was electronically transmitted";
            case EhrMeasureType.SecureMessaging:
                return "The number of patients in the denominator who send a secure electronic message to the EP that is received using the electronic messaging function of CEHRT during the EHR reporting period.";
            case EhrMeasureType.FamilyHistory:
                return "The number of patients in the denominator with a structured data entry for one or more first-degree relatives.";
            case EhrMeasureType.ElectronicNote:
                return "The number of unique patients in the denominator who have at least one electronic progress note from an eligible professional recorded as text searchable data.";
            case EhrMeasureType.LabImages:
                return "The number of results in the denominator that are accessible through CEHRT.";
        }

        return "";
        //throw new ApplicationException("Type not found: "+mtype.ToString());
    }

    ///<summary>Returns the explanation of the denominator based on the EHR certification documents.</summary>
    private static string GetDenominatorExplainMu2(EhrMeasureType mtype)
    {
        switch (mtype)
        {
            case EhrMeasureType.CPOE_MedOrdersOnly:
                return "The number of medication orders created by the Provider during the reporting period.";
            case EhrMeasureType.CPOE_LabOrdersOnly:
                return "The number of lab orders created by the Provider during the reporting period.";
            case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                return "The number of radiology orders created by the Provider during the reporting period.";
            case EhrMeasureType.Rx:
                return "All permissible prescriptions by the Provider during the reporting period.";
            case EhrMeasureType.Demographics:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.VitalSigns:
                return "All unique patients (age 3 and over for blood pressure) with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.VitalSignsBMIOnly:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.VitalSignsBPOnly:
                return "All unique patients age 3 and over with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.Smoking:
                return "All unique patients 13 years or older with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.ElectronicCopyAccess:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.ElectronicCopy:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.ClinicalSummaries:
                return "All office visits during the reporting period.  An office visit is calculated as any number of completed procedures by the Provider for a given date.";
            case EhrMeasureType.Lab:
                return "All lab orders by the Provider during the reporting period.";
            case EhrMeasureType.Reminders:
                return "Number of unique patients who have had two or more office visits with the EP in the 24 months prior to the beginning of the EHR reporting period.";
            case EhrMeasureType.Education:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.MedReconcile:
                return "Number of incoming transitions of care from another provider during the reporting period.";
            case EhrMeasureType.SummaryOfCare:
                return "Number of outgoing transitions of care and referrals during the reporting period.";
            case EhrMeasureType.SummaryOfCareElectronic:
                return "Number of outgoing transitions of care and referrals during the reporting period.";
            case EhrMeasureType.SecureMessaging:
                return "Number of unique patients seen by the EP during the EHR reporting period.";
            case EhrMeasureType.FamilyHistory:
                return "Number of unique patients seen by the EP during the EHR reporting period.";
            case EhrMeasureType.ElectronicNote:
                return "Number of unique patients with at least one office visit during the EHR reporting period for EPs during the EHR reporting period.";
            case EhrMeasureType.LabImages:
                return "Number of tests whose result is one or more images ordered by the EP during the EHR reporting period.";
        }

        return "";
        //throw new ApplicationException("Type not found: "+mtype.ToString());
    }

    ///<summary>Returns the explanation of the exclusion if there is one, if none returns 'No exclusions.'.</summary>
    private static string GetExclusionExplainMu2(EhrMeasureType mtype)
    {
        switch (mtype)
        {
            case EhrMeasureType.CPOE_MedOrdersOnly:
            case EhrMeasureType.CPOE_LabOrdersOnly:
            case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                return "Any Provider who writes fewer than 100 medication, radiology, or laboratory orders during the EHR reporting period.";
            case EhrMeasureType.Rx:
                return @"1. Any Provider who writes fewer than 100 prescriptions during the reporting period.
2. Any Provider who does not have a pharmacy within their organization and there are no pharmacies that accept electronic prescriptions within 10 miles of the practice at the start of the reporting period.";
            case EhrMeasureType.Demographics:
                return "No exclusions.";
            case EhrMeasureType.VitalSigns:
                return @"1. Any Provider who sees no patients 3 years or older is excluded from recording blood pressure.
2. Any Provider who believes that all three vital signs of height, weight, and blood pressure have no relevance to their scope of practice is excluded from recording them.
3. Any Provider who believes that height and weight are relevant to their scope of practice, but blood pressure is not, is excluded from recording blood pressure.
4. Any Provider who believes that blood pressure is relevant to their scope of practice, but height and weight are not, is excluded from recording height and weight.";
            case EhrMeasureType.VitalSignsBMIOnly:
                return "Any Provider who believes that height and weight are not relevant to their scope of practice is excluded from recording them.";
            case EhrMeasureType.VitalSignsBPOnly:
                return @"1. Any Provider who sees no patients 3 years or older is excluded from recording blood pressure.
2. Any Provider who believes that blood pressure is not relevant to their scope of practice is excluded from recording it.";
            case EhrMeasureType.Smoking:
                return "Any Provider who sees no patients 13 years or older during the reporting period.";
            case EhrMeasureType.ElectronicCopyAccess:
                return "Any Provider who neither orders nor creates any of the information listed for inclusion as part of both measures, except for Patient name and Provider's name and office contact information.";
            case EhrMeasureType.ElectronicCopy:
                return @"1. Any Provider who neither orders nor creates any of the information listed for inclusion as part of both measures, except for Patient name and Provider's name and office contact information.
2. Any Provider who conducts 50% or more of his or her patient encounters in a county that does not have 50% or more of its housing units with 3Mbps broadband availability according to the latest information available from the FCC on the first day of the EHR reporting period.";
            case EhrMeasureType.ClinicalSummaries:
                return "Any Provider who has no completed procedures during the reporting period.";
            case EhrMeasureType.Lab:
                return "Any Provider who orders no lab tests whose results are either in a positive/negative or numeric format during the reporting period.";
            case EhrMeasureType.Reminders:
                return "Any Provider who has had no office visits in the 24 months before the EHR reporting period.";
            case EhrMeasureType.Education:
                return "Any Provider who has no office visits during the EHR reporting period.";
            case EhrMeasureType.MedReconcile:
                return "Any Provider who was not the recipient of any transitions of care during the EHR reporting period.";
            case EhrMeasureType.SummaryOfCare:
                return "Any Provider who transfers a patient to another setting or refers a patient to another provider less than 100 times during the EHR reporting period is excluded from all three measures.";
            case EhrMeasureType.SummaryOfCareElectronic:
                return "Any Provider who transfers a patient to another setting or refers a patient to another provider less than 100 times during the EHR reporting period is excluded from all three measures.";
            case EhrMeasureType.SecureMessaging:
                return "Any EP who has no office visits during the EHR reporting period, or any EP who conducts 50% or more of his or her patient encounters in a county that does not have 50% or more of its housing units with 3Mbps broadband availability according to the latest information available from the FCC on the first day of the EHR reporting period.";
            case EhrMeasureType.FamilyHistory:
                return "Any EP who has no office visits during the EHR reporting period.";
            case EhrMeasureType.ElectronicNote:
                return "No exclusion.";
            case EhrMeasureType.LabImages:
                return "Any EP who orders less than 100 tests whose result is an image during the EHR reporting period; or any EP who has no access to electronic imaging results at the start of the EHR reporting period.";
        }

        return "";
        //throw new ApplicationException("Type not found: "+mtype.ToString());
    }

    /// <summary>
    ///     Returns the count the office will need to report in order to attest to being excluded from this measure.  Will
    ///     return -1 if there is no applicable count for this measure.
    /// </summary>
    private static int GetExclusionCountMu2(EhrMeasureType mtype, DateTime dateStart, DateTime dateEnd, long provNum)
    {
        var retval = 0;
        var command = "";
        var tableRaw = new DataTable();
        var provCur = Providers.GetProv(provNum);
        command = "SELECT GROUP_CONCAT(provider.ProvNum) FROM provider WHERE provider.ProvNum IN "
                  + "(SELECT ProvNum FROM provider pv, ehrprovkey epk WHERE pv.LName=epk.LName AND pv.FName=epk.FName "
                  + "AND epk.LName='" + SOut.String(provCur.LName) + "' AND epk.FName='" + SOut.String(provCur.FName) + "')";
        var provs = DataCore.GetScalar(command);
        switch (mtype)
        {
            #region CPOE_MedOrdersOnly

            case EhrMeasureType.CPOE_MedOrdersOnly:
                command = "SELECT COUNT(DISTINCT rxpat.RxNum) AS 'Count' "
                          + "FROM patient "
                          + "INNER JOIN rxpat ON rxpat.PatNum=patient.PatNum "
                          + "AND rxpat.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND RxDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd);
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region CPOE_LabOrdersOnly

            case EhrMeasureType.CPOE_LabOrdersOnly:
                command = "SELECT COUNT(DISTINCT ehrlab.EhrLabNum) AS 'Count' "
                          + "FROM patient "
                          + "INNER JOIN ehrlab ON ehrlab.PatNum=patient.PatNum "
                          + "AND ehrlab.OrderingProviderID IN(" + SOut.String(provs) + ")	"
                          + "AND ObservationDateTimeStart BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd)
                          + " INNER JOIN loinc on ehrlab.UsiID=loinc.LoincCode"
                          + " AND loinc.ClassType NOT LIKE '%rad%'";
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region CPOE_RadiologyOrdersOnly

            case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                var countRadOrders = 0;
                command = "SELECT COUNT(DISTINCT ehrlab.EhrLabNum) AS 'Count' "
                          + "FROM patient "
                          + "INNER JOIN ehrlab ON ehrlab.PatNum=patient.PatNum "
                          + "AND ehrlab.OrderingProviderID IN(" + SOut.String(provs) + ")	"
                          + "AND ObservationDateTimeStart BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd)
                          + " INNER JOIN loinc on ehrlab.UsiID=loinc.LoincCode"
                          + " AND loinc.ClassType LIKE '%rad%'";
                countRadOrders = SIn.Int(DataCore.GetScalar(command));
                var dateStartRad154 = PrefC.GetDate(PrefName.RadiologyDateStartedUsing154);
                //Only count radiology orders via the procedurelog table if the date that the office updated to v15.4.1 is less than the end date.
                if (dateStartRad154 < dateEnd.Date)
                {
                    var dateStartStr = SOut.Date(dateStart);
                    //We need to use a start date of whichever date is greater, dateStart or the date the the office updated to v15.4.1.
                    if (dateStartRad154 > dateStart)
                        //The date that the office updated past 15.4.1 is greater than the reporting period's start date.
                        //Use the preference date instead so that the office isn't penalized for radiology orders prior to updating past v15.4.1.
                        dateStartStr = SOut.Date(dateStartRad154);
                    //As of v15.4 we started storing radiology orders at the procedure level.
                    //Add all the procedure radiology orders to the count of radiology lab orders above.
                    command = "SELECT COUNT(*) AS 'Count' "
                              + "FROM procedurelog "
                              + "INNER JOIN procedurecode ON procedurelog.CodeNum=procedurecode.CodeNum AND procedurecode.IsRadiology=1 "
                              + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                              + "WHERE procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.C) + " "
                              + "AND procedurelog.ProvNum IN (" + SOut.String(provs) + ") "
                              + "AND procedurelog.DateEntryC BETWEEN " + dateStartStr + " AND " + SOut.Date(dateEnd);
                    countRadOrders += SIn.Int(Db.GetCount(command));
                }

                return countRadOrders;

            #endregion

            #region Rx

            case EhrMeasureType.Rx:
                command = "SELECT COUNT(DISTINCT rxpat.RxNum) AS 'Count' "
                          + "FROM patient "
                          + "INNER JOIN rxpat ON rxpat.PatNum=patient.PatNum "
                          + "AND rxpat.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND RxDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd);
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region Demographics

            case EhrMeasureType.Demographics:
                return retval = -1;

            #endregion

            #region VitalSigns

            case EhrMeasureType.VitalSigns:
                command = "SELECT SUM((CASE WHEN A.Birthdate <= (A.LastVisitInDateRange-INTERVAL 3 YEAR) THEN 1 ELSE 0 END)) AS 'Count' "
                          + "FROM (SELECT Birthdate,MAX(procedurelog.ProcDate) AS LastVisitInDateRange "
                          + "FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) A ";
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region VitalSignsBMIOnly

            case EhrMeasureType.VitalSignsBMIOnly:
                return retval = -1;

            #endregion

            #region VitalSignsBPOnly

            case EhrMeasureType.VitalSignsBPOnly:
                command = "SELECT SUM((CASE WHEN A.Birthdate <= (A.LastVisitInDateRange-INTERVAL 3 YEAR) THEN 1 ELSE 0 END)) AS 'Count' "
                          + "FROM (SELECT Birthdate,MAX(procedurelog.ProcDate) AS LastVisitInDateRange "
                          + "FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) A ";
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region Smoking

            case EhrMeasureType.Smoking:
                command = "SELECT SUM((CASE WHEN A.Birthdate <= (A.LastVisitInDateRange-INTERVAL 13 YEAR) THEN 1 ELSE 0 END)) AS 'Count' "
                          + "FROM (SELECT Birthdate,MAX(procedurelog.ProcDate) AS LastVisitInDateRange "
                          + "FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) A ";
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region ElectronicCopyAccess

            case EhrMeasureType.ElectronicCopyAccess:
                return retval = -1;

            #endregion

            #region ElectronicCopy

            case EhrMeasureType.ElectronicCopy:
                return retval = -1;

            #endregion

            #region ClinicalSummaries

            case EhrMeasureType.ClinicalSummaries:
                //Excluded if no completed procedures during the reporting period
                command = "SELECT COUNT(DISTINCT ProcNum) FROM procedurelog "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "WHERE ProcStatus=2 AND ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " ";
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region Lab

            case EhrMeasureType.Lab:
                command = "SELECT COUNT(DISTINCT ehrlab.EhrLabNum) AS 'Count' "
                          + "FROM patient "
                          + "INNER JOIN ehrlab ON ehrlab.PatNum=patient.PatNum "
                          + "AND ehrlab.OrderingProviderID IN(" + SOut.String(provs) + ")	"
                          + "AND ObservationDateTimeStart BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd)
                          + " INNER JOIN loinc on ehrlab.UsiID=loinc.LoincCode"
                          + " AND loinc.ClassType NOT LIKE '%rad%'";
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region Reminders

            case EhrMeasureType.Reminders:
                //Excluded if Provider has had no office visits in the 24 months before the EHR reporting period.
                command = "SELECT COUNT(DISTINCT ProcNum) FROM procedurelog "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "WHERE ProcStatus=2 AND ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart.AddMonths(-24)) + " AND " + SOut.Date(dateStart) + " ";
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region Education

            case EhrMeasureType.Education:
                //Excluded if no completed procedures during the reporting period
                command = "SELECT COUNT(DISTINCT ProcNum) FROM procedurelog "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "WHERE ProcStatus=2 AND ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " ";
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region MedReconcile

            case EhrMeasureType.MedReconcile:
                return retval = -1;

            #endregion

            #region SummaryOfCare

            case EhrMeasureType.SummaryOfCare:
                command = "SELECT COUNT(referral.ReferralNum) FROM referral "
                          + "INNER JOIN provider ON provider.NationalProvID=referral.NationalProvID "
                          + "AND provider.ProvNum=" + SOut.Long(provNum) + " "
                          + "LEFT JOIN refattach ON referral.referralNum=refattach.referralNum "
                          + "AND RefDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd);
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region SummaryOfCareElectronic

            case EhrMeasureType.SummaryOfCareElectronic:
                command = "SELECT COUNT(referral.ReferralNum) FROM referral "
                          + "INNER JOIN provider ON provider.NationalProvID=referral.NationalProvID "
                          + "AND provider.ProvNum=" + SOut.Long(provNum) + " "
                          + "LEFT JOIN refattach ON referral.referralNum=refattach.referralNum "
                          + "AND RefDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd);
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region SecureMessaging

            case EhrMeasureType.SecureMessaging:
                //Excluded if no completed procedures during the reporting period
                command = "SELECT COUNT(DISTINCT ProcNum) FROM procedurelog "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "WHERE ProcStatus=2 AND ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " ";
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region FamilyHistory

            case EhrMeasureType.FamilyHistory:
                //Excluded if no completed procedures during the reporting period
                command = "SELECT COUNT(DISTINCT ProcNum) FROM procedurelog "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "WHERE ProcStatus=2 AND ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " ";
                return retval = SIn.Int(DataCore.GetScalar(command));

            #endregion

            #region ElectricNote

            case EhrMeasureType.ElectronicNote:
                return retval = -1;

            #endregion

            #region LabImages

            case EhrMeasureType.LabImages:
                //This is currently not possible in OD and is always excluded
                return retval = -1;

            #endregion
        }

        return -1;
        //throw new ApplicationException("Type not found: "+mtype.ToString());
    }

    /// <summary>
    ///     Returns the description of what the count displayed is.  May be count of patients under a certain age or
    ///     number of Rx's written, this will be the label that describes the number.
    /// </summary>
    private static string GetExclusionCountDescriptMu2(EhrMeasureType mtype)
    {
        //switch(mtype) {
        //	case EhrMeasureType.Demographics:
        //	case EhrMeasureType.Education:
        //	case EhrMeasureType.VitalSignsBMIOnly:
        //	case EhrMeasureType.ElectronicCopy:
        //	case EhrMeasureType.Lab:
        //	case EhrMeasureType.MedReconcile:
        //	case EhrMeasureType.SummaryOfCare:
        //		return "";
        //	case EhrMeasureType.CPOE_MedOrdersOnly:
        //	case EhrMeasureType.Rx:
        //		return "Count of prescriptions entered during the reporting period.";
        //	case EhrMeasureType.CPOE_LabOrdersOnly:
        //		return "Count of labs entered during the reporting period.";
        //	case EhrMeasureType.CPOE_RadiologyOrdersOnly:
        //		return "Count of radiology labs entered during the reporting period.";
        //	case EhrMeasureType.VitalSigns:
        //	case EhrMeasureType.VitalSignsBPOnly:
        //		return "Count of patients seen who were 3 years or older at the time of their last visit during the reporting period.";
        //	case EhrMeasureType.Smoking:
        //		return "Count of patients seen who were 13 years or older at the time of their last visit during the reporting period.";
        //	case EhrMeasureType.ClinicalSummaries:
        //		return "Count of procedures completed during the reporting period.";
        //	case EhrMeasureType.Reminders:
        //		return "Count of procedures completed during the 24 months prior to the reporting period.";
        //}
        //return "";
        switch (mtype)
        {
            case EhrMeasureType.CPOE_MedOrdersOnly:
                return "Count of prescriptions entered during the reporting period.";
            case EhrMeasureType.CPOE_LabOrdersOnly:
                return "Count of non-radiology labs entered during the reporting period.";
            case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                return "Count of radiology labs entered during the reporting period.";
            case EhrMeasureType.Rx:
                return "Count of prescriptions entered during the reporting period.";
            case EhrMeasureType.Demographics:
                return "";
            case EhrMeasureType.VitalSigns:
                return "Count of patients seen who were 3 years or older at the time of their last visit during the reporting period.";
            case EhrMeasureType.VitalSignsBMIOnly:
                return "";
            case EhrMeasureType.VitalSignsBPOnly:
                return "Count of patients seen who were 3 years or older at the time of their last visit during the reporting period.";
            case EhrMeasureType.Smoking:
                return "Count of patients seen who were 13 years or older at the time of their last visit during the reporting period.";
            case EhrMeasureType.ElectronicCopyAccess:
                return "";
            case EhrMeasureType.ElectronicCopy:
                return "";
            case EhrMeasureType.ClinicalSummaries:
                return "Count of procedures completed during the reporting period.";
            case EhrMeasureType.Lab:
                return "Count of labs entered during the reporting period.";
            case EhrMeasureType.Reminders:
                return "Count of procedures completed during the 24 months prior to the reporting period.";
            case EhrMeasureType.Education:
                return "Count of procedures completed during the reporting period.";
            case EhrMeasureType.MedReconcile:
                return "";
            case EhrMeasureType.SummaryOfCare:
                return "Count of transitions of care completed during the reporting period.";
            case EhrMeasureType.SummaryOfCareElectronic:
                return "Count of transitions of care completed during the reporting period.";
            case EhrMeasureType.SecureMessaging:
                return "Count of procedures completed during the reporting period.";
            case EhrMeasureType.FamilyHistory:
                return "Count of procedures completed during the reporting period.";
            case EhrMeasureType.ElectronicNote:
                return "";
            case EhrMeasureType.LabImages:
                return "";
        }

        return "";
        //throw new ApplicationException("Type not found: "+mtype.ToString());
    }


    private static List<EhrMeasure> GetMU2List()
    {
        var command = "SELECT * FROM ehrmeasure "
                      + "WHERE MeasureType IN ("
                      + SOut.Int((int) EhrMeasureType.CPOE_MedOrdersOnly) + ","
                      + SOut.Int((int) EhrMeasureType.CPOE_LabOrdersOnly) + ","
                      + SOut.Int((int) EhrMeasureType.CPOE_RadiologyOrdersOnly) + ","
                      + SOut.Int((int) EhrMeasureType.Rx) + ","
                      + SOut.Int((int) EhrMeasureType.Demographics) + ","
                      + SOut.Int((int) EhrMeasureType.VitalSigns) + ","
                      + SOut.Int((int) EhrMeasureType.VitalSignsBMIOnly) + ","
                      + SOut.Int((int) EhrMeasureType.VitalSignsBPOnly) + ","
                      + SOut.Int((int) EhrMeasureType.Smoking) + ","
                      + SOut.Int((int) EhrMeasureType.ElectronicCopyAccess) + ","
                      + SOut.Int((int) EhrMeasureType.ElectronicCopy) + ","
                      + SOut.Int((int) EhrMeasureType.ClinicalSummaries) + ","
                      + SOut.Int((int) EhrMeasureType.Lab) + ","
                      + SOut.Int((int) EhrMeasureType.Reminders) + ","
                      + SOut.Int((int) EhrMeasureType.Education) + ","
                      + SOut.Int((int) EhrMeasureType.MedReconcile) + ","
                      + SOut.Int((int) EhrMeasureType.SummaryOfCare) + ","
                      + SOut.Int((int) EhrMeasureType.SummaryOfCareElectronic) + ","
                      + SOut.Int((int) EhrMeasureType.SecureMessaging) + ","
                      + SOut.Int((int) EhrMeasureType.FamilyHistory) + ","
                      + SOut.Int((int) EhrMeasureType.ElectronicNote) + ","
                      + SOut.Int((int) EhrMeasureType.LabImages) + ","
                      + SOut.Int((int) EhrMeasureType.DrugDrugInteractChecking) + ","
                      + SOut.Int((int) EhrMeasureType.DrugFormularyChecking) + ","
                      + SOut.Int((int) EhrMeasureType.ProtectElectHealthInfo) + ","
                      + SOut.Int((int) EhrMeasureType.ImmunizationRegistries) + ","
                      + SOut.Int((int) EhrMeasureType.SyndromicSurveillance) + ","
                      + SOut.Int((int) EhrMeasureType.PatientList) + ","
                      + SOut.Int((int) EhrMeasureType.ClinicalInterventionRules) + ") ";
            command += "ORDER BY FIELD(MeasureType,"
                       + SOut.Int((int) EhrMeasureType.CPOE_MedOrdersOnly) + ","
                       + SOut.Int((int) EhrMeasureType.CPOE_LabOrdersOnly) + ","
                       + SOut.Int((int) EhrMeasureType.CPOE_RadiologyOrdersOnly) + ","
                       + SOut.Int((int) EhrMeasureType.Rx) + ","
                       + SOut.Int((int) EhrMeasureType.Demographics) + ","
                       + SOut.Int((int) EhrMeasureType.VitalSigns) + ","
                       + SOut.Int((int) EhrMeasureType.VitalSignsBMIOnly) + ","
                       + SOut.Int((int) EhrMeasureType.VitalSignsBPOnly) + ","
                       + SOut.Int((int) EhrMeasureType.Smoking) + ","
                       + SOut.Int((int) EhrMeasureType.ElectronicCopyAccess) + ","
                       + SOut.Int((int) EhrMeasureType.ElectronicCopy) + ","
                       + SOut.Int((int) EhrMeasureType.ClinicalSummaries) + ","
                       + SOut.Int((int) EhrMeasureType.Lab) + ","
                       + SOut.Int((int) EhrMeasureType.Reminders) + ","
                       + SOut.Int((int) EhrMeasureType.Education) + ","
                       + SOut.Int((int) EhrMeasureType.MedReconcile) + ","
                       + SOut.Int((int) EhrMeasureType.SummaryOfCare) + ","
                       + SOut.Int((int) EhrMeasureType.SummaryOfCareElectronic) + ","
                       + SOut.Int((int) EhrMeasureType.SecureMessaging) + ","
                       + SOut.Int((int) EhrMeasureType.FamilyHistory) + ","
                       + SOut.Int((int) EhrMeasureType.ElectronicNote) + ","
                       + SOut.Int((int) EhrMeasureType.LabImages) + ","
                       + SOut.Int((int) EhrMeasureType.DrugDrugInteractChecking) + ","
                       + SOut.Int((int) EhrMeasureType.DrugFormularyChecking) + ","
                       + SOut.Int((int) EhrMeasureType.ProtectElectHealthInfo) + ","
                       + SOut.Int((int) EhrMeasureType.ImmunizationRegistries) + ","
                       + SOut.Int((int) EhrMeasureType.SyndromicSurveillance) + ","
                       + SOut.Int((int) EhrMeasureType.PatientList) + ","
                       + SOut.Int((int) EhrMeasureType.ClinicalInterventionRules) + ") "; //Is always going to be excluded

        //TODO: Order the measures nicely for Oracle users.
        var retVal = EhrMeasureCrud.SelectMany(command);
        return retVal;
    }

    /// <summary>
    ///     Only called from FormEHR to load the patient specific MU data and tell the user what action to take to get
    ///     closer to meeting MU.
    /// </summary>
    public static List<EhrMu> GetMu2(Patient pat)
    {
        var list = new List<EhrMu>();
        //add one of each type
        EhrMu mu;
        string explanation;
        var retVal = GetMU2List();
        var medList = MedicationPats.Refresh(pat.PatNum, true);
        var ehrLabList = new List<EhrLab>();
        ehrLabList = EhrLabs.GetAllForPat(pat.PatNum);
        var listRadCpoeProcs = Procedures.GetProcsRadiologyCpoeForPat(pat.PatNum);
        var listMeasureEvents = EhrMeasureEvents.Refresh(pat.PatNum);
        var listRefAttach = RefAttaches.Refresh(pat.PatNum);
        for (var i = 0; i < retVal.Count; i++)
        {
            mu = new EhrMu();
            mu.Met = MuMet.False;
            mu.MeasureType = retVal[i].MeasureType;
            switch (mu.MeasureType)
            {
                #region Demographics

                case EhrMeasureType.Demographics:
                    explanation = "";
                    if (pat.Birthdate.Year < 1880) explanation += "birthdate"; //missing
                    if (pat.Language == "")
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "language";
                    }

                    if (pat.Gender == PatientGender.Unknown)
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "gender";
                    }

                    if (PatientRaces.GetForPatient(pat.PatNum).Count == 0)
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "race, ethnicity";
                    }

                    if (explanation == "")
                    {
                        mu.Details = "All demographic elements recorded";
                        mu.Met = MuMet.True;
                    }
                    else
                    {
                        mu.Details = "Missing: " + explanation;
                    }

                    mu.Action = "Enter demographics";
                    break;

                #endregion

                #region Education

                case EhrMeasureType.Education:
                    var listEd = EhrMeasureEvents.RefreshByType(pat.PatNum, EhrMeasureEventType.EducationProvided);
                    if (listEd.Count == 0)
                    {
                        mu.Details = "No education resources provided.";
                    }
                    else
                    {
                        mu.Details = "Education resources provided: " + listEd.Count;
                        mu.Met = MuMet.True;
                    }

                    mu.Action = "Provide education resources";
                    break;

                #endregion

                #region ElectronicCopyAccess

                case EhrMeasureType.ElectronicCopyAccess:
                    var listOnline = EhrMeasureEvents.RefreshByType(pat.PatNum, EhrMeasureEventType.OnlineAccessProvided);
                    if (listOnline.Count == 0)
                    {
                        mu.Details = "No online access provided.";
                    }
                    else
                    {
                        mu.Details = "Online access provided: " + listOnline[listOnline.Count - 1].DateTEvent.ToShortDateString(); //most recent
                        mu.Met = MuMet.True;
                    }

                    mu.Action = "Provide online Access";
                    break;

                #endregion

                #region CPOE_MedOrdersOnly

                case EhrMeasureType.CPOE_MedOrdersOnly:
                    var medOrderCount = 0;
                    var medOrderCpoeCount = 0;
                    for (var m = 0; m < medList.Count; m++)
                        //Using the last year as the reporting period, following pattern in ElectronicCopy, ClinicalSummaries, Reminders...
                        if (medList[m].DateStart < DateTime.Now.AddYears(-1))
                        {
                            //either no start date so not an order, or not within the last year so not during the reporting period
                        }
                        else if (medList[m].PatNote != "" && medList[m].ProvNum == pat.PriProv)
                        {
                            //if there's a note and it was created by the patient's PriProv, then count as order created by this provider and would count toward the denominator for MU
                            medOrderCount++;
                            if (medList[m].IsCpoe)
                                //if also marked as CPOE, then this would count in the numerator of the calculation MU
                                medOrderCpoeCount++;
                        }

                    if (medOrderCount == 0)
                    {
                        mu.Details = "No medication order in CPOE.";
                    }
                    else
                    {
                        mu.Details = "Medications entered in CPOE: " + medOrderCount;
                        mu.Met = MuMet.True;
                    }

                    mu.Action = "(edit Rxs from Chart)";
                    break;

                #endregion

                #region CPOE_LabOrdersOnly

                case EhrMeasureType.CPOE_LabOrdersOnly:
                    var labOrderCount = 0;
                    var labOrderCpoeCount = 0;
                    for (var m = 0; m < ehrLabList.Count; m++)
                    {
                        //Using the last year as the reporting period, following pattern in ElectronicCopy, ClinicalSummaries, Reminders...
                        var loinc = Loincs.GetByCode(ehrLabList[m].UsiID);
                        var dateSt = ehrLabList[m].ObservationDateTimeStart.PadRight(8, '0').Substring(0, 8); //stored in DB as yyyyMMddhhmmss-zzzz
                        var dateT = SIn.Date(dateSt.Substring(4, 2) + "/" + dateSt.Substring(6, 2) + "/" + dateSt.Substring(0, 4));
                        if (dateT < DateTime.Now.AddYears(-1))
                        {
                            //either no start date so not an order, or not within the last year so not during the reporting period
                        }
                        else if ((ehrLabList[m].OrderingProviderID == pat.PriProv.ToString()
                                  || ehrLabList[m].OrderingProviderID == Providers.GetProv(pat.PriProv).NationalProvID)
                                 && (loinc == null || !loinc.ClassType.ToUpper().Contains("RAD")))
                        {
                            labOrderCount++;
                            if (ehrLabList[m].IsCpoe) labOrderCpoeCount++;
                        }
                    }

                    if (labOrderCount == 0)
                    {
                        mu.Details = "No Lab order in CPOE.";
                    }
                    else
                    {
                        mu.Details = "Labs entered in CPOE: " + labOrderCount;
                        mu.Met = MuMet.True;
                    }

                    mu.Action = "Edit labs";
                    break;

                #endregion

                #region CPOE_RadiologyOrdersOnly

                case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                    var radOrderCount = 0;
                    var radOrderCpoeCount = 0;
                    //Loop through all the EHR lab orders and find the ones that are related to RAD LOINC codes.
                    for (var m = 0; m < ehrLabList.Count; m++)
                    {
                        //Using the last year as the reporting period, following pattern in ElectronicCopy, ClinicalSummaries, Reminders...
                        var loinc = Loincs.GetByCode(ehrLabList[m].UsiID);
                        var dateSt = ehrLabList[m].ObservationDateTimeStart.PadRight(8, '0').Substring(0, 8); //stored in DB as yyyyMMddhhmmss-zzzz
                        var dateT = SIn.Date(dateSt.Substring(4, 2) + "/" + dateSt.Substring(6, 2) + "/" + dateSt.Substring(0, 4));
                        if (dateT < DateTime.Now.AddYears(-1))
                        {
                            //either no start date so not an order, or not within the last year so not during the reporting period
                        }
                        else if ((ehrLabList[m].OrderingProviderID == pat.PriProv.ToString()
                                  || ehrLabList[m].OrderingProviderID == Providers.GetProv(pat.PriProv).NationalProvID)
                                 && loinc != null && loinc.ClassType.ToUpper().Contains("RAD"))
                        {
                            radOrderCount++;
                            if (ehrLabList[m].IsCpoe) radOrderCpoeCount++;
                        }
                    }

                    //As of v15.4 we started storing radiology orders at the procedure level.
                    //Go through all completed radiology procedures for the denominator and only count ones that are flagged as IsCpoe in the numerator.
                    for (var m = 0; m < listRadCpoeProcs.Count; m++)
                    {
                        radOrderCount++;
                        if (listRadCpoeProcs[m].IsCpoe) radOrderCpoeCount++;
                    }

                    if (radOrderCount == 0)
                    {
                        mu.Details = "No Rad order in CPOE.";
                    }
                    else
                    {
                        mu.Details = "Rads entered in CPOE: " + radOrderCount;
                        mu.Met = MuMet.True;
                    }

                    mu.Action = "Edit labs";
                    mu.Action2 = "Approve radiology orders";
                    break;

                #endregion

                #region Rx

                case EhrMeasureType.Rx:
                    var listRx = RxPats.GetPermissableForDateRange(pat.PatNum, DateTime.Today.AddYears(-1), DateTime.Today);
                    if (listRx.Count == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No Rxs entered.";
                    }
                    else
                    {
                        explanation = "";
                        for (var rx = 0; rx < listRx.Count; rx++)
                        {
                            if (listRx[rx].SendStatus == RxSendStatus.SentElect) continue;
                            if (explanation != "") explanation += ", ";
                            explanation += listRx[rx].RxDate.ToShortDateString();
                        }

                        if (explanation == "")
                        {
                            mu.Met = MuMet.True;
                            mu.Details = "All Rxs sent electronically.";
                        }
                        else
                        {
                            mu.Met = MuMet.False;
                            mu.Details = "Rxs not sent electronically: " + explanation;
                        }
                    }

                    mu.Action = "(edit Rxs from Chart)"; //no action
                    break;

                #endregion

                #region VitalSigns

                case EhrMeasureType.VitalSigns:
                    var vitalsignList = Vitalsigns.Refresh(pat.PatNum);
                    if (vitalsignList.Count == 0)
                    {
                        mu.Details = "No vital signs entered.";
                    }
                    else
                    {
                        var hFound = false;
                        var wFound = false;
                        var bpFound = false;
                        for (var v = 0; v < vitalsignList.Count; v++)
                        {
                            if (vitalsignList[v].Height > 0) hFound = true;
                            if (vitalsignList[v].Weight > 0) wFound = true;
                            if (pat.Birthdate > DateTime.Today.AddYears(-3) //3 and older for BP
                                || (vitalsignList[v].BpDiastolic > 0 && vitalsignList[v].BpSystolic > 0))
                                bpFound = true;
                        }

                        explanation = "";
                        if (!hFound) explanation += "height"; //missing
                        if (!wFound)
                        {
                            if (explanation != "") explanation += ", ";
                            explanation += "weight";
                        }

                        if (!bpFound)
                        {
                            if (explanation != "") explanation += ", ";
                            explanation += "blood pressure";
                        }

                        if (explanation == "")
                        {
                            mu.Details = "Vital signs entered";
                            mu.Met = MuMet.True;
                        }
                        else
                        {
                            mu.Details = "Missing: " + explanation;
                        }
                    }

                    mu.Action = "Enter vital signs";
                    break;

                #endregion

                #region VitalSignsBMIOnly

                case EhrMeasureType.VitalSignsBMIOnly:
                    vitalsignList = Vitalsigns.Refresh(pat.PatNum);
                    if (vitalsignList.Count == 0)
                    {
                        mu.Details = "No vital signs entered.";
                    }
                    else
                    {
                        var hFound = false;
                        var wFound = false;
                        for (var v = 0; v < vitalsignList.Count; v++)
                        {
                            if (vitalsignList[v].Height > 0) hFound = true;
                            if (vitalsignList[v].Weight > 0) wFound = true;
                        }

                        explanation = "";
                        if (!hFound) explanation += "height"; //missing
                        if (!wFound)
                        {
                            if (explanation != "") explanation += ", ";
                            explanation += "weight";
                        }

                        if (explanation == "")
                        {
                            mu.Details = "Vital signs entered";
                            mu.Met = MuMet.True;
                        }
                        else
                        {
                            mu.Details = "Missing: " + explanation;
                        }
                    }

                    mu.Action = "Enter vital signs";
                    break;

                #endregion

                #region VitalSignsBPOnly

                case EhrMeasureType.VitalSignsBPOnly:
                    vitalsignList = Vitalsigns.Refresh(pat.PatNum);
                    if (pat.Birthdate > DateTime.Today.AddYears(-3))
                    {
                        //3 and older for BP
                        mu.Details = "Age 3 and older for BP.";
                        mu.Met = MuMet.NA;
                    }
                    else if (vitalsignList.Count == 0)
                    {
                        mu.Details = "No vital signs entered.";
                    }
                    else
                    {
                        for (var v = 0; v < vitalsignList.Count; v++)
                            if (vitalsignList[v].BpDiastolic > 0 && vitalsignList[v].BpSystolic > 0)
                            {
                                mu.Details = "Vital signs entered";
                                mu.Met = MuMet.True;
                            }
                            else
                            {
                                mu.Details = "Missing: blood pressure";
                            }
                    }

                    mu.Action = "Enter vital signs";
                    break;

                #endregion

                #region Smoking

                case EhrMeasureType.Smoking:
                    if (pat.SmokingSnoMed == "")
                    {
                        //None
                        mu.Details = "Smoking status not entered";
                    }
                    else
                    {
                        mu.Details = "Smoking status entered";
                        mu.Met = MuMet.True;
                    }

                    if (pat.Age < 13) //The smoking measure does not apply to patients under the age of 13.
                        mu.Met = MuMet.NA;
                    mu.Action = "Edit smoking status";
                    break;

                #endregion

                #region Lab

                case EhrMeasureType.Lab:
                    if (ehrLabList.Count == 0)
                    {
                        mu.Details = "No lab orders";
                        mu.Met = MuMet.NA;
                    }
                    else
                    {
                        var labResultCount = 0;
                        for (var lo = 0; lo < ehrLabList.Count; lo++)
                        {
                            var ehrLabResults = EhrLabResults.GetForLab(ehrLabList[lo].EhrLabNum);
                            if (ehrLabResults.Count > 0) labResultCount++;
                        }

                        if (labResultCount < ehrLabList.Count)
                        {
                            mu.Details = "Lab orders missing results: " + (ehrLabList.Count - labResultCount);
                        }
                        else
                        {
                            mu.Details = "Lab results entered for each lab order.";
                            mu.Met = MuMet.True;
                        }
                    }

                    mu.Action = "Edit labs";
                    break;

                #endregion

                #region ElectronicCopy

                case EhrMeasureType.ElectronicCopy:
                    var listRequests = EhrMeasureEvents.GetByType(listMeasureEvents, EhrMeasureEventType.ElectronicCopyRequested);
                    var listRequestsPeriod = new List<EhrMeasureEvent>();
                    for (var r = 0; r < listRequests.Count; r++)
                    {
                        if (listRequests[r].DateTEvent < DateTime.Now.AddYears(-1) || listRequests[r].PatNum != pat.PatNum) //not within the last year
                            continue;
                        listRequestsPeriod.Add(listRequests[r]);
                    }

                    if (listRequestsPeriod.Count == 0)
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Patient has not viewed/downloaded/transmitted their health info";
                    }
                    else
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Patient has viewed/downloaded/transmitted their health info";
                    }

                    mu.Action = "Patient must use the Patient Portal";
                    break;

                #endregion

                #region ClinicalSummaries

                case EhrMeasureType.ClinicalSummaries:
                    var listVisits = new List<DateTime>(); //for this year
                    var listProcs = Procedures.Refresh(pat.PatNum);
                    for (var p = 0; p < listProcs.Count; p++)
                    {
                        if (listProcs[p].ProcDate < DateTime.Now.AddYears(-1) || listProcs[p].ProcStatus != ProcStat.C) //not within the last year or not a completed procedure
                            continue;
                        if (!listVisits.Contains(listProcs[p].ProcDate)) listVisits.Add(listProcs[p].ProcDate);
                    }

                    if (listVisits.Count == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No visits within the last year.";
                    }
                    else
                    {
                        var countMissing = 0;
                        bool summaryProvidedinTime;
                        var listClinSum = EhrMeasureEvents.GetByType(listMeasureEvents, EhrMeasureEventType.ClinicalSummaryProvidedToPt);
                        for (var p = 0; p < listVisits.Count; p++)
                        {
                            summaryProvidedinTime = false;
                            var deadlineDate = listVisits[p].AddDays(1);
                            if (listVisits[p].DayOfWeek == DayOfWeek.Friday) deadlineDate = deadlineDate.AddDays(2); //add two days for the weekend
                            for (var r = 0; r < listClinSum.Count; r++)
                            {
                                if (listClinSum[r].DateTEvent.Date > deadlineDate) continue;
                                if (listClinSum[r].DateTEvent.Date < listVisits[p]) continue;
                                summaryProvidedinTime = true;
                            }

                            if (!summaryProvidedinTime) countMissing++;
                        }

                        if (countMissing == 0)
                        {
                            mu.Met = MuMet.True;
                            mu.Details = "Clinical summary provided to Pt within 1 business day of each visit.";
                        }
                        else
                        {
                            mu.Met = MuMet.False;
                            mu.Details = "Clinical summaries not provided to Pt within 1 business day of a visit:" + countMissing;
                        }
                    }

                    mu.Action = "Send clinical summary to Pt";
                    break;

                #endregion

                #region Reminders

                case EhrMeasureType.Reminders:
                    var listVisitsRem = new List<DateTime>(); //for this year
                    var listProcsRem = Procedures.Refresh(pat.PatNum);
                    for (var p = 0; p < listProcsRem.Count; p++)
                    {
                        if (listProcsRem[p].ProcDate < DateTime.Now.AddYears(-2) || listProcsRem[p].ProcStatus != ProcStat.C) //not within the last year or not a completed procedure
                            continue;
                        if (!listVisitsRem.Contains(listProcsRem[p].ProcDate)) listVisitsRem.Add(listProcsRem[p].ProcDate);
                    }

                    if (listVisitsRem.Count <= 1)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No two visits within the last two years.";
                    }
                    else if (pat.PatStatus != PatientStatus.Patient)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "Status not patient.";
                    }
                    else
                    {
                        var listReminders = EhrMeasureEvents.GetByType(listMeasureEvents, EhrMeasureEventType.ReminderSent);
                        //during reporting period.
                        var withinLastYear = false;
                        for (var r = 0; r < listReminders.Count; r++)
                            if (listReminders[r].DateTEvent > DateTime.Now.AddYears(-1))
                                withinLastYear = true;

                        if (withinLastYear)
                        {
                            mu.Details = "Reminder sent within the last year.";
                            mu.Met = MuMet.True;
                        }
                        else
                        {
                            mu.Details = "No reminders sent within the last year.";
                        }
                    }

                    mu.Action = "Send reminders";
                    break;

                #endregion

                #region MedReconcile

                case EhrMeasureType.MedReconcile:
                    var countFromRef = 0;
                    var countFromRefPeriod = 0;
                    for (var c = 0; c < listRefAttach.Count; c++)
                        if (listRefAttach[c].RefType == ReferralType.RefFrom && listRefAttach[c].IsTransitionOfCare)
                        {
                            countFromRef++;
                            if (listRefAttach[c].RefDate > DateTime.Now.AddYears(-1))
                                //within the last year
                                countFromRefPeriod++;
                        }

                    if (countFromRef == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "Referral 'from' not entered.";
                    }
                    else if (countFromRefPeriod == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "Referral 'from' not entered within the last year.";
                    }
                    else if (countFromRefPeriod > 0)
                    {
                        var listReconciles = EhrMeasureEvents.GetByType(listMeasureEvents, EhrMeasureEventType.MedicationReconcile);
                        var countReconciles = 0; //during reporting period.
                        for (var r = 0; r < listReconciles.Count; r++)
                            if (listReconciles[r].DateTEvent > DateTime.Now.AddYears(-1))
                                //within the same period as the count for referrals.
                                countReconciles++;

                        mu.Details = "Referrals:" + countFromRefPeriod + ", Reconciles:" + countReconciles;
                        if (countReconciles >= countFromRefPeriod) mu.Met = MuMet.True;
                    }

                    mu.Action = "Electronic Reconciliation";
                    mu.Action2 = "Manual Reconciliation";
                    break;

                #endregion

                #region SummaryOfCare

                case EhrMeasureType.SummaryOfCare:
                    var countToRefPeriod = 0;
                    for (var c = 0; c < listRefAttach.Count; c++)
                        if (listRefAttach[c].RefType == ReferralType.RefTo && listRefAttach[c].IsTransitionOfCare && listRefAttach[c].ProvNum != 0)
                            if (listRefAttach[c].RefDate > DateTime.Now.AddYears(-1))
                                //within the last year
                                countToRefPeriod++;

                    if (countToRefPeriod == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No outgoing transitions of care within the last year.";
                    }
                    else
                    {
                        // > 0
                        var listCcds = EhrMeasureEvents.GetByType(listMeasureEvents, EhrMeasureEventType.SummaryOfCareProvidedToDr);
                        var countCcds = 0; //during reporting period.
                        for (var r = 0; r < listCcds.Count; r++)
                            if (listCcds[r].DateTEvent > DateTime.Now.AddYears(-1) && listCcds[r].FKey != 0)
                                //Quick filter for measure events that do not meet the criterion.
                                //Loop through all ref attaches and only count the ones that meet all measure requirements.
                                for (var c = 0; c < listRefAttach.Count; c++)
                                    if (listRefAttach[c].RefType == ReferralType.RefTo
                                        && listRefAttach[c].RefAttachNum == listCcds[r].FKey
                                        && listRefAttach[c].IsTransitionOfCare
                                        && listRefAttach[c].ProvNum != 0
                                        && listRefAttach[c].RefDate > DateTime.Now.AddYears(-1))
                                    {
                                        countCcds++;
                                        break;
                                    }

                        mu.Details = "Referrals:" + countToRefPeriod + ", Summaries:" + countCcds;
                        if (countCcds >= countToRefPeriod) mu.Met = MuMet.True;
                    }

                    mu.Action = "Send/Receive summary of care";
                    mu.Action2 = "Enter Referrals";
                    break;

                #endregion

                #region SummaryOfCareElectronic

                case EhrMeasureType.SummaryOfCareElectronic:
                    countToRefPeriod = 0;
                    for (var c = 0; c < listRefAttach.Count; c++)
                        if (listRefAttach[c].RefType == ReferralType.RefTo && listRefAttach[c].IsTransitionOfCare && listRefAttach[c].ProvNum != 0)
                            if (listRefAttach[c].RefDate > DateTime.Now.AddYears(-1))
                                //within the last year
                                countToRefPeriod++;

                    if (countToRefPeriod == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No outgoing transitions of care within the last year.";
                    }
                    else
                    {
                        // > 0
                        var listCcds = EhrMeasureEvents.GetByType(listMeasureEvents, EhrMeasureEventType.SummaryOfCareProvidedToDrElectronic);
                        var countCcds = 0; //during reporting period.
                        for (var r = 0; r < listCcds.Count; r++)
                            if (listCcds[r].DateTEvent > DateTime.Now.AddYears(-1) && listCcds[r].FKey != 0)
                                //Quick filter for measure events that do not meet the criterion.
                                //Loop through all ref attaches and only count the ones that meet all measure requirements.
                                for (var c = 0; c < listRefAttach.Count; c++)
                                    if (listRefAttach[c].RefType == ReferralType.RefTo
                                        && listRefAttach[c].RefAttachNum == listCcds[r].FKey
                                        && listRefAttach[c].IsTransitionOfCare
                                        && listRefAttach[c].ProvNum != 0
                                        && listRefAttach[c].RefDate > DateTime.Now.AddYears(-1))
                                    {
                                        countCcds++;
                                        break;
                                    }

                        mu.Details = "Referrals:" + countToRefPeriod + ", Summaries:" + countCcds;
                        if (countCcds >= countToRefPeriod) mu.Met = MuMet.True;
                    }

                    mu.Action = "Send/Receive summary of care via email";
                    mu.Action2 = "Enter Referrals";
                    break;

                #endregion

                #region SecureMessaging

                case EhrMeasureType.SecureMessaging:
                    var listMsg = EhrMeasureEvents.GetByType(listMeasureEvents, EhrMeasureEventType.SecureMessageFromPat);
                    var listVisitsMsg = new List<DateTime>(); //for this year
                    var listProcsMsg = Procedures.Refresh(pat.PatNum);
                    var msgCount = 0;
                    for (var p = 0; p < listProcsMsg.Count; p++)
                    {
                        if (listProcsMsg[p].ProcDate < DateTime.Now.AddYears(-1) || listProcsMsg[p].ProcStatus != ProcStat.C) //not within the last year or not a completed procedure
                            continue;
                        if (!listVisitsMsg.Contains(listProcsMsg[p].ProcDate)) listVisitsMsg.Add(listProcsMsg[p].ProcDate);
                    }

                    for (var p = 0; p < listMsg.Count; p++)
                        if (listMsg[p].PatNum == pat.PatNum)
                            msgCount++;

                    if (listVisitsMsg.Count == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No visits within the last year.";
                    }
                    else if (msgCount == 0)
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "No patient web mail messages.";
                    }
                    else
                    {
                        // > 0
                        mu.Met = MuMet.True;
                        mu.Details = "Web mail message has been received.";
                    }

                    mu.Action = "Patient must send a web mail message via the Patient Portal.";
                    break;

                #endregion

                #region FamilyHistory

                case EhrMeasureType.FamilyHistory:
                    var listFamilyHealth = FamilyHealths.GetFamilyHealthForPat(pat.PatNum);
                    if (listFamilyHealth.Count == 0)
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "No family members entered";
                    }
                    else
                    {
                        // > 0
                        mu.Met = MuMet.True;
                        mu.Details = "Family Members: " + listFamilyHealth.Count;
                    }

                    mu.Action = "Enter family history";
                    break;

                #endregion

                #region ElectronicNote

                case EhrMeasureType.ElectronicNote:
                    var procNote = ProcNotes.GetProcNotesForPat(pat.PatNum, DateTime.Now.AddYears(-1), DateTime.Now.AddDays(1));
                    var notesSigned = 0;
                    if (procNote == null)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No procedure notes";
                    }
                    else if (procNote.Signature != "" && procNote.Note != "")
                    {
                        notesSigned++;
                    }

                    if (notesSigned == 0)
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Unsigned procedure notes";
                    }
                    else
                    {
                        // > 0
                        mu.Met = MuMet.True;
                        mu.Details = "Signed procedure note is present";
                    }

                    mu.Action = "Sign all procedure notes";
                    break;

                #endregion

                #region LabImages

                case EhrMeasureType.LabImages:
                    var labCount = 0;
                    var labCountImages = 0;
                    var listEhrLabs = EhrLabs.GetAllForPatInDateRange(pat.PatNum, DateTime.Now.AddYears(-1), DateTime.Now.AddDays(1));
                    for (var img = 0; img < listEhrLabs.Count; img++)
                        if (EhrLabImages.IsWaitingForImages(listEhrLabs[img].EhrLabNum))
                        {
                            labCount++;
                        }
                        else if (EhrLabImages.Refresh(listEhrLabs[img].EhrLabNum).Count > 0)
                        {
                            labCount++;
                            labCountImages++;
                        }

                    if (labCount == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No labs are waiting for images";
                    }
                    else if (labCount > labCountImages)
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Labs currently waiting on images";
                    }
                    else if (labCount == labCountImages)
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "All labs have images attached";
                    }

                    mu.Action = "Manage Lab Images";
                    break;

                #endregion

                #region DrugDrugInteractChecking

                case EhrMeasureType.DrugDrugInteractChecking:
                    var listDDIC = EhrMeasureEvents.GetAllByTypeFromDB(new DateTime(DateTime.Now.Year, 1, 1), new DateTime(DateTime.Now.Year, 12, 31), EhrMeasureEventType.DrugDrugInteractChecking, false);
                    if (listDDIC.Count != 0 && !string.IsNullOrWhiteSpace(listDDIC[listDDIC.Count - 1].MoreInfo))
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Action taken/Explanation given";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Explanation missing";
                    }

                    mu.Action = "Edit Explanation";
                    break;

                #endregion

                #region DrugFormularyChecking

                case EhrMeasureType.DrugFormularyChecking:
                    var listDFC = EhrMeasureEvents.GetAllByTypeFromDB(new DateTime(DateTime.Now.Year, 1, 1), new DateTime(DateTime.Now.Year, 12, 31), EhrMeasureEventType.DrugFormularyChecking, false);
                    if (listDFC.Count != 0 && !string.IsNullOrWhiteSpace(listDFC[listDFC.Count - 1].MoreInfo))
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Action taken/Explanation given";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Explanation missing";
                    }

                    mu.Action = "Edit Explanation";
                    break;

                #endregion

                #region ProtectElectHealthInfo

                case EhrMeasureType.ProtectElectHealthInfo:
                    var listPEHI = EhrMeasureEvents.GetAllByTypeFromDB(new DateTime(DateTime.Now.Year, 1, 1), new DateTime(DateTime.Now.Year, 12, 31), EhrMeasureEventType.ProtectElectHealthInfo, false);
                    if (listPEHI.Count != 0 && !string.IsNullOrWhiteSpace(listPEHI[listPEHI.Count - 1].MoreInfo))
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Action taken/Explanation given";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Explanation missing";
                    }

                    mu.Action = "Edit Explanation";
                    break;

                #endregion

                #region ImmunizationRegistries

                case EhrMeasureType.ImmunizationRegistries:
                    var listIR = EhrMeasureEvents.GetAllByTypeFromDB(new DateTime(DateTime.Now.Year, 1, 1), new DateTime(DateTime.Now.Year, 12, 31), EhrMeasureEventType.ImmunizationRegistries, false);
                    if (listIR.Count != 0 && !string.IsNullOrWhiteSpace(listIR[listIR.Count - 1].MoreInfo))
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Action taken/Explanation given";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Explanation missing";
                    }

                    mu.Action = "Edit Explanation";
                    break;

                #endregion

                #region SyndromicSurveillance

                case EhrMeasureType.SyndromicSurveillance:
                    var listSS = EhrMeasureEvents.GetAllByTypeFromDB(new DateTime(DateTime.Now.Year, 1, 1), new DateTime(DateTime.Now.Year, 12, 31), EhrMeasureEventType.SyndromicSurveillance, false);
                    if (listSS.Count != 0 && !string.IsNullOrWhiteSpace(listSS[listSS.Count - 1].MoreInfo))
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Action taken/Explanation given";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Explanation missing";
                    }

                    mu.Action = "Edit Explanation";
                    break;

                #endregion

                #region PatientList

                case EhrMeasureType.PatientList:
                    var listPL = EhrMeasureEvents.GetAllByTypeFromDB(new DateTime(DateTime.Now.Year, 1, 1), new DateTime(DateTime.Now.Year, 12, 31), EhrMeasureEventType.PatientList, false);
                    if (listPL.Count != 0)
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "List generated";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "List not generated";
                    }

                    mu.Action = "Generate List";
                    break;

                #endregion

                #region ClinicalInterventionRules

                case EhrMeasureType.ClinicalInterventionRules:
                    var listEhrTriggers = EhrTriggers.GetAll();
                    if (listEhrTriggers.Count > 4)
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Triggers enabled";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Less than 5 triggers enabled";
                    }

                    mu.Action = "Edit triggers";
                    mu.Action2 = "Screenshot CDS Triggers for audit";
                    break;

                #endregion
            }

            list.Add(mu);
        }

        return list;
    }

    #endregion

    #region Meaningful Use 2 Modified

    private static List<EhrMeasure> GetMU2ModList()
    {
        var command = "SELECT * FROM ehrmeasure "
                      + "WHERE MeasureType IN ("
                      + SOut.Int((int) EhrMeasureType.CPOE_MedOrdersOnly) + ","
                      + SOut.Int((int) EhrMeasureType.CPOE_LabOrdersOnly) + ","
                      + SOut.Int((int) EhrMeasureType.CPOE_RadiologyOrdersOnly) + ","
                      + SOut.Int((int) EhrMeasureType.Rx) + ","
                      + SOut.Int((int) EhrMeasureType.Demographics) + ","
                      + SOut.Int((int) EhrMeasureType.VitalSigns) + ","
                      + SOut.Int((int) EhrMeasureType.Smoking) + ","
                      + SOut.Int((int) EhrMeasureType.ElectronicCopyAccess) + ","
                      + SOut.Int((int) EhrMeasureType.ElectronicCopy) + ","
                      + SOut.Int((int) EhrMeasureType.Education) + ","
                      + SOut.Int((int) EhrMeasureType.MedReconcile) + ","
                      + SOut.Int((int) EhrMeasureType.SummaryOfCare) + ","
                      + SOut.Int((int) EhrMeasureType.SummaryOfCareElectronic) + ","
                      + SOut.Int((int) EhrMeasureType.SecureMessaging) + ","
                      + SOut.Int((int) EhrMeasureType.DrugDrugInteractChecking) + ","
                      + SOut.Int((int) EhrMeasureType.DrugFormularyChecking) + ","
                      + SOut.Int((int) EhrMeasureType.ProtectElectHealthInfo) + ","
                      + SOut.Int((int) EhrMeasureType.ImmunizationRegistries) + ","
                      + SOut.Int((int) EhrMeasureType.SyndromicSurveillance) + ","
                      + SOut.Int((int) EhrMeasureType.ClinicalInterventionRules) + ","
                      + SOut.Int((int) EhrMeasureType.ProblemList) + ","
                      + SOut.Int((int) EhrMeasureType.MedicationList) + ","
                      + SOut.Int((int) EhrMeasureType.AllergyList) + ") ";
            command += "ORDER BY FIELD(MeasureType,"
                       + SOut.Int((int) EhrMeasureType.ProtectElectHealthInfo) + ","
                       + SOut.Int((int) EhrMeasureType.ClinicalInterventionRules) + ","
                       + SOut.Int((int) EhrMeasureType.DrugDrugInteractChecking) + ","
                       + SOut.Int((int) EhrMeasureType.CPOE_MedOrdersOnly) + ","
                       + SOut.Int((int) EhrMeasureType.CPOE_LabOrdersOnly) + ","
                       + SOut.Int((int) EhrMeasureType.CPOE_RadiologyOrdersOnly) + ","
                       + SOut.Int((int) EhrMeasureType.Rx) + ","
                       + SOut.Int((int) EhrMeasureType.DrugFormularyChecking) + ","
                       + SOut.Int((int) EhrMeasureType.SummaryOfCare) + ","
                       + SOut.Int((int) EhrMeasureType.SummaryOfCareElectronic) + ","
                       + SOut.Int((int) EhrMeasureType.Education) + ","
                       + SOut.Int((int) EhrMeasureType.MedReconcile) + ","
                       + SOut.Int((int) EhrMeasureType.ElectronicCopyAccess) + ","
                       + SOut.Int((int) EhrMeasureType.ElectronicCopy) + ","
                       + SOut.Int((int) EhrMeasureType.SecureMessaging) + ","
                       + SOut.Int((int) EhrMeasureType.ImmunizationRegistries) + ","
                       + SOut.Int((int) EhrMeasureType.SyndromicSurveillance) + ","
                       + SOut.Int((int) EhrMeasureType.VitalSigns) + ","
                       + SOut.Int((int) EhrMeasureType.Smoking) + ","
                       + SOut.Int((int) EhrMeasureType.Demographics) + ","
                       + SOut.Int((int) EhrMeasureType.ProblemList) + ","
                       + SOut.Int((int) EhrMeasureType.MedicationList) + ","
                       + SOut.Int((int) EhrMeasureType.AllergyList) + ") ";

        //TODO: Order the measures nicely for Oracle users.
        var retVal = EhrMeasureCrud.SelectMany(command);
        return retVal;
    }

    /// <summary>
    ///     Only called from FormEHR to load the patient specific MU data and tell the user what action to take to get
    ///     closer to meeting MU.
    /// </summary>
    public static List<EhrMu> GetMu2Mod(Patient pat)
    {
        var list = new List<EhrMu>();
        //add one of each type
        EhrMu mu;
        string explanation;
        var retVal = GetMU2ModList();
        var medList = MedicationPats.Refresh(pat.PatNum, true);
        var ehrLabList = new List<EhrLab>();
        ehrLabList = EhrLabs.GetAllForPat(pat.PatNum);
        var listRadCpoeProcs = Procedures.GetProcsRadiologyCpoeForPat(pat.PatNum);
        var listMeasureEvents = EhrMeasureEvents.Refresh(pat.PatNum);
        var listRefAttach = RefAttaches.Refresh(pat.PatNum);
        for (var i = 0; i < retVal.Count; i++)
        {
            mu = new EhrMu();
            mu.Met = MuMet.False;
            mu.MeasureType = retVal[i].MeasureType;
            switch (mu.MeasureType)
            {
                #region Demographics

                case EhrMeasureType.Demographics:
                    explanation = "";
                    if (pat.Birthdate.Year < 1880) explanation += "birthdate"; //missing
                    if (pat.Language == "")
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "language";
                    }

                    if (pat.Gender == PatientGender.Unknown)
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "gender";
                    }

                    if (PatientRaces.GetForPatient(pat.PatNum).Count == 0)
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "race, ethnicity";
                    }

                    if (explanation == "")
                    {
                        mu.Details = "All demographic elements recorded";
                        mu.Met = MuMet.True;
                    }
                    else
                    {
                        mu.Details = "Missing: " + explanation;
                    }

                    mu.Action = "Enter demographics";
                    break;

                #endregion

                #region Education

                case EhrMeasureType.Education:
                    var listEd = EhrMeasureEvents.RefreshByType(pat.PatNum, EhrMeasureEventType.EducationProvided);
                    if (listEd.Count == 0)
                    {
                        mu.Details = "No education resources provided.";
                    }
                    else
                    {
                        mu.Details = "Education resources provided: " + listEd.Count;
                        mu.Met = MuMet.True;
                    }

                    mu.Action = "Provide education resources";
                    break;

                #endregion

                #region ElectronicCopyAccess

                case EhrMeasureType.ElectronicCopyAccess:
                    var listOnline = EhrMeasureEvents.RefreshByType(pat.PatNum, EhrMeasureEventType.OnlineAccessProvided);
                    if (listOnline.Count == 0)
                    {
                        mu.Details = "No online access provided.";
                    }
                    else
                    {
                        mu.Details = "Online access provided: " + listOnline[listOnline.Count - 1].DateTEvent.ToShortDateString(); //most recent
                        mu.Met = MuMet.True;
                    }

                    mu.Action = "Provide online Access";
                    break;

                #endregion

                #region CPOE_MedOrdersOnly

                case EhrMeasureType.CPOE_MedOrdersOnly:
                    var medOrderCount = 0;
                    var medOrderCpoeCount = 0;
                    for (var m = 0; m < medList.Count; m++)
                        //Using the last year as the reporting period, following pattern in ElectronicCopy, ClinicalSummaries, Reminders...
                        if (medList[m].DateStart < DateTime.Now.AddYears(-1))
                        {
                            //either no start date so not an order, or not within the last year so not during the reporting period
                        }
                        else if (medList[m].PatNote != "" && medList[m].ProvNum == pat.PriProv)
                        {
                            //if there's a note and it was created by the patient's PriProv, then count as order created by this provider and would count toward the denominator for MU
                            medOrderCount++;
                            if (medList[m].IsCpoe)
                                //if also marked as CPOE, then this would count in the numerator of the calculation MU
                                medOrderCpoeCount++;
                        }

                    if (medOrderCount == 0)
                    {
                        mu.Details = "No medication order in CPOE.";
                    }
                    else
                    {
                        mu.Details = "Medications entered in CPOE: " + medOrderCount;
                        mu.Met = MuMet.True;
                    }

                    mu.Action = "(edit Rxs from Chart)";
                    break;

                #endregion

                #region CPOE_LabOrdersOnly

                case EhrMeasureType.CPOE_LabOrdersOnly:
                    var labOrderCount = 0;
                    var labOrderCpoeCount = 0;
                    for (var m = 0; m < ehrLabList.Count; m++)
                    {
                        //Using the last year as the reporting period, following pattern in ElectronicCopy, ClinicalSummaries, Reminders...
                        var loinc = Loincs.GetByCode(ehrLabList[m].UsiID);
                        var dateSt = ehrLabList[m].ObservationDateTimeStart.PadRight(8, '0').Substring(0, 8); //stored in DB as yyyyMMddhhmmss-zzzz
                        var dateT = SIn.Date(dateSt.Substring(4, 2) + "/" + dateSt.Substring(6, 2) + "/" + dateSt.Substring(0, 4));
                        if (dateT < DateTime.Now.AddYears(-1))
                        {
                            //either no start date so not an order, or not within the last year so not during the reporting period
                        }
                        else if ((ehrLabList[m].OrderingProviderID == pat.PriProv.ToString()
                                  || ehrLabList[m].OrderingProviderID == Providers.GetProv(pat.PriProv).NationalProvID)
                                 && (loinc == null || !loinc.ClassType.ToUpper().Contains("RAD")))
                        {
                            labOrderCount++;
                            if (ehrLabList[m].IsCpoe) labOrderCpoeCount++;
                        }
                    }

                    if (labOrderCount == 0)
                    {
                        mu.Details = "No Lab order in CPOE.";
                    }
                    else
                    {
                        mu.Details = "Labs entered in CPOE: " + labOrderCount;
                        mu.Met = MuMet.True;
                    }

                    mu.Action = "Edit labs";
                    break;

                #endregion

                #region CPOE_RadiologyOrdersOnly

                case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                    var radOrderCount = 0;
                    var radOrderCpoeCount = 0;
                    //Loop through all the EHR lab orders and find the ones that are related to RAD LOINC codes.
                    for (var m = 0; m < ehrLabList.Count; m++)
                    {
                        //Using the last year as the reporting period, following pattern in ElectronicCopy, ClinicalSummaries, Reminders...
                        var loinc = Loincs.GetByCode(ehrLabList[m].UsiID);
                        var dateSt = ehrLabList[m].ObservationDateTimeStart.PadRight(8, '0').Substring(0, 8); //stored in DB as yyyyMMddhhmmss-zzzz
                        var dateT = SIn.Date(dateSt.Substring(4, 2) + "/" + dateSt.Substring(6, 2) + "/" + dateSt.Substring(0, 4));
                        if (dateT < DateTime.Now.AddYears(-1))
                        {
                            //either no start date so not an order, or not within the last year so not during the reporting period
                        }
                        else if ((ehrLabList[m].OrderingProviderID == pat.PriProv.ToString()
                                  || ehrLabList[m].OrderingProviderID == Providers.GetProv(pat.PriProv).NationalProvID)
                                 && loinc != null && loinc.ClassType.ToUpper().Contains("RAD"))
                        {
                            radOrderCount++;
                            if (ehrLabList[m].IsCpoe) radOrderCpoeCount++;
                        }
                    }

                    //As of v15.4 we started storing radiology orders at the procedure level.
                    //Go through all completed radiology procedures for the denominator and only count ones that are flagged as IsCpoe in the numerator.
                    for (var m = 0; m < listRadCpoeProcs.Count; m++)
                    {
                        radOrderCount++;
                        if (listRadCpoeProcs[m].IsCpoe) radOrderCpoeCount++;
                    }

                    if (radOrderCount == 0)
                    {
                        mu.Details = "No Rad order in CPOE.";
                    }
                    else
                    {
                        mu.Details = "Rads entered in CPOE: " + radOrderCount;
                        mu.Met = MuMet.True;
                    }

                    mu.Action = "Edit labs";
                    mu.Action2 = "Approve radiology orders";
                    break;

                #endregion

                #region Rx

                case EhrMeasureType.Rx:
                    var listRx = RxPats.GetPermissableForDateRange(pat.PatNum, DateTime.Today.AddYears(-1), DateTime.Today);
                    if (listRx.Count == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No Rxs entered.";
                    }
                    else
                    {
                        explanation = "";
                        for (var rx = 0; rx < listRx.Count; rx++)
                        {
                            if (listRx[rx].SendStatus == RxSendStatus.SentElect) continue;
                            if (explanation != "") explanation += ", ";
                            explanation += listRx[rx].RxDate.ToShortDateString();
                        }

                        if (explanation == "")
                        {
                            mu.Met = MuMet.True;
                            mu.Details = "All Rxs sent electronically.";
                        }
                        else
                        {
                            mu.Met = MuMet.False;
                            mu.Details = "Rxs not sent electronically: " + explanation;
                        }
                    }

                    mu.Action = "(edit Rxs from Chart)"; //no action
                    break;

                #endregion

                #region VitalSigns

                case EhrMeasureType.VitalSigns:
                    var vitalsignList = Vitalsigns.Refresh(pat.PatNum);
                    if (vitalsignList.Count == 0)
                    {
                        mu.Details = "No vital signs entered.";
                    }
                    else
                    {
                        var hFound = false;
                        var wFound = false;
                        var bpFound = false;
                        for (var v = 0; v < vitalsignList.Count; v++)
                        {
                            if (vitalsignList[v].Height > 0) hFound = true;
                            if (vitalsignList[v].Weight > 0) wFound = true;
                            if (pat.Birthdate > DateTime.Today.AddYears(-3) //3 and older for BP
                                || (vitalsignList[v].BpDiastolic > 0 && vitalsignList[v].BpSystolic > 0))
                                bpFound = true;
                        }

                        explanation = "";
                        if (!hFound) explanation += "height"; //missing
                        if (!wFound)
                        {
                            if (explanation != "") explanation += ", ";
                            explanation += "weight";
                        }

                        if (!bpFound)
                        {
                            if (explanation != "") explanation += ", ";
                            explanation += "blood pressure";
                        }

                        if (explanation == "")
                        {
                            mu.Details = "Vital signs entered";
                            mu.Met = MuMet.True;
                        }
                        else
                        {
                            mu.Details = "Missing: " + explanation;
                        }
                    }

                    mu.Action = "Enter vital signs";
                    break;

                #endregion

                #region Smoking

                case EhrMeasureType.Smoking:
                    if (pat.SmokingSnoMed == "")
                    {
                        //None
                        mu.Details = "Smoking status not entered";
                    }
                    else
                    {
                        mu.Details = "Smoking status entered";
                        mu.Met = MuMet.True;
                    }

                    if (pat.Age < 13) //The smoking measure does not apply to patients under the age of 13.
                        mu.Met = MuMet.NA;
                    mu.Action = "Edit smoking status";
                    break;

                #endregion

                #region ElectronicCopy

                case EhrMeasureType.ElectronicCopy:
                    var listRequests = EhrMeasureEvents.GetByType(listMeasureEvents, EhrMeasureEventType.ElectronicCopyRequested);
                    var listRequestsPeriod = new List<EhrMeasureEvent>();
                    for (var r = 0; r < listRequests.Count; r++)
                    {
                        if (listRequests[r].DateTEvent < DateTime.Now.AddYears(-1) || listRequests[r].PatNum != pat.PatNum) //not within the last year
                            continue;
                        listRequestsPeriod.Add(listRequests[r]);
                    }

                    if (listRequestsPeriod.Count == 0)
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Patient has not viewed/downloaded/transmitted their health info";
                    }
                    else
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Patient has viewed/downloaded/transmitted their health info";
                    }

                    mu.Action = "Patient must use the Patient Portal";
                    break;

                #endregion

                #region MedReconcile

                case EhrMeasureType.MedReconcile:
                    var countFromRef = 0;
                    var countFromRefPeriod = 0;
                    for (var c = 0; c < listRefAttach.Count; c++)
                        if (listRefAttach[c].RefType == ReferralType.RefFrom && listRefAttach[c].IsTransitionOfCare)
                        {
                            countFromRef++;
                            if (listRefAttach[c].RefDate > DateTime.Now.AddYears(-1))
                                //within the last year
                                countFromRefPeriod++;
                        }

                    if (countFromRef == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "Referral 'from' not entered.";
                    }
                    else if (countFromRefPeriod == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "Referral 'from' not entered within the last year.";
                    }
                    else if (countFromRefPeriod > 0)
                    {
                        var listReconciles = EhrMeasureEvents.GetByType(listMeasureEvents, EhrMeasureEventType.MedicationReconcile);
                        var countReconciles = 0; //during reporting period.
                        for (var r = 0; r < listReconciles.Count; r++)
                            if (listReconciles[r].DateTEvent > DateTime.Now.AddYears(-1))
                                //within the same period as the count for referrals.
                                countReconciles++;

                        mu.Details = "Referrals:" + countFromRefPeriod + ", Reconciles:" + countReconciles;
                        if (countReconciles >= countFromRefPeriod) mu.Met = MuMet.True;
                    }

                    mu.Action = "Electronic Reconciliation";
                    mu.Action2 = "Manual Reconciliation";
                    break;

                #endregion

                #region SummaryOfCare

                case EhrMeasureType.SummaryOfCare:
                    var countToRefPeriod = 0;
                    for (var c = 0; c < listRefAttach.Count; c++)
                        if (listRefAttach[c].RefType == ReferralType.RefTo && listRefAttach[c].IsTransitionOfCare && listRefAttach[c].ProvNum != 0)
                            if (listRefAttach[c].RefDate > DateTime.Now.AddYears(-1))
                                //within the last year
                                countToRefPeriod++;

                    if (countToRefPeriod == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No outgoing transitions of care within the last year.";
                    }
                    else
                    {
                        // > 0
                        var listCcds = EhrMeasureEvents.GetByType(listMeasureEvents, EhrMeasureEventType.SummaryOfCareProvidedToDr);
                        var countCcds = 0; //during reporting period.
                        for (var r = 0; r < listCcds.Count; r++)
                            if (listCcds[r].DateTEvent > DateTime.Now.AddYears(-1) && listCcds[r].FKey != 0)
                                //Quick filter for measure events that do not meet the criterion.
                                //Loop through all ref attaches and only count the ones that meet all measure requirements.
                                for (var c = 0; c < listRefAttach.Count; c++)
                                    if (listRefAttach[c].RefType == ReferralType.RefTo
                                        && listRefAttach[c].RefAttachNum == listCcds[r].FKey
                                        && listRefAttach[c].IsTransitionOfCare
                                        && listRefAttach[c].ProvNum != 0
                                        && listRefAttach[c].RefDate > DateTime.Now.AddYears(-1))
                                    {
                                        countCcds++;
                                        break;
                                    }

                        mu.Details = "Referrals:" + countToRefPeriod + ", Summaries:" + countCcds;
                        if (countCcds >= countToRefPeriod) mu.Met = MuMet.True;
                    }

                    mu.Action = "Send/Receive summary of care";
                    mu.Action2 = "Enter Referrals";
                    break;

                #endregion

                #region SummaryOfCareElectronic

                case EhrMeasureType.SummaryOfCareElectronic:
                    countToRefPeriod = 0;
                    for (var c = 0; c < listRefAttach.Count; c++)
                        if (listRefAttach[c].RefType == ReferralType.RefTo && listRefAttach[c].IsTransitionOfCare && listRefAttach[c].ProvNum != 0)
                            if (listRefAttach[c].RefDate > DateTime.Now.AddYears(-1))
                                //within the last year
                                countToRefPeriod++;

                    if (countToRefPeriod == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No outgoing transitions of care within the last year.";
                    }
                    else
                    {
                        // > 0
                        var listCcds = EhrMeasureEvents.GetByType(listMeasureEvents, EhrMeasureEventType.SummaryOfCareProvidedToDrElectronic);
                        var countCcds = 0; //during reporting period.
                        for (var r = 0; r < listCcds.Count; r++)
                            if (listCcds[r].DateTEvent > DateTime.Now.AddYears(-1) && listCcds[r].FKey != 0)
                                //Quick filter for measure events that do not meet the criterion.
                                //Loop through all ref attaches and only count the ones that meet all measure requirements.
                                for (var c = 0; c < listRefAttach.Count; c++)
                                    if (listRefAttach[c].RefType == ReferralType.RefTo
                                        && listRefAttach[c].RefAttachNum == listCcds[r].FKey
                                        && listRefAttach[c].IsTransitionOfCare
                                        && listRefAttach[c].ProvNum != 0
                                        && listRefAttach[c].RefDate > DateTime.Now.AddYears(-1))
                                    {
                                        countCcds++;
                                        break;
                                    }

                        mu.Details = "Referrals:" + countToRefPeriod + ", Summaries:" + countCcds;
                        if (countCcds >= countToRefPeriod) mu.Met = MuMet.True;
                    }

                    mu.Action = "Send/Receive summary of care via email";
                    mu.Action2 = "Enter Referrals";
                    break;

                #endregion

                #region SecureMessaging

                case EhrMeasureType.SecureMessaging:
                    var listProcsMsg = Procedures.GetCompletedForDateRange(DateTime.Now.AddYears(-1), DateTime.Now, listPatNums: new List<long> {pat.PatNum});
                    if (listProcsMsg.Count == 0)
                    {
                        mu.Met = MuMet.NA;
                        mu.Details = "No visits within the last year.";
                    }
                    else if (EmailMessages.GetWebMailForPat(pat.PatNum)
                             .Any(x => EmailMessages.IsSecureWebMail(x.SentOrReceived) && EmailMessages.IsSent(x.SentOrReceived)))
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Web mail message has been sent.";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "No web mail messages sent to this patient.";
                    }

                    mu.Action = "Send a web mail message to this patient.";
                    break;

                #endregion

                #region DrugDrugInteractChecking

                case EhrMeasureType.DrugDrugInteractChecking:
                    var listDDIC = EhrMeasureEvents.GetAllByTypeFromDB(new DateTime(DateTime.Now.Year, 1, 1), new DateTime(DateTime.Now.Year, 12, 31), EhrMeasureEventType.DrugDrugInteractChecking, false);
                    if (listDDIC.Count != 0 && !string.IsNullOrWhiteSpace(listDDIC[listDDIC.Count - 1].MoreInfo))
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Action taken/Explanation given";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Explanation missing";
                    }

                    mu.Action = "Edit Explanation";
                    break;

                #endregion

                #region DrugFormularyChecking

                case EhrMeasureType.DrugFormularyChecking:
                    var listDFC = EhrMeasureEvents.GetAllByTypeFromDB(new DateTime(DateTime.Now.Year, 1, 1), new DateTime(DateTime.Now.Year, 12, 31), EhrMeasureEventType.DrugFormularyChecking, false);
                    if (listDFC.Count != 0 && !string.IsNullOrWhiteSpace(listDFC[listDFC.Count - 1].MoreInfo))
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Action taken/Explanation given";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Explanation missing";
                    }

                    mu.Action = "Edit Explanation";
                    break;

                #endregion

                #region ProtectElectHealthInfo

                case EhrMeasureType.ProtectElectHealthInfo:
                    var listPEHI = EhrMeasureEvents.GetAllByTypeFromDB(new DateTime(DateTime.Now.Year, 1, 1), new DateTime(DateTime.Now.Year, 12, 31), EhrMeasureEventType.ProtectElectHealthInfo, false);
                    if (listPEHI.Count != 0 && !string.IsNullOrWhiteSpace(listPEHI[listPEHI.Count - 1].MoreInfo))
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Action taken/Explanation given";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Explanation missing";
                    }

                    mu.Action = "Edit Explanation";
                    break;

                #endregion

                #region ImmunizationRegistries

                case EhrMeasureType.ImmunizationRegistries:
                    var listIR = EhrMeasureEvents.GetAllByTypeFromDB(new DateTime(DateTime.Now.Year, 1, 1), new DateTime(DateTime.Now.Year, 12, 31), EhrMeasureEventType.ImmunizationRegistries, false);
                    if (listIR.Count != 0 && !string.IsNullOrWhiteSpace(listIR[listIR.Count - 1].MoreInfo))
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Action taken/Explanation given";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Explanation missing";
                    }

                    mu.Action = "Edit Explanation";
                    break;

                #endregion

                #region SyndromicSurveillance

                case EhrMeasureType.SyndromicSurveillance:
                    var listSS = EhrMeasureEvents.GetAllByTypeFromDB(new DateTime(DateTime.Now.Year, 1, 1), new DateTime(DateTime.Now.Year, 12, 31), EhrMeasureEventType.SyndromicSurveillance, false);
                    if (listSS.Count != 0 && !string.IsNullOrWhiteSpace(listSS[listSS.Count - 1].MoreInfo))
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Action taken/Explanation given";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Explanation missing";
                    }

                    mu.Action = "Edit Explanation";
                    break;

                #endregion

                #region ClinicalInterventionRules

                case EhrMeasureType.ClinicalInterventionRules:
                    var listEhrTriggers = EhrTriggers.GetAll();
                    if (listEhrTriggers.Count > 4)
                    {
                        mu.Met = MuMet.True;
                        mu.Details = "Triggers enabled";
                    }
                    else
                    {
                        mu.Met = MuMet.False;
                        mu.Details = "Less than 5 triggers enabled";
                    }

                    mu.Action = "Edit triggers";
                    mu.Action2 = "Screenshot CDS Triggers for audit";
                    break;

                #endregion

                #region ProblemList

                case EhrMeasureType.ProblemList:
                    var listDisease = Diseases.Refresh(pat.PatNum);
                    var validDiseaseCount = 0;
                    if (listDisease.Count == 0)
                    {
                        mu.Details = "No problems entered.";
                    }
                    else
                    {
                        var diseasesNone = false;
                        if (listDisease.Count == 1 && listDisease[0].DiseaseDefNum == PrefC.GetLong(PrefName.ProblemsIndicateNone)) diseasesNone = true;
                        if (diseasesNone)
                        {
                            mu.Met = MuMet.True;
                            mu.Details = "Problems marked 'none'.";
                        }
                        else
                        {
                            for (var m = 0; m < listDisease.Count; m++)
                            {
                                var diseaseCur = DiseaseDefs.GetItem(listDisease[m].DiseaseDefNum);
                                if (diseaseCur.ICD9Code == "" && diseaseCur.SnomedCode == "") continue;
                                validDiseaseCount++;
                            }

                            if (validDiseaseCount == 0)
                            {
                                mu.Details = "No problems with ICD-9 or Snomed code entered.";
                            }
                            else
                            {
                                mu.Met = MuMet.True;
                                mu.Details = "Problems with ICD-9 or Snomed code entered: " + validDiseaseCount;
                            }
                        }
                    }

                    mu.Action = "Enter problems";
                    break;

                #endregion

                #region MedicationList

                case EhrMeasureType.MedicationList:
                    if (medList.Count == 0)
                    {
                        mu.Details = "No medications entered.";
                    }
                    else
                    {
                        mu.Met = MuMet.True;
                        var medsNone = false;
                        if (medList.Count == 1 && medList[0].MedicationNum == PrefC.GetLong(PrefName.MedicationsIndicateNone)) medsNone = true;
                        if (medsNone)
                            mu.Details = "Medications marked 'none'.";
                        else
                            mu.Details = "Medications entered: " + medList.Count;
                    }

                    mu.Action = "Enter medications";
                    break;

                #endregion

                #region AllergyList

                case EhrMeasureType.AllergyList:
                    var listAllergies = Allergies.Refresh(pat.PatNum);
                    if (listAllergies.Count == 0)
                    {
                        mu.Details = "No allergies entered.";
                    }
                    else
                    {
                        mu.Met = MuMet.True;
                        var allergiesNone = false;
                        if (listAllergies.Count == 1 && listAllergies[0].AllergyDefNum == PrefC.GetLong(PrefName.AllergiesIndicateNone)) allergiesNone = true;
                        if (allergiesNone)
                            mu.Details = "Allergies marked 'none'.";
                        else
                            mu.Details = "Allergies entered: " + listAllergies.Count;
                    }

                    mu.Action = "Enter allergies";
                    break;

                #endregion
            }

            list.Add(mu);
        }

        return list;
    }

    ///<summary>Returns the Measures text based on the EHR certification documents.</summary>
    private static string GetMeasureMu2Mod(EhrMeasureType mtype)
    {
        var thresh = GetThresholdMu2(mtype);
        switch (mtype)
        {
            case EhrMeasureType.CPOE_MedOrdersOnly:
                return "More than " + thresh + "% of medication orders created by the EP during the EHR reporting period are recorded using CPOE.";
            case EhrMeasureType.CPOE_LabOrdersOnly:
                return "More than " + thresh + "% of lab orders created by the EP during the EHR reporting period are recorded using CPOE.";
            case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                return "More than " + thresh + "% of radiology orders created by the EP during the EHR reporting period are recorded using CPOE.";
            case EhrMeasureType.Rx:
                return "More than " + thresh + "% of all permissible prescriptions, or all prescriptions, written by the EP are queried for a drug formulary and transmitted electronically using CEHRT.";
            case EhrMeasureType.Demographics:
                return "More than " + thresh + "% of all unique patients seen by the EP have demographics recorded as structured data.";
            case EhrMeasureType.VitalSigns:
                return "More than " + thresh + "% of all unique patients seen by the EP have blood pressure (for patients age 3 and over only) and/or height and weight (for all ages) recorded as structured data.";
            case EhrMeasureType.VitalSignsBMIOnly:
                return "More than " + thresh + "% of all unique patients seen by the EP have blood pressure (for patients age 3 and over only) and/or height and weight (for all ages) recorded as structured data.";
            case EhrMeasureType.VitalSignsBPOnly:
                return "More than " + thresh + "% of all unique patients seen by the EP have blood pressure (for patients age 3 and over only) and/or height and weight (for all ages) recorded as structured data.";
            case EhrMeasureType.Smoking:
                return "More than " + thresh + "% of all unique patients 13 years old or older seen by the EP have smoking status recorded as structured data.";
            case EhrMeasureType.ElectronicCopyAccess:
                return "More than " + thresh + "% of all unique patients seen by the EP during the EHR reporting period are provided timely (available to the patient within 4 business days after the information is available to the EP) online access to their health information.";
            case EhrMeasureType.ElectronicCopy:
                return "More than " + thresh + "% of all unique patients seen by the EP during the EHR reporting period (or their authorized representatives) view, download, or transmit to a third party their health information.";
            case EhrMeasureType.ClinicalSummaries:
                return "Clinical summaries provided to patients or patient-authorized representatives within one business day for more than " + thresh + "% of office visits.";
            case EhrMeasureType.Lab:
                return "More than " + thresh + "% of all clinical lab tests results ordered by the EP during the EHR reporting period whose results are either in a positive/negative or numerical format are incorporated in Certified EHR Technology as structured data.";
            case EhrMeasureType.Reminders:
                return "More than " + thresh + "% of all unique patients who have had 2 or more office visits with the EP within the 24 months before the beginning of the EHR reporting period were sent a reminder, per patient preference when available.";
            case EhrMeasureType.Education:
                return "Patient-specific education resources identified by Certified EHR Technology are provided to patients for more than " + thresh + "% of all unique patients with office visits seen by the EP during the EHR reporting period.";
            case EhrMeasureType.MedReconcile:
                return "The EP who performs medication reconciliation for more than " + thresh + "% of transitions of care in which the patient is transitioned into the care of the EP.";
            case EhrMeasureType.SummaryOfCare:
                return "The EP who transitions or refers their patient to another setting of care or provider of care provides a summary of care record for more than " + thresh + "% of transitions of care and referrals.";
            case EhrMeasureType.SummaryOfCareElectronic:
                return "The EP who transitions or refers their patient to another setting of care or provider of care provides a summary of care record for more than " + thresh + "% of such transitions, and referrals, electronically transmitted using CEHRT to a recipient";
            case EhrMeasureType.SecureMessaging:
                return "For an EHR reporting period in 2017, for more than " + thresh + "% of unique patients seen by the EP during the EHR reporting period, a secure message was sent using the electronic messaging function of CEHRT to the patient(or the patient-authorized representative), or in response to a secure message sent by the patient(or the patient-authorized representative) during the EHR reporting period.";
            case EhrMeasureType.FamilyHistory:
                return "More than " + thresh + "% of all unique patients seen by the EP during the EHR reporting period have a structured data entry for one or more first-degree relatives.";
            case EhrMeasureType.ElectronicNote:
                return "Enter at least one electronic progress note created, edited and signed by an EP for more than " + thresh + "% of unique patients with at least one office visit during the EHR Measure reporting period. The text of the electronic note must be text searchable and may contain drawings and other content";
            case EhrMeasureType.LabImages:
                return "More than " + thresh + "% of all tests whose result is one or more images ordered by the EP during the EHR reporting period are accessible through CEHRT.";
        }

        return "";
        //throw new ApplicationException("Type not in use for MU2: "+mtype.ToString());
    }

    public static DataTable GetTableMu2Mod(EhrMeasureType mtype, DateTime dateStart, DateTime dateEnd, long provNum)
    {
        var command = "";
        var tableRaw = new DataTable();
        var provCur = Providers.GetProv(provNum);
        command = "SELECT GROUP_CONCAT(provider.ProvNum) FROM provider WHERE provider.ProvNum IN "
                  + "(SELECT ProvNum FROM provider pv, ehrprovkey epk WHERE pv.LName=epk.LName AND pv.FName=epk.FName "
                  + "AND epk.LName='" + SOut.String(provCur.LName) + "' AND epk.FName='" + SOut.String(provCur.FName) + "')";
        var provs = DataCore.GetScalar(command);
        var tempProv = provs.Split(',');
        var provOID = "";
        for (var oi = 0; oi < tempProv.Length; oi++)
        {
            provOID = provOID + tempProv[oi];
            if (oi < tempProv.Length - 1) provOID += ",";
        }

        command = "SELECT GROUP_CONCAT(provider.NationalProvID) FROM provider WHERE provider.ProvNum IN "
                  + "(SELECT ProvNum FROM provider pv, ehrprovkey epk WHERE pv.LName=epk.LName AND pv.FName=epk.FName "
                  + "AND epk.LName='" + SOut.String(provCur.LName) + "' AND epk.FName='" + SOut.String(provCur.FName) + "') "
                  + "AND provider.NationalProvID !='' AND provider.NationalProvID !=','";
        var provNPIs = DataCore.GetScalar(command);
        provNPIs.Trim(',');
        if (provNPIs == "") provNPIs = "NULL"; //No NPI entered, queries below use IN statements and require at least one value.
        //Some measures use a temp table.  Create a random number to tack onto the end of the temp table name to avoid possible table collisions.
        var rnd = new Random();
        var rndStr = rnd.Next(1000000).ToString();
        switch (mtype)
        {
            #region CPOE_MedOrdersOnly

            case EhrMeasureType.CPOE_MedOrdersOnly:
                command = "SELECT patient.LName, patient.FName, medPat.* "
                          + "FROM medicationpat as medPat "
                          + "INNER JOIN patient ON patient.PatNum=medPat.PatNum "
                          + "LEFT JOIN ehrmeasureevent as eme ON medPat.MedicationPatNum=eme.FKey "
                          + "AND eme.EventType=" + SOut.Int((int) EhrMeasureEventType.CPOE_MedOrdered) + " "
                          + "WHERE medPat.ProvNum IN(" + SOut.String(provs) + ") "
                          + "AND medPat.MedicationNum=0 " //MedicationNum is 0 when creating an Rx either manually or through NewCrop.  MedicationNum is not 0 when manually added through the medical window.
                          + "AND medPat.DateStart BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd);
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region CPOE_LabOrdersOnly

            case EhrMeasureType.CPOE_LabOrdersOnly:
                command = "SELECT patient.PatNum,patient.LName,patient.FName,ehrlab.IsCpoe,STR_TO_DATE(ehrlab.ObservationDateTimeStart,'%Y%m%d') AS ObservationDateTimeStart "
                          + "FROM ehrlab "
                          + "INNER JOIN patient ON ehrlab.PatNum=patient.PatNum "
                          + "WHERE (CASE WHEN ehrlab.OrderingProviderIdentifierTypeCode='NPI' THEN ehrlab.OrderingProviderID IN(" + SOut.String(provNPIs) + ") " //When the lab is using a NPI number to determine provider.
                          + "WHEN ehrlab.OrderingProviderIdentifierTypeCode='PRN' THEN ( " //When the lab is using provider number to determine provider.
                          + "CASE WHEN ehrlab.OrderingProviderAssigningAuthorityUniversalID=( " //If the AssigningAuthority is OpenDental.
                          + "SELECT IDRoot FROM oidinternal WHERE IDType='Provider' GROUP BY IDType "
                          + ") THEN ehrlab.OrderingProviderID IN(" + SOut.String(provs) + ") END) " //Use the ProvNum to determine provider.
                          + "ELSE FALSE END) " //If the AssigningAuthority is not OpenDental, we have no way to tell who the provider is.
                          + "AND ehrlab.ObservationDateTimeStart BETWEEN DATE_FORMAT(" + SOut.Date(dateStart) + ",'%Y%m%d') AND DATE_FORMAT(" + SOut.Date(dateEnd) + ",'%Y%m%d') "
                          + "AND (CASE WHEN ehrlab.UsiCodeSystemName='LN' THEN ehrlab.UsiID WHEN ehrlab.UsiCodeSystemNameAlt='LN' THEN ehrlab.UsiIDAlt ELSE '' END) "
                          + "NOT IN (SELECT LoincCode FROM loinc WHERE loinc.ClassType LIKE '%RAD%')";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region CPOE_RadiologyOrdersOnly

            case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                command = "SELECT patient.PatNum,patient.LName,patient.FName,ehrlab.IsCpoe,STR_TO_DATE(ehrlab.ObservationDateTimeStart,'%Y%m%d') AS ObservationDateTimeStart, "
                          + "(CASE WHEN ehrlab.UsiCodeSystemName='LN' THEN ehrlab.UsiID WHEN ehrlab.UsiCodeSystemNameAlt='LN' THEN ehrlab.UsiIDAlt ELSE '' END) AS LoincCode "
                          + "FROM ehrlab "
                          + "INNER JOIN patient ON ehrlab.PatNum=patient.PatNum "
                          + "WHERE (CASE WHEN ehrlab.OrderingProviderIdentifierTypeCode='NPI' THEN ehrlab.OrderingProviderID IN(" + SOut.String(provNPIs) + ") " //When the lab is using a NPI number to determine provider.
                          + "WHEN ehrlab.OrderingProviderIdentifierTypeCode='PRN' THEN ( " //When the lab is using provider number to determine provider.
                          + "CASE WHEN ehrlab.OrderingProviderAssigningAuthorityUniversalID=( " //If the AssigningAuthority is OpenDental.
                          + "SELECT IDRoot FROM oidinternal WHERE IDType='Provider' GROUP BY IDType "
                          + ") THEN ehrlab.OrderingProviderID IN(" + SOut.String(provs) + ") END) " //Use the ProvNum to determine provider.
                          + "ELSE FALSE END) " //If the AssigningAuthority is not OpenDental, we have no way to tell who the provider is.
                          + "AND ehrlab.ObservationDateTimeStart BETWEEN DATE_FORMAT(" + SOut.Date(dateStart) + ",'%Y%m%d') AND DATE_FORMAT(" + SOut.Date(dateEnd) + ",'%Y%m%d') "
                          + "AND (CASE WHEN ehrlab.UsiCodeSystemName='LN' THEN ehrlab.UsiID WHEN ehrlab.UsiCodeSystemNameAlt='LN' THEN ehrlab.UsiIDAlt ELSE '' END) "
                          + "IN (SELECT LoincCode FROM loinc WHERE loinc.ClassType LIKE '%RAD%')";
                var dateStartRad154 = PrefC.GetDate(PrefName.RadiologyDateStartedUsing154);
                //Only count radiology orders via the procedurelog table if the date that the office updated to v15.4.1 is less than the end date.
                if (dateStartRad154 < dateEnd.Date)
                {
                    var dateStartStr = SOut.Date(dateStart);
                    //We need to use a start date of whichever date is greater, dateStart or the date the the office updated to v15.4.1.
                    if (dateStartRad154 > dateStart)
                        //The date that the office updated past 15.4.1 is greater than the reporting period's start date.
                        //Use the preference date instead so that the office isn't penalized for radiology orders prior to updating past v15.4.1.
                        dateStartStr = SOut.Date(dateStartRad154);
                    //As of v15.4 we started storing radiology orders at the procedure level by flagging the procedure itself as IsCpoe.
                    //We are going to keep on running both of these queries at the same time.  
                    //Users can simply unmark procedure codes as IsRadiology if they want to only count radiology orders the via the old way (lab order window).
                    command += " UNION ALL "; //Use union all so that duplicate columns do NOT get grouped together.
                    //Get all completed procedures within the date range that are associated to procedure codes that are flagged as IsRadiology.
                    command += "SELECT patient.PatNum,patient.LName,patient.FName,procedurelog.IsCpoe,procedurelog.DateEntryC,'' as LoincCode "
                               + "FROM procedurelog "
                               + "INNER JOIN procedurecode ON procedurelog.CodeNum=procedurecode.CodeNum AND procedurecode.IsRadiology=1 "
                               + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                               + "LEFT JOIN patient ON procedurelog.PatNum=patient.PatNum "
                               + "WHERE procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.C) + " "
                               + "AND procedurelog.ProvNum IN (" + SOut.String(provs) + ") "
                               + "AND procedurelog.DateEntryC BETWEEN " + dateStartStr + " AND " + SOut.Date(dateEnd);
                }

                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region Rx

            case EhrMeasureType.Rx:
                command = "SELECT patient.PatNum,LName,FName,SendStatus,RxDate "
                          + "FROM rxpat,patient "
                          + "WHERE rxpat.PatNum=patient.PatNum "
                          + "AND IsControlled = 0 "
                          //+"AND rxpat.ProvNum="+POut.Long(provNum)+" "
                          + "AND rxpat.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND RxDate >= " + SOut.Date(dateStart) + " "
                          + "AND RxDate <= " + SOut.Date(dateEnd);
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region Demographics

            case EhrMeasureType.Demographics:
                command = "SELECT patient.PatNum,LName,FName,Birthdate,Gender,Language,COALESCE(race.HasRace,0) AS HasRace,COALESCE(race.HasEthnicity,0) AS HasEthnicity "
                          + "FROM patient "
                          + "INNER JOIN ( "
                          + "SELECT DISTINCT procedurelog.PatNum "
                          + "FROM procedurelog "
                          + "WHERE procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.C) + " ";
                var brokenApptCodes = string.Join(",", ProcedureCodes.GetWhereFromList(x => x.ProcCode == "D9986" || x.ProcCode == "D9987")
                    .Select(x => x.CodeNum));
                if (!string.IsNullOrEmpty(brokenApptCodes)) command += "AND CodeNum NOT IN (" + brokenApptCodes + ") ";
                command += "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                           + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                           + ") ProcCheck ON ProcCheck.PatNum = patient.PatNum "
                           + "LEFT JOIN ( "
                           + "SELECT PatNum, "
                           + "MAX(CASE WHEN cdcrec.HeirarchicalCode LIKE 'R%' " //Race Hierarchical Codes start with an 'R'
                           + "OR patientrace.CdcrecCode='" + SOut.String(PatientRace.DECLINE_SPECIFY_RACE_CODE) + "' "
                           + "THEN 1 ELSE 0 END) HasRace, "
                           + "MAX(CASE WHEN cdcrec.HeirarchicalCode LIKE 'E%' " //Ethnicity Hierarchical Codes start with an 'E'
                           + "OR patientrace.CdcrecCode='" + SOut.String(PatientRace.DECLINE_SPECIFY_ETHNICITY_CODE) + "' "
                           + "THEN 1 ELSE 0 END) HasEthnicity "
                           + "FROM patientrace "
                           + "LEFT JOIN cdcrec ON cdcrec.CdcrecCode=patientrace.CdcrecCode "
                           + "GROUP BY PatNum "
                           + "ORDER BY NULL "
                           + ") race ON race.PatNum=patient.PatNum;";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region VitalSigns

            case EhrMeasureType.VitalSigns:
                command = "SELECT A.*,COALESCE(hwCount.Count,0) AS hwCount,"
                          + "(CASE WHEN A.Birthdate <= (A.LastVisitInDateRange-INTERVAL 3 YEAR) "; //BP count only if 3 and older at time of last visit in date range
                command += "THEN COALESCE(bpCount.Count,0) ELSE 1 END) AS bpCount "
                           + "FROM (SELECT patient.PatNum,LName,FName,Birthdate,MAX(procedurelog.ProcDate) AS LastVisitInDateRange "
                           + "FROM patient "
                           + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                           + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                           + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                           + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                           + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                           + "GROUP BY patient.PatNum) A "
                           + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM vitalsign	WHERE Height>0 AND Weight>0 GROUP BY PatNum) hwCount ON hwCount.PatNum=A.PatNum "
                           + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM vitalsign WHERE BpSystolic>0 AND BpDiastolic>0 GROUP BY PatNum) bpCount ON bpCount.PatNum=A.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region VitalSignsBMIOnly

            case EhrMeasureType.VitalSignsBMIOnly:
                command = "SELECT A.*,COALESCE(hwCount.Count,0) AS hwCount "
                          + "FROM (SELECT patient.PatNum,LName,FName FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) A "
                          + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM vitalsign	WHERE Height>0 AND Weight>0 GROUP BY PatNum) hwCount ON hwCount.PatNum=A.PatNum ";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region VitalSignsBPOnly

            case EhrMeasureType.VitalSignsBPOnly:
                command = "SELECT patient.PatNum,LName,FName,Birthdate,COUNT(DISTINCT VitalsignNum) AS bpcount "
                          + "FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum "
                          + "AND procedurelog.ProcStatus=2	AND procedurelog.ProvNum IN(" + SOut.String(provs) + ") "
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "LEFT JOIN vitalsign ON vitalsign.PatNum=patient.PatNum AND BpSystolic!=0 AND BpDiastolic!=0 "
                          + "GROUP BY patient.PatNum "
                          + "HAVING Birthdate<=MAX(ProcDate)-INTERVAL 3 YEAR "; //only include in results if over 3 yrs old at date of last visit
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region Smoking

            case EhrMeasureType.Smoking:
                command = "SELECT patient.PatNum,LName,FName,SmokingSnoMed FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ") "
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "WHERE patient.Birthdate <= " + SOut.Date(DateTime.Today.AddYears(-13)) + " " //13 and older
                          + "GROUP BY patient.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region ElectronicCopyAccess

            case EhrMeasureType.ElectronicCopyAccess:
                command = "SELECT patient.PatNum,patient.LName,patient.FName,OnlineAccess.dateProvided,ProcCheck.MinProcDate AS leastRecentDate "
                          + "FROM patient "
                          + "INNER JOIN ( "
                          + "SELECT procedurelog.PatNum,MIN(procedurelog.ProcDate) AS MinProcDate "
                          + "FROM procedurelog "
                          + "WHERE procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.C) + " ";
                brokenApptCodes = string.Join(",", ProcedureCodes.GetWhereFromList(x => x.ProcCode == "D9986" || x.ProcCode == "D9987")
                    .Select(x => x.CodeNum));
                if (!string.IsNullOrEmpty(brokenApptCodes)) command += "AND CodeNum NOT IN (" + brokenApptCodes + ") ";
                command += "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                           + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                           + "GROUP BY procedurelog.PatNum "
                           + "ORDER BY NULL "
                           + ") ProcCheck ON ProcCheck.PatNum = patient.PatNum "
                           + "LEFT JOIN ( "
                           + "SELECT ehrmeasureevent.PatNum,MIN(ehrmeasureevent.DateTEvent) AS dateProvided "
                           + "FROM ehrmeasureevent "
                           + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.OnlineAccessProvided) + " "
                           + "GROUP BY ehrmeasureevent.PatNum "
                           + "ORDER BY NULL "
                           + ") OnlineAccess ON patient.PatNum=OnlineAccess.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region ElectronicCopy

            case EhrMeasureType.ElectronicCopy:
                command = "SELECT patient.PatNum,patient.LName,patient.FName,OnlineAccess.dateRequested "
                          + "FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "LEFT JOIN (SELECT ehrmeasureevent.PatNum, MIN(ehrmeasureevent.DateTEvent) as dateRequested FROM ehrmeasureevent "
                          + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.ElectronicCopyRequested) + " "
                          + "AND " + DbHelper.Year("ehrmeasureevent.DateTEvent") + "=" + SOut.Int(dateEnd.Year) + " "
                          + "GROUP BY patnum) "
                          + "OnlineAccess ON patient.PatNum=OnlineAccess.PatNum "
                          + "GROUP BY patient.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region ClinicalSummaries

            case EhrMeasureType.ClinicalSummaries:
                command = "SELECT patient.PatNum,LName,FName,ClinSum.summaryProvided as summaryProvided,ProcCheck.ProcDate as procDate "
                          + "FROM patient "
                          + "INNER JOIN ( "
                          + "SELECT procedurelog.PatNum,procedurelog.ProcNum,procedurelog.ProcDate "
                          + "FROM procedurelog "
                          + "WHERE procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.C) + " ";
                brokenApptCodes = string.Join(",", ProcedureCodes.GetWhereFromList(x => x.ProcCode == "D9986" || x.ProcCode == "D9987")
                    .Select(x => x.CodeNum));
                if (!string.IsNullOrEmpty(brokenApptCodes)) command += "AND CodeNum NOT IN (" + brokenApptCodes + ") ";
                command += "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                           + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                           + ") ProcCheck ON ProcCheck.PatNum = patient.PatNum "
                           + "LEFT JOIN ( "
                           + "SELECT ehrmeasureevent.PatNum,MIN(ehrmeasureevent.DateTEvent) AS summaryProvided "
                           + "FROM ehrmeasureevent "
                           + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.ClinicalSummaryProvidedToPt) + " "
                           + "GROUP BY ehrmeasureevent.PatNum "
                           + "ORDER BY NULL "
                           + ") ClinSum ON patient.PatNum=ClinSum.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region Lab

            case EhrMeasureType.Lab:
                command = "SELECT 1 AS IsOldLab,patient.PatNum,LName,FName,DateTimeOrder,COALESCE(panels.Count,0) AS ResultCount FROM patient "
                          + "INNER JOIN medicalorder ON patient.PatNum=medicalorder.PatNum "
                          + "AND MedOrderType=" + SOut.Int((int) MedicalOrderType.Laboratory) + " "
                          + "AND medicalorder.ProvNum IN(" + SOut.String(provs) + ") "
                          + "AND DATE(DateTimeOrder) BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "LEFT JOIN (SELECT MedicalOrderNum,COUNT(*) AS 'Count' FROM labpanel GROUP BY MedicalOrderNum "
                          + ") panels ON panels.MedicalOrderNum=medicalorder.MedicalOrderNum "
                          + "UNION ALL "
                          + "SELECT 0 AS IsOldLab,patient.PatNum,LName,FName,STR_TO_DATE(ObservationDateTimeStart,'%Y%m%d') AS DateTimeOrder,COALESCE(ehrlabs.Count,0) AS ResultCount FROM patient "
                          + "INNER JOIN ehrlab ON patient.PatNum=ehrlab.PatNum "
                          + "LEFT JOIN (SELECT EhrLabNum, COUNT(*) AS 'Count' FROM ehrlabresult "
                          + "WHERE ehrlabresult.ValueType='NM' OR ehrlabresult.ValueType='SN' "
                          + "OR ehrlabresult.ObservationValueCodedElementID IN (" + _snomedLabResult + ") "
                          + "OR ehrlabresult.ObservationValueCodedElementIDAlt IN (" + _snomedLabResult + ") "
                          + "GROUP BY EhrLabNum "
                          + ") ehrlabs ON ehrlab.EhrLabNum=ehrlabs.EhrLabNum "
                          + "WHERE (CASE WHEN ehrlab.OrderingProviderIdentifierTypeCode='NPI' THEN ehrlab.OrderingProviderID IN(" + SOut.String(provNPIs) + ") " //When the lab is using a NPI number to determine provider.
                          + "WHEN ehrlab.OrderingProviderIdentifierTypeCode='PRN' THEN ( " //When the lab is using provider number to determine provider.
                          + "CASE WHEN ehrlab.OrderingProviderAssigningAuthorityUniversalID=( " //If the AssigningAuthority is OpenDental.
                          + "SELECT IDRoot FROM oidinternal WHERE IDType='Provider' GROUP BY IDType "
                          + ") THEN ehrlab.OrderingProviderID IN(" + SOut.String(provs) + ") END) " //Use the ProvNum to determine provider.
                          + "ELSE FALSE END) " //If the AssigningAuthority is not OpenDental, we have no way to tell who the provider is.
                          + "AND ehrlab.ObservationDateTimeStart BETWEEN DATE_FORMAT(" + SOut.Date(dateStart) + ",'%Y%m%d') AND DATE_FORMAT(" + SOut.Date(dateEnd) + ",'%Y%m%d') "
                          + "AND (CASE WHEN ehrlab.UsiCodeSystemName='LN' THEN ehrlab.UsiID WHEN ehrlab.UsiCodeSystemNameAlt='LN' THEN ehrlab.UsiIDAlt ELSE '' END) "
                          + "NOT IN (SELECT LoincCode FROM loinc WHERE loinc.ClassType LIKE '%RAD%')"; //Not sure if we need this since rad labs shouldnt be set to numeric results
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region Reminders

            case EhrMeasureType.Reminders:
                command = "SELECT patient.PatNum,LName,FName,COALESCE(reminderCount.Count,0) AS reminderCount FROM patient "
                          + "INNER JOIN(SELECT PatNum FROM ( "
                          + "SELECT PatNum, ProcDate FROM procedurelog "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "WHERE ProcStatus=" + SOut.Int((int) ProcStat.C) + " "
                          + "AND ProcDate>" + SOut.Date(dateStart) + "-INTERVAL 2 YEAR "
                          + "AND ProcDate<" + SOut.Date(dateStart) + " GROUP BY PatNum,ProcDate) uniqueprocdates "
                          + "GROUP BY uniqueprocdates.PatNum HAVING COUNT(*)>1) procscomplete ON procscomplete.PatNum=patient.PatNum "
                          + "LEFT JOIN (SELECT ehrmeasureevent.PatNum,COUNT(*) AS 'Count' FROM ehrmeasureevent "
                          + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.ReminderSent) + " "
                          + "AND DATE(ehrmeasureevent.DateTEvent) BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "GROUP BY ehrmeasureevent.PatNum) reminderCount ON reminderCount.PatNum=patient.PatNum "
                          + "WHERE patient.Birthdate > '1880-01-01' " //a birthdate is entered
                          + "AND patient.PatStatus=" + SOut.Int((int) PatientStatus.Patient) + " "
                          + "AND patient.PriProv IN(" + SOut.String(provs) + ") "
                          + "GROUP BY patient.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region Education

            case EhrMeasureType.Education:
                command = "SELECT A.*,COALESCE(edCount.Count,0) AS edCount "
                          + "FROM (SELECT patient.PatNum,LName,FName	FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) A "
                          + "LEFT JOIN (SELECT PatNum,COUNT(*) AS 'Count' FROM ehrmeasureevent "
                          + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.EducationProvided) + " "
                          + "GROUP BY PatNum) edCount ON edCount.PatNum=A.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region MedReconcile

            case EhrMeasureType.MedReconcile:
                command = "SELECT ptsRefCnt.*,COALESCE(RecCount,0) AS ReconcileCount "
                          + "FROM (SELECT ptsSeen.*,COUNT(DISTINCT refattach.RefAttachNum) AS RefCount "
                          + "FROM (SELECT patient.PatNum,LName,FName FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum "
                          + "AND ProcStatus=2 AND ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) ptsSeen "
                          + "INNER JOIN refattach ON ptsSeen.PatNum=refattach.PatNum "
                          + "AND RefDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "AND RefType=" + SOut.Int((int) ReferralType.RefFrom) + " AND IsTransitionOfCare=1 "
                          + "GROUP BY ptsSeen.PatNum) ptsRefCnt "
                          + "LEFT JOIN (SELECT ehrmeasureevent.PatNum,COUNT(*) AS RecCount FROM ehrmeasureevent "
                          + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.MedicationReconcile) + " "
                          + "AND DATE(ehrmeasureevent.DateTEvent) BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "GROUP BY ehrmeasureevent.PatNum) ptsRecCount ON ptsRefCnt.PatNum=ptsRecCount.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region SummaryOfCare

            case EhrMeasureType.SummaryOfCare:
                command = "SELECT patient.PatNum,patient.LName,patient.FName,refattach.RefDate, "
                          + "referral.FName AS RefFName,referral.LName AS RefLName,SUM(CASE WHEN ISNULL(socevent.FKey) THEN 0 ELSE 1 END) AS SOCSent "
                          + "FROM refattach "
                          + "INNER JOIN referral ON referral.ReferralNum=refattach.ReferralNum "
                          + "INNER JOIN patient ON patient.PatNum=refattach.PatNum "
                          + "LEFT JOIN ( "
                          + "SELECT ehrmeasureevent.FKey "
                          + "FROM ehrmeasureevent "
                          + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.SummaryOfCareProvidedToDr) + " "
                          + "AND DATE(ehrmeasureevent.DateTEvent) BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + ") socevent ON socevent.FKey=refattach.RefAttachNum "
                          + "WHERE RefDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "AND RefType=" + SOut.Int((int) ReferralType.RefTo) + " AND IsTransitionOfCare=1 "
                          + "AND refattach.ProvNum IN(" + SOut.String(provs) + ") "
                          + "GROUP BY refattach.RefAttachNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region SummaryOfCareElectronic

            case EhrMeasureType.SummaryOfCareElectronic:
                command = "SELECT patient.PatNum,patient.LName,patient.FName,refattach.RefDate, "
                          + "referral.FName AS RefFName,referral.LName AS RefLName,SUM(CASE WHEN ISNULL(socevent.FKey) THEN 0 ELSE 1 END) AS ElecSOCSent "
                          + "FROM refattach "
                          + "INNER JOIN referral ON referral.ReferralNum=refattach.ReferralNum "
                          + "INNER JOIN patient ON patient.PatNum=refattach.PatNum "
                          + "LEFT JOIN ( "
                          + "SELECT ehrmeasureevent.FKey "
                          + "FROM ehrmeasureevent "
                          + "WHERE EventType=" + SOut.Int((int) EhrMeasureEventType.SummaryOfCareProvidedToDrElectronic) + " "
                          + "AND DATE(ehrmeasureevent.DateTEvent) BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + ") socevent ON socevent.FKey=refattach.RefAttachNum "
                          + "WHERE RefDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "AND RefType=" + SOut.Int((int) ReferralType.RefTo) + " AND IsTransitionOfCare=1 "
                          + "AND refattach.ProvNum IN(" + SOut.String(provs) + ") "
                          + "GROUP BY refattach.RefAttachNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region SecureMessaging

            case EhrMeasureType.SecureMessaging:
                var listWebMailSentTypes = EmailMessages.GetSentTypes(EmailPlatform.WebMail);
                var webMailSentTypesStr = string.Join(",", listWebMailSentTypes.Select(x => SOut.Int((int) x)));
                command = "SELECT patient.PatNum, "
                          + "patient.LName, "
                          + "patient.FName, "
                          + "(CASE COALESCE(Emails.PatNumSubj,0) WHEN 0 THEN 0 ELSE 1 END) AS PatEmailed "
                          + "FROM procedurelog "
                          + "INNER JOIN procedurecode ON procedurecode.CodeNum = procedurelog.CodeNum AND procedurecode.ProcCode NOT IN ('D9986', 'D9987') "
                          + "INNER JOIN patient ON patient.PatNum = procedurelog.PatNum "
                          + "LEFT JOIN( "
                          + "SELECT emailmessage.PatNumSubj "
                          + "FROM emailmessage "
                          + "WHERE emailmessage.SentOrReceived IN (" + webMailSentTypesStr + ") "
                          + "AND emailmessage.MsgDateTime BETWEEN DATE_FORMAT(" + SOut.Date(dateStart) + ",'%Y-01-01') AND (DATE_FORMAT(" + SOut.Date(dateEnd) + ",'%Y-12-31')+INTERVAL 1 DAY) "
                          + "AND emailmessage.ProvNumWebMail IN(" + SOut.String(provs) + ")	"
                          + "GROUP BY emailmessage.PatNumSubj "
                          + ") Emails ON Emails.PatNumSubj = procedurelog.PatNum "
                          + "WHERE procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.C) + " "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "GROUP BY procedurelog.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region FamilyHistory

            case EhrMeasureType.FamilyHistory:
                command = "SELECT * FROM (SELECT patient.PatNum,LName,FName, procedurelog.ProcDate as procDate "
                          + "FROM patient "
                          + "INNER JOIN procedurelog ON procedurelog.PatNum=patient.PatNum AND procedurelog.ProcStatus=2 "
                          + "AND procedurelog.ProvNum IN(" + SOut.String(provs) + ")	"
                          + "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                          + "INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum "
                          + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                          + "GROUP BY patient.PatNum) AS UniquePatsAndProcs "
                          + "LEFT JOIN familyhealth ON UniquePatsAndProcs.PatNum=familyhealth.PatNum "
                          + "GROUP BY UniquePatsAndProcs.PatNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            #region ElectricNote

            case EhrMeasureType.ElectronicNote:
                //We changed this query to use temp tables so that we could add an index to the table. MySQL 5.6 adds indexes to subqueries
                //on the fly, so we could go back to the commented out version once most of our customers are on 5.6. In my testing the original query
                //took 1 hr 2 min on 5.5, the temp table query took 45 sec on either MySQL version, and the original query took 2 min on MySQL 5.6.
                //cmcgehee 01/23/2017.
                //command="SELECT patient.PatNum,patient.LName,patient.FName,notes.NumNotes "
                //	+"FROM patient "
                //	+"INNER JOIN ( "
                //		+"SELECT procedurelog.PatNum,MAX(procedurelog.ProvNum IN("+POut.String(provs)+") AND procedurelog.ProcStatus="+POut.Int((int)ProcStat.C)+") ComplProcs,"
                //		+"COUNT(DISTINCT procnotesigned.ProcNum) AS NumNotes "
                //		+"FROM procedurelog "
                //		+"LEFT JOIN ( "
                //			+"SELECT procnote.PatNum, procnote.ProcNum "
                //			+"FROM procnote "
                //			+"INNER JOIN ( "
                //				+"SELECT ProcNum,MAX(EntryDateTime) AS NewestNoteDateTime "
                //				+"FROM procnote "
                //				+"GROUP BY ProcNum "
                //				+"ORDER BY NULL"
                //			+") newestnote ON newestnote.ProcNum=procNote.ProcNum AND newestnote.NewestNoteDateTime=procnote.EntryDateTime "
                //			+"WHERE Signature!='' AND Note!='' "
                //			+"GROUP BY procnote.ProcNum "
                //			+"ORDER BY NULL"
                //		+") procnotesigned ON procedurelog.PatNum=procnotesigned.PatNum "
                //		+"WHERE procedurelog.ProcStatus!="+POut.Int((int)ProcStat.D)+" ";
                //brokenApptCodes=string.Join(",",ProcedureCodes.ListDeep.Where(x => x.ProcCode=="D9986" || x.ProcCode=="D9987")
                //	.Select(x => x.CodeNum));
                //if(!string.IsNullOrEmpty(brokenApptCodes)) {
                //	command+="AND CodeNum NOT IN ("+brokenApptCodes+") ";
                //}
                //command+="AND procedurelog.ProcDate BETWEEN "+POut.Date(dateStart)+" AND "+POut.Date(dateEnd)+" "
                //		+"GROUP BY procedurelog.PatNum "
                //		+"ORDER BY NULL"
                //	+") notes ON notes.PatNum=patient.PatNum AND notes.ComplProcs=1";
                //tableRaw=DataCore.GetTable(command);
                command = "CREATE TABLE tempprocnotesigned" + rndStr + @" (
						PatNum BIGINT(20) NOT NULL,
						ProcNum BIGINT(20) NOT NULL,
						INDEX(PatNum)
					)";
                Db.NonQ(command);
                command = "INSERT INTO tempprocnotesigned" + rndStr + @" (
						SELECT procnote.PatNum,procnote.ProcNum 
						FROM procnote 
						INNER JOIN (
							SELECT ProcNum,MAX(EntryDateTime) AS NewestNoteDateTime 
							FROM procnote 
							GROUP BY ProcNum 
							ORDER BY NULL
						) newestnote ON newestnote.ProcNum = procNote.ProcNum 
							AND newestnote.NewestNoteDateTime = procnote.EntryDateTime 
						WHERE Signature != '' 
						AND Note != '' 
						GROUP BY procnote.ProcNum 
						ORDER BY NULL
					)";
                Db.NonQ(command);
                command = @"SELECT patient.PatNum,patient.LName,patient.FName,notes.NumNotes 
						FROM patient 
						INNER JOIN (
							SELECT procedurelog.PatNum,
							MAX(procedurelog.ProvNum IN(" + SOut.String(provs) + ") AND procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.C) + @") ComplProcs,
							COUNT(DISTINCT tempprocnotesigned" + rndStr + @".ProcNum) AS NumNotes 
							FROM procedurelog 
							LEFT JOIN tempprocnotesigned" + rndStr + " ON procedurelog.PatNum=tempprocnotesigned" + rndStr + @".PatNum 
							WHERE procedurelog.ProcStatus !=" + SOut.Int((int) ProcStat.D) + " ";
                brokenApptCodes = string.Join(",", ProcedureCodes.GetWhereFromList(x => x.ProcCode == "D9986" || x.ProcCode == "D9987")
                    .Select(x => x.CodeNum));
                if (!string.IsNullOrEmpty(brokenApptCodes)) command += "AND CodeNum NOT IN (" + brokenApptCodes + ") ";
                command += "AND procedurelog.ProcDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + @"
							GROUP BY procedurelog.PatNum 
							ORDER BY NULL
						) notes ON notes.PatNum=patient.PatNum AND notes.ComplProcs=1";
                tableRaw = DataCore.GetTable(command);
                command = "DROP TABLE IF EXISTS tempprocnotesigned" + rndStr;
                Db.NonQ(command);
                break;

            #endregion

            #region LabImages

            case EhrMeasureType.LabImages:
                command = "SELECT labsTable.PatNum,labsTable.LName,labsTable.FName,labImage.DocNum,STR_TO_DATE(ObservationDateTimeStart,'%Y%m%d') AS ObservationDateTimeStart FROM ( "
                          + "SELECT patient.PatNum,patient.LName,patient.FName,ehrlab.ObservationDateTimeStart,ehrlab.EhrLabNum "
                          + "FROM ehrlab "
                          + "INNER JOIN patient ON ehrlab.PatNum=Patient.PatNum "
                          + "WHERE (CASE WHEN ehrlab.OrderingProviderIdentifierTypeCode='NPI' THEN ehrlab.OrderingProviderID IN(" + SOut.String(provNPIs) + ") " //When the lab is using a NPI number to determine provider.
                          + "WHEN ehrlab.OrderingProviderIdentifierTypeCode='PRN' THEN ( " //When the lab is using provider number to determine provider.
                          + "CASE WHEN ehrlab.OrderingProviderAssigningAuthorityUniversalID=( " //If the AssigningAuthority is OpenDental.
                          + "SELECT IDRoot FROM oidinternal WHERE IDType='Provider' GROUP BY IDType "
                          + ") THEN ehrlab.OrderingProviderID IN(" + SOut.String(provs) + ") END) " //Use the ProvNum to determine provider.
                          + "ELSE FALSE END) " //If the AssigningAuthority is not OpenDental, we have no way to tell who the provider is.
                          + "AND ehrlab.ObservationDateTimeStart BETWEEN DATE_FORMAT(" + SOut.Date(dateStart) + ",'%Y%m%d') AND DATE_FORMAT(" + SOut.Date(dateEnd) + ",'%Y%m%d') "
                          + ") as labsTable "
                          + "INNER JOIN (SELECT DISTINCT EhrLabNum,DocNum FROM ehrlabimage) AS labImage ON labsTable.EhrLabNum=labImage.EhrLabNum";
                tableRaw = DataCore.GetTable(command);
                break;

            #endregion

            //default:
            //throw new ApplicationException("Type not found: "+mtype.ToString());
        }

        //PatNum, PatientName, Explanation, and Met (X).
        var table = new DataTable("audit");
        DataRow row;
        table.Columns.Add("PatNum");
        table.Columns.Add("patientName");
        table.Columns.Add("explanation");
        table.Columns.Add("met"); //X or empty
        var rows = new List<DataRow>();
        Patient pat;
        string explanation;
        for (var i = 0; i < tableRaw.Rows.Count; i++)
        {
            row = table.NewRow();
            row["PatNum"] = tableRaw.Rows[i]["PatNum"].ToString();
            pat = new Patient();
            pat.LName = tableRaw.Rows[i]["LName"].ToString();
            pat.FName = tableRaw.Rows[i]["FName"].ToString();
            pat.Preferred = "";
            row["patientName"] = pat.GetNameLF();
            row["met"] = "";
            explanation = "";
            switch (mtype)
            {
                #region CPOE_MedOrdersOnly

                case EhrMeasureType.CPOE_MedOrdersOnly:
                    var medOrderStartDate = SIn.Date(tableRaw.Rows[i]["DateStart"].ToString());
                    explanation = "Medication order: " + tableRaw.Rows[i]["MedDescript"] + ", start date: " + medOrderStartDate.ToShortDateString() + ".";
                    if (tableRaw.Rows[i]["IsCpoe"].ToString() == "1") row["met"] = "X";
                    break;

                #endregion

                #region CPOE_LabOrdersOnly

                case EhrMeasureType.CPOE_LabOrdersOnly:
                    var labOrderStartDate = SIn.DateTime(tableRaw.Rows[i]["ObservationDateTimeStart"].ToString());
                    var labIsCpoe = SIn.Bool(tableRaw.Rows[i]["IsCpoe"].ToString());
                    explanation = "Laboratory order: " + labOrderStartDate.ToShortDateString() + " ";
                    if (labIsCpoe)
                    {
                        row["met"] = "X";
                        explanation += " is Cpoe";
                    }
                    else
                    {
                        explanation += " is not Cpoe";
                    }

                    break;

                #endregion

                #region CPOE_RadiologyOrdersOnly

                case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                    var radOrderStartDate = SIn.DateTime(tableRaw.Rows[i]["ObservationDateTimeStart"].ToString());
                    var radIsCpoe = SIn.Bool(tableRaw.Rows[i]["IsCpoe"].ToString());
                    explanation = "Radiology order: " + radOrderStartDate.ToShortDateString() + " ";
                    if (radIsCpoe)
                    {
                        row["met"] = "X";
                        explanation += " is Cpoe";
                    }
                    else
                    {
                        explanation += " is not Cpoe";
                    }

                    break;

                #endregion

                #region Rx

                case EhrMeasureType.Rx:
                    var sendStatus = (RxSendStatus) SIn.Int(tableRaw.Rows[i]["SendStatus"].ToString());
                    var rxDate = SIn.Date(tableRaw.Rows[i]["rxDate"].ToString());
                    if (sendStatus == RxSendStatus.SentElect)
                    {
                        explanation = rxDate.ToShortDateString() + " Rx sent electronically.";
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = rxDate.ToShortDateString() + " Rx not sent electronically.";
                    }

                    break;

                #endregion

                #region Demographics

                case EhrMeasureType.Demographics:
                    if (SIn.Date(tableRaw.Rows[i]["Birthdate"].ToString()).Year < 1880) explanation += "birthdate"; //missing
                    if (tableRaw.Rows[i]["Language"].ToString() == "")
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "language";
                    }

                    if (SIn.Int(tableRaw.Rows[i]["Gender"].ToString()) == (int) PatientGender.Unknown)
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "gender";
                    }

                    //if(PatientRaces.GetForPatient(PIn.Long(row["PatNum"].ToString())).Count==0) {
                    //	if(explanation!="") {
                    //		explanation+=", ";
                    //	}
                    //	explanation+="race, ethnicity";
                    //}
                    if (SIn.Int(tableRaw.Rows[i]["HasRace"].ToString()) == 0)
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "race";
                    }

                    if (SIn.Int(tableRaw.Rows[i]["HasEthnicity"].ToString()) == 0)
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "ethnicity";
                    }

                    if (explanation == "")
                    {
                        explanation = "All demographic elements recorded";
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = "Missing: " + explanation;
                    }

                    break;

                #endregion

                #region VitalSigns

                case EhrMeasureType.VitalSigns:
                    if (tableRaw.Rows[i]["hwCount"].ToString() == "0") explanation += "height, weight";
                    if (tableRaw.Rows[i]["bpCount"].ToString() == "0")
                    {
                        if (explanation != "") explanation += ", ";
                        explanation += "blood pressure";
                    }

                    if (explanation == "")
                    {
                        explanation = "Vital signs entered";
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = "Missing: " + explanation;
                    }

                    break;

                #endregion

                #region VitalSignsBMIOnly

                case EhrMeasureType.VitalSignsBMIOnly:
                    if (tableRaw.Rows[i]["hwCount"].ToString() == "0") explanation += "height, weight";
                    if (explanation == "")
                    {
                        explanation = "Vital signs entered";
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = "Missing: " + explanation;
                    }

                    break;

                #endregion

                #region VitalSignsBPOnly

                case EhrMeasureType.VitalSignsBPOnly:
                    if (tableRaw.Rows[i]["bpCount"].ToString() == "0")
                    {
                        explanation = "Missing: blood pressure";
                    }
                    else
                    {
                        explanation = "Vital signs entered";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region Smoking

                case EhrMeasureType.Smoking:
                    var smokeSnoMed = tableRaw.Rows[i]["SmokingSnoMed"].ToString();
                    if (smokeSnoMed == "449868002" //Current every day smoker
                        || smokeSnoMed == "428041000124106" //Current some day smoker
                        || smokeSnoMed == "8517006" //Former smoker
                        || smokeSnoMed == "266919005" //Never smoker
                        || smokeSnoMed == "77176002" //Smoker, current status unknown
                        || smokeSnoMed == "266927001" //Unknown if ever smoked
                        || smokeSnoMed == "428071000124103" //Heavy tobacco smoker
                        || smokeSnoMed == "428061000124105") //Light tobacco smoker
                    {
                        explanation = "Smoking status entered.";
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = "Smoking status not entered.";
                    }

                    break;

                #endregion

                #region ElectronicCopyAccess

                case EhrMeasureType.ElectronicCopyAccess:
                    var visitDate = SIn.Date(tableRaw.Rows[i]["leastRecentDate"].ToString());
                    var deadlineDate = SIn.Date(tableRaw.Rows[i]["leastRecentDate"].ToString());
                    var providedDate = SIn.Date(tableRaw.Rows[i]["dateProvided"].ToString());
                    deadlineDate = deadlineDate.AddDays(4);
                    if (visitDate.DayOfWeek > DayOfWeek.Tuesday) deadlineDate = deadlineDate.AddDays(2);
                    if (providedDate <= deadlineDate && providedDate.Year > 1880)
                    {
                        explanation = "Online access provided before " + deadlineDate.ToShortDateString();
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = visitDate.ToShortDateString() + " no online access provided";
                    }

                    break;

                #endregion

                #region ElectronicCopy

                case EhrMeasureType.ElectronicCopy:
                    var dateRequested = SIn.Date(tableRaw.Rows[i]["dateRequested"].ToString());
                    if (dateRequested.Year != dateEnd.Year)
                    {
                        explanation = "No requests in the calendar year of the specified period.";
                    }
                    else if (dateRequested.Year > 1880)
                    {
                        explanation = "Requested on " + dateRequested.ToShortDateString();
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region ClinicalSummaries

                case EhrMeasureType.ClinicalSummaries:
                    var procDate = SIn.Date(tableRaw.Rows[i]["procDate"].ToString());
                    var deadlineDateClinSum = procDate.AddDays(1);
                    if (procDate.DayOfWeek == DayOfWeek.Friday) deadlineDateClinSum = deadlineDateClinSum.AddDays(2);
                    var summaryProvidedDate = SIn.Date(tableRaw.Rows[i]["summaryProvided"].ToString());
                    if (summaryProvidedDate == DateTime.MinValue)
                    {
                        explanation = procDate.ToShortDateString() + " no summary provided to patient";
                    }
                    else if (summaryProvidedDate <= deadlineDateClinSum)
                    {
                        explanation = procDate.ToShortDateString() + " summary provided to patient";
                        row["met"] = "X";
                    }
                    else
                    {
                        explanation = procDate.ToShortDateString() + " summary provided to patient after more than one buisness day";
                    }

                    break;

                #endregion

                #region Lab

                case EhrMeasureType.Lab:
                    var resultCount = SIn.Int(tableRaw.Rows[i]["ResultCount"].ToString());
                    var isOldLab = SIn.Bool(tableRaw.Rows[i]["IsOldLab"].ToString());
                    var dateOrder = SIn.Date(tableRaw.Rows[i]["DateTimeOrder"].ToString());
                    if (resultCount == 0)
                    {
                        explanation += dateOrder.ToShortDateString() + " results not attached.";
                        explanation += isOldLab ? " (2011 edition)" : "";
                    }
                    else
                    {
                        explanation = dateOrder.ToShortDateString() + " results attached.";
                        explanation += isOldLab ? " (2011 edition)" : "";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region Reminders

                case EhrMeasureType.Reminders:
                    if (tableRaw.Rows[i]["reminderCount"].ToString() == "0")
                    {
                        explanation = "No reminders sent";
                    }
                    else
                    {
                        explanation = "Reminders sent";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region Education

                case EhrMeasureType.Education:
                    if (tableRaw.Rows[i]["edCount"].ToString() == "0")
                    {
                        explanation = "No education resources";
                    }
                    else
                    {
                        explanation = "Education resources provided";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region MedReconcile

                case EhrMeasureType.MedReconcile:
                    var refCount = SIn.Int(tableRaw.Rows[i]["RefCount"].ToString()); //this will always be greater than zero
                    var reconcileCount = SIn.Int(tableRaw.Rows[i]["ReconcileCount"].ToString());
                    if (reconcileCount < refCount)
                    {
                        explanation = "Transitions of Care:" + refCount + ", Reconciles:" + reconcileCount;
                    }
                    else
                    {
                        explanation = "Reconciles performed for each transition of care.";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region SummaryOfCare

                case EhrMeasureType.SummaryOfCare:
                    var socSent = SIn.Int(tableRaw.Rows[i]["SOCSent"].ToString());
                    var refDate = SIn.DateTime(tableRaw.Rows[i]["RefDate"].ToString());
                    var refLName = SIn.String(tableRaw.Rows[i]["RefLName"].ToString());
                    var refFName = SIn.String(tableRaw.Rows[i]["RefFName"].ToString());
                    if (socSent < 1)
                    {
                        explanation = "Referral on: " + refDate.ToShortDateString() + " to " + refLName + ", " + refFName + " not sent summary of care.";
                    }
                    else
                    {
                        explanation = "Referral on: " + refDate.ToShortDateString() + " to " + refLName + ", " + refFName + " sent summary of care.";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region SummaryOfCareElectronic

                case EhrMeasureType.SummaryOfCareElectronic:
                    var elecSOCSent = SIn.Int(tableRaw.Rows[i]["ElecSOCSent"].ToString());
                    var elecRefDate = SIn.DateTime(tableRaw.Rows[i]["RefDate"].ToString());
                    var elecRefLName = SIn.String(tableRaw.Rows[i]["RefLName"].ToString());
                    var elecRefFName = SIn.String(tableRaw.Rows[i]["RefFName"].ToString());
                    if (elecSOCSent < 1)
                    {
                        explanation = "Referral on: " + elecRefDate.ToShortDateString() + " to " + elecRefLName + ", " + elecRefFName + " not sent electronic summary of care.";
                    }
                    else
                    {
                        explanation = "Referral on: " + elecRefDate.ToShortDateString() + " to " + elecRefLName + ", " + elecRefFName + " sent electronic summary of care.";
                        row["met"] = "X";
                    }

                    break;

                #endregion

                #region SecureMessaging

                case EhrMeasureType.SecureMessaging:
                    if (SIn.Bool(tableRaw.Rows[i]["PatEmailed"].ToString())) row["met"] = "X";
                    break;

                #endregion

                #region FamilyHistory

                case EhrMeasureType.FamilyHistory:
                    if (SIn.Long(tableRaw.Rows[i]["FamilyHealthNum"].ToString()) > 0) row["met"] = "X";
                    break;

                #endregion

                #region ElectricNote

                case EhrMeasureType.ElectronicNote:
                    if (SIn.Long(tableRaw.Rows[i]["NumNotes"].ToString()) > 0) row["met"] = "X";
                    break;

                #endregion

                #region LabImages

                case EhrMeasureType.LabImages:
                    var labImageStartDate = SIn.Date(tableRaw.Rows[i]["ObservationDateTimeStart"].ToString());
                    var docNum = SIn.Long(tableRaw.Rows[i]["DocNum"].ToString());
                    explanation = "Laboratory order: " + labImageStartDate.ToShortDateString() + " ";
                    if (docNum > 0)
                    {
                        row["met"] = "X";
                        explanation += " image attached.";
                    }
                    else
                    {
                        explanation += " image not attached.";
                    }

                    break;

                #endregion

                //default:
                //throw new ApplicationException("Type not found: "+mtype.ToString());
            }

            row["explanation"] = explanation;
            rows.Add(row);
        }

        for (var i = 0; i < rows.Count; i++) table.Rows.Add(rows[i]);
        return table;
    }

    ///<summary>Returns the explanation of the numerator based on the EHR certification documents.</summary>
    private static string GetNumeratorExplainMu2Mod(EhrMeasureType mtype)
    {
        switch (mtype)
        {
            case EhrMeasureType.CPOE_MedOrdersOnly:
                return "The number of medication orders entered by the Provider during the reporting period using CPOE.";
            case EhrMeasureType.CPOE_LabOrdersOnly:
                return "The number of lab orders entered by the Provider during the reporting period using CPOE.";
            case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                return "The number of radiology orders entered by the Provider during the reporting period using CPOE.";
            case EhrMeasureType.Rx:
                return "Permissible prescriptions transmitted electronically.";
            case EhrMeasureType.Demographics:
                return "Patients with all required demographic elements recorded as structured data: language, gender, race, ethnicity, and birthdate.";
            case EhrMeasureType.VitalSigns:
                return "Patients with height, weight, and blood pressure recorded.";
            case EhrMeasureType.VitalSignsBMIOnly:
                return "Patients with height and weight recorded.";
            case EhrMeasureType.VitalSignsBPOnly:
                return "Patients with blood pressure recorded.";
            case EhrMeasureType.Smoking:
                return "Patients with smoking status recorded.";
            case EhrMeasureType.ElectronicCopyAccess:
                return "Electronic copy received within 4 business days.";
            case EhrMeasureType.ElectronicCopy:
                return "The number of unique patients in the denominator who have viewed online, downloaded, or transmitted to a third party the patient's health information.";
            case EhrMeasureType.ClinicalSummaries:
                return "Number of office visits in the denominator where the patient or a patient-authorized representative is provided a clinical summary of their visit within one business day.";
            case EhrMeasureType.Lab:
                return "Lab results entered.";
            case EhrMeasureType.Reminders:
                return "Number of patients in the denominator who were sent a reminder per patient preference when available during the EHR reporting period.";
            case EhrMeasureType.Education:
                return "Patients provided patient-specific education resources, not dependent on requests.";
            case EhrMeasureType.MedReconcile:
                return "Number of transitions of care in the denominator where medication reconciliation was performed.";
            case EhrMeasureType.SummaryOfCare:
                return "Number of transitions of care and referrals in the denominator where a summary of care record was provided.";
            case EhrMeasureType.SummaryOfCareElectronic:
                return "Number of transitions of care and referrals in the denominator where a summary of care record was electronically transmitted";
            case EhrMeasureType.SecureMessaging:
                return "The number of patients in the denominator for whom a secure electronic message is sent to the patient (or patient-authorized representative), or in response to a secure message sent by the patient (or patient-authorized representative).";
            case EhrMeasureType.FamilyHistory:
                return "The number of patients in the denominator with a structured data entry for one or more first-degree relatives.";
            case EhrMeasureType.ElectronicNote:
                return "The number of unique patients in the denominator who have at least one electronic progress note from an eligible professional recorded as text searchable data.";
            case EhrMeasureType.LabImages:
                return "The number of results in the denominator that are accessible through CEHRT.";
        }

        return "";
        //throw new ApplicationException("Type not found: "+mtype.ToString());
    }

    ///<summary>Returns the explanation of the denominator based on the EHR certification documents.</summary>
    private static string GetDenominatorExplainMu2Mod(EhrMeasureType mtype)
    {
        switch (mtype)
        {
            case EhrMeasureType.CPOE_MedOrdersOnly:
                return "The number of medication orders created by the Provider during the reporting period.";
            case EhrMeasureType.CPOE_LabOrdersOnly:
                return "The number of lab orders created by the Provider during the reporting period.";
            case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                return "The number of radiology orders created by the Provider during the reporting period.";
            case EhrMeasureType.Rx:
                return "All permissible prescriptions by the Provider during the reporting period.";
            case EhrMeasureType.Demographics:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.VitalSigns:
                return "All unique patients (age 3 and over for blood pressure) with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.VitalSignsBMIOnly:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.VitalSignsBPOnly:
                return "All unique patients age 3 and over with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.Smoking:
                return "All unique patients 13 years or older with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.ElectronicCopyAccess:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.ElectronicCopy:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.ClinicalSummaries:
                return "All office visits during the reporting period.  An office visit is calculated as any number of completed procedures by the Provider for a given date.";
            case EhrMeasureType.Lab:
                return "All lab orders by the Provider during the reporting period.";
            case EhrMeasureType.Reminders:
                return "Number of unique patients who have had two or more office visits with the EP in the 24 months prior to the beginning of the EHR reporting period.";
            case EhrMeasureType.Education:
                return "All unique patients with at least one completed procedure by the Provider during the reporting period.";
            case EhrMeasureType.MedReconcile:
                return "Number of incoming transitions of care from another provider during the reporting period.";
            case EhrMeasureType.SummaryOfCare:
                return "Number of outgoing transitions of care and referrals during the reporting period.";
            case EhrMeasureType.SummaryOfCareElectronic:
                return "Number of outgoing transitions of care and referrals during the reporting period.";
            case EhrMeasureType.SecureMessaging:
                return "Number of unique patients seen by the EP during the EHR reporting period.";
            case EhrMeasureType.FamilyHistory:
                return "Number of unique patients seen by the EP during the EHR reporting period.";
            case EhrMeasureType.ElectronicNote:
                return "Number of unique patients with at least one office visit during the EHR reporting period for EPs during the EHR reporting period.";
            case EhrMeasureType.LabImages:
                return "Number of tests whose result is one or more images ordered by the EP during the EHR reporting period.";
        }

        return "";
        //throw new ApplicationException("Type not found: "+mtype.ToString());
    }

    ///<summary>Returns the explanation of the exclusion if there is one, if none returns 'No exclusions.'.</summary>
    private static string GetExclusionExplainMu2Mod(EhrMeasureType mtype)
    {
        switch (mtype)
        {
            case EhrMeasureType.CPOE_MedOrdersOnly:
            case EhrMeasureType.CPOE_LabOrdersOnly:
            case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                return "Any Provider who writes fewer than 100 medication, radiology, or laboratory orders during the EHR reporting period.";
            case EhrMeasureType.Rx:
                return @"1. Any Provider who writes fewer than 100 prescriptions during the reporting period.
2. Any Provider who does not have a pharmacy within their organization and there are no pharmacies that accept electronic prescriptions within 10 miles of the practice at the start of the reporting period.";
            case EhrMeasureType.Demographics:
                return "No exclusions.";
            case EhrMeasureType.VitalSigns:
                return @"1. Any Provider who sees no patients 3 years or older is excluded from recording blood pressure.
2. Any Provider who believes that all three vital signs of height, weight, and blood pressure have no relevance to their scope of practice is excluded from recording them.
3. Any Provider who believes that height and weight are relevant to their scope of practice, but blood pressure is not, is excluded from recording blood pressure.
4. Any Provider who believes that blood pressure is relevant to their scope of practice, but height and weight are not, is excluded from recording height and weight.";
            case EhrMeasureType.VitalSignsBMIOnly:
                return "Any Provider who believes that height and weight are not relevant to their scope of practice is excluded from recording them.";
            case EhrMeasureType.VitalSignsBPOnly:
                return @"1. Any Provider who sees no patients 3 years or older is excluded from recording blood pressure.
2. Any Provider who believes that blood pressure is not relevant to their scope of practice is excluded from recording it.";
            case EhrMeasureType.Smoking:
                return "Any Provider who sees no patients 13 years or older during the reporting period.";
            case EhrMeasureType.ElectronicCopyAccess:
                return "Any Provider who neither orders nor creates any of the information listed for inclusion as part of both measures, except for Patient name and Provider's name and office contact information.";
            case EhrMeasureType.ElectronicCopy:
                return @"1. Any Provider who neither orders nor creates any of the information listed for inclusion as part of both measures, except for Patient name and Provider's name and office contact information.
2. Any Provider who conducts 50% or more of his or her patient encounters in a county that does not have 50% or more of its housing units with 3Mbps broadband availability according to the latest information available from the FCC on the first day of the EHR reporting period.";
            case EhrMeasureType.ClinicalSummaries:
                return "Any Provider who has no completed procedures during the reporting period.";
            case EhrMeasureType.Lab:
                return "Any Provider who orders no lab tests whose results are either in a positive/negative or numeric format during the reporting period.";
            case EhrMeasureType.Reminders:
                return "Any Provider who has had no office visits in the 24 months before the EHR reporting period.";
            case EhrMeasureType.Education:
                return "Any Provider who has no office visits during the EHR reporting period.";
            case EhrMeasureType.MedReconcile:
                return "Any Provider who was not the recipient of any transitions of care during the EHR reporting period.";
            case EhrMeasureType.SummaryOfCare:
                return "Any Provider who transfers a patient to another setting or refers a patient to another provider less than 100 times during the EHR reporting period is excluded from all three measures.";
            case EhrMeasureType.SummaryOfCareElectronic:
                return "Any Provider who transfers a patient to another setting or refers a patient to another provider less than 100 times during the EHR reporting period is excluded from all three measures.";
            case EhrMeasureType.SecureMessaging:
                return "Any EP who has no office visits during the EHR reporting period; or any EP who conducts 50 percent or more of his or her patient encounters in a county that does not have 50 percent or more of its housing units with 4Mbps broadband availability according to the latest information available from the FCC on the first day of the EHR reporting period.";
            case EhrMeasureType.FamilyHistory:
                return "Any EP who has no office visits during the EHR reporting period.";
            case EhrMeasureType.ElectronicNote:
                return "No exclusion.";
            case EhrMeasureType.LabImages:
                return "Any EP who orders less than 100 tests whose result is an image during the EHR reporting period; or any EP who has no access to electronic imaging results at the start of the EHR reporting period.";
        }

        return "";
        //throw new ApplicationException("Type not found: "+mtype.ToString());
    }

    #endregion

    #region Helper Methods

    /// <summary>
    ///     Returns an int comparison of count when comparing reconciles and referrals. If reconciles are greater than or
    ///     equal to referrals, returns 1. If referrals are greater than reconciles, returns -1. If there are no referrals,
    ///     returns 0.
    /// </summary>
    public static int CompareReferralsToReconciles(long patNum)
    {
        var listRefAttach = RefAttaches.Refresh(patNum);
        var listMeasureEvents = EhrMeasureEvents.Refresh(patNum);
        var refFromCount = 0;
        var refFromPeriodCount = 0;
        for (var i = 0; i < listRefAttach.Count; i++)
            if (listRefAttach[i].RefType == ReferralType.RefFrom && listRefAttach[i].IsTransitionOfCare)
            {
                refFromCount++;
                if (listRefAttach[i].RefDate > DateTime.Now.AddYears(-1))
                    //within the last year
                    refFromPeriodCount++;
            }

        if (refFromPeriodCount == 0) return 0;

        var listReconciles = EhrMeasureEvents.GetByType(listMeasureEvents, EhrMeasureEventType.MedicationReconcile);
        var reconcileCount = 0; //during reporting period.
        for (var i = 0; i < listReconciles.Count; i++)
            if (listReconciles[i].DateTEvent > DateTime.Now.AddYears(-1))
                //within the same period as the count for referrals.
                reconcileCount++;

        if (refFromPeriodCount > reconcileCount) return -1;
        return 1; //countReconciles is greater than or equal to countFromRefPeriod
    }

    /// <summary>
    ///     Currently we only want to call this for Stage 2 Modified.
    ///     Returns human friendly/readable string for given EhrMeasureType.
    /// </summary>
    public static string MeasureTypeTitleHelper(EhrMeasureType measureType)
    {
        switch (measureType)
        {
            //<summary>0</summary>
            case EhrMeasureType.ProblemList:
                return "Problems";
            //<summary>1</summary>
            case EhrMeasureType.MedicationList:
                return "Medications";
            //<summary>2</summary>
            case EhrMeasureType.AllergyList:
                return "Allergies";
            //<summary>3</summary>
            case EhrMeasureType.Demographics:
                return "Demographics";
            //<summary>4</summary>
            case EhrMeasureType.Education:
                return "Education Resources";
            ////<summary>5</summary>
            //case EhrMeasureType.TimelyAccess:
            //	return "";
            ////<summary>6</summary>
            //case EhrMeasureType.ProvOrderEntry:
            //	return "";
            //<summary>7</summary>
            case EhrMeasureType.CPOE_MedOrdersOnly:
                return "CPOE Med Orders";
            //<summary>8</summary>
            //case EhrMeasureType.CPOE_PreviouslyOrdered:
            //	return "";
            //<summary>9</summary>
            case EhrMeasureType.Rx:
                return "E-Prescribing";
            //<summary>10</summary>
            case EhrMeasureType.VitalSigns:
                return "Vital Signs";
            ////<summary>11</summary>
            //case EhrMeasureType.VitalSignsBMIOnly:
            //	return "";
            ////<summary>12</summary>
            //case EhrMeasureType.VitalSignsBPOnly:
            //	return "";
            //<summary>13</summary>
            case EhrMeasureType.Smoking:
                return "Smoking Status";
            ////<summary>14</summary>
            //case ehrmeasuretype.lab:
            //	return "";
            //<summary>15</summary>
            case EhrMeasureType.ElectronicCopy:
                return "Electronic Access View Download Transmit";
            ////<summary>16</summary>
            //case EhrMeasureType.ClinicalSummaries:
            //	return "";
            ////<summary>17</summary>
            //case EhrMeasureType.Reminders:
            //	return "";
            ////<summary>18</summary>
            case EhrMeasureType.MedReconcile:
                return "Med Reconcile";
            //<summary>19</summary>
            case EhrMeasureType.SummaryOfCare:
                return "Summary of Care";
            //<summary>20</summary>
            case EhrMeasureType.CPOE_LabOrdersOnly:
                return "CPOE Lab Orders";
            //<summary>21</summary>
            case EhrMeasureType.CPOE_RadiologyOrdersOnly:
                return "CPOE Rad Orders";
            //<summary>22</summary>
            case EhrMeasureType.ElectronicCopyAccess:
                return "Electronic Access";
            //<summary>23</summary>
            case EhrMeasureType.SummaryOfCareElectronic:
                return "Summary of Care Electronic";
            //<summary>24</summary>
            case EhrMeasureType.SecureMessaging:
                return "Secure Messaging";
            ////<summary>25</summary>
            //case EhrMeasureType.FamilyHistory:
            //	return "";
            ////<summary>26</summary>
            //case EhrMeasureType.ElectronicNote:
            //	return "";
            ////<summary>27</summary>
            //case EhrMeasureType.LabImages:
            //	return "";
            ////<summary>28</summary>
            //case EhrMeasureType.VitalSigns2014:
            //	return "";
            //<summary>29</summary>
            case EhrMeasureType.DrugDrugInteractChecking:
                return "CDS Drug Interaction Checks";
            //<summary>30</summary>
            case EhrMeasureType.DrugFormularyChecking:
                return "Drug Formulary Checks";
            //<summary>31</summary>
            case EhrMeasureType.ProtectElectHealthInfo:
                return "Protected Health Information";
            //<summary>32</summary>
            case EhrMeasureType.ImmunizationRegistries:
                return "Immunizations";
            //<summary>33</summary>
            case EhrMeasureType.SyndromicSurveillance:
                return "Syndromic Surveillance";
            ////<summary>34</summary>
            //case EhrMeasureType.PatientList:
            //	return "";
            //<summary>35</summary>
            case EhrMeasureType.ClinicalInterventionRules:
                return "CDS Interventions";
            default:
                return measureType.ToString();
        }
    }

    #endregion
}

///<summary>When FormEHR closes, the result will be one of these.  Different results will lead to different behaviors.</summary>
public enum EhrFormResult
{
    None,
    RxSelect,
    RxEdit,
    Medical,
    PatientEdit,
    Online,
    MedReconcile,
    Referrals,
    MedicationPatNew,
    MedicationPatEdit,
    PatientSelect //When this form closes, OD will set the current patient to PatNum
}