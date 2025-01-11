#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PrinterCrud
{
    public static Printer SelectOne(long printerNum)
    {
        var command = "SELECT * FROM printer "
                      + "WHERE PrinterNum = " + SOut.Long(printerNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Printer SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Printer> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Printer> TableToList(DataTable table)
    {
        var retVal = new List<Printer>();
        Printer printer;
        foreach (DataRow row in table.Rows)
        {
            printer = new Printer();
            printer.PrinterNum = SIn.Long(row["PrinterNum"].ToString());
            printer.ComputerNum = SIn.Long(row["ComputerNum"].ToString());
            printer.PrintSit = (PrintSituation) SIn.Int(row["PrintSit"].ToString());
            printer.PrinterName = SIn.String(row["PrinterName"].ToString());
            printer.DisplayPrompt = SIn.Bool(row["DisplayPrompt"].ToString());
            printer.FileExtension = SIn.String(row["FileExtension"].ToString());
            printer.IsVirtualPrinter = SIn.Bool(row["IsVirtualPrinter"].ToString());
            retVal.Add(printer);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Printer> listPrinters, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Printer";
        var table = new DataTable(tableName);
        table.Columns.Add("PrinterNum");
        table.Columns.Add("ComputerNum");
        table.Columns.Add("PrintSit");
        table.Columns.Add("PrinterName");
        table.Columns.Add("DisplayPrompt");
        table.Columns.Add("FileExtension");
        table.Columns.Add("IsVirtualPrinter");
        foreach (var printer in listPrinters)
            table.Rows.Add(SOut.Long(printer.PrinterNum), SOut.Long(printer.ComputerNum), SOut.Int((int) printer.PrintSit), printer.PrinterName, SOut.Bool(printer.DisplayPrompt), printer.FileExtension, SOut.Bool(printer.IsVirtualPrinter));
        return table;
    }

    public static long Insert(Printer printer)
    {
        return Insert(printer, false);
    }

    public static long Insert(Printer printer, bool useExistingPK)
    {
        var command = "INSERT INTO printer (";

        command += "ComputerNum,PrintSit,PrinterName,DisplayPrompt,FileExtension,IsVirtualPrinter) VALUES(";

        command +=
            SOut.Long(printer.ComputerNum) + ","
                                           + SOut.Int((int) printer.PrintSit) + ","
                                           + "'" + SOut.String(printer.PrinterName) + "',"
                                           + SOut.Bool(printer.DisplayPrompt) + ","
                                           + "'" + SOut.String(printer.FileExtension) + "',"
                                           + SOut.Bool(printer.IsVirtualPrinter) + ")";
        {
            printer.PrinterNum = Db.NonQ(command, true, "PrinterNum", "printer");
        }
        return printer.PrinterNum;
    }

    public static long InsertNoCache(Printer printer)
    {
        return InsertNoCache(printer, false);
    }

    public static long InsertNoCache(Printer printer, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO printer (";
        if (isRandomKeys || useExistingPK) command += "PrinterNum,";
        command += "ComputerNum,PrintSit,PrinterName,DisplayPrompt,FileExtension,IsVirtualPrinter) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(printer.PrinterNum) + ",";
        command +=
            SOut.Long(printer.ComputerNum) + ","
                                           + SOut.Int((int) printer.PrintSit) + ","
                                           + "'" + SOut.String(printer.PrinterName) + "',"
                                           + SOut.Bool(printer.DisplayPrompt) + ","
                                           + "'" + SOut.String(printer.FileExtension) + "',"
                                           + SOut.Bool(printer.IsVirtualPrinter) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            printer.PrinterNum = Db.NonQ(command, true, "PrinterNum", "printer");
        return printer.PrinterNum;
    }

    public static void Update(Printer printer)
    {
        var command = "UPDATE printer SET "
                      + "ComputerNum     =  " + SOut.Long(printer.ComputerNum) + ", "
                      + "PrintSit        =  " + SOut.Int((int) printer.PrintSit) + ", "
                      + "PrinterName     = '" + SOut.String(printer.PrinterName) + "', "
                      + "DisplayPrompt   =  " + SOut.Bool(printer.DisplayPrompt) + ", "
                      + "FileExtension   = '" + SOut.String(printer.FileExtension) + "', "
                      + "IsVirtualPrinter=  " + SOut.Bool(printer.IsVirtualPrinter) + " "
                      + "WHERE PrinterNum = " + SOut.Long(printer.PrinterNum);
        Db.NonQ(command);
    }

    public static bool Update(Printer printer, Printer oldPrinter)
    {
        var command = "";
        if (printer.ComputerNum != oldPrinter.ComputerNum)
        {
            if (command != "") command += ",";
            command += "ComputerNum = " + SOut.Long(printer.ComputerNum) + "";
        }

        if (printer.PrintSit != oldPrinter.PrintSit)
        {
            if (command != "") command += ",";
            command += "PrintSit = " + SOut.Int((int) printer.PrintSit) + "";
        }

        if (printer.PrinterName != oldPrinter.PrinterName)
        {
            if (command != "") command += ",";
            command += "PrinterName = '" + SOut.String(printer.PrinterName) + "'";
        }

        if (printer.DisplayPrompt != oldPrinter.DisplayPrompt)
        {
            if (command != "") command += ",";
            command += "DisplayPrompt = " + SOut.Bool(printer.DisplayPrompt) + "";
        }

        if (printer.FileExtension != oldPrinter.FileExtension)
        {
            if (command != "") command += ",";
            command += "FileExtension = '" + SOut.String(printer.FileExtension) + "'";
        }

        if (printer.IsVirtualPrinter != oldPrinter.IsVirtualPrinter)
        {
            if (command != "") command += ",";
            command += "IsVirtualPrinter = " + SOut.Bool(printer.IsVirtualPrinter) + "";
        }

        if (command == "") return false;
        command = "UPDATE printer SET " + command
                                        + " WHERE PrinterNum = " + SOut.Long(printer.PrinterNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Printer printer, Printer oldPrinter)
    {
        if (printer.ComputerNum != oldPrinter.ComputerNum) return true;
        if (printer.PrintSit != oldPrinter.PrintSit) return true;
        if (printer.PrinterName != oldPrinter.PrinterName) return true;
        if (printer.DisplayPrompt != oldPrinter.DisplayPrompt) return true;
        if (printer.FileExtension != oldPrinter.FileExtension) return true;
        if (printer.IsVirtualPrinter != oldPrinter.IsVirtualPrinter) return true;
        return false;
    }

    public static void Delete(long printerNum)
    {
        var command = "DELETE FROM printer "
                      + "WHERE PrinterNum = " + SOut.Long(printerNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPrinterNums)
    {
        if (listPrinterNums == null || listPrinterNums.Count == 0) return;
        var command = "DELETE FROM printer "
                      + "WHERE PrinterNum IN(" + string.Join(",", listPrinterNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}