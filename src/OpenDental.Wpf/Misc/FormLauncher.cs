using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace OpenDental
{
    public class FormLauncher
    {
        public bool IsDialogOK;
        private FormLauncherEventArgs _formLauncherEventArgs;

        
        public FormLauncher(EnumFormName enumFormName)
        {
            _formLauncherEventArgs = new FormLauncherEventArgs
            {
                EnumFormName_ = enumFormName
            };
        }

        public static event EventHandler<FormLauncherEventArgs> EventLaunch;
        
        public bool IsDialogCancel => !IsDialogOK;
        
        public T GetField<T>(string fieldName)
        {
            if (_formLauncherEventArgs.Form is null)
            {
                throw new Exception("Form is null");
            }

            if (_formLauncherEventArgs.Form.IsDisposed)
            {
                throw new Exception("Form is disposed");
            }

            var type = _formLauncherEventArgs.Form.GetType();
            var fieldInfo = type.GetField(fieldName);
            
            return (T) fieldInfo.GetValue(_formLauncherEventArgs.Form);
        }

        public bool IsNullDisposedOrNotVis()
        {
            if (_formLauncherEventArgs.Form is null)
            {
                return true;
            }

            if (_formLauncherEventArgs.Form.IsDisposed)
            {
                return true;
            }

            if (!_formLauncherEventArgs.Form.Visible)
            {
                //In WinForms, this equates to whether a form is open or closed.
                return true;
            }

            return false;
        }

        public void MethodGetVoid(string methodName, params object[] parameters)
        {
            if (IsNullDisposedOrNotVis())
            {
                throw new Exception("Only for non-modal. Form is not open.");
            }

            var type = _formLauncherEventArgs.Form.GetType();
            var methodInfo = type.GetMethod(methodName);
            methodInfo.Invoke(_formLauncherEventArgs.Form, parameters);
        }

        public void RestoreAndFront()
        {
            if (IsNullDisposedOrNotVis())
            {
                return;
            }

            if (_formLauncherEventArgs.Form.WindowState == FormWindowState.Minimized)
            {
                _formLauncherEventArgs.Form.WindowState = FormWindowState.Normal;
            }

            _formLauncherEventArgs.Form.BringToFront();
        }

        public void SetEvent(string eventName, EventHandler<Bitmap> bitmapCaptured)
        {
            SetEvent(eventName, (Delegate) bitmapCaptured);
        }

        public void SetEvent(string eventName, Delegate eventHandler)
        {
            if (IsNullDisposedOrNotVis())
            {
                //setting the event prior to showing the form is the typical approach
                _formLauncherEventArgs.AddEvent(eventName, eventHandler);
                return;
            }

            //Nobody adds events after launching a form, but it's allowed
            var type = _formLauncherEventArgs.Form.GetType();
            var eventInfo = type.GetEvent(eventName);
            eventInfo.AddEventHandler(_formLauncherEventArgs.Form, eventHandler);
        }

        public void SetField(string fieldName, object fieldValue)
        {
            if (IsNullDisposedOrNotVis())
            {
                //setting the field prior to showing the form
                _formLauncherEventArgs.AddField(fieldName, fieldValue);
                return;
            }

            //setting a field after showing the form is far less common
            var type = _formLauncherEventArgs.Form.GetType();
            var fieldInfo = type.GetField(fieldName);
            fieldInfo.SetValue(_formLauncherEventArgs.Form, fieldValue);
        }

        ///<summary>Shows a non-modal form that you can interact with.</summary>
        public void Show()
        {
            _formLauncherEventArgs.IsDialog = false;
            EventLaunch?.Invoke(null, _formLauncherEventArgs);
            //A reference to the form will now be in the EAs.
            //Because we now have a reference, we won't need the global event anymore.
            //We will just use reflection each time we need to do something.
        }

        
        public bool ShowDialog()
        {
            EventLaunch?.Invoke(null, _formLauncherEventArgs);
            //continues after dialog closes
            IsDialogOK = _formLauncherEventArgs.IsDialogOK;
            return IsDialogOK;
        }
    }

    public class FormLauncherEventArgs
    {
        ///<summary>Example: OpenDental.FormEserviceText</summary>
        public EnumFormName EnumFormName_;

        
        public Form Form;

        public bool IsDialogOK;

        ///<summary>Default is true to launch a dialog. If this is set to false, then it will launch as Show() which is non-modal.</summary>
        public bool IsDialog = true;

        public List<EventPair> ListEventPairs = new List<EventPair>();
        public List<FieldPair> ListFieldPairs = new List<FieldPair>();

        public class EventPair
        {
            public string EventName;
            public Delegate EventHandler;
        }

        public class FieldPair
        {
            public string FieldName;
            public object FieldValue;
        }

        public void AddEvent(string eventName, Delegate eventHandler)
        {
            var eventPair = new EventPair();
            eventPair.EventName = eventName;
            eventPair.EventHandler = eventHandler;
            ListEventPairs.Add(eventPair);
        }

        public void AddField(string fieldName, object fieldValue)
        {
            var fieldPair = new FieldPair
            {
                FieldName = fieldName,
                FieldValue = fieldValue
            };
            ListFieldPairs.Add(fieldPair);
        }
    }

    /// <summary>Jordan will be involved in any changes to this enum. Items can be freely added. Keep it alphabetical. Also add to the switch statement in FormLauncherHelper.</summary>
    public enum EnumFormName
    {
        FormAllergySetup,
        FormCareCredit,
        FormCodeSystemsImport,
        FormDiseaseDefs,
        FormDrCeph,
        FormEServicesTexting,
        FormHouseCalls,
        FormMedications,
        FormNotePick,
        FormOryxUserSettings,
        FormPatientEdit,
        FormPrintTrojan,
        FormSheetFillEdit,
        FormTrojanCollect,
        FormTrophyNamePick,
        FormVideo,
        FormWebBrowser,
        FormWebView
    }
}