using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental
{
    public partial class FormClinics : FormODBase
    {
        private long _clinicNumTo = -1;
        private List<Clinics.ClinicCount> _listClinicCounts;
        private List<DefLink> _listDefLinksSpecialties;
        private bool _clinicChanged;

        public bool IsSelectionMode { get; set; }
        public List<ClinicDto> Clinics { get; set; }
        public long SelectedClinicId { get; set; }

        public FormClinics()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            InitializeLayoutManager();
            Lan.F(this);
        }

        private void FormClinics_Load(object sender, EventArgs e)
        {
            Clinics ??= Imedisoft.Core.Features.Clinics.Clinics.GetAllForUserod(Security.CurUser);

            _listClinicCounts = new List<Clinics.ClinicCount>();
            _clinicChanged = false;

            if (IsSelectionMode)
            {
                butAdd.Visible = false;
                groupMovePats.Visible = false;
                checkShowHidden.Visible = false;
                checkShowHidden.Checked = false;
            }
            else
            {
                _listClinicCounts = Imedisoft.Core.Features.Clinics.Clinics.GetListClinicPatientCount();
                butSave.Visible = false;
            }

            FillGrid(false);
        }

        private void FillGrid(bool doReselctRows = true)
        {
            var listClinicNumsSelected = new List<long>();
            if (doReselctRows)
            {
                listClinicNumsSelected = gridMain.SelectedTags<ClinicDto>().Select(x => x.Id).ToList();
            }

            _listDefLinksSpecialties = DefLinks.GetDefLinksByType(DefLinkType.ClinicSpecialty);

            gridMain.BeginUpdate();
            gridMain.Columns.Clear();
            gridMain.Columns.Add(new GridColumn(Lan.g("TableClinics", "Abbr"), 120));
            gridMain.Columns.Add(new GridColumn(Lan.g("TableClinics", "Description"), 200));
            gridMain.Columns.Add(new GridColumn(Lan.g("TableClinics", "Specialty"), 150));
            if (!IsSelectionMode)
            {
                gridMain.Columns.Add(new GridColumn(Lan.g("TableClinics", "Pat Count"), 80, HorizontalAlignment.Center));
                gridMain.Columns.Add(new GridColumn(Lan.g("TableClinics", "Hidden"), 40, HorizontalAlignment.Center) {IsWidthDynamic = true});
            }

            gridMain.ListGridRows.Clear();
            var listIndicesToReselect = new List<int>();
            for (var i = 0; i < Clinics.Count; i++)
            {
                if (!checkShowHidden.Checked && Clinics[i].IsHidden)
                {
                    continue;
                }

                var row = new GridRow();
                row.Cells.Add((Clinics[i].Id == 0 ? "" : Clinics[i].Abbr));
                row.Cells.Add(Clinics[i].Description);
                var listDefNums = _listDefLinksSpecialties.FindAll(x => x.FKey == Clinics[i].Id).Select(x => x.DefNum).ToList();
                var listDescripts = Defs.GetDefs(DefCat.ClinicSpecialty, listDefNums)
                    .Where(x => !string.IsNullOrWhiteSpace(x.ItemName))
                    .Select(x => x.ItemName).ToList();
                var specialties = string.Join(",", listDescripts);
                row.Cells.Add(specialties);
                if (!IsSelectionMode)
                {
                    //selection mode means no IsHidden or Pat Count columns
                    var clinicCount = _listClinicCounts.FirstOrDefault(x => x.ClinicNum == Clinics[i].Id);
                    if (clinicCount is null)
                    {
                        row.Cells.Add("0");
                    }
                    else
                    {
                        row.Cells.Add(POut.Int(clinicCount.Count));
                    }

                    row.Cells.Add(Clinics[i].IsHidden ? "X" : "");
                }

                row.Tag = Clinics[i];
                gridMain.ListGridRows.Add(row);
                if (listClinicNumsSelected.Contains(Clinics[i].Id))
                {
                    listIndicesToReselect.Add(gridMain.ListGridRows.Count - 1);
                }
            }

            gridMain.EndUpdate();
            if (doReselctRows && listIndicesToReselect.Count > 0)
            {
                gridMain.SetSelected(listIndicesToReselect.ToArray(), true);
            }
        }

        private void butAdd_Click(object sender, EventArgs e)
        {
            if (!Security.IsAuthorized(EnumPermType.ClinicEdit))
            {
                return;
            }

            var clinic = new ClinicDto();
            //clinic.IsNew=true;
            using var formClinicEdit = new FormClinicEdit(clinic, Clinics);
            if (formClinicEdit.ShowDialog() == DialogResult.OK)
            {
                // TODO: clinic.ClinicNum=Clinics.Insert(clinic);
                Clinics.Add(clinic);
                var clinicCount = new Clinics.ClinicCount();
                clinicCount.ClinicNum = clinic.Id;
                clinicCount.Count = 0;
                _listClinicCounts.Add(clinicCount);
                formClinicEdit.ListDefLinksSpecialties.ForEach(x => x.FKey = clinic.Id); //Change ClinicNum to match inserted clinic before Inserting
                formClinicEdit.ListDefLinksSpecialties.ForEach(x => DefLinks.Insert(x));
                _clinicChanged = true;
            }

            FillGrid();
        }

        private void Grid_CellDoubleClick(object sender, ODGridClickEventArgs e)
        {
            if (!IsSelectionMode && !Security.IsAuthorized(EnumPermType.ClinicEdit))
            {
                return;
            }

            if (gridMain.ListGridRows.Count == 0)
            {
                return;
            }

            if (IsSelectionMode)
            {
                SelectedClinicId = ((ClinicDto) gridMain.ListGridRows[e.Row].Tag).Id;
                DialogResult = DialogResult.OK;
                return;
            }

            var clinicOld = (ClinicDto) gridMain.ListGridRows[e.Row].Tag;
            
            using var formClinicEdit = new FormClinicEdit(clinicOld, Clinics);
            
            if (formClinicEdit.ShowDialog() == DialogResult.OK)
            {
                var clinicNew = formClinicEdit.ClinicCur;
                if (clinicNew == null)
                {
                    Clinics.Remove(clinicOld);
                    
                    _clinicChanged = true;
                }
                else
                {
                    var listDefLinksOldSpecialties = DefLinks.GetDefLinksForClinicSpecialties(clinicOld.Id);
                    
                    formClinicEdit.ListDefLinksSpecialties.ForEach(x => x.FKey = clinicNew.Id);
                    
                    for (var i = 0; i < listDefLinksOldSpecialties.Count; i++)
                    {
                        if (formClinicEdit.ListDefLinksSpecialties.Select(x => x.DefNum).Contains(listDefLinksOldSpecialties[i].DefNum))
                        {
                            continue;
                        }

                        DefLinks.Delete(listDefLinksOldSpecialties[i].DefLinkNum);
                        
                        _clinicChanged = true;
                    }
                    
                    for (var i = 0; i < formClinicEdit.ListDefLinksSpecialties.Count; i++)
                    {
                        if (listDefLinksOldSpecialties.Select(x => x.DefNum).Contains(formClinicEdit.ListDefLinksSpecialties[i].DefNum))
                        {
                            continue;
                        }

                        DefLinks.Insert(formClinicEdit.ListDefLinksSpecialties[i]);
                        
                        _clinicChanged = true;
                    }

                    Clinics[Clinics.IndexOf(clinicOld)] = clinicNew;
                }
            }

            FillGrid();
        }

        private void ButtonPickClinic_Click(object sender, EventArgs e)
        {
            var clinics = gridMain.GetTags<ClinicDto>();
            
            using var formClinics = new FormClinics();
            
            formClinics.Clinics = clinics;
            formClinics.IsSelectionMode = true;
            
            if (formClinics.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            _clinicNumTo = formClinics.SelectedClinicId;
            
            textMoveTo.Text = clinics.FirstOrDefault(x => x.Id == _clinicNumTo)?.Abbr ?? "";
        }

        private void ButtonMovePatients_Click(object sender, EventArgs e)
        {
            if (gridMain.SelectedIndices.Length < 1)
            {
                MsgBox.Show(this, "You must select at least one clinic to move patients from.");
                return;
            }

            if (_clinicNumTo == -1)
            {
                MsgBox.Show(this, "You must pick a 'To' clinic in the box above to move patients to.");
                return;
            }

            var clinicsFrom = gridMain.SelectedTags<ClinicDto>();
            var clinicTo = gridMain.GetTags<ClinicDto>().FirstOrDefault(x => x.Id == _clinicNumTo);
            
            if (clinicTo is null)
            {
                MsgBox.Show(this, "The clinic could not be found.");
                return;
            }

            if (clinicsFrom.Any(x => x.Id == clinicTo.Id))
            {
                MsgBox.Show(this, "The 'To' clinic should not also be one of the 'From' clinics.");
                return;
            }

            var listClinicCountsAll = Imedisoft.Core.Features.Clinics.Clinics.GetListClinicPatientCount(true);
            var listClinicCountsSelected = listClinicCountsAll.FindAll(x => clinicsFrom.Any(y => y.Id == x.ClinicNum));
            if (listClinicCountsSelected.Sum(x => x.Count) == 0)
            {
                MsgBox.Show(this, "There are no patients assigned to the selected clinics.");
                return;
            }

            var msg = Lan.g(this, "This will move all patients to") + " " + clinicTo.Abbr + " " + Lan.g(this, "from the following clinics") + ":\r\n"
                      + string.Join("\r\n", clinicsFrom.Select(x => x.Abbr)) + "\r\n" + Lan.g(this, "Continue?");
            if (ODMessageBox.Show(msg, "", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            var progressOD = new ProgressWin();
            progressOD.ActionMain = () =>
            {
                var patsMoved = 0;
                var countTotal = listClinicCountsSelected.Sum(x => x.Count);
                var listActions = listClinicCountsSelected.Select(x => new Action(() =>
                {
                    Patients.ChangeClinicsForAll(x.ClinicNum, clinicTo.Id);
                    SecurityLogs.MakeLogEntry(EnumPermType.PatientEdit, 0, 
                        "Clinic changed for " + x.Count + " patients from " + 
                        Imedisoft.Core.Features.Clinics.Clinics.GetAbbr(x.ClinicNum) + " to " + clinicTo.Abbr + ".");
                    patsMoved += x.Count;
                    ODEvent.Fire(ODEventType.ProgressBar, Lan.g(this, "Moved patients") + ": " + patsMoved + " " + Lan.g(this, "out of total") + " "
                                                          + countTotal);
                })).ToList();
                ODThread.RunParallel(listActions, TimeSpan.FromMinutes(2));
            };
            
            progressOD.StartingMessage = Lan.g(this, "Moving patients") + "...";
            progressOD.TestSleep = true;
            progressOD.ShowDialog();
            
            _listClinicCounts = Imedisoft.Core.Features.Clinics.Clinics.GetListClinicPatientCount();
            
            FillGrid();
            
            if (progressOD.IsCancelled)
            {
                return;
            }

            MsgBox.Show(this, "Done");
        }
        
        private void CheckBoxShowHidden_CheckedChanged(object sender, EventArgs e)
        {
            FillGrid();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            if (IsSelectionMode && gridMain.SelectedIndices.Length > 0)
            {
                SelectedClinicId = gridMain.SelectedTag<ClinicDto>()?.Id ?? 0;
                DialogResult = DialogResult.OK;
            }

            Close();
        }

        private void FormClinics_Closing(object sender, CancelEventArgs e)
        {
            if (DialogResult == DialogResult.Cancel)
            {
                SelectedClinicId = 0;
            }

            if (IsSelectionMode)
            {
                return;
            }

            // TODO: _clinicChanged|=Clinics.Sync(ListClinics,_listClinicsOld);//returns true if clinics were updated/inserted/deleted
            //Joe - Now that we have called sync on ListClinics we want to make sure that each clinic has program properties for PayConnect and XCharge
            //We are doing this because of a previous bug that caused some customers to have over 3.4 million duplicate rows in their programproperty table
            var payConnectProgNum = Programs.GetProgramNum(ProgramName.PayConnect);
            var xChargeProgNum = Programs.GetProgramNum(ProgramName.Xcharge);
            //Don't need to do this for PaySimple, because these will get generated as needed in FormPaySimpleSetup
            var hasProgramChanges = ProgramProperties.InsertForClinic(payConnectProgNum,
                Clinics.Select(x => x.Id).Where(x => ProgramProperties.GetListForProgramAndClinic(payConnectProgNum, x).Count == 0).ToList());
            hasProgramChanges |= ProgramProperties.InsertForClinic(xChargeProgNum,
                Clinics.Select(x => x.Id).Where(x => ProgramProperties.GetListForProgramAndClinic(xChargeProgNum, x).Count == 0).ToList());
            if (hasProgramChanges)
            {
                DataValid.SetInvalid(InvalidType.Programs);
            }

            if (_clinicChanged)
            {
                DataValid.SetInvalid(InvalidType.Providers);
            }
        }
    }
}