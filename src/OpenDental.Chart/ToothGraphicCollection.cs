using System;
using System.Collections;
using System.Linq;

namespace OpenDental.Chart;

public class ToothGraphicCollection : CollectionBase
{
    public ToothGraphic this[int index] => (ToothGraphic) List[index];

    public ToothGraphic this[string toothId]
    {
        get
        {
            if (toothId != "implant" && !ToothGraphic.IsValidToothId(toothId))
            {
                throw new ArgumentException("Tooth ID not valid: " + toothId);
            }

            return List.Cast<ToothGraphic>().FirstOrDefault(t => t.ToothId == toothId);
        }
    }

    public void Add(ToothGraphic value)
    {
        List.Add(value);
    }

    protected override void OnInsert(int index, object value)
    {
        if (value.GetType() != typeof(ToothGraphic))
        {
            throw new ArgumentException("value must be of type ToothGraphic.", nameof(value));
        }
    }
    
    protected override void OnRemove(int index, object value)
    {
        if (value.GetType() != typeof(ToothGraphic))
        {
            throw new ArgumentException("value must be of type ToothGraphic.", nameof(value));
        }
    }
    
    protected override void OnSet(int index, object oldValue, object newValue)
    {
        if (newValue.GetType() != typeof(ToothGraphic))
        {
            throw new ArgumentException("newValue must be of type ToothGraphic.", nameof(newValue));
        }
    }

    protected override void OnValidate(object value)
    {
        if (value.GetType() != typeof(ToothGraphic))
        {
            throw new ArgumentException("value must be of type ToothGraphic.");
        }
    }
    
    public ToothGraphicCollection Copy()
    {
        var collection = new ToothGraphicCollection();
        
        for (var i = 0; i < Count; i++)
        {
            collection.Add(this[i].Copy());
        }

        return collection;
    }
}