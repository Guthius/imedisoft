using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Providers;
using Imedisoft.Core.Features.Providers.Dtos;
using OpenDental.Bridges;
using OpenDental.Features.Providers.Forms;
using OpenDental.Forms;
using OpenDental.Logic;
using OpenDental.UI;
using OpenDentBusiness;
using OpenDentBusiness.AutoComm;
using OpenDentBusiness.HL7;
using ProgramL = WpfControls.ProgramL;

namespace OpenDental;

public partial class ControlAppt : UserControl
{
    private long _opNumClickedBlockout;
    private DateTime _dateTimeClickedBlockout;
    private DateTime _dateTimeWaitingRmRefreshed;
    private bool _isPrintCardFamily;
    private FormASAP _formAsap;
    private FormConfirmList _formConfirmList;
    private FormRecallList _formRecallList;
    private FormTrackNext _formTrackNext;
    private FormUnsched _formUnsched;
    private bool _hasInitializedOnStartup;
    private bool _hasSetInitialStartTime;
    private List<ProviderDto> _listProvidersSearch;
    private List<ScheduleOpening> _listScheduleOpenings;
    private ToolStripMenuItem _toolStripMenuItem;
    private Patient _patient;
    private Schedule _scheduleBlockoutClipboard;
    private Arrivals _arrivalsLoaded;
    
    public ControlAppt()
    {
        InitializeComponent();
        
        gridReminders.ContextMenu = menuReminderEdit;
    }
    
    private Arrivals GetArrivalsLoaded()
    {
        return _arrivalsLoaded ??= Arrivals.LoadArrivals();
    }

    private void SetArrivalsLoaded(Arrivals value)
    {
        _arrivalsLoaded = value;
    }

    private void ContrAppt_Resize(object sender, EventArgs e)
    {
        if (_hasInitializedOnStartup)
        {
            contrApptPanel.SizeFont = float.Parse(PrefC.GetString(PrefName.ApptFontSize));
        }
    }
    
    private void contrApptPanel_ApptDoubleClicked(object sender, ApptEventArgs e)
    {
        var patnum = e.Appt.PatNum;
        
        using var formApptEdit = new FormApptEdit(contrApptPanel.SelectedAptNum);
 
        if (formApptEdit.ShowDialog() == DialogResult.OK)
        {
            var appointment = Appointments.GetOneApt(contrApptPanel.SelectedAptNum);
            if (appointment is not null)
            {
                var appointmentOld = appointment.Copy();
                
                if (!CanScheduleAppointmentTypeOnBlockoutType(appointment))
                {
                    MsgBox.Show(this, "Appointment type cannot be scheduled on this blockout.  Moving appointment to pinboard.");
                    
                    SendToPinBoardAptNums([appointment.AptNum]);
                    
                    UpdateAppointmentToUnscheduled(appointment, appointmentOld);
                }
                else if (TryAdjustAppointmentPattern(appointment, contrApptPanel.ListOpsVisible))
                {
                    MsgBox.Show(this, "Appointment is too long and would overlap another appointment or blockout.  Automatically shortened to fit.");
                    try
                    {
                        Appointments.Update(appointment, appointmentOld);
                    }
                    catch (ApplicationException ex)
                    {
                        ODMessageBox.Show(ex.Message);
                    }
                }
            }

            ModuleSelected(patnum);
        }
        else if (formApptEdit.DialogResult == DialogResult.Cancel && formApptEdit.HasProcsChangedAndCancel)
        {
            //If user canceled but changed the procs on appt first
            //Refresh the grid, don't need to check length because it didn't change.  Plus user might not want to change length.
            ModuleSelected(patnum);
            
            Signalods.SetInvalidAppt(formApptEdit.GetAppointmentOld()); //use old here because they cancelled.  Only calling this because there is no S-Class call.
        }
    }

    private void contrApptPanel_ApptMainAreaDoubleClicked(object sender, ApptMainClickEventArgs e)
    {
        if (Operatories.GetOperatory(e.OpNum) is null)
        {
            return;
        }

        var frmPatientSelect = new FrmPatientSelect
        {
            CanAddPatients = true
        };
        
        if (_patient is not null)
        {
            frmPatientSelect.PatNumInitial = _patient.PatNum;
        }

        frmPatientSelect.ShowDialog();
        if (frmPatientSelect.IsDialogCancel)
        {
            return;
        }

        if (_patient == null || frmPatientSelect.PatNumSelected != _patient.PatNum)
        {
            //if the patient was changed
            RefreshModuleDataPatient(frmPatientSelect.PatNumSelected);
            GlobalFormOpenDental.PatientSelected(_patient, true, false);
        }

        if (_patient != null && PatRestrictionL.IsRestricted(_patient.PatNum, PatRestrict.ApptSchedule))
        {
            return;
        }

        var patientMerged = AppointmentL.GetPatientMergePrompt(_patient.PatNum);
        if (patientMerged != null)
        {
            _patient = patientMerged;
            RefreshModuleDataPatient(_patient.PatNum);
            GlobalFormOpenDental.PatientSelected(_patient, isRefreshCurModule: true, isApptRefreshDataPat: false);
        }

        if (_patient != null && _patient.PatStatus.In(PatientStatus.Archived, PatientStatus.Deceased))
        {
            MsgBox.Show("Appointments cannot be scheduled for " + _patient.PatStatus.ToString().ToLower() + " patients.");
            return;
        }

        Appointment appointment = null;
        var isUpdateAppt = false;
        if (frmPatientSelect.IsNewPatientAdded)
        {
            var operatory = Operatories.GetOperatory(e.OpNum);

            var dateTimeAskedToArrive = DateTime.MinValue;
            if (_patient.AskToArriveEarly > 0)
            {
                dateTimeAskedToArrive = e.DateT.AddMinutes(-_patient.AskToArriveEarly);
                
                ODMessageBox.Show("Ask patient to arrive " + _patient.AskToArriveEarly + " minutes early at " + dateTimeAskedToArrive.ToShortTimeString() + ".");
            }

            appointment = Appointments.CreateNewAppointment(_patient, operatory, e.DateT, dateTimeAskedToArrive, null, contrApptPanel.ListSchedules);

            if (operatory.SetProspective)
            {
                if (MsgBox.Show(this, MsgBoxButtons.OKCancel, "Patient's status will be set to Prospective."))
                {
                    var patientOld = _patient.Copy();
                    _patient.PatStatus = PatientStatus.Prospective;
                    Patients.UpdateRecalls(_patient, patientOld, "Appointment Module, New Patient appointment created in prospective operatory");
                    Patients.Update(_patient, patientOld);
                    var logEntry = "Patient's status changed from " + patientOld.PatStatus.GetDescription() + " to "
                                   + _patient.PatStatus.GetDescription() + 
                                   " by creating an appointment in a prospective operatory.";
                    SecurityLogs.MakeLogEntry(EnumPermType.PatientEdit, _patient.PatNum, logEntry);
                }
            }

            using var formApptEdit = new FormApptEdit(appointment.AptNum);
            
            formApptEdit.IsNew = true;
            
            if (formApptEdit.ShowDialog() == DialogResult.OK)
            {
                if (appointment.IsNewPatient)
                {
                    AutomationL.Trigger(EnumAutomationTrigger.ApptNewPatCreate, null, appointment.PatNum, appointment.AptNum);
                }

                AutomationL.Trigger(EnumAutomationTrigger.ApptCreate, null, appointment.PatNum, appointment.AptNum);
                RefreshModuleDataPatient(_patient.PatNum);
                GlobalFormOpenDental.PatientSelected(_patient, true, false);
                if (!HasValidStartTime(appointment))
                {
                    var appointmentOld2 = appointment.Copy();
                    MsgBox.Show(this, "Appointment start time would overlap another appointment.  Moving appointment to pinboard.");
                    SendToPinBoardAptNums([appointment.AptNum]);
                    appointment.AptStatus = ApptStatus.UnschedList;
                    try
                    {
                        Appointments.Update(appointment, appointmentOld2); //Appointments S-Class handles Signalods
                    }
                    catch (ApplicationException ex)
                    {
                        ODMessageBox.Show(ex.Message);
                    }

                    RefreshPeriod();
                    return; //It's ok to skip the rest of the method here. The appointment is now on the pinboard and must be rescheduled
                }

                appointment = Appointments.GetOneApt(appointment.AptNum); //Need to get appt from DB so we have the time pattern
                contrApptPanel.SelectedAptNum = appointment.AptNum;
                isUpdateAppt = true;
            }
        }
        else
        {
            //new patient not added
            if (Appointments.HasOutstandingAppts(_patient.PatNum))
            {
                DisplayOtherDlg(true, e.DateT, e.OpNum);
                RefreshModuleScreenButtonsRight();
            }
            else
            {
                using var formApptsOther = new FormApptsOther(_patient.PatNum, pinBoard.ListPinBoardItems.Select(x => x.AptNum).ToList()); //not shown
                CheckStatus();
                formApptsOther.IsInitialDoubleClick = true;
                formApptsOther.DateTimeClicked = contrApptPanel.DateTimeClicked;
                formApptsOther.OpNumClicked = contrApptPanel.OpNumClicked;
                formApptsOther.DateTNew = e.DateT;
                formApptsOther.OpNumNew = e.OpNum;
                formApptsOther.MakeAppointment();
                if (formApptsOther.ListAptNumsSelected.Count > 0)
                {
                    contrApptPanel.SelectedAptNum = formApptsOther.ListAptNumsSelected[0];
                }

                appointment = Appointments.GetOneApt(contrApptPanel.SelectedAptNum);
                isUpdateAppt = true;
            }
        }

        if (appointment is null)
        {
            return;
        }

        var appointmentOld = appointment.Copy();
        
        if (!CanScheduleAppointmentTypeOnBlockoutType(appointment))
        {
            MsgBox.Show(this, "Appointment type cannot be scheduled on this blockout.  Moving appointment to pinboard.");
            SendToPinBoardAptNums([appointment.AptNum]);
            UpdateAppointmentToUnscheduled(appointment, appointmentOld);
            RefreshPeriod();
            return; //It's ok to skip the rest of the method here. The appointment is now on the pinboard and must be rescheduled
        }

        if (!HasValidStartTime(appointment))
        {
            MsgBox.Show(this, "Appointment start time would overlap another appointment.  Moving appointment to pinboard.");
            SendToPinBoardAptNums([appointment.AptNum]);
            UpdateAppointmentToUnscheduled(appointment, appointmentOld);
            RefreshPeriod();
            return; //It's ok to skip the rest of the method here. The appointment is now on the pinboard and must be rescheduled
        }

        if (isUpdateAppt && appointment != null)
        {
            #region Provider Term Date Check

            //Prevents appointments with providers that are past their term end date from being scheduled
            var message = Providers.CheckApptProvidersTermDates(appointment);
            if (message != "")
            {
                ODMessageBox.Show(message); //translated in Providers S class method
                return;
            }

            #endregion Provider Term Date Check

            appointmentOld = appointment.Copy();
            if (TryAdjustAppointmentPattern(appointment, contrApptPanel.ListOpsVisible))
            {
                MsgBox.Show(this, "Appointment is too long and would overlap another appointment or blockout.  Automatically shortened to fit.");
                try
                {
                    Appointments.Update(appointment, appointmentOld); //Appointments S-Class handles Signalods
                }
                catch (ApplicationException ex)
                {
                    ODMessageBox.Show(ex.Message);
                }

                Appointments.TryAddPerVisitProcCodesToAppt(appointment, appointmentOld.AptStatus);
            }

            RefreshPeriod();
            RefreshModuleScreenButtonsRight();
        }
    }
    
    private ToolStripItem FindBlockoutToolStripItem(string name)
    {
        var toolStripItems = menuBlockout.Items.Find(name, false);
        if (toolStripItems.Length > 0)
        {
            return toolStripItems[0];
        }
        
        var patient = "No Patient Selected";
        if (_patient is not null)
        {
            patient = _patient.PatNum.ToString();
        }

        throw new ODException(
            $"""
             Patnum: {patient}
             OD Version: {Application.ProductVersion}
             Missing ToolStripItem: {name}
             """);

    }

    private void ContrApptPanel_ApptMainAreaRightClicked(object sender, ApptMainClickEventArgs e)
    {
        var toolStripItemEdit = FindBlockoutToolStripItem(MenuItemNames.EditBlockout);
        var toolStripItemCut = FindBlockoutToolStripItem(MenuItemNames.CutBlockout);
        var toolStripItemCopy = FindBlockoutToolStripItem(MenuItemNames.CopyBlockout);
        var toolStripItemPaste = FindBlockoutToolStripItem(MenuItemNames.PasteBlockout);
        var toolStripItemDelete = FindBlockoutToolStripItem(MenuItemNames.DeleteBlockout);
        //AddBlockout is not used here
        var toolStripItemCutCopyPaste = FindBlockoutToolStripItem(MenuItemNames.BlockoutCutCopyPaste);
        ToolStripItem toolStripItemClearForDay = new ToolStripMenuItem();
        ToolStripItem toolStripItemClearForDayOp = new ToolStripMenuItem();
        ToolStripItem toolStripItemClearForDayClinics = new ToolStripMenuItem();
        //No clear for day if clinics enabled
        toolStripItemClearForDayOp = FindBlockoutToolStripItem(MenuItemNames.ClearAllBlockoutsForDayOpOnly);
        toolStripItemClearForDayClinics = FindBlockoutToolStripItem(MenuItemNames.ClearAllBlockoutsForDayClinicOnly);

        if (!Security.IsAuthorized(EnumPermType.Blockouts, true))
        {
            toolStripItemCutCopyPaste.Enabled = false;
            toolStripItemClearForDay.Enabled = false;
            toolStripItemClearForDayOp.Enabled = false;
            toolStripItemClearForDayClinics.Enabled = false;
        }
        else if (Security.IsAuthorized(EnumPermType.Blockouts, true))
        {
            toolStripItemCutCopyPaste.Enabled = true;
            toolStripItemClearForDay.Enabled = true;
            toolStripItemClearForDayOp.Enabled = true;
            toolStripItemClearForDayClinics.Enabled = true;
        }

        _opNumClickedBlockout = e.OpNum;
        _dateTimeClickedBlockout = e.DateT;
        var clickedOnBlockCount = 0;
        var blockoutFlags = "";
        var ListSchedulesBlockout = Schedules.GetListForType(contrApptPanel.ListSchedules, ScheduleType.Blockout, 0);
        //List<ScheduleOp> listForSched;
        for (var i = 0; i < ListSchedulesBlockout.Count; i++)
        {
            if (ListSchedulesBlockout[i].SchedDate.Date != e.DateT.Date)
            {
                continue;
            }

            if (ListSchedulesBlockout[i].StartTime > e.DateT.TimeOfDay
                || ListSchedulesBlockout[i].StopTime <= e.DateT.TimeOfDay)
            {
                continue;
            }

            //listForSched=ScheduleOps.GetForSched(ListForType[i].ScheduleNum);
            for (var p = 0; p < ListSchedulesBlockout[i].Ops.Count; p++)
            {
                if (ListSchedulesBlockout[i].Ops[p] == e.OpNum)
                {
                    clickedOnBlockCount++;
                    blockoutFlags = Defs.GetDef(DefCat.BlockoutTypes, ListSchedulesBlockout[i].BlockoutType).ItemValue;
                    break; //out of ops loop
                }
            }
        }

        if (clickedOnBlockCount > 0)
        {
            toolStripItemPaste.Enabled = false; //Can't paste on top of an existing blockout
            toolStripItemEdit.Enabled = true;
            toolStripItemCopy.Enabled = true;
            toolStripItemCut.Enabled = true;
            toolStripItemDelete.Enabled = true;
            if (blockoutFlags.Contains(BlockoutType.DontCopy.GetDescription()))
            {
                //users without blockout permission are still allowed to add and edit this blockout. 
                toolStripItemCut.Enabled = false;
                toolStripItemCopy.Enabled = false;
            }
            else if (blockoutFlags.Contains(BlockoutType.NoSchedule.GetDescription()))
            {
                //users without blockout permission are still allowed to add and edit this blockout.
                if (!Security.IsAuthorized(EnumPermType.Blockouts, true))
                {
                    toolStripItemCut.Enabled = false;
                    toolStripItemCopy.Enabled = false;
                }
            }
            else
            {
                //this is not a blockout type that this user is allowed to edit. 
                if (!Security.IsAuthorized(EnumPermType.Blockouts, true))
                {
                    toolStripItemEdit.Enabled = false;
                    toolStripItemCopy.Enabled = false;
                    toolStripItemCut.Enabled = false;
                    toolStripItemDelete.Enabled = false;
                }
            }

            if (clickedOnBlockCount > 1)
            {
                var message = Lan.g(this, "There are multiple blockouts in this slot.  You should try to delete or move one of them.");
                var formPopupFade = new FormPopupFade(message);
                formPopupFade.Show();
            }
        }
        else
        {
            //Not clicked on blockout
            toolStripItemEdit.Enabled = false;
            toolStripItemCut.Enabled = false;
            toolStripItemCopy.Enabled = false;
            if (_scheduleBlockoutClipboard == null)
            {
                toolStripItemPaste.Enabled = false;
            }
            else
            {
                toolStripItemPaste.Enabled = true;
            }

            toolStripItemDelete.Enabled = false;
        }

        var isTextingEnabled = SmsPhones.IsIntegratedTextingEnabled();

        if (isTextingEnabled)
        {
            textASAPContextMenuHelper(true, "Text ASAP List (manual)");
        }
        else
        {
            textASAPContextMenuHelper(false);
        }

        SetMenuItemProperty(menuBlockout, MenuItemNames.DeleteWebSchedAsapBlockout, x => x.Visible = false);
        SetMenuItemProperty(menuBlockout, MenuItemNames.TextApptsForDayOp, x => x.Visible = isTextingEnabled);
        SetMenuItemProperty(menuBlockout, MenuItemNames.TextApptsForDayView, x => x.Visible = isTextingEnabled);
        SetMenuItemProperty(menuBlockout, MenuItemNames.TextApptsForDay, x =>
        {
            x.Visible = isTextingEnabled;
            x.Text = MenuItemNames.TextApptsForDay + ", Clinic only";
        });
        
        menuBlockout.Show(contrApptPanel, e.Location);
    }

    private void textASAPContextMenuHelper(bool isVisible, string itemText = "")
    {
        var toolStripItemArray = menuBlockout.Items.Find(MenuItemNames.TextAsapList, false);
        if (toolStripItemArray.Length > 0)
        {
            toolStripItemArray[0].Visible = isVisible;
            toolStripItemArray[0].Text = Lans.g(itemText);
        }
    }

    private void contrApptPanel_ApptMoved(object sender, ApptMovedEventArgs e)
    {
        var appointment = e.Appt;
        var appointmentOld = e.ApptOld;
        var listProceduresOld = Procedures.GetProcsForSingle(appointment.AptNum, false); //get the procedures on the appointment before they are updated
        MoveAppointment(appointment, appointmentOld); //This does a lot.  Many nested calls, including to SetProvidersInAppointment.

        #region Update UI and cache

        var procFeeHelper = new ProcFeeHelper(e.Appt.PatNum);
        //check if the proc fees on the moved appointment need updating
        var isUpdatingFees = false;
        var listProceduresNew = listProceduresOld.Select(x => Procedures.ChangeProcInAppointment(appointment, x.Copy())).ToList();
        if (listProceduresOld.Exists(x => x.ProvNum != listProceduresNew.FirstOrDefault(y => y.ProcNum == x.ProcNum).ProvNum))
        {
            //Either the primary or hygienist changed.
            var promptText = "";
            isUpdatingFees = Procedures.ShouldFeesChange(listProceduresNew, listProceduresOld, ref promptText, procFeeHelper);
            if (isUpdatingFees)
            {
                //Made it pass the pref check.
                if (promptText != "" && !MsgBox.Show(this, MsgBoxButtons.YesNo, promptText))
                {
                    //prompt is fixed text
                    isUpdatingFees = false;
                }
            }
        }

        Procedures.SetProvidersInAppointment(appointment, listProceduresOld, isUpdatingFees, procFeeHelper); //to update fees to db
        RefreshModuleDataPatient(appointment.PatNum);
        GlobalFormOpenDental.PatientSelected(_patient, true, false);
        RefreshPeriod();
        Recalls.SynchScheduledApptFull(appointment.PatNum);
        ODEvent.Fire(ODEventType.AppointmentEdited, appointment);

        #endregion Update UI and cache
    }

    private void contrApptPanel_ApptMovedToPinboard(object sender, ApptDataRowEventArgs e)
    {
        SendToPinboardDataRow(e.DataRowAppt);
        
        if (PrefC.GetBool(PrefName.BrokenApptRequiredOnMove))
        {
            RefreshPinboardImages();
        }
    }

    private void contrApptPanel_ApptNullFound(object sender, EventArgs e)
    {
        MsgBox.Show(this, "Selected appointment no longer exists.");
        
        RefreshPeriod();
    }

    private void contrApptPanel_ApptResized(object sender, ApptEventArgs e)
    {
        RefreshModuleDataPatient(e.Appt.PatNum);
        GlobalFormOpenDental.PatientSelected(_patient, true, false);
        RefreshPeriod();
        ODEvent.Fire(ODEventType.AppointmentEdited, e.Appt);
    }

    private void ContrApptPanel_ApptRightClicked(object sender, ApptRightClickEventArgs e)
    {
        var dataRowAppointment = contrApptPanel.TableAppointments.Select().FirstOrDefault(x => SIn.Long(x["AptNum"].ToString()) == contrApptPanel.SelectedAptNum);
        menuApt.Items.RemoveByKey(MenuItemNames.Tasks);
        menuApt.Items.RemoveByKey(MenuItemNames.TasksSpacer);
        menuApt.Items.Add(new ToolStripSeparator {Name = MenuItemNames.TasksSpacer});
        var menuTasks = new ToolStripMenuItem("Appointment Tasks", null, menuTasks_Click, MenuItemNames.Tasks);
        menuApt.Items.Add(menuTasks);
        menuApt.Items.RemoveByKey(MenuItemNames.PhoneDiv);
        menuApt.Items.RemoveByKey(MenuItemNames.HomePhone);
        menuApt.Items.RemoveByKey(MenuItemNames.WorkPhone);
        menuApt.Items.RemoveByKey(MenuItemNames.WirelessPhone);
        menuApt.Items.RemoveByKey(MenuItemNames.TextDiv);
        menuApt.Items.RemoveByKey(MenuItemNames.SendText);
        menuApt.Items.RemoveByKey(MenuItemNames.SendConfirmationText);
        menuApt.Items.RemoveByKey(MenuItemNames.SendComeInText);
        menuApt.Items.RemoveByKey(MenuItemNames.SendMessageToPay);
        menuApt.Items.RemoveByKey(MenuItemNames.OrthoChart);
        ToolStripItem menuItem;
        if (PrefC.GetBool(PrefName.ApptModuleShowOrthoChartItem))
        {
            menuApt.Items.Add(new ToolStripMenuItem(Lan.g(this, "Go To ") + OrthoChartTabs.GetFirst(true).TabName, null, menuApt_Click, MenuItemNames.OrthoChart));
        }

        //Phone numbers
        //The menu items to "Call Home/Work/Cell" will only be added to this context menu if action will actually be taken when clicking on them, i.e.
        //when DentalTek bridge is disabled and advertising is disabled, nothing happens when clicking this buttons.
        if (Programs.GetCur(ProgramName.DentalTekSmartOfficePhone).Enabled
            || !ProgramProperties.IsAdvertisingDisabled(ProgramName.DentalTekSmartOfficePhone))
        {
            if (!string.IsNullOrEmpty(_patient.HmPhone) || !string.IsNullOrEmpty(_patient.WkPhone) || !string.IsNullOrEmpty(_patient.WirelessPhone))
            {
                menuApt.Items.Add(new ToolStripSeparator {Name = MenuItemNames.PhoneDiv});
            }

            if (!string.IsNullOrEmpty(_patient.HmPhone))
            {
                menuApt.Items.Add(new ToolStripMenuItem("Call Home Phone " + _patient.HmPhone, null, menuApt_Click, MenuItemNames.HomePhone));
            }

            if (!string.IsNullOrEmpty(_patient.WkPhone))
            {
                menuApt.Items.Add(new ToolStripMenuItem("Call Work Phone " + _patient.WkPhone, null, menuApt_Click, MenuItemNames.WorkPhone));
            }

            if (!string.IsNullOrEmpty(_patient.WirelessPhone))
            {
                menuApt.Items.Add(new ToolStripMenuItem("Call Wireless Phone " + _patient.WirelessPhone, null, menuApt_Click, MenuItemNames.WirelessPhone));
            }
        }

        //Texting
        menuApt.Items.Add(new ToolStripSeparator {Name = MenuItemNames.TextDiv});
        menuItem = new ToolStripMenuItem("Send Text", null, menuApt_Click, MenuItemNames.SendText);
        menuApt.Items.Add(menuItem);
        if (!SmsPhones.IsIntegratedTextingEnabled() && !Programs.IsEnabled(ProgramName.CallFire))
        {
            menuItem.Enabled = false;
        }

        menuItem = new ToolStripMenuItem("Send Confirmation Text", null, menuApt_Click, MenuItemNames.SendConfirmationText);
        menuApt.Items.Add(menuItem);
        if (!SmsPhones.IsIntegratedTextingEnabled() && !Programs.IsEnabled(ProgramName.CallFire))
        {
            menuItem.Enabled = false;
        }

        menuItem = new ToolStripMenuItem(MenuItemNames.SendComeInText, null, menuApt_Click, MenuItemNames.SendComeInText);
        menuApt.Items.Add(menuItem);
        if (!GetArrivalsLoaded().HasComeInMsg(contrApptPanel.SelectedAptNum))
        {
            menuItem.Enabled = false;
        }
        else if (!SmsPhones.IsIntegratedTextingEnabled() && !Programs.IsEnabled(ProgramName.CallFire))
        {
            menuItem.Enabled = false;
        }

        menuItem = new ToolStripMenuItem(Lan.g(this, MenuItemNames.SendMessageToPay), null, menuApt_Click, MenuItemNames.SendMessageToPay);
        menuApt.Items.Add(menuItem);
        if (!SmsPhones.IsIntegratedTextingEnabled())
        {
            //Does not require a completed appt, only requires texting.
            menuItem.Enabled = false;
        }

        menuApt.Show(contrApptPanel, e.Location);
    }

    private void ContrApptPanel_DateChanged(object sender, EventArgs e)
    {
        SetWeeklyView(contrApptPanel.IsWeeklyView); //because weekly view changed internally as well.
    }

    private void contrApptPanel_SelectedApptChanged(object sender, ApptSelectedChangedEventArgs e)
    {
        pinBoard.SelectedIndex = -1;
        if (e.AptNumNew == -1)
        {
            RefreshModuleScreenButtonsRight(); //just disables the buttons on the right
            //we will leave current patient selected.
            return;
        }

        ;
        if (_patient == null || _patient.PatNum != e.PatNumNew)
        {
            //patient changed
            RefreshModuleDataPatient(e.PatNumNew); //this clears selected appt because pt changed.
            contrApptPanel.SelectedAptNum = e.AptNumNew; //reselect appt
            if (_patient.PatStatus == PatientStatus.Deleted)
            {
                var patientOld = Patients.GetPat(e.PatNumNew);
                _patient.PatStatus = PatientStatus.Archived;
                if (Patients.Update(_patient, patientOld))
                {
                    MsgBox.Show("Patient has been set to archived because they were deleted by another user while making this appointment.");
                }
            }

            GlobalFormOpenDental.PatientSelected(_patient, true, false);
            return;
        }

        //patient not changed
        if (e.AptNumNew != e.AptNumOld)
        {
            RefreshModuleScreenButtonsRight(); //otherwise included above in RefreshModuleDataPatient
        }
    }

