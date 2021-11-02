using System;

namespace OKRFeedbackService.ViewModel.Response
{
    public class NotificationsDetailsResponse
    {
        public long NotificationsDetailsId { get; set; }
        public long NotificationsBy { get; set; }
        public long NotificationsTo { get; set; }
        public string NotificationsMessage { get; set; }
        public int ApplicationMasterId { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
        public long NotificationTypeId { get; set; }
        public long MessageTypeId { get; set; }
        public string Url { get; set; }
        public DateTime CreatedOn { get; set; }
        public int NotificationOnTypeId { get; set; }
        public long NotificationOnId { get; set; }
    }
}
