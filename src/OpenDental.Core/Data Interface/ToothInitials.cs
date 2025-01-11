using System.Collections.Generic;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class ToothInitials
{
    public static List<ToothInitial> GetPatientData(long patNum)
    {
        var command = "SELECT * FROM toothinitial" + " WHERE PatNum = " + SOut.Long(patNum);
        return ToothInitialCrud.SelectMany(command);
    }

    public static long Insert(ToothInitial toothInitial)
    {
        return ToothInitialCrud.Insert(toothInitial);
    }

    public static void Update(ToothInitial toothInitial)
    {
        ToothInitialCrud.Update(toothInitial);
    }

    public static void Delete(ToothInitial toothInitial)
    {
        var command = "DELETE FROM toothinitial WHERE ToothInitialNum=" + SOut.Long(toothInitial.ToothInitialNum);
        Db.NonQ(command);
    }

    /// <summary>
    ///     Sets teeth missing, or sets primary, or sets movement values.  It first clears the value from the database,
    ///     then adds a new row to represent that value.  Movements require an amount.  If movement amt is 0, then no row gets
    ///     added.
    /// </summary>
    public static void SetValue(long patNum, string toothId, ToothInitialType toothInitialType)
    {
        SetValue(patNum, toothId, toothInitialType, 0);
    }

    /// <summary>
    ///     Sets teeth missing, or sets primary, or sets movement values.  It first clears the value from the database,
    ///     then adds a new row to represent that value.  Movements require an amount.  If movement amt is 0, then no row gets
    ///     added.
    /// </summary>
    public static void SetValue(long patNum, string toothId, ToothInitialType toothInitialType, float moveAmt)
    {
        ClearValue(patNum, toothId, toothInitialType);
        SetValueQuick(patNum, toothId, toothInitialType, moveAmt);
    }

    /// <summary>
    ///     Same as SetValue, but does not clear any values first.  Only use this if you have first run
    ///     ClearAllValuesForType.
    /// </summary>
    public static void SetValueQuick(long patNum, string toothId, ToothInitialType toothInitialType, float moveAmt)
    {
        //if initialType is a movement and the movement amt is 0, then don't add a row, just return;
        if (moveAmt == 0
            && toothInitialType.In(ToothInitialType.ShiftM, ToothInitialType.ShiftO, ToothInitialType.ShiftB, ToothInitialType.Rotate, ToothInitialType.TipM,
                ToothInitialType.TipB))
            return;

        var toothInitial = new ToothInitial();
        toothInitial.PatNum = patNum;
        toothInitial.ToothNum = toothId;
        toothInitial.InitialType = toothInitialType;
        toothInitial.Movement = moveAmt;
        Insert(toothInitial);
    }

    /// <summary>
    ///     Only used for incremental tooth movements.  Automatically adds a movement to any existing movement.  Supply a
    ///     list of all toothInitials for the patient.
    /// </summary>
    public static void AddMovement(List<ToothInitial> listToothInitials, long patNum, string toothId, ToothInitialType toothInitialType, float moveAmt)
    {
        if (moveAmt == 0) return;

        var toothInitial = listToothInitials.Find(x => x.ToothNum == toothId && x.InitialType == toothInitialType)?.Copy();
        if (toothInitial == null)
        {
            toothInitial = new ToothInitial();
            toothInitial.PatNum = patNum;
            toothInitial.ToothNum = toothId;
            toothInitial.InitialType = toothInitialType;
            toothInitial.Movement = moveAmt;
            Insert(toothInitial);
            return;
        }

        toothInitial.Movement += moveAmt;
        if (toothInitial.Movement == 0)
        {
            ClearValue(patNum, toothId, toothInitialType);
            return;
        }

        Update(toothInitial);
    }

    ///<summary>Sets teeth not missing, or sets to perm, or clears movement values.</summary>
    public static void ClearValue(long patNum, string toothId, ToothInitialType toothInitialType)
    {
        var command = "DELETE FROM toothinitial WHERE PatNum=" + SOut.Long(patNum)
                                                               + " AND ToothNum='" + SOut.String(toothId)
                                                               + "' AND InitialType=" + SOut.Long((int) toothInitialType);
        Db.NonQ(command);
    }

    /// <summary>
    ///     Sets teeth not missing, or sets to perm, or clears movement values.  Clears all the values of one type for all
    ///     teeth in the mouth.
    /// </summary>
    public static void ClearAllValuesForType(long patNum, ToothInitialType toothInitialType)
    {
        var command = "DELETE FROM toothinitial WHERE PatNum=" + SOut.Long(patNum)
                                                               + " AND InitialType=" + SOut.Long((int) toothInitialType);
        Db.NonQ(command);
    }

    ///<summary>Gets a list of missing teeth as strings. Includes "1"-"32", and "A"-"Z".</summary>
    public static List<string> GetMissingOrHiddenTeeth(List<ToothInitial> listToothInitials)
    {
        var listMissingTeeth = new List<string>();
        for (var i = 0; i < listToothInitials.Count; i++)
            if ((listToothInitials[i].InitialType == ToothInitialType.Missing || listToothInitials[i].InitialType == ToothInitialType.Hidden)
                && Tooth.IsValidDB(listToothInitials[i].ToothNum)
                && !Tooth.IsSuperNum(listToothInitials[i].ToothNum)
                && !listMissingTeeth.Contains(listToothInitials[i].ToothNum))
                listMissingTeeth.Add(listToothInitials[i].ToothNum);

        return listMissingTeeth;
    }

    ///<summary>Gets a list of primary teeth as strings. Includes "1"-"32".</summary>
    public static List<string> GetPriTeeth(List<ToothInitial> listToothInitials)
    {
        var listPrimaryTeeth = new List<string>();
        for (var i = 0; i < listToothInitials.Count; i++)
            if (listToothInitials[i].InitialType == ToothInitialType.Primary
                && Tooth.IsValidDB(listToothInitials[i].ToothNum)
                && !Tooth.IsPrimary(listToothInitials[i].ToothNum)
                && !Tooth.IsSuperNum(listToothInitials[i].ToothNum))
                listPrimaryTeeth.Add(listToothInitials[i].ToothNum);

        return listPrimaryTeeth;
    }

    /// <summary>
    ///     Loops through supplied initial list to see if the specified tooth is already marked as missing or hidden.
    ///     Tooth numbers 1-32 or A-T.  Supernumeraries not supported here yet.
    /// </summary>
    public static bool ToothIsMissingOrHidden(List<ToothInitial> listToothInitials, string strToothNum)
    {
        for (var i = 0; i < listToothInitials.Count; i++)
        {
            if (listToothInitials[i].InitialType != ToothInitialType.Missing
                && listToothInitials[i].InitialType != ToothInitialType.Hidden)
                continue;

            if (listToothInitials[i].ToothNum != strToothNum) continue;

            return true;
        }

        return false;
    }

    ///<summary>Gets the current movement value for a single tooth by looping through the supplied list.</summary>
    public static float GetMovement(List<ToothInitial> listToothInitals, string strToothNum, ToothInitialType toothInitialType)
    {
        for (var i = 0; i < listToothInitals.Count; i++)
            if (listToothInitals[i].InitialType == toothInitialType
                && listToothInitals[i].ToothNum == strToothNum)
                return listToothInitals[i].Movement;

        return 0;
    }

    ///<summary>Gets a list of the hidden teeth as strings. Includes "1"-"32", and "A"-"Z".</summary>
    public static List<string> GetHiddenTeeth(List<ToothInitial> listToothInitials)
    {
        var listHiddenTeeth = new List<string>();
        if (listToothInitials.IsNullOrEmpty()) return listHiddenTeeth;

        for (var i = 0; i < listToothInitials.Count; i++)
            if (listToothInitials[i].InitialType == ToothInitialType.Hidden
                && Tooth.IsValidDB(listToothInitials[i].ToothNum)
                && !Tooth.IsSuperNum(listToothInitials[i].ToothNum))
                listHiddenTeeth.Add(listToothInitials[i].ToothNum);

        return listHiddenTeeth;
    }
}