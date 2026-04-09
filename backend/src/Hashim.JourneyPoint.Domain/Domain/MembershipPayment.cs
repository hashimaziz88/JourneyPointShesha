using Abp.Domain.Entities.Auditing;
using Shesha.Domain.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Domain.Domain
{
    /// <summary>
    /// A member's membership payment
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.MembershipPayment")]
    public class MembershipPayment : FullAuditedEntity<Guid>
    {
        /// <summary>
        /// The unique member
        /// </summary>
        public virtual Member Member { get; set; }
        /// <summary>
        /// The payment amount
        /// </summary>
        public virtual double Amount { get; set; }
        /// <summary>
        /// The date when the payment was made
        /// </summary>
        public virtual DateTime? PaymentDate { get; set; }
    }
}
