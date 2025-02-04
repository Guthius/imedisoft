using System;
using System.Data;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenDental.UI;
using OpenDentBusiness;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Features.Providers.Dtos;

namespace OpenDental;

public partial class FormProviderSetup : FormODBase
{
    private bool _changed;
    private DataTable _tableProvs;
    private long _provNumMoveTo = -1;
    private List<UserGroup> _userGroups;
    private ToolTip _toolTipPriProvEdit = new ToolTip {ShowAlways = true};
    private List<ProviderDto> _providers;

    public FormProviderSetup()
    {
        InitializeComponent();

        AutoSize = true;

        Width = 960;

        WaitFilterMs = 200;
    }

    private void FormProviderSetup_Load(object sender, EventArgs e)
    {
        SetFilterControlsAndAction(FillGrid, textSearch);

        if (!Security.IsAuthorized(EnumPermType.ProviderAdd, suppressMessage: true))
        {
            butAdd.Enabled = false;
        }

        _providers = Providers.GetDeepCopy();
        
        if (Security.IsAuthorized(EnumPermType.SecurityAdmin, true))
        {
            _userGroups = UserGroups.GetList();

            foreach (var userGroup in _userGroups)
            {
                comboUserGroup.Items.Add(userGroup.Description, userGroup);
            }

            if (comboUserGroup.Items.Count > 0)
            {
                comboUserGroup.SetSelected(0, true);
            }
        }
        else
        {
            groupCreateUsers.Enabled = false;
            groupMovePats.Enabled = false;
        }

        checkShowHidden.Checked = true;
        if (Security.IsAuthorized(EnumPermType.PatPriProvEdit, DateTime.MinValue, true, true))
        {
            return;
        }

        var strToolTip = "Not authorized for " + GroupPermissions.GetDesc(EnumPermType.PatPriProvEdit);

        _toolTipPriProvEdit.SetToolTip(butReassign, strToolTip);
        _toolTipPriProvEdit.SetToolTip(butMovePri, strToolTip);
    }

