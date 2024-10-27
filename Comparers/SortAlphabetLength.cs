/*
 * Created by SharpDevelop.
 * User: GONCARJ3
 * Date: 06/01/2017
 * Time: 20:48
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.Collections;

namespace Common.Lib.Comparers
{
    public class SortAlphabetLength : IComparer
    {
        public int Compare(object x, object y)
        {
            if (x.ToString().Length == y.ToString().Length)
                return string.Compare(x.ToString(), y.ToString());
            if (x.ToString().Length > y.ToString().Length)
                return 1;
            return -1;
        }
    }
}