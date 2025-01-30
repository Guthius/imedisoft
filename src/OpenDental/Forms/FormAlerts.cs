using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDental.UI;

namespace OpenDental.Forms;

public partial class FormAlerts : FormODBase
{
    public List<AlertItem> ListAlertItems;
    public List<AlertRead> ListAlertReads;

    private int _gridNum;
    
    public AlertItem SelectedAlertItem { get; set; }
    public ActionType SelectedAlertType { get; set; }
    
    public FormAlerts(List<AlertItem> alertItems, List<AlertRead> alertReads)
    {
        ListAlertItems = alertItems;
        ListAlertReads = alertReads;
        
        InitializeComponent();
    }

    protected void OnClick()
    {
        ClickAlert?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler ClickAlert;

    private void ButtonOpenForm_Click(object sender, EventArgs e)
    {
        SelectedAlertType = ActionType.OpenForm;

        Close();
    }

    private void FormAlerts_Load(object sender, EventArgs e)
    {
        labelOpenForm.Text = "";

        FillGrid();
    }

    private void GridAlerts_CellClick(object sender, ODGridClickEventArgs e)
    {
        butDelete.Enabled = false;
        butMarkAsRead.Enabled = false;
        butOpenForm.Enabled = false;
        butViewDetails.Enabled = false;
        
        labelOpenForm.Text = "";

        if (gridMain.SelectedIndices.Length > 1)
        {
            return;
        }

        SelectedAlertItem = ListAlertItems[e.Row];

        _gridNum = e.Row;

        var actionTypes = Enum.GetValues(typeof(ActionType)).Cast<ActionType>().ToList();

        actionTypes.Sort(AlertItem.CompareActionType);

        foreach (var actionType in actionTypes)
        {
            if (!SelectedAlertItem.Actions.HasFlag(actionType))
            {
                continue;
            }

            switch (actionType)
            {
                case ActionType.Delete:
                    butDelete.Enabled = true;
                    break;

                case ActionType.MarkAsRead:
                    butMarkAsRead.Enabled = true;
                    break;

                case ActionType.ShowItemValue:
                    butViewDetails.Enabled = true;
                    break;
            }

            if (actionType != ActionType.OpenForm)
            {
                continue;
            }

            butOpenForm.Enabled = true;
            labelOpenForm.Text = "Window to open: " + SelectedAlertItem.FormToOpen.GetDescription();
        }
    }

    private void ButtonMarkAsRead_Click(object sender, EventArgs e)
    {
        SelectedAlertType = ActionType.MarkAsRead;

        OnClick();

        gridMain.SetSelected(_gridNum);
    }

    private void ButtonViewDetails_Click(object sender, EventArgs e)
    {
        SelectedAlertType = ActionType.ShowItemValue;

        OnClick();

        gridMain.SetSelected(_gridNum);
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (!ConfirmOk("This will delete the alert for all users. Are you sure you want to delete it?"))
        {
            return;
        }

        SelectedAlertType = ActionType.Delete;

        OnClick();
    }

    private void ButtonAcknowledge_Click(object sender, EventArgs e)
    {
        var selectedAlertItems = gridMain.SelectedIndices.Select(x => ListAlertItems[x]).ToList();

        var numberOfAlertsToDelete = selectedAlertItems.Count(x => x.Actions.HasFlag(ActionType.Delete));
        if (numberOfAlertsToDelete > 0)
        {
            var message = "This will delete the alert for all users. Are you sure you want to delete it?";
            if (numberOfAlertsToDelete > 1)
            {
                message = $"This will delete {numberOfAlertsToDelete} alerts for all users. Are you sure you want to delete them?";
            }

            if (!ConfirmOk(message))
            {
                return;
            }
        }

        foreach (var alertItem in selectedAlertItems)
        {
            SelectedAlertItem = alertItem;
            SelectedAlertType = SelectedAlertItem.Actions.HasFlag(ActionType.Delete) ? ActionType.Delete : ActionType.MarkAsRead;

            OnClick();
        }
    }

    public void FillGrid()
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Alert #", 45, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn("Read", 45, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn("Description", 0, HorizontalAlignment.Left) {IsWidthDynamic = true});

        gridMain.ListGridRows.Clear();

        for (var i = 0; i < ListAlertItems.Count; i++)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add((i + 1).ToString());
            gridRow.Cells.Add(ListAlertReads.Select(x => x.AlertItemNum).Contains(ListAlertItems[i].AlertItemNum) ? "X" : "");
            gridRow.Cells.Add(AlertMenuItemHelper(ListAlertItems[i]) + ListAlertItems[i].Description);

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private static string AlertMenuItemHelper(AlertItem alertItem)
    {
        var value = "";

        switch (alertItem.Type)
        {
            case AlertType.Generic:
            case AlertType.ClinicsChangedInternal:
                break;

            case AlertType.OnlinePaymentsPending:
                value += "Pending Online Payments: ";
                break;

            case AlertType.RadiologyProcedures:
                value += "Radiology Orders: ";
                break;

            case AlertType.CallbackRequested:
                value += "Patient would like a callback regarding this appointment: ";
                break;

            case AlertType.WebSchedNewPat:
                value += "eServices: ";
                break;

            case AlertType.WebSchedNewPatApptCreated:
                value += "New Web Sched New Patient Appointment: ";
                break;

            case AlertType.MaxConnectionsMonitor:
                value += "MySQL Max Connections: ";
                break;

            case AlertType.WebSchedASAPApptCreated:
                value += "New Web Sched ASAP Appointment: ";
                break;

            case AlertType.WebSchedRecallApptCreated:
                value += "New Web Sched Recall Appointment: ";
                break;

            case AlertType.WebMailReceived:
                value += "Unread Web Mails: ";
                break;

            case AlertType.WebFormsReady:
                value += "Web Forms Ready to Retrieve: ";
                break;

            case AlertType.SignatureCleared:
                value += "Signature Cleared: ";
                break;
            case AlertType.EconnectorEmailTooManySendFails:
            case AlertType.NumberBarredFromTexting:
            case AlertType.MultipleEConnectors:
            case AlertType.EConnectorDown:
            case AlertType.EConnectorError:
            case AlertType.DoseSpotProviderRegistered:
            case AlertType.DoseSpotClinicRegistered:
            case AlertType.ClinicsChanged:
            case AlertType.CloudAlertWithinLimit:
            case AlertType.WebSchedRecallsNotSending:
            case AlertType.EConnectorRedistributableMissing:
            default:
                value += alertItem.Type.GetDescription() + ": ";
                break;
        }

        return value;
    }
}