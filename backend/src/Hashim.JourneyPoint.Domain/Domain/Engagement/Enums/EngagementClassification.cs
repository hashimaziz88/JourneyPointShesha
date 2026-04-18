using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>Engagement classification derived from a hire's composite engagement score.</summary>
    [ReferenceList("JourneyPoint", "EngagementClassifications")]
    public enum EngagementClassification : long
    {
        /// <summary>Hire is actively progressing — completion rate and recency are healthy.</summary>
        [Description("Healthy")]
        Healthy = 1,

        /// <summary>Hire is showing signs of disengagement — facilitator should check in.</summary>
        [Description("Needs Attention")]
        NeedsAttention = 2,

        /// <summary>Hire is significantly disengaged — immediate facilitator intervention required.</summary>
        [Description("At Risk")]
        AtRisk = 3
    }
}
