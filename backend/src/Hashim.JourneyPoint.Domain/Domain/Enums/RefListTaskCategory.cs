using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>
    /// Functional category of an onboarding task, used for grouping and reporting.
    /// </summary>
    [ReferenceList("JourneyPoint", "TaskCategories")]
    public enum RefListTaskCategory : long
    {
        /// <summary>Administrative and paperwork tasks (contracts, ID documents, etc.).</summary>
        [Description("Administrative")]
        Administrative = 1,

        /// <summary>Technical setup tasks (accounts, tools, systems access).</summary>
        [Description("Technical")]
        Technical = 2,

        /// <summary>Cultural integration tasks (team introductions, values, norms).</summary>
        [Description("Cultural")]
        Cultural = 3,

        /// <summary>Compliance and regulatory tasks (policies, certifications).</summary>
        [Description("Compliance")]
        Compliance = 4,

        /// <summary>Scheduled meetings or check-ins.</summary>
        [Description("Meeting")]
        Meeting = 5,

        /// <summary>Training and learning activities.</summary>
        [Description("Training")]
        Training = 6
    }
}
