using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeBase;

public static class ODPrimitiveExtensions
{
    public static string[] Split(this string stringToSplit, string separator, StringSplitOptions options)
    {
        return stringToSplit.Split([separator], options);
    }

    public static bool IsNullOrEmpty<T>(this List<T> list)
    {
        return list == null || !list.Any();
    }

    public static bool IsNullOrEmpty(this Array array)
    {
        return array == null || array.Length == 0;
    }

    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static string GetDescription(this Enum value, bool useShortVersionIfAvailable = false)
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value);
        if (name == null)
        {
            return value.ToString();
        }

        var fieldInfo = type.GetField(name);
        if (fieldInfo == null)
        {
            return value.ToString();
        }

        if (useShortVersionIfAvailable)
        {
            var attrShort = (ShortDescriptionAttribute) Attribute.GetCustomAttribute(fieldInfo, typeof(ShortDescriptionAttribute));
            if (attrShort != null)
            {
                return attrShort.ShortDesc;
            }
        }

        var attr = (DescriptionAttribute) Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));
        if (attr == null)
        {
            return value.ToString();
        }

        return attr.Description;
    }

    public static bool In<T>(this T item, params T[] list)
    {
        //jordan approved
        return list.Contains(item);
    }
}

public class DateTools
{
    public static DateTime AddWeekDays(DateTime dateT, int numberOfDays)
    {
        var numberOfDaysToAdd = 0;
        for (var i = 0; i < numberOfDays; i++)
        {
            numberOfDaysToAdd++;
            if (dateT.AddDays(numberOfDaysToAdd).DayOfWeek == DayOfWeek.Saturday
                || dateT.AddDays(numberOfDaysToAdd).DayOfWeek == DayOfWeek.Sunday)
            {
                i--;
            }
        }

        dateT = dateT.AddDays(numberOfDaysToAdd);
        return dateT;
    }

    public static DateTime ToBeginningOfMonth(DateTime dateT)
    {
        return new DateTime(dateT.Year, dateT.Month, 1, 0, 0, 0, dateT.Kind);
    }

    public static DateTime ToEndOfMonth(DateTime dateT)
    {
        return new DateTime(dateT.Year, dateT.Month, DateTime.DaysInMonth(dateT.Year, dateT.Month), 23, 59, 59, dateT.Kind);
    }

    public static string ToStringDH(TimeSpan ts)
    {
        return string.Format("{0:%d} Days {0:%h} Hours", ts);
    }
}

public class StringTools
{
    public static string Truncate(string s, int maxCharacters, bool hasElipsis = false)
    {
        if (s == null || string.IsNullOrEmpty(s) || maxCharacters < 1)
        {
            return "";
        }

        if (s.Length > maxCharacters)
        {
            if (hasElipsis && maxCharacters > 4)
            {
                return s.Substring(0, maxCharacters - 3) + "...";
            }

            return s.Substring(0, maxCharacters);
        }

        return s;
    }

    public static string TruncateBeginning(string s, int maxCharacters)
    {
        if (s == null || string.IsNullOrEmpty(s) || maxCharacters < 1)
        {
            return "";
        }

        if (s.Length > maxCharacters)
        {
            return s.Substring(s.Length - maxCharacters, maxCharacters);
        }

        return s;
    }

    public static string ToUpperFirstOnly(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (value.Length == 1)
        {
            return value.ToUpper();
        }

        return value.Substring(0, 1).ToUpper() + value.Substring(1, value.Length - 1).ToLower();
    }

    public static string StripNonDigits(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return new string(Array.FindAll(value.ToCharArray(), y => char.IsDigit(y)));
    }

    public static string AppendLine(string orig, string addition)
    {
        if (string.IsNullOrEmpty(orig))
        {
            return addition;
        }

        if (orig != "")
        {
            orig += "\r\n";
        }

        return orig + addition;
    }

    public static string SubstringBefore(string value, string beforeThis)
    {
        return value.Substring(0, value.IndexOf(beforeThis));
    }

