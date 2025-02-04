using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Chart;

public partial class ToothChartWrapper : UserControl
{
    private ToothChartOpenGL _toothChartOpenGl;
    private ToothChartDirectX _toothChartDirectX;
    private DrawingMode _drawMode;
    private ToothChartData _tcData;

    [Category("Action"), Description("Occurs when the mouse goes up ending a drawing segment.")]
    public event ToothChartDrawEventHandler SegmentDrawn;

    [Category("Action"), Description("Occurs when the mouse goes up committing tooth selection.")]
    public event ToothChartSelectionEventHandler ToothSelectionsChanged;

    public ToothChartWrapper()
    {
        _drawMode = DrawingMode.Simple2D;
        _tcData = new ToothChartData();

        InitializeComponent();

        ResetControls();
    }

    public DrawingMode DrawMode
    {
        get => _drawMode;
        set
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                return;
            }

            if (_drawMode == DrawingMode.DirectX && value != DrawingMode.DirectX)
            {
                _toothChartDirectX.Dispose();
                _toothChartDirectX = null;
            }

            try
            {
                _drawMode = value;
                ResetControls();
            }
            catch
            {
                _drawMode = DrawingMode.Simple2D;
                ResetControls();
            }
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ToothChartData TcData
    {
        get => _tcData;
        set
        {
            switch (_drawMode)
            {
                case DrawingMode.Simple2D:
                    toothChart2D.TcData = value;
                    break;

                case DrawingMode.DirectX:
                    _tcData?.CleanupDirectX();

                    _toothChartDirectX.TcData = value;
                    _toothChartDirectX.TcData.PrepareForDirectX(_toothChartDirectX.Device);
                    break;

                case DrawingMode.OpenGL:
                    _toothChartOpenGl.TcData = value;
                    break;
            }

            _tcData = value;
        }
    }

    public List<string> SelectedTeeth => _tcData.SelectedTeeth;

