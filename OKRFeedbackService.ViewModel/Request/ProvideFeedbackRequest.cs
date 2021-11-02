using System.ComponentModel.DataAnnotations;

namespace OKRFeedbackService.ViewModel.Request
{
    public class ProvideFeedbackRequest
    {
        public long FeedbackDetailId { get; set; }
        public int RaisedTypeId { get; set; }
        public long RaisedForId { get; set; }
        [Required]
        public long FeedbackRequestId { get; set; }
        [Required]
        public int FeedbackOnTypeId { get; set; }
        [Required]
        public long FeedbackOnId { get; set; }
        [Required]
        public string SharedRemark { get; set; }
        public int Status { get; set; }
        public long TeamId { get; set; }
    }
}
