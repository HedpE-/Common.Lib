using Common.Lib.Enumerators;
using Common.Lib.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Common.Lib.Converters
{
    public class NumberToVisibilityConverter : IValueConverter
    {
        public NumericComparisonMethods NumericComparerMethod { get; set; } = NumericComparisonMethods.GreaterThan;

        public double VisibleValue { get; set; } = 0;

        public object TrueValue { get; set; }
        public object FalseValue { get; set; }

        public bool InvertResult { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue)
                value = null;

            double val = 0;
            double.TryParse((value ?? "").ToString(), out val);

            bool result = false;

            switch (NumericComparerMethod)
            {
                case NumericComparisonMethods.Equal:
                    result = val == VisibleValue;
                    break;
                case NumericComparisonMethods.NotEqual:
                    result = val != VisibleValue;
                    break;
                case NumericComparisonMethods.LessThan:
                    result = val < VisibleValue;
                    break;
                case NumericComparisonMethods.LessThanOrEqual:
                    result = val <= VisibleValue;
                    break;
                case NumericComparisonMethods.GreaterThan:
                    result = val > VisibleValue;
                    break;
                case NumericComparisonMethods.GreaterThanOrEqual:
                    result = val >= VisibleValue;
                    break;
            }

            result = InvertResult ? !result : result;

            return result ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class ObjectEqualityConverter : IValueConverter
    {
        //public object CompareWith { get; set; }

        public object TrueValue { get; set; } = true;
        public object FalseValue { get; set; } = false;

        public bool InvertResult { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value == null)
                return Binding.DoNothing;
            if (parameter == null)
                return Binding.DoNothing;

            var result = value.Equals(parameter);

            result = InvertResult ? !result : result;

            return result ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return Binding.DoNothing;
        }
    }

    public class EnumToByteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (!(value is Enum))
                return Binding.DoNothing;

            if (typeof(byte).IsAssignableFrom(Enum.GetUnderlyingType(value.GetType())))
                return (byte)value;

            if (typeof(int).IsAssignableFrom(Enum.GetUnderlyingType(value.GetType())))
                return (byte)(int)value;

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return Binding.DoNothing;
        }
    }

    public class DateStringToDateTimeConverter : IValueConverter
    {
        public DateTimeFormats Format { get; set; } = DateTimeFormats.Other;
        public bool DateTimeMaxAsNull { get; set; } = true;
        public bool DateTimeMinAsNull { get; set; } = true;

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value == null)
                return Binding.DoNothing;

            long val = 0;

            if (Format == DateTimeFormats.Other)
            {
                if (!(value is string))
                    return Binding.DoNothing;
                if (!(parameter is string))
                    return Binding.DoNothing;
            }
            else
            {
                if (!long.TryParse(value.ToString(), out val))
                    return Binding.DoNothing;
            }

            DateTime? result = null;

            switch (Format)
            {
                case DateTimeFormats.JavaTimeStamp:
                    result = val.JavaTimeStampToDateTime();
                    break;
                case DateTimeFormats.UnixTimeStamp:
                    result = val.UnixTimeStampToDateTime();
                    break;
                case DateTimeFormats.Other:
                    result = DateTime.ParseExact(value.ToString(), parameter.ToString(), null);
                    break;
            }

            if (DateTimeMaxAsNull && (result == DateTime.MaxValue || result == DateTime.MaxValue.Date))
                return null;
            if (DateTimeMinAsNull && (result == DateTime.MinValue || result == DateTime.MinValue.Date))
                return null;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return Binding.DoNothing;
        }
    }

    public class MultiValueEqualityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values?.All(o => o?.Equals(values[0]) == true) == true || values?.All(o => o == null) == true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return new SolidColorBrush(color);
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                return brush.Color;
            }
            return default(Color);
        }
    }

    public class SolidColorBrushToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is SolidColorBrush)
                return ((SolidColorBrush)value).Color;

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return Binding.DoNothing;
        }
    }

    public class HasEnumFlagConverter : IValueConverter
    {
        public object TrueValue { get; set; } = true;
        public object FalseValue { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            var result = false;
            if (value is Enum && parameter is Enum)
            {
                if (value.GetType() == parameter.GetType())
                {
                    if (value.GetType().IsDefined(typeof(FlagsAttribute), inherit: false)) // check if the value parameter is of an Enum type with [Flags] attribute
                        result = ((Enum)parameter).HasFlag((Enum)value);
                    else
                        result = ((Enum)value).Equals(parameter);
                }
            }

            return result ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return Binding.DoNothing;
        }
    }

    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is Enum)
                return ((Enum)value).GetDescription();

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return Binding.DoNothing;
        }
    }

    public class CredentialsEncryptedPasswordConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
                return value.ToString().Unprotect();

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class BooleanConverter : IValueConverter
    {
        public object TrueValue { get; set; } = true;
        public object FalseValue { get; set; } = false;

        public bool Invert { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = value is bool ? (bool)value : false;

            return result ? (Invert ? FalseValue : TrueValue) : (Invert ? TrueValue : FalseValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class StringComparerConverter : IValueConverter
    {
        public StringComparisonMethods ComparisonMethod { get; set; } = StringComparisonMethods.Equals;

        public bool IgnoreCase { get; set; } = false;

        public object TrueValue { get; set; } = true;
        public object FalseValue { get; set; } = false;

        public bool Invert { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;
            string checkValue;
            if (parameter is Enum)
                checkValue = EnumExtensions.GetDescription((Enum)parameter);
            else
                checkValue = parameter.ToString();

            if (value is string)
            {
                var str = value.ToString();

                switch (ComparisonMethod)
                {
                    case StringComparisonMethods.Equals:
                        result = str.Equals(checkValue, (IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
                        break;
                    case StringComparisonMethods.Contains:
                        result = str.Contains(checkValue, (IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
                        break;
                    case StringComparisonMethods.StartsWith:
                        result = str.StartsWith(checkValue, (IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
                        break;
                    case StringComparisonMethods.EndsWith:
                        result = str.EndsWith(checkValue, (IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
                        break;
                }
            }
            return result ? (Invert ? FalseValue : TrueValue) : (Invert ? TrueValue : FalseValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class StringComparerMultiConverter : IMultiValueConverter
    {
        public StringComparisonMethods ComparisonMethod { get; set; } = StringComparisonMethods.Equals;
        public StringComparisonMatchMethods ComparisonMatchMethod { get; set; } = StringComparisonMatchMethods.All;

        public bool MatchWholeWord { get; set; } = false;

        public bool IgnoreCase { get; set; } = false;

        public object TrueValue { get; set; } = true;
        public object FalseValue { get; set; } = false;

        public bool Invert { get; set; } = false;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
                return Binding.DoNothing;
            if (values.Length < 2)
                return Binding.DoNothing;

            string str;
            if (values[0] is Enum)
                str = EnumExtensions.GetDescription((Enum)values[0]);
            else
                str = values[0] == null ? "" : values[0].ToString();

            List<string> comparingValues = new List<string>();
            foreach (var value in values.Skip(1))
            {
                string finalString;
                if (value is Enum)
                    finalString = EnumExtensions.GetDescription((Enum)value);
                else
                    finalString = value.ToString();

                comparingValues.Add(finalString);
            }

            List<bool> results = new List<bool>();

            foreach (var value in comparingValues)
            {
                bool result = false;
                switch (ComparisonMethod)
                {
                    case StringComparisonMethods.Equals:
                        result = str.Equals(value, (IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
                        break;
                    case StringComparisonMethods.Contains:
                        result = str.Contains(value, !IgnoreCase, true);
                        break;
                    case StringComparisonMethods.StartsWith:
                        result = str.StartsWith(value, (IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
                        break;
                    case StringComparisonMethods.EndsWith:
                        result = str.EndsWith(value, (IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
                        break;
                }
                results.Add(result);
            }

            bool finalResult;
            if (ComparisonMatchMethod == StringComparisonMatchMethods.All)
                finalResult = results.All(r => r == true);
            else
                finalResult = results.Any(r => r == true);

            return finalResult ? (Invert ? FalseValue : TrueValue) : (Invert ? TrueValue : FalseValue);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class CaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string && parameter is StringCasing)
            {
                var str = value as string;
                var casing = (StringCasing)parameter;
                if (str != null)
                {
                    switch (casing)
                    {
                        case StringCasing.Lower:
                            return str.ToLower();
                        case StringCasing.Upper:
                            return str.ToUpper();
                        case StringCasing.CapitalizeFirstWord:
                            var temp = str.ToLower().ToCharArray();
                            temp[0] = char.ToUpper(temp[0]);
                            return string.Join("", temp);
                        case StringCasing.CapitalizeAllWords:
                            return str.CapitalizeWords();
                        case StringCasing.Normal:
                        default:
                            return str;
                    }
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class MultiBooleanConverter : IMultiValueConverter
    {
        public string BooleanOperation { get; set; } = "AND"; // "AND", "OR"

        public object TrueValue { get; set; } = true;
        public object FalseValue { get; set; } = false;

        public bool Invert { get; set; } = false;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return Binding.DoNothing;

            if (values[0] is bool)
            {
                bool result = (bool)values[0];
                for (int c = 1; c < values.Length; c++)
                {
                    if (values[c] is bool)
                    {
                        switch (BooleanOperation.ToUpper())
                        {
                            case "AND":
                                result = result && (bool)values[c];
                                break;
                            case "OR":
                                result = result || (bool)values[c];
                                break;
                        }
                    }
                }
                return result ? (Invert ? FalseValue : TrueValue) : (Invert ? TrueValue : FalseValue);
            }

            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class NumericComparerConverter : IValueConverter
    {
        public NumericComparisonMethods NumericComparerMethod { get; set; } = NumericComparisonMethods.GreaterThan;

        public object TrueValue { get; set; } = true;
        public object FalseValue { get; set; } = false;

        public bool Invert { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue)
                value = null;

            if (parameter == null || parameter == DependencyProperty.UnsetValue)
                parameter = 0.0;

            if (parameter is string)
            {
                if (parameter.ToString().IsAllDigits())
                    parameter = double.Parse(parameter.ToString());
                else
                    parameter = 0.0;
            }

            double val = 0;
            double.TryParse((value ?? "").ToString(), out val);

            bool result = false;

            if (parameter is double)
            {
                var valueToCompare = (double)parameter;
                switch (NumericComparerMethod)
                {
                    case NumericComparisonMethods.Equal:
                        result = val == valueToCompare;
                        break;
                    case NumericComparisonMethods.NotEqual:
                        result = val != valueToCompare;
                        break;
                    case NumericComparisonMethods.LessThan:
                        result = val < valueToCompare;
                        break;
                    case NumericComparisonMethods.LessThanOrEqual:
                        result = val <= valueToCompare;
                        break;
                    case NumericComparisonMethods.GreaterThan:
                        result = val > valueToCompare;
                        break;
                    case NumericComparisonMethods.GreaterThanOrEqual:
                        result = val >= valueToCompare;
                        break;
                }
            }

            return result ? (Invert ? FalseValue : TrueValue) : (Invert ? TrueValue : FalseValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class NumericComparerToBooleanConverter : IMultiValueConverter
    {
        //numeric value
        //numeric value
        //
        //parameter "GreaterThan", "LessThan", "GreaterOrEqualThan"
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;

            if (values.Length == 2)
            {
                if (values[0] is int)
                {
                    int baseValue = (int)values[0];
                    int compareValue = values[1] is int ? (int)values[1] : 0;

                    result = baseValue > compareValue;
                }
            }

            return parameter is string ? (parameter.ToString() == "Inverse" ? !result : result) : result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class IsGreaterThanConverter : IValueConverter
    //{
    //    //int
    //    //int -> greater than
    //    //
    //    //parameter "Inverse"
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo language)
    //    {
    //        bool result = false;

    //        if (value.IsNumber() && parameter.IsNumber())
    //        {
    //            var baseValue = (int)value;
    //            var compareValue = (int)parameter;

    //            result = baseValue > compareValue;
    //        }

    //        return result;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
    //    {
    //        return Binding.DoNothing;
    //    }
    //}

    //public class IsLessThanToBooleanConverter : IValueConverter
    //{
    //    //int
    //    //int -> greater than
    //    //
    //    //parameter "Inverse"
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo language)
    //    {
    //        bool result = false;

    //        if (value.IsNumber() && parameter.IsNumber())
    //        {
    //            var baseValue = (int)value;
    //            var compareValue = (int)parameter;

    //            result = baseValue < compareValue;
    //        }

    //        return result;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
    //    {
    //        return Binding.DoNothing;
    //    }
    //}

    public class IntToVisibilityConverter : IMultiValueConverter
    {
        //int
        //int -> greater than
        //
        //parameter "Inverse"
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var intToBoolConv = new NumericComparerToBooleanConverter();
            var boolResult = (bool)intToBoolConv.Convert(values, targetType, parameter, culture);

            Visibility result = boolResult ? Visibility.Visible : Visibility.Collapsed;

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullObjectOrStringEmptyConverter : IValueConverter
    {
        public object TrueValue { get; set; } = true;
        public object FalseValue { get; set; } = false;

        public bool EmptyIEnumerableAsFalse { get; set; }

        public bool Invert { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value == DependencyProperty.UnsetValue)
                value = null;
            if (value is string)
            {
                if (value.ToString() == string.Empty)
                    value = null;
            }
            if (value.IsNumber())
            {
                if (System.Convert.ToDouble(value) == 0)
                    value = null;
            }
            if (EmptyIEnumerableAsFalse && value is Enumerable)
            {
                if (!((IEnumerable<object>)value).Any())
                    value = null;
            }

            return value == null ? (Invert ? FalseValue : TrueValue) : (Invert ? TrueValue : FalseValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return Binding.DoNothing;
        }
    }

    public class TypeEqualityConverter : IValueConverter
    {
        public object TrueValue { get; set; } = true;
        public object FalseValue { get; set; } = false;

        public bool NullAsTrue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (!(parameter is System.Reflection.TypeInfo parameterType))
                return Binding.DoNothing;

            if (value == DependencyProperty.UnsetValue)
                value = null;

            if(value != null)
            {
                var valueType = value.GetType();

                return valueType == parameterType ? TrueValue : FalseValue;
            }

            return NullAsTrue ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return Binding.DoNothing;
        }
    }

    public class VisibilityConverter : IValueConverter
    {
        public object VisibleValue { get; set; }
        public object NotVisibleValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is Visibility)
            {
                Visibility val = (Visibility)value;


                return val == Visibility.Visible ? VisibleValue : NotVisibleValue;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return Binding.DoNothing;
        }
    }

    public class ListViewItemIndexToIntConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            int result = -1;

            if (value is ListViewItem)
            {
                ListViewItem item = (ListViewItem)value;
                ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
                result = listView.ItemContainerGenerator.IndexFromContainer(item) + 1;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class BooleanToEnabledDisabledTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                value = false;
            if (value == DependencyProperty.UnsetValue)
                value = false;

            return (bool)value ? "Enabled" : "Disabled";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class EnabledDisabledTextToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                value = "Disabled";
            if (value == DependencyProperty.UnsetValue)
                value = "Disabled";

            return (string)value == "Enabled" ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class DateTimeToWeekConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime)
                return ((DateTime)value).Iso8601WeekOfYear();

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BrushToHexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            string lowerHexString(int i) => i.ToString("X2").ToLower();
            var brush = (SolidColorBrush)value;
            var hex = lowerHexString(brush.Color.R) +
                      lowerHexString(brush.Color.G) +
                      lowerHexString(brush.Color.B);
            return "#" + hex;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntegerToHexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !int.TryParse(value.ToString(), out int result))
                return Binding.DoNothing;

            return "#" + result.ToString("X2");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