    [Browsable(false)]
    public Color ColorBackground
    {
        get => _tcData.ColorBackground;
        set
        {
            _tcData.ColorBackground = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    public Color ColorText
    {
        set
        {
            _tcData.ColorText = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    public Color ColorTextHighlight
    {
        set
        {
            _tcData.ColorTextHighlight = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    public Color ColorBackHighlight
    {
        set
        {
            _tcData.ColorBackHighlight = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    public bool UseHardware { get; set; }

    [Browsable(false)]
    public bool AutoFinish
    {
        get => _drawMode == DrawingMode.OpenGL && _toothChartOpenGl.AutoFinish;
        set
        {
            if (_drawMode == DrawingMode.OpenGL)
            {
                _toothChartOpenGl.AutoFinish = value;
            }
        }
    }

    [Browsable(false)]
    public int PreferredPixelFormatNumber { get; set; }

    [Browsable(false)]
    public ToothChartDirectX.DirectXDeviceFormat DeviceFormat { get; set; }

    [Browsable(false)]
    public CursorTool CursorTool
    {
        get => _tcData.CursorTool;
        set
        {
            _tcData.CursorTool = value;
            
            Cursor = _tcData.CursorTool switch
            {
                CursorTool.Pointer => Cursors.Default,
                CursorTool.Pen => new Cursor(GetType(), "Pen.cur"),
                CursorTool.Eraser => new Cursor(GetType(), "EraseCircle.cur"),
                CursorTool.ColorChanger => new Cursor(GetType(), "ColorChanger.cur"),
                _ => Cursor
            };
        }
    }

    [Browsable(false)]
    public Color ColorDrawing
    {
        set => _tcData.ColorDrawing = value;
    }

    [Browsable(false)]
    public bool PerioMode
    {
        get => _tcData.PerioMode;
        set
        {
            if (_drawMode != DrawingMode.DirectX && value)
            {
                throw new Exception("Only allowed in DirectX");
            }

            _tcData.PerioMode = value;

            Invalidate();
        }
    }

    [Browsable(false)]
    public Color ColorBleeding
    {
        set => _tcData.ColorBleeding = value;
    }

    [Browsable(false)]
    public Color ColorSuppuration
    {
        set => _tcData.ColorSuppuration = value;
    }

    [Browsable(false)]
    public Color ColorFurcations
    {
        set => _tcData.ColorFurcations = value;
    }

    [Browsable(false)]
    public Color ColorFurcationsRed
    {
        set => _tcData.ColorFurcationsRed = value;
    }

    [Browsable(false)]
    public Color ColorGingivalMargin
    {
        set => _tcData.ColorGingivalMargin = value;
    }

    [Browsable(false)]
    public Color ColorCals
    {
        set => _tcData.ColorCal = value;
    }

    [Browsable(false)]
    public Color ColorMgjs
    {
        set => _tcData.ColorMgj = value;
    }
    
    [Browsable(false)]
    public Color ColorProbing
    {
        set => _tcData.ColorProbing = value;
    }

    [Browsable(false)]
    public Color ColorProbingRed
    {
        set => _tcData.ColorProbingRed = value;
    }

    [Browsable(false)]
    public int RedLimitProbing
    {
        set => _tcData.RedLimitProbing = value;
    }

    [Browsable(false)]
    public int RedLimitFurcations
    {
        set => _tcData.RedLimitFurcations = value;
    }

    protected override void OnInvalidated(InvalidateEventArgs e)
    {
        base.OnInvalidated(e);

        switch (_drawMode)
        {
            case DrawingMode.Simple2D:
                toothChart2D.Invalidate();
                break;

            case DrawingMode.DirectX:
                _toothChartDirectX.Invalidate();
                break;

            case DrawingMode.OpenGL:
                _toothChartOpenGl.Invalidate();
                break;
        }
    }

    private void ResetControls()
    {
        Controls.Clear();

        switch (_drawMode)
        {
            case DrawingMode.Simple2D:
                toothChart2D = new ToothChart2D();
                toothChart2D.Dock = DockStyle.Fill;
                toothChart2D.Location = new Point(0, 0);
                toothChart2D.Name = "toothChart2D";
                toothChart2D.SegmentDrawn += toothChart_SegmentDrawn;
                toothChart2D.ToothSelectionsChanged += toothChart_ToothSelectionsChanged;
                toothChart2D.TcData = _tcData;
                toothChart2D.SuspendLayout();

                Controls.Add(toothChart2D);

                ResetTeeth();

                toothChart2D.InitializeGraphics();
                toothChart2D.ResumeLayout();
                break;
            
            case DrawingMode.DirectX:
            {
                var initialized = _toothChartDirectX is not null;
                if (!initialized)
                {
                    _toothChartDirectX = new ToothChartDirectX();
                }

                _toothChartDirectX.Dock = DockStyle.Fill;
                _toothChartDirectX.Location = new Point(0, 0);
                _toothChartDirectX.Name = "toothChartDirectX";
                _toothChartDirectX.SegmentDrawn += toothChart_SegmentDrawn;
                _toothChartDirectX.ToothSelectionsChanged += toothChart_ToothSelectionsChanged;
                _toothChartDirectX.TcData = _tcData;
                _toothChartDirectX.SuspendLayout();

                Controls.Add(_toothChartDirectX);

                ResetTeeth();

                _toothChartDirectX.deviceFormat = DeviceFormat;
                if (!initialized)
                {
                    _toothChartDirectX.InitializeGraphics();
                }

                _toothChartDirectX.ResumeLayout();
                break;
            }
            
            case DrawingMode.OpenGL:
                _toothChartOpenGl = new ToothChartOpenGL(UseHardware, PreferredPixelFormatNumber);

                PreferredPixelFormatNumber = _toothChartOpenGl.SelectedPixelFormatNumber;
                _toothChartOpenGl.Dock = DockStyle.Fill;
                _toothChartOpenGl.Location = new Point(0, 0);
                _toothChartOpenGl.Name = "toothChartOpenGL";
                _toothChartOpenGl.TcData = _tcData;
                _toothChartOpenGl.SegmentDrawn += toothChart_SegmentDrawn;
                _toothChartOpenGl.ToothSelectionsChanged += toothChart_ToothSelectionsChanged;
                _toothChartOpenGl.SuspendLayout();

                Controls.Add(_toothChartOpenGl);

                ResetTeeth();

                _toothChartOpenGl.InitializeGraphics();
                _toothChartOpenGl.ResumeLayout();
                break;
        }
    }

    public void ResetTeeth()
    {
        if (_tcData.ListToothGraphics.Count == 0)
        {
            _tcData.ListToothGraphics.Clear();
            
            ToothGraphic tooth;
            
            for (var i = 1; i <= 32; i++)
            {
                tooth = new ToothGraphic(i.ToString())
                {
                    Visible = true
                };
                
                _tcData.ListToothGraphics.Add(tooth);

                if (Tooth.PermToPri(i.ToString()) != "")
                {
                    tooth = new ToothGraphic(Tooth.PermToPri(i.ToString()))
                    {
                        Visible = false
                    };
                    
                    _tcData.ListToothGraphics.Add(tooth);
                }
            }

            tooth = new ToothGraphic("implant");
            
            _tcData.ListToothGraphics.Add(tooth);
        }
        else
        {
            for (var i = 0; i < _tcData.ListToothGraphics.Count; i++)
            {
                _tcData.ListToothGraphics[i].Reset();
            }
        }

        _tcData.SelectedTeeth.Clear();
        _tcData.DrawingSegmentList = [];
        _tcData.PointList = [];
        
        Invalidate();
    }
    
    public void MoveTooth(string toothId, float rotate, float tipM, float tipB, float shiftM, float shiftO, float shiftB)
    {
        if (!ToothGraphic.IsValidToothId(toothId))
        {
            return;
        }

        _tcData.ListToothGraphics[toothId].ShiftM += shiftM;
        _tcData.ListToothGraphics[toothId].ShiftO += shiftO;
        _tcData.ListToothGraphics[toothId].ShiftB += shiftB;
        _tcData.ListToothGraphics[toothId].Rotate += rotate;
        _tcData.ListToothGraphics[toothId].TipM += tipM;
        _tcData.ListToothGraphics[toothId].TipB += tipB;
        
        Invalidate();
    }
    
    public void SetPrimary(string toothId)
    {
        if (!ToothGraphic.IsValidToothId(toothId))
        {
            return;
        }

        if (Tooth.IsPrimary(toothId))
        {
            return;
        }

        _tcData.ListToothGraphics[toothId].ShiftO -= 12;
        
        if (ToothGraphic.IsValidToothId(Tooth.PermToPri(toothId)))
        {
            _tcData.ListToothGraphics[Tooth.PermToPri(toothId)].Visible = true;
            _tcData.ListToothGraphics[toothId].ShowPrimaryLetter = true;
        }
        
        switch (toothId)
        {
            case "21":
                _tcData.ListToothGraphics["J"].ShiftM += 0.5f;
                break;
            
            case "28":
                _tcData.ListToothGraphics["S"].ShiftM += 0.5f;
                break;
            
            case "4":
                _tcData.ListToothGraphics["A"].ShiftM -= 0.5f;
                _tcData.ListToothGraphics["1"].ShiftM -= 1;
                _tcData.ListToothGraphics["2"].ShiftM -= 1;
                _tcData.ListToothGraphics["3"].ShiftM -= 1;
                break;
            
            case "13":
                _tcData.ListToothGraphics["J"].ShiftM -= 0.5f;
                _tcData.ListToothGraphics["14"].ShiftM -= 1;
                _tcData.ListToothGraphics["15"].ShiftM -= 1;
                _tcData.ListToothGraphics["16"].ShiftM -= 1;
                break;
            
            case "20":
                _tcData.ListToothGraphics["K"].ShiftM -= 1.2f;
                _tcData.ListToothGraphics["17"].ShiftM -= 2.3f;
                _tcData.ListToothGraphics["18"].ShiftM -= 2.3f;
                _tcData.ListToothGraphics["19"].ShiftM -= 2.3f;
                break;
            
            case "29":
                _tcData.ListToothGraphics["T"].ShiftM -= 1.2f;
                _tcData.ListToothGraphics["30"].ShiftM -= 2.3f;
                _tcData.ListToothGraphics["31"].ShiftM -= 2.3f;
                _tcData.ListToothGraphics["32"].ShiftM -= 2.3f;
                break;
        }

        Invalidate();
    }
    
    public void SetCrown(string toothId, Color color)
    {
        if (!ToothGraphic.IsValidToothId(toothId))
        {
            return;
        }

        _tcData.ListToothGraphics[toothId].IsCrown = true;
        
        if (!_tcData.ListToothGraphics[toothId].Visible)
        {
            _tcData.ListToothGraphics[toothId].SetGroupVisibility(ToothGroupType.Cementum, false);
        }

        _tcData.ListToothGraphics[toothId].SetSurfaceColors("MODBLFIV", color);
        _tcData.ListToothGraphics[toothId].SetGroupColor(ToothGroupType.Enamel, color);
        _tcData.ListToothGraphics[toothId].SetGroupColor(ToothGroupType.EnamelF, color);
        
        Invalidate();
    }

    public void SetSurfaceColors(string toothId, string surfaces, Color color)
    {
        if (!ToothGraphic.IsValidToothId(toothId))
        {
            return;
        }

        _tcData.ListToothGraphics[toothId].SetSurfaceColors(surfaces, color);
        
        Invalidate();
    }
    
    public void SetMissing(string toothId)
    {
        if (!ToothGraphic.IsValidToothId(toothId))
        {
            return;
        }

        _tcData.ListToothGraphics[toothId].Visible = false;
        
        Invalidate();
    }
    
    public void SetHidden(string toothId)
    {
        if (!ToothGraphic.IsValidToothId(toothId))
        {
            return;
        }

        _tcData.ListToothGraphics[toothId].Visible = false;
        _tcData.ListToothGraphics[toothId].HideNumber = true;
        
        Invalidate();
    }
    
    public void SetPontic(string toothId, Color color)
    {
        if (!ToothGraphic.IsValidToothId(toothId))
        {
            return;
        }

        _tcData.ListToothGraphics[toothId].IsPontic = true;
        
        if (!_tcData.ListToothGraphics[toothId].Visible)
        {
            _tcData.ListToothGraphics[toothId].SetGroupVisibility(ToothGroupType.Cementum, false);
        }

        _tcData.ListToothGraphics[toothId].SetSurfaceColors("MODBLFIV", color);
        _tcData.ListToothGraphics[toothId].SetGroupColor(ToothGroupType.Enamel, color);
        _tcData.ListToothGraphics[toothId].SetGroupColor(ToothGroupType.EnamelF, color);
        
        Invalidate();
    }
    
    public void SetRct(string toothId, Color color)
    {
        if (!ToothGraphic.IsValidToothId(toothId))
        {
            return;
        }

        _tcData.ListToothGraphics[toothId].IsRct = true;
        _tcData.ListToothGraphics[toothId].ColorRct = color;
        
        Invalidate();
    }
    
    public void SetBigX(string toothId, Color color)
    {
        if (!ToothGraphic.IsValidToothId(toothId))
        {
            return;
        }

        _tcData.ListToothGraphics[toothId].DrawBigX = true;
        _tcData.ListToothGraphics[toothId].ColorX = color;
        
        Invalidate();
    }

    public void SetBu(string toothId, Color color)
    {
        if (!ToothGraphic.IsValidToothId(toothId))
        {
            return;
        }
        
        _tcData.ListToothGraphics[toothId].SetGroupVisibility(ToothGroupType.Buildup, true);
        _tcData.ListToothGraphics[toothId].SetGroupColor(ToothGroupType.Buildup, color);
        
        Invalidate();
    }

    public void SetImplant(string toothId, Color color)
    {
        if (!ToothGraphic.IsValidToothId(toothId))
        {
            return;
        }

        _tcData.ListToothGraphics[toothId].IsImplant = true;
        _tcData.ListToothGraphics[toothId].ColorImplant = color;
        
        Invalidate();
    }

    public void SetSealant(string toothId, Color color)
    {
        if (!ToothGraphic.IsValidToothId(toothId))
        {
            return;
        }

        _tcData.ListToothGraphics[toothId].IsSealant = true;
        _tcData.ListToothGraphics[toothId].ColorSealant = color;
        
        Invalidate();
    }

    public void SetVeneer(string toothId, Color color)
    {
        if (!ToothGraphic.IsValidToothId(toothId))
        {
            return;
        }

        _tcData.ListToothGraphics[toothId].SetSurfaceColors("BFV", color);
        _tcData.ListToothGraphics[toothId].SetGroupColor(ToothGroupType.EnamelF, color);
        _tcData.ListToothGraphics[toothId].SetGroupColor(ToothGroupType.DF, color);
        _tcData.ListToothGraphics[toothId].SetGroupColor(ToothGroupType.MF, color);
        _tcData.ListToothGraphics[toothId].SetGroupColor(ToothGroupType.IF, color);
        
        Invalidate();
    }
    
    public void SetWatch(string toothId, Color color)
    {
        if (!ToothGraphic.IsValidToothId(toothId))
        {
            return;
        }

        _tcData.ListToothGraphics[toothId].Watch = true;
        _tcData.ListToothGraphics[toothId].ColorWatch = color;
        
        Invalidate();
    }
    
    public void AddDrawingSegment(ToothInitial drawingSegment)
    {
        var alreadyAdded = false;
        
        foreach (var toothInitial in _tcData.DrawingSegmentList)
        {
            if (toothInitial.DrawingSegment != drawingSegment.DrawingSegment)
            {
                continue;
            }
            
            alreadyAdded = true;
            break;
        }

        if (!alreadyAdded)
        {
            _tcData.DrawingSegmentList.Add(drawingSegment);
        }

        Invalidate();
    }

    public Bitmap GetBitmap()
    {
        return _drawMode switch
        {
            DrawingMode.Simple2D => toothChart2D.GetBitmap(),
            DrawingMode.OpenGL => _toothChartOpenGl.GetBitmap(),
            _ => _drawMode == DrawingMode.DirectX ? _toothChartDirectX.GetBitmap() : null
        };
    }

    public void SetToothNumberingNomenclature(ToothNumberingNomenclature nomenclature)
    {
        _tcData.ToothNumberingNomenclature = nomenclature;
        
        Invalidate();
    }

    public void SetMobility(string toothId, string mobility, Color color)
    {
        if (!ToothGraphic.IsValidToothId(toothId))
        {
            return;
        }

        _tcData.ListToothGraphics[toothId].Mobility = mobility;
        _tcData.ListToothGraphics[toothId].ColorMobility = color;
        
        Invalidate();
    }

    public void AddPerioMeasure(int intTooth, PerioSequenceType sequenceType, int mb, int b, int db, int ml, int l, int dl)
    {
        _tcData.ListPerioMeasure.Add(new PerioMeasure
        {
            MBvalue = mb,
            Bvalue = b,
            DBvalue = db,
            MLvalue = ml,
            Lvalue = l,
            DLvalue = dl,
            IntTooth = intTooth,
            SequenceType = sequenceType
        });
    }

    public void AddPerioMeasure(PerioMeasure pm)
    {
        _tcData.ListPerioMeasure.Add(pm);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        _tcData.SizeControl = Size;

        Invalidate();

        if (_drawMode == DrawingMode.DirectX)
        {
            _toothChartDirectX.SetSize(Size);
        }
    }

    public void SetSelected(string toothId, bool setValue)
    {
        _tcData.SetSelected(toothId, setValue);

        Invalidate();
    }

    protected void OnSegmentDrawn(string drawingSegment)
    {
        var toothChartDrawEventArgs = new ToothChartDrawEventArgs(drawingSegment);

        SegmentDrawn?.Invoke(this, toothChartDrawEventArgs);
    }

    protected void OnToothSelectionsChanged()
    {
        ToothSelectionsChanged?.Invoke(this);
    }

    private void toothChart_SegmentDrawn(object sender, ToothChartDrawEventArgs e)
    {
        OnSegmentDrawn(e.DrawingSegement);
    }

    private void toothChart_ToothSelectionsChanged(object sender)
    {
        OnToothSelectionsChanged();
    }
}

public enum CursorTool
{
    Pointer,
    Pen,
    Eraser,
    ColorChanger,
    MoveText
}

public delegate void ToothChartDrawEventHandler(object sender, ToothChartDrawEventArgs e);

public delegate void ToothChartSelectionEventHandler(object sender);

public class ToothChartDrawEventArgs(string drawingSeg)
{
    public string DrawingSegement => drawingSeg;
}