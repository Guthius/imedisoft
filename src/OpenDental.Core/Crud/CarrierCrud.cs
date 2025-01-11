using System.Collections.Generic;
using System.Data;
using System.Drawing;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class CarrierCrud
{
    public static Carrier SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Carrier> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Carrier> TableToList(DataTable table)
    {
        var retVal = new List<Carrier>();
        Carrier carrier;
        foreach (DataRow row in table.Rows)
        {
            carrier = new Carrier();
            carrier.CarrierNum = SIn.Long(row["CarrierNum"].ToString());
            carrier.CarrierName = SIn.String(row["CarrierName"].ToString());
            carrier.Address = SIn.String(row["Address"].ToString());
            carrier.Address2 = SIn.String(row["Address2"].ToString());
            carrier.City = SIn.String(row["City"].ToString());
            carrier.State = SIn.String(row["State"].ToString());
            carrier.Zip = SIn.String(row["Zip"].ToString());
            carrier.Phone = SIn.String(row["Phone"].ToString());
            carrier.ElectID = SIn.String(row["ElectID"].ToString());
            carrier.NoSendElect = (NoSendElectType) SIn.Int(row["NoSendElect"].ToString());
            carrier.IsCDA = SIn.Bool(row["IsCDA"].ToString());
            carrier.CDAnetVersion = SIn.String(row["CDAnetVersion"].ToString());
            carrier.CanadianNetworkNum = SIn.Long(row["CanadianNetworkNum"].ToString());
            carrier.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            carrier.CanadianEncryptionMethod = SIn.Byte(row["CanadianEncryptionMethod"].ToString());
            carrier.CanadianSupportedTypes = (CanSupTransTypes) SIn.Int(row["CanadianSupportedTypes"].ToString());
            carrier.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            carrier.SecDateEntry = SIn.Date(row["SecDateEntry"].ToString());
            carrier.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            carrier.TIN = SIn.String(row["TIN"].ToString());
            carrier.CarrierGroupName = SIn.Long(row["CarrierGroupName"].ToString());
            carrier.ApptTextBackColor = Color.FromArgb(SIn.Int(row["ApptTextBackColor"].ToString()));
            carrier.IsCoinsuranceInverted = SIn.Bool(row["IsCoinsuranceInverted"].ToString());
            carrier.TrustedEtransFlags = (TrustedEtransTypes) SIn.Int(row["TrustedEtransFlags"].ToString());
            carrier.CobInsPaidBehaviorOverride = (EclaimCobInsPaidBehavior) SIn.Int(row["CobInsPaidBehaviorOverride"].ToString());
            carrier.EraAutomationOverride = (EraAutomationMode) SIn.Int(row["EraAutomationOverride"].ToString());
            carrier.OrthoInsPayConsolidate = (EnumOrthoInsPayConsolidate) SIn.Int(row["OrthoInsPayConsolidate"].ToString());
            retVal.Add(carrier);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Carrier> listCarriers, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Carrier";
        var table = new DataTable(tableName);
        table.Columns.Add("CarrierNum");
        table.Columns.Add("CarrierName");
        table.Columns.Add("Address");
        table.Columns.Add("Address2");
        table.Columns.Add("City");
        table.Columns.Add("State");
        table.Columns.Add("Zip");
        table.Columns.Add("Phone");
        table.Columns.Add("ElectID");
        table.Columns.Add("NoSendElect");
        table.Columns.Add("IsCDA");
        table.Columns.Add("CDAnetVersion");
        table.Columns.Add("CanadianNetworkNum");
        table.Columns.Add("IsHidden");
        table.Columns.Add("CanadianEncryptionMethod");
        table.Columns.Add("CanadianSupportedTypes");
        table.Columns.Add("SecUserNumEntry");
        table.Columns.Add("SecDateEntry");
        table.Columns.Add("SecDateTEdit");
        table.Columns.Add("TIN");
        table.Columns.Add("CarrierGroupName");
        table.Columns.Add("ApptTextBackColor");
        table.Columns.Add("IsCoinsuranceInverted");
        table.Columns.Add("TrustedEtransFlags");
        table.Columns.Add("CobInsPaidBehaviorOverride");
        table.Columns.Add("EraAutomationOverride");
        table.Columns.Add("OrthoInsPayConsolidate");
        foreach (var carrier in listCarriers)
            table.Rows.Add(SOut.Long(carrier.CarrierNum), carrier.CarrierName, carrier.Address, carrier.Address2, carrier.City, carrier.State, carrier.Zip, carrier.Phone, carrier.ElectID, SOut.Int((int) carrier.NoSendElect), SOut.Bool(carrier.IsCDA), carrier.CDAnetVersion, SOut.Long(carrier.CanadianNetworkNum), SOut.Bool(carrier.IsHidden), SOut.Byte(carrier.CanadianEncryptionMethod), SOut.Int((int) carrier.CanadianSupportedTypes), SOut.Long(carrier.SecUserNumEntry), SOut.DateT(carrier.SecDateEntry, false), SOut.DateT(carrier.SecDateTEdit, false), carrier.TIN, SOut.Long(carrier.CarrierGroupName), SOut.Int(carrier.ApptTextBackColor.ToArgb()), SOut.Bool(carrier.IsCoinsuranceInverted), SOut.Int((int) carrier.TrustedEtransFlags), SOut.Int((int) carrier.CobInsPaidBehaviorOverride), SOut.Int((int) carrier.EraAutomationOverride), SOut.Int((int) carrier.OrthoInsPayConsolidate));
        return table;
    }

    public static void Insert(Carrier carrier)
    {
        var command = "INSERT INTO carrier (";

        command += "CarrierName,Address,Address2,City,State,Zip,Phone,ElectID,NoSendElect,IsCDA,CDAnetVersion,CanadianNetworkNum,IsHidden,CanadianEncryptionMethod,CanadianSupportedTypes,SecUserNumEntry,SecDateEntry,TIN,CarrierGroupName,ApptTextBackColor,IsCoinsuranceInverted,TrustedEtransFlags,CobInsPaidBehaviorOverride,EraAutomationOverride,OrthoInsPayConsolidate) VALUES(";

        command +=
            "'" + SOut.String(carrier.CarrierName) + "',"
            + "'" + SOut.String(carrier.Address) + "',"
            + "'" + SOut.String(carrier.Address2) + "',"
            + "'" + SOut.String(carrier.City) + "',"
            + "'" + SOut.String(carrier.State) + "',"
            + "'" + SOut.String(carrier.Zip) + "',"
            + "'" + SOut.String(carrier.Phone) + "',"
            + "'" + SOut.String(carrier.ElectID) + "',"
            + SOut.Int((int) carrier.NoSendElect) + ","
            + SOut.Bool(carrier.IsCDA) + ","
            + "'" + SOut.String(carrier.CDAnetVersion) + "',"
            + SOut.Long(carrier.CanadianNetworkNum) + ","
            + SOut.Bool(carrier.IsHidden) + ","
            + SOut.Byte(carrier.CanadianEncryptionMethod) + ","
            + SOut.Int((int) carrier.CanadianSupportedTypes) + ","
            + SOut.Long(carrier.SecUserNumEntry) + ","
            + DbHelper.Now() + ","
            //SecDateTEdit can only be set by MySQL
            + "'" + SOut.String(carrier.TIN) + "',"
            + SOut.Long(carrier.CarrierGroupName) + ","
            + SOut.Int(carrier.ApptTextBackColor.ToArgb()) + ","
            + SOut.Bool(carrier.IsCoinsuranceInverted) + ","
            + SOut.Int((int) carrier.TrustedEtransFlags) + ","
            + SOut.Int((int) carrier.CobInsPaidBehaviorOverride) + ","
            + SOut.Int((int) carrier.EraAutomationOverride) + ","
            + SOut.Int((int) carrier.OrthoInsPayConsolidate) + ")";
        {
            carrier.CarrierNum = Db.NonQ(command, true, "CarrierNum", "carrier");
        }
    }

    public static void Update(Carrier carrier, Carrier oldCarrier)
    {
        var command = "";
        if (carrier.CarrierName != oldCarrier.CarrierName)
        {
            if (command != "") command += ",";
            command += "CarrierName = '" + SOut.String(carrier.CarrierName) + "'";
        }

        if (carrier.Address != oldCarrier.Address)
        {
            if (command != "") command += ",";
            command += "Address = '" + SOut.String(carrier.Address) + "'";
        }

        if (carrier.Address2 != oldCarrier.Address2)
        {
            if (command != "") command += ",";
            command += "Address2 = '" + SOut.String(carrier.Address2) + "'";
        }

        if (carrier.City != oldCarrier.City)
        {
            if (command != "") command += ",";
            command += "City = '" + SOut.String(carrier.City) + "'";
        }

        if (carrier.State != oldCarrier.State)
        {
            if (command != "") command += ",";
            command += "State = '" + SOut.String(carrier.State) + "'";
        }

        if (carrier.Zip != oldCarrier.Zip)
        {
            if (command != "") command += ",";
            command += "Zip = '" + SOut.String(carrier.Zip) + "'";
        }

        if (carrier.Phone != oldCarrier.Phone)
        {
            if (command != "") command += ",";
            command += "Phone = '" + SOut.String(carrier.Phone) + "'";
        }

        if (carrier.ElectID != oldCarrier.ElectID)
        {
            if (command != "") command += ",";
            command += "ElectID = '" + SOut.String(carrier.ElectID) + "'";
        }

        if (carrier.NoSendElect != oldCarrier.NoSendElect)
        {
            if (command != "") command += ",";
            command += "NoSendElect = " + SOut.Int((int) carrier.NoSendElect) + "";
        }

        if (carrier.IsCDA != oldCarrier.IsCDA)
        {
            if (command != "") command += ",";
            command += "IsCDA = " + SOut.Bool(carrier.IsCDA) + "";
        }

        if (carrier.CDAnetVersion != oldCarrier.CDAnetVersion)
        {
            if (command != "") command += ",";
            command += "CDAnetVersion = '" + SOut.String(carrier.CDAnetVersion) + "'";
        }

        if (carrier.CanadianNetworkNum != oldCarrier.CanadianNetworkNum)
        {
            if (command != "") command += ",";
            command += "CanadianNetworkNum = " + SOut.Long(carrier.CanadianNetworkNum) + "";
        }

        if (carrier.IsHidden != oldCarrier.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(carrier.IsHidden) + "";
        }

        if (carrier.CanadianEncryptionMethod != oldCarrier.CanadianEncryptionMethod)
        {
            if (command != "") command += ",";
            command += "CanadianEncryptionMethod = " + SOut.Byte(carrier.CanadianEncryptionMethod) + "";
        }

        if (carrier.CanadianSupportedTypes != oldCarrier.CanadianSupportedTypes)
        {
            if (command != "") command += ",";
            command += "CanadianSupportedTypes = " + SOut.Int((int) carrier.CanadianSupportedTypes) + "";
        }

        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (carrier.TIN != oldCarrier.TIN)
        {
            if (command != "") command += ",";
            command += "TIN = '" + SOut.String(carrier.TIN) + "'";
        }

        if (carrier.CarrierGroupName != oldCarrier.CarrierGroupName)
        {
            if (command != "") command += ",";
            command += "CarrierGroupName = " + SOut.Long(carrier.CarrierGroupName) + "";
        }

        if (carrier.ApptTextBackColor != oldCarrier.ApptTextBackColor)
        {
            if (command != "") command += ",";
            command += "ApptTextBackColor = " + SOut.Int(carrier.ApptTextBackColor.ToArgb()) + "";
        }

        if (carrier.IsCoinsuranceInverted != oldCarrier.IsCoinsuranceInverted)
        {
            if (command != "") command += ",";
            command += "IsCoinsuranceInverted = " + SOut.Bool(carrier.IsCoinsuranceInverted) + "";
        }

        if (carrier.TrustedEtransFlags != oldCarrier.TrustedEtransFlags)
        {
            if (command != "") command += ",";
            command += "TrustedEtransFlags = " + SOut.Int((int) carrier.TrustedEtransFlags) + "";
        }

        if (carrier.CobInsPaidBehaviorOverride != oldCarrier.CobInsPaidBehaviorOverride)
        {
            if (command != "") command += ",";
            command += "CobInsPaidBehaviorOverride = " + SOut.Int((int) carrier.CobInsPaidBehaviorOverride) + "";
        }

        if (carrier.EraAutomationOverride != oldCarrier.EraAutomationOverride)
        {
            if (command != "") command += ",";
            command += "EraAutomationOverride = " + SOut.Int((int) carrier.EraAutomationOverride) + "";
        }

        if (carrier.OrthoInsPayConsolidate != oldCarrier.OrthoInsPayConsolidate)
        {
            if (command != "") command += ",";
            command += "OrthoInsPayConsolidate = " + SOut.Int((int) carrier.OrthoInsPayConsolidate) + "";
        }

        if (command == "") return;
        command = "UPDATE carrier SET " + command
                                        + " WHERE CarrierNum = " + SOut.Long(carrier.CarrierNum);
        Db.NonQ(command);
    }
}