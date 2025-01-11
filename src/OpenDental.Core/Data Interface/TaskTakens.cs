using System;
using DataConnectionBase;
using MySqlConnector;

namespace OpenDentBusiness;


public class TaskTakens
{
    #region Delete

    ///<summary>Deletes any TaskTaken for the given taskNum. Runs on the primary customers database.</summary>
    public static void DeleteForTask(long taskNum, bool retryOnLocal = true)
    {
        var command = "DELETE FROM tasktaken WHERE TaskNum=" + SOut.Long(taskNum);
        try
        {
            Db.NonQ(command);
        }
        catch (Exception ex)
        {
            MySqlException mysqlEx = null;
            if (ex is MySqlException)
                mysqlEx = (MySqlException) ex;
            else if (ex.InnerException is MySqlException) mysqlEx = (MySqlException) ex.InnerException;
            if (mysqlEx != null && mysqlEx.Number == 1042 && mysqlEx.Message.ToLower().Contains("unable to connect") && retryOnLocal)
            {
                //Unable to connect to the primary customers database. We will still delete the tasktaken on the local database.
                DeleteForTask(taskNum, false);
                return;
            }

            throw;
        }
    }

    #endregion
}