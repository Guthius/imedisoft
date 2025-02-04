using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class CommOptOut : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long CommOptOutNum;

    ///<summary>FK to patient.PatNum. The patient who is opting out of this form of communication.</summary>
    public long PatNum;

    ///<summary>Enum:CommOptOutType The type of communication for which this patient does not want to receive automated sms.</summary>
    public CommOptOutType OptOutSms;

    ///<summary>Enum:CommOptOutType The type of communication for which this patient does not want to receive automated email.</summary>
    public CommOptOutType OptOutEmail;

    public bool IsOptedOut(CommOptOutMode mode, CommOptOutType type)
    {
        if (type == 0)
        {
            //None
            return false;
        }

        return mode switch
        {
            CommOptOutMode.Text => OptOutSms.HasFlag(CommOptOutType.All) || OptOutSms.HasFlag(type),
            CommOptOutMode.Email => OptOutEmail.HasFlag(CommOptOutType.All) || OptOutEmail.HasFlag(type),
            _ => false
        };
    }
}

[Flags]
public enum CommOptOutType
{
    All = 0b1,
    Statements = 0b100000000,
    MsgToPay = 0b1000000000000
}

public enum CommOptOutMode
{
    Text,
    Email
}