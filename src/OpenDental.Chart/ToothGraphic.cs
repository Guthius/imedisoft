using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using OpenDental.Chart.Properties;
using OpenDentBusiness;
using SharpDX.Direct3D9;

namespace OpenDental.Chart;

public class ToothGraphic
{
    private static float[] _defaultOrthographicXpos;

    public readonly List<VertexNormal> VertexNormals;
    public VertexBuffer Vb;
    public List<ToothGroup> Groups;
    public float Rotate;
    public float TipB;
    public float TipM;
    public float ShiftM;
    public float ShiftO;
    public float ShiftB;
    public bool IsRct;
    public bool DrawBigX;
    public Color ColorX;
    public Color ColorRct;
    public Color ColorImplant;
    public Color ColorSealant;
    public Color ColorWatch = Color.Red;
    public Color ColorMobility;
    public bool ShowPrimaryLetter;
    public bool IsImplant;
    public bool IsCrown;
    public bool IsPontic;
    public bool IsSealant;
    public bool Watch;
    public string Mobility;

    public ToothGraphic Copy(ToothGraphic toothGraphicOtherDevice = null)
    {
        var copy = (ToothGraphic) MemberwiseClone();
        if (toothGraphicOtherDevice is null)
        {
            return copy;
        }

        copy.Vb = toothGraphicOtherDevice.Vb;
        copy.Groups = toothGraphicOtherDevice.Groups;

        return copy;
    }

    internal ToothGraphic()
    {
    }

    public ToothGraphic(string toothId)
    {
        if (toothId != "implant" && !IsValidToothId(toothId))
        {
            throw new ApplicationException("Invalid tooth ID");
        }

        ToothId = toothId;

        VertexNormals = [];

        ImportObj();
        SetDefaultColors();
    }

    public override string ToString()
    {
        var result = ToothId;

        if (IsRct)
        {
            result += ", RCT";
        }

        return result;
    }

    public string ToothId { get; }
    public bool Visible { get; set; }
    public bool HideNumber { get; set; }

    public void PrepareForDirectX(Device deviceRef)
    {
        CleanupDirectX();

        var arrayVerts = new ToothChartVertexPosNormX[VertexNormals.Count];
        for (var i = 0; i < VertexNormals.Count; i++)
        {
            var v = VertexNormals[i];
            arrayVerts[i] = new ToothChartVertexPosNormX(v.Vertex.X, v.Vertex.Y, v.Vertex.Z, v.Normal.X, v.Normal.Y, v.Normal.Z);
        }

        Vb = new VertexBuffer(deviceRef, arrayVerts.Length * ToothChartVertexPosNormX.StrideSize, Usage.WriteOnly, ToothChartVertexPosNormX.Format, Pool.Managed);

        var dataStream = Vb.Lock(0, 0, LockFlags.None);

        dataStream.WriteRange(arrayVerts);

        Vb.Unlock();

        foreach (var group in Groups)
        {
            group.PrepareForDirectX(deviceRef);
        }
    }

    public void CleanupDirectX()
    {
        if (Vb != null)
        {
            Vb.Dispose();
            Vb = null;
        }

        foreach (var group in Groups)
        {
            group.CleanupDirectX();
        }
    }

    public void Reset()
    {
        Visible = !Tooth.IsPrimary(ToothId);
        TipM = 0;
        TipB = 0;
        Rotate = 0;
        ShiftB = 0;
        ShiftM = 0;
        ShiftO = 0;
        SetDefaultColors();
        IsRct = false;
        HideNumber = false;
        DrawBigX = false;
        ShowPrimaryLetter = false;
        IsImplant = false;
        IsCrown = false;
        IsPontic = false;
        IsSealant = false;
        Watch = false;
    }

