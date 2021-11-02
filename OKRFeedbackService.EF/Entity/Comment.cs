using System;
using System.Diagnostics.CodeAnalysis;

namespace OKRFeedbackService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class Comment
    {
        public long CommentId { get; set; }
        public string Comments { get; set; }
        public long FeedbackDetailId { get; set; }
        public long ParentCommentId { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public long? UpdatedBy { get; set; } = null;
        public DateTime? UpdatedOn { get; set; } = null;
        public bool IsActive { get; set; } = true;

        public virtual FeedbackDetail FeedbackDetail { get; set; }
    }
}

