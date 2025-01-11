using System;
using System.Collections.Generic;
using System.IO;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;
using OpenDentBusiness.FileIO;

namespace OpenDentBusiness;


public class EmailAttaches
{
    public static long Insert(EmailAttach emailAttach)
    {
        return EmailAttachCrud.Insert(emailAttach);
    }

    public static void InsertMany(List<EmailAttach> listEmailAttaches)
    {
        if (listEmailAttaches.Count == 0) return;

        EmailAttachCrud.InsertMany(listEmailAttaches);
    }

    public static List<EmailAttach> GetForEmail(long emailMessageNum)
    {
        var listEmailMessageNums = new List<long> {emailMessageNum};
        return GetForEmails(listEmailMessageNums);
    }

    public static List<EmailAttach> GetForEmails(List<long> listEmailMessageNums)
    {
        var listEmailAttaches = new List<EmailAttach>();
        if (listEmailMessageNums == null || listEmailMessageNums.Count == 0) return listEmailAttaches;
        //Skip all attachments that are for EmailMessageNum of 0 because those are meant for Templates, not emails.
        //Use GetForTemplate() instead of GetForEmails if you want all template attachments.
        var listEmailMessageNumsFiltered = listEmailMessageNums.FindAll(x => x != 0);
        if (listEmailMessageNumsFiltered.Count < 1) return listEmailAttaches;
        var command = "SELECT * FROM emailattach WHERE EmailMessageNum IN(" + string.Join(",", listEmailMessageNums) + ")";
        return EmailAttachCrud.SelectMany(command);
    }

    ///<summary>Gets one EmailAttach from the db. Used by Patient Portal.</summary>
    public static EmailAttach GetOne(long emailAttachNum)
    {
        return EmailAttachCrud.SelectOne(emailAttachNum);
    }

    /// <summary>
    ///     Throws exceptions.  Creates a new file within the Out subfolder of the email attachment path (inside
    ///     OpenDentImages) and returns an EmailAttach object referencing the new file.  The displayFileName will always
    ///     contain valid file name characters, because it is either a hard coded value or is based on an existing valid file
    ///     name.  The actual file name will end with the displayFileName, so that the actual files are easier to locate and
    ///     have the same file extension as the displayedFileName.
    /// </summary>
    public static EmailAttach CreateAttach(string displayedFileName, byte[] byteArrayData)
    {
        return CreateAttach(displayedFileName, "", byteArrayData, true);
    }

