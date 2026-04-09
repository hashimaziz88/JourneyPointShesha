using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>
    /// Engagement classification derived from a hire's composite engagement score.
    /// </summary>
    [ReferenceList("JourneyPoint", "EngagementClassifications")]
    public enum RefListEngagementClassification : long
    {
        /// <summary>Score >= 70 — hire is actively progressing through their journey.</summary>
        [Description("Engaged")]
        Engaged = 1,

        /// <summary>Score >= 40 and < 70 — hire may need facilitator attention.</summary>
        [Description("Needs Attention")]
        NeedsAttention = 2,

        /// <summary>Score < 40 — hire is at risk of disengagement; flag raised automatically.</summary>
        [Description("At Risk")]
        AtRisk = 3
    }
}
