using System.Collections.Generic;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class ToothInitials
{
    public static List<ToothInitial> GetPatientData(long patNum)
    {
        return ToothInitialCrud.SelectMany("SELECT * FROM toothinitial" + " WHERE PatNum = " + patNum);
    }

    public static void Insert(ToothInitial toothInitial)
    {
        ToothInitialCrud.Insert(toothInitial);
    }

    public static void Update(ToothInitial toothInitial)
    {
        ToothInitialCrud.Update(toothInitial);
    }

    public static void Delete(ToothInitial toothInitial)
    {
        Db.NonQ("DELETE FROM toothinitial WHERE ToothInitialNum = " + toothInitial.ToothInitialNum);
    }

    public static void SetValue(long patNum, string toothId, ToothInitialType toothInitialType, float moveAmt = 0)
    {
        ClearValue(patNum, toothId, toothInitialType);

        SetValueQuick(patNum, toothId, toothInitialType, moveAmt);
    }

    public static void SetValueQuick(long patNum, string toothId, ToothInitialType toothInitialType, float moveAmt)
    {
        if (moveAmt == 0 && toothInitialType is ToothInitialType.ShiftM or ToothInitialType.ShiftO or ToothInitialType.ShiftB or ToothInitialType.Rotate or ToothInitialType.TipM or ToothInitialType.TipB)
        {
            return;
        }

        Insert(new ToothInitial
        {
            PatNum = patNum,
            ToothNum = toothId,
            InitialType = toothInitialType,
            Movement = moveAmt
        });
    }

    public static void AddMovement(List<ToothInitial> toothInitials, long patNum, string toothId, ToothInitialType toothInitialType, float moveAmt)
    {
        if (moveAmt == 0) return;

        var toothInitial = toothInitials.Find(x => x.ToothNum == toothId && x.InitialType == toothInitialType)?.Copy();
        if (toothInitial is null)
        {
            Insert(new ToothInitial
            {
                PatNum = patNum,
                ToothNum = toothId,
                InitialType = toothInitialType,
                Movement = moveAmt
            });

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

    public static void ClearValue(long patNum, string toothId, ToothInitialType toothInitialType)
    {
        Db.NonQ("DELETE FROM toothinitial WHERE PatNum=" + patNum + " AND ToothNum='" + SOut.String(toothId) + "' AND InitialType=" + (int) toothInitialType);
    }

    public static void ClearAllValuesForType(long patNum, ToothInitialType toothInitialType)
    {
        Db.NonQ("DELETE FROM toothinitial WHERE PatNum=" + patNum + " AND InitialType=" + (int) toothInitialType);
    }

    public static List<string> GetMissingOrHiddenTeeth(List<ToothInitial> toothInitials)
    {
        var missingTeeth = new List<string>();

        foreach (var tooth in toothInitials)
        {
            if (tooth.InitialType is ToothInitialType.Missing or ToothInitialType.Hidden &&
                Tooth.IsValidDB(tooth.ToothNum) && !Tooth.IsSuperNum(tooth.ToothNum) && !missingTeeth.Contains(tooth.ToothNum))
            {
                missingTeeth.Add(tooth.ToothNum);
            }
        }

        return missingTeeth;
    }

    public static List<string> GetPriTeeth(List<ToothInitial> toothInitials)
    {
        var primaryTeeth = new List<string>();

        foreach (var tooth in toothInitials)
        {
            if (tooth.InitialType == ToothInitialType.Primary
                && Tooth.IsValidDB(tooth.ToothNum)
                && !Tooth.IsPrimary(tooth.ToothNum)
                && !Tooth.IsSuperNum(tooth.ToothNum))
            {
                primaryTeeth.Add(tooth.ToothNum);
            }
        }

        return primaryTeeth;
    }

    public static bool ToothIsMissingOrHidden(List<ToothInitial> toothInitials, string toothNumber)
    {
        foreach (var tooth in toothInitials)
        {
            if (tooth.InitialType != ToothInitialType.Missing && tooth.InitialType != ToothInitialType.Hidden)
            {
                continue;
            }

            if (tooth.ToothNum != toothNumber)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    public static float GetMovement(List<ToothInitial> toothInitals, string toothNumber, ToothInitialType toothInitialType)
    {
        foreach (var tooth in toothInitals)
        {
            if (tooth.InitialType == toothInitialType && tooth.ToothNum == toothNumber)
            {
                return tooth.Movement;
            }
        }

        return 0;
    }

    public static List<string> GetHiddenTeeth(List<ToothInitial> toothInitials)
    {
        var hiddenTeeth = new List<string>();

        if (toothInitials.IsNullOrEmpty())
        {
            return hiddenTeeth;
        }

        foreach (var tooth in toothInitials)
        {
            if (tooth.InitialType == ToothInitialType.Hidden && Tooth.IsValidDB(tooth.ToothNum) && !Tooth.IsSuperNum(tooth.ToothNum))
            {
                hiddenTeeth.Add(tooth.ToothNum);
            }
        }

        return hiddenTeeth;
    }
}