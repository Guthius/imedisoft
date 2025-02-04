using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class AlertRead : TableBase
{
    ///<summary>FK to alertitem.AlertItemNum.</summary>
    public long AlertItemNum;

    ///<summary>FK to userod.UserNum.</summary>
    public long UserNum;

    public AlertRead()
    {
    }

    public AlertRead(long alertItemNum, long userNum)
    {
        AlertItemNum = alertItemNum;
        UserNum = userNum;
    }
}