    public static string SubstringAfter(string value, string afterThis, bool isCaseSensitive = true)
    {
        int idxStart;
        if (isCaseSensitive)
        {
            idxStart = value.IndexOf(afterThis);
        }
        else
        {
            idxStart = value.ToLower().IndexOf(afterThis.ToLower());
        }

        return value.Substring(idxStart + afterThis.Length);
    }

    public static void RegReplace(StringBuilder stringBuilder, string pattern, string replacement, RegexOptions regexOptions = RegexOptions.IgnoreCase)
    {
        var newVal = Regex.Replace(stringBuilder.ToString(), pattern, replacement, regexOptions);
        stringBuilder.Clear();
        stringBuilder.Append(newVal);
    }
}

public class CompareDecimal
{
    public static bool IsZero(decimal val)
    {
        return Math.Abs(val) <= 0.0000001M;
    }

    public static bool IsLessThanZero(decimal val)
    {
        return val < -0.0000001M;
    }

    public static bool IsLessThanOrEqualToZero(decimal val)
    {
        return (val < -0.0000001M || Math.Abs(val) <= 0.0000001M);
    }

    public static bool IsGreaterThanOrEqualToZero(double val)
    {
        return (val > 0.0000001f || Math.Abs(val) <= 0.0000001f);
    }

    public static bool IsGreaterThanOrEqualToZero(decimal val)
    {
        return (val > 0.0000001M || Math.Abs(val) <= 0.0000001M);
    }

    public static bool IsGreaterThan(decimal val, decimal val2)
    {
        return val - val2 > 0.0000001M;
    }

    public static bool IsGreaterThanZero(decimal val)
    {
        return val > 0.0000001M;
    }

    public static bool IsGreaterThanZero(double val)
    {
        return val > 0.0000001f;
    }

    public static bool IsEqual(decimal val, decimal val2)
    {
        return Math.Abs(val - val2) < 0.0000001M;
    }
}

public class CompareDouble
{
    public static bool IsZero(double val)
    {
        return Math.Abs(val) <= 0.0000001f;
    }

    public static bool IsEqual(double val, double val2)
    {
        return IsZero(val - val2);
    }

    public static bool IsLessThan(double val, double val2)
    {
        return val2 - val > 0.0000001f;
    }

    public static bool IsGreaterThan(double val, double val2)
    {
        return val - val2 > 0.0000001f;
    }

    public static bool IsLessThanZero(double val)
    {
        return val < -0.0000001f;
    }

    public static bool IsLessThanOrEqualToZero(double val)
    {
        return (val < -0.0000001f || Math.Abs(val) <= 0.0000001f);
    }
}

public class CompareFloat
{
    public static bool IsZero(float val)
    {
        return Math.Abs(val) <= 0.0000001f;
    }

    public static bool IsEqual(float val, float val2)
    {
        return IsZero(val - val2);
    }
}

