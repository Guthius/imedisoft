#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ScreenCrud
{
    public static Screen SelectOne(long screenNum)
    {
        var command = "SELECT * FROM screen "
                      + "WHERE ScreenNum = " + SOut.Long(screenNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Screen SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Screen> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Screen> TableToList(DataTable table)
    {
        var retVal = new List<Screen>();
        Screen screen;
        foreach (DataRow row in table.Rows)
        {
            screen = new Screen();
            screen.ScreenNum = SIn.Long(row["ScreenNum"].ToString());
            screen.Gender = (PatientGender) SIn.Int(row["Gender"].ToString());
            screen.RaceOld = (PatientRaceOld) SIn.Int(row["RaceOld"].ToString());
            screen.GradeLevel = (PatientGrade) SIn.Int(row["GradeLevel"].ToString());
            screen.Age = SIn.Byte(row["Age"].ToString());
            screen.Urgency = (TreatmentUrgency) SIn.Int(row["Urgency"].ToString());
            screen.HasCaries = (YN) SIn.Int(row["HasCaries"].ToString());
            screen.NeedsSealants = (YN) SIn.Int(row["NeedsSealants"].ToString());
            screen.CariesExperience = (YN) SIn.Int(row["CariesExperience"].ToString());
            screen.EarlyChildCaries = (YN) SIn.Int(row["EarlyChildCaries"].ToString());
            screen.ExistingSealants = (YN) SIn.Int(row["ExistingSealants"].ToString());
            screen.MissingAllTeeth = (YN) SIn.Int(row["MissingAllTeeth"].ToString());
            screen.Birthdate = SIn.Date(row["Birthdate"].ToString());
            screen.ScreenGroupNum = SIn.Long(row["ScreenGroupNum"].ToString());
            screen.ScreenGroupOrder = SIn.Int(row["ScreenGroupOrder"].ToString());
            screen.Comments = SIn.String(row["Comments"].ToString());
            screen.ScreenPatNum = SIn.Long(row["ScreenPatNum"].ToString());
            screen.SheetNum = SIn.Long(row["SheetNum"].ToString());
            retVal.Add(screen);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Screen> listScreens, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Screen";
        var table = new DataTable(tableName);
        table.Columns.Add("ScreenNum");
        table.Columns.Add("Gender");
        table.Columns.Add("RaceOld");
        table.Columns.Add("GradeLevel");
        table.Columns.Add("Age");
        table.Columns.Add("Urgency");
        table.Columns.Add("HasCaries");
        table.Columns.Add("NeedsSealants");
        table.Columns.Add("CariesExperience");
        table.Columns.Add("EarlyChildCaries");
        table.Columns.Add("ExistingSealants");
        table.Columns.Add("MissingAllTeeth");
        table.Columns.Add("Birthdate");
        table.Columns.Add("ScreenGroupNum");
        table.Columns.Add("ScreenGroupOrder");
        table.Columns.Add("Comments");
        table.Columns.Add("ScreenPatNum");
        table.Columns.Add("SheetNum");
        foreach (var screen in listScreens)
            table.Rows.Add(SOut.Long(screen.ScreenNum), SOut.Int((int) screen.Gender), SOut.Int((int) screen.RaceOld), SOut.Int((int) screen.GradeLevel), SOut.Byte(screen.Age), SOut.Int((int) screen.Urgency), SOut.Int((int) screen.HasCaries), SOut.Int((int) screen.NeedsSealants), SOut.Int((int) screen.CariesExperience), SOut.Int((int) screen.EarlyChildCaries), SOut.Int((int) screen.ExistingSealants), SOut.Int((int) screen.MissingAllTeeth), SOut.DateT(screen.Birthdate, false), SOut.Long(screen.ScreenGroupNum), SOut.Int(screen.ScreenGroupOrder), screen.Comments, SOut.Long(screen.ScreenPatNum), SOut.Long(screen.SheetNum));
        return table;
    }

    public static long Insert(Screen screen)
    {
        return Insert(screen, false);
    }

    public static long Insert(Screen screen, bool useExistingPK)
    {
        var command = "INSERT INTO screen (";

        command += "Gender,RaceOld,GradeLevel,Age,Urgency,HasCaries,NeedsSealants,CariesExperience,EarlyChildCaries,ExistingSealants,MissingAllTeeth,Birthdate,ScreenGroupNum,ScreenGroupOrder,Comments,ScreenPatNum,SheetNum) VALUES(";

        command +=
            SOut.Int((int) screen.Gender) + ","
                                          + SOut.Int((int) screen.RaceOld) + ","
                                          + SOut.Int((int) screen.GradeLevel) + ","
                                          + SOut.Byte(screen.Age) + ","
                                          + SOut.Int((int) screen.Urgency) + ","
                                          + SOut.Int((int) screen.HasCaries) + ","
                                          + SOut.Int((int) screen.NeedsSealants) + ","
                                          + SOut.Int((int) screen.CariesExperience) + ","
                                          + SOut.Int((int) screen.EarlyChildCaries) + ","
                                          + SOut.Int((int) screen.ExistingSealants) + ","
                                          + SOut.Int((int) screen.MissingAllTeeth) + ","
                                          + SOut.Date(screen.Birthdate) + ","
                                          + SOut.Long(screen.ScreenGroupNum) + ","
                                          + SOut.Int(screen.ScreenGroupOrder) + ","
                                          + "'" + SOut.String(screen.Comments) + "',"
                                          + SOut.Long(screen.ScreenPatNum) + ","
                                          + SOut.Long(screen.SheetNum) + ")";
        {
            screen.ScreenNum = Db.NonQ(command, true, "ScreenNum", "screen");
        }
        return screen.ScreenNum;
    }

    public static long InsertNoCache(Screen screen)
    {
        return InsertNoCache(screen, false);
    }

    public static long InsertNoCache(Screen screen, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO screen (";
        if (isRandomKeys || useExistingPK) command += "ScreenNum,";
        command += "Gender,RaceOld,GradeLevel,Age,Urgency,HasCaries,NeedsSealants,CariesExperience,EarlyChildCaries,ExistingSealants,MissingAllTeeth,Birthdate,ScreenGroupNum,ScreenGroupOrder,Comments,ScreenPatNum,SheetNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(screen.ScreenNum) + ",";
        command +=
            SOut.Int((int) screen.Gender) + ","
                                          + SOut.Int((int) screen.RaceOld) + ","
                                          + SOut.Int((int) screen.GradeLevel) + ","
                                          + SOut.Byte(screen.Age) + ","
                                          + SOut.Int((int) screen.Urgency) + ","
                                          + SOut.Int((int) screen.HasCaries) + ","
                                          + SOut.Int((int) screen.NeedsSealants) + ","
                                          + SOut.Int((int) screen.CariesExperience) + ","
                                          + SOut.Int((int) screen.EarlyChildCaries) + ","
                                          + SOut.Int((int) screen.ExistingSealants) + ","
                                          + SOut.Int((int) screen.MissingAllTeeth) + ","
                                          + SOut.Date(screen.Birthdate) + ","
                                          + SOut.Long(screen.ScreenGroupNum) + ","
                                          + SOut.Int(screen.ScreenGroupOrder) + ","
                                          + "'" + SOut.String(screen.Comments) + "',"
                                          + SOut.Long(screen.ScreenPatNum) + ","
                                          + SOut.Long(screen.SheetNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            screen.ScreenNum = Db.NonQ(command, true, "ScreenNum", "screen");
        return screen.ScreenNum;
    }

    public static void Update(Screen screen)
    {
        var command = "UPDATE screen SET "
                      + "Gender          =  " + SOut.Int((int) screen.Gender) + ", "
                      + "RaceOld         =  " + SOut.Int((int) screen.RaceOld) + ", "
                      + "GradeLevel      =  " + SOut.Int((int) screen.GradeLevel) + ", "
                      + "Age             =  " + SOut.Byte(screen.Age) + ", "
                      + "Urgency         =  " + SOut.Int((int) screen.Urgency) + ", "
                      + "HasCaries       =  " + SOut.Int((int) screen.HasCaries) + ", "
                      + "NeedsSealants   =  " + SOut.Int((int) screen.NeedsSealants) + ", "
                      + "CariesExperience=  " + SOut.Int((int) screen.CariesExperience) + ", "
                      + "EarlyChildCaries=  " + SOut.Int((int) screen.EarlyChildCaries) + ", "
                      + "ExistingSealants=  " + SOut.Int((int) screen.ExistingSealants) + ", "
                      + "MissingAllTeeth =  " + SOut.Int((int) screen.MissingAllTeeth) + ", "
                      + "Birthdate       =  " + SOut.Date(screen.Birthdate) + ", "
                      + "ScreenGroupNum  =  " + SOut.Long(screen.ScreenGroupNum) + ", "
                      + "ScreenGroupOrder=  " + SOut.Int(screen.ScreenGroupOrder) + ", "
                      + "Comments        = '" + SOut.String(screen.Comments) + "', "
                      + "ScreenPatNum    =  " + SOut.Long(screen.ScreenPatNum) + ", "
                      + "SheetNum        =  " + SOut.Long(screen.SheetNum) + " "
                      + "WHERE ScreenNum = " + SOut.Long(screen.ScreenNum);
        Db.NonQ(command);
    }

    public static bool Update(Screen screen, Screen oldScreen)
    {
        var command = "";
        if (screen.Gender != oldScreen.Gender)
        {
            if (command != "") command += ",";
            command += "Gender = " + SOut.Int((int) screen.Gender) + "";
        }

        if (screen.RaceOld != oldScreen.RaceOld)
        {
            if (command != "") command += ",";
            command += "RaceOld = " + SOut.Int((int) screen.RaceOld) + "";
        }

        if (screen.GradeLevel != oldScreen.GradeLevel)
        {
            if (command != "") command += ",";
            command += "GradeLevel = " + SOut.Int((int) screen.GradeLevel) + "";
        }

        if (screen.Age != oldScreen.Age)
        {
            if (command != "") command += ",";
            command += "Age = " + SOut.Byte(screen.Age) + "";
        }

        if (screen.Urgency != oldScreen.Urgency)
        {
            if (command != "") command += ",";
            command += "Urgency = " + SOut.Int((int) screen.Urgency) + "";
        }

        if (screen.HasCaries != oldScreen.HasCaries)
        {
            if (command != "") command += ",";
            command += "HasCaries = " + SOut.Int((int) screen.HasCaries) + "";
        }

        if (screen.NeedsSealants != oldScreen.NeedsSealants)
        {
            if (command != "") command += ",";
            command += "NeedsSealants = " + SOut.Int((int) screen.NeedsSealants) + "";
        }

        if (screen.CariesExperience != oldScreen.CariesExperience)
        {
            if (command != "") command += ",";
            command += "CariesExperience = " + SOut.Int((int) screen.CariesExperience) + "";
        }

        if (screen.EarlyChildCaries != oldScreen.EarlyChildCaries)
        {
            if (command != "") command += ",";
            command += "EarlyChildCaries = " + SOut.Int((int) screen.EarlyChildCaries) + "";
        }

        if (screen.ExistingSealants != oldScreen.ExistingSealants)
        {
            if (command != "") command += ",";
            command += "ExistingSealants = " + SOut.Int((int) screen.ExistingSealants) + "";
        }

        if (screen.MissingAllTeeth != oldScreen.MissingAllTeeth)
        {
            if (command != "") command += ",";
            command += "MissingAllTeeth = " + SOut.Int((int) screen.MissingAllTeeth) + "";
        }

        if (screen.Birthdate.Date != oldScreen.Birthdate.Date)
        {
            if (command != "") command += ",";
            command += "Birthdate = " + SOut.Date(screen.Birthdate) + "";
        }

        if (screen.ScreenGroupNum != oldScreen.ScreenGroupNum)
        {
            if (command != "") command += ",";
            command += "ScreenGroupNum = " + SOut.Long(screen.ScreenGroupNum) + "";
        }

        if (screen.ScreenGroupOrder != oldScreen.ScreenGroupOrder)
        {
            if (command != "") command += ",";
            command += "ScreenGroupOrder = " + SOut.Int(screen.ScreenGroupOrder) + "";
        }

        if (screen.Comments != oldScreen.Comments)
        {
            if (command != "") command += ",";
            command += "Comments = '" + SOut.String(screen.Comments) + "'";
        }

        if (screen.ScreenPatNum != oldScreen.ScreenPatNum)
        {
            if (command != "") command += ",";
            command += "ScreenPatNum = " + SOut.Long(screen.ScreenPatNum) + "";
        }

        if (screen.SheetNum != oldScreen.SheetNum)
        {
            if (command != "") command += ",";
            command += "SheetNum = " + SOut.Long(screen.SheetNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE screen SET " + command
                                       + " WHERE ScreenNum = " + SOut.Long(screen.ScreenNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Screen screen, Screen oldScreen)
    {
        if (screen.Gender != oldScreen.Gender) return true;
        if (screen.RaceOld != oldScreen.RaceOld) return true;
        if (screen.GradeLevel != oldScreen.GradeLevel) return true;
        if (screen.Age != oldScreen.Age) return true;
        if (screen.Urgency != oldScreen.Urgency) return true;
        if (screen.HasCaries != oldScreen.HasCaries) return true;
        if (screen.NeedsSealants != oldScreen.NeedsSealants) return true;
        if (screen.CariesExperience != oldScreen.CariesExperience) return true;
        if (screen.EarlyChildCaries != oldScreen.EarlyChildCaries) return true;
        if (screen.ExistingSealants != oldScreen.ExistingSealants) return true;
        if (screen.MissingAllTeeth != oldScreen.MissingAllTeeth) return true;
        if (screen.Birthdate.Date != oldScreen.Birthdate.Date) return true;
        if (screen.ScreenGroupNum != oldScreen.ScreenGroupNum) return true;
        if (screen.ScreenGroupOrder != oldScreen.ScreenGroupOrder) return true;
        if (screen.Comments != oldScreen.Comments) return true;
        if (screen.ScreenPatNum != oldScreen.ScreenPatNum) return true;
        if (screen.SheetNum != oldScreen.SheetNum) return true;
        return false;
    }

    public static void Delete(long screenNum)
    {
        var command = "DELETE FROM screen "
                      + "WHERE ScreenNum = " + SOut.Long(screenNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listScreenNums)
    {
        if (listScreenNums == null || listScreenNums.Count == 0) return;
        var command = "DELETE FROM screen "
                      + "WHERE ScreenNum IN(" + string.Join(",", listScreenNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}