
namespace OKRFeedbackService.ViewModel.Response
{
    public class AskPersonalFeedbackResponse
    {
        public long FeedbackRequestId { get; set; }
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Designation { get; set; }
        public int Status { get; set; }
        public string ImagePath { get; set; }
        public string Date { get; set; }
        public string RequestRemark { get; set; }
    }
}