    private void contrApptPanel_MouseMoved(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.None)
        {
            MouseUpForced();
        }
    }

    private void SetMenuItemProperty(ContextMenuStrip contextMenu, string menuItemName, Action<ToolStripItem> actionSetMenuItem)
    {
        var toolStripItemArray = contextMenu.Items.Find(menuItemName, false);
        if (toolStripItemArray.Length > 0)
        {
            actionSetMenuItem(toolStripItemArray[0]);
        }
    }

    private void FormIVL_FormClosed(object sender, FormClosedEventArgs e)
    {
        RefreshModuleDataPeriod();
        RefreshModuleScreenPeriod();
    }

    private void ListASAP_Click()
    {
        if (_formAsap == null || _formAsap.IsDisposed)
        {
            _formAsap = new FormASAP();
        }

        _formAsap.Show();
        if (_formAsap.WindowState == FormWindowState.Minimized)
        {
            _formAsap.WindowState = FormWindowState.Normal;
        }

        _formAsap.BringToFront();
    }

    private void ListConfirm_Click()
    {
        if (_formConfirmList == null || _formConfirmList.IsDisposed)
        {
            _formConfirmList = new FormConfirmList();
        }

        _formConfirmList.Show();
        if (_formConfirmList.WindowState == FormWindowState.Minimized)
        {
            _formConfirmList.WindowState = FormWindowState.Normal;
        }

        _formConfirmList.BringToFront();
    }

    private void ListInsVerify_Click()
    {
        var listFormInsVerificationLists = Application.OpenForms.OfType<FormInsVerificationList>().ToList();
        if (listFormInsVerificationLists.Count > 0)
        {
            listFormInsVerificationLists[0].FillControls();
            listFormInsVerificationLists[0].BringToFront();
            return;
        }

        if (Security.IsAuthorized(EnumPermType.InsuranceVerification))
        {
            var formInsVerificationList = new FormInsVerificationList();
            formInsVerificationList.FormClosed += FormIVL_FormClosed;
            formInsVerificationList.Show();
        }
    }

    private void ListPlanned_Click()
    {
        if (_formTrackNext == null || _formTrackNext.IsDisposed)
        {
            _formTrackNext = new FormTrackNext();
        }

        _formTrackNext.Show();
        if (_formTrackNext.WindowState == FormWindowState.Minimized)
        {
            _formTrackNext.WindowState = FormWindowState.Normal;
        }

        _formTrackNext.BringToFront();
    }

    private void ListRadiology_Click()
    {
        var listFormRadOrderLists = Application.OpenForms.OfType<FormRadOrderList>().ToList();
        if (listFormRadOrderLists.Count > 0)
        {
            listFormRadOrderLists[0].RefreshRadOrdersForUser(Security.CurUser);
            listFormRadOrderLists[0].BringToFront();
            return;
        }

        var formRadOrderList = new FormRadOrderList(Security.CurUser);
        formRadOrderList.Show();
    }

    private void ListRecall_Click()
    {
        if (_formRecallList == null || _formRecallList.IsDisposed)
        {
            _formRecallList = new FormRecallList();
        }

        _formRecallList.Show();
        if (_formRecallList.WindowState == FormWindowState.Minimized)
        {
            _formRecallList.WindowState = FormWindowState.Normal;
        }

        _formRecallList.BringToFront();
    }

    private void ListUnsched_Click()
    {
        //Reselect existing window if available, if not create a new instance
        if (_formUnsched == null || _formUnsched.IsDisposed)
        {
            _formUnsched = new FormUnsched();
        }

        _formUnsched.Show();
        if (_formUnsched.WindowState == FormWindowState.Minimized)
        {
            //only applicable if re-using an existing instance
            _formUnsched.WindowState = FormWindowState.Normal;
        }

        _formUnsched.BringToFront();
    }

    private void toolBarLists_Click()
    {
        var frmApptLists = new FrmApptLists();
        frmApptLists.ShowDialog();
        if (!frmApptLists.IsDialogOK)
        {
            return;
        }

        switch (frmApptLists.ApptListSelectionResult)
        {
            case ApptListSelection.Recall:
                ListRecall_Click();
                break;
            case ApptListSelection.Confirm:
                ListConfirm_Click();
                break;
            case ApptListSelection.Planned:
                ListPlanned_Click();
                break;
            case ApptListSelection.Unsched:
                ListUnsched_Click();
                break;
            case ApptListSelection.ASAP:
                ListASAP_Click();
                break;
            case ApptListSelection.Radiology:
                ListRadiology_Click();
                break;
            case ApptListSelection.InsVerify:
                ListInsVerify_Click();
                break;
        }
    }

    private void toolBarMain_ButtonClick(object sender, ODToolBarButtonClickEventArgs e)
    {
        if (e.Button.Tag.GetType() == typeof(string))
        {
            //standard predefined button
            switch (e.Button.Tag.ToString())
            {
                case "Print":
                    toolBarPrint_Click();
                    break;
                case "Lists":
                    toolBarLists_Click();
                    break;
                case "Unsched":
                    butUnsched_Click();
                    break;
                case "Break":
                    butBreak_Click();
                    break;
                case "Complete":
                    butComplete_Click();
                    break;
                case "Delete":
                    butDelete_Click(showPrompt: true);
                    break;
                case "PatAppts":
                    DisplayOtherDlg(false);
                    break;
                case "Make":
                    butMakeAppt_Click(this, EventArgs.Empty);
                    break;
                case "Recall":
                    butMakeRecall_Click(this, EventArgs.Empty);
                    break;
                //Family recall handled in context menu
                case "RapidCall":
                    try
                    {
                        RapidCall.ShowPage();
                    }
                    catch (Exception ex)
                    {
                        ODMessageBox.Show(ex.Message);
                    }

                    break;
            }

            return;
        }

        if (e.Button.Tag.GetType() == typeof(Program))
        {
            Patient patient = null;
            if (_patient != null)
            {
                patient = Patients.GetPat(_patient.PatNum);
            }

            ProgramL.Execute(((Program) e.Button.Tag).ProgramNum, patient);
        }
    }

    private void toolBarPrint_Click()
    {
        if (contrApptPanel.ListOpsVisible.Count == 0 || contrApptPanel.ListDayOfWeeks.IsNullOrEmpty())
        {
            MsgBox.Show(this, "There must be at least one operatory showing in order to Print Appointments.");
            return;
        }

        var listOperatoryNums = contrApptPanel.ListOpsVisible.Select(x => x.OperatoryNum).ToList();
        
        var listAptNums = contrApptPanel.TableAppointments.Select()
            .Where(x => listOperatoryNums.Contains(SIn.Long(x["Op"].ToString())))
            .OrderBy(x => SIn.DateTime(x["AptDateTime"].ToString()))
            .Select(x => SIn.Long(x["AptNum"].ToString()))
            .ToList();
        
        using var formApptPrintSetup = new FormApptPrintSetup(listAptNums, contrApptPanel.DateSelected, contrApptPanel.IsWeeklyView);

        if (formApptPrintSetup.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        contrApptPanel.DateTimePrintStart = formApptPrintSetup.DateTimeApptPrintStart;
        contrApptPanel.DateTimePrintStop = formApptPrintSetup.DateTimeApptPrintStop;
        contrApptPanel.PrintingSizeFont = formApptPrintSetup.ApptPrintFontSize;
        contrApptPanel.PrintingColsPerPage = formApptPrintSetup.ApptPrintColsPerPage;
        contrApptPanel.IsPrintPreview = formApptPrintSetup.IsPrintPreview;
        contrApptPanel.PrintColorBehavior = formApptPrintSetup.PrintColorBehavior;
        contrApptPanel.PagesPrinted = 0;
        contrApptPanel.PrintingPageRow = 0;
        contrApptPanel.PrintingPageColumn = 0;
        
        var dateTimePrintStart = formApptPrintSetup.DateTimeApptPrintStart;

        var printoutOrientation = PrintoutOrientation.Portrait;
        if (formApptPrintSetup.IsLandscape)
        {
            printoutOrientation = PrintoutOrientation.Landscape;
        }

        PrinterL.TryPrintOrDebugClassicPreview(contrApptPanel.PrintPage,
            "Daily appointment view for " + dateTimePrintStart.ToShortDateString() + " printed",
            totalPages: 0,
            printSituation: PrintSituation.Appointments,
            printoutOrientation: printoutOrientation, isForcedPreview: contrApptPanel.IsPrintPreview);
        
        if (_patient == null)
        {
            ModuleSelected(0);
            return;
        }

        ModuleSelected(_patient.PatNum);
    }

    private void butToday_Click(object sender, EventArgs e)
    {
        ModuleSelected(DateTime.Today);
    }

    private void butBack_Click(object sender, EventArgs e)
    {
        ModuleSelected(contrApptPanel.DateSelected.AddDays(-1));
    }

    private void butFwd_Click(object sender, EventArgs e)
    {
        ModuleSelected(contrApptPanel.DateSelected.AddDays(1));
    }

    private void butBackWeek_Click(object sender, EventArgs e)
    {
        butBackWeek.Enabled = false;
        Application.DoEvents();
        ModuleSelected(contrApptPanel.DateSelected.AddDays(-7));
        butBackWeek.Enabled = true;
    }

    private void butFwdWeek_Click(object sender, EventArgs e)
    {
        ModuleSelected(contrApptPanel.DateSelected.AddDays(7));
    }

    private void butBackMonth_Click(object sender, EventArgs e)
    {
        ModuleSelected(contrApptPanel.DateSelected.AddMonths(-1));
    }

    private void butFwdMonth_Click(object sender, EventArgs e)
    {
        ModuleSelected(contrApptPanel.DateSelected.AddMonths(1));
    }

    private void butFwd3_Click(object sender, EventArgs e)
    {
        ModuleSelected(contrApptPanel.DateSelected.AddMonths(3));
    }

    private void butFwd4_Click(object sender, EventArgs e)
    {
        ModuleSelected(contrApptPanel.DateSelected.AddMonths(4));
    }

    private void butFwd6_Click(object sender, EventArgs e)
    {
        ModuleSelected(contrApptPanel.DateSelected.AddMonths(6));
    }

    private void Calendar2_DateSelected(object sender, EventArgs e)
    {
        ModuleSelected(monthCalendarOD.GetDateSelected());
    }
    
    private void comboView_SelectionChangeCommitted(object sender, EventArgs e)
    {
        ComboViewChanged();
    }

    private void toggleDayWeek_DayClick(object sender, EventArgs e)
    {
        SetWeeklyView(false);
    }

    private void toggleDayWeek_WeekClick(object sender, EventArgs e)
    {
        SetWeeklyView(true);
    }
    
    #region Methods - Event Handlers PanelCalendar Buttons

    //Left Buttons------------------------------------------------------------------------------------------------
    private void butBreak_Click()
    {
        if (PrefC.GetBool(PrefName.BrokenApptAdjustment)
            && PrefC.GetLong(PrefName.BrokenAppointmentAdjustmentType) == 0)
        {
            //They want broken appointment adjustments but don't have it set up.
            MsgBox.Show(this, "Broken appointment adjustment type is not setup yet.  Please go to Setup | Appointment | Appts Preferences to fix this.");
            return;
        }

        if (contrApptPanel.SelectedAptNum == -1)
        {
            MsgBox.Show(this, "Please select an appointment first.");
            return;
        }

        var appointment = Appointments.GetOneApt(contrApptPanel.SelectedAptNum);
        if (ApptIsNull(appointment))
        {
            return;
        }

        var patient = Patients.GetPat(appointment.PatNum);
        if (appointment.AptStatus != ApptStatus.Complete && !Security.IsAuthorized(EnumPermType.AppointmentEdit))
        {
            //separate permissions for completed appts.
            return;
        }

        if (appointment.AptStatus == ApptStatus.Complete && !Security.IsAuthorized(EnumPermType.AppointmentCompleteEdit))
        {
            return;
        }

        if (appointment.AptStatus == ApptStatus.PtNote || appointment.AptStatus == ApptStatus.PtNoteCompleted)
        {
            MsgBox.Show(this, "Patient Notes cannot be broken.");
            return;
        }

        ProcedureCode procedureCode = null; //Will not chart if it stays null.
        var apptBreakSelection = ApptBreakSelection.None;
        var hasBrokenProcs = AppointmentL.HasBrokenApptProcs(); //When true, we show FormApptBreak.cs
        if (hasBrokenProcs)
        {
            //If true, user cannot get here from right click 'Break Appointment' directly.
            using var formApptBreak = new FormApptBreak(appointment);
            if (formApptBreak.ShowDialog() != DialogResult.OK)
            {
                if (formApptBreak.SelectedApptBreak == ApptBreakSelection.Delete)
                {
                    //User wants to delete the appointment.
                    butDelete_Click(showPrompt: false);
                }

                return;
            }

            procedureCode = formApptBreak.SelectedProcedureCode;
            apptBreakSelection = formApptBreak.SelectedApptBreak;
        }
        else if (!MsgBox.Show(this, MsgBoxButtons.OKCancel, "Break appointment?"))
        {
            return;
        }

        //This hook is specifically called after we know a valid appointment has been identified.
        AppointmentL.BreakApptHelper(appointment, patient, procedureCode);
        if (hasBrokenProcs)
        {
            //FormApptBreak was shown and user made a selection
            switch (apptBreakSelection)
            {
                case ApptBreakSelection.Unsched:
                    if (AppointmentL.ValidateApptUnsched(appointment))
                    {
                        AppointmentL.SetApptUnschedHelper(appointment, patient, false);
                    }

                    break;
                case ApptBreakSelection.Pinboard:
                    if (AppointmentL.ValidateApptToPinboard(appointment))
                    {
                        AppointmentL.CopyAptToPinboardHelper(appointment);
                    }

                    break;
                case ApptBreakSelection.ApptBook:
                    //Intentionally blank.
                    break;
            }
        }

        ModuleSelected(patient.PatNum); //Must be ran after the "D9986" break logic due to the addition of a completed procedure.
    }

    private void butComplete_Click()
    {
        if (!Security.IsAuthorized(EnumPermType.AppointmentEdit))
        {
            return;
        }

        if (contrApptPanel.SelectedAptNum == -1)
        {
            MsgBox.Show(this, "Please select an appointment first.");
            return;
        }

        var appointment = Appointments.GetOneApt(contrApptPanel.SelectedAptNum);
        if (ApptIsNull(appointment))
        {
            return;
        }

        var patient = Patients.GetPat(appointment.PatNum);
        if (appointment.AptDateTime.Date > DateTime.Today)
        {
            if (!PrefC.GetBool(PrefName.ApptAllowFutureComplete))
            {
                MsgBox.Show(this, "Not allowed to set future appointments complete.");
                return;
            }
        }

        var listProcedures = Procedures.GetProcsForSingle(appointment.AptNum, false);
        var listHiddenProcCodes = ProcedureCodes.GetProcCodesInHiddenCats(listProcedures.Select(x => x.CodeNum).ToArray());
        if (listHiddenProcCodes.Count > 0)
        {
            MsgBox.Show(Lan.g(this, "Cannot complete appointment because the following procedures are in a hidden category:") + " " + string.Join(", ", listHiddenProcCodes));
            return;
        }

        if (appointment.AptStatus != ApptStatus.PtNote && appointment.AptStatus != ApptStatus.PtNoteCompleted //Ptnote cannot have procs attached
                                                       && !PrefC.GetBool(PrefName.ApptAllowEmptyComplete) //Appointments must have at least 1 proc
                                                       && listProcedures.Count == 0)
        {
            MsgBox.Show(this, "Appointments without procedures attached cannot be set complete.");
            return;
        }

        if (appointment.AptStatus == ApptStatus.PtNoteCompleted)
        {
            return;
        }

        if (ProcedureCodes.DoAnyBypassLockDate())
        {
            for (var i = 0; i < listProcedures.Count; i++)
            {
                if (!Security.IsAuthorized(EnumPermType.ProcComplCreate, appointment.AptDateTime, listProcedures[i].CodeNum, listProcedures[i].ProcFee))
                {
                    return;
                }
            }
        }
        else if (!Security.IsAuthorized(EnumPermType.ProcComplCreate, appointment.AptDateTime))
        {
            return;
        }

        if (listProcedures.Count > 0 && appointment.AptDateTime.Date > DateTime.Today.Date && !PrefC.GetBool(PrefName.FutureTransDatesAllowed))
        {
            MsgBox.Show(this, "Not allowed to set procedures complete with future dates.");
            return;
        }

        #region Provider Term Date Check

        //Prevents appointments with providers that are past their term end date from being completed
        var message = Providers.CheckApptProvidersTermDates(appointment, isSetComplete: true);
        if (message != "")
        {
            MsgBox.Show(message);
            return;
        }

        #endregion Provider Term Date Check

        var removeCompletedProcs = ProcedureL.DoRemoveCompletedProcs(appointment, listProcedures.FindAll(x => x.ProcStatus == ProcStat.C));
        var apptStatusOld = appointment.AptStatus;
        var result = Appointments.CompleteClick(appointment, listProcedures, removeCompletedProcs);
        appointment = result.Item1;
        listProcedures = result.Item2;
        if (apptStatusOld != ApptStatus.Complete && appointment.AptStatus == ApptStatus.Complete)
        {
            AutomationL.Trigger(EnumAutomationTrigger.ApptComplete, null, appointment.PatNum);
        }

        if (appointment.AptStatus != ApptStatus.PtNote)
        {
            AutomationL.Trigger(EnumAutomationTrigger.ProcedureComplete, listProcedures.Select(x => ProcedureCodes.GetStringProcCode(x.CodeNum)).ToList(), appointment.PatNum);
        }

        var listProceduresForAppt = Procedures.GetProcsForSingle(appointment.AptNum, false); //The procedures were never updated, fetch from db.
        Procedures.AfterProcsSetComplete(listProcedures);
        ModuleSelected(appointment.PatNum);
        ODEvent.Fire(ODEventType.AppointmentEdited, appointment);
        //If necessary, prompt the user to ask the patient to opt in to using Short Codes.
        FrmShortCodeOptIn.PromptIfNecessary(patient, appointment.ClinicNum);
        Signalods.SetInvalid(InvalidType.BillingList);
    }

    private void butDelete_Click(bool showPrompt)
    {
        if (contrApptPanel.SelectedAptNum == -1)
        {
            MsgBox.Show(this, "Please select an appointment first.");
            return;
        }

        var appointment = Appointments.GetOneApt(contrApptPanel.SelectedAptNum);
        if (ApptIsNull(appointment))
        {
            return;
        }

        if ((appointment.AptStatus != ApptStatus.Complete && !Security.IsAuthorized(EnumPermType.AppointmentDelete))
            || (appointment.AptStatus == ApptStatus.Complete && !Security.IsAuthorized(EnumPermType.AppointmentCompleteDelete)))
        {
            return;
        }

        var dataRow = contrApptPanel.GetDataRowForSelected();
        if (dataRow == null)
        {
            MsgBox.Show(this, "Appointment not found.");
            return;
        }

        if (AppointmentL.DoPreventChangesToCompletedAppt(appointment, PreventChangesApptAction.Delete))
        {
            return;
        }

        if (appointment.AptStatus == ApptStatus.PtNote | appointment.AptStatus == ApptStatus.PtNoteCompleted)
        {
            if (showPrompt && !MsgBox.Show(this, MsgBoxButtons.OKCancel, "Delete Patient Note?"))
            {
                return;
            }

            if (appointment.Note != "")
            {
                if (ODMessageBox.Show(Commlogs.GetDeleteApptCommlogMessage(appointment.Note, appointment.AptStatus), Lan.g(this, "Question..."), MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var commlog = new Commlog();
                    commlog.PatNum = appointment.PatNum;
                    commlog.CommDateTime = DateTime.Now;
                    commlog.CommType = Commlogs.GetTypeAuto(CommItemTypeAuto.APPT);
                    commlog.Note = Lan.g(this, "Deleted Patient NOTE from schedule, saved copy: ");
                    commlog.Note += appointment.Note;
                    commlog.UserNum = Security.CurUser.UserNum;
                    //there is no dialog here because it is just a simple entry
                    Commlogs.Insert(commlog);
                }
            }

            SecurityLogs.MakeLogEntry(EnumPermType.AppointmentEdit, _patient.PatNum,
                dataRow["procs"] + ", " + dataRow["AptDateTime"] + ", " + Lan.g(this, "NOTE Deleted"),
                appointment.AptNum, appointment.DateTStamp);
        }
        else
        {
            if (showPrompt && !MsgBox.Show(this, MsgBoxButtons.OKCancel, "Delete Appointment?"))
            {
                return;
            }

            if (appointment.Note != "")
            {
                if (ODMessageBox.Show(Commlogs.GetDeleteApptCommlogMessage(appointment.Note, appointment.AptStatus), Lan.g(this, "Question..."), MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var commlog = new Commlog();
                    commlog.PatNum = appointment.PatNum;
                    commlog.CommDateTime = DateTime.Now;
                    commlog.CommType = Commlogs.GetTypeAuto(CommItemTypeAuto.APPT);
                    commlog.Note = "Deleted Appointment & saved note: ";
                    if (appointment.ProcDescript != "")
                    {
                        commlog.Note += appointment.ProcDescript + ": ";
                    }

                    commlog.Note += appointment.Note;
                    commlog.UserNum = Security.CurUser.UserNum;
                    //there is no dialog here because it is just a simple entry
                    Commlogs.Insert(commlog);
                }
            }

            if (appointment.AptStatus == ApptStatus.Complete)
            {
                // seperate log entry for editing completed appointments.
                SecurityLogs.MakeLogEntry(EnumPermType.AppointmentCompleteDelete, _patient.PatNum,
                    dataRow["procs"] + ", " + dataRow["AptDateTime"] + ", " + Lan.g(this, "Deleted"),
                    appointment.AptNum, appointment.DateTStamp);
            }
            else
            {
                SecurityLogs.MakeLogEntry(EnumPermType.AppointmentDelete, _patient.PatNum,
                    dataRow["procs"] + ", " + dataRow["AptDateTime"] + ", " + Lan.g(this, "Deleted"),
                    appointment.AptNum, appointment.DateTStamp);
            }

            //If there is an existing HL7 def enabled, send a SIU message if there is an outbound SIU message defined
            if (HL7Defs.IsExistingHL7Enabled())
            {
                //S17 - Appt Deletion event
                var messageHL7 = MessageConstructor.GenerateSIU(_patient, Patients.GetPat(_patient.Guarantor), EventTypeHL7.S17, appointment);
                //Will be null if there is no outbound SIU message defined, so do nothing
                if (messageHL7 != null)
                {
                    var hl7Msg = new HL7Msg();
                    hl7Msg.AptNum = appointment.AptNum;
                    hl7Msg.HL7Status = HL7MessageStatus.OutPending; //it will be marked outSent by the HL7 service.
                    hl7Msg.MsgText = messageHL7.ToString();
                    hl7Msg.PatNum = _patient.PatNum;
                    HL7Msgs.Insert(hl7Msg);
                }
            }

            if (HieClinics.IsEnabled())
            {
                HieQueues.Insert(new HieQueue(_patient.PatNum));
            }
        }

        if (!DoApptBreakRequired(appointment))
        {
            return;
        }

        Appointments.Delete(contrApptPanel.SelectedAptNum, true); //Appointments S-Class handles Signalods
        ODEvent.Fire(ODEventType.AppointmentEdited, appointment);
        contrApptPanel.SelectedAptNum = -1;
        pinBoard.SelectedIndex = -1;
        for (var i = 0; i < pinBoard.ListPinBoardItems.Count; i++)
        {
            if (appointment.AptNum == pinBoard.ListPinBoardItems[i].AptNum)
            {
                pinBoard.ClearAt(i);
            }
        }

        if (_patient == null)
        {
            ModuleSelected(0);
        }
        else
        {
            ModuleSelected(_patient.PatNum);
        }

        Recalls.SynchScheduledApptFull(appointment.PatNum);
    }

    ///<summary>Sends current appointment to unscheduled list.</summary>
    private void butUnsched_Click()
    {
        if (contrApptPanel.SelectedAptNum == -1)
        {
            MsgBox.Show(this, "Please select an appointment first.");
            return;
        }

        var appointment = Appointments.GetOneApt(contrApptPanel.SelectedAptNum);
        if (ApptIsNull(appointment))
        {
            return;
        }

        if (appointment.AptStatus == ApptStatus.PtNote || appointment.AptStatus == ApptStatus.PtNoteCompleted)
        {
            MsgBox.Show(this, "Patient Notes cannot be sent to the Unscheduled List.");
            return;
        }

        if (!AppointmentL.ValidateApptUnsched(appointment))
        {
            return;
        }

        if (PrefC.GetBool(PrefName.UnscheduledListNoRecalls) && Appointments.IsRecallAppointment(appointment))
        {
            if (MsgBox.Show(this, MsgBoxButtons.YesNo, "Recall appointments cannot be sent to the Unscheduled List.\r\nDelete appointment instead?"))
            {
                butDelete_Click(showPrompt: false);
            }

            return;
        }

        if (!MsgBox.Show(this, MsgBoxButtons.OKCancel, "Send Appointment to Unscheduled List?"))
        {
            return;
        }

        var patient = Patients.GetPat(appointment.PatNum);
        if (!DoApptBreakRequired(appointment, patient))
        {
            return;
        }

        AppointmentL.SetApptUnschedHelper(appointment, patient);
        ModuleSelected(patient.PatNum);
    }

    //Confirmation list------------------------------------------------------------------------------------------
    private void ListConfirmed_MouseDown(object sender, MouseEventArgs e)
    {
        if (listConfirmed.IndexFromPoint(e.X, e.Y) == -1)
        {
            return;
        }

        if (contrApptPanel.SelectedAptNum == -1)
        {
            return;
        }

        var appointment = Appointments.GetOneApt(contrApptPanel.SelectedAptNum);
        if (appointment == null)
        {
            MsgBox.Show(this, "Patient appointment was removed.");
            contrApptPanel.SelectedAptNum = -1;
            ModuleSelected(_patient.PatNum); //keep same pat
            return;
        }

        var appointmentOld = appointment.Copy();
        var newStatus = Defs.GetDefsForCategory(DefCat.ApptConfirmed, true)[listConfirmed.IndexFromPoint(e.X, e.Y)].DefNum;
        Appointments.SetConfirmed(appointment, newStatus); //Appointments S-Class handles Signalods
        if (newStatus != appointmentOld.Confirmed)
        {
            //Log confirmation status changes.
            SecurityLogs.MakeLogEntry(EnumPermType.ApptConfirmStatusEdit, appointment.PatNum, Lan.g(this, "Appointment confirmation status changed from") + " "
                                                                                                                                                          + Defs.GetName(DefCat.ApptConfirmed, appointmentOld.Confirmed) + " " + Lan.g(this, "to") + " " + Defs.GetName(DefCat.ApptConfirmed, newStatus)
                                                                                                                                                          + " " + Lans.g("from the appointment module") + ".", contrApptPanel.SelectedAptNum, appointmentOld.DateTStamp);
        }

        RefreshPeriod();
        //Need to pass in aptOld since we compare the appointment's old confirmed status to the new confirmed status to help determine if the kiosk manager should be shown.
        AppointmentL.ShowKioskManagerIfNeeded(appointmentOld, newStatus);
    }

    //Right Make Buttons----------------------------------------------------------------------------------------
    private void butFamRecall_Click(object sender, EventArgs e)
    {
        if (_patient == null)
        {
            MsgBox.Show(this, "Please select a patient first.");
            return;
        }

        if (!Security.IsAuthorized(EnumPermType.AppointmentCreate))
        {
            return;
        }

        if (Appointments.HasOutstandingAppts(_patient.PatNum))
        {
            DisplayOtherDlg(false);
            return;
        }

        using var formApptsOther = new FormApptsOther(_patient.PatNum, pinBoard.ListPinBoardItems.Select(x => x.AptNum).ToList()); //not shown
        formApptsOther.IsInitialDoubleClick = false;
        formApptsOther.MakeRecallFamily();
        if (formApptsOther.DialogResult != DialogResult.OK)
        {
            return;
        }

        SendToPinBoardAptNums(formApptsOther.ListAptNumsSelected);
        RefreshPeriod(pinApptNums: formApptsOther.ListAptNumsSelected);
        if (contrApptPanel.IsWeeklyView)
        {
            return;
        }

        dateSearch.Text = formApptsOther.StringDateJumpTo;
        if (!groupSearch.Visible)
        {
            //if search not already visible
            ShowSearch();
        }

        DoSearch();
    }

    private void butMakeAppt_Click(object sender, EventArgs e)
    {
        if (_patient == null)
        {
            MsgBox.Show(this, "Please select a patient first.");
            return;
        }

        if (!Security.IsAuthorized(EnumPermType.AppointmentCreate))
        {
            return;
        }

        if (PatRestrictionL.IsRestricted(_patient.PatNum, PatRestrict.ApptSchedule))
        {
            return;
        }

        var patient = AppointmentL.GetPatientMergePrompt(_patient.PatNum);
        if (patient != null)
        {
            _patient = patient;
            RefreshModuleDataPatient(_patient.PatNum);
            GlobalFormOpenDental.PatientSelected(_patient, isRefreshCurModule: true, isApptRefreshDataPat: false);
        }

        if (_patient != null && _patient.PatStatus.In(PatientStatus.Archived, PatientStatus.Deceased))
        {
            MsgBox.Show(Lans.g("Appointments cannot be scheduled for") + " " + _patient.PatStatus.ToString().ToLower() + " " + Lans.g("patients."));
            return;
        }

        if (Appointments.HasOutstandingAppts(_patient.PatNum))
        {
            DisplayOtherDlg(false);
            return;
        }

        using var formApptsOther = new FormApptsOther(_patient.PatNum, pinBoard.ListPinBoardItems.Select(x => x.AptNum).ToList()); //not shown
        CheckStatus();
        formApptsOther.IsInitialDoubleClick = false;
        formApptsOther.MakeAppointment();
        SendToPinBoardAptNums(formApptsOther.ListAptNumsSelected);
        RefreshPeriod(pinApptNums: formApptsOther.ListAptNumsSelected);
    }

    private void butMakeRecall_Click(object sender, EventArgs e)
    {
        if (_patient == null)
        {
            MsgBox.Show(this, "Please select a patient first.");
            return;
        }

        if (!Security.IsAuthorized(EnumPermType.AppointmentCreate))
        {
            return;
        }

        if (PatRestrictionL.IsRestricted(_patient.PatNum, PatRestrict.ApptSchedule))
        {
            return;
        }

        var patient = AppointmentL.GetPatientMergePrompt(_patient.PatNum);
        if (patient != null)
        {
            _patient = patient;
            RefreshModuleDataPatient(_patient.PatNum);
            GlobalFormOpenDental.PatientSelected(_patient, isRefreshCurModule: true, isApptRefreshDataPat: false);
        }

        if (_patient != null && _patient.PatStatus.In(PatientStatus.Archived, PatientStatus.Deceased))
        {
            MsgBox.Show(Lans.g("Appointments cannot be scheduled for") + " " + _patient.PatStatus.ToString().ToLower() + " " + Lans.g("patients."));
            return;
        }

        if (Appointments.HasOutstandingAppts(_patient.PatNum, true))
        {
            DisplayOtherDlg(false);
            return;
        }

        using var formApptsOther = new FormApptsOther(_patient.PatNum, pinBoard.ListPinBoardItems.Select(x => x.AptNum).ToList()); //not shown
        formApptsOther.IsInitialDoubleClick = false;
        formApptsOther.MakeRecallAppointment();
        if (formApptsOther.DialogResult != DialogResult.OK)
        {
            return;
        }

        SendToPinBoardAptNums(formApptsOther.ListAptNumsSelected);
        RefreshPeriod(pinApptNums: formApptsOther.ListAptNumsSelected);
        if (contrApptPanel.IsWeeklyView)
        {
            return;
        }

        dateSearch.Text = formApptsOther.StringDateJumpTo;
        if (!groupSearch.Visible)
        {
            //if search not already visible
            ShowSearch();
        }

        DoSearch(isForMakeRecall: true);
    }

    private void butViewAppts_Click(object sender, EventArgs e)
    {
        DisplayOtherDlg(false);
    }

    #endregion Methods - Event Handlers PanelCalendar Buttons

    #region Methods - Event Handlers PinBoard

    private void butClearPin_Click(object sender, EventArgs e)
    {
        if (pinBoard.ListPinBoardItems.Count == 0)
        {
            MsgBox.Show(this, "There are no appointments on the pinboard to clear.");
            return;
        }

        DataRow dataRow;
        int idx;
        if (pinBoard.ListPinBoardItems.Count == 1)
        {
            dataRow = pinBoard.ListPinBoardItems[0].DataRowAppt; //even if unselected
            idx = 0;
        }
        else
        {
            //multiple items on pinboard
            if (pinBoard.SelectedIndex == -1)
            {
                MsgBox.Show(this, "Please select an appointment first.");
                return;
            }

            dataRow = pinBoard.ListPinBoardItems[pinBoard.SelectedIndex].DataRowAppt;
            idx = pinBoard.SelectedIndex;
        }

        var dateTime = SIn.DateTime(dataRow["AptDateTime"].ToString());
        var aptNum = SIn.Long(dataRow["AptNum"].ToString());
        var aptStatus = (ApptStatus) SIn.Int(dataRow["AptStatus"].ToString());
        if (aptStatus == ApptStatus.UnschedList)
        {
            //unscheduled status
            if (dateTime.Year < 1880)
            {
                //Indicates that this was a brand new appt
                var appointment = Appointments.GetOneApt(aptNum);
                if (appointment == null || appointment.AptDateTime.Year > 1880)
                {
                    //If appointment is already deleted or if date is now present
                    //don't do anything to db.  Appt removed from pinboard above, and Refresh will happen below.
                }
                else
                {
                    Appointments.Delete(aptNum, true);
                }
            }
            else
            {
                //was actually on the unscheduled list
                //do nothing to database
            }
        }
        else if (dateTime.Year > 1880)
        {
            //already scheduled
            //do nothing to database
        }
        else if (aptStatus == ApptStatus.Planned)
        {
            //do nothing except remove it from pinboard
        }
        else
        {
            //Not sure when this would apply, since new appts start out as unsched.  Maybe patient notes?  Leave it just in case.
            //this gets rid of new appointments that never made it off the pinboard
            Appointments.Delete(aptNum, true);
        }

        pinBoard.ClearAt(idx);
        if (pinBoard.ListPinBoardItems.Count > 0)
        {
            pinBoard.SelectedIndex = pinBoard.ListPinBoardItems.Count - 1;
        }

        if (_patient == null)
        {
            //not sure how to test this. Doesn't seem possible.
            RefreshModuleScreenButtonsRight();
            return;
        }

        ModuleSelected(_patient.PatNum);
        /*}
        else {
        RefreshModuleDataPatient(pinBoard.ApptList[pinBoard.SelectedIndex].PatNum);
        FormOpenDental.S_Contr_PatientSelected(PatCur,true,false);
        }*/
    }

    private void pinBoard_ApptMovedFromPinboard(object sender, ApptFromPinboardEventArgs e)
    {
        //Any return from this point forward will cause HideDraggableTempApptSingle();
        //Make sure there are operatories for the appointment to be scheduled and make sure the user dragged the appointment to a valid location.
        if (contrApptPanel.ListOpsVisible.Count == 0)
        {
            pinBoard_ApptMovedFromPinboard_Cleanup();
            return;
        }

        var apptStatus = (ApptStatus) SIn.Int(e.DataRowAppt["AptStatus"].ToString());
        var patNum = SIn.Long(e.DataRowAppt["PatNum"].ToString());
        if (apptStatus == ApptStatus.Planned)
        {
            //if Planned appt is on pinboard
            if (!Security.IsAuthorized(EnumPermType.AppointmentCreate))
            {
                //and no permission to create a new appt
                pinBoard_ApptMovedFromPinboard_Cleanup();
                return;
            }

            if (PatRestrictionL.IsRestricted(patNum, PatRestrict.ApptSchedule))
            {
                //or pat restricted
                pinBoard_ApptMovedFromPinboard_Cleanup();
                return;
            }
        }

        //security prevents moving an appointment by preventing placing it on the pinboard, not here
        //We do not ask user, "Move Appointment?" because that's just slow.
        //convert loc to new time
        var appointment = Appointments.GetOneApt(SIn.Long(e.DataRowAppt["AptNum"].ToString()));
        if (appointment == null)
        {
            MsgBox.Show(this, "This appointment has been deleted since it was moved to the pinboard. It will now be cleared from the pinboard.");
            pinBoard.ClearAt(pinBoard.SelectedIndex);
            pinBoard_ApptMovedFromPinboard_Cleanup();
            return;
        }

        //This hook is specifically called after we know a valid appointment has been identified.
        var appointmentOld = appointment.Copy();
        RefreshModuleDataPatient(appointment.PatNum); //This is to change _patCur.
        GlobalFormOpenDental.PatientSelected(_patient, true, false); //especially to change name showing in title bar
        if (appointment.IsNewPatient && contrApptPanel.DateSelected != appointment.AptDateTime.Date)
        {
            Procedures.SetDateFirstVisit(contrApptPanel.DateSelected, 4, _patient);
        }

        //e.Location is in ContrAppt coords
        var timeSpanNew = contrApptPanel.YPosToTime(e.Location.Y - contrApptPanel.Top); //in contrApptPanel coords
        if (timeSpanNew == null)
        {
            pinBoard_ApptMovedFromPinboard_Cleanup();
            return;
        }

        var timeSpanNewRounded = ControlApptPanel.RoundTimeToNearestIncrement(timeSpanNew.Value, contrApptPanel.MinPerIncr);
        contrApptPanel.RoundToNearestDateAndOp(e.Location.X - contrApptPanel.Location.X, //passing in as coordinates of the control
            out var dateNew,
            out var opIdx, e.BitmapAppt.Width);
        if (opIdx < 0)
        {
            MsgBox.Show(this, "Invalid operatory");
            pinBoard_ApptMovedFromPinboard_Cleanup();
            return;
        }

        appointment.AptDateTime = dateNew + timeSpanNewRounded;
        //Compare beginning of new appointment against end to see if the appointment spans two days
        if (appointment.AptDateTime.Day != appointment.AptDateTime.AddMinutes(appointment.Pattern.Length * 5).Day)
        {
            MsgBox.Show(this, "You cannot have an appointment that starts and ends on different days.");
            pinBoard_ApptMovedFromPinboard_Cleanup();
            return;
        }

        //Prevent double-booking
        if (contrApptPanel.IsDoubleBooked(appointment))
        {
            pinBoard_ApptMovedFromPinboard_Cleanup();
            return;
        }

        var operatory = contrApptPanel.ListOpsVisible[opIdx];
        appointment.Op = operatory.OperatoryNum;
        if (!appointment.IsHygiene)
        {
            //If a non-hygiene appointment is moved, update the IsHygiene value to that of the new operatory.
            appointment.IsHygiene = operatory.IsHygiene;
        }

        //opCur.OperatoryNum;
        //Set providers----------------------Similar to UpdateAppointments()
        var assignedDent = Schedules.GetAssignedProvNumForSpot(contrApptPanel.ListSchedules, operatory, false, appointment.AptDateTime);
        var assignedHyg = Schedules.GetAssignedProvNumForSpot(contrApptPanel.ListSchedules, operatory, true, appointment.AptDateTime);
        List<Procedure> procsForSingleApt = null;
        if (appointment.AptStatus != ApptStatus.PtNote && appointment.AptStatus != ApptStatus.PtNoteCompleted)
        {
            #region Update Appt's DateTimeAskedToArrive

            if (_patient.AskToArriveEarly > 0)
            {
                appointment.DateTimeAskedToArrive = appointment.AptDateTime.AddMinutes(-_patient.AskToArriveEarly);
                ODMessageBox.Show(Lan.g(this, "Ask patient to arrive") + " " + _patient.AskToArriveEarly
                                  + " " + Lan.g(this, "minutes early at") + " " + appointment.DateTimeAskedToArrive.ToShortTimeString() + ".");
            }
            else
            {
                appointment.DateTimeAskedToArrive = DateTime.MinValue;
            }

            #endregion Update Appt's DateTimeAskedToArrive

            #region Update Appt's Update Appt's ProvNum, ProvHyg, IsHygiene, Pattern

            //if no dentist/hygienist is assigned to spot, then keep the original dentist/hygienist without prompt.  All appts must have prov.
            if ((assignedDent != 0 && assignedDent != appointment.ProvNum) || (assignedHyg != 0 && assignedHyg != appointment.ProvHyg))
            {
                var frmApptProvPrompt = new FrmApptProvPrompt();
                var enumApptProvPrompt = PrefC.GetEnum<EnumApptProvPrompt>(PrefName.ApptModuleProviderPrompt);
                var screen = Screen.FromControl(this);
                frmApptProvPrompt.PointScreen = screen.Bounds.Location;
                frmApptProvPrompt.EnumApptProvPrompt_ = enumApptProvPrompt;
                if (enumApptProvPrompt == EnumApptProvPrompt.NoPromptChange || enumApptProvPrompt == EnumApptProvPrompt.NoPromptNoChange)
                {
                    //Pref is set to don't prompt. Automate the provider change selection.
                    frmApptProvPrompt.AutomateSelection();
                }

                if (enumApptProvPrompt == EnumApptProvPrompt.PromptDefaultYes || enumApptProvPrompt == EnumApptProvPrompt.PromptDefaultNo)
                {
                    //Only show the frm if preference is set to prompt
                    frmApptProvPrompt.ShowDialog();
                }

                if (frmApptProvPrompt.IsDialogOK)
                {
                    //True if changing provider
                    if (assignedDent != 0)
                    {
                        //the dentist will only be changed if the spot has a dentist.
                        appointment.ProvNum = assignedDent;
                    }

                    if (assignedHyg != 0 || PrefC.GetBool(PrefName.ApptSecondaryProviderConsiderOpOnly))
                    {
                        //the hygienist will only be changed if the spot has a hygienist.
                        appointment.ProvHyg = assignedHyg;
                    }

                    if (operatory.IsHygiene)
                    {
                        appointment.IsHygiene = true;
                    }
                    else
                    {
                        //op not marked as hygiene op
                        if (assignedDent == 0)
                        {
                            //no dentist assigned
                            if (assignedHyg != 0)
                            {
                                //hyg is assigned (we don't really have to test for this)
                                appointment.IsHygiene = true;
                            }
                        }
                        else
                        {
                            //dentist is assigned
                            if (assignedHyg == 0)
                            {
                                //hyg is not assigned
                                appointment.IsHygiene = false;
                            }

                            //if both dentist and hyg are assigned, it's tricky
                            //only explicitly set it if user has a dentist assigned to the op
                            if (operatory.ProvDentist != 0)
                            {
                                appointment.IsHygiene = false;
                            }
                        }
                    }

                    var isplanned = appointment.AptStatus == ApptStatus.Planned;
                    procsForSingleApt = Procedures.GetProcsForSingle(appointment.AptNum, isplanned);
                    var codeNums = new List<long>();
                    for (var p = 0; p < procsForSingleApt.Count; p++)
                    {
                        codeNums.Add(procsForSingleApt[p].CodeNum);
                    }

                    var doMake5Minute = procsForSingleApt.Count > 0; //Appointments without procs are already returned in 5 minute increments.
                    var calcPattern = Appointments.CalculatePattern(appointment.ProvNum, appointment.ProvHyg, codeNums, doMake5Minute);
                    if (appointment.Pattern != calcPattern)
                    {
                        if (Security.IsAuthorized(EnumPermType.AppointmentResize, suppressMessage: true) && !appointment.TimeLocked)
                        {
                            //User is authorized, and appt time not locked. Do not give popup for users without resizing permissions or for Timelocked appointments.
                            if (MsgBox.Show(this, MsgBoxButtons.YesNo, "Change length for new provider?"))
                            {
                                appointment.Pattern = calcPattern;
                            }
                        }
                    }
                }
            }

            #region Provider Term Date Check

            //Prevents appointments with providers that are past their term end date from being scheduled
            var message = Providers.CheckApptProvidersTermDates(appointment);
            if (message != "")
            {
                ODMessageBox.Show(message); //translated in Providers S class method
                pinBoard_ApptMovedFromPinboard_Cleanup();
                return;
            }

            #endregion Provider Term Date Check

            #endregion Update Appt's ProvNum, ProvHyg, IsHygiene, Pattern
        }

        #region Prevent overlap

        //Check for any blockout collisions when overlapping appointments are allowed.
        if (PrefC.GetBool(PrefName.ApptsAllowOverlap))
        {
            if (ShowAppointmentBlockoutMessage(appointment))
            {
                pinBoard_ApptMovedFromPinboard_Cleanup();
                return;
            }
        }
        else
        {
            //Appointments are not allowed to overlap so check for both appointment and blockout collisions.
            if (!Appointments.TryAdjustAppointmentOp(appointment, contrApptPanel.ListOpsVisible))
            {
                MsgBox.Show(this, "Appointment overlaps existing appointment or blockout.");
                pinBoard_ApptMovedFromPinboard_Cleanup();
                return;
            }
        }

        #endregion Prevent overlap

        #region Detect Frequency Conflicts

        //Detect frequency conflicts with procedures in the appointment
        var discountPlanSub = DiscountPlanSubs.GetSubForPat(_patient.PatNum);
        if (discountPlanSub == null)
        {
            if (PrefC.GetBool(PrefName.InsChecksFrequency))
            {
                procsForSingleApt = Procedures.GetProcsForSingle(appointment.AptNum, appointment.AptStatus == ApptStatus.Planned);
                var frequencyConflicts = "";
                try
                {
                    frequencyConflicts = Procedures.CheckFrequency(procsForSingleApt, appointment.PatNum, appointment.AptDateTime);
                }
                catch (Exception ex)
                {
                    ODMessageBox.Show(Lan.g(this, "There was an error checking frequencies.  Disable the Insurance Frequency Checking feature or try to fix the following error:")
                                      + "\r\n" + ex.Message);
                    pinBoard_ApptMovedFromPinboard_Cleanup();
                    return;
                }

                if (frequencyConflicts != "" && ODMessageBox.Show(
                        Lan.g(this, "Scheduling this appointment for this date will cause frequency conflicts for the following procedures")
                        + ":\r\n" + frequencyConflicts + "\r\n" + Lan.g(this, "Do you want to continue?"), "", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    pinBoard_ApptMovedFromPinboard_Cleanup();
                    return;
                }
            }
        }
        else
        {
            procsForSingleApt = Procedures.GetProcsForSingle(appointment.AptNum, appointment.AptStatus == ApptStatus.Planned);
            var frequencyDiscountConflicts = "";
            try
            {
                frequencyDiscountConflicts = DiscountPlans.CheckDiscountFrequencyAndValidateDiscountPlanSub(procsForSingleApt, appointment.PatNum, appointment.AptDateTime);
            }
            catch (Exception ex)
            {
                ODMessageBox.Show(Lan.g(this, "There was an error checking discount frequencies:")
                                  + "\r\n" + ex.Message);
                pinBoard_ApptMovedFromPinboard_Cleanup();
                return;
            }

            if (!string.IsNullOrEmpty(frequencyDiscountConflicts) && ODMessageBox.Show(Lan.g(this, "This appointment will cause frequency conflicts for the following procedures")
                                                                                       + ":\r\n" + frequencyDiscountConflicts + "\r\n" + Lan.g(this, "Do you want to continue?"), "", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                pinBoard_ApptMovedFromPinboard_Cleanup();
                return;
            }
        }

        #endregion Detect Frequency Conflicts

        #region Patient status

        //Operatory opCur=Operatories.GetOperatory(apptCur.Op);
        var opOld = Operatories.GetOperatory(appointmentOld.Op);
        if (opOld == null || operatory.SetProspective != opOld.SetProspective)
        {
            if (operatory.SetProspective && _patient.PatStatus != PatientStatus.Prospective)
            {
                //Don't need to prompt if patient is already prospective.
                if (MsgBox.Show(this, MsgBoxButtons.OKCancel, "Patient's status will be set to Prospective."))
                {
                    var patientOld = _patient.Copy();
                    _patient.PatStatus = PatientStatus.Prospective;
                    Patients.UpdateRecalls(_patient, patientOld, "Appointment Module, Appointment moved to prospective operatory");
                    Patients.Update(_patient, patientOld);
                    var logEntry = Lan.g(this, "Patient's status changed from ") + patientOld.PatStatus.GetDescription() + Lan.g(this, " to ")
                                   + _patient.PatStatus.GetDescription() + Lan.g(this, " by moving the patient appointment to a prospective operatory.");
                    SecurityLogs.MakeLogEntry(EnumPermType.PatientEdit, _patient.PatNum, logEntry);
                }
            }
            else if (!operatory.SetProspective && _patient.PatStatus == PatientStatus.Prospective)
            {
                //Do we need to warn about changing FROM prospective? Assume so for now.
                if (MsgBox.Show(this, MsgBoxButtons.OKCancel, "Patient's status will change from Prospective to Patient."))
                {
                    var patientOld = _patient.Copy();
                    _patient.PatStatus = PatientStatus.Patient;
                    Patients.UpdateRecalls(_patient, patientOld, "Appointment Module, Appointment moved from prospective operatory");
                    Patients.Update(_patient, patientOld);
                    var logEntry = Lan.g(this, "Patient's status changed from ") + patientOld.PatStatus.GetDescription() + Lan.g(this, " to ")
                                   + _patient.PatStatus.GetDescription() + Lan.g(this, " by moving the patient appointment from a prospective operatory.");
                    SecurityLogs.MakeLogEntry(EnumPermType.PatientEdit, _patient.PatNum, logEntry);
                }
            }
        }

        #endregion Patient status

        #region Update Appt's AptStatus, ClinicNum

        if (appointment.AptStatus == ApptStatus.Broken)
        {
            appointment.AptStatus = ApptStatus.Scheduled;
        }

        if (appointment.AptStatus == ApptStatus.UnschedList)
        {
            appointment.AptStatus = ApptStatus.Scheduled;
        }

        //original position of provider settings
        if (operatory.ClinicNum == 0)
        {
            appointment.ClinicNum = _patient.ClinicNum;
        }
        else
        {
            appointment.ClinicNum = operatory.ClinicNum;
        }

        #endregion Update Appt's AptStatus, ClinicNum

        var isCreate = false;

        #region Update/Insert Appt in db

        if (!AppointmentL.IsSpecialtyMismatchAllowed(_patient.PatNum, appointment.ClinicNum))
        {
            //ClinicNum just set using either opCur or PatCur.
            pinBoard_ApptMovedFromPinboard_Cleanup();
            return;
        }

        if (appointment.AptStatus == ApptStatus.Planned)
        {
            //if Planned appt is on pinboard

            #region Planned appointment

            var tableApptFields = pinBoard.ListPinBoardItems[pinBoard.SelectedIndex].TableApptFields;
            var listApptFields = new List<ApptField>();
            for (var i = 0; i < tableApptFields.Rows.Count; i++)
            {
                if (appointmentOld.AptNum != SIn.Long(tableApptFields.Rows[i]["AptNum"].ToString()))
                {
                    continue; //should never happen
                }

                var apptField = new ApptField();
                apptField.FieldName = SIn.String(tableApptFields.Rows[i]["FieldName"].ToString());
                apptField.FieldValue = SIn.String(tableApptFields.Rows[i]["FieldValue"].ToString());
                //the other two fields are not important
                listApptFields.Add(apptField);
            }

            bool procAlreadyAttached;
            procAlreadyAttached = Appointments.IsProcAlreadyAttached(appointment); //test planned appt
            appointment = Appointments.SchedulePlannedApt(appointment, _patient, listApptFields, appointment.AptDateTime, appointment.Op); //Appointments S-Class handles Signalods
            isCreate = true;
            if (procAlreadyAttached)
            {
                MsgBox.Show(this, "One or more procedures could not be scheduled because they were already attached to another appointment. Someone probably forgot to update the planned appointment in the Chart Module.");
                using var formApptEdit = new FormApptEdit(appointment.AptNum);
                CheckStatus();
                formApptEdit.IsNew = true;
                formApptEdit.ShowDialog(); //to force refresh of aptDescript
                if (formApptEdit.DialogResult != DialogResult.OK)
                {
                    //apt gets deleted from within aptEdit window.
                    RefreshModuleScreenButtonsRight();
                    RefreshPeriod();
                    pinBoard_ApptMovedFromPinboard_Cleanup();
                    return;
                }
            }
            else
            {
                SecurityLogs.MakeLogEntry(EnumPermType.AppointmentCreate, appointment.PatNum,
                    appointment.AptDateTime + ", " + appointment.ProcDescript,
                    appointment.AptNum, appointmentOld.DateTStamp);
            }

            procsForSingleApt = Procedures.GetProcsForSingle(appointment.AptNum, false);

            #endregion Planned appointment
        }
        else
        {
            //simple drag off pinboard to a new date/time

            #region Previously scheduled appointment (not a planned appointment)

            appointment.Confirmed = Defs.GetFirstForCategory(DefCat.ApptConfirmed, true).DefNum; //Causes the confirmation status to be reset.
            Appointments.Update(appointment, appointmentOld); //Appointments S-Class handles Signalods
            Appointments.TryAddPerVisitProcCodesToAppt(appointment, appointmentOld.AptStatus);
            if (appointmentOld.AptStatus == ApptStatus.UnschedList && appointmentOld.AptDateTime == DateTime.MinValue)
            {
                //If new appt is being added to schedule from pinboard
                SecurityLogs.MakeLogEntry(EnumPermType.AppointmentCreate, appointment.PatNum,
                    appointment.AptDateTime + ", " + appointment.ProcDescript,
                    appointment.AptNum, appointmentOld.DateTStamp);
                isCreate = true;
            }
            else
            {
                //If existing appt is being moved
                SecurityLogs.MakeLogEntry(EnumPermType.AppointmentMove, appointment.PatNum,
                    appointment.ProcDescript + ", from " + appointmentOld.AptDateTime + ", to " + appointment.AptDateTime,
                    appointment.AptNum, appointmentOld.DateTStamp);
                if (appointmentOld.AptStatus == ApptStatus.UnschedList)
                {
                    isCreate = true;
                }
            }

            if (appointment.Confirmed != appointmentOld.Confirmed)
            {
                //Log confirmation status changes.
                SecurityLogs.MakeLogEntry(EnumPermType.ApptConfirmStatusEdit, appointment.PatNum,
                    Lan.g(this, "Appointment confirmation status automatically changed from ")
                    + Defs.GetName(DefCat.ApptConfirmed, appointmentOld.Confirmed) + " to " + Defs.GetName(DefCat.ApptConfirmed, appointment.Confirmed)
                    + Lan.g(this, " from the appointment module") + ".", appointment.AptNum, appointmentOld.DateTStamp);
            }

            //If there is an existing HL7 def enabled, send a SIU message if there is an outbound SIU message defined
            if (HL7Defs.IsExistingHL7Enabled())
            {
                //S12 - New Appt Booking event, S13 - Appt Rescheduling
                MessageHL7 messageHL7 = null;
                if (isCreate)
                {
                    messageHL7 = MessageConstructor.GenerateSIU(_patient, Patients.GetPat(_patient.Guarantor), EventTypeHL7.S12, appointment);
                }
                else
                {
                    messageHL7 = MessageConstructor.GenerateSIU(_patient, Patients.GetPat(_patient.Guarantor), EventTypeHL7.S13, appointment);
                }

                //Will be null if there is no outbound SIU message defined, so do nothing
                if (messageHL7 != null)
                {
                    var hl7Msg = new HL7Msg();
                    hl7Msg.AptNum = appointment.AptNum;
                    hl7Msg.HL7Status = HL7MessageStatus.OutPending; //it will be marked outSent by the HL7 service.
                    hl7Msg.MsgText = messageHL7.ToString();
                    hl7Msg.PatNum = _patient.PatNum;
                    HL7Msgs.Insert(hl7Msg);
                }
            }

            if (HieClinics.IsEnabled())
            {
                HieQueues.Insert(new HieQueue(_patient.PatNum));
            }
        }

        #endregion Previously scheduled appointment (not a planned appointment)

        #endregion Update/Insert Appt in db

        if (procsForSingleApt == null)
        {
            procsForSingleApt = Procedures.GetProcsForSingle(appointment.AptNum, false);
        }

        #region Update UI and cache

        var procFeeHelper = new ProcFeeHelper(appointment.PatNum);
        var isUpdatingFees = false;
        var listProcedures = procsForSingleApt.Select(x => Procedures.ChangeProcInAppointment(appointment, x.Copy())).ToList();
        if (procsForSingleApt.Exists(x => x.ProvNum != listProcedures.FirstOrDefault(y => y.ProcNum == x.ProcNum).ProvNum))
        {
            //Either the primary or hygienist changed.
            var promptText = "";
            isUpdatingFees = Procedures.ShouldFeesChange(listProcedures, procsForSingleApt, ref promptText, procFeeHelper);
            if (isUpdatingFees)
            {
                //Made it pass the pref check.
                if (promptText != "" && !MsgBox.Show(this, MsgBoxButtons.YesNo, promptText))
                {
                    isUpdatingFees = false;
                }
            }
        }

        Procedures.SetProvidersInAppointment(appointment, procsForSingleApt, isUpdatingFees, procFeeHelper);
        pinBoard.ClearAt(pinBoard.SelectedIndex);
        contrApptPanel.SelectedAptNum = appointment.AptNum;
        //SetDateSelected(apptCur.AptDateTime);
        //RefreshPeriod(isRefreshSchedules:true);//date moving to for this computer; This line may not be needed
        RefreshPeriod();
        RefreshModuleScreenButtonsRight();
        if (isCreate)
        {
            //new appointment is being added to the schedule from the pinboard, trigger ScheduleProcedure automation
            var procCodes = procsForSingleApt.Select(x => ProcedureCodes.GetProcCode(x.CodeNum).ProcCode).ToList();
            AutomationL.Trigger(EnumAutomationTrigger.ProcSchedule, procCodes, appointment.PatNum);
        }

        ODEvent.Fire(ODEventType.AppointmentEdited, appointment);
        Recalls.SynchScheduledApptFull(appointment.PatNum);

        #endregion Update UI and cache

        pinBoard_ApptMovedFromPinboard_Cleanup();
    }

    private void pinBoard_ApptMovedFromPinboard_Cleanup()
    {
        pinBoard.HideDraggableTempApptSingle();
        if (pinBoard.ListPinBoardItems.Count > 0)
        {
            //If there are any more items in the pinboard, update the selected index.
            pinBoard.SelectedIndex = pinBoard.ListPinBoardItems.Count - 1;
        }
    }

    private void pinBoard_ModuleNeedsRefresh(object sender, EventArgs e)
    {
        long pinAptNum = 0;
        if (pinBoard.SelectedIndex != -1)
        {
            pinAptNum = pinBoard.ListPinBoardItems[pinBoard.SelectedIndex].AptNum;
        }

        List<long> listOpNums = null;
        List<long> listProvNums = null;
        if (Clinics.ClinicNum != 0 || !ApptViews.IsNoneView(GetApptViewCur()))
        {
            listOpNums = contrApptPanel.ListOpsVisible.Select(x => x.OperatoryNum).ToList();
            listProvNums = contrApptPanel.ListProvsVisible.Select(x => x.Id).ToList();
        }

        ModuleSelected(_patient.PatNum, opNums: listOpNums, provNums: listProvNums);
        if (pinAptNum != 0)
        {
            SendToPinBoardAptNums([pinAptNum]);
        }
    }

    private void pinBoard_PreparingToDragFromPinboard(object sender, ApptDataRowEventArgs e)
    {
        var pattern = SIn.String(e.DataRowAppt["Pattern"].ToString());
        var patternShowing = contrApptPanel.GetPatternShowing(pattern);
        var sizeAppt = contrApptPanel.SetSize(pattern);
        if (sizeAppt.Width == 0)
        {
            pinBoard.SetBitmapTempPinAppt(null); //tells pinboard that we can't drag appt off.
            return;
        }

        //the whole point of having all of this here is to set the width of the dragging appt to be same as in the main area instead of like pinboard
        var bitmap = new Bitmap((int) sizeAppt.Width, (int) sizeAppt.Height);
        using (var g = Graphics.FromImage(bitmap))
        {
            contrApptPanel.GetBitmapForPinboard(g, e.DataRowAppt, patternShowing, bitmap.Width, bitmap.Height);
        }

        pinBoard.SetBitmapTempPinAppt(bitmap);
        bitmap.Dispose(); //?
    }

    private void pinBoard_SelectedIndexChanged(object sender, EventArgs e)
    {
        contrApptPanel.SelectedAptNum = -1;
        RefreshModuleScreenButtonsRight();
    }

    #endregion Methods - Event Handlers PinBoard

    #region Methods - Event Handlers LR Tabs

    private void GridEmpOrProv_DoubleClick(object sender, EventArgs e)
    {
        if (contrApptPanel.IsWeeklyView)
        {
            MsgBox.Show(this, "Not available in weekly view");
            return;
        }

        if (!Security.IsAuthorized(EnumPermType.Schedules))
        {
            return;
        }

        using var formScheduleDayEdit = new FormScheduleDayEdit(contrApptPanel.DateSelected, Clinics.ClinicNum);
        formScheduleDayEdit.ShowOkSchedule = true;
        formScheduleDayEdit.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Schedules, 0, "");
        RefreshPeriod();
        if (formScheduleDayEdit.GotoScheduleOnClose)
        {
            using var formSchedule = new FormSchedule();
            formSchedule.ShowDialog();
        }
    }

    ///<summary>Logic mimics UserControlTasks.gridMain_CellDoubleClick()</summary>
    private void gridReminders_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        if (e.Col == 0)
        {
            //no longer allow double click on checkbox, because it's annoying.
            return;
        }

        var row = gridReminders.ListGridRows[e.Row];
        var taskReminder = (Task) row.Tag;
        //It's important to grab the task directly from the db because the status in this list is fake, being the "unread" status instead.
        var task = Tasks.GetOne(taskReminder.TaskNum);
        var formTaskEdit = new FormTaskEdit(task);
        formTaskEdit.Show(); //non-modal
    }

    ///<summary>The logic for this function was copied from UserControlTasks.gridMain_MouseDown() and modified slightly for this scenaro.</summary>
    private void gridReminders_MouseDown(object sender, MouseEventArgs e)
    {
        var clickedI = gridReminders.PointToRow(e.Y);
        var clickedCol = gridReminders.PointToCol(e.X);
        if (clickedI == -1)
        {
            return;
        }

        gridReminders.SetSelected(clickedI, true); //if right click.
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        var row = gridReminders.ListGridRows[clickedI];
        var taskReminder = ((Task) row.Tag).Copy();
        if (clickedCol == 0)
        {
            //check tasks off
            if (PrefC.GetBool(PrefName.TasksNewTrackedByUser))
            {
                var userNumInbox = TaskLists.GetMailboxUserNum(taskReminder.TaskListNum);
                if (userNumInbox != 0 && userNumInbox != Security.CurUser.UserNum)
                {
                    MsgBox.Show(this, "Not allowed to mark off tasks in someone else's inbox.");
                    return;
                }

                //might not need to go to db to get this info 
                //might be able to check this:
                //if(task.IsUnread) {
                //But seems safer to go to db.
                if (TaskUnreads.IsUnread(Security.CurUser.UserNum, taskReminder))
                {
                    TaskUnreads.SetRead(Security.CurUser.UserNum, taskReminder);
                    taskReminder.TaskStatus = TaskStatusEnum.Viewed;
                    gridReminders.BeginUpdate();
                    SetReminderGridRow(row, taskReminder); //To get the status to immediately show up in the reminders grid.
                    gridReminders.EndUpdate();
                    var signalNum = Signalods.SetInvalid(InvalidType.Task, KeyType.Task, taskReminder.TaskNum);
                    UserControlTasks.RefillLocalTaskGrids(taskReminder, TaskNotes.GetForTask(taskReminder.TaskNum), [signalNum]);
                }

                return;
                //if already read, nothing else to do.  If done, nothing to do
            }

            if (taskReminder.TaskStatus == TaskStatusEnum.New)
            {
                var taskOld = taskReminder.Copy();
                taskReminder.TaskStatus = TaskStatusEnum.Viewed;
                try
                {
                    Tasks.Update(taskReminder, taskOld);
                }
                catch (Exception ex)
                {
                    ODMessageBox.Show(ex.Message);
                    return;
                } //no longer allowed to mark done from here

                gridReminders.BeginUpdate();
                SetReminderGridRow(row, taskReminder); //To get the status to immediately show up in the reminders grid.
                gridReminders.EndUpdate();
                var signalNum = Signalods.SetInvalid(InvalidType.Task, KeyType.Task, taskReminder.TaskNum);
                UserControlTasks.RefillLocalTaskGrids(taskReminder, TaskNotes.GetForTask(taskReminder.TaskNum), [signalNum]);
            }
        }
    }

    ///<summary>Logic mimics UserControlTasks.DoneClicked(). Code copied from version 19.2 ContrAppt.cs.</summary>
    private void menuItemReminderDone_Click(object sender, EventArgs e)
    {
        if (gridReminders.GetSelectedIndex() == -1)
        {
            return;
        }

        var task = (Task) gridReminders.ListGridRows[gridReminders.GetSelectedIndex()].Tag;
        var oldTask = task.Copy();
        task.TaskStatus = TaskStatusEnum.Done;
        if (task.DateTimeFinished.Year < 1880)
        {
            task.DateTimeFinished = DateTime.Now;
        }

        try
        {
            Tasks.Update(task, oldTask);
        }
        catch (Exception ex)
        {
            //Revert the changes to the task because something went wrong.
            task.TaskStatus = oldTask.TaskStatus;
            task.DateTimeFinished = oldTask.DateTimeFinished;
            ODMessageBox.Show(ex.Message);
            return;
        }

        TaskUnreads.DeleteForTask(task);
        var taskHist = new TaskHist(oldTask);
        taskHist.UserNumHist = Security.CurUser.UserNum;
        TaskHists.Insert(taskHist);
        var signalNum = Signalods.SetInvalid(InvalidType.Task, KeyType.Task, task.TaskNum);
        UserControlTasks.RefillLocalTaskGrids(task, TaskNotes.GetForTask(task.TaskNum), [signalNum]);
        gridReminders.BeginUpdate();
        gridReminders.ListGridRows.RemoveAt(gridReminders.GetSelectedIndex());
        gridReminders.EndUpdate();
    }

    ///<summary>Logic mimics UserControlTasks.GoTo_Clicked(). Code copied from version 19.2 ContrAppt.cs.</summary>
    private void menuItemReminderGoto_Click(object sender, EventArgs e)
    {
        if (gridReminders.GetSelectedIndex() == -1)
        {
            return;
        }

        var task = (Task) gridReminders.ListGridRows[gridReminders.GetSelectedIndex()].Tag;
        FormOpenDental.S_TaskGoTo(task.ObjectType, task.KeyNum);
    }

    private void timerWaitingRoom_Tick(object sender, EventArgs e)
    {
        FillWaitingRoom();
    }

    #endregion Methods - Event Handlers LR Tabs

    #region Methods - Event Handlers Menu Popup

    private Appointment BreakApptHelper(object sender, bool suppressModuleSelected = false)
    {
        var menuItem = (ToolStripMenuItem) sender;
        var menuItemParent = menuItem.OwnerItem;
        var appointment = Appointments.GetOneApt((long) menuItemParent.Tag); //Refresh since they could of waited to interact with menu.
        if (appointment == null)
        {
            //This can happen if another user deleted the appt just after the current user right clicked on the appt.
            MsgBox.Show(this, "Appointment not found.");
            return null;
        }

        if (appointment.AptStatus == ApptStatus.PtNote || appointment.AptStatus == ApptStatus.PtNoteCompleted)
        {
            MsgBox.Show(this, "Patient Notes cannot be broken.");
            return null;
        }

        if (AppointmentL.DoPreventChangesToCompletedAppt(appointment, PreventChangesApptAction.Break))
        {
            return null;
        }

        var procedureCode = (ProcedureCode) menuItem.Tag;
        var patient = Patients.GetPat(appointment.PatNum);
        AppointmentL.BreakApptHelper(appointment, patient, procedureCode);
        if (!suppressModuleSelected)
        {
            ModuleSelected(patient.PatNum);
        }

        return appointment;
    }

    private void menuApt_Opening(object sender, CancelEventArgs e)
    {
        if (_toolStripMenuItem == null)
        {
            return;
        }

        _toolStripMenuItem.DropDownItems.Clear();
        _toolStripMenuItem.Tag = contrApptPanel.SelectedAptNum; //Refresh later, just in case.
        ToolStripItem item = null;
        var dontAllowUnscheduled = PrefC.GetBool(PrefName.UnscheduledListNoRecalls)
                                   && Appointments.IsRecallAppointment(Appointments.GetOneApt(contrApptPanel.SelectedAptNum));
        var brokenApptProcs = (BrokenApptProcedure) PrefC.GetInt(PrefName.BrokenApptProcedure);
        if (brokenApptProcs.In(BrokenApptProcedure.Missed, BrokenApptProcedure.Both))
        {
            if (dontAllowUnscheduled)
            {
                item = _toolStripMenuItem.DropDownItems.Add(Lan.g(this, "Missed - Delete Appointment"), null, menuBreakDelete_Click);
            }
            else
            {
                item = _toolStripMenuItem.DropDownItems.Add(Lan.g(this, "Missed - Send to Unscheduled List"), null, menuBreakToUnsched_Click);
            }

            item.Tag = ProcedureCodes.GetProcCode("D9986");
            item = _toolStripMenuItem.DropDownItems.Add(Lan.g(this, "Missed - Copy To Pinboard"), null, menuBreakToPin_Click);
            item.Tag = ProcedureCodes.GetProcCode("D9986");
            item = _toolStripMenuItem.DropDownItems.Add(Lan.g(this, "Missed - Leave on Appt Book"), null, menuBreak_Click);
            item.Tag = ProcedureCodes.GetProcCode("D9986");
        }

        if (brokenApptProcs.In(BrokenApptProcedure.Cancelled, BrokenApptProcedure.Both))
        {
            if (_toolStripMenuItem.DropDownItems.Count > 0)
            {
                _toolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            }

            if (dontAllowUnscheduled)
            {
                item = _toolStripMenuItem.DropDownItems.Add(Lan.g(this, "Cancelled - Delete Appointment"), null, menuBreakDelete_Click);
            }
            else
            {
                item = _toolStripMenuItem.DropDownItems.Add(Lan.g(this, "Cancelled - Send to Unscheduled List"), null, menuBreakToUnsched_Click);
            }

            item.Tag = ProcedureCodes.GetProcCode("D9987");
            item = _toolStripMenuItem.DropDownItems.Add(Lan.g(this, "Cancelled - Copy To Pinboard"), null, menuBreakToPin_Click);
            item.Tag = ProcedureCodes.GetProcCode("D9987");
            item = _toolStripMenuItem.DropDownItems.Add(Lan.g(this, "Cancelled - Leave on Appt Book"), null, menuBreak_Click);
            item.Tag = ProcedureCodes.GetProcCode("D9987");
        }

        _toolStripMenuItem.Click -= menuApt_Click; //if there are items in DropDownItems, clicking on "Break Appointment" should not fire menuApt_Click, just expand menu
        if (_toolStripMenuItem.DropDownItems.Count == 0)
        {
            _toolStripMenuItem.Click += menuApt_Click;
        }
    }

    private void menuBreak_Click(object sender, EventArgs e)
    {
        BreakApptHelper(sender);
    }

    private void menuBreakDelete_Click(object sender, EventArgs e)
    {
        var appointment = BreakApptHelper(sender, true);
        if (appointment != null)
        {
            butDelete_Click(showPrompt: false);
        }
    }

    private void menuBreakToPin_Click(object sender, EventArgs e)
    {
        var appointment = BreakApptHelper(sender);
        if (appointment != null && AppointmentL.ValidateApptToPinboard(appointment))
        {
            AppointmentL.CopyAptToPinboardHelper(appointment);
        }
    }

    private void menuBreakToUnsched_Click(object sender, EventArgs e)
    {
        var appointment = BreakApptHelper(sender, true);
        if (appointment != null && AppointmentL.ValidateApptUnsched(appointment))
        {
            AppointmentL.SetApptUnschedHelper(appointment);
            ModuleSelected(appointment.PatNum);
        }
    }

    #endregion Methods - Event Handlers Menu Popup

    #region Methods - Event Handlers Menu Apt Click

    private void menuApt_Click(object sender, EventArgs e)
    {
        Appointment appointment;
        switch (((ToolStripMenuItem) sender).Name)
        {
            case MenuItemNames.CopyToPinboard: //Menu: Copy to Pinboard
                CopyToPin_Click();
                //If pref on, refresh pinboard right away to show broken appt.
                if (PrefC.GetBool(PrefName.BrokenApptRequiredOnMove))
                {
                    RefreshPinboardImages();
                }

                break;
            case MenuItemNames.CopyAppointmentStructure: //Menu: Copy Appointment Structure
                CopyApptStructure(Appointments.GetOneApt(contrApptPanel.SelectedAptNum));
                break;
            //1: divider
            case MenuItemNames.SendToUnscheduledList: //Menu: Send to Unscheduled List
                butUnsched_Click();
                break;
            case MenuItemNames.BreakAppointment: //Menu: Break Appointment
                butBreak_Click();
                break;
            case MenuItemNames.MarkAsAsap:
                ASAP_Click();
                break;
            case MenuItemNames.SetComplete: // Menu: Set Complete
                butComplete_Click();
                break;
            case MenuItemNames.Delete: // Menu: Delete
                butDelete_Click(showPrompt: true);
                break;
            case MenuItemNames.PatientAppointments: // Menu: Patient Appointments
                DisplayOtherDlg(false);
                break;
            //8: divider
            case MenuItemNames.PrintLabel: // Menu: Print Label
                PrintApptLabel();
                break;
            case MenuItemNames.PrintCard: // Menu: Print Card
                _isPrintCardFamily = false;
                PrintApptCard();
                break;
            case MenuItemNames.PrintCardEntireFamily: // Menu: Print Card for Entire Family
                _isPrintCardFamily = true;
                PrintApptCard();
                break;
            case MenuItemNames.RoutingSlip: // Menu: Routing Slip
                //for now, this only allows one type of routing slip.  But it could be easily changed.
                appointment = Appointments.GetOneApt(contrApptPanel.SelectedAptNum);
                if (ApptIsNull(appointment))
                {
                    return;
                }

                using (var formRpRouting = new FormRpRouting())
                {
                    formRpRouting.AptNum = contrApptPanel.SelectedAptNum;
                    formRpRouting.DateSelected = contrApptPanel.DateSelected;
                    var customSheetDefs = SheetDefs.GetCustomForType(SheetTypeEnum.RoutingSlip);
                    if (customSheetDefs.Count == 0)
                    {
                        formRpRouting.SheetDefNum = 0;
                    }
                    else
                    {
                        formRpRouting.SheetDefNum = customSheetDefs[0].SheetDefNum;
                    }

                    formRpRouting.ShowDialog();
                }

                break;
            case MenuItemNames.OrthoChart: //Open Patient Ortho Chart
                using (var formOrthoChart = new FormOrthoChart(_patient))
                {
                    formOrthoChart.ShowDialog();
                }

                break;
            case MenuItemNames.HomePhone: //Call Home Phone
                if (Programs.GetCur(ProgramName.DentalTekSmartOfficePhone).Enabled)
                {
                    DentalTek.PlaceCall(_patient.HmPhone);
                }
                else
                {
                    AutomaticCallDialingDisabledMessage();
                }

                break;
            case MenuItemNames.WorkPhone: //Call Work Phone
                if (Programs.GetCur(ProgramName.DentalTekSmartOfficePhone).Enabled)
                {
                    DentalTek.PlaceCall(_patient.WkPhone);
                }
                else
                {
                    AutomaticCallDialingDisabledMessage();
                }

                break;
            case MenuItemNames.WirelessPhone: //Call Wireless Phone
                if (Programs.GetCur(ProgramName.DentalTekSmartOfficePhone).Enabled)
                {
                    DentalTek.PlaceCall(_patient.WirelessPhone);
                }
                else
                {
                    AutomaticCallDialingDisabledMessage();
                }

                break;
            case MenuItemNames.SendText:
                appointment = Appointments.GetOneApt(contrApptPanel.SelectedAptNum);
                if (ApptIsNull(appointment))
                {
                    return;
                }

                if (!Security.IsAuthorized(EnumPermType.TextMessageSend))
                {
                    return;
                }

                GlobalFormOpenDental.SendTextMessage(appointment.PatNum, "");
                break;
            case MenuItemNames.SendConfirmationText:
                appointment = Appointments.GetOneApt(contrApptPanel.SelectedAptNum);
                if (ApptIsNull(appointment))
                {
                    return;
                }

                var aptOld = appointment.Copy();
                var patient = Patients.GetPat(appointment.PatNum);
                var message = PatComm.BuildConfirmMessage(ContactMethod.TextMessage, patient, appointment);
                var wasTextSent = GlobalFormOpenDental.SendTextMessage(patient.PatNum, message);
                if (wasTextSent)
                {
                    var newStatus = PrefC.GetLong(PrefName.ConfirmStatusTextMessaged);
                    Appointments.SetConfirmed(appointment, newStatus);
                    appointment.Confirmed = newStatus; //SetConfirmed doesn't set the appt status in memory
                    if (appointment.Confirmed != aptOld.Confirmed)
                    {
                        SecurityLogs.MakeLogEntry(EnumPermType.ApptConfirmStatusEdit, appointment.PatNum, Lans.g("Appointment confirmation status changed from") + " "
                                                                                                                                                                 + Defs.GetName(DefCat.ApptConfirmed, aptOld.Confirmed) + " " + Lans.g("to") + " " + Defs.GetName(DefCat.ApptConfirmed, appointment.Confirmed)
                                                                                                                                                                 + " " + Lans.g("from the appointment module") + ".", appointment.AptNum, aptOld.DateTStamp);
                    }

                    ModuleSelected(patient.PatNum); //Refresh the module so that the current appt status is updated
                }

                break;
            case MenuItemNames.SendComeInText:
                appointment = Appointments.GetOneApt(contrApptPanel.SelectedAptNum);
                if (ApptIsNull(appointment))
                {
                    return;
                }

                if (!GetArrivalsLoaded().TryGetComeInMsg(appointment, out message))
                {
                    MsgBox.Show(this, $"Unable to {MenuItemNames.SendComeInText} ");
                    return;
                }

                GlobalFormOpenDental.SendTextMessage(appointment.PatNum, message);
                break;
            case MenuItemNames.SendMessageToPay:
                appointment = Appointments.GetOneApt(contrApptPanel.SelectedAptNum);
                if (ApptIsNull(appointment))
                {
                    return;
                }

                patient = Patients.GetPat(appointment.PatNum);
                var formMessageToPayEdit = new FormMessageToPayEdit(patient);
                formMessageToPayEdit.ShowDialog();
                break;
            /*case MenuItemNames.BringOverlapToFront:
                ContrApptSheet2.OverlapOrdering.CycleOverlappingAppts(contrApptPanel.SelectedAptNum);//Change priorities
                contrApptPanel.SelectedAptNum=-1;//removed selected appointment as to show changes with overlap ordering
                ContrApptSheet2.DoubleBufferDraw(createApptShadows:true);
                break;*/
        }
    }

    #endregion Methods - Event Handlers Menu Appt Click

    #region Methods - Event Handlers Menu Blockout Click

    /// <summary>Deletes selected web schedule ASAP blockout.</summary>
    private void DeleteWebSchedAsapBlockout_Click(object sender, EventArgs e)
    {
        var schedule = GetClickedSchedule(ScheduleType.WebSchedASAP);
        if (schedule != null && MsgBox.Show(this, MsgBoxButtons.OKCancel, "This could cause messages to not be sent.  Are you sure?"
                , MenuItemNames.DeleteWebSchedAsapBlockout))
        {
            Schedules.Delete(schedule, true);
            Schedules.BlockoutLogHelper(BlockoutAction.Delete, schedule);
            RefreshPeriodSchedules();
        }
    }

    private void menuBlockAdd_Click(object sender, EventArgs e)
    {
        //Pre-calculate the list of Blockout Types to show in FormScheduleBlockEdit
        var listDefsBlockoutTypes = Defs.GetDefsForCategory(DefCat.BlockoutTypes, true);
        if (!Security.IsAuthorized(EnumPermType.Blockouts, true))
        {
            //This is a special case, so we only keep blockouts that are marked as NoSchedule or DontCopy
            listDefsBlockoutTypes.RemoveAll(x => !x.ItemValue.Contains(BlockoutType.DontCopy.GetDescription())
                                                 && !x.ItemValue.Contains(BlockoutType.NoSchedule.GetDescription()));
            if (listDefsBlockoutTypes.Count == 0 && !Security.IsAuthorized(EnumPermType.Blockouts))
            {
                //Intentional for error message
                //Security.IsAuthorized(...) will display the "Not authorized message."
                return;
            }
        }

        var schedule = new Schedule();
        schedule.SchedDate = _dateTimeClickedBlockout.Date;
        schedule.StartTime = ControlApptPanel.RoundTimeDown(_dateTimeClickedBlockout.TimeOfDay, contrApptPanel.MinPerIncr);
        schedule.StopTime = schedule.StartTime + TimeSpan.FromHours(1);
        if (schedule.StartTime > TimeSpan.FromHours(23))
        {
            //if user clicked anywhere during the last hour of the day, set blockout to the last hour of the day.
            schedule.StartTime = new TimeSpan(23, 00, 00);
            schedule.StopTime = new TimeSpan(23, 59, 00);
        }

        schedule.Ops.Add(_opNumClickedBlockout); //jordan 2019-05-20-This is new behavior to prefill op
        schedule.SchedType = ScheduleType.Blockout;
        using var formScheduleBlockEdit = new FormScheduleBlockEdit(schedule, Clinics.ClinicNum, listDefsBlockoutTypes);
        formScheduleBlockEdit.IsNew = true;
        formScheduleBlockEdit.ShowDialog();
        RefreshPeriodSchedules();
    }

    private void menuBlockClearClinic_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Blockouts))
        {
            return;
        }

        if (!MsgBox.Show(this, MsgBoxButtons.OKCancel, "Clear all blockouts for day for this clinic?"))
        {
            return;
        }

        var operatory = Operatories.GetOperatory(_opNumClickedBlockout);
        Schedules.ClearBlockoutsForClinic(operatory.ClinicNum, _dateTimeClickedBlockout.Date);
        Schedules.BlockoutLogHelper(BlockoutAction.Clear, dateTime: _dateTimeClickedBlockout.Date, clinicNum: operatory.ClinicNum);
        RefreshPeriodSchedules();
    }

    private void menuBlockClearDay_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Blockouts))
        {
            return;
        }

        if (!MsgBox.Show(this, MsgBoxButtons.OKCancel, "Clear all blockouts for day? (This may include blockouts not shown in the current appointment view)"))
        {
            return;
        }

        Schedules.ClearBlockoutsForDay(_dateTimeClickedBlockout.Date);
        Schedules.BlockoutLogHelper(BlockoutAction.Clear, dateTime: _dateTimeClickedBlockout.Date);
        RefreshPeriodSchedules();
    }

    private void menuBlockClearOp_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Blockouts))
        {
            return;
        }

        if (!MsgBox.Show(this, MsgBoxButtons.OKCancel, "Clear all blockouts for day in this operatory?"))
        {
            return;
        }

        Schedules.ClearBlockoutsForOp(_opNumClickedBlockout, _dateTimeClickedBlockout.Date);
        Schedules.BlockoutLogHelper(BlockoutAction.Clear, dateTime: _dateTimeClickedBlockout.Date, opNum: _opNumClickedBlockout);
        RefreshPeriodSchedules();
    }

    private void menuBlockCopy_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Blockouts))
        {
            return;
        }

        //not even enabled if not right click on a blockout
        var schedule = GetClickedBlockout();
        if (schedule == null)
        {
            MsgBox.Show(this, "Blockout not found.");
            return; //should never happen
        }

        _scheduleBlockoutClipboard = schedule.Copy();
    }

    private void menuBlockCut_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Blockouts))
        {
            return;
        }

        //not even enabled if not right click on a blockout
        var schedule = GetClickedBlockout();
        if (schedule == null)
        {
            MsgBox.Show(this, "Blockout not found.");
            return; //should never happen
        }

        _scheduleBlockoutClipboard = schedule.Copy();
        Schedules.Delete(schedule, true);
        Schedules.BlockoutLogHelper(BlockoutAction.Cut, schedule);
        RefreshPeriodSchedules();
    }

    private void menuBlockCutCopyPaste_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Blockouts))
        {
            return;
        }

        using var formBlockoutCutCopyPaste = new FormBlockoutCutCopyPaste();
        formBlockoutCutCopyPaste.SelectedDate = _dateTimeClickedBlockout.Date;
        var apptView = GetApptViewCur();
        if (ApptViews.IsNoneView(apptView))
        {
            //None view.
            formBlockoutCutCopyPaste.ApptViewNum = ApptViews.ApptViewNumNone;
        }
        else
        {
            formBlockoutCutCopyPaste.ApptViewNum = apptView.ApptViewNum;
        }

        formBlockoutCutCopyPaste.ShowDialog();
        RefreshPeriodSchedules();
    }

    private void menuBlockDelete_Click(object sender, EventArgs e)
    {
        //If the user doesn't have permission to delete, the menu option will not be enabled.
        var schedule = GetClickedBlockout();
        if (schedule == null)
        {
            MsgBox.Show(this, "Blockout not found.");
            return; //should never happen
        }

        Schedules.Delete(schedule, true);
        Schedules.BlockoutLogHelper(BlockoutAction.Delete, schedule);
        RefreshPeriodSchedules();
    }

    private void menuBlockEdit_Click(object sender, EventArgs e)
    {
        //Pre-calculate the list of blockouts to show.  If the user doesn't have the permission then a modified list is shown.
        var listDefsUserBlockout = Defs.GetDefsForCategory(DefCat.BlockoutTypes, true);
        if (!Security.IsAuthorized(EnumPermType.Blockouts, true))
        {
            //The modified list will only show blockouts marked as "DontCopy" or "NoSchedule"
            listDefsUserBlockout.RemoveAll(x => !x.ItemValue.Contains(BlockoutType.DontCopy.GetDescription())
                                                && !x.ItemValue.Contains(BlockoutType.NoSchedule.GetDescription()));
        }

        //not even enabled if not right click on a blockout
        var scheduleClicked = GetClickedBlockout();
        if (scheduleClicked == null)
        {
            MsgBox.Show(this, "Blockout not found.");
            return; //should never happen
        }

        using var formScheduleBlockEdit = new FormScheduleBlockEdit(scheduleClicked, Clinics.ClinicNum, listDefsUserBlockout);
        formScheduleBlockEdit.ShowDialog();
        RefreshPeriodSchedules();
    }

    private void menuBlockPaste_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Blockouts))
        {
            return;
        }

        var schedule = _scheduleBlockoutClipboard.Copy();
        schedule.Ops =
        [
            _opNumClickedBlockout
        ];
        schedule.SchedDate = _dateTimeClickedBlockout.Date;
        var timeSpanOriginalLength = schedule.StopTime - schedule.StartTime;
        schedule.StartTime = ControlApptPanel.RoundTimeDown(_dateTimeClickedBlockout.TimeOfDay, contrApptPanel.MinPerIncr);
        schedule.StopTime = schedule.StartTime + timeSpanOriginalLength;
        if (schedule.StopTime >= TimeSpan.FromDays(1))
        {
            //long span that spills over to next day
            MsgBox.Show(this, "This Blockout would go past midnight.");
            return;
        }

        schedule.ScheduleNum = 0; //Because Schedules.Overlaps() ignores matching ScheduleNums and we used the Copy() function above. Also, we insert below, so a new key will be created anyway.
        List<Schedule> listSchedulesOverlap;
        if (Schedules.Overlaps(schedule, out listSchedulesOverlap))
        {
            if (!PrefC.GetBool(PrefName.ReplaceExistingBlockout) || !Schedules.IsAppointmentBlocking(schedule.BlockoutType))
            {
                MsgBox.Show(this, "Blockouts not allowed to overlap.");
                return;
            }

            if (!MsgBox.Show(this, MsgBoxButtons.OKCancel, "Creating this blockout will cause blockouts to overlap. Continuing will delete the existing blockout(s). Continue?"))
            {
                return;
            }

            Schedules.DeleteMany(listSchedulesOverlap.Select(x => x.ScheduleNum).ToList());
        }

        Schedules.Insert(schedule, true);
        Schedules.BlockoutLogHelper(BlockoutAction.Paste, schedule);
        RefreshPeriodSchedules();
    }

    private void menuBlockTypes_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.DefEdit))
        {
            return;
        }

        using var formDefinitions = new FormDefinitions(DefCat.BlockoutTypes);
        formDefinitions.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.DefEdit, 0, "Definitions.");
        RefreshPeriodSchedules();
    }

    private Schedule GetClickedBlockout()
    {
        return GetClickedSchedule(ScheduleType.Blockout);
    }

    private Schedule GetClickedSchedule(ScheduleType schedType)
    {
        var listSchedulesForType = Schedules.GetListForType(contrApptPanel.ListSchedules, schedType, 0);
        //now find which blockout
        Schedule schedule = null;
        for (var i = 0; i < listSchedulesForType.Count; i++)
        {
            //skip if op doesn't match
            if (!listSchedulesForType[i].Ops.Contains(_opNumClickedBlockout))
            {
                continue;
            }

            if (listSchedulesForType[i].SchedDate.Date != _dateTimeClickedBlockout.Date)
            {
                continue;
            }

            if (listSchedulesForType[i].StartTime <= _dateTimeClickedBlockout.TimeOfDay
                && _dateTimeClickedBlockout.TimeOfDay < listSchedulesForType[i].StopTime)
            {
                schedule = listSchedulesForType[i];
                break;
            }
        }

        return schedule; //might be null;
    }

    #endregion Methods - Event Handlers Menu Blockout Click

    #region Methods - Event Handlers Menu TextAppts Click

    private void menuTextApptsForDay_Click(object sender, EventArgs e)
    {
        var listAppointments = Appointments.GetForPeriodList(contrApptPanel.DateSelected, contrApptPanel.DateSelected, Clinics.ClinicNum)
            .Where(x => !x.AptStatus.In(ApptStatus.PtNote, ApptStatus.PtNoteCompleted)).ToList();
        if (true && Clinics.ClinicNum == 0)
        {
            //The above query would have gotten appointments for all clinics. We need to filter to just ClinicNums of 0.
            listAppointments = listAppointments.Where(x => x.ClinicNum == 0).ToList();
        }

        SendTextMessages(listAppointments.Select(x => x.PatNum).ToList());
    }

    private void menuTextApptsForDayOp_Click(object sender, EventArgs e)
    {
        var dateClicked = contrApptPanel.DateSelected;
        var opNumClicked = contrApptPanel.OpNumClicked;
        var listPatNums = new List<long>();
        for (var i = 0; i < contrApptPanel.TableAppointments.Rows.Count; i++)
        {
            if (SIn.Long(contrApptPanel.TableAppointments.Rows[i]["Op"].ToString()) != opNumClicked)
            {
                continue;
            }

            if (SIn.DateTime(contrApptPanel.TableAppointments.Rows[i]["AptDateTime"].ToString()).Date != dateClicked)
            {
                continue;
            }

            listPatNums.Add(SIn.Long(contrApptPanel.TableAppointments.Rows[i]["PatNum"].ToString()));
        }

        SendTextMessages(listPatNums);
    }

    private void menuTextApptsForDayView_Click(object sender, EventArgs e)
    {
        var dateClicked = contrApptPanel.DateSelected;
        var listPatNums = new List<long>();
        for (var i = 0; i < contrApptPanel.TableAppointments.Rows.Count; i++)
        {
            var opNum = SIn.Long(contrApptPanel.TableAppointments.Rows[i]["Op"].ToString());
            if (!contrApptPanel.ListOpsVisible.Any(y => y.OperatoryNum == opNum))
            {
                //Make sure the appointments are visible in the current view.
                continue;
            }

            if (SIn.DateTime(contrApptPanel.TableAppointments.Rows[i]["AptDateTime"].ToString()).Date != dateClicked)
            {
                continue;
            }

            listPatNums.Add(SIn.Long(contrApptPanel.TableAppointments.Rows[i]["PatNum"].ToString()));
        }

        SendTextMessages(listPatNums);
    }

    private void menuTextASAPList_Click(object sender, EventArgs e)
    {
        var dateTimeClicked = contrApptPanel.DateTimeClicked;

        //Texting the ASAP list manually
        if (_formAsap is {IsDisposed: false})
        {
            _formAsap.Close();
        }

        _formAsap = new FormASAP();
        _formAsap.DateTimeChosen = dateTimeClicked;
        _formAsap.Show();
        if (_formAsap.WindowState == FormWindowState.Minimized)
        {
            _formAsap.WindowState = FormWindowState.Normal;
        }

        _formAsap.BringToFront();
    }

    private void FormASAP_FormClosed(object sender, FormClosedEventArgs e)
    {
        RefreshModuleDataPeriod();
        RefreshModuleScreenPeriod();
    }

    #endregion Methods - Event Handlers Menu TextAppts Click

    #region Methods - Event Handlers Menu Tasks

    private void menuTasks_Click(object sender, EventArgs e)
    {
        using var formTasksForAppt = new FormTasksForAppt(contrApptPanel.SelectedAptNum);
        formTasksForAppt.ShowDialog();
    }

    #endregion Methods - Event Handlers Menu Tasks

    #region Methods - Event Handlers Search

    private void ButAdvSearch_Click(object sender, EventArgs e)
    {
        if (pinBoard.SelectedIndex == -1)
        {
            if (pinBoard.ListPinBoardItems.Count == 0)
            {
                MsgBox.Show(this, "There is no appointment on the pinboard.");
                return;
            }

            pinBoard.SelectedIndex = 0;
        }

        var aptNum = pinBoard.ListPinBoardItems[pinBoard.SelectedIndex].AptNum;
        using var formApptSearchAdvanced = new FormApptSearchAdvanced(aptNum);
        var listProvNumsInBox = new List<long>();
        var listProviders = _listBoxProviders.Items.GetAll<ProviderDto>();
        for (var i = 0; i < listProviders.Count; i++)
        {
            listProvNumsInBox.Add(listProviders[i].Id);
        }

        formApptSearchAdvanced.SetSearchArgs(listProvNumsInBox, textBefore.Text, textAfter.Text, SIn.Date(dateSearch.Text));
        formApptSearchAdvanced.ShowDialog();
    }

    private void ButLab_Click(object sender, EventArgs e)
    {
        using var formLabCases = new FormLabCases();
        formLabCases.ShowDialog();
        if (formLabCases.GoToAptNum != 0)
        {
            var appointment = Appointments.GetOneApt(formLabCases.GoToAptNum);
            var patient = Patients.GetPat(appointment.PatNum);
            //PatientSelectedEventArgs eArgs=new OpenDental.PatientSelectedEventArgs(pat.PatNum,pat.GetNameLF(),pat.Email!="",pat.ChartNumber);
            //if(PatientSelected!=null){
            //	PatientSelected(this,eArgs);
            //}
            //Contr_PatientSelected(this,eArgs);
            RefreshModuleDataPatient(patient.PatNum);
            GlobalFormOpenDental.PatientSelected(patient, false, false);
            GlobalFormOpenDental.GoToModule(EnumModuleType.Appointments, dateSelected: appointment.AptDateTime, selectedAptNum: appointment.AptNum);
        }
    }

    private void butProvDentist_Click(object sender, EventArgs e)
    {
        _listProvidersSearch = [];
        _listBoxProviders.Items.Clear();
        var listProvidersShort = Providers.GetDeepCopy(true);
        for (var i = 0; i < listProvidersShort.Count; i++)
        {
            if (contrApptPanel.ListApptViewItems.Exists(x => x.ProvNum == listProvidersShort[i].Id))
            {
                if (!listProvidersShort[i].IsSecondary)
                {
                    _listProvidersSearch.Add(listProvidersShort[i]);
                    _listBoxProviders.Items.Add(listProvidersShort[i].Abbr, listProvidersShort[i]);
                }
            }
        }

        if (pinBoard.SelectedIndex == -1)
        {
            MsgBox.Show(this, "There is no appointment on the pinboard.");
            return;
        }

        DoSearch();
    }

    private void butProvHygenist_Click(object sender, EventArgs e)
    {
        _listProvidersSearch = [];
        _listBoxProviders.Items.Clear();
        var listProvidersShort = Providers.GetDeepCopy(true);
        for (var i = 0; i < listProvidersShort.Count; i++)
        {
            if (contrApptPanel.ListApptViewItems.Exists(x => x.ProvNum == listProvidersShort[i].Id))
            {
                if (listProvidersShort[i].IsSecondary)
                {
                    _listProvidersSearch.Add(listProvidersShort[i]);
                    _listBoxProviders.Items.Add(listProvidersShort[i].Abbr, listProvidersShort[i]);
                }
            }
        }

        if (pinBoard.SelectedIndex == -1)
        {
            MsgBox.Show(this, "There is no appointment on the pinboard.");
            return;
        }

        DoSearch();
    }

    private void butProvPick_Click(object sender, EventArgs e)
    {
        using var formProvidersMultiPick = new FormProvidersMultiPick();
        formProvidersMultiPick.SelectedProviders = _listProvidersSearch;
        formProvidersMultiPick.ShowDialog();
        if (formProvidersMultiPick.DialogResult != DialogResult.OK)
        {
            return;
        }

        _listBoxProviders.Items.Clear();
        for (var i = 0; i < formProvidersMultiPick.SelectedProviders.Count; i++)
        {
            _listBoxProviders.Items.Add(formProvidersMultiPick.SelectedProviders[i].Abbr, formProvidersMultiPick.SelectedProviders[i]);
        }

        _listProvidersSearch = formProvidersMultiPick.SelectedProviders;
        if (pinBoard.SelectedIndex == -1)
        {
            MsgBox.Show(this, "There is no appointment on the pinboard.");
            return;
        }

        DoSearch();
    }

    private void butRefresh_Click(object sender, EventArgs e)
    {
        if (pinBoard.SelectedIndex == -1)
        {
            if (pinBoard.ListPinBoardItems.Count > 0)
            {
                //if there are any appointments on the pinboard.
                pinBoard.SelectedIndex = pinBoard.ListPinBoardItems.Count - 1; //select last appt
            }
            else
            {
                MsgBox.Show(this, "There are no appointments on the pinboard.");
                return;
            }
        }

        DoSearch();
    }

    private void butSearch_Click(object sender, EventArgs e)
    {
        if (pinBoard.ListPinBoardItems.Count == 0)
        {
            MsgBox.Show(this, "An appointment must be placed on the pinboard before a search can be done.");
            return;
        }

        if (pinBoard.SelectedIndex == -1)
        {
            if (pinBoard.ListPinBoardItems.Count == 1)
            {
                pinBoard.SelectedIndex = 0;
            }
            else
            {
                MsgBox.Show(this, "An appointment on the pinboard must be selected before a search can be done.");
                return;
            }
        }

        if (!groupSearch.Visible)
        {
            //if search not already visible
            dateSearch.Text = DateTime.Today.ToShortDateString();
            ShowSearch();
        }

        DoSearch();
    }

    private void butSearchClose_Click(object sender, EventArgs e)
    {
        groupSearch.Visible = false;
    }

    private void butSearchCloseX_Click(object sender, EventArgs e)
    {
        groupSearch.Visible = false;
    }

    private void butSearchMore_Click(object sender, EventArgs e)
    {
        if (pinBoard.SelectedIndex == -1)
        {
            MsgBox.Show(this, "There is no appointment on the pinboard.");
            return;
        }

        if (_listScheduleOpenings == null || _listScheduleOpenings.Count < 1)
        {
            return;
        }

        dateSearch.Text = _listScheduleOpenings[_listScheduleOpenings.Count - 1].DateTimeAvail.ToShortDateString();
        DoSearch();
    }

    private void listSearchResults_MouseDown(object sender, MouseEventArgs e)
    {
        var clickedI = listSearchResults.IndexFromPoint(e.X, e.Y);
        if (clickedI == -1)
        {
            return;
        }

        ModuleSelected(_listScheduleOpenings[clickedI].DateTimeAvail);
    }

    #endregion Methods - Event Handlers Search

    #region Methods - Public Initialize

    ///<summary>Called from FormOpenDental each time the module is selected, but it doesn't do anything after the first time.</summary>
    public void InitializeOnStartup()
    {
        if (_hasInitializedOnStartup)
        {
            return;
        }

        contrApptPanel.DateSelected = DateTime.Today;
        FillViews();
        menuApt.Items.Clear();
        menuApt.Items.Add(new ToolStripMenuItem(Lan.g(this, "Copy to Pinboard"), null, menuApt_Click, MenuItemNames.CopyToPinboard));
        menuApt.Items.Add(new ToolStripSeparator());
        menuApt.Items.Add(new ToolStripMenuItem(Lan.g(this, "Send to Unscheduled List"), null, menuApt_Click, MenuItemNames.SendToUnscheduledList));
        _toolStripMenuItem = new ToolStripMenuItem(Lan.g(this, "Break Appointment"), null, menuApt_Click, MenuItemNames.BreakAppointment);
        menuApt.Items.Add(_toolStripMenuItem);
        menuApt.Items.Add(new ToolStripMenuItem(Lan.g(this, "Mark as ASAP"), null, menuApt_Click, MenuItemNames.MarkAsAsap));
        menuApt.Items.Add(new ToolStripMenuItem(Lan.g(this, "Set Complete"), null, menuApt_Click, MenuItemNames.SetComplete));
        menuApt.Items.Add(new ToolStripMenuItem(Lan.g(this, "Delete"), null, menuApt_Click, MenuItemNames.Delete));
        menuApt.Items.Add(new ToolStripMenuItem(Lan.g(this, "Patient Appointments"), null, menuApt_Click, MenuItemNames.PatientAppointments));
        menuApt.Items.Add(new ToolStripSeparator());
        menuApt.Items.Add(new ToolStripMenuItem(Lan.g(this, "Print Label"), null, menuApt_Click, MenuItemNames.PrintLabel));
        menuApt.Items.Add(new ToolStripMenuItem(Lan.g(this, "Print Card"), null, menuApt_Click, MenuItemNames.PrintCard));
        menuApt.Items.Add(new ToolStripMenuItem(Lan.g(this, "Print Card for Entire Family"), null, menuApt_Click, MenuItemNames.PrintCardEntireFamily));
        menuApt.Items.Add(new ToolStripMenuItem(Lan.g(this, "Routing Slip"), null, menuApt_Click, MenuItemNames.RoutingSlip));
        //menuBlockout
        menuBlockout.Items.Clear();
        menuBlockout.Items.Add(new ToolStripMenuItem(Lan.g(this, "Edit Blockout"), null, menuBlockEdit_Click, MenuItemNames.EditBlockout));
        menuBlockout.Items.Add(new ToolStripMenuItem(Lan.g(this, "Cut Blockout"), null, menuBlockCut_Click, MenuItemNames.CutBlockout));
        menuBlockout.Items.Add(new ToolStripMenuItem(Lan.g(this, "Copy Blockout"), null, menuBlockCopy_Click, MenuItemNames.CopyBlockout));
        menuBlockout.Items.Add(new ToolStripMenuItem(Lan.g(this, "Paste Blockout"), null, menuBlockPaste_Click, MenuItemNames.PasteBlockout));
        menuBlockout.Items.Add(new ToolStripMenuItem(Lan.g(this, "Delete Blockout"), null, menuBlockDelete_Click, MenuItemNames.DeleteBlockout));
        menuBlockout.Items.Add(new ToolStripMenuItem(Lan.g(this, MenuItemNames.DeleteWebSchedAsapBlockout), null, DeleteWebSchedAsapBlockout_Click, MenuItemNames.DeleteWebSchedAsapBlockout));
        menuBlockout.Items.Add(new ToolStripMenuItem(Lan.g(this, "Add Blockout"), null, menuBlockAdd_Click, MenuItemNames.AddBlockout));
        menuBlockout.Items.Add(new ToolStripMenuItem(Lan.g(this, "Blockout Cut-Copy-Paste"), null, menuBlockCutCopyPaste_Click, MenuItemNames.BlockoutCutCopyPaste));
        menuBlockout.Items.Add(new ToolStripMenuItem(Lan.g(this, "Clear All Blockouts for Day, Op only"), null, menuBlockClearOp_Click, MenuItemNames.ClearAllBlockoutsForDayOpOnly));
        if (true)
        {
            menuBlockout.Items.Add(new ToolStripMenuItem(Lan.g(this, "Clear All Blockouts for Day, Clinic only"), null, menuBlockClearClinic_Click, MenuItemNames.ClearAllBlockoutsForDayClinicOnly));
        }

        menuBlockout.Items.Add(new ToolStripMenuItem(Lan.g(this, "Edit Blockout Types"), null, menuBlockTypes_Click, MenuItemNames.EditBlockoutTypes));
        menuBlockout.Items.Add(new ToolStripSeparator {Name = MenuItemNames.BlockoutSpacer});
        menuBlockout.Items.Add(new ToolStripMenuItem(Lan.g(this, "Text ASAP List"), null, menuTextASAPList_Click, MenuItemNames.TextAsapList));
        menuBlockout.Items.Add(new ToolStripMenuItem(Lan.g(this, MenuItemNames.TextApptsForDayOp), null, menuTextApptsForDayOp_Click, MenuItemNames.TextApptsForDayOp));
        menuBlockout.Items.Add(new ToolStripMenuItem(Lan.g(this, MenuItemNames.TextApptsForDayView), null, menuTextApptsForDayView_Click, MenuItemNames.TextApptsForDayView));
        menuBlockout.Items.Add(new ToolStripMenuItem(Lan.g(this, MenuItemNames.TextApptsForDay), null, menuTextApptsForDay_Click, MenuItemNames.TextApptsForDay));
        //Recall Family
        menuRecall.MenuItems.Clear();
        menuRecall.MenuItems.Add(Lan.g(this, "Make Family Recall"), butFamRecall_Click);
        LayoutToolBar();
        SetWeeklyView(PrefC.GetBool(PrefName.ApptModuleDefaultToWeek));
        ODEvent.Fired += HandlePinClicked;
        _hasInitializedOnStartup = true;
    }


    public void LayoutToolBar()
    {
        toolBarMain.Buttons.Clear();
        toolBarMain.Buttons.Add(new ODToolBarButton(Lan.g(this, "Print"), 0, "", "Print"));
        toolBarMain.Buttons.Add(new ODToolBarButton(Lan.g(this, "Lists"), 1, Lan.g(this, "Appointment Lists"), "Lists"));
        toolBarMain.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
        toolBarMain.Buttons.Add(new ODToolBarButton(Lan.g(this, "Pat Appts"), EnumIcons.Patient, Lan.g(this, "Patient Appointments"), "PatAppts"));
        toolBarMain.Buttons.Add(new ODToolBarButton(Lan.g(this, "Make Appt"), EnumIcons.Add, Lan.g(this, "Make Appointment"), "Make"));
        var toolBarButton = new ODToolBarButton(Lan.g(this, "Make Recall"), EnumIcons.Recall, Lan.g(this, "Make Recall"), "Recall");
        toolBarButton.Style = ODToolBarButtonStyle.DropDownButton;
        toolBarButton.DropDownMenu = menuRecall;
        toolBarMain.Buttons.Add(toolBarButton);
        toolBarMain.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
        toolBarMain.Buttons.Add(new ODToolBarButton(Lan.g(this, "Unsched"), 3, Lan.g(this, "Send to Unscheduled List"), "Unsched"));
        toolBarMain.Buttons.Add(new ODToolBarButton(Lan.g(this, "Break"), EnumIcons.BreakAptX, Lan.g(this, "Break Appointment"), "Break"));
        //asap?
        toolBarMain.Buttons.Add(new ODToolBarButton(Lan.g(this, "Complete"), EnumIcons.Complete, "", "Complete"));
        toolBarMain.Buttons.Add(new ODToolBarButton(Lan.g(this, "Delete"), EnumIcons.DeleteX, "", "Delete"));
        if (!ProgramProperties.IsAdvertisingDisabled(ProgramName.RapidCall))
        {
            toolBarMain.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
            toolBarMain.Buttons.Add(new ODToolBarButton(Lan.g(this, "Rapid Call"), 2, "", "RapidCall"));
        }

        Logic.ProgramL.LoadToolBar(toolBarMain, EnumToolBar.ApptModule);
        toolBarMain.Invalidate();
        UpdateToolbarButtons();
    }

    #endregion Methods - Public Initialize

    #region Methods - Public Module Select

    ///<summary>This is a good way to set contrApptPanel.DateSelected while also refreshing the module.  If you are not changing the date or patient, then instead, simply use RefreshPeriod().</summary>
    public void ModuleSelected(DateTime date)
    {
        contrApptPanel.BeginUpdate(); //otherwise, the appointments will disappear
        contrApptPanel.DateSelected = date;
        long apptViewNum = 0;
        if (contrApptPanel.ApptViewCur != null)
        {
            apptViewNum = contrApptPanel.ApptViewCur.ApptViewNum;
        }

        if (contrApptPanel.IsWeeklyView)
        {
            if (_patient == null)
            {
                ModuleSelected(0, opNums: ApptViewItems.GetOpsForView(apptViewNum), provNums: ApptViewItems.GetProvsForView(apptViewNum));
            }
            else
            {
                ModuleSelected(_patient.PatNum, opNums: ApptViewItems.GetOpsForView(apptViewNum), provNums: ApptViewItems.GetProvsForView(apptViewNum));
            }
        }
        else
        {
            RefreshPeriod(opNums: ApptViewItems.GetOpsForView(apptViewNum), provNums: ApptViewItems.GetProvsForView(apptViewNum), isRefreshSchedules: true);
        }

        contrApptPanel.EndUpdate();
    }

    ///<summary>Refreshes the module for the passed in patient.  A patNum of 0 is acceptable.  Any ApptNums within listPinApptNums will get forcefully added to the main DataSet for the appointment module.</summary>
    public void ModuleSelected(long patNum, List<long> pinApptNums = null, List<long> opNums = null, List<long> provNums = null)
    {
        LayoutControls();
        
        if (IsHqNoneView())
        {
            return;
        }

        contrApptPanel.BeginUpdate();
        contrApptPanel.MinPerIncr = PrefC.GetInt(PrefName.AppointmentTimeIncrement);
        
        var listDefs = Defs.GetDefsForCategory(DefCat.AppointmentColors, true);
        var colorOpen = listDefs[0].ItemColor;
        var colorClosed = listDefs[1].ItemColor;
        var colorHoliday = listDefs[3].ItemColor;
        var colorBlockText = listDefs[4].ItemColor;
        var colorTimeLine = PrefC.GetColor(PrefName.AppointmentTimeLineColor);
        
        contrApptPanel.SetColors(colorOpen, colorClosed, colorHoliday, colorBlockText, colorTimeLine);
        contrApptPanel.SizeFont = float.Parse(PrefC.GetString(PrefName.ApptFontSize));
        contrApptPanel.WidthProvOnAppt = float.Parse(PrefC.GetString(PrefName.ApptProvbarWidth));
        
        SetWeeklyView(contrApptPanel.IsWeeklyView, skipModuleSelection: true); 
        
        RefreshModuleDataPatient(patNum);
        
        if (_patient is {PatStatus: PatientStatus.Deleted})
        {
            MsgBox.Show("Selected patient has been deleted by another workstation.");
            PatientL.RemoveFromMenu(_patient.PatNum);
            GlobalFormOpenDental.PatientSelected(new Patient(), false);
            
            RefreshModuleDataPatient(0);
        }

        if (_patient is {PatStatus: PatientStatus.Archived} && !Security.IsAuthorized(EnumPermType.ArchivedPatientSelect, suppressMessage: true))
        {
            GlobalFormOpenDental.PatientSelected(new Patient(), false);
            RefreshModuleDataPatient(0);
        }

        RefreshModuleDataPeriod(pinApptNums, opNums, provNums, forceRefreshSchedules: true);
        RefreshModuleScreenButtonsRight();
        RefreshModuleScreenPeriod();
        
        SetInitialStartTime();
        
        contrApptPanel.EndUpdate();
        
        ODEvent.Fire(ODEventType.ModuleSelected, _patient);
    }
    
    public void ModuleSelectedGoToAppt(long aptNum, DateTime dateSelected)
    {
        ModuleSelected(dateSelected);
        
        var dataRow = contrApptPanel.TableAppointments.Select().FirstOrDefault(x => SIn.Long(x["AptNum"].ToString()) == aptNum);
        if (dataRow is not null)
        {
            var patNum = SIn.Long(dataRow["PatNum"].ToString());
            
            RefreshModuleDataPatient(patNum);
        }

        contrApptPanel.SelectedAptNum = aptNum;
    }

    public void ModuleSelectedWithPinboard(long patNum, List<long> pinApptNums, DateTime dateSelected, bool showSearch)
    {
        contrApptPanel.BeginUpdate();
        contrApptPanel.DateSelected = dateSelected;
        
        ModuleSelected(patNum, pinApptNums);
        SendToPinBoardAptNums(pinApptNums);
        
        if (showSearch)
        {
            dateSearch.Text = dateSelected.ToShortDateString();
            if (!groupSearch.Visible)
            {
                ShowSearch();
            }

            DoSearch();
        }

        contrApptPanel.EndUpdate();
    }
    
    public void ModuleUnselected()
    {
    }
    
    public void RefreshPeriod(List<long> opNums = null, List<long> provNums = null, bool isRefreshAppointments = true, bool isRefreshSchedules = false, List<long> pinApptNums = null)
    {
        if (IsHqNoneView())
        {
            return;
        }

        contrApptPanel.BeginUpdate();
        
        RefreshModuleDataPeriod(pinApptNums, opNums, provNums, isRefreshAppointments, isRefreshSchedules);
        RefreshModuleScreenPeriod();
        
        contrApptPanel.EndUpdate();
    }

    public void RefreshPeriodSchedules()
    {
        RefreshPeriod(isRefreshAppointments: false, isRefreshSchedules: true);
    }
    
    public DateTime GetDateSelected()
    {
        return contrApptPanel.DateSelected;
    }

    public List<Operatory> GetListOpsVisible()
    {
        return contrApptPanel.ListOpsVisible;
    }

    public List<ProviderDto> GetListProvsVisible()
    {
        return contrApptPanel.ListProvsVisible;
    }
    
    public void DisplayOtherDlg(bool didInitialClick, DateTime dateTime, long opNum)
    {
        if (_patient == null)
        {
            return;
        }

        using var formApptsOther = new FormApptsOther(_patient.PatNum, pinBoard.ListPinBoardItems.Select(x => x.AptNum).ToList());
        
        formApptsOther.IsInitialDoubleClick = didInitialClick;
        formApptsOther.DateTimeClicked = contrApptPanel.DateTimeClicked;
        formApptsOther.OpNumClicked = contrApptPanel.OpNumClicked;
        formApptsOther.DateTNew = dateTime;
        formApptsOther.OpNumNew = opNum;
        formApptsOther.ShowDialog();
        
        ProcessOtherDlg(formApptsOther.GetOtherResult(), formApptsOther.PatNumSelected, formApptsOther.StringDateJumpTo, formApptsOther.ListAptNumsSelected.ToArray());
    }
    
    public void DisplayOtherDlg(bool didInitialClick)
    {
        if (_patient is null)
        {
            MsgBox.Show(this, "Please select a patient first.");
            return;
        }

        using var formApptsOther = new FormApptsOther(_patient.PatNum, pinBoard.ListPinBoardItems.Select(x => x.AptNum).ToList());
        
        formApptsOther.IsInitialDoubleClick = didInitialClick;
        formApptsOther.DateTimeClicked = contrApptPanel.DateTimeClicked;
        formApptsOther.OpNumClicked = contrApptPanel.OpNumClicked;
        formApptsOther.ShowDialog();
        
        ProcessOtherDlg(formApptsOther.GetOtherResult(), formApptsOther.PatNumSelected, formApptsOther.StringDateJumpTo, formApptsOther.ListAptNumsSelected.ToArray(), formApptsOther.ListAptViewJumpTos);
    }
    
    public void FunctionKeyPress(Keys keys)
    {
        var keyName = Enum.GetName(typeof(Keys), keys); //keyName will be F1, F2, ... F12
        var fKeyVal = int.Parse(keyName.TrimStart('F')); //strip off the F and convert to an int
        if (comboView.Items.GetAll<ApptView>().All(x => x.ApptViewNum != ApptViews.ApptViewNumNone))
        {
            fKeyVal--; //None view was excluded during "FillViews".  Map index 0 to F1, 1 to F2, etc.
        }

        if (comboView.Items.Count - 1 < fKeyVal)
        {
            return;
        }

        SetView(((ApptView) comboView.Items.GetObjectAt(fKeyVal)).ApptViewNum, true);
    }

    ///<summary>Used by parent form when a dialog needs to be displayed, but mouse might be down.  This forces a mouse up, and cleans up any mess so that dlg can show.</summary>
    public void MouseUpForced()
    {
        pinBoard.MouseUpForced();
        contrApptPanel.MouseUpForced();
    }

    ///<summary>This is public so that FormOpenDental can pass refreshed tasks here in order to avoid an extra query.</summary>
    public void RefreshReminders(List<Task> reminderTasks)
    {
        var sortedReminderTasks = reminderTasks
            .Where(x => x.DateTimeEntry.Date <= DateTime.Today)
            .OrderBy(x => x.DateTimeEntry)
            .ToList();
        
        tabReminders.Text = "Reminders";
        if (sortedReminderTasks.Count > 0)
        {
            tabReminders.Text += "*";
        }

        gridReminders.BeginUpdate();
        if (gridReminders.Columns.Count == 0)
        {
            gridReminders.Columns.Clear();
            gridReminders.Columns.Add(new GridColumn("", 17) {ImageList = imageListTasks});
            gridReminders.Columns.Add(new GridColumn("Description", 200));
        }

        gridReminders.ListGridRows.Clear();
        foreach (var reminder in sortedReminderTasks)
        {
            var gridRow = new GridRow();
            
            SetReminderGridRow(gridRow, reminder);
            
            gridReminders.ListGridRows.Add(gridRow);
        }

        gridReminders.EndUpdate();
    }

    private void SetReminderGridRow(GridRow gridRow, Task taskReminder)
    {
        gridRow.Tag = taskReminder;
        gridRow.Cells.Clear();
        
        var dateStr = "";
        if (taskReminder.DateTask.Year > 1880)
        {
            switch (taskReminder.DateType)
            {
                case TaskDateType.Day:
                    dateStr += taskReminder.DateTask.ToShortDateString() + " - ";
                    break;
                
                case TaskDateType.Week:
                    dateStr += "Week of " + taskReminder.DateTask.ToShortDateString() + " - ";
                    break;
                
                case TaskDateType.Month:
                    dateStr += taskReminder.DateTask.ToString("MMMM") + " - ";
                    break;
            }
        }
        else if (taskReminder.DateTimeEntry.Year > 1880)
        {
            dateStr += taskReminder.DateTimeEntry.ToShortDateString() + " " + taskReminder.DateTimeEntry.ToShortTimeString() + " - ";
        }

        var objDesc = "";
        if (taskReminder.TaskStatus == TaskStatusEnum.Done)
        {
            objDesc = Lan.g(this, "Done:") + taskReminder.DateTimeFinished.ToShortDateString() + " - ";
        }

        if (taskReminder.ObjectType == TaskObjectType.Patient)
        {
            if (taskReminder.KeyNum != 0)
            {
                objDesc += Patients.GetPat(taskReminder.KeyNum).GetNameLF() + " - ";
            }
        }
        else if (taskReminder.ObjectType == TaskObjectType.Appointment)
        {
            if (taskReminder.KeyNum != 0)
            {
                var appointment = Appointments.GetOneApt(taskReminder.KeyNum);
                if (appointment != null)
                {
                    objDesc = Patients.GetPat(appointment.PatNum).GetNameLF() //this is going to stay. Still not optimized, but here at HQ, we don't use it.
                              + "  " + appointment.AptDateTime
                              + "  " + appointment.ProcDescript
                              + "  " + appointment.Note
                              + " - ";
                }
            }
        }

        if (!taskReminder.Descript.StartsWith("==") && taskReminder.UserNum != 0)
        {
            objDesc += Userods.GetName(taskReminder.UserNum) + " - ";
        }

        if (PrefC.GetBool(PrefName.TasksNewTrackedByUser))
        {
            if (taskReminder.TaskStatus == TaskStatusEnum.Done)
            {
                gridRow.Cells.Add("1");
            }
            else
            {
                gridRow.Cells.Add(taskReminder.IsUnread ? "4" : "2");
            }
        }
        else
        {
            switch (taskReminder.TaskStatus)
            {
                case TaskStatusEnum.New:
                    gridRow.Cells.Add("4");
                    break;
                
                case TaskStatusEnum.Viewed:
                    gridRow.Cells.Add("2");
                    break;
                
                case TaskStatusEnum.Done:
                    gridRow.Cells.Add("1");
                    break;
            }
        }

        gridRow.Cells.Add(dateStr + objDesc + taskReminder.Descript);
        gridRow.ColorBackG = Defs.GetColor(DefCat.TaskPriorities, taskReminder.PriorityDefNum);
    }
    
    private void GetForCurView(ApptView apptView, bool isWeekly, List<Schedule> listSchedulesDaily)
    {
        //contrApptPanel.BeginUpdate();//already handled in RefreshModuleDataPeriod
        contrApptPanel.ApptViewCur = apptView;
        List<ProviderDto> listProvidersVis = null;
        List<Operatory> listOperatoriesVis = null;
        var rowsPerIncr = 0;
        List<ApptViewItem> listApptViewItemRowElements = null;
        List<ApptViewItem> listApptViewItems = null;
        ApptViewItemL.FillForApptView(isWeekly, apptView, out listProvidersVis, out listOperatoriesVis, out listApptViewItems, out listApptViewItemRowElements, out rowsPerIncr);
        ApptViewItemL.AddOpsForScheduledProvs(isWeekly, listSchedulesDaily, apptView, ref listOperatoriesVis);
        listOperatoriesVis.Sort(ApptViewItemL.CompareOps);
        contrApptPanel.ListProvsVisible = listProvidersVis;
        contrApptPanel.ListOpsVisible = listOperatoriesVis;
        contrApptPanel.RowsPerIncr = rowsPerIncr;
        contrApptPanel.ListApptViewItemRowElements = listApptViewItemRowElements;
        contrApptPanel.ListApptViewItems = listApptViewItems;
        //contrApptPanel.EndUpdate();
    }

    ///<summary>If needed, refreshes TableAppointments, TableApptFields, and TablePatFields tables.</summary>
    private void RefreshAppointmentsIfNeeded(DateTime dateStart, DateTime dateEnd, List<long> listPinApptNums = null,
        List<long> listOpNums = null, List<long> listProvNums = null, bool forceRefresh = false)
    {
        if (contrApptPanel.TableAppointments != null && contrApptPanel.TableApptFields != null && contrApptPanel.TablePatFields != null && !forceRefresh)
        {
            return; //If all data is already in memory and we are not forcing the refresh.
        }

        var includeVerifyIns = 
            contrApptPanel.ListApptViewItems != null && 
            contrApptPanel.ListApptViewItems.Exists(
                x => x.ElementDesc == EnumApptViewElement.VerifyIns_V.GetDescription());

        var table = Appointments.GetPeriodApptsTable(dateStart, dateEnd, aptNum: 0, isPlanned: false, listPinApptNums, listOpNums, listProvNums, allowRunQueryOnNoOps: false, includeVerifyIns: includeVerifyIns);
        if (table.Rows.Count > 0)
        {
            //This is an arbitrary but fixed order so that appointments always get drawn in the same order and don't appear to jump
            table = table.Select().OrderBy(x => SIn.Long(x["AptNum"].ToString())).CopyToDataTable();
        }

        contrApptPanel.TableAppointments = table;
        contrApptPanel.TableApptFields = Appointments.GetApptFields(contrApptPanel.TableAppointments);
        contrApptPanel.TablePatFields = Appointments.GetPatFields(contrApptPanel.TableAppointments.Select().Select(x => SIn.Long(x["PatNum"].ToString())).ToList());
        var arrivals = Arrivals.LoadArrivals(
            contrApptPanel.TableAppointments.Select().Select(x => SIn.Long(x["ClinicNum"].ToString())).Distinct().ToList(),
            contrApptPanel.TableAppointments.Select().Select(x => SIn.Long(x["AptNum"].ToString())).Distinct().ToList()
        );
        SetArrivalsLoaded(arrivals);
    }

    ///<summary>Fills PatCur from the database. If a patient change occurs, any selected appointment will be cleared.</summary>
    public void RefreshModuleDataPatient(long patNum)
    {
        if (_patient != null && _patient.PatNum != patNum)
        {
            //if patient changed
            contrApptPanel.SelectedAptNum = -1;
        }

        if (patNum == 0)
        {
            _patient = null;
            return;
        }

        //We have to go to the db because we need to get the most recent patient info, mainly the AskedToArriveEarly time.
        _patient = Patients.GetPat(patNum);
    }

    ///<summary>Gets op nums and prov nums for current view if not passed in.  Will refresh the appointments and schedules if the respective forceRefreshes are set.</summary>
    private void RefreshModuleDataPeriod(List<long> listPinApptNums = null, List<long> opNums = null, List<long> provNums = null, bool forceRefreshAppointments = true, bool forceRefreshSchedules = false)
    {
        long apptViewNum = -1;
        if (opNums == null)
        {
            apptViewNum = GetApptViewNumForUser();
            opNums = ApptViewItems.GetOpsForView(apptViewNum);
        }

        if (provNums == null)
        {
            if (apptViewNum < 0)
            {
                apptViewNum = GetApptViewNumForUser(); //Only run this query if we have to (haven't run it yet from this method).
            }

            provNums = ApptViewItems.GetProvsForView(apptViewNum);
        }

        RefreshSchedulesIfNeeded(contrApptPanel.DateStart, contrApptPanel.DateEnd, opNums, forceRefreshSchedules);
        //no dependencies:
        RefreshWaitingRoomTable();
        _dateTimeWaitingRmRefreshed = DateTime.Now;
        //SchedListPeriod=Schedules.ConvertTableToList(_dtSchedule);//happens internally in contrApptPanel when setting TableSchedule
        //no dependencies:
        var apptView = GetApptViewCur(apptViewNumOverride: apptViewNum);
        //for this line, I need contrApptPanel.ListSchedules to be already filled, which happens in RefreshSchedulesIfNeeded.
        GetForCurView(apptView, contrApptPanel.IsWeeklyView, contrApptPanel.ListSchedules);
        //for this line, I need contrApptPanel.ListApptViewItems to be already filled, which currently happens in GetForCurView:
        //Also need listOpNums and listProvNums to be prefilled.
        RefreshAppointmentsIfNeeded(contrApptPanel.DateStart, contrApptPanel.DateEnd, listPinApptNums, opNums, provNums, forceRefresh: forceRefreshAppointments);
    }

    /// <summary>If needed, refreshes TableSchedule, TableEmpSched, and TableProvSched tables.</summary>
    private void RefreshSchedulesIfNeeded(DateTime dateStart, DateTime dateEnd, List<long> listOpNums, bool isRefreshNeeded = false)
    {
        if (contrApptPanel.TableSchedule != null && contrApptPanel.TableEmpSched != null && contrApptPanel.TableProvSched != null && !isRefreshNeeded)
        {
            return; //If all data is already in memory and we are not forcing the refresh.
        }

        contrApptPanel.TableEmpSched = Schedules.GetPeriodEmployeeSchedTable(dateStart, dateEnd, Clinics.ClinicNum);
        contrApptPanel.TableProvSched = Schedules.GetPeriodProviderSchedTable(dateStart, dateEnd, Clinics.ClinicNum);
        contrApptPanel.TableSchedule = Schedules.GetPeriodSchedule(dateStart, dateEnd, listOpNums, false);
    }

    ///<summary>Always refreshes the _dtWaitingRoom table.</summary>
    private void RefreshWaitingRoomTable()
    {
        contrApptPanel.TableWaitingRoom = Appointments.GetPeriodWaitingRoomTable(DateTime.Now);
    }

    #endregion Methods - Private Refresh Data

    #region Methods - Private Refresh Screen

    private void FillEmpSched(bool hasNotes)
    {
        var table = contrApptPanel.TableEmpSched;
        gridEmpSched.BeginUpdate();
        gridEmpSched.Columns.Clear();
        gridEmpSched.Columns.Add(new GridColumn("Employee", 80));
        gridEmpSched.Columns.Add(new GridColumn("Schedule", 70));
        
        if (hasNotes)
        {
            gridEmpSched.Columns.Add(new GridColumn("Notes", 100));
        }

        gridEmpSched.ListGridRows.Clear();
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var gridRow = new GridRow();
            
            gridRow.Cells.Add(table.Rows[i]["empName"].ToString());
            gridRow.Cells.Add(table.Rows[i]["schedule"].ToString());
            
            if (hasNotes)
            {
                gridRow.Cells.Add(table.Rows[i]["Note"].ToString());
            }

            gridEmpSched.ListGridRows.Add(gridRow);
        }

        gridEmpSched.EndUpdate();
    }

    private void FillLab(List<LabCase> labCases)
    {
        var numberNotReceived = 0;
        
        foreach (var labCase in labCases)
        {
            if (labCase.DateTimeChecked.Year > 1880)
            {
                continue;
            }

            if (labCase.DateTimeRecd.Year > 1880)
            {
                continue;
            }

            numberNotReceived++;
        }

        if (numberNotReceived == 0)
        {
            textLab.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular);
            textLab.ForeColor = Color.Black;
            textLab.Text = "All Received";
            return;
        }

        textLab.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold);
        textLab.ForeColor = Color.DarkRed;
        textLab.Text = numberNotReceived + " NOT RECEIVED";
    }
    
    private void FillProduction(DateTime start, DateTime end)
    {
        if (!contrApptPanel.ListApptViewItemRowElements.Exists(x => x.ElementDesc.In("Production", "NetProduction")))
        {
            textProduction.Text = "";
            return;
        }
        
        var listProvNumsForApptView = new List<long>();
        var listOpsForApptView = new List<long>();
        var apptViewNum = GetApptViewNumForUser();
        if (PrefC.GetBool(PrefName.ApptModuleProductionUsesOps))
        {
            listOpsForApptView = ApptViewItems.GetOpsForView(apptViewNum);
        }
        else
        {
            listProvNumsForApptView = ApptViewItems.GetProvsForView(apptViewNum);
        }

        textProduction.Text = contrApptPanel.GetProduction(
            contrApptPanel.TableAppointments.Rows.OfType<DataRow>().ToList(), listOpsForApptView, listProvNumsForApptView, start, end);
    }

    private void FillProductionGoal(DateTime start, DateTime end)
    {
        if (!contrApptPanel.ListApptViewItemRowElements.Exists(x => x.ElementDesc.In("Production", "NetProduction")))
        {
            textProdGoal.Text = "";
            return;
        }

        var apptViewNum = GetApptViewNumForUser();
        //If the PrefName.ApptModuleProductionUsesOps is false, it will be filled with ProvNums for the appointment view. Otherwise, the list will be empty.
        var listProvNumsForApptView = new List<long>();
        //If the PrefName.ApptModuleProductionUsesOps is true, the list will be filled with OpNums for the appointment view. Otherwise, the list will be empty.
        var listOpsForApptView = new List<long>();
        if (PrefC.GetBool(PrefName.ApptModuleProductionUsesOps))
        {
            listOpsForApptView = ApptViewItems.GetOpsForView(apptViewNum);
        }
        else
        {
            listProvNumsForApptView = ApptViewItems.GetProvsForView(apptViewNum);
        }

        //This will return a dict of production goals for either the providers for the provider bars for the appointment view or the providers 
        //scheduled for the appointment view ops. 
        var prodGoalAmt = Providers.GetProductionGoalForProviders(listProvNumsForApptView, listOpsForApptView, start, end);
        textProdGoal.Text = prodGoalAmt.ToString("c0");
    }


    private void FillProvSched(bool hasNotes)
    {
        var table = contrApptPanel.TableProvSched;
        gridProv.BeginUpdate();
        gridProv.Columns.Clear();
        var col = new GridColumn(Lan.g("TableAppProv", "Provider"), 80);
        gridProv.Columns.Add(col);
        col = new GridColumn(Lan.g("TableAppProv", "Schedule"), 70);
        gridProv.Columns.Add(col);
        if (hasNotes)
        {
            col = new GridColumn(Lan.g("TableAppProv", "Notes"), 100);
            gridProv.Columns.Add(col);
        }

        gridProv.ListGridRows.Clear();
        GridRow row;
        for (var i = 0; i < table.Rows.Count; i++)
        {
            row = new GridRow();
            row.Cells.Add(table.Rows[i]["ProvAbbr"].ToString());
            row.Cells.Add(table.Rows[i]["schedule"].ToString());
            if (hasNotes)
            {
                row.Cells.Add(table.Rows[i]["Note"].ToString());
            }

            gridProv.ListGridRows.Add(row);
        }

        gridProv.EndUpdate();
    }

    ///<summary>Fills comboView with the current list of views. Triggers ModuleSelected().  Also called from FormOpenDental.RefreshLocalData().</summary>
    public void FillViews()
    {
        var listApptViews = new List<ApptView>();
        //Do NOT allow 'Headquarters' to have access to clinic specific apptviews.
        //Likewise, we do not want clinic specific views to be accessible from specific clinic filters.
        var listApptViewsClinic = ApptViews.GetWhere(x => !(Clinics.ClinicNum != x.ClinicNum));
        var offset = 1;
        if (listApptViewsClinic.Count == 0 || !PrefC.GetBool(PrefName.EnterpriseNoneApptViewDefaultDisabled))
        {
            //First item is "None" view.
            listApptViews.Add(new ApptView
            {
                ApptViewNum = ApptViews.ApptViewNumNone,
                Description = Lan.g(this, "none")
            });
            offset = 0;
        }

        listApptViews.AddRange(listApptViewsClinic);
        comboView.Items.Clear();
        comboView.Items.AddList(listApptViews
            //Views 1 through 12 get Function key shortcuts. None view does not.
            , (x) => (listApptViews.IndexOf(x).Between(1 - offset, 12 - offset) ? $"F{listApptViews.IndexOf(x) + offset}-" : "") + x.Description);
        var apptView = GetApptViewForUser(); //Determine which view this user should select on load.
        SetView(apptView.ApptViewNum, false); //this also triggers ModuleSelected()
    }

    ///<summary>Once per second, this grid refills itself in order to show the time ticking by.  This does not require a trip to the database.</summary>
    private void FillWaitingRoom()
    {
        if (!Visible)
        {
            return;
        }

        if (contrApptPanel.TableWaitingRoom == null)
        {
            return;
        }

        var timeSpanDeltaSinceRefresh = DateTime.Now - _dateTimeWaitingRmRefreshed;
        var table = contrApptPanel.TableWaitingRoom;
        var listOperatoriesForClinic = new List<Operatory>();
        var listOperatoriesForApptView = new List<Operatory>();
        if (PrefC.GetBool(PrefName.WaitingRoomFilterByView))
        {
            //In order to filter the waiting room by appointment view, we need to always grab the operatories visible for TODAY.
            //This way, regardless of what day the customer is looking at, the waiting room will only change when they change appointment views.
            //Always use the schedules from SchedListPeriod which is refreshed any time RefreshModuleDataPeriod() is invoked.
            var apptView = GetApptViewCur();
            var listSchedulesForToday = contrApptPanel.ListSchedules.FindAll(x => x.SchedDate == DateTime.Today);
            listOperatoriesForApptView = ApptViewItemL.GetOpsForApptView(apptView, contrApptPanel.IsWeeklyView, listSchedulesForToday);
        }

        if (true)
        {
            //Using clinics
            listOperatoriesForClinic = Operatories.GetOpsForClinic(Clinics.ClinicNum);
        }

        gridWaiting.BeginUpdate();
        gridWaiting.Columns.Clear();
        gridWaiting.Columns.Add(new GridColumn("Patient", 130));
        gridWaiting.Columns.Add(new GridColumn("Waited", 100, HorizontalAlignment.Center));
        gridWaiting.ListGridRows.Clear();
        DateTime timeWait;
        GridRow row;
        var waitingRoomAlertTime = PrefC.GetInt(PrefName.WaitingRoomAlertTime);
        var waitingRoomAlertColor = PrefC.GetColor(PrefName.WaitingRoomAlertColor);
        for (var i = 0; i < table.Rows.Count; i++)
        {
            //Always filter the waiting room by appointment view first, regardless of using clinics or not.
            if (PrefC.GetBool(PrefName.WaitingRoomFilterByView))
            {
                var isInView = false;
                for (var j = 0; j < listOperatoriesForApptView.Count; j++)
                {
                    if (listOperatoriesForApptView[j].OperatoryNum == SIn.Long(table.Rows[i]["OpNum"].ToString()))
                    {
                        isInView = true;
                        break;
                    }
                }

                if (!isInView)
                {
                    continue;
                }
            }

            //We only want to filter the waiting room by the clinic's operatories when clinics are enabled and they are not using 'Headquarters' mode.
            if (true && Clinics.ClinicNum != 0)
            {
                var isInView = false;
                for (var j = 0; j < listOperatoriesForClinic.Count; j++)
                {
                    if (listOperatoriesForClinic[j].OperatoryNum == SIn.Long(table.Rows[i]["OpNum"].ToString()))
                    {
                        isInView = true;
                        break;
                    }
                }

                if (!isInView)
                {
                    continue;
                }
            }

            row = new GridRow();
            var patName = "";
            var apptView = GetApptViewCur();
            if (apptView != null)
            {
                switch (apptView.WaitingRmName)
                {
                    case EnumWaitingRmName.LastFirst:
                        patName = $"{table.Rows[i]["LName"]}, {table.Rows[i]["FName"]}";
                        break;
                    case EnumWaitingRmName.FirstLastI:
                        patName = $"{table.Rows[i]["FName"]}, {table.Rows[i]["LName"].ToString().Substring(0, 1)}";
                        break;
                    case EnumWaitingRmName.First:
                        patName = $"{table.Rows[i]["FName"]}";
                        break;
                }
            }
            else
            {
                patName = table.Rows[i]["patName"].ToString();
            }

            row.Cells.Add(patName);
            timeWait = DateTime.Parse(table.Rows[i]["waitTime"].ToString()); //we ignore date
            timeWait += timeSpanDeltaSinceRefresh;
            row.Cells.Add(timeWait.ToString("H:mm:ss"));
            row.Bold = false;
            if (waitingRoomAlertTime > 0 && waitingRoomAlertTime <= timeWait.Minute + timeWait.Hour * 60)
            {
                row.ColorText = waitingRoomAlertColor;
                row.Bold = true;
            }

            gridWaiting.ListGridRows.Add(row);
        }

        gridWaiting.EndUpdate();
    }

    ///<summary>Sets buttons at right to enabled/disabled. Sets value of listConfirmed. Was previously called RefreshModuleScreenPatient.</summary>
    public void RefreshModuleScreenButtonsRight()
    {
        //considered only doing this once when starting program, but would then need to refresh it if we change definitions.  Might still try that.
        listConfirmed.Items.Clear();
        var listDefs = Defs.GetDefsForCategory(DefCat.ApptConfirmed, true);
        for (var i = 0; i < listDefs.Count; i++)
        {
            listConfirmed.Items.Add(listDefs[i].ItemValue);
        }

        UpdateToolbarButtons();
    }

    ///<summary>Redraws screen based on data already gathered.  RefreshModuleDataPeriod will have already retrieved the data from the db.</summary>
    public void RefreshModuleScreenPeriod()
    {
        monthCalendarOD.SetDateSelected(contrApptPanel.DateSelected);
        //LayoutPanels();
        labelDate.Text = contrApptPanel.DateStart.ToString("ddd");
        labelDate2.Text = contrApptPanel.DateStart.ToString("-  MMM d");
        RefreshPinboardImages();
        List<long> listOperatoryNums = null;
        if (Clinics.ClinicNum > 0)
        {
            listOperatoryNums = Operatories.GetOpsForClinic(Clinics.ClinicNum).Select(x => x.OperatoryNum).ToList();
        }

        var listLabCases = LabCases.GetForPeriod(contrApptPanel.DateStart, contrApptPanel.DateEnd, listOperatoryNums);
        FillLab(listLabCases);
        FillProduction(contrApptPanel.DateStart, contrApptPanel.DateEnd);
        FillProductionGoal(contrApptPanel.DateStart, contrApptPanel.DateEnd);
        FillProvSched(true);
        FillEmpSched(true);
        FillWaitingRoom();
        contrApptPanel.RedrawAsNeeded();
    }

    /// <summary>This refreshes the images on the pinboard, in case the view changed.</summary>
    private void RefreshPinboardImages()
    {
        if (contrApptPanel.TableAppointments == null)
        {
            return;
        }

        for (var i = 0; i < pinBoard.ListPinBoardItems.Count; i++)
        {
            //I'm a little worried that this could be slow, but there is usually only 0 to 1 appt on pinboard
            var dataRow = contrApptPanel.TableAppointments.Rows.OfType<DataRow>().FirstOrDefault(x => SIn.Long(x["AptNum"].ToString()) == pinBoard.ListPinBoardItems[i].AptNum);
            if (dataRow == null)
            {
                continue;
            }

            pinBoard.ListPinBoardItems[i].DataRowAppt = dataRow;
            var pattern = SIn.String(dataRow["Pattern"].ToString());
            var patternShowing = contrApptPanel.GetPatternShowing(pattern);
            var sizeAppt = contrApptPanel.SetSize(pattern);
            var bitmap = new Bitmap(pinBoard.Width - 2, (int) sizeAppt.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                contrApptPanel.GetBitmapForPinboard(g, dataRow, patternShowing, bitmap.Width, bitmap.Height);
            }

            pinBoard.ListPinBoardItems[i].BitmapAppt = bitmap;
            pinBoard.Invalidate();
            //bitmap.Dispose();//crashes
        }
    }

    ///<summary>Happens once per minute.  It used to just move the red timebar down without querying the database.  If pref.ApptModuleRefreshesEveryMinute is on (it is by default), then this instead queries the database for appt signals so that the waiting room list shows accurately.  The update to the waiting room grid is on a different timer.</summary>
    public void TickRefresh()
    {
        //dates already set
        if (PrefC.GetBool(PrefName.ApptModuleRefreshesEveryMinute))
        {
            if (PrefC.GetLong(PrefName.ProcessSigsIntervalInSecs) == 0)
            {
                //Signal processing is disabled.
                RefreshPeriod();
            }
            else
            {
                //Calling Signalods.RefreshTimed() was causing issues for large customers. This resulted in 100,000+ rows of signalod's returned.
                //Now we only query for the specific signals we care about. Instead of using Signalods.SignalLastRefreshed we now use Signalods.ApptSignalLastRefreshed.
                //Signalods.ApptSignalLastRefreshed mimics the behavior of Signalods.SignalLastRefreshed but is guaranteed to not be stale from inactive sessions.
                var listSignals = Signalods.RefreshTimed(Signalods.DateTApptSignalLastRefreshed, [InvalidType.Appointment, InvalidType.Schedules]);
                var listOpNumsVisible = contrApptPanel.ListOpsVisible.Select(x => x.OperatoryNum).ToList();
                var listProvNumsVisible = contrApptPanel.ListProvsVisible.Select(x => x.Id).ToList();
                var isApptRefresh = Signalods.IsApptRefreshNeeded(contrApptPanel.DateStart, contrApptPanel.DateEnd, listSignals, listOpNumsVisible, listProvNumsVisible);
                var isSchedRefresh = Signalods.IsSchedRefreshNeeded(contrApptPanel.DateStart, contrApptPanel.DateEnd, listSignals, listOpNumsVisible, listProvNumsVisible);
                //either we have signals from other machines telling us to refresh, or we aren't using signals, in which case we still want to refresh
                RefreshPeriod(isRefreshAppointments: isApptRefresh, isRefreshSchedules: isSchedRefresh);
            }
        }
        else
        {
            contrApptPanel.RedrawAsNeeded(); //just for the red time line
        }

        Signalods.DateTApptSignalLastRefreshed = MiscData.GetNowDateTime();
        //GC.Collect();	
    }

    ///<summary>Enables toolbar buttons if a patient is selected, otherwise disables them.</summary>
    private void UpdateToolbarButtons()
    {
        var dataRow = contrApptPanel.GetDataRowForSelected();
        if (dataRow != null)
        {
            toolBarMain.Buttons["Unsched"].Enabled = true;
            toolBarMain.Buttons["Break"].Enabled = true;
            toolBarMain.Buttons["Complete"].Enabled = true;
            toolBarMain.Buttons["Delete"].Enabled = true;
            var confirmed = dataRow["Confirmed"].ToString();
            listConfirmed.SelectedIndex = Defs.GetOrder(DefCat.ApptConfirmed, SIn.Long(confirmed)); //could be -1
            if (!Security.IsAuthorized(EnumPermType.ApptConfirmStatusEdit, true))
            {
                //Suppress message because it would be very annoying to users.
                listConfirmed.Enabled = false;
            }
            else
            {
                listConfirmed.Enabled = true;
            }
        }
        else
        {
            //even if an appt on the pinboard is selected, these are all grayed out
            toolBarMain.Buttons["Unsched"].Enabled = false;
            toolBarMain.Buttons["Break"].Enabled = false;
            toolBarMain.Buttons["Complete"].Enabled = false;
            toolBarMain.Buttons["Delete"].Enabled = false;
            listConfirmed.Enabled = false;
            if (pinBoard.SelectedIndex != -1)
            {
                dataRow = pinBoard.ListPinBoardItems[pinBoard.SelectedIndex].DataRowAppt;
                listConfirmed.SelectedIndex = Defs.GetOrder(DefCat.ApptConfirmed, SIn.Long(dataRow["Confirmed"].ToString())); //could be -1
            }
        }

        toolBarMain.Invalidate();
    }

    #endregion Methods - Private Refresh Screen

    #region Methods - Private Search

    private void DoSearch(bool isForMakeRecall = false)
    {
        Cursor = Cursors.WaitCursor;
        DateTime dateAfter;
        dateAfter = SIn.Date(dateSearch.Text);
        if (dateAfter.Year < 1880)
        {
            Cursor = Cursors.Default;
            MsgBox.Show(this, "Invalid date.");
            return;
        }

        var timeSpanBefore = new TimeSpan(0);
        if (textBefore.Text != "")
        {
            var stringArrayHrmin = textBefore.Text.Split([':'], StringSplitOptions.RemoveEmptyEntries); //doesn't work with foreign times.
            var hr = "0";
            if (stringArrayHrmin.Length > 0)
            {
                hr = stringArrayHrmin[0];
            }

            var min = "0";
            if (stringArrayHrmin.Length > 1)
            {
                min = stringArrayHrmin[1];
            }

            timeSpanBefore = TimeSpan.FromHours(SIn.Double(hr))
                             + TimeSpan.FromMinutes(SIn.Double(min));
            if (radioBeforePM.Checked && timeSpanBefore.Hours < 12)
            {
                timeSpanBefore = timeSpanBefore + TimeSpan.FromHours(12);
            }
        }

        var timeSpanAfter = new TimeSpan(0);
        if (textAfter.Text != "")
        {
            var stringArrayHrmin = textAfter.Text.Split([':'], StringSplitOptions.RemoveEmptyEntries); //doesn't work with foreign times.
            var hr = "0";
            if (stringArrayHrmin.Length > 0)
            {
                hr = stringArrayHrmin[0];
            }

            var min = "0";
            if (stringArrayHrmin.Length > 1)
            {
                min = stringArrayHrmin[1];
            }

            timeSpanAfter = TimeSpan.FromHours(SIn.Double(hr))
                            + TimeSpan.FromMinutes(SIn.Double(min));
            if (radioAfterPM.Checked && timeSpanAfter.Hours < 12)
            {
                timeSpanAfter = timeSpanAfter + TimeSpan.FromHours(12);
            }
        }

        if (_listBoxProviders.Items.Count == 0)
        {
            Cursor = Cursors.Default;
            MsgBox.Show(this, "Please pick a provider.");
            return;
        }

        var longArrayProvNums = new long[_listBoxProviders.Items.Count];
        var listProvNums = new List<long>();
        for (var i = 0; i < longArrayProvNums.Length; i++)
        {
            longArrayProvNums[i] = _listProvidersSearch[i].Id;
            listProvNums.Add(_listProvidersSearch[i].Id);
            //providersList.Add(providers[i]);
        }

        var listOperatoryNums = new List<long>();
        var listClinicNums = new List<long>();

        if (Clinics.ClinicNum != 0)
        {
            //not HQ
            listClinicNums.Add(Clinics.ClinicNum);
            listOperatoryNums = Operatories.GetOpsForClinic(Clinics.ClinicNum).Select(x => x.OperatoryNum).ToList(); //get ops for the currently selected clinic only
        }
        else
        {
            //HQ
            var apptView = GetApptViewCur();
            if (ApptViews.IsNoneView(apptView))
            {
                //none view
                MsgBox.Show(this, "Must have a view selected to search for appointment."); //this should never get hit. Just in case.
                return;
            }

            //get the disctinct clinic nums for the operatories in the current appointment view
            var listOperatoryNumsForView = ApptViewItems.GetOpsForView(apptView.ApptViewNum);
            var listOperatories = Operatories.GetOperatories(listOperatoryNumsForView, true);
            listClinicNums = listOperatories.Select(x => x.ClinicNum).Distinct().ToList();
            listOperatoryNums = listOperatories.Select(x => x.OperatoryNum).ToList();
        }

        //the result might be empty
        if (pinBoard.SelectedIndex == -1)
        {
            Cursor = Cursors.Default;
            MsgBox.Show(this, "Please select an item on the pinboard."); //shouldn't happen
            return;
        }

        List<long> listDefNumsBlockoutTypes = null;
        AppointmentType appointmentType = null;
        var aptNum = pinBoard.ListPinBoardItems[pinBoard.SelectedIndex].AptNum;
        var appointment = Appointments.GetOneApt(aptNum);
        if (appointment != null)
        {
            appointmentType = AppointmentTypes.GetOne(appointment.AppointmentTypeNum);
        }

        //If this appointment has an AppointmentType associated to any BlockoutTypes, set them as listBlockoutTypes. We want to automatically filter results for them.
        if (appointmentType != null && !string.IsNullOrEmpty(appointmentType.BlockoutTypes))
        {
            var listStrings = appointmentType.BlockoutTypes.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
            //Convert into a list of longs
            listDefNumsBlockoutTypes = listStrings.Select(x => SIn.Long(x, throwExceptions: false)).ToList();
        }

        _listScheduleOpenings = ApptSearch.GetSearchResults(aptNum, dateAfter, dateAfter.AddDays(731)
            , listProvNums, listOperatoryNums, listClinicNums, timeSpanBefore, timeSpanAfter, listBlockoutTypes: listDefNumsBlockoutTypes, isForMakeRecall: isForMakeRecall);
        listSearchResults.Items.Clear();
        for (var i = 0; i < _listScheduleOpenings.Count; i++)
        {
            listSearchResults.Items.Add(
                _listScheduleOpenings[i].DateTimeAvail.ToString("ddd") + "\t" + _listScheduleOpenings[i].DateTimeAvail.ToShortDateString() + "     "
                + _listScheduleOpenings[i].DateTimeAvail.ToShortTimeString());
        }

        if (listSearchResults.Items.Count > 0)
        {
            listSearchResults.SetSelected(0);
            ModuleSelected(_listScheduleOpenings[0].DateTimeAvail);
        }

        Cursor = Cursors.Default;
        //scroll to make visible?
        //highlight anything?
    }

    ///<summary>Positions the search box, fills it with initial data except date, and makes it visible.</summary>
    private void ShowSearch()
    {
        _listProvidersSearch = [];
        var listProvidersShort = Providers.GetDeepCopy(true);
        groupSearch.Location = panelCalendar.Location with {Y = panelCalendar.Location.Y + panelCalendarLower.Location.Y + pinBoard.Bottom + 2};
        textBefore.Text = "";
        textAfter.Text = "";
        _listBoxProviders.Items.Clear();
        if (pinBoard.SelectedIndex == -1)
        {
            return;
        }

        var dataRow = pinBoard.ListPinBoardItems[pinBoard.SelectedIndex].DataRowAppt;
        var isHygiene = SIn.Bool(dataRow["IsHygiene"].ToString());
        var provHyg = SIn.Long(dataRow["ProvHyg"].ToString());
        var provNum = SIn.Long(dataRow["ProvNum"].ToString());
        var aptNum = SIn.Long(dataRow["AptNum"].ToString());
        for (var i = 0; i < listProvidersShort.Count; i++)
        {
            if (isHygiene && listProvidersShort[i].Id == provHyg)
            {
                //If their appiontment is hygine, the list will start with just their hygine provider
                _listBoxProviders.Items.Add(listProvidersShort[i].Abbr, listProvidersShort[i]);
                _listProvidersSearch.Add(listProvidersShort[i]);
            }
            else if (!isHygiene && listProvidersShort[i].Id == provNum)
            {
                //If their appointment is not hygine, they will start with just their primary provider
                _listBoxProviders.Items.Add(listProvidersShort[i].Abbr, listProvidersShort[i]);
                _listProvidersSearch.Add(listProvidersShort[i]);
            }
        }

        groupSearch.Visible = true;
    }

    #endregion Methods - Private Search

    private bool ApptIsNull(Appointment appointment)
    {
        if (appointment is not null)
        {
            return false;
        }

        MsgBox.Show(this, "Selected appointment no longer exists.");

        RefreshPeriod();

        return true;
    }

    private void ASAP_Click()
    {
        if (!Security.IsAuthorized(EnumPermType.AppointmentEdit))
        {
            return;
        }

        var appointment = Appointments.GetOneApt(contrApptPanel.SelectedAptNum);
        if (ApptIsNull(appointment))
        {
            return;
        }

        if (appointment.Priority == ApptPriority.ASAP)
        {
            MsgBox.Show(this, "Already ASAP");
            return;
        }

        Appointments.SetPriority(appointment, ApptPriority.ASAP);
        MsgBox.Show(this, "Done");
    }

    private void AutomaticCallDialingDisabledMessage()
    {
        if (ProgramProperties.IsAdvertisingDisabled(ProgramName.DentalTekSmartOfficePhone))
        {
            return;
        }

        MsgBox.Show("Automatic dialing of patient phone numbers requires an additional service.\r\n"
                    + "Contact Open Dental for more information.");
        try
        {
            Process.Start("http://www.opendental.com/resources/redirects/redirectdentaltekinfo.html");
        }
        catch (Exception)
        {
            MsgBox.Show("Could not find http://www.opendental.com/contact.html \r\n"
                        + "Please set up a default web browser.");
        }
    }

    private void CheckStatus()
    {
        if (_patient.PatStatus == PatientStatus.Inactive
            || _patient.PatStatus == PatientStatus.Archived
            || _patient.PatStatus == PatientStatus.Prospective)
        {
            MsgBox.Show(this, "Warning. Patient is not active.");
        }

        if (_patient.PatStatus == PatientStatus.Deceased)
        {
            MsgBox.Show(this, "Warning. Patient is deceased.");
        }
    }

    private void ComboViewChanged()
    {
        var viewNumSelected = comboView.GetSelected<ApptView>()?.ApptViewNum ?? 0; //Selected view.
        SetView(viewNumSelected, true);
    }

    private void CopyApptStructure(Appointment appointment)
    {
        if (ApptIsNull(appointment))
        {
            return;
        }

        var appointmentNew = Appointments.CopyStructure(appointment);
        Appointments.Insert(appointmentNew);
        var dataTable = Appointments.GetPeriodApptsTable(contrApptPanel.DateStart, contrApptPanel.DateStart, appointmentNew.AptNum, false);
        if (dataTable.Rows.Count == 0)
        {
            return; //silently fail
        }

        var dataRow = dataTable.Rows[0];
        SendToPinboardDataRow(dataRow);
    }

    private void CopyToPin_Click()
    {
        if (!Security.IsAuthorized(EnumPermType.AppointmentMove))
        {
            return;
        }

        //cannot allow moving completed procedure because it could cause completed procs to change date.  Security must block this.
        //ContrApptSingle3[thisIndex].DataRoww;
        var appointment = Appointments.GetOneApt(contrApptPanel.SelectedAptNum);
        if (appointment == null)
        {
            MsgBox.Show(this, "Appointment not found.");
            return;
        }

        if (appointment.AptStatus == ApptStatus.Complete)
        {
            MsgBox.Show(this, "Not allowed to move completed appointments.");
            return;
        }

        if (PatRestrictionL.IsRestricted(appointment.PatNum, PatRestrict.ApptSchedule))
        {
            return;
        }

        var dataRow = contrApptPanel.GetDataRowForSelected();
        if (dataRow == null)
        {
            return; //silently fail
        }

        SendToPinboardDataRow(dataRow);
    }

    private bool DoApptBreakRequired(Appointment appointment, Patient patient = null)
    {
        if (PrefC.GetBool(PrefName.BrokenApptRequiredOnMove) && appointment.AptStatus == ApptStatus.Scheduled)
        {
            var frmApptBreakRequiredForce = new FrmApptBreakRequired();
            frmApptBreakRequiredForce.ShowDialog();
            if (!frmApptBreakRequiredForce.IsDialogOK)
            {
                return false;
            }

            if (patient == null)
            {
                patient = Patients.GetPat(appointment.PatNum);
            }

            AppointmentL.BreakApptHelper(appointment, patient, frmApptBreakRequiredForce.ProcedureCodeBrokenSelected);
        }

        return true;
    }

    private ApptView GetApptViewCur(long apptViewNumOverride = -1)
    {
        var apptView = contrApptPanel.ApptViewCur;
        if (apptView != null)
        {
            return apptView;
        }

        if (apptViewNumOverride < 0)
        {
            apptViewNumOverride = GetApptViewNumForUser(); //Only run this query if we have to.
        }

        if (apptViewNumOverride == ApptViews.ApptViewNumNone //and we specified it's the None view
            && comboView.Items.GetAll<ApptView>().Any(x => x.ApptViewNum == ApptViews.ApptViewNumNone)) //and the None view exists
        {
            apptView = comboView.Items.GetAll<ApptView>().FirstOrDefault(x => x.ApptViewNum == ApptViews.ApptViewNumNone);
        }
        else
        {
            //Or a real ApptView was specified.
            apptView = ApptViews.GetApptView(apptViewNumOverride);
        }

        return apptView;
    }

    ///<summary>Returns an ApptView for the currently logged in user and clinic combination. Can return null.  Will return the first available appointment view if this is the first time that this computer has connected to this database.</summary>
    private ApptView GetApptViewForUser()
    {
        //load the recently used apptview from the db, either the userodapptview table if an entry exists or the computerpref table if an entry for this computer exists
        ApptView apptView = null;
        var userodApptView = UserodApptViews.GetOneForUserAndClinic(Security.CurUser.UserNum, Clinics.ClinicNum);
        if (userodApptView != null)
        {
            //if there is an entry in the userodapptview table for this user
            if (_hasInitializedOnStartup)
            {
                //if ContrAppt has already been initialized
                apptView = ApptViews.GetApptView(userodApptView.ApptViewNum); //then load the view for the user in the userodapptview table
            }
            else if (Security.CurUser.ClinicIsRestricted)
            {
                //current user is restricted
                if (Clinics.ClinicNum != ComputerPrefs.LocalComputer.ClinicNum)
                {
                    //and FormOpenDental.ClinicNum (set to the current user's clinic) is not the computerpref clinic
                    apptView = ApptViews.GetApptView(userodApptView.ApptViewNum); //then load the view for the user in the userodapptview table
                }
            }
        }

        if (apptView == null //if no entry in the userodapptview table
            && Clinics.ClinicNum == ComputerPrefs.LocalComputer.ClinicNum) //and if the program level ClinicNum is the stored recent ClinicNum for this computer 
        {
            apptView = ApptViews.GetApptView(ComputerPrefs.LocalComputer.ApptViewNum); //use the computerpref for this computer and user
        }

        //Larger offices do not want to take the time to load all the data required to display the "none" view.
        //Therefore, for a NEW computer that is connecting to the database for the first time, load up the first available view that is not the none view.
        if (apptView == null)
        {
            //if no entry in the ComputerPref table (or "none" view in ComputerPref table)
            if (comboView.Items.GetAll<ApptView>().Any(x => x.ApptViewNum != ApptViews.ApptViewNumNone))
            {
                //An appointment view other than "none" to select
                if (!_hasInitializedOnStartup)
                {
                    //and ContrAppt has NOT been initialized yet.
                    apptView = comboView.Items.GetAll<ApptView>().FirstOrDefault(x => x.ApptViewNum != ApptViews.ApptViewNumNone);
                }
                else if (PrefC.GetBool(PrefName.EnterpriseNoneApptViewDefaultDisabled) && comboView.SelectedIndex == -1)
                {
                    //or enterprise preference is on to never default to 'None' and no selection has been made thus preserving appointment view behavior and
                    //avoiding unnecessary refreshing of module that can introduce a bug where appointments would vanish if they were from hidden providers or
                    //from views that were visible through 'None' view directly. 
                    //comboView.SelectedIndex will be -1 when user selects a new clinic and our AptViews are refreshed.
                    //Get the first view in comboView that is not the "None" view.
                    apptView = comboView.Items.GetAll<ApptView>().FirstOrDefault(x => x.ApptViewNum != ApptViews.ApptViewNumNone);
                }
            }
        }

        //If apptView==null at this point, the user has explicitly selected the "none" view and we must honor this.
        //apptViewCur will be null at this point if the "None" view is the view the user last had selected, because we do not store an actual view for
        //the "None" view in the database.  However, we do keep a "None" view in memory in comboView's options, and we should return it here.
        if (apptView == null)
        {
            //Get the "None" view.  apptViewCur will be 
            apptView = comboView.Items.GetAll<ApptView>().FirstOrDefault(x => x.ApptViewNum == ApptViews.ApptViewNumNone);
        }

        return apptView;
    }

    private long GetApptViewNumForUser()
    {
        if (contrApptPanel.ApptViewCur != null)
        {
            return contrApptPanel.ApptViewCur.ApptViewNum;
        }

        //GetApptViewForUser needs a CurUser to the specific appointment views for the clinic/user combination
        if (Security.CurUser == null)
        {
            //No valid user so the appointment view will be set to whatever the computerpref table has for the current computer
            return ComputerPrefs.LocalComputer.ApptViewNum;
        }

        var apptView = GetApptViewForUser();
        if (apptView == null)
        {
            return ApptViews.ApptViewNumNone;
        }

        return apptView.ApptViewNum;
    }

    private void HandlePinClicked(ODEventArgs e)
    {
        //What to do if a user double clicks an appointment in DashApptGrid, then clicks 'Send to Pinboard'.
        if (e.EventType != ODEventType.SendToPinboard)
        {
            return;
        }

        if (e == null || e.Tag == null || e.Tag.GetType() != typeof(PinBoardArgs))
        {
            return;
        }

        var apptOther = ((PinBoardArgs) e.Tag).ApptOther_;
        var listApptOthers = ((PinBoardArgs) e.Tag).ListApptOthers;
        long[] apptOtherNumArray = [apptOther.AptNum];
        this.InvokeIfRequired(() =>
        {
            if (!AppointmentL.OKtoSendToPinboard(apptOther, listApptOthers, this))
            {
                return;
            }

            ProcessOtherDlg(OtherResult.CopyToPinBoard, ((PinBoardArgs) e.Tag).Patient_.PatNum, "", apptOtherNumArray);
        });
    }

    private bool HasValidStartTime(Appointment apt)
    {
        if (PrefC.GetBool(PrefName.ApptsAllowOverlap))
        {
            return true;
        }

        bool notUsed;
        //Only valid if no adjust was needed.
        return !Appointments.TryAdjustAppointment(apt, contrApptPanel.ListOpsVisible, false, false, false, false, out notUsed);
    }

    private bool CanScheduleAppointmentTypeOnBlockoutType(Appointment appointment)
    {
        var listScheduleBlockoutsOverlapping = Appointments.GetBlockoutsOverlappingNoSchedule(appointment);
        for (var i = 0; i < listScheduleBlockoutsOverlapping.Count; i++)
        {
            if (appointment.AptDateTime >= listScheduleBlockoutsOverlapping[i].DateTimeStart && appointment.AptDateTime < listScheduleBlockoutsOverlapping[i].DateTimeStop)
            {
                return false;
            }
        }

        return true;
    }

    private void UpdateAppointmentToUnscheduled(Appointment appointment, Appointment appointmentOld)
    {
        appointment.AptStatus = ApptStatus.UnschedList;
        try
        {
            Appointments.Update(appointment, appointmentOld);
        }
        catch (ApplicationException ex)
        {
            ODMessageBox.Show(ex.Message);
        }
    }

    ///<summary>Returns true if the none appointment view is selected, clinics is turned on, and the Headquarters clinic is selected.  Also disables pretty much every control available in the appointment module if it is going to return true, otherwise re-enables them.</summary>
    private bool IsHqNoneView()
    {
        if (true && Clinics.ClinicNum == 0 && ApptViews.IsNoneView(contrApptPanel.ApptViewCur))
        {
            //The "none" appt view is selected
            contrApptPanel.Visible = false;
            labelNoneView.Visible = true;
            butBack.Enabled = false;
            butBackMonth.Enabled = false;
            butBackWeek.Enabled = false;
            butToday.Enabled = false;
            butFwd.Enabled = false;
            butFwdMonth.Enabled = false;
            butFwdWeek.Enabled = false;
            monthCalendarOD.Enabled = false;
            pinBoard.ClearAt(pinBoard.SelectedIndex);
            textLab.Text = "";
            textProduction.Text = "";
            //Future improvement: Change this to only stop printing and lists
            toolBarMain.Visible = false;
            return true;
        }

        //either clinics are not enabled, or a clinic is selected
        contrApptPanel.Visible = true;
        labelNoneView.Visible = false;
        butBack.Enabled = true;
        butBackMonth.Enabled = true;
        butBackWeek.Enabled = true;
        butToday.Enabled = true;
        butFwd.Enabled = true;
        butFwdMonth.Enabled = true;
        butFwdWeek.Enabled = true;
        monthCalendarOD.Enabled = true;
        toolBarMain.Visible = true;
        return false;
    }

    public void LayoutControls()
    {
        var sizeMonthCalendar = monthCalendarOD.GetDefaultSize();
        monthCalendarOD.Bounds = new Rectangle(0, labelDate.Bottom + 3, sizeMonthCalendar.Width, sizeMonthCalendar.Height);
        var w = monthCalendarOD.Width; //a number of things will be set to this width
        panelCalendarLower.Bounds = new Rectangle(0, monthCalendarOD.Bottom + 1, w, 317);
        panelCalendar.Bounds = new Rectangle(Width - w, toolBarMain.Bottom, w, panelCalendarLower.Bottom);
        tabControl.Bounds = new Rectangle(panelCalendar.Left, panelCalendar.Bottom, Width - tabControl.Left, Height - tabControl.Top);
        contrApptPanel.Bounds = new Rectangle(0, toolBarMain.Height, panelCalendar.Left, Height - toolBarMain.Height);
    }

    ///<summary>Used for the UpdateProvs tool to reassign all future appointments for one op to another prov.
    ///Returns true if appointments were updated successfully.</summary>
    public bool MoveAppointments(List<Appointment> listAppts, List<Appointment> listApptsOld, Operatory operatoryCur)
    {
        if (listAppts.Count == 0)
        {
            //no appointments to update, so we're done
            return true;
        }

        var listSchedulesForOp = Schedules.GetSchedsForOp(operatoryCur, listAppts.Select(x => x.AptDateTime).ToList());
        var listOperatoriesForClinic = contrApptPanel.ListOpsVisible.Select(x => x.Copy()).ToList();
        if (((ApptSchedEnforceSpecialty) PrefC.GetInt(PrefName.ApptSchedEnforceSpecialty)).In(
                ApptSchedEnforceSpecialty.Block, ApptSchedEnforceSpecialty.Warn))
        {
            //if specialties are enforced, don't auto-move appt into an op assigned to a different clinic than the curOp's clinic
            listOperatoriesForClinic.RemoveAll(x => x.ClinicNum != operatoryCur.ClinicNum);
        }

        var listProvNumsScheduled = listSchedulesForOp.Select(x => x.ProvNum).ToList();
        listProvNumsScheduled.Add(operatoryCur.ProvDentist); //Check default provs for operatory.
        listProvNumsScheduled.Add(operatoryCur.ProvHygienist);
        //Consider any providers term'd before the appt with the furthest date as invalid.
        var listProvNumsInvalid = Providers.GetInvalidProvsByTermDate(listProvNumsScheduled, listAppts.Max(x => x.AptDateTime));
        if (listProvNumsInvalid.Count > 0)
        {
            var message = Lan.g("FormOperatoryEdit", "Cannot update appointments. This operatory has default or scheduled providers with a Term Date prior to a future appointment's date:") + "\r\n"
                                                                                                                                                                                             + string.Join("\r\n", listProvNumsInvalid.Select(x => Providers.GetLongDesc(x)));
            MsgBox.Show(message);
            return false;
        }

        for (var i = 0; i < listAppts.Count; i++)
        {
            var appointment = listAppts[i];
            var appointmentOld = listApptsOld[i];
            MoveAppointment(appointment, appointmentOld, listSchedulesForOp, true);
        }

        return true;
    }
    
    private void MoveAppointment(Appointment appointment, Appointment appointmentOld, List<Schedule> listSchedeulesForOp = null, bool isOpUpdate = false)
    {
        var timeWasMoved = appointment.AptDateTime != appointmentOld.AptDateTime;
        var isOpChanged = appointment.Op != appointmentOld.Op;
        var operatory = Operatories.GetOperatory(appointment.Op);
        Patient patient = null;
        //List<Schedule> listSchedsForOp=Schedules.GetSchedsForOp(appt.Op,listAppts.Select(x => x.AptDateTime).ToList());
        var listOperatoriesForClinic = contrApptPanel.ListOpsVisible.Select(x => x.Copy()).ToList();
        if (((ApptSchedEnforceSpecialty) PrefC.GetInt(PrefName.ApptSchedEnforceSpecialty)).In(
                ApptSchedEnforceSpecialty.Block, ApptSchedEnforceSpecialty.Warn))
        {
            //if specialties are enforced, don't auto-move appt into an op assigned to a different clinic than the curOp's clinic
            listOperatoriesForClinic.RemoveAll(x => x.ClinicNum != operatory.ClinicNum);
        }

        patient = Patients.GetPat(appointment.PatNum);
        if (!isOpUpdate && appointment.AptDateTime.Date != appointmentOld.AptDateTime.Date)
        {
            //Not moving a list of appointments, and the appointment is moving across days. Check if we need to force an appt break.
            if (PrefC.GetBool(PrefName.BrokenApptRequiredOnMove) && appointment.AptStatus == ApptStatus.Scheduled)
            {
                var frmApptBreakRequiredForce = new FrmApptBreakRequired();
                frmApptBreakRequiredForce.ShowDialog();
                if (!frmApptBreakRequiredForce.IsDialogOK)
                {
                    return;
                }

                AppointmentL.BreakApptHelper(appointment, patient, frmApptBreakRequiredForce.ProcedureCodeBrokenSelected);
                //The appointment status in the database has been updated, but the in memory object MUST have its status updated for
                //logic that will run later on down the line.
                appointment.AptStatus = ApptStatus.Broken;
            }
        }

        var provChanged = false;
        var hygChanged = false;
        long assignedDent = 0;
        long assignedHyg = 0;
        if (isOpUpdate)
        {
            assignedDent = Schedules.GetAssignedProvNumForSpot(listSchedeulesForOp, operatory, false, appointment.AptDateTime);
            assignedHyg = Schedules.GetAssignedProvNumForSpot(listSchedeulesForOp, operatory, true, appointment.AptDateTime);
        }
        else
        {
            assignedDent = Schedules.GetAssignedProvNumForSpot(contrApptPanel.ListSchedules, operatory, false, appointment.AptDateTime);
            assignedHyg = Schedules.GetAssignedProvNumForSpot(contrApptPanel.ListSchedules, operatory, true, appointment.AptDateTime);
        }

        List<Procedure> listProceduresForSingleApt = null;
        if (appointment.AptStatus != ApptStatus.PtNote && appointment.AptStatus != ApptStatus.PtNoteCompleted)
        {
            if (timeWasMoved)
            {
                #region Update Appt's DateTimeAskedToArrive

                if (patient.AskToArriveEarly > 0)
                {
                    appointment.DateTimeAskedToArrive = appointment.AptDateTime.AddMinutes(-patient.AskToArriveEarly);
                    ODMessageBox.Show(Lan.g(this, "Ask patient to arrive") + " " + patient.AskToArriveEarly
                                      + " " + Lan.g(this, "minutes early at") + " " + appointment.DateTimeAskedToArrive.ToShortTimeString() + ".");
                }
                else
                {
                    if (appointment.DateTimeAskedToArrive.Year > 1880 && (appointmentOld.AptDateTime - appointmentOld.DateTimeAskedToArrive).TotalMinutes > 0)
                    {
                        appointment.DateTimeAskedToArrive = appointment.AptDateTime - (appointmentOld.AptDateTime - appointmentOld.DateTimeAskedToArrive);
                        if (ODMessageBox.Show(Lan.g(this, "Ask patient to arrive") + " " + (appointmentOld.AptDateTime - appointmentOld.DateTimeAskedToArrive).TotalMinutes
                                              + " " + Lan.g(this, "minutes early at") + " " + appointment.DateTimeAskedToArrive.ToShortTimeString() + "?", "", MessageBoxButtons.YesNo) == DialogResult.No)
                        {
                            appointment.DateTimeAskedToArrive = appointmentOld.DateTimeAskedToArrive;
                        }
                    }
                    else
                    {
                        appointment.DateTimeAskedToArrive = DateTime.MinValue;
                    }
                }

                #endregion Update Appt's DateTimeAskedToArrive
            }

            #region Update Appt's ProvNum, ProvHyg, IsHygiene, Pattern

            //if no dentist/hygienist is assigned to spot, then keep the original dentist/hygienist without prompt.  All appts must have prov.
            if ((assignedDent != 0 && assignedDent != appointment.ProvNum) || (assignedHyg != 0 && assignedHyg != appointment.ProvHyg))
            {
                var frmApptProvPrompt = new FrmApptProvPrompt();
                var enumApptProvPrompt = PrefC.GetEnum<EnumApptProvPrompt>(PrefName.ApptModuleProviderPrompt);
                var screen = Screen.FromControl(this);
                frmApptProvPrompt.PointScreen = screen.Bounds.Location;
                frmApptProvPrompt.EnumApptProvPrompt_ = enumApptProvPrompt;
                if (!isOpUpdate && (enumApptProvPrompt == EnumApptProvPrompt.NoPromptChange || enumApptProvPrompt == EnumApptProvPrompt.NoPromptNoChange))
                {
                    //We're not updating op through Update All an pref is set to don't prompt. Automate the provider change selection.
                    frmApptProvPrompt.AutomateSelection();
                }

                if (!isOpUpdate && (enumApptProvPrompt == EnumApptProvPrompt.PromptDefaultYes || enumApptProvPrompt == EnumApptProvPrompt.PromptDefaultNo))
                {
                    //Only show the frm if we're not updating op through Update All and if preference is set to prompt
                    frmApptProvPrompt.ShowDialog();
                }

                if (isOpUpdate || frmApptProvPrompt.IsDialogOK)
                {
                    //Short circuit logic.  If we're updating op through right click, never ask.
                    if (assignedDent != 0)
                    {
                        //the dentist will only be changed if the spot has a dentist.
                        appointment.ProvNum = assignedDent;
                        provChanged = true;
                    }

                    if (assignedHyg != 0 || PrefC.GetBool(PrefName.ApptSecondaryProviderConsiderOpOnly))
                    {
                        //the hygienist will only be changed if the spot has a hygienist.
                        appointment.ProvHyg = assignedHyg;
                        hygChanged = true;
                    }

                    appointment.IsHygiene = IsOperatoryHygiene(appointment.IsHygiene, operatory, assignedDent, assignedHyg);
                    listProceduresForSingleApt = Procedures.GetProcsForSingle(appointment.AptNum, false);
                    var codeNums = new List<long>();
                    for (var p = 0; p < listProceduresForSingleApt.Count; p++)
                    {
                        codeNums.Add(listProceduresForSingleApt[p].CodeNum);
                    }

                    if (!isOpUpdate)
                    {
                        var doMake5Minute = listProceduresForSingleApt.Count > 0; //Appointments without procs are already returned in 5 minute increments.
                        var calcPattern = Appointments.CalculatePattern(appointment.ProvNum, appointment.ProvHyg, codeNums, doMake5Minute);
                        if (appointment.Pattern != calcPattern)
                        {
                            //Updating op provs will not change apt lengths.
                            if (Security.IsAuthorized(EnumPermType.AppointmentResize, suppressMessage: true) && !appointment.TimeLocked)
                            {
                                //User is authorized, and appt time not locked. Do not give popup for users without resizing permissions or for Timelocked appointments.
                                if (MsgBox.Show(this, MsgBoxButtons.YesNo, "Change length for new provider?"))
                                {
                                    appointment.Pattern = calcPattern;
                                }
                            }
                        }
                    }
                }
            }
            else if (isOpUpdate)
            {
                //It should not be possible to remove the dentist from the appointment.
                //However, a user could have removed the hygienist from the operatory level (optional field) and should propagate to the appointment.
                if (assignedHyg != appointment.ProvHyg)
                {
                    appointment.ProvHyg = assignedHyg;
                    hygChanged = true;
                }

                //Reconsider the IsHygiene flag defaulting to the appointments current value.
                appointment.IsHygiene = IsOperatoryHygiene(appointment.IsHygiene, operatory, assignedDent, assignedHyg);
            }

            #region Provider Term Date Check

            //Prevents appointments with providers that are past their term end date from being scheduled
            var message = Providers.CheckApptProvidersTermDates(appointment);
            if (message != "")
            {
                ODMessageBox.Show(message); //translated in Providers S class method
                return;
            }

            #endregion Provider Term Date Check

            #endregion Update Appt's ProvNum, ProvHyg, IsHygiene, Pattern
        }

        #region Prevent overlap

        //Check for any blockout collisions when overlapping appointments are allowed.
        if (PrefC.GetBool(PrefName.ApptsAllowOverlap))
        {
            if (!isOpUpdate && ShowAppointmentBlockoutMessage(appointment))
            {
                return;
            }
        }
        else
        {
            //Appointments are not allowed to overlap so check for both appointment and blockout collisions.
            if (!isOpUpdate && !Appointments.TryAdjustAppointmentOp(appointment, listOperatoriesForClinic))
            {
                MsgBox.Show(this, "Appointment overlaps existing appointment or blockout.");
                return;
            }
        }

        #endregion Prevent overlap

        #region Detect Frequency Conflicts

        //Detect frequency conflicts with procedures in the appointment
        var discountPlanSub = DiscountPlanSubs.GetSubForPat(patient.PatNum);
        if (discountPlanSub == null)
        {
            if (!isOpUpdate && PrefC.GetBool(PrefName.InsChecksFrequency))
            {
                listProceduresForSingleApt = Procedures.GetProcsForSingle(appointment.AptNum, appointment.AptStatus == ApptStatus.Planned);
                var frequencyConflicts = "";
                try
                {
                    frequencyConflicts = Procedures.CheckFrequency(listProceduresForSingleApt, appointment.PatNum, appointment.AptDateTime);
                }
                catch (Exception e)
                {
                    ODMessageBox.Show(Lan.g(this, "There was an error checking frequencies.  Disable the Insurance Frequency Checking feature or try to fix the following error:")
                                      + "\r\n" + e.Message);
                    return;
                }

                if (frequencyConflicts != "" && ODMessageBox.Show(Lan.g(this, "Scheduling this appointment for this date will cause frequency conflicts for the following procedures")
                                                                  + ":\r\n" + frequencyConflicts + "\r\n" + Lan.g(this, "Do you want to continue?"), "", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
            }
        }
        else
        {
            listProceduresForSingleApt = Procedures.GetProcsForSingle(appointment.AptNum, appointment.AptStatus == ApptStatus.Planned);
            var frequencyConflicts = "";
            try
            {
                frequencyConflicts = DiscountPlans.CheckDiscountFrequencyAndValidateDiscountPlanSub(listProceduresForSingleApt, appointment.PatNum, appointment.AptDateTime);
            }
            catch (Exception e)
            {
                ODMessageBox.Show(Lan.g(this, "There was an error checking discount frequencies:")
                                  + "\r\n" + e.Message);
                return;
            }

            if (!string.IsNullOrEmpty(frequencyConflicts) && ODMessageBox.Show(Lan.g(this, "This appointment will cause frequency conflicts for the following procedures")
                                                                               + ":\r\n" + frequencyConflicts + "\r\n" + Lan.g(this, "Do you want to continue?"), "", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }
        }

        #endregion Detect Frequency Conflicts

        #region Patient status

        if (!isOpUpdate)
        {
            var operatory2 = Operatories.GetOperatory(appointment.Op);
            var operatoryOld = Operatories.GetOperatory(appointmentOld.Op);
            if (operatoryOld == null || operatory2.SetProspective != operatoryOld.SetProspective)
            {
                if (operatory2.SetProspective && patient.PatStatus != PatientStatus.Prospective)
                {
                    //Don't need to prompt if patient is already prospective.
                    if (MsgBox.Show(this, MsgBoxButtons.OKCancel, "Patient's status will be set to Prospective."))
                    {
                        var patientOld = patient.Copy();
                        patient.PatStatus = PatientStatus.Prospective;
                        Patients.UpdateRecalls(patient, patientOld, "Appointment Module, Appointment moved to prospective operatory");
                        Patients.Update(patient, patientOld);
                        var logEntry = Lan.g(this, "Patient's status changed from ") + patientOld.PatStatus.GetDescription() + Lan.g(this, " to ")
                                       + patient.PatStatus.GetDescription() + Lan.g(this, " by moving the patient appointment to a prospective operatory.");
                        SecurityLogs.MakeLogEntry(EnumPermType.PatientEdit, patient.PatNum, logEntry);
                    }
                }
                else if (!operatory2.SetProspective && patient.PatStatus == PatientStatus.Prospective)
                {
                    //Do we need to warn about changing FROM prospective? Assume so for now.
                    if (MsgBox.Show(this, MsgBoxButtons.OKCancel, "Patient's status will change from Prospective to Patient."))
                    {
                        var patientOld = patient.Copy();
                        patient.PatStatus = PatientStatus.Patient;
                        Patients.UpdateRecalls(patient, patientOld, "Appointment Module, Appointment moved from prospective operatory");
                        Patients.Update(patient, patientOld);
                        var logEntry = Lan.g(this, "Patient's status changed from ") + patientOld.PatStatus.GetDescription() + Lan.g(this, " to ")
                                       + patient.PatStatus.GetDescription() + Lan.g(this, " by moving the patient appointment from a prospective operatory.");
                        SecurityLogs.MakeLogEntry(EnumPermType.PatientEdit, patient.PatNum, logEntry);
                    }
                }
            }
        }

        #endregion Patient status

        #region Update Appt's AptStatus, ClinicNum, Confirmed

        if (appointment.AptStatus == ApptStatus.Broken)
        {
            if (timeWasMoved || isOpChanged)
            {
                //If pref BrokenApptRequiredOnMove is on, then we want users to be able to move appointments across days but force a break.
                //Appt goes from scheduled to broken back to scheduled. This is relevent because later on we compare if the apptOld.AptStatus has changed
                //to update the DB and without this line we would be comparing Scheduled to Scheduled, missing the update for the broken in the middle.
                appointmentOld.AptStatus = appointmentOld.AptStatus == ApptStatus.Scheduled ? ApptStatus.Broken : appointmentOld.AptStatus;
                appointment.AptStatus = ApptStatus.Scheduled;
            }
        }

        if (operatory.ClinicNum == 0)
        {
            appointment.ClinicNum = patient.ClinicNum;
        }
        else
        {
            appointment.ClinicNum = operatory.ClinicNum;
        }

        if (appointment.AptDateTime != appointmentOld.AptDateTime && appointment.Confirmed != Defs.GetFirstForCategory(DefCat.ApptConfirmed, true).DefNum && appointment.AptDateTime.Date != DateTime.Today)
        {
            string prompt;
            if (PrefC.GetBool(PrefName.ApptConfirmAutoEnabled))
            {
                prompt = "Do you want to resend the eConfirmation?";
            }
            else if (PrefC.GetBool(PrefName.ApptThankYouAutoEnabled))
            {
                prompt = "Do you want to resend the eThankYou?";
            }
            else
            {
                prompt = "Reset Confirmation Status?";
            }

            var isResetConf = MsgBox.Show(this, MsgBoxButtons.YesNo, prompt);
            if (isResetConf)
            {
                appointment.Confirmed = Defs.GetFirstForCategory(DefCat.ApptConfirmed, true).DefNum; //Causes the confirmation status to be reset.
            }
        }

        if (isOpChanged && !appointment.IsHygiene)
        {
            //If a non-hygiene appointment is moved, update the IsHygiene value to that of the new operatory.
            appointment.IsHygiene = operatory.IsHygiene;
        }

        #endregion Update Appt's AptStatus, ClinicNum, Confirmed

        //Should only need this check if changing/updating Op. Assumes we didn't previously schedule the apt somewhere it shouldn't have been.
        if (!AppointmentL.IsSpecialtyMismatchAllowed(patient.PatNum, appointment.ClinicNum))
        {
            return;
        }

        try
        {
            if (isOpUpdate)
            {
                Appointments.MoveValidatedAppointment(appointment, appointmentOld, patient, operatory, listSchedeulesForOp, listOperatoriesForClinic, provChanged, hygChanged, timeWasMoved, isOpChanged, isOpUpdate);
            }
            else
            {
                Appointments.MoveValidatedAppointment(appointment, appointmentOld, patient, operatory, contrApptPanel.ListSchedules, listOperatoriesForClinic, provChanged, hygChanged, timeWasMoved, isOpChanged, isOpUpdate);
            }
        }
        catch (Exception e)
        {
            MsgBox.Show(this, e.Message);
        }
    }
    
    private static bool IsOperatoryHygiene(bool isHygiene, Operatory operatory, long assignedDent, long assignedHyg)
    {
        if (operatory is {IsHygiene: true})
        {
            return true;
        }

        if (assignedDent == 0)
        {
            return assignedHyg != 0 || isHygiene;
        }
        
        if (assignedHyg == 0)
        {
            return false;
        }

        if (operatory is not null && operatory.ProvDentist != 0)
        {
            return false;
        }

        return isHygiene;
    }
    
    private void PrintApptCard()
    {
        PrinterL.TryPrintOrDebugRpPreview(pd2_PrintApptCard,
            "Appointment reminder postcard printed",
            printoutOrientation: PrintoutOrientation.Default,
            printSituation: PrintSituation.Postcard,
            auditPatNum: _patient.PatNum,
            margins: new Margins(0, 0, 0, 0),
            printoutOrigin: PrintoutOrigin.AtMargin
        );
    }

    private void pd2_PrintApptCard(object sender, PrintPageEventArgs ev)
    {
        var g = ev.Graphics;
        long apptClinicNum = 0;
        if (contrApptPanel.SelectedAptNum > 0)
        {
            apptClinicNum = SIn.Long(contrApptPanel.GetDataRowForSelected()["ClinicNum"].ToString());
        }

        var clinic = Clinics.GetClinic(apptClinicNum);
        //Return Address--------------------------------------------------------------------------
        var str = "";
        var phone = "";
        str = clinic.Description + "\r\n";
        g.DrawString(str, new Font(FontFamily.GenericSansSerif, 9, FontStyle.Bold), Brushes.Black, 60, 60);
        str = clinic.AddressLine1 + "\r\n";
        if (clinic.AddressLine2 != "")
        {
            str += clinic.AddressLine2 + "\r\n";
        }

        str += clinic.City + "  " + clinic.State + "  " + clinic.Zip + "\r\n";
        phone = clinic.PhoneNumber;
        if (phone.Length == 10)
        {
            str += TelephoneNumbers.ReFormat(phone);
        }
        else
        {
            //any other phone format
            str += phone;
        }

        g.DrawString(str, new Font(FontFamily.GenericSansSerif, 8), Brushes.Black, 60, 75);
        //Body text-------------------------------------------------------------------------------
        string name;
        str = Lan.g(this, "Appointment Reminders:") + "\r\n\r\n";
        var family = Patients.GetFamily(_patient.PatNum);
        var patient = family.GetPatient(_patient.PatNum);
        for (var i = 0; i < family.ListPats.Length; i++)
        {
            if (!_isPrintCardFamily && family.ListPats[i].PatNum != patient.PatNum)
            {
                continue;
            }

            name = family.ListPats[i].FName;
            if (name.Length > 15)
            {
                //trim name so it won't be too long
                name = name.Substring(0, 15);
            }

            var appointmentArrayOnePat = Appointments.GetForPat(family.ListPats[i].PatNum);
            for (var a = 0; a < appointmentArrayOnePat.Length; a++)
            {
                if (appointmentArrayOnePat[a].AptDateTime.Date <= DateTime.Today)
                {
                    continue; //ignore old appts
                }

                if (appointmentArrayOnePat[a].AptStatus != ApptStatus.Scheduled)
                {
                    continue;
                }

                str += name + ": " + appointmentArrayOnePat[a].AptDateTime.ToShortDateString() + " " + appointmentArrayOnePat[a].AptDateTime.ToShortTimeString() + "\r\n";
            }
        }

        g.DrawString(str, new Font(FontFamily.GenericSansSerif, 9), Brushes.Black, 40, 180);
        //Patient's Address-----------------------------------------------------------------------
        Patient patientGuar;
        if (_isPrintCardFamily)
        {
            patientGuar = family.ListPats[0].Copy();
        }
        else
        {
            patientGuar = patient.Copy();
        }

        str = patientGuar.FName + " " + patientGuar.LName + "\r\n" + patientGuar.Address + "\r\n";
        if (patientGuar.Address2 != "")
        {
            str += patientGuar.Address2 + "\r\n";
        }

        str += patientGuar.City + "  " + patientGuar.State + "  " + patientGuar.Zip;
        g.DrawString(str, new Font(FontFamily.GenericSansSerif, 11), Brushes.Black, 300, 240);
        //CommLog entry---------------------------------------------------------------------------
        var commlog = new Commlog();
        commlog.CommDateTime = DateTime.Now;
        commlog.CommType = Commlogs.GetTypeAuto(CommItemTypeAuto.MISC);
        commlog.Note = Lan.g(this, "Appointment card sent");
        commlog.PatNum = patient.PatNum;
        commlog.UserNum = Security.CurUser.UserNum;
        //there is no dialog here because it is just a simple entry
        Commlogs.Insert(commlog);
        ev.HasMorePages = false;
    }

    private void PrintApptLabel()
    {
        var appointment = Appointments.GetOneApt(contrApptPanel.SelectedAptNum);
        if (ApptIsNull(appointment))
        {
            return;
        }

        LabelSingle.PrintAppointment(contrApptPanel.SelectedAptNum);
    }

    private void ProcessOtherDlg(OtherResult otherResult, long patNum, string jumpToDate, long[] aptNums, List<long> apptViewNums = null)
    {
        if (otherResult == OtherResult.Cancel)
        {
            return;
        }

        List<long> listSelectedAptNums;
        switch (otherResult)
        {
            case OtherResult.CopyToPinBoard:
            case OtherResult.NewToPinBoard:
                listSelectedAptNums = aptNums.ToList();
                if (!DoApptBreakRequired(Appointments.GetOneApt(listSelectedAptNums.First())))
                {
                    return;
                }

                SendToPinBoardAptNums(listSelectedAptNums);
                
                GlobalFormOpenDental.PatientSelected(_patient, true, false);
                RefreshPeriod(pinApptNums: listSelectedAptNums);
                break;
            case OtherResult.PinboardAndSearch:
                listSelectedAptNums = aptNums.ToList();
                SendToPinBoardAptNums(aptNums.ToList());
                if (contrApptPanel.IsWeeklyView)
                {
                    break;
                }

                dateSearch.Text = jumpToDate;
                if (!groupSearch.Visible)
                {
                    //if search not already visible
                    ShowSearch();
                }

                DoSearch(isForMakeRecall: true);
                RefreshPeriod(pinApptNums: listSelectedAptNums);
                break;
            case OtherResult.CreateNew:
                contrApptPanel.SelectedAptNum = aptNums[0];
                RefreshModuleDataPatient(patNum);
                GlobalFormOpenDental.PatientSelected(_patient, true, false);
                var appointment = Appointments.GetOneApt(contrApptPanel.SelectedAptNum);
                if (appointment == null)
                {
                    //apt has been deleted
                    return;
                }

                var appointmentOld = appointment.Copy();
                if (!HasValidStartTime(appointment))
                {
                    MsgBox.Show(this, "Appointment start time would overlap another appointment.  Moving appointment to pinboard.");
                    SendToPinBoardAptNums([appointment.AptNum]);
                    UpdateAppointmentToUnscheduled(appointment, appointmentOld);
                    RefreshPeriod();
                    break;
                }

                //if appts appttype is associated to blockouts and appt would start on an unassociated blockout, send to pinboard.
                if (!CanScheduleAppointmentTypeOnBlockoutType(appointment))
                {
                    MsgBox.Show(this, "Appointment type cannot be scheduled on this blockout.  Moving appointment to pinboard.");
                    SendToPinBoardAptNums([appointment.AptNum]);
                    UpdateAppointmentToUnscheduled(appointment, appointmentOld);
                    RefreshPeriod();
                    break;
                }

                if (TryAdjustAppointmentPattern(appointment, contrApptPanel.ListOpsVisible))
                {
                    MsgBox.Show(this, "Appointment is too long and would overlap another appointment or blockout.  Automatically shortened to fit.");
                    try
                    {
                        Appointments.Update(appointment, appointmentOld); //Appointments S-Class handles Signalods
                    }
                    catch (ApplicationException ex)
                    {
                        ODMessageBox.Show(ex.Message);
                    }
                }

                RefreshPeriod();
                break;
            case OtherResult.GoTo:
                contrApptPanel.SelectedAptNum = aptNums[0];
                contrApptPanel.DateSelected = SIn.Date(jumpToDate);
                if (_patient.PatNum != patNum)
                {
                    //ModuleSelected->RefreshModuleScreenPeriod, Appt won't be selected if PatCur.PatNum!=Appt.PatNum
                    _patient = Patients.GetPat(patNum);
                }

                GlobalFormOpenDental.PatientSelected(_patient, true, false, true);
                var apptViewNum = comboView.GetSelected<ApptView>()?.ApptViewNum ?? 0; //default to 'none' view
                var apptViewNumGoTo = GetApptViewNumGoTo(apptViewNums, apptViewNum);
                if (apptViewNum != apptViewNumGoTo && apptViewNumGoTo > 0)
                {
                    SetView(apptViewNumGoTo, saveToDb: false);
                }

                break;
        }
    }

    private static long GetApptViewNumGoTo(List<long> apptViewNums, long curApptViewNum)
    {
        if (apptViewNums == null)
        {
            return -1;
        }

        if (curApptViewNum == ApptViews.ApptViewNumNone)
        {
            return ApptViews.ApptViewNumNone;
        }

        if (apptViewNums.Count == 0)
        {
            MsgBox.Show(
                "There are no appointment views that contain the selected appointment's operatory. " +
                "You will need to create one in the setup menu.");
            
            return -1;
        }

        if (apptViewNums.Contains(curApptViewNum))
        {
            return curApptViewNum;
        }

        return apptViewNums.FirstOrDefault();
    }

    private void SendTextMessages(List<long> listPatNums)
    {
        if (!Security.IsAuthorized(EnumPermType.TextMessageSend))
        {
            return;
        }

        if (listPatNums.Count == 0)
        {
            MsgBox.Show(this, "No appointments this day to send text messages to.");
            return;
        }

        var clinic = Clinics.GetClinic(Clinics.ClinicNum) ?? Clinics.GetDefaultForTexting() ?? Clinics.GetPracticeAsClinicZero();
        var listPatComms = Patients.GetPatComms(listPatNums, clinic, false);
        var listPatsSkipped = new List<string>();
        for (var i = listPatComms.Count - 1; i >= 0; i--)
        {
            //Remove patients that can't receive texts.
            var patComm = listPatComms[i];
            if (patComm.IsSmsAnOption)
            {
                continue;
            }

            listPatsSkipped.Add(patComm.FName + " " + patComm.LName + ": " + patComm.GetReasonCantText());
            listPatComms.RemoveAt(i);
        }

        if (listPatsSkipped.Count > 0)
        {
            var msg = listPatsSkipped.Count + " of the " + listPatNums.Distinct().Count() + " patients cannot receive text messages:\r\n" + string.Join("\r\n", listPatsSkipped);
            if (listPatsSkipped.Count < 8)
            {
                ODMessageBox.Show(msg);
            }
            else
            {
                using var msgBoxCopyPaste = new MsgBoxCopyPaste(msg);
                msgBoxCopyPaste.ShowDialog();
            }
        }

        if (listPatComms.Count == 0)
        {
            return;
        }

        var formTxtMsgMany = new FormTxtMsgMany(listPatComms, "", Clinics.ClinicNum, SmsMessageSource.DirectSms);
        
        formTxtMsgMany.DoCombineNumbers = true;
        formTxtMsgMany.Show();
    }
    
    private void SendToPinBoardAptNums(List<long> aptNums)
    {
        if (IsHqNoneView())
        {
            MsgBox.Show(this, "Appointments can't be sent to the pinboard when an appointment view or clinic hasn't been selected.");
            return;
        }

        if (aptNums.Count == 0)
        {
            return;
        }

        long patNum = 0;
        for (var i = 0; i < aptNums.Count; i++)
        {
            //sometimes, before this method was called, module was refreshed, and these appts were included.
            DataRow dataRow = null;
            //We need to know the indexOfAppt in case it is a planned appointment and we need to find the appointment in the Rows collection in order to get information for it later
            var indexOfAppt = -1;
            for (var r = 0; r < contrApptPanel.TableAppointments.Rows.Count; r++)
            {
                if (contrApptPanel.TableAppointments.Rows[r]["AptNum"].ToString() != aptNums[i].ToString())
                {
                    continue;
                }

                indexOfAppt = r;
                dataRow = contrApptPanel.TableAppointments.Rows[r];
            }

            if (dataRow == null)
            {
                bool includeVerifyIns = 
                    contrApptPanel.ListApptViewItems != null && 
                    contrApptPanel.ListApptViewItems.Exists(x => 
                        x.ElementDesc == EnumApptViewElement.VerifyIns_V.GetDescription());

                //but sometimes, we need to go get the row manually
                var dataTable = Appointments.GetPeriodApptsTable(contrApptPanel.DateStart, contrApptPanel.DateEnd, aptNums[i], false, includeVerifyIns: includeVerifyIns);
                if (dataTable.Rows.Count == 0)
                {
                    continue; //fail silently?
                }

                dataRow = dataTable.Rows[0];
            }

            if (dataRow["AptStatus"].ToString() == ((int) ApptStatus.Planned).ToString())
            {
                //Planned appointment is on the pinboard, so we have to retrieve the table again for slightly different info.
                //This won't happen very frequently, and it's faster to do it again than to intelligently figure out how to do it once.
                var table = Appointments.RefreshOneApt(aptNums[i], true).Tables["Appointments"];
                if (table.Rows.Count == 0)
                {
                    MsgBox.Show(this, "Planned appointment no longer exists.");
                    continue;
                }

                dataRow = table.Rows[0];
                //We must update the information for this current appointment with what we just gathered in RefreshOneApt (currently it holds information from GetPeriodApptsTable which is incorrect)
                if (indexOfAppt >= 0)
                {
                    contrApptPanel.TableAppointments.Rows[indexOfAppt].ItemArray = dataRow.ItemArray;
                }
            }

            var pattern = SIn.String(dataRow["Pattern"].ToString());
            var patternShowing = contrApptPanel.GetPatternShowing(pattern);
            var sizeAppt = contrApptPanel.SetSize(pattern);
            var bitmap = new Bitmap(pinBoard.Width - 2, (int) sizeAppt.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                contrApptPanel.GetBitmapForPinboard(g, dataRow, patternShowing, bitmap.Width, bitmap.Height);
            }

            var aptNum = SIn.Long(dataRow["AptNum"].ToString());
            pinBoard.AddAppointment(bitmap, aptNum, dataRow);
            bitmap.Dispose(); //?
            if (i == aptNums.Count - 1)
            {
                //Set the pt to the last appt on the pinboard.
                patNum = SIn.Long(dataRow["PatNum"].ToString());
            }
        }

        if (patNum == 0 && _patient != null)
        {
            patNum = _patient.PatNum;
        }

        RefreshModuleDataPatient(patNum);
        if (_patient != null)
        {
            GlobalFormOpenDental.PatientSelected(_patient, true, false);
        }
    }
    
    private void SendToPinboardDataRow(DataRow dataRow)
    {
        if (IsHqNoneView())
        {
            MsgBox.Show(this, "Appointments can't be sent to the pinboard when an appointment view or clinic hasn't been selected.");
            return;
        }

        var aptNum = SIn.Long(dataRow["AptNum"].ToString());
        if (!DoApptBreakRequired(Appointments.GetOneApt(aptNum)))
        {
            return;
        }

        var pattern = SIn.String(dataRow["Pattern"].ToString());
        var patternShowing = contrApptPanel.GetPatternShowing(pattern);
        var sizeAppt = contrApptPanel.SetSize(pattern);
        var bitmap = new Bitmap(pinBoard.Width - 2, (int) sizeAppt.Height);
        using (var g = Graphics.FromImage(bitmap))
        {
            contrApptPanel.GetBitmapForPinboard(g, dataRow, patternShowing, bitmap.Width, bitmap.Height);
        }

        pinBoard.AddAppointment(bitmap, aptNum, dataRow);
        bitmap.Dispose(); //?
        var patNum = SIn.Long(dataRow["PatNum"].ToString());
        RefreshModuleDataPatient(patNum);
        GlobalFormOpenDental.PatientSelected(_patient, true, false);
    }

    public void SetInitialStartTime()
    {
        if (_hasSetInitialStartTime)
        {
            return;
        }

        var apptView = GetApptViewCur();
        if (!ApptViews.IsNoneView(apptView))
        {
            //None view NOT selected.
            var timeSpanApptScrollStart = apptView.ApptTimeScrollStart;

            #region IsScrollStartDynamic

            if (apptView.IsScrollStartDynamic)
            {
                //Scroll start time at the earliest scheduled operatory or appointment
                //jordan IsScrollStartDynamic seems annoying to me, but it does help prevent an appt from getting hidden above the start time.
                //And, it's just a one-time thing.
                //Get the schedules that have any operatory visible
                var listSchedulesVisible = new List<Schedule>();
                for (var i = 0; i < contrApptPanel.ListSchedules.Count; i++)
                {
                    if (contrApptPanel.ListSchedules[i].Ops.Any(x => contrApptPanel.ListOpsVisible.Exists(y => x == y.OperatoryNum)) //The schedule is linked to a visible operatory
                        || contrApptPanel.ListOpsVisible.Exists(x => x.ProvDentist == contrApptPanel.ListSchedules[i].ProvNum && !x.IsHygiene) //The dentist is in a visible operatory
                        || contrApptPanel.ListOpsVisible.Exists(x => x.ProvHygienist == contrApptPanel.ListSchedules[i].ProvNum && x.IsHygiene)) //The hygienist is in a visible operatory
                    {
                        listSchedulesVisible.Add(contrApptPanel.ListSchedules[i]);
                    }
                }

                var schedProvUnassinged = PrefC.GetLong(PrefName.ScheduleProvUnassigned);
                var opShowsDefaultProv = false;
                for (var i = 0; i < contrApptPanel.ListOpsVisible.Count; i++)
                {
                    if (contrApptPanel.ListOpsVisible[i].ProvDentist != 0 && !contrApptPanel.ListOpsVisible[i].IsHygiene)
                    {
                        continue; //The operatory has a provider assigned to it
                    }

                    if (contrApptPanel.ListOpsVisible[i].ProvHygienist != 0 && contrApptPanel.ListOpsVisible[i].IsHygiene)
                    {
                        continue; //The operatory has a provider assigned to it
                    }

                    if (contrApptPanel.ListSchedules.Any(x => x.Ops.Contains(contrApptPanel.ListOpsVisible[i].OperatoryNum)))
                    {
                        continue; //The operatory has a schedule assigned to it
                    }

                    opShowsDefaultProv = true; //The operatory will have the provider for unassigned operatories
                    break;
                }

                if (opShowsDefaultProv && contrApptPanel.ListSchedules.Exists(x => x.ProvNum == schedProvUnassinged))
                {
                    //The provider for unassigned ops has a schedule
                    //Add that provider's earliest schedule
                    listSchedulesVisible.Add(contrApptPanel.ListSchedules.FindAll(x => x.ProvNum == schedProvUnassinged).OrderBy(x => x.StartTime).FirstOrDefault());
                }

                //Get the appointment times that are in a visible operatory
                var listVisAptTimes = new List<TimeSpan>();
                for (var i = 0; i < contrApptPanel.TableAppointments.Rows.Count; i++)
                {
                    var opNum = SIn.Long(contrApptPanel.TableAppointments.Rows[i]["Op"].ToString());
//todo:
                    if (!contrApptPanel.ListOpsVisible.Exists(x => x.OperatoryNum == opNum) //The appointment is in a visible operatory
                        || !new[] {"1", "2", "4", "5", "7", "8"}.Contains(contrApptPanel.TableAppointments.Rows[i]["AptStatus"].ToString())) //Scheduled,Complete,ASAP,Broken,PtNote,PtNoteComp
                    {
                        continue;
                    }

                    listVisAptTimes.Add(SIn.Date(contrApptPanel.TableAppointments.Rows[i]["AptDateTime"].ToString()).TimeOfDay);
                }

                var timeSpanEarliestApt = new TimeSpan();
                var timeSpanEarliestOp = new TimeSpan();
                if (listVisAptTimes.Count > 0 && listSchedulesVisible.Count > 0)
                {
                    //There is at least one schedule and at least one appointment visible
                    timeSpanEarliestApt = listSchedulesVisible.Min(x => x.StartTime);
                    timeSpanEarliestOp = listVisAptTimes.Min();
                    if (TimeSpan.Compare(timeSpanEarliestOp, timeSpanEarliestApt) == 1)
                    {
                        //earliestOp is later than earliestApt
                        timeSpanApptScrollStart = timeSpanEarliestApt;
                    }
                    else
                    {
                        //earliestApt is later than earliestOp or they are both equal
                        timeSpanApptScrollStart = timeSpanEarliestOp;
                    }
                }
                else if (listSchedulesVisible.Count > 0)
                {
                    //There is at least one visible schedule and no visible appointments
                    timeSpanApptScrollStart = listSchedulesVisible.Min(x => x.StartTime);
                }
                else if (listVisAptTimes.Count > 0)
                {
                    //There is at least one visible appointment and no visible schedules
                    timeSpanApptScrollStart = listVisAptTimes.Min();
                }
                //else apptTimeScrollStart will remain as the start time listed in the appt view		
            }

            #endregion IsScrollStartDynamic

            //Even if we skip isScrollStartDynamic, above, the code below will still handle ApptTimeScrollStart
            contrApptPanel.SetScrollByTime(timeSpanApptScrollStart);
        }
        else
        {
            //else no view selected
            contrApptPanel.SetScrollByTime(TimeSpan.FromHours(8));
        }

        _hasSetInitialStartTime = true;
    }
    
    private void SetView(long apptViewNum, bool saveToDb)
    {
        comboView.SetSelectedKey<ApptView>(apptViewNum, x => x.ApptViewNum, _ => "none"); //First item is None/0 view.
        if (comboView.SelectedIndex < 0)
        {
            comboView.SetSelected(0);
        }

        contrApptPanel.ApptViewCur = comboView.GetSelected<ApptView>();
        if (!_hasInitializedOnStartup)
        {
            return;
        }

        if (_hasInitializedOnStartup && !Visible)
        {
            return;
        }

        if (saveToDb)
        {
            ComputerPrefs.LocalComputer.ApptViewNum = apptViewNum;
            ComputerPrefs.LocalComputer.ClinicNum = Clinics.ClinicNum;
            ComputerPrefs.Update(ComputerPrefs.LocalComputer);
            
            UserodApptViews.InsertOrUpdate(Security.CurUser.UserNum, Clinics.ClinicNum, apptViewNum);
        }

        if (_patient == null)
        {
            ModuleSelected(0, opNums: ApptViewItems.GetOpsForView(apptViewNum), provNums: ApptViewItems.GetProvsForView(apptViewNum));
            return;
        }

        ModuleSelected(_patient.PatNum, opNums: ApptViewItems.GetOpsForView(apptViewNum), provNums: ApptViewItems.GetProvsForView(apptViewNum));
    }

    private void SetWeeklyView(bool isWeeklyView, bool skipModuleSelection = false)
    {
        if (isWeeklyView)
        {
            toggleDayWeek.SetWeek();
        }
        else
        {
            toggleDayWeek.SetDay();
        }

        contrApptPanel.IsWeeklyView = isWeeklyView;
        if (!_hasInitializedOnStartup)
        {
            return;
        }

        if (skipModuleSelection)
        {
            return;
        }

        var apptViewNum = contrApptPanel.ApptViewCur?.ApptViewNum ?? ApptViews.ApptViewNumNone;
        if (isWeeklyView)
        {
            if (_patient == null)
            {
                ModuleSelected(0, opNums: ApptViewItems.GetOpsForView(apptViewNum), provNums: ApptViewItems.GetProvsForView(apptViewNum));
                return;
            }

            ModuleSelected(_patient.PatNum, opNums: ApptViewItems.GetOpsForView(apptViewNum), provNums: ApptViewItems.GetProvsForView(apptViewNum));
            return;
        }

        RefreshPeriod(opNums: ApptViewItems.GetOpsForView(apptViewNum), provNums: ApptViewItems.GetProvsForView(apptViewNum), isRefreshSchedules: true);
    }
    
    public static bool TryAdjustAppointmentPattern(Appointment appointment, List<Operatory> listOpsVisible)
    {
        Appointments.TryAdjustAppointment(appointment, listOpsVisible, false, true, true, true, out var isPatternChanged);
        
        return isPatternChanged;
    }

    private bool ShowAppointmentBlockoutMessage(Appointment appointment)
    {
        var scheduleBlockout = Appointments.GetBlockoutsOverlappingNoSchedule(appointment).FirstOrDefault();
        if (scheduleBlockout != null)
        {
            //Appointment was moved on top of at least one conflicting blockout.
            var itemValue = Defs.GetValue(DefCat.BlockoutTypes, scheduleBlockout.BlockoutType);
            if (itemValue.Contains(BlockoutType.NoSchedule.GetDescription()))
            {
                MsgBox.Show(this, "Appointment cannot be scheduled on a blockout marked as 'Block appointment scheduling'.");
            }
            else
            {
                MsgBox.Show(this, "Appointment type cannot be scheduled on this blockout.");
            }

            return true;
        }

        return false;
    }
}