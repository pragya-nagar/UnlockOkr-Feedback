using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OKRFeedbackService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class RaisedTypeMaster
    {
        public RaisedTypeMaster()
        {
            FeedbackRequest = new HashSet<FeedbackRequest>();
        }

        public int RaisedTypeId { get; set; }
        public string Name { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<FeedbackRequest> FeedbackRequest { get; set; }
    }
}
