using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace OpenDental.Graph.Base
{
    public static class GraphExtensions
    {
        public static double RoundSignificant(this double val)
        {
            var asInt = (int) Math.Abs(val);
            var ret = (Math.Truncate(asInt / Math.Pow(10, asInt.ToString().Length - 1)) + 1) * Math.Pow(10, asInt.ToString().Length - 1);
            if (val < 0)
            {
                ret *= -1;
            }

            return ret;
        }
    }

    public class ComboItemIntValue : ComboItem<int>
    {
    }

    public class ComboItem<T>
    {
        public T Value { get; set; }
        public string Display { get; set; }
    }

    public static class ComboBoxEx
    {
        public delegate string StringFromEnumArgs<in T>(T item);

        public delegate bool BoolFromEnumArgs<in T>(T item);

        public static void SetDataToEnums<T>(this ComboBox combo, bool includeAllAtTop, bool showValueIndDisplay = true, int min = -1, int max = -1, StringFromEnumArgs<T> getStringFromEnum = null) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new Exception("T must be an Enum type");
            }

            SetDataToEnums(combo, Enum.GetValues(typeof(T)).Cast<T>().ToList(), includeAllAtTop, showValueIndDisplay, min, max, getStringFromEnum);
        }

        public static void SetDataToEnumsPrimitive<T>(this ComboBox combo, StringFromEnumArgs<T> getStringFromEnum) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new Exception("T must be an Enum type");
            }

            var list = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            
            SetDataToEnumsPrimitive(combo, list, 0, list.Count - 1, getStringFromEnum);
        }

        public static void SetDataToEnumsPrimitive<T>(this ComboBox combo, int min = -1, int max = -1, StringFromEnumArgs<T> getStringFromEnum = null, BoolFromEnumArgs<T> includeEnumValue = null) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new Exception("T must be an Enum type");
            }

            SetDataToEnumsPrimitive(combo, Enum.GetValues(typeof(T)).Cast<T>().ToList(), min, max, getStringFromEnum, includeEnumValue);
        }

        public static T GetValue<T>(this ComboBox combo)
        {
            var item = (ComboItem<T>) combo.SelectedItem;
            return item == null ? default : item.Value;
        }

        public static ComboItem<T> GetItem<T>(this ComboBox combo, T item) where T : struct, IConvertible
        {
            foreach (var x in combo.Items)
            {
                var comboItem = (ComboItem<T>) x;
                if (comboItem.Value.ToString() == item.ToString())
                {
                    return comboItem;
                }
            }

            return null;
        }

        public static void SetItem<T>(this ComboBox combo, T item) where T : struct, IConvertible
        {
            foreach (var x in combo.Items)
            {
                var comboItem = (ComboItem<T>) x;
                if (comboItem.Value.ToString() != item.ToString())
                {
                    continue;
                }
                
                combo.SelectedItem = comboItem;
                return;
            }
        }

        public static void SetDataToEnumsPrimitive<T>(this ComboBox combo, List<T> enumValues, int min = -1, int max = -1, StringFromEnumArgs<T> getStringFromEnum = null, BoolFromEnumArgs<T> includeEnumValue = null) where T : struct, IConvertible
        {
            var listItems = new List<ComboItem<T>>();
            
            enumValues.ForEach(x =>
            {
                var val = Convert.ToInt32(x);
                if (min >= 0 && val < min)
                {
                    return;
                }

                if (max >= 0 && val > max)
                {
                    return;
                }

                if (includeEnumValue != null)
                {
                    if (!includeEnumValue(x))
                    {
                        return;
                    }
                }

                var display = x.ToString();
                if (getStringFromEnum != null)
                {
                    display = getStringFromEnum(x);
                }

                listItems.Add(new ComboItem<T> {Value = x, Display = display});
            });
            
            combo.BindData(listItems);
        }

        public static void UpdateDisplayName<T>(this ComboBox combo, T atValue, string newDisplayName) where T : struct, IConvertible
        {
            combo.GetItem(atValue).Display = newDisplayName;
            ((BindingList<ComboItem<T>>) combo.DataSource).ResetBindings();
        }
        
        public static void RemoveItem<T>(this ComboBox combo, T atValue) where T : struct, IConvertible
        {
            ((BindingList<ComboItem<T>>) combo.DataSource).Remove(combo.GetItem(atValue));
            ((BindingList<ComboItem<T>>) combo.DataSource).ResetBindings();
        }
        
        public static void AddItem<T>(this ComboBox combo, T displayValue, string displayName) where T : struct, IConvertible
        {
            if (combo.GetItem(displayValue) != null)
            {
                combo.UpdateDisplayName(displayValue, displayName);
                return;
            }

            ((BindingList<ComboItem<T>>) combo.DataSource).Add(new ComboItem<T> {Display = displayName, Value = displayValue});
            ((BindingList<ComboItem<T>>) combo.DataSource).ResetBindings();
        }
        
        public static void InsertItem<T>(this ComboBox combo, T displayValue, string displayName, int index) where T : struct, IConvertible
        {
            if (combo.GetItem(displayValue) != null)
            {
                combo.UpdateDisplayName(displayValue, displayName);
                return;
            }

            try
            {
                ((BindingList<ComboItem<T>>) combo.DataSource).Insert(index, new ComboItem<T> {Display = displayName, Value = displayValue});
                ((BindingList<ComboItem<T>>) combo.DataSource).ResetBindings();
            }
            catch
            {
                combo.AddItem(displayValue, displayName);
            }
        }

        public static void SetDataToEnums<T>(this ComboBox combo, List<T> enumValues, bool includeAllAtTop, bool showValueIndDisplay = true, int min = -1, int max = -1, StringFromEnumArgs<T> getStringFromEnum = null) where T : struct, IConvertible
        {
            var listItems = new List<ComboItemIntValue>();
            if (includeAllAtTop)
            {
                listItems.Add(new ComboItemIntValue {Value = -1, Display = "All"});
            }

            enumValues.ForEach(x =>
            {
                var val = Convert.ToInt32(x);
                if (min >= 0 && val < min)
                {
                    return;
                }

                if (max >= 0 && val > max)
                {
                    return;
                }

                var display = (showValueIndDisplay ? Convert.ToInt32(x) + " - " : "") + x;
                if (getStringFromEnum != null)
                {
                    display = getStringFromEnum(x);
                }

                listItems.Add(new ComboItemIntValue {Value = Convert.ToInt32(x), Display = display});
            });
            
            var selIdx = -1;
            if (combo.SelectedItem is ComboItemIntValue _)
            {
                selIdx = listItems.FindIndex(x => x.Value == ((ComboItemIntValue) combo.SelectedItem).Value);
            }

            combo.BindData(listItems);
            if (selIdx >= 0)
            {
                combo.SelectedIndex = selIdx;
            }
        }

        private static void BindData<T>(this ComboBox combo, List<T> list, string valueMember = "Value", string displayMember = "Display")
        {
            var binder = new BindingList<T>(list);
            combo.DataSource = binder;
            combo.ValueMember = valueMember;
            combo.DisplayMember = displayMember;
        }
    }
}