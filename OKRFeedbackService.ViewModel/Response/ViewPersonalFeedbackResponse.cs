using System;
using System.Collections.Generic;

namespace OKRFeedbackService.ViewModel.Response
{
    public class ViewPersonalFeedbackResponse
    {
        public int? CriteriaTypeId { get; set; }
        public string FeedbackCriteriaName { get; set; }
        public long EmployeeId { get; set; }
        public long FeedbackDetailId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SharedRemark { get; set; }
        public string ImagePath { get; set; }
        public DateTime Date { get; set; }

        public string AskByRequestRemark { get; set; }
        public string AskByFirstName { get; set; }
        public string AskByLastName { get; set; }
        public string AskByImagePath { get; set; }
        public DateTime? AskByCreatedOn { get; set; } = null;
        public List<CriteriaFeedbackMappingResponse> CriteriaFeedbackMappingResponses { get; set; }
        public List<FeedbackComment> FeedbackComments { get; set; }
       
    }

    
}
