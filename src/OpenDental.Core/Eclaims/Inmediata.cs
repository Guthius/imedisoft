using System;
using CodeBase;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness.Eclaims;

public class Inmediata
{
    public static string ErrorMessage = "";

    public static bool Launch(Clearinghouse clearinghouseClin)
    {
        try
        {
            ODFileUtils.ProcessStart(clearinghouseClin.ClientProgram);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;

            return false;
        }

        return true;
    }
}