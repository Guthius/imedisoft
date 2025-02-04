using System;
using CodeBase;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness.Eclaims;

public class VyneDental
{
    public static string ErrorMessage = "";

    public static bool Launch(Clearinghouse clearinghouse)
    {
        try
        {
            ODFileUtils.ProcessStart(clearinghouse.ClientProgram);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
                
            return false;
        }

        return true;
    }
}