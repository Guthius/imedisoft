using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PerioMeasureCrud
{
    public static List<PerioMeasure> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PerioMeasure> TableToList(DataTable table)
    {
        var retVal = new List<PerioMeasure>();
        foreach (DataRow row in table.Rows)
        {
            var perioMeasure = new PerioMeasure
            {
                PerioMeasureNum = SIn.Long(row["PerioMeasureNum"].ToString()),
                PerioExamNum = SIn.Long(row["PerioExamNum"].ToString()),
                SequenceType = (PerioSequenceType) SIn.Int(row["SequenceType"].ToString()),
                IntTooth = SIn.Int(row["IntTooth"].ToString()),
                ToothValue = SIn.Int(row["ToothValue"].ToString()),
                MBvalue = SIn.Int(row["MBvalue"].ToString()),
                Bvalue = SIn.Int(row["Bvalue"].ToString()),
                DBvalue = SIn.Int(row["DBvalue"].ToString()),
                MLvalue = SIn.Int(row["MLvalue"].ToString()),
                Lvalue = SIn.Int(row["Lvalue"].ToString()),
                DLvalue = SIn.Int(row["DLvalue"].ToString()),
                SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString())
            };
            retVal.Add(perioMeasure);
        }

        return retVal;
    }

    public static void Insert(PerioMeasure perioMeasure)
    {
        var command = "INSERT INTO periomeasure (";

        command += "PerioExamNum,SequenceType,IntTooth,ToothValue,MBvalue,Bvalue,DBvalue,MLvalue,Lvalue,DLvalue,SecDateTEntry) VALUES(";

        command +=
            SOut.Long(perioMeasure.PerioExamNum) + ","
                                                 + SOut.Int((int) perioMeasure.SequenceType) + ","
                                                 + SOut.Int(perioMeasure.IntTooth) + ","
                                                 + SOut.Int(perioMeasure.ToothValue) + ","
                                                 + SOut.Int(perioMeasure.MBvalue) + ","
                                                 + SOut.Int(perioMeasure.Bvalue) + ","
                                                 + SOut.Int(perioMeasure.DBvalue) + ","
                                                 + SOut.Int(perioMeasure.MLvalue) + ","
                                                 + SOut.Int(perioMeasure.Lvalue) + ","
                                                 + SOut.Int(perioMeasure.DLvalue) + ","
                                                 + "NOW()" + ")";
        //SecDateTEdit can only be set by MySQL

        perioMeasure.PerioMeasureNum = Db.NonQ(command, true, "PerioMeasureNum", "perioMeasure");
    }

    public static void InsertMany(List<PerioMeasure> listPerioMeasures, bool useExistingPK = false)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listPerioMeasures.Count)
        {
            var perioMeasure = listPerioMeasures[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO periomeasure (");
                if (useExistingPK) sbCommands.Append("PerioMeasureNum,");
                sbCommands.Append("PerioExamNum,SequenceType,IntTooth,ToothValue,MBvalue,Bvalue,DBvalue,MLvalue,Lvalue,DLvalue,SecDateTEntry) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(perioMeasure.PerioMeasureNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(perioMeasure.PerioExamNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) perioMeasure.SequenceType));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(perioMeasure.IntTooth));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(perioMeasure.ToothValue));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(perioMeasure.MBvalue));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(perioMeasure.Bvalue));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(perioMeasure.DBvalue));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(perioMeasure.MLvalue));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(perioMeasure.Lvalue));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(perioMeasure.DLvalue));
            sbRow.Append(",");
            sbRow.Append("NOW()");
            sbRow.Append(")");
            //SecDateTEdit can only be set by MySQL
            if (sbCommands.Length + sbRow.Length + 1 > TableBase.MaxAllowedPacketCount && countRows > 0)
            {
                Db.NonQ(sbCommands.ToString());
                sbCommands = null;
            }
            else
            {
                if (hasComma) sbCommands.Append(",");
                sbCommands.Append(sbRow);
                countRows++;
                if (index == listPerioMeasures.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static void Update(PerioMeasure perioMeasure)
    {
        var command = "UPDATE periomeasure SET "
                      + "PerioExamNum   =  " + SOut.Long(perioMeasure.PerioExamNum) + ", "
                      + "SequenceType   =  " + SOut.Int((int) perioMeasure.SequenceType) + ", "
                      + "IntTooth       =  " + SOut.Int(perioMeasure.IntTooth) + ", "
                      + "ToothValue     =  " + SOut.Int(perioMeasure.ToothValue) + ", "
                      + "MBvalue        =  " + SOut.Int(perioMeasure.MBvalue) + ", "
                      + "Bvalue         =  " + SOut.Int(perioMeasure.Bvalue) + ", "
                      + "DBvalue        =  " + SOut.Int(perioMeasure.DBvalue) + ", "
                      + "MLvalue        =  " + SOut.Int(perioMeasure.MLvalue) + ", "
                      + "Lvalue         =  " + SOut.Int(perioMeasure.Lvalue) + ", "
                      + "DLvalue        =  " + SOut.Int(perioMeasure.DLvalue) + " "
                      //SecDateTEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "WHERE PerioMeasureNum = " + SOut.Long(perioMeasure.PerioMeasureNum);
        Db.NonQ(command);
    }
}