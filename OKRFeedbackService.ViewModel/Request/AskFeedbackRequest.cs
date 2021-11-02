
using System.Collections.Generic;

namespace OKRFeedbackService.ViewModel.Request
{
    public class AskFeedbackRequest
    {
        public int RaisedTypeId { get; set; } 
        public List<long> RaisedForId { get; set; }
        public long FeedbackById { get; set; }
        public int FeedbackOnTypeId { get; set; } 
        public long FeedbackOnId { get; set; } 
        public string RequestRemark { get; set; }
    }
}
