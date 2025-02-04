using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class PatFields
{
    public static PatField[] Refresh(long patNum)
    {
        return PatFieldCrud.SelectMany("SELECT * FROM patfield WHERE PatNum=" + patNum).ToArray();
    }

    public static List<PatField> GetPatientData(long patNum)
    {
        return PatFieldCrud.SelectMany("SELECT * FROM patfield WHERE PatNum=" + patNum);
    }

    public static List<long> GetPatNumsUsingPickItem(string fieldValue, string fieldName)
    {
        return PatFieldCrud
            .SelectMany(
                "SELECT * FROM patfield " +
                "WHERE FieldName = '" + SOut.String(fieldName) + "' " +
                "AND FieldValue = '" + SOut.String(fieldValue) + "'")
            .ConvertAll(x => x.PatNum);
    }

    public static void Update(PatField patField)
    {
        PatFieldCrud.Update(patField);
    }

    public static void UpdateFieldName(string patFieldNameNew, string patFieldNameOld)
    {
        Db.NonQ("UPDATE patfield SET FieldName='" + SOut.String(patFieldNameNew) + "' " + "WHERE FieldName='" + SOut.String(patFieldNameOld) + "'");
    }

    public static void UpdatePatFieldValues(string patFieldName, string patFieldValueNew, string patFieldValueOld)
    {
        Db.NonQ("UPDATE patfield SET FieldValue='" + SOut.String(patFieldValueNew) + "' WHERE FieldName='" + SOut.String(patFieldName) + "' AND FieldValue='" + SOut.String(patFieldValueOld) + "'");
    }

    public static void Insert(PatField patField)
    {
        patField.SecUserNumEntry = Security.CurUser.UserNum;

        PatFieldCrud.Insert(patField);
    }

    public static void Delete(PatField pf)
    {
        Db.NonQ("DELETE FROM patfield WHERE PatFieldNum = " + pf.PatFieldNum);
    }

    public static PatField GetByName(string name, PatField[] patFields)
    {
        return patFields.FirstOrDefault(patField => patField.FieldName == name);
    }

    public static void MakeDeleteLogEntry(PatField patField)
    {
        SecurityLogs.MakeLogEntry(EnumPermType.PatientFieldEdit, patField.PatNum, "Deleted patient field " + patField.FieldName + ".  Value before deletion: \"" + patField.FieldValue + "\"");
    }

    public static void MakeEditLogEntry(PatField patFieldOld, PatField patFieldCur)
    {
        SecurityLogs.MakeLogEntry(EnumPermType.PatientFieldEdit, patFieldCur.PatNum,
            "Edited patient field " + patFieldCur.FieldName + "\r\n" +
            "Old value: \"" + patFieldOld.FieldValue + "\" " +
            "New value: \"" + patFieldCur.FieldValue + "\"");
    }

    public static List<PatField> GetPatFieldsForSuperFam(List<long> patNumsSuperFam)
    {
        if (patNumsSuperFam.Count == 0)
        {
            return [];
        }

        var displayFields = DisplayFields
            .GetForCategory(DisplayFieldCategory.SuperFamilyGridCols)
            .FindAll(x => string.IsNullOrWhiteSpace(x.InternalName));

        if (displayFields.Count == 0)
        {
            return [];
        }

        var displayFieldList = string.Join(",", displayFields.Select(x => "'" + SOut.String(x.Description) + "'"));
        var patNums = string.Join(",", patNumsSuperFam.Select(x => x));

        return PatFieldCrud.SelectMany("SELECT * FROM patfield WHERE FieldName IN (" + displayFieldList + ") AND PatNum IN (" + patNums + ")");
    }

    public static string GetAbbrOrValue(PatField patField, string displayFieldName)
    {
        if (patField is null)
        {
            return "";
        }

        var patFieldDef = PatFieldDefs.GetFieldDefByFieldName(displayFieldName);
        if (patFieldDef is not {FieldType: PatFieldType.PickList})
        {
            return patField.FieldValue;
        }

        var patFieldPickItems = PatFieldPickItems.GetWhere(x => x.PatFieldDefNum == patFieldDef.PatFieldDefNum);
        var patFieldPickItem = patFieldPickItems.Find(x => x.Name == patField.FieldValue);
        if (patFieldPickItem is not null && !string.IsNullOrWhiteSpace(patFieldPickItem.Abbreviation))
        {
            return patFieldPickItem.Abbreviation;
        }

        return patField.FieldValue;
    }

    public static PatField GetPatField(long patFieldNum)
    {
        return PatFieldCrud.SelectOne(patFieldNum);
    }
}