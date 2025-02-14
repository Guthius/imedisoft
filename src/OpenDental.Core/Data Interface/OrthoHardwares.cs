using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class OrthoHardwares
{
    public enum EnumOrthoWireType
    {
        InBracket,
        BetweenBrackets,
        Elastic
    }

    public static void Insert(OrthoHardware orthoHardware)
    {
        OrthoHardwareCrud.Insert(orthoHardware);
    }

    public static void Update(OrthoHardware orthoHardware)
    {
        OrthoHardwareCrud.Update(orthoHardware);
    }

    public static void Delete(long orthoHardwareNum)
    {
        OrthoHardwareCrud.Delete(orthoHardwareNum);
    }

    public static List<OrthoWire> GetElastics(string toothRange, Color color)
    {
        var listOrthoWires = new List<OrthoWire>();
        if (toothRange.IsNullOrEmpty()) return listOrthoWires;
        var listToothNums = toothRange.Split(',').ToList();
        if (listToothNums.Count < 2) return listOrthoWires;
        for (var i = 0; i < listToothNums.Count - 1; i++)
        {
            //loop does not include last tooth
            if (!Tooth.IsValidDB(listToothNums[i])) return listOrthoWires;
            if (!Tooth.IsValidDB(listToothNums[i + 1])) return listOrthoWires;
            var orthoWire = new OrthoWire();
            orthoWire.EnumOrthoWireType_ = EnumOrthoWireType.Elastic;
            orthoWire.ColorDraw = color;
            orthoWire.ToothIDstart = listToothNums[i];
            orthoWire.ToothIDend = listToothNums[i + 1];
            listOrthoWires.Add(orthoWire);
        }

        return listOrthoWires;
    }

    public static List<OrthoWire> GetWires(string toothRange, Color color)
    {
        var listOrthoWires = new List<OrthoWire>();
        if (toothRange.IsNullOrEmpty()) return listOrthoWires;
        if (toothRange.Contains("-"))
        {
            var listToothNums = toothRange.Split('-').ToList();
            if (listToothNums.Count != 2) return listOrthoWires;
            if (!Tooth.IsValidDB(listToothNums[0])) return listOrthoWires;
            if (!Tooth.IsValidDB(listToothNums[1])) return listOrthoWires;
            var int1 = Tooth.ToInt(listToothNums[0]);
            var int2 = Tooth.ToInt(listToothNums[1]);
            if (int1 == int2) return listOrthoWires;
            if (int1 > int2)
            {
                //flip them
                (int1, int2) = (int2, int1);
            }

            //They will all be in one arch
            for (var i = int1; i <= int2; i++)
            {
                var orthoWire = new OrthoWire();
                orthoWire.EnumOrthoWireType_ = EnumOrthoWireType.InBracket;
                orthoWire.ColorDraw = color;
                orthoWire.ToothIDstart = Tooth.FromInt(i);
                listOrthoWires.Add(orthoWire);
                if (i <= int2 - 1)
                {
                    //this gets skipped on the last item of loop
                    orthoWire = new OrthoWire();
                    orthoWire.EnumOrthoWireType_ = EnumOrthoWireType.BetweenBrackets;
                    orthoWire.ColorDraw = color;
                    orthoWire.ToothIDstart = Tooth.FromInt(i);
                    orthoWire.ToothIDend = Tooth.FromInt(i + 1);
                    listOrthoWires.Add(orthoWire);
                }
            }
        }

        if (toothRange.Contains(","))
        {
        }

        return listOrthoWires;
    }

    public class OrthoWire
    {
        public Color ColorDraw;
        public EnumOrthoWireType EnumOrthoWireType_;
        public string ToothIDend;
        public string ToothIDstart;
    }

    public static List<OrthoHardware> GetPatientData(long patNum)
    {
        var command = "SELECT * FROM orthohardware WHERE PatNum = " + patNum;
        var listOrthoHardwares = OrthoHardwareCrud.SelectMany(command);
        listOrthoHardwares = listOrthoHardwares.OrderBy(x => x.DateExam).ThenBy(x => x.OrthoHardwareType).ThenBy(GetToothInt).ToList();

        return listOrthoHardwares;
    }

    private static int GetToothInt(OrthoHardware orthoHardware)
    {
        if (orthoHardware.OrthoHardwareType == EnumOrthoHardwareType.Bracket)
            if (Tooth.IsValidDB(orthoHardware.ToothRange))
                return Tooth.ToInt(orthoHardware.ToothRange);

        if (orthoHardware.OrthoHardwareType == EnumOrthoHardwareType.Elastic)
        {
            var listToothNums = orthoHardware.ToothRange.Split(',').ToList();
            if (listToothNums.Count < 1) return 0;
            var tooth_id = listToothNums[0];
            if (Tooth.IsValidDB(tooth_id)) return Tooth.ToInt(tooth_id);
        }

        if (orthoHardware.OrthoHardwareType == EnumOrthoHardwareType.Wire)
        {
            var listToothNums = orthoHardware.ToothRange.Split('-').ToList();
            if (listToothNums.Count < 1) return 0;
            var tooth_id = listToothNums[0];
            if (Tooth.IsValidDB(tooth_id)) return Tooth.ToInt(tooth_id);
        }

        return 0;
    }
}