using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class CDSPermissions
{
    //TODO: implement caching;

    public static CDSPermission GetForUser(long userNum)
    {
        var command = "SELECT * FROM cdspermission WHERE UserNum=" + SOut.Long(userNum);
        var cDSPermission = CDSPermissionCrud.SelectOne(command);
        if (cDSPermission != null) return cDSPermission;
        return new CDSPermission(); //return new CDS permission that has no permissions granted.
    }

    
    public static List<CDSPermission> GetAll()
    {
        InsertMissingValues();
        var command = "SELECT * FROM cdspermission";
        return CDSPermissionCrud.SelectMany(command);
    }

    ///<summary>Inserts one row per UserOD if they do not have one already.</summary>
    private static void InsertMissingValues()
    {
        var command = "SELECT * FROM userod WHERE IsHidden=0 AND UserNum NOT IN (SELECT UserNum from cdsPermission)";
        var listUserods = UserodCrud.SelectMany(command);
        CDSPermission cDSPermission;
        for (var i = 0; i < listUserods.Count; i++)
        {
            cDSPermission = new CDSPermission();
            cDSPermission.UserNum = listUserods[i].UserNum;
            Insert(cDSPermission);
        }
    }

    
    public static long Insert(CDSPermission cDSPermission)
    {
        return CDSPermissionCrud.Insert(cDSPermission);
    }

    
    public static void Update(CDSPermission cDSPermission)
    {
        CDSPermissionCrud.Update(cDSPermission);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<CDSPermission> Refresh(long patNum){

        string command="SELECT * FROM cdspermission WHERE PatNum = "+POut.Long(patNum);
        return Crud.CDSPermissionCrud.SelectMany(command);
    }

    ///<summary>Gets one CDSPermission from the db.</summary>
    public static CDSPermission GetOne(long cDSPermissionNum){

        return Crud.CDSPermissionCrud.SelectOne(cDSPermissionNum);
    }

    
    public static void Delete(long cDSPermissionNum) {

        string command= "DELETE FROM cdspermission WHERE CDSPermissionNum = "+POut.Long(cDSPermissionNum);
        Db.NonQ(command);
    }
    */
}