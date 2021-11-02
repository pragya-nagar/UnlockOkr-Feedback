using System.Threading.Tasks;
using OKRFeedbackService.ViewModel.Response;

namespace OKRFeedbackService.Service.Contracts
{
   public  interface ICommonService
    {
        Task<UserIdentity> GetUserIdentity();
        Task<UserIdentity> GetUserIdentity(string jwtToken);
    }
}
