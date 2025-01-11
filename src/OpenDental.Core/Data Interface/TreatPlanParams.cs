using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class TreatPlanParams
{
    #region Methods - Get

    ///<summary>Gets one TreatPlanParam from the db based on the given TreatPlanNum.</summary>
    public static TreatPlanParam GetOneByTreatPlanNum(long treatPlanNum)
    {
        var command = $"SELECT * FROM treatplanparam WHERE TreatPlanNum={treatPlanNum}";
        var treatPlanParam = TreatPlanParamCrud.SelectOne(command);
        if (treatPlanParam != null) return treatPlanParam;

        treatPlanParam = new TreatPlanParam();
        treatPlanParam.ShowCompleted = PrefC.GetBool(PrefName.TreatPlanShowCompleted);
        treatPlanParam.ShowDiscount = true;
        treatPlanParam.ShowFees = true;
        treatPlanParam.ShowIns = !PrefC.GetBool(PrefName.EasyHideInsurance);
        treatPlanParam.ShowMaxDed = true;
        treatPlanParam.ShowSubTotals = true;
        treatPlanParam.ShowTotals = true;
        return treatPlanParam;
    }

    #endregion Methods - Get

    #region Methods - Modify

    public static long Insert(TreatPlanParam treatPlanParam)
    {
        return TreatPlanParamCrud.Insert(treatPlanParam);
    }

    public static void Delete(long treatPlanParamNum)
    {
        TreatPlanParamCrud.Delete(treatPlanParamNum);
    }

    ///<summary>Deletes a single TreatPlanParam from the db based on the given TreatPlanNum.</summary>
    public static void DeleteByTreatPlanNum(long treatPlanNum)
    {
        var command = $"DELETE FROM treatplanparam WHERE TreatPlanNum={SOut.Long(treatPlanNum)}";
        Db.NonQ(command);
    }

    ///<summary>Deletes all TreatPlanParams from the db that have the given PatNum.</summary>
    public static void RemoveAllByPatNum(long patNum)
    {
        var command = $"DELETE FROM treatplanparam WHERE PatNum={SOut.Long(patNum)}";
        Db.NonQ(command);
    }

    #endregion Methods - Modify
}