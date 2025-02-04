using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Caching;

[XmlType(TypeName = "A")]
public class FeeLim
{
    [XmlElement("A", typeof(long))]
    public long FeeNum;

    [XmlElement("B", typeof(double))]
    public double Amount;

    [XmlElement("C", typeof(long))]
    public long FeeSched;

    [XmlElement("D", typeof(long))]
    public long CodeNum;

    [XmlElement("E", typeof(long))] [DefaultValue(0L)]
    public long ClinicNum;

    [XmlElement("F", typeof(long))] [DefaultValue(0L)]
    public long ProvNum;

    [XmlElement("G", typeof(DateTime))]
    public DateTime SecDateTEdit;

    public static explicit operator Fee(FeeLim feeLim)
    {
        return new Fee
        {
            FeeNum = feeLim.FeeNum,
            Amount = feeLim.Amount,
            FeeSched = feeLim.FeeSched,
            CodeNum = feeLim.CodeNum,
            ClinicNum = feeLim.ClinicNum,
            ProvNum = feeLim.ProvNum,
            SecDateTEdit = feeLim.SecDateTEdit
        };
    }

    public static explicit operator FeeLim(Fee f)
    {
        return new FeeLim
        {
            FeeNum = f.FeeNum,
            Amount = f.Amount,
            FeeSched = f.FeeSched,
            CodeNum = f.CodeNum,
            ClinicNum = f.ClinicNum,
            ProvNum = f.ProvNum,
            SecDateTEdit = f.SecDateTEdit
        };
    }
}