using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;
using SharpDX.Direct3D9;

namespace OpenDental.Chart;

public partial class ToothChartDirectX : Control
{
    public Device Device;
    public DirectXDeviceFormat deviceFormat = null;
    public ToothChartData TcData;
    
    private static Direct3D _d3d = new Direct3D();
    private SharpDX.Color color_specular_normal;
    private SharpDX.Color color_specular_cementum;
    private SharpDX.Direct3D9.Font fontX;
    private SharpDX.Direct3D9.Font fontXSealant;
    private SharpDX.Direct3D9.Font fontXWatch;
    private SharpDX.Direct3D9.Font fontXWatchBig;
    private string hotTooth;
    private string hotToothOld;
    private bool _isMouseDown;
    private List<string> _listSelectedTeethOld = [];
    private float specularSharpness;
    
    public ToothChartDirectX()
    {
        InitializeComponent();
    }
    
    protected void OnSegmentDrawn(string drawingSegment)
    {
        SegmentDrawn?.Invoke(this, new ToothChartDrawEventArgs(drawingSegment));
    }

    [Category("OD"), Description("Occurs on mouse up that ends a drawing segment.")]
    public event ToothChartDrawEventHandler? SegmentDrawn;

    protected void OnToothSelectionsChanged()
    {
        ToothSelectionsChanged?.Invoke(this);
    }

    [Category("OD"), Description("Occurs on mouse up that commits tooth selection.")]
    public event ToothChartSelectionEventHandler ToothSelectionsChanged;
    
    public void CleanupDirectX()
    {
        if (fontXSealant != null)
        {
            fontXSealant.Dispose();
            fontXSealant = null;
        }

        if (fontXWatch != null)
        {
            fontXWatch.Dispose();
            fontXWatch = null;
        }

        if (fontXWatchBig != null)
        {
            fontXWatchBig.Dispose();
            fontXWatchBig = null;
        }

        if (fontX != null)
        {
            fontX.Dispose();
            fontX = null;
        }

        TcData.CleanupDirectX();
    }

    public static int GetFormatBitCount(string formatName)
    {
        var format = (Format) Enum.Parse(typeof(Format), formatName);
        var eightBitFormats = new Format[]
        {
            Format.A4L4, Format.A8, Format.L8, Format.P8, Format.R3G3B2,
        };
        var sixteenBitFormats = new Format[]
        {
            Format.A1R5G5B5, Format.A4R4G4B4, Format.A8L8, Format.A8P8, Format.A8R3G3B2, Format.D15S1, Format.D16,
            Format.D16Lockable, Format.L16, Format.L6V5U5, Format.R16F, Format.R5G6B5, Format.V8U8, Format.X1R5G5B5,
            Format.X4R4G4B4,
        };
        var twentyFourBitFormats = new Format[]
        {
            Format.R8G8B8,
        };
        var thirtyTwoBitFormats = new Format[]
        {
            Format.A2B10G10R10, Format.A2R10G10B10, Format.A2W10V10U10, Format.A8B8G8R8, Format.A8R8G8B8,
            Format.D24S8, Format.D24SingleS8, Format.D24X4S4, Format.D24X8, Format.D32, Format.D32SingleLockable,
            Format.G16R16, Format.G16R16F, Format.G8R8_G8B8, Format.Q8W8V8U8, Format.R32F, Format.R8G8_B8G8, Format.V16U16,
            Format.X8B8G8R8, Format.X8L8V8U8, Format.X8R8G8B8,
        };
        var sixtyFourBitFormats = new Format[]
        {
            Format.A16B16G16R16, Format.A16B16G16R16F, Format.G32R32F, Format.Q16W16V16U16,
        };
        var oneHundredTwentyEightBitFormats = new Format[]
        {
            Format.A32B32G32R32F,
        };
        for (var i = 0; i < eightBitFormats.Length; i++)
        {
            if (format == eightBitFormats[i])
            {
                return 8;
            }
        }

        for (var i = 0; i < sixteenBitFormats.Length; i++)
        {
            if (format == sixteenBitFormats[i])
            {
                return 16;
            }
        }

        for (var i = 0; i < twentyFourBitFormats.Length; i++)
        {
            if (format == twentyFourBitFormats[i])
            {
                return 24;
            }
        }

        for (var i = 0; i < thirtyTwoBitFormats.Length; i++)
        {
            if (format == thirtyTwoBitFormats[i])
            {
                return 32;
            }
        }

        for (var i = 0; i < sixtyFourBitFormats.Length; i++)
        {
            if (format == sixtyFourBitFormats[i])
            {
                return 64;
            }
        }

        for (var i = 0; i < oneHundredTwentyEightBitFormats.Length; i++)
        {
            if (format == oneHundredTwentyEightBitFormats[i])
            {
                return 128;
            }
        }

        //Format has not yet been accounted for.
        //Format.CxV8U8,Format.Dxt1,Format.Dxt2,Format.Dxt3,Format.Dxt4,Format.Dxt5,Format.Multi2Argb8,
        //Format.Unknown,Format.Uyvy,Format.VertexData,Format.Yuy2
        return 0;
    }

    public static DirectXDeviceFormat[] GetStandardDeviceFormats()
    {
        var deviceTypes = new DeviceType[] {DeviceType.Hardware, DeviceType.Reference};
        var backBufferFormats = new Format[]
        {
            //16bit formats
            Format.R5G6B5, Format.A1R5G5B5, Format.X1R5G5B5,
            //24bit formats
            Format.R8G8B8,
            //32bit formats
            Format.A8R8G8B8, Format.X8R8G8B8, Format.A2R10G10B10, Format.A2B10G10R10, Format.A8B8G8R8, Format.G8R8_G8B8, Format.R8G8_B8G8, Format.X8B8G8R8,
            //64bit formats
            Format.A16B16G16R16,
        };
        var depthFormats = new Format[]
        {
            Format.D16, Format.D32, Format.D24X8, Format.D15S1, Format.D24S8,
            Format.D24SingleS8, Format.D24X4S4, Format.L16
        };
        return GetPossibleDeviceFormats(deviceTypes, backBufferFormats, true, depthFormats);
    }

    public void InitializeGraphics()
    {
        if (deviceFormat != null)
        {
            Device = deviceFormat.CreateDevice(this);
            if (Device == null)
            {
                //this happens when you turn off a monitor and the chart is moved by Windows to the other monitor.
                return;
            }

            CleanupDirectX();
            PrepareForDirectX();
        }
    }

    public void Reinitialize()
    {
        CleanupDirectX();
        if (Device != null)
        {
            if (!Device.IsDisposed)
            {
                Device.Dispose();
            }

            Device = null;
        }

        InitializeGraphics();
    }

