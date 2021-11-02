using System.Collections.Generic;

namespace OKRFeedbackService.ViewModel.Response
{
    public class OkrDetailResponse
    {
        public long OkrId { get; set; }
        public string OkrDesc { get; set; }
        public List<KeyDetails> KeyDetails { get; set; }
    }
}