public class EnumTools
{
    public static T GetAttributeOrDefault<T>(Enum value) where T : Attribute, new()
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value);
        if (name == null)
        {
            return new T();
        }

        var field = type.GetField(name);
        if (field == null)
        {
            return new T();
        }

        var attr = Attribute.GetCustomAttribute(field, typeof(T)) as T;
        if (attr == null)
        {
            return new T();
        }

        return attr;
    }

    public static bool HasAnyFlag(Enum value, params Enum[] flags)
    {
        var valLong = Convert.ToInt64(value);
        if (valLong == 0)
        {
            return flags.Contains(value);
        }

        return flags.Any(x => (valLong & Convert.ToInt64(x)) > 0);
    }

    public static T AddFlag<T>(Enum value, params T[] flags)
    {
        var valLong = Convert.ToInt64(value);
        foreach (var flagToAdd in flags)
        {
            valLong |= Convert.ToInt64(flagToAdd);
        }

        return (T) Enum.ToObject(typeof(T), valLong);
    }

    public static T RemoveFlag<T>(Enum value, params T[] flags)
    {
        var valLong = Convert.ToInt64(value);
        foreach (var flagToRemove in flags)
        {
            valLong &= ~Convert.ToInt64(flagToRemove);
        }

        return (T) Enum.ToObject(typeof(T), valLong);
    }

    public static T ToggleFlag<T>(Enum value, T flagToToggle) where T : Enum
    {
        if (HasAnyFlag(value, flagToToggle))
        {
            value = RemoveFlag(value, flagToToggle);
        }
        else
        {
            value = AddFlag(value, flagToToggle);
        }

        return (T) Enum.ToObject(typeof(T), value);
    }

    public static IEnumerable<T> GetFlags<T>(T value) where T : Enum
    {
        foreach (var flag in Enum.GetValues(value.GetType()).Cast<T>().Where(x => Convert.ToInt64(x) != 0))
        {
            if (value.HasFlag(flag))
            {
                yield return flag;
            }
        }
    }

    public static List<T> ConvertListOfIntsToListOfEnums<T>(string input, bool doThrow = false) where T : Enum
    {
        var listOutput =
            //Comma-delim list of int values.
            input.Split([","], StringSplitOptions.RemoveEmptyEntries)
                //Only take strings which are convertible to int.
                .Where(x =>
                {
                    if (!int.TryParse(x, out var asInt))
                    {
                        if (doThrow)
                        {
                            throw new Exception($"Value {x} cannot be parsed to typeof {typeof(T)}");
                        }

                        return false;
                    }

                    return true;
                })
                //Already verified convertible to int.
                .Select(x => int.Parse(x))
                //Only take int(s) that are convertible to T enum.
                .Where(x =>
                {
                    if (!Enum.IsDefined(typeof(T), x))
                    {
                        if (doThrow)
                        {
                            throw new Exception($"Value {x} cannot be parsed to typeof {typeof(T)}");
                        }

                        return false;
                    }

                    return true;
                })
                //Convert from int to T enum.
                .Select(x => (T) Enum.ToObject(typeof(T), x)).ToList();
        return listOutput;
    }
}

public class GenericTools
{
    public static TTarget DeepCopy<TSource, TTarget>(TSource sourceObj) where TTarget : TSource
    {
        if (typeof(TTarget).IsInterface)
        {
            throw new Exception("Cannot deep copy to an interface.  TTarget must be a concrete class.");
        }

        return (TTarget) DeepClone(sourceObj, typeof(TTarget));
    }

    public static object DeepClone(object sourceObject, Type targetType = null)
    {
        if (sourceObject is null)
        {
            return null;
        }

        var binding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        targetType ??= sourceObject.GetType();
        object targetObject;
        var ex = new ArgumentException($"Failed to copy {targetType.FullName}");
        if (sourceObject is string sourceAsString)
        {
            var targetString = new char[sourceAsString.Length];
            sourceAsString.CopyTo(0, targetString, 0, sourceAsString.Length);
            targetObject = new string(targetString);
        }
        else if (sourceObject is Array sourceAsArray)
        {
            var array = Array.CreateInstance(targetType.GetElementType(), sourceAsArray.Length);
            for (var i = 0; i < sourceAsArray.Length; i++)
            {
                array.SetValue(DeepClone(sourceAsArray.GetValue(i)), i);
            }

            targetObject = array;
        }
        else if (sourceObject is IList sourceAsIList)
        {
            if (sourceAsIList.IsReadOnly)
            {
                throw ex; //Should we just skip these?
            }

            var list = (IList) Activator.CreateInstance(
                targetType.IsGenericTypeDefinition ? targetType.MakeGenericType(targetType.GenericTypeArguments) : targetType
            );
            foreach (var item in sourceAsIList)
            {
                list.Add(DeepClone(item, targetType.GenericTypeArguments.Single()));
            }

            targetObject = list;
        }
        else if (sourceObject is IDictionary sourceAsIDict)
        {
            if (sourceAsIDict.IsReadOnly)
            {
                throw ex; //Should we just skip these?
            }

            var dict = (IDictionary) Activator.CreateInstance(
                targetType.IsGenericTypeDefinition ? targetType.MakeGenericType(targetType.GenericTypeArguments) : targetType
            );
            foreach (DictionaryEntry entry in sourceAsIDict)
            {
                dict.Add(DeepClone(entry.Key), DeepClone(entry.Value));
            }

            targetObject = dict;
        }
        else
        {
            // Create an empty object and ignore its constructor.
            targetObject = FormatterServices.GetUninitializedObject(targetType);
            var sourceType = sourceObject.GetType();

            #region Copy Properties

            foreach (var property in sourceType.GetProperties(binding).Where(x => x.CanWrite && x.CanRead))
            {
                var value = property.GetValue(sourceObject, null);
                if (!property.PropertyType.IsPrimitive)
                {
                    value = DeepClone(value);
                }

                targetType.GetProperty(property.Name, binding)?.SetValue(targetObject, value, null);
            }

            #endregion

            #region Copy Fields

            foreach (var field in sourceType.GetFields(binding))
            {
                //Only writable fields
                var value = field.GetValue(sourceObject);
                if (!field.FieldType.IsPrimitive)
                {
                    value = DeepClone(value);
                }

                targetType.GetField(field.Name, binding)?.SetValue(targetObject, value);
            }

            #endregion
        }

