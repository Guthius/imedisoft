using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class PatFields
{
    #region Misc Methods

    public static void Upsert(PatField patField)
    {
        if (patField.PatFieldNum > 0)
            Update(patField);
        else
            Insert(patField);
    }

    #endregion

    ///<summary>Gets a list of all PatFields for a given patient.</summary>
    public static PatField[] Refresh(long patNum)
    {
        var command = "SELECT * FROM patfield WHERE PatNum=" + SOut.Long(patNum);
        return PatFieldCrud.SelectMany(command).ToArray();
    }

    
    public static List<PatField> GetPatientData(long patNum)
    {
        var command = "SELECT * FROM patfield WHERE PatNum=" + SOut.Long(patNum);
        return PatFieldCrud.SelectMany(command);
    }

    ///<summary>Gets all PatFields from the database. Used for API.</summary>
    public static List<PatField> GetPatFieldsForApi(int limit, int offset, long patNum, string fieldName, DateTime dateSecDateTEdit)
    {
        var command = "SELECT * FROM patfield WHERE SecDateTEdit >= " + SOut.DateT(dateSecDateTEdit) + " ";
        if (patNum > 0) command += "AND PatNum=" + SOut.Long(patNum) + " ";
        if (fieldName != "") command += "AND FieldName='" + SOut.String(fieldName) + "' ";
        command += "ORDER BY PatFieldNum " //Ensure order for limit and offset.
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return PatFieldCrud.SelectMany(command);
    }

    ///<summary>Gets one PatField from the database. Used for API.</summary>
    public static PatField GetPatFieldForApi(long patFieldNum)
    {
        var command = "SELECT * FROM patfield WHERE PatFieldNum=" + SOut.Long(patFieldNum);
        return PatFieldCrud.SelectOne(command);
    }

    ///<summary>Returns whether there are more than 0 PatFields with the given field name.</summary>
    public static bool IsFieldNameInUse(string fieldName)
    {
        var command = "SELECT COUNT(*) FROM patfield WHERE FieldName='" + SOut.String(fieldName) + "'";
        return Db.GetCount(command) != "0";
    }

    ///<summary>Returns list of patnums where the pickitem is still in use.</summary>
    public static List<long> GetPatNumsUsingPickItem(string patFieldPickItemName, string patFieldName)
    {
        var command = "SELECT * FROM patfield "
                      + "WHERE FieldName='" + SOut.String(patFieldName) + "' "
                      + "AND FieldValue='" + SOut.String(patFieldPickItemName) + "'";
        return PatFieldCrud.SelectMany(command).ConvertAll(x => x.PatNum);
    }

    /// <summary>
    ///     Get all PatFields for the given fieldName which belong to patients who have a corresponding entry in the
    ///     RegistrationKey table. DO NOT REMOVE! Used by OD WebApps solution.
    /// </summary>
    public static List<PatField> GetPatFieldsWithRegKeys(string fieldName)
    {
        var command = "SELECT * FROM patfield WHERE FieldName='" + SOut.String(fieldName) + "' AND PatNum IN (SELECT PatNum FROM registrationkey)";
        return PatFieldCrud.SelectMany(command);
    }

    
    public static void Update(PatField patField)
    {
        PatFieldCrud.Update(patField);
    }

    ///<summary>For all patients in the entire db when a FieldName changes.</summary>
    public static void UpdateFieldName(string patFieldNameNew, string patFieldNameOld)
    {
        var command = "UPDATE patfield SET FieldName='" + SOut.String(patFieldNameNew) + "' "
                      + "WHERE FieldName='" + SOut.String(patFieldNameOld) + "'";
        Db.NonQ(command);
    }

    ///<summary>For all patients in the entire db when a PatField's PickListItem value changes.</summary>
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

    ///<summary>Frequently returns null.</summary>
    public static PatField GetByName(string name, PatField[] fieldList)
    {
        for (var i = 0; i < fieldList.Length; i++)
            if (fieldList[i].FieldName == name)
                return fieldList[i];

        return null;
    }

    /// <summary>
    ///     A helper method to make a security log entry for deletion.  Because we have several patient field edit
    ///     windows, this will allow us to change them all at once.
    /// </summary>
    public static void MakeDeleteLogEntry(PatField patField)
    {
        SecurityLogs.MakeLogEntry(EnumPermType.PatientFieldEdit, patField.PatNum, "Deleted patient field " + patField.FieldName + ".  Value before deletion: \"" + patField.FieldValue + "\"");
    }

    /// <summary>
    ///     A helper method to make a security log entry for an edit.  Because we have several patient field edit windows,
    ///     this will allow us to change them all at once.
    /// </summary>
    public static void MakeEditLogEntry(PatField patFieldOld, PatField patFieldCur)
    {
        SecurityLogs.MakeLogEntry(EnumPermType.PatientFieldEdit, patFieldCur.PatNum
            , "Edited patient field " + patFieldCur.FieldName + "\r\n"
              + "Old value" + ": \"" + patFieldOld.FieldValue + "\"  New value: \"" + patFieldCur.FieldValue + "\"");
    }

    ///<summary>Gets all PatFields for all patients in list.</summary>
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

    /// <summary>
    ///     Abbreviations only exist for pick list items. The patFieldDefName passed in does not necessarily need to be a
    ///     picklist.
    /// </summary>
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