    public void SetSurfaceColors(string surfaces, Color color)
    {
        foreach (var surface in surfaces)
        {
            switch (surface)
            {
                case 'M':
                    SetGroupColor(ToothGroupType.M, color);
                    SetGroupColor(ToothGroupType.MF, color);
                    break;

                case 'O':
                    SetGroupColor(ToothGroupType.O, color);
                    break;

                case 'D':
                    SetGroupColor(ToothGroupType.D, color);
                    SetGroupColor(ToothGroupType.DF, color);
                    break;

                case 'B':
                    SetGroupColor(ToothGroupType.B, color);
                    break;

                case 'L':
                    SetGroupColor(ToothGroupType.L, color);
                    break;

                case 'V':
                    SetGroupColor(ToothGroupType.V, color);
                    break;

                case 'I':
                    SetGroupColor(ToothGroupType.I, color);
                    SetGroupColor(ToothGroupType.IF, color);
                    break;

                case 'F':
                    SetGroupColor(ToothGroupType.F, color);
                    break;
            }
        }
    }

    public void SetGroupColor(ToothGroupType groupType, Color paintColor)
    {
        foreach (var group in Groups)
        {
            if (group.GroupType != groupType)
            {
                continue;
            }

            group.PaintColor = paintColor;
        }
    }

    private void SetDefaultColors()
    {
        foreach (var group in Groups)
        {
            group.PaintColor = group.GroupType == ToothGroupType.Cementum
                ? Color.FromArgb(255, 250, 245, 223)
                : Color.FromArgb(255, 250, 250, 240);

            if (group.GroupType is ToothGroupType.Canals or ToothGroupType.Buildup)
            {
                group.Visible = false;
            }
            else
            {
                group.Visible = true;
            }
        }
    }

    public void SetGroupVisibility(ToothGroupType groupType, bool setVisible)
    {
        foreach (var group in Groups)
        {
            if (group.GroupType != groupType)
            {
                continue;
            }

            group.Visible = setVisible;
        }
    }


    public ToothGroup GetGroup(ToothGroupType groupType)
    {
        foreach (var group in Groups)
        {
            if (group.GroupType == groupType)
            {
                return group;
            }
        }

        return null;
    }

    public int GetIndexForDisplayList(ToothGroup group)
    {
        var toothInt = Tooth.ToOrdinal(ToothId);

        return (toothInt * 15) + (int) group.GroupType;
    }

    public static bool IsValidToothId(string toothId)
    {
        if (!Tooth.IsValidDB(toothId))
        {
            return false;
        }

        return !Tooth.IsSuperNum(toothId);
    }

    public static int IdToInt(string toothId)
    {
        if (!IsValidToothId(toothId))
        {
            return -1;
        }

        return Tooth.ToInt(toothId);
    }

    public static float GetWidth(string toothId)
    {
        return toothId switch
        {
            "1" or "16" => 8.5f,
            "2" or "15" => 9f,
            "3" or "14" => 10f,
            "4" or "5" or "13" or "12" => 7f,
            "6" or "11" => 7.5f,
            "7" or "10" => 6.5f,
            "8" or "9" => 8.5f,
            "17" or "32" => 10f,
            "18" or "31" => 10.5f,
            "19" or "30" => 11f,
            "20" or "21" or "29" or "28" => 7f,
            "22" or "27" => 7f,
            "23" or "26" => 5f,
            "24" or "25" => 5f,
            _ => throw new ApplicationException(toothId)
        };
    }


    public static float GetWidth(int toothNumber)
    {
        return GetWidth(toothNumber.ToString());
    }