    /// <summary>
    ///     Throws exceptions.  Creates a new file inside of the email attachment path (inside OpenDentImages) and returns an
    ///     EmailAttach object
    ///     referencing the new file.  If isOutbound is true, then the file will be saved to the "Out" subfolder, otherwise the
    ///     file will be saved to the
    ///     "In" subfolder.  The displayFileName will always contain valid file name characters, because it is either a hard
    ///     coded value or is based on an
    ///     existing valid file name.  If a file already exists matching the actualFileName, then an exception will occur.  Set
    ///     actualFileName to empty
    ///     string to generate a unique actual file name.  If the actual file name is generated, then actual file name will end
    ///     with the displayFileName,
    ///     so that the actual files are easier to locate and have the same file extension as the displayedFileName.
    /// </summary>
    public static EmailAttach CreateAttach(string displayedFileName, string actualFileName, byte[] byteArrayData, bool isOutbound)
    {
        var emailAttach = new EmailAttach();
        emailAttach.DisplayedFileName = displayedFileName;
        actualFileName = ODFileUtils.CleanFileName(actualFileName); //Clean the actual file name for the OS.
        if (string.IsNullOrEmpty(emailAttach.DisplayedFileName))
            //This could only happen for malformed incoming emails, but should not happen.  Name uniqueness is virtually guaranteed below.
            //The actual file name will not have an extension, so the user will be asked to pick the program to open the attachment with when
            //the attachment is double-clicked.
            emailAttach.DisplayedFileName = "attach";
        var attachDir = GetAttachPath();
        var subDir = "In";
        if (isOutbound) subDir = "Out";
        if (!false && !Directory.Exists(ODFileUtils.CombinePaths(attachDir, subDir))) Directory.CreateDirectory(ODFileUtils.CombinePaths(attachDir, subDir));
        if (string.IsNullOrEmpty(actualFileName))
            while (true)
            {
                if (!string.IsNullOrEmpty(emailAttach.ActualFileName))
                    if (!FileAtoZ.Exists(FileAtoZ.CombinePaths(attachDir, emailAttach.ActualFileName)))
                        break;

                //Display name is tacked onto actual file name last as to ensure file extensions are the same.
                emailAttach.ActualFileName = FileAtoZ.CombinePaths(subDir,
                    DateTime.Now.ToString("yyyyMMdd") + "_" + DateTime.Now.TimeOfDay.Ticks
                    + "_" + MiscUtils.CreateRandomAlphaNumericString(4) + "_" + ODFileUtils.CleanFileName(emailAttach.DisplayedFileName));
            }
        else
            //The caller wants a specific actualFileName.  Use the given name as is.
            emailAttach.ActualFileName = FileAtoZ.CombinePaths(subDir, actualFileName);

        var attachFilePath = FileAtoZ.CombinePaths(attachDir, emailAttach.ActualFileName);
        if (FileAtoZ.Exists(attachFilePath)) throw new ApplicationException("Email attachment could not be saved because a file with the same name already exists.");
        try
        {
            FileAtoZ.WriteAllBytes(attachFilePath, byteArrayData);
        }
        catch
        {
            if (!FileAtoZ.Exists(attachFilePath)) throw; //Show the initial error message
            try
            {
                FileAtoZ.Delete(attachFilePath);
            }
            catch
            {
                //We tried our best to delete the file, and there is nothing else to try.
            }

            throw; //Show the initial error message, even if the Delete() failed.
        }

        return emailAttach;
    }

    /// <summary>
    ///     Returns patient's AtoZ path if local AtoZ is used.  Returns Cloud AtoZ path if Dropbox is used. Returns temp
    ///     path if in database.
    /// </summary>
    public static string GetAttachPath()
    {
        string attachPath;
        if (true)
        {
            attachPath = ODFileUtils.CombinePaths(ImageStore.GetPreferredAtoZpath(), "EmailAttachments");
            if (!Directory.Exists(attachPath)) Directory.CreateDirectory(attachPath);
            return attachPath;
        }

        if (false)
        {
            attachPath = ODFileUtils.CombinePaths(ImageStore.GetPreferredAtoZpath(), "EmailAttachments", '/'); //Gets Cloud path with EmailAttachments folder.
            return attachPath;
        }

        //For users who have the A to Z folders disabled, there is no defined image path, so we
        //have to use a temp path.  This means that the attachments might be available immediately afterward,
        //but probably not later.
        attachPath = ODFileUtils.CombinePaths(Path.GetTempPath(), "opendental"); //Have to use Path.GetTempPath() here instead of PrefL.GetTempPathFolder() because we can't access PrefL.
        return attachPath;
    }

    ///<summary>Returns all EmailAttaches assocaited to a specific EmailTemplateNum.</summary>
    public static List<EmailAttach> GetForTemplate(long emailTemplateNum)
    {
        var command = "SELECT * FROM emailattach WHERE EmailTemplateNum=" + SOut.Long(emailTemplateNum);
        return EmailAttachCrud.SelectMany(command);
    }

    /// <summary>
    ///     Syncs a given list of EmailAttaches to a list of old EmailAttaches.
    ///     If emailAttachOld is not provided, it will use the emailMessageNum passed in to get the "old" attachments from the
    ///     database.
    /// </summary>
    public static void Sync(long emailMessageNum, List<EmailAttach> listEmailAttachesNew, List<EmailAttach> listEmailAttachesOld = null)
    {
        if (listEmailAttachesOld == null) listEmailAttachesOld = GetForEmail(emailMessageNum); //Get attachments from the database.
        EmailAttachCrud.Sync(listEmailAttachesNew, listEmailAttachesOld);
    }
}