using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>Describes how an AtRiskFlag was resolved.</summary>
    [ReferenceList("JourneyPoint", "AtRiskResolutionTypes")]
    public enum AtRiskResolutionType : long
    {
        /// <summary>Facilitator intervened directly — e.g. meeting, support conversation, plan adjustment.</summary>
        [Description("Manual Facilitator Resolution")]
        ManualFacilitatorResolution = 1,

        /// <summary>Hire's engagement score recovered to Healthy naturally without direct intervention.</summary>
        [Description("Automatic Healthy Recovery")]
        AutomaticHealthyRecovery = 2,

        /// <summary>Hire exited the organisation — flag closed as part of the exit process.</summary>
        [Description("Hire Exited")]
        HireExited = 3
    }
}
