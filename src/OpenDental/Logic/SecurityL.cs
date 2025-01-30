using System;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Logic;

public class SecurityL
{
    public static bool ChangePassword(bool isForcedLogOff, bool willRefreshSecurityCache = true)
    {
        if (Security.CurUser.UserNumCEMT != 0)
        {
            MsgBox.Show("FormOpenDental", "Use the CEMT tool to change your password.");
            return false;
        }

        using var formUserPassword = new FormUserPassword(isCreate: false, Security.CurUser.UserName);

        if (formUserPassword.ShowDialog() == DialogResult.Cancel)
        {
            if (!isForcedLogOff)
            {
                return false;
            }

            var formOpenDental = Application.OpenForms.OfType<FormOpenDental>().ToList()[0];

            formOpenDental.LogOffNow(true);

            return false;
        }

        var isPasswordStrong = formUserPassword.IsPasswordStrong;
        try
        {
            Userods.UpdatePassword(Security.CurUser, formUserPassword.PasswordContainer_, isPasswordStrong);
        }
        catch (Exception ex)
        {
            ODMessageBox.Show(ex.Message);
            return false;
        }

        Security.CurUser.PasswordIsStrong = formUserPassword.IsPasswordStrong;
        Security.CurUser.SetPassword(formUserPassword.PasswordContainer_);
        Security.PasswordTyped = formUserPassword.PasswordTyped;

        if (willRefreshSecurityCache)
        {
            DataValid.SetInvalid(InvalidType.Security);
        }

        return true;
    }

    public static bool IsAuthorizedToEditImage(Document document)
    {
        if (!Security.IsAuthorized(EnumPermType.ImageEdit, document.DateCreated))
        {
            return false;
        }

        return document.Signature.IsNullOrEmpty() || Security.IsAuthorized(EnumPermType.SignedImageEdit);
    }

    public static bool IsAuthorizedToDeleteImage(Document document)
    {
        if (!Security.IsAuthorized(EnumPermType.ImageDelete, document.DateCreated))
        {
            return false;
        }

        return document.Signature.IsNullOrEmpty() || Security.IsAuthorized(EnumPermType.SignedImageEdit);
    }
}