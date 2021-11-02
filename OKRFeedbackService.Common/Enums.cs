
namespace OKRFeedbackService.Common
{
    public enum MessageType
    {
        /// <summary>
        /// The information
        /// </summary>
        Info,
        /// <summary>
        /// The success
        /// </summary>
        Success,
        /// <summary>
        /// The alert
        /// </summary>
        Alert,
        /// <summary>
        /// The warning
        /// </summary>
        Warning,
        /// <summary>
        /// The error
        /// </summary>
        Error,
    }

    public enum NotificationType
    {
        ProvideFeedback = 1,
        AskFeedback = 2,
        Comments = 3,
        RequestOnetoOne = 4

    }

    public enum MessageTypeForNotifications
    {
        NotificationsMessages = 1,
        AlertMessages = 2
    }

    public enum FeedbackType
    {
        AskFeedback = 1,
        ProvideFeedback = 2,
        OneOnOne = 3,
        Comment = 4
    }

    public enum TemplateCodes
    {
        AF = 1,
        PF = 2,
        FRC = 3,
        RF = 4,
        PAF = 5,
        PRF = 6,
        PPF = 7,
        PFRC = 8,
        PGF = 9
    }
    public enum CancelRequest
    {
        CR = 4
    }
}
