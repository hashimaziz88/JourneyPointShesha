using Hashim.JourneyPoint.Domain.Domain.Hires;
using Shesha.DynamicEntities.Dtos;
using System;

namespace Hashim.JourneyPoint.Common.Services.Hires.Dtos
{
    /// <summary>
    /// Returned by HireAppService.Create. Contains the hire record and the
    /// one-time temporary password so the Facilitator can share it with the hire.
    /// </summary>
    public class HireCreatedDto
    {
        /// <summary>Full hire record.</summary>
        public DynamicDto<Hire, Guid> Hire { get; set; }

        /// <summary>Temporary password for the hire's platform account.</summary>
        public string TempPassword { get; set; }
    }
}
