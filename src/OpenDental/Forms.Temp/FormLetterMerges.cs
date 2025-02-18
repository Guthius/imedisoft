using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.Logic;
using OpenDentBusiness;
using Word = Microsoft.Office.Interop.Word;

namespace OpenDental;

public partial class FormLetterMerges : FormODBase
{
    private Patient _patient;
    private List<LetterMerge> _listLetterMergesForCat;
    private bool _isChanged;
    private List<Def> _listDefsLetterMergeCat;

    public FormLetterMerges(Patient patient)
    {
        InitializeComponent();

        _patient = patient;
    }

    private void FormLetterMerges_Load(object sender, EventArgs e)
    {
        PrefC.GetString(PrefName.LetterMergePath);

        FillCats();

        if (listCategories.Items.Count > 0)
        {
            listCategories.SelectedIndex = 0;
        }

        FillLetters();

        if (listLetters.Items.Count > 0)
        {
            listLetters.SelectedIndex = 0;
        }

        comboImageCategory.Items.AddDefNone();
        comboImageCategory.Items.AddDefs(Defs.GetDefsForCategory(DefCat.ImageCats, true));

        SelectImageCat();
    }

    private void FillCats()
    {
        _listDefsLetterMergeCat = Defs.GetDefsForCategory(DefCat.LetterMergeCats, true);

        listCategories.Items.Clear();

        for (var i = 0; i < _listDefsLetterMergeCat.Count; i++)
        {
            listCategories.Items.Add(_listDefsLetterMergeCat[i].ItemName);
        }
    }

    private void FillLetters()
    {
        listLetters.Items.Clear();
        if (listCategories.SelectedIndex == -1)
        {
            _listLetterMergesForCat = [];
            return;
        }

        LetterMergeFields.RefreshCache();
        LetterMerges.RefreshCache();
        _listLetterMergesForCat = LetterMerges.GetListForCat(listCategories.SelectedIndex);
        for (var i = 0; i < _listLetterMergesForCat.Count; i++)
        {
            listLetters.Items.Add(_listLetterMergesForCat[i].Description);
        }
    }

    private void SelectImageCat()
    {
        long defNumLetter = 0;
        if (listLetters.Items.Count > 0 && listLetters.SelectedIndex >= 0)
        {
            var letterMergeSelected = _listLetterMergesForCat[listLetters.SelectedIndex];
            if (letterMergeSelected != null)
            {
                defNumLetter = letterMergeSelected.ImageFolder;
            }
        }

        comboImageCategory.SetSelectedDefNum(defNumLetter);
    }

    private void listCategories_Click(object sender, EventArgs e)
    {
        FillLetters();

        if (listLetters.Items.Count > 0)
        {
            listLetters.SelectedIndex = 0;
        }

        SelectImageCat();
    }

