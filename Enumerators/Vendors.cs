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
    /// <summary>Enumeration of VF UK Vendors</summary>
    public enum Vendors
    {
        /// <summary/>
        [Description("ERICSSON")]
        Ericsson = 1,
        /// <summary/>
        [Description("ALU")]
        ALU = 2,
        /// <summary/>
        [Description("HUAWEI")]
        Huawei = 4,
        /// <summary/>
        [Description("NSN")]
        NSN = 8,
        /// <summary/>
        [Description("Unknown")]
        Unknown = 16
    };
}
