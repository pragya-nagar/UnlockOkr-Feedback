using System;
using System.Collections.Generic;

namespace OKRFeedbackService.ViewModel.Response
{
    public class PersonalFeedbackResponse
    {
        public long FeedbackRequestId { get; set; }
        public int FeedbackOnTypeId { get; set; }
        public long FeedbackOnId { get; set; }
        public string AskByRequestRemark { get; set; }
        public long FeedbackById { get; set; }
        public string AskByFirstName { get; set; }
        public string AskByLastName { get; set; }
        public string AskByImagePath { get; set; }
        public DateTime AskByCreatedOn { get; set; }
        public int Status { get; set; }
        public List<FeedbackProvideDetails> feedbackProvideDetails { get; set; }
    }
}
