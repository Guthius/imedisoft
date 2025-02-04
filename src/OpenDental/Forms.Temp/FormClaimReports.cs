using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormClaimReports : FormODBase
{
    public bool IsAutomaticMode { get; set; }

    private List<Clearinghouse> _clearinghousesHq;

    public FormClaimReports()
    {
        InitializeComponent();
    }

    private void FormClaimReports_Load(object sender, EventArgs e)
    {
        _clearinghousesHq = Clearinghouses.GetDeepCopy();

        for (var i = 0; i < _clearinghousesHq.Count; i++)
        {
            comboClearhouse.Items.Add(_clearinghousesHq[i].Description);
            if (PrefC.GetLong(PrefName.ClearinghouseDefaultDent) == _clearinghousesHq[i].ClearinghouseNum)
            {
                comboClearhouse.SelectedIndex = i;
            }
        }

        if (comboClearhouse.Items.Count > 0 && comboClearhouse.SelectedIndex == -1)
        {
            comboClearhouse.SelectedIndex = 0;
        }
    }

    private void FormClaimReports_Shown(object sender, EventArgs e)
    {
        if (!IsAutomaticMode)
        {
            return;
        }

        labelRetrieving.Visible = true;

        Cursor = Cursors.WaitCursor;

        var clearinghouseHq = _clearinghousesHq[comboClearhouse.SelectedIndex];
        var clearinghouseClin = Clearinghouses.OverrideFields(clearinghouseHq, Clinics.ClinicNum);

        var errorMessage = Clearinghouses.RetrieveAndImport(clearinghouseClin, IsAutomaticMode);
        if (!string.IsNullOrEmpty(errorMessage))
        {
            ShowError(errorMessage);
        }

        Cursor = Cursors.Default;

        Close();
    }

    private void ButtonRetrieve_Click(object sender, EventArgs e)
    {
        if (comboClearhouse.SelectedIndex == -1)
        {
            ShowError("Please select a clearinghouse first.");
            return;
        }

        if (!ConfirmOk("Connect to clearinghouse to retrieve reports?"))
        {
            return;
        }

        var clearhouseHq = _clearinghousesHq[comboClearhouse.SelectedIndex];
        var clearinghouseClin = Clearinghouses.OverrideFields(clearhouseHq, Clinics.ClinicNum);

        if (!Directory.Exists(clearinghouseClin.ResponsePath))
        {
            ShowError("Clearinghouse report path is invalid. Go to Setup, Family/Insurance, Clearinghouses, and double-click the desired clearinghouse to update the path.");
            return;
        }

        var progress = new ODProgressExtended(this, new ProgressBarHelper("Clearinghouse Progress", progressBarEventType: ProgBarEventType.Header), lanThis: Name);

        if (clearhouseHq.ISA08 == "113504607")
        {
            if (PrefC.GetLong(PrefName.ClearinghouseDefaultDent) != clearhouseHq.ClearinghouseNum)
            {
                var errorMessage = Clearinghouses.RetrieveAndImport(clearinghouseClin, false, progress);

                progress.UpdateProgressDetailed("", tagString: "reports", percentVal: "100%", barVal: 100);
                progress.UpdateProgress(errorMessage == "" ? "Retrieval and import successful" : errorMessage);
                progress.UpdateProgress("Done");
            }
            else
            {
                progress.UpdateProgress("No need to retrieve. Available reports are automatically downloaded every three minutes.");
            }

            progress.OnProgressDone();
            return;
        }

        if (clearhouseHq.CommBridge is EclaimsCommBridge.None or EclaimsCommBridge.Renaissance or EclaimsCommBridge.RECS)
        {
            progress.UpdateProgress("No built-in functionality for retrieving reports from this clearinghouse.");
            progress.OnProgressDone();
            return;
        }

        labelRetrieving.Visible = true;

        var errorMesssage = Clearinghouses.RetrieveAndImport(clearinghouseClin, false, progress);

        progress.UpdateProgressDetailed("", tagString: "reports", percentVal: "100%", barVal: 100);

        if (clearhouseHq.CommBridge == EclaimsCommBridge.ClaimConnect && errorMesssage == "" && Directory.Exists(clearinghouseClin.ResponsePath))
        {
        }
        else if (errorMesssage == "")
        {
            progress.UpdateProgress("Retrieve and import successful.");
        }
        else
        {
            progress.UpdateProgress("Error Log:\r\n" + errorMesssage);
        }

        labelRetrieving.Visible = false;

        progress.OnProgressDone();

        if (progress.IsCanceled)
        {
            progress.Close();
        }
    }
}