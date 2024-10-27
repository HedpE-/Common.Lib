using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Common.Lib.Extensions
{
    /// <summary>
	/// Summary description for EnumExtensions.
	/// </summary>
	public static class EnumExtensions
    {
        /// <summary>
        /// Returns the value of the DescriptionAttribute if the specified Enum value has one.
        /// If not, returns the ToString() representation of the Enum value.
        /// </summary>
        /// <param name="value">The Enum to get the description for</param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static string GetDescription<T>(this T value) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            return GetDescription(value as Enum);
        }

        public static IEnumerable<T> GetValues<T>() where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivilant enumerated object.
        /// A parameter specified whether the operation is case-sensitive.
        /// Note: Utilised the DescriptionAttribute for values that use it.
        /// </summary>
        /// <param name="enumType">The System.Type of the enumeration.</param>
        /// <param name="value">A string containing the name or value to convert.</param>
        /// <param name="ignoreCase">Whether the operation is case-sensitive or not.</param>
        /// <returns></returns>
        public static bool TryParse<T>(string value, out T enumValue, bool ignoreCase = false) where T : struct, IConvertible
        {
            T? finalValue = null;
            enumValue = Enum.GetValues(typeof(T)).Cast<T>().First();

            try
            {
                finalValue = Parse<T>(value);
                if (finalValue.HasValue)
                    enumValue = finalValue.Value;

            }
            catch (Exception)
            {
                finalValue = null;
            }

            return finalValue.HasValue;
        }

        public static T Parse<T>(string value, bool ignoreCase = false) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            if (value.IsNullOrEmpty())
                throw new ArgumentException("A value must be provided");

            foreach (T val in Enum.GetValues(typeof(T)))
            {
                string comparison = GetDescription(val);

                if (comparison.Equals(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                    return val;
                comparison = val.ToString();
                if (comparison.Equals(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                    return val;
            }

            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivilant enumerated object.
        /// Note: Utilised the DescriptionAttribute for values that use it.
        /// </summary>
        /// <param name="enumType">The System.Type of the enumeration.</param>
        /// <param name="value">A string containing the name or value to convert.</param>
        /// <returns></returns>
        public static bool TryParse(Type enumType, string value, out dynamic enumValue)
        {
            object finalValue;
            enumValue = Enum.GetValues(enumType).Cast<object>().First();

            try
            {
                finalValue = Parse(enumType, value);
                if (finalValue != null)
                    enumValue = finalValue;

            }
            catch (Exception)
            {
                finalValue = null;
            }

            return finalValue != null;
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivilant enumerated object.
        /// A parameter specified whether the operation is case-sensitive.
        /// Note: Utilised the DescriptionAttribute for values that use it.
        /// </summary>
        /// <param name="enumType">The System.Type of the enumeration.</param>
        /// <param name="value">A string containing the name or value to convert.</param>
        /// <param name="ignoreCase">Whether the operation is case-sensitive or not.</param>
        /// <returns></returns>
        public static object Parse(Type enumType, string value, bool ignoreCase = false)
        {
            if (value.IsNullOrEmpty())
                return null;

            foreach (Enum val in Enum.GetValues(enumType))
            {
                string comparison = val.GetDescription();

                if (comparison.Equals(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                    return val;
                comparison = val.ToString();
                if (comparison.Equals(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                    return val;
            }

            return Enum.Parse(enumType, value, ignoreCase);
        }

        public static IEnumerable<Enum> GetFlags(this Enum value)
        {
            return GetFlags(value, Enum.GetValues(value.GetType()).Cast<Enum>().ToArray());
        }

        public static IEnumerable<Enum> GetIndividualFlags(this Enum value)
        {
            return GetFlags(value, GetFlagValues(value.GetType()).ToArray());
        }

        public static IEnumerable<T> GetIndividualFlags<T>(this T value) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            return value.GetFlags(GetFlagValues<T>().ToList());
        }

        private static IEnumerable<Enum> GetFlags(Enum value, Enum[] values)
        {
            ulong bits = Convert.ToUInt64(value);
            List<Enum> results = new List<Enum>();
            for (int i = values.Length - 1; i >= 0; i--)
            {
                ulong mask = Convert.ToUInt64(values[i]);
                if (i == 0 && mask == 0L)
                    break;
                if ((bits & mask) == mask)
                {
                    results.Add(values[i]);
                    bits -= mask;
                }
            }
            if (bits != 0L)
                return Enumerable.Empty<Enum>();
            if (Convert.ToUInt64(value) != 0L)
                return results.Reverse<Enum>();
            if (bits == Convert.ToUInt64(value) && values.Length > 0 && Convert.ToUInt64(values[0]) == 0L)
                return values.Take(1);
            return Enumerable.Empty<Enum>();
        }

        private static IEnumerable<T> GetFlags<T>(this T value, IList<T> values) where T : struct, IConvertible
        {
            ulong bits = Convert.ToUInt64(value);
            List<T> results = new List<T>();

            for (int i = values.Count() - 1; i >= 0; i--)
            {
                ulong mask = Convert.ToUInt64(values[i]);
                if (i == 0 && mask == 0L)
                    break;
                if ((bits & mask) == mask)
                {
                    results.Add(values[i]);
                    bits -= mask;
                }
            }
            if (bits != 0L)
                return Enumerable.Empty<T>();
            if (Convert.ToUInt64(value) != 0L)
                return results.Reverse<T>();
            if (bits == Convert.ToUInt64(value) && values.Count() > 0 && Convert.ToUInt64(values[0]) == 0L)
                return values.Take(1);
            return Enumerable.Empty<T>();
        }

        private static IEnumerable<Enum> GetFlagValues(Type enumType)
        {
            ulong flag = 0x1;
            foreach (var value in Enum.GetValues(enumType).Cast<Enum>())
            {
                ulong bits = Convert.ToUInt64(value);
                if (bits == 0L)
                    //yield return value;
                    continue; // skip the zero value
                while (flag < bits) flag <<= 1;
                if (flag == bits)
                    yield return value;
            }
        }

        private static IEnumerable<T> GetFlagValues<T>() where T : struct, IConvertible
        {
            var type = typeof(T);
            if (!type.IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            ulong flag = 0x1;
            foreach (var value in Enum.GetValues(type).Cast<T>())
            {
                ulong bits = Convert.ToUInt64(value);
                if (bits == 0L)
                    //yield return value;
                    continue; // skip the zero value
                while (flag < bits) flag <<= 1;
                if (flag == bits)
                    yield return value;
            }
        }
    }
}

