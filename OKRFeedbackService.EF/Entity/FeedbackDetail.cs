using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OKRFeedbackService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class FeedbackDetail
    {
        public FeedbackDetail()
        {
            Comment = new HashSet<Comment>();
            CriteriaFeedbackMapping = new HashSet<CriteriaFeedbackMapping>();
        }

      

        public long FeedbackDetailId { get; set; }
        public long FeedbackRequestId { get; set; }
        public int FeedbackOnTypeId { get; set; }
        public long FeedbackOnId { get; set; }
        public string SharedRemark { get; set; }
        public bool? IsOneToOneRequested { get; set; } = false;
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public long? UpdatedBy { get; set; } = null;
        public DateTime? UpdatedOn { get; set; } = null;
        public bool IsActive { get; set; } = true;

        public int? CriteriaTypeId { get; set; } = 0;
        public virtual FeedbackOnTypeMaster FeedbackOnType { get; set; }
        public virtual FeedbackRequest FeedbackRequest { get; set; }
        public virtual ICollection<Comment> Comment { get; set; }
        public virtual ICollection<CriteriaFeedbackMapping> CriteriaFeedbackMapping { get;  set; }
    }
}
