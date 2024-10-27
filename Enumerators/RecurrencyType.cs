/*
 * Created by SharpDevelop.
 * User: goncarj3
 * Date: 21-07-2016
 * Time: 19:47
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.ComponentModel;

namespace Common.Lib.Enumerators
{
    public enum RecurrencyType
    {
        [Description("Hours")]
        Hours = 1,
        [Description("Minutes")]
        Minutes = 2,
        [Description("StartUp")]
        StartUp = 4
    }
}
