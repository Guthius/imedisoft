using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class DispSupplies
{
    
    public static DataTable RefreshDispensary(long provNum)
    {
        var command = "SELECT supply.Descript,dispsupply.DateDispensed,dispsupply.DispQuantity,dispsupply.Note "
                      + "FROM dispsupply LEFT JOIN supply ON dispsupply.SupplyNum=supply.SupplyNum "
                      + "WHERE dispsupply.ProvNum=" + SOut.Long(provNum) + " "
                      + "ORDER BY DateDispensed,Descript";
        return DataCore.GetTable(command);
    }

    
    public static long Insert(DispSupply dispSupply)
    {
        return DispSupplyCrud.Insert(dispSupply);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<DispSupply> Refresh(long patNum){

        string command="SELECT * FROM dispsupply WHERE PatNum = "+POut.Long(patNum);
        return Crud.DispSupplyCrud.SelectMany(command);
    }

    ///<summary>Gets one DispSupply from the db.</summary>
    public static DispSupply GetOne(long dispSupplyNum){

        return Crud.DispSupplyCrud.SelectOne(dispSupplyNum);
    }



    
    public static void Update(DispSupply dispSupply){

        Crud.DispSupplyCrud.Update(dispSupply);
    }

    
    public static void Delete(long dispSupplyNum) {

        string command= "DELETE FROM dispsupply WHERE DispSupplyNum = "+POut.Long(dispSupplyNum);
        Db.NonQ(command);
    }
    */
}