using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class UserodApptViews
{
    public static UserodApptView GetOneForUserAndClinic(long userNum, long clinicNum)
    {
        return UserodApptViewCrud.SelectOne("SELECT * FROM userodapptview WHERE UserNum = " + userNum + " AND ClinicNum = " + clinicNum);
    }

    public static void InsertOrUpdate(long userNum, long clinicNum, long apptViewNum)
    {
        var userodApptView = new UserodApptView
        {
            UserNum = userNum,
            ClinicNum = clinicNum,
            ApptViewNum = apptViewNum
        };

        var existingUserodApptView = GetOneForUserAndClinic(userodApptView.UserNum, userodApptView.ClinicNum);
        if (existingUserodApptView is null)
        {
            Insert(userodApptView);
        }

        else if (existingUserodApptView.ApptViewNum != userodApptView.ApptViewNum)
        {
            existingUserodApptView.ApptViewNum = userodApptView.ApptViewNum;

            Update(existingUserodApptView);
        }
    }

    public static void Insert(UserodApptView userodApptView)
    {
        UserodApptViewCrud.Insert(userodApptView);
    }

    public static void Update(UserodApptView userodApptView)
    {
        UserodApptViewCrud.Update(userodApptView);
    }
}