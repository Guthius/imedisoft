using CodeBase;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.AutoComm;

namespace OpenDentBusiness;

public class BillingL
{
    public static void SendStatements(SendStatementsIO sendStatementsIO)
    {
        //Run aging for all patients. May be skipped if aging has already run today.
        if (!RunAgingEnterprise(sendStatementsIO))
        {
            return;
        }

        //Statements will be ordered in GetStatements.
        List<Statement> listStatements = Statements.GetStatements(sendStatementsIO.ListStatementNumsToSend);
        Statement popUpCheck = listStatements.FirstOrDefault(x => x.Mode_ == StatementMode.Electronic);
        //In case the user didn't come directly from FormBillingOptions check the DateRangeFrom on an electronic statement to see if we need to
        //display the warning message. Spot checking to save time. 
        if (popUpCheck != null && (sendStatementsIO.FuncGetIsHistoryStartMinDate() || popUpCheck.DateRangeFrom.Year < 1880))
        {
            if (!sendStatementsIO.FuncAskQuestion(Lans.g("FormBilling", "Sending statements electronically for all account history could result in many pages. Continue?")))
            {
                return;
            }

            SecurityLogs.MakeLogEntry(EnumPermType.Billing, 0, "User proceeded with electronic billing for all dates.");
            sendStatementsIO.LogWrite(Lans.g("FormBilling", "User proceeded with electronic billing for all dates."), LogLevel.Information);
        }

        sendStatementsIO.DictionaryFamilies = Statements.GetFamiliesForStatements(listStatements);
        //Installment plans don't get added to db. They get retrieved from db and appended to the appropriate statement passed in.
        //We will use installments later in Statements.CreateStatementPdfSheets.
        Statements.AddInstallmentPlansToStatements(listStatements, sendStatementsIO.DictionaryFamilies);
        List<Patient> listPatients = sendStatementsIO.DictionaryFamilies.Values.SelectMany(x => x.ListPats).DistinctBy(x => x.PatNum).ToList();
        sendStatementsIO.ListStatementBatches = Statements.GetBatchesForStatements(listStatements, listPatients);
        for (int i = 0; i < sendStatementsIO.ListStatementBatches.Count; i++)
        {
            sendStatementsIO.CurStatementBatch = sendStatementsIO.ListStatementBatches[i];
            if (!BillingProgressPause(sendStatementsIO))
            {
                return;
            }

            //Fire progress event before we start.
            sendStatementsIO.FireOverallProgress();
            sendStatementsIO.FireTextMsgProgress(Lans.g("FormBilling", "Preparing Batch") + " " + sendStatementsIO.CurStatementBatch.BatchNum);
            //Now to print, send eBills, and text messages.
            //If any return false, the user canceled during execution OR other catastrophic failure occurred.
            bool isBatchValid = PrintBatch(sendStatementsIO);
            if (isBatchValid)
            {
                //Only send eBills if batch ok.
                isBatchValid = SendEBills(sendStatementsIO);
            }

            if (isBatchValid)
            {
                //Only send texts if batch and eBills ok.
                isBatchValid = SendTextMessages(sendStatementsIO);
            }

            //Always sync back to StatementProd.
            Statements.SyncStatementProdsForMultipleStatements(sendStatementsIO.CurStatementBatch.ListStatementDatas);
            //There was an issue with this batch don't continue.
            if (!isBatchValid)
            {
                return;
            }

            sendStatementsIO.FireTextMsgProgress(Lans.g("FormBilling", "Batch Completed") + "...");
        }

        //Pass back all temp files to FormBilling for file delete.
        for (int i = 0; i < sendStatementsIO.ListTempPdfFiles.Count; i++)
        {
            //Any failures will most likely get cleaned up when the user closes OD.
            ODException.SwallowAnyException(() => sendStatementsIO.ActionDeleteTempPdfFile(sendStatementsIO.ListTempPdfFiles[i]));
        }

        //Reporting on billing results.
        string message = "";
        int count = sendStatementsIO.ListSkippedPatients.Count(x => x.Reason == SkipReason.BadEmailAddress);
        if (count > 0)
        {
            message += Lans.g("FormBilling", "Skipped due to missing or bad email address:") + " " + count.ToString() + "\r\n";
        }

        count = sendStatementsIO.ListSkippedPatients.Count(x => x.Reason == SkipReason.BadMailingAddress);
        if (count > 0)
        {
            message += Lans.g("FormBilling", "Skipped due to missing or bad mailing address:") + " " + count.ToString() + "\r\n";
        }

        count = sendStatementsIO.ListSkippedPatients.Count(x => x.Reason == SkipReason.BadSmsSetup);
        if (count > 0)
        {
            message += Lans.g("FormBilling", "No text message sent due to SMS setup issue:") + " " + count.ToString() + "\r\n";
        }

        if (sendStatementsIO.CountStatementsSkippedForDeletion > 0)
        {
            message += Lans.g("FormBilling", "Skipped due to being deleted by another user:") + " " + sendStatementsIO.CountStatementsSkippedForDeletion.ToString() + "\r\n";
        }

        count = sendStatementsIO.ListSkippedPatients.Count(x => x.Reason == SkipReason.Misc);
        if (count > 0)
        {
            message += Lans.g("FormBilling", "Skipped due to miscellaneous error") + ": " + count.ToString() + "\r\n";
        }

        message += Lans.g("FormBilling", "Printed:") + " " + sendStatementsIO.CountStatementsPrinted.ToString() + "\r\n"
                   + Lans.g("FormBilling", "Emailed:") + " " + sendStatementsIO.CountStatementsEmailed.ToString() + "\r\n"
                   + Lans.g("FormBilling", "SentElect:") + " " + sendStatementsIO.CountStatementsSentElectronic.ToString() + "\r\n"
                   + Lans.g("FormBilling", "Texted:") + " " + sendStatementsIO.CountStatmentsSentPayPortalText.ToString();
        sendStatementsIO.LogWrite(message, LogLevel.Error);
        if (sendStatementsIO.ListSkippedPatients.Count > 0)
        {
            //Modify original box to have yes/no buttons to see if they want to see who errored out
            message += "\r\n\r\n" + Lans.g("FormBilling", "Would you like to see skipped patnums?");
            string skippedPatNums = Lans.g("FormBilling", "Skipped Patients...") + "\r\n" + string.Join("\r\n", sendStatementsIO.ListSkippedPatients
                .OrderBy(x => (int) x.Reason)
                .Select(x => $"{Lans.g("FormBilling", "PatNum:")} {x.PatNum} ({x.Reason}) {x.Error}"));
            if (sendStatementsIO.FuncAskQuestion(message))
            {
                sendStatementsIO.ActionPrompt(skippedPatNums, true);
            }
        }
        else
        {
            //If there were no errors, we simply show this.
            sendStatementsIO.ActionPrompt(message, false);
        }
    }
    
