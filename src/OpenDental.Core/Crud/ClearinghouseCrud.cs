using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ClearinghouseCrud
{
    public static Clearinghouse SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Clearinghouse> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Clearinghouse> TableToList(DataTable table)
    {
        var retVal = new List<Clearinghouse>();
        Clearinghouse clearinghouse;
        foreach (DataRow row in table.Rows)
        {
            clearinghouse = new Clearinghouse();
            clearinghouse.ClearinghouseNum = SIn.Long(row["ClearinghouseNum"].ToString());
            clearinghouse.Description = SIn.String(row["Description"].ToString());
            clearinghouse.ExportPath = SIn.String(row["ExportPath"].ToString());
            clearinghouse.Payors = SIn.String(row["Payors"].ToString());
            clearinghouse.Eformat = (ElectronicClaimFormat) SIn.Int(row["Eformat"].ToString());
            clearinghouse.ISA05 = SIn.String(row["ISA05"].ToString());
            clearinghouse.SenderTIN = SIn.String(row["SenderTIN"].ToString());
            clearinghouse.ISA07 = SIn.String(row["ISA07"].ToString());
            clearinghouse.ISA08 = SIn.String(row["ISA08"].ToString());
            clearinghouse.ISA15 = SIn.String(row["ISA15"].ToString());
            clearinghouse.Password = SIn.String(row["Password"].ToString());
            clearinghouse.ResponsePath = SIn.String(row["ResponsePath"].ToString());
            clearinghouse.CommBridge = (EclaimsCommBridge) SIn.Int(row["CommBridge"].ToString());
            clearinghouse.ClientProgram = SIn.String(row["ClientProgram"].ToString());
            clearinghouse.LastBatchNumber = SIn.Int(row["LastBatchNumber"].ToString());
            clearinghouse.ModemPort = SIn.Byte(row["ModemPort"].ToString());
            clearinghouse.LoginID = SIn.String(row["LoginID"].ToString());
            clearinghouse.SenderName = SIn.String(row["SenderName"].ToString());
            clearinghouse.SenderTelephone = SIn.String(row["SenderTelephone"].ToString());
            clearinghouse.GS03 = SIn.String(row["GS03"].ToString());
            clearinghouse.ISA02 = SIn.String(row["ISA02"].ToString());
            clearinghouse.ISA04 = SIn.String(row["ISA04"].ToString());
            clearinghouse.ISA16 = SIn.String(row["ISA16"].ToString());
            clearinghouse.SeparatorData = SIn.String(row["SeparatorData"].ToString());
            clearinghouse.SeparatorSegment = SIn.String(row["SeparatorSegment"].ToString());
            clearinghouse.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            clearinghouse.HqClearinghouseNum = SIn.Long(row["HqClearinghouseNum"].ToString());
            clearinghouse.IsEraDownloadAllowed = (EraBehaviors) SIn.Int(row["IsEraDownloadAllowed"].ToString());
            clearinghouse.IsClaimExportAllowed = SIn.Bool(row["IsClaimExportAllowed"].ToString());
            clearinghouse.IsAttachmentSendAllowed = SIn.Bool(row["IsAttachmentSendAllowed"].ToString());
            clearinghouse.LocationID = SIn.String(row["LocationID"].ToString());
            retVal.Add(clearinghouse);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Clearinghouse> listClearinghouses, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Clearinghouse";
        var table = new DataTable(tableName);
        table.Columns.Add("ClearinghouseNum");
        table.Columns.Add("Description");
        table.Columns.Add("ExportPath");
        table.Columns.Add("Payors");
        table.Columns.Add("Eformat");
        table.Columns.Add("ISA05");
        table.Columns.Add("SenderTIN");
        table.Columns.Add("ISA07");
        table.Columns.Add("ISA08");
        table.Columns.Add("ISA15");
        table.Columns.Add("Password");
        table.Columns.Add("ResponsePath");
        table.Columns.Add("CommBridge");
        table.Columns.Add("ClientProgram");
        table.Columns.Add("LastBatchNumber");
        table.Columns.Add("ModemPort");
        table.Columns.Add("LoginID");
        table.Columns.Add("SenderName");
        table.Columns.Add("SenderTelephone");
        table.Columns.Add("GS03");
        table.Columns.Add("ISA02");
        table.Columns.Add("ISA04");
        table.Columns.Add("ISA16");
        table.Columns.Add("SeparatorData");
        table.Columns.Add("SeparatorSegment");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("HqClearinghouseNum");
        table.Columns.Add("IsEraDownloadAllowed");
        table.Columns.Add("IsClaimExportAllowed");
        table.Columns.Add("IsAttachmentSendAllowed");
        table.Columns.Add("LocationID");
        foreach (var clearinghouse in listClearinghouses)
            table.Rows.Add(SOut.Long(clearinghouse.ClearinghouseNum), clearinghouse.Description, clearinghouse.ExportPath, clearinghouse.Payors, SOut.Int((int) clearinghouse.Eformat), clearinghouse.ISA05, clearinghouse.SenderTIN, clearinghouse.ISA07, clearinghouse.ISA08, clearinghouse.ISA15, clearinghouse.Password, clearinghouse.ResponsePath, SOut.Int((int) clearinghouse.CommBridge), clearinghouse.ClientProgram, SOut.Int(clearinghouse.LastBatchNumber), SOut.Byte(clearinghouse.ModemPort), clearinghouse.LoginID, clearinghouse.SenderName, clearinghouse.SenderTelephone, clearinghouse.GS03, clearinghouse.ISA02, clearinghouse.ISA04, clearinghouse.ISA16, clearinghouse.SeparatorData, clearinghouse.SeparatorSegment, SOut.Long(clearinghouse.ClinicNum), SOut.Long(clearinghouse.HqClearinghouseNum), SOut.Int((int) clearinghouse.IsEraDownloadAllowed), SOut.Bool(clearinghouse.IsClaimExportAllowed), SOut.Bool(clearinghouse.IsAttachmentSendAllowed), clearinghouse.LocationID);
        return table;
    }

    public static long Insert(Clearinghouse clearinghouse)
    {
        var command = "INSERT INTO clearinghouse (";

        command += "Description,ExportPath,Payors,Eformat,ISA05,SenderTIN,ISA07,ISA08,ISA15,Password,ResponsePath,CommBridge,ClientProgram,LastBatchNumber,ModemPort,LoginID,SenderName,SenderTelephone,GS03,ISA02,ISA04,ISA16,SeparatorData,SeparatorSegment,ClinicNum,HqClearinghouseNum,IsEraDownloadAllowed,IsClaimExportAllowed,IsAttachmentSendAllowed,LocationID) VALUES(";

        command +=
            "'" + SOut.String(clearinghouse.Description) + "',"
            + DbHelper.ParamChar + "paramExportPath,"
            + DbHelper.ParamChar + "paramPayors,"
            + SOut.Int((int) clearinghouse.Eformat) + ","
            + "'" + SOut.String(clearinghouse.ISA05) + "',"
            + "'" + SOut.String(clearinghouse.SenderTIN) + "',"
            + "'" + SOut.String(clearinghouse.ISA07) + "',"
            + "'" + SOut.String(clearinghouse.ISA08) + "',"
            + "'" + SOut.String(clearinghouse.ISA15) + "',"
            + "'" + SOut.String(clearinghouse.Password) + "',"
            + "'" + SOut.String(clearinghouse.ResponsePath) + "',"
            + SOut.Int((int) clearinghouse.CommBridge) + ","
            + "'" + SOut.String(clearinghouse.ClientProgram) + "',"
            + SOut.Int(clearinghouse.LastBatchNumber) + ","
            + SOut.Byte(clearinghouse.ModemPort) + ","
            + "'" + SOut.String(clearinghouse.LoginID) + "',"
            + "'" + SOut.String(clearinghouse.SenderName) + "',"
            + "'" + SOut.String(clearinghouse.SenderTelephone) + "',"
            + "'" + SOut.String(clearinghouse.GS03) + "',"
            + "'" + SOut.String(clearinghouse.ISA02) + "',"
            + "'" + SOut.String(clearinghouse.ISA04) + "',"
            + "'" + SOut.String(clearinghouse.ISA16) + "',"
            + "'" + SOut.String(clearinghouse.SeparatorData) + "',"
            + "'" + SOut.String(clearinghouse.SeparatorSegment) + "',"
            + SOut.Long(clearinghouse.ClinicNum) + ","
            + SOut.Long(clearinghouse.HqClearinghouseNum) + ","
            + SOut.Int((int) clearinghouse.IsEraDownloadAllowed) + ","
            + SOut.Bool(clearinghouse.IsClaimExportAllowed) + ","
            + SOut.Bool(clearinghouse.IsAttachmentSendAllowed) + ","
            + "'" + SOut.String(clearinghouse.LocationID) + "')";
        if (clearinghouse.ExportPath == null) clearinghouse.ExportPath = "";
        var paramExportPath = new OdSqlParameter("paramExportPath", OdDbType.Text, SOut.StringParam(clearinghouse.ExportPath));
        if (clearinghouse.Payors == null) clearinghouse.Payors = "";
        var paramPayors = new OdSqlParameter("paramPayors", OdDbType.Text, SOut.StringParam(clearinghouse.Payors));
        {
            clearinghouse.ClearinghouseNum = Db.NonQ(command, true, "ClearinghouseNum", "clearinghouse", paramExportPath, paramPayors);
        }
        return clearinghouse.ClearinghouseNum;
    }

    public static void Update(Clearinghouse clearinghouse)
    {
        var command = "UPDATE clearinghouse SET "
                      + "Description            = '" + SOut.String(clearinghouse.Description) + "', "
                      + "ExportPath             =  " + DbHelper.ParamChar + "paramExportPath, "
                      + "Payors                 =  " + DbHelper.ParamChar + "paramPayors, "
                      + "Eformat                =  " + SOut.Int((int) clearinghouse.Eformat) + ", "
                      + "ISA05                  = '" + SOut.String(clearinghouse.ISA05) + "', "
                      + "SenderTIN              = '" + SOut.String(clearinghouse.SenderTIN) + "', "
                      + "ISA07                  = '" + SOut.String(clearinghouse.ISA07) + "', "
                      + "ISA08                  = '" + SOut.String(clearinghouse.ISA08) + "', "
                      + "ISA15                  = '" + SOut.String(clearinghouse.ISA15) + "', "
                      + "Password               = '" + SOut.String(clearinghouse.Password) + "', "
                      + "ResponsePath           = '" + SOut.String(clearinghouse.ResponsePath) + "', "
                      + "CommBridge             =  " + SOut.Int((int) clearinghouse.CommBridge) + ", "
                      + "ClientProgram          = '" + SOut.String(clearinghouse.ClientProgram) + "', "
                      //LastBatchNumber excluded from update
                      + "ModemPort              =  " + SOut.Byte(clearinghouse.ModemPort) + ", "
                      + "LoginID                = '" + SOut.String(clearinghouse.LoginID) + "', "
                      + "SenderName             = '" + SOut.String(clearinghouse.SenderName) + "', "
                      + "SenderTelephone        = '" + SOut.String(clearinghouse.SenderTelephone) + "', "
                      + "GS03                   = '" + SOut.String(clearinghouse.GS03) + "', "
                      + "ISA02                  = '" + SOut.String(clearinghouse.ISA02) + "', "
                      + "ISA04                  = '" + SOut.String(clearinghouse.ISA04) + "', "
                      + "ISA16                  = '" + SOut.String(clearinghouse.ISA16) + "', "
                      + "SeparatorData          = '" + SOut.String(clearinghouse.SeparatorData) + "', "
                      + "SeparatorSegment       = '" + SOut.String(clearinghouse.SeparatorSegment) + "', "
                      + "ClinicNum              =  " + SOut.Long(clearinghouse.ClinicNum) + ", "
                      + "HqClearinghouseNum     =  " + SOut.Long(clearinghouse.HqClearinghouseNum) + ", "
                      + "IsEraDownloadAllowed   =  " + SOut.Int((int) clearinghouse.IsEraDownloadAllowed) + ", "
                      + "IsClaimExportAllowed   =  " + SOut.Bool(clearinghouse.IsClaimExportAllowed) + ", "
                      + "IsAttachmentSendAllowed=  " + SOut.Bool(clearinghouse.IsAttachmentSendAllowed) + ", "
                      + "LocationID             = '" + SOut.String(clearinghouse.LocationID) + "' "
                      + "WHERE ClearinghouseNum = " + SOut.Long(clearinghouse.ClearinghouseNum);
        if (clearinghouse.ExportPath == null) clearinghouse.ExportPath = "";
        var paramExportPath = new OdSqlParameter("paramExportPath", OdDbType.Text, SOut.StringParam(clearinghouse.ExportPath));
        if (clearinghouse.Payors == null) clearinghouse.Payors = "";
        var paramPayors = new OdSqlParameter("paramPayors", OdDbType.Text, SOut.StringParam(clearinghouse.Payors));
        Db.NonQ(command, paramExportPath, paramPayors);
    }

    public static bool Update(Clearinghouse clearinghouse, Clearinghouse oldClearinghouse)
    {
        var command = "";
        if (clearinghouse.Description != oldClearinghouse.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(clearinghouse.Description) + "'";
        }

        if (clearinghouse.ExportPath != oldClearinghouse.ExportPath)
        {
            if (command != "") command += ",";
            command += "ExportPath = " + DbHelper.ParamChar + "paramExportPath";
        }

        if (clearinghouse.Payors != oldClearinghouse.Payors)
        {
            if (command != "") command += ",";
            command += "Payors = " + DbHelper.ParamChar + "paramPayors";
        }

        if (clearinghouse.Eformat != oldClearinghouse.Eformat)
        {
            if (command != "") command += ",";
            command += "Eformat = " + SOut.Int((int) clearinghouse.Eformat) + "";
        }

        if (clearinghouse.ISA05 != oldClearinghouse.ISA05)
        {
            if (command != "") command += ",";
            command += "ISA05 = '" + SOut.String(clearinghouse.ISA05) + "'";
        }

        if (clearinghouse.SenderTIN != oldClearinghouse.SenderTIN)
        {
            if (command != "") command += ",";
            command += "SenderTIN = '" + SOut.String(clearinghouse.SenderTIN) + "'";
        }

        if (clearinghouse.ISA07 != oldClearinghouse.ISA07)
        {
            if (command != "") command += ",";
            command += "ISA07 = '" + SOut.String(clearinghouse.ISA07) + "'";
        }

        if (clearinghouse.ISA08 != oldClearinghouse.ISA08)
        {
            if (command != "") command += ",";
            command += "ISA08 = '" + SOut.String(clearinghouse.ISA08) + "'";
        }

        if (clearinghouse.ISA15 != oldClearinghouse.ISA15)
        {
            if (command != "") command += ",";
            command += "ISA15 = '" + SOut.String(clearinghouse.ISA15) + "'";
        }

        if (clearinghouse.Password != oldClearinghouse.Password)
        {
            if (command != "") command += ",";
            command += "Password = '" + SOut.String(clearinghouse.Password) + "'";
        }

        if (clearinghouse.ResponsePath != oldClearinghouse.ResponsePath)
        {
            if (command != "") command += ",";
            command += "ResponsePath = '" + SOut.String(clearinghouse.ResponsePath) + "'";
        }

        if (clearinghouse.CommBridge != oldClearinghouse.CommBridge)
        {
            if (command != "") command += ",";
            command += "CommBridge = " + SOut.Int((int) clearinghouse.CommBridge) + "";
        }

        if (clearinghouse.ClientProgram != oldClearinghouse.ClientProgram)
        {
            if (command != "") command += ",";
            command += "ClientProgram = '" + SOut.String(clearinghouse.ClientProgram) + "'";
        }

        //LastBatchNumber excluded from update
        if (clearinghouse.ModemPort != oldClearinghouse.ModemPort)
        {
            if (command != "") command += ",";
            command += "ModemPort = " + SOut.Byte(clearinghouse.ModemPort) + "";
        }

        if (clearinghouse.LoginID != oldClearinghouse.LoginID)
        {
            if (command != "") command += ",";
            command += "LoginID = '" + SOut.String(clearinghouse.LoginID) + "'";
        }

        if (clearinghouse.SenderName != oldClearinghouse.SenderName)
        {
            if (command != "") command += ",";
            command += "SenderName = '" + SOut.String(clearinghouse.SenderName) + "'";
        }

        if (clearinghouse.SenderTelephone != oldClearinghouse.SenderTelephone)
        {
            if (command != "") command += ",";
            command += "SenderTelephone = '" + SOut.String(clearinghouse.SenderTelephone) + "'";
        }

        if (clearinghouse.GS03 != oldClearinghouse.GS03)
        {
            if (command != "") command += ",";
            command += "GS03 = '" + SOut.String(clearinghouse.GS03) + "'";
        }

        if (clearinghouse.ISA02 != oldClearinghouse.ISA02)
        {
            if (command != "") command += ",";
            command += "ISA02 = '" + SOut.String(clearinghouse.ISA02) + "'";
        }

        if (clearinghouse.ISA04 != oldClearinghouse.ISA04)
        {
            if (command != "") command += ",";
            command += "ISA04 = '" + SOut.String(clearinghouse.ISA04) + "'";
        }

        if (clearinghouse.ISA16 != oldClearinghouse.ISA16)
        {
            if (command != "") command += ",";
            command += "ISA16 = '" + SOut.String(clearinghouse.ISA16) + "'";
        }

        if (clearinghouse.SeparatorData != oldClearinghouse.SeparatorData)
        {
            if (command != "") command += ",";
            command += "SeparatorData = '" + SOut.String(clearinghouse.SeparatorData) + "'";
        }

        if (clearinghouse.SeparatorSegment != oldClearinghouse.SeparatorSegment)
        {
            if (command != "") command += ",";
            command += "SeparatorSegment = '" + SOut.String(clearinghouse.SeparatorSegment) + "'";
        }

        if (clearinghouse.ClinicNum != oldClearinghouse.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(clearinghouse.ClinicNum) + "";
        }

        if (clearinghouse.HqClearinghouseNum != oldClearinghouse.HqClearinghouseNum)
        {
            if (command != "") command += ",";
            command += "HqClearinghouseNum = " + SOut.Long(clearinghouse.HqClearinghouseNum) + "";
        }

        if (clearinghouse.IsEraDownloadAllowed != oldClearinghouse.IsEraDownloadAllowed)
        {
            if (command != "") command += ",";
            command += "IsEraDownloadAllowed = " + SOut.Int((int) clearinghouse.IsEraDownloadAllowed) + "";
        }

        if (clearinghouse.IsClaimExportAllowed != oldClearinghouse.IsClaimExportAllowed)
        {
            if (command != "") command += ",";
            command += "IsClaimExportAllowed = " + SOut.Bool(clearinghouse.IsClaimExportAllowed) + "";
        }

        if (clearinghouse.IsAttachmentSendAllowed != oldClearinghouse.IsAttachmentSendAllowed)
        {
            if (command != "") command += ",";
            command += "IsAttachmentSendAllowed = " + SOut.Bool(clearinghouse.IsAttachmentSendAllowed) + "";
        }

        if (clearinghouse.LocationID != oldClearinghouse.LocationID)
        {
            if (command != "") command += ",";
            command += "LocationID = '" + SOut.String(clearinghouse.LocationID) + "'";
        }

        if (command == "") return false;
        if (clearinghouse.ExportPath == null) clearinghouse.ExportPath = "";
        var paramExportPath = new OdSqlParameter("paramExportPath", OdDbType.Text, SOut.StringParam(clearinghouse.ExportPath));
        if (clearinghouse.Payors == null) clearinghouse.Payors = "";
        var paramPayors = new OdSqlParameter("paramPayors", OdDbType.Text, SOut.StringParam(clearinghouse.Payors));
        command = "UPDATE clearinghouse SET " + command
                                              + " WHERE ClearinghouseNum = " + SOut.Long(clearinghouse.ClearinghouseNum);
        Db.NonQ(command, paramExportPath, paramPayors);
        return true;
    }

    public static void DeleteMany(List<long> listClearinghouseNums)
    {
        if (listClearinghouseNums == null || listClearinghouseNums.Count == 0) return;
        var command = "DELETE FROM clearinghouse "
                      + "WHERE ClearinghouseNum IN(" + string.Join(",", listClearinghouseNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static void Sync(List<Clearinghouse> listNew, List<Clearinghouse> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<Clearinghouse>();
        var listUpdNew = new List<Clearinghouse>();
        var listUpdDB = new List<Clearinghouse>();
        var listDel = new List<Clearinghouse>();
        listNew.Sort((x, y) => { return x.ClearinghouseNum.CompareTo(y.ClearinghouseNum); });
        listDB.Sort((x, y) => { return x.ClearinghouseNum.CompareTo(y.ClearinghouseNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        Clearinghouse fieldNew;
        Clearinghouse fieldDB;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            fieldDB = null;
            if (idxDB < listDB.Count) fieldDB = listDB[idxDB];
            //begin compare
            if (fieldNew != null && fieldDB == null)
            {
                //listNew has more items, listDB does not.
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew == null && fieldDB != null)
            {
                //listDB has more items, listNew does not.
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            if (fieldNew.ClearinghouseNum < fieldDB.ClearinghouseNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.ClearinghouseNum > fieldDB.ClearinghouseNum)
            {
                //dbPK less than newPK, dbItem is 'next'
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            //Both lists contain the 'next' item, update required
            listUpdNew.Add(fieldNew);
            listUpdDB.Add(fieldDB);
            idxNew++;
            idxDB++;
        }

        //Commit changes to DB
        for (var i = 0; i < listIns.Count; i++) Insert(listIns[i]);
        for (var i = 0; i < listUpdNew.Count; i++)
            if (Update(listUpdNew[i], listUpdDB[i]))
                rowsUpdatedCount++;

        DeleteMany(listDel.Select(x => x.ClearinghouseNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return;
    }
}