using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class HieQueues
{
    #region Methods - Misc

    ///<summary>Process outstanding queue records. Does not create ccds if running unit tests.</summary>
    public static string ProcessQueues()
    {
        var listHieClinicsEnabled = HieClinics.GetAllEnabled();
        if (listHieClinicsEnabled.IsNullOrEmpty()) return "No HIE clinics enabled.";
        var listHieQueuesAll = GetAll();
        if (listHieQueuesAll.IsNullOrEmpty()) return "No HIE queues to process.";
        var listHieQueuesToProcess = listHieQueuesAll.DistinctBy(x => x.PatNum).ToList();
        var listPatNumsDistinct = listHieQueuesToProcess.Select(x => x.PatNum).Distinct().ToList();
        var listPatients = Patients.GetMultPats(listPatNumsDistinct).ToList();
        var listPatPlans = PatPlans.GetPatPlansForPats(listPatNumsDistinct);
        var listInsSubs = InsSubs.GetMany(listPatPlans.Select(x => x.InsSubNum).ToList());
        var listInsPlans = InsPlans.GetPlans(listInsSubs.Select(x => x.PlanNum).ToList());
        var listCarriers = Carriers.GetCarriers(listInsPlans.Select(x => x.CarrierNum).ToList());
        var hieClinicHQ = listHieClinicsEnabled.FirstOrDefault(x => x.ClinicNum == 0);
        var listPatNumsProcessed = new List<long>();
        //Hie queue rows should only be valid for one day. This list will keep track of patnums that could not be processed.
        var listPatNumsFiltered = new List<long>();
        var listLogMsgs = new List<string>();
        for (var i = 0; i < listHieQueuesToProcess.Count; i++)
            try
            {
                var patient = listPatients.Find(x => x.PatNum == listHieQueuesToProcess[i].PatNum);
                if (patient == null) patient = Patients.GetPat(listHieQueuesToProcess[i].PatNum);
                if (patient == null)
                {
                    listPatNumsFiltered.Add(listHieQueuesToProcess[i].PatNum);
                    listLogMsgs.Add($"Patient object could not be found for PatNum '{listHieQueuesToProcess[i].PatNum}'.");
                    continue;
                }

                //Get HIE clinic that is associated to the patient's clinic
                var hieClinicForPat = listHieClinicsEnabled.Find(x => x.ClinicNum == patient.ClinicNum);
                if (hieClinicForPat == null) hieClinicForPat = hieClinicHQ;
                if (hieClinicForPat == null)
                {
                    listPatNumsFiltered.Add(listHieQueuesToProcess[i].PatNum);
                    listLogMsgs.Add($"No HIE clinic found for ClinicNum '{patient.ClinicNum}' or HQ.");
                    continue;
                }

                if (!hieClinicForPat.IsTimeToProcess())
                {
                    listLogMsgs.Add($"HIE clinic '{Clinics.GetDesc(hieClinicForPat.ClinicNum)}' was skipped because time of day not between {hieClinicForPat.TimeOfDayExportCCD.ToShortTimeString()} and {hieClinicForPat.TimeOfDayExportCCDEnd.ToShortTimeString()}.");
                    continue;
                }

                if (!Directory.Exists(hieClinicForPat.PathExportCCD) && !ODBuild.IsUnitTest)
                {
                    listLogMsgs.Add($"Export directory does not exist '{hieClinicForPat.PathExportCCD}' for HIE clinic number '{hieClinicForPat.HieClinicNum}'.");
                    continue;
                }

                if (hieClinicForPat.SupportedCarrierFlags != HieCarrierFlags.AllCarriers)
                {
                    //hieclinic has medicaid flag
                    var listPatPlansForPat = listPatPlans.FindAll(x => x.PatNum == patient.PatNum);
                    if (listPatPlansForPat.IsNullOrEmpty())
                    {
                        listPatNumsFiltered.Add(listHieQueuesToProcess[i].PatNum);
                        continue;
                    }

                    var listInsSubsForPat = listInsSubs.FindAll(x => listPatPlansForPat.Select(x => x.InsSubNum).ToList().Contains(x.InsSubNum));
                    if (listInsSubsForPat.IsNullOrEmpty())
                    {
                        listPatNumsFiltered.Add(listHieQueuesToProcess[i].PatNum);
                        continue;
                    }

                    var listInsPlansForPat = listInsPlans.FindAll(x => listInsSubsForPat.Select(x => x.PlanNum).Contains(x.PlanNum));
                    if (listInsPlansForPat.IsNullOrEmpty())
                    {
                        listPatNumsFiltered.Add(listHieQueuesToProcess[i].PatNum);
                        continue;
                    }

                    var listCarriersForPat = listCarriers.FindAll(x => listInsPlansForPat.Select(x => x.CarrierNum).ToList().Contains(x.CarrierNum));
                    if (hieClinicForPat.SupportedCarrierFlags.HasFlag(HieCarrierFlags.Medicaid) && !listCarriers.Any(x => ElectIDs.IsMedicaid(x.ElectID)))
                    {
                        //Patient does not have any Medicaid insurance plans, continue.
                        listPatNumsFiltered.Add(listHieQueuesToProcess[i].PatNum);
                        continue;
                    }
                }

                if (!ODBuild.IsUnitTest)
                {
                    //Don't create ccd export if running Unit Tests
                    //Process summary of care for the patient to the export path specified.
                    var ccdTextForPat = EhrCCD.GenerateSummaryOfCare(patient, out _, false);
                    var pathCcdExportWOExt = ODFileUtils.CombinePaths(hieClinicForPat.PathExportCCD, $"ccd_{DateTime_.Now.ToString("yyyyMMdd")}_{patient.PatNum}");
                    var fileExt = ".xml";
                    var pathCcdExport = pathCcdExportWOExt + fileExt;
                    if (File.Exists(pathCcdExport))
                    {
                        var loopCount = 1;
                        while (true)
                        {
                            if (!File.Exists(pathCcdExportWOExt + $"_{loopCount}{fileExt}")) break;
                            loopCount++;
                        }

                        pathCcdExport = pathCcdExportWOExt + $"_{loopCount}{fileExt}";
                    }

                    try
                    {
                        File.WriteAllText(pathCcdExport, ccdTextForPat);
                    }
                    catch (Exception ex)
                    {
                        listLogMsgs.Add($"Failed to write ccd file for PatNum {patient.PatNum} to path {pathCcdExport}.\r\n{ex.Message}");
                        continue; //We will try again later.
                    }
                }

                EhrMeasureEvents.CreateEventForPat(patient.PatNum, EhrMeasureEventType.SummaryOfCareProvidedToDrElectronic);
                listPatNumsProcessed.Add(patient.PatNum);
            }
            catch (Exception ex)
            {
                listLogMsgs.Add($"Failed to generate CCD for PatNum {listHieQueuesToProcess[i].PatNum}.");
                listLogMsgs.Add($"Error:{ex.Message}");
            }

        var listHieQueueNumsToDeleted = new List<long>();
        if (!listPatNumsFiltered.IsNullOrEmpty())
        {
            listLogMsgs.Add($"Count of HIE queues filtered {listHieQueuesToProcess.Count}.");
            //Get the list of hiequeues for patnums that we could not process in memory to delete.
            listHieQueueNumsToDeleted
                .AddRange(listHieQueuesAll.Where(x => listPatNumsFiltered.Contains(x.PatNum)).Select(x => x.HieQueueNum).ToList());
        }

        if (!listPatNumsProcessed.IsNullOrEmpty())
        {
            listLogMsgs.Add($"Count of HIE queues processed {listPatNumsProcessed.Count}.");
            //Get the list of hiequeues for patnums that were processed in memory to delete.
            listHieQueueNumsToDeleted
                .AddRange(listHieQueuesAll.Where(x => listPatNumsProcessed.Contains(x.PatNum)).Select(x => x.HieQueueNum).ToList());
        }

        for (var i = 0; i < listHieQueueNumsToDeleted.Count; i++) Delete(listHieQueueNumsToDeleted[i]);
        return string.Join("\r\n", listLogMsgs.Distinct());
    }

    #endregion Methods - Misc

    #region Methods - Get

    public static List<HieQueue> GetAll()
    {
        var command = "SELECT * FROM hiequeue";
        return HieQueueCrud.SelectMany(command);
    }

    
    public static List<HieQueue> Refresh(long patNum)
    {
        var command = "SELECT * FROM hiequeue WHERE PatNum = " + SOut.Long(patNum);
        return HieQueueCrud.SelectMany(command);
    }

    ///<summary>Gets one HieQueue from the db.</summary>
    public static HieQueue GetOne(long hieQueueNum)
    {
        return HieQueueCrud.SelectOne(hieQueueNum);
    }

    #endregion Methods - Get

    #region Methods - Modify

    
    public static long Insert(HieQueue hieQueue)
    {
        return HieQueueCrud.Insert(hieQueue);
    }

    
    public static void Update(HieQueue hieQueue)
    {
        HieQueueCrud.Update(hieQueue);
    }

    
    public static void Delete(long hieQueueNum)
    {
        HieQueueCrud.Delete(hieQueueNum);
    }

    #endregion Methods - Modify
}