using System;
using System.Diagnostics.CodeAnalysis;

namespace OKRFeedbackService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class CriteriaFeedbackMapping
    {
        public long CriteriaFeedbackMappingId { get; set; }
        public long FeedbackDetailId { get; set; }
        public long CriteriaMasterId { get; set; }
        public decimal Score { get; set; }
        public DateTime? CreatedOn { get; set; } = DateTime.UtcNow;
        public long? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public long? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual CriteriaMaster CriteriaMaster { get; set; }
        public virtual FeedbackDetail FeedbackDetail { get; set; }
    }
}