    private static bool PrintBatch(SendStatementsIO sendStatementsIO)
    {
        EmailMessage emailMessage;
        EmailAttach emailAttach;
        EmailAddress emailAddress;
        Patient patient;
        string patFolder;
        PdfDocument pdfDocumentInput;
        PdfPage pdfPage;
        string savedPdfPath;
        DataSet dataSet;
        List<EmailAutograph> listEmailAutographs = EmailAutographs.GetDeepCopy();
        BillingUseElectronicEnum electronicBillingType = PrefC.GetEnum<BillingUseElectronicEnum>(PrefName.BillingUseElectronic);
        //From Saul/Derek attempted fix B31268.
        //If we don't send emails first and there are a lot of e-bills and email address is set to implicit ssl then the emails fail to send sometimes.
        //This ordering will ensure that each batch will process emails before electronic.
        sendStatementsIO.CurStatementBatch.ListStatements = sendStatementsIO.CurStatementBatch.ListStatements.OrderBy(x => x.Mode_).ToList();
        for (int i = 0; i < sendStatementsIO.CurStatementBatch.ListStatements.Count; i++)
        {
            Statement statement = sendStatementsIO.CurStatementBatch.ListStatements[i];
            if (!BillingProgressPause(sendStatementsIO))
            {
                return false;
            }

            //Fire progress event before we iterate so we deal with counts before this loop iteration. That is most accurate.
            sendStatementsIO.FireOverallProgress();
            sendStatementsIO.CurStatementIdx++;
            //Pre-increment CountStatementsProcessed.
            //We will only decrement at the very end in the case where we determine this is an eBill and should be processed later as an eBill.
            sendStatementsIO.CurStatementBatch.CountStatementsProcessed++;
            if (statement == null)
            {
                //The statement was probably deleted by another user.
                sendStatementsIO.CountStatementsSkippedForDeletion++;
                continue;
            }

            if (sendStatementsIO.ListStatementNumsToSkipAfterPause.Contains(statement.StatementNum) || statement.IsSent)
            {
                //The statement was deleted or marked sent elsewhere.
                sendStatementsIO.AddSkippedPatient(statement.PatNum, SkipReason.Misc, Lans.g("FormBilling", "Statement was adjusted elsewhere."));
                continue;
            }

            sendStatementsIO.FireStatementProgress(5);
            sendStatementsIO.FireTextMsgProgress(Lans.g("FormBilling", "Generating Single PDFs") + "...");
            //We need the family of this patient so we can use the guarantor address later.
            if (!sendStatementsIO.DictionaryFamilies.TryGetValue(statement.PatNum, out Family family))
            {
                family = Patients.GetFamily(statement.PatNum);
            }

            patient = family.GetPatient(statement.PatNum);
            patFolder = ImageStore.GetPatientFolder(patient, ImageStore.GetPreferredAtoZpath());
            dataSet = AccountModules.GetStatementDataSet(statement, isComputeAging: false, doIncludePatLName: false);
            //Verify send email before trying to loop through any statements. If it's bad, we won't continue.
            emailAddress = sendStatementsIO.FuncGetSenderEmailAddress(patient.ClinicNum);
            sendStatementsIO.FireStatementProgress(10);
            if (statement.Mode_ == StatementMode.Email)
            {
                //Bad sender email is a show stopper, don't bother to continue.
                if (string.IsNullOrEmpty(emailAddress?.SMTPserver))
                {
                    sendStatementsIO.ActionPrompt(Lans.g("FormBilling", "You need to enter an SMTP server name in e-mail setup before you can send e-mail."), false);
                    return false;
                }

                //Bad patient email, just log it an move to next statement.
                if (patient.Email == "")
                {
                    sendStatementsIO.AddSkippedPatient(statement.PatNum, SkipReason.BadEmailAddress, Lans.g("FormBilling", "Empty patient Email"));
                    continue;
                }
            }

            sendStatementsIO.FireStatementProgress(15);
            statement.IsSent = true;
            statement.DateSent = DateTime.Today;

            #region Print PDFs

            string tempPdfFile = "";
            if (statement.Mode_ == StatementMode.Electronic && electronicBillingType.In(BillingUseElectronicEnum.EHG, BillingUseElectronicEnum.ClaimX) && !PrefC.GetBool(PrefName.BillingElectCreatePDF))
            {
                //Do not create a pdf
                //Detach the previously created document for the statement if one exists because it may not match what is sent to the patient,
                //and the Statement.DocNum will need to match the StatementProd.DocNum if late charges are going to be created.
                Statements.DetachDocFromStatements(statement.DocNum);
                //DocNum is set to zero for StatementProds when no pdf is created for electronic statements.
                sendStatementsIO.CurStatementBatch.ListStatementDatas.Add(new StatementData(dataSet, 0));
            }
            else
            {
                try
                {
                    tempPdfFile = Statements.CreateStatementPdfSheets(statement, patient, family, dataSet);
                    //The above method throws an exception or returns an empty string if unable to create a PDF.
                    if (tempPdfFile == "")
                    {
                        throw new Exception("Error creating statement PDF");
                    }

                    //Add to temp file list so we can delete it when we are done.
                    sendStatementsIO.ListTempPdfFiles.Add(tempPdfFile);
                }
                catch (Exception ex)
                {
                    sendStatementsIO.AddSkippedPatient(statement.PatNum, SkipReason.Misc, Lans.g("FormBilling", "Error creating PDF") + ": " + ex.ToString());
                    continue;
                }

                sendStatementsIO.FireStatementProgress(100);
                sendStatementsIO.FireTextMsgProgress(Lans.g("FormBilling", "PDF Created") + "...");
                if (statement.DocNum == 0)
                {
                    sendStatementsIO.ActionPrompt(Lans.g("FormBilling", "Failed to save PDF.  In Setup, DataPaths, please make sure the top radio button is checked."), false);
                    return false;
                }

                sendStatementsIO.CurStatementBatch.ListStatementDatas.Add(new StatementData(dataSet, statement.DocNum));
            }

            //imageStore = OpenDental.Imaging.ImageStore.GetImageStore(pat);
            //If stmt.DocNum==0, savedPdfPath will be "".  A blank savedPdfPath is fine for electronic statements.
            Document documentStatement = Documents.GetByNum(statement.DocNum);
            savedPdfPath = sendStatementsIO.FuncGetPatientPdfPath(tempPdfFile, ImageStore.GetFilePath(documentStatement, patFolder));
            if (statement.Mode_ == StatementMode.InPerson || statement.Mode_ == StatementMode.Mail)
            {
                //Will be null by default to indicate no printing necessary.
                sendStatementsIO.PdfMasterDocument = sendStatementsIO.PdfMasterDocument ?? new PdfDocument();
                pdfDocumentInput = sendStatementsIO.FuncGetPdfDocument(documentStatement.RawBase64, savedPdfPath);
                for (int idx = 0; idx < pdfDocumentInput.PageCount; idx++)
                {
                    pdfPage = pdfDocumentInput.Pages[idx];
                    sendStatementsIO.PdfMasterDocument.AddPage(pdfPage);
                    sendStatementsIO.FirePdfProgress(idx, pdfDocumentInput.PageCount);
                    sendStatementsIO.FireTextMsgProgress(Lans.g("FormBilling", "PDF Added to Print List") + "...");
                }

                sendStatementsIO.CountStatementsPrinted++;
                sendStatementsIO.ListStatementNumsSent.Add(statement.StatementNum);
                Statements.MarkSent(statement.StatementNum, statement.DateSent);
            }

            #endregion

            #region Preparing Email

            if (statement.Mode_ == StatementMode.Email)
            {
                sendStatementsIO.FireTextMsgProgress(Lans.g("FormBilling", "Preparing Email") + "...");
                sendStatementsIO.FireStatementProgress(40);
                try
                {
                    emailMessage = Statements.GetEmailMessageForStatement(statement, patient, emailAddress);
                    emailAttach = sendStatementsIO.FuncGetEmailAttachment(savedPdfPath, documentStatement, patient);
                    sendStatementsIO.FireStatementProgress(70);
                    emailMessage.Attachments.Add(emailAttach);
                    emailMessage.SentOrReceived = EmailSentOrReceived.Sent;
                    emailMessage.MsgDateTime = DateTime.Now;
                    if (PrefC.GetBool(PrefName.BillingEmailIncludeAutograph))
                    {
                        EmailAutograph emailAutograph = EmailAutographs.GetForOutgoing(listEmailAutographs, emailAddress);
                        if (emailAutograph != null)
                        {
                            //Always set the BodyText, we will additionally set HtmlText below if necessary (mimics FormEmailMessageEdit).
                            emailMessage.BodyText = EmailMessages.InsertAutograph(emailMessage.BodyText, emailAutograph);
                            if (MarkupEdit.ContainsOdHtmlTags(emailAutograph.AutographText))
                            {
                                //Attempt to convert entire message to html to accomodate for html autograph.
                                ODException.SwallowAnyException(() =>
                                {
                                    //This will format the entire body at HTML, not just the autograph. This now becomes an undocumented loophole to deploy an html statement email.
                                    string markup = MarkupEdit.TranslateToXhtml(EmailMessages.InsertAutograph(emailMessage.BodyText, emailAutograph), false, isEmail: true);
                                    //We got this far so change the message body and html type.
                                    emailMessage.HtmlText = markup;
                                    emailMessage.HtmlType = EmailType.Html;
                                });
                            }
                        }
                    }

                    long clinicNumPat = 0;
                    if (true)
                    {
                        clinicNumPat = patient.ClinicNum;
                        if (clinicNumPat == 0)
                        {
                            //set 0 clinic to use default clinic settings
                            clinicNumPat = PrefC.GetLong(PrefName.EmailSecureDefaultClinic);
                        }
                    }

                    bool useSecureEmail =
                        Enum.TryParse(ClinicPrefs.GetPrefValue(PrefName.EmailStatementsSecure, clinicNumPat), out EmailPlatform emailPlatform)
                        && emailPlatform == EmailPlatform.Secure
                        && Clinics.IsSecureEmailEnabled(clinicNumPat);
                    if (useSecureEmail)
                    {
                        emailMessage.SentOrReceived = EmailSentOrReceived.SecureEmailSent;
                    }

                    sendStatementsIO.ActionSendEmail(clinicNumPat, emailMessage, emailAddress, useSecureEmail);
                    sendStatementsIO.FireStatementProgress(90);
                    sendStatementsIO.CountStatementsEmailed++;
                    sendStatementsIO.FireTextMsgProgress(Lans.g("FormBilling", "Email Sent") + "...");
                }
                catch (Exception ex)
                {
                    sendStatementsIO.AddSkippedPatient(statement.PatNum, SkipReason.Misc, Lans.g("FormBilling", "Error sending email") + ": " + ex.ToString());
                    sendStatementsIO.FireStatementProgress(100);
                    continue;
                }

                sendStatementsIO.ListStatementNumsSent.Add(statement.StatementNum);
                Statements.MarkSent(statement.StatementNum, statement.DateSent);
            }

            #endregion

            #region Preparing E-Bills

            if (statement.Mode_ == StatementMode.Electronic)
            {
                sendStatementsIO.FireStatementProgress(65);
                sendStatementsIO.FireTextMsgProgress(Lans.g("FormBilling", "Preparing E-Bills") + "...");
                Patient guarantor = family.ListPats[0];
                if (guarantor.Address.Trim() == "" || guarantor.City.Trim() == "" || guarantor.State.Trim() == "" || guarantor.Zip.Trim() == "")
                {
                    sendStatementsIO.AddSkippedPatient(statement.PatNum, SkipReason.BadMailingAddress, Lans.g("FormBilling", "Error with patient address"));
                    continue;
                }

                //Eventually will not use Statement.IsRecipt or Statement.IsInvoice but rather StmtType.Invoice and StmtType.Receipt.
                if (statement.StatementType == StmtType.LimitedStatement || statement.IsReceipt || statement.IsInvoice)
                {
                    sendStatementsIO.AddSkippedPatient(statement.PatNum, SkipReason.Misc, Lans.g("FormBilling", "Limited statements, Receipts, and Invoices cannot be sent electronically."));
                    continue;
                }

                EbillStatement ebillStatement = new EbillStatement();
                ebillStatement.Family = family;
                ebillStatement.Statement = statement;
                long clinicNum = 0; //If clinics are disabled, then all bills will go into the same "bucket"
                if (true)
                {
                    clinicNum = family.Guarantor.ClinicNum;
                }

                if (electronicBillingType == BillingUseElectronicEnum.EHG)
                {
                    List<string> listElectErrors = Bridges.EHG_statements.Validate(clinicNum);
                    if (!listElectErrors.IsNullOrEmpty())
                    {
                        sendStatementsIO.AddSkippedPatient(statement.PatNum, SkipReason.Misc, listElectErrors.Last());
                        continue; //skip the current statement, since there are errors.
                    }
                }

                //We made it this far so this statement will be processed by SendEBills.
                //Let's decrement here since we already incremented at the top of this loop.
                //We will re-increment in SendEBills when we truly process this statement.
                sendStatementsIO.CurStatementBatch.CountStatementsProcessed--;
                sendStatementsIO.CurStatementBatch.ListEbillStatements.Add(ebillStatement);
                sendStatementsIO.FireStatementProgress(70);
                sendStatementsIO.FireTextMsgProgress(Lans.g("FormBilling", "E-Bill Added To Send List") + "...");
            }

            #endregion
        }

        //Fire progress event one last time after our last loop iteration.
        sendStatementsIO.FireOverallProgress();
        return true;
    }

