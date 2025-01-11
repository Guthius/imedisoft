#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class HL7DefCrud
{
    public static HL7Def SelectOne(long hL7DefNum)
    {
        var command = "SELECT * FROM hl7def "
                      + "WHERE HL7DefNum = " + SOut.Long(hL7DefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static HL7Def SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<HL7Def> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<HL7Def> TableToList(DataTable table)
    {
        var retVal = new List<HL7Def>();
        HL7Def hL7Def;
        foreach (DataRow row in table.Rows)
        {
            hL7Def = new HL7Def();
            hL7Def.HL7DefNum = SIn.Long(row["HL7DefNum"].ToString());
            hL7Def.Description = SIn.String(row["Description"].ToString());
            hL7Def.ModeTx = (ModeTxHL7) SIn.Int(row["ModeTx"].ToString());
            hL7Def.IncomingFolder = SIn.String(row["IncomingFolder"].ToString());
            hL7Def.OutgoingFolder = SIn.String(row["OutgoingFolder"].ToString());
            hL7Def.IncomingPort = SIn.String(row["IncomingPort"].ToString());
            hL7Def.OutgoingIpPort = SIn.String(row["OutgoingIpPort"].ToString());
            hL7Def.FieldSeparator = SIn.String(row["FieldSeparator"].ToString());
            hL7Def.ComponentSeparator = SIn.String(row["ComponentSeparator"].ToString());
            hL7Def.SubcomponentSeparator = SIn.String(row["SubcomponentSeparator"].ToString());
            hL7Def.RepetitionSeparator = SIn.String(row["RepetitionSeparator"].ToString());
            hL7Def.EscapeCharacter = SIn.String(row["EscapeCharacter"].ToString());
            hL7Def.IsInternal = SIn.Bool(row["IsInternal"].ToString());
            var internalType = row["InternalType"].ToString();
            if (internalType == "")
                hL7Def.InternalType = 0;
            else
                try
                {
                    hL7Def.InternalType = (HL7InternalType) Enum.Parse(typeof(HL7InternalType), internalType);
                }
                catch
                {
                    hL7Def.InternalType = 0;
                }

            hL7Def.InternalTypeVersion = SIn.String(row["InternalTypeVersion"].ToString());
            hL7Def.IsEnabled = SIn.Bool(row["IsEnabled"].ToString());
            hL7Def.Note = SIn.String(row["Note"].ToString());
            hL7Def.HL7Server = SIn.String(row["HL7Server"].ToString());
            hL7Def.HL7ServiceName = SIn.String(row["HL7ServiceName"].ToString());
            hL7Def.ShowDemographics = (HL7ShowDemographics) SIn.Int(row["ShowDemographics"].ToString());
            hL7Def.ShowAppts = SIn.Bool(row["ShowAppts"].ToString());
            hL7Def.ShowAccount = SIn.Bool(row["ShowAccount"].ToString());
            hL7Def.IsQuadAsToothNum = SIn.Bool(row["IsQuadAsToothNum"].ToString());
            hL7Def.LabResultImageCat = SIn.Long(row["LabResultImageCat"].ToString());
            hL7Def.SftpUsername = SIn.String(row["SftpUsername"].ToString());
            hL7Def.SftpPassword = SIn.String(row["SftpPassword"].ToString());
            hL7Def.SftpInSocket = SIn.String(row["SftpInSocket"].ToString());
            hL7Def.HasLongDCodes = SIn.Bool(row["HasLongDCodes"].ToString());
            hL7Def.IsProcApptEnforced = SIn.Bool(row["IsProcApptEnforced"].ToString());
            retVal.Add(hL7Def);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<HL7Def> listHL7Defs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "HL7Def";
        var table = new DataTable(tableName);
        table.Columns.Add("HL7DefNum");
        table.Columns.Add("Description");
        table.Columns.Add("ModeTx");
        table.Columns.Add("IncomingFolder");
        table.Columns.Add("OutgoingFolder");
        table.Columns.Add("IncomingPort");
        table.Columns.Add("OutgoingIpPort");
        table.Columns.Add("FieldSeparator");
        table.Columns.Add("ComponentSeparator");
        table.Columns.Add("SubcomponentSeparator");
        table.Columns.Add("RepetitionSeparator");
        table.Columns.Add("EscapeCharacter");
        table.Columns.Add("IsInternal");
        table.Columns.Add("InternalType");
        table.Columns.Add("InternalTypeVersion");
        table.Columns.Add("IsEnabled");
        table.Columns.Add("Note");
        table.Columns.Add("HL7Server");
        table.Columns.Add("HL7ServiceName");
        table.Columns.Add("ShowDemographics");
        table.Columns.Add("ShowAppts");
        table.Columns.Add("ShowAccount");
        table.Columns.Add("IsQuadAsToothNum");
        table.Columns.Add("LabResultImageCat");
        table.Columns.Add("SftpUsername");
        table.Columns.Add("SftpPassword");
        table.Columns.Add("SftpInSocket");
        table.Columns.Add("HasLongDCodes");
        table.Columns.Add("IsProcApptEnforced");
        foreach (var hL7Def in listHL7Defs)
            table.Rows.Add(SOut.Long(hL7Def.HL7DefNum), hL7Def.Description, SOut.Int((int) hL7Def.ModeTx), hL7Def.IncomingFolder, hL7Def.OutgoingFolder, hL7Def.IncomingPort, hL7Def.OutgoingIpPort, hL7Def.FieldSeparator, hL7Def.ComponentSeparator, hL7Def.SubcomponentSeparator, hL7Def.RepetitionSeparator, hL7Def.EscapeCharacter, SOut.Bool(hL7Def.IsInternal), SOut.Int((int) hL7Def.InternalType), hL7Def.InternalTypeVersion, SOut.Bool(hL7Def.IsEnabled), hL7Def.Note, hL7Def.HL7Server, hL7Def.HL7ServiceName, SOut.Int((int) hL7Def.ShowDemographics), SOut.Bool(hL7Def.ShowAppts), SOut.Bool(hL7Def.ShowAccount), SOut.Bool(hL7Def.IsQuadAsToothNum), SOut.Long(hL7Def.LabResultImageCat), hL7Def.SftpUsername, hL7Def.SftpPassword, hL7Def.SftpInSocket, SOut.Bool(hL7Def.HasLongDCodes), SOut.Bool(hL7Def.IsProcApptEnforced));
        return table;
    }

    public static long Insert(HL7Def hL7Def)
    {
        return Insert(hL7Def, false);
    }

    public static long Insert(HL7Def hL7Def, bool useExistingPK)
    {
        var command = "INSERT INTO hl7def (";

        command += "Description,ModeTx,IncomingFolder,OutgoingFolder,IncomingPort,OutgoingIpPort,FieldSeparator,ComponentSeparator,SubcomponentSeparator,RepetitionSeparator,EscapeCharacter,IsInternal,InternalType,InternalTypeVersion,IsEnabled,Note,HL7Server,HL7ServiceName,ShowDemographics,ShowAppts,ShowAccount,IsQuadAsToothNum,LabResultImageCat,SftpUsername,SftpPassword,SftpInSocket,HasLongDCodes,IsProcApptEnforced) VALUES(";

        command +=
            "'" + SOut.String(hL7Def.Description) + "',"
            + SOut.Int((int) hL7Def.ModeTx) + ","
            + "'" + SOut.String(hL7Def.IncomingFolder) + "',"
            + "'" + SOut.String(hL7Def.OutgoingFolder) + "',"
            + "'" + SOut.String(hL7Def.IncomingPort) + "',"
            + "'" + SOut.String(hL7Def.OutgoingIpPort) + "',"
            + "'" + SOut.String(hL7Def.FieldSeparator) + "',"
            + "'" + SOut.String(hL7Def.ComponentSeparator) + "',"
            + "'" + SOut.String(hL7Def.SubcomponentSeparator) + "',"
            + "'" + SOut.String(hL7Def.RepetitionSeparator) + "',"
            + "'" + SOut.String(hL7Def.EscapeCharacter) + "',"
            + SOut.Bool(hL7Def.IsInternal) + ","
            + "'" + SOut.String(hL7Def.InternalType.ToString()) + "',"
            + "'" + SOut.String(hL7Def.InternalTypeVersion) + "',"
            + SOut.Bool(hL7Def.IsEnabled) + ","
            + DbHelper.ParamChar + "paramNote,"
            + "'" + SOut.String(hL7Def.HL7Server) + "',"
            + "'" + SOut.String(hL7Def.HL7ServiceName) + "',"
            + SOut.Int((int) hL7Def.ShowDemographics) + ","
            + SOut.Bool(hL7Def.ShowAppts) + ","
            + SOut.Bool(hL7Def.ShowAccount) + ","
            + SOut.Bool(hL7Def.IsQuadAsToothNum) + ","
            + SOut.Long(hL7Def.LabResultImageCat) + ","
            + "'" + SOut.String(hL7Def.SftpUsername) + "',"
            + "'" + SOut.String(hL7Def.SftpPassword) + "',"
            + "'" + SOut.String(hL7Def.SftpInSocket) + "',"
            + SOut.Bool(hL7Def.HasLongDCodes) + ","
            + SOut.Bool(hL7Def.IsProcApptEnforced) + ")";
        if (hL7Def.Note == null) hL7Def.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(hL7Def.Note));
        {
            hL7Def.HL7DefNum = Db.NonQ(command, true, "HL7DefNum", "hL7Def", paramNote);
        }
        return hL7Def.HL7DefNum;
    }

    public static long InsertNoCache(HL7Def hL7Def)
    {
        return InsertNoCache(hL7Def, false);
    }

    public static long InsertNoCache(HL7Def hL7Def, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO hl7def (";
        if (isRandomKeys || useExistingPK) command += "HL7DefNum,";
        command += "Description,ModeTx,IncomingFolder,OutgoingFolder,IncomingPort,OutgoingIpPort,FieldSeparator,ComponentSeparator,SubcomponentSeparator,RepetitionSeparator,EscapeCharacter,IsInternal,InternalType,InternalTypeVersion,IsEnabled,Note,HL7Server,HL7ServiceName,ShowDemographics,ShowAppts,ShowAccount,IsQuadAsToothNum,LabResultImageCat,SftpUsername,SftpPassword,SftpInSocket,HasLongDCodes,IsProcApptEnforced) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(hL7Def.HL7DefNum) + ",";
        command +=
            "'" + SOut.String(hL7Def.Description) + "',"
            + SOut.Int((int) hL7Def.ModeTx) + ","
            + "'" + SOut.String(hL7Def.IncomingFolder) + "',"
            + "'" + SOut.String(hL7Def.OutgoingFolder) + "',"
            + "'" + SOut.String(hL7Def.IncomingPort) + "',"
            + "'" + SOut.String(hL7Def.OutgoingIpPort) + "',"
            + "'" + SOut.String(hL7Def.FieldSeparator) + "',"
            + "'" + SOut.String(hL7Def.ComponentSeparator) + "',"
            + "'" + SOut.String(hL7Def.SubcomponentSeparator) + "',"
            + "'" + SOut.String(hL7Def.RepetitionSeparator) + "',"
            + "'" + SOut.String(hL7Def.EscapeCharacter) + "',"
            + SOut.Bool(hL7Def.IsInternal) + ","
            + "'" + SOut.String(hL7Def.InternalType.ToString()) + "',"
            + "'" + SOut.String(hL7Def.InternalTypeVersion) + "',"
            + SOut.Bool(hL7Def.IsEnabled) + ","
            + DbHelper.ParamChar + "paramNote,"
            + "'" + SOut.String(hL7Def.HL7Server) + "',"
            + "'" + SOut.String(hL7Def.HL7ServiceName) + "',"
            + SOut.Int((int) hL7Def.ShowDemographics) + ","
            + SOut.Bool(hL7Def.ShowAppts) + ","
            + SOut.Bool(hL7Def.ShowAccount) + ","
            + SOut.Bool(hL7Def.IsQuadAsToothNum) + ","
            + SOut.Long(hL7Def.LabResultImageCat) + ","
            + "'" + SOut.String(hL7Def.SftpUsername) + "',"
            + "'" + SOut.String(hL7Def.SftpPassword) + "',"
            + "'" + SOut.String(hL7Def.SftpInSocket) + "',"
            + SOut.Bool(hL7Def.HasLongDCodes) + ","
            + SOut.Bool(hL7Def.IsProcApptEnforced) + ")";
        if (hL7Def.Note == null) hL7Def.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(hL7Def.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            hL7Def.HL7DefNum = Db.NonQ(command, true, "HL7DefNum", "hL7Def", paramNote);
        return hL7Def.HL7DefNum;
    }

    public static void Update(HL7Def hL7Def)
    {
        var command = "UPDATE hl7def SET "
                      + "Description          = '" + SOut.String(hL7Def.Description) + "', "
                      + "ModeTx               =  " + SOut.Int((int) hL7Def.ModeTx) + ", "
                      + "IncomingFolder       = '" + SOut.String(hL7Def.IncomingFolder) + "', "
                      + "OutgoingFolder       = '" + SOut.String(hL7Def.OutgoingFolder) + "', "
                      + "IncomingPort         = '" + SOut.String(hL7Def.IncomingPort) + "', "
                      + "OutgoingIpPort       = '" + SOut.String(hL7Def.OutgoingIpPort) + "', "
                      + "FieldSeparator       = '" + SOut.String(hL7Def.FieldSeparator) + "', "
                      + "ComponentSeparator   = '" + SOut.String(hL7Def.ComponentSeparator) + "', "
                      + "SubcomponentSeparator= '" + SOut.String(hL7Def.SubcomponentSeparator) + "', "
                      + "RepetitionSeparator  = '" + SOut.String(hL7Def.RepetitionSeparator) + "', "
                      + "EscapeCharacter      = '" + SOut.String(hL7Def.EscapeCharacter) + "', "
                      + "IsInternal           =  " + SOut.Bool(hL7Def.IsInternal) + ", "
                      + "InternalType         = '" + SOut.String(hL7Def.InternalType.ToString()) + "', "
                      + "InternalTypeVersion  = '" + SOut.String(hL7Def.InternalTypeVersion) + "', "
                      + "IsEnabled            =  " + SOut.Bool(hL7Def.IsEnabled) + ", "
                      + "Note                 =  " + DbHelper.ParamChar + "paramNote, "
                      + "HL7Server            = '" + SOut.String(hL7Def.HL7Server) + "', "
                      + "HL7ServiceName       = '" + SOut.String(hL7Def.HL7ServiceName) + "', "
                      + "ShowDemographics     =  " + SOut.Int((int) hL7Def.ShowDemographics) + ", "
                      + "ShowAppts            =  " + SOut.Bool(hL7Def.ShowAppts) + ", "
                      + "ShowAccount          =  " + SOut.Bool(hL7Def.ShowAccount) + ", "
                      + "IsQuadAsToothNum     =  " + SOut.Bool(hL7Def.IsQuadAsToothNum) + ", "
                      + "LabResultImageCat    =  " + SOut.Long(hL7Def.LabResultImageCat) + ", "
                      + "SftpUsername         = '" + SOut.String(hL7Def.SftpUsername) + "', "
                      + "SftpPassword         = '" + SOut.String(hL7Def.SftpPassword) + "', "
                      + "SftpInSocket         = '" + SOut.String(hL7Def.SftpInSocket) + "', "
                      + "HasLongDCodes        =  " + SOut.Bool(hL7Def.HasLongDCodes) + ", "
                      + "IsProcApptEnforced   =  " + SOut.Bool(hL7Def.IsProcApptEnforced) + " "
                      + "WHERE HL7DefNum = " + SOut.Long(hL7Def.HL7DefNum);
        if (hL7Def.Note == null) hL7Def.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(hL7Def.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(HL7Def hL7Def, HL7Def oldHL7Def)
    {
        var command = "";
        if (hL7Def.Description != oldHL7Def.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(hL7Def.Description) + "'";
        }

        if (hL7Def.ModeTx != oldHL7Def.ModeTx)
        {
            if (command != "") command += ",";
            command += "ModeTx = " + SOut.Int((int) hL7Def.ModeTx) + "";
        }

        if (hL7Def.IncomingFolder != oldHL7Def.IncomingFolder)
        {
            if (command != "") command += ",";
            command += "IncomingFolder = '" + SOut.String(hL7Def.IncomingFolder) + "'";
        }

        if (hL7Def.OutgoingFolder != oldHL7Def.OutgoingFolder)
        {
            if (command != "") command += ",";
            command += "OutgoingFolder = '" + SOut.String(hL7Def.OutgoingFolder) + "'";
        }

        if (hL7Def.IncomingPort != oldHL7Def.IncomingPort)
        {
            if (command != "") command += ",";
            command += "IncomingPort = '" + SOut.String(hL7Def.IncomingPort) + "'";
        }

        if (hL7Def.OutgoingIpPort != oldHL7Def.OutgoingIpPort)
        {
            if (command != "") command += ",";
            command += "OutgoingIpPort = '" + SOut.String(hL7Def.OutgoingIpPort) + "'";
        }

        if (hL7Def.FieldSeparator != oldHL7Def.FieldSeparator)
        {
            if (command != "") command += ",";
            command += "FieldSeparator = '" + SOut.String(hL7Def.FieldSeparator) + "'";
        }

        if (hL7Def.ComponentSeparator != oldHL7Def.ComponentSeparator)
        {
            if (command != "") command += ",";
            command += "ComponentSeparator = '" + SOut.String(hL7Def.ComponentSeparator) + "'";
        }

        if (hL7Def.SubcomponentSeparator != oldHL7Def.SubcomponentSeparator)
        {
            if (command != "") command += ",";
            command += "SubcomponentSeparator = '" + SOut.String(hL7Def.SubcomponentSeparator) + "'";
        }

        if (hL7Def.RepetitionSeparator != oldHL7Def.RepetitionSeparator)
        {
            if (command != "") command += ",";
            command += "RepetitionSeparator = '" + SOut.String(hL7Def.RepetitionSeparator) + "'";
        }

        if (hL7Def.EscapeCharacter != oldHL7Def.EscapeCharacter)
        {
            if (command != "") command += ",";
            command += "EscapeCharacter = '" + SOut.String(hL7Def.EscapeCharacter) + "'";
        }

        if (hL7Def.IsInternal != oldHL7Def.IsInternal)
        {
            if (command != "") command += ",";
            command += "IsInternal = " + SOut.Bool(hL7Def.IsInternal) + "";
        }

        if (hL7Def.InternalType != oldHL7Def.InternalType)
        {
            if (command != "") command += ",";
            command += "InternalType = '" + SOut.String(hL7Def.InternalType.ToString()) + "'";
        }

        if (hL7Def.InternalTypeVersion != oldHL7Def.InternalTypeVersion)
        {
            if (command != "") command += ",";
            command += "InternalTypeVersion = '" + SOut.String(hL7Def.InternalTypeVersion) + "'";
        }

        if (hL7Def.IsEnabled != oldHL7Def.IsEnabled)
        {
            if (command != "") command += ",";
            command += "IsEnabled = " + SOut.Bool(hL7Def.IsEnabled) + "";
        }

        if (hL7Def.Note != oldHL7Def.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (hL7Def.HL7Server != oldHL7Def.HL7Server)
        {
            if (command != "") command += ",";
            command += "HL7Server = '" + SOut.String(hL7Def.HL7Server) + "'";
        }

        if (hL7Def.HL7ServiceName != oldHL7Def.HL7ServiceName)
        {
            if (command != "") command += ",";
            command += "HL7ServiceName = '" + SOut.String(hL7Def.HL7ServiceName) + "'";
        }

        if (hL7Def.ShowDemographics != oldHL7Def.ShowDemographics)
        {
            if (command != "") command += ",";
            command += "ShowDemographics = " + SOut.Int((int) hL7Def.ShowDemographics) + "";
        }

        if (hL7Def.ShowAppts != oldHL7Def.ShowAppts)
        {
            if (command != "") command += ",";
            command += "ShowAppts = " + SOut.Bool(hL7Def.ShowAppts) + "";
        }

        if (hL7Def.ShowAccount != oldHL7Def.ShowAccount)
        {
            if (command != "") command += ",";
            command += "ShowAccount = " + SOut.Bool(hL7Def.ShowAccount) + "";
        }

        if (hL7Def.IsQuadAsToothNum != oldHL7Def.IsQuadAsToothNum)
        {
            if (command != "") command += ",";
            command += "IsQuadAsToothNum = " + SOut.Bool(hL7Def.IsQuadAsToothNum) + "";
        }

        if (hL7Def.LabResultImageCat != oldHL7Def.LabResultImageCat)
        {
            if (command != "") command += ",";
            command += "LabResultImageCat = " + SOut.Long(hL7Def.LabResultImageCat) + "";
        }

        if (hL7Def.SftpUsername != oldHL7Def.SftpUsername)
        {
            if (command != "") command += ",";
            command += "SftpUsername = '" + SOut.String(hL7Def.SftpUsername) + "'";
        }

        if (hL7Def.SftpPassword != oldHL7Def.SftpPassword)
        {
            if (command != "") command += ",";
            command += "SftpPassword = '" + SOut.String(hL7Def.SftpPassword) + "'";
        }

        if (hL7Def.SftpInSocket != oldHL7Def.SftpInSocket)
        {
            if (command != "") command += ",";
            command += "SftpInSocket = '" + SOut.String(hL7Def.SftpInSocket) + "'";
        }

        if (hL7Def.HasLongDCodes != oldHL7Def.HasLongDCodes)
        {
            if (command != "") command += ",";
            command += "HasLongDCodes = " + SOut.Bool(hL7Def.HasLongDCodes) + "";
        }

        if (hL7Def.IsProcApptEnforced != oldHL7Def.IsProcApptEnforced)
        {
            if (command != "") command += ",";
            command += "IsProcApptEnforced = " + SOut.Bool(hL7Def.IsProcApptEnforced) + "";
        }

        if (command == "") return false;
        if (hL7Def.Note == null) hL7Def.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(hL7Def.Note));
        command = "UPDATE hl7def SET " + command
                                       + " WHERE HL7DefNum = " + SOut.Long(hL7Def.HL7DefNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(HL7Def hL7Def, HL7Def oldHL7Def)
    {
        if (hL7Def.Description != oldHL7Def.Description) return true;
        if (hL7Def.ModeTx != oldHL7Def.ModeTx) return true;
        if (hL7Def.IncomingFolder != oldHL7Def.IncomingFolder) return true;
        if (hL7Def.OutgoingFolder != oldHL7Def.OutgoingFolder) return true;
        if (hL7Def.IncomingPort != oldHL7Def.IncomingPort) return true;
        if (hL7Def.OutgoingIpPort != oldHL7Def.OutgoingIpPort) return true;
        if (hL7Def.FieldSeparator != oldHL7Def.FieldSeparator) return true;
        if (hL7Def.ComponentSeparator != oldHL7Def.ComponentSeparator) return true;
        if (hL7Def.SubcomponentSeparator != oldHL7Def.SubcomponentSeparator) return true;
        if (hL7Def.RepetitionSeparator != oldHL7Def.RepetitionSeparator) return true;
        if (hL7Def.EscapeCharacter != oldHL7Def.EscapeCharacter) return true;
        if (hL7Def.IsInternal != oldHL7Def.IsInternal) return true;
        if (hL7Def.InternalType != oldHL7Def.InternalType) return true;
        if (hL7Def.InternalTypeVersion != oldHL7Def.InternalTypeVersion) return true;
        if (hL7Def.IsEnabled != oldHL7Def.IsEnabled) return true;
        if (hL7Def.Note != oldHL7Def.Note) return true;
        if (hL7Def.HL7Server != oldHL7Def.HL7Server) return true;
        if (hL7Def.HL7ServiceName != oldHL7Def.HL7ServiceName) return true;
        if (hL7Def.ShowDemographics != oldHL7Def.ShowDemographics) return true;
        if (hL7Def.ShowAppts != oldHL7Def.ShowAppts) return true;
        if (hL7Def.ShowAccount != oldHL7Def.ShowAccount) return true;
        if (hL7Def.IsQuadAsToothNum != oldHL7Def.IsQuadAsToothNum) return true;
        if (hL7Def.LabResultImageCat != oldHL7Def.LabResultImageCat) return true;
        if (hL7Def.SftpUsername != oldHL7Def.SftpUsername) return true;
        if (hL7Def.SftpPassword != oldHL7Def.SftpPassword) return true;
        if (hL7Def.SftpInSocket != oldHL7Def.SftpInSocket) return true;
        if (hL7Def.HasLongDCodes != oldHL7Def.HasLongDCodes) return true;
        if (hL7Def.IsProcApptEnforced != oldHL7Def.IsProcApptEnforced) return true;
        return false;
    }

    public static void Delete(long hL7DefNum)
    {
        var command = "DELETE FROM hl7def "
                      + "WHERE HL7DefNum = " + SOut.Long(hL7DefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listHL7DefNums)
    {
        if (listHL7DefNums == null || listHL7DefNums.Count == 0) return;
        var command = "DELETE FROM hl7def "
                      + "WHERE HL7DefNum IN(" + string.Join(",", listHL7DefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}