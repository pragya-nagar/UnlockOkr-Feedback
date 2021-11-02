using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OKRFeedbackService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class StatusMaster
    {
        public StatusMaster()
        {
            FeedbackRequest = new HashSet<FeedbackRequest>();
        }

        public int StatusId { get; set; }
        public string Name { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<FeedbackRequest> FeedbackRequest { get; set; }
    }
}