    private static bool SendEBills(SendStatementsIO sendStatementsIO)
    {
        sendStatementsIO.FireStatementProgress(80);
        sendStatementsIO.FireTextMsgProgress(Lans.g("FormBilling", "Sending E-Bills") + "...");
        if (!BillingProgressPause(sendStatementsIO))
        {
            return false;
        }

        if (sendStatementsIO.CurStatementBatch.ListEbillStatements.Count == 0)
        {
            //All statements have been sent for the current batch.  Nothing more to do.
            sendStatementsIO.LogWrite(Lans.g("FormBilling", "No ebills need to be sent."), LogLevel.Information);
            return true;
        }

        sendStatementsIO.LogWrite(Lans.g("FormBilling", "Sending ebills for ClinicNum") + " " + sendStatementsIO.CurStatementBatch.ClinicNum.ToString(), LogLevel.Information);
        BillingUseElectronicEnum electronicBillingType = PrefC.GetEnum<BillingUseElectronicEnum>(PrefName.BillingUseElectronic);
        XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
        xmlWriterSettings.OmitXmlDeclaration = true;
        xmlWriterSettings.Encoding = Encoding.UTF8;
        xmlWriterSettings.Indent = true;
        xmlWriterSettings.IndentChars = "   ";
        //Holds all statements. Will be sent to 1 of 4 electronic billing vendors.
        //Each vendor will perform 3 phases on this string.
        //1) Append general practice info
        //2) Append each individual statement's info to an xml writer
        //3) Send xlm string to vendor
        StringBuilder strBuildElect = new StringBuilder();
        XmlWriter xmlWriterElect = XmlWriter.Create(strBuildElect, xmlWriterSettings);

        #region 1) Append general practice info

        if (electronicBillingType == BillingUseElectronicEnum.EHG)
        {
            Bridges.EHG_statements.GeneratePracticeInfo(xmlWriterElect, sendStatementsIO.CurStatementBatch.ClinicNum);
        }
        else if (electronicBillingType == BillingUseElectronicEnum.POS)
        {
            Bridges.POS_statements.GeneratePracticeInfo(xmlWriterElect, sendStatementsIO.CurStatementBatch.ClinicNum);
        }
        else if (electronicBillingType == BillingUseElectronicEnum.ClaimX)
        {
            Bridges.ClaimX_Statements.GeneratePracticeInfo(xmlWriterElect, sendStatementsIO.CurStatementBatch.ClinicNum);
        }
        else if (electronicBillingType == BillingUseElectronicEnum.EDS)
        {
            Bridges.EDS_Statements.GeneratePracticeInfo(xmlWriterElect, sendStatementsIO.CurStatementBatch.ClinicNum);
        }
        else
        {
            sendStatementsIO.LogWrite(Lans.g("FormBilling", "\'No billing electronic\' is currently selected in Billing Defaults."), LogLevel.Error);
        }

        #endregion 1) Append general practice info

        #region 2) Append each individual statement's info to an xml writer

        Family family;
        Patient patient;
        DataSet dataSet;
        List<long> listElectStmtNums = [];
        sendStatementsIO.FireStatementProgress(85);
        //This loop has iterated backwards since day 1. Many years ago, we would batch and remove from the end of this list instead of just making a new list.
        //Batching was solved in a different way sometime along the way so iterating backwards is no longer important. Leaving backwards iteration in-tact just in case.
        for (int i = 0; i < sendStatementsIO.CurStatementBatch.ListEbillStatements.Count; i++)
        {
            if (!BillingProgressPause(sendStatementsIO))
            {
                return false;
            }

            //Fire progress event before we iterate so we deal with counts before this loop iteration. That is most accurate.
            sendStatementsIO.FireOverallProgress();
            //It is finally time to increment for this statement.
            sendStatementsIO.CurStatementBatch.CountStatementsProcessed++;
            Statement statementCur = sendStatementsIO.CurStatementBatch.ListEbillStatements[i].Statement;
            if (statementCur == null)
            {
                //The statement was probably deleted by another user.
                sendStatementsIO.CountStatementsSkippedForDeletion++;
                continue;
            }

            if (sendStatementsIO.ListStatementNumsToSkipAfterPause.Contains(statementCur.StatementNum))
            {
                //The statement was deleted or marked sent elsewhere while this billing session was paused and subsequently resumed.
                sendStatementsIO.AddSkippedPatient(statementCur.PatNum, SkipReason.Misc, Lans.g("FormBilling", "Statement was adjusted elsewhere."));
                continue;
            }

            family = sendStatementsIO.CurStatementBatch.ListEbillStatements[i].Family;
            patient = family.GetPatient(statementCur.PatNum);
            dataSet = AccountModules.GetStatementDataSet(statementCur, isComputeAging: false, doIncludePatLName: false);
            try
            {
                //Write the statement into a temporary string builder, so that if the statement fails to generate (due to exception),
                //then the partially generated statement will not be added to the strBuildElect.
                StringBuilder strBuildStatement = new StringBuilder();
                using (XmlWriter xmlWriterStatement = XmlWriter.Create(strBuildStatement, xmlWriterElect.Settings))
                {
                    if (electronicBillingType == BillingUseElectronicEnum.None)
                    {
                        throw new Exception(Lans.g("FormBilling", "\'No billing electronic\' is currently selected in Billing Defaults."));
                    }
                    else if (electronicBillingType == BillingUseElectronicEnum.EHG)
                    {
                        Bridges.EHG_statements.GenerateOneStatement(xmlWriterStatement, statementCur, patient, family, dataSet);
                    }
                    else if (electronicBillingType == BillingUseElectronicEnum.POS)
                    {
                        Bridges.POS_statements.GenerateOneStatement(xmlWriterStatement, statementCur, patient, family, dataSet);
                    }
                    else if (electronicBillingType == BillingUseElectronicEnum.ClaimX)
                    {
                        Bridges.ClaimX_Statements.GenerateOneStatement(xmlWriterStatement, statementCur, patient, family, dataSet);
                    }
                    else if (electronicBillingType == BillingUseElectronicEnum.EDS)
                    {
                        Bridges.EDS_Statements.GenerateOneStatement(xmlWriterStatement, statementCur, patient, family, dataSet);
                    }
                }

                //Write this statement's XML to the XML document with all the statements.
                using (XmlReader readerStatement = XmlReader.Create(new StringReader(strBuildStatement.ToString())))
                {
                    xmlWriterElect.WriteNode(readerStatement, true);
                }

                //We made it this far for this statement so it was generated successfully.
                listElectStmtNums.Add(statementCur.StatementNum);
            }
            catch (Exception ex)
            {
                sendStatementsIO.AddSkippedPatient(patient.PatNum, SkipReason.Misc, Lans.g("FormBilling", "Error sending statement") + ": " + ex.ToString());
            }
        }

        xmlWriterElect.Close();
        if (listElectStmtNums.Count == 0)
        {
            //All statements for this batch were either deleted or had an exception thrown while generating.
            return true; //Go on to next batch
        }

        sendStatementsIO.FireStatementProgress(90);

        #endregion 2) Append each individual statement's info

        #region 3) Send xlm string to vendor

        //Each vendor uses initial directory and xml pref slightly different.
        string xmlFilePathFromPref = "";
        string initialSaveDirectory = "";
        bool doMarkSent = false;
        if (electronicBillingType == BillingUseElectronicEnum.EHG)
        {
            //This is a web call to DentalXChange so we will try 3 times before we consider it failed.
            for (int attempts = 0; attempts < 3; attempts++)
            {
                string alertMsg = null;
                try
                {
                    alertMsg = sendStatementsIO.FuncSendEhgStatement(strBuildElect.ToString(), sendStatementsIO.CurStatementBatch.ClinicNum);
                    if (!string.IsNullOrEmpty(alertMsg))
                    {
                        sendStatementsIO.ActionPrompt(alertMsg, false);
                    }

                    //We made it this far so batch succeeded.
                    doMarkSent = true;
                    break;
                }
                catch (Exception ex)
                {
                    if (attempts < 2)
                    {
                        //Don't indicate the error unless it failed on the last attempt.
                        continue; //The only thing skipped besides the error message is evaluating if the statement was written, which it wasn't.
                    }

                    //This batch was not sent
                    if (ex.Message.Contains("(404) Not Found"))
                    {
                        //Custom 404 error message
                        sendStatementsIO.AppendMiscSystemError(Lans.g("FormBilling", "The connection to the server could not be established or was lost, or the upload timed out.  "
                                                                                     + "Ensure your internet connection is working and that your firewall is not blocking this application.  "
                                                                                     + "If the upload timed out after 10 minutes, try sending 25 statements or less in each batch to reduce upload time."));
                    }
                    else
                    {
                        //Document any other errors to make troubleshooting much easier.
                        sendStatementsIO.AppendMiscSystemError(Lans.g("FormBilling", ex.Message));
                    }
                    //An API exception will return true below, which will allow subsequent batches to continue to run.
                }
            }
        }
        else if (electronicBillingType == BillingUseElectronicEnum.POS)
        {
            xmlFilePathFromPref = PrefC.GetString(PrefName.BillingElectStmtOutputPathPos);
            initialSaveDirectory = "";
        }
        else if (electronicBillingType == BillingUseElectronicEnum.ClaimX)
        {
            xmlFilePathFromPref = "";
            //Clint from ExtraDent requested this default path.
            initialSaveDirectory = @"C:\StatementX\";
        }
        else if (electronicBillingType == BillingUseElectronicEnum.EDS)
        {
            xmlFilePathFromPref = PrefC.GetString(PrefName.BillingElectStmtOutputPathEds);
            initialSaveDirectory = "";
        }

        if (electronicBillingType != BillingUseElectronicEnum.EHG)
        {
            //DentalXChange does not write to a file. All others do.
            if (!sendStatementsIO.SetXmlFilePath(xmlFilePathFromPref, initialSaveDirectory))
            {
                if (!sendStatementsIO.AllowXmlFileSelection)
                {
                    sendStatementsIO.AppendMiscSystemError(Lans.g("FormBilling", $"The preference for {electronicBillingType} does not have a valid path."));
                }

                //User elected to cancel when prompted for a file path on current or previous iteration.
                //Dont make them cancel again, just cancel for all batches and move on to next batch.
                return true;
            }

            //Convert base path to clinic specific path.
            string xmlFilePathClinic = Statements.GetEbillFilePathForClinic(sendStatementsIO.XmlFilePath, sendStatementsIO.CurStatementBatch.ClinicNum);
            File.WriteAllText(xmlFilePathClinic, strBuildElect.ToString());
            doMarkSent = true;
        }

        if (doMarkSent)
        {
            //Loop through all statements in the batch and mark them sent.
            for (int i = 0; i < listElectStmtNums.Count; i++)
            {
                //Adding here assures that only IsSent=true statements will be attempted in SendTextMessages.
                sendStatementsIO.ListStatementNumsSent.Add(listElectStmtNums[i]);
                Statements.MarkSent(listElectStmtNums[i], DateTime.Today);
                sendStatementsIO.CountStatementsSentElectronic++;
                sendStatementsIO.FireStatementProgress(100);
            }

            sendStatementsIO.FireTextMsgProgress(Lans.g("FormBilling", "E-Bills Sent") + "...");
        }
        else
        {
            sendStatementsIO.FireTextMsgProgress(Lans.g("FormBilling", "E-Bills Not Sent") + "...");
        }

        #endregion 3) Send to vendor

        //Fire progress event one last time after our last loop iteration.
        sendStatementsIO.FireOverallProgress();
        return true;
    }

