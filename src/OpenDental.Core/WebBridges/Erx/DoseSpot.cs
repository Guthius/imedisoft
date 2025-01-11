using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CodeBase;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using Newtonsoft.Json;
using OpenDentBusiness.DoseSpotService;
using OpenDentBusiness.localhost;

namespace OpenDentBusiness
{
    public class DoseSpot
    {
        private const string DoseSpotPatNum = "25128"; //25128 is DoseSpot's patnum in the OD HQ database.
        private const string DoseSpotOid = "2.16.840.1.113883.3.4337.25128";
        private static string _randomPhrase32;
        
        public static string GetDoseSpotRoot()
        {
            //The advantage of returning the root from the database is that there could be a scary edge case where 
            // the PatNum of the office's regkey in OD HQ database could have been changed
            OIDExternal oIDExternal = OIDExternals.GetByPartialRootExternal(DoseSpotOid);
            if (oIDExternal == null)
            {
                oIDExternal = new OIDExternal();
                oIDExternal.IDType = IdentifierType.Root;
                oIDExternal.rootExternal = OIDInternals.OpenDentalOID + "." + DoseSpotPatNum + "." + OIDInternals.GetCustomerPatNum();
                OIDExternals.Insert(oIDExternal);
            }

            return oIDExternal.rootExternal;
        }

        public static OIDExternal GetDoseSpotPatID(long patNum)
        {
            return OIDExternals.GetOidExternal(GetDoseSpotRoot() + "." + POut.Int((int) IdentifierType.Patient), patNum, IdentifierType.Patient);
        }

        public static OIDExternal GetDoseSpotRootOid()
        {
            return OIDExternals.GetByPartialRootExternal(DoseSpotOid);
        }
        
        public static bool IsDoseSpotAccountId(string accountId)
        {
            return accountId.ToUpper().StartsWith("DS;"); //ToUpper because user might have manually typed in.
        }

        public static string GenerateAccountId(long patNum)
        {
            string accountId = "DS;" + POut.Long(patNum);
            accountId += "-" + MiscUtils.CreateRandomAlphaNumericString(3);
            long checkSum = patNum;
            checkSum += Convert.ToByte(accountId[accountId.IndexOf('-') + 1]) * 3;
            checkSum += Convert.ToByte(accountId[accountId.IndexOf('-') + 2]) * 5;
            checkSum += Convert.ToByte(accountId[accountId.IndexOf('-') + 3]) * 7;
            accountId += (checkSum % 100).ToString().PadLeft(2, '0');
            return accountId;
        }
        
        public static string CreateSsoCode(string clinicKey, bool isQueryStr = true)
        {
            string singleSignOnCode = ""; //1. You have been provided a clinic key (in UTF8).
            string phrase = Get32CharPhrase(); //2. Create a random phrase 32 characters long in UTF8
            string phraseAndKey = phrase;
            phraseAndKey += clinicKey; //3. Append the key to the phrase
            byte[] arrayBytes = GetBytesFromUTF8(phraseAndKey); //4. Get the value in Bytes from UTF8 String
            byte[] arrayHashedBytes = GetSHA512Hash(arrayBytes); //5. Use SHA512 to hash the byte value you just received
            string base64hash = Convert.ToBase64String(arrayHashedBytes); //6. Get a Base64String out of the hash that you created
            base64hash = RemoveExtraEqualSigns(base64hash); //7. If there are two = signs at the end, then remove them
            singleSignOnCode = phrase + base64hash; //8. Prepend the same random phrase from step 2 to your code
            if (isQueryStr)
            {
                //9. If the SingleSignOnCode is going to be passed in a query string, be sure to UrlEncode the entire code
                singleSignOnCode = WebUtility.UrlEncode(singleSignOnCode);
            }

            return singleSignOnCode;
        }
        
        public static string CreateSsoUserIdVerify(string clinicKey, string userID, bool isQueryStr = true)
        {
            string singleSignOnCode = "";
            string phrase = Get32CharPhrase();
            string idPhraseAndKey = phrase.Substring(0, 22); //1. Grab the first 22 characters of the phrase used in CreateSSOCode from step 1 of CreateSsoCode
            idPhraseAndKey = userID + idPhraseAndKey; //2. Append to the UserID string the 22 characters grabbed from step one
            idPhraseAndKey += clinicKey; //3. Append the key to the string created in 2b
            byte[] arrayBytes = GetBytesFromUTF8(idPhraseAndKey); //4. Get the Byte value of the string
            byte[] arrayHashedBytes = GetSHA512Hash(arrayBytes); //5. Use SHA512 to hash the byte value you just received
            string base64hash = Convert.ToBase64String(arrayHashedBytes); //6. Get a Base64String out of the hash that you created
            singleSignOnCode = RemoveExtraEqualSigns(base64hash); //7. If there are two = signs at the end, then remove them
            if (isQueryStr)
            {
                //8. If the SingleSignOnUserIdVerify is going to be passed in a query string, be sure to UrlEncode the entire code
                singleSignOnCode = WebUtility.UrlEncode(singleSignOnCode);
            }

            _randomPhrase32 = null;
            return singleSignOnCode;
        }

