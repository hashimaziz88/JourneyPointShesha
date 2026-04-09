using Hashim.JourneyPoint.Domain.Domain.Enums;
using Shesha.Domain;
using Shesha.Domain.Attributes;
using System;

namespace Hashim.JourneyPoint.Domain.Domain
{
    /// <summary>
    /// A person within the application that is a Member
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.Member")]
    public class Member : Person
    {
        /// <summary>
        /// The membership number for the Member
        /// </summary>
        public virtual string MembershipNumber { get; set; }
        /// <summary>
        /// The date when the Members membership started
        /// </summary>
        public virtual DateTime? MembershipStartDate { get; set; }
        /// <summary>
        /// The date when the Members membership ended
        /// </summary>
        public virtual DateTime? MembershipEndDate { get; set; }
        /// <summary>
        /// Identification document for the Member
        /// </summary>
        public virtual StoredFile IdDocument { get; set; }
        /// <summary>
        /// The status of the membership
        /// </summary>
        [ReferenceList("JourneyPoint", "MembershipStatuses")]
        public virtual RefListMembershipStatuses? MembershipStatus { get; set; }
    }
}
