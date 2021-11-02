
namespace OKRFeedbackService.ViewModel.Request
{
    public class CommentRequest
    {
        public string Comments { get; set; }
        public long FeedbackDetailId { get; set; }
        public long ParentCommentId { get; set; }
    }
}