        public static bool SyncPrescriptionsFromDoseSpot(string clinicID, string clinicKey, string userID, long patNum, Action<List<RxPat>> onRxAdd = null)
        {
            OIDExternal oIDExternal = GetDoseSpotPatID(patNum);
            if (oIDExternal == null)
            {
                return false; //We don't have a PatID from DoseSpot for this patient.  Therefore there is nothing to sync with.
            }

            bool hasChangedPrescriptions = false;
            Patient patient = Patients.GetPat(patNum);
            List<long> listMedicationPatNumsActive = new List<long>();
            Dictionary<int, string> dictPharmacyIdToPharmacyName = new Dictionary<int, string>();
            List<RxPat> listRxPats = new List<RxPat>();
            string token = DoseSpotREST.GetToken(userID, clinicID, clinicKey);
            //Get rid of any deleted prescriptions.
            List<DoseSpotPrescription> listDoseSpotPerscriptions = DoseSpotREST.GetPrescriptions(token, oIDExternal.IDExternal);
            List<DoseSpotMedicationWrapper> listDoseSpotMedicationWrappers = listDoseSpotPerscriptions.Select(x => new DoseSpotMedicationWrapper(x, null)).ToList();
            //Add self reported medications.
            listDoseSpotMedicationWrappers.AddRange(DoseSpotREST.GetSelfReported(token, oIDExternal.IDExternal).Select(x => new DoseSpotMedicationWrapper(null, x)).ToList());
            foreach (DoseSpotMedicationWrapper doseSpotMedicationWrapper in listDoseSpotMedicationWrappers.Where(x => x.MedicationStatus != DoseSpotREST.MedicationStatus.Deleted))
            {
                RxPat rxPatOld = null;
                if (doseSpotMedicationWrapper.IsSelfReported)
                {
                    //Get self reported that originated in OD
                    rxPatOld = RxPats.GetErxByIdForPat(Erx.OpenDentalErxPrefix + doseSpotMedicationWrapper.MedicationId.ToString(), patNum);
                    if (rxPatOld == null)
                    {
                        //Get self reported that originated in DS
                        rxPatOld = RxPats.GetErxByIdForPat(Erx.DoseSpotPatReportedPrefix + doseSpotMedicationWrapper.MedicationId.ToString(), patNum);
                    }
                }
                else
                {
                    //Isn't self reported, Guid won't have a prefix.
                    rxPatOld = RxPats.GetErxByIdForPat(doseSpotMedicationWrapper.MedicationId.ToString(), patNum);
                }

                RxPat rxPat = new RxPat();
                long rxCui = doseSpotMedicationWrapper.RxCUI; //If this is zero either DoseSpot didn't send the value or there was an issue casting from string to long.
                rxPat.IsControlled = (PIn.Int(doseSpotMedicationWrapper.Schedule) != 0); //Controlled if Schedule is I,II,III,IV,V
                rxPat.DosageCode = "";
                rxPat.SendStatus = RxSendStatus.Unsent;
                switch (doseSpotMedicationWrapper.PrescriptionStatus)
                {
                    case DoseSpotREST.PrescriptionStatus.PharmacyVerified:
                    case DoseSpotREST.PrescriptionStatus.eRxSent:
                        rxPat.SendStatus = RxSendStatus.SentElect;
                        break;
                    case DoseSpotREST.PrescriptionStatus.FaxSent:
                        rxPat.SendStatus = RxSendStatus.Faxed;
                        break;
                    case DoseSpotREST.PrescriptionStatus.Printed:
                        rxPat.SendStatus = RxSendStatus.Printed;
                        break;
                    case DoseSpotREST.PrescriptionStatus.Sending:
                        rxPat.SendStatus = RxSendStatus.Pending;
                        break;
                    case DoseSpotREST.PrescriptionStatus.Deleted:
                        if (rxPatOld == null)
                        {
                            //DoseSpot sent a deleted medication that we don't have a record of. Skip it.
                            continue;
                        }

                        MedicationPat medicationPat = MedicationPats.GetMedicationOrderByErxIdAndPat(rxPatOld.ErxGuid, rxPatOld.PatNum);
                        RxPats.Delete(rxPatOld.RxNum);
                        hasChangedPrescriptions = true;
                        SecurityLog securityLog = new SecurityLog();
                        securityLog.UserNum = 0; //Don't attach to user since this is being done by DoseSpot
                        securityLog.CompName = Security.GetComplexComputerName();
                        securityLog.PermType = EnumPermType.RxEdit;
                        securityLog.FKey = 0;
                        securityLog.LogSource = LogSources.eRx;
                        securityLog.LogText = "FROM(" + rxPatOld.RxDate.ToShortDateString() + "," + rxPatOld.Drug + "," + rxPatOld.ProvNum + "," + rxPatOld.Disp + "," + rxPatOld.Refills + ")" + "\r\nTO 'deleted' change made by DoseSpot eRx.";
                        securityLog.PatNum = rxPatOld.PatNum;
                        securityLog.DefNum = 0;
                        securityLog.DefNumError = 0;
                        securityLog.DateTPrevious = DateTime.MinValue;
                        SecurityLogs.MakeLogEntry(securityLog);
                        if (medicationPat != null)
                        {
                            MedicationPats.Delete(medicationPat);
                            securityLog.PermType = EnumPermType.PatMedicationListEdit;
                            securityLog.LogText = (medicationPat.MedicationNum == 0 ? medicationPat.MedDescript : Medications.GetMedication(medicationPat.MedicationNum).MedName
                                                  ) + " deleted by DoseSpot" + "\r\n"
                                                  + (String.IsNullOrEmpty(medicationPat.PatNote) ? "" : "Pat note: " + medicationPat.PatNote);
                            securityLog.PatNum = medicationPat.PatNum; //probably the same but better safe than sorry.
                            securityLog.SecurityLogNum = 0; //Reset primary key to guarantee insert will work no matter what changes get made to securitylog code.
                            SecurityLogs.MakeLogEntry(securityLog);
                        }

                        break;
                    case DoseSpotREST.PrescriptionStatus.Error:
                    case DoseSpotREST.PrescriptionStatus.EpcsError:
                        continue; //Skip these medications since DoseSpot is saying that they are invalid
                    case DoseSpotREST.PrescriptionStatus.Edited:
                    case DoseSpotREST.PrescriptionStatus.Entered:
                    case DoseSpotREST.PrescriptionStatus.EpcsSigned:
                    case DoseSpotREST.PrescriptionStatus.ReadyToSign:
                    case DoseSpotREST.PrescriptionStatus.Requested:
                    default:
                        rxPat.SendStatus = RxSendStatus.Unsent;
                        break;
                }

                rxPat.Refills = doseSpotMedicationWrapper.Refills;
                rxPat.Disp = doseSpotMedicationWrapper.Quantity; //In DoseSpot, the Quanitity textbox's label says "Dispense".
                rxPat.Drug = doseSpotMedicationWrapper.DisplayName;
                if (doseSpotMedicationWrapper.PharmacyId.HasValue)
                {
                    try
                    {
                        if (!dictPharmacyIdToPharmacyName.ContainsKey(doseSpotMedicationWrapper.PharmacyId.Value))
                        {
                            dictPharmacyIdToPharmacyName.Add(doseSpotMedicationWrapper.PharmacyId.Value, DoseSpotREST.GetPharmacyName(token, doseSpotMedicationWrapper.PharmacyId.Value));
                        }

                        rxPat.ErxPharmacyInfo = dictPharmacyIdToPharmacyName[doseSpotMedicationWrapper.PharmacyId.Value];
                    }
                    catch (Exception ex)
                    {
                        //Do nothing.  It was a nicety anyways.
                    }
                }

                rxPat.PatNum = patNum;
                rxPat.Sig = doseSpotMedicationWrapper.Directions;
                rxPat.Notes = doseSpotMedicationWrapper.RxNotes;
                rxPat.RxDate = DateTime.MinValue;
                //If none of dates have values, the RxDate will be MinValue.
                //This is acceptable if DoseSpot doesn't give us anything, which should never happen.
                if (doseSpotMedicationWrapper.DateWritten.HasValue)
                {
                    rxPat.RxDate = doseSpotMedicationWrapper.DateWritten.Value;
                }
                else if (doseSpotMedicationWrapper.DateReported.HasValue)
                {
                    rxPat.RxDate = doseSpotMedicationWrapper.DateReported.Value;
                }
                else if (doseSpotMedicationWrapper.DateLastFilled.HasValue)
                {
                    rxPat.RxDate = doseSpotMedicationWrapper.DateLastFilled.Value;
                }
                else if (doseSpotMedicationWrapper.DateInactive.HasValue)
                {
                    rxPat.RxDate = doseSpotMedicationWrapper.DateInactive.Value;
                }

                //Save DoseSpot's unique ID into our rx
                int doseSpotMedId = (int?) doseSpotMedicationWrapper.MedicationId ?? 0; //If this changes, we need to ensure that Erx.IsFromDoseSpot() is updated to match.
                rxPat.ErxGuid = doseSpotMedId.ToString();
                bool isProvider = false;
                if (doseSpotMedicationWrapper.IsSelfReported)
                {
                    //Self Reported medications won't have a prescriber number
                    if (rxPatOld == null)
                    {
                        //Rx doesn't exist in the database.  This probably originated from DoseSpot
                        MedicationPat medicationPat = MedicationPats.GetMedicationOrderByErxIdAndPat(Erx.OpenDentalErxPrefix + doseSpotMedicationWrapper.MedicationId.ToString(), patNum);
                        if (medicationPat == null)
                        {
                            //If there isn't a record of the medication 
                            medicationPat = MedicationPats.GetMedicationOrderByErxIdAndPat(Erx.DoseSpotPatReportedPrefix + doseSpotMedicationWrapper.MedicationId.ToString(), patNum);
                        }

                        if (medicationPat == null)
                        {
                            //If medPat is null at this point we don't have a record of this patient having the medication, so it probably was just made in DoseSpot.
                            rxPat.ErxGuid = Erx.DoseSpotPatReportedPrefix + doseSpotMedicationWrapper.MedicationId;
                        }
                        else
                        {
                            rxPat.ErxGuid = medicationPat.ErxGuid; //Maintain the ErxGuid that was assigned for the MedicationPat that already exists.
                        }
                    }
                    else
                    {
                        rxPat.ErxGuid = rxPatOld.ErxGuid; //Maintain the ErxGuid that was already assigned for the Rx.
                    }
                }
                else
                {
                    //The prescriber ID for each medication is the doctor that approved the prescription.
                    UserOdPref userOdPref = UserOdPrefs.GetByFkeyAndFkeyType(Programs.GetCur(ProgramName.eRx).ProgramNum, UserOdFkeyType.Program)
                        .FirstOrDefault(x => x.ValueString == doseSpotMedicationWrapper.PrescriberId.ToString());
                    if (userOdPref == null)
                    {
                        //The Dose Spot User ID from this medication is not present in Open Dental.
                        continue; //I don't know if we want to do anything with this.  Maybe we want to just get the ErxLog from before this medication was made.
                    }

                    Userod userod = Userods.GetUser(userOdPref.UserNum);
                    Provider provider = new Provider();
                    isProvider = !Erx.IsUserAnEmployee(userod);
                    if (isProvider)
                    {
                        //A user always be a provider if there is a ProvNum > 0
                        provider = Providers.GetProv(userod.ProvNum);
                    }
                    else
                    {
                        provider = Providers.GetProv(patient.PriProv);
                    }

                    if (provider == null && rxPatOld != null)
                    {
                        //Provide a fallback in case the provider is 'missing' due to being detached from the userod
                        rxPat.ProvNum = rxPatOld.ProvNum;
                    }

                    if (provider != null)
                    {
                        rxPat.ProvNum = provider.ProvNum;
                    }
                }

                //These fields are possibly set above, preserve old values if they are not.
                if (rxPatOld != null)
                {
                    rxPat.Disp = rxPatOld.Disp; //The medication Disp currently always returns 0. Preserve the old value.
                    rxPat.Refills = rxPat.Refills == null ? rxPatOld.Refills : rxPat.Refills;
                    rxPat.Notes = rxPatOld.Notes.IsNullOrEmpty() ? rxPat.Notes : rxPatOld.Notes; //Preserve the note already in OD no matter what if there is one.
                    rxPat.PatientInstruction = rxPat.PatientInstruction.IsNullOrEmpty() ? rxPatOld.PatientInstruction : rxPat.PatientInstruction;
                    rxPat.ErxPharmacyInfo = rxPat.ErxPharmacyInfo == null ? rxPatOld.ErxPharmacyInfo : rxPat.ErxPharmacyInfo;
                    rxPat.IsControlled = rxPatOld.IsControlled;
                    if (rxPatOld.RxDate.Year > 1880)
                    {
                        rxPat.RxDate = rxPatOld.RxDate;
                    }
                }

                long medicationPatNum = 0;
                hasChangedPrescriptions |= RxPats.UpdateComparison(rxPat, rxPatOld);
                if (Erx.IsDoseSpotPatReported(rxPat.ErxGuid) || Erx.IsTwoWayIntegrated(rxPat.ErxGuid))
                {
                    //For DoseSpot self reported, do not insert a prescription.
                    medicationPatNum = Erx.InsertOrUpdateErxMedication(rxPatOld, rxPat, rxCui, doseSpotMedicationWrapper.DisplayName, doseSpotMedicationWrapper.GenericProductName, isProvider, false);
                }
                else
                {
                    medicationPatNum = Erx.InsertOrUpdateErxMedication(rxPatOld, rxPat, rxCui, doseSpotMedicationWrapper.DisplayName, doseSpotMedicationWrapper.GenericProductName, isProvider);
                }

                if (rxPatOld == null && !doseSpotMedicationWrapper.MedicationStatus.In(DoseSpotREST.MedicationStatus.Unknown, DoseSpotREST.MedicationStatus.Inactive))
                {
                    //Only add the rx if it is new and it's status isn't deprecated.  We don't want to trigger automation for existing prescriptions.
                    listRxPats.Add(rxPat);
                }

                if (doseSpotMedicationWrapper.MedicationStatus == DoseSpotREST.MedicationStatus.Active)
                {
                    listMedicationPatNumsActive.Add(medicationPatNum);
                }
            }

            List<MedicationPat> listMedicationPats = MedicationPats.Refresh(patNum, false);
            for (int i = 0; i < listMedicationPats.Count; i++)
            {
                //This loop should update the end date for: Perscriptions made in DoseSpot, Medications made in DoseSpot, and Medications made in OD
                string eRxGuidCur = listMedicationPats[i].ErxGuid;
                if (!Erx.IsFromDoseSpot(eRxGuidCur) && !Erx.IsDoseSpotPatReported(eRxGuidCur) && !Erx.IsTwoWayIntegrated(eRxGuidCur))
                {
                    continue; //This medication is not synced with DoseSpot, don't update.
                }

                if (listMedicationPatNumsActive.Contains(listMedicationPats[i].MedicationPatNum))
                {
                    continue; //The medication is still active.
                }

                if (listMedicationPats[i].DateStop.Year > 1880)
                {
                    continue; //The medication is already discontinued in Open Dental.
                }

                //The medication was discontinued inside the eRx interface.
                DoseSpotMedicationWrapper doseSpotMedicationWrapper = listDoseSpotMedicationWrappers.FirstOrDefault(x => Erx.OpenDentalErxPrefix + x.MedicationId.ToString() == eRxGuidCur);
                if (doseSpotMedicationWrapper == null)
                {
                    doseSpotMedicationWrapper = listDoseSpotMedicationWrappers.FirstOrDefault(x => Erx.DoseSpotPatReportedPrefix + x.MedicationId.ToString() == eRxGuidCur);
                }

                if (doseSpotMedicationWrapper == null)
                {
                    //We don't have a medication from DS for this medicationpat, kick out.
                    continue;
                }

                //Try to get the date stop.
                if (doseSpotMedicationWrapper.DateInactive != null)
                {
                    listMedicationPats[i].DateStop = doseSpotMedicationWrapper.DateInactive.Value;
                }
                else
                {
                    listMedicationPats[i].DateStop = DateTime.Today.AddDays(-1); //Discontinue the medication as of yesterday so that it will immediately show as discontinued.
                }

                MedicationPats.Update(listMedicationPats[i]); //Discontinue the medication inside OD to match what shows in the eRx interface.
                string medDescript = listMedicationPats[i].MedDescript;
                if (listMedicationPats[i].MedicationNum != 0)
                {
                    medDescript = Medications.GetMedication(listMedicationPats[i].MedicationNum).MedName;
                }

                SecurityLogs.MakeLogEntry(EnumPermType.PatMedicationListEdit, listMedicationPats[i].PatNum, medDescript + " DoseSpot set inactive", LogSources.eRx);
            }

            if (onRxAdd != null && listRxPats.Count != 0)
            {
                onRxAdd(listRxPats);
            }

            return hasChangedPrescriptions;
        }
        
        public static void SyncPrescriptionsToDoseSpot(string clinicID, string clinicKey, string userID, long patNum)
        {
            string token = DoseSpotREST.GetToken(userID, clinicID, clinicKey);
            OIDExternal oIDExternal = GetDoseSpotPatID(patNum);
            if (oIDExternal == null)
            {
                return; //We don't have a PatID from DoseSpot for this patient.  Therefore there is nothing to sync with.
            }

            List<MedicationPat> listMedicationPats = MedicationPats.Refresh(patNum, true).FindAll(x => !Erx.IsFromDoseSpot(x.ErxGuid));
            if (listMedicationPats.Count == 0)
            {
                return; //There are no medications to send to DoseSpot.
            }

            foreach (MedicationPat medicationPat in listMedicationPats)
            {
                //Medications originating from DS are filtered out when the list is retrieved.
                DoseSpotSelfReported doseSpotSelfReported = DoseSpotREST.MedicationPatToDoseSpotSelfReport(medicationPat);
                if (string.IsNullOrWhiteSpace(doseSpotSelfReported.DisplayName))
                {
                    //Couldn't get a name from the medicationpat or the medication, don't send to DS without a name.
                    continue;
                }

                if (doseSpotSelfReported.SelfReportedMedicationId > 0)
                {
                    //We were able to get the external ID for this self reported from the medicationpat, therefore we want to do an edit.
                    try
                    {
                        DoseSpotREST.PutSelfReportedMedications(token, oIDExternal.IDExternal, doseSpotSelfReported);
                    }
                    catch (ODException e)
                    {
                        if (e.Message.Contains("does not belong to PatientID"))
                        {
                            //skip these, get past a bug in the DS API where it returns medications for the wrong patient.
                            continue;
                        }
                        else
                        {
                            throw e;
                        }
                    }
                }
                else
                {
                    //Save the ID returned by DoseSpot.
                    RxPat rxPat = null;
                    if (medicationPat.ErxGuid.StartsWith(Erx.UnsentPrefix))
                    {
                        rxPat = RxPats.GetErxByIdForPat(medicationPat.ErxGuid, medicationPat.PatNum);
                    }

                    medicationPat.ErxGuid = Erx.OpenDentalErxPrefix + DoseSpotREST.PostSelfReportedMedications(token, oIDExternal.IDExternal, doseSpotSelfReported);
                    if (rxPat != null)
                    {
                        rxPat.ErxGuid = medicationPat.ErxGuid;
                        RxPats.Update(rxPat);
                    }
                }

                //Update the medPat to store an ErxGuid and the returned ID from DoseSpot so that we don't keep sending this unnecessarily.
                MedicationPats.Update(medicationPat);
            }
        }
        