    private void butEditCats_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.DefEdit))
        {
            return;
        }

        using var formDefinitions = new FormDefinitions(DefCat.LetterMergeCats);

        formDefinitions.ShowDialog();

        FillCats();
    }

    private void listLetters_DoubleClick(object sender, EventArgs e)
    {
        if (listLetters.SelectedIndex == -1)
        {
            return;
        }

        var selectedRow = listLetters.SelectedIndex;
        var letterMerge = _listLetterMergesForCat[listLetters.SelectedIndex];
        using var formLetterMergeEdit = new FormLetterMergeEdit(letterMerge);
        formLetterMergeEdit.ShowDialog();
        FillLetters();
        if (listLetters.Items.Count > selectedRow)
        {
            listLetters.SetSelected(selectedRow);
        }

        if (listLetters.SelectedIndex == -1 && listLetters.Items.Count > 0)
        {
            listLetters.SelectedIndex = 0;
        }

        SelectImageCat();
        _isChanged = true;
    }

    private void listLetters_Click(object sender, EventArgs e)
    {
        if (listLetters.SelectedIndex == -1)
        {
            return;
        }

        SelectImageCat();
    }

    private void butAdd_Click(object sender, EventArgs e)
    {
        if (listCategories.SelectedIndex == -1)
        {
            MsgBox.Show(this, "Please select a category first.");
            return;
        }

        var letterMerge = new LetterMerge();
        letterMerge.Category = _listDefsLetterMergeCat[listCategories.SelectedIndex].DefNum;
        using var formLetterMergeEdit = new FormLetterMergeEdit(letterMerge);
        formLetterMergeEdit.IsNew = true;
        formLetterMergeEdit.ShowDialog();
        FillLetters();
        _isChanged = true;
    }

    private bool CreateDataFile(string fileName, LetterMerge letterMerge)
    {
        DataTable dataTable;
        try
        {
            dataTable = LetterMergesQueries.GetLetterMergeInfo(_patient, letterMerge);
        }
        catch (Exception ex)
        {
            ShowError("There was a error getting letter merge info:\r\n" + ex.Message);

            return false;
        }

        dataTable = FormQuery.MakeReadable(dataTable, null, false);
        try
        {
            using var streamWriter = new StreamWriter(fileName, false);

            var line = "";
            for (var i = 0; i < letterMerge.Fields.Count; i++)
            {
                if (letterMerge.Fields[i].StartsWith("referral."))
                {
                    line += "Ref" + letterMerge.Fields[i].Substring(9);
                }
                else
                {
                    line += letterMerge.Fields[i];
                }

                if (i < letterMerge.Fields.Count - 1)
                {
                    line += "\t";
                }
            }

            streamWriter.WriteLine(line);
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                line = "";
                for (var j = 0; j < dataTable.Columns.Count; j++)
                {
                    var cell = dataTable.Rows[i][j].ToString();

                    cell = cell.Replace("\r", "");
                    cell = cell.Replace("\n", "");
                    cell = cell.Replace("\t", "");
                    cell = cell.Replace("\"", "");

                    line += cell;
                    if (j < dataTable.Columns.Count - 1)
                    {
                        line += "\t";
                    }
                }

                streamWriter.WriteLine(line);
            }

            streamWriter.Close();
        }
        catch
        {
            ShowError("File in use by another program.  Close and try again.");
            return false;
        }

        return true;
    }

    private void butCreateData_Click(object sender, EventArgs e)
    {
        if (!CreateData())
        {
            return;
        }

        ShowInfo("Done.");
    }

    private void butViewData_Click(object sender, EventArgs e)
    {
        if (!CreateData())
        {
            return;
        }

        var letterMerge = _listLetterMergesForCat[listLetters.SelectedIndex];
        var dataFile = PrefC.GetString(PrefName.LetterMergePath) + letterMerge.DataFileName;
        Process.Start(dataFile);
    }

    private bool CreateData()
    {
        if (listLetters.SelectedIndex == -1)
        {
            ShowError("Please select a letter first.");
            return false;
        }

        var letterMerge = _listLetterMergesForCat[listLetters.SelectedIndex];

        var dataFile = PrefC.GetString(PrefName.LetterMergePath) + letterMerge.DataFileName;

        if (!Directory.Exists(PrefC.GetString(PrefName.LetterMergePath)))
        {
            ShowError("Letter merge path not valid.");
            return false;
        }

        Cursor = Cursors.WaitCursor;

        if (!CreateDataFile(dataFile, letterMerge))
        {
            Cursor = Cursors.Default;
            return false;
        }

        Cursor = Cursors.Default;
        return true;
    }

    private void butPrint_Click(object sender, EventArgs e)
    {
        if (listLetters.SelectedIndex == -1)
        {
            ShowError("Please select a letter first.");
            return;
        }

        var letterMerge = _listLetterMergesForCat[listLetters.SelectedIndex];
        letterMerge.ImageFolder = comboImageCategory.GetSelectedDefNum();
        var templateFilePath = ODFileUtils.CombinePaths(PrefC.GetString(PrefName.LetterMergePath), letterMerge.TemplateName);
        var dataFile = ODFileUtils.CombinePaths(PrefC.GetString(PrefName.LetterMergePath), letterMerge.DataFileName);
        if (!File.Exists(templateFilePath))
        {
            ShowError("Template file does not exist.");
            return;
        }

        var printDocument = new PrintDocument();
        if (!PrinterL.SetPrinter(printDocument, PrintSituation.Default, _patient.PatNum, "Letter merge " + letterMerge.Description + " printed"))
        {
            return;
        }

        if (!CreateDataFile(dataFile, letterMerge))
        {
            return;
        }

        Word.MailMerge word_MailMerge = null;
        Word.Application word_Application = null;
        Word._Document word_Document = null;
        try
        {
            word_Application = new Word.Application();
            word_Application.Visible = true;
            word_Application.Activate();
            //Open a document.
            word_Document = word_Application.Documents.Open(templateFilePath);
            word_Document.Select();
            word_MailMerge = word_Document.MailMerge;
            //Attach the data file.
            word_Document.MailMerge.OpenDataSource(dataFile);
            word_MailMerge.Destination = Word.WdMailMergeDestination.wdSendToPrinter;
            //WrdApp.ActivePrinter=pd.PrinterSettings.PrinterName;
            //replaced with following 4 lines due to MS bug that changes computer default printer
            object word_Basic = word_Application.WordBasic;
            var objectArrayWBValues = new object[] {printDocument.PrinterSettings.PrinterName, 1};
            var stringArrayWBNames = new[] {"Printer", "DoNotSetAsSysDefault"};
            word_Basic.GetType().InvokeMember("FilePrintSetup", BindingFlags.InvokeMethod, null, word_Basic, objectArrayWBValues, null, null, stringArrayWBNames);
            word_MailMerge.Execute(Pause: false);
            if (letterMerge.ImageFolder != 0)
            {
                //if image folder exist for this letter, save to AtoZ folder
                word_Document.Select();
                word_MailMerge.Destination = Word.WdMailMergeDestination.wdSendToNewDocument;
                word_MailMerge.Execute(Pause: false);
                word_Application.Activate();
                var tempFilePath = ODFileUtils.CreateRandomFile(Path.GetTempPath(), GetFileExtensionForWordDoc(templateFilePath));
                word_Application.ActiveDocument.SaveAs(tempFilePath); //save the document 
                word_Application.ActiveDocument.Close();
                SaveToImageFolder(tempFilePath, letterMerge);
            }

            //Close the original form document since just one record.
            word_Document.Saved = true;
            word_Document.Close(SaveChanges: false);
            //At this point, Word remains open with no documents.
        }
        catch (Exception ex)
        {
            FriendlyException.Show(Lan.g(this, "Error. Is MS Word installed?"), ex);
            return;
        }
        finally
        {
            if (word_Application != null)
            {
                Marshal.ReleaseComObject(word_Application);
                word_Application = null;
            }

            if (word_MailMerge != null)
            {
                Marshal.ReleaseComObject(word_MailMerge);
                word_MailMerge = null;
            }

            if (word_Document != null)
            {
                Marshal.ReleaseComObject(word_Document);
                word_Document = null;
            }
        }

        Commlogs.Insert(new Commlog
        {
            CommDateTime = DateTime.Now,
            CommType = Commlogs.GetTypeAuto(CommItemTypeAuto.MISC),
            Mode_ = CommItemMode.Mail,
            SentOrReceived = CommSentOrReceived.Sent,
            PatNum = _patient.PatNum,
            Note = "Letter sent: " + letterMerge.Description + ". ",
            UserNum = Security.CurUser.UserNum
        });

        DialogResult = DialogResult.OK;
    }

    private void butPreview_Click(object sender, EventArgs e)
    {
        if (listLetters.SelectedIndex == -1)
        {
            ShowError("Please select a template first.");
            return;
        }

        var letterMerge = _listLetterMergesForCat[listLetters.SelectedIndex];

        letterMerge.ImageFolder = comboImageCategory.GetSelectedDefNum();

        var templateFile = ODFileUtils.CombinePaths(PrefC.GetString(PrefName.LetterMergePath), letterMerge.TemplateName);
        var tempDataFile = PrefC.GetRandomTempFile(".txt");
        if (!File.Exists(templateFile))
        {
            ShowError("Template file does not exist.");
            return;
        }

        if (!CreateDataFile(tempDataFile, letterMerge))
        {
            return;
        }

        Word.MailMerge word_MailMerge = null;
        Word.Application word_Application = null;
        Word._Document word_Document = null;
        try
        {
            word_Application = new Word.Application();
            word_Application.Visible = true;
            word_Application.Activate();
            //Open a document.
            word_Document = word_Application.Documents.Open(templateFile);
            word_Document.Select();
            word_MailMerge = word_Document.MailMerge;
            word_Document.MailMerge.OpenDataSource(tempDataFile);
            word_MailMerge.Destination = Word.WdMailMergeDestination.wdSendToNewDocument;
            word_MailMerge.Execute(Pause: false);
            if (letterMerge.ImageFolder != 0)
            {
                //if image folder is set for this letter, save to AtoZ folder
                //Open document from the atoz folder.
                word_Application.Activate(); //brings window to foreground and gives it focus.
                var tempFilePath = ODFileUtils.CreateRandomFile(Path.GetTempPath(), GetFileExtensionForWordDoc(templateFile));
                word_Application.ActiveDocument.SaveAs(tempFilePath); //save the document to temp location
                var document = SaveToImageFolder(tempFilePath, letterMerge);
                var patFolder = ImageStore.GetPatientFolder(_patient, ImageStore.GetDataFolder());
                var fileName = ImageStore.GetFilePath(document, patFolder);
                if (!File.Exists(fileName))
                {
                    MsgBox.Show(Lan.g(this, "Error saving file to the Imaging module: ") + " " + document.FileName);
                    return;
                }

                Process.Start(fileName);
                word_Application.ActiveDocument.Close(); //Necessary since we created an extra document
                try
                {
                    File.Delete(tempFilePath); //Clean up the temp file
                }
                catch
                {
                }
            }

            //Close the original form document since just one record.
            word_Document.Saved = true; //to avoid triggering a save message
            word_Document.Close(SaveChanges: false);
            //At this point, Word remains open with just one new document.
            //Either the original that we created, or the copy was saved to Imaging and reopened.
            word_Application.Activate();
            if (word_Application.WindowState == Word.WdWindowState.wdWindowStateMinimize)
            {
                word_Application.WindowState = Word.WdWindowState.wdWindowStateMaximize;
            }
        }
        catch (Exception ex)
        {
            FriendlyException.Show(Lan.g(this, "Error. Is MS Word installed?"), ex);
            return;
        }
        finally
        {
            if (word_Application != null)
            {
                Marshal.ReleaseComObject(word_Application);
                word_Application = null;
            }

            if (word_MailMerge != null)
            {
                Marshal.ReleaseComObject(word_MailMerge);
                word_MailMerge = null;
            }

            if (word_Document != null)
            {
                Marshal.ReleaseComObject(word_Document);
                word_Document = null;
            }
        }

        Commlogs.Insert(new Commlog
        {
            CommDateTime = DateTime.Now,
            CommType = Commlogs.GetTypeAuto(CommItemTypeAuto.MISC),
            Mode_ = CommItemMode.Mail,
            SentOrReceived = CommSentOrReceived.Sent,
            PatNum = _patient.PatNum,
            Note = "Letter sent: " + letterMerge.Description + ". ",
            UserNum = Security.CurUser.UserNum
        });

        DialogResult = DialogResult.OK;
    }

    private void butChartLetter_Click(object sender, EventArgs e)
    {
        if (listLetters.SelectedIndex == -1)
        {
            ShowError("Please select a template first.");
            return;
        }

        var letterMerge = _listLetterMergesForCat[listLetters.SelectedIndex];
        var templateFile = ODFileUtils.CombinePaths(PrefC.GetString(PrefName.LetterMergePath), letterMerge.TemplateName);
        var tempDataFile = PrefC.GetRandomTempFile(".txt");
        if (!File.Exists(templateFile))
        {
            ShowError("Template file does not exist.");
            return;
        }

        if (!CreateDataFile(tempDataFile, letterMerge))
        {
            return;
        }

        Cursor = Cursors.WaitCursor;
        Word._Document word_Document = null;
        try
        {
            ChartLetterL.InitializeWordApplication();
            word_Document = ChartLetterL.Word_Application.Documents.Open(templateFile);
            word_Document.MailMerge.OpenDataSource(tempDataFile);
            word_Document.MailMerge.Destination = Word.WdMailMergeDestination.wdSendToNewDocument;
            word_Document.MailMerge.Execute();
            word_Document.Saved = true; //Avoid "save changes" prompt
            word_Document.Close(SaveChanges: false); //close the template
            Marshal.ReleaseComObject(word_Document);
            word_Document = null;
            //We will now use the word_Document variable for the merged document
            word_Document = ChartLetterL.Word_Application.ActiveDocument;
            var tempFilePath = ODFileUtils.CreateRandomFile(Path.GetTempPath(), GetFileExtensionForWordDoc(templateFile));
            word_Document.SaveAs2(tempFilePath); //save the document to temp location
            var document = SaveToImageFolder(tempFilePath, letterMerge, isChartLetter: true);
            //The above line creates a copy of the file in Images.
            var patFolder = ImageStore.GetPatientFolder(_patient, ImageStore.GetDataFolder());
            var fullPath = ImageStore.GetFilePath(document, patFolder); //this is our new copy location
            if (!File.Exists(fullPath))
            {
                Cursor = Cursors.Default;
                MsgBox.Show(Lan.g(this, "Error saving file to the Images module: ") + " " + document.FileName);
                return;
            }

            word_Document.Saved = true; //to avoid triggering a save message?
            word_Document.Close(SaveChanges: false); //closes the temp doc.
            Marshal.ReleaseComObject(word_Document);
            word_Document = null;
            try
            {
                File.Delete(tempFilePath); //Clean up the temp file
            }
            catch
            {
            }

            //Finally, we will use the word_Document variable for our new file opened from AtoZ folder
            word_Document = ChartLetterL.Word_Application.Documents.Open(fullPath);
            ChartLetterL.Word_Application.Visible = true;
            ChartLetterL.Word_Application.Activate(); //brings to front
            word_Document = null; //release our handle
            ChartLetterL.AddTracker(document, fullPath); //,alreadyCreatedOrArchived:true,this);
            //When the Word document eventually closes, event handler in ChartLetterL will notice and perform any necessary db updates.
        }
        catch (Exception ex)
        {
            ShowException(ex, "Error. Is MS Word installed?");
            return;
        }
        finally
        {
            if (word_Document != null)
            {
                Marshal.ReleaseComObject(word_Document);
            }
        }

        Cursor = Cursors.Default;
        
        DialogResult = DialogResult.OK;
    }

    private string GetFileExtensionForWordDoc(string filePath)
    {
        var retVal = ".docx"; //default file extension
        var ext = Path.GetExtension(filePath).ToLower();
        var listBackwardCompat = new List<string> {".dot", ".doc", ".dotm"};
        if (listBackwardCompat.Contains(ext))
        {
            retVal = ".doc";
        }

        return retVal;
    }

    private Document SaveToImageFolder(string fileSourcePath, LetterMerge letterMerge, bool isChartLetter = false)
    {
        if (letterMerge.ImageFolder == 0 && !isChartLetter)
        {
            return new Document();
        }

        var rawBase64 = "";
        var documentSave = new Document();
        documentSave.DocNum = Documents.Insert(documentSave);
        documentSave.ImgType = ImageType.Document;
        documentSave.DateCreated = DateTime.Now;
        documentSave.PatNum = _patient.PatNum;
        documentSave.DocCategory = letterMerge.ImageFolder; //0 for isChartNote
        documentSave.ChartLetterStatus = EnumDocChartLetterStatus.Active;
        documentSave.UserNum = Security.CurUser.UserNum;
        documentSave.Description = letterMerge.Description;
        documentSave.RawBase64 = rawBase64; //blank if using AtoZfolder
        documentSave.FileName = ODFileUtils.CleanFileName(documentSave.Description + documentSave.DocNum) + GetFileExtensionForWordDoc(fileSourcePath);
        var fileDestPath = ImageStore.GetFilePath(documentSave, ImageStore.GetPatientFolder(_patient, ImageStore.GetDataFolder()));
        FileAtoZ.Copy(fileSourcePath, fileDestPath);
        Documents.Update(documentSave);
        return documentSave;
    }

    private void butEditTemplate_Click(object sender, EventArgs e)
    {
        if (listLetters.SelectedIndex == -1)
        {
            ShowError("Please select a letter first.");
            return;
        }

        var letterMerge = _listLetterMergesForCat[listLetters.SelectedIndex];
        var templateFile = ODFileUtils.CombinePaths(PrefC.GetString(PrefName.LetterMergePath), letterMerge.TemplateName);
        var dataFile = ODFileUtils.CombinePaths(PrefC.GetString(PrefName.LetterMergePath), letterMerge.DataFileName);
        if (!File.Exists(templateFile))
        {
            ShowError("Template file does not exist:  " + templateFile);
            return;
        }

        if (!CreateDataFile(dataFile, letterMerge))
        {
            return;
        }
        
        Word.Application wordApplication = null;
        Word._Document wordDocument = null;
        
        try
        {
            wordApplication = new Word.Application
            {
                Visible = true
            };
            
            wordApplication.Activate();
            wordDocument = wordApplication.Documents.Open(templateFile);
            wordDocument.Select();
            wordDocument.MailMerge.OpenDataSource(dataFile);
            if (wordApplication.WindowState == Word.WdWindowState.wdWindowStateMinimize)
            {
                wordApplication.WindowState = Word.WdWindowState.wdWindowStateMaximize;
            }
        }
        catch (Exception ex)
        {
            ShowException(ex, "Error. Is MS Word installed?");
        }
        finally
        {
            if (wordApplication is not null)
            {
                Marshal.ReleaseComObject(wordApplication);
            }

            if (wordDocument is not null)
            {
                Marshal.ReleaseComObject(wordDocument);
            }
        }
    }

    private void FormLetterMerges_Closing(object sender, CancelEventArgs e)
    {
        if (_isChanged)
        {
            DataValid.SetInvalid(InvalidType.LetterMerge);
        }
    }
}
