using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OpenDental;

public partial class ScreenToothChart : UserControl
{
    private readonly string _toothValues;

    public readonly bool IsPrimary;

    public List<UserControlScreenTooth> GetTeeth =>
    [
        controlTooth2,
        controlTooth3,
        controlTooth4,
        controlTooth5,
        controlTooth12,
        controlTooth13,
        controlTooth14,
        controlTooth15,
        controlTooth18,
        controlTooth19,
        controlTooth20,
        controlTooth21,
        controlTooth28,
        controlTooth29,
        controlTooth30,
        controlTooth31
    ];

    public List<UserControlScreenTooth> GetPrimaryTeeth =>
    [
        controlToothA,
        controlTooth2,
        controlTooth3,
        controlTooth4,
        controlTooth5,
        controlTooth12,
        controlTooth13,
        controlTooth14,
        controlTooth15,
        controlToothJ,
        controlToothK,
        controlTooth18,
        controlTooth19,
        controlTooth20,
        controlTooth21,
        controlTooth28,
        controlTooth29,
        controlTooth30,
        controlTooth31,
        controlToothT
    ];

    public void SetChartMode()
    {
        if (IsPrimary)
        {
            labelA.Text = "A";
            labelA.Visible = true;
            label2.Text = "B";
            label3.Text = "C";
            label4.Text = "D";
            label5.Text = "E";
            label12.Text = "F";
            label13.Text = "G";
            label14.Text = "H";
            label15.Text = "I";
            labelJ.Text = "J";
            labelJ.Visible = true;
            labelK.Text = "K";
            labelK.Visible = true;
            label18.Text = "L";
            label19.Text = "M";
            label20.Text = "N";
            label21.Text = "O";
            label28.Text = "P";
            label29.Text = "Q";
            label30.Text = "R";
            label31.Text = "S";
            labelT.Text = "T";
            labelT.Visible = true;
            controlToothA.Visible = true;
            controlToothA.IsPrimary = true;
            controlToothA.Width = 72;
            controlToothA.IsMolar = false;
            labelA.Width = 72;
            controlToothJ.Visible = true;
            controlToothJ.IsPrimary = true;
            controlToothJ.Width = 72;
            controlToothJ.IsMolar = false;
            labelJ.Width = 72;
            controlToothK.Visible = true;
            controlToothK.IsPrimary = true;
            controlToothK.Width = 72;
            controlToothK.IsMolar = false;
            labelK.Width = 72;
            controlToothT.Visible = true;
            controlToothT.IsPrimary = true;
            controlToothT.Width = 72;
            controlToothT.IsMolar = false;
            labelT.Width = 72;
            controlTooth2.IsPrimary = true;
            controlTooth2.Width = 72;
            controlTooth2.IsMolar = false;
            label2.Width = 72;
            controlTooth3.IsPrimary = true;
            controlTooth3.Width = 72;
            controlTooth3.IsMolar = false;
            label3.Width = 72;
            controlTooth4.IsPrimary = true;
            controlTooth4.Width = 72;
            controlTooth4.IsMolar = false;
            label4.Width = 72;
            controlTooth5.IsPrimary = true;
            controlTooth5.Width = 72;
            controlTooth5.IsMolar = false;
            label5.Width = 72;
            controlTooth12.IsPrimary = true;
            controlTooth12.Width = 72;
            controlTooth12.IsMolar = false;
            label12.Width = 72;
            controlTooth13.IsPrimary = true;
            controlTooth13.Width = 72;
            controlTooth13.IsMolar = false;
            label13.Width = 72;
            controlTooth14.IsPrimary = true;
            controlTooth14.Width = 72;
            controlTooth14.IsMolar = false;
            label14.Width = 72;
            controlTooth15.IsPrimary = true;
            controlTooth15.Width = 72;
            controlTooth15.IsMolar = false;
            label15.Width = 72;
            controlTooth18.IsPrimary = true;
            controlTooth18.Width = 72;
            controlTooth18.IsMolar = false;
            label18.Width = 72;
            controlTooth19.IsPrimary = true;
            controlTooth19.Width = 72;
            controlTooth19.IsMolar = false;
            label19.Width = 72;
            controlTooth20.IsPrimary = true;
            controlTooth20.Width = 72;
            controlTooth20.IsMolar = false;
            label20.Width = 72;
            controlTooth21.IsPrimary = true;
            controlTooth21.Width = 72;
            controlTooth21.IsMolar = false;
            label21.Width = 72;
            controlTooth28.IsPrimary = true;
            controlTooth28.Width = 72;
            controlTooth28.IsMolar = false;
            label28.Width = 72;
            controlTooth29.IsPrimary = true;
            controlTooth29.Width = 72;
            controlTooth29.IsMolar = false;
            label29.Width = 72;
            controlTooth30.IsPrimary = true;
            controlTooth30.Width = 72;
            controlTooth30.IsMolar = false;
            label30.Width = 72;
            controlTooth31.IsPrimary = true;
            controlTooth31.Width = 72;
            controlTooth31.IsMolar = false;
            label31.Width = 72;
            controlToothA.Location = controlTooth2.Location;
            labelA.Location = label2.Location;
            controlTooth2.Location = controlTooth2.Location with {X = controlToothA.Location.X + 72};
            label2.Location = label2.Location with {X = labelA.Location.X + 72};
            controlTooth3.Location = controlTooth3.Location with {X = controlTooth2.Location.X + 72};
            label3.Location = label3.Location with {X = label2.Location.X + 72};
            controlTooth4.Location = controlTooth4.Location with {X = controlTooth3.Location.X + 72};
            label4.Location = label4.Location with {X = label3.Location.X + 72};
            controlTooth5.Location = controlTooth5.Location with {X = controlTooth4.Location.X + 72};
            label5.Location = label5.Location with {X = label4.Location.X + 72};
            controlTooth12.Location = controlTooth12.Location with {X = controlTooth5.Location.X + 72};
            label12.Location = label12.Location with {X = label5.Location.X + 72};
            controlTooth13.Location = controlTooth13.Location with {X = controlTooth12.Location.X + 72};
            label13.Location = label13.Location with {X = label12.Location.X + 72};
            controlTooth14.Location = controlTooth14.Location with {X = controlTooth13.Location.X + 72};
            label14.Location = label14.Location with {X = label13.Location.X + 72};
            controlTooth15.Location = controlTooth15.Location with {X = controlTooth14.Location.X + 72};
            label15.Location = label15.Location with {X = label14.Location.X + 72};
            controlToothJ.Location = controlTooth15.Location with {X = controlTooth15.Location.X + 72};
            labelJ.Location = label15.Location with {X = label15.Location.X + 72};
            controlToothT.Location = controlTooth31.Location;
            labelT.Location = label31.Location;
            controlTooth31.Location = controlTooth31.Location with {X = controlToothT.Location.X + 72};
            label31.Location = label31.Location with {X = labelT.Location.X + 72};
            controlTooth30.Location = controlTooth30.Location with {X = controlTooth31.Location.X + 72};
            label30.Location = label30.Location with {X = label31.Location.X + 72};
            controlTooth29.Location = controlTooth29.Location with {X = controlTooth30.Location.X + 72};
            label29.Location = label29.Location with {X = label30.Location.X + 72};
            controlTooth28.Location = controlTooth28.Location with {X = controlTooth29.Location.X + 72};
            label28.Location = label28.Location with {X = label29.Location.X + 72};
            controlTooth21.Location = controlTooth21.Location with {X = controlTooth28.Location.X + 72};
            label21.Location = label21.Location with {X = label28.Location.X + 72};
            controlTooth20.Location = controlTooth20.Location with {X = controlTooth21.Location.X + 72};
            label20.Location = label20.Location with {X = label21.Location.X + 72};
            controlTooth19.Location = controlTooth19.Location with {X = controlTooth20.Location.X + 72};
            label19.Location = label19.Location with {X = label20.Location.X + 72};
            controlTooth18.Location = controlTooth18.Location with {X = controlTooth19.Location.X + 72};
            label18.Location = label18.Location with {X = label19.Location.X + 72};
            controlToothK.Location = controlTooth18.Location with {X = controlTooth18.Location.X + 72};
            labelK.Location = label18.Location with {X = label18.Location.X + 72};
        }
        else
        {
            label2.Text = "2";
            label3.Text = "3";
            label4.Text = "4";
            label5.Text = "5";
            label12.Text = "12";
            label13.Text = "13";
            label14.Text = "14";
            label15.Text = "15";
            label18.Text = "18";
            label19.Text = "19";
            label20.Text = "20";
            label21.Text = "21";
            label28.Text = "28";
            label29.Text = "29";
            label30.Text = "30";
            label31.Text = "31";
        }
    }

