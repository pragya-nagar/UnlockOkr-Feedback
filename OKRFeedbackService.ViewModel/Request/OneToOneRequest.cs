
using System.Collections.Generic;

namespace OKRFeedbackService.ViewModel.Request
{
    public class OneToOneRequest
    {
        public int RequestType { get; set; }
        public long RequestId { get; set; }
        public List<long> RequestedTo { get; set; }
        public long RequestedFrom { get; set; }
        public string OnetoOneRemark { get; set; }
    }
}
