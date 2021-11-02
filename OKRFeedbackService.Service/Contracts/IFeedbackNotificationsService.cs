using OKRFeedbackService.EF;
using OKRFeedbackService.ViewModel.Request;
using OKRFeedbackService.ViewModel.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OKRFeedbackService.Service.Contracts
{
    public interface IFeedbackNotificationsService
    {
        Task InsertAskRequestNotificationsAndEmails(List<FeedbackRequest> feedbackRequestList, AskFeedbackRequest askFeedbackRequest, string jwtToken);
        Task ProvideFeedbackNotificationsAndEmails(FeedbackRequest feedbackRequest, FeedbackDetail feedbackDetail, ProvideFeedbackRequest request, long loginUserId, string jwtToken);
        Task ProvidePersonalizeFeedbackNotificationsAndEmails(FeedbackRequest feedbackRequest, FeedbackDetail feedbackDetail, PersonalFeedbackRequest request, long loginUserId, string jwtToken);
        Task InsertCommentNotificationsAndEmails(FeedbackRequest feedbackRequest, FeedbackDetail result, string getLastComment, long receiver, EmployeeResult userList, Comment comment, string jwtToken, UserIdentity loginUser);
        Task InsertPersonalizeCommentNotificationsAndEmails(FeedbackRequest feedbackRequest, FeedbackDetail result, string getLastComment, long receiver, EmployeeResult userList, Comment comment, string jwtToken, UserIdentity loginUser);
        Task CreateOneToOneRequestNotificationsAndEmailsWhenRequestedForOkr(long OneToOneDetailId, OneToOneRequest request, long userId, UserResponse requestTo, UserResponse requestFrom, string emailId,long loginUserId, string jwtToken);
        Task CreateOneToOneRequestNotificationsAndEmailsWhenRequestedForFeedback(long OneToOneDetailId, OneToOneRequest request, UserResponse requestTo, UserResponse requestFrom, string emailId, FeedbackRequest feedbackRequest, string jwtToken);
        Task InsertAskPersonalNotificationsAndEmails(List<FeedbackRequest> feedbackRequestPersonal, AskPersonalFeedbackRequest askFeedbackRequest, string jwtToken);
        Task CreatePersonalizeOneOnOneRequestNotificationsAndEmails(List<OneToOneDetail> oneToOneDetails, PersonalFeedbackOneOnOneRequest request, string jwtToken);
        Task RequestAgainNotifications(long feedbackRequestId, string jwtToken, long raisedForId, long feedbackById, string requestRemark);
        Task ApproveAndRejectRequestOnetoOne(long detailId, int status, long notificationsDetailId, long requestedTo, long requestedFrom, string jwtToken);
        Task ProvideTeamFeedbackNotificationsAndEmails(long teamId, FeedbackRequest feedbackRequest, FeedbackDetail feedbackDetail, ProvideFeedbackRequest request, long loginUserId, string jwtToken);
        Task ProvideTeamOkrFeedbackNotificationsAndEmails(long teamId, FeedbackRequest feedbackRequest, FeedbackDetail feedbackDetail, ProvideFeedbackRequest request, long loginUserId, string jwtToken);
    }
}