        public static void GetPrescriberNotificationCounts(string clinicID, string clinicKey, string userID, out int countRefillReqs, out int countTransactionErrors, out int countPendingPrescriptionsCount)
        {
            countRefillReqs = 0;
            countTransactionErrors = 0;
            string token = DoseSpotREST.GetToken(userID, clinicID, clinicKey);
            DoseSpotREST.GetNotificationCounts(token, out countRefillReqs, out countTransactionErrors, out countPendingPrescriptionsCount);

            #region SOAP - Deprecated

            //DoseSpotService.API api=new DoseSpotService.API();
            //			if(ODBuild.IsDebug()) {
            //				api.Url="https://my.staging.dosespot.com/api/12/api.asmx?wsdl";
            //			}
            //			DoseSpotService.GetPrescriberNotificationCountsRequest req=new DoseSpotService.GetPrescriberNotificationCountsRequest();
            //			req.SingleSignOn=GetSingleSignOn(clinicID,clinicKey,userID,false);
            //			DoseSpotService.GetPrescriberNotificationCountsResponse res=api.GetPrescriberNotificationCounts(req);
            //			if(res.Result!=null && res.Result.ResultCode.ToLower()=="error") {
            //				ODException.ErrorCodes errorCode=ODException.ErrorCodes.NotDefined;
            //				if(Erx.IsUserAnEmployee(Security.CurUser) && res.Result.ResultDescription.Contains("not authorized") || res.Result.ResultDescription.Contains("unauthorized")) {
            //					errorCode=ODException.ErrorCodes.DoseSpotNotAuthorized;
            //				}
            //				throw new ODException(res.Result.ResultDescription,errorCode);
            //			}
            //			countRefillReqs=res.RefillRequestsCount;
            //			countTransactionErrors=res.TransactionErrorsCount;
            //			countPendingPrescriptionsCount=res.PendingPrescriptionsCount;

            #endregion
        }

        public static string GetSingleSignOnQueryString(string clinicID, string clinicKey, string userID, string onBehalfOfUserId, Patient patient)
        {
            //Pass in false for isQueryString because we will URLEncode the values below in QueryStringAddParameter.  It was a bug where we were double encoding the data.
            SingleSignOn singleSignOn = GetSingleSignOn(clinicID, clinicKey, userID, false);
            StringBuilder stringBuilder = new StringBuilder();
            QueryStringAddParameter(stringBuilder, "SingleSignOnCode", singleSignOn.SingleSignOnCode);
            QueryStringAddParameter(stringBuilder, "SingleSignOnUserId", POut.Int(singleSignOn.SingleSignOnUserId));
            QueryStringAddParameter(stringBuilder, "SingleSignOnUserIdVerify", singleSignOn.SingleSignOnUserIdVerify);
            QueryStringAddParameter(stringBuilder, "SingleSignOnClinicId", POut.Int(singleSignOn.SingleSignOnClinicId));
            if (!String.IsNullOrWhiteSpace(onBehalfOfUserId))
            {
                QueryStringAddParameter(stringBuilder, "OnBehalfOfUserId", POut.String(onBehalfOfUserId));
            }

            if (patient == null)
            {
                QueryStringAddParameter(stringBuilder, "RefillsErrors", POut.Int(1)); //Request transmission errors
            }
            else
            {
                OIDExternal oIDExternal = GetDoseSpotPatID(patient.PatNum);
                if (oIDExternal != null)
                {
                    QueryStringAddParameter(stringBuilder, "PatientId", oIDExternal.IDExternal);
                }
            }

            return stringBuilder.ToString();
        }

        public static void GetClinicIdAndKey(long clinicNum, Program program, List<ProgramProperty> listProgramProperties, out string clinicID, out string clinicKey)
        {
            clinicID = "";
            clinicKey = "";
            if (program == null)
            {
                program = Programs.GetCur(ProgramName.eRx);
            }

            if (listProgramProperties == null)
            {
                listProgramProperties = ProgramProperties.GetForProgram(program.ProgramNum)
                    .FindAll(x => x.ClinicNum == clinicNum
                                  && (x.PropertyDesc == Erx.PropertyDescs.ClinicID || x.PropertyDesc == Erx.PropertyDescs.ClinicKey));
            }

            ProgramProperty programPropertyClinicID = listProgramProperties.FirstOrDefault(x => x.ClinicNum == clinicNum && x.PropertyDesc == Erx.PropertyDescs.ClinicID);
            ProgramProperty programPropertyClinicKey = listProgramProperties.FirstOrDefault(x => x.ClinicNum == clinicNum && x.PropertyDesc == Erx.PropertyDescs.ClinicKey);
            //If the current clinic doesn't have a clinic id/key, use a different clinic to make them.
            if (programPropertyClinicID == null || string.IsNullOrWhiteSpace(programPropertyClinicID.PropertyValue)
                                                || programPropertyClinicKey == null || string.IsNullOrWhiteSpace(programPropertyClinicKey.PropertyValue))
            {
                throw new ODException(((clinicNum == 0) ? "HQ " : Clinics.GetAbbr(clinicNum) + " ")
                                      + Lans.g("DoseSpot", "is missing a valid ClinicID or Clinic Key.  This should have been entered when setting up DoseSpot."));
            }
            else
            {
                clinicID = programPropertyClinicID.PropertyValue;
                clinicKey = programPropertyClinicKey.PropertyValue;
            }
        }

        public static void RegisterClinic(long clinicNum, string clinicID, string clinicKey, string userID, out string clinicIdNew, out string clinicKeyNew)
        {
            string token = DoseSpotREST.GetToken(userID, clinicID, clinicKey);
            var clinic = GetClinicOrPracticeInfo(clinicNum);

            #region SOAP - Deprecated

            //			DoseSpotService.API api=new DoseSpotService.API();
            //			DoseSpotService.ClinicAddMessage req=new DoseSpotService.ClinicAddMessage();
            //			req.Clinic=MakeDoseSpotClinic(clinicCur);
            //			req.SingleSignOn=GetSingleSignOn(clinicID,clinicKey,userID,false);
            //			if(ODBuild.IsDebug()) {
            //				//This code will output the XML into the console.  This may be needed for DoseSpot when troubleshooting issues.
            //				//This XML will be the soap body and exclude the header and envelope.
            //				System.Xml.Serialization.XmlSerializer xml=new System.Xml.Serialization.XmlSerializer(req.GetType());
            //				xml.Serialize(Console.Out,req);
            //			}
            //DoseSpotService.ClinicAddResultMessage res=api.ClinicAdd(req);
            //if(res.Result!=null && res.Result.ResultCode.ToLower().Contains("error")) {
            //	throw new Exception(res.Result.ResultDescription);
            //}

            #endregion

            DoseSpotREST.PostClinic(token, clinic, out clinicIdNew, out clinicKeyNew);
            long programNumErx = Programs.GetCur(ProgramName.eRx).ProgramNum;
            List<ProgramProperty> listProgramProperties = ProgramProperties.GetListForProgramAndClinic(programNumErx, clinicNum);
            ProgramProperty programPropertyClinicID = listProgramProperties.FirstOrDefault(x => x.PropertyDesc == Erx.PropertyDescs.ClinicID);
            ProgramProperty programPropertyClinicKey = listProgramProperties.FirstOrDefault(x => x.PropertyDesc == Erx.PropertyDescs.ClinicKey);
            //Update the database with the new id/key.
            InsertOrUpdate(programPropertyClinicID, programNumErx, Erx.PropertyDescs.ClinicID, clinicIdNew, clinicNum);
            InsertOrUpdate(programPropertyClinicKey, programNumErx, Erx.PropertyDescs.ClinicKey, clinicKeyNew, clinicNum);
            //Ensure cache is not stale after setting the values.
            Cache.Refresh(InvalidType.Programs);
        }

        public static string GetUserID(Userod userod, long clinicNum)
        {
            string doseSpotUserID = "";
            //At this point we know that we have a valid clinic/practice info and valid provider.
            Program program = Programs.GetCur(ProgramName.eRx);
            //Get the DoseSpotID for the current user
            UserOdPref userOdPref = GetDoseSpotUserIdFromPref(userod.UserNum, clinicNum);
            //If the current user doesn't have a valid User ID, go retreive one from DoseSpot.
            if (userOdPref == null || string.IsNullOrWhiteSpace(userOdPref.ValueString))
            {
                //If there is no UserId for this user, throw an exception.  The below code was when we thought the Podio database matched the DoseSpot database.
                //The below code would add a proxy clinician via the API and give back the DoseSpot User ID.
                //This was causing issues with Podio and making sure the proxy clinician has access to the appropriate clinics.
                throw new ODException("Missing DoseSpot User ID for user.  Call support to register provider or proxy user, then enter User ID into security window.");

                #region Old Proxy User Registration

                //        UserOdPref otherRegisteredClinician=UserOdPrefs.GetAllByFkeyAndFkeyType(programErx.ProgramNum,UserOdFkeyType.Program)
                //          .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.ValueString) && Userods.GetUser(x.UserNum).ProvNum!=0);
                //        //userCur.ProvNum==0 means that this is a real clinician.  
                //        //We can add proxy clinicians for no charge, but actual clinicians will incur a fee.
                //        if(!Erx.IsUserAnEmployee(userCur) || otherRegisteredClinician==null) {
                //          //Either the prov isn't registered, or there are no credentials to create the proxy clinician.
                //          //Either way, we want the user to know they need to register the provider.
                //          throw new ODException("Missing DoseSpot User ID for provider.  Call support to register provider, then enter User ID into security window.");
                //        }
                //        //Get the provider from the doseSpotUserID we are using.  This ensures that DoseSpot knows the provider is valid,
                //        //instead of passing in the patient's primary provider, which may not be registered in DoseSpot.
                //        Provider provOther=Providers.GetProv(Userods.GetUser(otherRegisteredClinician.UserNum).ProvNum);
                //				ValidateProvider(provOther,clinicNum);
                //				string defaultDoseSpotUserID=otherRegisteredClinician.ValueString;
                //				string clinicID="";
                //				string clinicKey="";
                //				GetClinicIdAndKey(clinicNum,defaultDoseSpotUserID,programErx,null,out clinicID,out clinicKey);
                //				DoseSpotService.API api=new DoseSpotService.API();
                //				DoseSpotService.ClinicianAddMessage req=new DoseSpotService.ClinicianAddMessage();
                //				req.SingleSignOn=GetSingleSignOn(clinicID,clinicKey,defaultDoseSpotUserID,false);
                //				EmailAddress email=EmailAddresses.GetForUser(userCur.UserNum);
                //				if(email==null || string.IsNullOrWhiteSpace(email.EmailUsername)) {
                //					throw new ODException("Invalid email address for the current user.");
                //				}
                //				req.Clinician=MakeDoseSpotClinician(provOther,clinicCur,email.EmailUsername,true);//If the user isn't a provider, they are a proxy clinician.
                //				if(ODBuild.IsDebug()) {
                //					//This code will output the XML into the console.  This may be needed for DoseSpot when troubleshooting issues.
                //					//This XML will be the soap body and exclude the header and envelope.
                //					System.Xml.Serialization.XmlSerializer xml=new System.Xml.Serialization.XmlSerializer(req.GetType());
                //					xml.Serialize(Console.Out,req);
                //				}
                //				DoseSpotService.ClinicianAddResultsMessage res=api.ClinicianAdd(req);
                //				if(res.Result!=null && (res.Result.ResultCode.ToLower().Contains("error") || res.Clinician==null)) {
                //					throw new Exception(res.Result.ResultDescription);
                //				}
                //				retVal=res.Clinician.ClinicianId.ToString();
                //				//Since userPrefDoseSpotID can't be null, we just overwrite all of the fields to be sure that they are correct.
                //				userPrefDoseSpotID.UserNum=userCur.UserNum;
                //				userPrefDoseSpotID.Fkey=programErx.ProgramNum;
                //				userPrefDoseSpotID.FkeyType=UserOdFkeyType.Program;
                //				userPrefDoseSpotID.ValueString=retVal;
                //				if(userPrefDoseSpotID.IsNew) {
                //					UserOdPrefs.Insert(userPrefDoseSpotID);
                //				}
                //				else {
                //					UserOdPrefs.Update(userPrefDoseSpotID);
                //				}

                #endregion
            }
            else
            {
                doseSpotUserID = userOdPref.ValueString;
            }

            return doseSpotUserID;
        }