    public static float GetDefaultOrthographicXpos(int toothNumber)
    {
        if (_defaultOrthographicXpos == null)
        {
            _defaultOrthographicXpos = new float[33];

            _defaultOrthographicXpos[8] = 0 / 2f - GetWidth(8) / 2f;
            _defaultOrthographicXpos[7] = _defaultOrthographicXpos[8] - GetWidth(8) / 2f - GetWidth(7) / 2f;
            _defaultOrthographicXpos[6] = _defaultOrthographicXpos[7] - GetWidth(7) / 2f - GetWidth(6) / 2f;
            _defaultOrthographicXpos[5] = _defaultOrthographicXpos[6] - GetWidth(6) / 2f - GetWidth(5) / 2f;
            _defaultOrthographicXpos[4] = _defaultOrthographicXpos[5] - GetWidth(5) / 2f - GetWidth(4) / 2f;
            _defaultOrthographicXpos[3] = _defaultOrthographicXpos[4] - GetWidth(4) / 2f - GetWidth(3) / 2f;
            _defaultOrthographicXpos[2] = _defaultOrthographicXpos[3] - GetWidth(3) / 2f - GetWidth(2) / 2f;
            _defaultOrthographicXpos[1] = _defaultOrthographicXpos[2] - GetWidth(2) / 2f - GetWidth(1) / 2f;
            _defaultOrthographicXpos[9] = -_defaultOrthographicXpos[8];
            _defaultOrthographicXpos[10] = -_defaultOrthographicXpos[7];
            _defaultOrthographicXpos[11] = -_defaultOrthographicXpos[6];
            _defaultOrthographicXpos[12] = -_defaultOrthographicXpos[5];
            _defaultOrthographicXpos[13] = -_defaultOrthographicXpos[4];
            _defaultOrthographicXpos[14] = -_defaultOrthographicXpos[3];
            _defaultOrthographicXpos[15] = -_defaultOrthographicXpos[2];
            _defaultOrthographicXpos[16] = -_defaultOrthographicXpos[1];
            _defaultOrthographicXpos[24] = 0 / 2f + GetWidth(24) / 2f;
            _defaultOrthographicXpos[23] = _defaultOrthographicXpos[24] + GetWidth(24) / 2f + GetWidth(23) / 2f;
            _defaultOrthographicXpos[22] = _defaultOrthographicXpos[23] + GetWidth(23) / 2f + GetWidth(22) / 2f;
            _defaultOrthographicXpos[21] = _defaultOrthographicXpos[22] + GetWidth(22) / 2f + GetWidth(21) / 2f;
            _defaultOrthographicXpos[20] = _defaultOrthographicXpos[21] + GetWidth(21) / 2f + GetWidth(20) / 2f;
            _defaultOrthographicXpos[19] = _defaultOrthographicXpos[20] + GetWidth(20) / 2f + GetWidth(19) / 2f;
            _defaultOrthographicXpos[18] = _defaultOrthographicXpos[19] + GetWidth(19) / 2f + GetWidth(18) / 2f;
            _defaultOrthographicXpos[17] = _defaultOrthographicXpos[18] + GetWidth(18) / 2f + GetWidth(17) / 2f;
            _defaultOrthographicXpos[25] = -_defaultOrthographicXpos[24];
            _defaultOrthographicXpos[26] = -_defaultOrthographicXpos[23];
            _defaultOrthographicXpos[27] = -_defaultOrthographicXpos[22];
            _defaultOrthographicXpos[28] = -_defaultOrthographicXpos[21];
            _defaultOrthographicXpos[29] = -_defaultOrthographicXpos[20];
            _defaultOrthographicXpos[30] = -_defaultOrthographicXpos[19];
            _defaultOrthographicXpos[31] = -_defaultOrthographicXpos[18];
            _defaultOrthographicXpos[32] = -_defaultOrthographicXpos[17];
        }

        if (toothNumber is < 1 or > 32)
        {
            throw new ApplicationException("Invalid tooth_num: " + toothNumber);
        }

        return _defaultOrthographicXpos[toothNumber];
    }


    public static bool IsMaxillary(string toothId)
    {
        return IsValidToothId(toothId) && Tooth.IsMaxillary(toothId);
    }

    public static bool IsAnterior(string toothId)
    {
        return IsValidToothId(toothId) && Tooth.IsAnterior(toothId);
    }

    public static bool IsRight(string toothId)
    {
        if (!IsValidToothId(toothId))
        {
            return false;
        }

        var toothNumber = IdToInt(toothId);

        return toothNumber switch
        {
            >= 1 and <= 8 or >= 25 and <= 32 => true,
            _ => false
        };
    }