    private static bool SendTextMessages(SendStatementsIO sendStatementsIO)
    {
        List<SmsToMobile> listTextsToSend = [];
        List<Patient> listPatients = sendStatementsIO.DictionaryFamilies.Values.SelectMany(x => x.ListPats).DistinctBy(x => x.PatNum).ToList();
        List<PatComm> listPatComms = Patients.GetPatComms(listPatients);
        string guidBatch = null;
        for (int i = 0; i < sendStatementsIO.CurStatementBatch.ListStatements.Count; i++)
        {
            Statement statement = sendStatementsIO.CurStatementBatch.ListStatements[i];
            if (!BillingProgressPause(sendStatementsIO))
            {
                return false;
            }

            if (sendStatementsIO.ListSkippedPatients.Any(x => x.PatNum == statement.PatNum))
            {
                //Already skipped this patient for some other reason. Don't bother sending a text.
                //Skipping here is new behavior for 24.2 Billing refactor. Previously would send statement text for skipped patients (bug).
                continue;
            }

            if (!sendStatementsIO.ListStatementNumsSent.Contains(statement.StatementNum))
            {
                //This statement was not sent for whatever reason so don't bother sending a text message.
                continue;
            }

            //Statements are generated in FormBillingOptions.
            //This form will link certain statement modes (Email, Electronic, InPerson, etc) to receive a corresponding text message.
            //If the Mode of this statement is not set to send a text message, then we will skip the text message here.
            if (statement.SmsSendStatus != AutoCommStatus.SendNotAttempted)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(PrefC.GetString(PrefName.BillingDefaultsSmsTemplate)))
            {
                //User has opted to allow billing to run despite not having a valide sms template.
                //This combination would previously cause this method to return false, which would cause billing to halt after already having sent first batch of statements.
                //2/29/24 - SamO decided that this is not a haltable offense since user opted to allow it to happen. Instead we will just add this failed comm to the error list.
                sendStatementsIO.AddSkippedPatient(statement.PatNum, SkipReason.BadSmsSetup, Lans.g("FormBilling", "SMS Statements template not setup"));
                continue;
            }

            PatComm patComm = listPatComms.Find(x => x.PatNum == statement.PatNum);
            if (patComm == null)
            {
                sendStatementsIO.AddSkippedPatient(statement.PatNum, SkipReason.BadSmsSetup, Lans.g("FormBilling", "Unable to find patient communication method"));
                continue;
            }

            if (!patComm.IsSmsAnOption)
            {
                continue;
            }

            if (patComm.CommOptOut.IsOptedOut(CommOptOutMode.Text, CommOptOutType.Statements))
            {
                sendStatementsIO.AddSkippedPatient(statement.PatNum, SkipReason.BadSmsSetup, Lans.g("FormBilling", "Patient is opted out of automated messaging."));
                continue;
            }

            Patient patient = listPatients.Find(x => x.PatNum == statement.PatNum) ?? Patients.GetPat(statement.PatNum);
            if (patient == null)
            {
                sendStatementsIO.AddSkippedPatient(statement.PatNum, SkipReason.BadSmsSetup, Lans.g("FormBilling", "Unable to find patient"));
                continue;
            }

            SmsToMobile textToSend = new SmsToMobile();
            textToSend.ClinicNum = patient.ClinicNum;
            textToSend.GuidMessage = Guid.NewGuid().ToString();
            textToSend.IsTimeSensitive = false;
            textToSend.MobilePhoneNumber = patComm.SmsPhone;
            textToSend.PatNum = statement.PatNum;
            try
            {
                textToSend.MsgText = new MsgToPayTagReplacer().ReplaceTagsForStatement(PrefC.GetString(PrefName.BillingDefaultsSmsTemplate), patient, statement);
            }
            catch (Exception e)
            {
                sendStatementsIO.AddSkippedPatient(statement.PatNum, SkipReason.BadSmsSetup, Lans.g("FormBilling", "Failed to format text message correctly") + ": " + e.Message);
                continue;
            }

            textToSend.MsgType = SmsMessageSource.Statements;
            //First message guid is the batch guid.
            guidBatch = guidBatch ?? textToSend.GuidMessage;
            textToSend.GuidBatch = guidBatch;
            listTextsToSend.Add(textToSend);
            //Store the guid here so we can reference it below and link this statement back to it's message.
            statement.TagOD = textToSend.GuidMessage;
        }

