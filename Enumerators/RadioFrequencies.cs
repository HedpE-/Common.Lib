using System.ComponentModel;

namespace Common.Lib.Enumerators
{
    /// <summary>Enumeration of VF UK Radio Frequencies</summary>
    public enum RadioFrequencies
    {
        /// <summary/>
        [Description("G18")]
        GSM1800,
        /// <summary/>
        [Description("G9")]
        GSM900,
        /// <summary/>
        [Description("U9")]
        UMTS900,
        /// <summary/>
        [Description("U21")]
        UMTS2100,
        /// <summary/>
        [Description("L8")]
        LTE800,
        /// <summary/>
        [Description("L8 NB-IoT")]
        LTE800NBIOT,
        /// <summary/>
        [Description("L9")]
        LTE900,
        /// <summary/>
        [Description("L14")]
        LTE1400,
        /// <summary/>
        [Description("L18")]
        LTE1800,
        /// <summary/>
        [Description("L21")]
        LTE2100,
        /// <summary/>
        [Description("L23")]
        LTE2300,
        /// <summary/>
        [Description("L26")]
        LTE2600,
        /// <summary/>
        [Description("N7")]
        N700,
        /// <summary/>
        [Description("N34")]
        N3400,
        /// <summary/>
        [Description("N36")]
        N3600
    }
}
