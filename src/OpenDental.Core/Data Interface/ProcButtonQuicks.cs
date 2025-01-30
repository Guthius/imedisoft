using System.Collections.Generic;
using System.Globalization;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ProcButtonQuicks
{
    public static List<ProcButtonQuick> GetAll()
    {
        var command = "SELECT * FROM procbuttonquick";
        return ProcButtonQuickCrud.SelectMany(command);
    }

    public static int sortYX(ProcButtonQuick p1, ProcButtonQuick p2)
    {
        //#error Move this to the S class once it is generated.
        if (p1.YPos != p2.YPos) return p1.YPos.CompareTo(p2.YPos);
        return p1.ItemOrder.CompareTo(p2.ItemOrder);
    }
    
    public static void Insert(ProcButtonQuick procButtonQuick)
    {
        ProcButtonQuickCrud.Insert(procButtonQuick);
    }
    
    public static void Update(ProcButtonQuick procButtonQuick)
    {
        ProcButtonQuickCrud.Update(procButtonQuick);
    }

    public static void SetToDefault()
    {
        var command = "DELETE FROM procbuttonquick";
        Db.NonQ(command);
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA")) SetToDefaultMySQLCanada();
        SetToDefaultMySQL();
    }

    public static void InsertNewProcQuickButton(string description, string codeValue, string surf, int yPos, int itemOrder, bool isLabel)
    {
        var quickButton = new ProcButtonQuick();
        quickButton.Description = description;
        quickButton.CodeValue = codeValue;
        quickButton.Surf = surf;
        quickButton.YPos = yPos;
        quickButton.ItemOrder = itemOrder;
        quickButton.IsLabel = isLabel;
        Insert(quickButton);
    }

    public static void SetToDefaultMySQL()
    {
        if (ProcedureCodes.IsValidCode("D2392") || ProcedureCodes.IsValidCode("D2393") || ProcedureCodes.IsValidCode("D2391")) InsertNewProcQuickButton("Post. Composite", "", "", 0, 0, true);
        if (ProcedureCodes.IsValidCode("D2392")) InsertNewProcQuickButton("MO", "D2392", "MO", 1, 0, false);
        if (ProcedureCodes.IsValidCode("D2393")) InsertNewProcQuickButton("MOD", "D2393", "MOD", 1, 1, false);
        if (ProcedureCodes.IsValidCode("D2391")) InsertNewProcQuickButton("O", "D2391", "O", 1, 2, false);
        if (ProcedureCodes.IsValidCode("D2392")) InsertNewProcQuickButton("DO", "D2392", "DO", 1, 3, false);
        if (ProcedureCodes.IsValidCode("D1351")) InsertNewProcQuickButton("", "", "", 1, 4, true);
        if (ProcedureCodes.IsValidCode("D1351")) InsertNewProcQuickButton("Seal", "D1351", "", 1, 5, false);
        if (ProcedureCodes.IsValidCode("D2392")) InsertNewProcQuickButton("OL", "D2392", "OL", 2, 0, false);
        if (ProcedureCodes.IsValidCode("D2392")) InsertNewProcQuickButton("OB", "D2392", "OB", 2, 1, false);
        if (ProcedureCodes.IsValidCode("D2394")) InsertNewProcQuickButton("MODL", "D2394", "MODL", 2, 2, false);
        if (ProcedureCodes.IsValidCode("D2394")) InsertNewProcQuickButton("MODB", "D2394", "MODB", 2, 3, false);
        if (ProcedureCodes.IsValidCode("D2331") || ProcedureCodes.IsValidCode("D2332") || ProcedureCodes.IsValidCode("D2391")) InsertNewProcQuickButton("Ant. Composite", "", "", 4, 0, true);
        if (ProcedureCodes.IsValidCode("D2331")) InsertNewProcQuickButton("DL", "D2331", "DL", 5, 0, false);
        if (ProcedureCodes.IsValidCode("D2332")) InsertNewProcQuickButton("MDL", "D2332", "MDL", 5, 1, false);
        if (ProcedureCodes.IsValidCode("D2331")) InsertNewProcQuickButton("ML", "D2331", "ML", 5, 2, false);
        if (ProcedureCodes.IsValidCode("D2150") || ProcedureCodes.IsValidCode("D2160") || ProcedureCodes.IsValidCode("D2140") || ProcedureCodes.IsValidCode("D21615")) InsertNewProcQuickButton("Amalgam", "", "", 7, 0, true);
        if (ProcedureCodes.IsValidCode("D2150")) InsertNewProcQuickButton("MO", "D2150", "MO", 8, 0, false);
        if (ProcedureCodes.IsValidCode("D2160")) InsertNewProcQuickButton("MOD", "D2160", "MOD", 8, 1, false);
        if (ProcedureCodes.IsValidCode("D2140")) InsertNewProcQuickButton("O", "D2140", "O", 8, 2, false);
        if (ProcedureCodes.IsValidCode("D2150")) InsertNewProcQuickButton("DO", "D2150", "DO", 8, 3, false);
        if (ProcedureCodes.IsValidCode("D2150")) InsertNewProcQuickButton("OL", "D2150", "OL", 9, 0, false);
        if (ProcedureCodes.IsValidCode("D2150")) InsertNewProcQuickButton("OB", "D2150", "OB", 9, 1, false);
        if (ProcedureCodes.IsValidCode("D2161")) InsertNewProcQuickButton("MODL", "D2161", "MODL", 9, 2, false);
        if (ProcedureCodes.IsValidCode("D2161")) InsertNewProcQuickButton("MODB", "D2161", "MODB", 9, 3, false);
    }

    public static void SetToDefaultMySQLCanada()
    {
        string command;
        //1 - Molar Composites
        if (ProcedureCodes.IsValidCode("23321") || ProcedureCodes.IsValidCode("23322") || ProcedureCodes.IsValidCode("23323")
            || ProcedureCodes.IsValidCode("23324") || ProcedureCodes.IsValidCode("23325"))
        {
            command = "INSERT INTO procbuttonquick (Description, YPos, ItemOrder, IsLabel) VALUES ('Molar Composite',0,0,1)";
            Db.NonQ(command);
            if (ProcedureCodes.IsValidCode("23321"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('1S','23321',1,0,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("23322"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('2S','23322',1,1,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("23323"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('3S','23323',1,2,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("23324"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('4S','23324',1,3,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("23325"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('5S','23325',1,4,0)";
                Db.NonQ(command);
            }
        }

        //2 - Bicuspid Composite
        if (ProcedureCodes.IsValidCode("23311") || ProcedureCodes.IsValidCode("23312") || ProcedureCodes.IsValidCode("23313")
            || ProcedureCodes.IsValidCode("23314") || ProcedureCodes.IsValidCode("23315"))
        {
            command = "INSERT INTO procbuttonquick (Description, YPos, ItemOrder, IsLabel) VALUES ('Biscuspid Composite',2,0,1)";
            Db.NonQ(command);
            if (ProcedureCodes.IsValidCode("23311"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('1S','23311',3,0,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("23312"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('2S','23312',3,1,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("23313"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('3S','23313',3,2,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("23314"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('4S','23314',3,3,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("23315"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('5S','23315',3,4,0)";
                Db.NonQ(command);
            }
        }

        //3 - Anterior Composite
        if (ProcedureCodes.IsValidCode("23111") || ProcedureCodes.IsValidCode("23112") || ProcedureCodes.IsValidCode("23113")
            || ProcedureCodes.IsValidCode("23114") || ProcedureCodes.IsValidCode("23115"))
        {
            command = "INSERT INTO procbuttonquick (Description, YPos, ItemOrder, IsLabel) VALUES ('Anterior Composite',4,0,1)";
            Db.NonQ(command);
            if (ProcedureCodes.IsValidCode("23111"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('1S','23111',5,0,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("23112"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('2S','23112',5,1,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("23113"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('3S','23113',5,2,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("23114"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('4S','23114',5,3,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("23115"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('5S','23115',5,4,0)";
                Db.NonQ(command);
            }
        }

        //4 - Molar Amalgum
        if (ProcedureCodes.IsValidCode("21221") || ProcedureCodes.IsValidCode("21222") || ProcedureCodes.IsValidCode("21223")
            || ProcedureCodes.IsValidCode("21224") || ProcedureCodes.IsValidCode("21225"))
        {
            command = "INSERT INTO procbuttonquick (Description, YPos, ItemOrder, IsLabel) VALUES ('Molar Amalgam',6,0,1)";
            Db.NonQ(command);
            if (ProcedureCodes.IsValidCode("21221"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('1S','21221',7,0,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("21222"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('2S','21222',7,1,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("21223"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('3S','21223',7,2,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("21224"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('4S','21224',7,3,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("21225"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('5S','21225',7,4,0)";
                Db.NonQ(command);
            }
        }

        //5 - Anterior/Bicuspid
        if (ProcedureCodes.IsValidCode("21211") || ProcedureCodes.IsValidCode("21212") || ProcedureCodes.IsValidCode("21213")
            || ProcedureCodes.IsValidCode("21214") || ProcedureCodes.IsValidCode("21215"))
        {
            command = "INSERT INTO procbuttonquick (Description, YPos, ItemOrder, IsLabel) VALUES ('Anterior/Bicuspid Amalgam',8,0,1)";
            Db.NonQ(command);
            if (ProcedureCodes.IsValidCode("21211"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('1S','21211',9,0,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("21212"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('2S','21212',9,1,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("21213"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('3S','21213',9,2,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("21214"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('4S','21214',9,3,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("21215"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('5S','21215',9,4,0)";
                Db.NonQ(command);
            }
        }

        //6 - Watch
        if (ProcedureCodes.IsValidCode("Ztoth") || ProcedureCodes.IsValidCode("Ztoths"))
        {
            command = "INSERT INTO procbuttonquick (Description, YPos, ItemOrder, IsLabel) VALUES ('Watch',7,6,1)";
            Db.NonQ(command);
            if (ProcedureCodes.IsValidCode("Ztoth"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('Tooth','Ztoth',8,2,0)";
                Db.NonQ(command);
            }

            if (ProcedureCodes.IsValidCode("Ztoths"))
            {
                command = "INSERT INTO procbuttonquick (Description, CodeValue, YPos, ItemOrder, IsLabel) VALUES ('Surface','Ztoths',9,6,0)";
                Db.NonQ(command);
            }
        }

        //Spacing
        command = "INSERT INTO procbuttonquick (YPos, ItemOrder, IsLabel) VALUES (9,5,1)";
        Db.NonQ(command);
        command = "INSERT INTO procbuttonquick (YPos, ItemOrder, IsLabel) VALUES (7,5,1)";
        Db.NonQ(command);
        command = "INSERT INTO procbuttonquick (YPos, ItemOrder, IsLabel) VALUES (1,5,1)";
        Db.NonQ(command);
        command = "INSERT INTO procbuttonquick (YPos, ItemOrder, IsLabel) VALUES (3,5,1)";
        Db.NonQ(command);
        command = "INSERT INTO procbuttonquick (YPos, ItemOrder, IsLabel) VALUES (5,5,1)";
        Db.NonQ(command);
        command = "INSERT INTO procbuttonquick (YPos, ItemOrder, IsLabel) VALUES (2,1,1)";
        Db.NonQ(command);
        command = "INSERT INTO procbuttonquick (YPos, ItemOrder, IsLabel) VALUES (4,1,1)";
        Db.NonQ(command);
        command = "INSERT INTO procbuttonquick (YPos, ItemOrder, IsLabel) VALUES (0,1,1)";
        Db.NonQ(command);
        command = "INSERT INTO procbuttonquick (YPos, ItemOrder, IsLabel) VALUES (6,1,1)";
        Db.NonQ(command);
    }

    public static void ValidateAll()
    {
    }
    
    public static void Delete(long procButtonQuickNum)
    {
        var command = "DELETE FROM procbuttonquick WHERE ProcButtonQuickNum = " + SOut.Long(procButtonQuickNum);
        Db.NonQ(command);
    }
}