        if (!BillingProgressPause(sendStatementsIO))
        {
            return false;
        }

        if (listTextsToSend.Count == 0)
        {
            return true;
        }

        try
        {
            sendStatementsIO.FireTextMsgProgress(Lans.g("FormBilling", "Sending text messages") + "...");
            List<SmsToMobile> listSmsToMobiles = SmsToMobiles.SendSmsMany(listTextsToSend, userod: Security.CurUser);
            List<SmsToMobile> listSmsToMobilesFails = Statements.HandleSmsSent(listSmsToMobiles, sendStatementsIO.CurStatementBatch.ListStatements);
            for (int i = 0; i < listSmsToMobilesFails.Count; ++i)
            {
                sendStatementsIO.AddSkippedPatient(listSmsToMobilesFails[i].PatNum, SkipReason.BadSmsSetup, Lans.g("Statements", "Error Sending text messages") + ": " + listSmsToMobilesFails[i].CustErrorText);
            }

            sendStatementsIO.CountStatmentsSentPayPortalText += listSmsToMobiles.Where(x => x.SmsStatus != SmsDeliveryStatus.FailNoCharge).Count();
        }
        catch (Exception ex)
        {
            sendStatementsIO.AppendMiscSystemError(Lans.g("FormBilling", "Error Sending text messages") + ": " + ex.ToString());
            List<long> listFailedStatementNums = sendStatementsIO.CurStatementBatch.ListStatements
                //Statement.TagOD was set to SmsToMobile.GuidMessage above. Match all of those back here so we can fail them all.
                .FindAll(x => x.TagOD is string guidMessage && listTextsToSend.Any(y => y.GuidMessage == guidMessage))
                .Select(x => x.StatementNum).ToList();
            Statements.UpdateSmsSendStatus(listFailedStatementNums, AutoCommStatus.SendFailed);
        }

