using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.AutoComm;
using OpenDentBusiness.Crud;
using OpenDentBusiness.FileIO;
using OpenDentBusiness.SheetFramework;

namespace OpenDentBusiness;


public class Statements
{
    #region Get Methods

    ///<Summary>Gets one statement from the database.</Summary>
    public static Statement GetStatement(long statementNum)
    {
        return StatementCrud.SelectOne(statementNum);
    }

    ///<summary>Gets a list of statements optionally filtered for the API. Returns an empty list if not found.</summary>
    public static List<Statement> GetStatementsForApi(int limit, int offset, long patNum)
    {
        var command = "SELECT * FROM statement ";
        if (patNum > 0) command += "WHERE PatNum=" + SOut.Long(patNum) + " ";
        command += "ORDER BY StatementNum " //Ensure order for limit and offset.
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return StatementCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets a list of statements based on the passed in primary keys. If clinics are enabled and the preference
    ///     PrintStatementsAlphabetically is set, the statements will be sorted by patients last name then first name.
    ///     Otherwise statements will be ordered in the order of the listStatementNums passed in.
    /// </summary>
    public static List<Statement> GetStatements(List<long> listStatementNums)
    {
        if (listStatementNums == null || listStatementNums.Count < 1) return new List<Statement>();
        //Sort by patient num pref only valid when clinics turned on.
        var sortByPatientName = true && PrefC.GetBool(PrefName.PrintStatementsAlphabetically);
        var command = "SELECT * FROM statement ";
        if (sortByPatientName) command += "INNER JOIN patient ON patient.PatNum = statement.PatNum ";
        command += "WHERE StatementNum IN (" + string.Join(",", listStatementNums) + ") ";
        if (sortByPatientName) command += "ORDER BY patient.LName,patient.FName";
        var listStatements = StatementCrud.SelectMany(command);
        //If clinics are enabled, the practice has the option to order statements alphabetically.
        //For other cases, we are going to order the statements the way they are displayed in the grid.
        if (!sortByPatientName) listStatements = listStatements.OrderBy(x => listStatementNums.IndexOf(x.StatementNum)).ToList();
        return listStatements;
    }

    ///<summary>For orderBy, use 0 for BillingType and 1 for PatientName.</summary>
    public static DataTable GetBilling(bool isSent, int orderBy, DateTime dateFrom, DateTime dateTo, List<long> listClinicNums)
    {
        var table = new DataTable();
        DataRow dataRow;
        //columns that start with lowercase are altered for display rather than being raw data.
        table.Columns.Add("amountDue");
        table.Columns.Add("balTotal");
        table.Columns.Add("billingType");
        table.Columns.Add("insEst");
        table.Columns.Add("IsSent");
        table.Columns.Add("lastStatement");
        table.Columns.Add("mode");
        table.Columns.Add("name");
        table.Columns.Add("PatNum");
        table.Columns.Add("payPlanDue");
        table.Columns.Add("StatementNum");
        table.Columns.Add("SuperFamily");
        table.Columns.Add("ClinicNum");
        var command = "SELECT guar.BalTotal,patient.BillingType,patient.FName,guar.InsEst,statement.IsSent,"
                      + "IFNULL(MAX(s2.DateSent)," + SOut.Date(DateTime.MinValue) + ") LastStatement,"
                      + "patient.LName,patient.MiddleI,statement.Mode_,guar.PayPlanDue,patient.Preferred,"
                      + "statement.PatNum,statement.StatementNum,statement.SuperFamily,patient.ClinicNum "
                      + "FROM statement "
                      + "LEFT JOIN patient ON statement.PatNum=patient.PatNum "
                      + "LEFT JOIN patient guar ON guar.PatNum=patient.Guarantor "
                      + "LEFT JOIN statement s2 ON s2.PatNum=patient.PatNum "
                      + "AND s2.IsSent=1 ";
        if (PrefC.GetBool(PrefName.BillingIgnoreInPerson)) command += "AND s2.Mode_ !=1 ";
        if (orderBy == 0) //BillingType
            command += "LEFT JOIN definition ON patient.BillingType=definition.DefNum ";
        command += "WHERE statement.IsSent=" + SOut.Bool(isSent) + " ";
        //if(dateFrom.Year>1800){
        command += "AND statement.DateSent>=" + SOut.Date(dateFrom) + " "; //greater than midnight this morning
        //}
        //if(dateFrom.Year>1800){
        command += "AND statement.DateSent<" + SOut.Date(dateTo.AddDays(1)) + " "; //less than midnight tonight
        //}
        if (listClinicNums.Count > 0) command += "AND patient.ClinicNum IN (" + string.Join(",", listClinicNums) + ") ";
        command += "GROUP BY guar.BalTotal,patient.BillingType,patient.FName,guar.InsEst,statement.IsSent,"
                   + "patient.LName,patient.MiddleI,statement.Mode_,guar.PayPlanDue,patient.Preferred,"
                   + "statement.PatNum,statement.StatementNum,statement.SuperFamily ";
        if (orderBy == 0) //BillingType
            command += "ORDER BY definition.ItemOrder,patient.LName,patient.FName,patient.MiddleI,guar.PayPlanDue,statement.StatementNum";
        else
            command += "ORDER BY patient.LName,patient.FName,statement.StatementNum";
        var tableRaw = DataCore.GetTable(command);
        double balTotal;
        double insEst;
        double payPlanDue;
        DateTime dateLastStatement;
        List<Patient> listPatientsFamilyGuarantors;
        for (var i = 0; i < tableRaw.Rows.Count; i++)
        {
            dataRow = table.NewRow();
            if (tableRaw.Rows[i]["SuperFamily"].ToString() == "0")
            {
                //not a super statement, just get bal info from guarantor
                balTotal = SIn.Double(tableRaw.Rows[i]["BalTotal"].ToString());
                insEst = SIn.Double(tableRaw.Rows[i]["InsEst"].ToString());
                payPlanDue = SIn.Double(tableRaw.Rows[i]["PayPlanDue"].ToString());
            }
            else
            {
                //super statement, add all guar positive balances to get bal total for super family
                listPatientsFamilyGuarantors = Patients.GetSuperFamilyGuarantors(SIn.Long(tableRaw.Rows[i]["SuperFamily"].ToString())).FindAll(x => x.HasSuperBilling);
                //exclude fams with neg balances in the total for super family stmts (per Nathan 5/25/2016)
                if (PrefC.GetBool(PrefName.BalancesDontSubtractIns))
                {
                    listPatientsFamilyGuarantors = listPatientsFamilyGuarantors.FindAll(x => x.BalTotal > 0);
                    insEst = 0;
                }
                else
                {
                    listPatientsFamilyGuarantors = listPatientsFamilyGuarantors.FindAll(x => x.BalTotal - x.InsEst > 0);
                    insEst = listPatientsFamilyGuarantors.Sum(x => x.InsEst);
                }

                balTotal = listPatientsFamilyGuarantors.Sum(x => x.BalTotal);
                payPlanDue = listPatientsFamilyGuarantors.Sum(x => x.PayPlanDue);
            }

            dataRow["amountDue"] = (balTotal - insEst).ToString("F");
            dataRow["balTotal"] = balTotal.ToString("F");
            ;
            dataRow["billingType"] = Defs.GetName(DefCat.BillingTypes, SIn.Long(tableRaw.Rows[i]["BillingType"].ToString()));
            if (insEst == 0)
                dataRow["insEst"] = "";
            else
                dataRow["insEst"] = insEst.ToString("F");
            dataRow["IsSent"] = tableRaw.Rows[i]["IsSent"].ToString();
            dateLastStatement = SIn.Date(tableRaw.Rows[i]["LastStatement"].ToString());
            if (dateLastStatement.Year < 1880)
                dataRow["lastStatement"] = "";
            else
                dataRow["lastStatement"] = dateLastStatement.ToShortDateString();
            dataRow["mode"] = Lans.g("enumStatementMode", ((StatementMode) SIn.Int(tableRaw.Rows[i]["Mode_"].ToString())).ToString());
            dataRow["name"] = Patients.GetNameLF(tableRaw.Rows[i]["LName"].ToString(), tableRaw.Rows[i]["FName"].ToString(), tableRaw.Rows[i]["Preferred"].ToString(), tableRaw.Rows[i]["MiddleI"].ToString());
            dataRow["PatNum"] = tableRaw.Rows[i]["PatNum"].ToString();
            if (payPlanDue == 0)
                dataRow["payPlanDue"] = "";
            else
                dataRow["payPlanDue"] = payPlanDue.ToString("F");
            dataRow["StatementNum"] = tableRaw.Rows[i]["StatementNum"].ToString();
            dataRow["SuperFamily"] = tableRaw.Rows[i]["SuperFamily"].ToString();
            dataRow["ClinicNum"] = tableRaw.Rows[i]["ClinicNum"].ToString();
            table.Rows.Add(dataRow);
        }

        return table;
    }

    ///<summary>This query is flawed.</summary>
    public static DataTable GetStatementNotesPracticeWeb(long patnum)
    {
        var command = @"SELECT Note FROM statement Where Patnum=" + patnum;
        return DataCore.GetTable(command);
    }

    ///<summary>This query is flawed.</summary>
    public static Statement GetStatementInfoPracticeWeb(long patnum)
    {
        var command = @"Select SinglePatient,DateRangeFrom,DateRangeTo,Intermingled FROM statement WHERE PatNum = " + patnum;
        return StatementCrud.SelectOne(command);
    }

    /// <summary>
    ///     Fetches StatementNums restricted by the DateTStamp, PatNums and a limit of records per patient. If
    ///     limitPerPatient is zero all StatementNums of a patient are fetched
    /// </summary>
    public static List<long> GetChangedSinceStatementNums(DateTime dateChangedSince, List<long> listPatnumsEligibleForUpload, int limitPerPatient)
    {
        var listStatementNums = new List<long>();
        var strLimit = "";
        if (limitPerPatient > 0) strLimit = "LIMIT " + limitPerPatient;
        DataTable table;
        // there are possibly more efficient ways to implement this using a single sql statement but readability of the sql can be compromised
        if (listPatnumsEligibleForUpload.Count > 0)
            for (var i = 0; i < listPatnumsEligibleForUpload.Count; i++)
            {
                var command = "SELECT StatementNum FROM statement WHERE DateTStamp > " + SOut.DateT(dateChangedSince) + " AND PatNum='"
                              + listPatnumsEligibleForUpload[i] + "' ORDER BY DateSent DESC, StatementNum DESC " + strLimit;
                table = DataCore.GetTable(command);
                for (var j = 0; j < table.Rows.Count; j++) listStatementNums.Add(SIn.Long(table.Rows[j]["StatementNum"].ToString()));
            }

        return listStatementNums;
    }

    ///<summary>Used along with GetChangedSinceStatementNums</summary>
    public static List<Statement> GetMultStatements(List<long> listStatementNums)
    {
        var strStatementNums = "";
        DataTable table;
        if (listStatementNums.Count > 0)
        {
            for (var i = 0; i < listStatementNums.Count; i++)
            {
                if (i > 0) strStatementNums += "OR ";
                strStatementNums += "StatementNum='" + listStatementNums[i] + "' ";
            }

            var command = "SELECT * FROM statement WHERE " + strStatementNums;
            table = DataCore.GetTable(command);
        }
        else
        {
            table = new DataTable();
        }

        var listStatements = StatementCrud.TableToList(table);
        return listStatements;
    }

    ///<summary>Returns an email message for the patient based on the statement passed in.</summary>
    public static EmailMessage GetEmailMessageForStatement(Statement statement, Patient patient, EmailAddress fromAddress = null)
    {
        if (statement.PatNum != patient.PatNum)
        {
            var logMsg = Lans.g("Statements", "Mismatched PatNums detected between current patient and current statement:") + "\r\n"
                                                                                                                            + Lans.g("Statements", "Statement PatNum:") + " " + statement.PatNum + " " + Lans.g("Statements", "(assumed correct)") + "\r\n"
                                                                                                                            + Lans.g("Statements", "Patient PatNum:") + " " + patient.PatNum + " " + Lans.g("Statements", "(possibly incorrect)");
            SecurityLogs.MakeLogEntry(EnumPermType.StatementPatNumMismatch, statement.PatNum, logMsg, LogSources.Diagnostic);
        }

        var emailMessage = new EmailMessage();
        emailMessage.PatNum = patient.PatNum;
        emailMessage.ToAddress = patient.Email;
        var emailAddress = fromAddress;
        if (emailAddress == null) emailAddress = EmailAddresses.GetByClinic(patient.ClinicNum);
        emailMessage.FromAddress = EmailAddresses.OverrideSenderAddressClinical(emailAddress, patient.ClinicNum).GetFrom();
        string str;
        if (statement.EmailSubject != null && statement.EmailSubject != "")
            str = statement.EmailSubject; //Set str to the email subject if one was already set.
        else //Subject was not set.  Set str to the default billing email subject.
            str = PrefC.GetString(PrefName.BillingEmailSubject);
        emailMessage.Subject = new MsgToPayTagReplacer().ReplaceTagsForStatement(str, patient, statement, isEmail: true);
        if (statement.EmailBody != null && statement.EmailBody != "")
            str = statement.EmailBody; //Set str to the email body if one was already set.
        else //Body was not set.  Set str to the default billing email body text.
            str = PrefC.GetString(PrefName.BillingEmailBodyText);
        emailMessage.BodyText = new MsgToPayTagReplacer().ReplaceTagsForStatement(str, patient, statement, isEmail: true);
        emailMessage.MsgType = EmailMessageSource.Statement;
        return emailMessage;
    }

    public static EmailMessage GetEmailMessageForPortalStatement(Statement statement, Patient patient)
    {
        if (statement.PatNum != patient.PatNum)
        {
            var logMsg = Lans.g("Statements", "Mismatched PatNums detected between current patient and current statement:") + "\r\n"
                                                                                                                            + Lans.g("Statements", "Statement PatNum:") + " " + statement.PatNum + " " + Lans.g("Statements", "(assumed correct)") + "\r\n"
                                                                                                                            + Lans.g("Statements", "Patient PatNum:") + " " + patient.PatNum + " " + Lans.g("Statements", "(possibly incorrect)");
            SecurityLogs.MakeLogEntry(EnumPermType.StatementPatNumMismatch, statement.PatNum, logMsg, LogSources.Diagnostic);
        }

        var emailMessage = new EmailMessage();
        emailMessage.PatNum = patient.PatNum;
        emailMessage.ToAddress = patient.Email;
        var emailAddress = EmailAddresses.GetByClinic(patient.ClinicNum);
        emailMessage.FromAddress = EmailAddresses.OverrideSenderAddressClinical(emailAddress, patient.ClinicNum).GetFrom();
        string emailBody;
        if (statement.EmailSubject != null && statement.EmailSubject != "")
            emailMessage.Subject = statement.EmailSubject;
        else //Subject was not preset, set a default subject.
            emailMessage.Subject = Lans.g("Statements", "New Statement Available");
        if (statement.EmailBody != null && statement.EmailBody != "")
            emailBody = statement.EmailBody;
        else //Body was not preset, set a body text.
            emailBody = Lans.g("Statements", "Dear") + " [nameFnoPref],\r\n\r\n"
                                                     + Lans.g("Statements", "A new account statement is available.") + "\r\n\r\n"
                                                     + Lans.g("Statements", "To view your account statement, log on to our portal by following these steps:") + "\r\n\r\n"
                                                     + Lans.g("Statements", "1. Visit the following URL in a web browser:") + " " + PrefC.GetString(PrefName.PatientPortalURL) + "\r\n"
                                                     + Lans.g("Statements", "2. Enter your credentials to gain access to your account.") + "\r\n"
                                                     + Lans.g("Statements", "3. Click the Account icon on the left and select the most recent Statement to view.");
        emailMessage.BodyText = new MsgToPayTagReplacer().ReplaceTagsForStatement(emailBody, patient, statement, isEmail: true);
        emailMessage.MsgType = EmailMessageSource.Statement;
        return emailMessage;
    }

    ///<summary>Gets a list of unsent StatementNums.</summary>
    public static List<long> GetUnsentStatements(params StatementMode[] statementModeArray)
    {
        var command = "SELECT StatementNum FROM statement WHERE IsSent=0 ";
        if (statementModeArray.Length != 0) command += $"AND Mode_ IN({string.Join(",", statementModeArray.Select(x => SOut.Enum(x)))})";
        return Db.GetListLong(command);
    }

    #endregion

    #region Insert

    
    public static long Insert(Statement statement)
    {
        return StatementCrud.Insert(statement);
    }

    
    public static void InsertMany(List<Statement> listStatements)
    {
        if (listStatements == null || listStatements.Count == 0) return;
        StatementCrud.InsertMany(listStatements);
    }

    #endregion

    #region Update

    ///<summary>Updates the statements with the send status.</summary>
    public static void UpdateSmsSendStatus(List<long> listStmtNumsToUpdate, AutoCommStatus autoCommStatus)
    {
        if (listStmtNumsToUpdate.Count == 0) return;

        var command = "UPDATE statement SET SmsSendStatus=" + SOut.Int((int) autoCommStatus)
                                                            + " WHERE StatementNum IN(" + string.Join(",", listStmtNumsToUpdate.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    
    public static void Update(Statement statement)
    {
        StatementCrud.Update(statement);
    }

    
    public static bool Update(Statement statement, Statement statementOld)
    {
        return StatementCrud.Update(statement, statementOld);
    }

    public static void MarkSent(long statementNum, DateTime dateSent)
    {
        var command = "UPDATE statement SET DateSent=" + SOut.Date(dateSent) + ", "
                      + "IsSent=1 WHERE StatementNum=" + SOut.Long(statementNum);
        Db.NonQ(command);
    }

    public static void AttachDoc(long statementNum, Document document, bool doUpdateDoc = true)
    {
        if (doUpdateDoc) Documents.Update(document);
        var command = "UPDATE statement SET DocNum=" + SOut.Long(document.DocNum)
                                                     + " WHERE StatementNum=" + SOut.Long(statementNum);
        Db.NonQ(command);
    }

    public static void DetachDocFromStatements(long docNum)
    {
        if (docNum == 0) return; //Avoid MiddleTier.

        Db.NonQ("UPDATE statement SET DocNum=0 WHERE DocNum=" + SOut.Long(docNum));
    }

    ///<summary>Changes the value of the DateTStamp column to the current time stamp for all statements of a patient</summary>
    public static void ResetTimeStamps(long patNum)
    {
        var command = "UPDATE statement SET DateTStamp = CURRENT_TIMESTAMP WHERE PatNum =" + SOut.Long(patNum);
        Db.NonQ(command);
    }

    #endregion

    #region Delete

    /// <summary>
    ///     Deletes the passed in list of statements. Checks for permission before deleting the stored image in ODI folder. Can
    ///     force to delete the
    ///     stored image without the permission check. Will always delete the statement object. Throws UE.
    /// </summary>
    public static void DeleteStatements(List<Statement> listStatements, bool forceImageDelete = false)
    {
        if (listStatements.IsNullOrEmpty()) return;
        for (var i = 0; i < listStatements.Count; i++)
        {
            //Per Nathan the image should not be deleted if the user does not have the Image Delete permission. The statement can still be deleted.
            if (forceImageDelete)
                DeleteStatementDocument(listStatements[i]);
            else
                DeleteStatementDocumentIfAuthorized(listStatements[i]);
            Delete(listStatements[i]);
        }
    }

    private static void DeleteStatementDocumentIfAuthorized(Statement statement)
    {
        if (!Security.IsAuthorized(EnumPermType.ImageDelete, statement.DateSent, true)) return; //User not allowed to delete document due to missing permission or date restriction.
        var document = Documents.GetByNum(statement.DocNum, true);
        if (document == null) return; //No document to delete.
        if (!document.Signature.IsNullOrEmpty() && !Security.IsAuthorized(EnumPermType.SignedImageEdit, true)) return; //User can't delete signed documents.
        DeleteStatementDocument(statement, document);
    }

    /// <summary>
    ///     Will use passed in document if not null. Otherwise, queries for document. You probably want to use
    ///     DeleteStatementDocumentIfAuthorized(). This should only be used if the calling method must have the option to force
    ///     document deletion, like DeleteStatements() does.
    /// </summary>
    private static void DeleteStatementDocument(Statement statement, Document document = null)
    {
        if (document == null) document = Documents.GetByNum(statement.DocNum, true);
        if (document == null) return; //If it is still null we have nothing to delete.
        var patient = Patients.GetPat(statement.PatNum);
        var patFolder = ImageStore.GetPatientFolder(patient, ImageStore.GetPreferredAtoZpath());
        ImageStore.DeleteDocuments(new List<Document> {document}, patFolder);
    }

    
    public static void Delete(Statement statement)
    {
        Delete(statement.StatementNum);
    }

    ///<summary>For deleting a statement when user clicks Cancel.  No need to make entry in DeletedObject table.</summary>
    public static void Delete(long statementNum)
    {
        DeleteAll(new List<long> {statementNum});
    }

    public static void DeleteAll(List<long> listStatementNums)
    {
        if (listStatementNums == null || listStatementNums.Count == 0) return;
        //Removed all linked dependencies from these statements.
        StmtLinks.DetachAllFromStatements(listStatementNums);
        var command = DbHelper.WhereIn("UPDATE procedurelog SET StatementNum=0 WHERE StatementNum IN ({0})", false, listStatementNums.Select(x => SOut.Long(x)).ToList());
        Db.NonQ(command);
        command = DbHelper.WhereIn("UPDATE adjustment SET StatementNum=0 WHERE StatementNum IN({0})", false, listStatementNums.Select(x => SOut.Long(x)).ToList());
        Db.NonQ(command);
        command = DbHelper.WhereIn("UPDATE payplancharge SET StatementNum=0 WHERE StatementNum IN({0})", false, listStatementNums.Select(x => SOut.Long(x)).ToList());
        Db.NonQ(command);
        command = DbHelper.WhereIn("DELETE FROM statement WHERE StatementNum IN ({0})", false, listStatementNums.Select(x => SOut.Long(x)).ToList());
        Db.NonQ(command);
    }

    #endregion

    #region Misc Methods

    ///<summary>Queries the database to determine if there are any unsent statements.</summary>
    public static bool UnsentStatementsExist()
    {
        var command = "SELECT COUNT(*) FROM statement WHERE IsSent=0";
        if (Db.GetCount(command) == "0") return false;
        return true;
    }

    ///<summary>Queries the database to determine if there are any unsent statements for a particular clinic.</summary>
    public static bool UnsentClinicStatementsExist(long clinicNum)
    {
        if (clinicNum == 0) //All clinics.
            return UnsentStatementsExist();
        var command = @"SELECT COUNT(*) FROM statement 
				LEFT JOIN patient ON statement.PatNum=patient.PatNum
				WHERE statement.IsSent=0
				AND patient.ClinicNum=" + clinicNum;
        if (Db.GetCount(command) == "0") return false;
        return true;
    }

    /// <summary>
    ///     Allows an email receipt to be sent to a patient through the portal.  Throws exceptions in the case where the
    ///     email from address is not valid. Sends a seperate email if the document was unable to be created.
    /// </summary>
    public static void EmailStatementPatientPortal(Statement statement, string toAddress, EmailAddress emailAddressFrom, Patient patient)
    {
        //Create the Statement Object
        var dataSet = AccountModules.GetAccount(statement.PatNum, statement, doShowHiddenPaySplits: statement.IsReceipt);
        var patientGuar = Patients.GetPat(statement.PatNum);
        CalcBalTotalInsEst(statement, dataSet, patient, patientGuar);
        Insert(statement);
        //Create the .pdf file
        var sheetDef = SheetUtil.GetStatementSheetDef(statement);
        var sheet = SheetUtil.CreateSheet(sheetDef, statement.PatNum, statement.HidePayment);
        sheet.Parameters.Add(new SheetParameter(true, "Statement") {ParamValue = statement});
        SheetFiller.FillFields(sheet, dataSet, statement);
        SheetUtil.CalculateHeights(sheet, dataSet, statement);
        var tempPath = ODFileUtils.CombinePaths(PrefC.GetTempFolderPath(), statement.PatNum + ".pdf");
        var sheetDrawingJob = new SheetDrawingJob();
        var pdfDocument = sheetDrawingJob.CreatePdf(sheet, statement, dataSet: dataSet);
        long category = 0;
        //Find the image category for statements
        var listDefs = Defs.GetDefsForCategory(DefCat.ImageCats, true);
        for (var i = 0; i < listDefs.Count; i++)
            if (Regex.IsMatch(listDefs[i].ItemValue, @"S"))
            {
                category = listDefs[i].DefNum;
                break;
            }

        if (category == 0) category = listDefs[0].DefNum; //put it in the first category.
        //create doc--------------------------------------------------------------------------------------
        Document document = null;
        var errorMessage = "";
        try
        {
            SheetDrawingJob.SavePdfToFile(pdfDocument, tempPath);
            document = ImageStore.Import(tempPath, category, Patients.GetPat(statement.PatNum));
        }
        catch
        {
            errorMessage = "Thank you for your recent payment. An error occurred when creating your receipt, please contact your provider to request a copy.";
            var emailMessageError = GetEmailMessageForStatement(statement, Patients.GetPat(statement.PatNum));
            emailMessageError.ToAddress = toAddress;
            emailMessageError.MsgType = EmailMessageSource.PaymentReceipt;
            emailMessageError.SentOrReceived = EmailSentOrReceived.Sent;
            emailMessageError.MsgDateTime = DateTime_.Now;
            emailMessageError.BodyText = errorMessage;
            if (emailAddressFrom == null || EmailAddresses.GetValidMailAddress(emailAddressFrom.GetFrom()) == null) //Check to make sure that our "from" email address is valid.
                throw new ODException("Thank you for your recent payment. An error occurred when attempting to email your receipt,"
                                      + " please contact your provider.", ODException.ErrorCodes.ReceiptEmailAddressInvalid);
            emailAddressFrom = EmailAddresses.OverrideSenderAddressClinical(emailAddressFrom, patient.ClinicNum); //Use clinic's Email Sender Address Override, if present
            try
            {
                EmailMessages.SendEmail(emailMessageError, emailAddressFrom);
            }
            catch (Exception ex)
            {
                throw new ODException(errorMessage);
            }

            return;
        }

        var guarFolder = ImageStore.GetPatientFolder(patientGuar, ImageStore.GetPreferredAtoZpath());
        var fileName = "";
        document.ImgType = ImageType.Document;
        document.Description = Lans.g("Statement", "Receipt");
        document.DateCreated = statement.DateSent;
        statement.DocNum = document.DocNum; //this signals the calling class that the pdf was created successfully.
        //Attach Doc to the statement in the DB
        AttachDoc(statement.StatementNum, document);
        //Doc fileName and Copy to emailAttach Folder
        var attachPath = EmailAttaches.GetAttachPath();
        fileName = DateTime.Now.ToString("yyyyMMdd") + DateTime.Now.TimeOfDay.Ticks + ODRandom.Next(1000) + ".pdf";
        var filePathAndName = FileAtoZ.CombinePaths(attachPath, fileName);
        FileAtoZ.Copy(ImageStore.GetFilePath(Documents.GetByNum(statement.DocNum), guarFolder), filePathAndName, FileAtoZSourceDestination.AtoZToAtoZ);
        if (emailAddressFrom == null || EmailAddresses.GetValidMailAddress(emailAddressFrom.GetFrom()) == null) //Check to make sure that our "from" email address is valid.
            throw new ODException("Thank you for your recent payment. An error occurred when attempting to email your receipt,"
                                  + " please refresh your page to view your statement.", ODException.ErrorCodes.ReceiptEmailAddressInvalid);
        //Create email and attachment objects
        var emailMessage = GetEmailMessageForStatement(statement, patientGuar);
        var emailAttach = new EmailAttach();
        emailAttach.DisplayedFileName = "Statement.pdf";
        emailAttach.ActualFileName = fileName;
        emailMessage.Attachments.Add(emailAttach);
        emailMessage.ToAddress = toAddress;
        emailMessage.MsgType = EmailMessageSource.PaymentReceipt;
        emailMessage.SentOrReceived = EmailSentOrReceived.Sent;
        emailMessage.MsgDateTime = DateTime_.Now;
        try
        {
            EmailMessages.SendEmail(emailMessage, emailAddressFrom);
        }
        catch (Exception ex)
        {
            throw new ODException("Thank you for your recent payment. An error occurred when attempting to send an email receipt,"
                                  + " please refresh your page to view your statement.", ODException.ErrorCodes.ReceiptEmailFailedToSend);
        }
    }

    public static Statement CreateReceiptStatement(Patient patient, StatementMode statementMode)
    {
        var statement = new Statement();
        statement.PatNum = patient.PatNum;
        statement.DateSent = DateTime.Today;
        statement.IsSent = true;
        statement.Mode_ = StatementMode.Email;
        statement.HidePayment = true;
        statement.Intermingled = PrefC.GetBool(PrefName.IntermingleFamilyDefault);
        statement.SinglePatient = !statement.Intermingled;
        statement.IsReceipt = true;
        statement.StatementType = StmtType.NotSet;
        statement.DateRangeFrom = DateTime.Today;
        statement.DateRangeTo = DateTime.Today;
        statement.Note = "";
        statement.NoteBold = "";
        //BalTotal and InsEst will be calculated immediately prior to insertion in db using CalcBalTotalInsEst
        return statement;
    }

    /// <summary>
    ///     If the statement does not have a short guid or URL, a call will be made to HQ to assign it one. The statement will
    ///     be updated
    ///     to the database.
    /// </summary>
    public static void AssignURLsIfNecessary(Statement statement, Patient patient)
    {
        if (!string.IsNullOrEmpty(statement.ShortGUID) && !string.IsNullOrEmpty(statement.StatementURL)) return;
        var listShortGuidResultsUrls = WebServiceMainHQProxy.GetShortGUIDs(1, 1, patient.ClinicNum, eServiceCode.PatientPortalViewStatement);
        var statementOld = statement.Copy();
        statement.ShortGUID = listShortGuidResultsUrls[0].ShortGuid;
        statement.StatementURL = listShortGuidResultsUrls[0].MediumURL;
        statement.StatementShortURL = listShortGuidResultsUrls[0].ShortURL;
        Update(statement, statementOld);
    }

    ///<summary>Assigns the given ShortGUID to the Statement with the given StatementNum</summary>
    public static void UpdateShortGUID(long statementNum, string shortGuid)
    {
        var command = "UPDATE statement SET ShortGUID='" + SOut.String(shortGuid) + "' WHERE StatementNum=" + SOut.Long(statementNum);
        Db.NonQ(command);
    }

    public static Statement CreateLimitedStatement(List<long> listPatNumsSelected, long patNum, List<long> listPayClaimNums, List<long> listAdjustments,
        List<long> listPayNums, List<long> listProcNums, long superFamily = 0,
        EnumLimitedCustomFamily limitedCustomFamily = EnumLimitedCustomFamily.None)
    {
        var statement = new Statement();
        statement.PatNum = patNum;
        statement.DateSent = DateTime.Today;
        statement.IsSent = false;
        statement.Mode_ = StatementMode.InPerson;
        statement.HidePayment = false;
        statement.SinglePatient = listPatNumsSelected.Count == 1; //SinglePatient determined by the selected transactions
        statement.Intermingled = listPatNumsSelected.Count > 1 && PrefC.GetBool(PrefName.IntermingleFamilyDefault);
        statement.IsReceipt = false;
        statement.IsInvoice = false;
        statement.StatementType = StmtType.LimitedStatement;
        statement.DateRangeFrom = DateTime.MinValue;
        statement.DateRangeTo = DateTime.Today;
        statement.Note = "";
        statement.NoteBold = "";
        statement.IsBalValid = true;
        statement.BalTotal = 0; //Value should be updated in FormStatementOptions
        statement.InsEst = 0; //Value should be updated in FormStatementOptions
        statement.SuperFamily = superFamily;
        statement.LimitedCustomFamily = limitedCustomFamily;
        Insert(statement); //we need stmt.StatementNum for attaching procs, adjustments, and paysplits to the statement
        for (var i = 0; i < listAdjustments.Count; i++)
        {
            var stmtLink = new StmtLink();
            stmtLink.FKey = listAdjustments[i];
            stmtLink.StatementNum = statement.StatementNum;
            stmtLink.StmtLinkType = StmtLinkTypes.Adj;
            StmtLinks.Insert(stmtLink);
        }

        for (var i = 0; i < listPayNums.Count; i++)
        {
            var payment = Payments.GetPayment(listPayNums[i]);
            var listPaySplits = PaySplits.GetForPayment(listPayNums[i]);
            for (var l = 0; l < listPaySplits.Count; l++)
            {
                var stmtLink = new StmtLink();
                stmtLink.FKey = listPaySplits[l].SplitNum;
                stmtLink.StatementNum = statement.StatementNum;
                stmtLink.StmtLinkType = StmtLinkTypes.PaySplit;
                StmtLinks.Insert(stmtLink);
            }
        }

        for (var i = 0; i < listProcNums.Count; i++)
        {
            var stmtLink = new StmtLink();
            stmtLink.FKey = listProcNums[i];
            stmtLink.StatementNum = statement.StatementNum;
            stmtLink.StmtLinkType = StmtLinkTypes.Proc;
            StmtLinks.Insert(stmtLink);
        }

        for (var i = 0; i < listPayClaimNums.Count; i++)
        {
            var stmtLink = new StmtLink();
            stmtLink.FKey = listPayClaimNums[i];
            stmtLink.StatementNum = statement.StatementNum;
            stmtLink.StmtLinkType = StmtLinkTypes.ClaimPay;
            StmtLinks.Insert(stmtLink);
        }

        if (limitedCustomFamily != EnumLimitedCustomFamily.None || listPatNumsSelected.Any(x => x != patNum))
            for (var i = 0; i < listPatNumsSelected.Count; i++)
            {
                var stmtLink = new StmtLink();
                stmtLink.FKey = listPatNumsSelected[i];
                stmtLink.StatementNum = statement.StatementNum;
                stmtLink.StmtLinkType = StmtLinkTypes.PatNum;
                StmtLinks.Insert(stmtLink);
            }

        //set statement lists to null in order to force refresh the lists now that we've inserted all of the StmtAttaches
        statement.ListAdjNums = null;
        statement.ListPaySplitNums = null;
        statement.ListProcNums = null;
        statement.ListInsPayClaimNums = null;
        statement.ListPatNums = null;
        return statement;
    }

    /// <summary>
    ///     Creates statement prods for a statement based off of the dataSet passed in, then syncs this list with the
    ///     existing statementprods for the statement in the DB.
    /// </summary>
    public static void SyncStatementProdsForStatement(DataSet dataSet, long statementNum, long docNum)
    {
        if (docNum == 0) return;
        StatementProds.SyncForStatement(dataSet, statementNum, docNum);
    }

    /// <summary>
    ///     Pass in a list of statement DataSets. Creates statement prods for the statements based off of their dataSets,
    ///     then syncs these statementprods with the existing statementprods for the statements in the DB.
    /// </summary>
    public static void SyncStatementProdsForMultipleStatements(List<StatementData> listStatementDatas)
    {
        StatementProds.SyncForMultipleStatements(listStatementDatas);
    }

    ///<summary>Sets the installment plans field on each of the statements passed in.</summary>
    public static void AddInstallmentPlansToStatements(List<Statement> listStatements, Dictionary<long, Family> dictFamilies = null)
    {
        if (listStatements.IsNullOrEmpty()) return;
        if (dictFamilies == null) dictFamilies = GetFamiliesForStatements(listStatements);
        var dictionarySuperFamInstallmentPlans = InstallmentPlans.GetForSuperFams(
            listStatements.Where(x => x.SuperFamily > 0)
                .Select(x => dictFamilies[x.PatNum].Guarantor.SuperFamily).ToList());
        var dictionaryFamInstallmentPlans = InstallmentPlans.GetForFams(
            listStatements.Where(x => x.SuperFamily == 0)
                .Select(x => dictFamilies[x.PatNum].Guarantor.PatNum).ToList());
        for (var i = 0; i < listStatements.Count; ++i)
        {
            if (listStatements[i].SuperFamily > 0)
            {
                if (!dictionarySuperFamInstallmentPlans.TryGetValue(dictFamilies[listStatements[i].PatNum].Guarantor.SuperFamily, out listStatements[i].ListInstallmentPlans)) listStatements[i].ListInstallmentPlans = new List<InstallmentPlan>();
                continue;
            }

            if (dictionaryFamInstallmentPlans.ContainsKey(dictFamilies[listStatements[i].PatNum].Guarantor.PatNum))
            {
                listStatements[i].ListInstallmentPlans = new List<InstallmentPlan> {dictionaryFamInstallmentPlans[dictFamilies[listStatements[i].PatNum].Guarantor.PatNum]};
                continue;
            }

            listStatements[i].ListInstallmentPlans = new List<InstallmentPlan>();
        }
    }

    ///<summary>Returns the family's balance according to the most recent statement across the entire family.</summary>
    public static double GetFamilyBalance(long patNum)
    {
        var listPatients = Patients.GetFamily(patNum).ListPats.ToList();
        var command = "SELECT * FROM statement "
                      + "WHERE PatNum IN(" + string.Join(",", listPatients.Select(x => SOut.Long(x.PatNum)).ToList()) + ") "
                      + "AND IsSent=1 "
                      + "ORDER BY StatementNum DESC LIMIT 1";
        var statement = StatementCrud.SelectOne(command);
        if (statement == null) return 0;
        return statement.BalTotal - statement.InsEst;
    }

    ///<summary>Returns a dictionary of Key-PatNum, Value-Family for the statements passed in.</summary>
    public static Dictionary<long, Family> GetFamiliesForStatements(List<Statement> listStatements)
    {
        if (listStatements.IsNullOrEmpty()) return new Dictionary<long, Family>();
        var dictionaryFamilyValues = Patients.GetFamilies(listStatements.Select(x => x.PatNum).ToList())
            .SelectMany(fam => fam.ListPats.Select(y => new {y.PatNum, fam}))
            .Distinct()
            .ToDictionary(x => x.PatNum, x => x.fam);
        return dictionaryFamilyValues;
    }

    ///<summary>List of batches of statements. BatchNum will start with 1 for UI.</summary>
    public static List<StatementBatch> GetBatchesForStatements(List<Statement> listStatements, List<Patient> listPatients)
    {
        var listBatchesOfStatements = new List<StatementBatch>();
        if (listStatements.IsNullOrEmpty()) return listBatchesOfStatements;
        //If clinics are on, batch by guarantor of statement's clinicnum. Otherwise, batch by BillingElectBatchMax pref.
        //3/1/24 SamO. Bug that I don't care to fix. When clinics are on, we don't enforce the BillingElectBatchMax batch size for each clinic.
        //So if a clinic has a batch of statements that is bigger than max allowed by that vendor (EDS), that is likely a problem. Otherwise why did we implement batches in the first place?
        if (true)
        {
            listBatchesOfStatements = listStatements.GroupBy(x =>
                {
                    //No need to test for null. Guaranteed to have this patient.
                    var patient = listPatients.Find(y => y.PatNum == x.PatNum);
                    //Find the guranantor clinicnum.
                    var patientGuarantor = listPatients.Find(x => x.PatNum == patient.Guarantor);
                    if (patientGuarantor == null) return patient.ClinicNum; //should never happen, every pat should have a guarantor (sometimes yourself)
                    return patientGuarantor.ClinicNum;
                })
                .Select((x, i) => new StatementBatch
                {
                    BatchNum = i + 1, //1-based batch num. For UI.
                    ClinicNum = x.Key, //We grouped by guar ClinicNum above.
                    ListStatements = x.ToList(), //The grouping of statements for this ClinicNum.
                    ListStatementDatas = new List<StatementData>(),
                    ListEbillStatements = new List<EbillStatement>()
                }).ToList();
            return listBatchesOfStatements;
        }

        var maxStmtsPerBatch = PrefC.GetInt(PrefName.BillingElectBatchMax);
        var electronicBillingType = PrefC.GetEnum<BillingUseElectronicEnum>(PrefName.BillingUseElectronic);
        if (maxStmtsPerBatch == 0 || electronicBillingType.In(BillingUseElectronicEnum.POS, BillingUseElectronicEnum.EDS)) //Max is disabled for Output to File billing option or using EDS.
            maxStmtsPerBatch = listStatements.Count; //Make the batch size equal to the list of statements so that we send them all at once.
        StatementBatch statementBatch = null;
        for (var i = 0; i < listStatements.Count; i++)
        {
            if (i % maxStmtsPerBatch == 0)
            {
                statementBatch = new StatementBatch
                {
                    BatchNum = listBatchesOfStatements.Count + 1, //1-based batch num. For UI.
                    ClinicNum = 0,
                    ListStatements = new List<Statement>(),
                    ListStatementDatas = new List<StatementData>(),
                    ListEbillStatements = new List<EbillStatement>()
                };
                listBatchesOfStatements.Add(statementBatch);
            }

            statementBatch.ListStatements.Add(listStatements[i]);
        }

        return listBatchesOfStatements;
    }

    /// <summary>
    ///     The filePath is the full path to the output file if the clinics feature is disabled (for a single location
    ///     practice).
    /// </summary>
    public static string GetEbillFilePathForClinic(string filePath, long clinicNum)
    {
        if (!true) return filePath;
        string clinicAbbr;
        //Check for zero clinic
        if (clinicNum == 0)
            clinicAbbr = Lans.g("SendEBills", "Unassigned");
        else
            clinicAbbr = Clinics.GetClinic(clinicNum).Abbr; //Abbr is required by our interface, so no need to check if blank.
        var fileName = Path.GetFileNameWithoutExtension(filePath) + '-' + clinicAbbr + Path.GetExtension(filePath);
        return ODFileUtils.CombinePaths(Path.GetDirectoryName(filePath), ODFileUtils.CleanFileName(fileName));
    }

    /// <summary>
    ///     Returns a list of failed messages. If list is empty then all messages succeeded.
    ///     Statement.TagOD must be set to SmsToMobile.GuidMessage before calling this method.
    /// </summary>
    public static List<SmsToMobile> HandleSmsSent(List<SmsToMobile> listSmsToMobiles, List<Statement> listStatements)
    {
        //WSHQ.SmsSend will only return FailNoCharge or Pending so we only need to handle those 2 cases here. FailWithCharge is impossible at this stage of the text message life.
        var listStatementNumsSuccess = listSmsToMobiles
            //SmsToMobile that were queued successfully. GuidMessage was boxed into Statement.TagOD by BillingL.
            .Where(x => x.SmsStatus != SmsDeliveryStatus.FailNoCharge && listStatements.Any(y => y.TagOD is string guidMessage && guidMessage == x.GuidMessage))
            //That correspond to a statement
            .Select(x => listStatements.Find(y => y.TagOD is string guidMessage && guidMessage == x.GuidMessage).StatementNum)
            .ToList();
        UpdateSmsSendStatus(listStatementNumsSuccess, AutoCommStatus.SendSuccessful);
        var listStatementNumsFailure = listSmsToMobiles
            //SmsToMobile that were queued successfully. GuidMessage was boxed into Statement.TagOD by BillingL.
            .Where(x => x.SmsStatus == SmsDeliveryStatus.FailNoCharge && listStatements.Any(y => y.TagOD is string guidMessage && guidMessage == x.GuidMessage))
            //That correspond to a statement
            .Select(x => listStatements.Find(y => y.TagOD is string guidMessage && guidMessage == x.GuidMessage).StatementNum)
            .ToList();
        UpdateSmsSendStatus(listStatementNumsFailure, AutoCommStatus.SendFailed);
        return listSmsToMobiles.FindAll(x => x.SmsStatus == SmsDeliveryStatus.FailNoCharge);
    }

    ///<summary>Returns the mode for the statement.</summary>
    public static StatementMode GetStatementMode(PatAging patAging)
    {
        StatementMode statementMode;
        if (PrefC.GetEnum<BillingUseElectronicEnum>(PrefName.BillingUseElectronic) == BillingUseElectronicEnum.None)
            statementMode = StatementMode.Mail;
        else
            statementMode = StatementMode.Electronic;
        var defBillingType = Defs.GetDef(DefCat.BillingTypes, patAging.BillingType);
        if (defBillingType != null && defBillingType.ItemValue == "E") statementMode = StatementMode.Email;
        return statementMode;
    }

    ///<summary>Returns true if the patient statement mode has an option to send by SMS.</summary>
    public static bool DoSendSms(PatAging patAging, Dictionary<long, PatAgingData> dictPatAgingData, List<StatementMode> listStatementModes)
    {
        PatAgingData patAgingData;
        dictPatAgingData.TryGetValue(patAging.PatNum, out patAgingData);
        if (listStatementModes.Contains(GetStatementMode(patAging))
            && patAgingData != null
            && patAgingData.PatComm != null
            && patAgingData.PatComm.IsSmsAnOption)
            return true;
        return false;
    }

    /// <summary>
    ///     Creates a new pdf, attaches it to a new doc, and attaches that to the statement.  If it cannot create a pdf, for
    ///     example if no AtoZ
    ///     folders, then it will simply result in a docnum of zero, so no attached doc. Only used for batch statment printing.
    ///     Returns the path of the
    ///     temp file where the pdf is saved.Temp file should be deleted manually.  Will return an empty string when unable to
    ///     create the file.
    /// </summary>
    public static string CreateStatementPdfSheets(Statement statement, Patient patient, Family family, DataSet dataSet)
    {
        var statementNew = statement;
        var sheetDef = SheetUtil.GetStatementSheetDef(statementNew);
        var sheet = SheetUtil.CreateSheet(sheetDef, statementNew.PatNum, statementNew.HidePayment);
        sheet.Parameters.Add(new SheetParameter(true, "Statement") {ParamValue = statementNew});
        SheetFiller.FillFields(sheet, dataSet, statementNew, patient: patient, family: family);
        SheetUtil.CalculateHeights(sheet, dataSet, statementNew, pat: patient, patGuar: family.Guarantor);
        var tempPath = ODFileUtils.CombinePaths(PrefC.GetTempFolderPath(), statementNew.PatNum + ".pdf");
        SheetPrinting.CreatePdf(sheet, tempPath, statementNew, dataSet, null, patient, family.Guarantor);
        var listDefsImageCat = Defs.GetDefsForCategory(DefCat.ImageCats, true);
        long category = 0;
        for (var i = 0; i < listDefsImageCat.Count; i++)
            if (Regex.IsMatch(listDefsImageCat[i].ItemValue, @"S"))
            {
                category = listDefsImageCat[i].DefNum;
                break;
            }

        if (category == 0) category = listDefsImageCat[0].DefNum; //put it in the first category.
        //create doc--------------------------------------------------------------------------------------
        Document document = null;
        try
        {
            document = ImageStore.Import(tempPath, category, patient);
        }
        catch
        {
            return ""; //Error saving the document
        }

        document.ImgType = ImageType.Document;
        if (statementNew.IsInvoice)
        {
            document.Description = Lans.g(nameof(Statements), "Invoice");
        }
        else
        {
            if (statementNew.IsReceipt)
                document.Description = Lans.g(nameof(Statements), "Receipt");
            else
                document.Description = Lans.g(nameof(Statements), "Statement");
        }

        statementNew.DocNum = document.DocNum; //this signals the calling class that the pdf was created successfully.
        AttachDoc(statementNew.StatementNum, document);
        return tempPath;
    }

    
    public static string SaveStatementAsCSV(Statement statement)
    {
        var statementCategory = Defs.GetImageCat(ImageCategorySpecial.S);
        var prependCategoryNum = "";
        if (statementCategory > 0)
            //Files that start with "_###_" will automatically have Document entries created for them when the Imaging module loads.
            prependCategoryNum = "_" + statementCategory + "_";
        var patient = Patients.GetPat(statement.PatNum);
        var patFolder = ImageStore.GetPatientFolder(patient, ImageStore.GetPreferredAtoZpath());
        var fileName = prependCategoryNum + patient.LName + patient.FName + statement.DocNum + ".csv";
        return WriteStatementToCSV(statement, fileName, patFolder);
    }

    
    private static string WriteStatementToCSV(Statement statement, string fileName, string filePath)
    {
        if (statement == null) return "";
        var path = FileAtoZ.CombinePaths(filePath, fileName);
        var dataSet = AccountModules.GetStatementDataSet(statement);
        var dataTable = SheetDataTableUtil.GetTable_StatementMain(dataSet, statement);
        var stringBuilderExportCSV = new StringBuilder();
        var stateNum = statement.StatementNum;
        stringBuilderExportCSV.AppendLine("Invoice/Statement Number,"
                                          + "Date Created,"
                                          + "Procedure Total,"
                                          + "Ins Estimate,"
                                          + "Total Amount,");
        stringBuilderExportCSV.AppendLine($"\"{stateNum}\","
                                          + $"\"{statement.DateSent.ToShortDateString()}\","
                                          + $"\"{statement.BalTotal}\","
                                          + $"\"{statement.InsEst}\","
                                          + $"\"{statement.BalTotal - statement.InsEst}\",");
        stringBuilderExportCSV.AppendLine("");
        stringBuilderExportCSV.AppendLine("");
        stringBuilderExportCSV.AppendLine("Date,"
                                          + "Patient Number,"
                                          + "Patient Name,"
                                          + "Code,"
                                          + "Description,"
                                          + "Charges,"
                                          + "Credits,"
                                          + "Balance");
        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            var patNum = SIn.Long(dataTable.Rows[i]["PatNum"].ToString());
            stringBuilderExportCSV.AppendLine($"\"{dataTable.Rows[i]["date"]}\","
                                              + $"\"{patNum}\","
                                              + $"\"{Patients.GetNameFL(patNum)}\","
                                              + $"\"{dataTable.Rows[i]["ProcCode"]}\","
                                              + $"\"{dataTable.Rows[i]["description"]}\","
                                              + $"\"{dataTable.Rows[i]["charges"]}\","
                                              + $"\"{dataTable.Rows[i]["credits"]}\","
                                              + $"\"{dataTable.Rows[i]["balance"]}\",");
        }

        File.WriteAllText(path, stringBuilderExportCSV.ToString());
        return path;
    }

    /// <summary>
    ///     Takes the passed in patient to create a statement for the guarantor. This logic used to just exist behind the
    ///     toolBarButStatement_Click in the account controller
    /// </summary>
    public static Statement GenerateStatement(Patient patient, DateTime dateStart, DateTime dateEnd, StatementMode statementMode, bool isSinglePatient = false)
    {
        var statement = new Statement();
        statement.PatNum = patient.Guarantor;
        if (isSinglePatient) statement.PatNum = patient.PatNum;
        statement.DateSent = DateTime.Today;
        statement.IsSent = true;
        statement.Mode_ = statementMode;
        statement.HidePayment = false;
        statement.SinglePatient = isSinglePatient;
        statement.Intermingled = PrefC.GetBool(PrefName.BillingDefaultsIntermingle);
        statement.StatementType = StmtType.NotSet;
        statement.DateRangeTo = DateTime.Today; //This is needed for payment plan accuracy.//new DateTime(2200,1,1);
        if (dateEnd != DateTime.MinValue) statement.DateRangeTo = dateEnd;
        statement.DateRangeFrom = DateTime.MinValue;
        if (dateStart != DateTime.MinValue)
        {
            //dateStart has ultimate precedence. User may have intentionally set the date range for statement.
            statement.DateRangeFrom = dateStart;
        }
        else
        {
            //Use preferences to determine the "from" date.
            var billingDefaultsLastDaysPref = PrefC.GetLong(PrefName.BillingDefaultsLastDays);
            if (billingDefaultsLastDaysPref > 0) //0 days means ignore preference and show everything.
                statement.DateRangeFrom = DateTime.Today.AddDays(-billingDefaultsLastDaysPref);
            if (PrefC.GetBool(PrefName.BillingShowTransSinceBalZero))
            {
                var patientForAging = Patients.GetPat(statement.PatNum);
                var listPatAgings = Patients.GetAgingListSimple(new List<long>(), new List<long> {patientForAging.Guarantor}, true);
                var tableBals = Ledgers.GetDateBalanceBegan(listPatAgings, false); //More Options selection has a super family option. We would need new checkbox here.
                if (tableBals.Rows.Count > 0)
                {
                    var dateTimeFrom = SIn.Date(tableBals.Rows[0]["DateZeroBal"].ToString());
                    if (dateTimeFrom > statement.DateRangeFrom) //Zero balance date range has precedence if it's more recent than billing default date range.
                        statement.DateRangeFrom = dateTimeFrom;
                }
            }
        }

        statement.Note = "";
        statement.NoteBold = "";
        Patient patientGuarantor = null;
        if (patient != null) patientGuarantor = Patients.GetPat(patient.Guarantor);
        if (patientGuarantor != null)
        {
            statement.IsBalValid = true;
            statement.BalTotal = patientGuarantor.BalTotal;
            statement.InsEst = patientGuarantor.InsEst;
        }

        return statement;
    }

    /// <summary>
    ///     Returns the PatNum of the patient that this statement is responsible for. Typically returns StatementCur.PatNum.
    ///     Can return a different PatNum if this is a SinglePatient statement and there is only one PatNum StmtLink associated
    ///     with this statement.
    /// </summary>
    public static long GetPatNumForGetAccount(Statement statement)
    {
        var patNum = statement.PatNum;
        if (statement.SinglePatient && statement.ListPatNums.Distinct().Count() == 1) patNum = statement.ListPatNums.First();
        return patNum;
    }

    ///<summary>Calculates and sets the BalTotal and InsEst fields on the passed-in Statement object.</summary>
    public static void CalcBalTotalInsEst(Statement statement, DataSet dataSet, Patient patient = null, Patient patientGuar = null)
    {
        if (patient == null) patient = Patients.GetPat(statement.PatNum);
        if (patientGuar == null) patientGuar = Patients.GetPat(patient.Guarantor);
        DataTable tableAcct;
        var tableMisc = dataSet.Tables["misc"];
        if (tableMisc == null) tableMisc = new DataTable();
        if (statement.IsInvoice)
        {
            double amtAdj = 0;
            double amtProc = 0;
            string tableName;
            for (var i = 0; i < dataSet.Tables.Count; i++)
            {
                tableAcct = dataSet.Tables[i];
                tableName = tableAcct.TableName;
                if (!tableName.StartsWith("account")) continue;
                for (var p = 0; p < tableAcct.Rows.Count; p++)
                    if (tableAcct.Rows[p]["AdjNum"].ToString() != "0")
                    {
                        amtAdj -= SIn.Double(tableAcct.Rows[p]["creditsDouble"].ToString());
                        amtAdj += SIn.Double(tableAcct.Rows[p]["chargesDouble"].ToString());
                    }
                    else if (tableAcct.Rows[p]["PayPlanChargeNum"].ToString() != "0")
                    {
                        //
                    }
                    else
                    {
                        //must be a procedure
                        amtProc += SIn.Double(tableAcct.Rows[p]["chargesDouble"].ToString());
                    }
            }

            statement.BalTotal = amtProc;
            statement.InsEst = amtAdj;
        }
        else if (statement.StatementType == StmtType.LimitedStatement)
        {
            var listDataRows = dataSet.Tables.OfType<DataTable>().Where(x => x.TableName.StartsWith("account")).SelectMany(x => x.Rows.OfType<DataRow>()).ToList();
            var isOnlyCharges = listDataRows.Where(x => x["PayPlanChargeNum"].ToString() != "0").Count() == listDataRows.Count();
            var balTotal = listDataRows.Where(x =>
                        x["AdjNum"].ToString() != "0" //adjustments, may be charges or credits
                        || x["ProcNum"].ToString() != "0" //procs, will be charges with credits==0
                        || x["PayNum"].ToString() != "0" //patient payments, will be credits with charges==0
                        || x["ClaimPaymentNum"].ToString() != "0" //claimproc payments+writeoffs, will be credits with charges==0
                        || (x["PayPlanChargeNum"].ToString() != "0" && isOnlyCharges) //pay plans, will be charges with credits==0
                )
                .ToList()
                .Sum(x => SIn.Double(x["chargesDouble"].ToString()) - SIn.Double(x["creditsDouble"].ToString())); //add charges-credits
            if (PrefC.GetBool(PrefName.BalancesDontSubtractIns))
            {
                statement.BalTotal = balTotal;
            }
            else
            {
                var amtPatInsEst = tableMisc.Rows.OfType<DataRow>()
                    .Where(x => x["descript"].ToString() == "patInsEst")
                    .Sum(x => SIn.Double(x["value"].ToString()));
                statement.BalTotal = balTotal;
                statement.InsEst = amtPatInsEst;
            }
        }
        else if (PrefC.GetBool(PrefName.BalancesDontSubtractIns))
        {
            if (statement.SinglePatient)
            {
                statement.BalTotal = patient.EstBalance;
            }
            else
            {
                if (statement.SuperFamily != 0) //Superfam statement
                    //If the family is included in superfamily billing, sum their totals into the running total.
                    //don't include families with negative balances in the total balance for super family (per Nathan 5/25/2016)
                    statement.BalTotal = Patients.GetSuperFamilyGuarantors(statement.SuperFamily).FindAll(x => x.HasSuperBilling && x.BalTotal > 0).Sum(x => x.BalTotal);
                else
                    //Show the current family's balance without subtracting insurance estimates.
                    statement.BalTotal = patientGuar.BalTotal;
            }
        }
        else
        {
            //more common
            if (statement.SinglePatient)
            {
                double amtPatInsEst = 0;
                for (var m = 0; m < tableMisc.Rows.Count; m++)
                    if (tableMisc.Rows[m]["descript"].ToString() == "patInsEst")
                        amtPatInsEst = SIn.Double(tableMisc.Rows[m]["value"].ToString());

                statement.BalTotal = patient.EstBalance;
                statement.InsEst = amtPatInsEst;
            }
            else
            {
                if (statement.SuperFamily != 0)
                {
                    //Superfam statement
                    var listFamilyGuarantors = Patients.GetSuperFamilyGuarantors(statement.SuperFamily).FindAll(x => x.HasSuperBilling && x.BalTotal - x.InsEst >= 0);
                    var balTotal = listFamilyGuarantors.Sum(x => x.BalTotal);
                    var amtInsEst = PrefC.GetBool(PrefName.BalancesDontSubtractIns) ? 0 : listFamilyGuarantors.Sum(x => x.InsEst);
                    statement.BalTotal = balTotal;
                    statement.InsEst = amtInsEst;
                }
                else
                {
                    statement.BalTotal = patientGuar.BalTotal;
                    statement.InsEst = patientGuar.InsEst;
                }
            }
        }

        //For payplan v1, add payplandue value to total balance in non-Limited superfamily statements (payplan v2+ already accounts for this when calculating aging).
        if (PrefC.GetInt(PrefName.PayPlansVersion) == 1 && statement.StatementType != StmtType.LimitedStatement && statement.SuperFamily != 0)
            for (var m = 0; m < tableMisc.Rows.Count; m++)
                if (tableMisc.Rows[m]["descript"].ToString() == "payPlanDue")
                    statement.BalTotal += SIn.Double(tableMisc.Rows[m]["value"].ToString());
        //payPlanDue;//PatGuar.PayPlanDue;
        statement.IsBalValid = true;
    }

    #endregion
}

///<summary>Holds all of the statement and StatementProd data relevant to syncing StatementProds and late charges.</summary>
[Serializable]
public class StatementData
{
	/// <summary>
	///     Specific tables and columns from the DataSet used to create the statement for inserting or syncing
	///     StatementProds.
	/// </summary>
	[XmlIgnore]
    public DataSet DataSetStmtNew;

    ///<summary>Date the statement was sent.</summary>
    public DateTime DateSent;

    ///<summary>The DocNum of the document associated to the statement.</summary>
    public long DocNum;

    ///<summary>True if the statement is a superfamily statement.</summary>
    public bool IsSuperFamilyStatement;

    /// <summary>
    ///     Holds the PatNums of all guarantors in super family if the statement is a super family statement, otherwise it
    ///     just holds the family's guarantor.
    /// </summary>
    public List<long> ListPatNumsGuarantor = new();

    ///<summary>The StatementProds associated to the statement.</summary>
    public List<StatementProd> ListStatementProds = new();

    /// <summary>
    ///     PatNum of the guarantor of the family that the statement is for or the PatNum of the SuperFamily head if the
    ///     statement is a SuperFamily statement.
    /// </summary>
    public long PatNumGuarantor;

    /// <summary>
    ///     Guarantor's primary provider's ProvNum or the SuperFamily head's primary provider's ProvNum if the statement
    ///     is a SuperFamily statement.
    /// </summary>
    public long ProvNumPriGuarantor;

    ///<summary>For serialization purposes.</summary>
    public StatementData()
    {
    }

    /// <summary>
    ///     This constructur only sets the DocNum and StmtDataSet and is only used when building a collection of
    ///     StatementData sets for the purpose of syncing StatementProds for multiple statements.
    /// </summary>
    public StatementData(DataSet dataSetStmt, long docNum)
    {
        DataSetStmtNew = new DataSet();
        for (var i = 0; i < dataSetStmt.Tables.Count; i++)
        {
            //Each family member will have their own account table, so only consider tables that start with 'account'.
            if (!dataSetStmt.Tables[i].TableName.StartsWith("account")) continue;
            var tableAccount = dataSetStmt.Tables[i].Copy();
            //Remove columns that are not used when inserting or syncing StatementProds to save memory.
            for (var j = tableAccount.Columns.Count - 1; j >= 0; j--)
                if (!tableAccount.Columns[j].ColumnName.In("AdjNum", "creditsDouble", "PayPlanChargeNum", "ProcNum", "procsOnObj"))
                    tableAccount.Columns.RemoveAt(j);

            DataSetStmtNew.Tables.Add(tableAccount);
        }

        DocNum = docNum;
    }

    /// <summary>
    ///     Used when creating late charges. Gets a list of StatementData objects based on the filters used in
    ///     ForLateCharges. Should only be run after aging has been run.
    /// </summary>
    public static List<StatementData> GetListStatementDataForLateCharges(bool isExcludeAccountNoTil, bool isExcludeExistingLateCharges,
        decimal excludeBalancesLessThan, DateTime dateRangeStart, DateTime dateRangeEnd, List<long> listBillingTypes)
    {
        if (listBillingTypes.IsNullOrEmpty()) return new List<StatementData>();

        var command = $@"
				SELECT statementprod.*,statement.SuperFamily,statement.DateSent,guar.PatNum,guar.PriProv
				FROM statementprod
				INNER JOIN statement
					ON statement.StatementNum=statementprod.StatementNum
					AND statement.DocNum=statementprod.DocNum
					AND statementprod.LateChargeAdjNum=0
					AND statement.IsInvoice=0
					AND statement.IsReceipt=0
					AND statement.IsSent=1
					AND statement.Mode_!={SOut.Int((int) StatementMode.InPerson)}
					AND statement.DateSent BETWEEN {SOut.Date(dateRangeStart)} AND {SOut.Date(dateRangeEnd)}
				INNER JOIN patient
					ON statement.PatNum=patient.PatNum
				INNER JOIN patient guar
					ON patient.Guarantor=guar.PatNum 
					AND guar.BillingType IN({string.Join(",", listBillingTypes.Select(x => SOut.Long(x)).ToList())}) ";
        if (isExcludeAccountNoTil) command += "AND guar.HasSignedTil=1 ";
        command += @"
				LEFT JOIN document
					ON statement.DocNum=document.DocNum
				LEFT JOIN (
					SELECT patient.SuperFamily,
						SUM(CASE WHEN (patient.BalTotal-patient.InsEst)>0 THEN (patient.BalTotal-patient.InsEst) ELSE 0 END) AS 'SuperFamBal'
					FROM patient
					WHERE patient.PatNum=patient.Guarantor
					AND patient.SuperFamily!=0
					GROUP BY patient.SuperFamily
				) AS superfam
				ON superfam.SuperFamily=patient.PatNum ";
        if (isExcludeExistingLateCharges)
            command += @$"
					LEFT JOIN statementprod sp 
						ON statementprod.FKey=sp.LateChargeAdjNum 
						AND statementprod.ProdType={(int) ProductionType.Adjustment} ";
        //Filter out statements with missing documents, and superfamily statements or regular statements under the balance filter
        command += @$"WHERE (CASE WHEN statementprod.DocNum!=0 AND document.DocNum IS NULL THEN 0 ELSE 1 END)
				AND (CASE WHEN statement.SuperFamily=0 THEN guar.BalTotal-guar.InsEst ELSE COALESCE(superfam.SuperFamBal,0) END)
				>={SOut.Decimal(excludeBalancesLessThan)} ";
        if (isExcludeExistingLateCharges) command += "AND sp.LateChargeAdjNum IS NULL ";
        command += "ORDER BY statement.DateSent,statement.StatementNum";
        var table = DataCore.GetTable(command);
        var listStatementDatas = new List<StatementData>();
        //If we end up wanting to prevent late charges from being created twice for a single statement that has had multiple documents made for it,
        //we can remove the dictionary entry for the statement in this loop if we come across a StatementProd that has a non-zero LateChargeAdjNum.
        //We would then have to remove the "ON statementprod.LateChargeAdjNum=0 clause from the query above.
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var dataRow = table.Rows[i];
            var statementNum = SIn.Long(dataRow["StatementNum"].ToString());
            //If a list entry for the statement does not yet exist, create one.
            //All the StatementProds should have the same StatementNum.
            var statementData = listStatementDatas.Find(x => x.ListStatementProds.Any(y => y.StatementNum == statementNum));
            if (statementData == null)
            {
                statementData = new StatementData();
                statementData.DateSent = SIn.Date(dataRow["DateSent"].ToString());
                statementData.PatNumGuarantor = SIn.Long(dataRow["PatNum"].ToString());
                statementData.ProvNumPriGuarantor = SIn.Long(dataRow["PriProv"].ToString());
                statementData.IsSuperFamilyStatement = SIn.Long(dataRow["SuperFamily"].ToString()) != 0;
                statementData.ListPatNumsGuarantor.Add(statementData.PatNumGuarantor);
                if (statementData.IsSuperFamilyStatement) statementData.ListPatNumsGuarantor = Patients.GetSuperFamilyGuarantors(statementData.PatNumGuarantor).Select(x => x.PatNum).ToList();
                listStatementDatas.Add(statementData);
            }

            //Then, add the statement prod to the list in this statement's list entry.
            var statementProd = new StatementProd();
            statementProd.StatementProdNum = SIn.Long(dataRow["StatementProdNum"].ToString());
            statementProd.StatementNum = statementNum;
            statementProd.FKey = SIn.Long(dataRow["FKey"].ToString());
            statementProd.ProdType = (ProductionType) SIn.Int(dataRow["ProdType"].ToString());
            statementProd.LateChargeAdjNum = SIn.Long(dataRow["LateChargeAdjNum"].ToString());
            statementData.ListStatementProds.Add(statementProd);
        }

        return listStatementDatas;
    }
}

public struct EbillStatement
{
    public Statement Statement;
    public Family Family;
}