        return targetObject;
    }
}

public class ListTools
{
    public static List<T> FromSingle<T>(T item)
    {
        return [item];
    }

    private static void CompareList<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TSource, bool> funcCompare = null, bool doEnforceOrderMatch = false)
    {
        if (first.Count() != second.Count())
        {
            //In case there are duplicates in either list.
            throw new Exception("Item count mismatch");
        }

        //No duplicates so any non-intersecting items indicates not a match.
        if (funcCompare == null)
        {
            //Use the default comparison func (usually for primitives only).
            if (first.Except(second).Union(second.Except(first)).Count() != 0)
            {
                throw new Exception("Items do not match");
            }
        }
        else
        {
            //Use the custom comparison func.
            var compare = new ODEqualityComparer<TSource>(funcCompare);
            if (first.Except(second, compare).Union(second.Except(first, compare)).Count() != 0)
            {
                throw new Exception("Items do not match");
            }
        }

        //We got this far so the 2 lists are same size and all elements match. Now compare order if required.
        if (!doEnforceOrderMatch)
        {
            return;
        }

        for (var i = 0; i < first.Count(); i++)
        {
            if (funcCompare == null)
            {
                if (!first.ElementAt(i).Equals(second.ElementAt(i)))
                {
                    throw new Exception("Items do not match");
                }
            }
            else
            {
                if (!funcCompare(first.ElementAt(i), second.ElementAt(i)))
                {
                    throw new Exception("Items do not match");
                }
            }
        }
        //All conditions of match been met. Match.			
    }

    public static List<TTarget> DeepCopy<TSource, TTarget>(List<TSource> source) where TTarget : TSource
    {
        if (typeof(TTarget).IsInterface)
        {
            throw new Exception("Cannot deep copy to an interface.  TTarget must be a concrete class.");
        }

        return (List<TTarget>) GenericTools.DeepClone(source, typeof(List<TTarget>));
    }

    public static bool In<T>(T item, params T[] list)
    {
        return list.Contains(item);
    }

    public static bool TryCompareList<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TSource, bool> funcCompare = null, bool doEnforceOrderMatch = false)
    {
        try
        {
            CompareList(first, second, funcCompare, doEnforceOrderMatch);
            return true;
        }
        catch (Exception e)
        {
        }

        return false;
    }

    public class ODEqualityComparer<TSource> : IEqualityComparer<TSource>
    {
        private Func<TSource, TSource, bool> _funcCompare;

        public ODEqualityComparer(Func<TSource, TSource, bool> funcCompare)
        {
            this._funcCompare = funcCompare;
        }

        public bool Equals(TSource x, TSource y)
        {
            return _funcCompare(x, y);
        }

        public int GetHashCode(TSource obj)
        {
            //Do not use obj.GetHashCode(). This will return a non-determinant value and cause .Equals() to be skipped in most cases.
            //Always return the same value (0 is acceptable). This will defer to the Equals override as the tie-breaker, which is what we want in this case.
            return 0;
        }
    }
}

public class ShortDescriptionAttribute : Attribute
{
    public ShortDescriptionAttribute(string shortDesc)
    {
        ShortDesc = shortDesc;
    }

    private string _shortDesc = "";

    public string ShortDesc
    {
        get { return _shortDesc; }
        set { _shortDesc = value; }
    }
}