        return true;
    }

    public static bool BillingProgressPause(SendStatementsIO sendStatementsIO)
    {
        sendStatementsIO.ListStatementNumsToSkipAfterPause = [];
        bool hasEventFired = false;
        //Pause until resume.
        while (sendStatementsIO.FunctGetIsPaused())
        {
            if (!hasEventFired)
            {
                //Don't fire this event more than once.
                sendStatementsIO.FireTextMsgProgress(Lans.g("FormBilling", "Warning"), isWarningOffEvent: true);
                hasEventFired = true;
            }

            sendStatementsIO.ActionSleepDuringPause();
            //Resume?
            if (!sendStatementsIO.FunctGetIsPaused())
            {
                //Get remaining statements given original constraints from UI.
                DataTable table = sendStatementsIO.FuncGetBillingDataTable();
                List<long> listStatementNumsFromDb = table.Select().Select(x => PIn.Long(x["StatementNum"].ToString())).ToList();
                //Get statement nums yet to be sent from original run.
                List<long> listStatementNumsUnsent = sendStatementsIO.ListStatementNumsToSend.Except(sendStatementsIO.ListStatementNumsSent).ToList();
                //Capture any statements that were deleted while this billing progress was paused.
                sendStatementsIO.ListStatementNumsToSkipAfterPause = listStatementNumsUnsent.Except(listStatementNumsFromDb).ToList();
            }

            if (sendStatementsIO.FuncGetIsCancelled())
            {
                //State changed from paused to cancelled.
                return false;
            }
        }

        //Check to see if the user wants to stop sending statements.
        if (sendStatementsIO.FuncGetIsCancelled())
        {
            return false;
        }

        return true;
    }

    public static bool RunAgingEnterprise(SendStatementsIO sendStatementsIO)
    {
        DateTime dateTimeNow = MiscData.GetNowDateTime();
        DateTime dateTimeToday = dateTimeNow.Date;
        DateTime dateTimeLastAging = PrefC.GetDate(PrefName.DateLastAging);
        if (dateTimeLastAging.Date == dateTimeToday)
        {
            return true; //already ran aging for this date, just move on
        }

        Prefs.RefreshCache();
        if (!PrefC.IsAgingAllowedToStart())
        {
            string prompt = Lans.g("FormBilling", "In order to print or send statments, aging must be re-calculated, but you cannot run aging until it has "
                                                  + "finished the current calculations which began on") + " " + PrefC.GetDateT(PrefName.AgingBeginDateTime).ToString() + ".\r\n" + Lans.g("FormBilling", "If you believe the current "
                                                                                                                                                                                                         + "aging process has finished, a user with SecurityAdmin permission can manually clear the date and time by going to Setup | Preferences | Account - General "
                                                                                                                                                                                                         + "and pressing the 'Clear' button.");
            sendStatementsIO.ActionPrompt(prompt, false);
            return false;
        }

        SecurityLogs.MakeLogEntry(EnumPermType.AgingRan, 0, "Starting Aging - " + sendStatementsIO.Source);
        Prefs.UpdateString(PrefName.AgingBeginDateTime, POut.DateTime(dateTimeNow, false)); //get lock on pref to block others
        Signalods.SetInvalid(InvalidType.Prefs); //signal a cache refresh so other computers will have the updated pref as quickly as possible
        sendStatementsIO.LogWrite(Lans.g("FormBilling", "Calculating enterprise aging for all patients as of") + " " + dateTimeToday.ToShortDateString() + "...", LogLevel.Information);
        bool ret = sendStatementsIO.FuncComputeAging(dateTimeToday);
        Prefs.UpdateString(PrefName.AgingBeginDateTime, ""); //clear lock on pref whether aging was successful or not
        if (ret)
        {
            //Only move aging date forward if success.
            Prefs.UpdateString(PrefName.DateLastAging, POut.Date(dateTimeToday, false));
            SecurityLogs.MakeLogEntry(EnumPermType.AgingRan, 0, "Aging complete - " + sendStatementsIO.Source);
        }

        Signalods.SetInvalid(InvalidType.Prefs);
        return ret;
    }
}
    
public enum SkipReason
{
    BadEmailAddress,
    BadMailingAddress,
    BadSmsSetup,
    Misc
}

public class SendStatementsSkipped
{
    public long PatNum;
    public string Error;
    public SkipReason Reason;
}

public class SendStatementsIO
{
    #region Inputs

    ///<summary>Statements attempting to be sent. Generated from raw DataTable which is a reflection of grid selections in FormBilling.
    ///Will be converted to list of Statements as needed inside SendStatements.</summary>
    public List<long> ListStatementNumsToSend = [];

    ///<summary>Use for logging.</summary>
    public LogWriter LogWriter = null;

    ///<summary>ODService will not allow user to select xml file for e-bill generation. Default folder for e-billing type must exist or billing will fail.
    ///True by default.</summary>
    public bool AllowXmlFileSelection = true;

    ///<summary>Used for aging security logs. This Source will print to SecurityLog.LogText.</summary>
    public string Source = "Undefined";

    #endregion

    #region Outputs

    ///<summary>Any error that causes a bill to not be properly generated for a given patient is added to this list.
    ///This had previously been stored as a list of dictionaries, making it the most hideous use of dictionary ever recorded in Open Dental lore.
    ///Nothing prevents a given patient from having multiple entries in this list.
    ///The only duplicated prevented in this list is for PatNum 0 for SkipReason Misc. These are system errors and will all be appended to a single entry.</summary>
    public List<SendStatementsSkipped> ListSkippedPatients = [];

    ///<summary>Running tally of statements that were successfully emailed. Statement.Mode must be Email for this count to increment.
    ///Failed email results in no increment and and entry in ListSkippedPatients.</summary>
    public int CountStatementsEmailed;

    ///<summary>Running tally of statements that were successfully added to the PdfMasterDocument. 
    ///Statement.Mode must be InPerson or Mail for this count to increment.</summary>
    public int CountStatementsPrinted;

    ///<summary>Running tally of statements that were successfully sent electroincally. 
    ///Statement.Mode must be Electronic for this count to increment.
    ///There are 4 electronic billing providers, which provider is used by practice is stored in PrefName.BillingUseElectronic.
    ///Statements are sent in batches to electronic billing providers so if the operation fails, then all statements are failed and CountStatementsSentElectronic will be decremented accordingly.</summary>
    public int CountStatementsSentElectronic;

