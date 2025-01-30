using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Carriers
{
    public static DataTable GetBigList(bool isCanadian, bool showHidden, string carrierName, string carrierPhone, string carrierElectId)
    {
        DataTable tableRaw;
        DataTable table;
        string command;
        //if(isCanadian){
        //Strip out the digits from the phone number.
        var phonedigits = "";
        for (var i = 0; i < carrierPhone.Length; i++)
            if (Regex.IsMatch(carrierPhone[i].ToString(), "[0-9]"))
                phonedigits = phonedigits + carrierPhone[i];

        //Create a regular expression so that the phone search uses only numbers.
        var regexp = "";
        for (var i = 0; i < phonedigits.Length; i++)
        {
            if (i != 0) regexp += "[^0-9]*"; //zero or more intervening digits that are not numbers
            regexp += phonedigits[i];
        }

        command = "SELECT Address,Address2,canadiannetwork.Abbrev,carrier.CarrierNum,"
                  + "CarrierName,CDAnetVersion,City,ElectID,"
                  + "COUNT(insplan.PlanNum) insPlanCount,IsCDA,"
                  + "carrier.IsHidden,Phone,State,Zip "
                  + "FROM carrier "
                  + "LEFT JOIN canadiannetwork ON canadiannetwork.CanadianNetworkNum=carrier.CanadianNetworkNum "
                  + "LEFT JOIN insplan ON insplan.CarrierNum=carrier.CarrierNum "
                  + "WHERE "
                  + "CarrierName LIKE '%" + SOut.String(carrierName) + "%' "
                  + "AND ElectID LIKE '%" + SOut.String(carrierElectId) + "%'";
        if (regexp != "")
        {
            command += "AND Phone REGEXP '" + SOut.String(regexp) + "' ";
        }

        if (isCanadian) command += "AND IsCDA=1 ";
        if (!showHidden) command += "AND carrier.IsHidden=0 ";
        command += "GROUP BY carrier.CarrierNum ";
        command += "ORDER BY CarrierName";
        tableRaw = DataCore.GetTable(command);
        table = new DataTable();
        table.Columns.Add("Address");
        table.Columns.Add("Address2");
        table.Columns.Add("CarrierNum");
        table.Columns.Add("CarrierName");
        table.Columns.Add("City");
        table.Columns.Add("ElectID");
        table.Columns.Add("insPlanCount");
        table.Columns.Add("isCDA");
        table.Columns.Add("isHidden");
        table.Columns.Add("Phone");
        //table.Columns.Add("pMP");
        //table.Columns.Add("network");
        table.Columns.Add("State");
        //table.Columns.Add("version");
        table.Columns.Add("Zip");
        DataRow row;
        for (var i = 0; i < tableRaw.Rows.Count; i++)
        {
            row = table.NewRow();
            row["Address"] = tableRaw.Rows[i]["Address"].ToString();
            row["Address2"] = tableRaw.Rows[i]["Address2"].ToString();
            row["CarrierNum"] = tableRaw.Rows[i]["CarrierNum"].ToString();
            row["CarrierName"] = tableRaw.Rows[i]["CarrierName"].ToString();
            row["City"] = tableRaw.Rows[i]["City"].ToString();
            row["ElectID"] = tableRaw.Rows[i]["ElectID"].ToString();
            if (SIn.Bool(tableRaw.Rows[i]["IsCDA"].ToString()))
                row["isCDA"] = "X";
            else
                row["isCDA"] = "";
            if (SIn.Bool(tableRaw.Rows[i]["IsHidden"].ToString()))
                row["isHidden"] = "X";
            else
                row["isHidden"] = "";
            row["insPlanCount"] = tableRaw.Rows[i]["insPlanCount"].ToString();
            row["Phone"] = tableRaw.Rows[i]["Phone"].ToString();
            //if(PIn.Bool(tableRaw.Rows[i]["IsPMP"].ToString())){
            //	row["pMP"]="X";
            //}
            //else{
            //	row["pMP"]="";
            //}
            //row["network"]=tableRaw.Rows[i]["Abbrev"].ToString();
            row["State"] = tableRaw.Rows[i]["State"].ToString();
            //row["version"]=tableRaw.Rows[i]["CDAnetVersion"].ToString();
            row["Zip"] = tableRaw.Rows[i]["Zip"].ToString();
            table.Rows.Add(row);
        }

        return table;
    }

    public static void Update(Carrier carrier, Carrier carrierOld)
    {
        Update(carrier, carrierOld, Security.CurUser.UserNum);
    }

    public static void Update(Carrier carrier, Carrier carrierOld, long userNum)
    {
        string command;
        DataTable table;
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            //Canadian. en-CA or fr-CA
            if (carrier.IsCDA)
            {
                if (carrier.ElectID == "") throw new ApplicationException(Lans.g("Carriers", "Carrier Identification Number required."));
                if (!Regex.IsMatch(carrier.ElectID, "^[0-9]{6}$")) throw new ApplicationException(Lans.g("Carriers", "Carrier Identification Number must be exactly 6 numbers."));
            }

            //so the edited carrier looks good, but now we need to make sure that the original was allowed to be changed.
            command = "SELECT ElectID,IsCDA FROM carrier WHERE CarrierNum = '" + SOut.Long(carrier.CarrierNum) + "'";
            table = DataCore.GetTable(command);
            if (SIn.Bool(table.Rows[0]["IsCDA"].ToString()) //if original carrier IsCDA
                && SIn.String(table.Rows[0]["ElectID"].ToString()).Trim() != "" //and the ElectID was already set
                && SIn.String(table.Rows[0]["ElectID"].ToString()) != carrier.ElectID) //and the ElectID was changed
            {
                command = "SELECT COUNT(*) FROM etrans WHERE CarrierNum= " + SOut.Long(carrier.CarrierNum)
                                                                           + " OR CarrierNum2=" + SOut.Long(carrier.CarrierNum);
                if (Db.GetCount(command) != "0") throw new ApplicationException(Lans.g("Carriers", "Not allowed to change Carrier Identification Number because it's in use in the claim history."));
            }
        }

        CarrierCrud.Update(carrier, carrierOld);
        InsEditLogs.MakeLogEntry(carrier, carrierOld, InsEditLogType.Carrier, userNum);
    }

    public static void Insert(Carrier carrier, Carrier carrierOld = null)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        carrier.SecUserNumEntry = Security.CurUser.UserNum;
        //string command;
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA")) //Canadian. en-CA or fr-CA
            if (carrier.IsCDA)
            {
                if (carrier.ElectID == "") throw new ApplicationException(Lans.g("Carriers", "Carrier Identification Number required."));
                if (!Regex.IsMatch(carrier.ElectID, "^[0-9]{6}$")) throw new ApplicationException(Lans.g("Carriers", "Carrier Identification Number must be exactly 6 numbers."));
            }

        if (carrierOld == null) carrierOld = carrier.Copy();
        CarrierCrud.Insert(carrier);
        if (carrierOld.CarrierNum != 0)
            InsEditLogs.MakeLogEntry(carrier, carrierOld, InsEditLogType.Carrier, carrier.SecUserNumEntry);
        else
            InsEditLogs.MakeLogEntry(carrier, null, InsEditLogType.Carrier, carrier.SecUserNumEntry);
    }

    public static void Delete(Carrier carrier)
    {
        //look for dependencies in insplan table.
        var command = "SELECT insplan.PlanNum,CONCAT(CONCAT(LName,', '),FName) FROM insplan "
                      + "LEFT JOIN inssub ON insplan.PlanNum=inssub.PlanNum "
                      + "LEFT JOIN patient ON inssub.Subscriber=patient.PatNum "
                      + "WHERE insplan.CarrierNum = " + SOut.Long(carrier.CarrierNum) + " "
                      + "ORDER BY LName,FName";
        var table = DataCore.GetTable(command);
        string strInUse;
        if (table.Rows.Count > 0)
        {
            strInUse = ""; //new string[table.Rows.Count];
            for (var i = 0; i < table.Rows.Count; i++)
            {
                if (i > 0) strInUse += "; ";
                strInUse += SIn.String(table.Rows[i][1].ToString());
            }

            throw new ApplicationException(Lans.g("Carriers", "Not allowed to delete carrier because it is in use.  Subscribers using this carrier include ") + strInUse);
        }

        //look for dependencies in etrans table.
        command = "SELECT DateTimeTrans FROM etrans WHERE CarrierNum=" + SOut.Long(carrier.CarrierNum)
                                                                       + " OR CarrierNum2=" + SOut.Long(carrier.CarrierNum);
        table = DataCore.GetTable(command);
        if (table.Rows.Count > 0)
        {
            strInUse = "";
            for (var i = 0; i < table.Rows.Count; i++)
            {
                if (i > 0) strInUse += ", ";
                strInUse += SIn.DateTime(table.Rows[i][0].ToString()).ToShortDateString();
            }

            throw new ApplicationException(Lans.g("Carriers", "Not allowed to delete carrier because it is in use in the etrans table.  Dates of claim sent history include ") + strInUse);
        }

        command = "DELETE from carrier WHERE CarrierNum = " + SOut.Long(carrier.CarrierNum);
        Db.NonQ(command);
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        InsEditLogs.MakeLogEntry(null, carrier, InsEditLogType.Carrier, Security.CurUser.UserNum);
    }

    public static List<string> DependentPlans(Carrier carrier)
    {
        var command = "SELECT CONCAT(CONCAT(LName,', '),FName) FROM patient,insplan,inssub"
                      + " WHERE patient.PatNum=inssub.Subscriber"
                      + " AND insplan.PlanNum=inssub.PlanNum"
                      + " AND insplan.CarrierNum = '" + SOut.Long(carrier.CarrierNum) + "'"
                      + " ORDER BY LName,FName";
        var table = DataCore.GetTable(command);
        var listStrings = new List<string>();
        for (var i = 0; i < table.Rows.Count; i++) listStrings.Add(SIn.String(table.Rows[i][0].ToString()));
        return listStrings;
    }

    public static string GetName(long carrierNum)
    {
        var carrierName = "";
        //This is an uncommon pre-check because places throughout the program explicitly did not correctly send out a cache refresh signal.
        if (!GetContainsKey(carrierNum)) RefreshCache();
        ODException.SwallowAnyException(() => { carrierName = GetOne(carrierNum).CarrierName; });
        //Empty string can only happen if corrupted:
        return carrierName;
    }

    public static Carrier GetCarrierDB(long carrierNum)
    {
        var command = "SELECT * FROM carrier WHERE CarrierNum=" + SOut.Long(carrierNum);
        return CarrierCrud.SelectOne(command);
    }

    public static Carrier GetCarrier(long carrierNum)
    {
        var carrier = new Carrier {CarrierName = ""};
        //This is an uncommon pre-check because places throughout the program explicitly did not correctly send out a cache refresh signal.
        if (!GetContainsKey(carrierNum)) RefreshCache();
        ODException.SwallowAnyException(() => { carrier = GetOne(carrierNum); });
        //New and empty carrier can only happen if corrupted.
        return carrier;
    }

    private static string GetSecurityLogMessage(Carrier carrier, Carrier carrierOld)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{carrierOld.CarrierName} has split");
        stringBuilder.AppendLine($"New carrier {carrier.CarrierName}:");
        stringBuilder.Append(SecurityLogMessageHelper(carrier.Phone, "phone"));
        stringBuilder.Append(SecurityLogMessageHelper(carrier.Address, "address"));
        stringBuilder.Append(SecurityLogMessageHelper(carrier.Address2, "address2"));
        stringBuilder.Append(SecurityLogMessageHelper(carrier.City, "city"));
        stringBuilder.Append(SecurityLogMessageHelper(carrier.State, "state"));
        stringBuilder.Append(SecurityLogMessageHelper(carrier.Zip, "zip"));
        stringBuilder.Append(SecurityLogMessageHelper(carrier.ElectID, "electID"));
        stringBuilder.Append(SecurityLogMessageHelper(carrier.NoSendElect.ToString(), "send electronically"));
        stringBuilder.Append(SecurityLogMessageHelper(carrier.CobInsPaidBehaviorOverride.ToString(), "send paid by other insurance"));
        stringBuilder.Append(SecurityLogMessageHelper(carrier.EraAutomationOverride.ToString(), "ERA automation"));
        stringBuilder.Append(SecurityLogMessageHelper(carrier.OrthoInsPayConsolidate.ToString(), "consolidate ortho ins payments"));
        stringBuilder.Append(SecurityLogMessageHelper(carrier.CarrierGroupName.ToString(), "carrier group"));
        stringBuilder.Append(SecurityLogMessageHelper(carrier.IsHidden.ToString(), "is hidden"));
        stringBuilder.Append(SecurityLogMessageHelper(carrier.TrustedEtransFlags.ToString(), "is trusted"));
        stringBuilder.Append(SecurityLogMessageHelper(carrier.IsCDA.ToString(), "is CDA"));
        stringBuilder.Append(SecurityLogMessageHelper(carrier.ApptTextBackColor.ToString(), "text back color"));
        stringBuilder.Append(SecurityLogMessageHelper(carrier.IsCoinsuranceInverted.ToString(), "import benefit coinsurance inverted"));
        return stringBuilder.ToString();
    }

    private static string SecurityLogMessageHelper(string carrierVal, string colVal)
    {
        return $"{colVal} initialized with value '{carrierVal}'\r\n";
    }

    public static Carrier GetIdentical(Carrier carrier, Carrier carrierOld = null)
    {
        if (carrier.CarrierName == "") return new Carrier(); //should probably be null instead
        var carrierRetVal = carrier.Copy();
        var command = "SELECT CarrierNum,Phone FROM carrier WHERE "
                      + "CarrierName = '" + SOut.String(carrier.CarrierName) + "' "
                      + "AND Address = '" + SOut.String(carrier.Address) + "' "
                      + "AND Address2 = '" + SOut.String(carrier.Address2) + "' "
                      + "AND City = '" + SOut.String(carrier.City) + "' "
                      + "AND State LIKE '" + SOut.String(carrier.State) + "' " //This allows user to remove trailing spaces from the FormInsPlan interface.
                      + "AND Zip = '" + SOut.String(carrier.Zip) + "' "
                      + "AND ElectID = '" + SOut.String(carrier.ElectID) + "' "
                      + "AND NoSendElect = " + SOut.Int((int) carrier.NoSendElect) + " ";
        if (carrierOld != null)
            //When "Change Plan for all subscribers is selected on FormInsPlan, the user can be prompted to change the carrier for the plan's received
            //claims if any carrier information is edited. This prompt also could incorrectly appear when a plan was picked from the list
            //as a new plan for the patient, but no carrier info was changed. To prevent that, we first try to choose carrierOld if it is in our table.
            command += "ORDER BY (CASE WHEN CarrierNum=" + SOut.Long(carrierOld.CarrierNum) + " THEN 0 ELSE 1 END)";
        var table = DataCore.GetTable(command);
        //Previously carrier.Phone has been given to us after being formatted by ValidPhone in the UI (FormInsPlan).
        //Strip all formatting from the given phone number and the DB phone numbers to compare.
        //The phone in the database could be in a different format if it was imported in an old version.
        var carrierPhoneStripped = StringTools.StripNonDigits(carrier.Phone);
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var phone = SIn.String(table.Rows[i]["Phone"].ToString());
            if (StringTools.StripNonDigits(phone) == carrierPhoneStripped)
            {
                //A matching carrier was found in the database, so we will use it.
                carrierRetVal.CarrierNum = SIn.Long(table.Rows[i][0].ToString());
                return carrierRetVal;
            }
        }

        //No match found.  Decide what to do.  Usually add carrier.--------------------------------------------------------------
        //Canada:
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA")) //Canadian. en-CA or fr-CA
            throw new ApplicationException(Lans.g("Carriers", "Carrier not found.")); //gives user a chance to add manually.
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        carrier.SecUserNumEntry = Security.CurUser.UserNum;
        Insert(carrier, carrierOld); //insert function takes care of logging.
        if (carrierOld != null)
        {
            var message = GetSecurityLogMessage(carrier, carrierOld);
            SecurityLogs.MakeLogEntry(EnumPermType.CarrierCreate, 0, message);
        }

        carrierRetVal.CarrierNum = carrier.CarrierNum;
        return carrierRetVal;
    }

    public static bool Compare(Carrier carrierOne, Carrier carrierTwo)
    {
        if (carrierOne == null || carrierTwo == null) return false;
        if (carrierOne.Address != carrierTwo.Address
            || carrierOne.Address2 != carrierTwo.Address2
            || carrierOne.CarrierName != carrierTwo.CarrierName
            || carrierOne.City != carrierTwo.City
            || carrierOne.ElectID != carrierTwo.ElectID
            || carrierOne.NoSendElect != carrierTwo.NoSendElect
            || carrierOne.Phone != carrierTwo.Phone
            || carrierOne.State != carrierTwo.State
            || carrierOne.Zip != carrierTwo.Zip)
            return false;
        return true;
    }

    public static List<Carrier> GetSimilarNames(string carrierName)
    {
        return GetWhere(x => x.CarrierName.ToUpper().IndexOf(carrierName.ToUpper()) == 0, true);
    }

    public static List<Carrier> GetExactNames(string carrierName)
    {
        return GetWhere(x => x.CarrierName.ToUpper().Equals(carrierName.ToUpper()), true);
    }

    public static void Combine(List<long> listCarrierNums, long pickedCarrierNum)
    {
        if (listCarrierNums == null || listCarrierNums.Count <= 1) return; //Nothing to do.
        //Remove the CarrierNum that was picked as the superior carrier in order to get the list of carriers that will be combined into the picked carrier.
        var listCarrierNumsToCombine = listCarrierNums.FindAll(x => x != pickedCarrierNum);
        if (listCarrierNumsToCombine.IsNullOrEmpty()) return; //No carriers to combine.
        var strCarrierNums = string.Join(",", listCarrierNumsToCombine.Select(x => SOut.Long(x)));
        //Create InsEditLogs============================================================================================================
        //Get all of the related insplan objects from the database.
        var listInsPlans = InsPlans.GetAllByCarrierNums(listCarrierNumsToCombine);
        //Create an InsEditLog out of each InsPlan returned that will be inserted into the database later.
        var listInsEditLogs = listInsPlans.Select(x =>
            InsEditLogs.MakeLogEntry("CarrierNum",
                Security.CurUser.UserNum, //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
                SOut.Long(x.CarrierNum),
                SOut.Long(pickedCarrierNum),
                InsEditLogType.InsPlan,
                x.PlanNum,
                0,
                x.GroupNum + " - " + x.GroupName,
                false)
        ).ToList();
        //Update insplan.CarrierNum=====================================================================================================
        var command = $"UPDATE insplan SET CarrierNum = {SOut.Long(pickedCarrierNum)} WHERE CarrierNum IN({strCarrierNums})";
        Db.NonQ(command);
        //Update insbluebook.CarrierNum=================================================================================================
        command = $"UPDATE insbluebook SET insbluebook.CarrierNum = {SOut.Long(pickedCarrierNum)} WHERE insbluebook.CarrierNum IN({strCarrierNums})";
        Db.NonQ(command);
        //Update etrans.CarrierNum======================================================================================================
        command = $"UPDATE etrans SET CarrierNum = {SOut.Long(pickedCarrierNum)} WHERE CarrierNum IN({strCarrierNums})";
        Db.NonQ(command);
        //Update etrans.CarrierNum2=====================================================================================================
        command = $"UPDATE etrans SET CarrierNum2 = {SOut.Long(pickedCarrierNum)} WHERE CarrierNum2 IN({strCarrierNums})";
        Db.NonQ(command);
        //Insert InsEditLogs============================================================================================================
        InsEditLogs.InsertMany(listInsEditLogs);
        //Delete Carriers===============================================================================================================
        //Get all of the related carrier objects from the cache before they are deleted.
        var listCarriersToCombine = GetCarriers(listCarrierNumsToCombine);
        //Delete all of the carriers from the database that were just combined into the picked carrier.
        command = $"DELETE FROM carrier WHERE CarrierNum IN({strCarrierNums})";
        Db.NonQ(command);
        //Make an InsEditLog for each carrier that was just deleted.
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        for (var i = 0; i < listCarriersToCombine.Count; i++) InsEditLogs.MakeLogEntry(null, listCarriersToCombine[i], InsEditLogType.Carrier, Security.CurUser.UserNum);
    }

    public static List<Carrier> GetCarriers(List<long> listCarrierNums)
    {
        return GetWhere(x => listCarrierNums.Contains(x.CarrierNum));
    }

    public static List<Carrier> GetForInsPlans(List<InsPlan> listInsPlans)
    {
        if (listInsPlans.Count == 0) return new List<Carrier>();
        var listCarrierNumsForClaims = listInsPlans.Select(x => x.CarrierNum).Distinct().ToList();
        return GetCarriers(listCarrierNumsForClaims);
    }

    public static List<Carrier> GetCdaCarriersInUse()
    {
        //Use reflection to get all the values from the EtransType enumeration where the field is flagged as a request type via the EtransTypeAttr.
        var listEtransRequestTypes = typeof(EtransType).GetFields()
            .Select(x =>
                new
                {
                    fieldInfo = x,
                    etransTypeAttr = x.GetCustomAttributes(false).OfType<EtransTypeAttr>().FirstOrDefault()
                })
            .Where(x => x.etransTypeAttr != null && x.etransTypeAttr.IsRequestType)
            .Select(x => x.fieldInfo.GetValue(null))
            .Cast<EtransType>()
            .ToList();
        //Get all of the carriers flagged as IsCDA that have had at least one etrans request in the past.
        var command = "SELECT carrier.* "
                      + "FROM carrier "
                      + "INNER JOIN etrans ON carrier.CarrierNum=etrans.CarrierNum "
                      + "AND etrans.Etype IN(" + string.Join(",", listEtransRequestTypes.Select(x => SOut.Enum(x))) + ") "
                      + "WHERE carrier.IsCDA=1 "
                      + "GROUP BY carrier.CarrierNum ";
        return CarrierCrud.SelectMany(command);
    }

    public static bool ElectIdInUse(string electID)
    {
        if (string.IsNullOrEmpty(electID)) return true;
        return Cache.GetFirstOrDefault(x => x.ElectID == electID) != null;
    }

    public static List<Carrier> GetAllByElectId(string electID)
    {
        return GetWhere(x => x.ElectID == electID);
    }

    public static List<string> GetAllDistinctCarrierNames()
    {
        var command = "SELECT DISTINCT CarrierName FROM carrier WHERE CarrierName!='' ORDER BY CarrierName ASC";
        var listCarrierNames = Db.GetListString(command);
        return listCarrierNames;
    }

    public static Carrier GetByNameAndPhone(string carrierName, string phone, bool updateCacheIfNew = false)
    {
        if (string.IsNullOrEmpty(carrierName)) throw new ApplicationException("Carrier cannot be blank");
        var carrier = GetFirstOrDefault(x => x.CarrierName == carrierName && x.Phone == phone);
        if (carrier == null)
        {
            carrier = new Carrier();
            carrier.CarrierName = carrierName;
            carrier.Phone = phone;
            Insert(carrier); //Insert function takes care of logging.
            if (updateCacheIfNew)
            {
                Signalods.SetInvalid(InvalidType.Carriers);
                RefreshCache();
            }
        }

        return carrier;
    }

    public static List<Carrier> GetByNameAndTin(string carrierName, string tin)
    {
        return GetWhere(x => x.CarrierName.Trim().ToLower() == carrierName.Trim().ToLower() && x.TIN == tin);
    }

    public static Carrier GetCarrierByName(string carrierName)
    {
        return GetFirstOrDefault(x => x.CarrierName == carrierName);
    }

    public static List<Carrier> GetForClaim(Claim claim)
    {
        var command = "SELECT c.* " +
                      "FROM claimproc cp " +
                      "INNER JOIN insplan p ON cp.PlanNum=p.PlanNum " +
                      "INNER JOIN carrier c ON c.CarrierNum=p.CarrierNum " +
                      "WHERE cp.ClaimNum=" + SOut.Long(claim.ClaimNum) + " " +
                      "GROUP BY c.CarrierNum";
        return CarrierCrud.SelectMany(command);
    }

    public static bool IsMedicaid(Carrier carrier)
    {
        var electId = ElectIDs.GetId(carrier.ElectID);
        if (electId != null && electId.IsMedicaid) //Emdeon Medical requires loop 2420E when the claim is sent to DMERC (Medicaid) carriers.
            return true;
        return false;
    }

    public static bool DoConsolidateOrthoPayments(InsPlan insPlan)
    {
        if (insPlan == null) return false; //Invalid params, return false.
        var carrier = GetCarrier(insPlan.CarrierNum); //Returns a blank carrier object if not found which will use Global.
        if (carrier.OrthoInsPayConsolidate == EnumOrthoInsPayConsolidate.Global) return PrefC.GetBool(PrefName.OrthoInsPayConsolidated);
        return carrier.OrthoInsPayConsolidate == EnumOrthoInsPayConsolidate.ForceConsolidateOn;
    }
    
    private class CarrierCache : CacheDictAbs<Carrier, long, Carrier>
    {
        protected override List<Carrier> GetCacheFromDb()
        {
            var command = "SELECT * FROM carrier ORDER BY CarrierName";
            return CarrierCrud.SelectMany(command);
        }

        protected override List<Carrier> TableToList(DataTable dataTable)
        {
            return CarrierCrud.TableToList(dataTable);
        }

        protected override Carrier Copy(Carrier item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(Dictionary<long, Carrier> dict)
        {
            return CarrierCrud.ListToTable(dict.Values.ToList(), "Carrier");
        }

        protected override void FillCacheIfNeeded()
        {
            Carriers.GetTableFromCache(false);
        }

        protected override bool IsInDictShort(Carrier item)
        {
            return !item.IsHidden;
        }

        protected override long GetDictKey(Carrier item)
        {
            return item.CarrierNum;
        }

        protected override Carrier GetDictValue(Carrier item)
        {
            return item;
        }

        protected override Carrier CopyValue(Carrier carrier)
        {
            return carrier.Copy();
        }
    }
    
    private static readonly CarrierCache Cache = new();

    public static bool GetContainsKey(long key, bool isShort = false)
    {
        return Cache.GetContainsKey(key, isShort);
    }

    public static Carrier GetOne(long codeNum)
    {
        return Cache.GetOne(codeNum);
    }

    public static Carrier GetFirstOrDefault(Func<Carrier, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static List<Carrier> GetWhere(Func<Carrier, bool> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}