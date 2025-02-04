using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class HL7DefCrud
{
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
        foreach (DataRow row in table.Rows)
        {
            var hL7Def = new HL7Def
            {
                HL7DefNum = SIn.Long(row["HL7DefNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                ModeTx = (ModeTxHL7) SIn.Int(row["ModeTx"].ToString()),
                IncomingFolder = SIn.String(row["IncomingFolder"].ToString()),
                OutgoingFolder = SIn.String(row["OutgoingFolder"].ToString()),
                IncomingPort = SIn.String(row["IncomingPort"].ToString()),
                OutgoingIpPort = SIn.String(row["OutgoingIpPort"].ToString()),
                FieldSeparator = SIn.String(row["FieldSeparator"].ToString()),
                ComponentSeparator = SIn.String(row["ComponentSeparator"].ToString()),
                SubcomponentSeparator = SIn.String(row["SubcomponentSeparator"].ToString()),
                RepetitionSeparator = SIn.String(row["RepetitionSeparator"].ToString()),
                EscapeCharacter = SIn.String(row["EscapeCharacter"].ToString()),
                IsInternal = SIn.Bool(row["IsInternal"].ToString())
            };
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(hL7Def.Note));
        {
            hL7Def.HL7DefNum = Db.NonQ(command, true, "HL7DefNum", "hL7Def", paramNote);
        }
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(hL7Def.Note));
        Db.NonQ(command, paramNote);
    }
}