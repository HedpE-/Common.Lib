using Common.Lib.TypeConverters;
using System.ComponentModel;
using System.Globalization;

namespace Common.Lib.Models
{
    [TypeConverter(typeof(PercentageConverter))]
    public struct Percentage
    {
        public double Value;

        public Percentage(double value)
        {
            Value = value;
        }

        public Percentage(string value)
        {
            var pct = (Percentage)TypeDescriptor.GetConverter(typeof(Percentage)).ConvertFromString(value);
            Value = pct.Value;
        }

        public override string ToString()
        {
            return ToString(CultureInfo.InvariantCulture);
        }

        public string ToString(CultureInfo Culture)
        {
            return TypeDescriptor.GetConverter(GetType()).ConvertToString(null, Culture, this);
        }
    }
}