    ///<summary>Pdf that is the sum off all statements' pages for this billing run which had a statement mode of InPerson or Mail.
    ///Will be null if no statements required print. Check for null before using!
    ///Each statement's document will be opened and each page of that document will be added to the PdfMasterDocument.
    ///This is used to show a single printable pdf on screen when the billing run has completed. All statements needing print can then be printed with one user print command.</summary>
    public PdfDocument PdfMasterDocument;

    ///<summary>Running tally of statements that were not able to be sent due to statement no longer existing.
    ///This usually means that statement was deleted by another user will billing was running.
    ///These statements will be counted as processed by no bill will be produced.</summary>
    public int CountStatementsSkippedForDeletion;

    ///<summary>Running tally of statements that had a text message generated and succesfully sent to a patient.
    ///In order to receive a text, clinic must be signed up for texting, the patient must meet the criteria of PatComm.IsSmsAnOption, and patient must not be opted out.
    ///This tally only increments if all those criteria are met and then a text is sent to the patient and that text does not fail to send for any other reason.</summary>
    public int CountStatmentsSentPayPortalText;

    ///<summary>Current index of overall progress of all statement. Used for progress updates only.</summary>
    public int CurStatementIdx;

    ///<summary>Statements will be processed in batches. 
    ///If clinics on then each batch will be clinic num of statements' patient's guarantor. 
    ///If clinics off then batch will be set arbitrarily and limited to size determined by PrefName.BillingElectBatchMax.</summary>
    public List<StatementBatch> ListStatementBatches = [];

    ///<summary>Managed by BillingL.SendStatements as it iterates through ListStatementBatches.</summary>
    public StatementBatch CurStatementBatch = new();

    ///<summary>Statements actually sent. Used when Billing is paused and resumed to determine where we left off.</summary>
    public List<long> ListStatementNumsSent = [];

    ///<summary>If progress is paused and then resumed, this list will be filled with statement nums that may have been deleted in the meantime and should now be skipped.</summary>
    public List<long> ListStatementNumsToSkipAfterPause = [];

    ///<summary>The families that are selected when the user hits "Send". The key is the PatNum and the value is its Family.
    ///Dictionary has deep roots in the supporting code for Billing so this anti-pattern (using dictionary) must persist in this case.</summary>
    public Dictionary<long, Family> DictionaryFamilies = new();

    ///<summary>List of all temp files that were created for a given Billing run. These will be passed back to FormBilling once billing has completed so they can be safely deleted.</summary>
    public List<string> ListTempPdfFiles = [];

    ///<summary>User may be prompted for an eBilling xml file path. If they elect to cancel that prompt, hold onto their choice so we can auto cancel for all batches.</summary>
    public bool DidUserCancelXmlFileSelection;

    ///<summary>Holds the path that either came from an individual eBilling vendor pref or (if that path does not exist) user is prompted for a path and a manual path will be set.
    ///Retain this path so we only have to prompt the user once on first batch iteration.</summary>
    public string XmlFilePath;

    #endregion

    #region Callbacks

    ///<summary>User has the option to run billing for all time (no StartDate specified). 
    ///If this option is selected and at least one statement is billed electronically, user will be prompted that this may take an extremely long time to complete. 
    ///User will then be given option to cancel.</summary>
    public Func<bool> FuncGetIsHistoryStartMinDate;

    ///<summary>Fired when any progress event occurs that requires UI update.</summary>
    public Action<ODEventArgs> ActionProgressEvent;

    ///<summary>Passes a question from billing logic back to UI (or unit test). Expects yes/no (true/false) answer.</summary>
    public Func<string, bool> FuncAskQuestion;

    ///<summary>Passes a prompt from billing logic back to UI (or unit test). Bool is used to indicate MsgCopyPaste dialog should be used in place of MsgBox.</summary>
    public Action<string, bool> ActionPrompt;

    ///<summary>Passes an initial directory that should be shown in a directory/file chooser dialog. Returns true if OK and path exists. Returns false if Cancel. Also returns file name selected by user (not including full path).</summary>
    public Func<string, ChooseSaveFile> FuncChooseSaveFile;

    ///<summary>Asks UI if user has paused billing.</summary>
    public Func<bool> FunctGetIsPaused;

    ///<summary>Asks UI if user has cancelled billing.</summary>
    public Func<bool> FuncGetIsCancelled;

    ///<summary>Gives UI a chance to sleep while billing is paused. Unit test can use this as a callback to perform operations during pause.</summary>
    public Action ActionSleepDuringPause;

    ///<summary>Typically a reflection of Statements.GetBilling. Use that method to mock columns/rows if desired in unit test.</summary>
    public Func<DataTable> FuncGetBillingDataTable;

    ///<summary>Input: Patient's ClinicNum, Output: Sender Email. Will typically use the email belonging to the clinic of the patient attached to the statement.</summary>
    public Func<long, EmailAddress> FuncGetSenderEmailAddress;

    ///<summary>Input: Patient's ClinicNum, EmailMessage to send, Send EmailAddress, Use Secure Email. Invoker must send the email via given method, and insert email into db.</summary>
    public Action<long, EmailMessage, EmailAddress, bool> ActionSendEmail;

    ///<summary>Input: tempPdfFile, filePath. Output: savedPdfPath.</summary>
    public Func<string, string, string> FuncGetPatientPdfPath;

    ///<summary>Input: RawBase64 (only used when AtoZ is in database), savedPdfPath (was set by output of FuncGetPatientPdfPath). Output: pdf document opened using given inputs.
    ///Invoker must evaulate the input args and open the PdfDocument. Leave PdfDocument open so Billing can read its pages.</summary>
    public Func<string, string, PdfDocument> FuncGetPdfDocument;

    ///<summary>Input: savedPdfPath (was set by output of FuncGetPatientPdfPath). Statement document, Patient belonging to statement. Output: email attachment.
    ///Invoker must evaulate the input args and create the EmailAttach to be attached to the satement email.</summary>
    public Func<string, Document, Patient, EmailAttach> FuncGetEmailAttachment;

    ///<summary>Input: Temp pdf file name to be deleted.</summary>
    public Action<string> ActionDeleteTempPdfFile;

    ///<summary>Input: The inputs of Bridges.EHG_statements.Send(). Output: Any alert message returned by Bridges.EHG_statements.Send().
    ///This callback exists so unit tests can mock the DentalXChange api call.</summary>
    public Func<string, long, string> FuncSendEhgStatement;

    ///<summary>Input: Today's date, taken from MySql host. Output: true if Ledgers.ComputeAging succeeded. If return false, billing is cancelled.</summary>
    public Func<DateTime, bool> FuncComputeAging;

    #endregion

    #region Helper Methods

