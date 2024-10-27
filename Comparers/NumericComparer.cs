// (c) Vasian Cepa 2005
// Version 2

using System.Collections; // required for NumericComparer : IComparer only

namespace Common.Lib.Comparers
{
    public class NumericComparer : IComparer
    {
        public NumericComparer()
        { }

        public int Compare(object x, object y)
        {
            if (x is string && y is string)
                return StringLogicalComparer.Compare(x.ToString(), y.ToString());

            return -1;
        }
    }
}