using System.Collections.Generic;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public class PinBoardArgs(Patient patient, ApptOther apptOther, List<ApptOther> listApptOthers)
{
    public ApptOther ApptOther_ = apptOther;
    public List<ApptOther> ListApptOthers = listApptOthers;
    public Patient Patient_ = patient;
}