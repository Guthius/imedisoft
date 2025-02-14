using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class ApptFields
{
    public static void Insert(ApptField apptField)
    {
        ApptFieldCrud.Insert(apptField);
    }

    public static void Upsert(ApptField apptField)
    {
        DeleteFieldForAppt(apptField.FieldName, apptField.AptNum);

        Insert(apptField);
    }

    public static void DeleteFieldForAppt(string fieldName, long aptNum)
    {
        Db.NonQ($"DELETE FROM apptfield WHERE AptNum = {aptNum} AND FieldName = '{SOut.String(fieldName)}'");
    }

    public static ApptField GetOne(long apptFieldNum)
    {
        return ApptFieldCrud.SelectOne(apptFieldNum);
    }

    public static List<ApptField> GetForAppt(long aptNum)
    {
        return ApptFieldCrud.SelectMany("SELECT * FROM apptfield WHERE AptNum = " + aptNum);
    }
}