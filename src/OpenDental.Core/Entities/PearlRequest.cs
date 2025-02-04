using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class PearlRequest : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long PearlRequestNum;

    ///<summary>Request ID given to Pearl to uniquely identify this request. Generated as a GUID before uploading an image to Pearl.</summary>
    public string RequestId;

    ///<summary>FK to document. Links this request to the image that was sent. This is sufficient for mounts because mount images are sent individually to Pearl.</summary>
    public long DocNum;

    ///<summary>Enum:EnumPearlStatus Keeps track of the request's status. Can be Polling, Received, or Error.</summary>
    public EnumPearlStatus RequestStatus;

    ///<summary>The time the image was originally sent to Pearl.</summary>
    public DateTime DateTSent;

    ///<summary>The most recent time an API call was made to Pearl to check the status of this request.</summary>
    public DateTime DateTChecked;
}

public enum EnumPearlStatus
{
    ///<summary>0 - An individual machine is actively polling Pearl.</summary>
    Polling,

    ///<summary>1 - The image was successfully processed and AI annotations were returned from Pearl.</summary>
    Received,

    ///<summary>2 - An error occurred on Pearl’s side. Only set for errors that prevent this request from ever being fulfilled.</summary>
    Error,

    ///<summary>3 - Pearl did not give results within the timeout period of 10 minutes. Polling for this request can be retried.</summary>
    TimedOut,

    ///<summary>4 - The image is being uploaded to Pearl.</summary>
    Uploading
}