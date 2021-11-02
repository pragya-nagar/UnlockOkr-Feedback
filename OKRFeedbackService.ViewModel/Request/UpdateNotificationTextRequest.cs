
namespace OKRFeedbackService.ViewModel.Request
{
    public class UpdateNotificationTextRequest
    {
        public long NotificationsDetailsId { get; set; }
        public string Text { get; set; }
        public long NotificationTypeId { get; set; }
    }
}
