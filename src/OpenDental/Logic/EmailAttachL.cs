using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using OpenDentBusiness;

namespace OpenDental
{
    public class EmailAttachL
    {
        ///<summary>Allow the user to pick the files to be attached. The 'pat' argument can be null. If the user cancels at any step, the return value
        ///will be an empty list.</summary>
        public static List<EmailAttach> PickAttachments(Patient patient)
        {
            List<EmailAttach> listEmailAttaches = new List<EmailAttach>();
            List<string> listFileNames;
            bool isLocalFileSelected = false;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            if (patient != null && true)
            {
                string patFolder = ImageStore.GetPatientFolder(patient, ImageStore.GetPreferredAtoZpath());
                openFileDialog.InitialDirectory = patFolder;
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return listEmailAttaches;
                }

                listFileNames = openFileDialog.FileNames.ToList();
            }
            else
            {
                //No patient selected or images in database
                //Use the OS default directory for this type of file viewer.
                openFileDialog.InitialDirectory = "";
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return listEmailAttaches;
                }

                isLocalFileSelected = true;
                listFileNames = openFileDialog.FileNames.ToList();
            }

            for (int i = 0; i < listFileNames.Count; i++)
            {
                if (!false)
                {
                    try
                    {
                        listEmailAttaches.Add(EmailAttaches.CreateAttach(Path.GetFileName(listFileNames[i]), File.ReadAllBytes(listFileNames[i])));
                    }
                    catch (Exception ex)
                    {
                        MsgBox.Show(ex.Message);
                        return listEmailAttaches;
                    }

                    continue;
                }

                FileAtoZSourceDestination fileAtoZSourceDestination;
                if (isLocalFileSelected)
                {
                    fileAtoZSourceDestination = FileAtoZSourceDestination.LocalToAtoZ;
                }
                else
                {
                    fileAtoZSourceDestination = FileAtoZSourceDestination.AtoZToAtoZ;
                }

                //Create EmailAttach using EmailAttaches.CreateAttach logic, shortened for our specific purpose.
                EmailAttach emailAttach = new EmailAttach();
                emailAttach.DisplayedFileName = Path.GetFileName(listFileNames[i]);
                string attachDir = EmailAttaches.GetAttachPath();
                string subDir = "Out";
                emailAttach.ActualFileName = ODFileUtils.CombinePaths(subDir,
                    DateTime.Now.ToString("yyyyMMdd") + "_" + DateTime.Now.TimeOfDay.Ticks.ToString()
                    + "_" + MiscUtils.CreateRandomAlphaNumericString(4) + "_" + emailAttach.DisplayedFileName).Replace("\\", "/");
                try
                {
                    FileAtoZ.Copy(listFileNames[i], FileAtoZ.CombinePaths(attachDir, emailAttach.ActualFileName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return listEmailAttaches;
                }

                listEmailAttaches.Add(emailAttach);
            }

            return listEmailAttaches;
        }

        public static EmailAttach PickAttachmentsImages(Patient patient)
        {
            EmailAttach emailAttach;
            using FormImagePickerPatient formImagePickerPatient = new FormImagePickerPatient();
            formImagePickerPatient.PatientCur = patient;
            Document document = new Document();
            document.FileName = "";
            Bitmap bitmap = null;
            string displayedFileName = "";
            byte[] byteArray = { };
            if (patient == null)
            {
                return null;
            }

            if (formImagePickerPatient.ShowDialog() != DialogResult.OK)
            {
                return null;
            }

            //A document or mount has been selected by this point.
            //Very similar approach for Cloud Storage and local files since most attachments are getting byteArrays from the database
            if (formImagePickerPatient.DocNumSelected > 0)
            {
                document = Documents.GetByNum(formImagePickerPatient.DocNumSelected);
                //We don't want to allow specific files to be attached here so warn the end user when they attempt to add an incompatible document as an image
                if (!ImageHelper.HasImageExtension(document.FileName) && !document.FileName.EndsWith(".pdf") && !document.ImgType.In(ImageType.Photo, ImageType.Radiograph))
                {
                    MsgBox.Show("Not allowed to attach selected file type as an image. Attach as a file instead.");
                    return null;
                }

                if (document.FileName.EndsWith(".pdf"))
                {
                    {
                        //grab the file directly from A to Z folder, no manipulations needed
                        string patFolder = ImageStore.GetPatientFolder(patient, ImageStore.GetPreferredAtoZpath());
                        document.FileName = ODFileUtils.CombinePaths(patFolder, document.FileName);
                        if (!false)
                        {
                            //PDF's in Cloud Storage won't need the byteArray
                            try
                            {
                                byteArray = File.ReadAllBytes(document.FileName);
                            }
                            catch (Exception ex)
                            {
                                //most likely exception is file not found
                                MessageBox.Show(ex.Message);
                                return null;
                            }
                        }

                        displayedFileName = Path.GetFileName(document.FileName);
                    }
                }
                else
                {
                    string extension = ImageStore.GetExtension(document);
                    bitmap = ImageHelper.GetBitmapOfDocumentFromDb(formImagePickerPatient.DocNumSelected);
                    displayedFileName = document.FileName.Replace(extension, ".jpg");
                }
            }
            else if (formImagePickerPatient.MountNumSelected > 0)
            {
                bitmap = MountHelper.GetBitmapOfMountFromDb(formImagePickerPatient.MountNumSelected);
                string uniqueIdentifier = "Mount" + formImagePickerPatient.MountNumSelected;
                displayedFileName = Documents.GenerateUniqueFileName(".jpg", patient, uniqueIdentifier);
            }

            if (bitmap != null)
            {
                using MemoryStream memoryStream = new MemoryStream();
                //Save creates a system ref to the resources, preventing proper disposal of image,
                //so we use a second image
                bitmap.Save(memoryStream, ImageFormat.Jpeg); //consider setting our own quality
                byteArray = memoryStream.ToArray();
                bitmap.Dispose();
            }

            if (!false)
            {
                try
                {
                    emailAttach = EmailAttaches.CreateAttach(displayedFileName, byteArray);
                }
                catch (Exception ex)
                {
                    //most likely exception is file not found
                    MessageBox.Show(ex.Message);
                    return null;
                }

                return emailAttach;
            }
        }
    }
}