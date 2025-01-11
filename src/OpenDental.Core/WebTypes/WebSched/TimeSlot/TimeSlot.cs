using System;

namespace OpenDentBusiness.WebTypes.WebSched.TimeSlot;

[Serializable]
public class TimeSlot : WebBase
{
    public DateTime DateTimeStart;
    public DateTime DateTimeStop;
    public long OperatoryNum;
    public long ProvNum;

    public TimeSlot()
    {
    }

    public TimeSlot(DateTime dateTimeStart, DateTime dateTimeStop, long operatoryNum = 0, long provNum = 0, long defNumApptType = 0)
    {
        DateTimeStart = dateTimeStart;
        DateTimeStop = dateTimeStop;
        OperatoryNum = operatoryNum;
        ProvNum = provNum;
    }

    public TimeSlot Copy()
    {
        return (TimeSlot) MemberwiseClone();
    }
}