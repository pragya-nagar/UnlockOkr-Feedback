using System;
using System.Collections.Generic;

namespace OKRFeedbackService.ViewModel.Response
{
    public class ProvidePersonalFeedbackResponse
    {
        public long FeedbackDetailId { get; set; }
        public string CriteriaName { get; set; }
        public int CriteriaTypeId { get; set; }
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SharedRemark { get; set; }
        public string ImagePath { get; set; }
        public string AskByFirstName { get; set; }
        public string AskByLastName { get; set; }
        public string AskByImagePath { get; set; }
        public string AskByRequestRemark { get; set; }
        public DateTime? AskByCreatedOn { get; set; } = null;
        public DateTime Date { get; set; }
        public List<FeedbackComment> FeedbackComments { get; set; }
        public List<CriteriaFeedbackMappingResponse> criteriaFeedbackMappingResponses { get; set; }
    }
}
