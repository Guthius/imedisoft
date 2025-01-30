using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Logic;

public class EmailAttachL
{
    public static List<EmailAttach> PickAttachments(Patient patient)
    {
        var emailAttaches = new List<EmailAttach>();

        using var openFileDialog = new OpenFileDialog();

        openFileDialog.Multiselect = true;
        openFileDialog.InitialDirectory = patient != null ? ImageStore.GetPatientFolder(patient, ImageStore.GetDataFolder()) : "";

        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            return emailAttaches;
        }

        var filenames = openFileDialog.FileNames.ToList();

        foreach (var filename in filenames)
        {
            try
            {
                emailAttaches.Add(EmailAttaches.CreateAttach(Path.GetFileName(filename), File.ReadAllBytes(filename)));
            }
            catch (Exception ex)
            {
                MsgBox.Show(ex.Message);

                return emailAttaches;
            }
        }

        return emailAttaches;
    }

    public static EmailAttach PickAttachmentsImages(Patient patient)
    {
        Bitmap bitmap = null;
        var filename = "";
        byte[] bytes = [];

        if (patient == null)
        {
            return null;
        }

        using var formImagePickerPatient = new FormImagePickerPatient();

        formImagePickerPatient.PatientCur = patient;

        if (formImagePickerPatient.ShowDialog() != DialogResult.OK)
        {
            return null;
        }

        if (formImagePickerPatient.DocNumSelected > 0)
        {
            var document = Documents.GetByNum(formImagePickerPatient.DocNumSelected);

            if (!ImageHelper.HasImageExtension(document.FileName) && !document.FileName.EndsWith(".pdf") && document.ImgType is not (ImageType.Photo or ImageType.Radiograph))
            {
                MsgBox.Show("Not allowed to attach selected file type as an image. Attach as a file instead.");
                return null;
            }

            if (document.FileName.EndsWith(".pdf"))
            {
                document.FileName = Path.Combine(ImageStore.GetPatientFolder(patient, ImageStore.GetDataFolder()), document.FileName);

                try
                {
                    bytes = File.ReadAllBytes(document.FileName);
                }
                catch (Exception ex)
                {
                    ODMessageBox.Show(ex.Message);

                    return null;
                }

                filename = Path.GetFileName(document.FileName);
            }
            else
            {
                var extension = ImageStore.GetExtension(document);

                bitmap = ImageHelper.GetBitmapOfDocumentFromDb(formImagePickerPatient.DocNumSelected);

                filename = document.FileName.Replace(extension, ".jpg");
            }
        }
        else if (formImagePickerPatient.MountNumSelected > 0)
        {
            bitmap = MountHelper.GetBitmapOfMountFromDb(formImagePickerPatient.MountNumSelected);

            var uniqueIdentifier = "Mount" + formImagePickerPatient.MountNumSelected;

            filename = Documents.GenerateUniqueFileName(".jpg", patient, uniqueIdentifier);
        }

        if (bitmap is not null)
        {
            using var memoryStream = new MemoryStream();

            bitmap.Save(memoryStream, ImageFormat.Jpeg);
            bytes = memoryStream.ToArray();
            bitmap.Dispose();
        }

        EmailAttach emailAttach;
        try
        {
            emailAttach = EmailAttaches.CreateAttach(filename, bytes);
        }
        catch (Exception ex)
        {
            ODMessageBox.Show(ex.Message);

            return null;
        }

        return emailAttach;
    }
}