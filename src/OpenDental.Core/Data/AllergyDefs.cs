using System.Collections.Generic;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class AllergyDefs
{
    public static AllergyDef GetOne(long allergyDefNum)
    {
        return AllergyDefCrud.SelectOne(allergyDefNum);
    }

    public static void Insert(AllergyDef allergyDef)
    {
        AllergyDefCrud.Insert(allergyDef);
    }

    public static void Update(AllergyDef allergyDef)
    {
        AllergyDefCrud.Update(allergyDef);
    }

    public static void Delete(long allergyDefNum)
    {
        Db.NonQ("DELETE FROM allergydef WHERE AllergyDefNum = " + allergyDefNum);
    }

    public static List<AllergyDef> GetAll(bool includeHidden)
    {
        var commandText = includeHidden
            ? "SELECT * FROM allergydef ORDER BY Description"
            : "SELECT * FROM allergydef WHERE IsHidden = 0 ORDER BY Description";

        return AllergyDefCrud.SelectMany(commandText);
    }

    public static bool DefIsInUse(long allergyDefNum)
    {
        if (Db.GetCount("SELECT COUNT(*) FROM allergy WHERE AllergyDefNum = " + allergyDefNum) != "0")
        {
            return true;
        }

        if (Db.GetCount("SELECT COUNT(*) FROM rxalert WHERE AllergyDefNum = " + allergyDefNum) != "0")
        {
            return true;
        }

        return allergyDefNum == PrefC.GetLong(PrefName.AllergiesIndicateNone);
    }

    public static List<AllergyDef> GetAllergyDefs(long patNum, bool includeInactive)
    {
        var commandText =
            $"""
             SELECT allergydef.* FROM allergydef
             INNER JOIN allergy ON allergy.AllergyDefNum = allergydef.AllergyDefNum
             WHERE allergy.PatNum = {patNum}
             """;

        if (!includeInactive)
        {
            commandText += "AND allergy.StatusIsActive!=0";
        }

        return AllergyDefCrud.TableToList(DataCore.GetTable(commandText));
    }

    public static string GetDescription(long allergyDefNum)
    {
        return allergyDefNum == 0 ? "" : AllergyDefCrud.SelectOne(allergyDefNum).Description;
    }

    public static AllergyDef GetAllergyDefFromMedication(long medicationNum)
    {
        return medicationNum == 0 ? null : AllergyDefCrud.SelectOne("SELECT * FROM allergydef WHERE MedicationNum = " + medicationNum);
    }

    public static AllergyDef GetAllergyDefFromRxnorm(long rxCui)
    {
        if (rxCui == 0)
        {
            return null;
        }

        return AllergyDefCrud.SelectOne(
            "SELECT allergydef.* FROM allergydef " +
            "INNER JOIN medication ON allergydef.MedicationNum = medication.MedicationNum " +
            "AND medication.RxCui = " + rxCui + " " +
            "WHERE allergydef.SnomedType IN (" + (int) SnomedAllergy.DrugAllergy + ", " + (int) SnomedAllergy.DrugIntolerance + ")");
    }

    public static void Combine(long allergyDefNumKeep, long allergyDefNumCombine)
    {
        Db.NonQ("UPDATE allergy SET AllergyDefNum = " + allergyDefNumKeep + " WHERE AllergyDefNum = " + allergyDefNumCombine);
        Db.NonQ("UPDATE rxalert SET AllergyDefNum = " + allergyDefNumKeep + " WHERE AllergyDefNum = " + allergyDefNumCombine);
        Db.NonQ("Delete FROM allergydef WHERE AllergyDefNum =" + allergyDefNumCombine);
    }
}