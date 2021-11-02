using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OKRFeedbackService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class CriteriaMaster
    {
        public CriteriaMaster()
        {
            CriteriaFeedbackMapping = new HashSet<CriteriaFeedbackMapping>();
        }

        public long CriteriaMasterId { get; set; }
        public int CriteriaTypeId { get; set; }
        public string CriteriaName { get; set; }
        public long? OrganisationId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public long? UpdatedBy { get; set; }
        public bool IsActive { get; set; }

        public virtual CriteriaTypeMaster CriteriaType { get; set; }
        public virtual ICollection<CriteriaFeedbackMapping> CriteriaFeedbackMapping { get; set; }
    }
}
