
namespace OKRFeedbackService.ViewModel.Response
{
    public class MostFeedbackReponse
    {
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Shared { get; set; }
        public int OneOnOneRequested { get; set; }
        public int Requested { get; set; }   
    }

    public class Data
    {
        public long EmployeeId { get; set; }
        public string FeedbackType { get; set; }
        public int CountFeedback { get; set; }
    }
}
