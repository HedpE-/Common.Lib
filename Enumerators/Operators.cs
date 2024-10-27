/*
 * Created by SharpDevelop.
 * User: goncarj3
 * Date: 21-07-2016
 * Time: 19:47
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.ComponentModel;

namespace Common.Lib.Enumerators
{
    /// <summary>Enumeration of VF UK ANOC Operators</summary>
    [Flags]
    public enum Operators
    {
        /// <summary/>
        [Description("VF")]
        Vodafone = 1,
        /// <summary/>
        [Description("TF")]
        Telefonica = 2,
        /// <summary/>
        Unknown = 4
    }
}
