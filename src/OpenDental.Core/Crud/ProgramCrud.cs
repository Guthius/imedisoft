using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ProgramCrud
{
    public static List<Program> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Program> TableToList(DataTable table)
    {
        var retVal = new List<Program>();
        foreach (DataRow row in table.Rows)
        {
            var program = new Program
            {
                ProgramNum = SIn.Long(row["ProgramNum"].ToString()),
                ProgName = SIn.String(row["ProgName"].ToString()),
                ProgDesc = SIn.String(row["ProgDesc"].ToString()),
                Enabled = SIn.Bool(row["Enabled"].ToString()),
                Path = SIn.String(row["Path"].ToString()),
                CommandLine = SIn.String(row["CommandLine"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                PluginDllName = SIn.String(row["PluginDllName"].ToString()),
                ButtonImage = SIn.String(row["ButtonImage"].ToString()),
                FileTemplate = SIn.String(row["FileTemplate"].ToString()),
                FilePath = SIn.String(row["FilePath"].ToString()),
                IsDisabledByHq = SIn.Bool(row["IsDisabledByHq"].ToString()),
                CustErr = SIn.String(row["CustErr"].ToString())
            };
            retVal.Add(program);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Program> listPrograms, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Program";
        var table = new DataTable(tableName);
        table.Columns.Add("ProgramNum");
        table.Columns.Add("ProgName");
        table.Columns.Add("ProgDesc");
        table.Columns.Add("Enabled");
        table.Columns.Add("Path");
        table.Columns.Add("CommandLine");
        table.Columns.Add("Note");
        table.Columns.Add("PluginDllName");
        table.Columns.Add("ButtonImage");
        table.Columns.Add("FileTemplate");
        table.Columns.Add("FilePath");
        table.Columns.Add("IsDisabledByHq");
        table.Columns.Add("CustErr");
        foreach (var program in listPrograms)
            table.Rows.Add(SOut.Long(program.ProgramNum), program.ProgName, program.ProgDesc, SOut.Bool(program.Enabled), program.Path, program.CommandLine, program.Note, program.PluginDllName, program.ButtonImage, program.FileTemplate, program.FilePath, SOut.Bool(program.IsDisabledByHq), program.CustErr);
        return table;
    }

    public static void Insert(Program program)
    {
        var command = "INSERT INTO program (";

        command += "ProgName,ProgDesc,Enabled,Path,CommandLine,Note,PluginDllName,ButtonImage,FileTemplate,FilePath,IsDisabledByHq,CustErr) VALUES(";

        command +=
            "'" + SOut.String(program.ProgName) + "',"
            + "'" + SOut.String(program.ProgDesc) + "',"
            + SOut.Bool(program.Enabled) + ","
            + DbHelper.ParamChar + "paramPath,"
            + DbHelper.ParamChar + "paramCommandLine,"
            + DbHelper.ParamChar + "paramNote,"
            + "'" + SOut.String(program.PluginDllName) + "',"
            + DbHelper.ParamChar + "paramButtonImage,"
            + DbHelper.ParamChar + "paramFileTemplate,"
            + "'" + SOut.String(program.FilePath) + "',"
            + SOut.Bool(program.IsDisabledByHq) + ","
            + "'" + SOut.String(program.CustErr) + "')";
        if (program.Path == null) program.Path = "";
        var paramPath = new OdSqlParameter("paramPath", SOut.StringParam(program.Path));
        if (program.CommandLine == null) program.CommandLine = "";
        var paramCommandLine = new OdSqlParameter("paramCommandLine", SOut.StringParam(program.CommandLine));
        if (program.Note == null) program.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(program.Note));
        if (program.ButtonImage == null) program.ButtonImage = "";
        var paramButtonImage = new OdSqlParameter("paramButtonImage", SOut.StringParam(program.ButtonImage));
        if (program.FileTemplate == null) program.FileTemplate = "";
        var paramFileTemplate = new OdSqlParameter("paramFileTemplate", SOut.StringParam(program.FileTemplate));
        {
            program.ProgramNum = Db.NonQ(command, true, "ProgramNum", "program", paramPath, paramCommandLine, paramNote, paramButtonImage, paramFileTemplate);
        }
    }

    public static void Update(Program program)
    {
        var command = "UPDATE program SET "
                      + "ProgName      = '" + SOut.String(program.ProgName) + "', "
                      + "ProgDesc      = '" + SOut.String(program.ProgDesc) + "', "
                      + "Enabled       =  " + SOut.Bool(program.Enabled) + ", "
                      + "Path          =  " + DbHelper.ParamChar + "paramPath, "
                      + "CommandLine   =  " + DbHelper.ParamChar + "paramCommandLine, "
                      + "Note          =  " + DbHelper.ParamChar + "paramNote, "
                      + "PluginDllName = '" + SOut.String(program.PluginDllName) + "', "
                      + "ButtonImage   =  " + DbHelper.ParamChar + "paramButtonImage, "
                      + "FileTemplate  =  " + DbHelper.ParamChar + "paramFileTemplate, "
                      + "FilePath      = '" + SOut.String(program.FilePath) + "', "
                      + "IsDisabledByHq=  " + SOut.Bool(program.IsDisabledByHq) + ", "
                      + "CustErr       = '" + SOut.String(program.CustErr) + "' "
                      + "WHERE ProgramNum = " + SOut.Long(program.ProgramNum);
        if (program.Path == null) program.Path = "";
        var paramPath = new OdSqlParameter("paramPath", SOut.StringParam(program.Path));
        if (program.CommandLine == null) program.CommandLine = "";
        var paramCommandLine = new OdSqlParameter("paramCommandLine", SOut.StringParam(program.CommandLine));
        if (program.Note == null) program.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(program.Note));
        if (program.ButtonImage == null) program.ButtonImage = "";
        var paramButtonImage = new OdSqlParameter("paramButtonImage", SOut.StringParam(program.ButtonImage));
        if (program.FileTemplate == null) program.FileTemplate = "";
        var paramFileTemplate = new OdSqlParameter("paramFileTemplate", SOut.StringParam(program.FileTemplate));
        Db.NonQ(command, paramPath, paramCommandLine, paramNote, paramButtonImage, paramFileTemplate);
    }

    public static bool Update(Program program, Program oldProgram)
    {
        var command = "";
        if (program.ProgName != oldProgram.ProgName)
        {
            if (command != "") command += ",";
            command += "ProgName = '" + SOut.String(program.ProgName) + "'";
        }

        if (program.ProgDesc != oldProgram.ProgDesc)
        {
            if (command != "") command += ",";
            command += "ProgDesc = '" + SOut.String(program.ProgDesc) + "'";
        }

        if (program.Enabled != oldProgram.Enabled)
        {
            if (command != "") command += ",";
            command += "Enabled = " + SOut.Bool(program.Enabled) + "";
        }

        if (program.Path != oldProgram.Path)
        {
            if (command != "") command += ",";
            command += "Path = " + DbHelper.ParamChar + "paramPath";
        }

        if (program.CommandLine != oldProgram.CommandLine)
        {
            if (command != "") command += ",";
            command += "CommandLine = " + DbHelper.ParamChar + "paramCommandLine";
        }

        if (program.Note != oldProgram.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (program.PluginDllName != oldProgram.PluginDllName)
        {
            if (command != "") command += ",";
            command += "PluginDllName = '" + SOut.String(program.PluginDllName) + "'";
        }

        if (program.ButtonImage != oldProgram.ButtonImage)
        {
            if (command != "") command += ",";
            command += "ButtonImage = " + DbHelper.ParamChar + "paramButtonImage";
        }

        if (program.FileTemplate != oldProgram.FileTemplate)
        {
            if (command != "") command += ",";
            command += "FileTemplate = " + DbHelper.ParamChar + "paramFileTemplate";
        }

        if (program.FilePath != oldProgram.FilePath)
        {
            if (command != "") command += ",";
            command += "FilePath = '" + SOut.String(program.FilePath) + "'";
        }

        if (program.IsDisabledByHq != oldProgram.IsDisabledByHq)
        {
            if (command != "") command += ",";
            command += "IsDisabledByHq = " + SOut.Bool(program.IsDisabledByHq) + "";
        }

        if (program.CustErr != oldProgram.CustErr)
        {
            if (command != "") command += ",";
            command += "CustErr = '" + SOut.String(program.CustErr) + "'";
        }

        if (command == "") return false;
        if (program.Path == null) program.Path = "";
        var paramPath = new OdSqlParameter("paramPath", SOut.StringParam(program.Path));
        if (program.CommandLine == null) program.CommandLine = "";
        var paramCommandLine = new OdSqlParameter("paramCommandLine", SOut.StringParam(program.CommandLine));
        if (program.Note == null) program.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(program.Note));
        if (program.ButtonImage == null) program.ButtonImage = "";
        var paramButtonImage = new OdSqlParameter("paramButtonImage", SOut.StringParam(program.ButtonImage));
        if (program.FileTemplate == null) program.FileTemplate = "";
        var paramFileTemplate = new OdSqlParameter("paramFileTemplate", SOut.StringParam(program.FileTemplate));
        command = "UPDATE program SET " + command
                                        + " WHERE ProgramNum = " + SOut.Long(program.ProgramNum);
        Db.NonQ(command, paramPath, paramCommandLine, paramNote, paramButtonImage, paramFileTemplate);
        return true;
    }
}