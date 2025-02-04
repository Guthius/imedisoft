using System.Collections.Generic;
using System.Data;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PrinterCrud
{
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
        foreach (DataRow row in table.Rows)
        {
            var printer = new Printer
            {
                PrinterNum = SIn.Long(row["PrinterNum"].ToString()),
                ComputerNum = SIn.Long(row["ComputerNum"].ToString()),
                PrintSit = (PrintSituation) SIn.Int(row["PrintSit"].ToString()),
                PrinterName = SIn.String(row["PrinterName"].ToString()),
                DisplayPrompt = SIn.Bool(row["DisplayPrompt"].ToString()),
                FileExtension = SIn.String(row["FileExtension"].ToString()),
                IsVirtualPrinter = SIn.Bool(row["IsVirtualPrinter"].ToString())
            };
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

    public static void Insert(Printer printer)
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
}