
using System.Collections.Generic;

namespace OKRFeedbackService.ViewModel.Request
{
    public class AskPersonalFeedbackRequest
    {
        public int RaisedTypeId { get; set; }
        public List<long> RaisedForId { get; set; }
        public long FeedbackById { get; set; }
        public int FeedbackOnTypeId { get; set; }
        public string RequestRemark { get; set; }
    }
}
