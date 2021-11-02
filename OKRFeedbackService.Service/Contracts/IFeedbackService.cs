using System.Collections.Generic;
using System.Threading.Tasks;
using OKRFeedbackService.EF;
using OKRFeedbackService.ViewModel.Request;
using OKRFeedbackService.ViewModel.Response;

namespace OKRFeedbackService.Service.Contracts
{
    public interface IFeedbackService
    {
        Task<IOperationStatus> InsertAskRequest(AskFeedbackRequest askFeedbackRequest, UserIdentity loginUser, string jwtToken);
        void SaveLog(string pageName, string functionName, string errorDetail);
        Task<FeedbackDetail> ProvideFeedback(ProvideFeedbackRequest request, UserIdentity loginUser, string jwtToken);
        Task UpdateFeedbackStatus(long feedbackRequestId, int statusId, long loginUser);
        Task<Comment> InsertComment(CommentRequest commentRequest, UserIdentity loginUser, string jwtToken);
        Task<Comment> InsertComment(Comment comment);
        Task<OkrFeedbackResponse> GetFeedbackResponse(int feedbackOnTypeId, long feedbackOnId, string jwtToken, UserIdentity loginUser);
        Task CreateOneToOneRequest(OneToOneRequest request, UserIdentity currentUser, string jwtToken);
        Task<OneToOneDetail> CreateOneToOneRequest(OneToOneDetail oneToOneDetail);
        Task<List<AskFeedbackResponse>> GetFeedbackDetail(long employeeId, string jwtToken);
        Task<List<FeedbackDetail>> GetFeedbackDetail(int feedbackOnTypeId, long feedbackOnId);
        Task<List<FeedbackRequest>> GetFeedbackDetail(long employeeId);
        Task<MyGoalDetailFeedbackResponse> MyGoalFeedbackResponse(long feedbackRequestId, string jwtToken, UserIdentity userIdentity);
        List<MostFeedbackReponse> MostFeedbackReponses(long empId, string token);
        Task<FeedbackRequest> GetFeedbackRequest(long feedbackRequestId);
        Task<List<ViewFeedbackResponse>> GetViewFeedbackResponse(long empId);
        Task<FeedbackRequest> UpdateFeedbackRequest(FeedbackRequest feedbackRequest);
        Task<FeedbackRequest> GetFeedbackRequestById(long feedbackRequestId);
        Task<FeedbackDetail> InsertFeedbackDetail(FeedbackDetail feedbackDetail);
        Task<List<Comment>> GetCommentByFeedBackId(long feedbackDetailId);
        Task<FeedbackDetail> GetFeedback(long feedbackDetailId);
        Task<FeedbackRequest> GetFeedbackRequestByIdAsync(long feedbackRequestId);
        Task<int> GetCommentsCount(long feedbackDetailId);
        Task<long> GetReceiver(long feedbackDetailsId);
        List<MostFeedbackReponse> MostFeedbackReponse(long empId, string token);
        Task<List<FeedbackRequest>> GetViewFeedbackForMyGoals(long empId);
        Task<List<FeedbackRequest>> GetViewFeedbackForMyGoalsForDirectFeedback(long empId);
        Task<List<FeedbackDetail>> GetFeedbackDetailsById(long feedbackRequestId, long loginuser, int feedbackonTypeId, long feedbackOnId);
        Task<bool> GetReadFeedbackResponse(long okrId, string jwtToken, UserIdentity userIdentity);
    }
}
