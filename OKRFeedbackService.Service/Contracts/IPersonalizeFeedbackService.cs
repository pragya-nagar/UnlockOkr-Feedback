
using OKRFeedbackService.EF;
using OKRFeedbackService.ViewModel.Request;
using OKRFeedbackService.ViewModel.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OKRFeedbackService.Service.Contracts
{
    public interface IPersonalizeFeedbackService
    {
        Task<IOperationStatus> AddPersonalFeedbackAsync(AskPersonalFeedbackRequest askFeedbackRequest, UserIdentity loginUser, string jwtToken);
        Task<List<AskPersonalFeedbackResponse>> GetAskedFeedbackUserDetailAsync(long employeeId, bool sortOrder, string jwtToken, string searchName);
        Task<List<CriteriaMaster>> GetCriteriaMastersWithId(int typeId);
        Task AddOneOnOneRequestAsync(PersonalFeedbackOneOnOneRequest request, UserIdentity currentUser, string jwtToken);
        Task<List<ProvidePersonalFeedbackResponse>> GetProvideFeedbackUserDetailAsync(long employeeId, bool sortOrder, string jwtToken, string searchName);
        Task<FeedbackDetail> ProvideFeedback(PersonalFeedbackRequest request, UserIdentity loginUser, string jwtToken);
        Task<CriteriaMasterResponse> GetCriteriaDetails(int typeId);
        Task<bool> CancelFeedbackRequest(long feedbackRequestId, UserIdentity loginUser);
        Task<bool> FeedbackRequestAgain(long feedbackRequestId, string jwtToken);
        Task<List<ViewPersonalFeedbackResponse>> ViewPersonalFeedback(long employeeId, bool sortOrder, string searchName, string jwtToken);
        Task<PersonalFeedbackResponse> PersonalFeedbackResponse(long feedbackRequestId, string jwtToken, UserIdentity userIdentity);
        Task<bool> ApproveRejectRequestOnetoOne(AcceptRejectRequest acceptRejectRequest, UserIdentity loginUser, string jwtToken);
        Task<OneToOneDetail> GetRequestOnetoOneByIdAsync(long detailId);
    }
}
