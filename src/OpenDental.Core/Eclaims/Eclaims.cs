using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness.Eclaims
{
    public class Eclaims
    {
        public Eclaims()
        {
        }
        
        public static string GetNoProceduresOnClaimMessage()
        {
            return "No procedures attached please recreate claim";
        }

        ///<summary>Supply a list of ClaimSendQueueItems.  Called from FormClaimSend.  Can only send to one clearinghouse at a time.
        ///The queueItems must contain at least one item.  Each item in queueItems must have the same ClinicNum.  Cannot include Canadian.</summary>
        public static void SendBatch(Clearinghouse clearinghouseClin, List<ClaimSendQueueItem> queueItems, EnumClaimMedType medType,
            IFormClaimFormItemEdit formClaimFormItemEdit, Renaissance.FillRenaissanceDelegate fillRenaissance, ITerminalConnector terminalConnector)
        {
            string messageText = "";
            if (clearinghouseClin.Eformat == ElectronicClaimFormat.Canadian)
            {
                MessageBox.Show(Lans.g("Eclaims", "Cannot send Canadian claims as part of Eclaims.SendBatch."));
                return;
            }
            
            var batchNumber = Clearinghouses.GetNextBatchNumber(clearinghouseClin);

            messageText = clearinghouseClin.Eformat switch
            {
                ElectronicClaimFormat.x837D_4010 or ElectronicClaimFormat.x837D_5010_dental or ElectronicClaimFormat.x837_5010_med_inst => x837Controller.SendBatch(clearinghouseClin, queueItems, batchNumber, medType, false),
                ElectronicClaimFormat.Renaissance => Renaissance.SendBatch(clearinghouseClin, queueItems, batchNumber, formClaimFormItemEdit, fillRenaissance),
                ElectronicClaimFormat.Dutch => Dutch.SendBatch(clearinghouseClin, queueItems, batchNumber),
                ElectronicClaimFormat.Ramq => Ramq.SendBatch(clearinghouseClin, queueItems, batchNumber),
                _ => ""
            };

            if (messageText == "")
            {
                //if failed to create claim file properly,
                return; //don't launch program or change claim status
            }

            if (clearinghouseClin.CommBridge == EclaimsCommBridge.None)
            {
                AttemptLaunch(clearinghouseClin, batchNumber);
            }
            else if (clearinghouseClin.CommBridge == EclaimsCommBridge.WebMD)
            {
                if (!WebMD.Launch(clearinghouseClin, batchNumber))
                {
                    MessageBox.Show(Lans.g("Eclaims", "Error sending.") + "\r\n" + WebMD.ErrorMessage);
                    return;
                }
            }
            else if (clearinghouseClin.CommBridge == EclaimsCommBridge.BCBSGA)
            {
                if (!BCBSGA.Launch(clearinghouseClin, batchNumber, terminalConnector))
                {
                    MessageBox.Show(Lans.g("Eclaims", "Error sending.") + "\r\n" + BCBSGA.ErrorMessage);
                    return;
                }
            }
            else if (clearinghouseClin.CommBridge == EclaimsCommBridge.Renaissance)
            {
                AttemptLaunch(clearinghouseClin, batchNumber);
            }
            else if (clearinghouseClin.CommBridge == EclaimsCommBridge.ClaimConnect)
            {
                if (ClaimConnect.Launch(clearinghouseClin, batchNumber))
                {
                    MessageBox.Show("Upload successful.");
                }
                else
                {
                    MessageBox.Show(Lans.g("Eclaims", "Error sending.") + "\r\n" + ClaimConnect.ErrorMessage);
                    return;
                }
            }
            else if (clearinghouseClin.CommBridge == EclaimsCommBridge.RECS)
            {
                if (!RECS.Launch(clearinghouseClin))
                {
                    MessageBox.Show(Lans.g("Eclaims", "Claim file created, but could not launch RECS client.") + "\r\n" + RECS.ErrorMessage);
                }
            }
            else if (clearinghouseClin.CommBridge == EclaimsCommBridge.Inmediata)
            {
                if (!Inmediata.Launch(clearinghouseClin))
                {
                    MessageBox.Show(Lans.g("Eclaims", "Claim file created, but could not launch Inmediata client.") + "\r\n" + Inmediata.ErrorMessage);
                }
            }
            else if (clearinghouseClin.CommBridge == EclaimsCommBridge.AOS)
            {
                if (!AOS.Launch(clearinghouseClin))
                {
                    MessageBox.Show(Lans.g("Eclaims", "Claim file created, but could not launch AOS Communicator.") + "\r\n" + AOS.ErrorMessage);
                }
            }
            else if (clearinghouseClin.CommBridge == EclaimsCommBridge.PostnTrack)
            {
                AttemptLaunch(clearinghouseClin, batchNumber);
            }
            else if (clearinghouseClin.CommBridge == EclaimsCommBridge.MercuryDE)
            {
                if (!MercuryDE.Launch(clearinghouseClin, batchNumber))
                {
                    MessageBox.Show(Lans.g("Eclaims", "Error sending.") + "\r\n" + MercuryDE.ErrorMessage);
                    return;
                }
            }
            else if (clearinghouseClin.CommBridge == EclaimsCommBridge.ClaimX)
            {
                if (!ClaimX.Launch(clearinghouseClin))
                {
                    MessageBox.Show(Lans.g("Eclaims", "Claim file created, but encountered an error while launching ClaimX Client.") + ":\r\n" + ClaimX.ErrorMessage);
                }
            }
            else if (clearinghouseClin.CommBridge == EclaimsCommBridge.EmdeonMedical)
            {
                if (!EmdeonMedical.Launch(clearinghouseClin, batchNumber, medType))
                {
                    MessageBox.Show(Lans.g("Eclaims", "Error sending.") + "\r\n" + EmdeonMedical.ErrorMessage);
                    return;
                }
            }
            else if (clearinghouseClin.CommBridge == EclaimsCommBridge.DentiCal)
            {
                if (!DentiCal.Launch(clearinghouseClin, batchNumber))
                {
                    MessageBox.Show(Lans.g("Eclaims", "Error sending.") + DentiCal.ErrorMessage);
                    return;
                }
            }
            else if (clearinghouseClin.CommBridge == EclaimsCommBridge.NHS)
            {
                if (!NHS.Launch())
                {
                    MessageBox.Show(Lans.g("Eclaims", "Error sending.") + "\r\n" + NHS.ErrorMessage);
                    return;
                }
            }
            else if (clearinghouseClin.CommBridge == EclaimsCommBridge.EDS)
            {
                if (!EDS.Launch(clearinghouseClin, messageText))
                {
                    MessageBox.Show(Lans.g("Eclaims", "Error sending.") + "\r\n" + EDS.ErrorMessage);
                    return;
                }
            }
            else if (clearinghouseClin.CommBridge == EclaimsCommBridge.EdsMedical)
            {
                if (!EdsMedical.Launch(clearinghouseClin, messageText))
                {
                    MessageBox.Show(Lans.g("Eclaims", "Error sending.") + "\r\n" + EdsMedical.ErrorMessage);
                    return;
                }
            }
            else if (clearinghouseClin.CommBridge == EclaimsCommBridge.Ramq)
            {
                if (!Ramq.Launch(clearinghouseClin, batchNumber))
                {
                    MessageBox.Show(Lans.g("Eclaims", "Error sending.") + Ramq.ErrorMessage);
                    return;
                }
            }
            else if (clearinghouseClin.CommBridge == EclaimsCommBridge.Lantek)
            {
                if (!Lantek.Launch(clearinghouseClin))
                {
                    MessageBox.Show(Lans.g("Eclaims", "Error sending.") + Lantek.ErrorMessage);
                    return;
                }
            }
            else if (clearinghouseClin.CommBridge == EclaimsCommBridge.VyneDental)
            {
                if (!VyneDental.Launch(clearinghouseClin, batchNumber))
                {
                    MessageBox.Show(Lans.g("Eclaims", "Error sending.") + "\r\n" + VyneDental.ErrorMessage);
                    return;
                }
            }

            StringBuilder errorMessage = new StringBuilder();
            //----------------------------------------------------------------------------------------
            //finally, mark the claims sent. (only if not Canadian)
            EtransType etype = EtransType.ClaimSent;
            if (clearinghouseClin.Eformat == ElectronicClaimFormat.Renaissance)
            {
                etype = EtransType.Claim_Ren;
            }

            //Canadians cannot send in batches (see above).  RAMQ is performing a similar algorithm but the steps are in a different order in Ramq.cs.
            if (clearinghouseClin.Eformat != ElectronicClaimFormat.Canadian && clearinghouseClin.Eformat != ElectronicClaimFormat.Ramq)
            {
                //Create the etransmessagetext that all claims in the batch will point to.
                EtransMessageText etransMsgText = new EtransMessageText();
                etransMsgText.MessageText = messageText;
                EtransMessageTexts.Insert(etransMsgText);
                for (int j = 0; j < queueItems.Count; j++)
                {
                    Etrans etrans = Etranss.SetClaimSentOrPrinted(queueItems[j].ClaimNum, queueItems[j].ClaimStatus, queueItems[j].PatNum,
                        clearinghouseClin.HqClearinghouseNum, etype, batchNumber, Security.CurUser.UserNum);
                    //Attempted fix for problems with Eclaims SendBatch attempts throwing null reference UEs. Job #41284
                    //If SetClaimSentOrPrinted() returns null, then we try again.
                    if (etrans == null)
                    {
                        Thread.Sleep(100);
                        etrans = Etranss.SetClaimSentOrPrinted(queueItems[j].ClaimNum, queueItems[j].ClaimStatus, queueItems[j].PatNum,
                            clearinghouseClin.HqClearinghouseNum, etype, batchNumber, Security.CurUser.UserNum);
                    }

                    if (etrans == null)
                    {
                        //etrans still cannot be retrieved, so we give up at this point and continue onto the next iteration.
                        //In the future, we should consider adding a DBM which will retroactively set the etrans.EtransMessageTextNum.
                        //etrans.EtransMessageTextNum is allowed to be 0 so it will not crash the program.
                        continue;
                    }

                    etrans.EtransMessageTextNum = etransMsgText.EtransMessageTextNum;
                    Etranss.Update(etrans);
                    //Now we need to update our cache of claims to reflect the change that took place in the database above in Etranss.SetClaimSentOrPrinted()
                    queueItems[j].ClaimStatus = "S";
                }
            }
        }

        ///<summary>If no comm bridge is selected for a clearinghouse, this launches any client program the user has entered.  We do not want to cause a rollback, so no return value.</summary>
        private static void AttemptLaunch(Clearinghouse clearinghouseClin, int batchNum)
        {
            //called from Eclaims.cs. clinic-level clearinghouse passed in.
            if (clearinghouseClin.ClientProgram == "")
            {
                return;
            }

            if (! /* ODEnvironment.IsCloudServer */ false && !File.Exists(clearinghouseClin.ClientProgram))
            {
                MessageBox.Show(clearinghouseClin.ClientProgram + " " + Lans.g("Eclaims", "does not exist."));
                return;
            }

            try
            {
                ODFileUtils.ProcessStart(clearinghouseClin.ClientProgram);
            }
            catch (ODException odEx)
            {
                MessageBox.Show(odEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Lans.g("Eclaims", "Client program could not be started.  It may already be running. You must open your client program to finish sending claims."));
            }
        }

        ///<summary>Fills the missing data field on the queueItem that was passed in.  This contains all missing data on this claim.  Claim will not be allowed to be sent electronically unless this string comes back empty.  Set skipUB04 true to skip validating fields within the UB04 group box in the Claim Edit window.</summary>
        public static ClaimSendQueueItem GetMissingData(Clearinghouse clearinghouseClin, ClaimSendQueueItem queueItem, bool skipUB04 = false)
        {
            if (queueItem == null)
            {
                return new ClaimSendQueueItem() {MissingData = Lans.g("Eclaims", "Unable to fill claim data. Please recreate claim.")};
            }

            queueItem.Warnings = "";
            queueItem.MissingData = "";
            queueItem.ErrorsPreventingSave = "";
            //this is usually just the default clearinghouse or the clearinghouse for the PayorID.
            if (clearinghouseClin == null)
            {
                if (queueItem.MedType == EnumClaimMedType.Dental)
                {
                    queueItem.MissingData += "No default dental clearinghouse set.";
                }
                else
                {
                    queueItem.MissingData += "No default medical/institutional clearinghouse set.";
                }

                return queueItem;
            }

            #region Data Sanity Checking (for Replication)

            //Example: We had one replication customer who was able to delete an insurance plan for which was attached to a claim.
            //Imagine two replication servers, server A and server B.  An insplan is created which is not associated to any claims.
            //Both databases have a copy of the insplan.  The internet connection is lost.  On server A, a user deletes the insurance
            //plan (which is allowed because no claims are attached).  On server B, a user creates a claim with the insurance plan.
            //When the internet connection returns, the delete insplan statement is run on server B, which then creates a claim with
            //an invalid InsPlanNum on server B.  Without the checking below, the send claims window would crash for this one scenario.
            Claim claim = Claims.GetClaim(queueItem.ClaimNum); //This should always exist, because we just did a select to get the queue item.
            InsPlan insPlan = InsPlans.RefreshOne(claim.PlanNum);
            if (insPlan == null)
            {
                //Check for missing PlanNums
                queueItem.ErrorsPreventingSave = Lans.g("Eclaims", "Claim insurance plan record missing.  Please recreate claim.");
                queueItem.MissingData = Lans.g("Eclaims", "Claim insurance plan record missing.  Please recreate claim.");
                return queueItem;
            }

            if (claim.InsSubNum2 != 0)
            {
                InsPlan insPlan2 = InsPlans.RefreshOne(claim.PlanNum2);
                if (insPlan2 == null)
                {
                    //Check for missing PlanNums
                    queueItem.MissingData = Lans.g("Eclaims", "Claim other insurance plan record missing.  Please recreate claim.");
                    return queueItem; //This will let the office send other claims that passed validation without throwing an exception.
                }
            }

            #endregion Data Sanity Checking (for Replication)

            if (claim.ProvTreat == 0)
            {
                //This has only happened in the past due to a conversion.
                queueItem.MissingData = Lans.g("Eclaims", "No treating provider set.");
                return queueItem;
            }

            try
            {
                if (clearinghouseClin.Eformat == ElectronicClaimFormat.x837D_4010)
                {
                    X837_4010.Validate(clearinghouseClin, queueItem); //,out warnings);
                    //return;
                }
                else if (clearinghouseClin.Eformat == ElectronicClaimFormat.x837D_5010_dental
                         || clearinghouseClin.Eformat == ElectronicClaimFormat.x837_5010_med_inst)
                {
                    X837_5010.Validate(clearinghouseClin, queueItem, skipUB04); //,out warnings);
                    //return;
                }
                else if (clearinghouseClin.Eformat == ElectronicClaimFormat.Renaissance)
                {
                    queueItem.MissingData = Renaissance.GetMissingData(queueItem);
                    //return;
                }
                else if (clearinghouseClin.Eformat == ElectronicClaimFormat.Canadian)
                {
                    queueItem.MissingData = Canadian.GetMissingData(queueItem); //will also set warnings
                    //return;
                }
                else if (clearinghouseClin.Eformat == ElectronicClaimFormat.Dutch)
                {
                    Dutch.GetMissingData(queueItem); //,out warnings);
                    //return;
                }
                else if (clearinghouseClin.Eformat == ElectronicClaimFormat.Ramq)
                {
                    Ramq.GetMissingData(clearinghouseClin, queueItem);
                }
            }
            catch (Exception e)
            {
                queueItem.MissingData = Lans.g("Eclaims", "Unable to validate claim data:") + " " + e.Message + Lans.g("Eclaims", " Please recreate claim.");
            }

            return queueItem;
        }
    }
}