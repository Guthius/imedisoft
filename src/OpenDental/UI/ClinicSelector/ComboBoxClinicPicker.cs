using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using OpenDentBusiness;

namespace OpenDental.UI;

public partial class ComboBoxClinicPicker : UserControl
{
    private const long CLINIC_NUM_ALL = -2;
    private const long CLINIC_NUM_UNASSIGNED = 0;
    private const int WidthLabelArea = 37;

    private readonly List<ClinicDto> _listClinics = [];
    private readonly SolidBrush _brushBack = new(Color.White);
    private readonly SolidBrush _brushDisabledBack = new(Color.FromArgb(204, 204, 204));
    private readonly SolidBrush _brushDisabledText = new(Color.FromArgb(109, 109, 109));
    private readonly SolidBrush _brushHover = new(Color.FromArgb(229, 241, 251));
    private readonly Pen _penArrow = new(Color.FromArgb(20, 20, 20), 1.5f);
    private readonly Pen _penHoverOutline = new(Color.FromArgb(0, 120, 215));
    private readonly Pen _penOutline = new(Color.FromArgb(173, 173, 173), 1);
    private bool _includeAll;
    private bool _includeUnassigned;
    private int _indexSelected = -1;
    private bool _forceShowUnassigned;
    private FormComboPicker _formComboPicker;
    private string _hqDescription = "Unassigned";
    private bool _isMouseOver;
    private List<int> _listIndicesSelected = [];
    private ClinicDto _clinicSelectedNoPermission;
    private bool _isMultiSelect;
    private bool _showLabel = true;
    private Userod _userod = Security.CurUser;

    public ComboBoxClinicPicker()
    {
        InitializeComponent();
        Size = new Size(200, 21); 
        Name = "comboClinic";
    }

    private void OnSelectionChangeCommitted(object sender, EventArgs e)
    {
        SelectionChangeCommitted?.Invoke(sender, e);
    }

    [Category("OD"), Description("Occurs when user selects a Clinic from the drop-down list.")]
    public event EventHandler SelectionChangeCommitted;

    private void OnSelectedIndexChanged(object sender, EventArgs e)
    {
        SelectedIndexChanged?.Invoke(sender, e);
    }

    [Category("OD"), Description("Try not to use this. The preferred technique is to use SelectionChangeCommitted to react to each user click. In contrast, this event will fire even if the selection programmatically changes.")]
    public event EventHandler SelectedIndexChanged;

    [Category("OD")]
    [Description("This will be set to true if we always need to show Unassigned/0, regardless of user permissions.")]
    [DefaultValue(false)]
    public bool ForceShowUnassigned
    {
        get => _forceShowUnassigned;
        set
        {
            _forceShowUnassigned = value;
            FillClinics();
        }
    }

    [Category("OD")]
    [Description("The display value for ClinicNum 0. Default is 'Unassigned', but might want 'Default', 'HQ', 'None', 'Practice', etc.  Do not specify 'All' here, because that is not accurate.  Only used when 'DoIncludeUnassigned'")]
    [DefaultValue("Unassigned")]
    public string HqDescription
    {
        get => _hqDescription;
        set
        {
            _hqDescription = value;
            var listSelectedClinicNums = ListClinicNumsSelected;
            FillClinics();
            _listIndicesSelected = [];
            for (var i = 0; i < _listClinics.Count; i++)
            {
                if (listSelectedClinicNums.Contains(_listClinics[i].Id))
                {
                    _indexSelected = i;
                    _listIndicesSelected.Add(i);
                }
            }
        }
    }

    [Category("OD")]
    [Description("Set to true to include 'All' as a selection option. 'All' can sometimes (e.g. FormOperatories) be intended to included more clinics than are actually showing in list.")]
    [DefaultValue(false)]
    public bool IncludeAll
    {
        get => _includeAll;
        set
        {
            _includeAll = value;
            FillClinics();
        }
    }

    [Category("OD")]
    [Description("Set to true to include hidden clinics in ListSelectedClinicNums when 'All' is selected.  This also causes (includes hidden) to show next to All.  Used for reports where we don't want to miss any entries.  List will always include 'unassigned/0' clinic.")]
    [DefaultValue(false)]
    public bool IncludeHiddenInAll { get; set; }

