using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Resellers
{
    #region Insert

    
    public static long Insert(Reseller reseller)
    {
        return ResellerCrud.Insert(reseller);
    }

    #endregion

    #region Update

    
    public static void Update(Reseller reseller)
    {
        ResellerCrud.Update(reseller);
    }

    #endregion

    #region Delete

    ///<summary>Make sure to check that the reseller does not have any customers before deleting them.</summary>
    public static void Delete(long resellerNum)
    {
        var command = "DELETE FROM reseller WHERE ResellerNum = " + SOut.Long(resellerNum);
        Db.NonQ(command);
    }

    #endregion

    #region Get Methods

    
    public static List<Reseller> GetAll()
    {
        return ResellerCrud.SelectMany("SELECT * FROM reseller");
    }

    ///<summary>Gets one Reseller from the db.</summary>
    public static Reseller GetOne(long resellerNum)
    {
        return ResellerCrud.SelectOne(resellerNum);
    }

    ///<summary>Gets one Reseller from the db with the given patNum FK.</summary>
    public static Reseller GetOneByPatNum(long patNum)
    {
        var command = "SELECT * FROM reseller "
                      + "WHERE PatNum = " + SOut.Long(patNum);
        return ResellerCrud.SelectOne(command);
    }

    ///<summary>Gets one Reseller from the db with the given username.  Returns null if user not found.</summary>
    public static Reseller GetOneByUsername(string username)
    {
        var command = "SELECT * FROM reseller "
                      + "WHERE Username = '" + SOut.String(username) + "'";
        return ResellerCrud.SelectOne(command);
    }

    ///<summary>Gets a list of resellers and some of their information.  Only used from FormResellers to fill the grid.</summary>
    public static DataTable GetResellerList()
    {
        var command = "SELECT ResellerNum,patient.PatNum,LName,FName,Preferred,WkPhone,WirelessPhone,PhoneNumberVal,Address,City,State,Email,PatStatus "
                      + "FROM reseller "
                      + "INNER JOIN patient ON reseller.PatNum=patient.PatNum "
                      + "LEFT JOIN phonenumber ON phonenumber.PatNum=patient.PatNum "
                      + "GROUP BY patient.PatNum "
                      + "ORDER BY LName ";
        return DataCore.GetTable(command);
    }

    /// <summary>
    ///     Gets all of the customers of the reseller (family members) that have active services.
    ///     Only used from FormResellerEdit to fill the grid.
    /// </summary>
    public static DataTable GetResellerCustomersList(long patNum)
    {
        var command = "SELECT patient.PatNum,RegKey,procedurecode.ProcCode,procedurecode.Descript,resellerservice.Fee,repeatcharge.DateStart,repeatcharge.DateStop,repeatcharge.Note "
                      + "FROM patient "
                      + "INNER JOIN registrationkey ON patient.PatNum=registrationkey.PatNum AND IsResellerCustomer=1 "
                      + "LEFT JOIN repeatcharge ON patient.PatNum=repeatcharge.PatNum "
                      + "LEFT JOIN procedurecode ON repeatcharge.ProcCode=procedurecode.ProcCode "
                      + "LEFT JOIN reseller ON patient.Guarantor=reseller.PatNum OR patient.SuperFamily=reseller.PatNum "
                      + "LEFT JOIN resellerservice ON reseller.ResellerNum=resellerservice.resellerNum AND resellerservice.CodeNum=procedurecode.CodeNum "
                      + "WHERE patient.PatNum!=" + SOut.Long(patNum) + " "
                      + "AND (patient.Guarantor=" + SOut.Long(patNum) + " OR patient.SuperFamily=" + SOut.Long(patNum) + ") "
                      + "ORDER BY registrationkey.RegKey ";
        return DataCore.GetTable(command);
    }

    #endregion

    #region Misc Methods

    ///<summary>Checks the database to see if the user name is already in use.</summary>
    public static bool IsUserNameInUse(long patNum, string userName)
    {
        var command = "SELECT COUNT(*) FROM reseller WHERE PatNum!=" + SOut.Long(patNum) + " AND UserName='" + SOut.String(userName) + "'";
        if (SIn.Int(DataCore.GetScalar(command)) > 0) return true; //User name in use.
        return false;
    }

    /// <summary>
    ///     Checks the database to see if the patient passed in is part of a reseller family.
    ///     Patients can be part of a reseller family via the guarantor or super family.
    /// </summary>
    public static bool IsResellerFamily(Patient patient)
    {
        var command = "SELECT COUNT(*) FROM reseller "
                      + "WHERE PatNum IN(" + SOut.Long(patient.Guarantor) + "," + SOut.Long(patient.SuperFamily) + ")";
        if (SIn.Int(DataCore.GetScalar(command)) > 0) return true;
        return false;
    }

    ///<summary>Checks the database to see if the reseller has customers with active repeating charges.</summary>
    public static bool HasActiveResellerCustomers(Reseller reseller)
    {
        var command = @"SELECT COUNT(*) FROM patient
				INNER JOIN registrationkey ON patient.PatNum=registrationkey.PatNum AND IsResellerCustomer=1
				INNER JOIN repeatcharge ON patient.PatNum=repeatcharge.PatNum
				INNER JOIN procedurecode ON repeatcharge.ProcCode=procedurecode.ProcCode
				INNER JOIN resellerservice ON procedurecode.CodeNum=resellerservice.CodeNum 
				WHERE resellerservice.ResellerNum=" + SOut.Long(reseller.ResellerNum) + " "
                      + "AND (patient.Guarantor=" + SOut.Long(reseller.PatNum) + " OR patient.SuperFamily=" + SOut.Long(reseller.PatNum) + ") "
                      + "AND ("
                      + "(DATE(repeatcharge.DateStart)<=DATE(NOW()) "
                      + "AND "
                      + "((YEAR(repeatcharge.DateStop)<1880) OR (DATE(NOW()<DATE(repeatcharge.DateStop)))))"
                      + ") "
                      + "GROUP BY patient.PatNum";
        if (SIn.Int(DataCore.GetScalar(command)) > 0) return true;
        return false;
    }

    #endregion
}