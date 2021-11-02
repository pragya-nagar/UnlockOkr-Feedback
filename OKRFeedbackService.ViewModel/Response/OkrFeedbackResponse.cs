using System;
using System.Collections.Generic;

namespace OKRFeedbackService.ViewModel.Response
{
    public class OkrFeedbackResponse
    {
        public string ObjectiveDescription { get; set; }
        public string ObjectiveName { get; set; }
        public List<string> KeyDetails { get; set; }
        public int FeedbackOnTypeId { get; set; }
        public long FeedbackOnId { get; set; }
        public List<FeedbackResponse> FeedbackResponses { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; }
    }

    public class FeedbackResponse
    {
        public long FeedbackDetailId { get; set; }
        public string FeedbackDetail { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ImagePath { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long EmployeeId { get; set; }     
        public string AskByRequestRemark { get; set; }
        public string AskByFirstName { get; set; }
        public string AskByLastName { get; set; }
        public string AskByImagePath { get; set; }
        public DateTime? AskByCreatedOn { get; set; } = null;
        public List<FeedbackComment> FeedbackComments { get; set; }
    }

    public class FeedbackComment
    {
        public long CommentId { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ImagePath { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long EmployeeId { get; set; }
    }

    public class MyGoalDetailFeedbackResponse
    {
        public string ObjectiveDescription { get; set; }
        public string ObjectiveName { get; set; }
        public List<GoalKeyDetails> KeyDetails { get; set; }
        public int FeedbackOnTypeId { get; set; }
        public long FeedbackOnId { get; set; }
        public string AskByRequestRemark { get; set; }
        public long FeedbackById { get; set; }
        public string AskByFirstName { get; set; }
        public string AskByLastName { get; set; }
        public string AskByImagePath { get; set; }
        public DateTime AskByCreatedOn { get; set; }
        public List<FeedbackProvideDetails> FeedbackProvideDetails { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; }

    }

    public class FeedbackProvideDetails
    {
        public long FeedbackDetailId { get; set; }
        public string SharedRemark { get; set; }
        public DateTime SharedByCreatedOn { get; set; }
        public string SharedByFirstname { get; set; }
        public string SharedByLastname { get; set; }
        public string SharedByImagePath { get; set; }

    }
}
