using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using CheckBox = System.Windows.Forms.CheckBox;
using ComboBox = System.Windows.Forms.ComboBox;
using ListBox = System.Windows.Forms.ListBox;
using Screen = System.Windows.Forms.Screen;

namespace OpenDental;

public class FormODBase : Form
{
    public const bool AreBordersMs = true;

    private static Color _colorBorder = Color.FromArgb(65, 94, 154);
    private static Color _colorBorderText = Color.White;

    private readonly List<Control> _controlsFilter = [];
    private readonly Timer _timerHoverSnap;

    private Action _actionFilter;
    private DateTime _dateTimeLastModified = DateTime.MaxValue;
    private bool _isImageFloatSelected;
    private ODThread _threadFilter;
    private readonly GraphicsPath _graphicsPathHelp = new();
    private readonly Region _regionButHelp = new();
    private FormWindowState _windowStateOld;

    protected static readonly List<FormODBase> FormsSubscribed = [];

    protected int WaitFilterMs = 1000;
    protected Rectangle RectangleButMin;

    public Panel PanelClient = null;
    public PanelSubclassBorder PanelBorders;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _graphicsPathHelp?.Dispose();
            _regionButHelp?.Dispose();
            _timerHoverSnap?.Dispose();
        }

        base.Dispose(disposing);
    }

    public FormODBase()
    {
        AutoScaleMode = AutoScaleMode.None;
        DoubleBuffered = true;
        KeyPreview = true;
        Name = "Form";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Form";
        BackColor = ColorOD.Background;
        Font = new Font("Microsoft Sans Serif", 8.25f);

        Shown += ODForm_Shown;
        FormClosing += ODForm_FormClosing;

        _timerHoverSnap = new Timer();
        _timerHoverSnap.Interval = 400;
        _timerHoverSnap.Tick += timerHoverSnap_Tick;
    }

    [Description("Jordan-Written to force this to always be treated as None, regardless of what it shows.")]
    [Category("Layout")]
    [DefaultValue(AutoScaleMode.None)]
    public new AutoScaleMode AutoScaleMode
    {
        get => AutoScaleMode.None;
        set => base.AutoScaleMode = AutoScaleMode.None;
    }

    public new Rectangle ClientRectangle => PanelClient?.ClientRectangle ?? base.ClientRectangle;

    [Browsable(false)]
    public new Size ClientSize
    {
        get => PanelClient?.ClientSize ?? base.ClientSize;
        set => base.ClientSize = value;
    }

    [Category("OD")]
    [Description("Default true.")]
    [DefaultValue(true)]
    public bool EscClosesWindow { get; set; } = true;

    [Category("OD")]
    [Description("Default true. Set to false to hide the help button.  Windows 'HelpButton' property is ignored.")]
    [DefaultValue(true)]
    public bool HasHelpButton { get; set; } = true;

    [Browsable(false)]
    public bool HasShown { get; private set; }

    [Category("OD")]
    [Description("Set to true for Kiosk to block user from dragging or clicking.")]
    [DefaultValue(false)]
    public bool IsBorderLocked { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsImageFloatSelected
    {
        get => _isImageFloatSelected;
        set
        {
            _isImageFloatSelected = value;

            PanelBorders?.Invalidate();
        }
    }

    protected void ShowException(Exception ex, string message)
    {
        FriendlyException.Show(message, ex);
    }
    
    protected void ShowError(string message)
    {
        ODMessageBox.Show(message, Text);
    }
    
    protected void ShowInfo(string message)
    {
        ODMessageBox.Show(message, Text);
    }

    protected bool Confirm(string prompt, string title = null)
    {
        return MsgBox.Show(this, MsgBoxButtons.YesNo, prompt, title ?? Text);
    }

    protected bool ConfirmOk(string prompt, string title = null)
    {
        return MsgBox.Show(this, MsgBoxButtons.OKCancel, prompt, title ?? Text);
    }

    private void ODForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        FormClosed += ODForm_FormClosed;
    }

    private void ODForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        if (_threadFilter is null)
        {
            return;
        }

        _threadFilter.QuitAsync();
        _threadFilter = null;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData != Keys.Escape)
        {
            return base.ProcessCmdKey(ref msg, keyData);
        }

        if (EscClosesWindow)
        {
            Close();
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void ODForm_Shown(object sender, EventArgs e)
    {
        HasShown = true;

        FormClosed += delegate { FormsSubscribed.Remove(this); };
        FormsSubscribed.Add(this);

        if (ODProgress.FormProgressActive != null)
        {
            Activate();
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if (WindowState != FormWindowState.Minimized)
        {
            _windowStateOld = WindowState;
        }
    }

    private void timerHoverSnap_Tick(object sender, EventArgs e)
    {
        _timerHoverSnap.Enabled = false;
    }

    public void CenterFormOnMonitor()
    {
        var rectangleWorkingArea = Screen.FromHandle(Handle).WorkingArea;

        Location = new Point(
            rectangleWorkingArea.X + rectangleWorkingArea.Width / 2 - Width / 2,
            rectangleWorkingArea.Y + rectangleWorkingArea.Height / 2 - Height / 2);
    }

    public void DisableAllExcept(params Control[] enabledControls)
    {
        foreach (Control ctrl in PanelClient.Controls)
        {
            if (enabledControls.Contains(ctrl))
            {
                continue;
            }

            try
            {
                ctrl.Enabled = false;
            }
            catch
            {
                // ignored
            }
        }
    }

    public static bool IsDisposedOrClosed(Form form)
    {
        if (form.IsDisposed)
        {
            return true;
        }

        if (form.GetType().GetProperty("HasClosed") == null)
        {
            return false;
        }

        return (bool) form.GetType().GetProperty("HasClosed").GetValue(form);
    }

    public void Restore()
    {
        if (WindowState == FormWindowState.Minimized)
        {
            WindowState = _windowStateOld;
        }
    }

    protected void SetBorderColor(DefCatMiscColors defCatMiscColors, Color color)
    {
        switch (defCatMiscColors)
        {
            case DefCatMiscColors.MainBorder:
                _colorBorder = color;
                break;

            case DefCatMiscColors.MainBorderOutline:
                break;

            case DefCatMiscColors.MainBorderText:
                _colorBorderText = color;
                break;
        }

        if (defCatMiscColors is DefCatMiscColors.MainBorder or DefCatMiscColors.MainBorderText)
        {
            Color.FromArgb(
                (3 * _colorBorder.R + _colorBorderText.R) / 4,
                (3 * _colorBorder.G + _colorBorderText.G) / 4,
                (3 * _colorBorder.B + _colorBorderText.B) / 4);
        }

        PanelBorders?.Invalidate();
    }

    private const int WM_WINDOWPOSCHANGED = 0x0047;

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_WINDOWPOSCHANGED)
        {
            var restoredBoundsSpecified = typeof(Form).GetField("restoredWindowBoundsSpecified", BindingFlags.Instance | BindingFlags.NonPublic);
            if (restoredBoundsSpecified != null)
            {
                restoredBoundsSpecified.SetValue(this, BoundsSpecified.None);
            }
        }

        base.WndProc(ref m);
    }

    public void ProcessSignals(List<Signalod> signals)
    {
        ProcessSignalODs(signals);
    }

    protected virtual void ProcessSignalODs(List<Signalod> signals)
    {
    }

    protected virtual string GetHelpOverride()
    {
        return "";
    }

    protected void SetFilterControlsAndAction(Action action, int waitFilterMs, params Control[] controls)
    {
        SetFilterControlsAndAction(action, controls);

        WaitFilterMs = waitFilterMs;
    }

    protected void SetFilterControlsAndAction(Action action, params Control[] controls)
    {
        if (HasShown)
        {
            return;
        }

        _actionFilter = action;

        foreach (var control in controls)
        {
            if (control.GetType().IsSubclassOf(typeof(CheckBox)) || control.GetType() == typeof(CheckBox))
            {
                var checkbox = (CheckBox) control;

                checkbox.CheckedChanged += Control_FilterCommitImmediate;
            }
            else if (control.GetType() == typeof(UI.CheckBox))
            {
                var checkbox = (UI.CheckBox) control;

                checkbox.CheckedChanged += Control_FilterCommitImmediate;
            }
            else if (control.GetType().IsSubclassOf(typeof(ComboBox)) || control.GetType() == typeof(ComboBox))
            {
                var comboBox = (ComboBox) control;

                comboBox.SelectionChangeCommitted += Control_FilterCommitImmediate;
            }
            else if (control.GetType().IsSubclassOf(typeof(ODDateRangePicker)) || control.GetType() == typeof(ODDateRangePicker))
            {
                var dateRangePicker = (ODDateRangePicker) control;

                dateRangePicker.CalendarSelectionChanged += Control_FilterCommitImmediate;
            }
            else if (control.GetType().IsSubclassOf(typeof(ODDatePicker)) || control.GetType() == typeof(ODDatePicker))
            {
                var datePicker = (ODDatePicker) control;

                datePicker.DateTextChanged += Control_FilterChange;
            }
            else if (control.GetType().IsSubclassOf(typeof(TextBoxBase)) || control.GetType() == typeof(TextBoxBase))
            {
                control.TextChanged += Control_FilterChange;
            }
            else if (control.GetType().IsSubclassOf(typeof(ListBox)) || control.GetType() == typeof(ListBox))
            {
                control.MouseUp += Control_FilterChange;
            }
            else if (control.GetType().IsSubclassOf(typeof(ComboBoxClinicPicker)) || control.GetType() == typeof(ComboBoxClinicPicker))
            {
                ((ComboBoxClinicPicker) control).SelectionChangeCommitted += Control_FilterCommitImmediate;
            }
            else if (control.GetType().IsSubclassOf(typeof(UI.ComboBox)) || control.GetType() == typeof(UI.ComboBox))
            {
                ((UI.ComboBox) control).SelectionChangeCommitted += Control_FilterCommitImmediate;
            }
            else if (control.GetType().IsSubclassOf(typeof(UI.ListBox)) || control.GetType() == typeof(UI.ListBox))
            {
                ((UI.ListBox) control).MouseUp += Control_FilterCommitImmediate;
            }
            else
            {
                throw new NotImplementedException("Filter control of type " + control.GetType().Name + " is undefined.  Define it in ODForm.AddFilterControl().");
            }

            _controlsFilter.Add(control);
        }
    }

    private bool IsControlValid(Control control)
    {
        return !Disposing && !IsDisposed && !control.IsDisposed;
    }

    private void Control_FilterCommitImmediate(object sender, EventArgs e)
    {
        if (!HasShown)
        {
            return;
        }

        _dateTimeLastModified = DateTime.Now;

        FilterActionCommit();
    }

    private void Control_FilterChange(object sender, EventArgs e)
    {
        if (!HasShown)
        {
            return;
        }

        var control = (Control) sender;
        if (!IsControlValid(control))
        {
            return;
        }

        if (IsDisposedOrClosed(this))
        {
            return;
        }

        _dateTimeLastModified = DateTime.Now;
        if (_threadFilter is null)
        {
            FormClosed += ODForm_FormClosed;

            _threadFilter = new ODThread(1, ThreadCheckFilterChangeCommitted)
            {
                Name = "ODFormFilterThread_" + Name
            };

            _threadFilter.Start(false);
        }
        else
        {
            _threadFilter.Wakeup();
        }
    }

    private void ThreadCheckFilterChangeCommitted(ODThread thread)
    {
        foreach (var control in _controlsFilter)
        {
            if (thread.HasQuit)
            {
                return;
            }

            if (!IsControlValid(control))
            {
                continue;
            }

            var diff = (DateTime.Now - _dateTimeLastModified).TotalMilliseconds;
            if (diff <= WaitFilterMs)
            {
                continue;
            }

            FilterActionCommit();

            thread.Wait(int.MaxValue);

            break;
        }
    }

    private void FilterActionCommit()
    {
        Exception ex = null;

        this.InvokeIfNotDisposed(() =>
        {
            try
            {
                _actionFilter?.Invoke();
            }
            catch (Exception e)
            {
                ex = e;
            }
        });

        ODException.TryThrowPreservedCallstack(ex);
    }
}