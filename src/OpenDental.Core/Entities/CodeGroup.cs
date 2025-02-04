using System.ComponentModel;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class CodeGroup : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long CodeGroupNum;

    public string GroupName;

    ///<summary>List of D codes with commas and dashes. No spaces. Example: "D0000-D0999,D2140-D2161,D2750" would mean all exams and xrays, some amalgams, and a crown.</summary>
    public string ProcCodes;

    public int ItemOrder;

    ///<summary>Enum:EnumCodeGroupFixed 0=None,BW,PanoFMX,Exam,Perio,Prophy,SRP,FMDebride,Fluoride,Sealant. Six are used in sheet static text fields (example StaticTextField.dateLastBW), and seven are used in Ins History Window.</summary>
    public EnumCodeGroupFixed CodeGroupFixed;

    public bool IsHidden;

    ///<summary> If true, this codegroup will show in Age Limitations grid. Control of showing in Freq Lim is done separately using IsHidden.</summary>
    public bool ShowInAgeLimit;

    public CodeGroup Copy()
    {
        return (CodeGroup) MemberwiseClone();
    }

    public bool IsVisible()
    {
        if (ShowInAgeLimit)
        {
            return true;
        }

        return !IsHidden;
    }
}

///<summary>This enum replaces the FrequencyType enum at bottom of Benefits.cs.  See description in CodeGroup.CodeGroupFixed.</summary>
public enum EnumCodeGroupFixed
{
    ///<summary>0</summary>
    None,

    ///<summary>1</summary>
    [Description("Bitewing")]
    BW,

    ///<summary>2</summary>
    [Description("Pano/FMX")]
    PanoFMX,

    ///<summary>3</summary>
    [Description("Exam")]
    Exam,

    ///<summary>4</summary>
    [Description("Perio Maintenance")]
    Perio,

    ///<summary>5</summary>
    [Description("Prophylaxis")]
    Prophy,

    ///<summary>6- When used in InsHist window, the quadrant is hard coded for each of the 4 rows.</summary>
    [Description("SRP")]
    SRP,

    ///<summary>7</summary>
    [Description("Full Debridement")]
    FMDebride,

    ///<summary>8</summary>
    [Description("Fluoride")]
    Fluoride,

    ///<summary>9</summary>
    [Description("Sealant")]
    Sealant,
}