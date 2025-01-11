using System.Collections.Generic;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class HieClinics
{
    #region Methods - Misc

    ///<summary>Returns true if any of the rows have an PathExportCCD filed out and IsEnabled is true.</summary>
    public static bool IsEnabled()
    {
        var command = "SELECT COUNT(*) FROM hieclinic "
                      + "WHERE PathExportCCD !='' AND IsEnabled=1";
        return Db.GetCount(command) != "0";
    }

    #endregion Methods - Misc

    #region Methods - Get

    ///<summary>Returns all hieclinics.</summary>
    public static List<HieClinic> Refresh()
    {
        var command = "SELECT * FROM hieclinic";
        return HieClinicCrud.SelectMany(command);
    }

    ///<summary>Gets one HieClinic from the db.</summary>
    public static HieClinic GetOne(long hieClinicNum)
    {
        return HieClinicCrud.SelectOne(hieClinicNum);
    }

    ///<summary>Returns a list where PathExportCCD is not blank and IsEnabled is true.</summary>
    public static List<HieClinic> GetAllEnabled()
    {
        var command = "SELECT * FROM hieclinic WHERE PathExportCCD!='' AND IsEnabled=1";
        return HieClinicCrud.SelectMany(command);
    }

    #endregion Methods - Get

    #region Methods - Modify

    
    public static long Insert(HieClinic hieClinic)
    {
        return HieClinicCrud.Insert(hieClinic);
    }

    
    public static void Update(HieClinic hieClinic)
    {
        HieClinicCrud.Update(hieClinic);
    }

    
    public static void Delete(long hieClinicNum)
    {
        HieClinicCrud.Delete(hieClinicNum);
    }

    ///<summary>Syncs the passed in list of hieclinics with all of the hieclinics from the database.</summary>
    public static bool Sync(List<HieClinic> listHieClinics)
    {
        var listHieClinicsDb = Refresh();
        return HieClinicCrud.Sync(listHieClinics, listHieClinicsDb);
    }

    #endregion
}