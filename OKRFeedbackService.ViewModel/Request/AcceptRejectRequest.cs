namespace OKRFeedbackService.ViewModel.Request
{
   public class AcceptRejectRequest
    {
        public long OneToOneDetailId { get; set; }
        public int Status { get; set; }
        public long NotificationsDetailId { get; set; }
    }
}