    public void SetSize(Size size)
    {
        Size = size;
        Reinitialize();
    }
    
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        //Do nothing to eliminate flicker. 
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        Invalidate(); //Force the control to redraw. 
    }
    
    private static DirectXDeviceFormat[] GetPossibleDeviceFormats(DeviceType[] deviceTypes, Format[] backBufferFormats, bool windowed, Format[] depthStencilFormats)
    {
        var possibleFormats = new List<DirectXDeviceFormat>();
        for (var a = 0; a < _d3d.Adapters.Count; a++)
        {
            var adapter = _d3d.Adapters[a];
            for (var t = 0; t < deviceTypes.Length; t++)
            {
                var deviceType = deviceTypes[t];
                for (var b = 0; b < backBufferFormats.Length; b++)
                {
                    var backBufferFormat = backBufferFormats[b];
                    //We require the display buffer to have the same format as the back buffer,
                    //so that we know that a back buffer flip will work.
                    foreach (var displayMode in adapter.GetDisplayModes(backBufferFormat))
                    {
                        if (_d3d.CheckDeviceType(
                                adapter.Adapter,
                                deviceType,
                                displayMode.Format,
                                displayMode.Format,
                                windowed))
                        {
                            //Now make sure the depth buffer meets one of the required formats.
                            foreach (var depthStencilFormat in depthStencilFormats)
                            {
                                if (_d3d.CheckDeviceFormat(adapter.Adapter, deviceType, displayMode.Format, Usage.DepthStencil, ResourceType.Surface, depthStencilFormat))
                                {
                                    if (_d3d.CheckDepthStencilMatch(adapter.Adapter, deviceType, displayMode.Format, displayMode.Format, depthStencilFormat))
                                    {
                                        var format = new DirectXDeviceFormat
                                        {
                                            adapterIndex = adapter.Adapter,
                                            IsHardware = (deviceType == DeviceType.Hardware),
                                            createFlags = CreateFlags.SoftwareVertexProcessing,
                                            //format.createFlags=CreateFlags.FpuPreserve;
                                            DepthStencilFormat = depthStencilFormat.ToString(),
                                            BackBufferFormat = backBufferFormat.ToString(),
                                            MultiSampleCount = 0 //None
                                        };
                                        //Anti-aliasing/multi-sampling appears to only work on hardware devices.
                                        if (deviceType == DeviceType.Hardware)
                                        {
                                            format.createFlags = CreateFlags.HardwareVertexProcessing;
                                            var caps = _d3d.GetDeviceCaps(adapter.Adapter, deviceType);
                                            //An multisampling/anti-aliasing method must be supported both on the back buffer and with the depth buffer in order for it to be usable.
                                            if (IsDeviceMultiSampleOK(adapter.Adapter, deviceType, depthStencilFormat, backBufferFormat, MultisampleType.FourSamples, windowed))
                                            {
                                                format.MultiSampleCount = 4; //FourSamples
                                            }
                                            else if (IsDeviceMultiSampleOK(adapter.Adapter, deviceType, depthStencilFormat, backBufferFormat, MultisampleType.ThreeSamples, windowed))
                                            {
                                                format.MultiSampleCount = 3; //ThreeSamples
                                            }
                                            else if (IsDeviceMultiSampleOK(adapter.Adapter, deviceType, depthStencilFormat, backBufferFormat, MultisampleType.TwoSamples, windowed))
                                            {
                                                format.MultiSampleCount = 2; //TwoSamples
                                            }
                                        }

                                        if (possibleFormats.IndexOf(format) > -1)
                                        {
                                            //Skip duplicate formats.
                                            continue;
                                        }

                                        possibleFormats.Add(format);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return possibleFormats.ToArray();
    }

    private static bool IsDeviceMultiSampleOK(int adapterIndex, DeviceType deviceType, Format depthFormat, Format backbufferFormat, MultisampleType multisampleType, bool windowed)
    {
        var qualityLevels = 0;
        var result = new SharpDX.Result();
        //Verify that the render target surface supports the given multisample type
        if (_d3d.CheckDeviceMultisampleType(adapterIndex, deviceType, backbufferFormat, windowed, multisampleType, out qualityLevels, out result))
        {
            //Verify that the depth stencil surface supports the given multisample type
            if (_d3d.CheckDeviceMultisampleType(adapterIndex, deviceType, depthFormat, windowed, multisampleType, out qualityLevels, out result))
            {
                return result.Success;
            }
        }

        return false;
    }

    private void PrepareForDirectX()
    {
        //Required for calculating font background rectangle size in ToothChartData.
        new System.Drawing.Font("Arial", 9f);
        fontX = new SharpDX.Direct3D9.Font(Device,
            (int) Math.Round((float) (14 * Math.Sqrt(TcData.PixelScaleRatio))),
            (int) Math.Round((float) (5 * Math.Sqrt(TcData.PixelScaleRatio))),
            FontWeight.Normal, 1, false, FontCharacterSet.Ansi, FontPrecision.Device,
            FontQuality.ClearType, FontPitchAndFamily.Default, "Arial");
        fontXSealant = new SharpDX.Direct3D9.Font(Device,
            (int) Math.Round(25 * TcData.PixelScaleRatio),
            (int) Math.Round(9 * TcData.PixelScaleRatio),
            FontWeight.Regular, 1, false, FontCharacterSet.Ansi, FontPrecision.Device,
            FontQuality.ClearType, FontPitchAndFamily.Default, "Arial");
        fontXWatch = new SharpDX.Direct3D9.Font(Device,
            (int) Math.Round(15 * TcData.PixelScaleRatio),
            (int) Math.Round(6 * TcData.PixelScaleRatio),
            FontWeight.Regular, 1, false, FontCharacterSet.Ansi, FontPrecision.Device,
            FontQuality.ClearType, FontPitchAndFamily.Default, "Arial");
        fontXWatchBig = new SharpDX.Direct3D9.Font(Device,
            (int) Math.Round(19 * TcData.PixelScaleRatio),
            (int) Math.Round(7.4 * TcData.PixelScaleRatio),
            FontWeight.UltraBold, 1, false, FontCharacterSet.Ansi, FontPrecision.Device,
            FontQuality.ClearType, FontPitchAndFamily.Default, "Times New Roman");
        TcData.PrepareForDirectX(Device);
    }

    private void ReinitailizeIfNeeded()
    {
        //https://msdn.microsoft.com/en-us/library/windows/desktop/bb324479(v=vs.85).aspx
        try
        {
            var result = Device.TestCooperativeLevel();
            if (result.Success)
            {
                return;
            }
        }
        catch
        {
            //The device can be null if the previous attempt to reinitialize failed.
        }

        //Either the device is null or was lost.
        //First attempt to Reset the device because Resetting has less overhead than recreating the device.
        try
        {
            CleanupDirectX();
            Device.Reset(deviceFormat.GetPresentParameters(this));
            PrepareForDirectX();
            Invalidate();
            return;
        }
        catch
        {
        }

        //Resetting the device failed.  Try to recreate the device (more overhead).
        try
        {
            Reinitialize();
            Invalidate();
        }
        catch
        {
            //This can happen in uncommon cases when the user unlocks their computer and left the chart open, or
            //when someone RDPs into the computer in question when the Chart module is already open (limited memory resources, RDP is a memory hog).
            //We swallow the error here so that the user is not kicked out of the program.  Better to have the red X over the chart then
            //to prevent the user from continuing with their work.  Additionally, some reasons this would fail are temporary and 
            //ignoring the error now gives us time to try again in the near future.
        }
    }
    
    private SharpDX.Matrix ScreenSpaceMatrix()
    {
        SharpDX.Matrix matWorld = Device.GetTransform(TransformState.World);
        SharpDX.Matrix matView = Device.GetTransform(TransformState.View);
        SharpDX.Matrix matProj = Device.GetTransform(TransformState.Projection);
        return matWorld * matView * matProj;
    }
    
    protected override void OnPaint(PaintEventArgs pe)
    {
        //Color backColor=Color.FromArgb(150,145,152);
        if (Device == null)
        {
            //When no rendering context has been set, simply display the control
            //as a black rectangle. This will make the control draw as a blank
            //rectangle when in the designer. 
            pe.Graphics.FillRectangle(new SolidBrush(TcData.ColorBackground), new Rectangle(0, 0, Width, Height));
            return;
        }

        //When the OS user is switched then switched back or when coming back from stand by mode, the OS calls the OnPaint function
        //even before it calls the OnDeviceLost() function. When this happens, the render will fail
        //because the DirectX device is not in a valid state to be rendered to. Before the exception is returned from Render(), 
        //a call is made by the OS to OnDeviceLost(), which sets deviceLost=true (when the OnPaint() function begins, deviceLost=false)
        //so that further rendering will not occur with the device in its invalid state.
        try
        {
            Render();
        }
        catch
        {
            //Rendering failed because our device is invalid. Reinitialize the device and cached objects and force the control to be rerendered.
            ReinitailizeIfNeeded();
        }
    }

    ///<summary>The isUsingPen flag is only used for regular tooth chart rendering, not for perio charting. If isUsingPen is true, then the scene is not cleared and the teeth are not redrawn.  The tooth rendering is the slowest part of the scene rendering, and the pen drawings are always above the teeth in the scene.  For some customers with Windows 8 and Windows 10, the scene draws as slow as 1 frame per second, which causes the pen drawing to look very jagged if we redraw the teeth.</summary>
    protected void Render(bool isUsingPen = false)
    {
        //Set the view and projection matricies for the camera.
        var heightProj = TcData.SizeOriginalProjection.Height;
        var widthProj = TcData.SizeOriginalProjection.Width;
        if (TcData.IsWide)
        {
            widthProj = heightProj * Width / Height;
        }
        else
        {
            //tall
            heightProj = widthProj * Height / Width;
        }

        Device.SetTransform(TransformState.Projection, SharpDX.Matrix.OrthoLH(widthProj, heightProj, 0, 1000.0f));
        Device.SetTransform(TransformState.World, SharpDX.Matrix.Identity);
        //viewport transformation not used. Default is to fill entire control.
        Device.SetRenderState(RenderState.CullMode, Cull.None); //Do not cull triangles. Our triangles are too small for this feature to work reliably.
        Device.SetRenderState(RenderState.ZEnable, true); //Depth buffer enabled.
        Device.SetRenderState(RenderState.ZFunc, Compare.Less); //Pixels being drawn behind pixels already on screen will be culled.
        //Blend mode settings.
        Device.SetRenderState(RenderState.AlphaBlendEnable, false); //Disabled
        //Lighting settings
        Device.SetRenderState(RenderState.Lighting, true);
        Device.SetRenderState(RenderState.SpecularEnable, true);
        Device.SetRenderState(RenderState.SpecularMaterialSource, ColorSource.Material);
        var ambI = .4f; //.2f;//Darker for testing
        var difI = .6f; //.4f;//Darker for testing
        var specI = .8f; //.2f;//Had to turn specular down to avoid bleedout.
        if (TcData.PerioMode)
        {
            //Want perio teeth to be more washed out so that other graphics are more visible.
            ambI = .6f;
            difI = .4f;
        }

        //I think we're going to need to eventually use shaders to get our pinpoint reflections.
        //Set properties for light 0. Diffuse light.
        var light = new Light
        {
            Type = LightType.Directional,
            Ambient = new SharpDX.Color(ambI, ambI, ambI, 1),
            Diffuse = new SharpDX.Color(difI, difI, difI, 1),
            Specular = new SharpDX.Color(specI, specI, specI, 1),
            Direction = new SharpDX.Vector3(0.5f, -0.2f, 1f)
        };
        Device.SetLight(0, ref light);
        Device.EnableLight(0, true);
        //Material settings
        var specNorm = 1f;
        var specCem = .1f;
        //Also, see DrawTooth for the specular color used for enamel.
        color_specular_normal = new SharpDX.Color(specNorm);
        color_specular_cementum = new SharpDX.Color(specCem);
        specularSharpness = 70f; //70f;//Not the same as in OpenGL. No maximum value. Smaller number means light is more spread out.
        Device.VertexFormat = ToothChartVertexPosNormX.Format;
        //Draw
        if (TcData.PerioMode)
        {
            DrawScenePerio();
        }
        else
        {
            DrawScene(isUsingPen);
        }
    }

    ///<summary>If isUsingPen is true, then the scene is not cleared and the teeth are not redrawn.  The tooth rendering is the slowest part of the scene rendering, and the pen drawings are always above the teeth in the scene.  For some customers with Windows 8 and Windows 10, the scene draws as slow as 1 frame per second, which causes the pen drawing to look very jagged if we redraw the teeth.</summary>
    private void DrawScene(bool isUsingPen)
    {
        if (!isUsingPen)
        {
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorToRaw(TcData.ColorBackground), 1.0f, 0);
        }

        Device.BeginScene();
        //The Z values between OpenGL and DirectX are negated (the axis runs in the opposite direction).
        //We reflect that difference here by negating the z values for all coordinates.
        var defOrient = SharpDX.Matrix.Identity;
        defOrient.ScaleVector = new SharpDX.Vector3(1f, 1f, -1f);
        //We make sure to move all teeth forward a large step so that specular lighting will calculate properly.
        //This step does not affect the tooth locations on the screen because changes in z position for a tooth
        //does not affect position in orthographic projections.
        var trans = SharpDX.Matrix.Identity;
        trans.TranslationVector = new SharpDX.Vector3(0f, 0f, 400f);
        defOrient = defOrient * trans;
        if (!isUsingPen)
        {
            //frameBeginTime=DateTime.Now;
            for (var t = 0; t < TcData.ListToothGraphics.Count; t++)
            {
                //loop through each tooth
                if (TcData.ListToothGraphics[t].ToothId == "implant")
                {
                    //this is not an actual tooth.
                    continue;
                }

                DrawFacialView(TcData.ListToothGraphics[t], defOrient);
                DrawOcclusalView(TcData.ListToothGraphics[t], defOrient);
            }

            //frameEndTime=DateTime.Now;
            DrawWatches(defOrient);
        }

        DrawDrawingSegments();
        DrawNumbersAndLines(); //Numbers and center lines are top-most.
        Device.EndScene();
        Device.Present();
    }

    private void DrawScenePerio()
    {
        Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorToRaw(TcData.ColorBackground), 1.0f, 0);
        Device.BeginScene();
        //The distance from y=0 to the tip of the highest root in the upper-most maxillary row.
        const float maxYmm = 63f;
        //The baseY is used to force the perio chart to start as near to the top of the control as possible.
        //This is helpful with the perio legend, because it allows one to increase the height of this control
        //in order to give space for the perio legend to display at the bottom.
        var baseY = Height / (2f * TcData.ScaleMmToPix) - maxYmm;
        var defOrient = SharpDX.Matrix.Scaling(1f, 1f, -1f) * SharpDX.Matrix.Translation(0, baseY, 400f);
        var maxillaryTeeth = new List<ToothGraphic>();
        var mandibleTeeth = new List<ToothGraphic>();
        for (var t = 0; t < TcData.ListToothGraphics.Count; t++)
        {
            //loop through each tooth
            if (TcData.ListToothGraphics[t].ToothId == "implant")
            {
                //this is not an actual tooth.
                continue;
            }

            if (ToothGraphic.IsMaxillary(TcData.ListToothGraphics[t].ToothId))
            {
                maxillaryTeeth.Add(TcData.ListToothGraphics[t]);
            }
            else
            {
                mandibleTeeth.Add(TcData.ListToothGraphics[t]);
            }
        }

        //Draw the maxillary upper-most row.
        var maxillaryUpperRowMat = SharpDX.Matrix.Translation(0, 45f, 0) * defOrient;
        DrawPerioRow(maxillaryTeeth, maxillaryUpperRowMat, true, false);
        //Draw the maxillary lower-most row with teeth which have negated z values.
        var maxillaryLowerRowMat = SharpDX.Matrix.Scaling(1f, 1f, -1f) * SharpDX.Matrix.Translation(0, 12f, 0) * defOrient;
        DrawPerioRow(maxillaryTeeth, maxillaryLowerRowMat, true, true);
        //Draw the mandible upper-most row with teeth which have negated z values.
        var mandibleUpperRowMat = SharpDX.Matrix.Scaling(1f, 1f, -1f) * SharpDX.Matrix.Translation(0, -12f, 0) * defOrient;
        DrawPerioRow(mandibleTeeth, mandibleUpperRowMat, false, true);
        //Draw the mandible lower-most row.
        var mandibleLowerRowMat = SharpDX.Matrix.Translation(0, -45f, 0) * defOrient;
        DrawPerioRow(mandibleTeeth, mandibleLowerRowMat, false, false);
        DrawNumbersAndLinesPerio(baseY); //Numbers and center lines are top-most.			
        DrawPerioLegend(-60f, baseY - 63f);
        Device.EndScene();
        Device.Present();
    }

    private void DrawPerioRow(List<ToothGraphic> toothGraphics, SharpDX.Matrix orientation, bool maxillary, bool lingual)
    {
        for (var t = 0; t < toothGraphics.Count; t++)
        {
            Device.SetRenderState(RenderState.ZEnable, true);
            Device.SetTransform(TransformState.World, SharpDX.Matrix.Translation(GetTransX(toothGraphics[t].ToothId), 0, 0) * orientation);
            if (toothGraphics[t].Visible
                || (toothGraphics[t].IsCrown && toothGraphics[t].IsImplant)
                || toothGraphics[t].IsPontic)
            {
                DrawTooth(toothGraphics[t]);
            }

            if (toothGraphics[t].IsImplant)
            {
                DrawImplant(toothGraphics[t]);
            }
        }

        Device.SetRenderState(RenderState.ZEnable, false);
        float sign = maxillary ? 1 : -1;
        //The device.Transform.World matrix must be set before calling Line.Begin()
        //or else your lines end up in the wrong location! This is odd behavior, since you *MUST*
        //pass in your screen matrix when you call Line.DrawTransform(). This must be a DirectX bug.
        Device.SetTransform(TransformState.World, orientation);
        const float leftX = -65f;
        const float rightX = 65f;
        const int mmMax = 9;
        //Draw the horizontal line at 0
        DrawExtendedLineStrip(0, false, TcData.ColorText, 0.3f, new SharpDX.Vector3(leftX, 0, 0), new SharpDX.Vector3(rightX, 0, 0));
        //Draw the other horizontal lines
        for (var mm = 3; mm <= mmMax; mm += 3)
        {
            DrawExtendedLineStrip(0, false, Color.Gray, 0.16f, new SharpDX.Vector3(leftX, sign * mm, 0), new SharpDX.Vector3(rightX, sign * mm, 0));
        }

        //Separate loop than drawing the teeth, so that we don't need to change
        //the device.RenderState.ZBufferEnable state variable for every
        //tooth. This will help the speed a little.
        for (var t = 0; t < toothGraphics.Count; t++)
        {
            var mobTextSize = MeasureStringMm(toothGraphics[t].Mobility);
            if (!lingual)
            {
                //Draw mobility numbers.
                Device.SetTransform(TransformState.World, SharpDX.Matrix.Translation(GetTransX(toothGraphics[t].ToothId) - mobTextSize.Width / 2f, 0, 0) * orientation);
                PrintString(toothGraphics[t].Mobility, 0, maxillary ? -2.5f : 5.5f, 0, toothGraphics[t].ColorMobility, fontX);
            }

            Device.SetTransform(TransformState.World, SharpDX.Matrix.Translation(GetTransX(toothGraphics[t].ToothId), 0, 0) * orientation);
            var intTooth = ToothGraphic.IdToInt(toothGraphics[t].ToothId);
            if (lingual)
            {
                //Draw furcations at each tooth site if furcation present.
                DrawFurcationTriangle(intTooth, PerioSurf.DL, maxillary);
                DrawFurcationTriangle(intTooth, PerioSurf.L, maxillary);
                DrawFurcationTriangle(intTooth, PerioSurf.ML, maxillary);
                //Draw probing bars.
                DrawProbingBar(intTooth, PerioSurf.DL);
                DrawProbingBar(intTooth, PerioSurf.L);
                DrawProbingBar(intTooth, PerioSurf.ML);
                //Draw bleeding droplets.
                DrawDroplet(intTooth, PerioSurf.DL, true, maxillary);
                DrawDroplet(intTooth, PerioSurf.L, true, maxillary);
                DrawDroplet(intTooth, PerioSurf.ML, true, maxillary);
                //Draw suppuration droplets.
                DrawDroplet(intTooth, PerioSurf.DL, false, maxillary);
                DrawDroplet(intTooth, PerioSurf.L, false, maxillary);
                DrawDroplet(intTooth, PerioSurf.ML, false, maxillary);
            }
            else
            {
                //buccal
                //Draw furcations at each tooth site if furcation present.
                DrawFurcationTriangle(intTooth, PerioSurf.DB, maxillary);
                DrawFurcationTriangle(intTooth, PerioSurf.B, maxillary);
                DrawFurcationTriangle(intTooth, PerioSurf.MB, maxillary);
                //Draw probing bars.
                DrawProbingBar(intTooth, PerioSurf.DB);
                DrawProbingBar(intTooth, PerioSurf.B);
                DrawProbingBar(intTooth, PerioSurf.MB);
                //Draw bleeding droplets.
                DrawDroplet(intTooth, PerioSurf.DB, true, maxillary);
                DrawDroplet(intTooth, PerioSurf.B, true, maxillary);
                DrawDroplet(intTooth, PerioSurf.MB, true, maxillary);
                //Draw suppuration droplets.
                DrawDroplet(intTooth, PerioSurf.DB, false, maxillary);
                DrawDroplet(intTooth, PerioSurf.B, false, maxillary);
                DrawDroplet(intTooth, PerioSurf.MB, false, maxillary);
            }
        }

        Device.SetTransform(TransformState.World, orientation);
        //Draw GM lines.
        var gmLines = TcData.GetHorizontalLines(PerioSequenceType.GingMargin, maxillary, !lingual);
        for (var i = 0; i < gmLines.Count; i++)
        {
            var gmLineV = LineSimpleToVector3List(gmLines[i]);
            var gmLineA = gmLineV.ToArray();
            DrawExtendedLineStrip(0.04f, true, TcData.ColorGingivalMargin, 0.4f, gmLineA);
        }

        //Draw CAL lines.
        var calLines = TcData.GetHorizontalLines(PerioSequenceType.CAL, maxillary, !lingual);
        for (var i = 0; i < calLines.Count; i++)
        {
            var calLineV = LineSimpleToVector3List(calLines[i]);
            var calLineA = calLineV.ToArray();
            DrawExtendedLineStrip(0.04f, true, TcData.ColorCal, 0.4f, calLineA);
        }

        //Draw MGJ lines.
        var mgjLines = TcData.GetHorizontalLines(PerioSequenceType.MGJ, maxillary, !lingual);
        for (var i = 0; i < mgjLines.Count; i++)
        {
            var mgjLineV = LineSimpleToVector3List(mgjLines[i]);
            var mgjLineA = mgjLineV.ToArray();
            DrawExtendedLineStrip(0.04f, true, TcData.ColorMgj, 0.4f, mgjLineA);
        }
    }

    private void DrawDroplet(int intTooth, PerioSurf surf, bool isBleeding, bool maxillary)
    {
        var dropletPos = TcData.GetBleedingOrSuppuration(intTooth, surf, isBleeding);
        if (dropletPos.X == 0 && dropletPos.Y == 0)
        {
            return; //No droplet to draw at this site.
        }

        SharpDX.Matrix saveWorldMat = Device.GetTransform(TransformState.World);
        Device.SetTransform(TransformState.World, SharpDX.Matrix.Translation(dropletPos.X, dropletPos.Y, 0) * Device.GetTransform(TransformState.World));
        if (!maxillary)
        {
            //When the droplet is for a mandibular tooth, flip the droplet about the x-axis (negate y values).
            Device.SetTransform(TransformState.World, SharpDX.Matrix.Scaling(1f, -1f, 1f) * Device.GetTransform(TransformState.World));
        }

        var dropletColor = TcData.ColorSuppuration;
        if (isBleeding)
        {
            dropletColor = TcData.ColorBleeding;
        }

        DrawDroplet(0, 0, dropletColor);
        Device.SetTransform(TransformState.World, saveWorldMat);
    }

    private void DrawDroplet(float x, float y, Color dropletColor)
    {
        var listPositions = new List<SharpDX.Vector3>();
        foreach (var p in TcData.GetDropletVertices())
        {
            listPositions.Add(new SharpDX.Vector3(x + p.X, y + p.Y, p.Z));
        }

        DrawColoredPolygon(Device, dropletColor, listPositions.ToArray());
    }

    private void DrawProbingBar(int intTooth, PerioSurf perioSurf)
    {
        const float barWidthMM = 0.6f;
        Color colorBar;
        var barPoints = TcData.GetProbingLine(intTooth, perioSurf, out colorBar);
        if (barPoints == null)
        {
            return;
        }

        var rectBar = new RectangleF(barPoints.Vertices[0].X - barWidthMM / 2f, barPoints.Vertices[0].Y,
            barWidthMM, barPoints.Vertices[1].Y - barPoints.Vertices[0].Y);
        DrawColoredRectangle(Device, rectBar, colorBar);
    }

    private void DrawFurcationTriangle(int intTooth, PerioSurf perioSurf, bool maxillary)
    {
        var furcationValue = TcData.GetFurcationValue(intTooth, perioSurf);
        if (furcationValue < 1 || furcationValue > 3)
        {
            return;
        }

        var sitePos = TcData.GetFurcationPos(intTooth, perioSurf);
        DrawFurcationTriangle(sitePos.X, sitePos.Y, maxillary, furcationValue);
    }

    private Color GetFurcationColor(int furcationValue)
    {
        var color = TcData.ColorFurcations;
        if (furcationValue >= TcData.RedLimitFurcations)
        {
            color = TcData.ColorFurcationsRed;
        }

        return color;
    }

    private void DrawFurcationTriangle(float tipx, float tipy, bool pointUp, int furcationValue)
    {
        const float triSideLenMM = 2f;
        float sign = pointUp ? 1 : -1;
        var color = GetFurcationColor(furcationValue);
        var triPoints = new List<SharpDX.Vector3>();
        //We form an equilateral triangle.
        triPoints.Add(new SharpDX.Vector3(tipx + triSideLenMM / 2f, tipy + sign * ((float) (triSideLenMM * Math.Sqrt(3) / 2f)), 0));
        triPoints.Add(new SharpDX.Vector3(tipx, tipy, 0));
        triPoints.Add(new SharpDX.Vector3(tipx - triSideLenMM / 2f, tipy + sign * ((float) (triSideLenMM * Math.Sqrt(3) / 2f)), 0));
        if (furcationValue == 1)
        {
            DrawExtendedLineStrip(0.1f, false, color, 0.38f, triPoints[0], triPoints[1], triPoints[2]);
        }
        else if (furcationValue == 2)
        {
            DrawExtendedLineStrip(0.1f, true, color, 0.38f, triPoints[0], triPoints[1], triPoints[2], triPoints[0]);
        }
        else if (furcationValue == 3)
        {
            DrawExtendedLineStrip(0.1f, true, color, 0.38f, triPoints[0], triPoints[1], triPoints[2], triPoints[0]);
            DrawColoredPolygon(Device, color, triPoints.ToArray());
        }
        else
        {
            //invalid value. assume no furcation.
        }
    }

    private List<SharpDX.Vector3> LineSimpleToVector3List(LineSimple lineSimple)
    {
        var vectorList = new List<SharpDX.Vector3>();
        for (var p = 0; p < lineSimple.Vertices.Count; p++)
        {
            vectorList.Add(new SharpDX.Vector3(lineSimple.Vertices[p].X, lineSimple.Vertices[p].Y, lineSimple.Vertices[p].Z));
        }

        return vectorList;
    }

    private void DrawExtendedLineStrip(float extendDist, bool extendEndPoints, Color color, float lineWidthMm, params SharpDX.Vector3[] arrayPoints)
    {
        if (arrayPoints.Length < 2)
        {
            return; //Can happen with drawings somehow.
        }

        //Convert each line strip into very simple two point lines so that line extensions can be calculated more easily below.
        //Items in the array are tuples of (2D point,bool indicating end point).
        var twoPointLines = new List<object>();
        for (var p = 0; p < arrayPoints.Length - 1; p++)
        {
            twoPointLines.Add(arrayPoints[p]);
            twoPointLines.Add(p == 0);
            twoPointLines.Add(arrayPoints[p + 1]);
            twoPointLines.Add(p == arrayPoints.Length - 2);
        }

        var listLines = new List<SharpDX.Vector3>();
        //Draw each individual two point line. The lines must be broken down from line strips so that when individual two point
        //line locations are modified they do not affect any other two point lines within the same line strip.
        for (var j = 0; j < twoPointLines.Count; j += 4)
        {
            var p1 = (SharpDX.Vector3) twoPointLines[j];
            var p1IsEndPoint = (bool) twoPointLines[j + 1];
            var p2 = (SharpDX.Vector3) twoPointLines[j + 2];
            var p2IsEndPoint = (bool) twoPointLines[j + 3];
            var lineDir = p2 - p1;
            if (lineDir.Length() == 0)
            {
                continue;
            }

            lineDir.Normalize(); //Gives the line direction a single unit length.
            //Do not extend the endpoints for the ends of the line strips unless extendEndPoints=true.
            if (!p1IsEndPoint || extendEndPoints)
            {
                p1 = p1 - extendDist * lineDir;
            }

            //Do not extend the endpoints for the ends of the line strips unless extendEndPoints=true.
            if (!p2IsEndPoint || extendEndPoints)
            {
                p2 = p2 + extendDist * lineDir;
            }

            listLines.Add(p1);
            listLines.Add(p2);
        }

        DrawLineList(color, lineWidthMm, listLines.ToArray());
    }
    
    private void DrawLineList(Color color, float lineWidthMm, params SharpDX.Vector3[] arrayPoints)
    {
        var transform = ScreenSpaceMatrix();
        var listTriangles = new List<SharpDX.Vector3>();
        for (var i = 0; i < arrayPoints.Length - 1; i += 2)
        {
            var p1 = arrayPoints[i];
            var p2 = arrayPoints[i + 1];
            var v = p2 - p1;
            if (v.Length() == 0)
            {
                continue;
            }

            var up = new SharpDX.Vector3(-v.Y, v.X, 0); //Our "up" vector.  Z component does not matter because of orthographic projection.
            up.Normalize();
            up *= lineWidthMm / 2;
            var q1 = p1 + up;
            var q2 = p2 + up;
            var q3 = p2 - up;
            var q4 = p1 - up;
            listTriangles.Add(q1);
            listTriangles.Add(q2);
            listTriangles.Add(q3);
            listTriangles.Add(q1);
            listTriangles.Add(q3);
            listTriangles.Add(q4);
        }

        DrawColoredTriangleList(Device, color, listTriangles.ToArray());
    }

    private void DrawFacialView(ToothGraphic toothGraphic, SharpDX.Matrix defOrient)
    {
        var toothTrans = SharpDX.Matrix.Translation(GetTransX(toothGraphic.ToothId), GetTransYfacial(toothGraphic.ToothId), 0);
        var rotAndTranUser = ToothRotationAndTranslationMatrix(toothGraphic);
        Device.SetTransform(TransformState.World, rotAndTranUser * toothTrans * defOrient);
        if (toothGraphic.Visible
            || (toothGraphic.IsCrown && toothGraphic.IsImplant)
            || toothGraphic.IsPontic)
        {
            DrawTooth(toothGraphic);
        }

        Device.SetRenderState(RenderState.ZEnable, false);
        if (toothGraphic.DrawBigX)
        {
            //Extraction
            if (ToothGraphic.IsMaxillary(toothGraphic.ToothId))
            {
                DrawLineList(toothGraphic.ColorX, 0.71f, new SharpDX.Vector3(-2f, 12f, 0f), new SharpDX.Vector3(2f, -6f, 0f));
                DrawLineList(toothGraphic.ColorX, 0.71f, new SharpDX.Vector3(2f, 12f, 0f), new SharpDX.Vector3(-2f, -6f, 0f));
            }
            else
            {
                DrawLineList(toothGraphic.ColorX, 0.71f, new SharpDX.Vector3(-2f, 6f, 0f), new SharpDX.Vector3(2f, -12f, 0f));
                DrawLineList(toothGraphic.ColorX, 0.71f, new SharpDX.Vector3(2f, 6f, 0f), new SharpDX.Vector3(-2f, -12f, 0f));
            }
        }

        if (toothGraphic.Visible && toothGraphic.IsRct)
        {
            //Root Canal
            foreach (var line in toothGraphic.GetRctLines())
            {
                DrawExtendedLineStrip(0.08f, false, toothGraphic.ColorRct, 0.8f, line.Vertices.Select(x => new SharpDX.Vector3(x.X, x.Y, x.Z)).ToArray());
            }
        }

        var groupBU = toothGraphic.GetGroup(ToothGroupType.Buildup); //during debugging, not all teeth have a BU group yet.
        if (toothGraphic.Visible && groupBU != null && groupBU.Visible)
        {
            //BU or Post
            //Here we use emissive light to color the buildup with the specified user chosen color.
            //We cannot set the color on the vertices because the verticies might be shared with the tooth,
            //in which case the color would affect teeth without buildups.
            var materialBu = new Material
            {
                Emissive = ColorToRaw4(groupBU.PaintColor)
            };
            Device.Material = materialBu;
            Device.SetStreamSource(0, toothGraphic.Vb, 0, ToothChartVertexPosNormX.StrideSize);
            groupBU.DrawDirectX(Device, toothGraphic);
        }

        if (toothGraphic.IsImplant)
        {
            DrawImplant(toothGraphic);
        }

        Device.SetRenderState(RenderState.ZEnable, true);
    }

    private void DrawImplant(ToothGraphic toothGraphic)
    {
        Device.SetRenderState(RenderState.ZEnable, true);
        if (ToothGraphic.IsMaxillary(toothGraphic.ToothId))
        {
            //flip the implant upside down
            Device.SetTransform(TransformState.World, SharpDX.Matrix.RotationZ((float) Math.PI) * Device.GetTransform(TransformState.World));
        }

        var material = new Material
        {
            Ambient = ColorToRaw4(toothGraphic.ColorImplant),
            Diffuse = ColorToRaw4(toothGraphic.ColorImplant),
            Specular = color_specular_normal,
            Power = specularSharpness
        };
        Device.Material = material;
        var implantGraphic = TcData.ListToothGraphics["implant"];
        Device.SetStreamSource(0, implantGraphic.Vb, 0, ToothChartVertexPosNormX.StrideSize);
        for (var g = 0; g < implantGraphic.Groups.Count; g++)
        {
            var group = (ToothGroup) implantGraphic.Groups[g];
            if (!group.Visible || group.GroupType == ToothGroupType.Buildup)
            {
                continue;
            }

            group.DrawDirectX(Device, implantGraphic);
        }
    }

    private void DrawOcclusalView(ToothGraphic toothGraphic, SharpDX.Matrix defOrient)
    {
        //now the occlusal surface. Notice that it's relative to origin again
        var toothTrans = SharpDX.Matrix.Translation(GetTransX(toothGraphic.ToothId), GetTransYocclusal(toothGraphic.ToothId), 0);
        var toothRot = SharpDX.Matrix.Identity;
        if (ToothGraphic.IsMaxillary(toothGraphic.ToothId))
        {
            toothRot = toothRot * SharpDX.Matrix.RotationX((float) ((-110f * Math.PI) / 180f)); //rotate angle about line from origin to x,y,z
        }
        else
        {
            //mandibular
            if (ToothGraphic.IsAnterior(toothGraphic.ToothId))
            {
                toothRot = toothRot * SharpDX.Matrix.RotationX((float) ((110f * Math.PI) / 180f));
            }
            else
            {
                toothRot = toothRot * SharpDX.Matrix.RotationX((float) ((120f * Math.PI) / 180f));
            }
        }

        var rotAndTranUser = ToothRotationAndTranslationMatrix(toothGraphic);
        Device.SetTransform(TransformState.World, rotAndTranUser * toothRot * toothTrans * defOrient);
        if (!Tooth.IsPrimary(toothGraphic.ToothId) //if perm tooth
            && Tooth.IsValidDB(Tooth.PermToPri(toothGraphic.ToothId))
            && TcData.ListToothGraphics[Tooth.PermToPri(toothGraphic.ToothId)].Visible) //and the primary tooth is visible
        {
            //do not paint
        }
        else if (toothGraphic.Visible //might not be visible if an implant
                 || (toothGraphic.IsCrown && toothGraphic.IsImplant)) //a crown on an implant will paint
            //pontics won't paint, because tooth is invisible
        {
            DrawTooth(toothGraphic);
        }

        if (toothGraphic.Visible && toothGraphic.IsSealant)
        {
            //draw sealant
            var toMm = 1f / TcData.ScaleMmToPix;
            Device.SetTransform(TransformState.World, ToothTranslationMatrix(toothGraphic) * toothRot * toothTrans * defOrient);
            if (ToothGraphic.IsMaxillary(toothGraphic.ToothId))
            {
                PrintString("S", TcData.PixelScaleRatio * (-6f * toMm), TcData.PixelScaleRatio * (-100f * toMm), -6f, toothGraphic.ColorSealant, fontXSealant);
            }
            else
            {
                PrintString("S", TcData.PixelScaleRatio * (-6f * toMm), TcData.PixelScaleRatio * (22f * toMm), -6f, toothGraphic.ColorSealant, fontXSealant);
            }
        }
    }

    private Material GetGroupMaterial(ToothGraphic toothGraphic, ToothGroup group)
    {
        var material = new Material();
        Color materialColor;
        if (toothGraphic.ShiftO < -10)
        {
            //if unerupted
            materialColor = Color.FromArgb(group.PaintColor.A / 2, group.PaintColor.R / 2, group.PaintColor.G / 2, group.PaintColor.B / 2);
        }
        else
        {
            materialColor = group.PaintColor;
        }

        material.Ambient = ColorToRaw4(materialColor);
        material.Diffuse = ColorToRaw4(materialColor);
        if (group.GroupType == ToothGroupType.Cementum)
        {
            material.Specular = color_specular_cementum;
        }
        else if (group.PaintColor.R > 245 && group.PaintColor.G > 245 && group.PaintColor.B > 235)
        {
            //because DirectX washes out the specular on the enamel, we have to turn it down only for the enamel color
            //for reference, this is the current enamel color: Color.FromArgb(255,250,250,240)
            var specEnamel = .4f;
            material.Specular = new SharpDX.Mathematics.Interop.RawColor4(specEnamel, specEnamel, specEnamel, 1);
        }
        else
        {
            material.Specular = color_specular_normal;
        }

        material.Power = specularSharpness;
        return material;
    }

    private void DrawTooth(ToothGraphic toothGraphic)
    {
        ToothGroup group;
        Device.SetStreamSource(0, toothGraphic.Vb, 0, ToothChartVertexPosNormX.StrideSize);
        for (var g = 0; g < toothGraphic.Groups.Count; g++)
        {
            group = toothGraphic.Groups[g];
            if (!group.Visible || group.FacesDirectX == null ||
                group.GroupType == ToothGroupType.Buildup || group.GroupType == ToothGroupType.None)
            {
                continue;
            }

            Device.Material = GetGroupMaterial(toothGraphic, group);
            group.DrawDirectX(Device, toothGraphic);
        }
    }
    
    private void DrawPerioLegend(float xLeft, float yTop)
    {
        Device.SetRenderState(RenderState.ZEnable, false);
        var ySpace = 4f;
        var textColor = Color.Black;
        Device.SetTransform(TransformState.World, SharpDX.Matrix.Translation(xLeft, yTop, 0));
        PrintString("Bleeding", 0, 0, 0, textColor, fontX);
        DrawDroplet(18f, -1.5f, TcData.ColorBleeding);
        PrintString("Suppuration", 0, -ySpace, 0, textColor, fontX);
        DrawDroplet(18f, -ySpace - 1.5f, TcData.ColorSuppuration);
        PrintString("Probing", 0, -2 * ySpace, 0, textColor, fontX);
        DrawColoredRectangle(Device, new RectangleF(18f, -2 * ySpace - 3f, 0.6f, 3f), TcData.ColorProbing);
        DrawColoredRectangle(Device, new RectangleF(19f, -2 * ySpace - 3f, 0.6f, 3f), TcData.ColorProbingRed);
        Device.SetTransform(TransformState.World, Device.GetTransform(TransformState.World) * SharpDX.Matrix.Translation(35f, 0, 0));
        PrintString("Gingival Margin", 0, 0, 0, textColor, fontX);
        DrawColoredRectangle(Device, new RectangleF(35f, -1.5f, 15f, 2f / TcData.ScaleMmToPix), TcData.ColorGingivalMargin);
        PrintString("Clinical Attachment Level", 0, -ySpace, 0, textColor, fontX);
        DrawColoredRectangle(Device, new RectangleF(35f, -ySpace - 1.5f, 15f, 2f / TcData.ScaleMmToPix), TcData.ColorCal);
        PrintString("Mucogingival Junction", 0, -2 * ySpace, 0, textColor, fontX);
        DrawColoredRectangle(Device, new RectangleF(35f, -2 * ySpace - 1.5f, 15f, 2f / TcData.ScaleMmToPix), TcData.ColorMgj);
        Device.SetTransform(TransformState.World, Device.GetTransform(TransformState.World) * SharpDX.Matrix.Translation(65f, 0, 0));
        PrintString("Furcation 1", 0, 0, 0, textColor, fontX);
        DrawFurcationTriangle(17f, -0.5f, false, 1);
        PrintString("Furcation 2", 0, -ySpace, 0, textColor, fontX);
        DrawFurcationTriangle(17f, -ySpace - 0.5f, false, 2);
        PrintString("Furcation 3", 0, -2 * ySpace, 0, textColor, fontX);
        DrawFurcationTriangle(17f, -2 * ySpace - 0.5f, false, 3);
    }

    public static void DrawColoredRectangle(Device device, RectangleF rect, Color color)
    {
        var arrayVerts = new SharpDX.Vector3[]
        {
            new SharpDX.Vector3(rect.Left, rect.Bottom, 0),
            new SharpDX.Vector3(rect.Left, rect.Top, 0),
            new SharpDX.Vector3(rect.Right, rect.Top, 0),
            new SharpDX.Vector3(rect.Right, rect.Bottom, 0),
        };
        DrawColoredPolygon(device, color, arrayVerts);
    }

    public static void DrawColoredPolygon(Device device, Color color, params SharpDX.Vector3[] arrayPoints)
    {
        var arrayVertices = new ToothChartVertexPosNormX[arrayPoints.Length];
        for (var i = 0; i < arrayPoints.Length; i++)
        {
            arrayVertices[i] = new ToothChartVertexPosNormX(arrayPoints[i].X, arrayPoints[i].Y, arrayPoints[i].Z, 0, 0, 0);
        }

        var material = new Material
        {
            Emissive = ColorToRaw4(color)
        };
        device.Material = material;
        device.DrawUserPrimitives(PrimitiveType.TriangleFan, arrayVertices.Length - 2, arrayVertices);
    }
    
    public static void DrawColoredTriangleList(Device device, Color color, params SharpDX.Vector3[] arrayPoints)
    {
        if (arrayPoints.Length < 3 || arrayPoints.Length % 3 != 0)
        {
            return;
        }

        var arrayVertices = new ToothChartVertexPosNormX[arrayPoints.Length];
        for (var i = 0; i < arrayPoints.Length; i++)
        {
            arrayVertices[i] = new ToothChartVertexPosNormX(arrayPoints[i].X, arrayPoints[i].Y, arrayPoints[i].Z, 0, 0, 0);
        }

        var material = new Material
        {
            Emissive = ColorToRaw4(color)
        };
        device.Material = material;
        device.DrawUserPrimitives(PrimitiveType.TriangleList, arrayVertices.Length / 3, arrayVertices);
    }

    private SharpDX.Matrix ToothTranslationMatrix(ToothGraphic toothGraphic)
    {
        var tran = SharpDX.Matrix.Identity;
        if (ToothGraphic.IsRight(toothGraphic.ToothId))
        {
            if (ToothGraphic.IsMaxillary(toothGraphic.ToothId))
            {
                //UR
                tran = SharpDX.Matrix.Translation(toothGraphic.ShiftM, -toothGraphic.ShiftO, toothGraphic.ShiftB);
            }
            else
            {
                //LR
                tran = SharpDX.Matrix.Translation(toothGraphic.ShiftM, toothGraphic.ShiftO, toothGraphic.ShiftB);
            }
        }
        else
        {
            if (ToothGraphic.IsMaxillary(toothGraphic.ToothId))
            {
                //UL
                tran = SharpDX.Matrix.Translation(-toothGraphic.ShiftM, -toothGraphic.ShiftO, toothGraphic.ShiftB);
            }
            else
            {
                //LL
                tran = SharpDX.Matrix.Translation(-toothGraphic.ShiftM, toothGraphic.ShiftO, toothGraphic.ShiftB);
            }
        }

        return tran;
    }

    private SharpDX.Matrix ToothRotationMatrix(ToothGraphic toothGraphic)
    {
        //1: tipM last
        //2: tipB second
        //3: rotate first
        var rotM = SharpDX.Matrix.Identity;
        var rotB = SharpDX.Matrix.Identity;
        var rot = SharpDX.Matrix.Identity;
        if (ToothGraphic.IsRight(toothGraphic.ToothId))
        {
            if (ToothGraphic.IsMaxillary(toothGraphic.ToothId))
            {
                //UR
                rotM = SharpDX.Matrix.RotationZ(((float) (toothGraphic.TipM * Math.PI) / 180)); //Converts angle to radians as required.
                rotB = SharpDX.Matrix.RotationX(((float) (-toothGraphic.TipB * Math.PI) / 180)); //Converts angle to radians as required.
                rot = SharpDX.Matrix.RotationY(((float) (toothGraphic.Rotate * Math.PI) / 180)); //Converts angle to radians as required.
            }
            else
            {
                //LR
                rotM = SharpDX.Matrix.RotationZ(((float) (-toothGraphic.TipM * Math.PI) / 180)); //Converts angle to radians as required.
                rotB = SharpDX.Matrix.RotationX(((float) (toothGraphic.TipB * Math.PI) / 180)); //Converts angle to radians as required.
                rot = SharpDX.Matrix.RotationY(((float) (-toothGraphic.Rotate * Math.PI) / 180)); //Converts angle to radians as required.
            }
        }
        else
        {
            if (ToothGraphic.IsMaxillary(toothGraphic.ToothId))
            {
                //UL
                rotM = SharpDX.Matrix.RotationZ(((float) (-toothGraphic.TipM * Math.PI) / 180)); //Converts angle to radians as required.
                rotB = SharpDX.Matrix.RotationX(((float) (-toothGraphic.TipB * Math.PI) / 180)); //Converts angle to radians as required.
                rot = SharpDX.Matrix.RotationY(((float) (toothGraphic.Rotate * Math.PI) / 180)); //Converts angle to radians as required.
            }
            else
            {
                //LL
                rotM = SharpDX.Matrix.RotationZ(((float) (toothGraphic.TipM * Math.PI) / 180)); //Converts angle to radians as required.
                rotB = SharpDX.Matrix.RotationX(((float) (toothGraphic.TipB * Math.PI) / 180)); //Converts angle to radians as required.
                rot = SharpDX.Matrix.RotationY(((float) (-toothGraphic.Rotate * Math.PI) / 180)); //Converts angle to radians as required.
            }
        }

        return rot * rotB * rotM;
    }
    
    private SharpDX.Matrix ToothRotationAndTranslationMatrix(ToothGraphic toothGraphic)
    {
        //remembering that they actually show in the opposite order, so:
        //1: translate
        //2: tipM last
        //3: tipB second
        //4: rotate first
        return ToothRotationMatrix(toothGraphic) * ToothTranslationMatrix(toothGraphic);
    }

    private float GetTransX(string tooth_id)
    {
        var toothInt = ToothGraphic.IdToInt(tooth_id);
        if (toothInt == -1)
        {
            throw new ApplicationException("Invalid tooth number: " + tooth_id); //only for debugging
        }

        return ToothGraphic.GetDefaultOrthographicXpos(toothInt);
    }

    private float GetTransYfacial(string tooth_id)
    {
        var basic = 29f;
        if (tooth_id == "6" || tooth_id == "11")
        {
            return basic + 1f;
        }

        if (tooth_id == "7" || tooth_id == "10")
        {
            return basic + 1f;
        }
        else if (tooth_id == "8" || tooth_id == "9")
        {
            return basic + 2f;
        }
        else if (tooth_id == "22" || tooth_id == "27")
        {
            return -basic - 2f;
        }
        else if (tooth_id == "23" || tooth_id == "24" || tooth_id == "25" || tooth_id == "26")
        {
            return -basic - 2f;
        }
        else if (ToothGraphic.IsMaxillary(tooth_id))
        {
            return basic;
        }

        return -basic;
    }

    private float GetTransYocclusal(string tooth_id)
    {
        if (ToothGraphic.IsMaxillary(tooth_id))
        {
            return 13f;
        }

        return -13f;
    }

    private void DrawNumbersAndLines()
    {
        //Draw the center line.
        DrawLineList(Color.White, 0.32f, new SharpDX.Vector3(-65f, 0, 0), new SharpDX.Vector3(65f, 0, 0));
        //Draw the tooth numbers.
        string tooth_id;
        for (var i = 1; i <= 52; i++)
        {
            tooth_id = Tooth.FromOrdinal(i);
            if (TcData.SelectedTeeth.Contains(tooth_id))
            {
                DrawNumber(tooth_id, true, 0);
            }
            else
            {
                DrawNumber(tooth_id, false, 0);
            }
        }
        //TimeSpan displayTime=(frameEndTime-frameBeginTime);
        //float fps=1000f/displayTime.Milliseconds;
        //this.PrintString(fps.ToString(),0,0,0,Color.Blue,xfont);
    }

    private void DrawNumbersAndLinesPerio(float baseY)
    {
        Device.SetRenderState(RenderState.ZEnable, false);
        //Draw the center line.
        DrawLineList(Color.Black, 0.5f, new SharpDX.Vector3(-65f, 45f, 0), new SharpDX.Vector3(65f, 45f, 0));
        //Draw the tooth numbers.
        string tooth_id;
        for (var i = 1; i <= 32; i++)
        {
            tooth_id = Tooth.FromOrdinal(i);
            //bool isSelected=TcData.SelectedTeeth.Contains(tooth_id);
            float yOffset = ToothGraphic.IsMaxillary(tooth_id) ? 30 : -29;
            DrawNumber(tooth_id, false, baseY + yOffset);
        }
    }
    
    private void DrawNumber(string tooth_id, bool isSelected, float offsetY)
    {
        if (!Tooth.IsValidDB(tooth_id))
        {
            return;
        }

        if (TcData.ListToothGraphics[tooth_id].HideNumber)
        {
            //if this is a "hidden" number
            return; //skip
        }

        //primary, but not set to show primary letters
        if (Tooth.IsPrimary(tooth_id) && !TcData.ListToothGraphics[Tooth.PriToPerm(tooth_id)].ShowPrimaryLetter)
        {
            return;
        }

        Device.SetRenderState(RenderState.ZEnable, false);
        Device.SetTransform(TransformState.World, SharpDX.Matrix.Identity);
        var displayNum = Tooth.DisplayGraphic(tooth_id, TcData.ToothNumberingNomenclature);
        var labelSize = MeasureStringMm(displayNum);
        var recMm = TcData.GetNumberRecMm(tooth_id, labelSize);
        recMm.Y += offsetY;
        var foreColor = TcData.ColorText;
        if (isSelected)
        {
            foreColor = TcData.ColorTextHighlight;
            //Draw the background rectangle only if the text is selected.
            DrawColoredRectangle(Device, recMm, TcData.ColorBackHighlight);
        }

        //Offsets the text by 10% of the rectangle width to ensure that there is at least on pixel of space on
        //the left for the background rectangle.
        PrintString(displayNum, recMm.X + recMm.Width * 0.1f, recMm.Y + recMm.Height, 0, foreColor, fontX);
    }

    private SizeF MeasureStringMm(string text)
    {
        if (String.IsNullOrEmpty(text))
        {
            return new SizeF(0, 0);
        }

        var rectSize = fontX.MeasureText(null, text, FontDrawFlags.VerticalCenter);
        //DirectX appears to add more vertical spacing than GDI+. We scale the height here to 80% as a result.
        //DirectX does not appear to add horizontal spacing into the width consideration. As a result, we widen 
        //the output by 25% to ensure that the highlight border around the text has a border all the way around.
        float width = rectSize.Right - rectSize.Left;
        float height = rectSize.Bottom - rectSize.Top; //Since y value increases downward.
        return new SizeF((width * 1.25f) / TcData.ScaleMmToPix, (height * 0.8f) / TcData.ScaleMmToPix);
    }

    private void PrintString(string text, float x, float y, float z, Color color, SharpDX.Direct3D9.Font printFont)
    {
        if (String.IsNullOrEmpty(text))
        {
            return;
        }

        var screenPoint = SharpDX.Vector3.Project(new SharpDX.Vector3(x, y, z),
            Device.Viewport.X, Device.Viewport.Y, Device.Viewport.Width, Device.Viewport.Height, Device.Viewport.MinDepth, Device.Viewport.MaxDepth,
            ScreenSpaceMatrix());
        printFont.DrawText(null, text, (int) Math.Ceiling(screenPoint.X), (int) Math.Floor(screenPoint.Y), ColorToRaw(color));
    }

    private void DrawWatches(SharpDX.Matrix defOrient)
    {
        Device.SetRenderState(RenderState.ZEnable, false);
        var watchTeeth = new Hashtable(TcData.ListToothGraphics.Count);
        for (var t = 0; t < TcData.ListToothGraphics.Count; t++)
        {
            //loop through each adult tooth
            var toothGraphic = TcData.ListToothGraphics[t];
            //If a tooth is marked to be watched then it is always visible, even if the tooth is missing/hidden.
            if (toothGraphic.ToothId == "implant" || !toothGraphic.Watch || Tooth.IsPrimary(toothGraphic.ToothId))
            {
                continue;
            }

            watchTeeth[toothGraphic.ToothId] = toothGraphic;
        }

        for (var t = 0; t < TcData.ListToothGraphics.Count; t++)
        {
            //loop through each primary tooth
            var toothGraphic = TcData.ListToothGraphics[t];
            //If a tooth is marked to be watched then it is always visible, even if the tooth is missing/hidden.
            if (toothGraphic.ToothId == "implant" || !toothGraphic.Watch || !Tooth.IsPrimary(toothGraphic.ToothId) || !toothGraphic.Visible)
            {
                continue;
            }

            watchTeeth[Tooth.PriToPerm(toothGraphic.ToothId)] = toothGraphic;
        }

        foreach (DictionaryEntry toothGraphic in watchTeeth)
        {
            RenderToothWatch((ToothGraphic) toothGraphic.Value, defOrient);
        }
    }

    private void RenderToothWatch(ToothGraphic toothGraphic, SharpDX.Matrix defOrient)
    {
        var toothTrans = SharpDX.Matrix.Identity; //Start with world transform defined by calling function.
        if (ToothGraphic.IsRight(toothGraphic.ToothId))
        {
            toothTrans = SharpDX.Matrix.Translation(GetTransX(toothGraphic.ToothId) + toothGraphic.ShiftM, 0, 0);
        }
        else
        {
            toothTrans = SharpDX.Matrix.Translation(GetTransX(toothGraphic.ToothId) - toothGraphic.ShiftM, 0, 0);
        }

        Device.SetTransform(TransformState.World, toothTrans * defOrient);
        var toMm = 1f / TcData.ScaleMmToPix;
        if (ToothGraphic.IsMaxillary(toothGraphic.ToothId))
        {
            PrintString("W", TcData.PixelScaleRatio * (-8f * toMm), TcData.PixelScaleRatio * (155f * toMm), -6f, Color.White, fontXWatchBig); //Just white for now.
            PrintString("W", TcData.PixelScaleRatio * (-6f * toMm), TcData.PixelScaleRatio * (153f * toMm), -6f, toothGraphic.ColorWatch, fontXWatch);
        }
        else
        {
            PrintString("W", TcData.PixelScaleRatio * (-8f * toMm), TcData.PixelScaleRatio * (-136f * toMm), -6f, Color.White, fontXWatchBig); //Just white for now.
            PrintString("W", TcData.PixelScaleRatio * (-6f * toMm), TcData.PixelScaleRatio * (-138f * toMm), -6f, toothGraphic.ColorWatch, fontXWatch);
        }
    }

    private void DrawDrawingSegments()
    {
        Device.SetRenderState(RenderState.ZEnable, false);
        Device.SetTransform(TransformState.World, SharpDX.Matrix.Identity);
        for (var s = 0; s < TcData.DrawingSegmentList.Count; s++)
        {
            var pointStr = TcData.DrawingSegmentList[s].DrawingSegment.Split(';');
            var points = new List<SharpDX.Vector3>();
            for (var p = 0; p < pointStr.Length; p++)
            {
                var xy = pointStr[p].Split(',');
                if (IsValidCoordinate(xy, out var x, out var y))
                {
                    var point = new PointF(x, y);
                    //if we set 0,0 to center, then this is where we would convert it back.
                    var pointMm = TcData.PointDrawingPixToMm(point);
                    points.Add(new SharpDX.Vector3(pointMm.X, pointMm.Y, 0f));
                }
            }

            DrawExtendedLineStrip(0.08f, true, TcData.DrawingSegmentList[s].ColorDraw, 0.71f, points.ToArray());
            //no filled circle at intersections
        }

        //Draw the points that make up the segment which is currently being drawn
        //but which has not yet been sent to the database.
        var listCurPoints = TcData.PointList.Select(x => TcData.PointPixToMm(x)).ToList();
        DrawExtendedLineStrip(0.08f, true, TcData.ColorDrawing, 0.71f, listCurPoints.Select(x => new SharpDX.Vector3(x.X, x.Y, 0)).ToArray());
    }

    private bool IsValidCoordinate(string[] coordinate, out float x, out float y)
    {
        x = 0;
        y = 0;
        return coordinate.Length == 2 && float.TryParse(coordinate[0], out x) && float.TryParse(coordinate[1], out y);
    }

    private void ToothChartDirectX_MouseDown(object sender, MouseEventArgs e)
    {
        _isMouseDown = true;
        if (TcData.CursorTool == CursorTool.Pointer)
        {
            _listSelectedTeethOld = TcData.SelectedTeeth.FindAll(x => x != null); //Make a copy of the list.  No elements should ever be null (copy all).
            var toothClicked = TcData.GetToothAtPoint(e.Location);
            if (TcData.SelectedTeeth.Contains(toothClicked))
            {
                SetSelected(toothClicked, false);
            }
            else
            {
                SetSelected(toothClicked, true);
            }

            Invalidate();
            Application.DoEvents(); //Force redraw.
        }
        else if (TcData.CursorTool == CursorTool.Pen)
        {
            TcData.PointList.Add(new PointF(e.X, e.Y));
        }
        else if (TcData.CursorTool == CursorTool.Eraser)
        {
            //do nothing
        }
        else if (TcData.CursorTool == CursorTool.ColorChanger)
        {
            //look for any lines near the "wand".
            //since the line segments are so short, it's sufficient to check end points.
            string[] xy;
            string[] pointStr;
            float x;
            float y;
            float dist; //the distance between the point being tested and the center of the eraser circle.
            var radius = 2f; //by trial and error to achieve best feel.
            var pointMouseScaled = TcData.GetPointMouseScaled(e.X, e.Y, Size);
            for (var i = 0; i < TcData.DrawingSegmentList.Count; i++)
            {
                pointStr = TcData.DrawingSegmentList[i].DrawingSegment.Split(';');
                for (var p = 0; p < pointStr.Length; p++)
                {
                    xy = pointStr[p].Split(',');
                    if (IsValidCoordinate(xy, out x, out y))
                    {
                        dist = (float) Math.Sqrt(Math.Pow(Math.Abs(x - pointMouseScaled.X), 2) + Math.Pow(Math.Abs(y - pointMouseScaled.Y), 2));
                        if (dist <= radius)
                        {
                            //testing circle intersection here
                            OnSegmentDrawn(TcData.DrawingSegmentList[i].DrawingSegment);
                            TcData.DrawingSegmentList[i].ColorDraw = TcData.ColorDrawing;
                            Invalidate();
                            return;
                        }
                    }
                }
            }
        }
    }

    private void ToothChartDirectX_MouseMove(object sender, MouseEventArgs e)
    {
        if (TcData.ListToothGraphics.Count == 0)
        {
            return; //can happen during forced close of perio chart.
        }

        if (TcData.CursorTool == CursorTool.Pointer)
        {
            hotTooth = TcData.GetToothAtPoint(e.Location);
            if (hotTooth == hotToothOld)
            {
                //mouse has not moved to another tooth
                return;
            }

            if (_isMouseDown)
            {
                //drag action
                var affectedTeeth = TcData.GetAffectedTeeth(hotToothOld, hotTooth, TcData.PointPixToMm(e.Location).Y);
                for (var i = 0; i < affectedTeeth.Count; i++)
                {
                    if (TcData.SelectedTeeth.Contains(affectedTeeth[i]))
                    {
                        SetSelected(affectedTeeth[i], false);
                    }
                    else
                    {
                        SetSelected(affectedTeeth[i], true);
                    }
                }

                hotToothOld = hotTooth;
                Invalidate();
                Application.DoEvents();
            }
            else
            {
                hotToothOld = hotTooth;
            }
        }
        else if (TcData.CursorTool == CursorTool.Pen)
        {
            if (!_isMouseDown)
            {
                return;
            }

            TcData.PointList.Add(new PointF(e.X, e.Y));
            try
            {
                Render(true); //Since were are only adding lines to the scene, we can draw the new lines over the old scene for efficiency.
            }
            catch (Exception ex)
            {
                ex.ToString(); //We cannot reference OpenDentBusiness, so we cannot use our ex.DoNothing() extention.
                ReinitailizeIfNeeded(); //Happens when the chart loads for the first time after the program is launched.
            }
        }
        else if (TcData.CursorTool == CursorTool.Eraser)
        {
            if (!_isMouseDown)
            {
                return;
            }

            //look for any lines that intersect the "eraser".
            //since the line segments are so short, it's sufficient to check end points.
            string[] xy;
            string[] pointStr;
            float x;
            float y;
            float dist; //the distance between the point being tested and the center of the eraser circle.
            var radius = 8f; //by trial and error to achieve best feel.
            var eraserPt = TcData.GetPointMouseScaled(e.X + 8.49f, e.Y + 8.49f, Size);
            for (var i = 0; i < TcData.DrawingSegmentList.Count; i++)
            {
                pointStr = TcData.DrawingSegmentList[i].DrawingSegment.Split(';');
                for (var p = 0; p < pointStr.Length; p++)
                {
                    xy = pointStr[p].Split(',');
                    if (IsValidCoordinate(xy, out x, out y))
                    {
                        dist = (float) Math.Sqrt(Math.Pow(Math.Abs(x - eraserPt.X), 2) + Math.Pow(Math.Abs(y - eraserPt.Y), 2));
                        if (dist <= radius)
                        {
                            //testing circle intersection here
                            OnSegmentDrawn(TcData.DrawingSegmentList[i].DrawingSegment); //triggers a deletion from db.
                            TcData.DrawingSegmentList.RemoveAt(i);
                            Invalidate();
                            Application.DoEvents();
                            return;
                        }
                    }
                }
            }
        }
        else if (TcData.CursorTool == CursorTool.ColorChanger)
        {
            //do nothing	
        }
    }

    private void ToothChartDirectX_MouseUp(object sender, MouseEventArgs e)
    {
        _isMouseDown = false;
        if (TcData.CursorTool == CursorTool.Pointer)
        {
            if (TcData.HasSelectedTeethChanged(_listSelectedTeethOld))
            {
                OnToothSelectionsChanged();
            }
        }
        else if (TcData.CursorTool == CursorTool.Pen)
        {
            //We used to call Invalidate() inside MouseMove(), but it was too slow when drawing with the pen.
            //We had to move this line down to the MouseUp() event for efficiency, and also because we observed a strange side effect such that
            //when OD was minimized and another copy of OD was opened, the first copy's directx device would become lost and not come back, effectively
            //causing the chart to look like a blank gray box.
            Invalidate();
            var drawingSegment = "";
            for (var i = 0; i < TcData.PointList.Count; i++)
            {
                if (i > 0)
                {
                    drawingSegment += ";";
                }

                var pointMouseScaled = TcData.GetPointMouseScaled(TcData.PointList[i].X, TcData.PointList[i].Y, Size);
                //I could compensate to center point here:
                drawingSegment += pointMouseScaled.X + "," + pointMouseScaled.Y;
            }

            OnSegmentDrawn(drawingSegment);
            TcData.PointList = [];
            //Invalidate();
        }
        else if (TcData.CursorTool == CursorTool.Eraser)
        {
            //do nothing
        }
        else if (TcData.CursorTool == CursorTool.ColorChanger)
        {
            //do nothing
        }
    }
    
    private void SetSelected(string tooth_id, bool setValue)
    {
        TcData.SetSelected(tooth_id, setValue);
        try
        {
            if (setValue)
            {
                DrawNumber(tooth_id, true, 0);
            }
            else
            {
                DrawNumber(tooth_id, false, 0);
            }
        }
        catch
        {
            ReinitailizeIfNeeded();
        }
    }

    public Bitmap GetBitmap()
    {
        Render(); //Redraw the scene to make sure the back buffer is up to date before copying it to a bitmap.
        var backBuffer = Device.GetBackBuffer(0, 0);
        var gs = Surface.ToStream(backBuffer, ImageFileFormat.Png);
        var bitmap = new Bitmap(gs);
        backBuffer.Dispose();
        return bitmap;
    }

    public static SharpDX.Mathematics.Interop.RawColorBGRA ColorToRaw(Color color)
    {
        return new SharpDX.Mathematics.Interop.RawColorBGRA(color.B, color.G, color.R, color.A);
    }

    public static SharpDX.Mathematics.Interop.RawColor4 ColorToRaw4(Color color)
    {
        return new SharpDX.Mathematics.Interop.RawColor4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    }

    #region Class Nested

    public class DirectXDeviceFormat
    {
        public int adapterIndex;
        public bool IsHardware;
        public CreateFlags createFlags = CreateFlags.SoftwareVertexProcessing;
        public string DepthStencilFormat = "D16";
        public string BackBufferFormat = "R5G6B5";
        public int MultiSampleCount;

        public DirectXDeviceFormat()
        {
        }

        ///<summary>Inverse of ToString(). String to DirectXDeviceFormat.</summary>
        public DirectXDeviceFormat(string directXFormat)
        {
            if (directXFormat.IndexOf(';') < 0)
            {
                //Invalid format.
                return;
            }

            var settings = directXFormat.Split(new char[] {';'});
            adapterIndex = SIn.Int(settings[0]);
            IsHardware = (settings[1] == "Hardware");
            createFlags = (CreateFlags) SIn.Int(settings[2]);
            DepthStencilFormat = settings[3];
            BackBufferFormat = settings[4];
            MultiSampleCount = (int) Enum.Parse(typeof(MultisampleType), settings[5]);
        }

        public override string ToString()
        {
            return "" + adapterIndex + ";" + (IsHardware ? "Hardware" : "Reference") + ";" + ((int) createFlags) + ";" +
                   DepthStencilFormat + ";" + BackBufferFormat + ";" + Enum.GetName(typeof(MultisampleType), MultiSampleCount);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(DirectXDeviceFormat))
            {
                return false;
            }

            var xformat = (DirectXDeviceFormat) obj;
            return (xformat.ToString() == ToString());
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public PresentParameters GetPresentParameters(Control control)
        {
            var pp = new PresentParameters
            {
                BackBufferWidth = control.Width,
                BackBufferHeight = control.Height,
                BackBufferFormat = Format.Unknown, //used in windowed mode to tell runtime to use current format and avoids need to call GetDisplayMode.
                //(Format)Enum.Parse(typeof(Format),BackBufferFormat);
                BackBufferCount = 1,
                MultiSampleType = (MultisampleType) MultiSampleCount, //MultisampleType.FourSamples;//
                //todo: CheckDeviceMultiSample
                MultiSampleQuality = 0, //pQualityLevel-1
                //todo: revisit this:
                SwapEffect = SwapEffect.Discard, //Required to be set to discard for anti-aliasing.
                DeviceWindowHandle = control.Handle, //hDeviceWindow
                Windowed = true, //true=not full screen
                EnableAutoDepthStencil = true,
                AutoDepthStencilFormat = (Format) Enum.Parse(typeof(Format), DepthStencilFormat),
                PresentFlags = PresentFlags.None,
                FullScreenRefreshRateInHz = 0, //Must be 0 in windowed mode.
                PresentationInterval = PresentInterval.Default
            };
            return pp;
        }

        public Device CreateDevice(Control control)
        {
            //When a Device object is created, the common language runtime will change the floating-point unit (FPU) to single precision to maintain better performance. 
            //This was causing extensive rounding errors throughout OD.
            //To maintain the default double precision FPU, which is default for the common language runtime, we use the CreateFlags.FpuPreserve flag in the constructor.
            //HardwareVertexProcessing, MixedVertexProcessing, and SoftwareVertexProcessing constants (CreateFlags) are mutually exclusive.
            //One of them must be specified during creation of a "device".
            //Therefore, we pass in our variable createFlags (typically SoftwareVertexProcessing) along with FpuPreserve to maintain double precision.
            //return new Device(_d3d,adapterIndex,DeviceType.Hardware,control.Handle,
            //	createFlags|CreateFlags.FpuPreserve,GetPresentParameters(control));
            var deviceType = DeviceType.Reference;
            if (IsHardware)
            {
                deviceType = DeviceType.Hardware;
            }

            var createFlagsBehavior = createFlags | CreateFlags.FpuPreserve;
            var presentParameters = GetPresentParameters(control);
            Device device = null;
            try
            {
                device = new Device(_d3d, adapterIndex, deviceType, control.Handle, createFlagsBehavior, presentParameters);
            }
            catch
            {
                device = null; //can put breakpoint here for testing
            }

            return device;
        }
    }

    #endregion Class Nested
}

#region Other Classes

///<summary>Position and Surface Normal.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 0)]
public struct ToothChartVertexPosNormX(float x, float y, float z, float nx, float ny, float nz)
{
    public float X = x;
    public float Y = y;
    public float Z = z;
    public float Nx = nx;
    public float Ny = ny;
    public float Nz = nz;

    public static VertexFormat Format = VertexFormat.Position | VertexFormat.Normal;

    ///<summary>The total size of this structure in bytes.</summary>
    public static int StrideSize = SharpDX.Utilities.SizeOf<ToothChartVertexPosNormX>();
}

#endregion Other Classes