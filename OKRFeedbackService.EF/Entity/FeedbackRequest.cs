using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OKRFeedbackService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class FeedbackRequest
    {
        public FeedbackRequest()
        {
            FeedbackDetail = new HashSet<FeedbackDetail>();
        }

        public long FeedbackRequestId { get; set; }
        public int RaisedTypeId { get; set; }
        public long RaisedForId { get; set; }
        public long FeedbackById { get; set; }
        public int FeedbackOnTypeId { get; set; }
        public long FeedbackOnId { get; set; }
        public string RequestRemark { get; set; }
        public int? FeedbackRequestType { get; set; } = 1;
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public long? UpdatedBy { get; set; } = null;
        public DateTime? UpdatedOn { get; set; } = null;
        public int Status { get; set; } = 1;
        public bool IsActive { get; set; } = true;

        public virtual FeedbackOnTypeMaster FeedbackOnType { get; set; }
        public virtual RaisedTypeMaster RaisedType { get; set; }
        public virtual StatusMaster StatusNavigation { get; set; }
        public virtual ICollection<FeedbackDetail> FeedbackDetail { get; set; }
    }
}