    public ScreenToothChart(string toothValues, bool isPrimary)
    {
        InitializeComponent();

        IsPrimary = isPrimary;

        SetChartMode();

        _toothValues = toothValues;
    }

    private void ScreenToothChart_Load(object sender, EventArgs e)
    {
        var teethValues = _toothValues.Split(';');

        if (!IsPrimary)
        {
            controlTooth2.SetSelected(teethValues[1].Split(','));
            controlTooth3.SetSelected(teethValues[2].Split(','));
            controlTooth4.SetSelected(teethValues[3].Split(','));
            controlTooth5.SetSelected(teethValues[4].Split(','));
            controlTooth12.SetSelected(teethValues[5].Split(','));
            controlTooth13.SetSelected(teethValues[6].Split(','));
            controlTooth14.SetSelected(teethValues[7].Split(','));
            controlTooth15.SetSelected(teethValues[8].Split(','));
            controlTooth18.SetSelected(teethValues[9].Split(','));
            controlTooth19.SetSelected(teethValues[10].Split(','));
            controlTooth20.SetSelected(teethValues[11].Split(','));
            controlTooth21.SetSelected(teethValues[12].Split(','));
            controlTooth28.SetSelected(teethValues[13].Split(','));
            controlTooth29.SetSelected(teethValues[14].Split(','));
            controlTooth30.SetSelected(teethValues[15].Split(','));
            controlTooth31.SetSelected(teethValues[16].Split(','));
        }
        else
        {
            controlToothA.SetSelected(teethValues[1].Split(','));
            controlTooth2.SetSelected(teethValues[2].Split(','));
            controlTooth3.SetSelected(teethValues[3].Split(','));
            controlTooth4.SetSelected(teethValues[4].Split(','));
            controlTooth5.SetSelected(teethValues[5].Split(','));
            controlTooth12.SetSelected(teethValues[6].Split(','));
            controlTooth13.SetSelected(teethValues[7].Split(','));
            controlTooth14.SetSelected(teethValues[8].Split(','));
            controlTooth15.SetSelected(teethValues[9].Split(','));
            controlToothJ.SetSelected(teethValues[10].Split(','));
            controlToothK.SetSelected(teethValues[11].Split(','));
            controlTooth18.SetSelected(teethValues[12].Split(','));
            controlTooth19.SetSelected(teethValues[13].Split(','));
            controlTooth20.SetSelected(teethValues[14].Split(','));
            controlTooth21.SetSelected(teethValues[15].Split(','));
            controlTooth28.SetSelected(teethValues[16].Split(','));
            controlTooth29.SetSelected(teethValues[17].Split(','));
            controlTooth30.SetSelected(teethValues[18].Split(','));
            controlTooth31.SetSelected(teethValues[19].Split(','));
            controlToothT.SetSelected(teethValues[20].Split(','));
        }

        Invalidate();
    }
}