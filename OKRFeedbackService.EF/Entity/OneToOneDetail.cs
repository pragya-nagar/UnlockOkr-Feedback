using System;
using System.Diagnostics.CodeAnalysis;

namespace OKRFeedbackService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class OneToOneDetail
    {
        public long OneToOneDetailId { get; set; }
        public int RequestType { get; set; }
        public long RequestId { get; set; }
        public long RequestedTo { get; set; }
        public long RequestedFrom { get; set; }
        public string OneToOneRemark { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public long? UpdatedBy { get; set; } = null;
        public DateTime? UpdatedOn { get; set; } = null;
        public bool? IsActive { get; set; } = true;

        public int Status { get; set; } = 2;
        public virtual RequestMaster RequestTypeNavigation { get; set; }
    }
}