    private void ImportObj()
    {
        var bytes = ToothId switch
        {
            "1" or "16" => Resources.tooth1,
            "2" or "15" => Resources.tooth2,
            "3" or "14" => Resources.tooth3,
            "4" or "13" => Resources.tooth4,
            "5" or "12" => Resources.tooth5,
            "6" or "11" => Resources.tooth6,
            "7" or "10" => Resources.tooth7,
            "8" or "9" => Resources.tooth8,
            "17" or "32" => Resources.tooth32,
            "18" or "31" => Resources.tooth31,
            "19" or "30" => Resources.tooth30,
            "20" or "29" => Resources.tooth29,
            "21" or "28" => Resources.tooth28,
            "22" or "27" => Resources.tooth27,
            "23" or "24" or "25" or "26" => Resources.tooth25,
            "A" or "J" => Resources.toothA,
            "B" or "I" => Resources.toothB,
            "C" or "H" => Resources.toothC,
            "D" or "G" => Resources.toothD,
            "E" or "F" => Resources.toothE,
            "P" or "O" or "Q" or "N" => Resources.toothP,
            "R" or "M" => Resources.toothR,
            "S" or "L" => Resources.toothS,
            "T" or "K" => Resources.toothT,
            "implant" => Resources.implant,
            _ => null
        };

        if (bytes is null)
        {
            return;
        }

        var flipHorizontally = ToothId != "implant" && IdToInt(ToothId) >= 9 && IdToInt(ToothId) <= 24;

        var vertices = new List<Vertex3>();
        var normals = new List<Vertex3>();

        Groups = [];

        var faces = new List<Face>();
        var stream = new MemoryStream(bytes);

        using var streamReader = new StreamReader(stream);

        ToothGroup toothGroup = null;

        while (streamReader.ReadLine() is { } line)
        {
            if (line.StartsWith("#") || line.StartsWith("mtllib") || line.StartsWith("usemtl") || line.StartsWith("o"))
            {
                continue;
            }

            string[] items;
            Vertex3 vertex;

            if (line.StartsWith("v "))
            {
                items = line.Split(' ');

                vertex = new Vertex3();
                if (flipHorizontally)
                {
                    vertex.X = -Convert.ToSingle(items[1], CultureInfo.InvariantCulture);
                }
                else
                {
                    vertex.X = Convert.ToSingle(items[1], CultureInfo.InvariantCulture);
                }

                vertex.Y = Convert.ToSingle(items[2], CultureInfo.InvariantCulture);
                vertex.Z = Convert.ToSingle(items[3], CultureInfo.InvariantCulture);

                vertices.Add(vertex);

                continue;
            }

            if (line.StartsWith("vn"))
            {
                items = line.Split(' ');

                vertex = new Vertex3();
                if (flipHorizontally)
                {
                    vertex.X = -Convert.ToSingle(items[1], CultureInfo.InvariantCulture);
                }
                else
                {
                    vertex.X = Convert.ToSingle(items[1], CultureInfo.InvariantCulture);
                }

                vertex.Y = Convert.ToSingle(items[2], CultureInfo.InvariantCulture);
                vertex.Z = Convert.ToSingle(items[3], CultureInfo.InvariantCulture);

                normals.Add(vertex);

                continue;
            }

            if (line.StartsWith("g"))
            {
                if (toothGroup is not null)
                {
                    toothGroup.Faces = new List<Face>(faces);

                    Groups.Add(toothGroup);
                }

                toothGroup = new ToothGroup();
                faces = [];

                toothGroup.GroupType = line switch
                {
                    "g cube1_Cementum" => ToothGroupType.Cementum,
                    "g cube1_Enamel2" => ToothGroupType.Enamel,
                    "g cube1_M" => ToothGroupType.M,
                    "g cube1_D" => ToothGroupType.D,
                    "g cube1_F" => ToothGroupType.F,
                    "g cube1_I" => ToothGroupType.I,
                    "g cube1_L" => ToothGroupType.L,
                    "g cube1_V" => ToothGroupType.V,
                    "g cube1_B" => ToothGroupType.B,
                    "g cube1_O" => ToothGroupType.O,
                    "g cube1_Canals" => ToothGroupType.Canals,
                    "g cube1_Buildup" => ToothGroupType.Buildup,
                    "g cube1_Implant" => ToothGroupType.Implant,
                    "g cube1_EnamelF" => ToothGroupType.EnamelF,
                    "g cube1_DF" => ToothGroupType.DF,
                    "g cube1_MF" => ToothGroupType.MF,
                    "g cube1_IF" => ToothGroupType.IF,
                    _ => ToothGroupType.None
                };
            }

            if (!line.StartsWith("f"))
            {
                continue;
            }
            
            items = line.Split(' ');

            var face = new Face();

            for (var i = 1; i < items.Length; i++)
            {
                var parts = items[i].Split('/');
                var vertexNormal = new VertexNormal();
                var vertexIndex = Convert.ToInt32(parts[0]) - 1;
                var normalIndex = Convert.ToInt32(parts[2]) - 1;

                vertexNormal.Vertex = vertices[vertexIndex];
                vertexNormal.Normal = normals[normalIndex];

                face.IndexList.Add(GetIndexForVertNorm(vertexNormal));
            }

            faces.Add(face);
        }

        toothGroup.Faces = new List<Face>(faces);

        Groups.Add(toothGroup);
    }