        public static bool SyncClinicErxsWithHQ()
        {
            MakeClinicErxsForDoseSpot();
            List<ClinicErx> listClinicErxs = ClinicErxs.GetWhere(x => x.EnabledStatus != ErxStatus.Enabled);
            //Currently we do not have any intention of disabling clinics from HQ since there is no cost associated to adding a clinic.
            //Because of this, don't make extra web calls to check if HQ has tried to disable any clinics.
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.IndentChars = ("    ");
            StringBuilder stringBuilder = new StringBuilder();
            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, xmlWriterSettings))
            {
                xmlWriter.WriteStartElement("ErxClinicAccessRequest");
                xmlWriter.WriteStartElement("RegistrationKey");
                xmlWriter.WriteString(PrefC.GetString(PrefName.RegistrationKey));
                xmlWriter.WriteEndElement(); //End reg key
                xmlWriter.WriteStartElement("RegKeyDisabledOverride");
                //Allow disabled regkeys to use eRx.  This functionality matches how we handle a disabled regkey for providererx
                //providererx in CustUpdates only cares that the regkey is valid and associated to a patnum in ODHQ
                xmlWriter.WriteString("true");
                xmlWriter.WriteEndElement(); //End reg key disabled override
                foreach (ClinicErx clinicErx in listClinicErxs)
                {
                    xmlWriter.WriteStartElement("Clinic");
                    xmlWriter.WriteAttributeString("ClinicDesc", clinicErx.ClinicDesc);
                    xmlWriter.WriteAttributeString("EnabledStatus", ((int) clinicErx.EnabledStatus).ToString());
                    xmlWriter.WriteAttributeString("ClinicId", clinicErx.ClinicId);
                    xmlWriter.WriteAttributeString("ClinicKey", clinicErx.ClinicKey);
                    xmlWriter.WriteEndElement(); //End Clinic
                }

                xmlWriter.WriteEndElement(); //End ErxAccessRequest
            }
#if DEBUG
            Service1 service1 = new Service1();

#else
			OpenDentBusiness.customerUpdates.Service1 service1 = new OpenDentBusiness.customerUpdates.Service1();
			service1.Url = PrefC.GetString(PrefName.UpdateServerAddress);
#endif
            bool isCacheRefreshNeeded = false;
            try
            {
                string result = service1.GetClinicErxAccess(stringBuilder.ToString());
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(result);
                XmlNodeList xmlNodeList = xmlDocument.SelectNodes("//Clinic");
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    //Loop through clinics.
                    XmlNode xmlNode = xmlNodeList[i];
                    string clinicDesc = "";
                    string clinicId = "";
                    string clinicKey = "";
                    ErxStatus erxStatus = ErxStatus.Disabled;
                    for (int j = 0; j < xmlNode.Attributes.Count; j++)
                    {
                        //Loop through the attributes for the current provider.
                        XmlAttribute xmlAttribute = xmlNode.Attributes[j];
                        if (xmlAttribute.Name == "ClinicDesc")
                        {
                            clinicDesc = xmlAttribute.Value;
                        }
                        else if (xmlAttribute.Name == "EnabledStatus")
                        {
                            erxStatus = PIn.Enum<ErxStatus>(PIn.Int(xmlAttribute.Value));
                        }
                        else if (xmlAttribute.Name == "ClinicId")
                        {
                            clinicId = xmlAttribute.Value;
                        }
                        else if (xmlAttribute.Name == "ClinicKey")
                        {
                            clinicKey = xmlAttribute.Value;
                        }
                    }

                    ClinicErx clinicErxOld = ClinicErxs.GetByClinicIdAndKey(clinicId, clinicKey);
                    if (clinicErxOld == null)
                    {
                        continue;
                    }

                    ClinicErx clinicErx = clinicErxOld.Copy();
                    clinicErx.EnabledStatus = erxStatus;
                    clinicErx.ClinicId = clinicId;
                    clinicErx.ClinicKey = clinicKey;
                    //Dont need to set the ErxType here because it's not something that can be changed by HQ.
                    if (ClinicErxs.Update(clinicErx, clinicErxOld))
                    {
                        isCacheRefreshNeeded = true;
                    }
                }
            }
            catch (Exception ex)
            {
                //Failed to contact server and/or update clinicerx row at ODHQ. We will simply use what we already know in the local database.
            }

            return isCacheRefreshNeeded;
        }

        public static UserOdPref GetDoseSpotUserIdFromPref(long userNum, long clinicNum)
        {
            Program program = Programs.GetCur(ProgramName.eRx);
            UserOdPref userOdPref = UserOdPrefs.GetByCompositeKey(userNum, program.ProgramNum, UserOdFkeyType.Program, clinicNum);
            if (clinicNum != 0 && userOdPref.IsNew || string.IsNullOrWhiteSpace(userOdPref.ValueString))
            {
                userOdPref = UserOdPrefs.GetByCompositeKey(userNum, program.ProgramNum, UserOdFkeyType.Program, 0); //Try the default userodpref if the clinic specific one is empty.
            }

            return userOdPref;
        }

        public static OIDExternal CreateOIDForPatient(int doseSpotPatID, long patNum)
        {
            OIDExternal oIDExternal = new OIDExternal();
            oIDExternal.rootExternal = GetDoseSpotRoot() + "." + POut.Int((int) IdentifierType.Patient);
            oIDExternal.IDExternal = doseSpotPatID.ToString();
            oIDExternal.IDInternal = patNum;
            oIDExternal.IDType = IdentifierType.Patient;
            OIDExternals.Insert(oIDExternal);
            return oIDExternal;
        }

        private static void MakeClinicErxsForDoseSpot()
        {
            long programNum = Programs.GetCur(ProgramName.eRx).ProgramNum;
            List<ProgramProperty> listProgramPropertiesForClinicID = ProgramProperties.GetWhere(x => x.ProgramNum == programNum && x.PropertyDesc == Erx.PropertyDescs.ClinicID);
            List<ProgramProperty> listProgramPropertiesForClinicKey = ProgramProperties.GetWhere(x => x.ProgramNum == programNum && x.PropertyDesc == Erx.PropertyDescs.ClinicKey);
            bool isRefreshNeeded = false;
            foreach (ProgramProperty programProperty in listProgramPropertiesForClinicID)
            {
                ProgramProperty programPropertyClinicKey = listProgramPropertiesForClinicKey.FirstOrDefault(x => x.ClinicNum == programProperty.ClinicNum);
                if (programPropertyClinicKey == null || string.IsNullOrWhiteSpace(programPropertyClinicKey.PropertyValue) || string.IsNullOrWhiteSpace(programProperty.PropertyValue))
                {
                    continue;
                }

                ClinicErx clinicErx = ClinicErxs.GetByClinicNum(programProperty.ClinicNum);
                if (clinicErx == null)
                {
                    clinicErx = new ClinicErx();
                    clinicErx.ClinicNum = programProperty.ClinicNum;
                    clinicErx.ClinicId = programProperty.PropertyValue;
                    clinicErx.ClinicKey = programPropertyClinicKey.PropertyValue;
                    clinicErx.ClinicDesc = Clinics.GetDesc(programProperty.ClinicNum);
                    clinicErx.EnabledStatus = ErxStatus.PendingAccountId;
                    ClinicErxs.Insert(clinicErx);
                }
                else
                {
                    clinicErx.ClinicId = programProperty.PropertyValue;
                    clinicErx.ClinicKey = programPropertyClinicKey.PropertyValue;
                    ClinicErxs.Update(clinicErx);
                }

                isRefreshNeeded = true;
            }

            if (isRefreshNeeded)
            {
                Cache.Refresh(InvalidType.ClinicErxs);
            }
        }

        private static ClinicDto GetClinicOrPracticeInfo(long clinicNum)
        {
            var clinic = Clinics.GetClinic(clinicNum);
            bool isPractice = false;
            ValidateClinic(clinic, isPractice);
            //At this point we know the clinic is valid since we did not throw an exception.
            return clinic;
        }

        private static void InsertOrUpdate(ProgramProperty programProperty, long programNum, string propDesc, string propValue, long clinicNum)
        {
            if (programProperty == null)
            {
                programProperty = new ProgramProperty();
                programProperty.ProgramNum = programNum;
                programProperty.PropertyDesc = propDesc;
                programProperty.PropertyValue = propValue;
                programProperty.ClinicNum = clinicNum;
                ProgramProperties.Insert(programProperty);
            }
            else
            {
                programProperty.PropertyValue = propValue;
                ProgramProperties.Update(programProperty);
            }
        }

        private static void QueryStringAddParameter(StringBuilder stringBuilder, string paramName, string paramValue)
        {
            stringBuilder.Append("&" + paramName + "=");
            if (paramName != null)
            {
                stringBuilder.Append(Uri.EscapeDataString(paramValue));
            }
        }

        private static SingleSignOn GetSingleSignOn(string clinicID, string clinicKey, string userID, bool isQueryString)
        {
            clinicID = clinicID.Trim();
            clinicKey = clinicKey.Trim();
            userID = userID.Trim();
            string singleSignOnCode = CreateSsoCode(clinicKey, isQueryString);
            string singleSignOnUserIdVerify = CreateSsoUserIdVerify(clinicKey, userID, isQueryString);
            SingleSignOn dSSSingleSignOn = new SingleSignOn();
            dSSSingleSignOn.SingleSignOnClinicId = PIn.Int(clinicID);
            dSSSingleSignOn.SingleSignOnUserId = PIn.Int(userID);
            dSSSingleSignOn.SingleSignOnPhraseLength = 32;
            dSSSingleSignOn.SingleSignOnCode = singleSignOnCode;
            dSSSingleSignOn.SingleSignOnUserIdVerify = singleSignOnUserIdVerify;
            return dSSSingleSignOn;
        }

        public static void ValidateProvider(Provider provider, long clinicNum = 0)
        {
            if (provider == null)
            {
                throw new Exception("Invalid provider.");
            }

            ProviderClinic providerClinic = ProviderClinics.GetOneOrDefault(provider.ProvNum, clinicNum);
            StringBuilder stringBuilder = new StringBuilder();
            if (provider.IsErxEnabled == ErxEnabledStatus.Disabled)
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Erx is disabled for provider.  "
                                                            + "To enable, edit provider in Lists | Providers and acknowledge Electronic Prescription fees."));
            }

            if (provider.IsHidden)
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Provider is hidden"));
            }

            if (provider.IsNotPerson)
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Provider must be a person"));
            }

            string fName = provider.FName.Trim();
            if (fName == "")
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "First name missing"));
            }

            if (Regex.Replace(fName, "[^A-Za-z\\- ]*", "") != fName)
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "First name can only contain letters, dashes, or spaces"));
            }

            string lName = provider.LName.Trim();
            if (lName == "")
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Last name missing"));
            }

            string deaNum = "";
            if (providerClinic != null)
            {
                deaNum = providerClinic.DEANum;
            }

            if (deaNum.ToLower() != "none" && !Regex.IsMatch(deaNum, "^[A-Za-z]{2}[0-9]{7}$"))
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Provider DEA Number must be 2 letters followed by 7 digits.  If no DEA Number, enter NONE."));
            }

            string npi = Regex.Replace(provider.NationalProvID, "[^0-9]*", ""); //NPI with all non-numeric characters removed.
            if (npi.Length != 10)
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "NPI must be exactly 10 digits"));
            }

            if (providerClinic == null || providerClinic.StateLicense == "")
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "State license missing"));
            }

            if (providerClinic == null || !USlocales.IsValidAbbr(providerClinic.StateWhereLicensed))
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "State where licensed invalid"));
            }

            if (provider.Birthdate.Year < 1880)
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Birthdate invalid"));
            }

            if (stringBuilder.ToString().Length > 0)
            {
                string clinicText = "";
                if (true)
                {
                    clinicText = " " + Lans.g("DoseSpot", "in clinic") + " " + (clinicNum == 0 ? Lans.g("DoseSpot", "Headquarters") : Clinics.GetAbbr(clinicNum));
                }

                throw new ODException(Lans.g("DoseSpot", "Issues found for provider") + " " + provider.Abbr + clinicText + ":\r\n" + stringBuilder.ToString());
            }
        }

        private static void ValidateClinic(ClinicDto clinic, bool isPractice = false)
        {
            if (clinic == null)
            {
                throw new Exception(Lans.g("DoseSpot", "Invalid " + (isPractice ? "practice info." : "clinic.")));
            }

            StringBuilder stringBuilder = new StringBuilder();
            if (clinic.IsHidden)
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Clinic is hidden"));
            }

            if (string.IsNullOrWhiteSpace(clinic.PhoneNumber))
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Phone number is blank"));
            }
            else if (!IsPhoneNumberValid(clinic.PhoneNumber))
            {
                //If the phone number isn't valid, DoseSpot will break.
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Phone number invalid: ") + clinic.PhoneNumber);
            }

            if (clinic.AddressLine1 == "")
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Address is blank"));
            }

            if (IsAddressPOBox(clinic.AddressLine1))
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Address cannot be a PO BOX"));
            }

            if (clinic.City == "")
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "City is blank"));
            }

            if (string.IsNullOrWhiteSpace(clinic.State))
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "State abbreviation is blank"));
            }
            else if (clinic.State.Length <= 2 && (clinic.State == "" || (clinic.State != "" && !USlocales.IsValidAbbr(clinic.State))))
            {
                //Don't validate state values that are longer than 2 characters.
                stringBuilder.AppendLine(Lans.g("DoseSpot", "State abbreviation is invalid"));
            }

            if (clinic.Zip == "" && !Regex.IsMatch(clinic.Zip, @"^([0-9]{9})$|^([0-9]{5}-[0-9]{4})$|^([0-9]{5})$"))
            {
                //Blank, or #####, or #####-####, or #########
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Zip invalid."));
            }

            if (stringBuilder.ToString().Length > 0)
            {
                if (isPractice)
                {
                    throw new ODException(Lans.g("DoseSpot", "Issues found for practice information:") + "\r\n" + stringBuilder.ToString());
                }

                throw new ODException(Lans.g("DoseSpot", "Issues found for clinic") + " " + clinic.Abbr + ":\r\n" + stringBuilder.ToString());
            }
        }
        
        public static bool IsAddressPOBox(string address)
        {
            string regex = @".*( |^)P\.?O\.? .*";
            return Regex.IsMatch(address, regex, RegexOptions.IgnoreCase);
        }

        public static void ValidatePatientData(Patient patient)
        {
            string primaryPhone = GetPhoneAndType(patient, 0, out string phoneType);
            StringBuilder stringBuilder = new StringBuilder();
            if (patient.FName == "")
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Missing first name."));
            }

            if (patient.LName == "")
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Missing last name."));
            }

            if (patient.Birthdate.Year < 1880)
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Missing birthdate."));
            }

            if (patient.Birthdate > DateTime.Today)
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Invalid birthdate."));
            }

            if (patient.Address.Length == 0)
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Missing address."));
            }

            if (patient.City.Length < 2)
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Invalid city."));
            }

            if (string.IsNullOrWhiteSpace(patient.State))
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Blank state abbreviation."));
            }
            else if (patient.State.Length <= 2 && !USlocales.IsValidAbbr(patient.State))
            {
                //Don't validate state values that are longer than 2 characters.
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Invalid state abbreviation."));
            }

            if (string.IsNullOrWhiteSpace(patient.Zip))
            {
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Blank zip."));
            }
            else if (!Regex.IsMatch(patient.Zip, @"^([0-9]{9})$|^([0-9]{5}-[0-9]{4})$|^([0-9]{5})$"))
            {
                //#####, #####-####, or #########
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Invalid zip."));
            }

            if (!IsPhoneNumberValid(primaryPhone))
            {
                //If the primary phone number isn't valid, DoseSpot will break.
                stringBuilder.AppendLine(Lans.g("DoseSpot", "Invalid phone number: ") + primaryPhone);
            }

            if (stringBuilder.ToString().Length > 0)
            {
                throw new ODException(Lans.g("DoseSpot", "Issues found for current patient:") + "\r\n" + stringBuilder.ToString());
            }
        }

        public static void SetMedicationHistConsent(Patient patient, long clinicNum, Program program = null, List<ProgramProperty> listProgramProperties = null)
        {
            ValidatePatientData(patient);
            //Get Token
            string doseSpotUserID = GetUserID(Security.CurUser, clinicNum);
            GetClinicIdAndKey(clinicNum, program, listProgramProperties, out string doseSpotClinicID, out string doseSpotClinicKey);
            string token = DoseSpotREST.GetToken(doseSpotUserID, doseSpotClinicID, doseSpotClinicKey);
            //Get DoseSpotPatID
            OIDExternal oIDExternal = GetDoseSpotPatID(patient.PatNum);
            if (oIDExternal == null)
            {
                //Create a DoseSpot patient and save it for future uses with this patient.
                oIDExternal = CreateOIDForPatient(PIn.Int(DoseSpotREST.AddPatient(token, patient)), patient.PatNum);
            }
            else
            {
                DoseSpotREST.EditPatient(token, patient, oIDExternal.IDExternal);
            }

            //POST patient consent to DoseSpot
            DoseSpotREST.PostMedicationHistoryConsent(token, oIDExternal.IDExternal);
        }
        
        internal static string GetPhoneAndType(Patient patient, int ordinal, out string phoneType)
        {
            List<string> listPhoneTypes = new List<string>();
            List<string> listPhoneNumbers = new List<string>();
            if (IsPhoneNumberValid(patient.HmPhone))
            {
                listPhoneTypes.Add("Home");
                listPhoneNumbers.Add(patient.HmPhone);
            }

            if (IsPhoneNumberValid(patient.WirelessPhone))
            {
                listPhoneTypes.Add("Cell");
                listPhoneNumbers.Add(patient.WirelessPhone);
            }

            if (IsPhoneNumberValid(patient.WkPhone))
            {
                listPhoneTypes.Add("Work");
                listPhoneNumbers.Add(patient.WkPhone);
            }

            if (ordinal >= listPhoneNumbers.Count)
            {
                phoneType = "";
                return "";
            }

            phoneType = listPhoneTypes[ordinal];
            string phoneNumber = listPhoneNumbers[ordinal].Replace("(", "").Replace(")", "").Replace("-", ""); //remove all formatting as DoseSpot doesn't allow it.
            if (phoneNumber.Length == 11 && phoneNumber[0] == '1')
            {
                phoneNumber = phoneNumber.Substring(1); //Remove leading 1 from phone number since DoseSpot thinks that invalid.
            }

            return phoneNumber;
        }

        public static bool IsPhoneNumberValid(string phoneNumber)
        {
            string patternPhoneNumber = @"^1?\s*-?\s*(\d{3}|\(\s*\d{3}\s*\))\s*-?\s*\d{3}\s*-?\s*\d{4}(X\d{0,9})?";
            Regex regexPhoneNumber = new Regex(patternPhoneNumber, RegexOptions.IgnoreCase);
            if (phoneNumber != null)
            {
                phoneNumber = phoneNumber.Trim();
            }

            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length >= 35)
            {
                //Max length of 35 is what the DoseSpot example app checks for, there is no documentation supporting it.
                return false;
            }

            if (!regexPhoneNumber.IsMatch(phoneNumber))
            {
                //The regex was taken directly from the DoseSpot example app
                return false;
            }

            string phoneNumberDigits = Regex.Replace(phoneNumber, @"[^0-9]", ""); //Remove all non-digit characters.
            //Per DoseSpot on 11/15/18, any number starting with 0 or 1 will be rejected by SureScripts
            if (phoneNumberDigits.StartsWith("0") || phoneNumberDigits.StartsWith("1"))
            {
                return false;
            }

            if (!CheckAreaCode(phoneNumber))
            {
                return false;
            }

            return true;
        }

        private static bool CheckAreaCode(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            phoneNumber = Regex.Replace(phoneNumber, @"[^0-9]", ""); //Remove all non-digit characters.
            string areaCode = "";
            if (phoneNumber.Length <= 3)
            {
                return false;
            }

            if (phoneNumber.Substring(0, 1) == "1")
            {
                //Remove leading 1 for USA.
                areaCode = phoneNumber.Substring(1, 3);
            }
            else
            {
                areaCode = phoneNumber.Substring(0, 3);
            }

            //Per DoseSpot, the only invalid area code combination is 555.
            if (areaCode != "555")
            {
                return true;
            }

            return false;
        }

        private static string Get32CharPhrase()
        {
            if (_randomPhrase32 == null)
            {
                _randomPhrase32 = MiscUtils.CreateRandomAlphaNumericString(32);
            }

            return _randomPhrase32;
        }

        private static byte[] GetBytesFromUTF8(string val)
        {
            return new UTF8Encoding().GetBytes(val); //Get the value in Bytes from UTF8 String
        }

        private static byte[] GetSHA512Hash(byte[] arrayBytesToHash)
        {
            byte[] arrayHash;
            using (SHA512 sha512CryptoSP = new SHA512CryptoServiceProvider())
            {
                //Use SHA512 to hash the byte value you just received
                arrayHash = sha512CryptoSP.ComputeHash(arrayBytesToHash);
            }

            return arrayHash;
        }

        private static string RemoveExtraEqualSigns(string str)
        {
            if (str.EndsWith("=="))
            {
                //If there are two = signs at the end, then remove them
                str = str.Substring(0, str.Length - 2);
            }

            return str;
        }
    }

    public class DoseSpotREST
    {
        public static string GetToken(string userId, string clinicId, string clinicKey)
        {
            string userName = clinicId;
            string password = MakeEncryptedClinicId(clinicKey, false);
            string basicAuthContent = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(userName + ":" + password));
            string body = $"grant_type=password&Username={userId}&Password={MakeEncryptedUserId(clinicKey, userId)}";
            var resObj = Request(ApiRoute.Token, HttpMethod.Post, "Basic " + basicAuthContent, body, new
            {
                access_token = ""
            }, acceptType: "x-www-form-urlencoded");
            return resObj.access_token;
        }

        public static List<DoseSpotPrescription> GetPrescriptions(string authToken, string patientId)
        {
            //GET, don't need a body
            string body = "";
            var resObj = Request(ApiRoute.GetPrescriptions, HttpMethod.Get, "Bearer " + authToken, body, new
            {
                Items = new List<DoseSpotPrescription>(),
                Result = new {ResultCode = "", ResultDescription = ""}
            }, "application/json", patientId);
            if (resObj.Result.ResultCode.ToUpper().Contains("ERROR"))
            {
                throw new ODException(Lans.g("DoseSpot", "Error getting Prescriptions: ") + resObj.Result.ResultDescription);
            }

            return resObj.Items;
        }

        public static List<DoseSpotSelfReported> GetSelfReported(string authToken, string patientId)
        {
            //GET, don't need a body
            string body = "";
            var resObj = Request(ApiRoute.GetSelfReportedMedications, HttpMethod.Get, "Bearer " + authToken, body, new
            {
                Items = new List<DoseSpotSelfReported>(),
                Result = new {ResultCode = "", ResultDescription = ""}
            }, "application/json", patientId);
            if (resObj.Result.ResultCode.ToUpper().Contains("ERROR"))
            {
                throw new ODException(Lans.g("DoseSpot", "Error getting self reported medications: ") + resObj.Result.ResultDescription);
            }

            return resObj.Items;
        }

        public static string GetPharmacyName(string authToken, int pharmacyID)
        {
            //GET, don't need a body
            string body = "";
            var resObj = Request(ApiRoute.GetPharmacy, HttpMethod.Get, "Bearer " + authToken, body, new
            {
                Item = new
                {
                    StoreName = "",
                },
                Result = new
                {
                    ResultCode = "",
                    ResultDescription = ""
                }
            }, "application/json", pharmacyID.ToString());
            if (resObj.Result.ResultCode.ToUpper().Contains("ERROR") || resObj.Item == null)
            {
                throw new ODException(Lans.g("DoseSpot", "Error getting Pharmacy: ") + (resObj.Result.ResultCode.ToUpper().Contains("ERROR")
                    ? resObj.Result.ResultDescription
                    : Lans.g("DoseSpot", "Malformed response from DoseSpot")));
            }

            return resObj.Item.StoreName;
        }
        
        public static void GetNotificationCounts(string authToken, out int refillRequests, out int transactionErrors, out int pendingPerscriptions)
        {
            //GET, don't need a body
            string body = "";
            var resObj = Request(ApiRoute.GetNotificationCounts, HttpMethod.Get, "Bearer " + authToken, body, new
            {
                RefillRequestsCount = new int(),
                TransactionErrorsCount = new int(),
                PendingPrescriptionsCount = new int(),
                Result = new
                {
                    ResultCode = "",
                    ResultDescription = ""
                }
            });
            if (resObj.Result.ResultCode.ToUpper().Contains("ERROR"))
            {
                ODException.ErrorCodes errorCode = ODException.ErrorCodes.NotDefined;
                if (Erx.IsUserAnEmployee(Security.CurUser)
                    && (resObj.Result.ResultDescription.ToLower().Contains("not authorized") || resObj.Result.ResultDescription.ToLower().Contains("unauthorized")))
                {
                    errorCode = ODException.ErrorCodes.DoseSpotNotAuthorized;
                }

                throw new ODException(resObj.Result.ResultDescription, errorCode);
            }

            refillRequests = resObj.RefillRequestsCount;
            transactionErrors = resObj.TransactionErrorsCount;
            pendingPerscriptions = resObj.PendingPrescriptionsCount;
        }

        public static string AddPatient(string authToken, Patient patient)
        {
            Vitalsign vitalsign = Vitalsigns.GetOneWithValidHeightAndWeight(patient.PatNum);
            string primaryPhone = DoseSpot.GetPhoneAndType(patient, 0, out string phoneType);
            if (patient.Age < 18 && vitalsign == null)
            {
                throw new ODException(Lans.g("DoseSpot", "All patients under 18 must have a vital sign reading that includes height and weight. "
                                                         + "To add a vital sign to the patient, go to the 'Chart' module, and double click on the pink medical area. "
                                                         + "There is a tab in the next window labeled vitals, click it and add a vital reading that includes height and weight."));
            }

            PatientGender gender = patient.Gender;
            if (gender == PatientGender.Other)
            {
                gender = PatientGender.Unknown;
            }

            string body = JsonConvert.SerializeObject(
                new
                {
                    FirstName = patient.FName.Trim(),
                    LastName = patient.LName.Trim(),
                    DateOfBirth = patient.Birthdate,
                    Gender = gender + 1,
                    Address1 = patient.Address.Trim(),
                    City = patient.City.Trim(),
                    State = patient.State.Trim(),
                    ZipCode = patient.Zip.Trim(),
                    PrimaryPhone = primaryPhone,
                    PrimaryPhoneType = phoneType,
                    Active = "true"
                });
            if (vitalsign != null)
            {
                body = JsonConvert.SerializeObject(
                    new
                    {
                        FirstName = patient.FName.Trim(),
                        LastName = patient.LName.Trim(),
                        DateOfBirth = patient.Birthdate,
                        Gender = gender + 1,
                        Address1 = patient.Address.Trim(),
                        City = patient.City.Trim(),
                        State = patient.State.Trim(),
                        ZipCode = patient.Zip.Trim(),
                        PrimaryPhone = primaryPhone,
                        PrimaryPhoneType = phoneType,
                        Weight = vitalsign.Weight,
                        WeightMetric = 1,
                        Height = vitalsign.Height,
                        HeightMetric = 1,
                        Active = "true"
                    });
            }

            var resObj = Request(ApiRoute.AddPatient, HttpMethod.Post, "Bearer " + authToken, body, new
            {
                Id = "",
                Result = new {ResultCode = "", ResultDescription = ""}
            });
            if (resObj.Result.ResultCode.ToUpper().Contains("ERROR"))
            {
                throw new ODException(Lans.g("DoseSpot", "Error adding patient: ") + resObj.Result.ResultDescription);
            }

            if (resObj.Id == "-1")
            {
                throw new ODException(Lans.g("DoseSpot", "Error adding patient, DoseSpot returned an invalid patient ID: ") + resObj.Result.ResultDescription);
            }

            return resObj.Id;
        }

        public static void EditPatient(string authToken, Patient patient, string doseSpotPatId)
        {
            Vitalsign vitalsign = Vitalsigns.GetOneWithValidHeightAndWeight(patient.PatNum);
            string primaryPhone = DoseSpot.GetPhoneAndType(patient, 0, out string phoneType);
            if (patient.Age < 18 && vitalsign == null)
            {
                throw new ODException(Lans.g("DoseSpot", "All patients under 18 must have a vital sign reading that includes height and weight. "
                                                         + "To add a vital sign to the patient, go to the 'Chart' module, and double click on the pink medical area. "
                                                         + "There is a tab in the next window labeled vitals, click it and add a vital reading that includes height and weight."));
            }

            PatientGender gender = patient.Gender;
            if (gender == PatientGender.Other)
            {
                gender = PatientGender.Unknown;
            }

            string body = JsonConvert.SerializeObject(
                new
                {
                    FirstName = patient.FName.Trim(),
                    LastName = patient.LName.Trim(),
                    DateOfBirth = patient.Birthdate,
                    Gender = gender + 1,
                    Address1 = patient.Address.Trim(),
                    City = patient.City.Trim(),
                    State = patient.State.Trim(),
                    ZipCode = patient.Zip.Trim(),
                    PrimaryPhone = primaryPhone.Trim(),
                    PrimaryPhoneType = phoneType.Trim(),
                    Active = "true"
                });
            if (vitalsign != null)
            {
                body = JsonConvert.SerializeObject(
                    new
                    {
                        FirstName = patient.FName.Trim(),
                        LastName = patient.LName.Trim(),
                        DateOfBirth = patient.Birthdate,
                        Gender = gender + 1,
                        Address1 = patient.Address.Trim(),
                        City = patient.City.Trim(),
                        State = patient.State.Trim(),
                        ZipCode = patient.Zip.Trim(),
                        PrimaryPhone = primaryPhone.Trim(),
                        PrimaryPhoneType = phoneType.Trim(),
                        Weight = vitalsign.Weight,
                        WeightMetric = 1,
                        Height = vitalsign.Height,
                        HeightMetric = 1,
                        Active = "true"
                    });
            }

            var resObj = Request(ApiRoute.EditPatient, HttpMethod.Post, "Bearer " + authToken, body, new
            {
                Id = "",
                Result = new {ResultCode = "", ResultDescription = ""}
            }, "application/json", doseSpotPatId);
            if (resObj.Result.ResultCode.ToUpper().Contains("ERROR"))
            {
                throw new ODException(Lans.g("DoseSpot", "Error editing patient: ") + resObj.Result.ResultDescription);
            }
        }

        public static void PostMedicationHistoryConsent(string authToken, string patientId)
        {
            var resObj = Request(ApiRoute.LogMedicationHistoryConsent, HttpMethod.Post, "Bearer " + authToken, "", new
            {
                Item = "",
                Result = new {ResultCode = "", ResultDescription = ""}
            }, "application/json", patientId);
            if (resObj.Result.ResultCode.ToUpper().Contains("ERROR"))
            {
                throw new ODException(Lans.g("DoseSpot", "Error posting medication history consent: ") + resObj.Result.ResultDescription);
            }
        }

        public static void PostClinic(string authToken, ClinicDto clinic, out string clinicID, out string clinicKey)
        {
            clinicID = "";
            clinicKey = "";
            //From the DoseSpot REST API guide:
            //	Note: After adding a new clinic, you MUST email the the Clinic Name and Clinic ID to DoseSpot:
            //	STAGING: staging-clinicadd-notifications.907f8409@sniffle.dosespot.podio.com OR
            //	PRODUCTION: production-clinicadd-notifications.ef8bed85@sniffle.dosespot.podio.com
            //Phone type "7" means "Primary" which seemed like a good default because I don't see why a clinic needs a phone type.
            string body = JsonConvert.SerializeObject(
                new
                {
                    ClinicName = clinic.Abbr.Trim(),
                    Address1 = clinic.AddressLine1.Trim(),
                    City = clinic.City.Trim(),
                    State = clinic.State.Trim(),
                    ZipCode = clinic.Zip.Trim(),
                    PrimaryPhone = clinic.PhoneNumber.Trim(),
                    PrimaryPhoneType = "7"
                });
            var resObj = Request(ApiRoute.PostClinic, HttpMethod.Post, "Bearer " + authToken, body, new
            {
                ClinicId = "",
                ClinicKey = "",
                Result = new {ResultCode = "", ResultDescription = ""}
            });
            if (resObj.Result.ResultCode.ToUpper().Contains("ERROR"))
            {
                throw new ODException(Lans.g("DoseSpot", "Error posting clinic: ") + resObj.Result.ResultDescription);
            }

            clinicID = resObj.ClinicId;
            clinicKey = resObj.ClinicKey;
        }

        public static int PostSelfReportedMedications(string authToken, string patientId, DoseSpotSelfReported doseSpotSelfReported)
        {
            if (doseSpotSelfReported == null || doseSpotSelfReported.SelfReportedMedicationId == null)
            {
                throw new ODException(Lans.g("DoseSpot", "Error creating self reported medication ID: Could not convert medication ID to DoseSpot ID."));
            }

            bool doIncludeInactiveDate = doseSpotSelfReported.DateInactive.HasValue && doseSpotSelfReported.DateInactive.Value.Year > 1880;
            string body = JsonConvert.SerializeObject(
                new
                {
                    DisplayName = doseSpotSelfReported.DisplayName.Trim(),
                    Status = (int) doseSpotSelfReported.MedicationStatus,
                    Comment = doseSpotSelfReported.Comment.Trim()
                });
            if (doIncludeInactiveDate)
            {
                body = JsonConvert.SerializeObject(
                    new
                    {
                        DisplayName = doseSpotSelfReported.DisplayName.Trim(),
                        Status = (int) doseSpotSelfReported.MedicationStatus,
                        InactiveDate = doseSpotSelfReported.DateInactive,
                        Comment = doseSpotSelfReported.Comment.Trim()
                    });
            }

            var resObj = Request(ApiRoute.PostSelfReportedMedications, HttpMethod.Post, "Bearer " + authToken, body, new
            {
                Id = -1,
                Result = new {ResultCode = "", ResultDescription = ""}
            }, "application/json", patientId, doseSpotSelfReported.SelfReportedMedicationId.ToString());
            if (resObj.Result.ResultCode.ToUpper().Contains("ERROR"))
            {
                throw new ODException(Lans.g("DoseSpot", "Error posting medication: ") + resObj.Result.ResultDescription);
            }

            return resObj.Id;
        }

        public static void PostInitiateDrugDbMigration(string authToken, List<string> listClinicIDs, string clientId, string medispanClientId)
        {
            //DoseSpot docs specifically request an Array of clinicIDs.
            int[] arrayIds = new int[listClinicIDs.Count];
            try
            {
                arrayIds = listClinicIDs.ConvertAll(int.Parse).ToArray();
            }
            catch
            {
                throw new ODException(Lans.g("DoseSpot", "Error migrating clinics to V2, one or more clinic IDs are not valid numbers. Please contact support."));
            }

            string body = JsonConvert.SerializeObject(
                new
                {
                    MedispanClientId = medispanClientId,
                    ClinicIDs = arrayIds
                });
            var resObj = Request(ApiRoute.PostInitiateDrugDbMigration, HttpMethod.Post, "Bearer " + authToken, body, new
                {
                    Id = "",
                    Result = new {ResultCode = "", ResultDescription = ""}
                }, "application/json", clientId
            );
            if (resObj.Result.ResultCode.ToUpper().Contains("ERROR"))
            {
                throw new ODException(Lans.g("DoseSpot", "Error requesting database migration: ") + resObj.Result.ResultDescription);
            }
        }
        
        public static void PutSelfReportedMedications(string authToken, string patientId, DoseSpotSelfReported doseSpotSelfReported)
        {
            if (doseSpotSelfReported == null || doseSpotSelfReported.SelfReportedMedicationId == null)
            {
                throw new ODException(Lans.g("DoseSpot", "Error creating self reported medication ID: Could not convert medication ID to DoseSpot ID."));
            }

            bool doIncludeInactiveDate = doseSpotSelfReported.DateInactive.HasValue && doseSpotSelfReported.DateInactive.Value.Year > 1880;
            string body = JsonConvert.SerializeObject(
                new
                {
                    DisplayName = doseSpotSelfReported.DisplayName.Trim(),
                    Status = (int) doseSpotSelfReported.MedicationStatus,
                    Comment = doseSpotSelfReported.Comment.Trim()
                });
            if (doIncludeInactiveDate)
            {
                body = JsonConvert.SerializeObject(
                    new
                    {
                        DisplayName = doseSpotSelfReported.DisplayName.Trim(),
                        Status = (int) doseSpotSelfReported.MedicationStatus,
                        InactiveDate = doseSpotSelfReported.DateInactive,
                        Comment = doseSpotSelfReported.Comment.Trim()
                    });
            }

            var resObj = Request(ApiRoute.PutSelfReportedMedications, HttpMethod.Put, "Bearer " + authToken, body, new
            {
                Id = -1,
                Result = new {ResultCode = "", ResultDescription = ""}
            }, "application/json", patientId, doseSpotSelfReported.SelfReportedMedicationId.ToString());
            if (resObj.Result.ResultCode.ToUpper().Contains("ERROR"))
            {
                throw new ODException(Lans.g("DoseSpot", "Error putting medication: ") + resObj.Result.ResultDescription);
            }
        }
        
        private static T Request<T>(ApiRoute apiRoute, HttpMethod httpMethod, string authHeader, string body, T responseType, string acceptType = "application/json", params string[] listRouteIDs)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Headers[HttpRequestHeader.Accept] = acceptType;
                webClient.Headers[HttpRequestHeader.ContentType] = acceptType;
                webClient.Headers[HttpRequestHeader.Authorization] = authHeader;
                webClient.Encoding = UnicodeEncoding.UTF8;
                //Post with Authorization headers and a body comprised of a JSON serialized anonymous type.
                try
                {
                    string url = "";
                    string response = "";
                    //Only GET and POST are supported currently.
                    if (httpMethod == HttpMethod.Get)
                    {
                        url = GetApiUrl(apiRoute, listRouteIDs);
                        response = webClient.DownloadString(url);
                    }
                    else if (httpMethod == HttpMethod.Post)
                    {
                        url = GetApiUrl(apiRoute, listRouteIDs);
                        response = webClient.UploadString(url, HttpMethod.Post.Method, body);
                    }
                    else if (httpMethod == HttpMethod.Put)
                    {
                        url = GetApiUrl(apiRoute, listRouteIDs);
                        response = webClient.UploadString(url, HttpMethod.Put.Method, body);
                    }
                    else
                    {
                        throw new Exception("Unsupported HttpMethod type: " + httpMethod.Method);
                    }

                    if (ODBuild.IsDebug())
                    {
                        if ((typeof(T) == typeof(string)))
                        {
                            //If user wants the entire json response as a string
                            return (T) Convert.ChangeType(response, typeof(T));
                        }
                    }

                    return JsonConvert.DeserializeAnonymousType(response, responseType);
                }
                catch (WebException wex)
                {
                    if (!(wex.Response is HttpWebResponse))
                    {
                        throw new ODException(Lans.g("DoseSpot", "Could not connect to the DoseSpot server:") + "\r\n" + wex.Message, wex);
                    }

                    string responseErr = "";
                    using (var streamReader = new StreamReader(((HttpWebResponse) wex.Response).GetResponseStream()))
                    {
                        responseErr = streamReader.ReadToEnd();
                    }

                    if (string.IsNullOrWhiteSpace(responseErr))
                    {
                        //The response didn't contain a body.  Through my limited testing, it only happens for 401 (Unauthorized) requests.
                        if (wex.Response.GetType() == typeof(HttpWebResponse))
                        {
                            HttpStatusCode statusCode = ((HttpWebResponse) wex.Response).StatusCode;
                            if (statusCode == HttpStatusCode.Unauthorized)
                            {
                                throw new ODException(Lans.g("DoseSpot", "Invalid DoseSpot credentials."));
                            }
                        }
                    }

                    string errorMsg = wex.Message + (string.IsNullOrWhiteSpace(responseErr) ? "" : "\r\nRaw response:\r\n" + responseErr);
                    throw new Exception(errorMsg, wex); //If it got this far and haven't rethrown, simply throw the entire exception.
                }
                catch (Exception ex)
                {
                    //WebClient returned an http status code >= 300
                    //For now, rethrow error and let whoever is expecting errors to handle them.
                    //We may enhance this to care about codes at some point.
                    throw;
                }
            }
        }

        private static string GetApiUrl(ApiRoute apiRoute, params string[] arrayRouteIDs)
        {
            string apiUrl = Introspection.GetOverride(Introspection.IntrospectionEntity.DoseSpotURL, "https://my.dosespot.com/webapi");
            if (ODBuild.IsDebug())
            {
                apiUrl = "https://my.staging.dosespot.com/webapi";
            }

            switch (apiRoute)
            {
                case ApiRoute.Root:
                    //Do nothing.  This is to allow someone to quickly grab the URL without having to make a copy+paste reference.
                    break;
                case ApiRoute.Token:
                    apiUrl += "/token";
                    break;
                case ApiRoute.AddPatient:
                    apiUrl += "/api/patients";
                    break;
                case ApiRoute.EditPatient:
                    apiUrl += $"/api/patients/{arrayRouteIDs[0]}";
                    break;
                case ApiRoute.GetNotificationCounts:
                    apiUrl += $"/api/notifications/counts";
                    break;
                case ApiRoute.GetPharmacy:
                    //routeId[0]=pharmacyId
                    apiUrl += $"/api/pharmacies/{arrayRouteIDs[0]}";
                    break;
                case ApiRoute.GetPrescriptions:
                    //routeId[0]=PatientId
                    apiUrl += $"/api/patients/{arrayRouteIDs[0]}/prescriptions";
                    break;
                case ApiRoute.GetSelfReportedMedications:
                    apiUrl += $"/api/patients/{arrayRouteIDs[0]}/selfReportedMedications";
                    break;
                case ApiRoute.LogMedicationHistoryConsent:
                    //routeId[0]=PatientId
                    apiUrl += $"/api/patients/{arrayRouteIDs[0]}/logMedicationHistoryConsent";
                    break;
                case ApiRoute.PostClinic:
                    apiUrl += $"/api/clinics";
                    break;
                case ApiRoute.PostClinicGroup:
                    apiUrl += $"/api/clinics/clinicGroup";
                    break;
                case ApiRoute.PutSelfReportedMedications:
                    //routeId[0]=PatientId, routeId[1]=selfReportedMedicationId
                    apiUrl += $"/api/patients/{arrayRouteIDs[0]}/selfReportedMedications/freetext/{arrayRouteIDs[1]}";
                    break;
                case ApiRoute.PostSelfReportedMedications:
                    //routeId[0]=PatientId
                    apiUrl += $"/api/patients/{arrayRouteIDs[0]}/selfReportedMedications/freetext";
                    break;
                case ApiRoute.PostInitiateDrugDbMigration:
                    //routeId[0]=ClientId
                    apiUrl += $"/api/client/{arrayRouteIDs[0]}/initiateDrugDbMigration";
                    break;
                default:
                    break;
            }

            return apiUrl;
        }

        private enum ApiRoute
        {
            Root,
            Token,
            AddPatient,
            EditPatient,
            GetPharmacy,
            GetPrescriptions,
            GetNotificationCounts,
            GetSelfReportedMedications,
            LogMedicationHistoryConsent,
            PostClinic,
            PutSelfReportedMedications,
            PostClinicGroup,
            PostSelfReportedMedications,
            PostInitiateDrugDbMigration,
        }

        public enum MedicationStatus
        {
            Unknown, //Depricated by DoseSpot
            Active,
            Inactive, //Depricated by DoseSpot
            Discontinued,
            Deleted,
            Completed,
            CancelRequested,
            CancelPending,
            Cancelled,
            CancelDenied,
            ChangeD
        }

        public enum PrescriptionStatus
        {
            Entered = 1,
            Printed,
            Sending,
            eRxSent,
            FaxSent,
            Error,
            Deleted,
            Requested,
            Edited,
            EpcsError,
            EpcsSigned,
            ReadyToSign,
            PharmacyVerified,

            NotAPresciption
        }

        private static string MakeEncryptedClinicId(string clinicKey, bool isQueryStr = true)
        {
            return DoseSpot.CreateSsoCode(clinicKey, isQueryStr);
        }

        private static string MakeEncryptedUserId(string clinicKey, string userID, bool isQueryStr = true)
        {
            return DoseSpot.CreateSsoUserIdVerify(clinicKey, userID, isQueryStr);
        }

        public static DoseSpotSelfReported MedicationPatToDoseSpotSelfReport(MedicationPat medicationPat)
        {
            DoseSpotSelfReported doseSpotSelfReported = new DoseSpotSelfReported();
            doseSpotSelfReported.SelfReportedMedicationId = 0;
            //If we have the ErxGuid then it has already been sent to DS.
            if (!medicationPat.ErxGuid.IsNullOrEmpty())
            {
                //Remove any possible prefixes from the ErxGuid. It will either be a DS prefix, an OD prefix, or have no prefix at all if it came from an Rx.
                int ID = 0;
                string guid = medicationPat.ErxGuid;
                //If it has the unsent prefix we want to intentionally set the ID to 0 so we post a new self reported and get an ID back from DS.
                if (!guid.StartsWith(Erx.UnsentPrefix))
                {
                    guid = Regex.Replace(guid, Erx.DoseSpotPatReportedPrefix, ""); //Remove DS prefix.
                    guid = Regex.Replace(guid, Erx.OpenDentalErxPrefix, ""); //Remove OD prefix.
                    int.TryParse(guid, out ID);
                }

                doseSpotSelfReported.SelfReportedMedicationId = ID;
            }

            //Set the DisplayName.
            if (string.IsNullOrWhiteSpace(medicationPat.MedDescript) && medicationPat.MedicationNum != 0)
            {
                Medication medication = Medications.GetMedication(medicationPat.MedicationNum);
                doseSpotSelfReported.DisplayName = medication.MedName;
            }
            else
            {
                doseSpotSelfReported.DisplayName = medicationPat.MedDescript;
            }

            //Strip any newlines before sending. Newlines cause parsing errors for the DoseSpot API. The changes to this note will be synced back into OD when the eRx window is closed.
            doseSpotSelfReported.Comment = Regex.Replace(medicationPat.PatNote, @"\n|\r", " ");
            //500 characters is the max size for the comment field. Going over 500 will return a 400 error from the DoseSpot API.
            if (doseSpotSelfReported.Comment.Length > 500)
            {
                SecurityLogs.MakeLogEntry(EnumPermType.LogDoseSpotMedicationNoteEdit, medicationPat.PatNum, "Medication patient note automatically reduced to 500 characters for sending to DoseSpot. Original note: " + "\n"
                                                                                                                                                                                                                       + medicationPat.PatNote);
                doseSpotSelfReported.Comment = doseSpotSelfReported.Comment.Substring(0, 500);
            }

            //Set the MedicationStatus.
            doseSpotSelfReported.MedicationStatus = MedicationStatus.Discontinued;
            if (medicationPat.DateStop.Year < 1880 || medicationPat.DateStop >= DateTime.Today)
            {
                doseSpotSelfReported.MedicationStatus = MedicationStatus.Active;
            }
            else if (medicationPat.DateStop < DateTime.Today)
            {
                doseSpotSelfReported.MedicationStatus = MedicationStatus.Completed;
                doseSpotSelfReported.DateInactive = medicationPat.DateStop;
                //A comment is required when a medication has been discontinued (figured out through testing, not in docs)
                if (string.IsNullOrWhiteSpace(doseSpotSelfReported.Comment))
                {
                    //We were infinitely adding this note which was causing the comment to be greater than 500 characters and caused a 400 from DS.
                    doseSpotSelfReported.Comment = "Discontinued in Open Dental";
                }
            }

            return doseSpotSelfReported;
        }

        public class PropertyDescs
        {
            public static string DoseSpotApiVersion = "DoseSpotApiVersion";
            public static string DoseSpotApiMigrationRequested = "DoseSpotApiMigrationRequested";
        }
    }

    public class DoseSpotPrescription
    {
        //DO NOT RENAME ANYTHING IN THIS CLASS. IT IS USED FOR SERIALIZING DOSESPOT API RESPONSES AND NEEDS TO MATCH THE DOSESPOT API DOCS.
        public int? PrescriptionId;
        public DateTime? WrittenDate;
        public string Directions;
        public string Quantity;
        public int? DispenseUnitId;
        public string DispenseUnitDescription;
        public string Refills;
        public int? DaysSupply;
        public int? PharmacyId;
        public string PharmacyNotes;
        public bool? NoSubstitutions;
        public DateTime? EffectiveDate;
        public DateTime? LastFillDate;
        public int? PrescriberId;
        public int? PrescriberAgentId;
        public string RxReferenceNumber;
        public DoseSpotREST.PrescriptionStatus Status;
        public bool? Formulary;
        public int? EligibilityId;
        public string Type;
        public string NonDoseSpotPrescriptionId;
        public int? PatientMedicationId;
        public DoseSpotREST.MedicationStatus MedicationStatus;
        public string Comment;
        public DateTime? DateInactive;
        public string Encounter;
        public string DoseForm;
        public string Route;
        public string Strength;
        public string GenericProductName;
        public int? LexiGenProductId;
        public int? LexiDrugSynId;
        public int? LexiSynonymTypeId;
        public string LexiGenDrugId;

        ///<summary>According to US Dept. HHS RxCUI is a numeric value with max length of 8. DoseSpot sends this value as a string.</summary>
        public string RxCUI;

        public bool? OTC;
        public string NDC;
        public string Schedule;
        public string DisplayName;
        public string MonographPath;
        public string DrugClassification;
    }

    public class DoseSpotSelfReported
    {
        //DO NOT RENAME ANYTHING IN THIS CLASS. IT IS USED FOR SERIALIZING DOSESPOT API RESPONSES AND NEEDS TO MATCH THE DOSESPOT API DOCS.
        public int? SelfReportedMedicationId;
        public DateTime? DateReported;
        public DateTime? WrittenDate; //Changed from V2 to DatePrescribed
        public string Directions; //Changed from V2 to DosageText
        public string Quantity; //Changed from V2 to Dispense
        public int? DispenseUnitId; //Changed from V2 to DispenseUnitTypeId
        public string DispenseUnitDescription; //Changed from V2 to DispenseUnitType
        public string Refills;
        public int? DaysSupply;
        public int? PatientMedicationId;
        public DoseSpotREST.MedicationStatus MedicationStatus; //Changed from V2 to Status
        public string Comment;
        public DateTime? DateInactive; //Changed from V2 to DiscontinuedDate
        public string Encounter;
        public string DoseForm;
        public string Route;
        public string Strength;
        public string GenericProductName;
        public int? LexiGenProductId;
        public int? LexiDrugSynId;
        public int? LexiSynonymTypeId;
        public string LexiGenDrugId;
        public string RxCUI;
        public bool? OTC;
        public string NDC;
        public string Schedule;
        public string DisplayName;
        public string MonographPath;
        public string DrugClassification;

        [JsonProperty("DatePrescribed")]
        private DateTime? DatePrescribed
        {
            set { WrittenDate = value; }
        }

        [JsonProperty("DispenseUnitType")]
        private string? DispenseUnitType
        {
            set { DispenseUnitDescription = value; }
        }

        [JsonProperty("DosageText")]
        private string? DosageText
        {
            set { Directions = value; }
        }

        [JsonProperty("Dispense")]
        private string? Dispense
        {
            set { Quantity = value; }
        }

        [JsonProperty("DispenseUnitTypeId")]
        private int? DispenseUnitTypeId
        {
            set { DispenseUnitId = value; }
        }

        [JsonProperty("Status")]
        private DoseSpotREST.MedicationStatus Status
        {
            set { MedicationStatus = value; }
        }

        [JsonProperty("DiscontinuedDate")]
        private DateTime? DiscontinuedDate
        {
            set { DateInactive = value; }
        }

        private int DispensableDrugId;
        private bool RoutedDoseFormDrugID;
        private string StateSchedules;
    }

    public class DoseSpotMedicationWrapper
    {
        readonly DoseSpotSelfReported _doseSpotSelfReported;
        readonly DoseSpotPrescription _doseSpotPrescription;

        public bool IsSelfReported
        {
            get { return _doseSpotSelfReported != null; }
        }

        public DateTime? DateInactive
        {
            get
            {
                if (IsSelfReported)
                {
                    return _doseSpotSelfReported.DateInactive;
                }

                return _doseSpotPrescription.DateInactive;
            }
        }

        public string DisplayName
        {
            get
            {
                if (IsSelfReported)
                {
                    return _doseSpotSelfReported.DisplayName;
                }

                return _doseSpotPrescription.DisplayName;
            }
        }

        public DateTime? DateReported
        {
            get
            {
                if (IsSelfReported)
                {
                    return _doseSpotSelfReported.DateReported;
                }

                return _doseSpotPrescription.EffectiveDate;
            }
        }

        public string GenericProductName
        {
            get
            {
                if (IsSelfReported)
                {
                    return _doseSpotSelfReported.GenericProductName;
                }

                return _doseSpotPrescription.GenericProductName;
            }
        }

        public DateTime? DateLastFilled
        {
            get
            {
                if (IsSelfReported)
                {
                    return null;
                }

                return _doseSpotPrescription.LastFillDate;
            }
        }

        public long? MedicationId
        {
            get
            {
                if (IsSelfReported)
                {
                    return _doseSpotSelfReported.SelfReportedMedicationId;
                }

                return _doseSpotPrescription.PrescriptionId;
            }
        }

        public DoseSpotREST.MedicationStatus MedicationStatus
        {
            get
            {
                if (IsSelfReported)
                {
                    return _doseSpotSelfReported.MedicationStatus;
                }

                return _doseSpotPrescription.MedicationStatus;
            }
        }

        public int? PharmacyId
        {
            get
            {
                if (IsSelfReported)
                {
                    return null;
                }

                return _doseSpotPrescription.PharmacyId;
            }
        }

        public string Directions
        {
            get
            {
                if (IsSelfReported)
                {
                    return _doseSpotSelfReported.Comment;
                }

                return _doseSpotPrescription.Directions;
            }
        }

        public string RxNotes
        {
            get
            {
                if (IsSelfReported)
                {
                    return null;
                }

                return _doseSpotPrescription.PharmacyNotes;
            }
        }

        public int? PrescriberId
        {
            get
            {
                if (IsSelfReported)
                {
                    return null;
                }

                return _doseSpotPrescription.PrescriberId;
            }
        }

        public DoseSpotREST.PrescriptionStatus PrescriptionStatus
        {
            get
            {
                if (IsSelfReported)
                {
                    //This makes it fall through to the base case and do nothing.
                    return DoseSpotREST.PrescriptionStatus.NotAPresciption;
                }

                return _doseSpotPrescription.Status;
            }
        }

        public string Quantity
        {
            get
            {
                if (IsSelfReported)
                {
                    return _doseSpotSelfReported.Quantity;
                }

                return _doseSpotPrescription.Quantity;
            }
        }

        public string Refills
        {
            get
            {
                if (IsSelfReported)
                {
                    return _doseSpotSelfReported.Refills;
                }

                return _doseSpotPrescription.Refills;
            }
        }

        public long RxCUI
        {
            get
            {
                if (IsSelfReported)
                {
                    if (!_doseSpotSelfReported.RxCUI.IsNullOrEmpty())
                    {
                        return PIn.Long(_doseSpotSelfReported.RxCUI, false); //Cast from string to long is intentional.
                    }
                }
                else
                {
                    if (!_doseSpotPrescription.RxCUI.IsNullOrEmpty())
                    {
                        return PIn.Long(_doseSpotPrescription.RxCUI, false); //Cast from string to long is intentional.
                    }
                }

                //Something went wrong.
                return 0;
            }
        }

        public string Schedule
        {
            get
            {
                if (IsSelfReported)
                {
                    return _doseSpotSelfReported.Schedule;
                }

                return _doseSpotPrescription.Schedule;
            }
        }

        public DateTime? DateWritten
        {
            get
            {
                if (IsSelfReported)
                {
                    return _doseSpotSelfReported.WrittenDate;
                }

                return _doseSpotPrescription.WrittenDate;
            }
        }

        public DoseSpotMedicationWrapper(DoseSpotPrescription doseSpotPrescription, DoseSpotSelfReported doseSpotSelfReported)
        {
            _doseSpotPrescription = doseSpotPrescription;
            _doseSpotSelfReported = doseSpotSelfReported;
        }
    }
}