using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ComputerPrefCrud
{
    public static List<ComputerPref> TableToList(DataTable table)
    {
        var retVal = new List<ComputerPref>();
        ComputerPref computerPref;
        foreach (DataRow row in table.Rows)
        {
            computerPref = new ComputerPref();
            computerPref.ComputerPrefNum = SIn.Long(row["ComputerPrefNum"].ToString());
            computerPref.ComputerName = SIn.String(row["ComputerName"].ToString());
            computerPref.GraphicsUseHardware = SIn.Bool(row["GraphicsUseHardware"].ToString());
            computerPref.GraphicsSimple = (DrawingMode) SIn.Int(row["GraphicsSimple"].ToString());
            computerPref.SensorType = SIn.String(row["SensorType"].ToString());
            computerPref.SensorBinned = SIn.Bool(row["SensorBinned"].ToString());
            computerPref.SensorPort = SIn.Int(row["SensorPort"].ToString());
            computerPref.SensorExposure = SIn.Int(row["SensorExposure"].ToString());
            computerPref.GraphicsDoubleBuffering = SIn.Bool(row["GraphicsDoubleBuffering"].ToString());
            computerPref.PreferredPixelFormatNum = SIn.Int(row["PreferredPixelFormatNum"].ToString());
            computerPref.AtoZpath = SIn.String(row["AtoZpath"].ToString());
            computerPref.TaskKeepListHidden = SIn.Bool(row["TaskKeepListHidden"].ToString());
            computerPref.TaskDock = SIn.Int(row["TaskDock"].ToString());
            computerPref.TaskX = SIn.Int(row["TaskX"].ToString());
            computerPref.TaskY = SIn.Int(row["TaskY"].ToString());
            computerPref.DirectXFormat = SIn.String(row["DirectXFormat"].ToString());
            computerPref.ScanDocSelectSource = SIn.Bool(row["ScanDocSelectSource"].ToString());
            computerPref.ScanDocShowOptions = SIn.Bool(row["ScanDocShowOptions"].ToString());
            computerPref.ScanDocDuplex = SIn.Bool(row["ScanDocDuplex"].ToString());
            computerPref.ScanDocGrayscale = SIn.Bool(row["ScanDocGrayscale"].ToString());
            computerPref.ScanDocResolution = SIn.Int(row["ScanDocResolution"].ToString());
            computerPref.ScanDocQuality = SIn.Byte(row["ScanDocQuality"].ToString());
            computerPref.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            computerPref.ApptViewNum = SIn.Long(row["ApptViewNum"].ToString());
            computerPref.RecentApptView = SIn.Byte(row["RecentApptView"].ToString());
            computerPref.PatSelectSearchMode = (SearchMode) SIn.Int(row["PatSelectSearchMode"].ToString());
            computerPref.NoShowLanguage = SIn.Bool(row["NoShowLanguage"].ToString());
            computerPref.NoShowDecimal = SIn.Bool(row["NoShowDecimal"].ToString());
            var computerOS = row["ComputerOS"].ToString();
            if (computerOS == "")
                computerPref.ComputerOS = 0;
            else
                try
                {
                    computerPref.ComputerOS = (PlatformOD) Enum.Parse(typeof(PlatformOD), computerOS);
                }
                catch
                {
                    computerPref.ComputerOS = 0;
                }

            computerPref.HelpButtonXAdjustment = SIn.Double(row["HelpButtonXAdjustment"].ToString());
            computerPref.GraphicsUseDirectX11 = (YN) SIn.Int(row["GraphicsUseDirectX11"].ToString());
            computerPref.Zoom = SIn.Int(row["Zoom"].ToString());
            computerPref.VideoRectangle = SIn.String(row["VideoRectangle"].ToString());
            retVal.Add(computerPref);
        }

        return retVal;
    }

    public static long Insert(ComputerPref computerPref)
    {
        var command = "INSERT INTO computerpref (";

        command += "ComputerName,GraphicsUseHardware,GraphicsSimple,SensorType,SensorBinned,SensorPort,SensorExposure,GraphicsDoubleBuffering,PreferredPixelFormatNum,AtoZpath,TaskKeepListHidden,TaskDock,TaskX,TaskY,DirectXFormat,ScanDocSelectSource,ScanDocShowOptions,ScanDocDuplex,ScanDocGrayscale,ScanDocResolution,ScanDocQuality,ClinicNum,ApptViewNum,RecentApptView,PatSelectSearchMode,NoShowLanguage,NoShowDecimal,ComputerOS,HelpButtonXAdjustment,GraphicsUseDirectX11,Zoom,VideoRectangle) VALUES(";

        command +=
            "'" + SOut.String(computerPref.ComputerName) + "',"
            + SOut.Bool(computerPref.GraphicsUseHardware) + ","
            + SOut.Int((int) computerPref.GraphicsSimple) + ","
            + "'" + SOut.String(computerPref.SensorType) + "',"
            + SOut.Bool(computerPref.SensorBinned) + ","
            + SOut.Int(computerPref.SensorPort) + ","
            + SOut.Int(computerPref.SensorExposure) + ","
            + SOut.Bool(computerPref.GraphicsDoubleBuffering) + ","
            + SOut.Int(computerPref.PreferredPixelFormatNum) + ","
            + "'" + SOut.String(computerPref.AtoZpath) + "',"
            + SOut.Bool(computerPref.TaskKeepListHidden) + ","
            + SOut.Int(computerPref.TaskDock) + ","
            + SOut.Int(computerPref.TaskX) + ","
            + SOut.Int(computerPref.TaskY) + ","
            + "'" + SOut.String(computerPref.DirectXFormat) + "',"
            + SOut.Bool(computerPref.ScanDocSelectSource) + ","
            + SOut.Bool(computerPref.ScanDocShowOptions) + ","
            + SOut.Bool(computerPref.ScanDocDuplex) + ","
            + SOut.Bool(computerPref.ScanDocGrayscale) + ","
            + SOut.Int(computerPref.ScanDocResolution) + ","
            + SOut.Byte(computerPref.ScanDocQuality) + ","
            + SOut.Long(computerPref.ClinicNum) + ","
            + SOut.Long(computerPref.ApptViewNum) + ","
            + SOut.Byte(computerPref.RecentApptView) + ","
            + SOut.Int((int) computerPref.PatSelectSearchMode) + ","
            + SOut.Bool(computerPref.NoShowLanguage) + ","
            + SOut.Bool(computerPref.NoShowDecimal) + ","
            + "'" + SOut.String(computerPref.ComputerOS.ToString()) + "',"
            + SOut.Double(computerPref.HelpButtonXAdjustment) + ","
            + SOut.Int((int) computerPref.GraphicsUseDirectX11) + ","
            + SOut.Int(computerPref.Zoom) + ","
            + "'" + SOut.String(computerPref.VideoRectangle) + "')";
        {
            computerPref.ComputerPrefNum = Db.NonQ(command, true, "ComputerPrefNum", "computerPref");
        }
        return computerPref.ComputerPrefNum;
    }

    public static void Update(ComputerPref computerPref)
    {
        var command = "UPDATE computerpref SET "
                      + "ComputerName           = '" + SOut.String(computerPref.ComputerName) + "', "
                      + "GraphicsUseHardware    =  " + SOut.Bool(computerPref.GraphicsUseHardware) + ", "
                      + "GraphicsSimple         =  " + SOut.Int((int) computerPref.GraphicsSimple) + ", "
                      + "SensorType             = '" + SOut.String(computerPref.SensorType) + "', "
                      + "SensorBinned           =  " + SOut.Bool(computerPref.SensorBinned) + ", "
                      + "SensorPort             =  " + SOut.Int(computerPref.SensorPort) + ", "
                      + "SensorExposure         =  " + SOut.Int(computerPref.SensorExposure) + ", "
                      + "GraphicsDoubleBuffering=  " + SOut.Bool(computerPref.GraphicsDoubleBuffering) + ", "
                      + "PreferredPixelFormatNum=  " + SOut.Int(computerPref.PreferredPixelFormatNum) + ", "
                      + "AtoZpath               = '" + SOut.String(computerPref.AtoZpath) + "', "
                      + "TaskKeepListHidden     =  " + SOut.Bool(computerPref.TaskKeepListHidden) + ", "
                      + "TaskDock               =  " + SOut.Int(computerPref.TaskDock) + ", "
                      + "TaskX                  =  " + SOut.Int(computerPref.TaskX) + ", "
                      + "TaskY                  =  " + SOut.Int(computerPref.TaskY) + ", "
                      + "DirectXFormat          = '" + SOut.String(computerPref.DirectXFormat) + "', "
                      + "ScanDocSelectSource    =  " + SOut.Bool(computerPref.ScanDocSelectSource) + ", "
                      + "ScanDocShowOptions     =  " + SOut.Bool(computerPref.ScanDocShowOptions) + ", "
                      + "ScanDocDuplex          =  " + SOut.Bool(computerPref.ScanDocDuplex) + ", "
                      + "ScanDocGrayscale       =  " + SOut.Bool(computerPref.ScanDocGrayscale) + ", "
                      + "ScanDocResolution      =  " + SOut.Int(computerPref.ScanDocResolution) + ", "
                      + "ScanDocQuality         =  " + SOut.Byte(computerPref.ScanDocQuality) + ", "
                      + "ClinicNum              =  " + SOut.Long(computerPref.ClinicNum) + ", "
                      + "ApptViewNum            =  " + SOut.Long(computerPref.ApptViewNum) + ", "
                      + "RecentApptView         =  " + SOut.Byte(computerPref.RecentApptView) + ", "
                      + "PatSelectSearchMode    =  " + SOut.Int((int) computerPref.PatSelectSearchMode) + ", "
                      + "NoShowLanguage         =  " + SOut.Bool(computerPref.NoShowLanguage) + ", "
                      + "NoShowDecimal          =  " + SOut.Bool(computerPref.NoShowDecimal) + ", "
                      + "ComputerOS             = '" + SOut.String(computerPref.ComputerOS.ToString()) + "', "
                      + "HelpButtonXAdjustment  =  " + SOut.Double(computerPref.HelpButtonXAdjustment) + ", "
                      + "GraphicsUseDirectX11   =  " + SOut.Int((int) computerPref.GraphicsUseDirectX11) + ", "
                      + "Zoom                   =  " + SOut.Int(computerPref.Zoom) + ", "
                      + "VideoRectangle         = '" + SOut.String(computerPref.VideoRectangle) + "' "
                      + "WHERE ComputerPrefNum = " + SOut.Long(computerPref.ComputerPrefNum);
        Db.NonQ(command);
    }

    public static bool Update(ComputerPref computerPref, ComputerPref oldComputerPref)
    {
        var command = "";
        if (computerPref.ComputerName != oldComputerPref.ComputerName)
        {
            if (command != "") command += ",";
            command += "ComputerName = '" + SOut.String(computerPref.ComputerName) + "'";
        }

        if (computerPref.GraphicsUseHardware != oldComputerPref.GraphicsUseHardware)
        {
            if (command != "") command += ",";
            command += "GraphicsUseHardware = " + SOut.Bool(computerPref.GraphicsUseHardware) + "";
        }

        if (computerPref.GraphicsSimple != oldComputerPref.GraphicsSimple)
        {
            if (command != "") command += ",";
            command += "GraphicsSimple = " + SOut.Int((int) computerPref.GraphicsSimple) + "";
        }

        if (computerPref.SensorType != oldComputerPref.SensorType)
        {
            if (command != "") command += ",";
            command += "SensorType = '" + SOut.String(computerPref.SensorType) + "'";
        }

        if (computerPref.SensorBinned != oldComputerPref.SensorBinned)
        {
            if (command != "") command += ",";
            command += "SensorBinned = " + SOut.Bool(computerPref.SensorBinned) + "";
        }

        if (computerPref.SensorPort != oldComputerPref.SensorPort)
        {
            if (command != "") command += ",";
            command += "SensorPort = " + SOut.Int(computerPref.SensorPort) + "";
        }

        if (computerPref.SensorExposure != oldComputerPref.SensorExposure)
        {
            if (command != "") command += ",";
            command += "SensorExposure = " + SOut.Int(computerPref.SensorExposure) + "";
        }

        if (computerPref.GraphicsDoubleBuffering != oldComputerPref.GraphicsDoubleBuffering)
        {
            if (command != "") command += ",";
            command += "GraphicsDoubleBuffering = " + SOut.Bool(computerPref.GraphicsDoubleBuffering) + "";
        }

        if (computerPref.PreferredPixelFormatNum != oldComputerPref.PreferredPixelFormatNum)
        {
            if (command != "") command += ",";
            command += "PreferredPixelFormatNum = " + SOut.Int(computerPref.PreferredPixelFormatNum) + "";
        }

        if (computerPref.AtoZpath != oldComputerPref.AtoZpath)
        {
            if (command != "") command += ",";
            command += "AtoZpath = '" + SOut.String(computerPref.AtoZpath) + "'";
        }

        if (computerPref.TaskKeepListHidden != oldComputerPref.TaskKeepListHidden)
        {
            if (command != "") command += ",";
            command += "TaskKeepListHidden = " + SOut.Bool(computerPref.TaskKeepListHidden) + "";
        }

        if (computerPref.TaskDock != oldComputerPref.TaskDock)
        {
            if (command != "") command += ",";
            command += "TaskDock = " + SOut.Int(computerPref.TaskDock) + "";
        }

        if (computerPref.TaskX != oldComputerPref.TaskX)
        {
            if (command != "") command += ",";
            command += "TaskX = " + SOut.Int(computerPref.TaskX) + "";
        }

        if (computerPref.TaskY != oldComputerPref.TaskY)
        {
            if (command != "") command += ",";
            command += "TaskY = " + SOut.Int(computerPref.TaskY) + "";
        }

        if (computerPref.DirectXFormat != oldComputerPref.DirectXFormat)
        {
            if (command != "") command += ",";
            command += "DirectXFormat = '" + SOut.String(computerPref.DirectXFormat) + "'";
        }

        if (computerPref.ScanDocSelectSource != oldComputerPref.ScanDocSelectSource)
        {
            if (command != "") command += ",";
            command += "ScanDocSelectSource = " + SOut.Bool(computerPref.ScanDocSelectSource) + "";
        }

        if (computerPref.ScanDocShowOptions != oldComputerPref.ScanDocShowOptions)
        {
            if (command != "") command += ",";
            command += "ScanDocShowOptions = " + SOut.Bool(computerPref.ScanDocShowOptions) + "";
        }

        if (computerPref.ScanDocDuplex != oldComputerPref.ScanDocDuplex)
        {
            if (command != "") command += ",";
            command += "ScanDocDuplex = " + SOut.Bool(computerPref.ScanDocDuplex) + "";
        }

        if (computerPref.ScanDocGrayscale != oldComputerPref.ScanDocGrayscale)
        {
            if (command != "") command += ",";
            command += "ScanDocGrayscale = " + SOut.Bool(computerPref.ScanDocGrayscale) + "";
        }

        if (computerPref.ScanDocResolution != oldComputerPref.ScanDocResolution)
        {
            if (command != "") command += ",";
            command += "ScanDocResolution = " + SOut.Int(computerPref.ScanDocResolution) + "";
        }

        if (computerPref.ScanDocQuality != oldComputerPref.ScanDocQuality)
        {
            if (command != "") command += ",";
            command += "ScanDocQuality = " + SOut.Byte(computerPref.ScanDocQuality) + "";
        }

        if (computerPref.ClinicNum != oldComputerPref.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(computerPref.ClinicNum) + "";
        }

        if (computerPref.ApptViewNum != oldComputerPref.ApptViewNum)
        {
            if (command != "") command += ",";
            command += "ApptViewNum = " + SOut.Long(computerPref.ApptViewNum) + "";
        }

        if (computerPref.RecentApptView != oldComputerPref.RecentApptView)
        {
            if (command != "") command += ",";
            command += "RecentApptView = " + SOut.Byte(computerPref.RecentApptView) + "";
        }

        if (computerPref.PatSelectSearchMode != oldComputerPref.PatSelectSearchMode)
        {
            if (command != "") command += ",";
            command += "PatSelectSearchMode = " + SOut.Int((int) computerPref.PatSelectSearchMode) + "";
        }

        if (computerPref.NoShowLanguage != oldComputerPref.NoShowLanguage)
        {
            if (command != "") command += ",";
            command += "NoShowLanguage = " + SOut.Bool(computerPref.NoShowLanguage) + "";
        }

        if (computerPref.NoShowDecimal != oldComputerPref.NoShowDecimal)
        {
            if (command != "") command += ",";
            command += "NoShowDecimal = " + SOut.Bool(computerPref.NoShowDecimal) + "";
        }

        if (computerPref.ComputerOS != oldComputerPref.ComputerOS)
        {
            if (command != "") command += ",";
            command += "ComputerOS = '" + SOut.String(computerPref.ComputerOS.ToString()) + "'";
        }

        if (computerPref.HelpButtonXAdjustment != oldComputerPref.HelpButtonXAdjustment)
        {
            if (command != "") command += ",";
            command += "HelpButtonXAdjustment = " + SOut.Double(computerPref.HelpButtonXAdjustment) + "";
        }

        if (computerPref.GraphicsUseDirectX11 != oldComputerPref.GraphicsUseDirectX11)
        {
            if (command != "") command += ",";
            command += "GraphicsUseDirectX11 = " + SOut.Int((int) computerPref.GraphicsUseDirectX11) + "";
        }

        if (computerPref.Zoom != oldComputerPref.Zoom)
        {
            if (command != "") command += ",";
            command += "Zoom = " + SOut.Int(computerPref.Zoom) + "";
        }

        if (computerPref.VideoRectangle != oldComputerPref.VideoRectangle)
        {
            if (command != "") command += ",";
            command += "VideoRectangle = '" + SOut.String(computerPref.VideoRectangle) + "'";
        }

        if (command == "") return false;
        command = "UPDATE computerpref SET " + command
                                             + " WHERE ComputerPrefNum = " + SOut.Long(computerPref.ComputerPrefNum);
        Db.NonQ(command);
        return true;
    }
}