    [Category("OD")]
    [Description("Set to true to include 'Unassigned' as a selection option. The word 'Unassigned' can be changed with the HqDescription property.  This is ClinicNum=0.")]
    [DefaultValue(false)]
    public bool IncludeUnassigned
    {
        get => _includeUnassigned;
        set
        {
            _includeUnassigned = value;
            FillClinics();
        }
    }

    [Category("OD")]
    [Description("Set true for multi-select, false for single-select.")]
    [DefaultValue(false)]
    public bool IsMultiSelect
    {
        get => _isMultiSelect;
        set
        {
            if (_isMultiSelect == value)
            {
                return;
            }

            _isMultiSelect = value;
            if (_isMultiSelect)
            {
                labelFake.Text = "Clinics";
            }
        }
    }

    [Category("OD")]
    [Description("Normally true to show label on left.  Set to false if not enough space and you want to do your own separate label.  Make sure to manually set visibility of that label, based on whether Clinic feature is turned on.")]
    [DefaultValue(true)]
    public bool ShowLabel
    {
        get => _showLabel;
        set
        {
            if (_showLabel == value)
            {
                return;
            }

            _showLabel = value;
            
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public long ClinicNumSelected
    {
        get
        {
            if (_clinicSelectedNoPermission != null)
            {
                return _clinicSelectedNoPermission.Id;
            }

            if (_indexSelected == -1)
            {
                return -1;
            }

            return _listClinics[_indexSelected].Id;
        }
        set
        {
            switch (value)
            {
                case CLINIC_NUM_ALL:
                    throw new ApplicationException("Clinic num cannot be set to -2.  Instead, use IsAllSelected.");
                
                case -1:
                    throw new ApplicationException("Clinic num cannot be set to -1.  Instead, set IsNothingSelected.");
                
                default:
                    SetSelectedClinicNum(value);
                    break;
            }
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsAllSelected
    {
        get => ClinicNumSelected == CLINIC_NUM_ALL;
        set => SetSelectedClinicNum(CLINIC_NUM_ALL);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsNothingSelected => _indexSelected == -1;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [DefaultValue(false)]
    public bool IsTestModeNoDb => false;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsUnassignedSelected
    {
        get => ClinicNumSelected == CLINIC_NUM_UNASSIGNED;
        set => ClinicNumSelected = CLINIC_NUM_UNASSIGNED;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<long> ListClinicNumsSelected
    {
        get
        {
            List<long> listClinicNumsSelected = [];
            if (_clinicSelectedNoPermission != null)
            {
                listClinicNumsSelected.Add(_clinicSelectedNoPermission.Id);
                return listClinicNumsSelected;
            }

            if (_listIndicesSelected.Count == 0)
            {
                return listClinicNumsSelected;
            }

            if (_includeAll && _listIndicesSelected.Contains(0))
            {
                //The "All" item was selected
                if (IncludeHiddenInAll)
                {
                    var listClinicsAll = Clinics.GetAllForUserod(_userod);
                    var listClinicNums = listClinicsAll.Select(x => x.Id).ToList();
                    listClinicNums.Add(0);
                    return listClinicNums;
                }

                foreach (var clinic in _listClinics)
                {
                    if (clinic.Id == CLINIC_NUM_ALL)
                    {
                        continue;
                    }

                    //seems to include the "unassigned/default/hq/none/all" clinicNum=0, if present
                    listClinicNumsSelected.Add(clinic.Id);
                }

                return listClinicNumsSelected;
            }

            foreach (var idx in _listIndicesSelected)
            {
                listClinicNumsSelected.Add(_listClinics[idx].Id);
            }

            return listClinicNumsSelected;
        }
        set
        {
            _clinicSelectedNoPermission = null;
            _listIndicesSelected = [];
            for (var i = 0; i < _listClinics.Count; i++)
            {
                if (value.Contains(_listClinics[i].Id))
                {
                    _indexSelected = i;
                    _listIndicesSelected.Add(i);
                }
            }

            OnSelectedIndexChanged(this, EventArgs.Empty);
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<ClinicDto> ListClinics => [.._listClinics];

    protected override Size DefaultSize => new(200, 21);

    public override string Text
    {
        get
        {
            var widthMax = Width - 1 - 15;
            if (ShowLabel)
            {
                widthMax -= WidthLabelArea;
            }

            return GetDisplayText(widthMax);
        }
    }

    public string GetSelectedAbbr()
    {
        return _indexSelected == -1 ? "" : _listClinics[_indexSelected].Abbr;
    }

    public ClinicDto GetSelectedClinic()
    {
        return _indexSelected == -1 ? null : _listClinics[_indexSelected];
    }

    public string GetStringSelectedClinics()
    {
        var listSelectedClinics = _listIndicesSelected.Select(x => _listClinics[x]).ToList();
        if (listSelectedClinics.Any(x => x.Id == CLINIC_NUM_ALL))
        {
            return "All Clinics";
        }

        return string.Join(",", listSelectedClinics.Select(clinic => clinic.Abbr));
    }

    public void SetUser(Userod userod)
    {
        _userod = userod;
        
        FillClinics();
    }

    public bool UserHasPermission()
    {
        return _clinicSelectedNoPermission == null;
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        if (m.Msg == 0x020A)
        {
            int delta;
            if ((long) m.WParam >= int.MaxValue)
            {
                var wParam = new IntPtr((long) m.WParam << 32 >> 32);
                delta = wParam.ToInt32() >> 16;
            }
            else
            {
                delta = m.WParam.ToInt32() >> 16;
            }

            delta *= -1;
            
            var sarg = delta > 0 ? new ScrollEventArgs(ScrollEventType.EndScroll, 0, 1) : new ScrollEventArgs(ScrollEventType.EndScroll, 1, 0);
            
            ComboBoxClinicPicker_Scroll(sarg);
        }
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        base.OnHandleDestroyed(e);
        
        if (_formComboPicker is not {IsDisposed: false})
        {
            return;
        }
        
        _formComboPicker.Close();
        _formComboPicker.Dispose();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        
        var charKey = (char) e.KeyCode;
        if (e.KeyCode is >= Keys.NumPad0 and <= Keys.NumPad9)
        {
            charKey = e.KeyCode.ToString().Replace("NumPad", "")[0];
        }

        var foundMatch = false;
        if (char.IsLetterOrDigit(charKey) && !IsMultiSelect)
        {
            if (_listIndicesSelected.Count < 1)
            {
                foundMatch = SetSearchedIndex(0, charKey);
                if (foundMatch)
                {
                    OnSelectedIndexChanged(this, EventArgs.Empty);
                    OnSelectionChangeCommitted(this, EventArgs.Empty);
                }

                Invalidate();
                return;
            }

            foundMatch = SetSearchedIndex(_listIndicesSelected[0] + 1, charKey);
            if (!foundMatch)
            {
                foundMatch = SetSearchedIndex(0, charKey);
            }
        }

        if (foundMatch)
        {
            OnSelectedIndexChanged(this, EventArgs.Empty);
            OnSelectionChangeCommitted(this, EventArgs.Empty);
        }

        Invalidate();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        
        FillClinics();
        
        if (!DesignMode && !IsTestModeNoDb)
        {
            Visible = true;
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        
        if (ShowLabel && e.X < WidthLabelArea)
        {
            return;
        }

        if (_clinicSelectedNoPermission != null)
        {
            if (Security.IsAuthorized(EnumPermType.UnrestrictedSearch, true))
            {
            }
            else
            {
                MsgBox.Show(this, "Not allowed");
                return;
            }
        }

        _formComboPicker = new FormComboPicker();
        _formComboPicker.Font = Font;
        _formComboPicker.HeightCombo = Height;
        _formComboPicker.FormClosing += _formComboPicker_FormClosing;
        _formComboPicker.ListStrings = _listClinics.Select(x => x.Abbr).ToList();
        _formComboPicker.ListAbbrevs = _listClinics.Select(x => x.Abbr).ToList();
        _formComboPicker.PointInitialUR = PointToScreen(new Point(Width, 0));
        _formComboPicker.MinimumSize = new Size(15, 15);
        
        if (ShowLabel)
        {
            _formComboPicker.Width = Width - WidthLabelArea;
        }
        else
        {
            _formComboPicker.Width = Width;
        }

        if (IsMultiSelect)
        {
            _formComboPicker.IsMultiSelect = true;
            _formComboPicker.SelectedIndices = _listIndicesSelected;
        }
        else
        {
            _formComboPicker.SelectedIndex = _indexSelected;
        }

        _formComboPicker.Show();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        
        _isMouseOver = false;
        
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        
        if (ShowLabel && e.X < WidthLabelArea)
        {
            if (!_isMouseOver)
            {
                return;
            }

            _isMouseOver = false;
        }
        else
        {
            if (_isMouseOver)
            {
                return;
            }

            _isMouseOver = true;
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        
        g.SmoothingMode = SmoothingMode.AntiAlias;
        
        var rectangleCombo = new Rectangle
        {
            X = ShowLabel ? WidthLabelArea : 0,
            Y = 0
        };
        rectangleCombo.Width = Width - rectangleCombo.X - 1;
        rectangleCombo.Height = Height - 1;
        
        if (Enabled)
        {
            if (_isMouseOver)
            {
                g.FillRectangle(_brushHover, rectangleCombo);
                g.DrawRectangle(_penHoverOutline, rectangleCombo);
            }
            else
            {
                g.FillRectangle(_brushBack, rectangleCombo);
                g.DrawRectangle(_penOutline, rectangleCombo);
            }
        }
        else
        {
            g.FillRectangle(_brushDisabledBack, rectangleCombo);
            g.DrawRectangle(_penOutline, rectangleCombo);
        }
        
        g.DrawLine(_penArrow, Width - 13, 9, Width - 9.5f, 12);
        g.DrawLine(_penArrow, Width - 9.5f, 12, Width - 6, 9);
        
        var rectangleFString = new RectangleF
        {
            X = rectangleCombo.X + 2,
            Y = rectangleCombo.Y + 2,
            Width = rectangleCombo.Width - 2,
            Height = rectangleCombo.Height - 2
        };
        
        var widthMax = rectangleCombo.Width - 15;
        
        if (ShowLabel)
        {
            if (IsMultiSelect)
            {
                g.DrawString(labelFake.Text, Font, Brushes.Black, -2, 4);
            }
            else
            {
                g.DrawString(labelFake.Text, Font, Brushes.Black, 2, 4);
            }
        }

        var stringFormat = new StringFormat(StringFormatFlags.NoWrap);
        stringFormat.LineAlignment = StringAlignment.Center;
        string txtShow;
        try
        {
            txtShow = GetDisplayText(widthMax);
        }
        catch
        {
            return;
        }

        g.DrawString(txtShow, Font, Enabled ? Brushes.Black : _brushDisabledText, rectangleFString, stringFormat);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        
        Invalidate();
    }

    private void _formComboPicker_FormClosing(object sender, FormClosingEventArgs e)
    {
        _indexSelected = _formComboPicker.SelectedIndex;
        _listIndicesSelected = _formComboPicker.SelectedIndices;
        if (_listIndicesSelected.Count > 0)
        {
            _clinicSelectedNoPermission = null;
        }

        OnSelectionChangeCommitted(this, e);
        OnSelectedIndexChanged(this, e);
        
        Refresh();
    }

    private void ComboBoxClinicPicker_Scroll(ScrollEventArgs e)
    {
        if (IsMultiSelect)
        {
            return;
        }

        if (_listIndicesSelected.Count == 0)
        {
            _indexSelected = 0;
            _listIndicesSelected.Clear();
            _listIndicesSelected.Add(0);
            Invalidate();
            return;
        }

        if (e.OldValue > e.NewValue)
        {
            var index = _listIndicesSelected[0] - 1 < 0 ? _listIndicesSelected[0] : _listIndicesSelected[0] - 1;
            
            _indexSelected = index;
            _listIndicesSelected[0] = index;
        }
        else
        {
            var index = _listIndicesSelected[0] + 1 >= _listClinics.Count ? _listIndicesSelected[0] : _listIndicesSelected[0] + 1;
            
            _indexSelected = index;
            _listIndicesSelected[0] = index;
        }

        OnSelectedIndexChanged(this, EventArgs.Empty);
        OnSelectionChangeCommitted(this, EventArgs.Empty);
        
        Invalidate();
    }

    private void FillClinics()
    {
        if (!IsTestModeNoDb)
        {
            if (!Db.HasDatabaseConnection() && !Security.IsUserLoggedIn)
            {
                return;
            }
        }

        try
        {
            _listClinics.Clear();
            if (IncludeAll)
            {
                var txtAll = "All";
                if (IncludeHiddenInAll)
                {
                    txtAll += " (includes hidden)";
                }

                _listClinics.Add(new ClinicDto
                {
                    Abbr = txtAll,
                    Description = txtAll,
                    Id = CLINIC_NUM_ALL
                });
            }

            List<ClinicDto> listClinicsForUser;
            if (IsTestModeNoDb)
            {
                listClinicsForUser = [];
                for (var i = 0; i < 40; i++)
                {
                    listClinicsForUser.Add(new ClinicDto {Abbr = "Clinic" + i, Description = "Clinic" + i, Id = i});
                }

                if (IncludeAll)
                {
                    listClinicsForUser.Add(new ClinicDto {Abbr = HqDescription, Description = HqDescription, Id = 0});
                }
            }
            else
            {
                listClinicsForUser = Clinics.GetForUserod(_userod, true, HqDescription);
            }

            var clinicUnassigned = listClinicsForUser.Find(x => x.Id == CLINIC_NUM_UNASSIGNED);

            if (_forceShowUnassigned)
            {
                _listClinics.Add(new ClinicDto {Abbr = HqDescription, Description = HqDescription, Id = 0});
            }
            else if (IncludeUnassigned && clinicUnassigned != null)
            {
                _listClinics.Add(clinicUnassigned);
            }

            listClinicsForUser.RemoveAll(x => x.Id == CLINIC_NUM_UNASSIGNED);
            _listClinics.AddRange(listClinicsForUser);
            
            if (Clinics.ClinicNum == 0)
            {
                if (IncludeUnassigned)
                {
                    ClinicNumSelected = CLINIC_NUM_UNASSIGNED;
                }
                else if (IncludeAll)
                {
                    SetSelectedClinicNum(CLINIC_NUM_ALL);
                }
            }
            else
            {
                ClinicNumSelected = Clinics.ClinicNum;
            }
        }
        catch
        {
            // ignored
        }

        Invalidate();
    }

    private string GetDisplayText(int widthMax)
    {
        if (_clinicSelectedNoPermission != null)
        {
            if (_clinicSelectedNoPermission.IsHidden)
            {
                return _clinicSelectedNoPermission.Abbr + " (hidden)";
            }

            return _clinicSelectedNoPermission.Abbr;
        }

        if (_listIndicesSelected.Count == 0 || _listClinics.Count == 0)
        {
            return "";
        }

        if (_listIndicesSelected.Contains(0) && _listClinics[0].Id == CLINIC_NUM_ALL)
        {
            return _listClinics[0].Abbr;
        }

        var str = "";
        for (var i = 0; i < _listIndicesSelected.Count; i++)
        {
            if (i > 0)
            {
                str += ",";
            }

            if (_listIndicesSelected[i] > _listClinics.Count - 1)
            {
                continue;
            }

            str += _listClinics[_listIndicesSelected[i]].Abbr;
        }

        if (_listIndicesSelected.Count > 1)
        {
            if (TextRenderer.MeasureText(str, Font).Width > widthMax)
            {
                return "Multiple";
            }
        }

        return str;
    }

    private bool SetSearchedIndex(int startIdx, char charKey)
    {
        var listStrings = _listClinics.Select(x => x.Abbr).ToList();
        for (var i = startIdx; i < listStrings.Count; i++)
        {
            //loop through all of the items and only add the item if it is found
            if (listStrings[i].ToUpper().StartsWith(charKey.ToString()))
            {
                //charKey is already uppercased
                _indexSelected = i;
                _listIndicesSelected.Clear();
                _listIndicesSelected.Add(i);
                return true;
            }
        }

        return false;
    }

    private void SetSelectedClinicNum(long value)
    {
        var idx = _listClinics.FindIndex(x => x.Id == value);
        if (idx == -1)
        {
            if (value == -2)
            {
            }
            else
            {
                _indexSelected = -1;
                _listIndicesSelected = [];
                if (IsTestModeNoDb)
                {
                    _clinicSelectedNoPermission = new ClinicDto {Abbr = value.ToString(), Description = value.ToString(), Id = value};
                }
                else
                {
                    _clinicSelectedNoPermission = Clinics.GetClinic(value);
                    
                    if (_clinicSelectedNoPermission == null)
                    {
                        _clinicSelectedNoPermission = new ClinicDto {Abbr = value.ToString(), Description = value.ToString(), Id = value};
                    }
                }
            }
        }
        else
        {
            _clinicSelectedNoPermission = null;
            _indexSelected = idx;
            _listIndicesSelected = [idx];
        }

        OnSelectedIndexChanged(this, EventArgs.Empty);
        Invalidate();
    }
}