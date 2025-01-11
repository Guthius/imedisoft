using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EhrProvKeys
{
    
    public static List<EhrProvKey> RefreshForFam(long guarantor)
    {
        var command = "SELECT ehrprovkey.* FROM ehrprovkey,patient "
                      + "WHERE ehrprovkey.PatNum=patient.PatNum "
                      + "AND patient.Guarantor=" + SOut.Long(guarantor) + " "
                      + "GROUP BY ehrprovkey.EhrProvKeyNum "
                      + "ORDER BY ehrprovkey.LName,ehrprovkey.FName";
        return EhrProvKeyCrud.SelectMany(command);
    }

    ///<summary>Get a list of all EhrProvKeys. Ordered by LName and then YearValue.</summary>
    public static List<EhrProvKey> GetAllKeys()
    {
        var command = "SELECT ehrprovkey.* FROM ehrprovkey "
                      + "ORDER BY LName,YearValue";
        return EhrProvKeyCrud.SelectMany(command);
    }

    /// <summary>
    ///     Get a list of all EhrProvKeys for a provider matching the given first and last name.  Ordered by year value.
    ///     Returns empty list if lName or fName is empty.
    /// </summary>
    public static List<EhrProvKey> GetKeysByFLName(string lName, string fName)
    {
        if (lName == null || fName == null ||
            lName.Trim() == "" || fName.Trim() == "")
            return new List<EhrProvKey>();
        var command = "SELECT ehrprovkey.* FROM ehrprovkey"
                      + " WHERE ehrprovkey.LName='" + SOut.String(lName)
                      + "' AND ehrprovkey.FName='" + SOut.String(fName)
                      + "' ORDER BY ehrprovkey.YearValue DESC";
        return EhrProvKeyCrud.SelectMany(command);
    }

    ///<summary>Returns true if a provider with the same last and first name passed in has ever had an EHR prov key.</summary>
    public static bool HasProvHadKey(string lName, string fName)
    {
        var command = "SELECT COUNT(*) FROM ehrprovkey WHERE ehrprovkey.LName='" + SOut.String(lName) + "' AND ehrprovkey.FName='" + SOut.String(fName) + "'";
        return Db.GetCount(command) != "0";
    }

    ///<summary>True if the ehrprovkey table has any rows, otherwise false.</summary>
    public static bool HasEhrKeys()
    {
        var command = "SELECT COUNT(*) FROM ehrprovkey";
        return SIn.Bool(DataCore.GetScalar(command));
    }

    
    public static long Insert(EhrProvKey ehrProvKey)
    {
        return EhrProvKeyCrud.Insert(ehrProvKey);
    }

    
    public static void Update(EhrProvKey ehrProvKey)
    {
        EhrProvKeyCrud.Update(ehrProvKey);
    }

    
    public static void Delete(long ehrProvKeyNum)
    {
        var command = "DELETE FROM ehrprovkey WHERE EhrProvKeyNum = " + SOut.Long(ehrProvKeyNum);
        Db.NonQ(command);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.



    ///<summary>Gets one EhrProvKey from the db.</summary>
    public static EhrProvKey GetOne(long ehrProvKeyNum){

        return Crud.EhrProvKeyCrud.SelectOne(ehrProvKeyNum);
    }


    */
}