    private int GetIndexForVertNorm(VertexNormal vertnorm)
    {
        for (var i = 0; i < VertexNormals.Count; i++)
        {
            if (VertexNormals[i].Vertex != vertnorm.Vertex)
            {
                continue;
            }

            if (VertexNormals[i].Normal != vertnorm.Normal)
            {
                continue;
            }

            return i;
        }

        VertexNormals.Add(vertnorm);

        return VertexNormals.Count - 1;
    }

    public List<LineSimple> GetRctLines()
    {
        var results = new List<LineSimple>();

        LineSimple line;

        switch (ToothId)
        {
            case "1":
                results.Add(new LineSimple(
                    -.7f, 9.6f, 1.6f,
                    .6f, 8, 1.6f,
                    1.2f, 5.8f, 1.6f,
                    .8f, 0, .9f));
                results.Add(new LineSimple(
                    -1.8f, 9.5f, 1.6f,
                    -1.6f, 8, 1.6f,
                    -1.6f, 5.8f, 1.6f,
                    -.9f, 0, .9f));
                break;

            case "16":
                results.Add(new LineSimple(
                    .7f, 9.6f, 1.6f,
                    -.6f, 8, 1.6f,
                    -1.2f, 5.8f, 1.6f,
                    -.8f, 0, .9f));
                results.Add(new LineSimple(
                    1.8f, 9.5f, 1.6f,
                    1.6f, 8, 1.6f,
                    1.6f, 5.8f, 1.6f,
                    .9f, 0, .9f));
                break;

            case "2":
                results.Add(new LineSimple(
                    .3f, 10.6f, 3.4f,
                    1.4f, 8, 3.2f,
                    1.7f, 5, 1.9f,
                    .9f, 0, 1f));
                results.Add(new LineSimple(
                    -1.8f, 10.5f, 3.4f,
                    -2, 7, 3.2f,
                    -1.7f, 4, 1.9f,
                    -1, 0, 1.1f));
                results.Add(new LineSimple(
                    -2, 11.5f, -3.7f,
                    -.6f, 6.3f, -4,
                    0, 0, -2.3f));
                break;

            case "15":
                line = new LineSimple(
                    -.3f, 10.6f, 3.4f,
                    -1.4f, 8, 3.2f,
                    -1.7f, 5, 1.9f,
                    -.9f, 0, 1f);
                results.Add(line);
                line = new LineSimple(
                    1.8f, 10.5f, 3.4f,
                    2, 7, 3.2f,
                    1.7f, 4, 1.9f,
                    1, 0, 1.1f);
                results.Add(line);
                line = new LineSimple(
                    2, 11.5f, -3.7f,
                    .6f, 6.3f, -4,
                    0, 0, -2.3f);
                results.Add(line);
                break;
            case "3":
                line = new LineSimple(
                    1.4f, 11.5f, 3.4f,
                    2.2f, 9.4f, 3.2f,
                    2.4f, 6.7f, 3.2f,
                    1.2f, 0, 1.1f);
                results.Add(line);
                line = new LineSimple(
                    -2.7f, 11.5f, 3.4f,
                    -2.9f, 9, 3.2f,
                    -2.6f, 5, 3.2f,
                    -1.2f, 0, 1.1f);
                results.Add(line);
                line = new LineSimple(
                    0, 12.5f, -4.3f,
                    0, 9.4f, -4.3f,
                    0, 0, -2.2f);
                results.Add(line);
                break;
            case "14":
                line = new LineSimple(
                    -1.4f, 11.5f, 3.4f,
                    -2.2f, 9.4f, 3.2f,
                    -2.4f, 6.7f, 3.2f,
                    -1.2f, 0, 1.1f);
                results.Add(line);
                line = new LineSimple(
                    2.7f, 11.5f, 3.4f,
                    2.9f, 9, 3.2f,
                    2.6f, 5, 3.2f,
                    1.2f, 0, 1.1f);
                results.Add(line);
                line = new LineSimple(
                    0, 12.5f, -4.3f,
                    0, 9.4f, -4.3f,
                    0, 0, -2.2f);
                results.Add(line);
                break;
            case "4":
                line = new LineSimple(
                    0, 13.5f, 1.2f,
                    -.2f, 10, .6f,
                    0, 0, 0);
                results.Add(line);
                break;
            case "13":
                line = new LineSimple(
                    0, 13.5f, 1.2f,
                    .2f, 10, .6f,
                    0, 0, 0);
                results.Add(line);
                break;
            case "5":
                line = new LineSimple(
                    -1.1f, 13.5f, 1.6f,
                    0, 6, 1.6f,
                    0, 0, 1);
                results.Add(line);
                break;
            case "12":
                line = new LineSimple(
                    1.1f, 13.5f, 1.6f,
                    0, 6, 1.6f,
                    0, 0, 1);
                results.Add(line);
                break;
            case "6":
                line = new LineSimple(
                    -.4f, 16.5f, 0,
                    0, 11, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "11":
                line = new LineSimple(
                    .4f, 16.5f, 0,
                    0, 11, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "7":
                line = new LineSimple(
                    -.8f, 12.5f, .6f,
                    -.3f, 10, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "10":
                line = new LineSimple(
                    .8f, 12.5f, .6f,
                    .3f, 10, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "8":
                line = new LineSimple(
                    0, 12.6f, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "9":
                line = new LineSimple(
                    0, 12.6f, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "25" or "26":
                line = new LineSimple(
                    -.5f, -12, 0,
                    -.2f, -9, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "24" or "23":
                line = new LineSimple(
                    .5f, -12, 0,
                    .2f, -9, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "27":
                line = new LineSimple(
                    -.5f, -15.5f, 0,
                    -.1f, -13, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "22":
                line = new LineSimple(
                    .5f, -15.5f, 0,
                    .1f, -13, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "28":
                line = new LineSimple(
                    -.2f, -13.5f, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "21":
                line = new LineSimple(
                    .2f, -13.5f, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "29":
                line = new LineSimple(
                    -.3f, -14, 0,
                    0, -12, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "20":
                line = new LineSimple(
                    .3f, -14, 0,
                    0, -12, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "30":
                line = new LineSimple(
                    .9f, -13.5f, 0,
                    2.2f, -10, 0,
                    2.6f, -7, 0,
                    1.7f, 0, 0);
                results.Add(line);
                line = new LineSimple(
                    -4.3f, -13.5f, 0,
                    -4f, -9, 0,
                    -3.3f, -5, 0,
                    -1.7f, 0, 0);
                results.Add(line);
                break;
            case "19":
                line = new LineSimple(
                    -.9f, -13.5f, 0,
                    -2.2f, -10, 0,
                    -2.6f, -7, 0,
                    -1.7f, 0, 0);
                results.Add(line);
                line = new LineSimple(
                    4.3f, -13.5f, 0,
                    4f, -9, 0,
                    3.3f, -5, 0,
                    1.7f, 0, 0);
                results.Add(line);
                break;
            case "31":
                line = new LineSimple(
                    0, -12.5f, 0,
                    1.8f, -7.5f, 0,
                    2.2f, -4, 0,
                    1.7f, 0, 0);
                results.Add(line);
                line = new LineSimple(
                    -3.4f, -12.5f, 0,
                    -3.4f, -8, 0,
                    -3f, -5, 0,
                    -1.7f, 0, 0);
                results.Add(line);
                break;
            case "18":
                line = new LineSimple(
                    0, -12.5f, 0,
                    -1.8f, -7.5f, 0,
                    -2.2f, -4, 0,
                    -1.7f, 0, 0);
                results.Add(line);
                line = new LineSimple(
                    3.4f, -12.5f, 0,
                    3.4f, -8, 0,
                    3f, -5, 0,
                    1.7f, 0, 0);
                results.Add(line);
                break;
            case "32":
                line = new LineSimple(
                    -.7f, -10.5f, 0,
                    .8f, -7.5f, 0,
                    1.7f, -4, 0,
                    1.6f, 0, 0);
                results.Add(line);
                line = new LineSimple(
                    -3.2f, -10.5f, 0,
                    -3.4f, -8, 0,
                    -3f, -5, 0,
                    -1.7f, 0, 0);
                results.Add(line);
                break;
            case "17":
                line = new LineSimple(
                    .7f, -10.5f, 0,
                    -.8f, -7.5f, 0,
                    -1.7f, -4, 0,
                    -1.6f, 0, 0);
                results.Add(line);
                line = new LineSimple(
                    3.2f, -10.5f, 0,
                    3.4f, -8, 0,
                    3f, -5, 0,
                    1.7f, 0, 0);
                results.Add(line);
                break;
            case "A":
                line = new LineSimple(
                    -3.7f, 10.4f, 3.4f,
                    -3.6f, 7f, 3.2f,
                    -3.4f, 6f, 3.2f,
                    -1.5f, 0, 1.1f);
                results.Add(line);
                line = new LineSimple(
                    3.1f, 11.1f, 3.4f,
                    3.3f, 9, 3.2f,
                    3f, 5, 3.2f,
                    1.4f, 0, 1.1f);
                results.Add(line);
                break;
            case "J":
                line = new LineSimple(
                    3.7f, 10.4f, 3.4f,
                    3.6f, 7f, 3.2f,
                    3.4f, 6f, 3.2f,
                    1.5f, 0, 1.1f);
                results.Add(line);
                line = new LineSimple(
                    -3.1f, 11.1f, 3.4f,
                    -3.3f, 9, 3.2f,
                    -3f, 5, 3.2f,
                    -1.4f, 0, 1.1f);
                results.Add(line);
                break;
            case "B":
                line = new LineSimple(
                    -3.7f, 9f, 3.4f,
                    -3.6f, 7f, 3.2f,
                    -3.4f, 6f, 3.2f,
                    -1.5f, 0, 1.1f);
                results.Add(line);
                line = new LineSimple(
                    3f, 9.6f, 3.4f,
                    3.2f, 8.8f, 3.2f,
                    2.9f, 5f, 3.2f,
                    1.5f, 0, 1.1f);
                results.Add(line);
                break;
            case "I":
                line = new LineSimple(
                    3.7f, 9f, 3.4f,
                    3.6f, 7f, 3.2f,
                    3.4f, 6f, 3.2f,
                    1.5f, 0, 1.1f);
                results.Add(line);
                line = new LineSimple(
                    -3f, 9.6f, 3.4f,
                    -3.2f, 8.8f, 3.2f,
                    -2.9f, 5f, 3.2f,
                    -1.5f, 0, 1.1f);
                results.Add(line);
                break;
            case "C":
                line = new LineSimple(
                    0, 5.5f, 0,
                    0, 3, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "H":
                line = new LineSimple(
                    0, 5.5f, 0,
                    0, 3, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "D":
                line = new LineSimple(
                    0, 6.3f, 0,
                    0, 3, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "G":
                line = new LineSimple(
                    0, 6.3f, 0,
                    0, 3, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "E":
                line = new LineSimple(
                    0, 5, 0,
                    0, 3, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "F":
                line = new LineSimple(
                    0, 5, 0,
                    0, 3, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "P" or "Q":
                line = new LineSimple(
                    0, -6.5f, 0,
                    0, -3, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "O" or "N":
                line = new LineSimple(
                    0, -6.5f, 0,
                    0, -3, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "R":
                line = new LineSimple(
                    0, -4.8f, 0,
                    0, -4.5f, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "M":
                line = new LineSimple(
                    0, -4.8f, 0,
                    0, -4.5f, 0,
                    0, 0, 0);
                results.Add(line);
                break;
            case "S":
                line = new LineSimple(
                    -3.7f, -8.8f, 3.4f,
                    -3.6f, -7f, 3.2f,
                    -3.4f, -6f, 3.2f,
                    -1.5f, 0, 1.1f);
                results.Add(line);
                line = new LineSimple(
                    3.2f, -9.4f, 3.4f,
                    3.3f, -7.5f, 3.2f,
                    3f, -5f, 3.2f,
                    1.4f, 0, 1.1f);
                results.Add(line);
                break;
            case "L":
                line = new LineSimple(
                    3.7f, -8.8f, 3.4f,
                    3.6f, -7f, 3.2f,
                    3.4f, -6f, 3.2f,
                    1.5f, 0, 1.1f);
                results.Add(line);
                line = new LineSimple(
                    -3.2f, -9.4f, 3.4f,
                    -3.3f, -7.5f, 3.2f,
                    -3f, -5f, 3.2f,
                    -1.4f, 0, 1.1f);
                results.Add(line);
                break;
            case "T":
                line = new LineSimple(
                    4.5f, -10.8f, 3.4f,
                    4f, -7f, 3.2f,
                    3.5f, -6f, 3.2f,
                    1.5f, 0, 1.1f);
                results.Add(line);
                line = new LineSimple(
                    -5.1f, -10.9f, 3.4f,
                    -4.5f, -9, 3.2f,
                    -3.5f, -5, 3.2f,
                    -1.4f, 0, 1.1f);
                results.Add(line);
                break;
            case "K":
                line = new LineSimple(
                    -4.5f, -10.8f, 3.4f,
                    -4f, -7f, 3.2f,
                    -3.5f, -6f, 3.2f,
                    -1.5f, 0, 1.1f);
                results.Add(line);
                line = new LineSimple(
                    5.1f, -10.9f, 3.4f,
                    4.5f, -9, 3.2f,
                    3.5f, -5, 3.2f,
                    1.4f, 0, 1.1f);
                results.Add(line);
                break;
        }

        return results;
    }


    public LineSimple GetSealantLine()
    {
        if (IsMaxillary(ToothId))
        {
            return new LineSimple(
                1.5f, -4f, 1.5f,
                .75f, -4f, 2.25f,
                -.75f, -4f, 2.25f,
                -1.5f, -4f, 1.5f,
                -1.5f, -4f, .75f,
                1.5f, -4f, -.75f,
                1.5f, -4f, -1.5f,
                .75f, -4f, -2.25f,
                -.75f, -4f, -2.25f,
                -1.5f, -4f, -1.5f);
        }

        return new LineSimple(
            -1.5f, 4f, 1.5f,
            -.75f, 4f, 2.25f,
            .75f, 4f, 2.25f,
            1.5f, 4f, 1.5f,
            1.5f, 4f, .75f,
            -1.5f, 4f, -.75f,
            -1.5f, 4f, -1.5f,
            -.75f, 4f, -2.25f,
            .75f, 4f, -2.25f,
            1.5f, 4f, -1.5f);
    }

    public LineSimple GetWatchLine()
    {
        return new LineSimple(
            0f, 0f, 0f,
            .8f, -2.65f, 0f,
            1.6f, 0f, 0f,
            2.4f, -2.65f, 0f,
            3.2f, 0f, 0f);
    }
}