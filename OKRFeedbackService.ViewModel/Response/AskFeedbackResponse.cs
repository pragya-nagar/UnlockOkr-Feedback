
namespace OKRFeedbackService.ViewModel.Response
{
    public class AskFeedbackResponse
    {
        public int FeedbackOnTypeId { get; set; }
        public long FeedbackOnId { get; set; }
        public long FeedbackRequestId { get; set; }
        public int Status { get; set; }     
        public string RequestRemark { get; set; }

       
    }
}