    private void FormProviderSetup_Shown(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid()
    {
        var listProvNumsSelected = gridMain.SelectedIndices.Select(x => ((Provider) gridMain.ListGridRows[x].Tag).ProvNum).ToList();
        var scroll = gridMain.ScrollValue;
        var indexSortCol = gridMain.GetSortedByColumnIdx();
        var isSortAsc = gridMain.IsSortedAscending();

        gridMain.BeginUpdate();
        gridMain.Columns.Clear();

        gridMain.Columns.Add(new GridColumn("Abbrev", 90));
        gridMain.Columns.Add(new GridColumn("Last Name", 90));
        gridMain.Columns.Add(new GridColumn("First Name", 90));
        gridMain.Columns.Add(new GridColumn("User Name", 90));
        gridMain.Columns.Add(new GridColumn("Hidden", 50, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn("HideOnReports", 100, HorizontalAlignment.Center));

        gridMain.ListGridRows.Clear();
        var searchWords = textSearch.Text.ToLower().Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
        for (var i = 0; i < _tableProvs.Rows.Count; i++)
        {
            if (!checkShowHidden.Checked && _tableProvs.Rows[i]["IsHidden"].ToString() == "1")
            {
                continue;
            }

            var listColsToSearch = new List<string> {"Abbr", "LName", "FName"};

            if (searchWords.Count > 0 && !searchWords.All(x => listColsToSearch.Any(y => _tableProvs.Rows[i][y].ToString().ToLower().Contains(x))))
            {
                continue;
            }

            var gridRow = new GridRow();
            if (_tableProvs.Rows[i]["ProvStatus"].ToString() == ((int) ProviderStatus.Deleted).ToString())
            {
                if (!checkShowDeleted.Checked)
                {
                    continue;
                }

                gridRow.ColorText = Color.Red;
            }

            gridRow.Cells.Add(_tableProvs.Rows[i]["Abbr"].ToString());
            gridRow.Cells.Add(_tableProvs.Rows[i]["LName"].ToString());
            gridRow.Cells.Add(_tableProvs.Rows[i]["FName"].ToString());
            gridRow.Cells.Add(_tableProvs.Rows[i]["UserName"].ToString());
            gridRow.Cells.Add(_tableProvs.Rows[i]["IsHidden"].ToString() == "1" ? "X" : "");
            gridRow.Cells.Add(_tableProvs.Rows[i]["IsHiddenReport"].ToString() == "1" ? "X" : "");
            
            var provNumCur = SIn.Long(_tableProvs.Rows[i]["ProvNum"].ToString());

            gridRow.Tag = _providers.Find(x => x.Id == provNumCur);

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
        if (indexSortCol > -1 && indexSortCol < gridMain.Columns.Count)
        {
            gridMain.SortForced(indexSortCol, isSortAsc);
        }

        for (var i = 0; i < gridMain.ListGridRows.Count; i++)
        {
            var provNumCur = ((Provider) gridMain.ListGridRows[i].Tag).ProvNum;
            if (listProvNumsSelected.Contains(provNumCur))
            {
                gridMain.SetSelected(i);
            }
        }

        gridMain.ScrollValue = scroll;
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.ProviderAdd))
        {
            return;
        }

        var providerDto = new ProviderDto();
        
        using var formProvEdit = new FormProvEdit(providerDto);
        
        formProvEdit.IsNew = true;

        if (formProvEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        
        SecurityLogs.MakeLogEntry(EnumPermType.ProviderAdd, 0, "Provider: " + providerDto.Abbr + " added.");
        
        _changed = true;
        
        Cache.Refresh(InvalidType.Providers);
        
        _providers = Providers.GetDeepCopy();
        
        FillGrid();
        
        gridMain.ScrollToEnd();
        
        for (var i = 0; i < gridMain.ListGridRows.Count; i++)
        {
            if (((Provider) gridMain.ListGridRows[i].Tag).ProvNum == providerDto.Id)
            {
                gridMain.SetSelected(i);
                break;
            }
        }
    }

    private void checkShowHidden_Click(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.ProviderEdit))
        {
            return;
        }

        var providerDto = (ProviderDto) gridMain.ListGridRows[e.Row].Tag;
        
        using var formProvEdit = new FormProvEdit(providerDto);
        
        if (formProvEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        SecurityLogs.MakeLogEntry(EnumPermType.ProviderEdit, 0, "Provider: " + providerDto.Abbr + " edited.", providerDto.Id, SecurityLogs.LogSource, DateTime.MinValue);

        _changed = true;

        Cache.Refresh(InvalidType.Providers);

        _providers = Providers.GetDeepCopy();

        FillGrid();
    }

    private void butProvPick_Click(object sender, EventArgs e)
    {
        var frmProviderPick = new FrmProviderPick
        {
            IsNoneAvailable = true
        };

        frmProviderPick.ShowDialog();

        if (!frmProviderPick.IsDialogOK)
        {
            return;
        }

        _provNumMoveTo = frmProviderPick.ProvNumSelected;
        if (_provNumMoveTo > 0)
        {
            var provider = _providers.Find(x => x.Id == _provNumMoveTo);
            textMoveTo.Text = provider.Description;
            return;
        }

        textMoveTo.Text = "None";
    }

    private void butMovePri_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.PatPriProvEdit))
        {
            return;
        }

        if (gridMain.SelectedIndices.Length < 1)
        {
            ShowError("You must select at least one provider to move patients from.");
            return;
        }

        var listProvidersFrom = gridMain.SelectedIndices.OfType<int>().Select(x => (Provider) gridMain.ListGridRows[x].Tag).ToList();
        if (_provNumMoveTo == -1)
        {
            ShowError("You must pick a 'To' provider in the box above to move patients to.");
            return;
        }

        if (_provNumMoveTo == 0)
        {
            ShowError("'None' is not a valid primary provider.");
            return;
        }

        var provider = _providers.FirstOrDefault(x => x.Id == _provNumMoveTo);
        if (provider is null)
        {
            ShowError("The provider could not be found.");
            return;
        }

        Lookup<long, long> lookupPriProvPats = null;

        var progressOD = new ProgressWin();
        progressOD.ActionMain = () =>
        {
            //get pats with original (from) priprov
            var listProvNums = listProvidersFrom.Select(x => x.ProvNum).ToList();
            var table = Patients.GetPatNumsByPriProvs(listProvNums);
            var dataRowArray = table.Select();
            //key=ProvNum, gives list of PatNums
            lookupPriProvPats = (Lookup<long, long>) dataRowArray.ToLookup(x => SIn.Long(x["PriProv"].ToString()), x => SIn.Long(x["PatNum"].ToString()));
        };

        progressOD.StartingMessage = "Gathering patient data...";
        progressOD.ShowDialog();

        if (progressOD.IsCancelled)
        {
            return;
        }

        var patCountTotal = 0;
        var listKeys = lookupPriProvPats.Select(x => x.Key).ToList();
        for (var i = 0; i < listKeys.Count; i++)
        {
            patCountTotal += lookupPriProvPats[listKeys[i]].Count();
        }

        if (patCountTotal == 0)
        {
            ShowError("The selected providers are not primary providers for any patients.");
            return;
        }

        var strProvFromDesc = string.Join(", ", listProvidersFrom.FindAll(x => lookupPriProvPats.Contains(x.ProvNum)).Select(x => x.Abbr));
        var strProvToDesc = provider.Abbr;
        var msg = Lan.g(this, "Move all primary patients to") + " " + strProvToDesc + " " + Lan.g(this, "from the following providers") + ": " + strProvFromDesc + "?";
        if (ODMessageBox.Show(msg, "", MessageBoxButtons.OKCancel) != DialogResult.OK)
        {
            return;
        }

        var patsMoved = 0;
        progressOD = new ProgressWin();
        progressOD.ActionMain = () =>
        {
            var listActions = lookupPriProvPats.Select(x => new Action(() =>
            {
                patsMoved += x.Count();
                ODEvent.Fire(ODEventType.ProgressBar, Lan.g(this, "Moving patients") + ": " + patsMoved + " out of " + patCountTotal);
                Patients.ChangePrimaryProviders(x.Key, provider.Id); //update all priprovs to new provider
                SecurityLogs.MakeLogEntry(EnumPermType.PatPriProvEdit, 0, "Primary provider changed for " + x.Count() + " patients from "
                                                                          + Providers.GetLongDesc(x.Key) + " to " + provider.Description + ".");
            })).ToList();
            ODThread.RunParallel(listActions, TimeSpan.FromMinutes(2));
        };
        progressOD.StartingMessage = Lan.g(this, "Moving patients") + "...";
        progressOD.TestSleep = true;
        
        progressOD.ShowDialog();
        
        _changed = true;
        
        FillGrid();
    }

    private void butMoveSec_Click(object sender, EventArgs e)
    {
        if (gridMain.SelectedIndices.Length < 1)
        {
            ShowError("You must select at least one provider to move patients from.");
            return;
        }

        var providersFrom = gridMain.SelectedIndices.Select(x => (Provider) gridMain.ListGridRows[x].Tag).ToList();
        if (_provNumMoveTo == -1)
        {
            ShowError("You must pick a 'To' provider in the box above to move patients to.");
            return;
        }

        var provider = _providers.FirstOrDefault(x => x.Id == _provNumMoveTo);
        
        string msg;
        if (provider == null)
        {
            msg = "Remove all secondary patients from the selected providers" + "?";
        }
        else
        {
            var strProvsFrom = string.Join(", ", providersFrom.Select(x => x.Abbr));
            msg = "Move all secondary patients to " + provider.Abbr + " from the following providers: " + strProvsFrom + "?";
        }

        if (!ConfirmOk(msg))
        {
            return;
        }

        var progress = new ProgressWin
        {
            ActionMain = () =>
            {
                var listActions = providersFrom.Select(x => new Action(() =>
                {
                    Patients.ChangeSecondaryProviders(x.ProvNum, provider?.Id ?? 0);
                })).ToList();
                ODThread.RunParallel(listActions, TimeSpan.FromMinutes(2));
            },
            StartingMessage = "Reassigning patients...",
            TestSleep = true
        };
        progress.ShowDialog();
        
        _changed = true;
        
        FillGrid();
    }
    
    private class PatProv
    {
        public long PatNum;
        public long ProvNum;
    }

    private void butReassign_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.PatPriProvEdit))
        {
            return;
        }

        if (gridMain.SelectedIndices.Length == 0)
        {
            ShowError("Please select a provider, first.");
            return;
        }

        if (!ConfirmOk("Ready to look for possible reassignments.  This will take a few minutes, and may make the program unresponsive on other computers during that time.  You will be given one more chance after this to cancel before changes are made to the database.  Running this for one provider at a time can help minimize database slowdown.  Continue?"))
        {
            return;
        }

        Cursor = Cursors.WaitCursor;
        
        var provNumsFrom = gridMain.SelectedIndices.Select(x => ((Provider) gridMain.ListGridRows[x].Tag).ProvNum).ToList();
        
        var patNumsDataTable = Patients.GetPatNumsByPriProvs(provNumsFrom);
        if (patNumsDataTable.Rows.Count == 0)
        {
            Cursor = Cursors.Default;
            
            ShowError("No patients to reassign.");
            
            return;
        }

        var patProvsFrom = new List<PatProv>();
        for (var i = 0; i < patNumsDataTable.Rows.Count; i++)
        {
            patProvsFrom.Add(new PatProv
            {
                PatNum = SIn.Long(patNumsDataTable.Rows[i]["PatNum"].ToString()),
                ProvNum = SIn.Long(patNumsDataTable.Rows[i]["PriProv"].ToString())
            });
        }
        
        var patProvsSeen = new List<PatProv>();
        
        var progress = new ProgressWin
        {
            ActionMain = () =>
            {
                var patNums = patProvsFrom.Select(x => x.PatNum).ToList();
                var table = Procedures.GetTablePatProvUsed(patNums);

                for (var i = 0; i < table.Rows.Count; i++)
                {
                    var patProv = new PatProv
                    {
                        PatNum = SIn.Long(table.Rows[i]["PatNum"].ToString()),
                        ProvNum = SIn.Long(table.Rows[i]["ProvNum"].ToString())
                    };
                
                    if (patProvsSeen.Any(x => x.PatNum == patProv.PatNum))
                    {
                        continue;
                    }

                    var patProvOld = patProvsFrom.Find(x => x.PatNum == patProv.PatNum);
                    var provNumOld = patProvOld.ProvNum;
                    if (patProv.ProvNum == provNumOld)
                    {
                        continue;
                    }

                    patProvsSeen.Add(patProv);
                }
            },
            StartingMessage = "Gathering patient and provider details..."
        };

        progress.ShowDialog();

        Cursor = Cursors.Default;
        
        if (patProvsSeen.Count == 0)
        {
            ShowError("No patients to reassign.");
            return;
        }

        if (!ConfirmOk("You are about to reassign " + patProvsSeen.Count + " patients to different providers.  Continue?"))
        {
            return;
        }

        Cursor = Cursors.WaitCursor;
        
        progress = new ProgressWin
        {
            ActionMain = () =>
            {
                foreach (var patProv in patProvsSeen)
                {
                    Patients.UpdateProv(patProv.PatNum, patProv.ProvNum);
                }
            },
            StartingMessage = Lan.g(this, "Reassigning patients") + "..."
        };
        
        progress.ShowDialog();
        
        Cursor = Cursors.Default;
        
        FillGrid();
        
        ShowInfo("Done");
    }

    private void butCreateUsers_Click(object sender, EventArgs e)
    {
        if (gridMain.SelectedIndices.Length == 0)
        {
            ShowError("Please select one or more providers first.");
            return;
        }

        foreach (var t in gridMain.SelectedIndices)
        {
            if (!Providers.IsAttachedToUser(((Provider) gridMain.ListGridRows[gridMain.SelectedIndices[t]].Tag).ProvNum))
            {
                continue;
            }
            
            ShowError("Not allowed to create users on providers which already have users.");
            return;
        }

        if (comboUserGroup.GetListSelected<UserGroup>().Count == 0)
        {
            ShowError("Please select at least one User Group first.");
            return;
        }

        foreach (var index in gridMain.SelectedIndices)
        {
            var provider = (Provider) gridMain.ListGridRows[index].Tag;

            var userod = new Userod
            {
                ProvNum = provider.ProvNum,
                UserName = GetUniqueUserName(provider.LName, provider.FName)
            };

            if (userod.UserName.TrimEnd() != userod.UserName)
            {
                MsgBox.Show(this, "User Name cannot end with white space.");
                _changed = true;
                return;
            }

            userod.SetPassword(Authentication.GenerateLoginDetailsSHA512(userod.UserName));
            try
            {
                Userods.Insert(userod, comboUserGroup.GetListSelected<UserGroup>().Select(x => x.UserGroupNum).ToList());
            }
            catch (ApplicationException ex)
            {
                ODMessageBox.Show(ex.Message);
                _changed = true;
                return;
            }
        }

        _changed = true;
        FillGrid();
    }

    private string GetUniqueUserName(string lname, string fname)
    {
        var name = lname;
        if (fname.Length > 0)
        {
            name += fname.Substring(0, 1);
        }

        if (Userods.IsUserNameUnique(name, 0, false))
        {
            return name;
        }

        var fnameI = 1;
        while (fnameI < fname.Length)
        {
            name += fname.Substring(fnameI, 1);
            if (Userods.IsUserNameUnique(name, 0, false))
            {
                return name;
            }

            fnameI++;
        }

        do
        {
            name += "x";
        } while (!Userods.IsUserNameUnique(name, 0, false));

        return name;
    }

    private void checkShowDeleted_CheckedChanged(object sender, EventArgs e)
    {
        if (checkShowDeleted.Checked)
        {
            checkShowHidden.Checked = true;
        }

        FillGrid();
    }

    private void checkShowPatientCount_CheckedChanged(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FormProviderSelect_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        var duplicates = Providers.GetDuplicateAbbrs();
        if (duplicates != "")
        {
            if (!ConfirmOk("Warning.  The following abbreviations are duplicates.  Continue anyway?\r\n" + duplicates))
            {
                e.Cancel = true;
                return;
            }
        }

        if (_changed)
        {
            DataValid.SetInvalid(InvalidType.Providers, InvalidType.Security);
        }
    }
}