using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OKRFeedbackService.ViewModel.Request
{
    public class PersonalFeedbackRequest
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

        public int CriteriaTypeId { get; set; }
        public List<CriteriaFeedbackMappingRequest> criteriaFeedbackMappingRequests { get; set; }
    }
}
