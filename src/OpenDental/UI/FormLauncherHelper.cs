using System;
using System.Windows.Forms;
using OpenDental.Forms;

namespace OpenDental;

public class FormLauncherHelper
{
    public static void Launch(object sender, FormLauncherEventArgs e)
    {
        var type = e.EnumFormName_ switch
        {
            EnumFormName.FormAllergySetup => typeof(FormAllergySetup),
            EnumFormName.FormCodeSystemsImport => typeof(FormCodeSystemsImport),
            EnumFormName.FormDiseaseDefs => typeof(FormDiseaseDefs),
            EnumFormName.FormDrCeph => typeof(FormDrCeph),
            EnumFormName.FormHouseCalls => typeof(FormHouseCalls),
            EnumFormName.FormMedications => typeof(FormMedications),
            EnumFormName.FormNotePick => typeof(FormNotePick),
            EnumFormName.FormOryxUserSettings => typeof(FormOryxUserSettings),
            EnumFormName.FormPatientEdit => typeof(FormPatientEdit),
            EnumFormName.FormPrintTrojan => typeof(FormPrintTrojan),
            EnumFormName.FormSheetFillEdit => typeof(FormSheetFillEdit),
            EnumFormName.FormTrojanCollect => typeof(FormTrojanCollect),
            EnumFormName.FormTrophyNamePick => typeof(FormTrophyNamePick),
            EnumFormName.FormWebBrowser => typeof(FormWebBrowser),
            EnumFormName.FormWebView => typeof(FormWebView),
            _ => null
        };

        if (type == null)
        {
            throw new InvalidOperationException("Form type not found.");
        }

        var form = (Form) Activator.CreateInstance(type);
        
        e.Form = form;
        
        foreach (var eventPair in e.ListEventPairs)
        {
            var eventInfo = type.GetEvent(eventPair.EventName);
            
            eventInfo.AddEventHandler(e.Form, eventPair.EventHandler);
        }

        foreach (var fieldPair in e.ListFieldPairs)
        {
            var fieldInfo = type.GetField(fieldPair.FieldName);
            
            fieldInfo.SetValue(e.Form, fieldPair.FieldValue);
        }

        if (!e.IsDialog)
        {
            form.Show();
            
            return;
        }

        form.ShowDialog();
        
        e.IsDialogOK = form.DialogResult == DialogResult.OK;
    }
}