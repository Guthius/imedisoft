using System;
using System.Collections.Generic;
using System.IO;
using CodeBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class EmailAttaches
{
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

    public static EmailAttach CreateAttach(string displayedFileName, byte[] byteArrayData)
    {
        return CreateAttach(displayedFileName, "", byteArrayData, true);
    }

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
                    if (!File.Exists(Path.Combine(attachDir, emailAttach.ActualFileName)))
                        break;

                //Display name is tacked onto actual file name last as to ensure file extensions are the same.
                emailAttach.ActualFileName = Path.Combine(subDir,
                    DateTime.Now.ToString("yyyyMMdd") + "_" + DateTime.Now.TimeOfDay.Ticks
                    + "_" + MiscUtils.CreateRandomAlphaNumericString(4) + "_" + ODFileUtils.CleanFileName(emailAttach.DisplayedFileName));
            }
        else
            //The caller wants a specific actualFileName.  Use the given name as is.
            emailAttach.ActualFileName = Path.Combine(subDir, actualFileName);

        var attachFilePath = Path.Combine(attachDir, emailAttach.ActualFileName);
        if (File.Exists(attachFilePath)) throw new ApplicationException("Email attachment could not be saved because a file with the same name already exists.");
        try
        {
            File.WriteAllBytes(attachFilePath, byteArrayData);
        }
        catch
        {
            if (!File.Exists(attachFilePath)) throw; //Show the initial error message
            try
            {
                File.Delete(attachFilePath);
            }
            catch
            {
                //We tried our best to delete the file, and there is nothing else to try.
            }

            throw; //Show the initial error message, even if the Delete() failed.
        }

        return emailAttach;
    }

    public static string GetAttachPath()
    {
        string attachPath;
        if (true)
        {
            attachPath = ODFileUtils.CombinePaths(ImageStore.GetDataFolder(), "EmailAttachments");
            if (!Directory.Exists(attachPath)) Directory.CreateDirectory(attachPath);
            return attachPath;
        }
    }

    public static List<EmailAttach> GetForTemplate(long emailTemplateNum)
    {
        var command = "SELECT * FROM emailattach WHERE EmailTemplateNum=" + (emailTemplateNum);
        return EmailAttachCrud.SelectMany(command);
    }

    public static void Sync(long emailMessageNum, List<EmailAttach> listEmailAttachesNew, List<EmailAttach> listEmailAttachesOld = null)
    {
        if (listEmailAttachesOld == null) listEmailAttachesOld = GetForEmail(emailMessageNum); //Get attachments from the database.
        EmailAttachCrud.Sync(listEmailAttachesNew, listEmailAttachesOld);
    }
}