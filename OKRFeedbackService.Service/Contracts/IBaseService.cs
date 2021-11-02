using OKRFeedbackService.EF;
using OKRFeedbackService.ViewModel.Request;
using OKRFeedbackService.ViewModel.Response;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OKRFeedbackService.Service.Contracts
{
    public interface IBaseService
    {
        IUnitOfWorkAsync UnitOfWorkAsync { get; set; }
        IOperationStatus OperationStatus { get; set; }
        FeedbackDbContext FeedbackDBContext { get; set; }
        string ConnectionString { get; set; }
        HttpClient GetHttpClient(string jwtToken);
        Task<MyGoalFeedbackResponse> MyGoalFeedBackResponse(int feedbackOnTypeId, long feedbackOnId, string jwtToken);
        Task<UserDetails> GetUser(string jwtToken, long employeeId);
       
        EmployeeResult GetAllUserFromUsers(string jwtToken);
        Task<MailerTemplate> GetMailerTemplateAsync(string templateCode, string jwtToken = null);
        Task<bool> SentMailAsync(MailRequest mailRequest, string jwtToken = null);
        Task SaveNotificationAsync(NotificationsRequest notificationsResponse, string jwtToken = null);
        Task NotificationsAsync(NotificationDetails notificationDetails);
    }
}
