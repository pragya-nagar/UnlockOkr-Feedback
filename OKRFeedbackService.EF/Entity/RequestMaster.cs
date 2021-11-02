using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OKRFeedbackService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class RequestMaster
    {
        public RequestMaster()
        {
            OneToOneDetail = new HashSet<OneToOneDetail>();
        }

        public int RequestId { get; set; }
        public string RequestName { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<OneToOneDetail> OneToOneDetail { get; set; }
    }
}