    ///<summary>Tries to set XmlFilePath to be used on future batch iterations. If xmlFilePathBaseFromPref is no good then user will be prompted for local directory.
    ///If user cancels or has cancelled on previous iteration then returns false and you can assume no xml file should be generated for this batch.
    ///If XmlFilePath has been properly set, then returns true and you can continue with the batch.</summary>
    public bool SetXmlFilePath(string xmlFilePathBaseFromPref, string initialSaveDirectory)
    {
        if (DidUserCancelXmlFileSelection)
        {
            //User elected to cancel when prompted for a file path on previous iteration.
            //Dont make them cancel again, just cancel for all batches and move on to next batch.
            return false;
        }

        if (!string.IsNullOrEmpty(XmlFilePath))
        {
            //Path was already set on a previous iteration.
            return true;
        }

        //Path not set yet, let's try.
        if (!string.IsNullOrEmpty(xmlFilePathBaseFromPref) && Directory.Exists(xmlFilePathBaseFromPref))
        {
            //Pref is a valid path, set it.
            XmlFilePath = ODFileUtils.CombinePaths(xmlFilePathBaseFromPref, "Statements.xml");
            return true;
        }

        if (!AllowXmlFileSelection)
        {
            //ODService has no UI so xmlFilePathBaseFromPref directory must exist or we will have to return here and allow e-billing to fail.
            return false;
        }

        //Try to create initial save directory if one was provided.
        if (!string.IsNullOrEmpty(initialSaveDirectory))
        {
            ODException.SwallowAnyException(() => { Directory.CreateDirectory(initialSaveDirectory); });
        }

        //Directory did not exist, force user to choose a valid path.
        ChooseSaveFile chooseSaveFile = FuncChooseSaveFile(initialSaveDirectory);
        if (!chooseSaveFile.IsSelectionOk)
        {
            //To remember that the user canceled the first time through.  User only needs to cancel once to cancel all batches.
            DidUserCancelXmlFileSelection = true;
            //Go on to next batch
            return false;
        }

        //User chose a valid path, set it.
        XmlFilePath = chooseSaveFile.FileName;
        return true;
    }

    ///<summary>Adds a patient and reason to ListSkippedPatients when a patient statement cannot be processed.</summary>
    public void AddSkippedPatient(long patNum, SkipReason skipReason, string error)
    {
        SendStatementsSkipped sendStatementsSkipped = new SendStatementsSkipped();
        sendStatementsSkipped.PatNum = patNum;
        sendStatementsSkipped.Reason = skipReason;
        sendStatementsSkipped.Error = error;
        ListSkippedPatients.Add(sendStatementsSkipped);
    }

    ///<summary>There can be only 1 single misc system error. Usually because of a catastrophic failure like an outer loop exception. Always linked to PatNum 0.
    ///This method will append to that error or create it where necessary.</summary>
    public void AppendMiscSystemError(string error)
    {
        SendStatementsSkipped sendStatementsSkipped = ListSkippedPatients.Find(x => x.PatNum == 0 && x.Reason == SkipReason.Misc);
        if (sendStatementsSkipped == null)
        {
            //Insert new item.
            sendStatementsSkipped = new SendStatementsSkipped();
            sendStatementsSkipped.PatNum = 0;
            sendStatementsSkipped.Reason = SkipReason.Misc;
            sendStatementsSkipped.Error = error;
            ListSkippedPatients.Add(sendStatementsSkipped);
        }
        else if (!sendStatementsSkipped.Error.Contains(error))
        {
            //Update existing item.
            sendStatementsSkipped.Error += "; " + error;
        }
    }

    ///<summary>Update overall progress of all statement processing. Also fires current batch progress.</summary>
    public void FireOverallProgress()
    {
        ActionProgressEvent?.Invoke(new ODEventArgs(ODEventType.Billing,
            new ProgressBarHelper(
                labelValue: Lans.g("FormBilling", "Overall"),
                percentValue: Math.Ceiling(((double) CurStatementIdx / ListStatementNumsToSend.Count) * 100) + "%",
                blockValue: CurStatementIdx,
                blockMax: ListStatementNumsToSend.Count,
                progressStyle: ProgBarStyle.Blocks,
                tagString: "1")));
        ActionProgressEvent?.Invoke(new ODEventArgs(ODEventType.Billing,
            new ProgressBarHelper(
                labelValue: Lans.g(this, "Batch") + "\r\n" + CurStatementBatch.BatchNum + " / " + ListStatementBatches.Count,
                percentValue: Math.Ceiling(((double) CurStatementBatch.CountStatementsProcessed / CurStatementBatch.ListStatements.Count) * 100) + "%",
                blockValue: CurStatementBatch.CountStatementsProcessed,
                blockMax: CurStatementBatch.ListStatements.Count,
                progressStyle: ProgBarStyle.Blocks,
                tagString: "2")));
    }

    ///<summary>Just a rough estimate of percentage complete the current statment happens to be. 
    ///Blame Allen Job 819 : Progress bar for statements, 9/1/2016.</summary>
    public void FireStatementProgress(int fakePercentage)
    {
        ActionProgressEvent?.Invoke(new ODEventArgs(ODEventType.Billing,
            new ProgressBarHelper(
                labelValue: Lans.g("FormBilling", "Statement") + "\r\n" + CurStatementIdx + " / " + ListStatementNumsToSend.Count,
                percentValue: Math.Ceiling(((double) fakePercentage / 100) * 100) + "%",
                blockValue: fakePercentage,
                blockMax: 100,
                progressStyle: ProgBarStyle.Blocks,
                tagString: "3")));
    }

    ///<summary>Statement progress gets hijacked to indicate pdf page creation.</summary>
    public void FirePdfProgress(int pageIndex, int totalPageCount)
    {
        //Start with 15% base and add percentage of pages complete.
        int percentComplete = ((pageIndex / totalPageCount) * 85) + 15;
        ActionProgressEvent?.Invoke(new ODEventArgs(ODEventType.Billing,
            new ProgressBarHelper(
                labelValue: Lans.g("FormBilling", "Statement") + "\r\n" + CurStatementIdx + " / " + ListStatementNumsToSend.Count,
                percentValue: percentComplete + "%",
                blockValue: percentComplete,
                blockMax: 100,
                progressStyle: ProgBarStyle.Blocks,
                tagString: "3")));
    }

    ///<summary>Update progress bar text only. Does not update progress blocks.</summary>
    public void FireTextMsgProgress(string labelValue, bool isWarningOffEvent = false)
    {
        ActionProgressEvent?.Invoke(new ODEventArgs(ODEventType.Billing, new ProgressBarHelper(labelValue, progressBarEventType: isWarningOffEvent ? ProgBarEventType.WarningOff : ProgBarEventType.TextMsg)));
    }

    ///<summary>Helper method to log message to logger file for Statement action type.</summary>
    public void LogWrite(string logMsg, LogLevel logLevel)
    {
        LogWriter?.WriteLine(logMsg, logLevel, "Statements");
    }

    #endregion
}

///<summary>Used for billing to group statements by guarantor ClinicNum. BatchNum is 1-based and insignificant.</summary>
public class StatementBatch
{
    ///<summary>1-based batch num for displaying in UI. Typically assigned by Statements.GetBatchesForStatements.</summary>
    public int BatchNum;

    ///<summary>When clincs turned off, always 0. When clinics turned on, the clinicnum of the guarantor of the statements' patients.</summary>
    public long ClinicNum;

    ///<summary>All statements for this batch. Comes from a variety of patient, but all patients' guarantor will share same ClinicNum.</summary>
    public List<Statement> ListStatements = [];

    ///<summary>Tracks statements that have been processed in this batch and events progress back to UI. Processed does not mean success, just means dealt with.</summary>
    public int CountStatementsProcessed;

    ///<summary>Statement data that is retained throughout each batch. Will be used to sync statement data back to StatementProd table once batch has completed.</summary>
    public List<StatementData> ListStatementDatas = [];

    ///<summary>Statements with StatementMode.Electronic are added to this list as statements are added to print list. 
    ///These electronic statements will then be sent electronically via the proper eBilling vendor.</summary>
    public List<EbillStatement> ListEbillStatements = [];
}

///<summary>Used in place of tuple transport save file selection back from UI to BillingL.</summary>
public class ChooseSaveFile
{
    public string FileName;
    public bool IsSelectionOk;
}