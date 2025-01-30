using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class CDSPermissions
{
    public static CDSPermission GetForUser(long userNum)
    {
        var cDsPermission = CDSPermissionCrud.SelectOne("SELECT * FROM cdspermission WHERE UserNum=" + userNum);
        
        return cDsPermission ?? new CDSPermission();
    }
    
    public static List<CDSPermission> GetAll()
    {
        InsertMissingValues();
        
        return CDSPermissionCrud.SelectMany("SELECT * FROM cdspermission");
    }
    
    private static void InsertMissingValues()
    {
        var users = UserodCrud.SelectMany("SELECT * FROM userod WHERE IsHidden=0 AND UserNum NOT IN (SELECT UserNum from cdsPermission)");

        foreach (var user in users)
        {
            Insert(new CDSPermission
            {
                UserNum = user.UserNum
            });
        }
    }
    
    public static void Insert(CDSPermission cdsPermission)
    {
        CDSPermissionCrud.Insert(cdsPermission);
    }
    
    public static void Update(CDSPermission cdsPermission)
    {
        CDSPermissionCrud.Update(cdsPermission);
    }
}