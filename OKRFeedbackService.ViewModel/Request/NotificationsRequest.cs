using System.Collections.Generic;

namespace OKRFeedbackService.ViewModel.Request
{
    public class NotificationsRequest
    {
        public long By { get; set; }
        public List<long> To { get; set; }
        public string Text { get; set; }
        public int AppId { get; set; }
        public long NotificationType { get; set; }
        public int MessageType { get; set; }
        public string Url { get; set; } = "";
        public int NotificationOnTypeId { get; set; }
        public long NotificationOnId { get; set; }

    }
}
