using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CodeBase;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class SplitCollection : ICollection<PaySplit>, IXmlSerializable
{
    private readonly Dictionary<string, PaySplit> _dictPaySplits = new();

    public int Count => _dictPaySplits.Count;
    public bool IsReadOnly => false;

    private static string GetUniqueKeyFromPaySplit(PaySplit paySplit)
    {
        if (paySplit.SplitNum > 0)
        {
            return paySplit.SplitNum.ToString();
        }

        if (paySplit.TagOD is string od && od != "")
        {
            return od;
        }

        throw new ODException("Invalid PaySplit with no SplitNum or invalid TagOD");
    }

    public void Add(PaySplit paySplit)
    {
        var uniqueKey = GetUniqueKeyFromPaySplit(paySplit);
        
        if (_dictPaySplits.ContainsKey(uniqueKey))
        {
            return;
        }

        _dictPaySplits.Add(uniqueKey, paySplit);
    }

    public void AddRange(List<PaySplit> listPaySplits)
    {
        foreach (var split in listPaySplits)
        {
            Add(split);
        }
    }

    public void Clear()
    {
        _dictPaySplits.Clear();
    }

    public bool Contains(PaySplit paySplit)
    {
        return _dictPaySplits.ContainsKey(GetUniqueKeyFromPaySplit(paySplit));
    }

    public void CopyTo(PaySplit[] array, int arrayIndex)
    {
        for (var i = arrayIndex; i < _dictPaySplits.Values.Count; i++)
        {
            array[i] = _dictPaySplits.Values.ToList()[i];
        }
    }

    public bool Remove(PaySplit paySplit)
    {
        return _dictPaySplits.Remove(GetUniqueKeyFromPaySplit(paySplit));
    }

    public IEnumerator<PaySplit> GetEnumerator()
    {
        return _dictPaySplits.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<PaySplit>) _dictPaySplits.Values).GetEnumerator();
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        var serializer = new XmlSerializer(typeof(List<PaySplit>));
        var wasEmpty = reader.IsEmptyElement;
        reader.Read();
        if (wasEmpty)
        {
            return;
        }

        Clear();
        AddRange((List<PaySplit>) serializer.Deserialize(reader));
        reader.ReadEndElement();
    }

    public void WriteXml(XmlWriter writer)
    {
        var serializer = new XmlSerializer(typeof(List<PaySplit>));
        serializer.Serialize(writer, _dictPaySplits.Values.ToList());
    }
}