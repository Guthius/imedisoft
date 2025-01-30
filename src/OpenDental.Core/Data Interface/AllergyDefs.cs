using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class AllergyDefs
{
    public static AllergyDef GetOne(long allergyDefNum)
    {
        return AllergyDefCrud.SelectOne(allergyDefNum);
    }

    public static AllergyDef GetOne(long allergyDefNum, List<AllergyDef> listAllergyDefs)
    {
        for (var i = 0; i < listAllergyDefs.Count; i++)
            if (allergyDefNum == listAllergyDefs[i].AllergyDefNum)
                return listAllergyDefs[i]; //Gets the allergydef matching the allergy so we can use it to populate the grid

        return GetOne(allergyDefNum);
    }

    public static AllergyDef GetByDescription(string allergyDescription)
    {
        var command = "SELECT * FROM allergydef WHERE Description='" + SOut.String(allergyDescription) + "'";
        var retVal = AllergyDefCrud.SelectMany(command);
        if (retVal.Count > 0) return retVal[0];
        return null;
    }

    public static long Insert(AllergyDef allergyDef)
    {
        return AllergyDefCrud.Insert(allergyDef);
    }

    public static void Update(AllergyDef allergyDef)
    {
        AllergyDefCrud.Update(allergyDef);
    }

    public static void Delete(long allergyDefNum)
    {
        var command = "DELETE FROM allergydef WHERE AllergyDefNum = " + SOut.Long(allergyDefNum);
        Db.NonQ(command);
    }

    public static List<AllergyDef> GetAll(bool includeHidden)
    {
        var command = "";
        if (includeHidden)
            command = "SELECT * FROM allergydef ORDER BY Description";
        else
            command = "SELECT * FROM allergydef WHERE IsHidden=0"
                      + " ORDER BY Description";
        return AllergyDefCrud.SelectMany(command);
    }

    public static bool DefIsInUse(long allergyDefNum)
    {
        var command = "SELECT COUNT(*) FROM allergy WHERE AllergyDefNum=" + SOut.Long(allergyDefNum);
        if (Db.GetCount(command) != "0") return true;
        command = "SELECT COUNT(*) FROM rxalert WHERE AllergyDefNum=" + SOut.Long(allergyDefNum);
        if (Db.GetCount(command) != "0") return true;
        if (allergyDefNum == PrefC.GetLong(PrefName.AllergiesIndicateNone)) return true;
        return false;
    }

    public static List<AllergyDef> GetMultAllergyDefs(List<long> allergyDefNums)
    {
        var strAllergyDefNums = "";
        DataTable table;
        if (allergyDefNums.Count > 0)
        {
            for (var i = 0; i < allergyDefNums.Count; i++)
            {
                if (i > 0) strAllergyDefNums += "OR ";
                strAllergyDefNums += "AllergyDefNum='" + allergyDefNums[i] + "' ";
            }

            var command = "SELECT * FROM allergydef WHERE " + strAllergyDefNums;
            table = DataCore.GetTable(command);
        }
        else
        {
            table = new DataTable();
        }

        var listAllergyDefs = AllergyDefCrud.TableToList(table);
        return listAllergyDefs;
    }

    public static List<AllergyDef> GetAllergyDefs(long patNum, bool includeInactive)
    {
        var command = @"SELECT allergydef.* FROM allergydef
				INNER JOIN allergy ON allergy.AllergyDefNum=allergydef.AllergyDefNum
				WHERE allergy.PatNum=" + SOut.Long(patNum) + " ";
        if (!includeInactive) command += "AND allergy.StatusIsActive!=0";
        return AllergyDefCrud.TableToList(DataCore.GetTable(command));
    }

    public static string GetSnomedAllergyDesc(SnomedAllergy snowmedAllergy)
    {
        return snowmedAllergy switch
        {
            SnomedAllergy.AdverseReactions => "420134006 - Propensity to adverse reactions (disorder)",
            SnomedAllergy.AdverseReactionsToDrug => "419511003 - Propensity to adverse reactions to drug (disorder)",
            SnomedAllergy.AdverseReactionsToFood => "418471000 - Propensity to adverse reactions to food (disorder)",
            SnomedAllergy.AdverseReactionsToSubstance => "419199007 - Propensity to adverse reactions to substance (disorder)",
            SnomedAllergy.AllergyToSubstance => "418038007 - Allergy to substance (disorder)",
            SnomedAllergy.DrugAllergy => "416098002 - Drug allergy (disorder)",
            SnomedAllergy.DrugIntolerance => "59037007 - Drug intolerance (disorder)",
            SnomedAllergy.FoodAllergy => "235719002 - Food allergy (disorder)",
            SnomedAllergy.FoodIntolerance => "420134006 - Food intolerance (disorder)",
            SnomedAllergy.None => "",
            _ => "Error"
        };
    }

    public static string GetDescription(long allergyDefNum)
    {
        return allergyDefNum == 0 ? "" : AllergyDefCrud.SelectOne(allergyDefNum).Description;
    }

    public static AllergyDef GetAllergyDefFromMedication(long medicationNum)
    {
        if (medicationNum == 0) return null;
        var command = "SELECT * FROM allergydef WHERE MedicationNum=" + SOut.Long(medicationNum);
        return AllergyDefCrud.SelectOne(command);
    }

    public static AllergyDef GetAllergyDefFromRxnorm(long rxCui)
    {
        if (rxCui == 0) return null;
        var command = "SELECT allergydef.* FROM allergydef "
                      + "INNER JOIN medication ON allergydef.MedicationNum=medication.MedicationNum "
                      + "AND medication.RxCui=" + SOut.Long(rxCui) + " "
                      + "WHERE allergydef.SnomedType IN(" + (int) SnomedAllergy.DrugAllergy + "," + (int) SnomedAllergy.DrugIntolerance + ") ";
        return AllergyDefCrud.SelectOne(command);
    }

    public static void Combine(long allergyDefNumKeep, long allergyDefNumCombine)
    {
        //Update FKs
        //allergy table
        var command = "UPDATE allergy SET AllergyDefNum = "
                      + SOut.Long(allergyDefNumKeep)
                      + " WHERE AllergyDefNum ="
                      + SOut.Long(allergyDefNumCombine);
        Db.NonQ(command);
        //reminderrule table
        command = "UPDATE reminderrule SET CriterionFK = "
                  + SOut.Long(allergyDefNumKeep)
                  + " WHERE ReminderCriterion = " + SOut.Enum(EhrCriterion.Allergy)
                  + " AND CriterionFK = "
                  + SOut.Long(allergyDefNumCombine);
        Db.NonQ(command);
        //rxalert table
        command = "UPDATE rxalert SET AllergyDefNum = "
                  + SOut.Long(allergyDefNumKeep)
                  + " WHERE AllergyDefNum = "
                  + SOut.Long(allergyDefNumCombine);
        Db.NonQ(command);
        //Delete allergydef
        command = "Delete FROM allergydef WHERE AllergyDefNum ="
                  + SOut.Long(allergyDefNumCombine);
        Db.NonQ(command);
    }
}