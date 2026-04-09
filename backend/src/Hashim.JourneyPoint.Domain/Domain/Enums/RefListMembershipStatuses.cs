using Shesha.Domain.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>
    /// Statuses for a Members Membership
    /// </summary>
    [ReferenceList("JourneyPoint", "MembershipStatuses")]
    public enum RefListMembershipStatuses : long
    {
        /// <summary>
        /// Membership status is still being processed
        /// </summary>
        [Description("In Progress")]
        InProgress = 1,
        /// <summary>
        /// Membership status is active
        /// </summary>
        [Description("Active")]
        Active = 2,
        /// <summary>
        /// Membership status is cancelled
        /// </summary>
        [Description("Cancelled")]
        Cancelled = 3
    }
}