using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class PatFields
{
    public static PatField[] Refresh(long patNum)
    {
        return PatFieldCrud.SelectMany("SELECT * FROM patfield WHERE PatNum=" + (patNum)).ToArray();
    }

    public static List<PatField> GetPatientData(long patNum)
    {
        return PatFieldCrud.SelectMany("SELECT * FROM patfield WHERE PatNum=" + (patNum));
    }

    public static bool IsFieldNameInUse(string fieldName)
    {
        var command = "SELECT COUNT(*) FROM patfield WHERE FieldName='" + SOut.String(fieldName) + "'";
        return Db.GetCount(command) != "0";
    }

    public static List<long> GetPatNumsUsingPickItem(string patFieldPickItemName, string patFieldName)
    {
        var command = "SELECT * FROM patfield "
                      + "WHERE FieldName='" + SOut.String(patFieldName) + "' "
                      + "AND FieldValue='" + SOut.String(patFieldPickItemName) + "'";
        return PatFieldCrud.SelectMany(command).ConvertAll(x => x.PatNum);
    }

    public static void Update(PatField patField)
    {
        PatFieldCrud.Update(patField);
    }

    public static void UpdateFieldName(string patFieldNameNew, string patFieldNameOld)
    {
        var command = "UPDATE patfield SET FieldName='" + SOut.String(patFieldNameNew) + "' "
                      + "WHERE FieldName='" + SOut.String(patFieldNameOld) + "'";
        Db.NonQ(command);
    }

    public static void UpdatePatFieldValues(string patFieldName, string patFieldValueNew, string patFieldValueOld)
    {
        var command = "UPDATE patfield SET FieldValue='" + SOut.String(patFieldValueNew) + "' "
                      + "WHERE FieldName='" + SOut.String(patFieldName) + "' AND FieldValue='" + SOut.String(patFieldValueOld) + "'";
        Db.NonQ(command);
    }

    public static long Insert(PatField patField)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        patField.SecUserNumEntry = Security.CurUser.UserNum;
        return PatFieldCrud.Insert(patField);
    }

    public static void Delete(PatField pf)
    {
        var command = "DELETE FROM patfield WHERE PatFieldNum =" + SOut.Long(pf.PatFieldNum);
        Db.NonQ(command);
    }

    public static PatField GetByName(string name, PatField[] fieldList)
    {
        for (var i = 0; i < fieldList.Length; i++)
            if (fieldList[i].FieldName == name)
                return fieldList[i];

        return null;
    }

    public static void MakeDeleteLogEntry(PatField patField)
    {
        SecurityLogs.MakeLogEntry(EnumPermType.PatientFieldEdit, patField.PatNum, "Deleted patient field " + patField.FieldName + ".  Value before deletion: \"" + patField.FieldValue + "\"");
    }

    public static void MakeEditLogEntry(PatField patFieldOld, PatField patFieldCur)
    {
        SecurityLogs.MakeLogEntry(EnumPermType.PatientFieldEdit, patFieldCur.PatNum
            , "Edited patient field " + patFieldCur.FieldName + "\r\n"
              + "Old value" + ": \"" + patFieldOld.FieldValue + "\"  New value: \"" + patFieldCur.FieldValue + "\"");
    }

    public static List<PatField> GetPatFieldsForSuperFam(List<long> listPatNumsSuperFam)
    {
        if (listPatNumsSuperFam.Count == 0) return new List<PatField>();
        var listDisplayFields = DisplayFields.GetForCategory(DisplayFieldCategory.SuperFamilyGridCols)
            .FindAll(x => string.IsNullOrWhiteSpace(x.InternalName)); //patfields have DisplayField.InternalName blank.
        if (listDisplayFields.Count == 0) return new List<PatField>();
        var displayFieldList = string.Join(",", listDisplayFields.Select(x => "'" + SOut.String(x.Description) + "'"));
        var patNumList = string.Join(",", listPatNumsSuperFam.Select(x => SOut.Long(x)));
        var command = "SELECT * FROM patfield WHERE FieldName IN(" + displayFieldList + ") AND PatNum IN(" + patNumList + ")";
        return PatFieldCrud.SelectMany(command);
    }

    public static string GetAbbrOrValue(PatField patField, string displayFieldName)
    {
        if (patField == null) return ""; //Common if this patient has no patField yet.
        var patFieldDef = PatFieldDefs.GetFieldDefByFieldName(displayFieldName);
        if (patFieldDef == null || patFieldDef.FieldType != PatFieldType.PickList) return patField.FieldValue;
        //It's a picklist
        var listPatFieldPickItems = PatFieldPickItems.GetWhere(x => x.PatFieldDefNum == patFieldDef.PatFieldDefNum);
        var patFieldPickItem = listPatFieldPickItems.Find(x => x.Name == patField.FieldValue);
        if (patFieldPickItem != null && !string.IsNullOrWhiteSpace(patFieldPickItem.Abbreviation)) return patFieldPickItem.Abbreviation;
        return patField.FieldValue;
    }

    public static PatField GetPatField(long patFieldNum)
    {
        return PatFieldCrud.SelectOne(patFieldNum);
    }
}