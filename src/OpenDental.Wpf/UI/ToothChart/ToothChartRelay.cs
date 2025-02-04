using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.Chart;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental
{
    ///<summary>Relays commands to either the old SparksToothChart.ToothChartWrapper or the new Sparks3d.ToothChart.</summary>
    public class ToothChartRelay
    {
        public const bool IsSparks3DPresent = false;

        private Sparks3DInterface _sparks3DInterface;
        private ToothChartWrapper _toothChartWrapper;

        public event ToothChartDrawEventHandler SegmentDrawn;
        public event EventHandler<TextMovedEventArgs> TextMoved;
        public event ToothChartSelectionEventHandler ToothSelectionsChanged;

        public int Bottom => _toothChartWrapper.Bottom;

        public Color ColorBackgroundMain
        {
            set => _toothChartWrapper.ColorBackground = value;
        }

        public Color ColorDrawing
        {
            set => _toothChartWrapper.ColorDrawing = value;
        }

        public Color ColorText
        {
            set => _toothChartWrapper.ColorText = value;
        }

        public Color ColorTextHighlightBack
        {
            set => _toothChartWrapper.ColorBackHighlight = value;
        }

        public Color ColorTextHighlightFore
        {
            set => _toothChartWrapper.ColorTextHighlight = value;
        }

        public CursorTool CursorTool
        {
            get => _toothChartWrapper.CursorTool;
            set => _toothChartWrapper.CursorTool = value;
        }

        public bool Enabled
        {
            set => _toothChartWrapper.Enabled = value;
        }

        public int Height => _toothChartWrapper.Height;

        public bool IsPerioMode
        {
            set => _toothChartWrapper.PerioMode = value;
        }

        public List<string> SelectedTeeth => _toothChartWrapper.SelectedTeeth;

        public int Width => _toothChartWrapper.Width;

        public void AddDrawingSegment(ToothInitial drawingSegment)
        {
            _toothChartWrapper.AddDrawingSegment(drawingSegment);
        }

        public void AddOrthoWireBetweenBrackets(string toothIDstart, string toothIDend, Color color)
        {
            //not supported
        }

        public void AddOrthoWireInBracket(string toothId, Color color)
        {
            //not supported
        }

        public void AddOrthoElastic(string toothIDstart, string toothIDend, Color color)
        {
            //not supported
        }

        public void AddPerioMeasure(int intTooth, PerioSequenceType sequenceType, int mb, int b, int db, int ml, int l, int dl)
        {
            _toothChartWrapper.AddPerioMeasure(intTooth, sequenceType, mb, b, db, ml, l, dl);
        }

        public void AddText(string text, PointF location, Color color, long toothInitialNum)
        {
            //not supported
        }

        public void BeginUpdate()
        {
            _toothChartWrapper.SuspendLayout();
        }

        public void DisposeControl()
        {
            _toothChartWrapper.Dispose();
        }

        public void EndUpdate()
        {
            _toothChartWrapper.ResumeLayout();
        }

        public Bitmap GetBitmap()
        {
            return _toothChartWrapper.GetBitmap();
        }

        public Control GetToothChart()
        {
            return _sparks3DInterface.GetToothChart();
        }

        public void MoveTooth(string toothId, float rotate, float tipM, float tipB, float shiftM, float shiftO, float shiftB)
        {
            _toothChartWrapper.MoveTooth(toothId, rotate, tipM, tipB, shiftM, shiftO, shiftB);
        }

        public void ResetTeeth()
        {
            _toothChartWrapper.ResetTeeth();
        }

        public void SetBigX(string toothId, Color color)
        {
            _toothChartWrapper.SetBigX(toothId, color);
        }

        public void SetBracket(string toothId, Color color)
        {
            //not supported
        }

        public void SetBu(string toothId, Color color)
        {
            _toothChartWrapper.SetBu(toothId, color);
        }

        public void SetCrown(string toothId, Color color)
        {
            _toothChartWrapper.SetCrown(toothId, color);
        }

        public void SetHidden(string toothId)
        {
            _toothChartWrapper.SetHidden(toothId);
        }

        public void SetImplant(string toothId, Color color)
        {
            _toothChartWrapper.SetImplant(toothId, color);
        }

        public void SetMissing(string toothId)
        {
            _toothChartWrapper.SetMissing(toothId);
        }

        public void SetMobility(string toothId, string mobility, Color color)
        {
            _toothChartWrapper.SetMobility(toothId, mobility, color);
        }

        public void SetOrthoMode(bool isOrthoMode)
        {
        }

        public void SetPerioColors(Color colorBleeding, Color colorSuppuration, Color colorProbing, Color colorProbingRed, Color colorGm, Color colorCal, Color colorMgj, Color colorFurcations, Color colorFurcationsRed, int redLimitProbing, int redLimitFurcations)
        {
            _toothChartWrapper.ColorBleeding = colorBleeding;
            _toothChartWrapper.ColorSuppuration = colorSuppuration;
            _toothChartWrapper.ColorProbing = colorProbing;
            _toothChartWrapper.ColorProbingRed = colorProbingRed;
            _toothChartWrapper.ColorGingivalMargin = colorGm;
            _toothChartWrapper.ColorCals = colorCal;
            _toothChartWrapper.ColorMgjs = colorMgj;
            _toothChartWrapper.ColorFurcations = colorFurcations;
            _toothChartWrapper.ColorFurcationsRed = colorFurcationsRed;
            _toothChartWrapper.RedLimitProbing = redLimitProbing;
            _toothChartWrapper.RedLimitFurcations = redLimitFurcations;
        }

        public void SetPontic(string toothId, Color color)
        {
            _toothChartWrapper.SetPontic(toothId, color);
        }

        public void SetPrimary(string toothId)
        {
            _toothChartWrapper.SetPrimary(toothId);
        }

        public void SetRct(string toothId, Color color)
        {
            _toothChartWrapper.SetRct(toothId, color);
        }

        public void SetRetainedRoot(string toothId, Color color)
        {
            //not supported
        }

        public void SetSealant(string toothId, Color color)
        {
            _toothChartWrapper.SetSealant(toothId, color);
        }

        public void SetSelected(string toothId, bool setValue)
        {
            _toothChartWrapper.SetSelected(toothId, setValue);
        }

        public void SetSpaceMaintainer(string toothId, Color color)
        {
            //not supported
        }

        public void SetSurfaceColors(string toothId, string surfaces, Color color)
        {
            _toothChartWrapper.SetSurfaceColors(toothId, surfaces, color);
        }

        public void SetText(string toothId, Color color, string text)
        {
            if (text == "W")
            {
                _toothChartWrapper.SetWatch(toothId, color);
            }
        }

        public void SetToothChartWrapper(ToothChartWrapper toothChartWrapper)
        {
            _toothChartWrapper = toothChartWrapper;
        }

        public void SetToothNumberingNomenclature(ToothNumberingNomenclature toothNumberingNomenclature)
        {
            _toothChartWrapper.SetToothNumberingNomenclature(toothNumberingNomenclature);
        }

        public void SetVeneer(string toothId, Color color)
        {
            _toothChartWrapper.SetVeneer(toothId, color);
        }

        private static List<UserOdPref> _listUserOdPrefsToothChartColor;

        private static List<UserOdPref> GetListUserOdPrefs()
        {
            if (_listUserOdPrefsToothChartColor == null)
            {
                RefreshToothColorsPrefs();
            }

            return _listUserOdPrefsToothChartColor;
        }

        public bool DoesToothColorPrefApply(List<long> listCurProvNums, long procProvNum)
        {
            return HasToothColorUserPref() && !listCurProvNums.Contains(procProvNum);
        }

        ///<summary>
        ///Returns ProvNums related to the passed in entities in a specific order. Returns the first of the following conditions: 
        ///	1. Returns an empty list if the user currently logged in does not have ToothChartUsesDiffColorByProv user pref. 
        ///	2. Returns the ProvNum of the user currently logged in if one is set. 
        ///	3. Returns all primary providers for any appointment for the patient passed in that is scheduled for today. 
        ///	4. Returns the primary provider if none of the previous conditions were met. Can return an empty list.
        ///	</summary>
        public List<long> GetPertinentProvNumsForToothColorPref(Userod user, Patient patCur, List<Appointment> listAppts)
        {
            if (!HasToothColorUserPref())
            {
                return [];
            }

            if (user?.ProvNum != 0)
            {
                return [Security.CurUser.ProvNum];
            }

            if (patCur == null)
            {
                return [];
            }

            var listApptProvNums = listAppts?.Where(x => x.AptDateTime.Date == DateTime.Today && x.PatNum == patCur.PatNum).Select(x => x.ProvNum).ToList();
            if (!listApptProvNums.IsNullOrEmpty())
            {
                return listApptProvNums;
            }

            return [patCur.PriProv];
        }

        public bool GetToothColors(ProcedureCode curProc, ProcStat procStatus, bool doUseEcEoForCProcs, out Color cDark, out Color cLight)
        {
            cDark = Color.White;
            cLight = Color.White;
            var listDefsToothColors = Defs.GetDefsForCategory(DefCat.ChartGraphicColors, true);
            if (curProc.GraphicColor != Color.FromArgb(0) && procStatus != ProcStat.TPi)
            {
                cDark = curProc.GraphicColor;
                cLight = curProc.GraphicColor;
                return true;
            }

            switch (procStatus)
            {
                case ProcStat.C:
                    if (doUseEcEoForCProcs)
                    {
                        //Whoever is currently viewing the ToothChart wants other Provider's C procs to show as EC
                        cDark = listDefsToothColors[3].ItemColor; //same values as ProcStat.EO
                        cLight = listDefsToothColors[8].ItemColor;
                    }
                    else
                    {
                        cDark = listDefsToothColors[1].ItemColor;
                        cLight = listDefsToothColors[6].ItemColor;
                    }

                    return true;
                case ProcStat.TP:
                    cDark = listDefsToothColors[0].ItemColor;
                    cLight = listDefsToothColors[5].ItemColor;
                    return true;
                case ProcStat.EC:
                    cDark = listDefsToothColors[2].ItemColor;
                    cLight = listDefsToothColors[7].ItemColor;
                    return true;
                case ProcStat.EO:
                    cDark = listDefsToothColors[3].ItemColor;
                    cLight = listDefsToothColors[8].ItemColor;
                    return true;
                case ProcStat.R:
                    cDark = listDefsToothColors[4].ItemColor;
                    cLight = listDefsToothColors[9].ItemColor;
                    return true;
                case ProcStat.Cn:
                    cDark = listDefsToothColors[16].ItemColor;
                    cLight = listDefsToothColors[17].ItemColor;
                    return true;
                case ProcStat.D: //Can happen with invalidated locked procs.
                default:
                    cDark = Color.White;
                    cLight = Color.White;
                    return false; //Don't draw.
            }
        }

        public static void RefreshToothColorsPrefs()
        {
            _listUserOdPrefsToothChartColor = UserOdPrefs.GetByFkeyType(UserOdFkeyType.ToothChartUsesDiffColorByProv);
        }

        public bool HasToothColorUserPref()
        {
            return GetListUserOdPrefs().Select(x => x.UserNum).Contains(Security.CurUser.UserNum);
        }
    }

    public class TextMovedEventArgs : EventArgs
    {
        public Color ColorNew { get; set; }
        public PointF LocationNew { get; set; }
        public long ToothInitialNum { get; set; }
    }
}