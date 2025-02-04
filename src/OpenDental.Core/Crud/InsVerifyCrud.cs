using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class InsVerifyCrud
{
    public static InsVerify SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<InsVerify> TableToList(DataTable table)
    {
        var retVal = new List<InsVerify>();
        foreach (DataRow row in table.Rows)
        {
            var insVerify = new InsVerify
            {
                InsVerifyNum = SIn.Long(row["InsVerifyNum"].ToString()),
                DateLastVerified = SIn.Date(row["DateLastVerified"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                VerifyType = (VerifyTypes) SIn.Int(row["VerifyType"].ToString()),
                FKey = SIn.Long(row["FKey"].ToString()),
                DefNum = SIn.Long(row["DefNum"].ToString()),
                DateLastAssigned = SIn.Date(row["DateLastAssigned"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString()),
                HoursAvailableForVerification = SIn.Double(row["HoursAvailableForVerification"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString())
            };
            retVal.Add(insVerify);
        }

        return retVal;
    }

    public static void Insert(InsVerify insVerify)
    {
        var command = "INSERT INTO insverify (";

        command += "DateLastVerified,UserNum,VerifyType,FKey,DefNum,DateLastAssigned,Note,DateTimeEntry,HoursAvailableForVerification) VALUES(";

        command +=
            SOut.Date(insVerify.DateLastVerified) + ","
                                                  + SOut.Long(insVerify.UserNum) + ","
                                                  + SOut.Int((int) insVerify.VerifyType) + ","
                                                  + SOut.Long(insVerify.FKey) + ","
                                                  + SOut.Long(insVerify.DefNum) + ","
                                                  + SOut.Date(insVerify.DateLastAssigned) + ","
                                                  + DbHelper.ParamChar + "paramNote,"
                                                  + "NOW()" + ","
                                                  + SOut.Double(insVerify.HoursAvailableForVerification) + ")";
        //SecDateTEdit can only be set by MySQL
        if (insVerify.Note == null) insVerify.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringNote(insVerify.Note));
        {
            insVerify.InsVerifyNum = Db.NonQ(command, true, "InsVerifyNum", "insVerify", paramNote);
        }
    }

    public static void Update(InsVerify insVerify)
    {
        var command = "UPDATE insverify SET "
                      + "DateLastVerified             =  " + SOut.Date(insVerify.DateLastVerified) + ", "
                      + "UserNum                      =  " + SOut.Long(insVerify.UserNum) + ", "
                      + "VerifyType                   =  " + SOut.Int((int) insVerify.VerifyType) + ", "
                      + "FKey                         =  " + SOut.Long(insVerify.FKey) + ", "
                      + "DefNum                       =  " + SOut.Long(insVerify.DefNum) + ", "
                      + "DateLastAssigned             =  " + SOut.Date(insVerify.DateLastAssigned) + ", "
                      + "Note                         =  " + DbHelper.ParamChar + "paramNote, "
                      //DateTimeEntry not allowed to change
                      + "HoursAvailableForVerification=  " + SOut.Double(insVerify.HoursAvailableForVerification) + " "
                      //SecDateTEdit can only be set by MySQL
                      + "WHERE InsVerifyNum = " + SOut.Long(insVerify.InsVerifyNum);
        if (insVerify.Note == null) insVerify.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringNote(insVerify.Note));
        Db.NonQ(command, paramNote);
    }

    public static void Update(InsVerify insVerify, InsVerify oldInsVerify)
    {
        var command = "";
        if (insVerify.DateLastVerified.Date != oldInsVerify.DateLastVerified.Date)
        {
            if (command != "") command += ",";
            command += "DateLastVerified = " + SOut.Date(insVerify.DateLastVerified) + "";
        }

        if (insVerify.UserNum != oldInsVerify.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(insVerify.UserNum) + "";
        }

        if (insVerify.VerifyType != oldInsVerify.VerifyType)
        {
            if (command != "") command += ",";
            command += "VerifyType = " + SOut.Int((int) insVerify.VerifyType) + "";
        }

        if (insVerify.FKey != oldInsVerify.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(insVerify.FKey) + "";
        }

        if (insVerify.DefNum != oldInsVerify.DefNum)
        {
            if (command != "") command += ",";
            command += "DefNum = " + SOut.Long(insVerify.DefNum) + "";
        }

        if (insVerify.DateLastAssigned.Date != oldInsVerify.DateLastAssigned.Date)
        {
            if (command != "") command += ",";
            command += "DateLastAssigned = " + SOut.Date(insVerify.DateLastAssigned) + "";
        }

        if (insVerify.Note != oldInsVerify.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        //DateTimeEntry not allowed to change
        if (insVerify.HoursAvailableForVerification != oldInsVerify.HoursAvailableForVerification)
        {
            if (command != "") command += ",";
            command += "HoursAvailableForVerification = " + SOut.Double(insVerify.HoursAvailableForVerification) + "";
        }

        //SecDateTEdit can only be set by MySQL
        if (command == "") return;
        if (insVerify.Note == null) insVerify.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringNote(insVerify.Note));
        command = "UPDATE insverify SET " + command
                                          + " WHERE InsVerifyNum = " + SOut.Long(insVerify.InsVerifyNum);
        Db.NonQ(command, paramNote);
    }
}