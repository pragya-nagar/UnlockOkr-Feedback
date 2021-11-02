using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OKRFeedbackService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class FeedbackOnTypeMaster
    {
        public FeedbackOnTypeMaster()
        {
            FeedbackDetail = new HashSet<FeedbackDetail>();
            FeedbackRequest = new HashSet<FeedbackRequest>();
        }

        public int FeedbackOnTypeId { get; set; }
        public string Name { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<FeedbackDetail> FeedbackDetail { get; set; }
        public virtual ICollection<FeedbackRequest> FeedbackRequest { get; set; }
    }
}
