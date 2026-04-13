namespace Hashim.JourneyPoint.Domain
{
    /// <summary>
    /// Defines the static role names used across the JourneyPoint platform.
    /// These roles are seeded by migration M20260413120000 for all active tenants.
    /// </summary>
    public static class JourneyPointRoles
    {
        /// <summary>A new hire undergoing their onboarding journey.</summary>
        public const string Enrolee = "Enrolee";

        /// <summary>HR staff responsible for creating hires, managing plans, and reviewing the pipeline.</summary>
        public const string Facilitator = "Facilitator";

        /// <summary>The hire's line manager — responsible for manager-assigned journey tasks.</summary>
        public const string Manager = "Manager";

        /// <summary>All three roles, for use in seed loops.</summary>
        public static readonly string[] All = { Enrolee, Facilitator, Manager };
    }
}
