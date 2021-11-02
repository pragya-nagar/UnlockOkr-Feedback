using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OKRFeedbackService.Common;
using OKRFeedbackService.EF;
using OKRFeedbackService.Service.Contracts;
using OKRFeedbackService.ViewModel.Request;
using OKRFeedbackService.ViewModel.Response;
using Serilog;

namespace OKRFeedbackService.Service
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseService : IBaseService
    {
        public IUnitOfWorkAsync UnitOfWorkAsync { get; set; }
        public IOperationStatus OperationStatus { get; set; }
        public FeedbackDbContext FeedbackDBContext { get; set; }
        public IConfiguration Configuration { get; set; }
        public IHostingEnvironment HostingEnvironment { get; set; }
        protected ILogger Logger { get; private set; }
        protected IMapper Mapper { get; private set; }
        protected HttpContext HttpContext => new HttpContextAccessor().HttpContext;
        protected string LoggedInUserEmail => HttpContext.User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
        protected string UserToken => HttpContext.User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == "token")?.Value;
        protected bool IsTokenActive => (!string.IsNullOrEmpty(LoggedInUserEmail) && !string.IsNullOrEmpty(UserToken));
        protected string TenantId => HttpContext.User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == "tenantId")?.Value;
        private IKeyVaultService keyVaultService;
        public IKeyVaultService KeyVaultService => keyVaultService ??= HttpContext.RequestServices.GetRequiredService<IKeyVaultService>();
        private IDistributedCache distributedCache;
        public IDistributedCache DistributedCache => distributedCache ??= HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
        public string ConnectionString
        {
            get => FeedbackDBContext?.Database.GetDbConnection().ConnectionString;
            set
            {
                if (FeedbackDBContext != null)
                    FeedbackDBContext.Database.GetDbConnection().ConnectionString = value;
            }
        }

        protected BaseService(IServicesAggregator servicesAggregateService)
        {
            UnitOfWorkAsync = servicesAggregateService.UnitOfWorkAsync;
            FeedbackDBContext = UnitOfWorkAsync.DataContext as FeedbackDbContext;
            OperationStatus = servicesAggregateService.OperationStatus;
            Configuration = servicesAggregateService.Configuration;
            HostingEnvironment = servicesAggregateService.HostingEnvironment;
            Mapper = servicesAggregateService.Mapper;
            Logger = Log.Logger;

        }

        public HttpClient GetHttpClient(string jwtToken)
        {
            var hasTenant = HttpContext.Request.Headers.TryGetValue("TenantId", out var tenantId);
            if ((!hasTenant && HttpContext.Request.Host.Value.Contains("localhost")))
                tenantId = Configuration.GetValue<string>("TenantId");
            string domain;
            var hasOrigin = HttpContext.Request.Headers.TryGetValue("OriginHost", out var origin);
            if (!hasOrigin && HttpContext.Request.Host.Value.Contains("localhost"))
                domain = Configuration.GetValue<string>("FrontEndUrl");
            else
                domain = string.IsNullOrEmpty(origin) ? string.Empty : origin.ToString();
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(Configuration.GetSection("Notifications:BaseUrl").Value)
            };
            var token = !string.IsNullOrEmpty(jwtToken) ? jwtToken : UserToken;
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("TenantId", tenantId.ToString());
            httpClient.DefaultRequestHeaders.Add("OriginHost", domain);

            return httpClient;
        }

        public async Task<MyGoalFeedbackResponse> MyGoalFeedBackResponse(int feedbackOnTypeId, long feedbackOnId, string jwtToken)
        {
            using var httpClient = GetHttpClient(jwtToken);
            httpClient.BaseAddress = new Uri(Configuration.GetSection("Okr:BaseUrl").Value);
            var myFeedback = new MyGoalFeedbackResponse();
            using var response = await httpClient.GetAsync(Constants.GetKeyDetail + feedbackOnTypeId + Constants.TypeId + feedbackOnId);

            if (response.IsSuccessStatusCode)
            {
                var data = JsonConvert.DeserializeObject<PayloadCustom<MyGoalFeedbackResponse>>(await response.Content.ReadAsStringAsync());
                myFeedback = data.Entity;
            }
            return myFeedback;
        }
        public EmployeeResult GetAllUserFromUsers(string jwtToken)
        {
            var employeeResponse = new EmployeeResult();
            if (jwtToken != "")
            {
                using var httpClient = GetHttpClient(jwtToken);
                httpClient.BaseAddress = new Uri(Configuration.GetValue<string>("User:BaseUrl"));
                using var response = httpClient.GetAsync($"api/User/GetAllusers?pageIndex=1&pageSize=9999").Result;
                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = response.Content.ReadAsStringAsync().Result;
                    var user = JsonConvert.DeserializeObject<PayloadCustomList<PageResults<UserResponse>>>(apiResponse);
                    employeeResponse.Results = user.Entity.Records;
                }
            }
            return employeeResponse;

        }

        public async Task<UserDetails> GetUser(string jwtToken, long employeeId)
        {
            var httpClient = GetHttpClient(jwtToken);
            httpClient.BaseAddress = new Uri(Configuration.GetSection("User:BaseUrl").Value);
            var userDetails = new UserDetails();

            var cacheKey = TenantId + Constants.GetAllUsers;
            var redisList = await DistributedCache.GetAsync(cacheKey);
            if (redisList != null)
            {
                var serializedList = Encoding.UTF8.GetString(redisList);
                var resDeserializeObject = JsonConvert.DeserializeObject<List<UserDetails>>(serializedList);
                userDetails = resDeserializeObject?.FirstOrDefault(x => x.EmployeeId == employeeId && x.IsActive);
            }
            else
            {
                var response = await httpClient.GetAsync("api/User/GetUsersById?empId=" + employeeId);
                if (!response.IsSuccessStatusCode) return userDetails;
                var data = JsonConvert.DeserializeObject<PayloadCustom<UserDetails>>(await response.Content.ReadAsStringAsync());
                userDetails = data.Entity;
            }
            return userDetails;
        }

        public async Task<MailerTemplate> GetMailerTemplateAsync(string templateCode, string jwtToken = null)
        {
            var template = new MailerTemplate();
            using var httpClient = GetHttpClient(jwtToken);
            using var response = await httpClient.GetAsync("api/v2/OkrNotifications/GetTemplate?templateCode=" + templateCode);
            if (response.IsSuccessStatusCode)
            {
                var payload = JsonConvert.DeserializeObject<PayloadCustom<MailerTemplate>>(await response.Content.ReadAsStringAsync());
                template = payload.Entity;
            }
            return template;
        }

        public async Task<bool> SentMailAsync(MailRequest mailRequest, string jwtToken = null)
        {
            using var httpClient = GetHttpClient(jwtToken);
            var payload = new PayloadCustom<bool>();
            using var response = await httpClient.PostAsJsonAsync("api/v2/OkrNotifications/SentMailAsync", mailRequest);
            if (response.IsSuccessStatusCode)
            {
                payload = JsonConvert.DeserializeObject<PayloadCustom<bool>>(await response.Content.ReadAsStringAsync());
            }
            return payload.IsSuccess;
        }

        public async Task SaveNotificationAsync(NotificationsRequest notificationsResponse, string jwtToken = null)
        {
            using var httpClient = GetHttpClient(jwtToken);
            using var response = await httpClient.PostAsJsonAsync("api/v2/OkrNotifications/InsertNotificationsDetailsAsync", notificationsResponse);
            if (response.IsSuccessStatusCode)
                Console.Write("Success");

            else
                Console.Write("Error");
        }

        public async Task UpdateNotificationText(UpdateNotificationTextRequest updateNotificationTextRequest, string jwtToken = null)
        {
            using var httpClient = GetHttpClient(jwtToken);
            using var response = await httpClient.PutAsJsonAsync("api/v2/OkrNotifications/UpdateNotificationText", updateNotificationTextRequest);
            if (response.IsSuccessStatusCode)
                Console.Write("Success");

            else
                Console.Write("Error");
        }

        public async Task NotificationsAsync(NotificationDetails notificationDetails)
        {
            List<long> notificationTo = new List<long>();
            NotificationsRequest notificationsRequest = new NotificationsRequest();

            if (notificationDetails.NotificationToList == null)
            {
                notificationTo.Add(notificationDetails.To);

                notificationsRequest.To = notificationTo;
            }
            else
            {
                notificationsRequest.To = notificationDetails.NotificationToList;
            }
            notificationsRequest.By = notificationDetails.By;
            notificationsRequest.Url = notificationDetails.Url;
            notificationsRequest.Text = notificationDetails.NotificationText;
            notificationsRequest.AppId = notificationDetails.AppId;
            notificationsRequest.NotificationType = notificationDetails.NotificationType;
            notificationsRequest.MessageType = notificationDetails.MessageType;
            notificationsRequest.NotificationOnTypeId = notificationDetails.NotificationOnTypeId;
            notificationsRequest.NotificationOnId = notificationDetails.NotificationOnId;

            await SaveNotificationAsync(notificationsRequest, notificationDetails.JwtToken);
        }

        public TeamDetails GetTeamEmployeeByTeamId(long teamId, string jwtToken)
        {
            var detail = new TeamDetails();
            using var httpClient = GetHttpClient(jwtToken);
            httpClient.BaseAddress = new Uri(Configuration.GetSection("User:BaseUrl").Value);
            using var response = httpClient.GetAsync($"api/Organisation/TeamDetailsById?teamId=" + teamId).Result;
            if (response.IsSuccessStatusCode)
            {
                var organizationCycleDetails = JsonConvert.DeserializeObject<PayloadCustom<TeamDetails>>(response.Content.ReadAsStringAsync().Result);
                detail = organizationCycleDetails.Entity;
            }
            return detail;
        }

        public async Task<List<NotificationsDetailsResponse>> GetAllNotifications(string jwtToken = null)
        {
            var notificationsList = new List<NotificationsDetailsResponse>();
            using var httpClient = GetHttpClient(jwtToken);
            using var response = await httpClient.GetAsync("api/v2/OkrNotifications/GetAllNotifications");
            if (response.IsSuccessStatusCode)
            {
                var payload = JsonConvert.DeserializeObject<PayloadCustom<NotificationsDetailsResponse>>(await response.Content.ReadAsStringAsync());
                notificationsList = payload.EntityList;
            }
            return notificationsList;
        }

        public async Task<string> ReadNotificationsForFeedback(long notificationsDetailsId, string jwtToken = null)
        {
            var notifications = string.Empty;
            using var httpClient = GetHttpClient(jwtToken);
            using var response = await httpClient.PutAsJsonAsync("api/v2/OkrNotifications/Notifications/" + notificationsDetailsId, new StringContent(""));
            if (response.IsSuccessStatusCode)
            {
                var payload = JsonConvert.DeserializeObject<PayloadCustom<string>>(await response.Content.ReadAsStringAsync());
                notifications = payload.Entity;
            }
            return notifications;

        }
    }
}

