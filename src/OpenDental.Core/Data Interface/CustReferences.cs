using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class CustReferences
{
    ///<summary>Gets one CustReference from the db.</summary>
    public static CustReference GetOne(long custReferenceNum)
    {
        return CustReferenceCrud.SelectOne(custReferenceNum);
    }

    
    public static long Insert(CustReference custReference)
    {
        return CustReferenceCrud.Insert(custReference);
    }

    
    public static void Update(CustReference custReference)
    {
        CustReferenceCrud.Update(custReference);
    }

    ///<summary>Might not be used.  Might implement when a patient is deleted but doesn't happen often if ever.</summary>
    public static void Delete(long custReferenceNum)
    {
        var command = "DELETE FROM custreference WHERE CustReferenceNum = " + SOut.Long(custReferenceNum);
        Db.NonQ(command);
    }

    ///<summary>Used only from FormReferenceSelect to get the list of references.</summary>
    public static DataTable GetReferenceTable(bool limit, List<long> listBillingTypes, bool showBadRefs, bool showUsed, bool showGuarOnly, string city, string state, string zip,
        string areaCode, string specialty, int superFam, string lname, string fname, string patnum, int age, string country)
    {
        var billingSnippet = "";
        if (listBillingTypes.Count != 0)
            for (var i = 0; i < listBillingTypes.Count; i++)
            {
                if (i == 0)
                    billingSnippet += "AND (";
                else
                    billingSnippet += "OR ";

                billingSnippet += "BillingType=" + SOut.Long(listBillingTypes[i]) + " ";
                if (i == listBillingTypes.Count - 1) billingSnippet += ") ";
            }

        var phonedigits = "";
        for (var i = 0; i < areaCode.Length; i++)
            if (Regex.IsMatch(areaCode[i].ToString(), "[0-9]"))
                phonedigits = phonedigits + areaCode[i];

        var regexp = "";
        for (var i = 0; i < phonedigits.Length; i++)
        {
            if (i < 1) regexp = "^[^0-9]?"; //Allows phone to start with "("

            regexp += phonedigits[i] + "[^0-9]*";
        }

        var table = new DataTable();
        DataRow dataRow;
        //columns that start with lowercase are altered for display rather than being raw data.
        table.Columns.Add("CustReferenceNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("FName");
        table.Columns.Add("LName");
        table.Columns.Add("HmPhone");
        table.Columns.Add("State");
        table.Columns.Add("City");
        table.Columns.Add("Zip");
        table.Columns.Add("Country");
        table.Columns.Add("Specialty");
        table.Columns.Add("age");
        table.Columns.Add("SuperFamily");
        table.Columns.Add("DateMostRecent");
        table.Columns.Add("TimesUsed");
        table.Columns.Add("IsBadRef");
        var listDataRows = new List<DataRow>();
        var command = @"SELECT cr.*,p.LName,p.FName,p.HmPhone,p.State,p.City,p.Zip,p.Birthdate,pf.FieldValue,
					(SELECT COUNT(*) FROM patient tempp WHERE tempp.SuperFamily=p.SuperFamily AND tempp.SuperFamily<>0) AS SuperFamily,
					(SELECT COUNT(*) FROM custrefentry tempcre WHERE tempcre.PatNumRef=cr.PatNum) AS TimesUsed,p.Country
				FROM custreference cr
				INNER JOIN patient p ON cr.PatNum=p.PatNum
				LEFT JOIN patfield pf ON cr.PatNum=pf.PatNum AND pf.FieldName='Specialty' 
				WHERE TRUE "; //This just makes the following AND statements brainless.
        command += "AND (p.PatStatus=" + SOut.Int((int) PatientStatus.Patient) + " OR p.PatStatus=" + SOut.Int((int) PatientStatus.NonPatient) + ") " //excludes deleted, etc.
                   + billingSnippet;
        if (age > 0) command += "AND p.Birthdate <" + SOut.Date(DateTime.Now.AddYears(-age)) + " ";

        if (regexp != "") command += "AND (p.HmPhone REGEXP '" + SOut.String(regexp) + "' )";

        if (lname.Length > 0) command += "AND (p.LName LIKE '" + SOut.String(lname) + "%' OR p.Preferred LIKE '" + SOut.String(lname) + "%') ";

        if (fname.Length > 0) command += "AND (p.FName LIKE '" + SOut.String(fname) + "%' OR p.Preferred LIKE '" + SOut.String(fname) + "%') ";

        if (city.Length > 0) command += "AND p.City LIKE '" + SOut.String(city) + "%' ";

        if (state.Length > 0) command += "AND p.State LIKE '" + SOut.String(state) + "%' ";

        if (zip.Length > 0) command += "AND p.Zip LIKE '" + SOut.String(zip) + "%' ";

        if (country.Length > 0) command += "AND p.Country LIKE '" + SOut.String(country) + "%' ";

        if (patnum.Length > 0) command += "AND p.PatNum LIKE '" + SOut.String(patnum) + "%' ";

        if (specialty.Length > 0) command += "AND pf.FieldValue LIKE '" + SOut.String(specialty) + "%' ";

        if (!showBadRefs) command += "AND cr.IsBadRef=0 ";

        if (showGuarOnly) command += "AND p.Guarantor=p.PatNum ";

        command += "HAVING TRUE "; //Once again just making AND statements brainless.
        if (superFam > 0) command += "AND SuperFamily>" + SOut.Int(superFam) + " ";

        if (showUsed) command += "AND TimesUsed>0 ";

        if (limit) command = DbHelper.LimitOrderBy(command, 40);

        var tableRaws = DataCore.GetTable(command);
        for (var i = 0; i < tableRaws.Rows.Count; i++)
        {
            dataRow = table.NewRow();
            dataRow["CustReferenceNum"] = tableRaws.Rows[i]["CustReferenceNum"].ToString();
            dataRow["PatNum"] = tableRaws.Rows[i]["PatNum"].ToString();
            dataRow["FName"] = tableRaws.Rows[i]["FName"].ToString();
            dataRow["LName"] = tableRaws.Rows[i]["LName"].ToString();
            dataRow["HmPhone"] = tableRaws.Rows[i]["HmPhone"].ToString();
            dataRow["State"] = tableRaws.Rows[i]["State"].ToString();
            dataRow["City"] = tableRaws.Rows[i]["City"].ToString();
            dataRow["Zip"] = tableRaws.Rows[i]["Zip"].ToString();
            dataRow["Country"] = tableRaws.Rows[i]["Country"].ToString();
            dataRow["Specialty"] = tableRaws.Rows[i]["FieldValue"].ToString();
            dataRow["age"] = Patients.DateToAge(SIn.Date(tableRaws.Rows[i]["Birthdate"].ToString())).ToString();
            dataRow["SuperFamily"] = tableRaws.Rows[i]["SuperFamily"].ToString();
            var recentDate = SIn.DateTime(tableRaws.Rows[i]["DateMostRecent"].ToString());
            dataRow["DateMostRecent"] = "";
            if (recentDate.Year > 1880) dataRow["DateMostRecent"] = recentDate.ToShortDateString();

            dataRow["TimesUsed"] = tableRaws.Rows[i]["TimesUsed"].ToString();
            dataRow["IsBadRef"] = tableRaws.Rows[i]["IsBadRef"].ToString();
            listDataRows.Add(dataRow);
        }

        for (var i = 0; i < listDataRows.Count; i++) table.Rows.Add(listDataRows[i]);

        return table;
    }

    ///<summary>Returns FName 'Preferred' M LName.  This is here because I get names by patnum a lot with references.</summary>
    public static string GetCustNameFL(long patNum)
    {
        //Calls to the db happen in the other s classes.
        var patient = Patients.GetLim(patNum);
        return Patients.GetNameFL(patient.LName, patient.FName, patient.Preferred, patient.MiddleI);
    }

    /// <summary>
    ///     Gets the most recent CustReference entry for that patient.  Returns null if none found.  There should be only
    ///     one entry for each patient, but there was a bug before 14.3 that could have created multiple so we only get the
    ///     more relevant entry.
    /// </summary>
    public static CustReference GetOneByPatNum(long patNum)
    {
        var command = "SELECT * "
                      + "FROM custreference "
                      + "WHERE PatNum=" + SOut.Long(patNum) + " "
                      + "ORDER BY DateMostRecent DESC";
        var listCustReferences = CustReferenceCrud.SelectMany(command);
        if (listCustReferences.Count == 0) return null;

        return listCustReferences[0];
    }
}