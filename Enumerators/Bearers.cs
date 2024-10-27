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
    /// <summary>Enumeration of bearers</summary>
    public enum Bearers
    {
        /// <summary/>
        [Description("Unknown")]
        Unknown = 0,
        /// <summary/>
        [Description("2G")]
        GSM = 1,
        /// <summary/>
        [Description("3G")]
        UMTS = 2,
        /// <summary/>
        [Description("4G")]
        LTE = 4,
        /// <summary/>
        [Description("5G")]
        FIVEG = 8,
        /// <summary/>
        [Description("All")]
        All = GSM | UMTS | LTE | FIVEG
    };
}
